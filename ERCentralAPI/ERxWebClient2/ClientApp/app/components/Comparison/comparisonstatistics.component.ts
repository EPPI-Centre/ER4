import { Component, OnInit, Output, EventEmitter, Input, OnDestroy } from '@angular/core';
import { ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ComparisonsService, ComparisonStatistics, Comparison, iCompleteComparison } from '../services/comparisons.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';


@Component({
	selector: 'ComparisonStatsComp',
    templateUrl: './comparisonstatistics.component.html',
    providers: []
})

export class ComparisonStatsComp implements OnInit {
	constructor(
		private router: Router,
		public _comparisonsService: ComparisonsService,
		private _reviewInfoService: ReviewInfoService,
		private _reviewerIdentityServ: ReviewerIdentityService,
		private _reviewSetsEditingService: ReviewSetsEditingService,
		private _eventEmitter: EventEmitterService,
		private _ItemListService: ItemListService,
		private _notificationService: NotificationService
	) { }

	private PanelName: string = '';
	private CodeSets: ReviewSet[] = [];
	public selectedCodeSet: ReviewSet = new ReviewSet();

	public CompleteSectionShow: boolean = false;
	public tabSelected: string = 'AgreeStats';
    public lstContacts: Array<Contact> = new Array();
	public lockCoding: boolean = true;
	public Full: boolean = true;

	@Input('rowSelected') rowSelected!: number;
	@Output() setListSubType = new EventEmitter();
    private GoToReconcileSub: Subscription | null = null;
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
	ngOnInit() {
	
		this.RefreshData();
	}
	public get HasWriteRights(): boolean {
		return this._reviewerIdentityServ.HasWriteRights;
	}
	IsServiceBusy(): boolean {
		if (this._reviewSetsEditingService.IsBusy || this._reviewSetsEditingService.IsBusy || this._reviewInfoService.IsBusy) return true;
		else return false;
	}
	CanWrite(): boolean {
		if (this._reviewerIdentityServ.HasWriteRights && !this.IsServiceBusy()) {
			return true;
		}
		else {
			return false;
		}
	}
	NewComparisonStatsSectionOpen() {
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
	public LoadComparisonList(comparisonId: number, subtype: string) {
		for (let item of this._comparisonsService.Comparisons) {
			if (item.comparisonId == comparisonId) {
				this.ListSubType = subtype;
				this.setListSubType.emit(this.ListSubType);
				this.LoadComparisons(item, this.ListSubType);
				this._eventEmitter.PleaseSelectItemsListTab.emit();
				return;
			}
		}

	}
	public LoadReconciliation(comparisonId: number, subtype: string) {
		for (let item of this._comparisonsService.Comparisons) {
			if (item.comparisonId == comparisonId) {

				this.ListSubType = subtype;
				this.LoadComparisonsWithReconcile(item, this.ListSubType);
			}
		}
	}
	GoToReconcile() {
		this.router.navigate(['Reconciliation']);
	}
	public Complete(currentComparison: Comparison) {
		var completeComparison = <iCompleteComparison>{};
		completeComparison.comparisonId = currentComparison.comparisonId;
		completeComparison.contactId = this.selectedCompleteUser.contactId;
		completeComparison.whichReviewers = this.ListSubType;
        if (this.lockCoding) completeComparison.lockCoding = 'true';
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
	public SendToComplete(members: string, listSubType: string, lockCoding: boolean) {
		//console.log('testing', JSON.stringify({ members, listSubType, lockCoding}));
        this.lockCoding = lockCoding;
		this.ListSubType = listSubType;
		let currentComparison: Comparison = this._comparisonsService.currentComparison;
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
        this.selectedCompleteUser = new Contact();
		this.CompleteSectionShow = true;
	}
	public selectedTab: number = 0;
	public onTabSelect(e: any) {
		this.selectedTab = e.index
		
		if (this.selectedTab == 0) {
			this.tabSelected = 'AgreeStats';
		} else {
			this.tabSelected = 'AgreeStats';
		}
	}
	public CloseConfirm() {


		if (this.selectedTab == 0) {
			
			this.tabSelected = 'AgreeStats';
		}
		if (this.selectedTab == 1) {
			
			this.selectedTab = 1;
		}
		this.CompleteSectionShow = false;
		this.lstContacts = [];
		this.ListSubType = '';
		
	}
	LoadComparisons(comparison: Comparison, ListSubType: string) {

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

		this._ItemListService.FetchWithCrit(crit, listDescription);
				
		
	}
	LoadComparisonsWithReconcile(comparison: Comparison, ListSubType: string) {

		this.LoadComparisons(comparison, ListSubType);
		this.GoToReconcile();
	}
	RefreshData() {
		this.selectedCodeSet = this.CodeSets[0];
	}


	Clear() {
		this.selectedCodeSet = new ReviewSet();
	}
}