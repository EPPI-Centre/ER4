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
import { ReviewSetsService } from '../services/ReviewSets.service';

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

	//public checkboxOnly = false;
	//public mode = 'multiple';
	//public selectableSettings: SelectableSettings | null | undefined;


	//public setSelectableSettings(): void {

	//	this.selectableSettings = {

	//		checkboxOnly: this.checkboxOnly,
	//		mode: 'multiple'
	//	};
	//}

    public selectedAll: boolean = false;

    //private _dataSource: MatTableDataSource<any> | null = null;
    //public get dataSource(): MatTableDataSource<any>  {
    //    console.log('Getting searches data for table');
    //    if (this._dataSource == null) {
    //        this._dataSource = new MatTableDataSource(this._searchService.SearchList);
    //        this._dataSource.sort = this.sort;
    //        console.log('table length: ' + this._dataSource.data.length + ' Searchlist length: ' + this._searchService.SearchList.length);
    //    }
    //    return this._dataSource;
    //} 

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

		//alert('bllaaaahhh: ' + this.searchesGrid.columns.length);
		//alert(JSON.stringify(dataItem));
		//alert(dataItem.searchId);

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

		//cr.setId = item.setId;
		//cr.attributeid = item.attributeId;
		//// the below should get its value from the view radio buttons
		//cr.onlyIncluded = item.isIncluded;
		cr.filterAttributeId = -1;
		cr.searchId = item;
		cr.listType = 'GetItemSearchList';
		//cr.attributeSetIdList = item.attributeSetId;
		console.log('searchid is: ' + item);
		this.ItemListService.FetchWithCrit(cr, "GetItemSearchList");
        this._eventEmitter.PleaseSelectItemsListTab.emit();
	}

	ngOnDestroy() {
        //if (this.SearchesChanged) this.SearchesChanged.unsubscribe();
	}
	
	public tableArr: Search[] = this._searchService.SearchList;
	public testArr: Search[] = [];
	public selectAll: boolean =false;

	public initialSelection = [];
	public allowMultiSelect = true;
	public selection = new SelectionModel<Search>(this.allowMultiSelect, this.initialSelection);

	//public columnNames = [
	//{
	//	id: "selected",
	//	value: "Selected"
	//},
	//{
	//	id: "searchId",
	//	value: "SearchId"
	//},
	//{
	//	id: "title",
	//	value: "Title"
	//},
	//{
	//	id: "contactName",
	//	value: "ContactName"
	//},
	//{
	//	id: "searchDate",
	//	value: "SearchDate"
	//},
	//{
	//	id: "hitsNo",
	//	value: "HitsNo"

	//}];

	//testAlert() {

	//	alert('Not implemented');
	//}

	//updateCheck() {

	//	if (this.selectAll === true) {
	//		this.selectAll = false;
	//		this.tableArr.map((r) => {
	//			r.selected = false;
	//		});

	//	} else {
	//		this.selectAll = true;
	//		this.tableArr.map((r) => {
	//			r.selected = true;
	//		});
	//	}
	//}


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
	public nodeSelected: boolean = false;
	public selectedNodeDataName: string = '';

	_title: string = '';
	_answers: string = '';
	_included: boolean = false;
	_withCodes: boolean = false;;
	_searchId: number = 0;

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

		_title: '',
		_answers: '',
		_included: false,
		_withCodes: false,
		_searchId: 0
	};

	public withCode: boolean = false;
	public attributeNames: string = '';

	options = [1, 2, 3];
	optionSelected: any;

	onOptionSelected(event: any) {

		console.log(event); //option value will be sent as event
	}


	callSearches(selectedSearchDropDown: string, searchBool: boolean) {


		//alert(selectedSearchDropDown + '    asd ...==>>' + searchBool + ' ' + JSON.stringify(this.reviewSetsService.selectedNode ));

		let searchTitle: string = '';

		// Need to pull in here the attributeIDs and attributeNames
		if (selectedSearchDropDown.search('With this code') != -1) {

			this.withCode = true

			if (this.reviewSetsService.selectedNode != undefined) {

				let tmpID: number = this.reviewSetsService.selectedNode.attributeSetId;
				this.attributeNames = this.reviewSetsService.selectedNode.name;
				this.cmdSearches._answers = String(tmpID);
				alert(this.reviewSetsService.selectedNode);
			

			searchTitle = this.withCode == true ?
					"Coded with: " + this.attributeNames : "Not coded with: " + this.attributeNames;
			}
		}

		this.cmdSearches._title = searchTitle;
		this.cmdSearches._included = Boolean(searchBool);
		this.cmdSearches._withCodes = this.withCode;
		this.cmdSearches._searchId = 0;

		alert(JSON.stringify(this.cmdSearches));

		this._searchService.FetchSearchCodes(this.cmdSearches);

		this.activeModal.dismiss();

	}
	
	public nextDropDownList(num: number, val: string) {

		//console.log('got here');
		this.showDropDown2 = true;
		this.showTextBox = false;
		this.selectedSearchDropDown = val;
		switch (num) {

			case 1: {
				this.dropDownList = this.reviewSetsService.ReviewSets;
				this.showDropDown2 = false;
				this.showTextBox = false;
				break;
			}
			case 2: {
				//statements; 
				this.dropDownList = this.reviewSetsService.ReviewSets;
				this.showDropDown2 = false;
				this.showTextBox = false;
				break;
			}
			case 3: {
				//With these internal IDs (comma separated) show text box
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = false;
				this.showTextBox = true;
				break;
			}
			case 4: {
				//statements; 
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = true;
				this.showTextBox = true;
				break;
			}
			case 5: {
				//that have at least one code from this set
				this._eventEmitter.nodeSelected = false;
				this.showTextBox = false;
				this.showDropDown2 = true;
				this.dropDownList = this.reviewSetsService.ReviewSets;
				break;
			}
			case 6: {
				//that don't have any codes from this set
				this._eventEmitter.nodeSelected = false;
				this.showTextBox = false;
				this.showDropDown2 = true;
				this.dropDownList = this.reviewSetsService.ReviewSets;
				break;
			}
			case 7: {
				//statements; 
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = true;
				this.showTextBox = false;
				break;
			}
			case 8: {
				//statements; 
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = false;
				this.showTextBox = false;
				break;
			}
			case 9: {
				//statements;
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = false;
				this.showTextBox = false;
				break;
			}
			case 10: {
				//statements; 
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = false;
				this.showTextBox = false;
				break;
			}
			default: {
				//statements; 
				break;
			}
		}
	}

	public focus(canWrite: boolean) {
		this.canWrite = canWrite;
		//this.InfoBoxText.nativeElement.focus();
	}
}
