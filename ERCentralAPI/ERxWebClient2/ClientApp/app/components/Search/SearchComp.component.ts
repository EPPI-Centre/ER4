import { Component, OnInit, Input, OnDestroy, ViewChild, Attribute } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { searchService, Search } from '../services/search.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { RowClassArgs, GridDataResult, SelectableSettings, SelectableMode  } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { ReviewSetsService,  ReviewSet, singleNode, SetAttribute } from '../services/ReviewSets.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ClassifierService } from '../services/classifier.service';
import {  ReviewInfoService } from '../services/ReviewInfo.service';
import { BuildModelService } from '../services/buildmodel.service';
import { SourcesService } from '../services/sources.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import 'rxjs/add/observable/interval';
import 'rxjs/add/observable/from';
import 'rxjs/add/operator/bufferCount';
import 'rxjs/add/operator/map';
import { Observable } from 'rxjs';
import { ChartComponent } from '@progress/kendo-angular-charts';
import { saveAs } from '@progress/kendo-file-saver';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';

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
		public ItemListService: ItemListService,
		public _searchService: searchService,
		public _reviewSetsEditingServ: ReviewSetsEditingService,
		private _eventEmitter: EventEmitterService,
        private _reviewSetsService: ReviewSetsService,
		private classifierService: ClassifierService,
		private _buildModelService: BuildModelService,
		private notificationService: NotificationService,
		public _sourcesService: SourcesService,
		private confirmationDialogService: ConfirmationDialogService
	) {
		
	}
    ngOnInit() {

        //console.log("SearchComp init:", this.InstanceId);
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
		else {

			this._reviewSetsService.selectedNode = null;

        }
    }

    //private InstanceId: number = Math.random();
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
	public SearchVisualiseData!: Observable<any>[];

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
	public HideManuallyCreatedItems(ROS: ReadOnlySource): boolean {
		if (ROS.source_Name == 'NN_SOURCELESS_NN' && ROS.source_ID == -1) return true;
		else return false;
	}
	public modelIsInProgress: boolean = false;
	public selectedRows(e: any) {

		if (e.selectedRows[0] != undefined && this.modelNum == 5) {

			console.log("selected:", e.selectedRows[0].dataItem);

			this.ModelSelected = true;
			this.modelTitle = e.selectedRows[0].dataItem.modelTitle;
			this.ModelId = e.selectedRows[0].dataItem.modelId;
			if (this.modelTitle.indexOf('prog') != -1 || this.modelTitle.indexOf('failed') != -1 ) {
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
			data: orderBy(this._buildModelService.ClassifierModelList, this.sort),
			total: this._buildModelService.ClassifierModelList.length,
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

		this._buildModelService.Fetch();
		this._reviewSetsService.selectedNode = null;
		this.NewSearchSection = false;
		this.ModelSection = !this.ModelSection;
		this.modelResultsSection = false;
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
			else if (this.selectedNode.nodeType == "SetAttribute") {
				let Att = this.selectedNode as SetAttribute;
				if (!Att) return false;
				else {
					let Set = this._reviewSetsService.FindSetById(Att.set_id);
					if (!Set) return false;
					else {
						if (Set.subTypeName == "Screening") return false;
						else if (!Set.allowEditingCodeset) return false;
						else {
							let level = this._reviewSetsService.AttributeCurrentLevel(Att);
							if (level && Set.setType && Set.setType.maxDepth > level) {
								console.log("maxDepth", Set.setType.maxDepth);
								return true;
							}
							else return false;
						}
					}
				}
			}
			else return false;
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

	CustomModels() {

		if (this.modelTitle == '') {

			this.ModelSelected = false;
		}
		this.modelNum = 5;
		this.modelResultsSection = !this.modelResultsSection

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
		else if (this.selectedSearchDropDown == 'That have at least one code from this set' && this.selectedSearchCodeSetDropDown != '') {
			return true;
		}
		else if (this.selectedSearchDropDown == 'That dont have any codes from this set' && this.selectedSearchCodeSetDropDown != '') {
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
		}

		return false;

		// 

	}
	CanApplyModel(): boolean {

		if (this.modelNum != 5 && this.modelNum != 0) {
			//console.log('yes step 1');
			// Need to check for Apply code and apply source are filled if selected...
			if (this.ApplyCode && this._reviewSetsService.selectedNode != null && this._reviewSetsService.selectedNode.nodeType =='SetAttribute') {
				return true;
			} else if (this.ApplySource && this.selected != null) {
				return true;
			} else if (this.ApplyAll) {
				//console.log('yes step 2');
				return true;
			}
			// Need logic in the below about model still in progress
		}
		else if (this.modelNum == 5 && this.ModelSelected && this.ApplySource && this.selected != null && !this.modelIsInProgress)
		{

			return true;
		}
		else if (this.modelNum == 5 && !this.modelIsInProgress && this.ModelSelected && this.ApplyCode && this._reviewSetsService.selectedNode != null && this._reviewSetsService.selectedNode.nodeType == 'SetAttribute')
		{
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
	DisplayFriendlySourceNames(sourceItem: ReadOnlySource): string  {

		if (this.HideManuallyCreatedItems(sourceItem)) {
			return 'Manually Created Source';
		}
		return sourceItem.source_Name; 
	}

	public openConfirmationDialogDeleteSearches() {
		this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to delete the selected search ?', false, '')
			.then(
			(confirmed: any) => {
					console.log('User confirmed:', confirmed);
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
				(confirmed: any) =>
				{
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
			this.AttributeId = this._reviewSetsService.selectedNode ? Number(this._reviewSetsService.selectedNode.id.substr(1, this._reviewSetsService.selectedNode.id.length-1))  : -1;
			
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
					content: 'Please refresh the models list to check if it is updated',
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
							lstStrSearchNos =  this.DataSourceSearches.data[i].searchNo;
						}
						
					} else {
						lstStrSearchIds += ',' + this.DataSourceSearches.data[i].searchId ;
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
	public selectedSearchDropDown: string = '';
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

	public get DataSourceSearches(): GridDataResult {
		return {
			data: orderBy(this._searchService.SearchList, this.sort),
			total: this._searchService.SearchList.length,
        };
	}
	   
	refreshSearches() {
		this._searchService.Fetch();
	}
	
	DeleteSearchSelected() {

		// Need to check if user has rights to delete
		let lstStrSearchIds = '';

		for (var i = 0; i < this.DataSourceSearches.data.length; i++) {
			if (this.DataSourceSearches.data[i].add == true) {
				lstStrSearchIds += this.DataSourceSearches.data[i].searchId + ',' ;
			}
		}
		console.log(lstStrSearchIds);
		if (this.CanWrite()) {
			this._searchService.Delete(lstStrSearchIds);
		}
	}
	public convertToIncEx(incEx: boolean): string  {

		if (incEx == true) {
			return 'included';
		} else {
			return 'excluded'
		}

	}
	
	callSearches(selectedSearchDropDown: string, selectedSearchTextDropDown: string, searchBool: boolean) {

		if (this.CanWrite) {


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


					this._searchService.cmdSearches._title = searchTitle ;
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
			if (selectedSearchDropDown == 'That have at least one code from this set') {

				this._searchService.cmdSearches._withCodes = 'true';
				this._searchService.cmdSearches._title = this.selectedSearchCodeSetDropDown;

				this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchCodeSetCheck');

			}
			if (selectedSearchDropDown == 'That dont have any codes from this set') {

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

		}
	}

	public nextDropDownList(num: number, val: string) {

		let typeElement: "success" | "error" | "none" | "warning" | "info" | undefined = undefined;
		this.showTextBox = false;
		this.selectedSearchDropDown = val;

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

		dataItem.add = !dataItem.add;
		if (dataItem.add == true && this.modelNum == 5) {

			//this.ModelSelected = true;
			this.checkBoxSelected = true;
			this._searchService.searchToBeDeleted = dataItem.searchId;

		}
		if (dataItem.add == true ) {
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

	// Need to ask Sergio about this sort part
    public sort: SortDescriptor[] = [{
		field: 'modelId',
        dir: 'desc'
	}];

    public sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        console.log('sorting?' + this.sort[0].field + " ");
	}
	public visualiseTitle: string = '';
	public visualiseSearchId = 0;

	OpenClassifierVisualisation(search: Search) {

		console.log(JSON.stringify(search.title));
		this.visualiseTitle = search.title;
		this.visualiseSearchId = search.searchId;
		console.log(JSON.stringify(search));
		this._reviewSetsEditingServ.CreateVisualiseData(search.searchId);
		//alert('in here' + JSON.stringify(this.SearchVisualiseData));
		// for now just show the graph area unhidden
		this.ShowVisualiseSection = true;

	}

	ngOnDestroy() {

		this._reviewSetsService.selectedNode = null;
	}
	
	SearchGetItemList(dataItem: Search) {

		let search: Search = new Search();
		let cr: Criteria = new Criteria();
		cr.onlyIncluded = dataItem.selected;
		cr.showDeleted = false;
		cr.pageNumber = 0;
		cr.searchId = dataItem.searchId;
		let ListDescription: string = "GetItemSearchList";
		cr.listType = ListDescription;

		this.ItemListService.FetchWithCrit(cr, ListDescription);
		this._eventEmitter.PleaseSelectItemsListTab.emit();

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