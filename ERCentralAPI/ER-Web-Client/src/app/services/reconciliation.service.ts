import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Comparison } from './comparisons.service';
import { ReviewSet, SetAttribute, ItemSetCompleteCommand, ReviewSetsService, singleNode } from './ReviewSets.service';
import { Item } from './ItemList.service';
import { ItemSet, ItemAttributePDF, iItemSet } from './ItemCoding.service';
import { ArmTimepointLinkListService } from './ArmTimepointLinkList.service';
import { ModalService } from './modal.service';
import { Outcome } from './outcomes.service';
import { ConfigService } from './config.service';
import { lastValueFrom } from 'rxjs';
@Injectable({

    providedIn: 'root',

})

export class ReconciliationService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
        private _armsService: ArmTimepointLinkListService,
        private _modalService: ModalService,
        private _ReviewSetsService: ReviewSetsService,
        configService: ConfigService
    ) {
        super(configService);
    }

    //public localList: ReconcilingItemList = new ReconcilingItemList(new ReviewSet(),
    //	new Comparison(), ""
    //);
    public reconcilingArr: any[] = [];

    FetchItemSetList(ItemIDCrit: number): Promise<ItemSet[]> {

        this._BusyMethods.push("FetchItemSetList");
        let body = JSON.stringify({ Value: ItemIDCrit });

        return lastValueFrom(this._httpC.post<iItemSet[]>(this._baseUrl + 'api/ItemSetList/Fetch', body
        )
        ).then(
            (ires: iItemSet[]) => {
                let res: ItemSet[] = [];
                for (let iSet of ires) {
                    let NewRealItemSet: ItemSet = new ItemSet(iSet);
                    res.push(NewRealItemSet);
                }
                this.RemoveBusy('FetchItemSetList');
                return res;
            },
            (error) => {
                this.RemoveBusy("FetchItemSetList");
                this._modalService.GenericError(error);
                return error;
            }
        );
    }

    async FetchArmsForReconItems(items: Item[]) {

        for (var i = 0; i < items.length; i++) {
            await this._armsService.FetchPromiseArms(items[i]);
        }
        //return items;
    }

    ItemSetCompleteComparison(recon: ReconcilingItem, comp: Comparison,
        contactID: number, completeOrNot: boolean, LockOrNot: boolean): Promise<ReviewSet> {

        this._BusyMethods.push("ItemSetCompleteComparison");

        let bt: number = contactID;
        let isi: number = -1;
        let completor: string = "";
        let cmd: ItemSetCompleteCommand = new ItemSetCompleteCommand();

        if (completeOrNot) {
            if (comp.contactId1 == bt) {
                isi = recon.ItemSetR1;
                completor = comp.contactName1;
            }
            else if (comp.contactId2 == bt) {
                isi = recon.ItemSetR2;
                completor = comp.contactName2;
            }
            else if (comp.contactId3 == bt) {
                isi = recon.ItemSetR3;
                completor = comp.contactName3;
            }
            cmd.itemSetId = isi;
            cmd.isLocked = LockOrNot;
            cmd.complete = true;
        }
        else {

            let completedByID: number = recon.CompletedByID;
            if (comp.contactId1 == completedByID) {
                isi = recon.ItemSetR1;
            }
            else if (comp.contactId2 == completedByID) {
                isi = recon.ItemSetR2;
            }
            else if (comp.contactId3 == completedByID) {
                isi = recon.ItemSetR3;
            }
            else {
                isi = recon.CompletedItemSetID;
            }
            cmd.itemSetId = isi;
            cmd.isLocked = LockOrNot;
            cmd.complete = false;
        }
        //alert('testing...' + cmd.itemSetId);

        return lastValueFrom(this._httpC.post<ItemSetCompleteCommand>(this._baseUrl + 'api/ItemSetList/ExcecuteItemSetCompleteCommand', cmd)
        ).then(
            (res) => {
                //console.log('sadfgdfg' + res);
                let rSet = this._ReviewSetsService.ReviewSets.find(found => found.ItemSetId == cmd.itemSetId);
                this.RemoveBusy('ItemSetCompleteComparison');
                if (res.successful != null && res.successful) {

                    if (rSet) {
                        rSet.codingComplete = cmd.complete;
                        rSet.itemSetIsLocked = cmd.isLocked;
                    } else {
                        return Error;
                    }
                }
                return rSet;
            },
            (error) => {
                this.RemoveBusy("ItemSetCompleteComparison");
                this._modalService.GenericError(error);
                return error;
            }
        );
    }
    public TransferSingleCoding(cmd: iComparisonItemAttributeSaveCommand): Promise<boolean> {
        this._BusyMethods.push("TransferSingleCoding");
        return lastValueFrom(this._httpC.post<iComparisonItemAttributeSaveCommand>(this._baseUrl + 'api/ItemSetList/ComparisonItemAttributeSave', cmd)
        ).then(
            (res) => {
                //console.log('sadfgdfg' + res);
                this.RemoveBusy("TransferSingleCoding");
                if (res && res.result == 'success') return true;
                else return false;
            },
            (error) => {
                this.RemoveBusy("TransferSingleCoding");
                this._modalService.GenericError(error);
                return false;
            }
        );
    }
    ngOnInit() {

    }
}

enum Visibility {

    Visible = 0,
    Collapsed = 1

}
export class ReconcilingItemList {

    private _Attributes: ReconcilingCode[] = [];
    get Attributes(): ReconcilingCode[] { return this._Attributes; }
    private _Items: ReconcilingItem[] = [];
    get Items(): ReconcilingItem[] { return this._Items }
    public Description: string = '';
    get Comparison(): Comparison {
        return this._Comparison;
    }
    private _Comparison: Comparison = new Comparison();
    public ShowReviewer3: boolean = false;
    public Reviewer3Visibility: Visibility = Visibility.Collapsed;
    public Reviewer1: string = '';

    public GetReconcilingCodeFromID(AttributeID: number): ReconcilingCode | null {
        if (this._Attributes == null) return null;

        for (let rcc of this._Attributes) {
            if (rcc.ID == AttributeID) return rcc;
        }
        return null;
    }

    constructor(Set: ReviewSet, comp: Comparison, Descr: string) {
        this._Items = [];
        this._Attributes = [];
        this._Comparison = comp;
        this.Description = Descr;

        if (Set != null && Set != undefined) {
            if (Set.attributes != null && Set.attributes.length > 0) {
                for (let CaSet of Set.attributes) {
                    this.buildToPasteFlatUnsortedList(CaSet, "");
                }
            }

        }

    }

    private buildToPasteFlatUnsortedList(aSet: SetAttribute, path: string): any {//this is recursive!!

        let astp: ReconcilingCode = new ReconcilingCode(aSet.attribute_id,
            aSet.attributeSetId, aSet.attribute_name, path);

        this._Attributes.push(astp);

        for (let CaSet of aSet.attributes) {
            this.buildToPasteFlatUnsortedList(CaSet, path + "<¬sep¬>" + aSet.attribute_name);
        }
    }
    public AddItem(item: Item, itemSetList: ItemSet[]) {
        let tmp = this.GenerateItem(item, itemSetList);
        if (tmp) this._Items.push(tmp);
    }
    public GenerateItem(item: Item, itemSetList: ItemSet[]): ReconcilingItem | null {

        if (this._Comparison == null || itemSetList == null || itemSetList.length == 0) return null;

        let isCompleted: boolean = false;
        let CompletedBy: string = "";
        let CompletedByID: number = 0;
        let CompletedItemSetID: number = 0;
        let r1: ReconcilingCode[] = [];
        let r2: ReconcilingCode[] = [];
        let r3: ReconcilingCode[] = [];
        let o1: Outcome[] = [];
        let o2: Outcome[] = [];
        let o3: Outcome[] = [];

        let itSetR1: number = -1, itSetR2: number = -1, itSetR3: number = -1;

        for (let iSet of itemSetList) {
            if (iSet.setId != this.Comparison.setId) continue;
            else {

                if (iSet.isCompleted) {
                    isCompleted = iSet.isCompleted;
                    CompletedBy = iSet.contactName;
                    CompletedByID = iSet.contactId;
                    CompletedItemSetID = iSet.itemSetId;
                }

                if (iSet.contactId == this._Comparison.contactId1) {
                    itSetR1 = iSet.itemSetId;
                    for (let roia of iSet.itemAttributesList) {
                        let r0: ReconcilingCode | null = this.GetReconcilingCodeFromID(roia.attributeId);
                        if (r0 != null) {
                            let r: ReconcilingCode = r0.Clone();

                            r.InfoBox = roia.additionalText;
                            r.ArmID = roia.armId;
                            r.ArmName = roia.armTitle;
                            r.ItemAttributeID = roia.itemAttributeId;
                            r1.push(r);

                        }
                    }
                    o1 = iSet.OutcomeList;
                }
                else if (iSet.contactId == this._Comparison.contactId2) {
                    itSetR2 = iSet.itemSetId;
                    for (let roia of iSet.itemAttributesList) {
                        let r0: ReconcilingCode | null = this.GetReconcilingCodeFromID(roia.attributeId);
                        if (r0 != null)//this is necessary to avoid trying to add a code that belongs to the item, but is coming from a dead branch (a code for wich one of the parents got deleted)!
                        {//in such situations r0 is null
                            let r: ReconcilingCode = r0.Clone();
                            r.InfoBox = roia.additionalText;
                            r.ArmID = roia.armId;
                            r.ArmName = roia.armTitle;
                            r.ItemAttributeID = roia.itemAttributeId;
                            r2.push(r);
                        }
                    }
                    o2 = iSet.OutcomeList;
                }
                else if (iSet.contactId == this._Comparison.contactId3) {
                    itSetR3 = iSet.itemSetId;
                    for (let roia of iSet.itemAttributesList) {
                        let r0: ReconcilingCode | null = this.GetReconcilingCodeFromID(roia.attributeId);
                        if (r0 != null)//this is necessary to avoid trying to add a code that belongs to the item, but is coming from a dead branch (a code for wich one of the parents got deleted)!
                        {
                            let r = r0.Clone();
                            r.InfoBox = roia.additionalText;
                            r.ArmID = roia.armId;
                            r.ArmName = roia.armTitle;
                            r.ItemAttributeID = roia.itemAttributeId;
                            r3.push(r);
                        }
                    }
                    o3 = iSet.OutcomeList;
                }

            }
        }
        return new ReconcilingItem(item, isCompleted, r1, r2, r3,
            CompletedBy, CompletedByID, CompletedItemSetID, itSetR1, itSetR2, itSetR3, o1, o2, o3);
    }

}
export class ReconcilingCode {

    private _ID: number = 0;
    private _AttributeSetID: number = 0;
    private _ArmID: number = 0;
    private _Name: string = '';
    private _ArmName: string = '';
    private _Fullpath: string = '';
    private _InfoBox: string = '';
    private _ItemAttributeID: number = 0;
    private _PDFCoding: ItemAttributePDF[] | null = null;

    get ID(): number {
        return this._ID;
    }

    get AttributeSetID(): number {
        return this._AttributeSetID;
    }

    get ArmID(): number {
        return this._ArmID;
    }

    set ArmID(value: number) {
        this._ArmID = value;
    }


    get ArmName(): string {
        return this._ArmName;
    }

    set ArmName(value: string) {
        this._ArmName = value;
    }

    get Name(): string {
        return this._Name;
    }

    get Fullpath(): string {
        return this._Fullpath;
    }

    get InfoBox(): string {
        return this._InfoBox;
    }
    set InfoBox(value: string) {
        this._InfoBox = value;
    }

    get PDFCoding(): ItemAttributePDF[] | null {
        return this._PDFCoding;
    }
    set PDFCoding(data: ItemAttributePDF[] | null) {
        this._PDFCoding = data;
    }

    get ItemAttributeID(): number {
        return this._ItemAttributeID;
    }
    set ItemAttributeID(value: number) {
        this._ItemAttributeID = value;
    }

    constructor(AttributeID: number, attributeSetID: number,
        name: string, fullpath: string) {

        this._ID = AttributeID;
        this._Name = name;
        this._Fullpath = fullpath;
        this._AttributeSetID = attributeSetID;

    }

    public Clone(): ReconcilingCode {

        let res: ReconcilingCode =
            new ReconcilingCode(this.ID, this.AttributeSetID, this.Name, this.Fullpath);

        return res;
    }
}
export class ReconcilingItem {
    private _Item: Item = new Item();
    get Item(): Item { return this._Item; }

    private _IsCompleted: boolean = false;
    get IsCompleted(): boolean { return this._IsCompleted; }

    private _CodesReviewer1: ReconcilingCode[] = [];
    get CodesReviewer1(): ReconcilingCode[] { return this._CodesReviewer1; }

    private _CodesReviewer2: ReconcilingCode[] = [];
    get CodesReviewer2(): ReconcilingCode[] { return this._CodesReviewer2; }

    private _CodesReviewer3: ReconcilingCode[] = [];
    get CodesReviewer3(): ReconcilingCode[] { return this._CodesReviewer3; }

    private _CompletedByName: string = '';
    get CompletedByName(): string { return this._CompletedByName }
    private _Completedby: string = '';
    get Completedby(): string { return this._Completedby; }
    private _CompletedByID: number = 0;
    get CompletedByID(): number { return this._CompletedByID; }
    set CompletedByID(value: number) { this._CompletedByID = value; }
    private _CompletedItemSetID: number = 0;
    get CompletedItemSetID(): number { return this._CompletedItemSetID; }
    private _ItemSetR1: number = 0;
    get ItemSetR1(): number { return this._ItemSetR1; }
    private _ItemSetR2: number = 0;
    get ItemSetR2(): number { return this._ItemSetR2; }
    private _ItemSetR3: number = 0;
    get ItemSetR3(): number { return this._ItemSetR3; }

    private _OutcomesReviewer1: Outcome[] = [];
    get OutcomesReviewer1(): Outcome[] { return this._OutcomesReviewer1; }

    private _OutcomesReviewer2: Outcome[] = [];
    get OutcomesReviewer2(): Outcome[] { return this._OutcomesReviewer2; }

    private _OutcomesReviewer3: Outcome[] = [];
    get OutcomesReviewer3(): Outcome[] { return this._OutcomesReviewer3; }

    public UnMatchedOutcomesReviewer1: Outcome[] = [];
    public UnMatchedOutcomesReviewer2: Outcome[] = [];
    public UnMatchedOutcomesReviewer3: Outcome[] = [];

    private _MatchedOutcomes: any[] = [];
    get MatchedOutcomes() { return this._MatchedOutcomes; }
    public SetMatchedOutcomes(Ids: any[]) {
        this._MatchedOutcomes = [];
        if (Ids.length == 0) return;
        let IsThreeWay: boolean = true;
        if (Ids[0][2] == undefined) IsThreeWay = false;

        for (const OutcIds of Ids) {
            let NewLine: (Outcome | undefined)[] = [];
            let o1 = this._OutcomesReviewer1.find(f => f.outcomeId == OutcIds[0]);
            let o2 = this._OutcomesReviewer2.find(f => f.outcomeId == OutcIds[1]);
            if (IsThreeWay) {
                let o3 = this._OutcomesReviewer3.find(f => f.outcomeId == OutcIds[2]);
                //if (o1 && o2 && o3) {
                NewLine.push(o1);
                NewLine.push(o2);
                NewLine.push(o3);
                this._MatchedOutcomes.push(NewLine);
                //}
            } else {
                //if (o1 && o2 ) {
                NewLine.push(o1);
                NewLine.push(o2);
                this._MatchedOutcomes.push(NewLine);
                //}
            }
        }
    }
    constructor(item: Item, isCompleted: boolean, codesReviewer1: ReconcilingCode[],
        codesReviewer2: ReconcilingCode[], codesReviewer3: ReconcilingCode[]
        , completedby: string, completedbyID: number, completedItemSetID: number
        , itemsetR1: number, itemsetR2: number, itemsetR3: number
        , outcomesReviewer1: Outcome[], outcomesReviewer2: Outcome[], outcomesReviewer3: Outcome[]) {

        this._Item = item;
        this._CodesReviewer1 = codesReviewer1;
        this._CodesReviewer2 = codesReviewer2;
        this._CodesReviewer3 = codesReviewer3;
        this._IsCompleted = isCompleted;
        this._CompletedByName = completedby;
        this._CompletedByID = completedbyID;
        this._CompletedItemSetID = completedItemSetID;
        this._ItemSetR1 = itemsetR1;
        this._ItemSetR2 = itemsetR2;
        this._ItemSetR3 = itemsetR3;
        this._OutcomesReviewer1 = outcomesReviewer1;
        this._OutcomesReviewer2 = outcomesReviewer2;
        this._OutcomesReviewer3 = outcomesReviewer3;
    }
    public get HasOutcomes(): boolean {
        if (this.OutcomesReviewer1.length > 0
            || this.OutcomesReviewer2.length > 0
            || this.OutcomesReviewer3.length > 0) return true;
        return false;
    }
}
export interface iReconcilingReviewSet extends singleNode {
    Reviewer1Coding: ReconcilingCode[];
    Reviewer2Coding: ReconcilingCode[];
    Reviewer3Coding: ReconcilingCode[];
}
export class ReconcilingSetAttribute extends SetAttribute implements iReconcilingReviewSet {
    public Reviewer1Coding: ReconcilingCode[] = [];
    public Reviewer2Coding: ReconcilingCode[] = [];
    public Reviewer3Coding: ReconcilingCode[] = [];
    attributes: ReconcilingSetAttribute[];
    constructor(SetAtt: SetAttribute, AllReviewer1Coding: ReconcilingCode[], AllReviewer2Coding: ReconcilingCode[], AllReviewer3Coding: ReconcilingCode[]) {
        super();
        this.attribute_id = SetAtt.attribute_id;
        this.attribute_name = SetAtt.attribute_name;
        this.attribute_order = SetAtt.attribute_order;
        this.attributeSetId = SetAtt.attributeSetId;
        this.attribute_type = SetAtt.attribute_type;
        this.attribute_set_desc = SetAtt.attribute_set_desc;
        this.attribute_desc = SetAtt.attribute_desc;
        this.set_id = SetAtt.set_id;
        this.parent_attribute_id = SetAtt.parent_attribute_id;
        this.attribute_type_id = SetAtt.attribute_type_id;
        this.originalAttributeID = SetAtt.originalAttributeID;
        this.allowEditingCodeset = SetAtt.allowEditingCodeset;
        this.itemSetIsLocked = SetAtt.itemSetIsLocked;
        this.nodeType = SetAtt.nodeType;
        this.allowCodingEdits = SetAtt.allowCodingEdits;
        this.isSelected = SetAtt.isSelected;
        this.additionalText = SetAtt.additionalText;
        this.armId = SetAtt.armId;
        this.armTitle = SetAtt.armTitle;
        this.order = SetAtt.order;
        this.codingComplete = SetAtt.codingComplete;
        this.extURL = SetAtt.extURL;
        this.extType = SetAtt.extType;
        this.Reviewer1Coding = AllReviewer1Coding.filter(f => f.ID == this.attribute_id);
        this.Reviewer2Coding = AllReviewer2Coding.filter(f => f.ID == this.attribute_id);
        this.Reviewer3Coding = AllReviewer3Coding.filter(f => f.ID == this.attribute_id);
        this.attributes = [];
        for (let aset of SetAtt.attributes) {
            this.attributes.push(new ReconcilingSetAttribute(aset,
                AllReviewer1Coding.filter(f => f.ID !== this.attribute_id),
                AllReviewer2Coding.filter(f => f.ID !== this.attribute_id),
                AllReviewer3Coding.filter(f => f.ID !== this.attribute_id)));
        }
    }

    public FindByIdNumber(Id: number): ReconcilingSetAttribute | null {
        if (this.attribute_id == Id) return this;
        for (let a of this.attributes) {
            let b = a.FindByIdNumber(Id);
            if (b != null) return b;
        }
        return null;
    }

    public FindPreviousByIdNumber(Id: number): ReconcilingSetAttribute | null {
        let index = this.attributes.findIndex(f => f.attribute_id == Id);
        if (index > 0) {//the found attribute is not the first child, we want the last descendant of its previous sibling...
            let counter: number = 0;
            let LastAtt = this.attributes[index - 1];
            let FoundAtLast: ReconcilingSetAttribute | null = null;
            while (FoundAtLast == null && counter < 10000) {
                counter++;
                if (LastAtt.attributes.length == 0) {
                    //no children to crawl, this is the att we want
                    FoundAtLast = LastAtt;
                } else {
                    //this LastAtt is not the actual last, 'cause it has children...
                    LastAtt = LastAtt.attributes[LastAtt.attributes.length - 1];//replace with the last child of the current "LastAtt", repeat...
                }
            }
            return FoundAtLast;
        }
        else if (index == 0) {//the found attribute is the first code at its level, so its "parent" is the "previous code", which happens to be "this".
            return this;
        }
        else {//Attribute we're looking for is not an immediate child, keep crawling in reverse...
            for (let i = this.attributes.length - 1; i > -1; i--) {//we are searching from last to first
                let TmpRes = this.attributes[i].FindPreviousByIdNumber(Id);
                if (TmpRes != null) {//must be a ReconcilingSetAttribute, so it's our result...
                    return TmpRes;
                }
                //otherwise, we keep crawling in reverse...
            }
        }
        return null;
    }

    public FindNextByIdNumber(Id: number): ReconcilingSetAttribute | null | true {
        if (this.attribute_id == Id) {
            //good, we have found the "initial" node, now we need to find the "next" and return that...
            if (this.attributes.length > 0) return this.attributes[0];//if this node has children, the "next" node is the first child.
            else {//next we look at the siblings if our current 
                return true;//this signals the caller that the "next" node is the first of its siblings, or the first of its parent's siblings, etc.
            }
        }
        for (let i = 0; i < this.attributes.length; i++) {
            let a = this.attributes[i];
            let b = a.FindNextByIdNumber(Id);
            if (b != null && b != true) return b;
            else if (b == true) {
                //we need to see if we have a next sibling!
                if (i == this.attributes.length - 1) return true;//pass signal back: need to look for a next sibling or further up the tree
                else return this.attributes[i + 1];//the first sibling...
            }
        }
        return null;
    }
    public ParentsListByAttId(Id: number, listSoFar: ReconcilingSetAttribute[]): boolean {
        if (this.attribute_id == Id) return true;
        for (let a of this.attributes) {
            let b = a.ParentsListByAttId(Id, listSoFar);
            if (b == true) {
                listSoFar.push(this);
                return b;
            }
        }
        return false;
    }
}
export class ReconcilingReviewSet extends ReviewSet implements iReconcilingReviewSet {
    public readonly Reviewer1Coding: ReconcilingCode[] = [];
    public readonly Reviewer2Coding: ReconcilingCode[] = [];
    public readonly Reviewer3Coding: ReconcilingCode[] = [];
    public attributes: ReconcilingSetAttribute[];
    constructor(RevSet: ReviewSet, AllReviewer1Coding: ReconcilingCode[], AllReviewer2Coding: ReconcilingCode[], AllReviewer3Coding: ReconcilingCode[]) {
        super();
        this.set_id = RevSet.set_id;
        this.set_name = RevSet.set_name;
        this.set_order = RevSet.set_order;
        this.reviewSetId = RevSet.reviewSetId;
        this.description = RevSet.description;
        this.setType = RevSet.setType;
        this.reviewSetId = RevSet.reviewSetId;
        this.order = RevSet.order;
        this.allowEditingCodeset = RevSet.allowEditingCodeset;
        this.ItemSetId = RevSet.ItemSetId;
        this.itemSetIsLocked = RevSet.itemSetIsLocked;
        this.codingIsFinal = RevSet.codingIsFinal;
        this.attributeSetId = RevSet.attributeSetId;
        this.codingComplete = RevSet.codingComplete;
        this.userCanEditURLs = RevSet.userCanEditURLs;
        this.attributes = [];
        this.Reviewer1Coding = AllReviewer1Coding;
        this.Reviewer2Coding = AllReviewer2Coding;
        this.Reviewer3Coding = AllReviewer3Coding;
        for (let aset of RevSet.attributes) {
            this.attributes.push(new ReconcilingSetAttribute(aset, AllReviewer1Coding, AllReviewer2Coding, AllReviewer3Coding));
        }
    }
    public FindByIdNumber(Id: number): ReconcilingSetAttribute | null {

        for (let a of this.attributes) {
            let b = a.FindByIdNumber(Id);
            if (b != null) return b;
        }
        return null;
    }
    public FindPreviousByIdNumber(Id: number): ReconcilingSetAttribute | null {
        let index = this.attributes.findIndex(f => f.attribute_id == Id);
        if (index > 0) {//attribute we should start from is a direct child and is not the first child, we want the last descendant of its previous sibling
            let counter: number = 0;
            let LastAtt = this.attributes[index - 1];
            let FoundAtLast: ReconcilingSetAttribute | null = null;
            while (FoundAtLast == null && counter < 10000) {
                counter++;
                if (LastAtt.attributes.length == 0) {
                    //no children to crawl, this is the att we want
                    FoundAtLast = LastAtt;
                } else {
                    //this LastAtt is not the actual last, 'cause it has children...
                    LastAtt = LastAtt.attributes[LastAtt.attributes.length - 1];//replace with the last child of the current "LastAtt", repeat...
                }
            }
            return FoundAtLast;
        }
        else if (index == 0) {//attribute we should start from is the first child, there is no "attribute" before it, return null
            return null;
        }
        else {//attribute we should start from is NOT a direct child (index == -1), so we have to crawl...
            for (let i = this.attributes.length - 1; i > -1; i--) {//we are searching from last to first
                let TmpRes = this.attributes[i].FindPreviousByIdNumber(Id);
                if (TmpRes != null) {//must be a ReconcilingSetAttribute, so it's our result...
                    return TmpRes;
                }
            }
        }
        return null;
    }
    public FindNextByIdNumber(Id: number): ReconcilingSetAttribute | null {

        for (let i = 0; i < this.attributes.length; i++) {
            let a = this.attributes[i];
            let b = a.FindNextByIdNumber(Id);
            if (b != null && b != true) return b;
            else if (b == true) {//need to look for the next sibling
                if (i < this.attributes.length - 1) return this.attributes[i + 1];//the next sibling
            }
        }
        return null;
    }
    public ParentsListByAttId(Id: number): ReconcilingSetAttribute[] {
        let res: ReconcilingSetAttribute[] = [];
        for (let i = 0; i < this.attributes.length; i++) {
            let a = this.attributes[i];
            let b = a.ParentsListByAttId(Id, res);
            if (b == true) break;
        }
        return res;
    }
}

export interface iComparisonItemAttributeSaveCommand {
    destinationContactId: number;
    sourceContactId: number;
    attributeSetId: number;
    comparisonId: number;
    includePDFcoding: boolean;
    setId: number;
    itemId: number;
    itemArmId: number;

    result: string;
    itemAttributeId: number;
    itemSetId: number;
}
