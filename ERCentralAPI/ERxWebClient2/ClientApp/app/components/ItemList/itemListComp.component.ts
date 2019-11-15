import { Component, OnInit, Input, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationListService } from '../services/WorkAllocationList.service';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { _localeFactory } from '@angular/core/src/application_module';
import { Comparison } from '../services/comparisons.service';

@Component({
    selector: 'ItemListComp',
    templateUrl: './ItemListComp.component.html',
    providers: []
})
export class ItemListComp implements OnInit {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService,
		private _WorkAllocationService: WorkAllocationListService
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
        //console.log("get LocalPageSize", this._LocalPageSize);
        if (this._LocalPageSize == null && this.ItemListService.ItemList) {
            this._LocalPageSize = this.ItemListService.ItemList.pagesize;
        }
        else if (this._LocalPageSize == null) return -1;
        else return this._LocalPageSize;
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
    //selectAllItems(e: any): void {
    //    if (e.target.checked) {
    //        this.allItemsSelected = true;
    //        for (let i = 0; i < this.ItemListService.ItemList.items.length; i++) {
    //            this.ItemListService.ItemList.items[i].isSelected = true;
    //        }
    //    }
    //    else {
    //        this.allItemsSelected = false;
    //        for (let i = 0; i < this.ItemListService.ItemList.items.length; i++) {
    //            this.ItemListService.ItemList.items[i].isSelected= false;
    //        }
    //    }
    //}
}






