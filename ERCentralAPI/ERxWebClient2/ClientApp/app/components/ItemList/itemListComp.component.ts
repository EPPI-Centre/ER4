import { Component, OnInit, Input, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationListService } from '../services/WorkAllocationList.service';
import { ItemListService, Criteria, Item } from '../services/ItemList.service';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { _localeFactory } from '@angular/core/src/application_module';
import { Comparison } from '../services/comparisons.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';

@Component({
    selector: 'ItemListComp',
    templateUrl: './ItemListComp.component.html',
    providers: []
})
export class ItemListComp implements OnInit {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService,
        private _WorkAllocationService: WorkAllocationListService,
        private _magAdvancedService: MAGAdvancedService
    ) {

    }
    ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            //this.loadDefault.emit();
        }

    }
   
    //private sub: any;
    //@Output() loadDefault = new EventEmitter();
	@ViewChild('exportItemsTable') exportItemsTable!: ElementRef;

    @Input() Context: string | undefined;
    @Input() ShowItemsTable: boolean = false;
    public ShowOptions: boolean = false;
    //public ShowId: boolean = true;
    //public ShowImportedId: boolean = false;
    //public ShowShortTitle: boolean = true;
    //public ShowTitle: boolean = true;
    //public ShowYear: boolean = true;
    //public ShowAuthors: boolean = false;
    //public ShowJournal: boolean = false;
    //public ShowDocType: boolean = false;
    //public ShowInfo: boolean = false;
    //public ShowScore: boolean = false;
    public get allItemsSelected(): boolean {
        //console.log("get allItemsSelected:", this.ItemListService.ItemList.items);
        for (let i = 0; i < this.ItemListService.ItemList.items.length; i++) {
                if (this.ItemListService.ItemList.items[i].isSelected == false) return false;
        }
        return true;
    }
    public set allItemsSelected(val: boolean) {
        //console.log("aset llItemsSelected:", val);
        for (let i = 0; i < this.ItemListService.ItemList.items.length; i++) {
            this.ItemListService.ItemList.items[i].isSelected = val;
            }
	}

    private _LocalPageSize: number | null = null;
    public get LocalPageSize(): number {
        //console.log("get LocalPageSize", this._LocalPageSize, this.ItemListService.ItemList.pagesize, this.ItemListService.ListCriteria );
        if (this._LocalPageSize == null || this._LocalPageSize == 0) {
            if (this.ItemListService.ItemList.pagesize > 0) {
                this._LocalPageSize = this.ItemListService.ItemList.pagesize;
            }
            else if (this.ItemListService.ListCriteria.pageSize > 0) {
                this._LocalPageSize = this.ItemListService.ListCriteria.pageSize;
            }
            else return -1;
        }
        return this._LocalPageSize
    }
    public set LocalPageSize(val: number) {
        //console.log("set LocalPageSize", val);
        if (val < 1) val = 1;
        if (val > 4000) val = 4000;
        this._LocalPageSize = val;
    }
    ApplyNewPageSize() {
        this.ShowOptions = false;
        if (!this.ItemListService || !this.ItemListService.ItemList || !this.ItemListService.ItemList.pagesize) return;
        else if (this._LocalPageSize == this.ItemListService.ItemList.pagesize) return;
        else {
            //we want to change the page size, we'll change it
            if (this.ItemListService.ItemList && this._LocalPageSize && this.ItemListService.ListCriteria) {
                this.ItemListService.ItemList.pagesize = this._LocalPageSize;
                this.ItemListService.ListCriteria.pageSize = this._LocalPageSize;
                console.log("Changes", this.ItemListService.ListCriteria, this.ItemListService.ItemList.pagesize, this._LocalPageSize);
                //finally, can we get the current page?
                let newMaxPage = Math.floor(this.ItemListService.ItemList.totalItemCount / this._LocalPageSize);
                let Remainder = this.ItemListService.ItemList.totalItemCount % this._LocalPageSize;
                if (Remainder > 0) newMaxPage++;
                if (newMaxPage > 0) newMaxPage--;
                if (this.ItemListService.ListCriteria.pageNumber > newMaxPage) this.ItemListService.ListCriteria.pageNumber = newMaxPage;
                this.ItemListService.Refresh();
            }
        }
    }
    public get DataSource(): GridDataResult {
        //console.log('UI read itemList', this.sort);
        //if (this.ItemListService.ItemList.items[0]) 
        //console.log("AAA", (orderBy(this.ItemListService.ItemList.items, this.sort)[0] as Item).itemId
        //, this.ItemListService.ItemList.items.length);
        return {
            data: this.ItemListService.ItemList.items,
            total: this.ItemListService.ItemList.items.length 
        };
    }
    public get sort(): SortDescriptor[] {
        return this.ItemListService.sort;
    }
    public sortChange(sort: SortDescriptor[]): void {
        this.ItemListService.sortChange(sort);
    }
    ChangeSort(fieldName: string): void {
        let NewSort: SortDescriptor[] = [];
        if (this.ItemListService.sort.length > 0 && this.ItemListService.sort[0].field == fieldName) {
            let curr = this.ItemListService.sort[0];
            if (!curr.dir) curr.dir = 'asc';
            else if (curr.dir == 'asc') curr.dir = 'desc';
            else curr.dir = 'asc';
            NewSort.push(curr);
        }
        else {
            let curr: SortDescriptor = {
                field: fieldName,
                dir: 'asc'
            };
            NewSort.push(curr);
        }
        this.ItemListService.sortChange(NewSort);
    }
    sortSymbol(fieldName: string):string {
        if (this.ItemListService.sort.length > 0 && this.ItemListService.sort[0].field == fieldName) {
            if (this.ItemListService.sort[0].dir == 'asc') return "&#8593;";
            else if (this.ItemListService.sort[0].dir == 'desc') return "&#8595;";
            else return "";
        } else return "";
    }
	public LoadWorkAllocList(workAlloc: WorkAllocation, ListSubType: string) {

        //this.allItemsSelected = false;
        let crit = new Criteria();
        crit.listType = ListSubType;
		crit.workAllocationId = workAlloc.workAllocationId;
        let ListDescr: string = "";
        if (ListSubType == 'GetItemWorkAllocationListRemaining') {
			ListDescr = "work allocation remaining: " + workAlloc.attributeName;
        }
        else if (ListSubType == 'GetItemWorkAllocationListStarted') {
			ListDescr = "work allocation started: " + workAlloc.attributeName;
        }
        else if (ListSubType == 'GetItemWorkAllocationList') {
			ListDescr = "total work allocation: " + workAlloc.attributeName;
        }
        else {
            ListDescr = "work allocation (unknown)";
        }      
			this.ItemListService.FetchWithCrit(crit, ListDescr);
	}
	public LoadComparisonList(comparison: Comparison, ListSubType: string) {

		let crit = new Criteria();
		crit.listType = ListSubType;
		let typeMsg: string = '';
		if (ListSubType.indexOf('Disagree') != -1) {
			typeMsg = 'disagreements between';
		} else {
			typeMsg = 'agreements between';
		}
		let middleDescr: string = ' ' + comparison.contactName3 != '' ? ' and ' + comparison.contactName3 : '' ;
		let listDescription: string = typeMsg + '  ' + comparison.contactName1 + ' and ' +  comparison.contactName2 + middleDescr + ' using ' + comparison.setName;
		crit.description = listDescription;
		crit.listType = ListSubType;
		crit.comparisonId = comparison.comparisonId;
		console.log('checking: ' + JSON.stringify(crit) + '\n' + ListSubType);
		this.ItemListService.FetchWithCrit(crit, listDescription );

	}
    public LoadMAGAllocList(ListSubType: string) {

        console.log('got in here load magallocate', ListSubType);
        let SelectionCritieraItemList: Criteria = new Criteria();
        SelectionCritieraItemList.listType = ListSubType;
        if (ListSubType == 'MatchedIncluded') {
            SelectionCritieraItemList.listType = 'MagMatchesMatched';
            SelectionCritieraItemList.onlyIncluded = true;
        } else if (ListSubType == 'MatchedExcluded') {
            SelectionCritieraItemList.listType = 'MagMatchesMatched';
            SelectionCritieraItemList.onlyIncluded = false;

        } else if (ListSubType == 'MagMatchesNeedingCheckingInc') {
            SelectionCritieraItemList.listType = 'MagMatchesNeedingChecking';
            SelectionCritieraItemList.onlyIncluded = true;
        } else if (ListSubType == 'MagMatchesNeedingCheckingExc') {
            SelectionCritieraItemList.listType = 'MagMatchesNeedingChecking';
            SelectionCritieraItemList.onlyIncluded = false;
        } else if (ListSubType == 'MagMatchesNotMatchedInc') {
            SelectionCritieraItemList.listType = 'MagMatchesNotMatched';
            SelectionCritieraItemList.onlyIncluded = true;
        } else if (ListSubType == 'MagMatchesNotMatchedExc') {
            SelectionCritieraItemList.listType = 'MagMatchesNotMatched';
            SelectionCritieraItemList.onlyIncluded = false;
        } else if (ListSubType == 'MagSimulationTP') {

            SelectionCritieraItemList.listType = "MagSimulationTP";
            SelectionCritieraItemList.magSimulationId = this._magAdvancedService.CurrentMagSimId;

        } else if (ListSubType == 'MagSimulationFN') {

            SelectionCritieraItemList.listType = "MagSimulationFN";
            SelectionCritieraItemList.magSimulationId = this._magAdvancedService.CurrentMagSimId;

        }else {

            SelectionCritieraItemList.listType = "MagMatchesMatched";
            SelectionCritieraItemList.showDeleted = false;
            SelectionCritieraItemList.pageNumber = 0;
            
        }
        this.ItemListService.FetchWithCrit(SelectionCritieraItemList, ListSubType);
    }
    OpenItem(itemId: number) {

		if (itemId > 0) {
			// , { queryParams: { page: pageNum } }
			//if (this.Context == 'FullUI') this.router.navigate(['itemcoding', { queryParams: { itemId: itemId } }]);
            if (this.Context == 'FullUI') this.router.navigate(['itemcoding', itemId]);
            else if (this.Context == 'CodingOnly') this.router.navigate(['itemcodingOnly', itemId]);
            else alert("Sorry, don't know where we are, can't send you anywhere...");
        } 
	}

    
    nextPage() {
        this.ItemListService.FetchNextPage();
    }
    prevPage() {
        this.ItemListService.FetchPrevPage();

    }
    firstPage() {
        this.ItemListService.FetchFirstPage();
    }
    lastPage() {
        this.ItemListService.FetchLastPage();
    }
    ToggleOptionsPanel() {
        this.ShowOptions = !this.ShowOptions;
    }
    IncludedItemList() {
        this.ItemListService.GetIncludedItems();
    }
    ExcludedItemList() {
        this.ItemListService.GetExcludedItems();
    }
    DeletedItemList() {
        this.ItemListService.GetDeletedItems();
    }

    public EnhancedTableSelections: boolean = true;
    public ShowSelectionHelp: boolean = false;
    private _LastSelectedItem: Item | null = null;
    public thisIsTheLastSelectedItem(item: Item) {
        if (this._LastSelectedItem == null || !this.EnhancedTableSelections) return false;
        else return (item.itemId == this._LastSelectedItem.itemId);
    }
    public ItemRowClicked(event: any, itm: Item) {
        //console.log("ItemRowClicked", event.ctrlKey, event.altKey, itm.itemId);
        if (this.EnhancedTableSelections == false) {
            this._LastSelectedItem = null
            return;
        }
        if (event.ctrlKey) {
            itm.isSelected = !itm.isSelected;
        }
        else if (this._LastSelectedItem != null && event.altKey) {
            const indx1 = this.ItemListService.ItemList.items.indexOf(this._LastSelectedItem);
            if (indx1 == -1) {
                //last selected item exists does not appear in the current list
                this._LastSelectedItem = null;
                itm.isSelected = !itm.isSelected;
            }
            else {//last selected item exists and is in the current list
                const indx2 = this.ItemListService.ItemList.items.indexOf(itm);
                if (indx2 != -1) {
                    let range: Item[] = [];
                    if (indx1 < indx2) {
                        range = this.ItemListService.ItemList.items.slice(indx1, indx2 + 1);
                    } else if (indx2 < indx1) {
                        range = this.ItemListService.ItemList.items.slice(indx2, indx1 + 1);
                    } else {
                        //just one item!
                        range.push(itm);
                    }
                    for (let titem of range) {
                        titem.isSelected = true;
                    }
                }

            }
        } else if (this._LastSelectedItem == null && event.altKey) {
            //no range, but we won't remove selection from all other items...
            itm.isSelected = !itm.isSelected;
        }
        else { //no modifier, so we'll remove all selections and select only the current item.
            let itms = this.ItemListService.SelectedItems.slice(0);
            let aim = itm.isSelected ? true : false;
            for (let i of itms) {
                i.isSelected = false;
            }
            itm.isSelected = !aim;
        }
        this._LastSelectedItem = itm;
    }

}






