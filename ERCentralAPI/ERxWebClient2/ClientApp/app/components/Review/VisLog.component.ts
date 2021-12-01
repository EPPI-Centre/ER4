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
            this.LogDataGet;
        }

    }

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
                const dateUntil = new Date(this.valueKendoDatepickerUntil.toString());
                untilDate = dateUntil.toISOString();
            }

            const dateFrom = new Date(this.valueKendoDatepickerFrom.toString());
            var fromDate = dateFrom.toISOString();

            this.visLogService.GetWebDBLogs(this.visLogService.CurrentDB.webDBId, fromDate, untilDate, logTypeSelected);
            this.LogDataGet;
        }

        /*
        if (this.DateLimitSelection == 4) {

            this.magSearchDate1 = this.valueKendoDatepicker1;
            this.magSearchDate2 = this.valueKendoDatepicker2;

        } else if (this.DateLimitSelection == 5 || this.DateLimitSelection == 6 ||
            this.DateLimitSelection == 7) {

            this.magSearchDate1 = new Date(this.valueYearPicker3 + 1, 0, 0, 0, 0, 0, 0);

        } else if (this.DateLimitSelection == 8) {

            this.magSearchDate1 = new Date(this.valueYearPicker3 + 1, 0, 0, 0, 0, 0, 0);
            this.magSearchDate2 = new Date(this.valueYearPicker4 + 1, 0, 0, 0, 0, 0, 0);

        } else {

            this.magSearchDate1 = this.valueKendoDatepicker3;
        }

        let title: string = "";
        if (this.WordsInSelection != 3) title = this.magSearchInput;
        else title = this.SearchTextTopicDisplayName;
        this._magSearchService.CreateMagSearch(this.WordsInSelection, this.DateLimitSelection, this.PublicationTypeSelection,
            title, this.magSearchDate1, this.magSearchDate2, this.SearchTextTopic).then(

                () => {
                    this.FetchMagSearches();
                    this.DateLimitSelection = 0;

                    if (this.WordsInSelection == 3) {
                        //cleanup the topics...
                        this.SearchTextTopicsResults = [];
                        this.SearchTextTopic = "";
                        this.SearchedTopic = "";
                        this.SearchTextTopicDisplayName = "";
                    }
                    //let msg: string = 'You have created a new search';
                    //this._confirmationDialogService.showMAGRunMessage(msg);
                }
            );
        */
    }

}






