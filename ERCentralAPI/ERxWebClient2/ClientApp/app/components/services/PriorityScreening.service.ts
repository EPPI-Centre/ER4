import { Component, Inject, Injectable, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of, Subscription } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemSet } from './ItemCoding.service';
import { ReviewInfo, ReviewInfoService, iReviewInfo } from './ReviewInfo.service';
import { Item, iAdditionalItemDetails } from './ItemList.service';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';


//see: https://stackoverflow.com/questions/34031448/typescript-typeerror-myclass-myfunction-is-not-a-function
//JSON object is consumed via interfaces, but we immediately digest those and put them into actual classes.
//this is NECESSARY to consume the CSLA (server side) objects as produced by the API in a way that makes 
//then suitable for the angular-tree-component used to show codetrees to users.
@Injectable({
    providedIn: 'root',
})

export class PriorityScreeningService extends BusyAwareService {
    constructor(private router: Router, //private _http: Http, 
        private _httpC: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        private ReviewInfoService: ReviewInfoService,
        @Inject('BASE_URL') private _baseUrl: string) { super(); }
    @Output() gotList = new EventEmitter();
    @Output() gotItem = new EventEmitter();
    public ScreenedItemIds: number[] = [];
    public CurrentItem: Item = new Item();
    public CurrentItemIndex: number = 0;
    private _CurrentItemAdditionalData: iAdditionalItemDetails | null = null;
    public get CurrentItemAdditionalData(): iAdditionalItemDetails | null {
        if (!this.CurrentItem || !this._CurrentItemAdditionalData) return null;
        else if (this.CurrentItem.itemId !== this._CurrentItemAdditionalData.itemID) return null;
        else return this._CurrentItemAdditionalData;
    }
    private _TrainingList: Training[] = [];
    public get TrainingList(): Training[]{
        return this._TrainingList;
    }
    private _TrainingScreeningCriteria: iTrainingScreeningCriteria[] = [];
    public get TrainingScreeningCriteria(): iTrainingScreeningCriteria[] {
        return this._TrainingScreeningCriteria;
    }

    private subtrainingList: Subscription | null = null;
	public Fetch() {
		this._BusyMethods.push("Fetch");
        if (this.subtrainingList) {
            this.subtrainingList.unsubscribe();
            this.subtrainingList = null;
        }
        this.subtrainingList = this._httpC.get<Training[]>(this._baseUrl + 'api/PriorirtyScreening/TrainingList').subscribe(tL => {
			this._TrainingList = tL;
			this.RemoveBusy("Fetch");
            //this.Save();
		}, error => {
			this.RemoveBusy("Fetch");
			this.modalService.SendBackHomeWithError(error);
		}
        );
        return this.subtrainingList;
    }
	private DelayedFetch(waitSeconds: number) {

        setTimeout(() => {
            //console.log("I'm done waiting");
            this.Fetch();
            this.ReviewInfoService.Fetch();
        }, waitSeconds * 1000);
        
    }
    //public Save() {
    //    if (this._TrainingList && this._TrainingList.length > 0)
    //        localStorage.setItem('TrainingList', JSON.stringify(this._TrainingList));
    //    else if (localStorage.getItem('TrainingList'))//to be confirmed!! 
    //        localStorage.removeItem('TrainingList');
    //}
    //private _NextItem: Item = new Item();
    public NextItem() {
        //Logic: if we are within the list of already seen items, find who's next and fetch it,
        //otherwise ask for "next in list", which will reserve the item (TrainingNextItem)
        //console.log(this.CurrentItem == null);
        //console.log(this.CurrentItem.itemId == 0);
        console.log(this.ScreenedItemIds.indexOf(this.CurrentItem.itemId));
        console.log(this.ScreenedItemIds.length, this.ScreenedItemIds);

        if ((this.CurrentItem == null || this.CurrentItem.itemId == 0) || (this.ScreenedItemIds.indexOf(this.CurrentItem.itemId) == this.ScreenedItemIds.length - 1)) {
            this.FetchNextItem();
        }
        else {
            this.FetchScreenedItem(this.CurrentItemIndex + 1);
        }
        //return new Item();
    }
    public PreviousItem() {
        if (this.CurrentItemIndex > 0) this.FetchScreenedItem(this.CurrentItemIndex - 1);
    }
    private FetchNextItem() {
        this._BusyMethods.push("FetchNextItem");
        let body = JSON.stringify({ Value: this.ReviewInfoService.ReviewInfo.screeningCodeSetId });
        this._httpC.post<TrainingNextItem>(this._baseUrl + 'api/PriorirtyScreening/TrainingNextItem',
            body).toPromise().then(
            success => {
                this.RemoveBusy("FetchNextItem");
                this.CurrentItem = success.item;
                this.FetchAdditionalItemDetails();
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
                this.RemoveBusy("FetchNextItem");
                this.modalService.SendBackHomeWithError(error); 
                });
    }

    private FetchScreenedItem(index: number) {
        this._BusyMethods.push("FetchScreenedItem");
        let body = JSON.stringify({ Value: this.ScreenedItemIds[index] });
        //console.log("FetchScreenedItem", this.ScreenedItemIds.indexOf(this.CurrentItem.itemId));
        //console.log(this.ScreenedItemIds.length, this.ScreenedItemIds);
        this._httpC.post<TrainingPreviousItem>(this._baseUrl + 'api/PriorirtyScreening/TrainingPreviousItem',
            body).toPromise().then(
            success => {
                this.RemoveBusy("FetchScreenedItem");
                    this.CurrentItem = success.item;
                    this.FetchAdditionalItemDetails();
                    let currentIndex = this.ScreenedItemIds.indexOf(this.CurrentItem.itemId);
                    if (currentIndex == -1) this.ScreenedItemIds.push(this.CurrentItem.itemId);//do we really need this?? If it happens, means something is wrong...
                    this.CurrentItemIndex = currentIndex;
                    //return this.CurrentItem;
                    this.gotItem.emit();
                },
                error => {
                    this.RemoveBusy("FetchScreenedItem");
                    this.modalService.SendBackHomeWithError(error);
                });
    }

    public FetchAdditionalItemDetails() {
        if (this.CurrentItem.itemId !== 0) {
            this._BusyMethods.push("FetchAdditionalItemDetails");
            let body = JSON.stringify({ Value: this.CurrentItem.itemId });
            this._httpC.post<iAdditionalItemDetails>(this._baseUrl + 'api/ItemList/FetchAdditionalItemData', body)
                .subscribe(
                    result => {
                        this._CurrentItemAdditionalData = result;
                    }, error => {
                        this.modalService.GenericError(error);
                        this.RemoveBusy("FetchAdditionalItemDetails");
                    }
                    , () => { this.RemoveBusy("FetchAdditionalItemDetails"); }
                );
        }
    }


    private CheckRunTraining(currentCount: number) {
        let totalScreened = this._TrainingList[0].totalN;
        for (let training of this._TrainingList) {
            if (training.totalN > totalScreened) totalScreened = training.totalN;
        }
        if (totalScreened <= 1000) {
            if ((currentCount == 25 || currentCount == 50 || currentCount == 75 || currentCount == 100 || currentCount == 150 || currentCount == 500 ||
                currentCount == 750 )) {
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
	public RunNewTrainingCommand(delayedFetch:boolean = true) {
		this._BusyMethods.push("RunNewTrainingCommand");
        return this._httpC.get<iReviewTrainingRunCommand>(this._baseUrl + 'api/PriorirtyScreening/TrainingRunCommand').subscribe(tL => {
            //this.DelayedFetch(1 * 6);//seconds to wait...
            this.ReviewInfoService.ReviewInfo = new ReviewInfo(tL.RevInfo);
            console.log("Received RevInfo:", tL.RevInfo);
            if (delayedFetch) this.DelayedFetch(30 * 60);//seconds to wait... 30m, a decent guess of how long the retraining will take.
            //key is that user will get the next item from the current list (server side) even before receiving the "training" record via this current mechanism.
			this.RemoveBusy("RunNewTrainingCommand");
		}, error => {
			this.RemoveBusy("RunNewTrainingCommand");
			this.modalService.SendBackHomeWithError(error);
		}
        );
    }

    public HasPreviousItem(): boolean {
        if (this.CurrentItemIndex > 0 && this.ScreenedItemIds.length > 0) return true;
        else return false;
    }
    public GetTrainingScreeningCriteriaList() {
        this._BusyMethods.push("GetTrainingScreeningCriteriaList");
        this._httpC.get<iTrainingScreeningCriteria[]>(this._baseUrl + 'api/PriorirtyScreening/GetTrainingScreeningCriteriaList').subscribe(
            list => {
                this._TrainingScreeningCriteria = list;
                this.RemoveBusy("GetTrainingScreeningCriteriaList");
                //this.Save();
            }, error => {
                this.RemoveBusy("GetTrainingScreeningCriteriaList");
                this.modalService.GenericError(error);
            }
        );
    }
    public FlipTrainingScreeningCriteria(crit: iTrainingScreeningCriteria) {
        let body: iUpdatingTrainingScreeningCriteria = {
            trainingScreeningCriteriaId: crit.trainingScreeningCriteriaId
            , included: !crit.included
            , deleted: false
        };
        this.internalUpdateTrainingScreeningCriteria(body);
    }
    public DeleteTrainingScreeningCriteria(crit: iTrainingScreeningCriteria) {
        let body: iUpdatingTrainingScreeningCriteria = {
            trainingScreeningCriteriaId: crit.trainingScreeningCriteriaId
            , included: crit.included
            , deleted: true
        };
        this.internalUpdateTrainingScreeningCriteria(body);
    }
    private internalUpdateTrainingScreeningCriteria(crit: iUpdatingTrainingScreeningCriteria) {
        this._BusyMethods.push("UpdateTrainingScreeningCriteria");
        return this._httpC.post<iTrainingScreeningCriteria[]>(this._baseUrl +
            'api/PriorirtyScreening/UpdateTrainingScreeningCriteria', crit)
            .subscribe(
            (list: iTrainingScreeningCriteria[]) => {
                this.RemoveBusy("UpdateTrainingScreeningCriteria");
                this._TrainingScreeningCriteria = list;
                },
                error => {
                    this.RemoveBusy("UpdateTrainingScreeningCriteria");
                    this.modalService.GenericError(error);
                });
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
    tp: number;
    tn: number;
    fp: number;
    fn: number;
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
export interface iTrainingScreeningCriteria {
    trainingScreeningCriteriaId: number;
    attributeId: number;
    attributeName: string;
    included: boolean;
}
export interface iUpdatingTrainingScreeningCriteria {
    trainingScreeningCriteriaId: number;
    deleted: boolean;
    included: boolean;
}

export interface iReviewTrainingRunCommand {
    RevInfo: iReviewInfo;
    ReportBack: string;
    Parameters: string;
    SimulationResults: string;
}
