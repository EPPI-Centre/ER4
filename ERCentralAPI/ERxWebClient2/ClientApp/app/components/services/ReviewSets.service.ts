import { Component, Inject, Injectable, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of, Subscription } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemSet } from './ItemCoding.service';
import { ReviewInfo } from './ReviewInfo.service';
import { CheckBoxClickedEventData } from '../CodesetTrees/codesetTreeCoding.component';
import { ModalService } from './modal.service';
import { Node } from '@angular/compiler/src/render3/r3_ast';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ArmsService } from './arms.service';


//see: https://stackoverflow.com/questions/34031448/typescript-typeerror-myclass-myfunction-is-not-a-function
//JSON object is consumed via interfaces, but we immediately digest those and put them into actual classes.
//this is NECESSARY to consume the CSLA (server side) objects as produced by the API in a way that makes 
//then suitable for the angular-tree-component used to show codetrees to users.
@Injectable({
    providedIn: 'root',
})

export class ReviewSetsService extends BusyAwareService {
    constructor(private router: Router, //private _http: Http, 
        private _httpC: HttpClient,
        private ReviewerIdentityService: ReviewerIdentityService,
		private modalService: ModalService,

        @Inject('BASE_URL') private _baseUrl: string) {
        super();
    }

    @Output() GetReviewStatsEmit = new EventEmitter();
    private _ReviewSets: ReviewSet[] = [];
    //private _IsBusy: boolean = true;
    //public get IsBusy(): boolean {
    //    return this._IsBusy;
    //}
    private CurrentArmID: number = 0;
    public selectedNode: singleNode | null = null;
    public get SelectedCodeDescription(): string {
        if (this.selectedNode) return this.selectedNode.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
        else return "";
    }
	public CanWriteCoding(attribute: singleNode): boolean {
        //console.log('CanWriteCoding?');
        if (!this.ReviewerIdentityService || !this.ReviewerIdentityService.reviewerIdentity || (this.ReviewerIdentityService.reviewerIdentity.reviewId == 0)) {
            //console.log("can't edit coding, reason 1");
            return false;
        }
        else if ((this.IsBusy) || !this.ReviewerIdentityService.HasWriteRights) {
            //console.log("can't edit coding, reason 2", this._BusyMethods);
            return false;
        }
        else if (this.CurrentArmID > 0 && (attribute.subTypeName == 'Include' || attribute.subTypeName == 'Exclude'))
        {
            //console.log("can't edit coding, reason 3");
            return false;
        }
        let FullAttribute: SetAttribute | null = this.FindAttributeById(+attribute.id.substring(1));
        if (FullAttribute) {
            let Set = this.FindSetById(FullAttribute.set_id);
            if (Set && Set.itemSetIsLocked) {
                //console.log("can't edit coding, reason 4");
                return false;
            }
        }

		return true;
    }
    public get CurrentCodeCanHaveChildren(): boolean {
        return this.ThisCodeCanHaveChildren(this.selectedNode);
    }
    public ThisCodeCanHaveChildren(node: singleNode | null): boolean {
        if (!node) return false;//??
        //move the below to ReviewSetsService;
        else if (node.nodeType == "ReviewSet" && node.allowEditingCodeset) return true;
        else if (node.nodeType == "SetAttribute") {
            let Att: SetAttribute = node as SetAttribute;
            let Set: ReviewSet | null = this.FindSetById(Att.set_id);
            //console.log("I'm still checking: ", Att, Set);
            if (Set && Set.setType) {
                if (!Set.allowEditingCodeset) return false;
                let maxDepth: number = Set.setType.maxDepth;
                //console.log("I'm still checking2: ", maxDepth > this.ReviewSetsService.AttributeCurrentLevel(Att));
                return maxDepth > this.AttributeCurrentLevel(Att);
            }
            else return false;
        }
        else return false;
    }

    GetReviewSets(): ReviewSet[] {
         //console.log("GetReviewSets");
         this._BusyMethods.push("GetReviewSets");
         this._httpC.get<iReviewSet[]>(this._baseUrl + 'api/Codeset/CodesetsByReview').subscribe(
             data => {
                this.ReviewSets = ReviewSetsService.digestJSONarray(data);
                 //this._IsBusy = false;

                 this.GetReviewStatsEmit.emit();

            },
            error => {
                this.modalService.GenericError(error);
                this.Clear();
                this.RemoveBusy("GetReviewSets");
            },
             () => { this.RemoveBusy("GetReviewSets");}
        );
        return this.ReviewSets;
    }

    subOpeningReview: Subscription | null = null;

    public Clear() {
        this.selectedNode = null;
        this._ReviewSets = [];
        //localStorage.removeItem('ReviewSets');
    }
    public get ReviewSets(): ReviewSet[] {
        //if (this._ReviewSets.length == 0) {
        //    //this._IsBusy = true;
        //    const ReviewSetsJson = localStorage.getItem('ReviewSets');
        //    let ReviewSets: ReviewSet[] = ReviewSetsJson !== null ? ReviewSetsService.digestLocalJSONarray(JSON.parse(ReviewSetsJson)) : [];
        //    if (ReviewSets == undefined || ReviewSets == null || ReviewSets.length == 0) {

        //        //this._IsBusy = false;
        //        return this._ReviewSets;
        //    }
        //    else {
        //        this._ReviewSets = ReviewSets;
        //    }
        //}
        //this._IsBusy = false;
        return this._ReviewSets;
    }
    public set ReviewSets(sets: ReviewSet[]) {
        //this._IsBusy = true;
        this._ReviewSets = sets;
        //this.Save();
        //this._IsBusy = false;
    }
    //private Save() {
    //    if (this._ReviewSets != undefined && this._ReviewSets != null && this._ReviewSets.length > 0) //{ }
    //        localStorage.setItem('ReviewSets', JSON.stringify(this._ReviewSets));
    //    else if (localStorage.getItem('ReviewSets')) localStorage.removeItem('ReviewSets');
    //}

    public get AllowedChildTypesOfSelectedNode(): kvAllowedAttributeType[] {
        let res: kvAllowedAttributeType[] = [];
        if (!this.selectedNode) return res;
        let att: SetAttribute | null = null;
        let Set: ReviewSet | null = null;
        if (this.selectedNode.nodeType == "ReviewSet") Set = this.selectedNode as ReviewSet;
        else if (this.selectedNode.nodeType == "SetAttribute") {
            att = this.selectedNode as SetAttribute;
            if (att && att.set_id > 0) Set = this.FindSetById(att.set_id);
            if (!Set) return res;
        }
        //console.log("CurrentNode (Set)", Set);
        if (Set && Set.setType) {
            //console.log("allowed child types... ", Set.setType.allowedCodeTypes, Set.setType.allowedCodeTypes[0].key, Set.setType.allowedCodeTypes.filter(res => !res.value.endsWith('- N/A)')));
            return Set.setType.allowedCodeTypes.filter(res => !res.value.endsWith('- N/A)'));
        }
        return res;
    }

    public static digestJSONarray(data: iReviewSet[]): ReviewSet[] {
        let result: ReviewSet[] = [];
        for (let iItemset of data) {
            let newSet: ReviewSet = new ReviewSet();
            newSet.set_id = iItemset.setId;
            newSet.reviewSetId = iItemset.reviewSetId;
            newSet.set_name = iItemset.setName;
            newSet.order = iItemset.setOrder;
            newSet.codingIsFinal = iItemset.codingIsFinal;
            newSet.allowEditingCodeset = iItemset.allowCodingEdits;
            newSet.description = iItemset.setDescription;
            newSet.setType = iItemset.setType;
            newSet.attributes = ReviewSetsService.childrenFromJSONarray(iItemset.attributes.attributesList);
            result.push(newSet);
        }
        return result;
    }
    public static digestJsonReviewSet(data: iReviewSet): ReviewSet {
        let interim: iReviewSet[] = [];
        interim.push(data);
        let PreResult: ReviewSet[] = this.digestJSONarray(interim);
        if (PreResult.length > 0) return PreResult[0];
        else return new ReviewSet();
    }
    public static digestLocalJSONarray(data: any[]): ReviewSet[] {
        let result: ReviewSet[] = [];
        for (let Itemset of data) {
            let newSet: ReviewSet = new ReviewSet();
            newSet.set_id = Itemset.set_id;
            newSet.set_name = Itemset.set_name;
            newSet.order = Itemset.order;
            newSet.codingIsFinal = Itemset.codingIsFinal;
            newSet.allowEditingCodeset = Itemset.allowEditingCodeset;
            newSet.description = newSet.description;
            newSet.setType = Itemset.setType;
            newSet.attributes = ReviewSetsService.childrenFromLocalJSONarray(Itemset.attributes);
            result.push(newSet);
        }
        return result;
    }
    public static childrenFromJSONarray(data: iAttributeSet[]): SetAttribute[] {
        let result: SetAttribute[] = [];
        for (let iAtt of data) {
            let newAtt: SetAttribute = new SetAttribute();
            newAtt.attribute_id = iAtt.attributeId;
            newAtt.attribute_name = iAtt.attributeName;
            newAtt.order = iAtt.attributeOrder;
            newAtt.attribute_type = iAtt.attributeType;
            newAtt.attribute_type_id = iAtt.attributeTypeId;
			newAtt.attribute_set_desc = iAtt.attributeSetDescription;
            newAtt.attributeSetId = iAtt.attributeSetId;
            newAtt.parent_attribute_id = iAtt.parentAttributeId;
            newAtt.attribute_desc = iAtt.attributeDescription;
            newAtt.set_id = iAtt.setId; 
            newAtt.attribute_order = iAtt.attributeOrder;
            newAtt.attributes = ReviewSetsService.childrenFromJSONarray(iAtt.attributes.attributesList);
            result.push(newAtt);
        }
        return result;
    }
    public static childrenFromLocalJSONarray(data: any[]): SetAttribute[] {
        let result: SetAttribute[] = [];
        if (data && data != undefined) {
            
            for (let iAtt of data) {
                let newAtt: SetAttribute = new SetAttribute();
                newAtt.attribute_id = iAtt.attribute_id;
                newAtt.attribute_name = iAtt.attribute_name;
                newAtt.order = iAtt.order;
                newAtt.attribute_type = iAtt.attribute_type;
				newAtt.attribute_type_id = iAtt.attribute_type_id;
				newAtt.attributeSetId = iAtt.attributeSetId;
                newAtt.attribute_set_desc = iAtt.attribute_set_desc;
                newAtt.attribute_desc = iAtt.attribute_desc;
                newAtt.set_id = iAtt.set_id;
                if (iAtt.attributes) newAtt.attributes = ReviewSetsService.childrenFromLocalJSONarray(iAtt.attributes);
                else newAtt.attributes = []; 
                result.push(newAtt);
            }
        }
        return result;
    }

    public AddItemData(ItemCodingList: ItemSet[], itemArmID: number) {

        this._BusyMethods.push("AddItemData");
        //console.log('AAAAAAAAAAAAAAAAgot inside addItemData, arm title is: ' + itemArmID);
        this.CurrentArmID = itemArmID;
        //logic:
            //if ITEM_SET is complete, show the tickbox.
            //if ITEM_SET is not complete, show the tickbox only if the current user owns this item-set.
        let completedList: ItemSet[] | undefined = ItemCodingList.filter(iset => iset.isCompleted == true);
        let uncompletedList: ItemSet[] | undefined = ItemCodingList.filter(iset => iset.isCompleted == false && iset.contactId == this.ReviewerIdentityService.reviewerIdentity.userId);
        let UsedSets: number[] = [];
        for (let itemset of completedList) {
            let destSet = this._ReviewSets.find(d => d.set_id == itemset.setId);
            if (destSet) {
                let set_id: number = destSet.set_id;
                if (UsedSets.find(num => num == set_id)) { continue; }//LOGIC: we've already set the coding for this set.
                destSet.itemSetIsLocked = itemset.isLocked;
                destSet.ItemSetId = itemset.itemSetId;
                for (let itemAttribute of itemset.itemAttributesList) {
                    if (itemAttribute.armId != itemArmID) continue;
                    if (destSet.attributes) {
                        let dest = this.internalFindAttributeById(destSet.attributes, itemAttribute.attributeId);
                        if (dest) {
                            UsedSets.push(destSet.set_id);//record coding we've already added (for this set_id)
                            dest.isSelected = true;
                            dest.additionalText = itemAttribute.additionalText;
                            destSet.codingComplete = true;
                             }
                    }
                }
            }
        }
        for (let itemset of uncompletedList) {
            let destSet = this._ReviewSets.find(d => d.set_id == itemset.setId);
            if (destSet && destSet.set_id) {
                let set_id: number = destSet.set_id;
                if (UsedSets.find(num => num == set_id)) { continue; }//LOGIC: we've already set the coding for this set.
                destSet.itemSetIsLocked = itemset.isLocked;
                destSet.ItemSetId = itemset.itemSetId;
                for (let itemAttribute of itemset.itemAttributesList) {
                    if (itemAttribute.armId != itemArmID) continue;
                    //console.log('.' + destSet.set_name);
                    if (destSet.attributes) {
                        let dest = this.internalFindAttributeById(destSet.attributes, itemAttribute.attributeId);
                        if (dest) {
                            UsedSets.push(destSet.set_id);
                            dest.isSelected = true;
                            dest.additionalText = itemAttribute.additionalText;
                             }
                    }
                }
            }
		}

        console.log('finishing addItemData');
        this.RemoveBusy("AddItemData");
    }
    public FindAttributeById(AttributeId: number): SetAttribute | null {
        let result: SetAttribute | null = null;
        for (let Set of this.ReviewSets) {
            result = this.internalFindAttributeById(Set.attributes, AttributeId);
            if (result) {
                
                break;
            }
        }
        return result;
    }
    public FindSetById(SetId: number): ReviewSet | null {
        let result: ReviewSet | null = null;
        for (let Set of this.ReviewSets) {
            if (Set.set_id == SetId) {
                result = Set;
                break;
            }
        }
        return result;
    }
    public AttributeCurrentLevel(Att: SetAttribute): number {
        if (Att.parent == 0) return 1;//no need to do complicated stuff!
        let BadRes:number = 10000;//default is big 
        let interim: number = 2;
        let currentParent: SetAttribute | null = this.FindAttributeById(Att.parent_attribute_id);
        if (currentParent && currentParent.parent_attribute_id == 0) return interim;//again, keep it simple while you can.
        while (interim < 100 && currentParent && currentParent.parent_attribute_id > 0) {
            interim++;
            currentParent = this.FindAttributeById(currentParent.parent_attribute_id);
            //console.log("AttributeCurrentLevel... ", interim, currentParent);
        }
        if (currentParent) {
            //console.log("AttributeCurrentLevel did work:", interim);
            return interim;
        }
        //console.log("AttributeCurrentLevel did NOT work:", interim, currentParent, Att);
        return BadRes;
    }
    private internalFindAttributeById(list: SetAttribute[], AttributeId: number): SetAttribute | null {
        let result: SetAttribute | null = null;
        for (let candidate of list) {
            if (result) break;
            if (AttributeId == candidate.attribute_id ) {
                result = candidate;
                break;
            }
            else if (candidate.attributes) {
                result = this.internalFindAttributeById(candidate.attributes, AttributeId);
            }
        }
        return result;
    }
    public clearItemData() {
        this._BusyMethods.push("clearItemData");
        for (let set of this._ReviewSets) {
            set.itemSetIsLocked = false;
            set.ItemSetId = 0;
            set.isSelected = false;
            set.codingComplete = false;
            this.clearItemDataInChildren(set.attributes);
        }
        this.RemoveBusy("clearItemData");
    }
    private clearItemDataInChildren(children: SetAttribute[]) {
        
        for (let att of children) {
            att.additionalText = "";
            if (att.isSelected) att.isSelected = false;
            if (att.attributes && att.attributes.length > 0) this.clearItemDataInChildren(att.attributes);
        }
    }
    public createAttSaveCommand(currentItemSets: ItemSet[]): ItemAttributeSaveCommand{
        let result: ItemAttributeSaveCommand = new ItemAttributeSaveCommand();
        //ItemAttributeId: number = 0;
        //public ItemSetId: number = 0;
        //public AdditionalText: string = "";
        //public AttributeId: number = 0;
        //public SetId: number = 0;
        //public ItemId: number = 0;
        //public ItemArmId: number = 0;
        //public RevInfo: ReviewInfo | null = null;
        return result;
    }
    @Output() ItemCodingCheckBoxClickedEvent: EventEmitter<CheckBoxClickedEventData> = new EventEmitter<CheckBoxClickedEventData>();
    public PassItemCodingCeckboxChangedEvent(evdata: CheckBoxClickedEventData) {
        //this._IsBusy = true;

        this.ItemCodingCheckBoxClickedEvent.emit(evdata);
    }
    @Output() ItemCodingItemAttributeSaveCommandExecuted: EventEmitter<ItemAttributeSaveCommand> = new EventEmitter<ItemAttributeSaveCommand>();
    public ItemCodingItemAttributeSaveCommandHandled() {
        this.RemoveBusy("ExecuteItemAttributeSaveCommand");
    }
    @Output() ItemCodingItemAttributeSaveCommandError: EventEmitter<any> = new EventEmitter<any>();
    public ExecuteItemAttributeSaveCommand(cmd: ItemAttributeSaveCommand, currentCoding: ItemSet[]) {
        this._BusyMethods.push("ExecuteItemAttributeSaveCommand");
        //this "busy" situation is handled in ItemCodingItemAttributeSaveCommandHandled as it gets completed in the "coding" components...
        //thus, we don't simply remove it when the API call ends.
        this._httpC.post<ItemAttributeSaveCommand>(this._baseUrl + 'api/ItemSetList/ExcecuteItemAttributeSaveCommand', cmd).subscribe(
            data => {

                this.ItemCodingItemAttributeSaveCommandExecuted.emit(data);
                //this._IsBusy = false;
            }, error => {

                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
                //this.ItemCodingItemAttributeSaveCommandError.emit(error);
                //this._IsBusy = false;
                this.RemoveBusy("ExecuteItemAttributeSaveCommand");
            }
        );
    }
    public ExecuteItemAttributeBulkInsertCommand(cmd: ItemAttributeBulkSaveCommand) {
        this._BusyMethods.push("ExecuteItemAttributeBulkInsertCommand");
        //this "busy" situation is handled in ItemCodingItemAttributeSaveCommandHandled as it gets completed in the "coding" components...
        //thus, we don't simply remove it when the API call ends.
        this._httpC.post<ItemAttributeBulkSaveCommand>(this._baseUrl + 'api/ItemSetList/ExecuteItemAttributeBulkInsertCommand', cmd).subscribe(
            data => {
                this.RemoveBusy("ExecuteItemAttributeBulkInsertCommand");
                //this.ItemCodingItemAttributeSaveCommandExecuted.emit(data);
                //this._IsBusy = false;
            }, error => {
                console.log("error in ExecuteItemAttributeBulkInsertCommand:", error);
                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
                this.RemoveBusy("ExecuteItemAttributeBulkInsertCommand");
            }
        );
    }
    public ExecuteItemAttributeBulkDeleteCommand(cmd: ItemAttributeBulkSaveCommand) {
        this._BusyMethods.push("ExecuteItemAttributeBulkDeleteCommand");
        //this "busy" situation is handled in ItemCodingItemAttributeSaveCommandHandled as it gets completed in the "coding" components...
        //thus, we don't simply remove it when the API call ends.
        this._httpC.post<ItemAttributeBulkSaveCommand>(this._baseUrl + 'api/ItemSetList/ExecuteItemAttributeBulkDeleteCommand', cmd).subscribe(
            data => {
                this.RemoveBusy("ExecuteItemAttributeBulkDeleteCommand");
                //this.ItemCodingItemAttributeSaveCommandExecuted.emit(data);
                //this._IsBusy = false;
            }, error => {
                console.log("error in ExecuteItemAttributeBulkDeleteCommand:", error);
                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
                this.RemoveBusy("ExecuteItemAttributeBulkDeleteCommand");
            }
        );
    }
    public ExecuteItemSetCompleteCommand(cmd: ItemSetCompleteCommand): Promise<boolean> {
        //returns FALSE if something didn't work, error messages get triggered from within
        //updates data whithin this service, but NOT in ItemCodingService, components calling this method should do it, if method returns TRUE;
        this._BusyMethods.push("ExecuteItemSetCompleteCommand");
        return this._httpC.post<ItemSetCompleteCommand>(this._baseUrl + 'api/ItemSetList/ExcecuteItemSetCompleteCommand', cmd).toPromise()
        .then(
            data => {
                this.RemoveBusy("ExecuteItemSetCompleteCommand");
                if (data.successful != null && data.successful) {
                    let rSet = this._ReviewSets.find(found => found.ItemSetId == cmd.itemSetId);
                    if (rSet) {
                        rSet.codingComplete = cmd.complete;
                        rSet.itemSetIsLocked = cmd.isLocked;
                    }
                    else {
                        this.modalService.GenericErrorMessage("Sorry your changes have been saved, but we could not update it here.\n"
                            + "Please navigate to the next item and then back, to check if the expected changes did happen.\n" +
                            "If the problem persists please contact EPPISupport.");
                        return false;
                    }
                    return true;
                }
                else return false;
            }, error => {
                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. Please try again. If the problem persists please contact EPPISupport.");
                //this.ItemCodingItemAttributeSaveCommandError.emit(error);
                //this._IsBusy = false;
                console.log("Error in ExecuteItemSetCompleteCommand:", error);
                this.RemoveBusy("ExecuteItemSetCompleteCommand");
                return false;
            }
        ).catch(catched => {
            this.modalService.GenericError("Sorry, an ERROR occurred when saving your data. Please try again. If the problem persists please contact EPPISupport.");
            //this.ItemCodingItemAttributeSaveCommandError.emit(error);
            //this._IsBusy = false;
            console.log("Error(catch) in ExecuteItemSetCompleteCommand:", catched);
            this.RemoveBusy("ExecuteItemSetCompleteCommand");
            return false;
            });
    }
}

export interface singleNode {

    id: string;
    set_id: number;
    name: string;
    attributes: singleNode[];
    showCheckBox: boolean;
    nodeType: string;//codeset or attribute?
    subTypeName: string;//screening, admin, normal; selectable, non selectable, etc.
	description: string;
    attributeSetId: number;
    parent: number;

    isSelected: boolean;
    additionalText: string;
    armId: number;
    armTitle: string;
    order: number;
    codingComplete: boolean;
    allowEditingCodeset: boolean;
    itemSetIsLocked: boolean;
}

export class ReviewSet implements singleNode {
    set_id: number = -1;
    public get id(): string { return "C_" + this.set_id; }
    set_name: string = "";
    public get name(): string { return this.set_name; }
    set_order: number = -1;
    reviewSetId: number = -1;
    attributes: SetAttribute[] = [];
    showCheckBox: boolean = false;
    public get subTypeName(): string {
        if (this.setType) return this.setType.setTypeName;
        else return "";
    }
    public get parent(): number {
        return -1;//it's used only in attribues, ReviewSets have no parent!
    }
    public description: string = "";
	setType: iSetType = {
		setTypeId: 0,
		setTypeName: '',
		setTypeDescription: '',
		allowComparison: false,
		allowRandomAllocation: false,
		maxDepth: 1,
		allowedCodeTypes: [],
		allowedSetTypesID4Paste: []
	};
    nodeType: string = "ReviewSet";
    order: number = 0;
    allowEditingCodeset: boolean = false;

    ItemSetId: number = 0;
    itemSetIsLocked: boolean = false;
    codingIsFinal: boolean = true;
    
	attributeSetId: number = -1;
    isSelected: boolean = false;
    additionalText: string = "";
    armId: number = 0;
    armTitle: string = "";
    codingComplete: boolean = false;
}
export class SetAttribute implements singleNode {
    attribute_id: number = -1;
    public get id(): string { return "A" + this.attribute_id; };
    attribute_name: string = "";
    public get name(): string { return this.attribute_name; };
	attribute_order: number = -1;
	attributeSetId: number = -1;
    attribute_type: string = "";
    attribute_set_desc: string = "";
    attribute_desc: string = "";
    set_id: number = 0;
    public get description(): string {
        return this.attribute_set_desc;
    }
    public get showCheckBox(): boolean {
        if (this.attribute_type == 'Not selectable (no checkbox)') return false;
        else return true;
    }
    public get subTypeName(): string {
         return this.attribute_type;
    }
    public get parent(): number {
        return this.parent_attribute_id;
    }
    parent_attribute_id: number = -1;;
    attribute_type_id: number = -1;;
    attributes: SetAttribute[] = [];
    
    allowEditingCodeset: boolean = false;//not used for attributes
    itemSetIsLocked: boolean = false;//not used for attributes
    nodeType: string = "SetAttribute";

    allowCodingEdits: boolean = false; 
    isSelected: boolean = false; 
    additionalText: string = "";
    armId: number = 0;
    armTitle: string = "";
    order: number = 0;
    codingComplete: boolean = false;
}

export interface iReviewSet {
    reviewSetId: number;
    setId: number;
    setType: iSetType,
    setName: string;
    setDescription: string;
    setOrder: number;
    codingIsFinal: boolean;
    allowCodingEdits: boolean;//despite the name, this refers to whether the codeset is editable
    attributes: iAttributesList;
}
export interface iAttributesList
{  
    attributesList: iAttributeSet[];
}
export interface iAttributeSet {
    attributeSetId: number;
    attributeId: number;
    attributeSetDescription: string;
    attributeType: string;
    attributeTypeId: number;
    attributeName: string;
    attributeDescription: string;
    parentAttributeId: number;
    attributes: iAttributesList;
    isSelected: boolean;
    setId: number;
    attributeOrder: number;
}
export interface iSetType {
    setTypeId: number;
    setTypeName: string;
    setTypeDescription: string;
    allowComparison: boolean;
    allowRandomAllocation: boolean;
    maxDepth: number;
    allowedCodeTypes: kvAllowedAttributeType[];
    allowedSetTypesID4Paste: number[];
}
export interface kvAllowedAttributeType {
    key: number;
    value: string;
}
export class ItemAttributeSaveCommand {
    public saveType: string = "";
    public itemAttributeId: number = 0;
    public itemSetId: number = 0;
    public additionalText: string = "";
    public attributeId: number = 0;
    public setId: number = 0;
    public itemId: number = 0;
    public itemArmId: number = 0;
    
    public revInfo: ReviewInfo | null = null;
}
export class ItemAttributeBulkSaveCommand {
    //used to delete/insert coding in bulk (goes to different methods AND enpoints depending on "delete/insert")
    public attributeId: number = 0;
    public setId: number = 0;
    public itemIds: string = "";
    public searchIds: string = "";
}
export class ItemSetCompleteCommand {
    public itemSetId: number = 0;
    public complete: boolean = false;
    public successful: boolean = false;
    public isLocked: boolean = false;
}

