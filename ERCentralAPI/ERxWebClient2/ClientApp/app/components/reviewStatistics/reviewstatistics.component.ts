import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationContactList.service'
import { Criteria, ItemList } from '../services/ItemList.service'
import { WorkAllocationContactListComp } from '../WorkAllocationContactList/workAllocationContactListComp.component';
import { ItemListService } from '../services/ItemList.service'
import { ItemListComp } from '../ItemList/itemListComp.component';
import {  Subject, Subscription } from 'rxjs';
import { ReviewSetsService, ReviewSet } from '../services/ReviewSets.service';
import { CodesetStatisticsService, ReviewStatisticsCountsCommand, ReviewStatisticsCodeSet } from '../services/codesetstatistics.service';
import { NgbTabset } from '@ng-bootstrap/ng-bootstrap';


@Component({
	selector: 'reviewStatisticsComp',
	templateUrl: './reviewstatistics.component.html',
    providers: []
})

export class ReviewStatisticsComp implements OnInit, OnDestroy, AfterViewInit {
	constructor(private router: Router,
		public ReviewerIdentityServ: ReviewerIdentityService,
		private reviewSetsService: ReviewSetsService,
		@Inject('BASE_URL') private _baseUrl: string,
		private _httpC: HttpClient,
		private ItemListService: ItemListService,
		public codesetStatsServ: CodesetStatisticsService
	) {

	}
  

	@ViewChild('WorkAllocationContactList') workAllocationsComp!: WorkAllocationContactListComp;
	@ViewChild('tabset') tabset!: NgbTabset;
	@ViewChild('ItemList') ItemListComponent!: ItemListComp;

	public stats: ReviewStatisticsCountsCommand | null = null;
	public countDown: any | undefined;
	public count: number = 60;
	public isReviewPanelCollapsed = false;
	public isWorkAllocationsPanelCollapsed = false;
	private statsSub: Subscription = new Subscription();

	dtOptions: DataTables.Settings = {};
	dtTrigger: Subject<any> = new Subject();

	ngOnInit() {

		this.dtOptions = {
			pagingType: 'full_numbers',
			paging: false,
			searching: false,
			scrollY: "350px"
		};
		this.subOpeningReview = this.ReviewerIdentityServ.OpeningNewReview.subscribe(() => this.Reload());
		this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
			() => this.GetStats()
		);
		if (this.codesetStatsServ.ReviewStats.itemsIncluded == -1
			|| (this.reviewSetsService.ReviewSets.length > 0 && this.codesetStatsServ.tmpCodesets.length == 0)
		) this.Reload();
	}

	public get ReviewPanelTogglingSymbol(): string {
		if (this.isReviewPanelCollapsed) return '&uarr;';
		else return '&darr;';
	}
	public get WorkAllocationsPanelTogglingSymbol(): string {
		if (this.isWorkAllocationsPanelCollapsed) return '&uarr;';
		else return '&darr;';
	}
	ngAfterViewInit() {


	}
	toggleReviewPanel() {
		this.isReviewPanelCollapsed = !this.isReviewPanelCollapsed;
	}
	toggleWorkAllocationsPanel() {
		this.isWorkAllocationsPanelCollapsed = !this.isWorkAllocationsPanelCollapsed;
	}
	getDaysLeftAccount() {

		return this.ReviewerIdentityServ.reviewerIdentity.daysLeftAccount;
	}
	getDaysLeftReview() {

		return this.ReviewerIdentityServ.reviewerIdentity.daysLeftReview;
	}
	onLogin(u: string, p: string) {

		this.ReviewerIdentityServ.LoginReq(u, p);

	};
	subOpeningReview: Subscription | null = null;
	   
	Reload() {
		this.Clear();
		this.reviewSetsService.GetReviewSets();
		if (this.workAllocationsComp) this.workAllocationsComp.getWorkAllocationContactList();
		else console.log("work allocs comp is undef :-(");
	}

	GetStats() {
		this.codesetStatsServ.GetReviewStatisticsCountsCommand();
		this.codesetStatsServ.GetReviewSetsCodingCounts(true, this.dtTrigger);
	}

	Clear() {
		this.ItemListService.SaveItems(new ItemList(), new Criteria());

		this.reviewSetsService.Clear();

		if (this.statsSub) this.statsSub.unsubscribe();
		this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
			() => this.GetStats()
		);
	}
	IncludedItemsList() {
		let cr: Criteria = new Criteria();
		cr.listType = 'StandardItemList';
		this.ItemListService.FetchWithCrit(cr, "Included Items");
		this.tabset.select('ItemListTab');
	}
	ExcludedItemsList() {
		let cr: Criteria = new Criteria();
		cr.listType = 'StandardItemList';
		cr.onlyIncluded = false;
		this.ItemListService.FetchWithCrit(cr, "Excluded Items");
		this.tabset.select('ItemListTab');
	}
	GoToItemList() {
		this.tabset.select('ItemListTab');
	}
	LoadWorkAllocList(workAlloc: WorkAllocation) {
		console.log('try to load a (default?) work alloc');
		if (this.ItemListComponent) this.ItemListComponent.LoadWorkAllocList(workAlloc, this.workAllocationsComp.ListSubType);
		else console.log('attempt failed');
	}

	MyInfoMessage(): string {
		let msg: string = "Your account expires on: ";
		let revPart: string = "";
		let AccExp: string = new Date(this.ReviewerIdentityServ.reviewerIdentity.accountExpiration).toLocaleDateString();
		if (this.ReviewerIdentityServ.getDaysLeftReview() == -999999) {//review is private
			revPart = " | Current review is private (does not expire).";
		}
		else {
			let RevExp: string = new Date(this.ReviewerIdentityServ.reviewerIdentity.reviewExpiration).toLocaleDateString();
			revPart = " | Current(shared) review expires on " + RevExp + ".";
		}
		msg += AccExp + revPart;
		return msg;

	}

	ngOnDestroy() {
		if (this.subOpeningReview) {
			this.subOpeningReview.unsubscribe();
		}
	}
}
