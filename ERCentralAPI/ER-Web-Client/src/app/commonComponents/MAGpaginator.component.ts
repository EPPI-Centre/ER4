import { Component,  OnInit, Input } from '@angular/core';
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

    @Input() Purpose: string = "Orig";
    

    public get PagesCount(): number {
        if (this.Purpose == "References") {
            return this._magBrowserService.MAGList.pagecount;
        }
        else if (this.Purpose == "Orig") {
            return this._magBrowserService.MAGOriginalList.pagecount;
        }
        else if (this.Purpose == "CitedBy") {
            return this._magBrowserService.MagCitationsByPaperList.pagecount;
        }
        return 0;
    }
    public get TotalItems(): number {
        if (this.Purpose == "References") {
            return this._magBrowserService.MAGList.totalItemCount;
        }
        else if (this.Purpose == "Orig") {
            return this._magBrowserService.MAGOriginalList.totalItemCount;
        }
        else if (this.Purpose == "CitedBy") {
            return this._magBrowserService.MagCitationsByPaperList.totalItemCount;
        }
        return 0;
    }
    public get CurrentCount(): number {
        if (this.Purpose == "References") {
            return this._magBrowserService.MAGList.papers.length;
        }
        else if (this.Purpose == "Orig") {
            return this._magBrowserService.MAGOriginalList.papers.length;
        }
        else if (this.Purpose == "CitedBy") {
            return this._magBrowserService.MagCitationsByPaperList.papers.length;
        }
        return 0;
    }
    public get CurrentPage(): number {
        if (this.Purpose == "References") {
            return this._magBrowserService.MAGList.pageindex;
        }
        else if (this.Purpose == "Orig") {
            return this._magBrowserService.MAGOriginalList.pageindex;
        }
        else if (this.Purpose == "CitedBy") {
            return this._magBrowserService.MagCitationsByPaperList.pageindex;
        }
        return 0;
    }
  onEnter(val: string) {
    const value = parseInt(val);
    if (isNaN(value)) return;
        if (value > 0 ) {
            if (
                this.Purpose == "References"
                && this._magBrowserService.MAGList.pageindex - 1 != value
                && value <= this._magBrowserService.MAGList.pagecount
            )
            {
                this._magBrowserService.FetchParticularPage(value - 1);
            }
            else if (this.Purpose == "Orig"
                && this._magBrowserService.MAGOriginalList.pageindex - 1 != value
                && value <= this._magBrowserService.MAGOriginalList.pagecount
            )
            {
                this._magBrowserService.FetchOrigParticularPage(value - 1);
            }
            else if (this.Purpose == "CitedBy"
                && this._magBrowserService.MagCitationsByPaperList.pageindex - 1 != value
                && value <= this._magBrowserService.MagCitationsByPaperList.pagecount
            ) {
                this._magBrowserService.FetchCitedParticularPage(value - 1);
            }
        }
    }
    
    ngOnInit() {
	
    }
    nextPage() {
        if (this.Purpose == "References") {
            this._magBrowserService.FetchNextPage();
        }
        else if (this.Purpose == "Orig") {
            this._magBrowserService.FetchOrigNextPage();
        }
        else if (this.Purpose == "CitedBy") {
            this._magBrowserService.FetchCitedNextPage();
        }
        
    }
    prevPage() {
        if (this.Purpose == "References") {
            this._magBrowserService.FetchPrevPage();
        }
        else if (this.Purpose == "Orig") {
            this._magBrowserService.FetchOrigPrevPage();
        }
        else if (this.Purpose == "CitedBy") {
            this._magBrowserService.FetchCitedPrevPage();
        }
    }
    firstPage() {
        if (this.Purpose == "References") {
            this._magBrowserService.FetchFirstPage();
        }
        else if (this.Purpose == "Orig") {
            this._magBrowserService.FetchOrigFirstPage();
        }
        else if (this.Purpose == "CitedBy") {
            this._magBrowserService.FetchCitedFirstPage();
        }
    }
    lastPage() {
        if (this.Purpose == "References") {
            this._magBrowserService.FetchLastPage();
        }
        else if (this.Purpose == "Orig") {
            this._magBrowserService.FetchOrigLastPage();
        }
        else if (this.Purpose == "CitedBy") {
            this._magBrowserService
        }
    }
}






