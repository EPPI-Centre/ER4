import { Component, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter, ElementRef, NgZone, AfterViewInit } from '@angular/core';
import { ReviewSetsService, SetAttribute, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService, ChangeDataEntryMessage } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact, ReviewInfo } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { kvSelectFrom } from './WorkAllocationComp.component';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
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
        private router: Router, private ngZone: NgZone,
        private ReviewSetsService: ReviewSetsService,
        private modalService: ModalService,
        private ReviewSetsEditingService: ReviewSetsEditingService,
        private ReviewInfoService: ReviewInfoService,
        private ReviewerIdentityService: ReviewerIdentityService,
        private ConfirmationDialogService: ConfirmationDialogService,
        private PriorityScreeningService: PriorityScreeningService,
        private ItemListService: ItemListService
	) { }
    ngOnInit() {
        this.PriorityScreeningService.Fetch();
        this.RevInfoSub = this.ReviewInfoService.ReviewInfoChanged.subscribe(() => this.RefreshRevinfo());
        if (!this.ReviewerIdentityService.HasAdminRights) this.CurrentStep = 4;
    }
    ngAfterViewInit() {
        if (this.ReviewInfoService.ReviewInfo.showScreening == false) this.Cancel();
        else {
            //console.log("will clone revinfo:", this.ReviewInfoService.ReviewInfo);
            this.DoRefreshRevinfo();
            this.PriorityScreeningService.GetTrainingScreeningCriteriaList();
        }
    }
    public revInfo: ReviewInfo = new ReviewInfo();
    @Output() emitterCancel = new EventEmitter();
    public CurrentStep: number = 0;
    private _stepNames: string[] = ["Start", "define what to do", "define how to do it", "automation options", "Show all settings"];
    @ViewChild('faketablerow') faketablerow!: ElementRef;
    @ViewChild('WithOrWithoutCode') WithOrWithoutCode!: codesetSelectorComponent;
    @ViewChild('AddTrainingCriteriaDDown') AddTrainingCriteriaDDown!: codesetSelectorComponent;

    public DropdownWithWithoutSelectedCode: singleNode | null = null;
    public DropdownAddTrainingCriteriaSelectedCode: singleNode | null = null;
    private subGotPriorityScreeningData: Subscription | null = null;
    private RevInfoSub: Subscription | null = null;
    public AllowEditOnStep4: boolean = false;
    private _ScreenAllItems: boolean = true;
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
    public ConfirmTrainingListIsGood: string = "";
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
        if (this.CurrentStep > 0) this.CurrentStep--;
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
        if (this.selectedCodeSetDropDown == null || (!this.ScreenAllItems && this.DropdownWithWithoutSelectedCode == null)) return false;
        else return (this.CanGoToNextStep());
    }
    public CanGoToStep3(): boolean {
        //console.log("CanGoToStep3()", this.revInfo.screeningMode, this.revInfo.screeningCodeSetId, this.selectedCodeSetDropDown == null, !this.CheckAndUpdatePeoplePerItem());
        if (this.revInfo.screeningMode == "" || this.revInfo.screeningCodeSetId < 1 || this.selectedCodeSetDropDown == null || !this.CheckAndUpdatePeoplePerItem()) return false;
        //console.log("CanGoToStep3()2", this.ConfirmTrainingListIsGood);
        return (this.CanChangePeoplePerItem && this.CanGoToNextStep());
    }
    public get CanChangeWhatToScreen(): boolean {
        if (this.selectedCodeSetDropDown == null) return false;
        else return true;
    }
    public get CanChangePeoplePerItem(): boolean {
        if (this.revInfo.screeningMode == '') return false;
        if (this.revInfo.screeningMode == "Priority") {
            if (this.TrainingScreeningCriteriaListIsNotGoodMsg != "") return false;
            if (this.CurrentStep != 4 && this.ConfirmTrainingListIsGood !== "I've checked") return false;
            else return true;
        }
        else return true;
    }
    public get CanChangeAutoExcludeAndIndexing(): boolean {
        if (this.revInfo.screeningReconcilliation == "") return false;
        else return true;
    }
    public GoToAllinOneStep() {
        this.CurrentStep = 4;
    }
    public get ScreenAllItems(): boolean {
        return this._ScreenAllItems;
    }
    public set ScreenAllItems(val: boolean) {
        if (val == true) {
            if (this._ScreenAllItems == false) {
                this.revInfo.screeningWhatAttributeId = 0;
                this.DropdownWithWithoutSelectedCode = null;
            }
        }
        this._ScreenAllItems = val;
    }
    IsServiceBusy(): boolean {
        //console.log("IsWizardService busy?", this.ReviewSetsService.IsBusy, this.ReviewSetsEditingService.IsBusy, this.ReviewInfoService.IsBusy, this.WorkAllocationListService.IsBusy)
        if (this.ReviewSetsService.IsBusy
            || this.PriorityScreeningService.IsBusy
            || this.ReviewSetsEditingService.IsBusy
            || this.ReviewInfoService.IsBusy
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
    public get HasAdminRights(): boolean {
        return this.ReviewerIdentityService.HasAdminRights;
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
        if (this.CurrentStep == 4 && JSON.stringify(this.revInfo) === JSON.stringify(this.ReviewInfoService.ReviewInfo)) return false;
        return true;
    }
    public get TrainingScreeningCriteriaListIsNotGoodMsg(): string {
        if (this.PriorityScreeningService.TrainingScreeningCriteria.length < 2) {
            return "The list of training codes is <strong>incomplete: should contain at least two codes</strong> (one for 'Include' and one for 'Exclude').";
        }
        else if (this.PriorityScreeningService.TrainingScreeningCriteria.filter(f => f.included == true).length < 1) {
            return "The list of training codes is <strong>incomplete: should contain at least one code of 'Include' type</strong>. Without it, the machine cannot learn what items are likely to be included." ;
        }
        else if (this.PriorityScreeningService.TrainingScreeningCriteria.filter(f => f.included == false).length < 1) {
            return "The list of training codes is <strong>incomplete: should contain at least one code of 'Exclude' type</strong>. Without it, the machine cannot learn what items are likely to be excluded.";
        }
        else return "";
    }
    public get ConfigurationIsValid(): boolean {
        if (this.revInfo.screeningCodeSetId < 1) return false;//don't know what to screen
        if (this.revInfo.screeningWhatAttributeId < 0) return false; //0 if screen all items more than 0 if screen items with this code
        if (!this.ScreenAllItems && this.revInfo.screeningWhatAttributeId < 1) return false;//screen items with this code: need a valid AttributeId
        if (this.ScreenAllItems && this.revInfo.screeningWhatAttributeId != 0) return false;//screen all items: need this to be 0
        if (this.revInfo.screeningMode == "Priority" && this.TrainingScreeningCriteriaListIsNotGoodMsg != "") return false;//not enough codes to learn from
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
            if (this.CurrentStep == 4) {
                //user is doing the "I'll edit all my settings in one go" thing, so we'll ask what to do.
                this.ConfirmationDialogService.confirm("Update training codes?",
                    "You have <em>changed</em> the screening tool.<br /> Would you like to automatically update the list of training codes?<br />Training codes are <strong>important</strong>! They define what the machine will learn from, so <strong>please check</strong> that they are correct, in any case."
                    , false, "", "Yes please (I'll check)", "No, I'll do it myself"
                    , "lg")
                    .then((confirmed: any) => {
                        if (confirmed) {
                            this.DoResetTrainingCodes();
                        }
                    }).catch(() => { });
            } else {
                //we're doing this in the wizard, so we'll silently change the codes in all cases...
                this.DoResetTrainingCodes();
            }
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
        console.log("DoRefreshRevinfo", this.ReviewInfoService.ReviewInfo.screeningReconcilliation);
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
        //console.log("regenerate list: ", this.revInfo.screeningMode, this.PriorityScreeningService.TrainingScreeningCriteria.length);
        if (!this.CanWrite()) return;
        else if (this.revInfo.screeningMode == "Priority" && this.TrainingScreeningCriteriaListIsNotGoodMsg != "") {
            this.modalService.GenericErrorMessage("Sorry, the current list of training codes (the codes from which the machine will learn) isn't valid, please ensure the list contains the correct codes and try again.");
        }
        else {
            this.PriorityScreeningService.RunNewTrainingCommand(false);
        }
    }
    SaveOptions() {
        this.ReviewInfoService.Update(this.revInfo);
        this.AllowEditOnStep4 = false;
        this.CancelEditingAllOptions();
    }
    async SaveOptionsAndCreateList() {
        let res: boolean = await this.ReviewInfoService.Update(this.revInfo);
        if (res) {
            this.GenerateList();
            this.AllowEditOnStep4 = false;
            this.CancelEditingAllOptions();
            this.Cancel();
        }
    }
    async SaveOptionsAndGoToStep4() {
        let res: boolean = await this.ReviewInfoService.Update(this.revInfo);
        if (res) {
            this.AllowEditOnStep4 = false;
            this.CancelEditingAllOptions();
            this.GoToAllinOneStep();
        }
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
            if (this.revInfo.screeningReconcilliation != "Single") this.revInfo.screeningReconcilliation = "Single";
            if (this.revInfo.screeningNPeople >= 1) {
                //set is in normal data entry, so we'll automatically set screeningNPeople to 0
                if (this.revInfo.screeningNPeople == 1) {
                    this.ngZone.run(() => setTimeout(() => {
                        console.log("screeningNPeople goes to => 0");
                        this.revInfo.screeningNPeople = 0;
                    }, 8));
                }
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
                if (this.revInfo.screeningNPeople == 0) {
                    this.ngZone.run(() => setTimeout(() => {
                        console.log("screeningNPeople goes to => 1");
                        this.revInfo.screeningNPeople = 1;
                    }, 8));//just avoiding to have a 0 here...
                }//we delay this so to let Angular UI to catch up...
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
    
}
export interface kvStringSelectFrom {
    key: string;
    value: string;
}


