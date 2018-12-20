import { Component, Inject, OnInit, Input, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, iSetType, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { CodesetTreeEditComponent } from './codesetTreeEdit.component';

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
    ) { }
    ngOnInit() {
        console.log
        if (this.ReviewSetsEditingService.SetTypes.length == 0) {
            this.ReviewSetsEditingService.FetchSetTypes();
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
        if (this.ReviewSetsService.IsBusy || this.ReviewSetsEditingService.IsBusy) return true;
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
    ShowChangeDataEntry: boolean = false;
    ShowChangeDataEntryClicked() {
        this.ShowChangeDataEntry = true;
    }
    HideChangeDataEntry() {
        this.ShowChangeDataEntry = false;
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
