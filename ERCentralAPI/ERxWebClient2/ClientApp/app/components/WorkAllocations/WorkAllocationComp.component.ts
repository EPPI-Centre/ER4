/// <reference path="../services/itemlist.service.ts" />
import { Component, Inject, OnInit, EventEmitter, Output, AfterContentInit, OnDestroy, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subscription, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocationListService, WorkAllocation } from '../services/WorkAllocationList.service';
import { ItemListService } from '../services/ItemList.service'
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';

@Component({
	selector: 'WorkAllocationComp',
	templateUrl: './WorkAllocationComp.component.html',
    styles: ['.UsedWorkAllocation { font-weight: bold; background-color: lightblue;}'],
    providers: []
})

export class WorkAllocationComp implements OnInit {
    constructor(
    private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
		public _workAllocationListService: WorkAllocationListService,
		public reviewInfoService: ReviewInfoService,
		public itemListService: ItemListService
    ) { }

    ngOnInit() {

		this.getMembers();
		this._workAllocationListService.FetchAll();
    }


	getMembers() {

		if (!this.reviewInfoService.ReviewInfo || this.reviewInfoService.ReviewInfo.reviewId < 1) {
						this.reviewInfoService.Fetch();
		}
		this.reviewInfoService.FetchReviewMembers();

	}

	DeleteWorkAllocation() {

		alert('not implemented yet');
	}

	// this needs to open the item list as in ER4...
	WAGetItemList(i: any) {

		alert('asdf: ' + i);
	}
  
}





