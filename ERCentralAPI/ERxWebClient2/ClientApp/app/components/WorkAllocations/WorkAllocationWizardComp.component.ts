import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter, Attribute } from '@angular/core';
import { ReviewSetsService, kvAllowedAttributeType, SetAttribute, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService, ChangeDataEntryMessage } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocationListService, WorkAllocationFromWizardCommand, WorkAllocWizardResult } from '../services/WorkAllocationList.service';
import { kvSelectFrom } from './WorkAllocationComp.component';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { forEach } from '@angular/router/src/utils/collection';


@Component({
    selector: 'WorkAllocationWizardComp',
    templateUrl: './WorkAllocationWizardComp.component.html',
    styles: ['.DisabledStep { font-weight: light; opacity: 50%;}'],
    providers: []
})

export class WorkAllocationWizardComp implements OnInit, OnDestroy {
    constructor(
        private ReviewSetsService: ReviewSetsService,
        private ReviewSetsEditingService: ReviewSetsEditingService,
        private ReviewInfoService: ReviewInfoService,
        private ReviewerIdentityService: ReviewerIdentityService,
        private WorkAllocationListService: WorkAllocationListService
	) { }
    ngOnInit() { }
    @ViewChild('WithOrWithoutCode') WithOrWithoutCode!: codesetSelectorComponent;
    @ViewChild('DropDownCodeDestination') DropDownCodeDestination!: codesetSelectorComponent;
    public CurrentStep: number = 1;
    public DropdownWithWithoutSelectedCode: singleNode | null = null;
    public AllocationsDestination: singleNode | null = null;
    public selectedCodeSetDropDown: ReviewSet = new ReviewSet();
    public WorkToDoSelectedCodeSet: ReviewSet = new ReviewSet();
    public isCollapsedAllocateOptions: boolean = false;
    public ShowChangeDataEntry: boolean = false;
    public DestinationDataEntryMode: string = "";
    public ChangeDataEntryModeMessage: string = ""; 
    private _CanChangeDataEntryMode: boolean = false;
    private _ItemsWithIncompleteCoding: number = -1;
    //public isCollapsedAllocateOptions: boolean = false;
    private _contacts: SelectableContact[] = []; 
    public DistributeWorkEvenly: boolean = true;
    public WorkAllocWizardResult: WorkAllocWizardResult | null = null;

    public get Contacts(): SelectableContact[] {
        if (this._contacts.length == 0)
            for (let c of this.ReviewInfoService.Contacts) {
                let ec: SelectableContact = new SelectableContact();
                ec.contactId = c.contactId;
                ec.contactName = c.contactName;
                this._contacts.push(ec);
            }
        return this._contacts;
    }
    public get AllContactsSelected(): boolean {
        for (const c of this.Contacts) {
            if (!c.IsSelected) return false;
        }
        return true;
    }
    public get EnoughContactsSelected(): boolean {
        let count: number = 0
        for (const c of this.Contacts) {
            if (c.IsSelected) count++;
        }
        if (count >= this.workAllocationFromWizardCommand.peoplePerItem) return true;
        else return false;
    }
    public get SelectedContacts(): SelectableContact[] {
        return this._contacts.filter(found => found.IsSelected);
    }
    public SelectAllMembers(e: any) {
        if (e.target.checked) {
            for (const c of this.Contacts) {
                c.IsSelected = true;
            }
        } else {
            for (const c of this.Contacts) {
                c.IsSelected = false;
            }
        }
    }
    public workAllocationFromWizardCommand: WorkAllocationFromWizardCommand = new WorkAllocationFromWizardCommand();
    public NextStep() {
        if (this.CurrentStep < this.StepNames.length) this.CurrentStep++;
    }
    public CanGoToNextStep(): boolean {
        if (this.CurrentStep < this.StepNames.length) return true;
        else return false;
    }
    public CanGoToStep2(): boolean {
        //console.log("CanGoToStep2()", this.CurrentStep, this.workAllocationFromWizardCommand.numberOfItemsToAssign);
        if (this.CurrentStep != 1) return false;
        if (this.workAllocationFromWizardCommand.numberOfItemsToAssign < 1) return false;
        return (this.CanGoToNextStep());
    }
    public CanGoToStep3(): boolean {
        //console.log("CanGoToStep2()", this.CurrentStep, this.workAllocationFromWizardCommand.numberOfItemsToAssign);
        if (this.CanSelectAllocationDestination == false) return false;
        if (this.workAllocationFromWizardCommand.numberOfItemsToAssign < 1) return false;
        if (this.AllocationsDestination == null || this.AllocationsDestination.set_id < 1) return false;
        return (this.CanGoToNextStep());
    }
    public get CanSelectAllocationDestination(): boolean {
        if (this.WorkToDoSelectedCodeSet.set_id < 1) return false;
        if (!this.WorkToDoSelectedCodeSet.codingIsFinal && this.workAllocationFromWizardCommand.peoplePerItem == 1) return false;
        if (this.WorkToDoSelectedCodeSet.codingIsFinal && this.workAllocationFromWizardCommand.peoplePerItem > 1) return false;
        return true;
    }
    public PreviousStep() {
        if (this.CurrentStep > 1) this.CurrentStep--;
    }
    private _stepNames: string[] = ["", "Identify items to distribute", "Indentify the work to assign", "Distribute the work"];
    public get StepNames(): string[] {
        return this._stepNames;
    } 
    public get CurrentStepName(): string {
        if (this.CurrentStep <= this.StepNames.length) return this.StepNames[this.CurrentStep];
        else return '';
    }
    public get CodeSets(): ReviewSet[] {
        return this.ReviewSetsService.ReviewSets;
    }
    public get CodeSets4WorkToDo(): ReviewSet[] {
        //we can't have "circular relations - i.e. select items with some criteria that is affected by the allocations!
        return this.ReviewSetsService.ReviewSets.filter(found =>
            found.set_id != this.workAllocationFromWizardCommand.setIdFilter 
            && (!this.DropdownWithWithoutSelectedCode || found.set_id != this.DropdownWithWithoutSelectedCode.set_id)
            );
    }
    private _allocateOptions: kvSelectFrom[] = [{ key: 0, value: '[Please select]' },
        { key: 1, value: 'No code / coding tool filter' },
        { key: 2, value: 'All without any codes from this coding tool' },
        { key: 3, value: 'All with any codes from this coding tool' },
        { key: 4, value: 'All with this code' },
        { key: 5, value: 'All without this code' }];
    public get AllocateOptions(): kvSelectFrom[] {
        return this._allocateOptions;
    }
    public get SelectionCriteriaIsValid(): boolean {
        const crit = this.workAllocationFromWizardCommand;
        if (crit.filterType == '') return false;
        if (crit.filterType == 'No code / coding tool filter') return true;
        if (crit.filterType == 'All without any codes from this coding tool' || crit.filterType == 'All with any codes from this coding tool' ) return (crit.setIdFilter > 0);
        if (crit.filterType == 'All with this code' || crit.filterType == 'All without this code') return (crit.attributeIdFilter > 0);
        return false;
    }
    public get IncludedExcludedText(): string {
        //console.log("IncludedExcludedText", this.workAllocationFromWizardCommand.included);
        if (this.workAllocationFromWizardCommand.included == true) return "included";
        else return "excluded";
    }
    public get ItemsInTheSelectedPot(): string {
        if (this.workAllocationFromWizardCommand.numberOfItemsToAssign < 0) return 'Undefined';
        else return this.workAllocationFromWizardCommand.numberOfItemsToAssign.toString();
    }
    public get CanEditSelectedSet(): boolean {
        return (this.CanWrite && this.WorkToDoSelectedCodeSet.allowEditingCodeset);
    }
    public get WorkToDoSelectedCodeSetDataEntryM(): string {
        if (this.WorkToDoSelectedCodeSet.codingIsFinal) return "Comparison";
        else return "Normal";
    }
    public get CanChangeDataEntryMode(): boolean {
        //console.log("CanChangeDataEntryMode", this._CanChangeDataEntryMode, this.CanWrite(), this._ItemsWithIncompleteCoding)
        return (this._CanChangeDataEntryMode && this.CanWrite() && this._ItemsWithIncompleteCoding != -1);
    }
    IsServiceBusy(): boolean {
        if (this.ReviewSetsService.IsBusy || this.ReviewSetsEditingService.IsBusy || this.ReviewInfoService.IsBusy || this.WorkAllocationListService.IsBusy) return true;
        else return false;
    }
    CanWrite(): boolean {
        //console.log('CanWrite', this.ReviewerIdentityServ.HasWriteRights, this.IsServiceBusy());
        if (this.ReviewerIdentityService.HasWriteRights && !this.IsServiceBusy()) {
            //console.log('CanWrite', true);
            return true;
        }
        else {
            //console.log('CanWrite', false);
            return false;
        }
    }

    SetRelevantDropDownValues(selection: number) {
        //console.log("SetRelevantDropDownValues", JSON.stringify(selection));
        this.ClearPot();
        let ind = this.AllocateOptions.findIndex(found => found.key == selection);
        if (ind > 0) this.workAllocationFromWizardCommand.filterType = this.AllocateOptions[ind].value;
        else this.workAllocationFromWizardCommand.filterType = "";
    }
    CloseCodeDropDownCodeWithWithout() {
        if (this.WithOrWithoutCode) {
            this.DropdownWithWithoutSelectedCode = this.WithOrWithoutCode.SelectedNodeData;
            this.workAllocationFromWizardCommand.attributeIdFilter = (this.DropdownWithWithoutSelectedCode as SetAttribute).attribute_id;
        }
        this.ClearPot();
        this.workAllocationFromWizardCommand.setIdFilter = 0;
        this.selectedCodeSetDropDown = new ReviewSet();
        this.isCollapsedAllocateOptions = false;
    }
    CloseCodeDropDownCodeDestination() {
        if (this.DropDownCodeDestination) {
            this.AllocationsDestination = this.DropDownCodeDestination.SelectedNodeData;
            if (this.AllocationsDestination) {
                if (this.AllocationsDestination.nodeType == "SetAttribute") {
                    this.workAllocationFromWizardCommand.destination_Attribute_ID = (this.AllocationsDestination as SetAttribute).attribute_id;
                    this.workAllocationFromWizardCommand.destination_Set_ID = (this.AllocationsDestination as SetAttribute).set_id;
                } else {
                    this.workAllocationFromWizardCommand.destination_Attribute_ID = 0;
                    this.workAllocationFromWizardCommand.destination_Set_ID = (this.AllocationsDestination as ReviewSet).set_id;
                }
            }
        }
        //this.ClearPot();
        //this.workAllocationFromWizardCommand.setIdFilter = 0;
        //this.selectedCodeSetDropDown = new ReviewSet();
        this.isCollapsedAllocateOptions = false;
    }
    setCodeSetDropDown(codeset: ReviewSet) {
        this.ClearPot();
        this.selectedCodeSetDropDown = codeset;
        this.workAllocationFromWizardCommand.setIdFilter = this.selectedCodeSetDropDown.set_id;
        this.workAllocationFromWizardCommand.attributeIdFilter = 0;
    }
    async GetPreview(previewLevel: number) {
        this.WorkAllocWizardResult = null;
        if (previewLevel != 1 && previewLevel != 2) previewLevel = 1;
        this.workAllocationFromWizardCommand.isPreview = previewLevel;
        if (previewLevel == 1) this.WorkAllocationListService.RunAllocationFromWizardCommand(this.workAllocationFromWizardCommand);
        else if (previewLevel == 2) {
            //need to do work to create the correct data for the SQL side...
            this.workAllocationFromWizardCommand.groupsPrefix = "Coding on '" + this.WorkToDoSelectedCodeSet.name + "'";
            this.workAllocationFromWizardCommand.reviewerNames = "";
            this.workAllocationFromWizardCommand.reviewersIds = "";
            this.workAllocationFromWizardCommand.itemsPerEachReviewer = "";
            const rex = /,/g;
            if (this.DistributeWorkEvenly) {
                //relatively easy, equal pots for each person, then distribute the reminder.
                const NofPeople = this.SelectedContacts.length;
                let Remainder: number = (this.workAllocationFromWizardCommand.numberOfItemsToAssign * this.workAllocationFromWizardCommand.peoplePerItem) % NofPeople;
                let itemsPerPerson: number = ((this.workAllocationFromWizardCommand.numberOfItemsToAssign * this.workAllocationFromWizardCommand.peoplePerItem) - Remainder) / NofPeople;
                for (let i: number = 0; i < NofPeople; i++) {
                    this.workAllocationFromWizardCommand.reviewerNames += this.SelectedContacts[i].contactName.replace(rex, ' ') + ",";
                    this.workAllocationFromWizardCommand.reviewersIds += this.SelectedContacts[i].contactId.toString() + ',';
                    if (Remainder > 0) {
                        Remainder--;
                        this.workAllocationFromWizardCommand.itemsPerEachReviewer += (itemsPerPerson + 1).toString() + ",";
                    } else this.workAllocationFromWizardCommand.itemsPerEachReviewer += itemsPerPerson.toString() + ",";

                }
                this.workAllocationFromWizardCommand.reviewerNames =
                    this.workAllocationFromWizardCommand.reviewerNames.substring(0, this.workAllocationFromWizardCommand.reviewerNames.length - 1);
                this.workAllocationFromWizardCommand.reviewersIds =
                    this.workAllocationFromWizardCommand.reviewersIds.substring(0, this.workAllocationFromWizardCommand.reviewersIds.length - 1);
                this.workAllocationFromWizardCommand.itemsPerEachReviewer =
                    this.workAllocationFromWizardCommand.itemsPerEachReviewer.substring(0, this.workAllocationFromWizardCommand.itemsPerEachReviewer.length - 1);
            }
            let result: WorkAllocWizardResult | undefined = await this.WorkAllocationListService.RunAllocationFromWizardCommand(this.workAllocationFromWizardCommand);
            console.log("WorkAllocWizardResult", result);
            this.WorkAllocWizardResult = result;
        }
        else { }
    }
    SetWorkToDoCodeSet(codeset: ReviewSet) {
        this.WorkToDoSelectedCodeSet = codeset;
        codeset.codingIsFinal
        this.workAllocationFromWizardCommand.work_to_do_setID = codeset.set_id;
    }
    SelectMember(member: SelectableContact) {
        member.IsSelected = !member.IsSelected;
    }
    PeoplePerItemChanged() {
        this.ShowChangeDataEntry = false;
    }
    //ShowChangeDataEntryClicked() {
    //    this.ShowChangeDataEntry = !this.ShowChangeDataEntry;
    //}
    async ShowChangeDataEntryClicked() {
        this._ItemsWithIncompleteCoding = -1;
        this.DestinationDataEntryMode = "";
        this.ChangeDataEntryModeMessage = "";
        if (this.WorkToDoSelectedCodeSet.set_id > 0) {
            this.ShowChangeDataEntry = true;
            const res: ChangeDataEntryMessage = await this.ReviewSetsEditingService.GetChangeDataEntryMessage(this.WorkToDoSelectedCodeSet, this.ReviewInfoService.ReviewInfo.screeningCodeSetId);
            this.DestinationDataEntryMode = res.DestinationDataEntryMode;
            this.ChangeDataEntryModeMessage = res.ChangeDataEntryModeMessage;
            this._CanChangeDataEntryMode = res.CanChangeDataEntryMode;
            this._ItemsWithIncompleteCoding = res.ItemsWithIncompleteCoding;
            //console.log("ShowChangeDataEntryClicked", res);
        }
        //this.ShowChangeDataEntry = true;
    }
    DoChangeDataEntry() {
        if (this.WorkToDoSelectedCodeSet.set_id < 1 || !this._CanChangeDataEntryMode) return;
        this.WorkToDoSelectedCodeSet.codingIsFinal = !this.WorkToDoSelectedCodeSet.codingIsFinal;
        //console.log("Will update DataEntry (work alloc wizard):", this.WorkToDoSelectedCodeSet );
        this.ReviewSetsEditingService.SaveReviewSet(this.WorkToDoSelectedCodeSet);
        this.HideChangeDataEntry();
    }
    HideChangeDataEntry() {
        this.ShowChangeDataEntry = false;
        this.ChangeDataEntryModeMessage = "";
        this._CanChangeDataEntryMode = false;
    }
    ResetStep34() {

    }
    ClearPot() {
        this.workAllocationFromWizardCommand.numberOfItemsToAssign = -1
    }
    
    //public get CurrentCodeCanHaveChildren(): boolean {
    //    //safety first, if anything didn't work as expexcted return false;
    //    if (!this.CanWrite()) return false;
    //    else {
    //        return this.ReviewSetsService.CurrentCodeCanHaveChildren;
    //        //end of bit that goes into "ReviewSetsService.CanNodeHaveChildren(node: singleNode): boolean"
    //    }
    //}
    
    //public get CurrentNode(): singleNode | null {
    //    if (!this.ReviewSetsService.selectedNode) return null;
    //    else return this.ReviewSetsService.selectedNode;
    //}
   
    CancelActivity() {
    }
    
    ngOnDestroy() {
    }
}
class SelectableContact extends Contact {
    IsSelected: boolean = false;
    //contactName: string = '';
    //contactId: number = 0;
}

