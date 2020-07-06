import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import {  Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdminService } from '../services/MAGAdmin.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

@Component({
    selector: 'MAGAdmin',
    templateUrl: './MAGAdmin.component.html',
    providers: []
})

export class MAGAdminComp implements OnInit {

    constructor(
        private _location: Location,
        public _MAGBrowserHistoryService: MAGBrowserHistoryService,
        public _magAdminService: MAGAdminService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        public _confirmationDialogService: ConfirmationDialogService,
        private router: Router

    ) {

    }
    public previousMAG: string = '';
    public latestMag: string = '';

    public DoCheckChangedPaperIds() {

        let msg: string = "Are you sure?\nPlease check it is not already running first!\nOld: "
            + this.previousMAG + " new: " + this.latestMag;
        this._confirmationDialogService.confirm('MAG Admin', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magAdminService.DoCheckChangedPaperIds(latestMag);
                }
            });
        
    }
    ngOnInit() {


    }

   

}
