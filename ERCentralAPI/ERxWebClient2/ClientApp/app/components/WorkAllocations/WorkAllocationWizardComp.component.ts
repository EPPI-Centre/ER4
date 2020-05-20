import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter, Attribute } from '@angular/core';
import { ReviewSetsService, kvAllowedAttributeType, SetAttribute, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocationListService, WorkAllocationFromWizardCommand } from '../services/WorkAllocationList.service';
import { kvSelectFrom } from './WorkAllocationComp.component';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';


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
    public CurrentStep: number = 1;
    public DropdownWithWithoutSelectedCode: singleNode | null = null;
    public selectedCodeSetDropDown: ReviewSet = new ReviewSet();
    public isCollapsedAllocateOptions: boolean = false;

    public workAllocationFromWizardCommand: WorkAllocationFromWizardCommand = new WorkAllocationFromWizardCommand();
    public NextStep() {
        if (this.CurrentStep < this.StepNames.length) this.CurrentStep++;
    }
    public CanGoToNextStep(): boolean {
        if (this.CurrentStep < this.StepNames.length) return true;
        else return false;
    }
    public CanGoToStep2(): boolean {
        console.log("CanGoToStep2()", this.CurrentStep, this.workAllocationFromWizardCommand.numberOfItemsToAssign);
        if (this.CurrentStep != 1) return false;
        if (this.workAllocationFromWizardCommand.numberOfItemsToAssign < 1) return false;
        return (this.CanGoToNextStep());
    }
    public PreviousStep() {
        if (this.CurrentStep > 1) this.CurrentStep--;
    }
    private _stepNames: string[] = ["", "Identify items to distribute", "Do more", "Do EVEN more"];
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
    setCodeSetDropDown(codeset: ReviewSet) {
        this.ClearPot();
        this.selectedCodeSetDropDown = codeset;
        this.workAllocationFromWizardCommand.setIdFilter = this.selectedCodeSetDropDown.set_id;
        this.workAllocationFromWizardCommand.attributeIdFilter = 0;
    }
    GetPreview(previewLevel: number) {
        if (previewLevel != 1 && previewLevel != 2) previewLevel = 1;
        this.workAllocationFromWizardCommand.isPreview = previewLevel;
        this.WorkAllocationListService.RunAllocationFromWizardCommand(this.workAllocationFromWizardCommand);
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

