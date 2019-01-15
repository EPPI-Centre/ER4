import { Component, Inject, OnInit, Input, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, iSetType, ReviewSet, singleNode, kvAllowedAttributeType, SetAttribute } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { CodesetTreeEditComponent } from './codesetTreeEdit.component';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { Subscription } from 'rxjs';

@Component({
    selector: 'ReviewSetsEditor',
    templateUrl: './reviewSetsEditor.component.html',
    providers: []
})

export class ReviewSetsEditorComponent implements OnInit, OnDestroy {

   

    constructor(private router: Router,
                @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService,
        private ReviewSetsService: ReviewSetsService,
        private ReviewSetsEditingService: ReviewSetsEditingService,
        private ReviewInfoService: ReviewInfoService
    ) { }
    ngOnInit() {
        console.log
        if (this.ReviewSetsEditingService.SetTypes.length == 0) {
            this.ReviewSetsEditingService.FetchSetTypes();
        }
        if (this.ReviewInfoService.ReviewInfo.reviewId == 0) {
            this.ReviewInfoService.Fetch();
        }
    }
    @ViewChild('treeEditorComponent') treeEditorComponent!: CodesetTreeEditComponent;
    @ViewChild('CodeTypeSelect') CodeTypeSelect: any;
    private _ActivityPanelName: string = "";
    public get ActivityPanelName() {
        return this._ActivityPanelName;
    }
    public NewSetSelectedTypeId: number = 3;
    public get ReviewSets(): ReviewSet[] {
        return this.ReviewSetsService.ReviewSets;
    }
    public get SetTypes(): iSetType[] {
        return this.ReviewSetsEditingService.SetTypes;
    }
    public get NewSetSelectedTypeDescription(): string {
        let found = this.ReviewSetsEditingService.SetTypes.find(found => found.setTypeId == this.NewSetSelectedTypeId);
        if (found) return found.setTypeDescription;
        else return "";
    }
    private _NewReviewSet: ReviewSet = new ReviewSet();
    public get NewReviewSet(): ReviewSet {
        return this._NewReviewSet;
    }
    private _NewCode: SetAttribute = new SetAttribute();
    public get NewCode(): SetAttribute {
        return this._NewCode;
    }
    public get CurrentNode(): singleNode | null {
        if (!this.ReviewSetsService.selectedNode) return null;
        else return this.ReviewSetsService.selectedNode;
    }
    IsServiceBusy(): boolean {
        if (this.ReviewSetsService.IsBusy || this.ReviewSetsEditingService.IsBusy || this.ReviewInfoService.IsBusy) return true;
        else return false;
    }
    CanWrite(): boolean {
        //console.log('CanWrite', this.ReviewerIdentityServ.HasWriteRights, this.IsServiceBusy());
        if (this.ReviewerIdentityServ.HasWriteRights && !this.IsServiceBusy()) {
            //console.log('CanWrite', true);
            return true;
        }
        else {
            //console.log('CanWrite', false);
            return false;
        }
    }
    private _appliedCodes: number = -1;
    public get appliedCodes(): number {
        return this._appliedCodes;
    }
    
    ShowDeleteCodeset: boolean = false;
    ShowDeleteCodesetClicked() {
        //console.log('0');
        if (this.ActivityPanelName == "EditCodeset") {
            if (this.CurrentNode) {
                //console.log('1');
                let Set: ReviewSet = this.CurrentNode as ReviewSet;
                if (Set) {
                    //console.log('2');
                    this._appliedCodes = -1;
                    this.ShowDeleteCodeset = true;
                    this.ReviewSetsEditingService.AttributeOrSetDeleteCheck(Set.set_id, 0).then(
                        success => {
                            //alert("did it");
                            this._appliedCodes = success;
                            //return result;
                        },
                        error => {
                            //alert("Sorry, creating the new codeset failed.");
                            //this.modalService.GenericErrorMessage(ErrMsg);
                        });
                }
            }
        }
        else if (this.ActivityPanelName == "EditCode" && this._UpdatingCode) {
            this._appliedCodes = -1;
            this.ShowDeleteCodeset = true;
            this.ReviewSetsEditingService.AttributeOrSetDeleteCheck(0, this._UpdatingCode.attributeSetId).then(
                success => {
                    //alert("did it");
                    this._appliedCodes = success;
                    //return result;
                },
                error => {
                    //alert("Sorry, creating the new codeset failed.");
                    //this.modalService.GenericErrorMessage(ErrMsg);
                });
        }

    }
    HideDeleteCodeset() {
        this.ShowDeleteCodeset = false;
        this._appliedCodes = -1;
    }
    DoDeleteCodeset() {
        if (!this.CurrentNode) return;
        let Set: ReviewSet = this.CurrentNode as ReviewSet;
        if (!Set) return;
        console.log("will delete:", Set);
        this.ReviewSetsEditingService.ReviewSetDelete(Set)
            .then(
                success => {
                    //alert("did it");
                    this.ReviewSetsService.selectedNode = null;
                    this.ReviewSetsService.GetReviewSets();
                    this.CancelActivity();
                    //return result;
                },
            error => {
                this.ReviewSetsService.selectedNode = null;
                this.ReviewSetsService.GetReviewSets();
                this.CancelActivity();
                    //alert("Sorry, creating the new codeset failed.");
                    //this.modalService.GenericErrorMessage(ErrMsg);
                });
    }
    UpdateCodeSet() {
        if (!this.CurrentNode) return;
        let Set: ReviewSet = this.CurrentNode as ReviewSet;
        if (!Set) return;
        console.log("will update:", Set, this.CurrentNode);
        this.ReviewSetsEditingService.SaveReviewSet(Set);
        this.CancelActivity(); 
    }
    public ShowChangeDataEntry: boolean = false;
    public DestinationDataEntryMode: string = "";
    public ChangeDataEntryModeMessage = "";
    private _CanChangeDataEntryMode: boolean = false;
    public get CanChangeDataEntryMode(): boolean {
        return (this._CanChangeDataEntryMode && this.CanWrite() && this._ItemsWithIncompleteCoding != -1);
    }
    private _ItemsWithIncompleteCoding: number = -1;
    public get ItemsWithIncompleteCoding(): number {
        return this._ItemsWithIncompleteCoding;
    }
    public get AllowedChildTypes(): kvAllowedAttributeType[] {
        let res: kvAllowedAttributeType[] = [];
        if (!this.CurrentNode) return res;
        let att: SetAttribute | null = null;
        let Set: ReviewSet | null = null;
        if (this.CurrentNode.nodeType == "ReviewSet") Set = this.CurrentNode as ReviewSet;
        else if (this.CurrentNode.nodeType == "SetAttribute") {
            att = this.CurrentNode as SetAttribute;
            if (att && att.set_id > 0) Set = this.ReviewSetsService.FindSetById(att.set_id);
            if (!Set) return res;
        }
        //console.log("CurrentNode (Set)", Set);
        if (Set && Set.setType) {
            //console.log("allowed child types... ", Set.setType.allowedCodeTypes, Set.setType.allowedCodeTypes[0].key, Set.setType.allowedCodeTypes.filter(res => !res.value.endsWith('- N/A)')));
            return Set.setType.allowedCodeTypes.filter(res => !res.value.endsWith('- N/A)') );
        }
        return res;
    }
    public get CurrentCodeLevel(): number {
        if (!this.CurrentNode) return -1;//??
        else if (this.CurrentNode.nodeType == "ReviewSet") return 0;
        else if (this.CurrentNode.nodeType == "SetAttribute") return this.ReviewSetsService.AttributeCurrentLevel(this.ReviewSetsService.selectedNode as SetAttribute);
        else return -1;//??
    }
    public get CurrentCodeCanHaveChildren(): boolean {
        //safety first, if anything didn't work as expexcted return false;
        if (!this.CanWrite()) return false;
        else {
			if (!this.CurrentNode) return false;//??
			//move the below to ReviewSetsService;
            else if (this.CurrentNode.nodeType == "ReviewSet" && this.CurrentNode.allowEditingCodeset) return true;
            else if (this.CurrentNode.nodeType == "SetAttribute") {
                let Att: SetAttribute = this.CurrentNode as SetAttribute;
                let Set: ReviewSet | null = this.ReviewSetsService.FindSetById(Att.set_id);
                //console.log("I'm still checking: ", Att, Set);
                if (Set && Set.setType) {
                    let maxDepth: number = Set.setType.maxDepth;
                    //console.log("I'm still checking2: ", maxDepth > this.ReviewSetsService.AttributeCurrentLevel(Att));
                    return maxDepth > this.ReviewSetsService.AttributeCurrentLevel(Att);
                }
                else return false;
			}
			//end of bit that goes into "ReviewSetsService.CanNodeHaveChildren(node: singleNode): boolean"
        }
        return false;
    }
    public SetTypeCanChangeDataEntryMode(): boolean {
        let Set: ReviewSet = this.CurrentNode as ReviewSet;
        if (Set && Set.setType && Set.setType.allowComparison) return true;
        else return false;
    }
    public NewSetTypeCanChangeDataEntryMode(): boolean {
        if (this.NewReviewSet && !this.NewReviewSet.setType) {
            let found = this.ReviewSetsEditingService.SetTypes.find(found => found.setTypeId == this.NewSetSelectedTypeId);
            if (found) this._NewReviewSet.setType = found;
        }
        if (this.NewReviewSet && this.NewReviewSet.setType && this.NewReviewSet.setType.allowComparison) return true;

        else return false;
    }
    ShowChangeDataEntryClicked() {
        this._ItemsWithIncompleteCoding = -1;
        if (this.CurrentNode) {
            let Set: ReviewSet = this.CurrentNode as ReviewSet;
            if (Set) {
                this.ShowChangeDataEntry = true;
                if (Set.set_id == this.ReviewInfoService.ReviewInfo.screeningCodeSetId) {
                    this.DestinationDataEntryMode = "";
                    this.ChangeDataEntryModeMessage = "This set is your current Screening Set (used for Priority Screening).";
                    this.ChangeDataEntryModeMessage += "\r\nChanging the data entry mode would require to review/update the Priority Screening settings.";
                    this.ChangeDataEntryModeMessage += "\r\nUnfortuately this feature is not currently implemented in the current App.";
                    this.ChangeDataEntryModeMessage += "\r\nTo apply this change please use the full (Silverlight) version or EPPI-Reviewer 4.";
                    this.DestinationDataEntryMode = "";
                    this._CanChangeDataEntryMode = false;
                    return;
                }
                else if (Set.codingIsFinal) {//moving to comparison data entry, easy!
                    this.DestinationDataEntryMode = "Comparison";
                    this.ChangeDataEntryModeMessage = "Are you sure you want to change to 'Comparison' data entry?";
                    this.ChangeDataEntryModeMessage += "\r\nThis implies that you will have multiple users coding the same item using this codeset and then reconciling the disagreements.";
                    this.ChangeDataEntryModeMessage += "\r\nPlease ensure you have read the manual to check the implications of this.";
                    this._ItemsWithIncompleteCoding = 0;
                    this._CanChangeDataEntryMode = true;
                }
                else {//moving to normal data entry, need to check "troublesome items"
                    this.DestinationDataEntryMode = "Normal";
                    this.ChangeDataEntryModeMessage = "";
                    this.ReviewSetsEditingService.ReviewSetCheckCodingStatus(Set.set_id).then(
                        success => {
                            //alert("did it");
                            this._ItemsWithIncompleteCoding = success;
                            if (this._ItemsWithIncompleteCoding > 0) {
                                this.ChangeDataEntryModeMessage = "You are about to change your data entry method to 'Normal', ";
                                this.ChangeDataEntryModeMessage += "but there are '" + this._ItemsWithIncompleteCoding + "' items that should be completed before you proceed. ";
                                this.ChangeDataEntryModeMessage += "You can view these incomplete items from the 'Review Home' screen.";
                                this._CanChangeDataEntryMode = true;
                            }
                            else if (this._ItemsWithIncompleteCoding == 0) {
                                this.ChangeDataEntryModeMessage = "You are about to change your data entry method to 'Normal'. \nThere are no potential data conflicts so it is safe to proceed.";
                                this._CanChangeDataEntryMode = true;
                            }
                            else {//error in the service, returned -1
                                this.ChangeDataEntryModeMessage = "Sorry, could not check coding status, thus, you should change the data entry mode. ";
                                this.ChangeDataEntryModeMessage += "If the problem persist, please contact EPPISupport.";
                                this._CanChangeDataEntryMode = false;
                            }
                        },
                        error => {
                            console.log("ERROR IN: ShowChangeDataEntryClicked API result", error);
                            this.ChangeDataEntryModeMessage = "Sorry, could not check coding status, thus, you should change the data entry mode. ";
                            this.ChangeDataEntryModeMessage += "If the problem persist, please contact EPPISupport.";
                            this._CanChangeDataEntryMode = false;
                        });
                }
                
            }
        }
        //this.ShowChangeDataEntry = true;
    }
    DoChangeDataEntry() {
        if (!this.CurrentNode || !this._CanChangeDataEntryMode) return;
        let Set: ReviewSet = this.CurrentNode as ReviewSet;
        if (!Set) return;
        Set.codingIsFinal = !Set.codingIsFinal;
        console.log("will update DataEntry:", Set, this.CurrentNode);
        this.ReviewSetsEditingService.SaveReviewSet(Set);
        this.CancelActivity(); 
    }
    HideChangeDataEntry() {
        this.ShowChangeDataEntry = false;
        this.ChangeDataEntryModeMessage = "";
        this._CanChangeDataEntryMode = false;
    }
    CanChangeSelectedCode(): boolean {
        //console.log('CanChangeSelectedCode', this.ActivityPanelName);
        if (this.ActivityPanelName == "") return true;
        else {
            //console.log('CanChangeSelectedCode', this.ActivityPanelName, false);
            return false;
        }
    }
    SetIsSelected(): boolean {
        if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "ReviewSet") return true;
        else return false;
    }
    AttributeIsSelected(): boolean {
        if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "SetAttribute") return true;
        else return false;
    }
    ShowActivity(activityName: string) {
        if (this._ActivityPanelName != "") this.CancelActivity(true);
        this._ActivityPanelName = activityName;
    }
    CancelActivity(refreshTree?: boolean) {
        if (refreshTree) {
            if (this.ReviewSetsService.selectedNode) {
                let IsSet: boolean = this.ReviewSetsService.selectedNode.nodeType == "ReviewSet";
                let Id: number = -1;
                if (IsSet) Id = (this.ReviewSetsService.selectedNode as ReviewSet).set_id;
                else Id = (this.ReviewSetsService.selectedNode as SetAttribute).attribute_id;
                let sub: Subscription = this.ReviewSetsService.GetReviewStatsEmit.subscribe(() => {
                    console.log("trying to reselect: ", Id);
                    if (IsSet) this.ReviewSetsService.selectedNode = this.ReviewSetsService.FindSetById(Id);
                    else this.ReviewSetsService.selectedNode = this.ReviewSetsService.FindAttributeById(Id);
                    if (sub) sub.unsubscribe();
                }
                    , () => { if (sub) sub.unsubscribe(); }
                );
                this.ReviewSetsService.selectedNode = null;
                this.ReviewSetsService.GetReviewSets();
            }
        }
        this.ShowChangeDataEntry = false;
        this.ShowDeleteCodeset = false;
        this._appliedCodes = -1;
        this._ItemsWithIncompleteCoding = -1;
        this._CanChangeDataEntryMode = false;
        this.DestinationDataEntryMode = "";
        this.ChangeDataEntryModeMessage = "";
        this._ActivityPanelName = "";
    }
    CodesetTypeChanged(typeId: number) {
        this.NewSetSelectedTypeId = typeId;
        let current = this.ReviewSetsEditingService.SetTypes.find(found => found.setTypeId == this.NewSetSelectedTypeId);
        if (current) {
            this._NewReviewSet.setType = current;
            if (!current.allowComparison) this._NewReviewSet.codingIsFinal = true;
        }
    }
    IsNewSetNameValid() {
        if (this._NewReviewSet.set_name.trim() != "") return true;
        else return false;
    }
    IsNewCodeNameValid() {
        if (this.ActivityPanelName == 'AddCode') {
            if (this._NewCode.attribute_name.trim() != "") return true;
            else return false;
        } else if (this.ActivityPanelName == 'EditCode'){
            if (this._UpdatingCode && this._UpdatingCode.attribute_name.trim() != "") return true;
            else return false;
        }
    }
    IsEditingNodeNameValid() {
        if (this.CurrentNode && this.CurrentNode.name.trim().length > 0) return true;
        else return false;
    }
    
    public get CurrentCodesetDataEntryMode(): string {
        if (!this.CurrentNode) return "";
        let Set: ReviewSet = this.CurrentNode as ReviewSet;
        if (!Set) return "";
        if (Set.codingIsFinal) return "Normal";
        else return "Comparison";
    }
    CreateNewCodeSet() {
        console.log("will create:", this._NewReviewSet);
        if (!this._NewReviewSet.setType) {
            let found = this.ReviewSetsEditingService.SetTypes.find(found => found.setTypeId == this.NewSetSelectedTypeId);
            if (found) this._NewReviewSet.setType = found;
            else return;
        }
        this._NewReviewSet.order = this.ReviewSetsService.ReviewSets.length;
        this.ReviewSetsEditingService.SaveNewReviewSet(this._NewReviewSet)
        .then(
            success => {
                //alert("did it");
                let result: ReviewSet = new ReviewSet();
                if (success) result = ReviewSetsService.digestJsonReviewSet(success);
                //if (result.set_id < 1) this.modalService.GenericErrorMessage(ErrMsg);
                this.ReviewSetsService.ReviewSets.push(result);
                if (this.treeEditorComponent) this.treeEditorComponent.RefreshLocalTree();
                this._NewReviewSet = new ReviewSet();
                this.CancelActivity();
                //return result;
            },
            error => {
                //alert("Sorry, creating the new codeset failed.");
                //this.modalService.GenericErrorMessage(ErrMsg);
            });
    }
    
    CreateNewCode() {
        if (this.CurrentNode) {
            this._NewCode.order = this.CurrentNode.attributes.length;
            
            if (this.CurrentNode.nodeType == "ReviewSet") {
                this._NewCode.set_id = (this.CurrentNode as ReviewSet).set_id;
                this._NewCode.parent_attribute_id = 0;
            }
            else if (this.CurrentNode.nodeType == "SetAttribute") {
                this._NewCode.set_id = (this.CurrentNode as SetAttribute).set_id;
                this._NewCode.parent_attribute_id = (this.CurrentNode as SetAttribute).attribute_id;
            }
        }
        else {
            this._NewReviewSet.order = 0;
        }
        //console.log("What?", this.CodeTypeSelect);//, this.CodeTypeSelect.selectedOptions, this.CodeTypeSelect.selectedOptions.length);
        console.log("What?", this.CodeTypeSelect, this.CodeTypeSelect.nativeElement.selectedOptions, this.CodeTypeSelect.nativeElement.selectedOptions.length);
        if (this.CodeTypeSelect && this.CodeTypeSelect.nativeElement.selectedOptions && this.CodeTypeSelect.nativeElement.selectedOptions.length > 0) {
            this._NewCode.attribute_type_id = this.CodeTypeSelect.nativeElement.selectedOptions[0].value;
            this._NewCode.attribute_type = this.CodeTypeSelect.nativeElement.selectedOptions[0].text;
        }
        else {
            this._NewCode.attribute_type_id = 1;//non selectable HARDCODED WARNING!
            this._NewCode.attribute_type = "Not selectable(no checkbox)";
        }
       
        console.log("will create:", this._NewCode, this.CodeTypeSelect);
        this.ReviewSetsEditingService.SaveNewAttribute(this._NewCode)
            .then(
                success => {
                    //alert("did it");
                    if (success && this.CurrentNode) {
                        this.CurrentNode.attributes.push(success);
                        if (this.treeEditorComponent) this.treeEditorComponent.RefreshLocalTree();
                    }
                    //eventual error messages have been shown by the service...
                    this._NewCode = new SetAttribute();
                    this.CancelActivity();
                    //return result;
                },
            error => {
                this.CancelActivity();
                console.log("error saving new code:", error, this._NewCode);
                    //alert("Sorry, creating the new codeset failed.");
                    //this.modalService.GenericErrorMessage(ErrMsg);
            })
            .catch(
            error => {
                console.log("error(catch) saving new code:", error, this._NewCode);
                this.CancelActivity();
            }
            );
    }
    private _UpdatingCode: SetAttribute | null = null;
    public get UpdatingCode(): SetAttribute | null {
        if (this.ActivityPanelName != 'EditCode') {
            this._UpdatingCode = null;
        }
        else if (this._UpdatingCode == null && this.CurrentNode && this.CurrentNode.nodeType == "SetAttribute") {
            this._UpdatingCode = this.CurrentNode as SetAttribute;
        }
        else if (this._UpdatingCode && this._UpdatingCode.attribute_id != (this.CurrentNode as SetAttribute).attribute_id) {
            this._UpdatingCode = this.CurrentNode as SetAttribute;
        }
        return this._UpdatingCode;
    }
    AttributeTypeChanged(typeId: number) {
        console.log('selected att type:' + typeId);
        if (this.ActivityPanelName == 'AddCode') {
            //this.NewSetSelectedTypeId = typeId;
            //let current = this.ReviewSetsEditingService.SetTypes.find(found => found.setTypeId == this.NewSetSelectedTypeId);
            //if (current) {
            //    this._NewReviewSet.setType = current;
            //    if (!current.allowComparison) this._NewReviewSet.codingIsFinal = true;
            //}
        } else if (this.ActivityPanelName == 'EditCode' && this.UpdatingCode && this.UpdatingCode.nodeType == "SetAttribute") {
            this.UpdatingCode.attribute_type_id = typeId;
            let foundT = this.AllowedChildTypes.find(found => found.key == typeId);
            if (foundT) this.UpdatingCode.attribute_type = foundT.value;
        }
    }
    UpdateCode() {
        if (!this.UpdatingCode || this.UpdatingCode.nodeType != "SetAttribute") {
            this.CancelActivity();
            return;//fail silently, should be ok as it should never happen...
        }
        let Att = this.UpdatingCode;
        this.ReviewSetsEditingService.UpdateAttribute(Att)
            .then(
                success => {
                    //alert("did it");
                    if (success) {
                        this.ReviewSetsService.selectedNode = Att;
                        let OldAtt = this.ReviewSetsService.FindAttributeById(Att.attribute_id);
                        if (OldAtt) {
                            if (OldAtt.parent_attribute_id == 0) {
                                let Set = this.ReviewSetsService.FindSetById(Att.set_id);
                                if (Set) {
                                    let index = Set.attributes.findIndex(Old => OldAtt === Old);
                                    if (index != -1) {
                                        console.log("replacing updated Att...");
                                        Set.attributes.splice(index, 1, Att);
                                    }
                                }
                            }
                            else {
                                let Par = this.ReviewSetsService.FindAttributeById(Att.parent_attribute_id);
                                if (Par) {
                                    let index = Par.attributes.findIndex(Old => OldAtt === Old);
                                    if (index != -1) Par.attributes.splice(index, 1, Att);
                                }
                            }
                            if (this.treeEditorComponent) this.treeEditorComponent.RefreshLocalTree();
                        }
                        //eventual error messages have been shown by the service...
                        this.CancelActivity();
                    }
                },
                error => {
                    this.CancelActivity();
                    console.log("error updating code:", error, Att);
                    
                })
            .catch(
                error => {
                    console.log("error(catch) updating code:", error, Att);
                    this.CancelActivity();
                }
            );
    }
    DoDeleteCode() {
        if (!this._UpdatingCode) return;
       
        console.log("will delete:", this._UpdatingCode);
        this.ReviewSetsEditingService.SetAttributeDelete(this._UpdatingCode)
            .then(
                success => {
                    //alert("did it");
                    this.ReviewSetsService.selectedNode = null;
                    this.ReviewSetsService.GetReviewSets();
                    this.CancelActivity(true);
                },
                error => {
                    this.ReviewSetsService.selectedNode = null;
                    this.ReviewSetsService.GetReviewSets();
                    this.CancelActivity(true);
                });
    }
    BackToMain() {
        this.router.navigate(['Main']);
    }
    ngOnDestroy() {
    }
}
