import { Inject, Injectable, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { last, lastValueFrom, Subscription } from 'rxjs';
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
  private _UsingListFromSearch: boolean = false;
  public get UsingListFromSearch(): boolean {
    return this._UsingListFromSearch;
  }
  public set UsingListFromSearch(val: boolean) {
    if (val != this._UsingListFromSearch) {
      this._UsingListFromSearch = val;
      this.ScreenedItemIds = [];
    }
  }

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

  private _TrainingFromSearchList: ScreeningFromSearchIterationList = new ScreeningFromSearchIterationList([]);
  public get TrainingFromSearchList(): ScreeningFromSearchIterationList {
    return this._TrainingFromSearchList;
  }

  private _TrainingScreeningCriteria: iTrainingScreeningCriteria[] = [];
  public get TrainingScreeningCriteria(): iTrainingScreeningCriteria[] {
    return this._TrainingScreeningCriteria;
  }

  public Fetch(): Promise<boolean> {
    this._BusyMethods.push("Fetch");
    
    return lastValueFrom( this._httpC.get<Training[]>(this._baseUrl + 'api/PriorirtyScreening/TrainingList')).then(tL => {
      this._TrainingList = tL;
      this.RemoveBusy("Fetch");
      return true;
      //this.Save();
    }, error => {
      this.RemoveBusy("Fetch");
      this.modalService.SendBackHomeWithError(error);
      return false;
    }
    );
  }

  public FetchTrainingFromSearchList(): Promise<boolean> {
    this._BusyMethods.push("FetchTrainingFromSearchList");

    return lastValueFrom(this._httpC.get<iScreeningFromSearchIteration[]>(this._baseUrl + 'api/PriorirtyScreening/TrainingFromSearchList')).then(tL => {
      this._TrainingFromSearchList = new ScreeningFromSearchIterationList(tL);
      this.RemoveBusy("FetchTrainingFromSearchList");
      return true;
      //this.Save();
    }, error => {
      this.RemoveBusy("FetchTrainingFromSearchList");
      this.modalService.SendBackHomeWithError(error);
      return false;
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
          
          if (this.UsingListFromSearch == false) this.CheckRunTraining(success);
          else this.CheckUpdateFromSearchNumbers(success);
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

          if (success.item != null) {
            this.CurrentItem = success.item;
            this.FetchAdditionalItemDetails();
            let currentIndex = this.ScreenedItemIds.indexOf(this.CurrentItem.itemId);
            if (currentIndex == -1) this.ScreenedItemIds.push(this.CurrentItem.itemId);//do we really need this?? If it happens, means something is wrong...
            this.CurrentItemIndex = currentIndex;
            //return this.CurrentItem;
            this.gotItem.emit();
          }
          else {
            this.modalService.GenericErrorMessage(
              "Can't access the requested item at this time: \r\n"
              + "This is usually because it's being screened by someone else.\r\n"
              + "Please try again in a short while.\r\n\r\n"
              + "[If you're trying to reach a new item to screen, you can do this immediately by clicking on \"Close/back\" and then on \"Start Screening\".]"
            );
          }
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
      lastValueFrom(this._httpC.post<iAdditionalItemDetails>(this._baseUrl + 'api/ItemList/FetchAdditionalItemData', body))
        .then(
          result => {
            this.RemoveBusy("FetchAdditionalItemDetails");
            this._CurrentItemAdditionalData = result;
          }, error => {
            this.modalService.GenericError(error);
            this.RemoveBusy("FetchAdditionalItemDetails");
          }
        ).catch((caught) => {
          this.RemoveBusy("FetchAdditionalItemDetails");
          this.modalService.GenericError(caught);
        });
    }
  }


  private CheckRunTraining(screeningItem: TrainingNextItem) {
    let currentCount: number = screeningItem.rank;
    let totalScreened = this._TrainingList[0].totalN;
    let NeedsDoing: boolean = false;
    for (let training of this._TrainingList) {
      if (training.totalN > totalScreened) totalScreened = training.totalN;
    }
    if (totalScreened < 250) {//trigger training when we reach the next multiple of 25
      if (currentCount % 25 == 0) {
        console.log("Update training PS records, every 25 items");
        NeedsDoing = true;
      }
    }
    else if (totalScreened < 500) {//trigger training when we reach the next multiple of 50
      if (currentCount % 50 == 0) {
        console.log("Update training PS records, every 50 items");
        NeedsDoing = true;
      }
    }
    else if (totalScreened < 1000) {//trigger training when we reach the next multiple of 100
      if (currentCount % 100 == 0) {
        console.log("Update training PS records, every 100 items");
        NeedsDoing = true;
      }
    }
    else if (totalScreened < 2000) {//trigger training when we reach the next multiple of 200
      if (currentCount % 200 == 0) {
        console.log("Update training PS records, every 200 items");
        NeedsDoing = true;
      }
    }
    else if (totalScreened < 5000) {//trigger training when we reach the next multiple of 500
      if (currentCount % 500 == 0) {
        console.log("Update training PS records, every 500 items");
        NeedsDoing = true;
      }

    }
    else if (totalScreened < 15000) {//trigger training when we reach the next multiple of 1000
      if (currentCount % 1000 == 0) {
        console.log("Update training PS records, every 1000 items");
        NeedsDoing = true;
      }

    }
    else {//trigger training when we reach the next multiple of 2000
      if (currentCount % 2000 == 0) {
        console.log("Update training PS records, every 2000 items");
        NeedsDoing = true;
      }
    }
    if (NeedsDoing) this.RunNewTrainingCommand(screeningItem);

    //let totalscreened = this._TrainingList
  }

  private CheckUpdateFromSearchNumbers(screeningItem: TrainingNextItem) {
    let currentCount: number = screeningItem.rank;
    let totalScreened = this._TrainingFromSearchList.AllITerations[this._TrainingFromSearchList.AllITerations.length - 1].screenedFromList;
    let NeedsDoing: boolean = false;
    if (totalScreened < 500) {
      if (currentCount % 25 == 0) {
        console.log("Update training FS records, every 25 items");
        NeedsDoing = true;
      }
    }
    else if (totalScreened < 2000) {
      if (currentCount % 50 == 0) {
        console.log("Update training FS records, every 50 items");
        NeedsDoing = true;
      }
    }
    else {
      if (currentCount % 100 == 0) {
        console.log("Update training FS records, every 100 items");
        NeedsDoing = true;
      }
    }
    if (NeedsDoing) {
      this.RunNewFromSearchCommand(screeningItem.itemId, this.ReviewInfoService.ReviewInfo.screeningCodeSetId, false, 0);
    }
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
  public RunNewFromSearchCommand(TriggeringItemId: number, CodeSetId: number, IsNew: boolean, SearchId = 0): Promise<boolean | string> {
    const body = {
      searchId: SearchId,
      codeSetId: CodeSetId,
      triggeringItemId: TriggeringItemId,
      createNew: IsNew,
      result: ""
    };
    this._BusyMethods.push("RunNewFromSearchCommand");
    return lastValueFrom(this._httpC.post<iFromSearchCommand>(this._baseUrl + 'api/PriorirtyScreening/RunScreeningFromSearchCommand', body))
      .then(res => {
        this.RemoveBusy("RunNewFromSearchCommand");
        if (res.result == "Done") {
          this.FetchTrainingFromSearchList();
          return true;
        } else { return res.result; }
      }, error => {
        this.RemoveBusy("RunNewFromSearchCommand");
        this.modalService.GenericError(error);
        return false;
        }
    ).catch(
      (caught) => {
        this.RemoveBusy("RunNewFromSearchCommand");
        this.modalService.GenericError(caught);
        return false;
      });
  }
  public async DeleteFromSearchList(): Promise<boolean | string> {
    this._BusyMethods.push("DeleteFromSearchList");

    return lastValueFrom(this._httpC.get<iFromSearchCommand>(this._baseUrl + 'api/PriorirtyScreening/DeleteScreeningFromSearch')
    ).then(async res => {
      this.RemoveBusy("DeleteFromSearchList");
      if (res.result == "Done") {
        await this.FetchTrainingFromSearchList();
        return true;
      } else { return res.result; }
      //this.Save();
    }, error => {
      this.RemoveBusy("DeleteFromSearchList");
      this.modalService.SendBackHomeWithError(error);
      return false;
    }
    ).catch(
      (caught) => {
        this.RemoveBusy("DeleteFromSearchList");
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
    lastValueFrom( this._httpC.get<iTrainingScreeningCriteria[]>(this._baseUrl + 'api/PriorirtyScreening/GetTrainingScreeningCriteriaList')).then(
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
    lastValueFrom(this._httpC.post<iTrainingScreeningCriteria[]>(this._baseUrl +
      'api/PriorirtyScreening/UpdateTrainingScreeningCriteria', crit)
    ).then(
        (list: iTrainingScreeningCriteria[]) => {
          this.RemoveBusy("UpdateTrainingScreeningCriteria");
          this._TrainingScreeningCriteria = list;
        },
        error => {
          this.RemoveBusy("UpdateTrainingScreeningCriteria");
          this.modalService.GenericError(error);
      }).catch (caught => {
        this.RemoveBusy("UpdateTrainingScreeningCriteria");
        this.modalService.GenericError(caught);
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
    lastValueFrom( this._httpC.post<iTrainingScreeningCriteria[]>(this._baseUrl +
      'api/PriorirtyScreening/AddTrainingScreeningCriteria', body))
      .then(
        (list: iTrainingScreeningCriteria[]) => {
          this.RemoveBusy("AddTrainingScreeningCriteria");
          this._TrainingScreeningCriteria = list;
        },
        error => {
          this.RemoveBusy("AddTrainingScreeningCriteria");
          this.modalService.GenericError(error);
        }).catch(caught => {
          this.RemoveBusy("AddTrainingScreeningCriteria");
          this.modalService.GenericError(caught);
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
export interface iScreeningFromSearchIteration {
  trainingFsId: number;
  contactId: number;
  searchId: number;
  date: string;
  iteration: number;
  contactName: string;
  tp: number;
  tn: number;
  totalScreened: number;
  localTN: number;
  localTP: number;
  totItemsInList: number;
  screenedFromList: number;
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

export interface iFromSearchCommand {
  searchId: number;
  codeSetId: number;
  triggeringItemId: number;
  createNew: boolean;
  result: string;
}

//export class ScreeningFromSearchIterationList {
//  constructor(iterations: iScreeningFromSearchIteration[]) {
//    this._Iterations = iterations.filter(f =>  f.searchId != -1 );
//    if (iterations.length > 0 && iterations[iterations.length - 1].searchId == -1) this._ListIsStale = true;
//    if (this._Iterations.length > 0) {
//      this.SetSearchIdFilter(this._Iterations[this._Iterations.length - 1].searchId);
//    } else {
//      this.SetSearchIdFilter(0);
//    }
//    this._AllSearchIds = [];
//    for (const s of this._Iterations) {
//      if (this._AllSearchIds.findIndex(f => f == s.searchId) == -1) this._AllSearchIds.push(s.searchId);
//    }
//  }
//  private _FilterBySearchId: number = 0;
//  private _AllSearchIds: number[] = [];
//  private _Iterations: iScreeningFromSearchIteration[] = [];
//  private _FilteredIterations: iScreeningFromSearchIteration[] = [];
//  private _ListIsStale: boolean = false;
//  public get FilterBySearchId(): number {
//    return this._FilterBySearchId;
//  }
//  public get AllSearchIds(): number[] {
//    return this._AllSearchIds;
//  }
//  public get Iterations(): iScreeningFromSearchIteration[] {
//    return this._Iterations;
//  }
//  public get FilteredIterations(): iScreeningFromSearchIteration[] {
//    return this._FilteredIterations;
//  }
//  public get ListIsStale(): boolean {
//    return this._ListIsStale;
//  }
//  public SetSearchIdFilter(SearchId: number) {
//    this._FilterBySearchId = SearchId;
//    const res = this._Iterations.filter(f => f.searchId == SearchId);
//    this._FilteredIterations = res;
//  }
//}

export class ScreeningFromSearchIterationList {
  constructor(allruns: iScreeningFromSearchIteration[]) {
    this._AllRuns = [];
    this._AllITerations = allruns.filter(f => f.searchId != -1);
    this._CurrentRunVirtualId = 0;
    this._CurrentRun = new ScreeningFromSearchIterationRun(-1, []);
    let cVid: number = 0;
    let cSid: number = -1;
    let lastIndex: number = allruns.length -1;
    let runsBunch: iScreeningFromSearchIteration[] = [];
    for (let i = 0; i <= lastIndex; i++) {
      const run = allruns[i];
      if (cSid != run.searchId || i == lastIndex) {//new run found, or we reached the last
        if (i == lastIndex) {
          if (cSid != run.searchId && runsBunch.length > 0) {
            //the last element and current element needs to go in the last run and will be the only iteration there
            //so we "finish" the current run, before doing the next step
            let toAdd = new ScreeningFromSearchIterationRun(cVid, runsBunch);
            this._AllRuns.push(toAdd);
            runsBunch = [];
            cVid++;
          }
          runsBunch.push(run);
        }
        if (runsBunch.length > 0 && runsBunch[0].searchId != -1) {
          let toAdd = new ScreeningFromSearchIterationRun(cVid, runsBunch);
          this._AllRuns.push(toAdd);
          runsBunch = [];
          cSid = run.searchId;
          cVid++;
        }
        if (run.searchId == -1) {//separator/dummy run, we skip it
          if (i == lastIndex) this._ListIsStale = true;//signals that it doesn't make sense to update progress
          continue;
        }

      }
      cSid = run.searchId;
      runsBunch.push(run);
    }
    if (this._AllRuns.length > 0) {
      this._CurrentRun = this._AllRuns[this._AllRuns.length - 1];
      this._CurrentRunVirtualId = this._CurrentRun.VirtualId;
    }
  }
  private _AllRuns: ScreeningFromSearchIterationRun[];
  public get AllRuns(): ScreeningFromSearchIterationRun[] {
    return this._AllRuns;
  }
  private _AllITerations: iScreeningFromSearchIteration[];
  public get AllITerations(): iScreeningFromSearchIteration[] {
    return this._AllITerations;
  }
  private _CurrentRunVirtualId: number;
  public get CurrentRunVirtualId(): number {
    return this._CurrentRunVirtualId;
  }
  private _CurrentRun: ScreeningFromSearchIterationRun ;
  public get CurrentRun(): ScreeningFromSearchIterationRun {
    return this._CurrentRun;
  }
  private _ListIsStale: boolean = false;
  public get ListIsStale(): boolean {
    return this._ListIsStale;
  }

  public SetCurrentRun(runVirtualId: number) {
    for (const run of this._AllRuns) {
      if (run.VirtualId == runVirtualId) {
        this._CurrentRun = run;
        this._CurrentRunVirtualId = runVirtualId;
        return;
      }
    }
    this._CurrentRunVirtualId = 0;
    this._CurrentRun = new ScreeningFromSearchIterationRun(-1, []);
  }
}
export class ScreeningFromSearchIterationRun {
  constructor(virtualId: number, iterations: iScreeningFromSearchIteration[]) {
    this._VirtualId = virtualId;
    this._Iterations = iterations;
  }
  private _VirtualId: number;
  public get VirtualId(): number {
    return this._VirtualId;
  }
  private _Iterations: iScreeningFromSearchIteration[];
  public get Iterations(): iScreeningFromSearchIteration[] {
    return this._Iterations;
  }
  public get SearchId(): number {
    if (this._Iterations.length > 0) return this._Iterations[0].searchId;
    else return -1;
  }
}
