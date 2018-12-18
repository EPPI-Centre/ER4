import { Component, Inject, OnInit, Input, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, iSetType, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';

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
        if (this.ReviewSetsEditingService.SetTypes.length == 0) {
            this.ReviewSetsEditingService.FetchSetTypes();
        }
    }
    private ActivityPanelName: string = "";
    public get ReviewSets(): ReviewSet[] {
        return this.ReviewSetsService.ReviewSets;
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
        if (this.ReviewerIdentityServ.HasWriteRights && !this.ReviewSetsService.IsBusy && !this.ReviewSetsEditingService.IsBusy) return true;
        else return false;
    }
    CanChangeSelectedCode(): boolean {
        if (this.ActivityPanelName == "") return true;
        else return false;
    }
    SetIsSelected(): boolean {
        if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "ReviewSet") return true;
        else return false;
    }
    AttributeIsSelected(): boolean {
        if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "SetAttribute") return true;
        else return false;
    }
    private _ShowAddCode: boolean = false;
    ShowActivity(activityName: string) {
        this.ActivityPanelName = activityName;
    }
    CancelActivity() {
        this.ActivityPanelName = "";
    }
    BackToMain() {
        this.router.navigate(['Main']);
    }
    ngOnDestroy() {
    }
}
