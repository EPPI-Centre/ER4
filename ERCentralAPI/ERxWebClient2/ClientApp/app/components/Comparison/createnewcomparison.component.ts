import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewSetsService, kvAllowedAttributeType, SetAttribute, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { BuildModelService } from '../services/buildmodel.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Subscription } from 'rxjs';
import { Review } from '../services/review.service';
import { ComparisonsService, Comparison } from '../services/comparisons.service';


@Component({
    selector: 'CreateNewComparisonComp',
    templateUrl: './createnewcomparison.component.html',
    providers: []
})

export class CreateNewComparisonComp implements OnInit, OnDestroy {
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
		private _reviewSetsService: ReviewSetsService,
		public __comparisonsService: ComparisonsService,
		public _buildModelService: BuildModelService,
		public _eventEmitterService: EventEmitterService,
		public _reviewInfoService: ReviewInfoService,
		public _reviewerIdentityServ: ReviewerIdentityService,
		public _reviewSetsEditingService: ReviewSetsEditingService
	) { }

	@ViewChild('CodeTypeSelectCollaborate') CodeTypeSelect: any;
	public PanelName: string = '';
	public CodeSets: ReviewSet[] = [];
	public selectedReviewer1: Contact = new Contact();
	public selectedReviewer2: Contact = new Contact();
	public selectedReviewer3: Contact = new Contact();
	public selectedCodeSet: ReviewSet = new ReviewSet();
	@Output() emitterCancel = new EventEmitter();
	SetReviewer1(member: any) {
		this.selectedReviewer1 = member;
		alert(this.selectedReviewer1);
	}
	SetReviewer2(member: any) {
		this.selectedReviewer2 = member;

	}
	SetReviewer3(member: any) {
		this.selectedReviewer3 = member;

	}
	SetCodeSet(codeset: any) {
		this.selectedCodeSet = codeset;
	}
	getMembers() {

		if (!this._reviewInfoService.ReviewInfo || this._reviewInfoService.ReviewInfo.reviewId < 1) {
			this._reviewInfoService.Fetch();
		}
		this._reviewInfoService.FetchReviewMembers();

	}
	public getCodeSets() {
		this.CodeSets = this._reviewSetsService.ReviewSets.filter(x => x.nodeType == 'ReviewSet')
		.map(
			(y: ReviewSet) => {

				return y;
			}
		);
	}
	getComparisons() {

		if (!this.__comparisonsService.Comparisons || this.__comparisonsService.Comparisons.length <= 0) {
			this.__comparisonsService.FetchAll();
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
	public get CurrentCodeCanHaveChildren(): boolean {
		//safety first, if anything didn't work as expexcted return false;
		if (!this.CanWrite()) return false;
		else {
			return this._reviewSetsService.CurrentCodeCanHaveChildren;
			//end of bit that goes into "ReviewSetsService.CanNodeHaveChildren(node: singleNode): boolean"
		}
	}
	private _NewReviewSet: ReviewSet = new ReviewSet();
	public get NewReviewSet(): ReviewSet {
		return this._NewReviewSet;
	}
	private _NewCode: SetAttribute = new SetAttribute();
	public get CurrentNode(): singleNode | null {
		if (!this._reviewSetsService.selectedNode) return null;
		else return this._reviewSetsService.selectedNode;
	}
	public get NewCode(): SetAttribute {
		return this._NewCode;
	}
	CanCreateComparison(): boolean {

		if (this.selectedReviewer1.contactName != '' && this.selectedReviewer2.contactName != ''
			&& this.selectedCodeSet && this.CanWrite()) {
			return true;
		} else {
			return false;
		}

	}
	CreateNewComparison() {

		let newComparison: Comparison = new Comparison();
		newComparison.contactId1 = this.selectedReviewer1.contactId;
		newComparison.contactName1 = this.selectedReviewer1.contactName;
		newComparison.contactId2 = this.selectedReviewer2.contactId;
		newComparison.contactName2 = this.selectedReviewer2.contactName;
		if (this.selectedReviewer3 != null) { newComparison.contactId3 = this.selectedReviewer3.contactId; }
		if (this.selectedReviewer3 != null) { newComparison.contactName3 = this.selectedReviewer3.contactName; }
		if (this.selectedCodeSet) {
			newComparison.setId = this.selectedCodeSet.set_id;
		}
		//IN_GROUP_ATTRIBUTE_ID
		//SET_ID
		console.log(newComparison);
		this.__comparisonsService.CreateComparison(newComparison);

		//.then(
		//	success => {
		//		if (success && this.CurrentNode) {
		//			this.CurrentNode.attributes.push(success);
		//			this._reviewSetsService.GetReviewSets();

		//		}
		//		this._NewCode = new SetAttribute();
		//		this.CancelActivity();

		//	},
		//	error => {
		//		this.CancelActivity();
		//		console.log("error saving new code:", error, this._NewCode);

		//	})
		//.catch(
		//	error => {
		//		console.log("error(catch) saving new code:", error, this._NewCode);
		//		this.CancelActivity();
		//	}
		//);
	}
	CancelActivity(refreshTree?: boolean) {
		if (refreshTree) {
			if (this._reviewSetsService.selectedNode) {
				let IsSet: boolean = this._reviewSetsService.selectedNode.nodeType == "ReviewSet";
				let Id: number = -1;
				if (IsSet) Id = (this._reviewSetsService.selectedNode as ReviewSet).set_id;
				else Id = (this._reviewSetsService.selectedNode as SetAttribute).attribute_id;
				let sub: Subscription = this._reviewSetsService.GetReviewStatsEmit.subscribe(() => {
					console.log("trying to reselect: ", Id);
					if (IsSet) this._reviewSetsService.selectedNode = this._reviewSetsService.FindSetById(Id);
					else this._reviewSetsService.selectedNode = this._reviewSetsService.FindAttributeById(Id);
					if (sub) sub.unsubscribe();
				}
					, () => { if (sub) sub.unsubscribe(); }
				);
				this._reviewSetsService.selectedNode = null;
				this._reviewSetsService.GetReviewSets();
			}
		}
		this.PanelName = '';
		this.emitterCancel.emit();

	}
	public NewComparisonSectionOpen() {

		if (this.PanelName == 'NewComparisonSection') {
			this.PanelName = '';
		} else {
			this.PanelName = 'NewComparisonSection';
		}
	}
	public RefreshData() {

		this.getMembers();
		this.getCodeSets();
		this.getComparisons();
	}
	ngOnInit() {
		this.RefreshData();
	}
	ngOnDestroy() {

	}
    ngAfterViewInit() {

	}
	 
}
export interface kvSelectFrom {
	key: number;
	value: string;
}
