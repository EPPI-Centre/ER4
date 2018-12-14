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
        if (this.ReviewSetsService.ReviewSets.length == 0) {
            this.ReviewSetsService.GetReviewSets();
        } else {
            this.CheckReviewSetsOrder();
        }
    }
    public get SetTypes(): iSetType[] {
        return this.ReviewSetsEditingService.SetTypes;
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
    CheckReviewSetsOrder() {
        let i: number = 0;
        let changedSomething: boolean = false;
        for (let rs of this.ReviewSetsService.ReviewSets) {
            if (rs.order != i) {
                rs.order = i;
                changedSomething = true;
                //do something to save change!
                this.ReviewSetsEditingService.SaveReviewSet(rs);
                //recursive CheckAttributeSet of attributes?
            }
            i++;
        }
        if (changedSomething) {
            //for sanity, sort by order;
            this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
                return s1.order - s2.order;
            });
        }
    }
    MoveDownSet(rs: ReviewSet) {
        let index = this.ReviewSetsService.ReviewSets.findIndex(found => found.set_id == rs.set_id);
        if (index == -1 || index == this.ReviewSetsService.ReviewSets.length - 1) {
            //oh! should not happen... do nothing?
            return;
        }
        let swapper: ReviewSet = this.ReviewSetsService.ReviewSets[index + 1];
        rs.order++;
        this.ReviewSetsEditingService.SaveReviewSet(rs);
        if (swapper) {
            swapper.order--;
            this.ReviewSetsEditingService.SaveReviewSet(swapper);
        }
        //now sort by order;
        this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
            return s1.order - s2.order;
        });
    }
    MoveUpSet(rs: ReviewSet) {
        //console.log("before:", rs, rs.order);
        let index = this.ReviewSetsService.ReviewSets.findIndex(found => found.set_id == rs.set_id);
        if (index <= 0) {
            //oh! should not happen... do nothing?
            return;
        }
        let swapper: ReviewSet = this.ReviewSetsService.ReviewSets[index -1];
        //console.log("mid1:", rs, rs.order);
        rs.order = rs.order - 1;
        //console.log("mid2 :", rs, rs.order);
        this.ReviewSetsEditingService.SaveReviewSet(rs);
        if (swapper) {
            swapper.order = swapper.order + 1;
            this.ReviewSetsEditingService.SaveReviewSet(swapper);
        }
        //now sort by order;
        this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
            return s1.order - s2.order;
        });

        //console.log("after:", rs, rs.order);
    }
    BackToMain() {
        this.router.navigate(['Main']);
    }
    ngOnDestroy() {
    }
}
