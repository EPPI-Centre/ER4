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
    public Context: string = "Loading...";
    @Output() PleaseGoTo = new EventEmitter<string>();
    @Output() PleaseGoBackHome = new EventEmitter<string>();

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
        else return false;
    }
  public CanSync(): boolean {
    return !this._zoteroService.hasPermissions;
  }
    public ZoteroSync() {                
        this.Context = "ZoteroSync";
    }
    public ZoteroSetup() {
        this.Context = "ZoteroSetup";
    }
}




