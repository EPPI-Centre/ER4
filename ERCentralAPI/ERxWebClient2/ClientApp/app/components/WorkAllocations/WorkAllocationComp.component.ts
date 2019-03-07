import { Component, Inject, OnInit, EventEmitter, Output, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocationListService, WorkAllocation } from '../services/WorkAllocationList.service';
import { ItemListService } from '../services/ItemList.service'
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { singleNode, ReviewSetsService, ReviewSet, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ReviewSetsEditingService, PerformRandomAllocateCommand } from '../services/ReviewSetsEditing.service';

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
		public _reviewSetsService: ReviewSetsService,
		public _reviewSetsEditingService : ReviewSetsEditingService
    ) { }


	@ViewChild('WithOrWithoutCode') WithOrWithoutCode!: codesetSelectorComponent;
	@ViewChild('CodingToolTree') CodingToolTree!: codesetSelectorComponent;
	@ViewChild('CodeStudiesTree') CodeStudiesTree!: codesetSelectorComponent;
	@ViewChild('AllocateOptionsDropDown') AllocateOptionsDropDown: any;
	@Output() criteriaChange = new EventEmitter();
	@Output() AllocationClicked = new EventEmitter();
	public ListSubType: string = "GetItemWorkAllocationList";
	public RandomlyAssignSection: boolean = false;
	public AssignWorkSection: boolean = false;
	public NewCodeSection: boolean = false;
	public numericRandomSample: number = 100;
	public numericRandomCreate: number = 5;
	public selectedRandomAllocateDropDown: string = 'No code / code set filter';
	public DropdownWithWithoutSelectedCode: singleNode | null = null;
	public DropdownSelectedCodingTool: singleNode | null = null;
	public DropdownSelectedCodeStudies: singleNode | null = null;
	public selectedCodeSetDropDown: ReviewSet = new ReviewSet;
	public DropDownBasicCodingTool: ReviewSet = new ReviewSet;
	public selectedMemberDropDown: Contact = new Contact;
	public CodeSets: ReviewSet[] = [];
	public isCollapsedAllocateOptions: boolean = false;
	public isCollapsedCodingTool: boolean = false;
	public isCollapsedCodeStudies: boolean = false;
	public FilterNumber: number = 1;
	public description: string = '';
	public DestAttSet: SetAttribute = new SetAttribute();
	public DestRevSet: ReviewSet = new ReviewSet();
	public FiltAttSet: SetAttribute = new SetAttribute();
	public FiltRevSet: ReviewSet = new ReviewSet();
	public index: number = 0;
	public dropdownBasicCodingTool: boolean = false;
	public dropdownBasicPerson: boolean = false;
	public workAllocation: WorkAllocation = new WorkAllocation();
	public selectedAllocated: any = { key: 1, value: 'No code / coding tool filter' };

	private _allocateOptions: kvSelectFrom[] = [{ key: 1, value: 'No code / coding tool filter'},
		{ key: 2, value: 'All without any codes from this coding tool'},
		{ key: 3, value: 'All with any codes from this coding tool' },
		{ key: 4, value: 'All with this code' },
		{ key: 5, value: 'All without this code' }];

	public get AllocateOptions(): kvSelectFrom[] {
		
		return this._allocateOptions;

	}
	public set AllocateOptions(value: kvSelectFrom[]) {

		this._allocateOptions = value;
   
	}

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
	SetRelevantDropDownValues() {

		this.index = this.AllocateOptionsDropDown.nativeElement.selectedIndex;
		this.selectedRandomAllocateDropDown = this.AllocateOptions[this.index].value;
	}

	//public openConfirmationDialogWorkAllocation(message: string) {
	//	this.confirmationDialogService.confirm('Please confirm', message, false, '')
	//		.then(
	//			(confirmed: any) => {
					
	//				if (confirmed) {
	//					// do nothing
	//					this.Assignment();
	//				} else {
	//					// do nothing
	//				}
	//			}
	//		)
	//		.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	//}

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
		if (this.AssignWorkSection) {
			this.AssignWorkSection = !this.AssignWorkSection;
		}
		this.RandomlyAssignSection = !this.RandomlyAssignSection;
	}
	public NewWorkAllocation() {
		if (this.RandomlyAssignSection) {
			this.RandomlyAssignSection = !this.RandomlyAssignSection;
		}
		if (this.NewCodeSection) {
			this.NewCodeSection = !this.NewCodeSection;
		}
		this.AssignWorkSection = !this.AssignWorkSection;
	}
	public CloseAssignSection() {
		this.AssignWorkSection = !this.AssignWorkSection;
	}
	public CloseRandomlyAssignSection() {
		this.RandomlyAssignSection = !this.RandomlyAssignSection;

	}
	public CanAssign() {

		if (this.AllocateOptions[this.index].key == 1
			&& this.DropdownSelectedCodingTool != null
			&& this.DropdownSelectedCodingTool.name != '') {
			return true;

		} else if (this.AllocateOptions[this.index].key == 2
			&& this.selectedCodeSetDropDown != null
			&& this.selectedCodeSetDropDown.name != '') {
			return true;

		} else if (this.AllocateOptions[this.index].key == 3
			&& this.selectedCodeSetDropDown != null
			&& this.selectedCodeSetDropDown.name != '') {
			return true;

		} else if (this.AllocateOptions[this.index].key == 4
			&& this.DropdownWithWithoutSelectedCode != null
			&& this.DropdownWithWithoutSelectedCode.name != '') {
			return true;

		} else if (this.AllocateOptions[this.index].key == 5
			&& this.DropdownWithWithoutSelectedCode != null
			&& this.DropdownWithWithoutSelectedCode.name != '') {
			return true;

		} else {

			return false;
		}
	}
	public Assignment() {

		this.FilterNumber = this.AllocateOptionsDropDown.nativeElement.selectedIndex;

		//console.log('This is what I mean: ' + this.FilterNumber);

		//if (this.DropdownSelectedCodingTool != null && this.DropdownWithWithoutSelectedCode != null) {
		//	// comma goes here shown by Sergio
		//	console.log(' checking nodeType 1 : ' + JSON.stringify(this.DropdownWithWithoutSelectedCode.nodeType) + ' ');
		//	console.log(' checking nodeType 2 : ' + JSON.stringify(this.DropdownSelectedCodingTool.nodeType) + ' ');
		//}
		
		if (this.DropdownWithWithoutSelectedCode != null && this.DropdownWithWithoutSelectedCode.nodeType == 'SetAttribute') {

			this.FiltAttSet = this.DropdownWithWithoutSelectedCode as SetAttribute;

		} else {

			if (this.DropdownWithWithoutSelectedCode == null) {
				this.FiltRevSet = this.selectedCodeSetDropDown as ReviewSet;
			} else {
				this.FiltRevSet = this.DropdownWithWithoutSelectedCode as ReviewSet;
			}

		}
		if (this.DropdownSelectedCodingTool != null && this.DropdownSelectedCodingTool.nodeType == 'SetAttribute') {
			this.DestAttSet = this.DropdownSelectedCodingTool as SetAttribute;
			
		} else {

			if (this.DropdownSelectedCodingTool == null) {
				this.DestRevSet = this.selectedCodeSetDropDown as ReviewSet;
			} else {
				this.DestRevSet = this.DropdownSelectedCodingTool as ReviewSet;
			}
			
			
		}
			   
		if (this.DestAttSet.attribute_id != -1 && this.DestRevSet.set_id != -1 ) {
			//this.openConfirmationDialogWorkAllocation("Please select a coding tool or a Code \n to contain the new codes to be created");
			return;
		}

		let FilterType: string  = '';
		let attributeIdFilter: number = 0;
		let setIdFilter: number = 0;
		let attributeId: number = 0;
		let setId: number = 0;
		let howMany: number = this.numericRandomCreate;

		switch (this.FilterNumber) {

			case 0:
				FilterType = "No code / code set filter";
				break;
			case 1:
				if (this.FiltRevSet.set_id == -1) {
					//this.openConfirmationDialogWorkAllocation("Please select a code to filter your documents by");
					return;
				}
				setIdFilter = this.FiltRevSet.set_id;
				FilterType = "All without any codes from this set";
				break;
			case 2:
				if (this.FiltRevSet.set_id == -1) {
					//this.openConfirmationDialogWorkAllocation("Please select a code to filter your documents by");
					return;
				}
				setIdFilter = this.FiltRevSet.set_id;
				FilterType = "All with any codes from this set";
				break;
			case 3:
				if (this.FiltAttSet.attribute_id == -1) {
					//this.openConfirmationDialogWorkAllocation("Please select a code to filter your documents by");
					return;
				}
				attributeIdFilter = this.FiltAttSet.attribute_id;
				FilterType = "All with this code";
				break;
			case 4:
				if (this.FiltAttSet.attribute_id == -1) {
					//this.openConfirmationDialogWorkAllocation("Please select a code to filter your documents by");
					return;
				}
				attributeIdFilter = this.FiltAttSet.attribute_id;
				FilterType = "All without this code";
				break;
			default:
				break;
		}


		if (this.DestAttSet.attribute_id != -1) {
			attributeId = this.DestAttSet.attribute_id;
			setId = this.DestAttSet.set_id;
		}
		else {
			setId = this.DestRevSet.set_id;
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

		this._reviewSetsEditingService.RandomlyAssignCodeToItem(assignParameters);

		this.RandomlyAssignSection = false;
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
					
					return y;
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

	setCodeSetDropDown(codeset: any) {

		//alert(JSON.stringify(codeset));
		this.selectedCodeSetDropDown = codeset;

	}
	setCodeSetDropDown2(codeset: any) {

		this.DropDownBasicCodingTool = codeset;
		this.workAllocation.setId = codeset;

	}
	SetMemberDropDown(member: any) {

		this.selectedMemberDropDown = member;
		this.workAllocation.contactId = member.contactId
	}
	WorkAssignment() {
			
		let setAtt: SetAttribute = this.DropdownSelectedCodeStudies as SetAttribute;
		this.workAllocation.attributeId = setAtt.attribute_id;
		this.workAllocation.setId = this.DropDownBasicCodingTool.set_id;
		let contact: Contact = this.selectedMemberDropDown;
		this.workAllocation.contactId = contact.contactId.toString();
		this._workAllocationListService.AssignWorkAllocation(this.workAllocation);
		this.AssignWorkSection = false;
	}
	CanNewWorkAllocationCreate(): boolean {

		if (this.DropdownSelectedCodeStudies != null && this.DropdownSelectedCodeStudies.name != ''
			&& this.DropDownBasicCodingTool.name != ''
			&& this.selectedMemberDropDown.contactName != '') {

			return false;

		} else {

			return true;
		}
	}
	CloseCodeDropDownCodeWithWithout() {

		if (this.WithOrWithoutCode) {
			this.DropdownWithWithoutSelectedCode = this.WithOrWithoutCode.SelectedNodeData;
			
		}
		this.isCollapsedAllocateOptions = false;
	}
	CloseCodeDropDownCodingTool() {
		
		if (this.CodingToolTree) {
			this.DropdownSelectedCodingTool = this.CodingToolTree.SelectedNodeData;
			console.log(JSON.stringify(this.DropdownSelectedCodingTool));
		}
		this.isCollapsedCodingTool = false;
	}
	CloseCodeDropDownStudies() {

		if (this.CodeStudiesTree) {

			this.DropdownSelectedCodeStudies = this.CodeStudiesTree.SelectedNodeData;
			let setAtt: SetAttribute = this.DropdownSelectedCodeStudies as SetAttribute;
			this.workAllocation.attributeId = setAtt.attribute_id;

		}
		this.dropdownBasicCodingTool = false;
		this.dropdownBasicPerson = false
		this.isCollapsedCodeStudies = false;
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

export interface kvSelectFrom {
	key: number;
	value: string;
}





