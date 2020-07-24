import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter, Attribute, ElementRef, NgZone, AfterViewInit } from '@angular/core';
import { ReviewSetsService, kvAllowedAttributeType, SetAttribute, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService, ChangeDataEntryMessage } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact, ReviewInfo } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocationListService, WorkAllocationFromWizardCommand, WorkAllocWizardResult } from '../services/WorkAllocationList.service';
import { kvSelectFrom } from './WorkAllocationComp.component';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { NumericTextBoxComponent } from '@progress/kendo-angular-inputs';
import { ModalService } from '../services/modal.service';
import { PriorityScreeningService, Training, iTrainingScreeningCriteria } from '../services/PriorityScreening.service';
import { Helpers } from '../helpers/HelperMethods';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { ItemListService } from '../services/ItemList.service';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';


@Component({
    selector: 'ScreeningSetupComp',
    templateUrl: './ScreeningSetup.component.html',
    styles: ['.DisabledStep { font-weight: light; opacity: 50%;}'],
    providers: []
})

export class ScreeningSetupComp implements OnInit, OnDestroy, AfterViewInit {
    constructor(
        private router: Router,
        private ReviewSetsService: ReviewSetsService,
        private modalService: ModalService,
        private ReviewSetsEditingService: ReviewSetsEditingService,
        private ReviewInfoService: ReviewInfoService,
        private ReviewerIdentityService: ReviewerIdentityService,
        private ConfirmationDialogService: ConfirmationDialogService,
        private PriorityScreeningService: PriorityScreeningService,
        private WorkAllocationListService: WorkAllocationListService,
        private ItemListService: ItemListService
	) { }
    ngOnInit() {
        this.PriorityScreeningService.Fetch();
        this.RevInfoSub = this.ReviewInfoService.ReviewInfoChanged.subscribe(() => this.RefreshRevinfo());
    }
    ngAfterViewInit() {
        if (this.ReviewInfoService.ReviewInfo.showScreening == false) this.Cancel();
        else {
            console.log("will clone revinfo:", this.ReviewInfoService.ReviewInfo);
            this.DoRefreshRevinfo();
            this.PriorityScreeningService.GetTrainingScreeningCriteriaList();
        }
    }
    public revInfo: ReviewInfo = new ReviewInfo();
    @Output() emitterCancel = new EventEmitter();
    public CurrentStep: number = 0;
    private _stepNames: string[] = ["Start", "select the references to code.", "choose the coding to be done.", "assign coding work to people.", "Show all settings"];
    @ViewChild('faketablerow') faketablerow!: ElementRef;
    @ViewChild('WithOrWithoutCode') WithOrWithoutCode!: codesetSelectorComponent;
    @ViewChild('AddTrainingCriteriaDDown') AddTrainingCriteriaDDown!: codesetSelectorComponent;

    public DropdownWithWithoutSelectedCode: singleNode | null = null;
    public DropdownAddTrainingCriteriaSelectedCode: singleNode | null = null;
    private subGotPriorityScreeningData: Subscription | null = null;
    private RevInfoSub: Subscription | null = null;
    public AllowEditOnStep4: boolean = false;
    public ScreenAllItems: boolean = true;
    private _ItemsWithThisAttribute: SetAttribute | null = null;
    public selectedCodeSetDropDown: ReviewSet | null = null;
    public isCollapsedAllocateOptions: boolean = false;
    public isCollapsedDropdownAddTrainingCriteria: boolean = false;
    public ShowAddTrainingCriteria: boolean = false;
    public ShowTrainingTable: boolean = false;
    private _CanChangeDataEntryMode: boolean = false;
    public ShowChangeDataEntry: boolean = false;
    public DestinationDataEntryMode: string = "";
    private _ItemsWithIncompleteCoding: number = -1;
    public ChangeDataEntryModeMessage: string = ""; 
    private _ScreeningModeOptions: kvSelectFrom[] = [
        { key: 0, value: '[Please select]' },
        { key: 1, value: 'Priority' },
        { key: 2, value: 'Random' }
    ];
    public get ScreeningModeOptions(): kvSelectFrom[] {
        return this._ScreeningModeOptions;
    }
    private _ReconcileOptions: kvStringSelectFrom[] = [
        { key: "", value: '[Please select]' },
        { key: "no compl", value: 'Multiple (no auto-completion)' },
        { key: "auto code", value: 'Multiple: auto complete (code level)' },
        { key: "auto excl", value: 'Multiple: auto complete (include / exclude level)' },
        { key: "auto safet", value: 'Multiple: safety first' }
    ]; //{ key: "Single", value: 'Single (auto-completes)' }, //not used, as we set it automatically.
    public get ReconcileOptions(): kvStringSelectFrom[] {
        return this._ReconcileOptions;
    }
    
    public get StepNames(): string[] {
        return this._stepNames;
    }
    public get CurrentStepName(): string {
        if (this.CurrentStep <= this.StepNames.length) return this.StepNames[this.CurrentStep];
        else return '';
    }
    public get ShowTrainingTableText(): string {
        if (this.ShowTrainingTable) return "Hide Progress Table";
        else return "Show Progress Table";
    }
    
    public get ItemsWithThisAttribute(): SetAttribute | null {
        if (this.revInfo.screeningWhatAttributeId > 0
            && (this._ItemsWithThisAttribute == null || this._ItemsWithThisAttribute.attribute_id != this.revInfo.screeningWhatAttributeId)) {
            this._ItemsWithThisAttribute = this.ReviewSetsService.FindAttributeById(this.revInfo.screeningWhatAttributeId)
        } else {
            this._ItemsWithThisAttribute = null;
        }
        return this._ItemsWithThisAttribute;
    }

    public get Contacts(): Contact[] {
        return this.ReviewInfoService.Contacts;
    }
    public PreviousStep() {
        if (this.CurrentStep > 1) this.CurrentStep--;
    }
    
    public get CodeSets(): ReviewSet[] {
        return this.ReviewSetsService.ReviewSets;
    }
    public NextStep() {
        if (this.CurrentStep < this.StepNames.length) this.CurrentStep++;
    }
    public CanGoToNextStep(): boolean {
        if (this.CurrentStep < this.StepNames.length) return true;
        else return false;
    }
    public CanGoToStep2(): boolean {
        //console.log("CanGoToStep2()", this.CurrentStep, this.workAllocationFromWizardCommand.numberOfItemsToAssign);
        return (this.CanGoToNextStep());
    }
    public CanGoToStep3(): boolean {
        //console.log("CanGoToStep2()", this.CurrentStep, this.workAllocationFromWizardCommand.numberOfItemsToAssign);
        return (this.CanGoToNextStep());
    }
    public GoToAllinOneStep() {
        this.CurrentStep = 4;
    }
    IsServiceBusy(): boolean {
        //console.log("IsWizardService busy?", this.ReviewSetsService.IsBusy, this.ReviewSetsEditingService.IsBusy, this.ReviewInfoService.IsBusy, this.WorkAllocationListService.IsBusy)
        if (this.ReviewSetsService.IsBusy
            || this.PriorityScreeningService.IsBusy
            || this.ReviewSetsEditingService.IsBusy
            || this.ReviewInfoService.IsBusy
            || this.WorkAllocationListService.IsBusy
        ) return true;
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
    public get CanEditSelectedSet(): boolean {
        return (this.CanWrite && this.selectedCodeSetDropDown !=null && this.selectedCodeSetDropDown.allowEditingCodeset);
    }
    public get WorkToDoSelectedCodeSetDataEntryM(): string {
        if (this.selectedCodeSetDropDown != null && this.selectedCodeSetDropDown.codingIsFinal) return "Comparison";
        else if (this.selectedCodeSetDropDown != null) return "Normal";
        else return "Not Available";
    }

    public get TrainingList(): Training[] {
        return this.PriorityScreeningService.TrainingList;
    }
    public get TrainingListData(): GridDataResult {
        return {
            data: this.PriorityScreeningService.TrainingList,
            total: this.PriorityScreeningService.TrainingList.length,
        };
    }
    public get TrainingScreeningCriteriaList(): iTrainingScreeningCriteria[] {
        return this.PriorityScreeningService.TrainingScreeningCriteria;
    }
    public get TrainingCriteriaAddingCodeType(): string {
        if (this.DropdownAddTrainingCriteriaSelectedCode == null) return "Not Set";
        const code = (this.DropdownAddTrainingCriteriaSelectedCode) as SetAttribute;
        if (code && code.attribute_type_id) {
            if (code.attribute_type_id == 10) return "Include";
            else if (code.attribute_type_id == 11) return "Exclude";
            else return "Invalid";
        }
        return "Not Set";
    }
    FormatDate(DateSt: string): string {
        return Helpers.FormatDate2(DateSt);
    }
    ShowAddTrainingCriteriaClick() {
        this.ShowAddTrainingCriteria = !this.ShowAddTrainingCriteria;
    }

    //GetAttributeName(AttId: number): string {
    //    const Att = this.ReviewSetsService.FindAttributeById(AttId);
    //    if (Att != null) return Att.attribute_name;
    //    else return "Not configured...";
    //}
    public get CanChangeDataEntryMode(): boolean {
        //console.log("CanChangeDataEntryMode", this._CanChangeDataEntryMode, this.CanWrite(), this._ItemsWithIncompleteCoding)
        return (this._CanChangeDataEntryMode && this.CanWrite() && this._ItemsWithIncompleteCoding != -1);
    }
    public get ScreeningTools(): ReviewSet[] {
        return this.ReviewSetsService.ReviewSets.filter(found => found.setType.setTypeName == "Screening")
    }
    public get CanSaveConfiguration(): boolean {
        if (!this.CanWrite()) return false;
        if (!this.ConfigurationIsValid) return false;
        if (JSON.stringify(this.revInfo) === JSON.stringify(this.ReviewInfoService.ReviewInfo)) return false;
        return true;
    }
    public get ConfigurationIsValid(): boolean {
        if (this.revInfo.screeningCodeSetId < 1) return false;//don't know what to screen
        if (this.revInfo.screeningWhatAttributeId < 0) return false; //0 if screen all items more than 0 if screen items with this code
        if (!this.ScreenAllItems && this.revInfo.screeningWhatAttributeId < 1) return false;//screen items with this code: need a valid AttributeId
        if (this.ScreenAllItems && this.revInfo.screeningWhatAttributeId != 0) return false;//screen all items: need this to be 0
        if (this.PriorityScreeningService.TrainingScreeningCriteria.length == 0) return false;//no codes to learn from
        if (this.ScreeningModeOptions.findIndex(found => found.value == this.revInfo.screeningMode) < 1) {
            //console.log("type of list isn't set?", this.revInfo.screeningMode);
            return false;//type of list isn't set.
        }
        //check data entry mode...
        const set = this.selectedCodeSetDropDown;
        if (set == null) return false;//uh? Chosen set isn't in review!
        if (this.revInfo.screeningReconcilliation == "") return false;//reconciliation is NOT set
        if (this.revInfo.screeningNPeople > 1) {//multiple people per item:
            if (set.codingIsFinal) return false;//but codeset is in "normal" data entry.
            else if (this.revInfo.screeningReconcilliation == "Single") return false;//reconciliation set to auto complete
        } else {//one person per item...
            if (!set.codingIsFinal) return false;//but codeset is in "comparison" data entry.
            else if (this.revInfo.screeningReconcilliation !== "Single") return false;//reconciliation NOT set to auto complete
        }
        return true;//no concern found
    }
    public get ReconciliationModeError(): string {
        if (this.selectedCodeSetDropDown == null) return "";//we have bigger problems!
        else if (this.revInfo.screeningReconcilliation == "") return "Please select a reconciliation mode.";
        else if (this.selectedCodeSetDropDown.codingIsFinal && this.revInfo.screeningReconcilliation !== "Single") {
            //we'll fix this on the fly...
            this.revInfo.screeningReconcilliation = "Single";
            return "";
        }
        else if (!this.selectedCodeSetDropDown.codingIsFinal && this.revInfo.screeningReconcilliation == "Single") {
            return "Please select a valid reconciliation mode."; 
        }
        return "";    
    }

    public get ReconciliationModeSummary(): string {
        //console.log("ReconciliationModeSummary...", this.revInfo.screeningReconcilliation);
        const warn = " Note that if many people participate in screening, you might need to create many different reconciliations, to capture all possible \"pairs\".";
        const warn2 = "<br /><strong class='alert-warning'>Warning:</strong> Auto-Completions will not appear in comparisons, which <strong>makes it hard</strong> to measure the level of agreement.<br />";
        let result: string = "";
        if (this.revInfo.screeningReconcilliation == "Single") {
            result = "";
        }
        else if (this.revInfo.screeningReconcilliation == "no compl") {
            result = "No \"auto reconciliations\", you will need to \"Complete\" agreements yourself." + warn;
        }
        else if (this.revInfo.screeningReconcilliation == "auto code") {
            result = "Agreements at the level of (each) single code will be automatically completed." + warn2 + warn;
        }
        else if (this.revInfo.screeningReconcilliation == "auto excl") {
            result = "Agreements at the level of the inclusion/exclusion decision will be automatically completed." + warn2 + warn;
        }
        else if (this.revInfo.screeningReconcilliation == "auto safet") {
            result = "To guarantee no possible \"Include\" will be missed, all \"Include\" decisions will be automatically completed." + warn2 + warn;
        }
        return result;
    }
    
    ChangeWhatToScreen() {
        //do something?
    }
    CloseCodeDropDownCodeWithWithout() {
        if (this.WithOrWithoutCode) {
            this.DropdownWithWithoutSelectedCode = this.WithOrWithoutCode.SelectedNodeData;
            this.revInfo.screeningWhatAttributeId = (this.DropdownWithWithoutSelectedCode as SetAttribute).attribute_id;
        }
        //this.ClearPot();
        //this.workAllocationFromWizardCommand.setIdFilter = 0;
        //this.selectedCodeSetDropDown = new ReviewSet();
        this.isCollapsedAllocateOptions = false;
    }
    CloseCodeDropDownAddTrainingCriteria() {
        if (this.AddTrainingCriteriaDDown) {
            this.DropdownAddTrainingCriteriaSelectedCode = this.AddTrainingCriteriaDDown.SelectedNodeData;
        }
        this.isCollapsedDropdownAddTrainingCriteria = false;
    }
    AddTrainingCriteriaDoItClick() {
        const SA = this.DropdownAddTrainingCriteriaSelectedCode as SetAttribute;
        if (SA && SA.attribute_id > 0) {
            this.PriorityScreeningService.AddTrainingScreeningCriteria(SA);
            this.ShowAddTrainingCriteriaClick();
        }
    }
    setCodeSetDropDown(codeset: ReviewSet) {
        this.selectedCodeSetDropDown = codeset;
        if (this.revInfo.screeningCodeSetId !== codeset.set_id) {
            this.revInfo.screeningCodeSetId = codeset.set_id;
            this.ConfirmationDialogService.confirm("Update training codes?",
                "You have <em>changed</em> the screening tool.<br /> Would you like to automatically update the list of training codes?<br />Training codes are <strong>important</strong>! They define what the machine will learn from, so <strong>please check</strong> that they are correct, in any case."
                , false, "", "Yes please (I'll check)", "No, I'll do it myself"
                , "lg")
                .then((confirmed: any) => {
                if (confirmed) {
                    this.DoResetTrainingCodes();
                }
            }).catch(() => { });
        }
    }
    DoResetTrainingCodes() {
        if (this.selectedCodeSetDropDown) this.PriorityScreeningService.ReplaceTrainingScreeningCriteriaList(this.selectedCodeSetDropDown);
    }
    DeleteTrainingScreeningCriteria(crit: iTrainingScreeningCriteria) {
        this.PriorityScreeningService.DeleteTrainingScreeningCriteria(crit);
    }
    FlipTrainingScreeningCriteria(crit: iTrainingScreeningCriteria) {
        this.PriorityScreeningService.FlipTrainingScreeningCriteria(crit);
    }
    SetScreeningMode(selection: string) {
        let ind = this.ScreeningModeOptions.findIndex(found => found.value == selection);
        if (ind > 0) {
            this.revInfo.screeningMode = selection;
        } else {
            this.revInfo.screeningMode = "";
        }
        //console.log("SetRelevantDropDownValues", selection);
    }
    RefreshRevinfo() {
        //if we're editing, we should ask for "permission"...
        if (this.AllowEditOnStep4) {
            this.ConfirmationDialogService.confirm("Reset all form values?",
                "Sorry to interrupt! This app has just received an updated version of the screening settings.<br />"
                + "<strong>Do you want to reset the current form and load the current latest configuration instead?</strong><br />"
                + "This will ensure you'll be making changes to the most current version.<br />"
                + "(Training codes are <strong>not affected</strong>.)"
                , false, "", "Yes (default)", "No"
                , "sm")
                .then((confirmed: any) => {
                    if (confirmed) {
                        this.ResetAllEditFromValues();
                        this.DoRefreshRevinfo();
                    }
                }).catch(() => {
                    this.ResetAllEditFromValues();
                    this.DoRefreshRevinfo();
                });
        }
        else this.DoRefreshRevinfo();
    }
    DoRefreshRevinfo() {
        this.revInfo = this.ReviewInfoService.ReviewInfo.Clone();
        this.GetScreeningTool(this.ReviewInfoService.ReviewInfo.screeningCodeSetId);
    }
    GetScreeningTool(setId: number) {
        const set = this.ReviewSetsService.FindSetById(setId);
        if (set != null) this.selectedCodeSetDropDown = set;
    }
    GetCodingToolName(): string {
        if (this.selectedCodeSetDropDown == null) return "Not Configured...";
        else return this.selectedCodeSetDropDown.set_name;
    }
    StartScreening() {
        //alert('Start Screening: not implemented');
        this.ItemListService.IsInScreeningMode = true;
        this.subGotPriorityScreeningData = this.PriorityScreeningService.gotList.subscribe(this.ContinueStartScreening());
        this.PriorityScreeningService.Fetch();

    }
    ContinueStartScreening() {
        if (this.subGotPriorityScreeningData) this.subGotPriorityScreeningData.unsubscribe();
        this.router.navigate(['itemcoding', 'PriorityScreening']);
    }
    GenerateList() {
        this.PriorityScreeningService.RunNewTrainingCommand(false);
    }
    SaveOptions() {
        this.ReviewInfoService.Update(this.revInfo);
        this.AllowEditOnStep4 = false;
        this.CancelEditingAllOptions();
    }
    
    RefreshAll() {
        this.PriorityScreeningService.GetTrainingScreeningCriteriaList();
        this.ReviewInfoService.Fetch();
    }
    async CheckIfCancelEditAllOptions() {
        await Helpers.Sleep(80);
        console.log("CheckIfCancelEditAllOptions:", this.AllowEditOnStep4);
        if (this.AllowEditOnStep4 == false) this.CancelEditingAllOptions();
    }
    CancelEditingAllOptions() {
        this.AllowEditOnStep4 = false;
        this.ResetAllEditFromValues();
        this.DoRefreshRevinfo();
    }
    ResetAllEditFromValues() {
        this.ScreenAllItems = true;
        this._ItemsWithThisAttribute = null;
        this.selectedCodeSetDropDown = null;
        this.isCollapsedAllocateOptions = false;
        this.isCollapsedDropdownAddTrainingCriteria = false;
        this.DropdownAddTrainingCriteriaSelectedCode = null;
        this.DropdownWithWithoutSelectedCode = null;
    }


    CheckAndUpdatePeoplePerItem(): boolean {
        //returns true if OK, but will automatically set to 0 Npeople, if screening set is in normal mode...
        if (this.revInfo.screeningCodeSetId < 1 || this.selectedCodeSetDropDown == null) {
            return true;//nothing is set, so nothing to check
        }
        else if (this.selectedCodeSetDropDown.codingIsFinal) {
            if (this.revInfo.screeningReconcilliation != "Single") this.revInfo.screeningReconcilliation != "Single";
            if (this.revInfo.screeningNPeople >= 1) {
                //set is in normal data entry, so we'll automatically set screeningNPeople to 0
                this.revInfo.screeningNPeople = 0;
                return true;//true because we fixed it...
            }
            else {//finishing all other options, if set is in normal mode.
                return true;
            }
        }
        else {//set must be in comparison mode
            if (this.revInfo.screeningReconcilliation == "Single" || this.revInfo.screeningReconcilliation == "") {
                this.revInfo.screeningReconcilliation = "no compl";
            }
            if (this.revInfo.screeningNPeople > 1) return true;
            else {//multiple screening, but n of people 1 or less
                this.revInfo.screeningNPeople = 1;//just avoiding to have a 0 here...
                return false;//multiple screening, but n of people is 1...
            }
        }
    }
    SetReconciliationMode(mode: string) {
        //console.log("SetReconciliationMode", mode);
        this.revInfo.screeningReconcilliation = mode;
    }
    async ShowChangeDataEntryClicked() {
        this._ItemsWithIncompleteCoding = -1;
        this.DestinationDataEntryMode = "";
        this.ChangeDataEntryModeMessage = ""; 
        if (this.selectedCodeSetDropDown != null && this.selectedCodeSetDropDown.set_id > 0) {
            this.ShowChangeDataEntry = true;
            // the "-1" below is because in this case we specifically want to change the mode in the screening tool!
            const res: ChangeDataEntryMessage = await this.ReviewSetsEditingService.GetChangeDataEntryMessage(this.selectedCodeSetDropDown, -1);
            this.DestinationDataEntryMode = res.DestinationDataEntryMode;
            this.ChangeDataEntryModeMessage = res.ChangeDataEntryModeMessage;
            this._CanChangeDataEntryMode = res.CanChangeDataEntryMode;
            this._ItemsWithIncompleteCoding = res.ItemsWithIncompleteCoding;
            //console.log("ShowChangeDataEntryClicked", res);
        }
    }
    DoChangeDataEntry() {
        if (this.selectedCodeSetDropDown != null && this.selectedCodeSetDropDown.set_id < 1 || !this._CanChangeDataEntryMode) return;
        else if (this.selectedCodeSetDropDown != null) {
            this.selectedCodeSetDropDown.codingIsFinal = !this.selectedCodeSetDropDown.codingIsFinal;
            //console.log("Will update DataEntry (work alloc wizard):", this.WorkToDoSelectedCodeSet );
            this.ReviewSetsEditingService.SaveReviewSet(this.selectedCodeSetDropDown);
            this.HideChangeDataEntry();
        }
    }
    HideChangeDataEntry() {
        this.ShowChangeDataEntry = false;
        this.ChangeDataEntryModeMessage = "";
        this._CanChangeDataEntryMode = false;
    }



    Cancel() {
        console.log("cancel screening");
        this.emitterCancel.emit();
    }

    ngOnDestroy() {
        if (this.subGotPriorityScreeningData) this.subGotPriorityScreeningData.unsubscribe();
        if (this.RevInfoSub) this.RevInfoSub.unsubscribe();
    }


    //old methods, section below should be empty when it's all written!
    @ViewChild('DropDownCodeDestination') DropDownCodeDestination!: codesetSelectorComponent;
    @ViewChild('NofItemstoAssigntoPerson') NofItemstoAssigntoPerson!: NumericTextBoxComponent;


    public AllocationsDestination: singleNode | null = null;
    //public WorkToDoSelectedCodeSet: ReviewSet = new ReviewSet();
    //public isCollapsedAllocateOptions: boolean = false;
    //private _contacts: SelectableContact[] = []; 
    public DistributeWorkEvenly: boolean = true;
    public WorkAllocWizardResult: WorkAllocWizardResult | null = null;
    public ShowEditCodesPrefix: boolean = false;
    //private _manualAssignCol1: SelectableContact[] = [];
    //private _manualAssignCol2: SelectableContact[] = [];
    //private _manualAssignCol3: SelectableContact[] = [];
    //public ToAddManualMember: SelectableContact | null = null;

    //public get Contacts(): SelectableContact[] {
    //    if (this._contacts.length == 0)
    //        for (let c of this.ReviewInfoService.Contacts) {
    //            let ec: SelectableContact = new SelectableContact();
    //            ec.contactId = c.contactId;
    //            ec.contactName = c.contactName;
    //            this._contacts.push(ec);
    //        }
    //    return this._contacts;
    //}
    public get AllContactsSelected(): boolean {
        //for (const c of this.Contacts) {
        //    if (!c.IsSelected) return false;
        //}
        return true;
    }
    public get BarelyEnoughContactsSelected(): boolean {
        //let count: number = 0
        //for (const c of this.Contacts) {
        //    if (c.IsSelected) count++;
        //}
        //if (count >= this.workAllocationFromWizardCommand.peoplePerItem) return true;
        //else 
            return false;
    }
    public get CanDistributeUnevenly(): boolean {
        //let count: number = 0
        //for (const c of this.Contacts) {
        //    if (c.IsSelected) count++;
        //}
        //if (count > this.workAllocationFromWizardCommand.peoplePerItem) return true;
        //else
            return false;
    }
    public get EnoughContactsSelected(): boolean {
        if (this.DistributeWorkEvenly) return this.BarelyEnoughContactsSelected;
        else return this.CanDistributeUnevenly;
    }
    //public get SelectedContacts(): SelectableContact[] {
    //    return this._contacts.filter(found => found.IsSelected);
    //}
    public SelectAllMembers(e: any) {
        //this.ResetStep34();
        //if (e.target.checked) {
        //    for (const c of this.Contacts) {
        //        c.IsSelected = true;
        //    }
        //} else {
        //    for (const c of this.Contacts) {
        //        c.IsSelected = false;
        //    }
        //}
    }
    

    //public get CanSelectAllocationDestination(): boolean {
    //    if (this.WorkToDoSelectedCodeSet.set_id < 1) return false;
    //    //if (!this.WorkToDoSelectedCodeSet.codingIsFinal && this.workAllocationFromWizardCommand.peoplePerItem == 1) return false;
    //    //if (this.WorkToDoSelectedCodeSet.codingIsFinal && this.workAllocationFromWizardCommand.peoplePerItem > 1) return false;
    //    return true;
    //}
    


    public get SelectionCriteriaIsValid(): boolean {
        //const crit = this.workAllocationFromWizardCommand;
        //if (crit.filterType == '') return false;
        //if (crit.filterType == 'No code / coding tool filter') return true;
        //if (crit.filterType == 'All without any codes from this coding tool' || crit.filterType == 'All with any codes from this coding tool' ) return (crit.setIdFilter > 0);
        //if (crit.filterType == 'All with this code' || crit.filterType == 'All without this code') return (crit.attributeIdFilter > 0);
        return false;
    }
    //public get IncludedExcludedText(): string {
    //    //console.log("IncludedExcludedText", this.workAllocationFromWizardCommand.included);
    //    if (this.workAllocationFromWizardCommand.included == true) return "included";
    //    else return "excluded";
    //}
    //public get ItemsInTheSelectedPot(): string {
    //    if (this.workAllocationFromWizardCommand.numberOfItemsToAssign < 0) return 'Undefined';
    //    else return this.workAllocationFromWizardCommand.numberOfItemsToAssign.toString();
    //}

    
    //public get ManualAssignTable(): SelectableContact[][] {
    //    const res: SelectableContact[][] = [];
    //    //res.push(this._manualAssignCol1);
    //    //if (this.workAllocationFromWizardCommand.peoplePerItem > 1) {
    //    //    res.push(this._manualAssignCol2);
    //    //}
    //    //if (this.workAllocationFromWizardCommand.peoplePerItem > 2) {
    //    //    res.push(this._manualAssignCol3);
    //    //} 
    //    return res;
    //}
    //public IsLastInManualAssignTable(c: SelectableContact): boolean {//we can only remove the last element, from this table 
    //    if (this._manualAssignCol3.length > 0 && this.workAllocationFromWizardCommand.peoplePerItem == 3) {
    //        return (this._manualAssignCol3[this._manualAssignCol3.length -1] == c );
    //    }
    //    else if (this._manualAssignCol2.length > 0 && this.workAllocationFromWizardCommand.peoplePerItem > 1) {
    //        return (this._manualAssignCol2[this._manualAssignCol2.length - 1] == c);
    //    }
    //    else if (this._manualAssignCol1.length > 0) {
    //        return (this._manualAssignCol1[this._manualAssignCol1.length - 1] == c);
    //    }
    //    return true;
    //}
    public get AllManualAssignmentsDone(): boolean {
        //const target = this.workAllocationFromWizardCommand.numberOfItemsToAssign * this.workAllocationFromWizardCommand.peoplePerItem;
        //let assigned: number = 0;
        //this._manualAssignCol1.forEach(el => assigned += el.NumberOfItems);
        //this._manualAssignCol2.forEach(el => assigned += el.NumberOfItems);
        //this._manualAssignCol3.forEach(el => assigned += el.NumberOfItems);
        //return target <= assigned;
        return false;//!!
    }
    public get ToBeManuallyAssignedStill(): number {
        //const target = this.workAllocationFromWizardCommand.numberOfItemsToAssign * this.workAllocationFromWizardCommand.peoplePerItem;
        //let assigned: number = 0;
        //this._manualAssignCol1.forEach(el => assigned += el.NumberOfItems);
        //this._manualAssignCol2.forEach(el => assigned += el.NumberOfItems);
        //this._manualAssignCol3.forEach(el => assigned += el.NumberOfItems);
        //return target - assigned;
        return 0;//!!
    }

    
    CloseCodeDropDownCodeDestination() {
        //if (this.DropDownCodeDestination) {
        //    this.AllocationsDestination = this.DropDownCodeDestination.SelectedNodeData;
        //    if (this.AllocationsDestination) {
        //        if (this.AllocationsDestination.nodeType == "SetAttribute") {
        //            this.workAllocationFromWizardCommand.destination_Attribute_ID = (this.AllocationsDestination as SetAttribute).attribute_id;
        //            this.workAllocationFromWizardCommand.destination_Set_ID = (this.AllocationsDestination as SetAttribute).set_id;
        //        } else {
        //            this.workAllocationFromWizardCommand.destination_Attribute_ID = 0;
        //            this.workAllocationFromWizardCommand.destination_Set_ID = (this.AllocationsDestination as ReviewSet).set_id;
        //        }
        //    }
        //}
        //this.isCollapsedAllocateOptions = false;
    }
    
    async GetPreviewOrDoit(previewLevel: number) {
        //this.WorkAllocWizardResult = null;
        //if (previewLevel < 0 && previewLevel > 2) previewLevel = 1;
        //this.workAllocationFromWizardCommand.isPreview = previewLevel;
        //if (this.workAllocationFromWizardCommand.groupsPrefix == "")
        //    this.workAllocationFromWizardCommand.groupsPrefix = "Coding on '" + this.WorkToDoSelectedCodeSet.name + "'";
        //if (previewLevel == 1) {
        //    // value for this.workAllocationFromWizardCommand.numberOfItemsToAssign gets set within the service...
        //    this.WorkAllocationListService.RunAllocationFromWizardCommand(this.workAllocationFromWizardCommand);
        //}
        //else if (previewLevel == 2) {
        //    //need to do work to create the correct data for the SQL side...
        //    this.workAllocationFromWizardCommand.reviewerNames = "";
        //    this.workAllocationFromWizardCommand.reviewersIds = "";
        //    this.workAllocationFromWizardCommand.itemsPerEachReviewer = "";
        //    const rex = /,/g;
        //    if (this.DistributeWorkEvenly) {
        //        //relatively easy, equal pots for each person, then distribute the reminder.
        //        const NofPeople = this.SelectedContacts.length;
        //        let Remainder: number = (this.workAllocationFromWizardCommand.numberOfItemsToAssign * this.workAllocationFromWizardCommand.peoplePerItem) % NofPeople;
        //        let itemsPerPerson: number = ((this.workAllocationFromWizardCommand.numberOfItemsToAssign * this.workAllocationFromWizardCommand.peoplePerItem) - Remainder) / NofPeople;
        //        for (let i: number = 0; i < NofPeople; i++) {
        //            this.workAllocationFromWizardCommand.reviewerNames += this.SelectedContacts[i].contactName.replace(rex, ' ') + ",";
        //            this.workAllocationFromWizardCommand.reviewersIds += this.SelectedContacts[i].contactId.toString() + ',';
        //            if (Remainder > 0) {
        //                Remainder--;
        //                this.workAllocationFromWizardCommand.itemsPerEachReviewer += (itemsPerPerson + 1).toString() + ",";
        //            } else this.workAllocationFromWizardCommand.itemsPerEachReviewer += itemsPerPerson.toString() + ",";
        //        }                
        //    } else {
        //        const people = this.SelectedContacts.filter(found => found.NumberOfItems > 0);
        //        for (let person of people) {
        //            this.workAllocationFromWizardCommand.reviewerNames += person.contactName.replace(rex, ' ') + ",";
        //            this.workAllocationFromWizardCommand.reviewersIds += person.contactId.toString() + ",";
        //            this.workAllocationFromWizardCommand.itemsPerEachReviewer += person.NumberOfItems.toString() + ",";
        //        }
        //    }
        //    this.workAllocationFromWizardCommand.reviewerNames =
        //        this.workAllocationFromWizardCommand.reviewerNames.substring(0, this.workAllocationFromWizardCommand.reviewerNames.length - 1);
        //    this.workAllocationFromWizardCommand.reviewersIds =
        //        this.workAllocationFromWizardCommand.reviewersIds.substring(0, this.workAllocationFromWizardCommand.reviewersIds.length - 1);
        //    this.workAllocationFromWizardCommand.itemsPerEachReviewer =
        //        this.workAllocationFromWizardCommand.itemsPerEachReviewer.substring(0, this.workAllocationFromWizardCommand.itemsPerEachReviewer.length - 1);
        //    let result: WorkAllocWizardResult = await this.WorkAllocationListService.RunAllocationFromWizardCommand(this.workAllocationFromWizardCommand);
        //    //console.log("WorkAllocWizardResult", result);
        //    if (result.isSuccess) this.WorkAllocWizardResult = result;
        //    else {
        //        this.ErrorAndBackToStart();
        //    }
        //}
        //else {

        //    let result: WorkAllocWizardResult = await this.WorkAllocationListService.RunAllocationFromWizardCommand(this.workAllocationFromWizardCommand);
        //    if (result.isSuccess) {
        //        this.WorkAllocationListService.FetchAll();
        //        this.ReviewSetsService.GetReviewSets(false);
        //        this.Cancel();
        //    } else {
        //        this.ErrorAndBackToStart();
        //    }
        //}
    }
    ErrorAndBackToStart() {
        //this.modalService.GenericErrorMessage("Gettin the preview failed, this usually happens when the situation changed and the list of items you want to assign is now different. Please start again.");
        //this.ResetStep34();
        //this.ClearPot();
        //this.PreviousStep();
        //this.PreviousStep();
    }
    //ShowAddManualMember(member: SelectableContact) {
    //    this.ToAddManualMember = member.CloneAndAssingOneItem();
    //    //let's pick a decent default value
    //    const tbmas = this.ToBeManuallyAssignedStill;
    //    if (tbmas < this.workAllocationFromWizardCommand.numberOfItemsToAssign) {
    //        this.ToAddManualMember.NumberOfItems = tbmas;
    //    } else {
    //        this.ToAddManualMember.NumberOfItems = Math.round(this.workAllocationFromWizardCommand.numberOfItemsToAssign / 2);
    //    }
    //    if (this.NofItemstoAssigntoPerson) {
    //        //console.log("input found immediately");
    //        setTimeout(() => this.NofItemstoAssigntoPerson.focus(), 1);
    //    } else {
    //        //console.log("input not yet found");
    //        setTimeout(() => {
    //            if (this.NofItemstoAssigntoPerson) {
    //                this.NofItemstoAssigntoPerson.focus();
    //                //console.log("input found later");
    //            }
    //        }, 5);
    //    }
    //}
    //HideAddManualMember() {
    //    this.ToAddManualMember = null;
    //}
    //AddManualMemberToTable() {
    //    this.WipePreview();
    //    const colN = this.workAllocationFromWizardCommand.peoplePerItem;
    //    let inCurrentCol: number = 0;
    //    if (this.ToAddManualMember != null) {
    //        for (let m of this._manualAssignCol1) {
    //            inCurrentCol += m.NumberOfItems;
    //        }
    //        if (inCurrentCol < this.workAllocationFromWizardCommand.numberOfItemsToAssign) {
    //            //we have room in this column
    //            if (this.workAllocationFromWizardCommand.numberOfItemsToAssign - inCurrentCol >= this.ToAddManualMember.NumberOfItems) {
    //                //easy, we can add person to this column
    //                this._manualAssignCol1.push(this.ToAddManualMember);
    //                this.UpdateSelectedContact(this.ToAddManualMember.contactId, this.ToAddManualMember.NumberOfItems);
    //                this.ToAddManualMember = null;
    //                return;
    //            }
    //            else {
    //                //we need to split this person in two: some in this column, some in the next.
    //                //items for this person minus items we can add in current column
    //                const leftover = this.ToAddManualMember.NumberOfItems - (this.workAllocationFromWizardCommand.numberOfItemsToAssign - inCurrentCol); 
    //                //new cloned person to add in the next column
    //                const cloned:SelectableContact = this.ToAddManualMember.CloneAndAssingOneItem();
    //                cloned.NumberOfItems = leftover;
    //                this.ToAddManualMember.NumberOfItems = this.ToAddManualMember.NumberOfItems - leftover;
    //                this._manualAssignCol1.push(this.ToAddManualMember);
    //                if (colN > 1) {
    //                    this._manualAssignCol2.push(cloned);
    //                    this.UpdateSelectedContact(this.ToAddManualMember.contactId, this.ToAddManualMember.NumberOfItems + leftover);
    //                }
    //                else {//we won't use the second column, so leftover is a throwaway
    //                    this.UpdateSelectedContact(this.ToAddManualMember.contactId, this.ToAddManualMember.NumberOfItems);
    //                }
    //                this.ToAddManualMember = null;
    //                return;
    //            }
    //        }
    //        //do it again, for second column
    //        inCurrentCol = 0;
    //        for (let m of this._manualAssignCol2) {
    //            inCurrentCol += m.NumberOfItems;
    //        }
    //        if (inCurrentCol < this.workAllocationFromWizardCommand.numberOfItemsToAssign) {
    //            //we have room in this column
    //            if (this.workAllocationFromWizardCommand.numberOfItemsToAssign - inCurrentCol >= this.ToAddManualMember.NumberOfItems) {
    //                //easy, we can add person to this column
    //                this._manualAssignCol2.push(this.ToAddManualMember);
    //                this.UpdateSelectedContact(this.ToAddManualMember.contactId, this.ToAddManualMember.NumberOfItems);
    //                this.ToAddManualMember = null;
    //                return;
    //            }
    //            else {
    //                //we need to split this person in two: some in this column, some in the next.
    //                //items for this person minus items we can add in current column
    //                const leftover = this.ToAddManualMember.NumberOfItems - (this.workAllocationFromWizardCommand.numberOfItemsToAssign - inCurrentCol);
    //                //new cloned person to add in the next column
    //                const cloned: SelectableContact = this.ToAddManualMember.CloneAndAssingOneItem();
    //                cloned.NumberOfItems = leftover;
    //                this.ToAddManualMember.NumberOfItems = this.ToAddManualMember.NumberOfItems - leftover;
    //                this._manualAssignCol2.push(this.ToAddManualMember);
    //                if (colN > 2) {
    //                    this._manualAssignCol3.push(cloned);
    //                    this.UpdateSelectedContact(this.ToAddManualMember.contactId, this.ToAddManualMember.NumberOfItems + leftover);
    //                }
    //                else {//we won't use the third column, so leftover is a throwaway
    //                    this.UpdateSelectedContact(this.ToAddManualMember.contactId, this.ToAddManualMember.NumberOfItems);
    //                }
    //                this.ToAddManualMember = null;
    //                return;
    //            }
    //        }
    //        //do it again, for third column
    //        inCurrentCol = 0;
    //        for (let m of this._manualAssignCol3) {
    //            inCurrentCol += m.NumberOfItems;
    //        }
    //        if (inCurrentCol < this.workAllocationFromWizardCommand.numberOfItemsToAssign) {
    //            //we have room in this column
    //            if (this.workAllocationFromWizardCommand.numberOfItemsToAssign - inCurrentCol >= this.ToAddManualMember.NumberOfItems) {
    //                //easy, we can add person to this column
    //                this._manualAssignCol3.push(this.ToAddManualMember);
    //                this.UpdateSelectedContact(this.ToAddManualMember.contactId, this.ToAddManualMember.NumberOfItems);
    //                this.ToAddManualMember = null;
    //                return;
    //            }
    //            else {
    //                //we need to split this person in two: some in this column, some in the next.
    //                //items for this person minus items we can add in current column
    //                const leftover = this.ToAddManualMember.NumberOfItems - (this.workAllocationFromWizardCommand.numberOfItemsToAssign - inCurrentCol);
    //                //new cloned person to add in the next column, we'll throw it away, unless we'll start supporting quadruple coding...
    //                const cloned: SelectableContact = this.ToAddManualMember.CloneAndAssingOneItem();
    //                cloned.NumberOfItems = leftover;
    //                this.ToAddManualMember.NumberOfItems = this.ToAddManualMember.NumberOfItems - leftover;
    //                this._manualAssignCol3.push(this.ToAddManualMember);
    //                //can't add the leftover, we don't support _manualAssignCol4
    //                //this._manualAssignCol4.push(cloned);
    //                //leftover is a throwaway
    //                this.UpdateSelectedContact(this.ToAddManualMember.contactId, this.ToAddManualMember.NumberOfItems);
    //                this.ToAddManualMember = null;
    //                return;
    //            }
    //        }
    //    }
    //}
    //private UpdateSelectedContact(id: number, assignedItems: number) {
    //    //console.log("UpdateSelectedContact", id, assignedItems);
    //    let c = this.SelectedContacts.find(found => found.contactId == id);
    //    if (c != undefined) {
    //        c.NumberOfItems = assignedItems;
    //    }
    //}
    //RemoveLastManualMember() {
    //    //contact might appear in 2 columns, so we need to check
    //    this.WipePreview();
    //    let con: SelectableContact | undefined = undefined;
    //    if (this._manualAssignCol3.length > 0) {
    //        con = this._manualAssignCol3.pop();
    //        if (this._manualAssignCol3.length == 0) {
    //            let con2 = this._manualAssignCol2.find(found => con != undefined && found.contactId == con.contactId)
    //            if (con2 != undefined) {
    //                this._manualAssignCol2.pop();
    //            }
    //        }
    //        //we also need to reset the pot for this person in this.Contacts
    //        let con3 = this.Contacts.find(found => con != undefined && found.contactId == con.contactId);
    //        if (con3 != undefined) con3.NumberOfItems = 0;
    //        return;
    //    }
    //    else if (this._manualAssignCol2.length > 0) {
    //        con = this._manualAssignCol2.pop();
    //        if (this._manualAssignCol2.length == 0) {
    //            let con2 = this._manualAssignCol1.find(found => con != undefined && found.contactId == con.contactId)
    //            if (con2 != undefined) {
    //                this._manualAssignCol1.pop();
    //            }
    //        }
    //        //we also need to reset the pot for this person in this.Contacts
    //        let con3 = this.Contacts.find(found => con != undefined && found.contactId == con.contactId);
    //        if (con3 != undefined) con3.NumberOfItems = 0;
    //        return;
    //    }
    //    else if (this._manualAssignCol1.length > 0) {
    //        con = this._manualAssignCol1.pop();
    //        //we also need to reset the pot for this person in this.Contacts
    //        let con3 = this.Contacts.find(found => con != undefined && found.contactId == con.contactId);
    //        if (con3 != undefined) con3.NumberOfItems = 0;
    //        return;
    //    }
    //}
    //DistributeRemainingManualAssignTable() {
    //    this.WipePreview();
    //    let remaining = this.Contacts.filter(found => found.IsSelected && found.NumberOfItems == 0);
    //    const NofPeople = remaining.length;
    //    if (NofPeople == 0) return;
    //    const NtoAssign = this.ToBeManuallyAssignedStill;
    //    let Remainder: number = (NtoAssign) % NofPeople;
    //    let itemsPerPerson: number = (NtoAssign - Remainder) / NofPeople;
    //    for (let p of remaining) {
    //        if (Remainder > 0) {
    //            p.NumberOfItems = itemsPerPerson + 1;
    //            Remainder--;
    //        } else p.NumberOfItems = itemsPerPerson;
    //        const toAdd = p.CloneAndAssingOneItem();
    //        toAdd.NumberOfItems = p.NumberOfItems;
    //        this.ToAddManualMember = toAdd;
    //        this.AddManualMemberToTable();
    //    }
    //}
    ResetManualAssignTable() {
        //this.WipePreview();
        //this._manualAssignCol1 = [];
        //this._manualAssignCol2 = [];
        //this._manualAssignCol3 = [];
        //for (let c of this.Contacts) {
        //    c.NumberOfItems = 0;
        //}
    }
    SetWorkToDoCodeSet(codeset: ReviewSet) {
        //this.WorkToDoSelectedCodeSet = codeset;
        //this.workAllocationFromWizardCommand.groupsPrefix = "Coding on '" + this.WorkToDoSelectedCodeSet.name + "'";
        //this.workAllocationFromWizardCommand.work_to_do_setID = codeset.set_id;
    }
    //SelectMember(member: SelectableContact) {
    //    this.ResetStep34();
    //    member.IsSelected = !member.IsSelected;
    //}
    PeoplePerItemChanged() {
        this.ShowChangeDataEntry = false;
    }
    //ShowChangeDataEntryClicked() {
    //    this.ShowChangeDataEntry = !this.ShowChangeDataEntry;
    //}
    
    ShowEditCodesPrefixClicked() {
        //if (this.ShowEditCodesPrefix == false && this.workAllocationFromWizardCommand.groupsPrefix == "") {
        //    this.workAllocationFromWizardCommand.groupsPrefix = "Coding on '" + this.WorkToDoSelectedCodeSet.name + "'";
        //}
        //this.ShowEditCodesPrefix = !this.ShowEditCodesPrefix;
    }
    

    ClearPot() {
        //this.workAllocationFromWizardCommand.numberOfItemsToAssign = -1
    }
    ResetStep34() {
        this.ResetManualAssignTable();
    }
    WipePreview() {
        //this.WorkAllocWizardResult = null;
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
   

}
export interface kvStringSelectFrom {
    key: string;
    value: string;
}


