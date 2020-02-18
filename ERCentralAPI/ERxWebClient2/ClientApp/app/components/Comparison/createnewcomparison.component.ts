import { Component,  OnInit,  Output, EventEmitter } from '@angular/core';
import { ReviewSetsService, SetAttribute, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Subscription } from 'rxjs';
import { ComparisonsService, Comparison } from '../services/comparisons.service';


@Component({
    selector: 'CreateNewComparisonComp',
    templateUrl: './createnewcomparison.component.html',
    providers: []
})

export class ComparisonComp implements OnInit {
    constructor(
		private _reviewSetsService: ReviewSetsService,
		private __comparisonsService: ComparisonsService,
		private _reviewInfoService: ReviewInfoService,
		private _reviewerIdentityServ: ReviewerIdentityService,
		private _reviewSetsEditingService: ReviewSetsEditingService
	) { }

	
	public PanelName: string = '';
	public chosenFilter: SetAttribute | null = null;
    public get CodeSets(): ReviewSet[] {
		return this._reviewSetsService.ReviewSets.filter(x => x.setType.allowComparison != false);
    }
	public selectedReviewer1: Contact = new Contact();
	public selectedReviewer2: Contact = new Contact();
	public selectedReviewer3: Contact = new Contact();
	public selectedCodeSet: ReviewSet = new ReviewSet();
	@Output() emitterCancel = new EventEmitter();
	ngOnInit() {
		this.RefreshData();

    }
    onSubmit(): boolean {
        console.log("create new comparison onSubmit");
        return false;
    }
	public get Contacts(): Contact[] {
		return this._reviewInfoService.Contacts;
	}
	canSetFilter(): boolean {
		if (this._reviewSetsService.selectedNode
			&& this._reviewSetsService.selectedNode.nodeType == "SetAttribute") return true;
		return false;
	}
	SetFilter() {
		if (this._reviewSetsService.selectedNode && this._reviewSetsService.selectedNode.nodeType == "SetAttribute")
			this.chosenFilter = this._reviewSetsService.selectedNode as SetAttribute;
		//if (this.chosenFilter) {
		//	this.selectedFilter = this.chosenFilter;
		//}
		
	}
	clearChosenFilter() {
		this.chosenFilter = null;
		//this.selectedFilter = new SetAttribute();
	}
	//getMembers() {

	//	if (!this._reviewInfoService.ReviewInfo || this._reviewInfoService.ReviewInfo.reviewId < 1) {
	//		this._reviewInfoService.Fetch();
	//	}
	//	this._reviewInfoService.FetchReviewMembers();

	//}

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
	CanCreateComparison(): boolean {

		if (this.selectedReviewer1.contactId > 0 &&
			this.selectedReviewer2.contactId > 0 &&
			this.selectedCodeSet != null &&
			this.selectedCodeSet.setType.allowComparison &&
			this.selectedCodeSet.set_id > 0
			&& this.CanWrite()
			)
		{
			if (this.selectedReviewer1.contactId != this.selectedReviewer2.contactId &&
				this.selectedReviewer1.contactId != this.selectedReviewer3.contactId &&
				this.selectedReviewer3.contactId != this.selectedReviewer2.contactId ) {
				return false;
			} else {
				return true;
			}

		} else {
			return true;
		}
	}
	setOptionalMember(member: Contact) {

		this.selectedReviewer3.contactId = member.contactId;
		this.selectedReviewer3.contactName = member.contactName;
	
	}

	public get CurrentNode(): singleNode | null {
		if (!this._reviewSetsService.selectedNode) return null;
		else return this._reviewSetsService.selectedNode;
	}
	CreateNewComparison() {

		if(this.CanCreateComparison()) return;
		let newComparison: Comparison = new Comparison();
		

		if (this.selectedReviewer1.contactId > 0) {
			newComparison.contactId1 = this.selectedReviewer1.contactId;
			newComparison.contactName1 = this.selectedReviewer1.contactName;
		}
		if (this.selectedReviewer2.contactId > 0) {
			newComparison.contactId2 = this.selectedReviewer2.contactId;
			newComparison.contactName2 = this.selectedReviewer2.contactName;
		} 
		if (this.selectedReviewer3.contactId > 0) {
			newComparison.contactId3 = this.selectedReviewer3.contactId;
			newComparison.contactName3 = this.selectedReviewer3.contactName
		}

		if (this.selectedCodeSet) {
			newComparison.setId = this.selectedCodeSet.set_id;
			newComparison.setName  = this.selectedCodeSet.set_name;
		}
		
		if (this.chosenFilter) {
			if (this.chosenFilter.name != '' && this.chosenFilter.nodeType == 'SetAttribute') {
				let temp = this.chosenFilter as SetAttribute;
				newComparison.inGroupAttributeId = temp.attribute_id ;
				newComparison.attributeName = temp.attribute_name;
				}
		}
		
		//console.log('New comaprison: ',newComparison);
        this.__comparisonsService.CreateComparison(newComparison);
        this.emitterCancel.emit();
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
	public RefreshData() {
		//this.getMembers();
		this.getComparisons();
		this.selectedCodeSet = this.CodeSets[0];
		this.chosenFilter = null;
	}

	Clear() {
		this.selectedCodeSet = new ReviewSet();
		this.chosenFilter = new SetAttribute();
		this.selectedReviewer1 = new Contact();
		this.selectedReviewer2 = new Contact();
		this.selectedReviewer3 = new Contact();
	}
}