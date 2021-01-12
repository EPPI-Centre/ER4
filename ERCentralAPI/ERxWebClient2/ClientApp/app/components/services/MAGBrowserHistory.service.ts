import { Injectable } from "@angular/core";
import { Subscription } from "rxjs";
import { BusyAwareService } from "../helpers/BusyAwareService";
import { MagBrowseHistoryItem } from "./MAGClasses.service";

@Injectable({

    providedIn: 'root',

})

export class MAGBrowserHistoryService extends BusyAwareService  {

    public MAGSubscription: Subscription = new Subscription();
    public _MAGBrowserHistoryList: MagBrowseHistoryItem[] = [];
    constructor(
    ) {
        super();
    }
    public currentBrowsePosition: number = 0;
    
    public UnsubscribeMAGHistory() {

        this.MAGSubscription.unsubscribe();
    }
    public IncrementHistoryCount() {
        
        if (this._MAGBrowserHistoryList != null) {
            
                this.currentBrowsePosition = this._MAGBrowserHistoryList.length + 1; // otherwise we leave it where it is (i.e. user has navigated 'back')
        }
    }
    public AddHistory(magBrowserHistoryItem: MagBrowseHistoryItem) {

        let item: MagBrowseHistoryItem = magBrowserHistoryItem;
        this.IncrementHistoryCount();
        this.AddToBrowseHistory(item);
    }

    public AddToBrowseHistory(item: MagBrowseHistoryItem ) {

        this._MAGBrowserHistoryList.push(item);
    }

    public FetchMAGBrowserHistory() {

        return this._MAGBrowserHistoryList;
    }

}