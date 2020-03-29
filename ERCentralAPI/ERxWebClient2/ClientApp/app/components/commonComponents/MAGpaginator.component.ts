import { Component,  OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MAGListService } from '../services/MagList.service';


@Component({
    selector: 'MAGpaginatorComp',
    templateUrl: './MAGpaginator.component.html',
    providers: [],
    styles: ["button.disabled {color:black; }"]
})
export class MAGpaginatorComp implements OnInit {

    constructor(private router: Router,
        public _magListService: MAGListService 

    ) {

	}
    onSubmit(f: string) {

    }
    private sub: any;

    value = 1;
    onEnter(value: number) {
        this.value = value; 
        this._magListService.FetchParticularPage(value - 1);
    }
    
    ngOnInit() {
	
    }

    nextPage() {
       
        this._magListService.FetchNextPage();
    }
    prevPage() {
      
        this._magListService.FetchPrevPage();
    }
    firstPage() {
     
        this._magListService.FetchFirstPage();
    }
    lastPage() {
       
        this._magListService.FetchLastPage();
    }
    
}






