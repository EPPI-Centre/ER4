import { Component, OnInit, Input, OnDestroy, ViewChild, Attribute, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { searchService, Search } from '../services/search.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { RowClassArgs, GridDataResult, SelectableSettings, SelectableMode, PageChangeEvent, DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { ReviewSetsService, ReviewSet, singleNode, SetAttribute } from '../services/ReviewSets.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ClassifierService, ClassifierModel } from '../services/classifier.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { SourcesService } from '../services/sources.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { Subscription } from 'rxjs';
import { ChartComponent } from '@progress/kendo-angular-charts';
import { saveAs } from '@progress/kendo-file-saver';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { Helpers } from '../helpers/HelperMethods';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import 'hammerjs';

@Component({
  selector: 'SearchComp',
  templateUrl: './SearchComp.component.html'
})

export class SearchComp implements OnInit, OnDestroy {


  constructor(private router: Router,
    private ReviewerIdentityServ: ReviewerIdentityService,
    private ItemListService: ItemListService,
    public _searchService: searchService,
    private _reviewSetsEditingServ: ReviewSetsEditingService,
    private _eventEmitter: EventEmitterService,
    private _reviewSetsService: ReviewSetsService,
    private classifierService: ClassifierService,
    private notificationService: NotificationService,
    private _sourcesService: SourcesService,
    private confirmationDialogService: ConfirmationDialogService,
    private _reviewInfoService: ReviewInfoService,
    public _reviewerIdentityServ: ReviewerIdentityService
  ) {

  }

  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;

  ngOnInit() {

    //console.log("SearchComp init:", this.InstanceId);
    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
      this.router.navigate(['home']);
    }
    else {

      this._reviewSetsService.selectedNode = null;
      //this.getMembers();
      //console.log(this.Contacts);
      this.clearSub = this._eventEmitter.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); })

      this._sourcesService.FetchSources();
    }
  }
  //getMembers() {

  //	if (!this._reviewInfoService.ReviewInfo || this._reviewInfoService.ReviewInfo.reviewId < 1) {
  //		this._reviewInfoService.Fetch();
  //	}
  //	this._reviewInfoService.FetchReviewMembers();

  //}

  public get Contacts(): Contact[] {
    return this._reviewInfoService.Contacts;
  }
  //private InstanceId: number = Math.random();
  public clearSub: Subscription | null = null;
  public modelNum: number = 0;
  public modelTitle: string = '';
  public ModelId = -1;
  public AttributeId = 0;
  public SourceId = 0;
  public selected?: ReadOnlySource;
  public NewSearchSection: boolean = false;
  public CheckScreeningSection: boolean = false;
  //public PriorityScreeningSection: boolean = false;
  public ModelSection: boolean = false;
  public ShowVisualiseSection: boolean = false;
  public modelResultsSection: boolean = false;
  public modelResultsAllReviewSection: boolean = false;
  public radioButtonApplyModelSection: boolean = false;
  public isCollapsed: boolean = false;
  public isCollapsedVisualise: boolean = false;
  public firstName: string = '';
  public modeModels: SelectableMode = 'single';
  public withCode: boolean = false;
  public attributeNames: string = '';
  public commaIDs: string = '';
  public email: string = '';
  public searchText: string = '';
  public searchTextModel: string = '';
  public CurrentDropdownSelectedCode: singleNode | null = null;
  public SearchForPeoplesModel: string = 'true';
  public LowerScoreThreshold: number = 50;
  public UpperScoreThreshold: number = 50;
  public ShowSources: boolean = false;
  public CurrentSearchNameEditing: boolean = false;
  public searchN: string = '';
  public searchId: string = 'N/A';
  public popUpTitle: string = '';
  public pageSizes: number[] = [10, 25, 50, 100, 200, 300];
  @Input() autoRefreshThreshold: number = 500;

  public get ClassifierServiceIsBusy(): boolean {
    return this.classifierService.IsBusy;
  }
  //bridge data properties/methods for the searches Telerik grid, now all hosted in searchService
  public get SearchList(): Search[] {
    return this._searchService.SearchList;
  }

  public get DataSourceSearches(): GridDataResult {
    //console.log("DataSourceSearches");
    return this._searchService.DataSourceSearches;
    
  }
  public get sortSearches(): SortDescriptor[] {
    return this._searchService.sortSearches;
  }
  public get initialTake(): number {
    if (this.state && this.state.take) return this.state.take;
    return 100;
  }
  public get state(): State {
    return this._searchService.state;
  }
  public dataStateChange(state: DataStateChangeEvent): void {
    this._searchService.dataStateChange(state);
  }

  public sortChangeSearches(sort: SortDescriptor[]): void {
    this._searchService.sortChangeSearches(sort);
  }
  //END of bridge data properties/methods for the searches Telerik grid, now all hosted in searchService


  public ContactChoice: Contact = new Contact();
  @Output() PleaseOpenTheCodes = new EventEmitter();
  @ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
  //@ViewChild('WithOrWithoutCodeSelectorVisualise') WithOrWithoutCodeSelectorVisualise!: codesetSelectorComponent;
  @ViewChild('VisualiseChart')
  private VisualiseChart!: ChartComponent;
  public get selectedNode(): singleNode | null {
    return this._reviewSetsService.selectedNode;
  }
  public selectableSettings: SelectableSettings = {
    checkboxOnly: true,
    mode: 'single'
  };
  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }

  public get isSearchServiceBusy(): boolean {
    return this._searchService.IsBusy;
  }

  public get ReviewSources(): ReadOnlySource[] {
    return this._sourcesService.ReviewSources;
  }
  public setSelectableSettings(): void {

    this.selectableSettings = {
      checkboxOnly: true,
      mode: 'single'
    };
  }
  public get SearchVisualiseData() {
    return this._reviewSetsEditingServ.SearchVisualiseData;
  }


  CancelVisualise() {

    this.ShowVisualiseSection = false;
  }
  public exportChart(): void {

    this.VisualiseChart.exportImage().then((dataURI) => {
      saveAs(dataURI, 'chart.png');
    });

  }
  public FindItemsScoring(): void {

    if (this.CanWrite()) {

      this._searchService.cmdSearches._scoreOne = this.LowerScoreThreshold;
      this._searchService.cmdSearches._scoreTwo = this.UpperScoreThreshold;
      this._searchService.cmdSearches._searchType = this._searchMTLTB;
      if (this.visualiseSearchId > 0) {
        this._searchService.cmdSearches._searchId = this.visualiseSearchId;
      } else {
        this.notificationService.show({
          content: 'Null search Id',
          animation: { type: 'slide', duration: 400 },
          position: { horizontal: 'center', vertical: 'top' },
          type: { style: "warning", icon: true },
          closable: true,
          hideAfter: 3000
        });
        return;
      }
      let searchText: string = '';
      if (this._searchMTLTB === 'More') {
        searchText = "Search #" + this._searchService.cmdSearches._searchId + " scores more than " +
          this.LowerScoreThreshold;
      } else if (this._searchMTLTB === 'Less') {
        searchText = "Search #" + this._searchService.cmdSearches._searchId + " scores less than " +
          this.LowerScoreThreshold;
      } else if (this._searchMTLTB === 'Between') {
        searchText = "Search #" + this._searchService.cmdSearches._searchId + " scores is between " +
          this.LowerScoreThreshold + " and " + this.UpperScoreThreshold;
        if (this.UpperScoreThreshold <= this.LowerScoreThreshold) {
          this.notificationService.show({
            content: 'Second score must be higher than the first',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "warning", icon: true },
            closable: true,
            hideAfter: 3000
          });
          return;
        }
      } else {
        this.notificationService.show({
          content: 'Null search type',
          animation: { type: 'slide', duration: 400 },
          position: { horizontal: 'center', vertical: 'top' },
          type: { style: "warning", icon: true },
          closable: true,
          hideAfter: 3000
        });
        return;
      }

      this._searchService.cmdSearches._searchText = searchText;
      this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchClassifierScores');
      this.notificationService.show({
        content: 'New search with classification scores created below',
        animation: { type: 'slide', duration: 400 },
        position: { horizontal: 'center', vertical: 'top' },
        type: { style: "info", icon: true },
        closable: true,
        hideAfter: 3000
      });
    }

  }
  public HideManuallyCreatedItems(ROS: ReadOnlySource): boolean {
    if (ROS.source_Name == 'NN_SOURCELESS_NN' && ROS.source_ID == -1) return true;
    else return false;
  }
  public modelIsInProgress: boolean = false;
  public selectedRows(e: any) {

    if (e.selectedRows[0] != undefined && (this.modelNum == 7 || this.modelNum == 8)) {

      //console.log("selected:", e.selectedRows[0].dataItem);

      this.ModelSelected = true;
      this.modelTitle = e.selectedRows[0].dataItem.modelTitle;
      this.ModelId = e.selectedRows[0].dataItem.modelId;
      if (this.modelTitle.indexOf('(in progress...)') != -1 || (this.modelTitle.indexOf('fail') != -1 && e.selectedRows[0].dataItem.precision <= 0)) {
        this.modelIsInProgress = true;
        //alert('model is in progress');
      } else {
        this.modelIsInProgress = false;
      }


    } else {

      //console.log("selected:", e.selectedRows);
      this.modelTitle = '';
      this.ModelId = 0;
      this.ModelSelected = false;

      //alert('model has been DESELECTED that is in fact a custom model');
    }
  }
  public get DataSourceModel(): GridDataResult {
    return {
      data: orderBy(this.classifierService.ClassifierModelCurrentReviewList, this.sortCustomModel),
      total: this.classifierService.ClassifierModelCurrentReviewList.length,
    };
  }
  public get DataSourceModelAllReviews(): GridDataResult {
    return {

      data: orderBy(this.classifierService.ClassifierContactAllModelList, this.sortCustomModel),
      total: this.classifierService.ClassifierContactAllModelList.length,
    };
  }

  CombineSearches() {
    alert("Not implemented!");
  }

  public mode: string = '1';

  NewSearch() {

    this.mode = '1';
    this.NewSearchSection = !this.NewSearchSection;
    this.ModelSection = false;
    this.modelResultsSection = false;
    this.radioButtonApplyModelSection = false;
    this.ShowVisualiseSection = false;
    this.CheckScreeningSection = false;
    //this.PriorityScreeningSection = false;
    this._searchService.cmdSearches._searchWhat = "";
    this._searchService.cmdSearches._sourceIds = "";
    this._searchService.cmdSearches._title = "";
    this.ShowSources = false;
  }
  CloseCodeDropDown() {
    if (this.WithOrWithoutCodeSelector) {
      //console.log("yes, doing it", this.WithOrWithoutCodeSelector.SelectedNodeData);
      this.CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData;
    }
    this.isCollapsed = false;
  }
  //CloseCodeVisualiseDropDown() {

  //	if (this.WithOrWithoutCodeSelectorVisualise) {
  //		//console.log("yes, doing it", this.WithOrWithoutCodeSelectorVisualise.SelectedNodeData);
  //		this.CurrentDropdownVisualiseSelectedCode = this.WithOrWithoutCodeSelectorVisualise.SelectedNodeData;
  //	}
  //	this.isCollapsedVisualise = false;

  //}

  Classify() {

    this.classifierService.FetchClassifierContactModelList(this.ReviewerIdentityServ.reviewerIdentity.userId);
    this._reviewSetsService.selectedNode = null;
    this.NewSearchSection = false;
    this.CheckScreeningSection = false;
    //this.PriorityScreeningSection = false;
    this.ModelSection = !this.ModelSection;
    this.modelResultsSection = false;
    this.modelResultsAllReviewSection = false;
    this.radioButtonApplyModelSection = true;
    this.ShowVisualiseSection = false;
  }

  OpenCheckScreening() {
    this.CheckScreeningSection = !this.CheckScreeningSection;
    this.ModelSection = false;
    this.NewSearchSection = false;
    //this.PriorityScreeningSection = false;
  }

  OpenPriorityScreening() {
    this.router.navigate(['PriorityScreeningSim']);
  }

  CanCreateClassifierCodes(): boolean {
    // logic for enabling visualise button
    if (this.selectedNode == null) return false;
    else {
      if (this.selectedNode.nodeType == "ReviewSet") {
        let Set = this.selectedNode as ReviewSet;
        if (!Set) return false;
        else {
          if (Set.subTypeName == "Screening") return false;
          else return Set.allowEditingCodeset;
        }
      }
      else {
        return this._reviewSetsService.CurrentCodeCanHaveChildren;
      }
    }
  }

  CreateCodesBelow() {
    if (!this.CanCreateClassifierCodes()
      || !this.selectedNode) return;
    else {
      if (this.selectedNode.nodeType == 'ReviewSet') {

        let Set = this.selectedNode as ReviewSet;
        if (Set && Set.set_id > 0) {
          this._reviewSetsEditingServ.CreateVisualiseCodeSet(this.visualiseTitle, this.visualiseSearchId,
            0, Set.set_id
          ).then(
            () => {
              this._reviewSetsService.GetReviewSets();
            });
        }
        else return;

      } else {
        let Att = this.selectedNode as SetAttribute;
        if (Att && Att.attribute_id > 0 && Att.set_id > 0) {
          this._reviewSetsEditingServ.CreateVisualiseCodeSet(this.visualiseTitle, this.visualiseSearchId,
            Att.attribute_id, Att.set_id
          ).then(
            () => {
              this._reviewSetsService.GetReviewSets();
            });
        }
        else return;
      }
    }
  }

  CustomModels(modelNum: number) {
    if (this.modelTitle == '') {

      this.ModelSelected = false;
    }
    if (modelNum == 7) {
      this.modelNum = 7;
      this.modelResultsSection = !this.modelResultsSection;
      this.modelResultsAllReviewSection = false;
      this.ModelSelected = false;
    } else if (modelNum == 8) {
      this.modelNum = 8;
      this.modelResultsAllReviewSection = !this.modelResultsAllReviewSection;
      this.modelResultsSection = false;
      this.ModelSelected = false;
    }

  }
  SetModelSelection(num: number) {

    //alert('SelectedNode is: ' + this._reviewSetsService.selectedNode);
    this.modelNum = num;
    this.NewSearchSection = false;
    this.modelResultsSection = false;
    this.CheckScreeningSection = false;
    //this.PriorityScreeningSection = false;
    //alert('Model Number is: ' + this.modelNum);

  }
  //subscribe below

  public ApplyCode: boolean = false;
  public ApplyAll: boolean = true;
  public ApplySource: boolean = false;

  chooseAll() {
    this.ApplyCode = false;
    this.ApplyAll = true;
    this.ApplySource = false;

  }
  get CanApplySearch(): boolean {
    // Easy ones do not have a condition in the seacrhes DD
    // Without an abstract and without any documents uploaded
    if (this.selectedSearchDropDown == 'Without any documents uploaded') {
      return true;
    } else if (this.selectedSearchDropDown == 'Without an abstract') {
      return true;
    } else if (this.selectedSearchDropDown == 'With at least one document uploaded') {
      return true;
    } else if (this.selectedSearchDropDown == 'With linked references') {
      return true;
    } else if (this.selectedSearchDropDown == 'With duplicate references') {
      return true;
    }
    // Codes in set options next: ''
    else if (this.selectedSearchDropDown == 'That have at least one code from this Coding Tool'
      && this.selectedSearchCodeSetDropDown != '' && this.SearchForPeoplesModel == 'true') {
      return true;
    }
    else if (this.selectedSearchDropDown == 'That have at least one code from this Coding Tool'
      && this.selectedSearchCodeSetDropDown != '' && this.SearchForPeoplesModel == 'false' && this.ContactChoice.contactId > 0) {
      return true;
    }
    else if (this.selectedSearchDropDown == "That don't have any codes from this Coding Tool" && this.selectedSearchCodeSetDropDown != '') {
      return true;
    }// hard ones based on code selected from tree first : CurrentDropdownSelectedCode
    else if (this.selectedSearchDropDown == 'With this code' && this.CurrentDropdownSelectedCode != null && this.CurrentDropdownSelectedCode != undefined) {
      return true;
    }
    else if (this.selectedSearchDropDown == 'Without this code' && this.CurrentDropdownSelectedCode != null && this.CurrentDropdownSelectedCode != undefined) {
      return true;
    }
    else if (this.selectedSearchDropDown == 'With these internal IDs (comma separated)' && this.commaIDs != '') {
      return true;
    }
    else if (this.selectedSearchDropDown == 'With these imported IDs (comma separated)' && this.commaIDs != '') {
      return true;
    }
    else if (this.selectedSearchDropDown == 'Containing this text' && this.searchText != '' && this.selectedSearchTextDropDown != '') {
      return true;
    } else if (this.selectedSearchDropDown == 'From source(s)' &&
      this._sourcesService.ReviewSources.filter(x => x.isSelected == true).length > 0 &&
      this._searchService.selectedSourceDropDown.length > 0) {
      return true;
    }

    return false;
    // 
  }
  CanApplyModel(): boolean {
    if (this.modelIsInProgress || this.classifierService.IsBusy) return false;
    else if (this.modelNum == 7 && this.ModelSelected && this.ApplySource && this.selected != null) {
      return true;
    }
    else if (this.modelNum == 7 && this.ModelSelected && this.ApplyCode && this._reviewSetsService.selectedNode != null && this._reviewSetsService.selectedNode.nodeType == 'SetAttribute') {
      //alert('custom models');
      return true;
    }
    else if (this.modelNum == 7 && this.ModelSelected && this.ApplyAll) {
      //alert('custom models');
      return true;
    }
    else if (this.modelNum == 8 && this.ModelSelected && this.ApplySource && this.selected != null) {
      return true;
    }
    else if (this.modelNum == 8 && this.ModelSelected && this.ApplyCode && this._reviewSetsService.selectedNode != null && this._reviewSetsService.selectedNode.nodeType == 'SetAttribute') {
      //alert('custom models');
      return true;
    }
    else if (this.modelNum == 8 && this.ModelSelected && this.ApplyAll) {
      //alert('custom models');
      return true;
    }
    else if ((this.modelNum < 7 || this.modelNum == 9) && this.modelNum != 0) {
      if (this.ApplyCode && this._reviewSetsService.selectedNode != null && this._reviewSetsService.selectedNode.nodeType == 'SetAttribute') {
        return true;
      } else if (this.ApplySource && this.selected != null) {
        return true;
      } else if (this.ApplyAll) {
        //console.log('yes step 2');
        return true;
      }
    }
    return false;
  }

  private hideAfter: number = 900;
  chooseCodeMessage() {

    if (this.ApplyCode == false) {

      this.ApplyCode = true;
      this.ApplyAll = false;
      this.ApplySource = false;
      //this.notificationService.show({
      //	content: 'Please use the tree on the right hand side to choose a code',
      //	animation: { type: 'fade', duration: 100 },
      //	position: { horizontal: 'center', vertical: 'top' },
      //	type: { style: "warning", icon: true },
      //	closable: true,
      //	hideAfter: this.hideAfter
      //});
    }
  }
  chooseSourceDD() {

    this._sourcesService.FetchSources();
    this.ApplyCode = false;
    this.ApplyAll = false;
    this.ApplySource = true;
    //If only required once this is okay; else we are repeating ViewModel code across the app
    this._reviewSetsService.selectedNode = null;
    //
    //console.log('Blah: ' + this._ReviewSources.values);
  }
  DisplayFriendlySourceNames(sourceItem: ReadOnlySource): string {

    if (this.HideManuallyCreatedItems(sourceItem)) {
      return 'Manually Created Source';
    }
    return sourceItem.source_Name;
  }



  public openConfirmationDialog() {
    this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to run the selected model ?', false, '')
      .then(
        (confirmed: any) => {
          //console.log('User confirmed:', confirmed);
          if (confirmed) {
            this.RunModel();
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
  }

  public openRebuildConfirmationDialog(model: ClassifierModel) {
    this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to rebuild this model ?', false, '')
      .then(
        (confirmed: any) => {
          //console.log('User confirmed:', confirmed);
          if (confirmed) {
            this.RebuildModel(model);
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
  }

  

  hasError(searchText: string) {

    //alert(searchText);
    if (searchText.trim() == '') {
      return true;
    } else {
      return false;
    }


  }

  async RunModel() {
    if (this.CanWrite()) {
      this.AttributeId = -1;
      this.SourceId = -2;

      if (this.mode == '1') {
        // standard
      } else if (this.mode == '2') {
        //then set the attributeid to begin with
        this.AttributeId = this._reviewSetsService.selectedNode ? Number(this._reviewSetsService.selectedNode.id.substr(1, this._reviewSetsService.selectedNode.id.length - 1)) : -1;
      } else if (this.mode == '3') {
        this.SourceId = Number(this.selected);
      } else {
        alert('You must apply the model to some items');
        return;
      }
      if (this.modelNum == 1) {
        this.modelTitle = 'RCT';
        this.ModelId = -1;
      } else if (this.modelNum == 2) {
        this.modelTitle = 'Systematic review';
        this.ModelId = -2;
      } else if (this.modelNum == 3) {
        this.modelTitle = 'Economic evaluation';
        this.ModelId = -3;
      } else if (this.modelNum == 4) {
        this.modelTitle = 'New Cochrane RCT classifier model';
        this.ModelId = -4;
      } else if (this.modelNum == 5) {
        this.modelTitle = 'COVID-19 map categories';
        this.ModelId = -5;
      } else if (this.modelNum == 6) {
        this.modelTitle = 'Long COVID binary model';
        this.ModelId = -6;
      } else if (this.modelNum == 9) {
        this.modelTitle = 'PubMed study designs';
        this.ModelId = -9;
      } else {
        //return;
      }
      //alert(this.modelTitle + ' ModelTitle ' + this.AttributeId + ' ATTID ' + this.ModelId + ' MODELID ' + this.SourceId + ' sourceID ');
      let res = await this.classifierService.Apply(this.modelTitle, this.AttributeId, this.ModelId, this.SourceId);

      if (res != false) {//we get "false" if an error happened...
        if (res == "Successful upload of data" || res == "The data will be submitted and scored. Please monitor the list of search results for output.") {
          this.notificationService.show({
            content: 'Job Submitted. Results will appear as search results (please refresh them in a few minutes)',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true,
            hideAfter: 3000
          });
        }
        else if (res == "") {
          this.notificationService.show({
            content: 'Job Submitted, but returned no status. Results might appear as search results (please refresh them in a few minutes), otherwise try again',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "warning", icon: true },
            closable: true,
            hideAfter: 3000
          });
        }
        else if ((res as string).startsWith("Error")) {
          this.notificationService.show({
            content: 'Job Submitted, but returned an error. Please try again, if it happens again, please contact EPPISupport.',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "error", icon: true },
            closable: true,
            hideAfter: 3000
          });
        }
        else {
          this.notificationService.show({
            content: 'Job Submitted, with status: ' + res + '.',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "warning", icon: true },
            closable: true,
            hideAfter: 3000
          });
        }
        this.ModelSection = false;
      }
      else {
        //we don't show anything, because the error is handled in the service.
      }
    }
  }

  SelectModel(model: string) {
    this.ModelSelected = true;
    //alert('you selected model: ' + model);
  }

  async RebuildModel(model: ClassifierModel) {

    if (model.modelId > 0) {
      if (model.attributeIdOn !== undefined && model.attributeIdNotOn !== undefined) {
        await this.classifierService.CreateAsync(model.modelTitle, model.attributeIdOn, model.attributeIdNotOn, model.modelId);
      }
    }

  }

  public data: Array<any> = [{
    text: 'AND',
    click: () => {
      this.getLogicSearches('AND');
      //alert('AND');
    }
  }, {
    text: 'OR',
    click: () => {
      this.getLogicSearches('OR');
      //alert('OR');
    }
  }, {
    text: 'NOT',
    click: () => {
      this.getLogicSearches('NOT');
      //alert('NOT');
    }
  }, {
    text: 'NOT (excluded)',
    click: () => {
      this.getLogicSearches('NOT (excluded)');
      //alert('NOT (excluded)');
    }
  }];

  private _searchInclOrExcl: string = 'true';
  public ModelSelected: boolean = false;

  public get searchInclOrExcl(): string {

    this._searchService.cmdSearches._included = this._searchInclOrExcl;

    return this._searchInclOrExcl;
  }
  public set searchInclOrExcl(value: string) {

    this._searchService.cmdSearches._included = this._searchInclOrExcl;

    if (value == 'true' || value == 'false') this._searchInclOrExcl = value;
    //else console.log("I'm not doing it :-P ", value);

  }

  private _searchMTLTB: string = 'More';
  public scoreOne: number = 0;
  public scoreTwo: number = 0;

  public get searchMTLTB(): string {

    this._searchService.cmdSearches._searchType = this._searchMTLTB;

    return this._searchMTLTB;
  }
  public set searchMTLTB(value: string) {

    this._searchService.cmdSearches._searchType = this._searchMTLTB;

    if (value == 'More' || value == 'Less' || value == 'Between') this._searchMTLTB = value;
    //else console.log("I'm not doing it :-P ", value);

  }

  private _logic: string = '';

  public get logic(): string {

    return this._logic;
  }

  getLogicSearches(logicChoice: string) {


    if (this.CanWrite() && this.HasSelectedSearches > 0) {

      if (logicChoice == 'NOT (excluded)') {
        this._searchService.cmdSearches._included = 'false';
        logicChoice = 'NOT';

      } else {
        this._searchService.cmdSearches._included = 'true';
      }

      let lstStrSearchIds = '';
      let lstStrSearchNos = logicChoice;
      //alert(logicChoice);
      for (var i = 0; i < this.DataSourceSearches.data.length; i++) {

        if (this.DataSourceSearches.data[i].add == true) {
          if (lstStrSearchIds == "") {
            lstStrSearchIds = this.DataSourceSearches.data[i].searchId;
            if (logicChoice == 'NOT' || logicChoice == 'NOT(excluded)') {
              lstStrSearchNos = logicChoice + ' ' + this.DataSourceSearches.data[i].searchNo;
            } else {
              lstStrSearchNos = this.DataSourceSearches.data[i].searchNo;
            }

          } else {
            lstStrSearchIds += ',' + this.DataSourceSearches.data[i].searchId;
            lstStrSearchNos += ' ' + logicChoice + ' ' + this.DataSourceSearches.data[i].searchNo;
          }
        }
      }
      this._searchService.cmdSearches._title = lstStrSearchNos;

      this._searchService.cmdSearches._logicType = logicChoice;
      this._searchService.cmdSearches._searches = lstStrSearchIds;
      if (this._searchService.cmdSearches._logicType != '') {

        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchCodeLogic');
        //reset
        this._searchService.cmdSearches._logicType = '';

      }

    }
  }

  public dropDownList: any = null;
  public showTextBox: boolean = false;
  public selectedSearchDropDown: string = 'With this code';
  public selectedSearchTextDropDown: string = '';
  public selectedSearchCodeSetDropDown: string = '';
  public CodeSets: any[] = [];

  CanWrite(): boolean {

    if (this.ReviewerIdentityServ.HasWriteRights && !this._searchService.IsBusy) {

      return true;
    } else {

      return false;
    }
  }

  public selectedAll: boolean = false;
  public modalClass: boolean = false;

  allSearchesSelected: boolean = false;
  allModelsSelected: boolean = false;

  stateAdd: State = {
    // will hold grid state
    skip: 0,
    take: 2
  };





  refreshSearches() {
    this._searchService.Fetch();
  }

  refreshModels() {
    this.classifierService.FetchClassifierContactModelList(this.ReviewerIdentityServ.reviewerIdentity.userId);
  }

  public SearchForPersonModel: boolean = false;
  public SearchForPersonDropDown: string = 'true';
  SelectPerson(event: string) {

    if (event == 'true') {
      this.SearchForPersonModel = true;

    } else {
      this.ContactChoice = new Contact();
      this.SearchForPersonModel = false;
    }
  }

  public get HasSelectedSearches(): number { //0 = nothing selected, 1 = some selected, 2 all searches in page are selected
    //console.log("HasSelectedSearches", this.DataSourceSearches.data);
    const list = this.DataSourceSearches.data as Search[];
    const found = list.filter(f => f.add == true);
    if (found.length > 0) {
      if (found.length < list.length) return 1;
      else return 2;
    }
    else return 0;
  }

  public openConfirmationDialogDeleteSearches() {
    const selectedS = (this.DataSourceSearches.data as Search[]).filter(f => f.add == true);
    if (selectedS.length > 0) {
      //this.ConfirmationDialogService.confirm("Assign selected ("
      //  + this.ItemListService.SelectedItems.length + ") items ? "
      //  , "Are you sure you want to assign all selected items (<strong>"
      //  + this.ItemListService.SelectedItems.length + "</strong>) to this code?<br>"
      //  + "<div class='w-100 p-0 mx-0 my-2 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
      //  + encoded + "</strong></div>"
      //  , false, '')
      let msg: string = 'Are you sure you want to delete the selected searches?<br />';
      if (selectedS.length == 1) {
        msg = "Are you sure you want to delete the selected search?<br />"
          + "This search contains: <strong>" + selectedS[0].hitsNo.toString() + " results</strong>."
      }
      else {
        msg += "You will delete <strong>" + selectedS.length.toString() + " searches</strong>, ";
        let tot = selectedS.reduce((accumulator, currentValue) => accumulator + currentValue.hitsNo, 0);
        msg += "comprising of a total of (up to) <strong>" + tot.toString() + " results.";
      }

      this.confirmationDialogService.confirm('Please confirm', msg, false, '')
          .then(
            (confirmed: any) => {
              //console.log('User confirmed:', confirmed);
              if (confirmed) {
                this.DeleteSearchSelected(selectedS);
              } else {
                //alert('did not confirm');
              }
            }
          )
          .catch(() => { });
    }
  }
  private DeleteSearchSelected(selected: Search[]) {
    if (selected.length == 0 || !this.HasWriteRights) return;
    // Need to check if user has rights to delete
    let lstStrSearchIds = '';

    for (var i = 0; i < selected.length; i++) {
      if (selected[i].add == true) {
        lstStrSearchIds += selected[i].searchId + ',';
      }
    }
    //console.log(lstStrSearchIds, lstStrSearchIds.substring(0, lstStrSearchIds.length - 1));
    if (lstStrSearchIds.length > 1) {
      this._searchService.Delete(lstStrSearchIds.substring(0, lstStrSearchIds.length - 1));
    }
  }
  public convertToIncEx(incEx: boolean): string {

    if (incEx == true) {
      return 'included';
    } else {
      return 'excluded'
    }

  }

  callSearches(selectedSearchDropDown: string, selectedSearchTextDropDown: string, searchBool: boolean) {

    if (this.CanWrite()) {
      this.selectedSearchTextDropDown = selectedSearchTextDropDown;
      let searchTitle: string = '';
      let firstNum: boolean = selectedSearchDropDown.search('With this code') != -1;
      let secNum: boolean = selectedSearchDropDown.search('Without this code') != -1
      this._searchService.cmdSearches._included = String(searchBool);

      this._searchService.cmdSearches._withCodes = String(this.withCode);
      this._searchService.cmdSearches._searchId = 0;

      if (firstNum == true || secNum == true) {

        if (firstNum) {

          this.withCode = true;
        } else {

          this.withCode = false;
        }

        if (this.CurrentDropdownSelectedCode != undefined) {

          let tmpID: number = this.CurrentDropdownSelectedCode.attributeSetId;
          this.attributeNames = this.CurrentDropdownSelectedCode.name;
          this._searchService.cmdSearches._answers = String(tmpID);
          //alert(this.CurrentDropdownSelectedCode);

          searchTitle = this.withCode == true ?
            "Coded with: " + this.attributeNames : "Not coded with: " + this.attributeNames;


          this._searchService.cmdSearches._title = searchTitle;
          this._searchService.cmdSearches._withCodes = String(this.withCode);

          this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchCodes');

        }
      }

      if (selectedSearchDropDown == 'With these internal IDs (comma separated)') {

        this._searchService.cmdSearches._IDs = this.commaIDs;
        this._searchService.cmdSearches._title = this.commaIDs;
        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchIDs');


      }
      if (selectedSearchDropDown == 'With these imported IDs (comma separated)') {

        this._searchService.cmdSearches._IDs = this.commaIDs;
        this._searchService.cmdSearches._title = this.commaIDs;

        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchImportedIDs');


      }
      if (selectedSearchDropDown == 'Containing this text') {

        let tmpStr: string = '';

        if (selectedSearchTextDropDown == 'Title and abstract') {
          tmpStr = 'TitleAbstract'
        } else if (selectedSearchTextDropDown == 'Title only') {
          tmpStr = 'Title'
        } else if (selectedSearchTextDropDown == 'Abstract only') {
          tmpStr = 'Abstract'
        } else if (selectedSearchTextDropDown == 'Additional text') {
          tmpStr = 'AdditionalText'
        } else if (selectedSearchTextDropDown == 'Uploaded documents') {
          tmpStr = 'UploadedDocs'
        } else if (selectedSearchTextDropDown == 'Authors') {
          tmpStr = 'Authors'
        } else if (selectedSearchTextDropDown == 'Publication year') {
          tmpStr = 'PubYear'
        }
        this._searchService.cmdSearches._searchText = tmpStr;

        this._searchService.cmdSearches._title = this.searchText;

        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchText');
      }
      if (selectedSearchDropDown == 'That have at least one code from this Coding Tool') {

        this._searchService.cmdSearches._withCodes = 'true';
        this._searchService.cmdSearches._title = this.selectedSearchCodeSetDropDown;
        if (this.SearchForPersonModel === false) {
          this._searchService.cmdSearches._contactId = 0;
          this._searchService.cmdSearches._contactName = "";
        }
        else {
          this._searchService.cmdSearches._contactId = this.ContactChoice.contactId;
          this._searchService.cmdSearches._contactName = this.ContactChoice.contactName;
        }
        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchCodeSetCheck');

      }
      if (selectedSearchDropDown == "That don't have any codes from this Coding Tool") {

        this._searchService.cmdSearches._withCodes = 'false';
        this._searchService.cmdSearches._title = this.selectedSearchCodeSetDropDown;
        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchCodeSetCheck');

      }
      if (selectedSearchDropDown == 'Without an abstract') {

        //alert(selectedSearchDropDown);
        this._searchService.cmdSearches._title = searchTitle;

        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchNoAbstract');

      }

      if (selectedSearchDropDown == 'Without any documents uploaded') {

        //alert(selectedSearchDropDown);
        this._searchService.cmdSearches._title = 'Without any documents uploaded';

        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchNoFiles');

      }
      if (selectedSearchDropDown == 'With at least one document uploaded') {

        this._searchService.cmdSearches._title = 'With at least one document uploaded.';
        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchOneFile');

      }
      if (selectedSearchDropDown == 'From source(s)') {
        if (this.MakeSearchBySourceName()) this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchSources');
      }
      if (selectedSearchDropDown == 'With linked references') {
        this._searchService.cmdSearches._title = 'With linked references';
        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchWithLinkedReferences');
      }
      if (selectedSearchDropDown == 'With duplicate references') {
        this._searchService.cmdSearches._title = 'With duplicate references';
        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchWithDuplicateReferences');
      }

    }
  }
  MakeSearchBySourceName(): boolean {
    //this._searchService.selectedSourceDropDown = val;
    let NameSt: string = "";
    let ids: string[] = this._sourcesService.ReviewSources.filter(x => x.isSelected == true).map<string>(y => y.source_ID.toString());
    if (ids.length == 0) {
      return false;//nothing to do, can't search on sources without a selected source...
    }
    else if (ids.length == 1) {
      NameSt = "Source search, Id:" + ids[0] + " - ";
    }
    else if (ids.length > 1 && ids.length < 4) {
      //selected multiple sources
      NameSt = "Sources search  (IDs: " + ids.join(',') + ") - ";
    }
    else {
      NameSt = "Sources search  (" + ids.length.toString() + " IDs: " + ids[0] + "," + ids[1] + "," + ids[2] + "...) - ";
    }
    this._searchService.cmdSearches._sourceIds = ids.join(',');

    switch (this._searchService.cmdSearches._searchWhat) {

      case "AllItems": {
        this._searchService.cmdSearches._title = NameSt + 'All items in source';
        //this._searchService.cmdSearches._searchWhat = "AllItems";
        break;
      }
      case "Included": {
        this._searchService.cmdSearches._title = NameSt + 'Only included';
        //this._searchService.cmdSearches._searchWhat = "Included";
        break;
      }
      case "Excluded": {
        this._searchService.cmdSearches._title = NameSt + 'Only excluded';
        //this._searchService.cmdSearches._searchWhat = "Excluded";
        break;
      }
      case "Deleted": {
        this._searchService.cmdSearches._title = NameSt + 'Only deleted';
        //this._searchService.cmdSearches._searchWhat = "Deleted";
        break;
      }
      case "Duplicates": {
        this._searchService.cmdSearches._title = NameSt + 'Only duplicates';
        //this._searchService.cmdSearches._searchWhat = "Duplicates";
        break;
      }
      default:
        return false;
    }
    return true;
  }
  public ShowSearchForAnyone: boolean = false;
  public nextDropDownList(num: number, val: string) {

    this.ShowSearchForAnyone = false;
    this.SearchForPersonModel = false;
    this.SearchForPersonDropDown = 'true';
    let typeElement: "success" | "error" | "none" | "warning" | "info" | undefined = undefined;
    this.showTextBox = false;
    this.selectedSearchDropDown = val;
    this.ShowSources = false;
    switch (num) {

      case 1: {

        this.dropDownList = this._reviewSetsService.ReviewSets;

        break;
      }
      case 2: {
        this.dropDownList = this._reviewSetsService.ReviewSets;

        break;
      }
      case 3: {
        this.showTextBox = true;
        break;
      }
      case 4: {
        this.showTextBox = true;
        break;
      }
      case 5: {

        this.CodeSets = this._reviewSetsService.ReviewSets.filter(x => x.nodeType == 'ReviewSet')
          .map(
            (y: ReviewSet) => {
              return y.name;
            }
          );
        this.dropDownList = this._reviewSetsService.ReviewSets;
        this.ShowSearchForAnyone = true;
        this.SearchForPersonDropDown = 'true';
        this.SearchForPersonModel = false;
        break;
      }
      case 6: {
        this.CodeSets = this._reviewSetsService.ReviewSets.filter(x => x.nodeType == 'ReviewSet')
          .map(
            (y: ReviewSet) => {
              return y.name;
            }
          );
        this.dropDownList = this._reviewSetsService.ReviewSets;

        break;
      }
      case 11: {
        this.ShowSources = true;
        if (this.ReviewSources.length === 0) {
          this._sourcesService.FetchSources();
        }
        break;
      }

      default: {
        break;
      }
    }
  }

  public setSearchCodeSetDropDown(codeSetName: string) {

    this.selectedSearchCodeSetDropDown = this._reviewSetsService.ReviewSets.filter(x => x.name == codeSetName)
      .map(
        (y: ReviewSet) => {

          this._searchService.cmdSearches._setID = y.set_id;
          return y.name;
        }
      )[0];
  }

  public setSearchTextDropDown(heading: string) {

    this.selectedSearchTextDropDown = heading;
    this._searchService.cmdSearches._searchText = heading;
  }

  public checkboxClicked(dataItem: any) {
    //console.log(" checkboxClicked Before", dataItem.add);
    if (dataItem.add == undefined) dataItem.add = true;
    else dataItem.add = !dataItem.add;
    const t = this.DataSourceSearches;
    //console.log("after", dataItem.add, t);
  }
  public selectAllinPageClicked(currentState: number) {
    const list = this.DataSourceSearches.data as Search[];
    //const found = list.filter(f => f.add == true);
    let DestinationState: boolean = true;
    if (currentState == 2 ) {//the one case when we "unselect all"
      DestinationState = false;
    }
    for (const search of list) {
      search.add = DestinationState;
    }
  }


  BuildModel() {
    this.router.navigate(['BuildModel']);
  }


  public sortCustomModel: SortDescriptor[] = [{
    field: 'modelId',
    dir: 'desc'
  }];


  public visualiseTitle: string = '';
  public visualiseSearchId = 0;

  OpenClassifierVisualisation(search: Search) {


    //console.log(JSON.stringify(search.title));
    this.NewSearchSection = false;
    this.ModelSection = false;
    this.visualiseTitle = search.title;
    this.visualiseSearchId = search.searchId;
    this.CheckScreeningSection = false;
    //this.PriorityScreeningSection = false;
    //console.log(JSON.stringify(search));
    this._reviewSetsEditingServ.CreateVisualiseData(search.searchId);
    this.PleaseOpenTheCodes.emit();
    //alert('in here' + JSON.stringify(this.SearchVisualiseData));
    // for now just show the graph area unhidden
    this.ShowVisualiseSection = true;

  }

  ngOnDestroy() {
    console.log("destroy search component.");
    if (this.clearSub != null) this.clearSub.unsubscribe();
    this._reviewSetsService.selectedNode = null;
  }
  FormatDate(DateSt: string): string {
    return Helpers.FormatDate(DateSt);
  }
  SearchGetItemList(dataItem: Search) {
    let cr: Criteria = new Criteria();
    cr.onlyIncluded = dataItem.selected;
    cr.showDeleted = false;
    cr.pageNumber = 0;
    cr.searchId = dataItem.searchId;
    let ListDescription: string = dataItem.title;
    cr.listType = 'GetItemSearchList';
    if (dataItem.isClassifierResult) cr.showScoreColumn = true;

    this.ItemListService.FetchWithCrit(cr, ListDescription);
    this._eventEmitter.PleaseSelectItemsListTab.emit();

  }





  SearchNameEdit(searchId: string, searchNo: string, searchName: string) {
    this.CurrentSearchNameEditing = true;
    this.searchN = searchName;
    this.searchId = searchId;
    this.popUpTitle = "Edit search name (search #: " + searchNo + ")";
    window.scrollTo(0, 0);
  }


  SaveSearchName() {
    // I am using the same popup for both search and model edit so the variable names are 'search related' //
    if (this.searchN.trim().length > 0) {
      if (this.popUpTitle.startsWith("Edit search")) {
        this._searchService.UpdateSearchName(this.searchN.trim(), this.searchId);
      }
      else {
        this.classifierService.UpdateModelName(this.searchN.trim(), this.searchId);
      }
      this.CurrentSearchNameEditing = false;
      this.showNameUpdatedNotification();
    }
  }

  CanEditSearchName() {
    if (this.CanWriteName() && this.searchN.trim() != '') {
      return true;
    } else return false;
  }

  ModelNameEdit(modelId: string, modelTitle: string) {
    // I am using the same popup as the search edit so the variable names are 'search related' //
    this.CurrentSearchNameEditing = true;
    this.searchN = modelTitle;
    this.searchId = modelId;
    this.popUpTitle = "Edit model name (model ID: " + modelId + ")";
    window.scrollTo(0, 0);
  }


  private showNameUpdatedNotification(): void {
    let contentSt: string = "Name / Title updated";
    this.notificationService.show({
      content: contentSt,
      animation: { type: 'slide', duration: 400 },
      position: { horizontal: 'center', vertical: 'top' },
      type: { style: "success", icon: true },
      closable: true
    });
  }

  CanWriteName(): boolean {
    if (!this._reviewerIdentityServ.HasWriteRights) {
      //one more check: is the user in the first screen?
      if (this._reviewerIdentityServ.reviewerIdentity.reviewId == 0
        && this._reviewerIdentityServ.reviewerIdentity.ticket === ""
        && this._reviewerIdentityServ.reviewerIdentity.token
        && this._reviewerIdentityServ.reviewerIdentity.token.length > 100
        && this._reviewerIdentityServ.reviewerIdentity.roles
        && this._reviewerIdentityServ.reviewerIdentity.roles.length > 0
        && this._reviewerIdentityServ.reviewerIdentity.roles.indexOf('ReadOnlyUser') == -1
        && this._reviewerIdentityServ.reviewerIdentity.daysLeftAccount >= 0
      )
        return true;
      else return false;
    }
    else return true;
  }

  CancelEditSearchName() {
    this.CurrentSearchNameEditing = false;
  }

  Clear() {
    console.log("clear in search component.");
    this.CurrentDropdownSelectedCode = null;
    this.selectedSearchCodeSetDropDown = '';
    this.selectedSearchDropDown = 'With this code';
    this.commaIDs = '';
    this.searchText = '';
    this.selectedSearchTextDropDown = '';
    this.searchTextModel = '';
    this.NewSearchSection = false;
    this.CheckScreeningSection = false;
    //this.PriorityScreeningSection = false;
    this.modelResultsSection = false;
    this.SearchForPersonModel = false;
    this.selected = undefined;
    this.ModelSection = false;
    this.ShowVisualiseSection = false;
    this.radioButtonApplyModelSection = false;
    this.isCollapsed = false;
    this.isCollapsedVisualise = false;
    this.firstName = "";
    this.modeModels = 'single';
    this.withCode = false;
    this.attributeNames = '';
    this.commaIDs = '';
    this.email = '';
    this.searchText = '';
    this.searchTextModel = '';
    this.CurrentDropdownSelectedCode = null;
    this.SearchForPeoplesModel = 'true';
    this.classifierService.Clear();

  }
}

export interface ReadOnlySource {
  source_ID: number;
  source_Name: string;
  total_Items: number;
  deleted_Items: number;
  duplicates: number;
  isDeleted: boolean;
}
