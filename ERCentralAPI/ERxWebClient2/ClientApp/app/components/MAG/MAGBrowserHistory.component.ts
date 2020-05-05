import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { BasicMAGService } from '../services/BasicMAG.service';
import { ActivatedRoute, Router } from '@angular/router';


@Component({
    selector: 'MAGBrowserHistory',
    templateUrl: './MAGBrowserHistory.component.html',
	providers: []
})

export class MAGBrowserHistory implements OnInit {

   

	constructor(
        private _location: Location,
        private _basicMAGService: BasicMAGService,
        private _activatedRoute: ActivatedRoute

	) {

    }

    ngOnInit() {

        this._activatedRoute.url.subscribe(() => {

            console.log(this._activatedRoute.snapshot.url) ; // any time url changes, this callback is fired

        });


        this._location.subscribe(
            (value: any)=> {

                console.log("locaton OnNext")
                console.log(value);

            }),
            () => {
                console.log("some error");
            };
    }

    public get IsServiceBusy(): boolean {

        return false;
    }

    goBack() {
        this._location.back();
    }


   
}
	