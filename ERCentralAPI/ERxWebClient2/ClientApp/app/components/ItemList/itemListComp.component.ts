import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';

@Component({
    selector: 'ItemListComp',
    templateUrl: './ItemListComp.component.html',
    providers: []
})
export class ItemListComp implements OnInit {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService,
        private _WorkAllocationService: WorkAllocationContactListService
    ) {

    }
    onSubmit(f: string) {

    }
    //private sub: any;
    //@Output() loadDefault = new EventEmitter();

    @Input() Context: string | undefined;
    public get DataSource(): GridDataResult {
        //console.log('UI read itemList', this.sort);
        //if (this.ItemListService.ItemList.items[0]) 
        //console.log("AAA", (orderBy(this.ItemListService.ItemList.items, this.sort)[0] as Item).itemId
        //, this.ItemListService.ItemList.items.length);
        return {
            data: orderBy(this.ItemListService.ItemList.items, this.sort),
            total: this.ItemListService.ItemList.items.length 
        };
    }
    public sort: SortDescriptor[] = [{
        field: 'shortTitle',
        dir: 'asc'
    }];
    public sortChange(sort: SortDescriptor[]): void {

        this.sort = sort;
        console.log('sorting items by ' + this.sort[0].field + " ");
        //this.loadProducts();
    }

    
    public LoadWorkAllocList(workAlloc: WorkAllocation, ListSubType: string) {
	
        let crit = new Criteria();
        crit.listType = ListSubType;
        crit.workAllocationId = workAlloc.workAllocationId;
        let ListDescr: string = "Showing ";
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

    OpenItem(itemId: number) {

		if (itemId > 0) {
			// , { queryParams: { page: pageNum } }
			//if (this.Context == 'FullUI') this.router.navigate(['itemcoding', { queryParams: { itemId: itemId } }]);
            if (this.Context == 'FullUI') this.router.navigate(['itemcoding', itemId]);
            else if (this.Context == 'CodingOnly') this.router.navigate(['itemcodingOnly', itemId]);
            else alert("Sorry, don't know where we are, can't send you anywhere...");
        } 
	}

    ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            //this.loadDefault.emit();
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
        
}






