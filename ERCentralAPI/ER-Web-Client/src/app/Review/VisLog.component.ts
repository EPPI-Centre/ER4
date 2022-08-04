import { Component, Inject, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { WebDBService, iWebDB, iWebDBLog } from '../services/WebDB.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { RowClassArgs, GridDataResult, SelectableSettings, SelectableMode, PageChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { Helpers } from '../helpers/HelperMethods';
import { ExcelService } from '../services/excel.service';



@Component({
    selector: 'VisLogComp',
    templateUrl: './VisLog.component.html', 
    providers: []
})




export class VisLogComp implements OnInit{
    constructor(
        private visLogService: WebDBService,
        private ExcelService: ExcelService
    ) {
    }

    ngOnInit() {
        // IF the current logs are empty and CurrentDB is not null, 
        // CALL GetWebDBLogs(...) with the correct WebDBid and 
        // some default values for "from, until, logType".

        //if ((this.visLogService.CurrentLogs.length == 0) && (this.visLogService.CurrentDB != null)) { // (I removed the '== 0' check to have a proper 'close')
        if (this.visLogService.CurrentDB != null) {
            const currDB = this.visLogService.CurrentDB;
            setTimeout(() => { this.visLogService.GetWebDBLogs(currDB.webDBId, "2021/09/27 00:00:00", "1980/01/01 00:00:00", "All") }, 20);
        }

    }

    public valueKendoDatepickerFrom: Date = new Date(2021, 9, 27);
    public valueKendoDatepickerUntil: Date = new Date();

    public fromMin: Date = new Date(2021, 9, 27);
    public fromMax: Date = this.valueKendoDatepickerUntil;
    public untilMin: Date = this.valueKendoDatepickerFrom;
    public untilMax: Date = new Date();


    public LogTypeSelection: number = 0;

    public get LogDataGet(): GridDataResult {
        if (this.skip > this.visLogService.CurrentLogs.length) this.skip = 0;
        return {
            data: orderBy(this.visLogService.CurrentLogs, this.sortLogs).slice(this.skip, this.skip + this.pageSize),
            total: this.visLogService.CurrentLogs.length,
        };
    }
    public sortLogs: SortDescriptor[] = [{
        field: 'dateTimeCreated',
        dir: 'desc'
    }];
    public pageSize = 100;
    public skip = 0;
    pageChange({ skip, take }: PageChangeEvent): void {
        this.skip = skip;
        this.pageSize = take;
    }
    public sortChangeLogs(sort: SortDescriptor[]): void {
        this.sortLogs = sort;
    }


    public visLogDate: string = 'dateFrom';

    // needs to  be 'public' for compiling in production mode
    public get CurrentLogs(): iWebDBLog[] {
        return this.visLogService.CurrentLogs;
    }

    GetLogs(CurrentDB: iWebDB) {
        this.visLogService.GetWebDBLogs(CurrentDB.webDBId, "2021/09/27 00:00:00", "1980/01/01 00:00:00", "All");
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

            this.fromMax = this.valueKendoDatepickerUntil;
            this.untilMin = this.valueKendoDatepickerFrom;


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

            this.fromMax = this.valueKendoDatepickerUntil;

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

            this.fromMax = this.valueKendoDatepickerUntil;
            this.untilMin = this.valueKendoDatepickerFrom;

            this.visLogService.GetWebDBLogs(this.visLogService.CurrentDB.webDBId, fromDate, untilDate, logTypeSelected);
            this.LogDataGet;
        }

    }

    public GridToExcel() {
        let res: any[] = [];
        for (let row of this.visLogService.CurrentLogs) {
            //let rrow = { ID: row.webDBLogIdentity, Date: row.dateTimeCreated, LogType: row.logType, Details: row.logDetails };
            let rrow = { Date: row.dateTimeCreated, LogType: row.logType, Details: row.logDetails };
            res.push(rrow);
        }
        this.ExcelService.exportAsExcelFile(res, 'EPPIVis_logging_data');
    }


}






