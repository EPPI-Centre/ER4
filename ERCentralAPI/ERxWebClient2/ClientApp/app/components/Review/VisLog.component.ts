import { Component, Inject, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { WebDBService, iWebDB, iWebDBLog } from '../services/WebDB.service';
import { NotificationService } from '@progress/kendo-angular-notification';




@Component({
    selector: 'VisLogComp',
    templateUrl: './VisLog.component.html', 
    providers: []
})




export class VisLogComp implements OnInit{

    public value: Date = new Date(2019, 5, 1, 22);
    public format = "MM/dd/yyyy HH:mm";
    public valueKendoDatepickerCombine1: Date = new Date();

    constructor(
        private visLogService: WebDBService      
        ) {
    }

    ngOnInit() {
        // IF the current logs are empty and CurrentDB is not null, 
        // CALL GetWebDBLogs(...) with the correct WebDBid and 
        // some default values for "from, until, logType".

        if ((this.visLogService.CurrentLogs.length == 0) && (this.visLogService.CurrentDB != null)) {
            this.visLogService.GetWebDBLogs(this.visLogService.CurrentDB.webDBId, "1980/01/01 00:00:00", "1980/01/01 00:00:00", "All");
        }

    }


    // needso to  be 'public' for compiling in production mode
    public get CurrentLogs(): iWebDBLog[] {
        return this.visLogService.CurrentLogs;
    }

    GetLogs(CurrentDB: iWebDB) {
        this.visLogService.GetWebDBLogs(CurrentDB.webDBId, "1980/01/01 00:00:00", "1980/01/01 00:00:00", "All");
    }


}






