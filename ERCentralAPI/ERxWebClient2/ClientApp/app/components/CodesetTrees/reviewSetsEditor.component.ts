import { Component, Inject, OnInit, Input, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, iSetType, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { CodesetTreeEditComponent } from './codesetTreeEdit.component';
import { ReviewInfoService } from '../services/ReviewInfo.service';

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
        if (this.CurrentNode) {
            let Set: ReviewSet = this.CurrentNode as ReviewSet;
            if (Set) {
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
    HideDeleteCodeset() {
        this.ShowDeleteCodeset = false;
        this._appliedCodes = -1;
    }
    DoDeleteCodeset() {
        if (!this.CurrentNode) return;
        let Set: ReviewSet = this.CurrentNode as ReviewSet;
        if (!Set) return;
        console.log("will create:", this._NewReviewSet);
        this.ReviewSetsEditingService.ReviewSetDelete(Set)
            .then(
                success => {//we want to update the tree accordingly, without going to fetch latest as we'll lose the tree state.
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
    public SetTypeCanChangeDataEntryMode(): boolean {
        let Set: ReviewSet = this.CurrentNode as ReviewSet;
        if (Set && Set.setType && Set.setType.allowComparison) return true;
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
        this._ActivityPanelName = activityName;
    }
    CancelActivity() {
        this.ShowChangeDataEntry = false;
        this.ShowDeleteCodeset = false;
        this._appliedCodes = -1;
        this._ItemsWithIncompleteCoding = -1;
        this._CanChangeDataEntryMode = false;
        this.DestinationDataEntryMode = "";
        this.ChangeDataEntryModeMessage = "";
        this._ActivityPanelName = "";
    }
    TypeChanged(typeId: number) {
        this.NewSetSelectedTypeId = typeId;
        let current = this.ReviewSetsEditingService.SetTypes.find(found => found.setTypeId == this.NewSetSelectedTypeId);
        if (current) this._NewReviewSet.setType = current;
    }
    IsNewSetNameValid() {
        if (this._NewReviewSet.set_name.trim() != "") return true;
        else return false;
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
    
    BackToMain() {
        this.router.navigate(['Main']);
    }
    ngOnDestroy() {
    }
}
