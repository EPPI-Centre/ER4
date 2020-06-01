import { Component,  OnInit } from '@angular/core';
import { MAGBrowserService } from '../services/MAGBrowser.service';

@Component({
    selector: 'MAGpaginatorComp',
    templateUrl: './MAGpaginator.component.html',
    providers: [],
    styles: ["button.disabled {color:black; }"]
})
export class MAGpaginatorComp implements OnInit {

    constructor(public _magBrowserService: MAGBrowserService 
    ) {

	}
    onSubmit(f: string) {

    }

    value = 1;
    onEnter(value: number) {
        this.value = value; 
        this._magBrowserService.FetchParticularPage(value - 1);
    }
    
    ngOnInit() {
	
    }
    nextPage() {
       
        this._magBrowserService.FetchNextPage();
    }
    prevPage() {
      
        this._magBrowserService.FetchPrevPage();
    }
    firstPage() {
     
        this._magBrowserService.FetchFirstPage();
    }
    lastPage() {
       
        this._magBrowserService.FetchLastPage();
    }
}






