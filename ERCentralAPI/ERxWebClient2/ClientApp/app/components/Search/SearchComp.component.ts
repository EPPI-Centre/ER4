import { Component, OnInit, Input, OnDestroy, ViewEncapsulation, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { searchService, Search, SearchCodeCommand } from '../services/search.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { RowClassArgs, GridDataResult, GridComponent, SelectableSettings, SelectableMode  } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { ReviewSetsService,  ReviewSet } from '../services/ReviewSets.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ClassifierService } from '../services/classifier.service';
import { ReviewInfo, ReviewInfoService } from '../services/ReviewInfo.service';
import { BuildModelService } from '../services/buildmodel.service';
import { SourcesService } from '../services/sources.service';

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
		private reviewInfoService: ReviewInfoService,
		public _searchService: searchService,
		private _eventEmitter: EventEmitterService,
		public _reviewSetsService: ReviewSetsService,
		private classifierService: ClassifierService,
		private _buildModelService: BuildModelService,
		private notificationService: NotificationService,
		private _sourcesService: SourcesService
	) {
		
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
    public modelResultsSection: boolean = false;
	public radioButtonApplyModelSection: boolean = false;
	public isCollapsed: boolean = false;

	public modeModels: SelectableMode = 'single';

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

	public selectedRows(e: any) {

		if (e.selectedRows[0] != undefined ) {
			console.log("selected:", e.selectedRows[0].dataItem);
			this.ModelSelected = true;
			this.modelTitle = e.selectedRows[0].dataItem.modelTitle;
			this.ModelId = e.selectedRows[0].dataItem.modelId;

		} else {

			console.log("selected:", e.selectedRows);
			this.modelTitle = '';
			this.ModelId = 0;
			this.ModelSelected = false;
		}
		
		//console.log(JSON.stringify(dataItem));

		//dataItem.add = !dataItem.add;

		//if (dataItem.add == true) {

		//	this.ModelSelected = true;
		//	this.modelTitle = dataItem.modelTitle;
		//	this.ModelId = dataItem.modelId;
		//	console.log(this.modelTitle);
		//	console.log(this.ModelId);
		//} else {

		//	this.modelTitle = '';
		//	this.ModelId = 0;
		//	this.ModelSelected = false;
		//}
			   
	}

    //public dropdownBasic1: boolean = 
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
    removeHandler(event: any) {

        alert("Not implemented!");
	}

	public mode: string = '1';

	NewSearch() {

		this.mode = '1';
		this.NewSearchSection = !this.NewSearchSection;
		this.ModelSection = false;
		this.modelResultsSection = false;
		this.radioButtonApplyModelSection = false;
	}
	CloseCodeDropDown() {
		this.isCollapsed = false;
	}
	Classify() {

		this._reviewSetsService.selectedNode = null;
		this.NewSearchSection = false;
		this.ModelSection = !this.ModelSection;
		this.modelResultsSection = false;
		this.radioButtonApplyModelSection = true;

	}

	CustomModels() {

		if (this.modelTitle == '') {

			this.ModelSelected = false;
		}
		this.ModelSection = true;
		this.modelResultsSection = !this.modelResultsSection

	}
	SetModelSelection(num: number) {

		this.NewSearchSection = false;
		this.LogicSection = false;
		this.modelResultsSection = false;

		if (this.modelNum !=4) {
			this.ModelSelected = true;
		}

		this.modelNum = num;
	}

	chooseCodeMessage() {
		this.notificationService.show({
			content: 'Please use the tree on the right hand side to choose a code',
			animation: { type: 'slide', duration: 400 },
			position: { horizontal: 'center', vertical: 'top' },
			type: { style: "warning", icon: true },
			closable: true
		});
	}
	chooseSourceDD() {

		this._reviewSetsService.selectedNode = null;
		this._listSources = this._sourcesService.ReviewSources;
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

			// must be a custom model!!
			// hardcode for now
			//this.modelTitle = 't89';
			//this.ModelId = ??
		}

		if (this.CanWrite()) {

			this.classifierService.Apply(this.modelTitle, this.AttributeId, this.ModelId, this.SourceId);
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
			alert('AND');
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
		//console.log('I get it', this._searchInclOrExcl);

		return this._searchInclOrExcl;
	}
	public set searchInclOrExcl(value: string) {

		this._searchService.cmdSearches._included = this._searchInclOrExcl;

		if (value == 'true' || value == 'false') this._searchInclOrExcl = value;
		else console.log("I'm not doing it :-P ", value);

	}

	private _logic: string = '';

	public get logic(): string {

		//this._searchService.cmdSearches._included = this._searchInclOrExcl;
		//console.log('I get it', this._logic);

		return this._logic;
	}

	//public set logic(value: string) {

	//	alert('Check this method named: "logic..."');
	//	this._searchService.cmdSearches._included = 'true';
	//	this._searchService.cmdSearches._title = "159 AND 158";
	//	this._searchService.cmdSearches._logicType = "AND";
	//	this._searchService.cmdSearches._searches = "21712,21711";
	//	this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchCodeLogic');
	//	this._logic = value;
	//}

	getLogicSearches(logicChoice: string) {

		if (this.CanWrite()) {
			//alert('got inside here: ' + logicChoice);
			if (logicChoice == 'NOT (excluded)') {
				this._searchService.cmdSearches._included = 'false';
				logicChoice = 'NOT';

			} else {
				this._searchService.cmdSearches._included = 'true';
			}

			let lstStrSearchIds = '';
			let lstStrSearchNos = '';
			for (var i = 0; i < this.DataSourceSearches.data.length; i++) {

				if (this.DataSourceSearches.data[i].add == true) {
					if (lstStrSearchIds == "") {
						lstStrSearchIds = this.DataSourceSearches.data[i].searchId;
						lstStrSearchNos = this.DataSourceSearches.data[i].searchNo;
					} else {
						lstStrSearchIds += ',' + this.DataSourceSearches.data[i].searchId ;
						lstStrSearchNos += ' ' + logicChoice + ' ' + this.DataSourceSearches.data[i].searchNo;
					}
				}
			}
			this._searchService.cmdSearches._title = lstStrSearchNos;
			this._searchService.cmdSearches._logicType = logicChoice;
			this._searchService.cmdSearches._searches = lstStrSearchIds;
			this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchCodeLogic');

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
	// bound to header checkbox

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

	//selectAllModelsChange(e: any): void {

	//	if (e.target.checked) {
	//		this.allModelsSelected = true;

	//		for (let i = 0; i < this.DataSourceModel.data.length; i++) {

	//			this.DataSourceModel.data[i].add = true;
	//		}
	//	} else {
	//		this.allModelsSelected = false;

	//		for (let i = 0; i < this.DataSourceModel.data.length; i++) {

	//			this.DataSourceModel.data[i].add = false;
	//		}
	//	}
	//}

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
	
	public withCode: boolean = false;
	public attributeNames: string = '';
	public commaIDs: string = '';
	public searchText: string = '';

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

				if (this._reviewSetsService.selectedNode != undefined) {

					let tmpID: number = this._reviewSetsService.selectedNode.attributeSetId;
					this.attributeNames = this._reviewSetsService.selectedNode.name;
					this._searchService.cmdSearches._answers = String(tmpID);
					alert(this._reviewSetsService.selectedNode);

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

				this._searchService.cmdSearches._title = this.searchText;
				//this._searchService.cmdSearches._included = Boolean(searchBool);
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

				alert(selectedSearchDropDown);
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
		
	public checkboxClicked(dataItem: any) {

		dataItem.add = !dataItem.add;
		if (dataItem.add == true) {
			this._searchService.searchToBeDeleted = dataItem.searchId;
		} else {
			this._searchService.searchToBeDeleted = '';
		}
	};

	//public checkboxModelClicked(dataItem: any) {

	//	console.log(JSON.stringify(dataItem));

	//	//if (this.modelTitle != null && this.modelTitle != dataItem.modelTitle) {
	//	//	this.modelTitle = dataItem.modelTitle;
	//	//	this.ModelId = dataItem.modelId;

	//	//}

	//	dataItem.add = !dataItem.add;
		
	//	if (dataItem.add == true) {

	//		this.ModelSelected = true;
	//		this.modelTitle = dataItem.modelTitle;
	//		this.ModelId = dataItem.modelId;
	//		console.log(this.modelTitle);
	//		console.log(this.ModelId);
	//	} else {

	//		this.modelTitle = '';
	//		this.ModelId = 0;
	//		this.ModelSelected = false;
	//	}

	//};

    public rowCallback(context: RowClassArgs) {
        const isEven = context.index % 2 == 0;
        return {
            even: isEven,
            odd: !isEven
        };
	}

	// Need to ask Sergio about this sort part
    public sort: SortDescriptor[] = [{
		field: 'searchNo',
        dir: 'desc'
	}];

    public sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        console.log('sorting?' + this.sort[0].field + " ");
    }
	OpenClassifierVisualisation(search: Search) {
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
	ngOnInit() {

        //console.log("SearchComp init:", this.InstanceId);
		if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
			this.router.navigate(['home']);
		}
		else {

			this._reviewSetsService.selectedNode = null;
			this._sourcesService.FetchSources();
			this.reviewInfoService.Fetch();
			this._buildModelService.Fetch();
            this._searchService.Fetch();

		}
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