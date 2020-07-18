import { Component,  OnInit } from '@angular/core';
import { MAGBrowserService } from '../services/MAGBrowser.service';

@Component({
    selector: 'MAGOrigpaginatorComp',
    templateUrl: './MAGOrigpaginator.component.html',
    providers: [],
    styles: ["button.disabled {color:black; }"]
})
export class MAGOrigpaginatorComp implements OnInit {

    constructor(public _magBrowserService: MAGBrowserService 
    ) {

	}
    onSubmit(f: string) {

    }

    value = 1;
    onEnter(value: number) {
        this.value = value; 
        this._magBrowserService.FetchOrigParticularPage(value - 1);
    }
    
    ngOnInit() {
	
    }
    nextPage() {
       
        this._magBrowserService.FetchOrigNextPage();
    }
    prevPage() {
      
        this._magBrowserService.FetchOrigPrevPage();
    }
    firstPage() {
     
        this._magBrowserService.FetchOrigFirstPage();
    }
    lastPage() {
       
        this._magBrowserService.FetchOrigLastPage();
    }
}






