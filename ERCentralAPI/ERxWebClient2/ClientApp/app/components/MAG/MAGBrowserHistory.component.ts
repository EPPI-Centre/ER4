import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { NavigationEnd } from '@angular/router';

@Component({
    selector: 'MAGBrowserHistory',
    templateUrl: './MAGBrowserHistory.component.html',
	providers: []
})

export class MAGBrowserHistory implements OnInit {
      
	constructor(
        private _location: Location,
        public _MAGBrowserHistoryService: MAGBrowserHistoryService

	) {

    }

    public MAGBrowsingHistory: NavigationEnd[] = [];

    ngOnInit() {

        this.MAGBrowsingHistory = this._MAGBrowserHistoryService.getHistory();

        console.log('really: ', this.MAGBrowsingHistory);

        //this._activatedRoute.url.subscribe(() => {

        //    console.log('testeroo: ', this._activatedRoute.snapshot.url) ; // any time url changes, this callback is fired

        //});


        //this._location.subscribe(
        //    (value: any)=> {

        //        console.log("locaton OnNext")
        //        console.log(value);

        //    }),
        //    () => {
        //        console.log("some error");
        //    };
    }

    public get IsServiceBusy(): boolean {

        return false;
    }

    goBack() {
        this._location.back();
    }

}
	