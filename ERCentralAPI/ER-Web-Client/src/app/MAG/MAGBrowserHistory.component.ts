import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import {  Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagBrowseHistoryItem } from '../services/MAGClasses.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MAGAdminService } from '../services/MAGAdmin.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

@Component({
    selector: 'MAGBrowserHistory',
    templateUrl: './MAGBrowserHistory.component.html',
    providers: []
})

export class MAGBrowserHistory implements OnInit {

    constructor(
        private notificationService: ConfirmationDialogService,
        public _MAGBrowserHistoryService: MAGBrowserHistoryService,
        public _magAdvancedService: MAGAdvancedService,
        public _magBasicService: MAGRelatedRunsService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private router: Router,
        public _eventEmitterService: EventEmitterService,
        private _magBrowserService: MAGBrowserService,
        public _magAdminService: MAGAdminService
    ) {

    }

    public magBrowseHistoryList: MagBrowseHistoryItem[] = [];
    ngOnInit() {
        this.fetchMAGHistory();
    }

    @Output() PleaseGoTo = new EventEmitter<string>();
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    GoToUrl(index: number) {

        console.log('go to url', index);
        this.NavigateToThisPoint(index);
    }
    RemoveUrl(index: number) {

        if (index != -1) {
            this._MAGBrowserHistoryService._MAGBrowserHistoryList.splice(index,1);
        }
    }
    public ClearHistory() {
        this._MAGBrowserHistoryService._MAGBrowserHistoryList = [];
    }
    public get IsServiceBusy(): boolean {
        if (
             this._magBrowserService.IsBusy
            || this._magBasicService.IsBusy
            || this._magAdvancedService.IsBusy
            || this._MAGBrowserHistoryService.IsBusy
            || this._magAdminService.IsBusy
        ) return true;
        return false;
    }
    Back() {
        //this.router.navigate(['Main']);
    }
    fetchMAGHistory() {
        this._MAGBrowserHistoryService.FetchMAGBrowserHistory();
    }
    public async NavigateToThisPoint(browsePosition: number) {
        let res = await this._MAGBrowserHistoryService.NavigateToThisPoint(browsePosition);
        if (res != "") this.PleaseGoTo.emit(res);
    }
}
