import { Component, Inject, OnInit, Input, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, iSetType, ReviewSet } from '../services/ReviewSets.service';
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
    public get ReviewSets(): ReviewSet[] {
        return this.ReviewSetsService.ReviewSets;
    }
    private _CurrentReviewSet: ReviewSet | null = null;
    public get CurrentReviewSet(): ReviewSet | null {
        return this._CurrentReviewSet;
    }
    SelectReviewSet(rs: ReviewSet) {
        this._CurrentReviewSet = rs;
    }
    IsServiceBusy(): boolean {
        if (this.ReviewSetsService.IsBusy || this.ReviewSetsEditingService.IsBusy) return true;
        else return false;
    }
    CanWrite(): boolean {
        if (this.ReviewerIdentityServ.HasWriteRights && !this.ReviewSetsService.IsBusy && !this.ReviewSetsEditingService.IsBusy) return true;
        else return false;
    }
    
    BackToMain() {
        this.router.navigate(['Main']);
    }
    ngOnDestroy() {
    }
}
