import { Inject, Injectable, EventEmitter, Output, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { ReviewSetsService, ReviewSet } from './ReviewSets.service';
import { lastValueFrom, Subject, Subscription } from 'rxjs';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { EventEmitterService } from './EventEmitter.service';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root',
})

export class CodesetStatisticsService extends BusyAwareService implements OnDestroy {

  constructor(
    private _http: HttpClient,
    private modalService: ModalService,
    private reviewSetsService: ReviewSetsService,
    private EventEmitterService: EventEmitterService,
    configService: ConfigService
  ) {
    super(configService);
    console.log("On create CodesetStatisticsService");
    this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
  }
  ngOnDestroy() {
    console.log("Destroy CodesetStatisticsService");
    if (this.clearSub != null) this.clearSub.unsubscribe();
  }
  private clearSub: Subscription | null = null;


  public SkippedFullStats: boolean = false; //is true when we got stats, but not all of them
  public WouldSkipFullStats: boolean = false;//is true when, to get the full stats the service requires to override the "review size" check, i.e. when current review is considered "big".

  //@Output() GetCompletedSetsEmit: EventEmitter<any> = new EventEmitter<any>();
  //@Output() GetIncompleteSetsEmit: EventEmitter<any> = new EventEmitter<any>();

  private _ReviewStats: ReviewStatisticsCountsCommand = {
    itemsIncluded: -1,
    itemsExcluded: -1,
    itemsDeleted: -1,
    duplicateItems: -1
  };
  public get ReviewStats(): ReviewStatisticsCountsCommand {
    return this._ReviewStats;
  }

  public CodingProgressStats: ReviewStatisticsCodeSet2[] = [];

  public GetReviewStatisticsCountsCommand(DoAlsoDetailedStats: boolean = false, ForceDetailedStats: boolean = false) {
    this._BusyMethods.push("GetReviewStatisticsCountsCommand");
    lastValueFrom(this._http.get<ReviewStatisticsCountsCommand>(this._baseUrl + 'api/ReviewStatistics/ExcecuteReviewStatisticsCountCommand')
    ).then(
      data => {
        this.RemoveBusy("GetReviewStatisticsCountsCommand");
        this._ReviewStats = data;
        //let's check how many items and coding tools we have...
        const TotItems = data.itemsIncluded + data.itemsExcluded;
        const TotTools = this.reviewSetsService.ReviewSets.length;
        console.log("Should we skip the full stats? (tot items, tot tools)", TotItems, TotTools);
        if (TotItems > 100000 //just too many items!
          || (TotItems > 80000 && TotTools > 6) //lots of items and a fair amount of tools
          || (TotItems > 60000 && TotTools >= 9) //lots of items and a fair amount of tools
          || (TotItems > 40000 && TotTools >= 12) //plenty of items and a many tools
          || (TotItems > 30000 && TotTools >= 20) //quite a few items, but so many tools!
        ) {
          this.WouldSkipFullStats = true;
        }
        else {
          this.WouldSkipFullStats = false;
        }
        if (DoAlsoDetailedStats == true) {
          //we now want to get the details of complete/uncomplete coding numbers. But should we? 
          //If we probably have too much data, we won't unless ForceDetailedStats is true;
          if (ForceDetailedStats == true) {
            //we get all stats, no matter what!!
            this.GetAllReviewSetsCodingCounts();
          }
          else {
            //let's check how many items and coding tools we have...
            const TotItems = data.itemsIncluded + data.itemsExcluded;
            const TotTools = this.reviewSetsService.ReviewSets.length;
            //console.log("Should we skip the full stats? (tot items, tot tools)", TotItems, TotTools);
            if (this.WouldSkipFullStats) {
              this.SkippedFullStats = true;
              return;
            }
            else {
              this.GetAllReviewSetsCodingCounts();
            }
          }
        }
        //this.Save();
        //if (DoAlsoDetailedStats) this.GetCompletedSetsEmit.emit(data);
        return data;
      },
      (error) => {
        this.modalService.GenericError(error);
        this.RemoveBusy("GetReviewStatisticsCountsCommand");
      }).catch(() => {
        this.RemoveBusy("GetReviewStatisticsCountsCommand");
      });
  }
  public GetAllReviewSetsCodingCounts() {
    this.SkippedFullStats = false;
    this._BusyMethods.push("GetAllReviewSetsCodingCounts");
    lastValueFrom(this._http.get<iReviewStatisticsCodeSet2[]>(this._baseUrl + 'api/ReviewStatistics/FetchAllCounts')
    ).then(

      result => {
        this.CodingProgressStats = [];
        for (let iStat of result) {
          let index = this.reviewSetsService.ReviewSets.findIndex(f => f.set_id == iStat.setId);
          if (index != -1) {
            const RS = this.reviewSetsService.ReviewSets[index];
            let Stat: ReviewStatisticsCodeSet2 = new ReviewStatisticsCodeSet2(iStat, RS.order, RS.subTypeName, RS.codingIsFinal);
            this.CodingProgressStats.push(Stat);
          }
        }
        this.CodingProgressStats.sort(function (a, b) { return a.order - b.order });
        this.RemoveBusy("GetAllReviewSetsCodingCounts");
      }, error => {
        this.modalService.GenericError(error);
        this.RemoveBusy("GetAllReviewSetsCodingCounts");
      }
    ).catch((caught) => {
      this.modalService.GenericError(caught);
      this.RemoveBusy("GetAllReviewSetsCodingCounts");
    });

  }


  SendToItemBulkCompleteCommand(setID: number, contactID: number, completeOrNot: string): Promise<void> {

    this._BusyMethods.push("SendToItemBulkCompleteCommand");
    let MVCcmd: ItemSetBulkCompleteCommand = new ItemSetBulkCompleteCommand();
    MVCcmd.setID = setID;
    MVCcmd.contactID = contactID;
    MVCcmd.completeOrNot = completeOrNot;
    return lastValueFrom(this._http.post<ItemSetBulkCompleteCommand>(this._baseUrl + 'api/ReviewStatistics/ExcecuteItemSetBulkCompleteCommand',
      MVCcmd)).then(() => {

        this.GetAllReviewSetsCodingCounts();//refresh coding counts - we just changed completion states is bulk, so we'd better!
        this.RemoveBusy("SendToItemBulkCompleteCommand");

      }, error => {
        this.RemoveBusy("SendToItemBulkCompleteCommand");
        this.modalService.SendBackHomeWithError(error);
      }).catch(() => {
        this.RemoveBusy("SendToItemBulkCompleteCommand");
      });
  }
  SendItemsToBulkCompleteOrNotCommand(attributeId: number, isCompleting: string,
    setId: number, isPreview: string, reviewerId?: number): Promise<BulkCompleteUncompleteCommand> {

    this._BusyMethods.push("ItemsToBulkCompleteOrNotCommand");
    let MVCcmd: BulkCompleteUncompleteCommand = new BulkCompleteUncompleteCommand();
    MVCcmd.attributeId = attributeId;
    MVCcmd.isCompleting = isCompleting;
    MVCcmd.setId = setId;
    MVCcmd.reviewerId = reviewerId == null ? 0 : reviewerId;
    MVCcmd.isPreview = isPreview;
    return lastValueFrom(this._http.post<BulkCompleteUncompleteCommand>(this._baseUrl + 'api/ReviewStatistics/PreviewCompleteUncompleteCommand',
      MVCcmd)).then(
        (result) => {
          if (isPreview == 'false') {
            this.GetAllReviewSetsCodingCounts();//refresh coding counts - we just changed completion states is bulk, so we'd better!
          }
          this.RemoveBusy("ItemsToBulkCompleteOrNotCommand");
          return result;

        }, error => {
          this.RemoveBusy("ItemsToBulkCompleteOrNotCommand");
          this.modalService.SendBackHomeWithError(error);
          return error;
        }
      );
  }

  ExecuteBulkDeleteCodingCommand(command: iBulkDeleteCodingCommand): Promise<iBulkDeleteCodingCommand | boolean> {

    this._BusyMethods.push("ExecuteBulkDeleteCodingCommand");
    
    return lastValueFrom(this._http.post<iBulkDeleteCodingCommand>(this._baseUrl + 'api/ReviewStatistics/BulkDeleteCodingCommand',
      command)).then(
        (result) => {
          if (command.isPreview == false) {
            this.GetAllReviewSetsCodingCounts();//refresh coding counts - we just deleted coding in bulk, so we'd better!
          }
          this.RemoveBusy("ExecuteBulkDeleteCodingCommand");
          return result;

        }, error => {
          this.RemoveBusy("ExecuteBulkDeleteCodingCommand");
          this.modalService.GenericError(error);
          return false;
      }).catch(err => {
        this.RemoveBusy("ExecuteBulkDeleteCodingCommand");
        this.modalService.GenericError(err);
        return false;
      });
  }

  public Clear() {
    console.log("Clear on CodesetStatisticsService");
    this._ReviewStats = {
      itemsIncluded: -1,
      itemsExcluded: -1,
      itemsDeleted: -1,
      duplicateItems: -1
    };
    this.CodingProgressStats = [];
    this.SkippedFullStats = false;
    this.WouldSkipFullStats = false;
    //localStorage.removeItem('tmpCodesets');
    //localStorage.removeItem('CompletedSets');
    //localStorage.removeItem('IncompleteCodesets');
    //localStorage.removeItem('ReviewStats');
  }
}
export class ItemSetBulkCompleteCommand {

  public contactID: number = 0;
  public setID: number = 0;
  public completeOrNot: string = '';

}
export class BulkCompleteUncompleteCommand {

  public attributeId: number = 0;
  public isCompleting: string = '';
  public setId: number = 0;
  public reviewerId?: number = 0;
  public isPreview: string = '';
  public potentiallyAffectedItems: number = 0;
  public affectedItems: number = 0;

}
export interface ReviewStatisticsCountsCommand {
  itemsIncluded: number;
  itemsExcluded: number;
  itemsDeleted: number;
  duplicateItems: number;
}
export class ReviewStatisticsCodeSet {

  public setName: string = '';
  public title: string = '';
  public setId: number = 0;
  public numItems: number = 0;
  public completed: boolean = false;
  public reviewerStatistics: ReviewStatisticsReviewer[] = [];
}
export interface ReviewStatisticsReviewer {
  contactId: number;
  contactName: string;
  numItems: number;
  setId: number;
  setName: string;
  title: string;
}
export interface iReviewStatisticsCodeSet2 {
  setName: string;
  title: string;
  setId: number;
  numItemsCompleted: number;
  numItemsIncomplete: number;
  reviewerStatistics: iReviewStatisticsReviewer2[];
}
export class ReviewStatisticsCodeSet2 implements iReviewStatisticsCodeSet2 {
  constructor(stats: iReviewStatisticsCodeSet2, Order: number, SubTypeName: string, CodingIsFinal:boolean) {
    this.order = Order;
    this.subTypeName = SubTypeName;
    this.codingIsFinal = CodingIsFinal;
    this.setName = stats.setName;
    this.title = stats.title;
    this.setId = stats.setId;
    this.numItemsCompleted = stats.numItemsCompleted;
    this.numItemsIncomplete = stats.numItemsIncomplete;
    this.reviewerStatistics = stats.reviewerStatistics;
  }
  public order: number;
  public subTypeName: string;
  public codingIsFinal: boolean;

  public setName: string;
  public title: string;
  public setId: number;

  public numItemsCompleted: number;
  public numItemsIncomplete: number;
  public reviewerStatistics: iReviewStatisticsReviewer2[];
}
export interface iReviewStatisticsReviewer2 {
  contactId: number;
  contactName: string;
  numItemsCompleted: number;
  numItemsIncomplete: number;
  setId: number;
  setName: string;
  title: string;
}

export class StatsCompletion {

  setName: string = '';
  setId: number = 0;
  countCompleted: number = 0;
  countIncomplete: number = 0;
  public order: number = 0;
  public subTypeName: string = '';//admin, standard or screening
  codingIsFinal: boolean = true;//normal or comparison mode
  public CompletedByReviewer: ReviewStatisticsReviewer[] = [];
  public IncompleteByReviewer: ReviewStatisticsReviewer[] = [];
  private _StatsByReviewer: StatsByReviewer[] | null = null;
  public get ReviewerStats(): StatsByReviewer[] {
    // console.log("getting ReviewerStats", this.CompletedByReviewer, this.IncompleteByReviewer);
    if (this._StatsByReviewer == null) {
      //we parse the raw data once, to get the digested view used by the reviewstats component...
      //once data is in it's in :-)
      let result: StatsByReviewer[] = [];
      let ContactIds: number[] = [];
      for (let singleStat of this.CompletedByReviewer) {
        if (ContactIds.indexOf(singleStat.contactId) == -1) {
          ContactIds.push(singleStat.contactId);
        }
      }
      for (let singleStat of this.IncompleteByReviewer) {
        if (ContactIds.indexOf(singleStat.contactId) == -1) {
          ContactIds.push(singleStat.contactId);
        }
      }
      for (let CID of ContactIds) {
        let perCID: StatsByReviewer = new StatsByReviewer();
        perCID.ContactId = CID;
        perCID.SetId = this.setId;
        let tmp = this.CompletedByReviewer.find(found => found.contactId == CID);
        if (tmp) {
          perCID.CompletedCount = tmp.numItems;
          if (perCID.ContactName == "") perCID.ContactName = tmp.contactName;
        }
        tmp = this.IncompleteByReviewer.find(found => found.contactId == CID);
        if (tmp) {
          perCID.IncompleteCount = tmp.numItems;
          if (perCID.ContactName == "") perCID.ContactName = tmp.contactName;
        }
        result.push(perCID);
      }
      this._StatsByReviewer = result;
    }
    return this._StatsByReviewer;
  }
}
export class StatsByReviewer {
  ContactId: number = 0;
  ContactName: string = "";
  SetId: number = 0;
  CompletedCount: number = 0;
  IncompleteCount: number = 0;
}
export interface iBulkDeleteCodingCommand {
  setId: number;
  reviewerId: number;
  isPreview: boolean;
  totalItemsAffected: number;
  completedCodingToBeDeleted: number;
  incompletedCodingToBeDeleted: number;
  hiddenIncompletedCodingToBeDeleted: number;
}
