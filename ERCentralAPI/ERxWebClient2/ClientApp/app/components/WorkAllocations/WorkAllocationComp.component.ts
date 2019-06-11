import { Component, Inject, OnInit, EventEmitter, Output, ViewChild, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocationListService, WorkAllocation } from '../services/WorkAllocationList.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { singleNode, ReviewSetsService, ReviewSet, SetAttribute, kvAllowedAttributeType } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ReviewSetsEditingService, PerformRandomAllocateCommand } from '../services/ReviewSetsEditing.service';
import { Subscription } from 'rxjs';
import { ComparisonsService, Comparison } from '../services/comparisons.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ComparisonComp } from '../Comparison/createnewcomparison.component';
import { ComparisonStatsComp } from '../Comparison/comparisonstatistics.component';
import { TabStripComponent } from '@progress/kendo-angular-layout';
import { ItemListComp } from '../ItemList/itemListComp.component';

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
		private reviewInfoService: ReviewInfoService,
		private confirmationDialogService: ConfirmationDialogService,
		private _reviewSetsService: ReviewSetsService,
		private _reviewSetsEditingService: ReviewSetsEditingService,
		public _comparisonsService: ComparisonsService,
		private _notificationService: NotificationService,
		 @Inject('BASE_URL') private _baseUrl: string
    ) { }

	
	@ViewChild('WithOrWithoutCode') WithOrWithoutCode!: codesetSelectorComponent;
    @ViewChild('CodingToolTree') CodingToolTree!: codesetSelectorComponent;
    @ViewChild('CodeStudiesTree') CodeStudiesTree!: codesetSelectorComponent;
	@ViewChild('AllocateOptionsDropDown') AllocateOptionsDropDown: any;
	@ViewChild('CodeTypeSelectCollaborate') CodeTypeSelect: any;
	@ViewChild('ComparisonComp') ComparisonComp!: ComparisonComp;
	@ViewChild('ComparisonStatsCompList') ComparisonStatsComp!: ComparisonStatsComp;
	@Output() criteriaChange = new EventEmitter();
	@Output() AllocationClicked = new EventEmitter();
	@Input() tabstrip!: TabStripComponent;
	@Input() ItemList!: ItemListComp;

	public ListSubType: string = "GetItemWorkAllocationList";
	public RandomlyAssignSection: boolean = false;
	public AssignWorkSection: boolean = false;
	public NewCodeSection: boolean = false;
	public numericRandomSample: number = 100;
	public numericRandomCreate: number = 5;
	public DropdownWithWithoutSelectedCode: singleNode | null = null;
	public DropdownSelectedCodingTool: singleNode | null = null;
	public DropdownSelectedCodeStudies: singleNode | null = null;
	public selectedCodeSetDropDown: ReviewSet = new ReviewSet();
	public DropDownBasicCodingTool: ReviewSet = new ReviewSet();
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
	public dropdownBasicCodingTool: boolean = false;
    public dropdownBasicPerson: boolean = false;
    public ShowAllocations: boolean = true;
    public ShowComparisons: boolean = true;
	public workAllocation: WorkAllocation = new WorkAllocation();
    public selectedAllocated: kvSelectFrom = { key: 1, value: 'No code / coding tool filter' };
	public PanelName: string = '';
    public get chosenFilter(): singleNode | null {
        return this._reviewSetsService.selectedNode;
    }
    public get chosenFilterName(): string{
        if (this._reviewSetsService.selectedNode) {
            return this._reviewSetsService.selectedNode.name;
        }
        else {
            return "No code selected";
        }
    }
	private _allocateOptions: kvSelectFrom[] = [{ key: 1, value: 'No code / coding tool filter'},
		{ key: 2, value: 'All without any codes from this coding tool'},
		{ key: 3, value: 'All with any codes from this coding tool' },
		{ key: 4, value: 'All with this code' },
		{ key: 5, value: 'All without this code' }];

	private _ReportHTML: string = "";
	public get ReportHTML(): string {
		return this._ReportHTML;
	}
    public get ContactWorkAllocations(): WorkAllocation[] {
        return this._workAllocationListService.ContactWorkAllocations;
    }
    public get AllWorkAllocationsForReview(): WorkAllocation[] {
        return this._workAllocationListService.AllWorkAllocationsForReview;
    }
    public get Comparisons(): Comparison[] {
        return this._comparisonsService.Comparisons;
    }
    public get Contacts(): Contact[] {
        return this.reviewInfoService.Contacts;
    }
    public get ShowAllocationsText(): string {
        if (this.ShowAllocations) return "Collapse";
        else return "Expand";
    }
    public get ShowComparisonsText(): string {
        if (this.ShowComparisons) return "Collapse";
        else return "Expand";
    }
	public get AllocateOptions(): kvSelectFrom[] {
		
		return this._allocateOptions;

	}
	public set AllocateOptions(value: kvSelectFrom[]) {

		this._allocateOptions = value;
   
	}
	public get HasWriteRights(): boolean {

        return this.ReviewerIdentityServ.HasWriteRights;
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
    public get NoSuitableCodeSet(): boolean {
        //console.log("NoSuitableCodeSet", this.CodingToolTree);
        //if (!this.CodingToolTree) return true;
        //else if (this.CodingToolTree.nodes && this.CodingToolTree.nodes.length > 0) return false;
        //else return true;
        let ind = this._reviewSetsService.ReviewSets.findIndex(found => found.setType.allowRandomAllocation == true);
        if (ind > -1) return false;
        else return true;
    }
    
	public NewComparisonSectionOpen() {

		if (this.PanelName == 'NewComparisonSection') {
			this.PanelName = '';
		} else {
			this.PanelName = 'NewComparisonSection';
		}
	}
	
	ngOnInit() {
		this.RefreshData();
	}
	public get AllowedChildTypes(): kvAllowedAttributeType[] {
		let res: kvAllowedAttributeType[] = [];
		if (!this.CurrentNode) return res;
		let att: SetAttribute | null = null;
		let Set: ReviewSet | null = null;
		if (this.CurrentNode.nodeType == "ReviewSet") Set = this.CurrentNode as ReviewSet;
		else if (this.CurrentNode.nodeType == "SetAttribute") {
			att = this.CurrentNode as SetAttribute;
			if (att && att.set_id > 0) Set = this._reviewSetsService.FindSetById(att.set_id);
			if (!Set) return res;
		}
		//console.log("CurrentNode (Set)", Set);
		if (Set && Set.setType) {
			//console.log("allowed child types... ", Set.setType.allowedCodeTypes, Set.setType.allowedCodeTypes[0].key, Set.setType.allowedCodeTypes.filter(res => !res.value.endsWith('- N/A)')));
			return Set.setType.allowedCodeTypes.filter(res => !res.value.endsWith('- N/A)'));
		}
		return res;
	}
	IsNewCodeNameValid() {

		return true;
		//if (this.PanelName == 'NewCodeSection') {
		//	if (this._NewCode.attribute_name.trim() != "") return true;
		//	else return false;
		//} 
	}
	IsServiceBusy(): boolean {
		if (this._reviewSetsEditingService.IsBusy || this.reviewInfoService.IsBusy) return true;
		else return false;
	}
	CanWrite(): boolean {
		//console.log('CanWrite', this.ReviewerIdentityServ.HasWriteRights, this.IsServiceBusy());
		if (this.ReviewerIdentityServ.HasWriteRights && !this.IsServiceBusy()) {
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
	getStatistics(comparisonId: number) {

		if (this.PanelName == 'getStats' + comparisonId.toString()) {
			this.PanelName = '';
		} else {
			this.PanelName = 'getStats' + comparisonId.toString();
			if (this._comparisonsService && comparisonId != null) {
				
				this._comparisonsService.FetchStats(comparisonId);
			}
		}
		
    }
    public get CanRunQuickReport(): boolean {
        if (this.chosenFilter == null || this._comparisonsService.currentComparison == null) {
            return false;
        }
        else if (this._comparisonsService.currentComparison.setId == this.chosenFilter.set_id
            && this._comparisonsService.currentComparison.setId > 0
            && this.chosenFilter.attributes.length > 0
        ) {
            return true;
        }
        else return false;
    }
	RunHTMLComparisonReport() {

		if (this.chosenFilter == null) {
			return;
		}
		if (this._comparisonsService.currentComparison == null) {
			return;
		}
		let ParentAttributeId: number = 0;
		let SetId: number = 0;

        if (this.chosenFilter.nodeType == 'SetAttribute')
        {
            let aSet = this.chosenFilter as SetAttribute;
            ParentAttributeId = aSet.attribute_id;
            SetId = aSet.set_id;
		}else
		{
			if (this.chosenFilter.nodeType == 'ReviewSet')
			{
				SetId = (this.chosenFilter as ReviewSet).set_id;
			}
		}

		this._comparisonsService.FetchComparisonReport(
			this._comparisonsService.currentComparison.comparisonId,
			ParentAttributeId, SetId, this.chosenFilter);
	}
	
	getPanelRunQuickReport(comparisonId: number) {
		
		this._comparisonsService.currentComparison = this._comparisonsService.Comparisons.filter(x => x.comparisonId == comparisonId)[0];
		if (this.PanelName == 'runQuickReport' + comparisonId.toString()) {
			this.PanelName = '';
		} else {
			this.PanelName = 'runQuickReport' + comparisonId.toString();
		}
	}
    SetRelevantDropDownValues(selection: number) {
        console.log("SetRelevantDropDownValues", JSON.stringify(selection));
        let ind = this.AllocateOptions.findIndex(found => found.key == selection);
		if (ind > -1) this.selectedAllocated = this.
			AllocateOptions[ind];
        else this.selectedAllocated = this.AllocateOptions[0];
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
	canSetFilter(): boolean {
		if (this._reviewSetsService.selectedNode
			) return true;
		return false;
	}
	CreateNewCode() {
	
		if (this.CurrentNode) {

			this._NewCode.order = this.CurrentNode.attributes.length;

			if (this.CurrentNode.nodeType == "ReviewSet") {
				this._NewCode.set_id = (this.CurrentNode as ReviewSet).set_id;
				this._NewCode.parent_attribute_id = 0;
			}
			else if (this.CurrentNode.nodeType == "SetAttribute") {
				this._NewCode.set_id = (this.CurrentNode as SetAttribute).set_id;
				this._NewCode.parent_attribute_id = (this.CurrentNode as SetAttribute).attribute_id;
			}
		}
		else {
			this._NewReviewSet.order = 0;
		}
		console.log("What the hell?", this.CodeTypeSelect, this.CodeTypeSelect.nativeElement.selectedOptions, this.CodeTypeSelect.nativeElement.selectedOptions.length);
		
		if (this.CodeTypeSelect && this.CodeTypeSelect.nativeElement.selectedOptions && this.CodeTypeSelect.nativeElement.selectedOptions.length > 0) {
			this._NewCode.attribute_type_id = this.CodeTypeSelect.nativeElement.selectedOptions[0].value;
			this._NewCode.attribute_type = this.CodeTypeSelect.nativeElement.selectedOptions[0].text;
		}
		else {
			this._NewCode.attribute_type_id = 1;//non selectable HARDCODED WARNING!
			this._NewCode.attribute_type = "Not selectable(no checkbox)";
		}

		console.log("will create:", this._NewCode, this.CodeTypeSelect);
		this._reviewSetsEditingService.SaveNewAttribute(this._NewCode)
			.then(
				success => {
					if (success && this.CurrentNode) {
						this.CurrentNode.attributes.push(success);
						this._reviewSetsService.GetReviewSets();
						
					}
					this._NewCode = new SetAttribute();
					this.CancelActivity();
					
				},
				error => {
					this.CancelActivity();
					console.log("error saving new code:", error, this._NewCode);
					
				})
			.catch(
				error => {
					console.log("error(catch) saving new code:", error, this._NewCode);
					this.CancelActivity();
				}
			);
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
	}

	public NewCodeSectionOpen() {

		if (this.PanelName == 'NewCodeSection') {
			this.PanelName = '';
		} else {
			this.PanelName = 'NewCodeSection';
		}
	}
	public RandomlyAssign() {

		if (this.PanelName == 'RandomlyAssignSection') {
			this.PanelName = '';
		} else {
			this.PanelName = 'RandomlyAssignSection';
		}
	}

	public CanCreateNewCode(): boolean {

		if (this._reviewSetsService.selectedNode && this.CurrentCodeCanHaveChildren) return true;
		else return false;

	}
	public Clear() {
		
        this.selectedAllocated = this.AllocateOptions[0];
		this.DropdownSelectedCodingTool = null;
		this.selectedCodeSetDropDown = new ReviewSet();
		this.DropdownWithWithoutSelectedCode = null;
		this.DropdownSelectedCodeStudies = null;
		this.DropDownBasicCodingTool = new ReviewSet();
		this.selectedMemberDropDown = new Contact();
		this.PanelName = '';
		//alert('Called Clear: ' + this.DropdownSelectedCodingTool);
		
	}
	public NewWorkAllocation() {
		if (this.PanelName == 'AssignWorkSection') {
			this.PanelName = '';
		} else {
			this.PanelName = 'AssignWorkSection';
		}
	}
	public CloseAssignSection() {
			this.PanelName = '';
	}
	public CloseRandomlyAssignSection() {

		this.RandomlyAssign();

	}
    public CanAssign() {
        //console.log(this.numericRandomCreate, this.numericRandomSample);
        if (this.numericRandomCreate == null || this.numericRandomSample == null) return false;
        else if (this.numericRandomCreate == 0 || this.numericRandomSample == 0) return false;
		if (this.DropdownSelectedCodingTool != null
			&& this.DropdownSelectedCodingTool.name != '') {
			if (this.selectedAllocated.key == 1
				&& this.DropdownSelectedCodingTool != null
				&& this.DropdownSelectedCodingTool.name != '') {
				return true;

			} else if (this.selectedAllocated.key == 2
				&& this.selectedCodeSetDropDown != null
				&& this.selectedCodeSetDropDown.name != '') {
				return true;

			} else if (this.selectedAllocated.key == 3
				&& this.selectedCodeSetDropDown != null
				&& this.selectedCodeSetDropDown.name != '') {
				return true;

			} else if (this.selectedAllocated.key == 4
				&& this.DropdownWithWithoutSelectedCode != null
				&& this.DropdownWithWithoutSelectedCode.name != '') {
				return true;

			} else if (this.selectedAllocated.key == 5
				&& this.DropdownWithWithoutSelectedCode != null
				&& this.DropdownWithWithoutSelectedCode.name != '') {
				return true;

            } else {
                return false;
			}
        }
        else {
            return false;
		}

	}

	public Assignment() {

        if (!this.CanAssign()) return;
		this.FilterNumber = this.AllocateOptionsDropDown.nativeElement.selectedIndex;
				
		
		if (this.DropdownWithWithoutSelectedCode != null && this.DropdownWithWithoutSelectedCode.nodeType == 'SetAttribute') {

			this.FiltAttSet = this.DropdownWithWithoutSelectedCode as SetAttribute;

		} else {

			if (this.DropdownWithWithoutSelectedCode == null) {

				this.FiltRevSet = this.selectedCodeSetDropDown as ReviewSet;

			} else {
				this.FiltRevSet = this.DropdownWithWithoutSelectedCode as ReviewSet;
			}

		}

		//if (this.DropdownSelectedCodingTool != null ) {
			// comma goes here shown by Sergio
			//console.log(' checking nodeType 1 : ' + JSON.stringify(this.DropdownWithWithoutSelectedCode.nodeType) + ' ');
			//console.log(' testing 1 : ' + JSON.stringify(this.DropdownSelectedCodingTool.nodeType) + ' ');
		//}

		if (this.DropdownSelectedCodingTool != null && this.DropdownSelectedCodingTool.nodeType == 'SetAttribute') {

			this.DestAttSet = this.DropdownSelectedCodingTool as SetAttribute;
			//alert(JSON.stringify(this.DestAttSet));
			
		} else {

			if (this.DropdownSelectedCodingTool != null) {
			//	this.DestRevSet = this.selectedCodeSetDropDown as ReviewSet;
			//} else {
				this.DestRevSet = this.DropdownSelectedCodingTool as ReviewSet;
			}
		}

		//console.log(' testing attribtue destination : ' + JSON.stringify(this.DestAttSet.attribute_id));
		//console.log(' testing codeset destination : ' + JSON.stringify(this.DestRevSet.set_id));

		//console.log(' testing dropdown : ' + JSON.stringify(this.DropdownSelectedCodingTool));
		
		if (this.DestAttSet.attribute_id == -1 && this.DestRevSet.set_id == -1) {
			//alert('in here now');
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

		// Could place logic in here for allowed to randomly allocate
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

		if (this.numericRandomCreate == null || this.numericRandomCreate == undefined
			|| this.numericRandomCreate < 1 || this.numericRandomCreate > 10) {

			return;
		}
		if (this.numericRandomSample == null || this.numericRandomSample == undefined
			|| this.numericRandomSample < 1 || this.numericRandomSample > 100) {

			return;
		}
		//console.log(JSON.stringify(assignParameters));

		this._reviewSetsEditingService.RandomlyAssignCodeToItem(assignParameters);

		//this.DropdownSelectedCodingTool = null;
		//this.DropdownWithWithoutSelectedCode = null;
		//this.selectedCodeSetDropDown = new ReviewSet();
		//this.DestRevSet.set_id = -1;
		//this.DestAttSet.attribute_id = -1;

		this.RandomlyAssignSection = false;
	}

    GoToEditCodesets() {
        this.RandomlyAssignSection = false;
        this.router.navigate(['EditCodeSets']);
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
	public openConfirmationDialogDeleteComp(comparisonId: number) {

		this.confirmationDialogService.confirm('Please confirm', 'You are deleting a comparison', false, '')
			.then(
				(confirmed: any) => {

					if (confirmed) {

						this.DeleteComparison(comparisonId);

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
	removeComparisonWarning(comparisonId: number) {
		
		this.openConfirmationDialogDeleteComp(comparisonId);
	}
	NotImplemented() {
		alert('not implemented');
	}
    public RefreshData() {
        this.getMembers();
		this._workAllocationListService.FetchAll();
		this.getCodeSets();
		this._comparisonsService.FetchAll();
	}
	public getCodeSets() {
		this.CodeSets = this._reviewSetsService.ReviewSets.filter(x => x.nodeType == 'ReviewSet')
			.map(
			(y: ReviewSet) => {
					
					return y;
				}
			);
	}
	
	public CanOnlySelectRoots() : boolean{
		return true;
	}

	setCodeSetDropDown(codeset: any) {

		//alert(JSON.stringify(codeset));
		this.selectedCodeSetDropDown = codeset;
		this.DropdownSelectedCodingTool = null;
	}
	setCodeSetDropDown2(codeset: any) {

		this.DropDownBasicCodingTool = codeset;
		this.workAllocation.setId = codeset;

	}
	SetMemberDropDown(member: any) {

		this.selectedMemberDropDown = member;
		this.workAllocation.contactId = member.contactId
	}
	private showAssignmentNotification(status: string): void {

		let typeElement: "success" | "error" | "none" | "warning" | "info" | undefined = undefined;
		let contentSt: string = "";
		if (status == "Success") {
			typeElement = "success";
			contentSt = 'Assignment updated succesfully.';
		}//type: { style: 'error', icon: true }
		else {
			typeElement = "error";
			contentSt = 'The Assignment creation failed, if the problem persists, please contact EPPISupport.';
		}
		this._notificationService.show({
			content: contentSt,
			animation: { type: 'slide', duration: 400 },
			position: { horizontal: 'center', vertical: 'top' },
			type: { style: typeElement, icon: true },
			closable: true
		});
	}
	//calculateStats() {
	//	alert('calling');
	//	this._comparisonsService.calculateStats();
	//}
	WorkAssignment() {


		let setAtt: SetAttribute = this.DropdownSelectedCodeStudies as SetAttribute;
		this.workAllocation.attributeId = setAtt.attribute_id;
		this.workAllocation.setId = this.DropDownBasicCodingTool.set_id;
		let contact: Contact = this.selectedMemberDropDown;
		this.workAllocation.contactId = contact.contactId.toString();
		this._workAllocationListService.AssignWorkAllocation(this.workAllocation)
			.then(

			() => {

					this.showAssignmentNotification("Success");
					this.AssignWorkSection = false;
					this.PanelName = '';

				});
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
			this.selectedCodeSetDropDown = new ReviewSet();
		}
		this.isCollapsedAllocateOptions = false;
	}
	CloseCodeDropDownCodingTool() {
		
        if (this.CodingToolTree) {
            //note here ViewChild inside *ngIf are tricky:
            //https://stackoverflow.com/questions/39366981/angular-2-viewchild-in-ngif
            //for some reason, when this code executes we do have this.CodingToolTree...

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
	DeleteComparison(comparisonId: number) {

		this._comparisonsService.DeleteComparison(comparisonId);
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

	//LoadComparisonList(comparison: Comparison) {

	//	this..LoadComparisonList(comparison, this.ListSubType);
	//}

	setCompListType($event: any) {
		this.ComparisonStatsComp.ListSubType = $event;

	}
}

export interface kvSelectFrom {
	key: number;
	value: string;
}





