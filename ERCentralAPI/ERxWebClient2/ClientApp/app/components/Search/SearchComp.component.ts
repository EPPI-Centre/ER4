import { Component, OnInit, Input, OnDestroy, ViewEncapsulation, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { searchService, Search, SearchCodeCommand } from '../services/search.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { RowClassArgs, GridDataResult, GridComponent  } from '@progress/kendo-angular-grid';
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
		private _reviewSetsService: ReviewSetsService,
		private classifierService: ClassifierService,
		private _buildModelService: BuildModelService,
		private notificationService: NotificationService,
		private _sourcesService: SourcesService
	) {
	}
	public modelNum: number = 0;
	public modelTitle: string = '';
	public ModelId = -1;
	public mode: number = 0;
	public AttributeId = 0;
	public SourceId = 0;
	private _listSources: any[] = [];
	public selected?: ReadOnlySource;

	public get DataSourceModel(): GridDataResult {
		return {
			data: orderBy(this._buildModelService.ClassifierModelList, this.sort),
			total: this._buildModelService.ClassifierModelList.length,
		};
	}

	SetModelSelection(num: number) {

		this.ModelSelected = true;
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

		this._listSources = this._sourcesService.ReviewSources;
	}


	SetMode(num: number) {
		this.ModelSelected = true;
		this.mode = num;
	}

	RunModel() {

		this.AttributeId = -1;
		this.SourceId = -2;

		if (this.mode == 1) {
			// standard

		} else if (this.mode == 2) {

			//then set the attributeid to begin with
			this.AttributeId = this._reviewSetsService.selectedNode ? Number(this._reviewSetsService.selectedNode.id.substr(1, this._reviewSetsService.selectedNode.id.length-1))  : -1;
			
		} else if (this.mode == 3) {
			// not implmented
			
			this.SourceId = Number(this.selected);
			alert(this.SourceId);

		} else {
			//
			alert('You must apply the model to some items');
			return;
		}


		alert('The model number is: ' + this.modelNum);
		
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

			}

		alert(this.modelTitle + ' ' + this.AttributeId + ' ' + this.ModelId + ' ' + this.SourceId);
		// call service with http call here somewhere...
		this.classifierService.Apply(this.modelTitle, this.AttributeId, this.ModelId, this.SourceId);

	}

	public OpenClassifierScreen(ML: boolean) {

		if (ML) {
			alert('need to open a relevant screen');
			// for now harcode values and send the variables ot the controller
			// in order to test that we can get a dotnetcore version
			//working
			// need to remove this...
			this.classifierService.Create('','','');


		} else {

			alert('do nothing here');
		}
	}
	
	SelectModel(model: string) {

		this.ModelSelected = true;
		alert('you selected model: ' + model);

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
				alert('OR');
			}
	}, {
		text: 'NOT',
			click: () => {
				this.getLogicSearches('NOT');
				alert('NOT');
			}
	}, {
		text: 'NOT (excluded)',
			click: () => {
				this.getLogicSearches('NOT (excluded)');
				alert('NOT (excluded)');
			}
	}];

	private _searchInclOrExcl: string = '';
	public ModelSelected: boolean = false;

	public get searchInclOrExcl(): string {

		this._searchService.cmdSearches._included = this._searchInclOrExcl;
		console.log('I get it', this._searchInclOrExcl);

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
		console.log('I get it', this._logic);

		return this._logic;
	}

	public set logic(value: string) {



		alert('got inside here');
		this._searchService.cmdSearches._included = 'true';
		this._searchService.cmdSearches._title = "159 AND 158";
		this._searchService.cmdSearches._logicType = "AND";
		this._searchService.cmdSearches._searches = "21712,21711";
		this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchCodeLogic');
		this._logic = value;
	}

	getLogicSearches(logicChoice: string) {

		//alert('got inside here: ' + logicChoice);
		if (logicChoice == 'NOT (excluded)') {
			this._searchService.cmdSearches._included = 'false';
			logicChoice = 'NOT';

		} else {
			this._searchService.cmdSearches._included = 'true';
		}

		let lstStrSearchIds = '';
		let lstStrSearchNos = '';
		for (var i = 0; i < this.DataSource.data.length; i++) {

			if (this.DataSource.data[i].add == true) {

				if (lstStrSearchIds == "") {
					lstStrSearchIds = this.DataSource.data[i].searchId;
					lstStrSearchNos = this.DataSource.data[i].searchNo;
				} else {
					lstStrSearchIds += ',' + this.DataSource.data[i].searchId ;
					lstStrSearchNos += ' ' + logicChoice + ' ' + this.DataSource.data[i].searchNo;
				}

			}
		}
		alert(lstStrSearchNos);
		//need some logic around the checkbox selection for title and searches
		//these should be separated by a comma
		this._searchService.cmdSearches._title = lstStrSearchNos;
		this._searchService.cmdSearches._logicType = logicChoice;
		this._searchService.cmdSearches._searches = lstStrSearchIds;
		this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchCodeLogic');
	}
	
	private canWrite: boolean = true;
	public dropDownList: any = null;
	public showTextBox: boolean = false;
	public selectedSearchDropDown: string = '';
	public selectedSearchTextDropDown: string = '';
	public selectedSearchCodeSetDropDown: string = '';
	public CodeSets: any[] = [];

	public get IsReadOnly(): boolean {
		return this.canWrite;
	}
    public selectedAll: boolean = false;
	public modalClass: boolean = false;

	allSearchesSelected: boolean = false;
	// bound to header checkbox

	stateAdd: State = {
		// will hold grid state
		skip: 0,
		take: 2
	};
	
	selectAllSearchesChange(e: any): void {
		
		if (e.target.checked) {
			this.allSearchesSelected = true;
			
			for (let i = 0; i < this.DataSource.data.length; i++) {
				
				this.DataSource.data[i].add = true;
			}
		} else {
			this.allSearchesSelected = false;
			
			for (let i = 0; i < this.DataSource.data.length; i++) {

				this.DataSource.data[i].add = false;
			}
		}
	}

	public get DataSource(): GridDataResult {
		return {
			data: orderBy(this._searchService.SearchList, this.sort),
			total: this._searchService.SearchList.length,
        };
	}
	   
	refreshSearches() {
		
		this._searchService.Fetch();

		//this.gridData = process(this.DataSource.data, this.stateAdd);
	}
	
	DeleteSearchSelected() {

		let lstStrSearchIds = '';

		for (var i = 0; i < this.DataSource.data.length; i++) {
			if (this.DataSource.data[i].add == true) {
				lstStrSearchIds += this.DataSource.data[i].searchId + ',' ;
			}
		}
		console.log(lstStrSearchIds);
		this._searchService.Delete(lstStrSearchIds);
	}
	
	public withCode: boolean = false;
	public attributeNames: string = '';
	public commaIDs: string = '';
	public searchText: string = '';

	callSearches(selectedSearchDropDown: string, selectedSearchTextDropDown: string, searchBool: boolean) {

		this.selectedSearchTextDropDown = selectedSearchTextDropDown;
		let searchTitle: string = '';
		let firstNum: boolean = selectedSearchDropDown.search('With this code') != -1;
		let secNum: boolean = selectedSearchDropDown.search('Without this code') != -1
		this._searchService.cmdSearches._included = String(searchBool);

		//alert('model ang value: ' + searchBool + ' value being passed is: ' + this._searchService.cmdSearches._included);

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

	public nextDropDownList(num: number, val: string) {

		let typeElement: "success" | "error" | "none" | "warning" | "info" | undefined = undefined;
		this.showTextBox = false;
		this.selectedSearchDropDown = val;

		switch (num) {

			case 1: {

				typeElement = 'warning';
				this.dropDownList = this._reviewSetsService.ReviewSets;
				this.notificationService.show({
					content: 'Please use the tree on the right hand side to choose a code',
					animation: { type: 'slide', duration: 400 },
					position: { horizontal: 'center', vertical: 'top' },
					type: { style: typeElement, icon: true },
					closable: true
				});
				break;
			}
			case 2: {
				this.dropDownList = this._reviewSetsService.ReviewSets;
				typeElement = 'warning';
				this.notificationService.show({
					content: 'Please use the tree on the right hand side to choose a code',
					animation: { type: 'slide', duration: 400 },
					position: { horizontal: 'center', vertical: 'top' },
					type: { style: typeElement, icon: true },
					closable: true
				});
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

    public rowCallback(context: RowClassArgs) {
        const isEven = context.index % 2 == 0;
        return {
            even: isEven,
            odd: !isEven
        };
	}

    public sort: SortDescriptor[] = [{
        field: 'hitsNo',
        dir: 'desc'
	}];

    public sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        console.log('sorting?' + this.sort[0].field + " ");
    }

	ngOnDestroy() {
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

		if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
			this.router.navigate(['home']);
		}
		else {

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