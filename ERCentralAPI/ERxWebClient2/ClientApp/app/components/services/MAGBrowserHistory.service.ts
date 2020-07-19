import { Injectable, Inject } from "@angular/core";
import { Router,  NavigationEnd } from '@angular/router';
import { filter } from "rxjs/operators";
import { Subscription } from "rxjs";
import { MAGBrowserService } from "./MAGBrowser.service";
import { HttpClient } from "@angular/common/http";
import { ModalService } from "./modal.service";
import { BusyAwareService } from "../helpers/BusyAwareService";
import { MagBrowseHistoryItem } from "./MAGClasses.service";

@Injectable({

    providedIn: 'root',

})

export class MAGBrowserHistoryService extends BusyAwareService  {
    private history: NavigationEnd[] = [];
    public MAGSubscription: Subscription = new Subscription();
    public _MAGBrowserHistoryList: MagBrowseHistoryItem[] = [];
    constructor(
        private _httpC: HttpClient,
        private _magBrowserService: MAGBrowserService,
        private modalService: ModalService,
        private router: Router,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
    public currentBrowsePosition: number = 0;
    public loadRouting(): void {

        this.MAGSubscription = this.router.events
            .pipe(filter(event => event instanceof NavigationEnd))

            .subscribe(
                    ( url: any) =>
                    {
                        var navigationURL = url as NavigationEnd;
                        this.history = [...this.history, navigationURL];
                    });
    }
    public getHistory(): NavigationEnd[] {
        return this.history;

    }
    public ClearHistory() {

        this.history = [];
    }
    public getPreviousUrl(): NavigationEnd {

        return this.history[this.history.length - 2] || '/index';
    }
    public UnsubscribeMAGHistory() {

        this.MAGSubscription.unsubscribe();
    }
    public IncrementHistoryCount() {
        
        if (this._MAGBrowserHistoryList != null) {
            
                this.currentBrowsePosition = this._MAGBrowserHistoryList.length + 1; // otherwise we leave it where it is (i.e. user has navigated 'back')
        }
    }
    public AddToBrowseHistory(item: MagBrowseHistoryItem ) {

        console.log('1', item);
        this._MAGBrowserHistoryList.push(item);
        console.log('2', this._MAGBrowserHistoryList);

        //TODO in the future when it is going to the DB
        //this._BusyMethods.push("AddToBrowseHistory");
        //return this._httpC.post<MagBrowseHistoryItem>(this._baseUrl + 'api/MagBrowseHistoryList/AddToBrowseHistory', item)
        //    .toPromise().then(() => {
        //        this.RemoveBusy("AddToBrowseHistory");
        //        return;
        //    },
        //        (error: any) => {
        //            this.RemoveBusy("AddToBrowseHistory");
        //            //this.modalService.GenericError(error);
        //            return error;
        //        });
    }

    public FetchMAGBrowserHistory() {

        console.log('3', this._MAGBrowserHistoryList);
        return this._MAGBrowserHistoryList;
         //TODO in the future when it is going to the DB
        //this._BusyMethods.push("FetchMAGBrowserHistory");
        //return this._httpC.get<MagBrowseHistoryItem[]>(this._baseUrl + 'api/MagBrowseHistoryList/GetMagBrowseHistoryList')
        //    .toPromise().then((result) => {
        //        this.RemoveBusy("FetchMAGBrowserHistory");
        //        this._MAGBrowserHistoryList = result;
        //        console.log(result);
        //        return;
        //    },
        //    (error: any) => {
        //        this.RemoveBusy("FetchMAGBrowserHistory");
        //        //this.modalService.GenericError(error);
        //        return error;
        //    });
    }

}