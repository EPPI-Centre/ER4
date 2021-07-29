import { Component, OnInit, Input, OnDestroy, ViewChild, Attribute, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { searchService, Search } from '../services/search.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { RowClassArgs, GridDataResult, SelectableSettings, SelectableMode, PageChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { ReviewSetsService, ReviewSet, singleNode, SetAttribute } from '../services/ReviewSets.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ClassifierService, ClassifierModel } from '../services/classifier.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { SourcesService } from '../services/sources.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import 'rxjs/add/observable/interval';
import 'rxjs/add/observable/from';
import 'rxjs/add/operator/bufferCount';
import 'rxjs/add/operator/map';
import { Subscription } from 'rxjs';
import { ChartComponent } from '@progress/kendo-angular-charts';
import { saveAs } from '@progress/kendo-file-saver';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { Helpers } from '../helpers/HelperMethods';

@Component({
    selector: 'SearchComp',
    templateUrl: './SearchComp.component.html',
    styles: [`
       .k-grid tr.even { background-color: white; }
       .k-grid tr.odd { background-color: light-grey; }
   `],
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
        private _reviewInfoService: ReviewInfoService
    ) {

    }
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
    private _listSources: any[] = [];
    public selected?: ReadOnlySource;
    public NewSearchSection: boolean = false;
    public LogicSection: boolean = false;
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
    public ReviewSources: ReadOnlySource[] = [];

    public get DataSourceSearches(): GridDataResult {
        return {
            data: orderBy(this._searchService.SearchList, this.sortSearches).slice(this.skip, this.skip + this.pageSize),
            total: this._searchService.SearchList.length,
        };
    }
    public sortSearches: SortDescriptor[] = [{
        field: 'searchNo',
        dir: 'desc'
    }];
    public pageSize = 100;
    public skip = 0;
    protected pageChange({ skip, take }: PageChangeEvent): void {
        this.skip = skip;
        this.pageSize = take;
    }

    public sortChangeSearches(sort: SortDescriptor[]): void {
        this.sortSearches = sort;
        //console.log('sorting?' + this.sortSearches[0].field + " ");
    }
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
    public get SearchList(): Search[] {
        return this._searchService.SearchList;
    }
    public get isSearchServiceBusy(): boolean {
        return this._searchService.IsBusy;
    }
    public setSelectableSettings(): void {

        this.selectableSettings = {
            checkboxOnly: true,
            mode: 'single'
        };
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

        if (e.selectedRows[0] != undefined && this.modelNum == 5 || this.modelNum == 6) {

            console.log("selected:", e.selectedRows[0].dataItem);

            this.ModelSelected = true;
            this.modelTitle = e.selectedRows[0].dataItem.modelTitle;
            this.ModelId = e.selectedRows[0].dataItem.modelId;
            if (this.modelTitle.indexOf('prog') != -1 || this.modelTitle.indexOf('failed') != -1) {
                this.modelIsInProgress = true;
                //alert('model is in progress');
            } else {
                this.modelIsInProgress = false;
            }


        } else {

            console.log("selected:", e.selectedRows);
            this.modelTitle = '';
            this.ModelId = 0;
            this.ModelSelected = false;

            //alert('model has been DESELECTED that is in fact a custom model');
        }
    }
    public get DataSourceModel(): GridDataResult {
        return {
            data: orderBy(this.classifierService.ClassifierModelList, this.sortCustomModel),
            total: this.classifierService.ClassifierModelList.length,
        };
    }
    public get DataSourceModelAllReviews(): GridDataResult {
        return {
            
            data: orderBy(this.classifierService.ClassifierContactModelList, this.sortCustomModel),
            total: this.classifierService.ClassifierContactModelList.length,
        };
    }
    CanOnlySelectRoots() {
        return true;
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
        this.ModelSection = !this.ModelSection;
        this.modelResultsSection = false;
        this.modelResultsAllReviewSection = false;
        this.radioButtonApplyModelSection = true;
        this.ShowVisualiseSection = false;
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
        if (modelNum ==5) {
            this.modelNum = 5;
            this.modelResultsSection = !this.modelResultsSection;
            this.modelResultsAllReviewSection = false;
        } else if (modelNum ==6) {
            this.modelNum = 6;
            this.modelResultsAllReviewSection = !this.modelResultsAllReviewSection;
            this.modelResultsSection = false;
        }

    }
    SetModelSelection(num: number) {

        //alert('SelectedNode is: ' + this._reviewSetsService.selectedNode);
        this.modelNum = num;
        this.NewSearchSection = false;
        this.LogicSection = false;
        this.modelResultsSection = false;
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
    CanApplySearch(): boolean {

        // Easy ones do not have a condition in the seacrhes DD
        // Without an abstract and without any documents uploaded
        if (this.selectedSearchDropDown == 'Without any documents uploaded') {
            return true;
        } else if (this.selectedSearchDropDown == 'Without an abstract') {
            return true;
        } else if (this.selectedSearchDropDown == 'With at least one document uploaded') {
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
        } else if (this.selectedSearchDropDown == 'With Specific Sources' && this._sourcesService.ReviewSources.map(x => x.isSelected == true).length > 0 ) {
            return true;
        }

        return false;

        // 

    }
    CanApplyModel(): boolean {

        if (this.modelNum != 5 && this.modelNum != 0) {
            //console.log('yes step 1');
            // Need to check for Apply code and apply source are filled if selected...
            if (this.ApplyCode && this._reviewSetsService.selectedNode != null && this._reviewSetsService.selectedNode.nodeType == 'SetAttribute') {
                return true;
            } else if (this.ApplySource && this.selected != null) {
                return true;
            } else if (this.ApplyAll) {
                //console.log('yes step 2');
                return true;
            }
            // Need logic in the below about model still in progress
        }
        else if (this.modelNum == 5 && this.ModelSelected && this.ApplySource && this.selected != null && !this.modelIsInProgress) {

            return true;
        }
        else if (this.modelNum == 5 && !this.modelIsInProgress && this.ModelSelected && this.ApplyCode && this._reviewSetsService.selectedNode != null && this._reviewSetsService.selectedNode.nodeType == 'SetAttribute') {
            //alert('custom models');
            return true;

        } else if (this.modelNum == 5 && this.ModelSelected && this.ApplyAll && !this.modelIsInProgress) {

            //alert('custom models');
            return true;
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

    public openConfirmationDialogDeleteSearches() {
        this.confirmationDialogService.confirm('Please confirm', 'Are you sure you want to delete the selected search(es)?', false, '')
            .then(
                (confirmed: any) => {
                    //console.log('User confirmed:', confirmed);
                    if (confirmed) {
                        this.DeleteSearchSelected();
                    } else {
                        //alert('did not confirm');
                    }
                }
            )
            .catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
    }

    public openConfirmationDialog() {
        this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to run the selected model ?', false, '')
            .then(
                (confirmed: any) => {
                    console.log('User confirmed:', confirmed);
                    if (confirmed) {
                        this.RunModel();
                        this.ModelSection = false;
                    }
                    else {
                        //alert('pressed cancel close dialog');
                    };
                }
            )
            .catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
    }

    public openRebuildConfirmationDialog(model: ClassifierModel) {
        this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to rebuild this model ?', false, '')
            .then(
                (confirmed: any) => {
                    console.log('User confirmed:', confirmed);
                    if (confirmed) {
                        this.RebuildModel(model);
                    }
                    else {
                        //alert('pressed cancel close dialog');
                    };
                }
            )
            .catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
    }

    hasError(searchText: string) {

        //alert(searchText);
        if (searchText.trim() == '') {
            return true;
        } else {
            return false;
        }


    }

    RunModel() {


        this.AttributeId = -1;
        this.SourceId = -2;

        if (this.mode == '1') {
            // standard

        } else if (this.mode == '2') {

            //then set the attributeid to begin with
            this.AttributeId = this._reviewSetsService.selectedNode ? Number(this._reviewSetsService.selectedNode.id.substr(1, this._reviewSetsService.selectedNode.id.length - 1)) : -1;

        } else if (this.mode == '3') {
            // not implmented

            this.SourceId = Number(this.selected);

        } else {
            //
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

        } else {

        }

        if (this.CanWrite()) {

            //alert(this.modelTitle + ' ModelTitle ' + this.AttributeId + ' ATTID ' + this.ModelId + ' MODELID ' + this.SourceId + ' sourceID ');
            this.classifierService.Apply(this.modelTitle, this.AttributeId, this.ModelId, this.SourceId);
            //Very sorry notification show


            this.notificationService.show({
                content: 'Refresh List to see results',
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "warning", icon: true },
                closable: true,
                hideAfter: 3000
            });

        }
    }

    SelectModel(model: string) {
        this.ModelSelected = true;
        //alert('you selected model: ' + model);
    }

    async RebuildModel(model: ClassifierModel) {

        if (model.modelId > 0) {

            await this.classifierService.CreateAsync(model.modelTitle, model.attributeIdOn, model.attributeIdNotOn, model.modelId);
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
        else console.log("I'm not doing it :-P ", value);

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
        else console.log("I'm not doing it :-P ", value);

    }

    private _logic: string = '';

    public get logic(): string {

        return this._logic;
    }

    getLogicSearches(logicChoice: string) {


        if (this.CanWrite() && this.checkBoxSelected == true) {

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
                this.checkBoxSelected = false;

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

    selectAllSearchesChange(e: any): void {

        if (e.target.checked) {
            this.allSearchesSelected = true;

            for (let i = 0; i < this.DataSourceSearches.data.length; i++) {

                this.DataSourceSearches.data[i].add = true;
            }
        } else {
            this.allSearchesSelected = false;

            for (let i = 0; i < this.DataSourceSearches.data.length; i++) {

                this.DataSourceSearches.data[i].add = false;
            }
        }
    }



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
    DeleteSearchSelected() {

        // Need to check if user has rights to delete
        let lstStrSearchIds = '';

        for (var i = 0; i < this.DataSourceSearches.data.length; i++) {
            if (this.DataSourceSearches.data[i].add == true) {
                lstStrSearchIds += this.DataSourceSearches.data[i].searchId + ',';
            }
        }
        //console.log(lstStrSearchIds, lstStrSearchIds.substring(0, lstStrSearchIds.length - 1));
        if (this.CanWrite() && lstStrSearchIds.length > 1) {
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
                this._searchService.cmdSearches._contactId = this.ContactChoice.contactId;
                this._searchService.cmdSearches._contactName = this.ContactChoice.contactName;
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
            if (selectedSearchDropDown == 'With Specific Sources') {

               this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchSources');
            }

        }
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
                
                this.ReviewSources = this._sourcesService.ReviewSources;
                if (this.ReviewSources.length > 0) {

                    this.ShowSources = true;
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

    public checkBoxSelected: boolean = false;
    public checkboxClicked(dataItem: any) {

        console.log(dataItem);
        dataItem.add = !dataItem.add;
        if (dataItem.add == true && this.modelNum == 5) {

            //this.ModelSelected = true;
            this.checkBoxSelected = true;
            this._searchService.searchToBeDeleted = dataItem.searchId;

        }
        if (dataItem.add == true) {
            this.checkBoxSelected = true;
        }

    };

    public rowCallback(context: RowClassArgs) {
        const isEven = context.index % 2 == 0;
        return {
            even: isEven,
            odd: !isEven
        };
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

        let search: Search = new Search();
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
        this.modelResultsSection = false;
        this.LogicSection = false;
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