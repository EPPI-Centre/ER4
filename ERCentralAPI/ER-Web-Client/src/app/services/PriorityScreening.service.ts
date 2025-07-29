import { Inject, Injectable, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { lastValueFrom, Subscription } from 'rxjs';
import { HttpClient, } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfo, ReviewInfoService, iReviewInfo } from './ReviewInfo.service';
import { Item, iAdditionalItemDetails } from './ItemList.service';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewSet, SetAttribute } from './ReviewSets.service';
import { EventEmitterService } from './EventEmitter.service';
import { ConfigService } from './config.service';


//see: https://stackoverflow.com/questions/34031448/typescript-typeerror-myclass-myfunction-is-not-a-function
//JSON object is consumed via interfaces, but we immediately digest those and put them into actual classes.
//this is NECESSARY to consume the CSLA (server side) objects as produced by the API in a way that makes 
//then suitable for the angular-tree-component used to show codetrees to users.
@Injectable({
  providedIn: 'root',
})

export class PriorityScreeningService extends BusyAwareService implements OnDestroy {
  constructor(private router: Router, //private _http: Http, 
    private _httpC: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
    private modalService: ModalService,
    private ReviewInfoService: ReviewInfoService,
    private EventEmitterService: EventEmitterService,
    configService: ConfigService
  ) {
    super(configService);
    //console.log("On create PriorityScreeningService");
    this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
    this.clearSub2 = this.EventEmitterService.OpeningNewReview.subscribe(() => { this.Clear(); });
  }
  ngOnDestroy() {
    console.log("Destroy search service");
    if (this.clearSub != null) this.clearSub.unsubscribe();
    if (this.clearSub2 != null) this.clearSub2.unsubscribe();
  }
  private clearSub: Subscription | null = null;
  private clearSub2: Subscription | null = null;

  @Output() gotList = new EventEmitter();
  @Output() gotItem = new EventEmitter();
  public ScreenedItemIds: number[] = [];
  public CurrentItem: Item = new Item();
  public CurrentItemIndex: number = 0;
  public LastItemInTheQueueIsDone: boolean = false;
  public UsingListFromSearch: boolean = false;

  public get NextItemAPIendpoint(): string {
    if (this.UsingListFromSearch) return 'api/PriorirtyScreening/TrainingNextItemFromList';
    else return 'api/PriorirtyScreening/TrainingNextItem';
  }
  public get ScreenedItemAPIendpoint(): string {
    if (this.UsingListFromSearch) return 'api/PriorirtyScreening/TrainingPreviousItemFromList';
    else return 'api/PriorirtyScreening/TrainingPreviousItem';
  }

  private _CurrentItemAdditionalData: iAdditionalItemDetails | null = null;
  public get CurrentItemAdditionalData(): iAdditionalItemDetails | null {
    if (!this.CurrentItem || !this._CurrentItemAdditionalData) return null;
    else if (this.CurrentItem.itemId !== this._CurrentItemAdditionalData.itemID) return null;
    else return this._CurrentItemAdditionalData;
  }
  private _TrainingList: Training[] = [];
  public get TrainingList(): Training[] {
    return this._TrainingList;
  }
  private _TrainingScreeningCriteria: iTrainingScreeningCriteria[] = [];
  public get TrainingScreeningCriteria(): iTrainingScreeningCriteria[] {
    return this._TrainingScreeningCriteria;
  }

  private subtrainingList: Subscription | null = null;
  public Fetch() {
    this._BusyMethods.push("Fetch");
    
    lastValueFrom( this._httpC.get<Training[]>(this._baseUrl + 'api/PriorirtyScreening/TrainingList')).then(tL => {
      this._TrainingList = tL;
      this.RemoveBusy("Fetch");
      //this.Save();
    }, error => {
      this.RemoveBusy("Fetch");
      this.modalService.SendBackHomeWithError(error);
    }
    );
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
    //console.log(this.ScreenedItemIds.indexOf(this.CurrentItem.itemId));
    //console.log(this.ScreenedItemIds.length, this.ScreenedItemIds);

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
    lastValueFrom(this._httpC.post<TrainingNextItem>(this._baseUrl + this.NextItemAPIendpoint,
      body)).then(
        success => {
          this.RemoveBusy("FetchNextItem");
          this.CurrentItem = success.item;
          this.FetchAdditionalItemDetails();
          let currentIndex = this.ScreenedItemIds.indexOf(this.CurrentItem.itemId);
          //we put this item at the end of the queue, ALWAYS.
          //Otherwise "next item" can be an item we screened already, if we receive an item we've seen already (but didn't screen)
          if (currentIndex != -1) {
            this.ScreenedItemIds.splice(currentIndex, 1); //if this item is in the list already: remove it
          }
          //add the item we received at the end of our list
          this.ScreenedItemIds.push(this.CurrentItem.itemId);
          this.CurrentItemIndex = this.ScreenedItemIds.indexOf(this.CurrentItem.itemId);
          
          this.CheckRunTraining(success);
          this.LastItemInTheQueueIsDone = false;
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
    lastValueFrom(this._httpC.post<TrainingPreviousItem>(this._baseUrl + this.ScreenedItemAPIendpoint,
      body)).then(
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


  private CheckRunTraining(screeningItem: TrainingNextItem) {
    let currentCount: number = screeningItem.rank;
    let totalScreened = this._TrainingList[0].totalN;
    for (let training of this._TrainingList) {
      if (training.totalN > totalScreened) totalScreened = training.totalN;
    }
    if (totalScreened <= 1000) {
      if ((currentCount == 25 || currentCount == 50 || currentCount == 75 || currentCount == 100 || currentCount == 150 || currentCount == 300 || currentCount == 500 ||
        currentCount == 750 || currentCount == 1000 || currentCount == 2000 || currentCount == 3000)) {
        this.RunNewTrainingCommand(screeningItem);
      }
    }
    else if (totalScreened > 1000 && totalScreened < 5000) {
      if ((currentCount == 250 || currentCount == 500 || currentCount == 750 || currentCount == 1000 || currentCount == 2000 || currentCount == 3000)) {
        this.RunNewTrainingCommand(screeningItem);
      }
    }
    else if (totalScreened >= 5000) {
      if ((currentCount == 500 || currentCount == 1000 || currentCount == 1500 || currentCount == 2000 || currentCount == 2500 || currentCount == 3000
        || currentCount == 3800 || currentCount == 4800 || currentCount == 5800 || currentCount == 6800 || currentCount == 7800
        || currentCount == 8800 || currentCount == 9800 || currentCount == 18000 || currentCount == 11800)) {
        this.RunNewTrainingCommand(screeningItem);
      }
    }
    //let totalscreened = this._TrainingList
  }
  public RunNewTrainingCommand(screeningItem: TrainingNextItem | null, delayedFetch: boolean = true): Promise<String|boolean> {
    this._BusyMethods.push("RunNewTrainingCommand");
    const body = JSON.stringify(screeningItem == null ? { Value: 0 } : { Value: screeningItem.itemId });
    return lastValueFrom( this._httpC.post<iReviewTrainingRunCommand>(this._baseUrl + 'api/PriorirtyScreening/TrainingRunCommand', body)).then(tL => {
      //this.DelayedFetch(1 * 6);//seconds to wait...
      this.ReviewInfoService.ReviewInfo = new ReviewInfo(tL.revInfo);
      //console.log("Received RevInfo:", tL.revInfo);
      if (delayedFetch) this.DelayedFetch(20 * 60);//seconds to wait... 20m, a decent guess of how long the retraining will take.
      //key is that user will get the next item from the current list (server side) even before receiving the "training" record via this current mechanism.
      this.RemoveBusy("RunNewTrainingCommand");
      return tL.reportBack;
    }, error => {
      this.RemoveBusy("RunNewTrainingCommand");
      this.modalService.GenericError(error);
      return false;
    }
    ).catch(caught => {
      this.RemoveBusy("RunNewTrainingCommand");
      this.modalService.GenericError(caught);
      return false;
    });
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
    this._httpC.post<iTrainingScreeningCriteria[]>(this._baseUrl +
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
  public AddTrainingScreeningCriteria(SetAtt: SetAttribute) {
    this._BusyMethods.push("AddTrainingScreeningCriteria");
    if (SetAtt.attribute_type_id != 10 && SetAtt.attribute_type_id != 11) {
      //not a screening code-type!
      return;
    }
    let body: iUpdatingTrainingScreeningCriteria = {
      trainingScreeningCriteriaId: SetAtt.attribute_id
      , included: (SetAtt.attribute_type_id == 10)
      , deleted: false
    };
    this._httpC.post<iTrainingScreeningCriteria[]>(this._baseUrl +
      'api/PriorirtyScreening/AddTrainingScreeningCriteria', body)
      .subscribe(
        (list: iTrainingScreeningCriteria[]) => {
          this.RemoveBusy("AddTrainingScreeningCriteria");
          this._TrainingScreeningCriteria = list;
        },
        error => {
          this.RemoveBusy("AddTrainingScreeningCriteria");
          this.modalService.GenericError(error);
        });
  }
  public ReplaceTrainingScreeningCriteriaList(set: ReviewSet): Promise<boolean> {
    this._BusyMethods.push("ReplaceTrainingScreeningCriteriaList");
    let Data: iUpdatingTrainingScreeningCriteria[] = [];
    for (let aSet of set.attributes) {
      Data.push({
        trainingScreeningCriteriaId: aSet.attribute_id,
        deleted: false,
        included: aSet.attribute_type_id == 10
      });
    }
    return lastValueFrom(this._httpC.post<iTrainingScreeningCriteria[]>(this._baseUrl +
      'api/PriorirtyScreening/ReplaceTrainingScreeningCriteriaList', Data)
    ).then(
      success => {
        this.RemoveBusy("ReplaceTrainingScreeningCriteriaList");
        this._TrainingScreeningCriteria = success;
        return true;
      },
      error => {
        this.RemoveBusy("ReplaceTrainingScreeningCriteriaList");
        this.modalService.SendBackHomeWithError(error);
        return false;
      }).catch(caught => {
        this.RemoveBusy("ReplaceTrainingScreeningCriteriaList");
        this.modalService.SendBackHomeWithError(caught);
        return false;
      }
      );
  }
  public Clear() {
    this.ScreenedItemIds = [];
    this.CurrentItem = new Item();
    this.CurrentItemIndex = 0;
    this._CurrentItemAdditionalData = null;
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
  revInfo: iReviewInfo;
  reportBack: string;
  parameters: string;
  simulationResults: string;
}
