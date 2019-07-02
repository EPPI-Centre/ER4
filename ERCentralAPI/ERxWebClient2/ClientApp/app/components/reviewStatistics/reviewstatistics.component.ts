import { Component, Inject, OnInit, ViewChild, OnDestroy, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Criteria, ItemList } from '../services/ItemList.service';
import { ItemListService } from '../services/ItemList.service'
import { ItemListComp } from '../ItemList/itemListComp.component';
import {  Subject, Subscription } from 'rxjs';
import { ReviewSetsService, ReviewSet, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { CodesetStatisticsService, ReviewStatisticsCountsCommand, StatsCompletion, StatsByReviewer, BulkCompleteUncompleteCommand } from '../services/codesetstatistics.service';
import { NgbTabset } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';


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
		private _reviewSetsService: ReviewSetsService
	) {

	}
    
	@ViewChild('CodeStudiesTreeOne') CodeStudiesTreeOne!: codesetSelectorComponent;
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
	CompleteCoding(contactName: string, setName: string, setId: number, contactId: number,  completeOrNot: string) {

		if (setId != null && contactId != null && completeOrNot != null) {

			let tmpComplete: string = '';
			if (completeOrNot == 'true') {
				tmpComplete = 'Completed';
			} else {
				tmpComplete = 'Uncompleted';
			}
			let tmpStrItemVisible: string = '';
			if (tmpComplete == 'Complete') {
				tmpStrItemVisible = ' items will no longer be visible in searches and reports';
			} else {
				tmpStrItemVisible = ' items will be visible in searches and reports';
			}
			this.confirmationDialogService.confirm('Please confirm', 'Are you sure you want to change the codings by ' + contactName + ' for the ' + setName + ' coding tool to <b>' + tmpComplete + '</b> ?' +
				'<br /><b>Note:</b> ' +
				'<br />Please check in the manual if you are unsure about the implications.' +
				'<br /><b>' + tmpComplete + '</b> ' + tmpStrItemVisible , false, '')
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
	CloseCodeDropDownStudies() {

		if (this.CodeStudiesTreeOne) {

			this.DropdownSelectedCodeStudies = this.CodeStudiesTreeOne.SelectedNodeData;
			let setAtt: SetAttribute = this.DropdownSelectedCodeStudies as SetAttribute;
			

		}
		this.isCollapsedCodeStudies = false;
		this.msg = '';
	}
	public get CodeSets(): ReviewSet[] {
		return this._reviewSetsService.ReviewSets.filter(x => x.setType.allowComparison != false);
	}
	public canBulkComplete: boolean = false;
	public CanPreview() {

		return false;
	}
	public msg: string = '';
	public Preview(isCompleting: string) {

		let completing: string = '';
		if (isCompleting == 'Complete') {

			completing = 'true';
		} else {

			completing = 'false';
		}

		if (this.DropdownSelectedCodeStudies == null || this.DropdownSelectedCodeStudies.name == ''
			|| this.selectedCodeSet == null) {
			return;
		}

		let setId: number = this.selectedCodeSet.set_id;
		let node: SetAttribute = this.DropdownSelectedCodeStudies as SetAttribute;
		let attId: number = node.attribute_id;
		let reviewerId: number = this.ReviewerIdentityServ.reviewerIdentity.userId; 
		let apiResult: Promise<BulkCompleteUncompleteCommand> | any;



		if (attId != null) {

			apiResult = this.codesetStatsServ.SendItemsToBulkCompleteOrNotCommand(
				attId,
				completing,
				setId,
				reviewerId,
				'true'

			).then(

				(result: BulkCompleteUncompleteCommand) => {

					this.msg = "Your selected code (" + node.attribute_name + ") is associated with ";
					this.msg += "<b>" + result.potentiallyAffectedItems + " Items. </b>";

					if (result.potentiallyAffectedItems > 0) {

						this.msg += "<br\> Of these, "
							+ (result.isCompleting ? "un-completed" : "completed")
							+ " codings in the chosen Codeset (\"" + this.selectedCodeSet.set_name + "\") will be "
							+ (result.isCompleting ? "completed, if they belong to contacts name should go here...." : "un-completed");
						+ "." + "<br\>";

						this.msg += "<br\> As a result, <b> the coding of ";
						this.msg += result.affectedItems + " Items ";
						this.msg += "will be " + (result.isCompleting ? "completed" : "un-completed") + "</b>.";

						if (result.affectedItems > 0) {
							this.msg +=  "<br\>" + "If this looks ok, you may now press the "
								+ (result.isCompleting ? "\"Complete!\"" : "\"Un-Complete!\"") + " button.";
							this.msg +=  "<br\>" + "<b>Warning: this action does not have a direct \"Undo\" function so please use with care!</b>";

							this.canBulkComplete = true;

						} else {

							this.msg +=  "<br\>" + "Nothing to be " + (result.isCompleting ? "completed" : "un-completed") + "!";
						}


					} else {

						this.msg +=  "Nothing to be " + (result.isCompleting ? "completed" : "un-completed") + "!";
					}
					console.log(this.msg);
					this.showMessage = true;
				});
			}
	}
	public showMessage: boolean = false;
	public CanCompleteOrNot() {

		return this.canBulkComplete;
		
	}

	public DropdownSelectedCodeStudies: singleNode | null = null;
	public isCollapsedCodeStudies: boolean = false;
	public selectedCodeSet: ReviewSet = new ReviewSet();
	public PanelName: string = '';
	public complete: string ='';
	changePanel(completeOrNot: string) {
		if (completeOrNot == 'true') {
			this.complete = 'Complete';
		} else {
			this.complete = 'Uncomplete';
		}
		if (this.PanelName == '') {
			this.PanelName = 'BulkCompleteSection';

		} else {
			this.PanelName =  '';
		}
	}

	ngOnDestroy() {
		//if (this.subOpeningReview) {
		//	this.subOpeningReview.unsubscribe();
		//}
	}
}
