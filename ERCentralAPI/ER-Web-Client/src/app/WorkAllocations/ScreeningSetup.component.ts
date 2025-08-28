import { Component, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter, ElementRef, NgZone, AfterViewInit } from '@angular/core';
import { ReviewSetsService, SetAttribute, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService, ChangeDataEntryMessage } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService, Contact, ReviewInfo } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { kvSelectFrom } from './WorkAllocationComp.component';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ModalService } from '../services/modal.service';
import { PriorityScreeningService, Training, iTrainingScreeningCriteria, ScreeningFromSearchIterationList, ScreeningFromSearchIterationRun, iScreeningFromSearchIteration } from '../services/PriorityScreening.service';
import { Helpers } from '../helpers/HelperMethods';
import { GridDataResult, RowClassArgs } from '@progress/kendo-angular-grid';
import { ItemListService } from '../services/ItemList.service';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import 'hammerjs';
import { Search, searchService } from '../services/search.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { TabStripComponent } from '@progress/kendo-angular-layout';

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
    private ItemListService: ItemListService,
    private notificationService: NotificationService,
    private SearchService: searchService
  ) { }

  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;

  ngOnInit() {
    this.PriorityScreeningService.Fetch().then(()=> this.PriorityScreeningService.FetchTrainingFromSearchList());
    this.RevInfoSub = this.ReviewInfoService.ReviewInfoChanged.subscribe(() => this.RefreshRevinfo());
    if (!this.ReviewerIdentityService.HasAdminRights) this.CurrentStep = 5;
    
    if (this.ReviewInfoService.ReviewInfo.reviewId == 0) {
      this.ReviewInfoService.Fetch();
    }
  }
  ngAfterViewInit() {
    if (this.ReviewInfoService.ReviewInfo.showScreening == false) this.Cancel();
    else {
      //console.log("will clone revinfo:", this.ReviewInfoService.ReviewInfo);
      this.DoRefreshRevinfo();
      this.PriorityScreeningService.GetTrainingScreeningCriteriaList();
      this.PrepareSearchesList();
    }
  }
  public EditingRevInfo: ReviewInfo = new ReviewInfo();
  @Output() emitterCancel = new EventEmitter();
  public CurrentStep: number =0;
  private _stepNames: string[] = ["Start", "define what type of list to use" ,"define what to do", "define how to do it", "automation options", "show all settings"
    , "screening tool and what search to use", "screening mode and automation"];
  @ViewChild('WithOrWithoutCode') WithOrWithoutCode!: codesetSelectorComponent;
  @ViewChild('AddTrainingCriteriaDDown') AddTrainingCriteriaDDown!: codesetSelectorComponent;
  @ViewChild('tabstripScreening') tabstripScreening!: TabStripComponent;

  public DropdownWithWithoutSelectedCode: singleNode | null = null;
  public DropdownAddTrainingCriteriaSelectedCode: singleNode | null = null;
  private RevInfoSub: Subscription | null = null;
  public AllowEditOnStep4: boolean = false;
  public AllowEditOnStep4fs: boolean = false;
  private _ScreenAllItems: boolean = true;
  private _ItemsWithThisAttribute: SetAttribute | null = null;
  public selectedCodeSetDropDown: ReviewSet | null = null;
  public isCollapsedAllocateOptions: boolean = false;
  public isCollapsedDropdownAddTrainingCriteria: boolean = false;
  public ShowAddTrainingCriteria: boolean = false;
  public ShowTrainingTable: boolean = false;
  public ShowEditTrainingTable: boolean = false;
  public ShowTrainingFSTable: boolean = false;
  private _CanChangeDataEntryMode: boolean = false;
  public ShowChangeDataEntry: boolean = false;
  public DestinationDataEntryMode: string = "";
  private _ItemsWithIncompleteCoding: number = -1;
  public ChangeDataEntryModeMessage: string = "";
  public ConfirmTrainingListIsGood: string = "";

  public WizardDoPS: boolean = true;
  public FS_ShowListProgress: boolean = false;
  public SearchToUseForFromSearchList: Search | null = null;
  private _SearchesWithScores: Search[] = [];
  public get SearchesWithScores(): Search[] {
    return this._SearchesWithScores;
  }
  public get TrainingFromSearchList(): ScreeningFromSearchIterationList {
    return this.PriorityScreeningService.TrainingFromSearchList;
  }

  async PrepareSearchesList() {
    if (this.ReviewInfoService.ReviewInfo.reviewId < 1) {
      await this.ReviewInfoService.Fetch();
    }
    if (this.SearchService.SearchList.length == 0) {
      if (this.SearchService.IsBusy) {
        //console.log("SearchService.IsBusy, waiting...");
        let safetyC: number = 0;
        while (this.SearchService.IsBusy && safetyC < 350) {//not going to wait here more than ~35s
          await Helpers.Sleep(100);
          safetyC++;
        }
      }
      if (this.SearchService.SearchList.length == 0) {
        //console.log("Last ditch attempt to get searches list...");
        await setTimeout(async () => {
          //we need to delay this call, otherwise SearchComponent throws the value changed after checking it error
          await this.SearchService.Fetch();
          if (this.SearchService.SearchList.length > 0) {
            //now we have some searches in the list, so we can consume it to set-up things in here.
            this.PrepareSearchesList();
          }
        }, 10);
      }
    }
    
    this._SearchesWithScores = this.SearchService.SearchList.filter(f => f.isClassifierResult == true);
    this._SearchesWithScores.sort((a, b) => b.searchNo - a.searchNo);
    this.innerSetCurrentSearchFS();
    
  }
  private innerSetCurrentSearchFS() {
    
    this.SearchToUseForFromSearchList = null;
    this.EditingRevInfo.screeningFSListSearchId = this.ReviewInfoService.ReviewInfo.screeningFSListSearchId;
    this.EditingRevInfo.screeningFSListSearchName = this.ReviewInfoService.ReviewInfo.screeningFSListSearchName;
    if (this.ReviewInfoService.ReviewInfo.screeningFSListSearchId > 0) {
      const foundSearch = this.SearchesWithScores.find(f => f.searchId == this.ReviewInfoService.ReviewInfo.screeningFSListSearchId);
      if (foundSearch) {
        this.SearchToUseForFromSearchList = foundSearch;
        const SearchIdTouse = this.SearchToUseForFromSearchList.searchId;
        for (let s of this._SearchesWithScores) {
          s.selected = false;
          if (s.searchId == SearchIdTouse) {
            s.selected = true;
          }
        }
      }
      else {//we couldn't find the search to "select", but we still want to unselect all searches
        for (let s of this._SearchesWithScores) {
          s.selected = false;
        }
      }
    }
    else {//we make sure no search is selected, because nothing is configured in RevInfo. so nothing _should_ be selected
      for (let s of this._SearchesWithScores) {
        s.selected = false;
      }
    }
  }


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
    { key: "auto safet", value: 'Multiple: auto complete (safety first)' }
  ]; //{ key: "Single", value: 'Single (auto-completes)' }, //not used, as we set it automatically.
  public get ReconcileOptions(): kvStringSelectFrom[] {
    return this._ReconcileOptions;
  }
  public get SelectedReconcileOptionName(): string {
    if (this.EditingRevInfo.screeningReconcilliation == "Single") return "Single (auto completes)";
    else {
      let found = this._ReconcileOptions.find(f => f.key == this.EditingRevInfo.screeningReconcilliation);
      if (found != undefined) return found.value;
    }
    return "Not Set";
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

  public get ShowTrainingFSTableText(): string {
    if (this.ShowTrainingFSTable) return "Hide Progress Table";
    else return "Show Progress Table";
  }
  public get ScreeningListIsGood(): boolean {
    return this.ReviewInfoService.ReviewInfo.screeningListIsGood;
  }

  public get ScreeningFromSearchListIsGood(): boolean {
    return this.ReviewInfoService.ReviewInfo.screeningFromSearchListIsGood;
  }

  public get ItemsWithThisAttribute(): SetAttribute | null {
    if (this.EditingRevInfo.screeningWhatAttributeId > 0
      && (this._ItemsWithThisAttribute == null || this._ItemsWithThisAttribute.attribute_id != this.EditingRevInfo.screeningWhatAttributeId)) {
      this._ItemsWithThisAttribute = this.ReviewSetsService.FindAttributeById(this.EditingRevInfo.screeningWhatAttributeId)
    } else if (this.EditingRevInfo.screeningWhatAttributeId < 1) {
      this._ItemsWithThisAttribute = null;
    }
    //console.log("ItemsWithThisAttribute", this.EditingRevInfo.screeningWhatAttributeId, this._ItemsWithThisAttribute);
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
    //console.log("CanGoToStep3()", this.EditingRevInfo.screeningMode, this.EditingRevInfo.screeningCodeSetId, this.selectedCodeSetDropDown == null, !this.CheckAndUpdatePeoplePerItem());
    if (this.EditingRevInfo.screeningMode == "" || this.EditingRevInfo.screeningCodeSetId < 1 || this.selectedCodeSetDropDown == null || !this.CheckAndUpdatePeoplePerItem()) return false;
    //console.log("CanGoToStep3()2", this.ConfirmTrainingListIsGood);
    return (this.CanChangePeoplePerItem && this.CanGoToNextStep());
  }
  public CanGoToStep7(): boolean {
    //console.log("Can go to S7:", this.SearchToUseForFromSearchList);
    if (this.selectedCodeSetDropDown == null || this.SearchToUseForFromSearchList == null) return false;
    else return (this.CanGoToNextStep());
  }
  public get CanChangeWhatToScreen(): boolean {
    if (this.selectedCodeSetDropDown == null) return false;
    else return true;
  }
  public get CanChangePeoplePerItem(): boolean {
    if (this.CurrentStep == 7) return this.selectedCodeSetDropDown != null;//can always change if wizard for the FS type of list
    if (this.CurrentStep == 5 && this.AllowEditOnStep4fs == true) return this.selectedCodeSetDropDown != null;//as above
    if (this.EditingRevInfo.screeningMode == '') return false;
    if (this.EditingRevInfo.screeningMode == "Priority") {
      if (this.TrainingScreeningCriteriaListIsNotGoodMsg != "") return false;
      if (this.CurrentStep == 3 && this.ConfirmTrainingListIsGood !== "I've checked") return false;
      else return true;
    }
    else return true;
  }
  public get CanChangeAutoExcludeAndIndexing(): boolean {
    if (this.EditingRevInfo.screeningReconcilliation == "") return false;
    else return true;
  }

  public get CanEditSelectedSet(): boolean {
    return (this.CanWrite() && this.selectedCodeSetDropDown != null && this.selectedCodeSetDropDown.allowEditingCodeset);
  }

  public get CanChangeScreeningTool(): boolean {
    if (this.CurrentStep == 5 && this.AllowEditOnStep4) return true;//editing all settings for PS
    if (this.CurrentStep < 5) return true;//editing via the PS wizard
    if (this.TrainingScreeningCriteriaList.length == 0) return true;//safe to change the screening tool: PS isn't learning from anything!
    return false;//not allowed in all other cases
  }

  public GoToAllinOneStep() {
    this.CurrentStep = 5;
  }
  public get ScreenAllItems(): boolean {
    return this._ScreenAllItems;
  }
  public set ScreenAllItems(val: boolean) {
    if (val == true) {
      if (this._ScreenAllItems == false) {
        this.EditingRevInfo.screeningWhatAttributeId = 0;
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
    return Helpers.FormatDate(DateSt);
  }

  FormatDate2(DateSt: string): string {
    return Helpers.FormatDate2(DateSt);
  }
  ShowAddTrainingCriteriaClick() {
    this.ShowAddTrainingCriteria = !this.ShowAddTrainingCriteria;
  }

  public get CanChangeDataEntryMode(): boolean {
    //console.log("CanChangeDataEntryMode", this._CanChangeDataEntryMode, this.CanWrite(), this._ItemsWithIncompleteCoding)
    return (this._CanChangeDataEntryMode && this.CanWrite() && this._ItemsWithIncompleteCoding != -1);
  }
  public get ScreeningTools(): ReviewSet[] {
    return this.ReviewSetsService.ReviewSets.filter(found => found.setType.setTypeName == "Screening")
  }
  public get ConfigHasChanged(): boolean {
    if (JSON.stringify(this.EditingRevInfo) === JSON.stringify(this.ReviewInfoService.ReviewInfo)) return false;
    else return true;
  }
  public get TrainingScreeningCriteriaListIsNotGoodMsg(): string {
    if (this.PriorityScreeningService.TrainingScreeningCriteria.length < 2) {
      return "The list of training codes is <strong>incomplete: should contain at least two codes</strong> (one for 'Include' and one for 'Exclude').";
    }
    else if (this.PriorityScreeningService.TrainingScreeningCriteria.filter(f => f.included == true).length < 1) {
      return "The list of training codes is <strong>incomplete: should contain at least one code of 'Include' type</strong>. Without it, the machine cannot learn what items are likely to be included.";
    }
    else if (this.PriorityScreeningService.TrainingScreeningCriteria.filter(f => f.included == false).length < 1) {
      return "The list of training codes is <strong>incomplete: should contain at least one code of 'Exclude' type</strong>. Without it, the machine cannot learn what items are likely to be excluded.";
    }
    else return "";
  }
  public get CanSaveConfiguration(): boolean {
    if (!this.CanWrite()) return false;
    if (!this.ConfigurationIsValid) return false;
    if (this.CurrentStep == 5 && this.ConfigHasChanged == false) return false;
    return true;
  }
  public get ConfigurationIsValid(): boolean {
    if (this.EditingRevInfo.screeningCodeSetId < 1) return false;//don't know what to screen against
    if (this.EditingRevInfo.screeningWhatAttributeId < 0) return false; //0 if screen all items more than 0 if screen items with this code
    if (!this.ScreenAllItems && this.EditingRevInfo.screeningWhatAttributeId < 1) return false;//screen items with this code: need a valid AttributeId
    if (this.ScreenAllItems && this.EditingRevInfo.screeningWhatAttributeId != 0) return false;//screen all items: need this to be 0
    if (this.EditingRevInfo.screeningMode == "Priority" && this.TrainingScreeningCriteriaListIsNotGoodMsg != "") return false;//not enough codes to learn from
    if (this.ScreeningModeOptions.findIndex(found => found.value == this.EditingRevInfo.screeningMode) < 1) {
      //console.log("type of list isn't set?", this.EditingRevInfo.screeningMode);
      return false;//type of list isn't set.
    }
    return this.CheckDataEntryMode();    
  }
  private CheckDataEntryMode(): boolean {
    const set = this.selectedCodeSetDropDown;
    if (set == null) return false;//uh? Chosen set isn't in review!
    //console.log("Got here", this.EditingRevInfo);
    if (this.EditingRevInfo.screeningReconcilliation == "") return false;//reconciliation is NOT set
    if (this.EditingRevInfo.screeningNPeople > 1) {//multiple people per item:
      if (set.codingIsFinal) return false;//but codeset is in "normal" data entry.
      else if (this.EditingRevInfo.screeningReconcilliation == "Single") return false;//reconciliation set to auto complete
    } else {//one person per item...
      if (!set.codingIsFinal) return false;//but codeset is in "comparison" data entry.
      else if (this.EditingRevInfo.screeningReconcilliation !== "Single") return false;//reconciliation NOT set to auto complete
    }
    return true;//no concern found
  }

  public get CanSaveConfigurationFs(): boolean {
    if (!this.CanWrite()) return false;
    if (!this.ConfigurationIsValidFs) return false;
    if (this.CurrentStep == 5 && this.ConfigHasChanged == false) return false;
    return true;
  }

  public get ConfigurationIsValidFs(): boolean {
    if (this.EditingRevInfo.screeningCodeSetId < 1) return false;//don't know what to screen against
    if (this.EditingRevInfo.screeningFSListSearchId < 1 && this.SearchToUseForFromSearchList == null) return false;//search
    return this.CheckDataEntryMode();
  }


  public get ReconciliationModeError(): string {
    if (this.selectedCodeSetDropDown == null) return "";//we have bigger problems!
    else if (this.EditingRevInfo.screeningReconcilliation == "") return "Please select a reconciliation mode.";
    else if (this.selectedCodeSetDropDown.codingIsFinal && this.EditingRevInfo.screeningReconcilliation !== "Single") {
      //we'll fix this on the fly...
      this.EditingRevInfo.screeningReconcilliation = "Single";
      return "";
    }
    else if (!this.selectedCodeSetDropDown.codingIsFinal && this.EditingRevInfo.screeningReconcilliation == "Single") {
      return "Please select a valid reconciliation mode.";
    }
    return "";
  }

  public get ReconciliationModeSummary(): string {
    //console.log("ReconciliationModeSummary...", this.EditingRevInfo.screeningReconcilliation);
    const warn = " Note that if many people participate in screening, you might need to create many different reconciliations, to capture all possible \"pairs\".";
    const warn2 = "<br /><strong class='alert-warning'>Warning:</strong> Auto-Completions will not appear in comparisons, which <strong>makes it hard</strong> to measure the level of agreement.<br />";
    let result: string = "";
    if (this.EditingRevInfo.screeningReconcilliation == "Single") {
      result = "";
    }
    else if (this.EditingRevInfo.screeningReconcilliation == "no compl") {
      result = "No \"auto reconciliations\", you will need to \"Complete\" agreements yourself." + warn;
    }
    else if (this.EditingRevInfo.screeningReconcilliation == "auto code") {
      result = "Agreements at the level of (each) single code will be automatically completed." + warn2 + warn;
    }
    else if (this.EditingRevInfo.screeningReconcilliation == "auto excl") {
      result = "Agreements at the level of the inclusion/exclusion decision will be automatically completed." + warn2 + warn;
    }
    else if (this.EditingRevInfo.screeningReconcilliation == "auto safet") {
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
      this.EditingRevInfo.screeningWhatAttributeId = (this.DropdownWithWithoutSelectedCode as SetAttribute).attribute_id;
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

  //this changes what screening tool is set for the review
  setCodeSetDropDown(codeset: ReviewSet) {
    //we need to check 2 things.
    //(1) if we're editing PS settings and there is a FS list in place, changing the Screening tool makes the FS list potentially wrong - got to warn users.
    //(2) if we're editing  PS settings "all at once" we have to ask what to do about training codes
    //So, we do the 1st check here, tell people about the gotcha if needed, then proceed to DoSetCodeSetDropDown(...), where we may ask another question to users.
    if (this.selectedCodeSetDropDown && this.selectedCodeSetDropDown.set_id == codeset.set_id) {
      return;//nothing to change!!
    }
    if (this.selectedCodeSetDropDown == null) {
      //starting from scratch, safe to proceed without warnings
      this.DoSetCodeSetDropDown(codeset);
      return;
    }
    let Msg = "Changing the screening tool is allowed, but it will instantly invalidate all values in the progress graphs and tables.<br />Proceed anyway?";
    const iterats = this.TrainingFromSearchList.AllITerations;
    if (iterats.length > 0 //FS is in use or has been in use
      && this.ScreeningFromSearchListIsGood //we haven't "finished" this list!
    ) {
      Msg = "Changing the screening tool is allowed, but it will instantly invalidate all values in the progress graphs and tables.<br /><br />"
        + "Moreover, it's likely to make the current 'from search' screening list <strong>invalid/nonsensical</strong>, so you should then check and possibly re-create that list too.<br />"
        + "Proceed anyway?";
    }
    this.ConfirmationDialogService.confirm("Change the screening tool?"
      , Msg, false, "", "Proceed", "Cancel"
    ).then((res: any) => {
      if (res == true) this.DoSetCodeSetDropDown(codeset);
    }).catch(() => { });
  }
  //this changes what screening tool is set for the review
  DoSetCodeSetDropDown(codeset: ReviewSet) {
    this.selectedCodeSetDropDown = codeset;
    if (this.EditingRevInfo.screeningCodeSetId !== codeset.set_id) {
      //we're changing the "screening tool" for the review, might need to change the Codes that PS is learning from
      this.EditingRevInfo.screeningCodeSetId = codeset.set_id;
      if (this.CurrentStep == 5
        && this.AllowEditOnStep4 == true
      ) {
        //user is doing the "I'll edit all my settings in one go (for PS)" thing, so we'll ask what to do.
        this.ConfirmationDialogService.confirm("Update training codes?",
          "You have <em>changed</em> the screening tool.<br /> Would you like to automatically update the list of training codes?<br />Training codes are <strong>important</strong>! They define what the machine will learn from, so <strong>please check</strong> that they are correct, in any case."
          , false, "", "Yes, auto-update", "No, I'll do it myself"
          , "lg")
          .then((confirmed: any) => {
            if (confirmed) {
              this.DoResetTrainingCodes();
            }
          }).catch(() => { });
      }
      else if (this.CurrentStep < 5) {
        //we're doing this in the wizard, so we'll silently change the codes in all cases...
        this.DoResetTrainingCodes();
      }// we don't change the training codes in other cases, because code is supposed to NOT allow other cases where a list of training codes already exists

      if (codeset.codingIsFinal) {
        //we picked a "normal" data entry set, people per item needs to be 0 (for some reason!)
        this.EditingRevInfo.screeningNPeople = 0;
      }
      else if (!codeset.codingIsFinal && this.EditingRevInfo.screeningNPeople < 2) {
        //put it to 2 at least...
        this.EditingRevInfo.screeningNPeople = 2;
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
  SetScreeningMode(event: Event) {
    let selection = (event.target as HTMLOptionElement).value;
    let ind = this.ScreeningModeOptions.findIndex(found => found.value == selection);
    if (ind > 0) {
      this.EditingRevInfo.screeningMode = selection;
    } else {
      this.EditingRevInfo.screeningMode = "";
    }
    //console.log("SetRelevantDropDownValues", selection);
  }
  RefreshRevinfo() {
    //if we're editing, we should ask for "permission"...
    if (this.AllowEditOnStep4) {
      this.ConfirmationDialogService.confirm("Reload settings?",
        "The displayed settings <strong>might not match</strong> the settings stored on the server.<br /> "
        + "Do you wish to <strong>reload</strong> the stored settings?<br />"
        + "(Training codes are <strong>not affected</strong>.)"
        , false, "", "Yes (default)", "Cancel"
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
    //console.log("DoRefreshRevinfo", this.ReviewInfoService.ReviewInfo.screeningReconcilliation);

    this.EditingRevInfo = this.ReviewInfoService.ReviewInfo.Clone();
    if (this.EditingRevInfo.screeningWhatAttributeId > 0) {
      this.ScreenAllItems = false;
      if (
        this.DropdownWithWithoutSelectedCode == null ||
        (this.DropdownWithWithoutSelectedCode as SetAttribute).attribute_id != this.EditingRevInfo.screeningWhatAttributeId
      ) {
        this._ItemsWithThisAttribute =
          this.DropdownWithWithoutSelectedCode = this.ReviewSetsService.FindAttributeById(this.EditingRevInfo.screeningWhatAttributeId);
      }
    } else {
      this.ScreenAllItems = true;
      this.DropdownWithWithoutSelectedCode = null;
    }
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
  GetCodingToolDEMode(): string {
    if (this.selectedCodeSetDropDown == null) return "N/A";
    else if (this.selectedCodeSetDropDown.codingIsFinal) return "Single data entry";
    else return "Comparison mode"
  }
  async StartScreening() {
    this.ItemListService.IsInScreeningMode = true;
    await this.PriorityScreeningService.Fetch();
    this.ContinueStartScreening();
  }
  ContinueStartScreening() {
    this.router.navigate(['itemcoding', 'PriorityScreening']);
  }


  async StartScreeningFs() {
    this.ItemListService.IsInScreeningMode = true;
    await this.PriorityScreeningService.FetchTrainingFromSearchList();
    this.router.navigate(['itemcoding', 'ScreeningFromList']);
  }

  public async GenerateList() {
    //console.log("regenerate list: ", this.EditingRevInfo.screeningMode, this.PriorityScreeningService.TrainingScreeningCriteria.length);
    if (!this.CanWrite()) return;
    else if (this.EditingRevInfo.screeningMode == "Priority" && this.TrainingScreeningCriteriaListIsNotGoodMsg != "") {
      this.modalService.GenericErrorMessage("Sorry, the current list of training codes (the codes from which the machine will learn) isn't valid, please ensure the list contains the correct codes and try again.");
    }
    else {
      const res = await this.PriorityScreeningService.RunNewTrainingCommand(null, false);
      if (res != false) {
        //Already Running | Done (Random List) | Starting...
        let msg: string;
        switch (res) {
          case "Starting...":
            msg = "Request to (Re)Generate list submitted. Current state is:"
              + "<div class='w-100 p-0 mx-0 my-2 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
              + res + "</strong></div>"
              + "Ordinarily, re-training takes between 6 and 12 minutes."
            this.ConfirmationDialogService.ShowInformationalModal(msg, "Training in progress");
            break;
          case "Already Running":
            msg = "Request to (Re)Generate list submitted, but training is <strong>already running</strong>. <br />"
              + "Ordinarily, re-training takes between 6 and 12 minutes."
            this.ConfirmationDialogService.ShowInformationalModal(msg, "Training in progress");
            break;
          case "Done (Random List)":
            if (this.EditingRevInfo.screeningMode == "Random") {
              msg = "Request to (Re)Generate list submitted and <strong>completed already</strong>.";
            }
            else {
              msg = "Request to (Re)Generate list submitted and <strong>completed already</strong>.<br />"
                + "The current list is a <strong>Random List</strong> as there isn't enough data for training a Priority Screening model. <br />"
                + "EPPI Reviewer will automatically produce a Prioritised list when enough data becomes available.";
            }
            this.ConfirmationDialogService.ShowInformationalModal(msg, "List updated");
            break;

        }
      }
    }
  }
  SaveOptions() {
    this.ReviewInfoService.Update(this.EditingRevInfo);
    this.AllowEditOnStep4 = false;
    this.CancelEditingAllOptions();
  }
  async SaveOptionsAndCreateList() {
    this.AllowEditOnStep4 = false;
    let res: boolean = await this.ReviewInfoService.Update(this.EditingRevInfo);
    if (res) {
      this.GenerateList();
      this.AllowEditOnStep4 = false;
      this.CancelEditingAllOptions();
      this.CurrentStep = 5;
      //this.Cancel();
    }
  }
  async SaveOptionsAndGoToStep5() {
    let res: boolean = await this.ReviewInfoService.Update(this.EditingRevInfo);
    if (res) {
      this.AllowEditOnStep4 = false;
      this.CancelEditingAllOptions();
      this.GoToAllinOneStep();
    }
  }
  async SaveOptionsAndCreate_FS_List() {
    if (!this.CanWrite()
      || this.EditingRevInfo.screeningCodeSetId < 1
      || this.SearchToUseForFromSearchList == null
      || this.SearchToUseForFromSearchList.searchId < 1
    ) return;
    let IsNew: boolean = true;
    if (this.ReviewInfoService.ReviewInfo.screeningFSListSearchId == this.SearchToUseForFromSearchList.searchId) IsNew = false;
    let res: boolean | string = await this.ReviewInfoService.Update(this.EditingRevInfo);
    if (res == true) {
      res = await this.PriorityScreeningService.RunNewFromSearchCommand(0, this.EditingRevInfo.screeningCodeSetId, IsNew, this.SearchToUseForFromSearchList.searchId);
      if (IsNew && this.ReviewInfoService.ReviewInfo.screeningFromSearchListIsGood == false) {
        //it's possible that we now have a screeningFromSearchList so we need to do the work of re-fetching the ReviewInfo
        this.ReviewInfoService.Fetch();
      }
      this.AllowEditOnStep4 = false;
      this.CancelEditingAllOptions();
      this.GoToAllinOneStep();
      let counter: number = 0;
      if (!this.tabstripScreening) {
        while (!this.tabstripScreening && counter < 20) {
          //console.log("Wait for tabstripScreening", counter);
          await Helpers.Sleep(25);
          counter++;
        }
      }
      if (this.tabstripScreening) {
        this.tabstripScreening.selectTab(1);
      }
    }
  }
  async ReGenerateFsList() {
    if (!this.CanWrite()
      || this.EditingRevInfo.screeningCodeSetId < 1
      || this.EditingRevInfo.screeningFSListSearchId < 1
    ) return;
    let res = await this.PriorityScreeningService.RunNewFromSearchCommand(0, this.EditingRevInfo.screeningCodeSetId, false, this.EditingRevInfo.screeningFSListSearchId);
    if (res == true) {
      //false: error happened, shown in the service
      //true: it worked
      this.notificationService.show({
        content: 'Progress in the "from search" queue has been updated',
        animation: { type: 'slide', duration: 400 },
        position: { horizontal: 'center', vertical: 'top' },
        type: { style: "info", icon: true },
        hideAfter: 5000
      });
      if (this.AllowEditOnStep4fs == true && this.CurrentStep == 5) {
        this.CancelEditingAllOptions();
      }
    }
    else if (res != false) {
      this.notificationService.show({
        content: 'Update request returned with error: ' + res,
        animation: { type: 'slide', duration: 400 },
        position: { horizontal: 'center', vertical: 'top' },
        type: { style: "error", icon: true },
        closable: true
      });
    }
  }

  async DeleteFSlist() {
    if (!this.CanWrite()
    ) return;
    this.EditingRevInfo.screeningFSListSearchId = 0;
    let res: string | boolean = await this.ReviewInfoService.Update(this.EditingRevInfo);
    if (res) {
      res = await this.PriorityScreeningService.DeleteFromSearchList();
      if (res == true) {
        //false: error happened, shown in the service
        //true: it worked
        //string: we have something to show from the component
        this.notificationService.show({
          content: 'Current "From Search" list deleted. Progress records are unaffected.',
          animation: { type: 'slide', duration: 400 },
          position: { horizontal: 'center', vertical: 'top' },
          type: { style: "info", icon: true },
          hideAfter: 5000
        });
      }
      else if (res != false) {
        this.notificationService.show({
          content: 'Request failed with error: ' + res,
          animation: { type: 'slide', duration: 400 },
          position: { horizontal: 'center', vertical: 'top' },
          type: { style: "error", icon: true },
          closable: true
        });
        
      }
    }
    this.CancelEditingAllOptions();
    this.RefreshAll();
  }
  public rowCallback = (context: RowClassArgs) => {
    const row = context.dataItem as iScreeningFromSearchIteration;
    if ( this.TrainingFromSearchList.CurrentRun.Iterations.findIndex(f=> f === row) != -1) {
      return { 'RowShowingCurrentRun': true,};
    } else {
      return { 'RowShowingCurrentRun': false, };
    }
  };


  ChangeFSprogressFilter(event: Event) {
    const Id = parseInt((event.target as HTMLOptionElement).value);
    this.TrainingFromSearchList.SetCurrentRun(Id);
  }
  FilterBySearchText(SearchId: number): string {
    const ind = this.SearchesWithScores.findIndex(f => f.searchId == SearchId);
    if (ind == -1) return "Unknown/deleted search (Id:" + SearchId + ")";
    else return this.SearchesWithScores[ind].title + " (#" + this.SearchesWithScores[ind].searchNo + ")";
  }

  RefreshAll() {
    this.PriorityScreeningService.Fetch().then(() => this.PriorityScreeningService.FetchTrainingFromSearchList());
    this.PriorityScreeningService.GetTrainingScreeningCriteriaList();
    this.ReviewInfoService.Fetch().then(() => { this.PrepareSearchesList(); });
  }
  async CheckIfCancelEditAllOptions(Origin:string) {
    await Helpers.Sleep(80);
    //console.log("CheckIfCancelEditAllOptions:", this.AllowEditOnStep4);
    if (Origin == "PS" && this.AllowEditOnStep4 == false) this.CancelEditingAllOptions();
    if (Origin == "FS" && this.AllowEditOnStep4fs == false) this.CancelEditingAllOptions();
  }
  CancelEditingAllOptions() {
    this.AllowEditOnStep4 = false;
    this.AllowEditOnStep4fs = false;
    this.ResetAllEditFromValues();
    this.DoRefreshRevinfo();
    this.innerSetCurrentSearchFS();
  }
  ResetAllEditFromValues() {
    this.ScreenAllItems = true;
    this._ItemsWithThisAttribute = null;
    this.selectedCodeSetDropDown = null;
    this.isCollapsedAllocateOptions = false;
    this.isCollapsedDropdownAddTrainingCriteria = false;
    this.DropdownAddTrainingCriteriaSelectedCode = null;
    this.DropdownWithWithoutSelectedCode = null;
    this.SearchToUseForFromSearchList = null;
  }


  CheckAndUpdatePeoplePerItem(): boolean {
    //console.log("CheckAndUpdatePeoplePerItem");
    //returns true if OK, but will automatically set to 0 Npeople, if screening set is in normal mode...
    if (this.EditingRevInfo.screeningCodeSetId < 1 || this.selectedCodeSetDropDown == null) {
      return true;//nothing is set, so nothing to check
    }
    else if (this.selectedCodeSetDropDown.codingIsFinal) {
      if (this.EditingRevInfo.screeningReconcilliation != "Single") this.EditingRevInfo.screeningReconcilliation = "Single";
      if (this.EditingRevInfo.screeningNPeople >= 1) {
        //set is in normal data entry, so we'll automatically set screeningNPeople to 0
        if (this.EditingRevInfo.screeningNPeople == 1) {
          this.ngZone.run(() => setTimeout(() => {
            //console.log("screeningNPeople goes to => 0");
            this.EditingRevInfo.screeningNPeople = 0;
          }, 8));
        }
        return true;//true because we fixed it...
      }
      else {//finishing all other options, if set is in normal mode.
        return true;
      }
    }
    else {//set must be in comparison mode
      if (this.EditingRevInfo.screeningReconcilliation == "Single" || this.EditingRevInfo.screeningReconcilliation == "") {
        this.EditingRevInfo.screeningReconcilliation = "no compl";
      }
      if (this.EditingRevInfo.screeningNPeople > 1) return true;
      else {//multiple screening, but n of people 1 or less
        if (this.EditingRevInfo.screeningNPeople == 0) {
          this.ngZone.run(() => setTimeout(() => {
            //console.log("screeningNPeople goes to => 1");
            this.EditingRevInfo.screeningNPeople = 1;
          }, 8));//just avoiding to have a 0 here...
        }//we delay this so to let Angular UI to catch up...
        return false;//multiple screening, but n of people is 1...
      }
    }
  }
  SetReconciliationMode(event: Event) {
    let mode = (event.target as HTMLOptionElement).value;
    //console.log("SetReconciliationMode", mode);
    this.EditingRevInfo.screeningReconcilliation = mode;
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

  SelectThisSearch(s: Search) {
    //console.log("selecting search:", s.selected);
    const list = this.SearchesWithScores;
    const todo = s.selected;
    if (todo == true && this.SearchToUseForFromSearchList && s.searchId == this.SearchToUseForFromSearchList.searchId) {
      //we're un-selecting the currently selected search, which isn't allowed.
      return;
    }
    for (let SS of list) { SS.selected = false; }
    s.selected = !todo;
    if (s.selected == true) {
      //console.log("selecting search2:", s);
      this.SearchToUseForFromSearchList = s;
      this.EditingRevInfo.screeningFSListSearchId = s.searchId;
      this.EditingRevInfo.screeningFSListSearchName = s.title;
    } else {
      //console.log("selecting search3:", s);
      this.innerSetCurrentSearchFS();
    }

  }

  Cancel() {
    //console.log("cancel screening");
    this.emitterCancel.emit();
  }

  CancelEditTrainingTable() {
    this.ShowEditTrainingTable = false;
  }


  ngOnDestroy() {
    if (this.RevInfoSub) this.RevInfoSub.unsubscribe();
  }

}
export interface kvStringSelectFrom {
  key: string;
  value: string;
}


