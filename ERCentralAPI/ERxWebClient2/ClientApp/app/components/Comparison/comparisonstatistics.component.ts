import { Component, Inject, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { Router } from '@angular/router';
import {  ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ComparisonsService, ComparisonStatistics, Comparison } from '../services/comparisons.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ItemListService, Criteria } from '../services/ItemList.service';


@Component({
	selector: 'ComparisonStatsComp',
    templateUrl: './comparisonstatistics.component.html',
    providers: []
})

export class ComparisonStatsComp implements OnInit {
    constructor(
		private _comparisonsService: ComparisonsService,
		private _reviewInfoService: ReviewInfoService,
		private _reviewerIdentityServ: ReviewerIdentityService,
		private _reviewSetsEditingService: ReviewSetsEditingService,
		private _eventEmitter: EventEmitterService,
		private _ItemListService: ItemListService
	) { }
		
	public PanelName: string = '';
	public CodeSets: ReviewSet[] = [];
	public selectedReviewer1: Contact = new Contact();
	public selectedReviewer2: Contact = new Contact();
	public selectedReviewer3: Contact = new Contact();
	public selectedCodeSet: ReviewSet = new ReviewSet();
	public selectedFilter!: singleNode;
	@Output() criteriaChange = new EventEmitter();
	@Output() ComparisonClicked = new EventEmitter();
	@Input('rowSelected') rowSelected!: number;
	@Output() setListSubType = new EventEmitter();
	public ListSubType: string = "";

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
		console.log('checking: ' + JSON.stringify(crit) + '\n' + ListSubType);
		this._ItemListService.FetchWithCrit(crit, listDescription);

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