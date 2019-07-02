import { Component, Inject, OnInit, ViewChild, OnDestroy, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Criteria, ItemList } from '../services/ItemList.service';
import { ItemListService } from '../services/ItemList.service'
import { ItemListComp } from '../ItemList/itemListComp.component';
import {  Subject, Subscription } from 'rxjs';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { CodesetStatisticsService, ReviewStatisticsCountsCommand, StatsCompletion, StatsByReviewer } from '../services/codesetstatistics.service';
import { NgbTabset } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';


@Component({
	selector: 'reviewStatisticsComp',
	templateUrl: './reviewstatistics.component.html',
    providers: []
})

export class ReviewStatisticsComp implements OnInit, OnDestroy {
	constructor(private router: Router,
		public ReviewerIdentityServ: ReviewerIdentityService,
		private reviewSetsService: ReviewSetsService,
		@Inject('BASE_URL') private _baseUrl: string,
		private _httpC: HttpClient,
		private ItemListService: ItemListService,
		private codesetStatsServ: CodesetStatisticsService,
		private confirmationDialogService: ConfirmationDialogService,
	) {

	}
    
	
	@Output() tabSelectEvent = new EventEmitter();

	public stats: ReviewStatisticsCountsCommand | null = null;
	public countDown: any | undefined;
    public count: number = 60;
    public DetailsForSetId: number = 0;
	public isReviewPanelCollapsed = false;
	public isWorkAllocationsPanelCollapsed = false;
	private statsSub: Subscription = new Subscription();

	dtOptions: DataTables.Settings = {};
	dtTrigger: Subject<any> = new Subject();

    public get IsServiceBusy(): boolean {
        return this.codesetStatsServ.IsBusy;
    }
    public get ScreeningSets(): StatsCompletion[] {
        return this.codesetStatsServ.tmpCodesets.filter(found => found.subTypeName == 'Screening');
    }
    public get StandardSets(): StatsCompletion[] {
        return this.codesetStatsServ.tmpCodesets.filter(found => found.subTypeName == 'Standard');
    }
    public get AdminSets(): StatsCompletion[] {
        return this.codesetStatsServ.tmpCodesets.filter(found => found.subTypeName == 'Administration');
    }
    
	ngOnInit() {

		console.log('inititating stats');
		
		//this.subOpeningReview = this.ReviewerIdentityServ.OpeningNewReview.subscribe(() => this.Reload());
		//this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
			
		//	() => {
		//		console.log('gettign the stats');
		//		this.GetStats()
		//	}
		//		);
		//if (this.codesetStatsServ.ReviewStats.itemsIncluded == -1
		//	|| (this.reviewSetsService.ReviewSets.length > 0 && this.codesetStatsServ.tmpCodesets.length == 0)
		//) this.Reload();
	}

    ShowDetailsForSetId(SetId: number) {
        if (this.DetailsForSetId == SetId) this.DetailsForSetId = 0;
        else this.DetailsForSetId = SetId;
    }
	
    RefreshStats() {
        this.reviewSetsService.GetReviewStatsEmit.emit();
        //this.codesetStatsServ.GetReviewStatisticsCountsCommand();
    }
	Reload() {
		this.Clear();
		//this.reviewSetsService.GetReviewSets();
		//if (this.workAllocationsComp) this.workAllocationsComp.getWorkAllocationContactList();
		//else console.log("work allocs comp is undef :-(");
	}

	GetStats() {
		//console.log('getting stats...');
		//this.codesetStatsServ.GetReviewStatisticsCountsCommand();
		//this.codesetStatsServ.GetReviewSetsCodingCounts(true, this.dtTrigger);
	}

	Clear() {
		this.ItemListService.SaveItems(new ItemList(), new Criteria());

		this.reviewSetsService.Clear();

		//if (this.statsSub) this.statsSub.unsubscribe();
		//this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
		//	() => this.GetStats()
		//);
	}
	EditCodeSets() {
		this.router.navigate(['EditCodeSets']);
	}
	ImportCodesetClick() {
		this.router.navigate(['ImportCodesets']);
	}
	IncludedItemsList() {
        this.ItemListService.GetIncludedItems();
		this.tabSelectEvent.emit();
		//this.tabset.select('ItemListTab');
	}
	ExcludedItemsList() {
        this.ItemListService.GetExcludedItems();
		this.tabSelectEvent.emit();
		//this.tabset.select('ItemListTab');
    }
    DeletedItemsList() {
        this.ItemListService.GetDeletedItems();
        this.tabSelectEvent.emit();
        //this.tabset.select('ItemListTab');
    }
    CompletedBySetAndContact(statsByContact: StatsByReviewer, setName: string) {
        let cri: Criteria = new Criteria();
        cri.contactId = statsByContact.ContactId;
        cri.setId = statsByContact.SetId;
        cri.pageSize = this.ItemListService.ListCriteria.pageSize;
        cri.listType = "ReviewerCodingCompleted";
        this.ItemListService.FetchWithCrit(cri, statsByContact.ContactName + ": documents with completed coding using '" + setName + "'");
        this.tabSelectEvent.emit();
    }
    IncompleteBySetAndContact(statsByContact: StatsByReviewer, setName: string) {
        let cri: Criteria = new Criteria();
        cri.contactId = statsByContact.ContactId;
        cri.setId = statsByContact.SetId;
        cri.pageSize = this.ItemListService.ListCriteria.pageSize;
        cri.listType = "ReviewerCodingIncomplete";
        this.ItemListService.FetchWithCrit(cri, statsByContact.ContactName + ": documents with incomplete (but started) coding using '" + setName + "'");
        this.tabSelectEvent.emit();
	}
	public ImportOrNewDDData: Array<any> = [{
		text: 'New Reference',
		click: () => {
			this.NewReference();
		}
	}];
	NewReference() {
		this.router.navigate(['EditItem'], { queryParams: { return: 'Main' } });
	}
	CompleteCoding(setId: number, contactId: number,  completeOrNot: string) {

		if (setId != null && contactId != null && completeOrNot != null) {

			this.confirmationDialogService.confirm('Please confirm', 'Are you sure you want to set these codings by the selected reviewer to being uncompleted?' +
				'<br /><b>Note:</b> If these are double coded items and are reconciled disagreements you will be loosing the reconcile information.' +
				'<br />Please check in the manual if you are unsure about the implications.' +
				'<br />Uncompleted items will no longer be visible in searches and reports', false, '')
				.then(
					(confirmed: any) => {
						console.log('User confirmed:');
						if (confirmed) {

							this.codesetStatsServ.SendToItemBulkCompleteCommand(
								setId,
								contactId,
								completeOrNot);

							this.RefreshStats();

						} else {
							//alert('did not confirm');
						}
					}
				)
				.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
		}
	}

	//GoToItemList() {
	//	console.log('selecting tab 3...');
	//	this.tabset.select('ItemListTab');
	//}
	//LoadWorkAllocList(workAlloc: WorkAllocation) {
	//	console.log('try to load a (default?) work alloc');
	//	if (this.ItemListComponent) this.ItemListComponent.LoadWorkAllocList(workAlloc, this.workAllocationsComp.ListSubType);
	//	else console.log('attempt failed');
	//}

	//MyInfoMessage(): string {
	//	let msg: string = "Your account expires on: ";
	//	let revPart: string = "";
	//	let AccExp: string = new Date(this.ReviewerIdentityServ.reviewerIdentity.accountExpiration).toLocaleDateString();
	//	if (this.ReviewerIdentityServ.getDaysLeftReview() == -999999) {//review is private
	//		revPart = " | Current review is private (does not expire).";
	//	}
	//	else {
	//		let RevExp: string = new Date(this.ReviewerIdentityServ.reviewerIdentity.reviewExpiration).toLocaleDateString();
	//		revPart = " | Current(shared) review expires on " + RevExp + ".";
	//	}
	//	msg += AccExp + revPart;
	//	return msg;

	//}

	ngOnDestroy() {
		//if (this.subOpeningReview) {
		//	this.subOpeningReview.unsubscribe();
		//}
	}
}
