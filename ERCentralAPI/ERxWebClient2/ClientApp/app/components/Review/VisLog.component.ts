import { Component, Inject, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { WebDBService, iWebDB, iWebDBLog } from '../services/WebDB.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { RowClassArgs, GridDataResult, SelectableSettings, SelectableMode, PageChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { Helpers } from '../helpers/HelperMethods';




@Component({
    selector: 'VisLogComp',
    templateUrl: './VisLog.component.html', 
    providers: []
})




export class VisLogComp implements OnInit{
    constructor(
        private visLogService: WebDBService
    ) {
    }

    ngOnInit() {
        // IF the current logs are empty and CurrentDB is not null, 
        // CALL GetWebDBLogs(...) with the correct WebDBid and 
        // some default values for "from, until, logType".

        if ((this.visLogService.CurrentLogs.length == 0) && (this.visLogService.CurrentDB != null)) {
            const currDB = this.visLogService.CurrentDB;
            setTimeout(() => { this.visLogService.GetWebDBLogs(currDB.webDBId, "1980/01/01 00:00:00", "1980/01/01 00:00:00", "All") }, 20);
        }

    }

    //public value: Date = new Date(2019, 5, 1, 22);
    //public value: Date = new Date(2000, 2, 10, 10, 30, 0);
    public valueKendoDatepickerFrom: Date = new Date(2021, 0, 1, 12, 0, 0);
    public valueKendoDatepickerUntil: Date = new Date();
    public LogTypeSelection: number = 0;

    public get LogDataGet(): GridDataResult {
        return {
            data: orderBy(this.visLogService.CurrentLogs, this.sortLogs).slice(this.skip, this.skip + this.pageSize),
            total: this.visLogService.CurrentLogs.length,
        };
    }
    public sortLogs: SortDescriptor[] = [{
        field: 'webDBLogIdentity',
        dir: 'desc'
    }];
    public pageSize = 100;
    public skip = 0;
    protected pageChange({ skip, take }: PageChangeEvent): void {
        this.skip = skip;
        this.pageSize = take;
    }
    public sortChangeLogs(sort: SortDescriptor[]): void {
        this.sortLogs = sort;
    }

    

    //public get IsServiceBusy1(): boolean {
    //    return (this.visLogService.IsBusy);
    //}

    public visLogDate: string = 'dateFrom';

    // needso to  be 'public' for compiling in production mode
    public get CurrentLogs(): iWebDBLog[] {
        return this.visLogService.CurrentLogs;
    }

    GetLogs(CurrentDB: iWebDB) {
        this.visLogService.GetWebDBLogs(CurrentDB.webDBId, "1980/01/01 00:00:00", "1980/01/01 00:00:00", "All");
    }

    FormatDate(DateSt: string): string {
        return Helpers.FormatDateTime(DateSt);
    }

    public RetrieveLog() {
        if (this.visLogService.CurrentDB != null) {

            var logTypeSelected = "";
            switch (this.LogTypeSelection) {
                case 1: logTypeSelected = "Login"; break;
                case 2: logTypeSelected = "Search"; break;
                case 3: logTypeSelected = "GetFrequency"; break;
                case 4: logTypeSelected = "GetSetFrequency"; break;
                case 5: logTypeSelected = "GetFrequencyNewPage"; break;
                case 6: logTypeSelected = "GetItemList"; break;
                case 7: logTypeSelected = "GetMap"; break;
                case 8: logTypeSelected = "ItemDetailsFromList"; break;
                default: logTypeSelected = "All";
            }

            var untilDate = "1980/01/01 00:00:00";
            if (this.visLogDate == "dateUntil") {
                var targetDate = new Date(this.valueKendoDatepickerUntil);
                var uFullYear = targetDate.getFullYear();
                var uMonth = targetDate.getMonth() + 1;
                var uDate = targetDate.getDate();
                untilDate = uFullYear.toString() + "/" + uMonth.toString() + "/" + uDate.toString() + " 23:59:59";
            }

            var targetDate = new Date(this.valueKendoDatepickerFrom);
            var fFullYear = targetDate.getFullYear();
            var fMonth = targetDate.getMonth() + 1;
            var fDate = targetDate.getDate();
            var fromDate = fFullYear.toString() + "/" + fMonth.toString() + "/" + fDate.toString() + " 00:00:00";

            this.visLogService.GetWebDBLogs(this.visLogService.CurrentDB.webDBId, fromDate, untilDate, logTypeSelected);
            this.LogDataGet;
        }

    }

    public RetrieveLogFrom() {
        if (this.visLogService.CurrentDB != null) {

            var logTypeSelected = "";
            switch (this.LogTypeSelection) {
                case 1: logTypeSelected = "Login"; break;
                case 2: logTypeSelected = "Search"; break;
                case 3: logTypeSelected = "GetFrequency"; break;
                case 4: logTypeSelected = "GetSetFrequency"; break;
                case 5: logTypeSelected = "GetFrequencyNewPage"; break;
                case 6: logTypeSelected = "GetItemList"; break;
                case 7: logTypeSelected = "GetMap"; break;
                case 8: logTypeSelected = "ItemDetailsFromList"; break;
                default: logTypeSelected = "All";
            }
         
            var targetDate = new Date(this.valueKendoDatepickerFrom);
            var fFullYear = targetDate.getFullYear();
            var fMonth = targetDate.getMonth() + 1;
            var fDate = targetDate.getDate();
            var fromDate = fFullYear.toString() + "/" + fMonth.toString() + "/" + fDate.toString() + " 00:00:00";

            var untilDate = "1980/01/01 00:00:00"

            this.visLogService.GetWebDBLogs(this.visLogService.CurrentDB.webDBId, fromDate, untilDate, logTypeSelected);
            this.LogDataGet;
        }

    }

    public RetrieveLogUntil() {
        if (this.visLogService.CurrentDB != null) {

            var logTypeSelected = "";
            switch (this.LogTypeSelection) {
                case 1: logTypeSelected = "Login"; break;
                case 2: logTypeSelected = "Search"; break;
                case 3: logTypeSelected = "GetFrequency"; break;
                case 4: logTypeSelected = "GetSetFrequency"; break;
                case 5: logTypeSelected = "GetFrequencyNewPage"; break;
                case 6: logTypeSelected = "GetItemList"; break;
                case 7: logTypeSelected = "GetMap"; break;
                case 8: logTypeSelected = "ItemDetailsFromList"; break;
                default: logTypeSelected = "All";
            }

            var targetDate = new Date(this.valueKendoDatepickerFrom);
            var fFullYear = targetDate.getFullYear();
            var fMonth = targetDate.getMonth() + 1;
            var fDate = targetDate.getDate();
            var fromDate = fFullYear.toString() + "/" + fMonth.toString() + "/" + fDate.toString() + " 00:00:00";

            this.valueKendoDatepickerUntil = new Date();
            var targetDate = new Date(this.valueKendoDatepickerUntil);
            var uFullYear = targetDate.getFullYear();
            var uMonth = targetDate.getMonth() + 1;
            var uDate = targetDate.getDate();
            var untilDate = uFullYear.toString() + "/" + uMonth.toString() + "/" + uDate.toString() + " 23:59:59";

            this.visLogService.GetWebDBLogs(this.visLogService.CurrentDB.webDBId, fromDate, untilDate, logTypeSelected);
            this.LogDataGet;
        }

    }


}






