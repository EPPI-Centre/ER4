import { Component, Inject, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewSetsService, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { BuildModelService } from '../services/buildmodel.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ComparisonsService } from '../services/comparisons.service';


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

	//getMembers() {

	//	if (!this._reviewInfoService.ReviewInfo || this._reviewInfoService.ReviewInfo.reviewId < 1) {
	//		this._reviewInfoService.Fetch();
	//	}
	//	this._reviewInfoService.FetchReviewMembers();

	//}
	//public getCodeSets() {
	//	this.CodeSets = this._reviewSetsService.ReviewSets.filter(x => x.nodeType == 'ReviewSet')
	//	.map(
	//		(y: ReviewSet) => {

	//			return y;
	//		}
	//	);
	//}
	//getComparisons() {

	//	if (!this.__comparisonsService.Comparisons || this.__comparisonsService.Comparisons.length <= 0) {
	//		this.__comparisonsService.FetchAll();
	//	}

	//}
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
	//public get CurrentCodeCanHaveChildren(): boolean {
	//	//safety first, if anything didn't work as expexcted return false;
	//	if (!this.CanWrite()) return false;
	//	else {
	//		return this._reviewSetsService.CurrentCodeCanHaveChildren;
	//		//end of bit that goes into "ReviewSetsService.CanNodeHaveChildren(node: singleNode): boolean"
	//	}
	//}
	//CanCreateComparison(): boolean {

	//	if (this.selectedReviewer1.contactName != '' &&
	//		this.selectedReviewer2.contactName != '' &&
	//		this.selectedCodeSet != null &&
	//		this.selectedCodeSet.set_name != ''
	//		&& this.CanWrite())
	//	{
	//		return true;
	//	} else {
	//		return false;
	//	}

	//}
	//setOptionalMember(member: Contact) {

	//	this.selectedReviewer3.contactId = member.contactId;
	//	this.selectedReviewer3.contactName = member.contactName;
	
	//}
	//setsFilter() {

	//	if (this._reviewSetsService.selectedNode && this._reviewSetsService.selectedNode.nodeType == "SetAttribute") {
	//		this.selectedFilter = this._reviewSetsService.selectedNode as SetAttribute;
	//	} 

	//}
	//public get CurrentNode(): singleNode | null {
	//	if (!this._reviewSetsService.selectedNode) return null;
	//	else return this._reviewSetsService.selectedNode;
	//}
	
	//CancelActivity(refreshTree?: boolean) {
	//	if (refreshTree) {
	//		if (this._reviewSetsService.selectedNode) {
	//			let IsSet: boolean = this._reviewSetsService.selectedNode.nodeType == "ReviewSet";
	//			let Id: number = -1;
	//			if (IsSet) Id = (this._reviewSetsService.selectedNode as ReviewSet).set_id;
	//			else Id = (this._reviewSetsService.selectedNode as SetAttribute).attribute_id;
	//			let sub: Subscription = this._reviewSetsService.GetReviewStatsEmit.subscribe(() => {
	//				console.log("trying to reselect: ", Id);
	//				if (IsSet) this._reviewSetsService.selectedNode = this._reviewSetsService.FindSetById(Id);
	//				else this._reviewSetsService.selectedNode = this._reviewSetsService.FindAttributeById(Id);
	//				if (sub) sub.unsubscribe();
	//			}
	//				, () => { if (sub) sub.unsubscribe(); }
	//			);
	//			this._reviewSetsService.selectedNode = null;
	//			this._reviewSetsService.GetReviewSets();
	//		}
	//	}
	//	this.PanelName = '';
	//	this.emitterCancel.emit();

	//}

	public NewComparisonStatsSectionOpen() {

		if (this.PanelName == 'NewComparisonStatsSection') {
			this.PanelName = '';
		} else {
			this.PanelName = 'NewComparisonStatsSection';
		}
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