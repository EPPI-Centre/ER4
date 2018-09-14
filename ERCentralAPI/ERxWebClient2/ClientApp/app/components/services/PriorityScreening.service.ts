import { Component, Inject, Injectable, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of, Subscription } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemSet } from './ItemCoding.service';
import { ReviewInfo, ReviewInfoService } from './ReviewInfo.service';
import { CheckBoxClickedEventData } from '../reviewsets/reviewsets.component';
import { Item } from './ItemList.service';


//see: https://stackoverflow.com/questions/34031448/typescript-typeerror-myclass-myfunction-is-not-a-function
//JSON object is consumed via interfaces, but we immediately digest those and put them into actual classes.
//this is NECESSARY to consume the CSLA (server side) objects as produced by the API in a way that makes 
//then suitable for the angular-tree-component used to show codetrees to users.
@Injectable({
    providedIn: 'root',
})

export class PriorityScreeningService {
    constructor(private router: Router, //private _http: Http, 
        private _httpC: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private ReviewInfoService: ReviewInfoService,
        @Inject('BASE_URL') private _baseUrl: string) { }
    @Output() gotList = new EventEmitter();
    @Output() gotItem = new EventEmitter();
    public ScreenedItemIds: number[] = [];
    public CurrentItem: Item = new Item();
    public CurrentItemIndex: number = 0;
    private _TrainingList: Training[] = [];
    public get TrainingList(): Training[]{
        if (this._TrainingList && this.TrainingList.length > 0) {
            return this._TrainingList;
        }
        else {
            const TrainingListJson = localStorage.getItem('TrainingList');
            let tTrainingList: Training[] = TrainingListJson !== null ? JSON.parse(TrainingListJson) : [];
            
            if (tTrainingList == undefined || tTrainingList == null || tTrainingList.length == 0) {
                return this._TrainingList;
            }
            else {
                //console.log("Got User from LS");
                this._TrainingList = tTrainingList;
            }
        }
        return this._TrainingList;
    }

    private subtrainingList: Subscription | null = null;
    public Fetch() {
        if (this.subtrainingList) {
            this.subtrainingList.unsubscribe();
            this.subtrainingList = null;
        }
        this.subtrainingList = this._httpC.get<Training[]>(this._baseUrl + 'api/PriorirtyScreening/TrainingList').subscribe(tL => {
            this._TrainingList = tL;
            this.Save();
            //console.log('This is the review name: ' + rI.reviewId + ' ' + this.ReviewInfo.reviewName);
        });
        return this.subtrainingList;
    }
    private DelayedFetch(waitSeconds: number) {
        //console.log('In DelayedFetch waiting ' + waitSeconds + 's');
        setTimeout(() => {
            //console.log("I'm done waiting");
            this.Fetch();
        }, waitSeconds * 1000);
        
    }
    public Save() {
        if (this._TrainingList && this._TrainingList.length > 0)
            localStorage.setItem('TrainingList', JSON.stringify(this._TrainingList));
        else if (localStorage.getItem('TrainingList'))//to be confirmed!! 
            localStorage.removeItem('TrainingList');
    }
    //private _NextItem: Item = new Item();
    public NextItem() {
        //Logic: if we are within the list of already seen items, find who's next and fetch it,
        //otherwise ask for "next in list", which will reserve the item (TrainingNextItem)
        //console.log(this.CurrentItem == null);
        //console.log(this.CurrentItem.itemId == 0);
        //console.log(this.ScreenedItemIds.indexOf(this.CurrentItem.itemId));
        //console.log(this.ScreenedItemIds.length);

        if ((this.CurrentItem == null || this.CurrentItem.itemId == 0) || (this.ScreenedItemIds.indexOf(this.CurrentItem.itemId) == this.ScreenedItemIds.length - 1)) {
            this.FetchNextItem();
        }
        else {
            this.FetchScreenedItem(this.CurrentItemIndex + 1);
        }
        //return new Item();
    }
    public PreviousItem() {
        //console.log('api/.../TrainingNextItem');
        if (this.CurrentItemIndex > 0) this.FetchScreenedItem(this.CurrentItemIndex - 1);
    }
    private FetchNextItem() {
        //console.log('api/PriorirtyScreening/TrainingNextItem');
        let body = JSON.stringify({ Value: this.ReviewInfoService.ReviewInfo.screeningCodeSetId });
        this._httpC.post<TrainingNextItem>(this._baseUrl + 'api/PriorirtyScreening/TrainingNextItem',
            body).toPromise().then(
                success => {
                    this.CurrentItem = success.item;
                    //console.log(this.CurrentItem.itemId);
                    let currentIndex = this.ScreenedItemIds.indexOf(this.CurrentItem.itemId);
                    if (currentIndex == -1) {
                        this.ScreenedItemIds.push(this.CurrentItem.itemId);
                        this.CurrentItemIndex = this.ScreenedItemIds.indexOf(this.CurrentItem.itemId);
                    }
                    else this.CurrentItemIndex = currentIndex;
                    this.CheckRunTraining(success.rank);
                    this.gotItem.emit();
                },
                error => {
                    //return new Item();
                    this.gotItem.emit();
                });
    }

    private FetchScreenedItem(index: number) {
        //console.log('api/PriorirtyScreening/TrainingPreviousItem index: ' + index);
        let body = JSON.stringify({ Value: this.ScreenedItemIds[index] });
        this._httpC.post<TrainingPreviousItem>(this._baseUrl + 'api/PriorirtyScreening/TrainingPreviousItem',
            body).toPromise().then(
            success => {
                //console.log(success.item.title);
                    this.CurrentItem = success.item;
                    let currentIndex = this.ScreenedItemIds.indexOf(this.CurrentItem.itemId);
                    if (currentIndex != -1) this.ScreenedItemIds.push(this.CurrentItem.itemId);
                    this.CurrentItemIndex = currentIndex;
                    //return this.CurrentItem;
                    this.gotItem.emit();
                },
                error => {
                    //return new Item();
                    this.gotItem.emit();
                });
    }

    private CheckRunTraining(currentCount: number) {
        let totalScreened = this._TrainingList[0].totalN;
        for (let training of this._TrainingList) {
            if (training.totalN > totalScreened) totalScreened = training.totalN;
        }
        if (totalScreened <= 1000) {
            if ((currentCount == 25 || currentCount == 50 || currentCount == 75 || currentCount == 100 || currentCount == 150 || currentCount == 500 ||
                currentCount == 750 )) {
                //console.log('RunningTraining!!!!');
                this.RunNewTrainingCommand();
            }
        }
        else if (totalScreened > 1000 && totalScreened < 5000) {
            if ((currentCount == 500 || currentCount == 750 || currentCount == 1000 || currentCount == 2000 || currentCount == 3000)) {
                this.RunNewTrainingCommand();
            }
        }
        else if (totalScreened >= 5000) {
            if ((currentCount == 1000 || currentCount == 1500 || currentCount == 2000 || currentCount == 2500 || currentCount == 3000
                || currentCount == 3800 || currentCount == 4800 || currentCount == 5800 || currentCount == 6800 || currentCount == 7800
                || currentCount == 8800 || currentCount == 9800 || currentCount == 18000 || currentCount == 11800))
            {
                this.RunNewTrainingCommand();
            }
        }
        //let totalscreened = this._TrainingList
    }
    private RunNewTrainingCommand() {
        return this._httpC.get<any>(this._baseUrl + 'api/PriorirtyScreening/TrainingRunCommand').subscribe(tL => {
            //console.log(tL);
            this.DelayedFetch(30 * 60);//seconds to wait...
            //console.log('This is the review name: ' + rI.reviewId + ' ' + this.ReviewInfo.reviewName);
        });
    }
    //private _PreviousItem: Item = new Item();
    //public get PreviousItem(): Item {
    //    return this._PreviousItem;
    //}
    //public HasNextItem(): boolean {
    //    if (this._NextItem.itemId == 0) return false;
    //    else return true;
    //}
    public HasPreviousItem(): boolean {
        if (this.CurrentItemIndex > 0 && this.ScreenedItemIds.length > 0) return true;
        else return false;
    }
}
export interface Training {
    trainingId: number;
    contactId: number;
    startTime: string;
    endTime: string;
    iteration: number;
     nTrainingItemsInc: number;
    nTrainingItemsExc: number;
    contactName: string;
    c: number;
    tP: number;
    tN: number;
    fP: number;
    fN: number;
    totalN: number;
    totalIncludes: number;
    totalExcludes: number;
}
export interface TrainingNextItem {
    itemId: number;
    item: Item;
    trainingItemId: number;
    trainingId: number;
    rank: number;
}
export interface TrainingPreviousItem {
    itemId: number;
    item: Item;
}
