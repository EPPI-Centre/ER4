import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { NavigationEnd, Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';

@Component({
    selector: 'MAGBrowserHistory',
    templateUrl: './MAGBrowserHistory.component.html',
    providers: []
})

export class MAGBrowserHistory implements OnInit {

    constructor(
        private _location: Location,
        public _MAGBrowserHistoryService: MAGBrowserHistoryService,
        public _magAdvancedService: MAGAdvancedService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private router: Router

    ) {

    }

    public MAGBrowsingHistory: NavigationEnd[] = [];

    ngOnInit() {

        this.MAGBrowsingHistory = this._MAGBrowserHistoryService.getHistory();
        this.fetchMAGHistory();
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    GoToUrl(url: string) {

        console.log(url);
        this.router.navigate([url]);
    }
    RemoveUrl(item: NavigationEnd) {

        let id: number = item.id;
        let index: number = this.MAGBrowsingHistory.findIndex(x => x.id == id);
        if (index != -1) {
        this.MAGBrowsingHistory.splice(index,1);
        }
    }
    public ClearHistory() {
        this._MAGBrowserHistoryService.ClearHistory();
    }
    public get IsServiceBusy(): boolean {

        return false;
    }
    Back() {
        this._location.back();
    }
    fetchMAGHistory() {

        this._MAGBrowserHistoryService.FetchMAGBrowserHistory();
    }
}
