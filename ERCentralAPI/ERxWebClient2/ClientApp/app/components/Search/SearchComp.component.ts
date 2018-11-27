import { Component, OnInit, Input, OnDestroy, ViewEncapsulation, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { searchService, Search, SearchCodeCommand } from '../services/search.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { SelectionModel } from '@angular/cdk/collections';
import { RowClassArgs, GridDataResult, GridComponent  } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { NgbModal, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ReviewSetsService,  ReviewSet } from '../services/ReviewSets.service';

@Component({
	selector: 'SearchComp',
    templateUrl: './SearchComp.component.html',
    styles: [`
       .k-grid tr.even { background-color: white; }
       .k-grid tr.odd { background-color: light-grey; }
   `],
	providers: [],
	encapsulation: ViewEncapsulation.None
})

export class SearchComp implements OnInit, OnDestroy {
	displayedColumns = ['selected', 'searchId', 'title', 'contactName', 'searchDate', 'hitsNo'];
	constructor(private router: Router,
		private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService,
		public _searchService: searchService,
		private _eventEmitter: EventEmitterService,
		private modalService: NgbModal
	) {
	}

	@ViewChild('testKendoGrid') searchesGrid!: GridComponent;

    onSubmit(f: string) {

	}
	
    public selectedAll: boolean = false;

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

	openNewSearchModal() {

		let modalComp = this.modalService.open(SearchesModalContent, { size: 'lg', centered: true });
		modalComp.componentInstance.InfoBoxTextInput = 'tester';
		modalComp.componentInstance.focus(null);

		modalComp.result.then(() => {
		},
			() => {
			}
		);
	}


	public checkboxClicked(dataItem: any) {

		dataItem.add = !dataItem.add;
		if (dataItem.add == true) {
			this._searchService.searchToBeDeleted = dataItem.searchId;
		} else {
			this._searchService.searchToBeDeleted = '';
		}
	};

	RemoveOneLocalSource() {

        this._searchService.SearchList = this._searchService.SearchList.slice(3);
		
    }
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
    

	OpenItems(item: number) {

		let cr: Criteria = new Criteria();

		cr.filterAttributeId = -1;
		cr.searchId = item;
		cr.listType = 'GetItemSearchList';
		cr.pageNumber = 0;
		//alert('cr.listType: ' + cr.listType);

		this.ItemListService.FetchWithCrit(cr, 'GetItemSearchList');
        this._eventEmitter.PleaseSelectItemsListTab.emit();
	}

	ngOnDestroy() {
	}
	
	public tableArr: Search[] = this._searchService.SearchList;
	public testArr: Search[] = [];
	public selectAll: boolean =false;

	public initialSelection = [];
	public allowMultiSelect = true;
	public selection = new SelectionModel<Search>(this.allowMultiSelect, this.initialSelection);

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

            this._searchService.Fetch();

		}
	}

}


@Component({
	selector: 'ngbd-SearchesModal-content',
	templateUrl: './SearchesModal.component.html'
})
export class SearchesModalContent implements SearchCodeCommand {

	@ViewChild('SearchesModal')

	private canWrite: boolean = true;
	public dropDownList: any = null;
	public showTextBox: boolean = false;
	public showDropDown2: boolean = true;
	public selectedSearchDropDown: string = '';
	public selectedSearchTextDropDown: string = '';
	public nodeSelected: boolean = false;
	public selectedNodeDataName: string = '';
	public CodeSets: any[] = [];

	_setID: number = 0;
	_searchText: string = '';
	_title: string = '';
	_answers: string = '';
	_included: boolean = true;
	_withCodes: boolean = false;;
	_searchId: number = 0;
	_IDs: string = '';

	public get IsReadOnly(): boolean {
		return this.canWrite;
	}
	constructor(public activeModal: NgbActiveModal,
		private reviewSetsService: ReviewSetsService,
		private _eventEmitter: EventEmitterService,
		private _searchService: searchService
	) { }

	public cmdSearches: SearchCodeCommand = {

		_setID: 0,
		_searchText: '',
		_IDs: '',
		_title: '',
		_answers: '',
		_included: false,
		_withCodes: false,
		_searchId: 0
	};

	public withCode: boolean = false;
	public attributeNames: string = '';
	public commaIDs: string = '';
	public searchText: string = '';
	
	callSearches(selectedSearchDropDown: string, selectedSearchTextDropDown: string, searchBool: boolean) {

		this.selectedSearchTextDropDown = selectedSearchTextDropDown;
		let searchTitle: string = '';
		let firstNum: boolean = selectedSearchDropDown.search('With this code') != -1;
		let secNum: boolean = selectedSearchDropDown.search('Without this code') != -1
		this.cmdSearches._included = Boolean(searchBool);
		this.cmdSearches._withCodes = this.withCode;
		this.cmdSearches._searchId = 0;
		
		if (firstNum == true || secNum == true ) {

			if (firstNum) {

				this.withCode = true;
			} else {

				this.withCode = false;
			}			

			if (this.reviewSetsService.selectedNode != undefined) {

				let tmpID: number = this.reviewSetsService.selectedNode.attributeSetId;
				this.attributeNames = this.reviewSetsService.selectedNode.name;
				this.cmdSearches._answers = String(tmpID);
				alert(this.reviewSetsService.selectedNode);
			
				searchTitle = this.withCode == true ?
					"Coded with: " + this.attributeNames : "Not coded with: " + this.attributeNames;


				this.cmdSearches._title = searchTitle;
				this.cmdSearches._withCodes = this.withCode;

				this._searchService.FetchSearchGeneric(this.cmdSearches, 'SearchCodes');
	
			}
		}

		if (selectedSearchDropDown == 'With these internal IDs (comma separated)') {

			this.cmdSearches._IDs = this.commaIDs;
			this.cmdSearches._title = this.commaIDs;
			this._searchService.FetchSearchGeneric(this.cmdSearches, 'SearchIDs');
		

		}
		if(selectedSearchDropDown == 'With these imported IDs (comma separated)') {

			this.cmdSearches._IDs = this.commaIDs;
			this.cmdSearches._title = this.commaIDs;

			this._searchService.FetchSearchGeneric(this.cmdSearches, 'SearchImportedIDs');
		

		}
		if (selectedSearchDropDown == 'Containing this text') {

			this.cmdSearches._title = this.searchText;
			this.cmdSearches._included = Boolean(searchBool);
			this._searchService.FetchSearchGeneric(this.cmdSearches, 'SearchText');
		}
		if (selectedSearchDropDown == 'That have at least one code from this set') {

			this.cmdSearches._withCodes = true;
			this.cmdSearches._title = this.selectedSearchTextDropDown;

			this._searchService.FetchSearchGeneric(this.cmdSearches, 'SearchCodeSetCheck');

		}
		if (selectedSearchDropDown == 'That dont have any codes from this set') {

			this.cmdSearches._withCodes = false;
			this.cmdSearches._title = this.selectedSearchTextDropDown;
			this._searchService.FetchSearchGeneric(this.cmdSearches, 'SearchCodeSetCheck');

		}
		if (selectedSearchDropDown == 'Without an abstract') {

			alert(selectedSearchDropDown);
			this.cmdSearches._title = searchTitle;
			
			this._searchService.FetchSearchGeneric(this.cmdSearches, 'SearchNoAbstract');
		
		}

		if (selectedSearchDropDown == 'Without any documents uploaded') {

			alert(selectedSearchDropDown);
			this.cmdSearches._title = 'Without any documents uploaded';

			this._searchService.FetchSearchGeneric(this.cmdSearches, 'SearchNoFiles');
			
		}
		if (selectedSearchDropDown == 'With at least one document uploaded') {

			this.cmdSearches._title = 'With at least one document uploaded.';
			this._searchService.FetchSearchGeneric(this.cmdSearches, 'SearchOneFile');
			
		}

		this.activeModal.dismiss();

	}
	
	public nextDropDownList(num: number, val: string) {

		this.showDropDown2 = true;
		this.showTextBox = false;
		this.selectedSearchDropDown = val;
		this._eventEmitter.nodeSelected = false;

		switch (num) {

			case 1: {
				this._eventEmitter.nodeSelected = true;
				this.dropDownList = this.reviewSetsService.ReviewSets;
				this.showDropDown2 = false;
				break;
			}
			case 2: {
				//statements; 
				this._eventEmitter.nodeSelected = true;
				this.dropDownList = this.reviewSetsService.ReviewSets;
				this.showDropDown2 = false;
				break;
			}
			case 3: {
				//With these internal IDs (comma separated) show text box
				
				this.showDropDown2 = false;
				this.showTextBox = true;
				break;
			}
			case 4: {
				this.showDropDown2 = true;
				this.showTextBox = true;
				break;
			}
			case 5: {

				this.CodeSets = this.reviewSetsService.ReviewSets.filter(x => x.nodeType == 'ReviewSet')
					.map(
						(y: ReviewSet) => {
							return y.name;
						}
					);
				this.showDropDown2 = true;
				this.dropDownList = this.reviewSetsService.ReviewSets;
				break;
			}
			case 6: {
				this.CodeSets = this.reviewSetsService.ReviewSets.filter(x => x.nodeType == 'ReviewSet')
					.map(
						(y: ReviewSet) => {
							return y.name;
						}
					);
				this.showDropDown2 = true;
				this.dropDownList = this.reviewSetsService.ReviewSets;
				break;
			}
			case 7: {

				this.showDropDown2 = true;
				break;
			}
			case 8: {
				//statements; 
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = false;
				break;
			}
			case 9: {
				this.showDropDown2 = false;
				break;
			}
			case 10: {
				this.showDropDown2 = false;
				break;
			}
			default: {
				break;
			}
		}
	}
	
	public setSearchCodeSetDropDown(codeSetName: string) {

		this.selectedSearchTextDropDown = this.reviewSetsService.ReviewSets.filter(x => x.name == codeSetName)
			.map(
				(y: ReviewSet) => {

						this.cmdSearches._setID = y.set_id;
						return y.name;
					}
				)[0];
	}

	public setSearchTextDropDown(heading: string) {

		this.selectedSearchTextDropDown = heading;
		this.cmdSearches._searchText = heading;
	}

	public focus(canWrite: boolean) {
		this.canWrite = canWrite;
	}

}
