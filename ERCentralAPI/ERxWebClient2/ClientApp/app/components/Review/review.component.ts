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

		// do some stuff about creating a review here


	}
	public RevId: number = 0;;
	CreateReview() {

		// Hardcode these for now...
		this.RevId = this._reviewService.CreateReview('test4', '1800');

		// if the above line is successful then need to call the
		// readonlyreviewstuff
		this.onFullSubmit();

	}
	onFullSubmit() {
			
		//console.log('all the way to here: ' +  RevId);
		this._reviewerIdentityServ.LoginToFullReview(this.RevId);
	}
	BackToMain() {
		
		this.router.navigate(['Main']);
	}
	ngOnDestroy() {

		this._reviewSetsService.selectedNode = null;
	}

	ngAfterViewInit() {

	}
}
