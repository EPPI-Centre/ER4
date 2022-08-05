import { Component, OnInit, ViewChild, EventEmitter, Input, Output } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ZoteroService } from '../services/Zotero.service';

@Component({
    selector: 'ZoteroHeaderBar',
    templateUrl: './ZoteroHeaderBar.component.html',
    providers: []
})
export class ZoteroHeaderBarComp implements OnInit {

    constructor(private router: Router,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        public _notificationService: NotificationService,
        public _eventEmitterService: EventEmitterService,
        public _confirmationDialogService: ConfirmationDialogService,
        public _zoteroService: ZoteroService
    ) {

    }


    ngOnInit() {

    }
    public Context: string = "ZoteroSetup";
    @Output() PleaseGoTo = new EventEmitter<string>();
    @Output() PleaseGoBackHome = new EventEmitter<string>();
    @Output() IHaveImportedSomething = new EventEmitter<void>();

    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get isSiteAdmin(): boolean {
        return this._ReviewerIdentityServ.reviewerIdentity.isSiteAdmin;
    }
    BackHome() {
        this.PleaseGoBackHome.emit();
    }
   
    public DisableButton(destination: string) {
        if (this.Context == undefined || !this.HasWriteRights) return false;
        else if (this.Context == destination) return true;
        //else if (this.MustMatchItems && destination != "matching" && destination != "MagSearch") return true;
        else return false;
    }
    public CheckKeyAndGroup(): boolean {

        return !this._zoteroService.editApiKeyPermissions;
    }
    public get CanGoForward(): boolean {
        //if (this._mAGBrowserHistoryService.currentBrowsePosition <
        //    this._mAGBrowserHistoryService._MAGBrowserHistoryList.length - 1)
            return true;
        //else return false;
    }
    public get CanGoBackwards(): boolean {
    /*    if (this._mAGBrowserHistoryService.currentBrowsePosition > 0)*/
            return true;
        //else return false;
    }
    public async Forward() {
        //let res = await this._mAGBrowserHistoryService.NavigateToThisPoint(this._mAGBrowserHistoryService.currentBrowsePosition + 1);
        //if (res != "") this.PleaseGoTo.emit(res);
        //this._location.forward();
    }
    public async Back() {
        //let res = await this._mAGBrowserHistoryService.NavigateToThisPoint(this._mAGBrowserHistoryService.currentBrowsePosition - 1);
        //if (res != "") this.PleaseGoTo.emit(res);
        //this._location.back();
    }

    public ZoteroSync() {                
        this.Context = "ZoteroSync";
    }
    public ZoteroSetup() {
        this.Context = "ZoteroSetup";
    }
}




