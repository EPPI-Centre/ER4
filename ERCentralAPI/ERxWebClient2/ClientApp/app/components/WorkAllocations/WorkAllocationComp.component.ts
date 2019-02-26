import { Component, Inject, OnInit, EventEmitter, Output, AfterContentInit, OnDestroy, Input, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subscription, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocationListService, WorkAllocation, PerformRandomAllocateCommand } from '../services/WorkAllocationList.service';
import { ItemListService } from '../services/ItemList.service'
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { singleNode, ReviewSetsService, ReviewSet, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';

@Component({
	selector: 'WorkAllocationComp',
	templateUrl: './WorkAllocationComp.component.html',
    styles: ['.UsedWorkAllocation { font-weight: bold; background-color: lightblue;}'],
    providers: []
})

export class WorkAllocationComp implements OnInit {
    constructor(
    private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
		public _workAllocationListService: WorkAllocationListService,
		public reviewInfoService: ReviewInfoService,
		public itemListService: ItemListService,
		public confirmationDialogService: ConfirmationDialogService,
		public _reviewSetsService: ReviewSetsService
    ) { }


	@ViewChild('WithOrWithoutCode1') WithOrWithoutCode1!: codesetSelectorComponent;
	@ViewChild('WithOrWithoutCode2') WithOrWithoutCode2!: codesetSelectorComponent;
	@Output() criteriaChange = new EventEmitter();
	@Output() AllocationClicked = new EventEmitter();
	public ListSubType: string = "GetItemWorkAllocationList";
	public RandomlyAssignSection: boolean = false;
	public NewCodeSection: boolean = false;
	public numericRandomSample: number = 100;
	public numericRandomCreate: number = 5;
	public selectedRandomAllocateDropDown: string = 'No code / code set filter';
	public CurrentDropdownSelectedCode: singleNode | null = null;
	public CurrentDropdownSelectedCode2: singleNode | null = null;
	public CodeSets: any[] = [];
	public isCollapsed: boolean = false;
	public isCollapsed2: boolean = false;
	public FilterNumber: number = 0;
	public description: string = '';

	private _allocateInclOrExcl: string = 'true';

	public get allocateInclOrExcl(): string {

		 this._allocateInclOrExcl;

		return this._allocateInclOrExcl;
	}
	public set allocateInclOrExcl(value: string) {

		 this._allocateInclOrExcl = value;

		if (value == 'true' || value == 'false') this._allocateInclOrExcl = value;
		else console.log("I'm not doing it :-P ", value);

	}

    ngOnInit() {
        this.RefreshData();
	}
	public openConfirmationDialogWorkAllocation(message: string) {
		this.confirmationDialogService.confirm('Please confirm', message, false, '')
			.then(
				(confirmed: any) => {
					
					if (confirmed) {
						// do nothing
						this.Assignment();
					} else {
						// do nothing
					}
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}
	public NewCode() {

		if (this.RandomlyAssignSection) {
			this.RandomlyAssignSection = !this.RandomlyAssignSection;
		}
		this.NewCodeSection = !this.NewCodeSection;
	}
	public RandomlyAssign() {

		if (this.NewCodeSection) {
			this.NewCodeSection = !this.NewCodeSection;
		}
		this.RandomlyAssignSection = !this.RandomlyAssignSection;
	}
	public Assignment() {

		let DestAttSet: SetAttribute = new SetAttribute();
		let DestRevSet: ReviewSet = new ReviewSet();
		let FiltAttSet: SetAttribute = new SetAttribute();
		let FiltRevSet: ReviewSet = new ReviewSet();

		if (this.CurrentDropdownSelectedCode2 != null && this.CurrentDropdownSelectedCode != null) {

			console.log(' checking nodeType 1 : ' + JSON.stringify(this.CurrentDropdownSelectedCode.nodeType) + ' ');
			console.log(' checking nodeType 2 : ' + JSON.stringify(this.CurrentDropdownSelectedCode2.nodeType) + ' ');
		}

		if (this.CurrentDropdownSelectedCode != null && this.CurrentDropdownSelectedCode.nodeType == 'SetAttribute') {

			FiltAttSet = this.CurrentDropdownSelectedCode as SetAttribute;

		} else {

			FiltRevSet = this.CurrentDropdownSelectedCode as ReviewSet;

		}
		if (this.CurrentDropdownSelectedCode2 != null && this.CurrentDropdownSelectedCode2.nodeType == 'SetAttribute') {
			DestAttSet = this.CurrentDropdownSelectedCode2 as SetAttribute;
			
		} else {
			DestRevSet = this.CurrentDropdownSelectedCode2 as ReviewSet;
			
		}

		//console.log(' checking parameters 1: ' + JSON.stringify(FiltAttSet) + ' ');
		//console.log(' checking parameters 2: ' + JSON.stringify(FiltRevSet) + ' ');
		//console.log(' checking parameters 3: ' + JSON.stringify(DestAttSet) + ' ');
		//console.log(' checking parameters 4: ' + JSON.stringify(DestRevSet) + ' ');


		if (DestAttSet.attribute_id != -1  && DestRevSet.set_id != -1 ) {

			this.openConfirmationDialogWorkAllocation("Please select a CodeSet or a Code \n to contain the new codes to be created");
			return;
		}

		let FilterType: string  = '';
		let attributeIdFilter: number = 0;
		let setIdFilter: number = 0;
		let attributeId: number = 0;
		let setId: number = 0;
		let howMany: number = this.numericRandomCreate;

		switch (this.FilterNumber) {

			case 1:
				FilterType = "No code / code set filter";
				break;
			case 2:
				if (FiltRevSet.set_id == -1) {
					this.openConfirmationDialogWorkAllocation("Please select a code to filter your documents by");
					return;
				}
				setIdFilter = FiltRevSet.set_id;
				FilterType = "All without any codes from this set";
				break;
			case 3:
				if (FiltRevSet.set_id == -1) {
					this.openConfirmationDialogWorkAllocation("Please select a code to filter your documents by");
					return;
				}
				setIdFilter = FiltRevSet.set_id;
				FilterType = "All with any codes from this set";
				break;
			case 4:
				if (FiltAttSet.attribute_id == -1) {
					this.openConfirmationDialogWorkAllocation("Please select a code to filter your documents by");
					return;
				}
				attributeIdFilter = FiltAttSet.attribute_id;
				FilterType = "All with this code";
				break;
			case 5:
				if (FiltAttSet.attribute_id == -1) {
					this.openConfirmationDialogWorkAllocation("Please select a code to filter your documents by");
					return;
				}
				attributeIdFilter = FiltAttSet.attribute_id;
				FilterType = "All without this code";
				break;
			default:
				break;
		}


		if (DestAttSet.attribute_id != -1) {
			attributeId = DestAttSet.attribute_id;
			setId = DestAttSet.set_id;
		}
		else {
			setId = DestRevSet.set_id;
			attributeId = 0;
		}

		let assignParameters: PerformRandomAllocateCommand = new PerformRandomAllocateCommand();
		assignParameters.FilterType = FilterType;
		assignParameters.attributeIdFilter = attributeIdFilter;
		assignParameters.setIdFilter = setIdFilter;
		assignParameters.attributeId = attributeId;
		assignParameters.setId = setId;
		assignParameters.howMany = howMany;
		assignParameters.numericRandomSample = this.numericRandomSample;
		assignParameters.RandomSampleIncluded = this.allocateInclOrExcl;

		this._workAllocationListService.RandomlyAssignCodeToItem(assignParameters);
	}


	public openConfirmationDialogDeleteWA(workAllocationId: number) {

		this.confirmationDialogService.confirm('Please confirm', 'You are deleting a work allocation', false, '')
			.then(
				(confirmed: any) => {
					
					if (confirmed) {

						this.DeleteWorkAllocation(workAllocationId);

					} else {
						//alert('did not confirm');
					}
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}
	removeWarning(workAllocationId: number) {

		this.openConfirmationDialogDeleteWA(workAllocationId);
	}

    public RefreshData() {
        this.getMembers();
		this._workAllocationListService.FetchAll();
		this.getCodeSets();
	}
	public getCodeSets() {

		this.CodeSets = this._reviewSetsService.ReviewSets.filter(x => x.nodeType == 'ReviewSet')
			.map(
				(y: ReviewSet) => {
					return y.name;
				}
			);
		

	}
	public nextAllocateDropDownList(num: number, val: string) {

		this.FilterNumber = num;
		this.selectedRandomAllocateDropDown = val;

	}
	public CanOnlySelectRoots() : boolean{
		return true;
	}
	public selectedCodeSetDropDown: any = undefined;
	setCodeSetDropDown(codeset: any) {

		this.selectedCodeSetDropDown = codeset;
	}
	CloseCodeDropDown() {
		//alert(this.WithOrWithoutCode1);
		if (this.WithOrWithoutCode1) {
			this.CurrentDropdownSelectedCode = this.WithOrWithoutCode1.SelectedNodeData;
			
		}
		this.isCollapsed = false;
	}
	CloseCodeDropDown2() {
		//alert(this.WithOrWithoutCode2);
		if (this.WithOrWithoutCode2) {
			this.CurrentDropdownSelectedCode2 = this.WithOrWithoutCode2.SelectedNodeData;
			
		}
		this.isCollapsed2 = false;
	}
	getMembers() {

		if (!this.reviewInfoService.ReviewInfo || this.reviewInfoService.ReviewInfo.reviewId < 1) {
						this.reviewInfoService.Fetch();
		}
		this.reviewInfoService.FetchReviewMembers();

	}

	DeleteWorkAllocation(workAllocationId: number) {

		this._workAllocationListService.DeleteWorkAllocation(workAllocationId);
	}

	LoadGivenList(workAllocationId: number, subtype: string) {

		
		for (let workAll of this._workAllocationListService.AllWorkAllocationsForReview) {
			if (workAll.workAllocationId == workAllocationId) {
	
				this.ListSubType = subtype;
				this.criteriaChange.emit(workAll);
				this.AllocationClicked.emit();
				return;
			}
		}
	}


}





