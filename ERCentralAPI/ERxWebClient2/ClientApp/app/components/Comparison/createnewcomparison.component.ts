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
import { NONE_TYPE } from '@angular/compiler/src/output/output_ast';


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

	
	public PanelName: string = '';
	public CodeSets: ReviewSet[] = [];
	public selectedReviewer1: Contact = new Contact();
	public selectedReviewer2: Contact = new Contact();
	public selectedReviewer3: Contact = new Contact();
	public selectedCodeSet: ReviewSet = new ReviewSet();
	public selectedFilter!: singleNode;
	@Output() emitterCancel = new EventEmitter();


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
	setOptionalMember(member: Contact) {

		this.selectedReviewer3.contactId = member.contactId;
		this.selectedReviewer3.contactName = member.contactName;
	
	}
	setsFilter() {

		if (this._reviewSetsService.selectedNode && this._reviewSetsService.selectedNode.nodeType == "SetAttribute") {
			this.selectedFilter = this._reviewSetsService.selectedNode as SetAttribute;
		} 

	}
	CreateNewComparison() {

		let newComparison: Comparison = new Comparison();
		let tempReviewer = this._reviewInfoService.Contacts;
		if (this.selectedReviewer1.contactName == '') {
			newComparison.contactId1 = tempReviewer[0].contactId;
			newComparison.contactName1 = tempReviewer[0].contactName;
		} else {
			newComparison.contactId1 = this.selectedReviewer1.contactId;
			newComparison.contactName1 = this.selectedReviewer1.contactName;
		}
		if (this.selectedReviewer2.contactName == '') {
			newComparison.contactId2 = tempReviewer[0].contactId;
			newComparison.contactName2 = tempReviewer[0].contactName;
		} else {
			newComparison.contactId2 = this.selectedReviewer2.contactId;
			newComparison.contactName2 = this.selectedReviewer2.contactName;
		}

		if (this.selectedCodeSet) {
			newComparison.setId = this.selectedCodeSet.set_id;
			newComparison.setName  = this.selectedCodeSet.set_name;
		}
		if (this.selectedReviewer3.contactName != '') {
			newComparison.contactId3 = this.selectedReviewer3.contactId;
			newComparison.contactName3 = this.selectedReviewer3.contactName
		}
		if (this.selectedFilter.name != '' && this.selectedFilter.nodeType == 'SetAttribute') {
			let temp = this.selectedFilter as SetAttribute;
			newComparison.inGroupAttributeId = temp.attribute_id ;
			newComparison.attributeName = temp.attribute_name;
		}

		console.log('hello' + newComparison);
		this.__comparisonsService.CreateComparison(newComparison);

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
		this.selectedCodeSet = this.CodeSets[0];
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
