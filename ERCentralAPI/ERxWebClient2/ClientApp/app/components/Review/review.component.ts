import { Component, Inject, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { BuildModelService } from '../services/buildmodel.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

@Component({
	selector: 'ReviewComp',
	templateUrl: './review.component.html',
	providers: []
})

export class ReviewComponent implements OnInit, OnDestroy {
	constructor(private router: Router,
		@Inject('BASE_URL') private _baseUrl: string,
		private _reviewSetsService: ReviewSetsService,
		public _buildModelService: BuildModelService,
		public _eventEmitterService: EventEmitterService,
		public _reviewService: ReviewService,
		public _reviewerIdentityServ: ReviewerIdentityService
	)
	{ }

	ngOnInit() {

	}
	CanWrite(): boolean {
		if (this._reviewerIdentityServ.HasWriteRights && !this._reviewService.IsBusy) {
			return true;
		} else {
			return false;
		}
	}

	public RevId: number = 0;
	public reviewN: string = '';

	CreateReview(reviewN: string) {

		if (this.CanWrite()) {
				
			this._reviewService.CreateReview(reviewN, this._reviewerIdentityServ.reviewerIdentity.userId.toString()).then(
				(res: any) => {
					if (res != null) {
						this.onFullSubmit(res);
					}
				});
		}
	}

	CanCreateReview() {

		if (this.CanWrite() && this.reviewN != '') {
			return true;
		}

	}
	onFullSubmit(revId: number) {
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
