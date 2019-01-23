import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { BuildModelService } from '../services/buildmodel.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';

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
		public _reviewerIdentityServ: ReviewerIdentityService,
		public modalService: ModalService
	)
	{ }

	@Output() onCloseClick = new EventEmitter();

	ngOnInit() {

	}
	CanWrite(): boolean {
		if (this._reviewerIdentityServ.HasWriteRights && !this._reviewService.IsBusy) {
			return true;
		} else {
			return false;
		}
	}
	CloseNewReview() {
		this.onCloseClick.emit(false);
	}
	public RevId: number = 0;
	public reviewN: string = '';

	CreateReview() {

		if (this.CanWrite()) {

			this._reviewService.CreateReview(this.reviewN, this._reviewerIdentityServ.reviewerIdentity.userId.toString()).then(
				(res: any) => {
					if (res != null) {
						this.onFullSubmit(res);
					}
				},
				(error: any) => {
					this.modalService.GenericError(error);

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
