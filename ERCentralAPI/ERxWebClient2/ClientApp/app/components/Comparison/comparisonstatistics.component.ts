import { Component, Inject, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { Router } from '@angular/router';
import {  ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ComparisonsService, ComparisonStatistics } from '../services/comparisons.service';


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
		private _reviewSetsEditingService: ReviewSetsEditingService
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
	public ListSubType: string = "ComparisonAgree1vs2";

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

		alert('here');
		for (let item of this._comparisonsService.Comparisons) {
			if (item.comparisonId == comparisonId) {
				alert('and here..');
				console.log('about to emit: ' + subtype + ' ' + JSON.stringify(item));
				this.ListSubType = subtype;
				this.criteriaChange.emit(item);
				this.ComparisonClicked.emit();
				return;
			}
		}
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