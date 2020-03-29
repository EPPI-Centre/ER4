import { Component, Inject, OnInit, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { ItemListService, Criteria, Item, ItemList } from '../services/MagList.service';


@Component({
    selector: 'MAGpaginatorComp',
    templateUrl: './MAGpaginator.component.html',
    providers: [],
    styles: ["button.disabled {color:black; }"]
})
export class paginatorComp implements OnInit {

    constructor(private router: Router,
        public MagListService: MagListService    // I would like to make this generic

    ) {

	}


    onSubmit(f: string) {

    }
    private sub: any;

    value = 1;
    onEnter(value: number) {
        this.value = value; 
        this.MagListService.FetchParticularPage(value - 1);
    }
    
    ngOnInit() {
	
    }

    nextPage() {
       
        this.MagListService.FetchNextPage();
    }
    prevPage() {
      
        this.MagListService.FetchPrevPage();
    }
    firstPage() {
     
        this.MagListService.FetchFirstPage();
    }
    lastPage() {
       
        this.MagListService.FetchLastPage();
    }
    
}






