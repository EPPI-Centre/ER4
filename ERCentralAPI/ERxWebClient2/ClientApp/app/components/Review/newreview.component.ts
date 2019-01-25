import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { readonlyreviewsService } from '../services/readonlyreviews.service';

@Component({
	selector: 'NewReviewComp',
	templateUrl: './newreview.component.html',
	providers: []
})

export class NewReviewComponent implements OnInit, OnDestroy {
	constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        public _reviewService: ReviewService,
        private readonlyreviewsService: readonlyreviewsService,
		public _reviewerIdentityServ: ReviewerIdentityService,
		public modalService: ModalService
	)
	{ }

	//@Output() onCloseClick = new EventEmitter();
    public isExpanded: boolean = false;
	ngOnInit() {

	}
    CanWrite(): boolean {
        //console.log("create rev check:", this._reviewerIdentityServ.reviewerIdentity);
        if (!this._reviewService.IsBusy) {
            if (!this._reviewerIdentityServ.HasWriteRights) {
                //one more check: is the user in the first screen?
                if (this._reviewerIdentityServ.reviewerIdentity.reviewId == 0
                    && this._reviewerIdentityServ.reviewerIdentity.ticket === ""
                    && this._reviewerIdentityServ.reviewerIdentity.token
                    && this._reviewerIdentityServ.reviewerIdentity.token.length > 100
                    && this._reviewerIdentityServ.reviewerIdentity.roles
                    && this._reviewerIdentityServ.reviewerIdentity.roles.length > 0
                    && this._reviewerIdentityServ.reviewerIdentity.roles.indexOf('ReadOnlyUser') == -1
                    && this._reviewerIdentityServ.reviewerIdentity.daysLeftAccount >= 0
                )
                    return true;
                else return false;
            }
			else return true;
        } else {
			return false;
		}
    }
    Expand() {
        this.isExpanded = true;
    }
    CloseNewReview() {
        this.reviewN = "";
        this.isExpanded = false;
	}
	public RevId: number = 0;
	public reviewN: string = '';

	CreateReview() {

		if (this.CanWrite()) {

			this._reviewService.CreateReview(this.reviewN, this._reviewerIdentityServ.reviewerIdentity.userId.toString()).then(
				(res: number) => {
                    if (res != null) {
                        if (res < 1) return;
						this.onFullSubmit(res);
					}
				}
			);
		}
	}

	CanCreateReview() {
		if (this.CanWrite() && this.reviewN != '') {
			return true;
		}
	}
    onFullSubmit(revId: number) {
        this.readonlyreviewsService.Fetch();
        this.CloseNewReview();
		this._reviewerIdentityServ.LoginToFullReview(revId);
	}
	BackToMain() {
		this.router.navigate(['Main']);
	}
	ngOnDestroy() {
		
	}
	ngAfterViewInit() {

	}
}
