import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ComparisonsService, ComparisonStatistics, Comparison, iCompleteComparison } from '../services/comparisons.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Router } from '@angular/router';


@Component({
	selector: 'ComparisonStatsComp',
    templateUrl: './comparisonstatistics.component.html',
    providers: []
})

export class ComparisonStatsComp implements OnInit {
	constructor(
		private router: Router,
		private _comparisonsService: ComparisonsService,
		private _reviewInfoService: ReviewInfoService,
		private _reviewerIdentityServ: ReviewerIdentityService,
		private _reviewSetsEditingService: ReviewSetsEditingService,
		private _eventEmitter: EventEmitterService,
		private _ItemListService: ItemListService,
		private _notificationService: NotificationService
	) { }

	private comparisonID: number = 0;
	public PanelName: string = '';
	public CodeSets: ReviewSet[] = [];
	public selectedReviewer1: Contact = new Contact();
	public selectedReviewer2: Contact = new Contact();
	public selectedReviewer3: Contact = new Contact();
	public selectedCodeSet: ReviewSet = new ReviewSet();
	public selectedFilter!: singleNode;
	public CompleteSectionShow: boolean = false;
	public RunQuickReportShow: boolean = false;
	public tabSelected: string = 'AgreeStats';
	public lstContacts: Array<Contact> = new Array();
	@Output() criteriaChange = new EventEmitter();
	@Output() ComparisonClicked = new EventEmitter();
	@Input('rowSelected') rowSelected!: number;
	//@Input('runQuickReport') runQuickReport!: boolean;
	@Output() setListSubType = new EventEmitter();
	public ListSubType: string = "";
	public selectedCompleteUser: Contact = new Contact();

	private _Contacts: Contact[] = [];

	public get Contacts(): Contact[] {

		this._Contacts = this._reviewInfoService.Contacts;
		
		if (this._Contacts) {
			return this._Contacts;
		}
		else {
			this._Contacts = [];
			return this._Contacts;
		}
	}

	public get HasWriteRights(): boolean {
		return this._reviewerIdentityServ.HasWriteRights;
	}

	IsServiceBusy(): boolean {
		if (this._reviewSetsEditingService.IsBusy || this._reviewSetsEditingService.IsBusy || this._reviewInfoService.IsBusy) return true;
		else return false;
	}
	CanWrite(): boolean {
		//console.log('CanWrite', this.ReviewerIdentityServ.HasWriteRights, this.IsServiceBusy());
		if (this._reviewerIdentityServ.HasWriteRights && !this.IsServiceBusy()) {
			//console.log('CanWrite', true);
			return true;
		}
		else {
			//console.log('CanWrite', false);
			return false;
		}
	}
	
	public NewComparisonStatsSectionOpen() {

		if (this.PanelName == 'NewComparisonStatsSection') {
			this.PanelName = '';
		} else {
			this.PanelName = 'NewComparisonStatsSection';
		}
		
	}
	public checkCanComplete1(): boolean {

		let stats: ComparisonStatistics = this._comparisonsService.Statistics!;

		return stats.RawStats.canComplete1vs2;

	}
	public checkCanComplete2(): boolean {

		let stats: ComparisonStatistics = this._comparisonsService.Statistics!;

		return stats.RawStats.canComplete2vs3;

	}
	public checkCanComplete3(): boolean {

		let stats: ComparisonStatistics = this._comparisonsService.Statistics!;

		return stats.RawStats.canComplete1vs3;

	}	

	LoadComparisonList(comparisonId: number, subtype: string) {

		for (let item of this._comparisonsService.Comparisons) {
			if (item.comparisonId == comparisonId) {
				this.ListSubType = subtype;
				this.setListSubType.emit(this.ListSubType);
				//this.criteriaChange.emit(item);
				this.LoadComparisons(item, this.ListSubType);
				this._eventEmitter.PleaseSelectItemsListTab.emit();
				return;
			}
		}

	}

	LoadReconciliation(comparisonId: number, subtype: string) {
		for (let item of this._comparisonsService.Comparisons) {
			if (item.comparisonId == comparisonId) {

				this.ListSubType = subtype;
				this.LoadComparisonsWithPromise(item, this.ListSubType);
			}
		}
	}
	GoToReconcile() {
		this.router.navigate(['Reconciliation']);
	}
	Complete(currentComparison: Comparison) {

		var completeComparison = <iCompleteComparison>{};
		completeComparison.comparisonId = currentComparison.comparisonId;
		completeComparison.contactId = this.selectedCompleteUser.contactId;
		completeComparison.whichReviewers = this.ListSubType;
		console.log(completeComparison);

		this._comparisonsService.CompleteComparison(completeComparison).then(
			(res: any) => {

				this._notificationService.show({
					content: 'Number of records affected is: ' + res.numberAffected,
					animation: { type: 'slide', duration: 400 },
					position: { horizontal: 'center', vertical: 'top' },
					type: { style: "warning", icon: true },
					closable: true,
					hideAfter: 3000
				});
				this.CloseConfirm();
			}
			);
	}
	SendToComplete(members: string, listSubType: string) {

		this.ListSubType = listSubType;

		let currentComparison: Comparison = this._comparisonsService.currentComparison;
		//console.log(currentComparison);
	
		if (members == '1And2') {
			let contact = new Contact();
			contact.contactId = currentComparison.contactId1;
			contact.contactName = currentComparison.contactName1;
			this.lstContacts.push(contact);
			contact = new Contact();
			contact.contactId = currentComparison.contactId2;
			contact.contactName = currentComparison.contactName2;
			this.lstContacts.push(contact);

		}else if (members == '2And3') {
			let contact = new Contact();
			contact.contactId = currentComparison.contactId2;
			contact.contactName = currentComparison.contactName2;
			this.lstContacts.push(contact);
			contact = new Contact();
			contact.contactId = currentComparison.contactId3;
			contact.contactName = currentComparison.contactName3;
			this.lstContacts.push(contact);

		}else if (members == '1And3') {
			let contact = new Contact();
			contact.contactId = currentComparison.contactId1;
			contact.contactName = currentComparison.contactName1;
			this.lstContacts.push(contact);
			contact = new Contact();
			contact.contactId = currentComparison.contactId3;
			contact.contactName = currentComparison.contactName3;
			this.lstContacts.push(contact);
		}
		this.tabSelected = 'confirm';
		this.CompleteSectionShow = true;

	}
	CloseConfirm() {

		this.tabSelected = 'AgreeStats';
		this.CompleteSectionShow = false;
		this.lstContacts = [];
		this.ListSubType = '';
	}
	public LoadComparisons(comparison: Comparison, ListSubType: string) {

		let crit = new Criteria();
		crit.listType = ListSubType;
		let typeMsg: string = '';
		if (ListSubType.indexOf('Disagree') != -1) {
			typeMsg = 'disagreements between';
		} else {
			typeMsg = 'agreements between';
		}
		let middleDescr: string = ' ' + comparison.contactName3 != '' ? ' and ' + comparison.contactName3 : '';
		let listDescription: string = typeMsg + '  ' + comparison.contactName1 + ' and ' + comparison.contactName2 + middleDescr + ' using ' + comparison.setName;
		crit.description = listDescription;
		crit.listType = ListSubType;
		crit.comparisonId = comparison.comparisonId;
		crit.setId = comparison.setId;
		console.log('checking: ' + JSON.stringify(crit) + '\n' + ListSubType);
		this._ItemListService.FetchWithCrit(crit, listDescription);
		console.log('checking: ' + JSON.stringify(this._ItemListService.ItemList.items.length));
	}
	public LoadComparisonsWithPromise(comparison: Comparison, ListSubType: string) {

		let crit = new Criteria();
		crit.listType = ListSubType;
		let typeMsg: string = '';
		if (ListSubType.indexOf('Disagree') != -1) {
			typeMsg = 'disagreements between';
		} else {
			typeMsg = 'agreements between';
		}
		let middleDescr: string = ' ' + comparison.contactName3 != '' ? ' and ' + comparison.contactName3 : '';
		let listDescription: string = typeMsg + '  ' + comparison.contactName1 + ' and ' + comparison.contactName2 + middleDescr + ' using ' + comparison.setName;
		crit.description = listDescription;
		crit.listType = ListSubType;
		crit.comparisonId = comparison.comparisonId;
		crit.setId = comparison.setId;
		console.log('checking: ' + JSON.stringify(crit) + '\n' + ListSubType);
		this._ItemListService.FetchWithItems(crit, listDescription).then(

			(res: any) => {

				console.log(JSON.stringify(res.length));
				this.GoToReconcile();
			}
			
			);
	}
	public RefreshData() {

		this.selectedCodeSet = this.CodeSets[0];
	}
	ngOnInit() {

		this.RefreshData();
		
	}
	Clear() {
		this.selectedCodeSet = new ReviewSet();
	}
	 
}