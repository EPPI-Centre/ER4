import { Component, Inject, OnInit, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';


@Component({
    selector: 'paginatorComp',
    templateUrl: './paginator.component.html',
    providers: [],
    styles: ["button.disabled {color:black; }"]
})
export class paginatorComp implements OnInit {

    constructor(private router: Router,
        public ItemListService: ItemListService    // I would like to make this generic

    ) {

	}


    onSubmit(f: string) {

    }
    private sub: any;

  value = 1;

  onEnter(value: string) {
    const val = parseInt(value);
    if (isNaN(val)) return;
    this.value = val;
    this.ItemListService.FetchParticularPage(val - 1);
  }
    
    ngOnInit() {
	
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






