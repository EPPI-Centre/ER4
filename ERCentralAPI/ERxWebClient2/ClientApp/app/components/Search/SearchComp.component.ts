import { Component, OnInit, Input, OnDestroy, ViewEncapsulation, ViewChild, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { searchService, Search, SearchCodeCommand } from '../services/search.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { RowClassArgs, GridDataResult, GridComponent  } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { NgbModal, NgbActiveModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { ReviewSetsService,  ReviewSet } from '../services/ReviewSets.service';
import { ModalSearchService } from '../services/modalSearch.service';

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
		private modalService: ModalSearchService,
		//public activeModal: NgbActiveModal,
		private reviewSetsService: ReviewSetsService
	) {
	}

	@ViewChild('testKendoGrid') searchesGridRes!: GridComponent;

	//public _searchesModalContent: SearchesModalContent = new
	//	SearchesModalContent(this.activeModal, this.reviewSetsService, this._searchService);
	
    public selectedAll: boolean = false;
	public modalClass: boolean = false;

	allSearchesSelected: boolean = false;
	// bound to header checkbox

	stateAdd: State = {
		// will hold grid state
		skip: 0,
		take: 2
	};

	//public gridData: GridDataResult = process(this.DataSource.data, this.stateAdd);
	private bodyText!: string;
	
	openModal(id: string) {

		alert('got in here...');

		this.modalClass = true;

		this.modalService.open(id);

	}

	closeModal(id: string) {
		this.modalClass = false;
		this.modalService.close(id);
	}
	
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
	
	//openNewSearchModal() {
				

	//	let modalNew = this.modalService.open(SearchesModalContent, { size: 'lg', centered: true });
	//	modalNew.componentInstance.InfoBoxTextInput = 'tester';
	//	modalNew.componentInstance.focus(null);

	//	modalNew.result.then(

	//		//called if search button is pressed
	//		(result) => {

	//			console.log('pressed okay: ' + JSON.stringify(result));

	//		},
	//		//called if modal is cancelled
	//		(result) => {
	//			console.log('cancelled: ' + JSON.stringify(result));
	//		}
	//	);
	//}
	
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
			this.bodyText = 'This text can be updated in modal 1';
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

    private _searchInclOrExcl: string = 'true';
    public get searchInclOrExcl(): string {
        console.log('I get it', this._searchInclOrExcl);
        return this._searchInclOrExcl;
    }
    public set searchInclOrExcl(value: string) {
        if (value == 'true' || value == 'false') this._searchInclOrExcl = value;
        else console.log("I'm not doing it :-P ", value);
	}


	private canWrite: boolean = true;
	public dropDownList: any = null;
	public showTextBox: boolean = false;
	public selectedSearchDropDown: string = '';
	public selectedSearchTextDropDown: string = '';
	public selectedSearchCodeSetDropDown: string = '';
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

	constructor(public _activeModal: NgbActiveModal,
		private _reviewSetsService: ReviewSetsService,
		private _searchService: searchService,
	) {

	}

	ngOnInit() {
        this._searchInclOrExcl = 'true';
	}

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

			if (this._reviewSetsService.selectedNode != undefined) {

				let tmpID: number = this._reviewSetsService.selectedNode.attributeSetId;
				this.attributeNames = this._reviewSetsService.selectedNode.name;
				this.cmdSearches._answers = String(tmpID);
				alert(this._reviewSetsService.selectedNode);
			
				searchTitle = this.withCode == true ?
					"Coded with: " + this.attributeNames : "Not coded with: " + this.attributeNames;


				this.cmdSearches._title = searchTitle;
				this.cmdSearches._withCodes = this.withCode;

				this._searchService.CreateSearch(this.cmdSearches, 'SearchCodes');
	
			}
		}

		if (selectedSearchDropDown == 'With these internal IDs (comma separated)') {

			this.cmdSearches._IDs = this.commaIDs;
			this.cmdSearches._title = this.commaIDs;
			this._searchService.CreateSearch(this.cmdSearches, 'SearchIDs');
		

		}
		if(selectedSearchDropDown == 'With these imported IDs (comma separated)') {

			this.cmdSearches._IDs = this.commaIDs;
			this.cmdSearches._title = this.commaIDs;

			this._searchService.CreateSearch(this.cmdSearches, 'SearchImportedIDs');
		

		}
		if (selectedSearchDropDown == 'Containing this text') {

			this.cmdSearches._title = this.searchText;
			this.cmdSearches._included = Boolean(searchBool);
			this._searchService.CreateSearch(this.cmdSearches, 'SearchText');
		}
		if (selectedSearchDropDown == 'That have at least one code from this set') {

			this.cmdSearches._withCodes = true;
			this.cmdSearches._title = this.selectedSearchCodeSetDropDown;

			this._searchService.CreateSearch(this.cmdSearches, 'SearchCodeSetCheck');

		}
		if (selectedSearchDropDown == 'That dont have any codes from this set') {

			this.cmdSearches._withCodes = false;
			this.cmdSearches._title = this.selectedSearchCodeSetDropDown;
			this._searchService.CreateSearch(this.cmdSearches, 'SearchCodeSetCheck');

		}
		if (selectedSearchDropDown == 'Without an abstract') {

			alert(selectedSearchDropDown);
			this.cmdSearches._title = searchTitle;
			
			this._searchService.CreateSearch(this.cmdSearches, 'SearchNoAbstract');
		
		}

		if (selectedSearchDropDown == 'Without any documents uploaded') {

			alert(selectedSearchDropDown);
			this.cmdSearches._title = 'Without any documents uploaded';

			this._searchService.CreateSearch(this.cmdSearches, 'SearchNoFiles');
			
		}
		if (selectedSearchDropDown == 'With at least one document uploaded') {

			this.cmdSearches._title = 'With at least one document uploaded.';
			this._searchService.CreateSearch(this.cmdSearches, 'SearchOneFile');
			
		}
		alert('got to end of return of BO from CSLA');
		this._activeModal.close('testData');

	}
	
	public nextDropDownList(num: number, val: string) {

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
			case 7: {
				break;
			}
			case 8: {
				break;
			}
			case 9: {
				break;
			}
			case 10: {
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
