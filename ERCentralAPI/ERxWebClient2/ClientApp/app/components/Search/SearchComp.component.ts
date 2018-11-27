import { Component, OnInit, Input, OnDestroy, ViewEncapsulation, ViewChild } from '@angular/core';
import { forEach } from '@angular/router/src/utils/collection';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';
import { debounceTime, startWith, merge, switchMap, share  } from 'rxjs/operators/index';
import { pipe } from 'rxjs'
import { style } from '@angular/animations';
import { searchService, Search, SearchCodeCommand } from '../services/search.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { SelectionModel } from '@angular/cdk/collections';
import { RowClassArgs, GridDataResult, RowArgs, SelectableSettings, GridModule, GridComponent  } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { NgbModal, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ReviewSetsService, SetAttribute } from '../services/ReviewSets.service';

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
		private modalService: NgbModal,
		private reviewSetService: ReviewSetsService
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

		//let tmp: number = 0;
		let lstStrSearchIds = '';
		// check which rows have been selected already
		for (var i = 0; i < this.DataSource.data.length; i++) {

			if (this.DataSource.data[i].add == true) {
			//	tmp += 1;
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

			//data.additionalText = infoTxt;
			//if (!data.isSelected) {


			//	this.CheckBoxClickedAfterCheck('InfoboxTextAdded', data);
			//}
			//else {

			//	this.CheckBoxClickedAfterCheck('InfoboxTextUpdate', data);
			//}
		},
			() => {

				alert('testing 123 correct');
			}
		);
	}


	public checkboxClicked(dataItem: any) {

		dataItem.add = !dataItem.add;
		//alert(dataItem.add);
		if (dataItem.add == true) {
			this._searchService.searchToBeDeleted = dataItem.searchId;
		} else {
			this._searchService.searchToBeDeleted = '';
		}
		//this._searchService.removeHandler(dataItem);
		console.log(this._searchService.searchToBeDeleted );
	};
	   

    //@ViewChild(MatSort) sort!: MatSort;
    //private SearchesChanged: Subscription = new Subscription();
    public RebuildDataTableSource() {
        //this._dataSource = new MatTableDataSource(this._searchService.SearchList);
        //this._dataSource.sort = this.sort;
        //console.log('table length: ' + this._dataSource.data.length + ' Searchlist length: ' + this._searchService.SearchList.length);
	}

	//displayedColumns: string [] | undefined;
	RemoveOneLocalSource() {

        console.log(' Searchlist length: ' + this._searchService.SearchList.length);
        this._searchService.SearchList = this._searchService.SearchList.slice(3);
		console.log(' Searchlist length: ' + this._searchService.SearchList.length);

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
        //this.loadProducts();
    }
    value = 1;
    onEnter(value: number) {
        this.value = value ;
        this.ItemListService.FetchParticularPage(value-1);
    }
    
	dataTable: any;


	OpenItems(item: number) {

		let cr: Criteria = new Criteria();

		cr.filterAttributeId = -1;
		cr.searchId = item;
		cr.listType = 'GetItemSearchList';
		cr.pageNumber = 0;
		alert('cr.listType: ' + cr.listType);

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

	//InfoBoxText!: ElementRef;

	private canWrite: boolean = true;
	public dropDownList: any = null;
	public showTextBox: boolean = false;
	public showDropDown2: boolean = true;
	public selectedSearchDropDown: string = '';
	public selectedSearchTextDropDown: string = '';
	public nodeSelected: boolean = false;
	public selectedNodeDataName: string = '';

	_searchText: string = '';
	_title: string = '';
	_answers: string = '';
	_included: boolean = false;
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

	test() {

		alert('hello again');

	}

	public cmdSearches: SearchCodeCommand = {

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

	options = [1, 2, 3];
	optionSelected: any;

	onOptionSelected(event: any) {

		console.log(event); //option value will be sent as event
	}


	
	callSearches(selectedSearchDropDown: string, searchBool: boolean) {

		let searchTitle: string = '';
		let firstNum: boolean = selectedSearchDropDown.search('With this code') != -1;
		let secNum: boolean = selectedSearchDropDown.search('Without this code') != -1
		this.cmdSearches._included = Boolean(searchBool);
		this.cmdSearches._withCodes = this.withCode;
		this.cmdSearches._searchId = 0;

		alert(selectedSearchDropDown + ' '  + searchBool);

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

			alert('Along the write lines...');
			this.cmdSearches._title = this.searchText;
			this.cmdSearches._searchText = this.selectedSearchTextDropDown;
			this.cmdSearches._included = Boolean(searchBool);
			this._searchService.FetchSearchGeneric(this.cmdSearches, 'SearchText');
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

		//console.log('got here');
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
				this.showDropDown2 = true;
				this.dropDownList = this.reviewSetsService.ReviewSets;
				break;
			}
			case 6: {
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


	public setSearchTextDropDown(balh: any) {

		this.selectedSearchTextDropDown = balh;
		alert('okay');
	}

	public focus(canWrite: boolean) {
		this.canWrite = canWrite;
		//this.InfoBoxText.nativeElement.focus();
	}
}
