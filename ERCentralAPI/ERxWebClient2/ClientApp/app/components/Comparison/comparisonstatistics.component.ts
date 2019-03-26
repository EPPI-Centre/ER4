import { Component, Inject, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewSetsService, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { BuildModelService } from '../services/buildmodel.service';
import { EventEmitterService } from '../services/EventEmitter.service';
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
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
		private _reviewSetsService: ReviewSetsService,
		public _comparisonsService: ComparisonsService,
		public _buildModelService: BuildModelService,
		public _eventEmitterService: EventEmitterService,
		public _reviewInfoService: ReviewInfoService,
		public _reviewerIdentityServ: ReviewerIdentityService,
		public _reviewSetsEditingService: ReviewSetsEditingService
	) { }
		
	public PanelName: string = '';
	public CodeSets: ReviewSet[] = [];
	public selectedReviewer1: Contact = new Contact();
	public selectedReviewer2: Contact = new Contact();
	public selectedReviewer3: Contact = new Contact();
	public selectedCodeSet: ReviewSet = new ReviewSet();
	public selectedFilter!: singleNode;

	@Output() emitterCancel = new EventEmitter();
	@Input('rowSelected') rowSelected!: number;


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

		let stats: ComparisonStatistics = this._comparisonsService.Statistics;

		return stats.canComplete1vs2;

	}
	public checkCanComplete2(): boolean {

		let stats: ComparisonStatistics = this._comparisonsService.Statistics;

		return stats.canComplete2vs3;

	}
	public checkCanComplete3(): boolean {

		let stats: ComparisonStatistics = this._comparisonsService.Statistics;

		return stats.canComplete1vs3;

	}
	
	public RefreshData() {

		//this.getMembers();
		//this.getCodeSets();
		//this.getComparisons();
		//this.getStatistics();
		this.selectedCodeSet = this.CodeSets[0];
	}
	ngOnInit() {
		this.RefreshData();
		
	}
	Clear() {
		this.selectedCodeSet = new ReviewSet();
	}
	 
}