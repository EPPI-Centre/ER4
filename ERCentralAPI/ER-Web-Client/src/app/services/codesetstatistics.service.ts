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
    private _CompletedCodesets: ReviewStatisticsCodeSet[] = [];
	private _IncompleteCodesets: ReviewStatisticsCodeSet[] = [];
    private _tmpCodesets: StatsCompletion[] = [];
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
    public get CompletedCodesets(): ReviewStatisticsCodeSet[]  {
        return this._CompletedCodesets;
	}
	public set CompletedCodesets(CompletedCodesets: ReviewStatisticsCodeSet[]) {
		this._CompletedCodesets = CompletedCodesets;
	}
	public set IncompleteCodesets(IncompleteCodesets: ReviewStatisticsCodeSet[]) {
		this._IncompleteCodesets = IncompleteCodesets;
	}
	public set tmpCodesets(tmpCodesets: StatsCompletion[]) {
		this._tmpCodesets = tmpCodesets;
	}
	public get tmpCodesets(): StatsCompletion[] {
		return this._tmpCodesets;
    }
	public get IncompleteCodesets(): ReviewStatisticsCodeSet[] {
		return this._IncompleteCodesets;
	}
    public GetReviewStatisticsCountsCommand(DoAlsoDetailedStats: boolean = false, ForceDetailedStats: boolean = false) {
        this._BusyMethods.push("GetReviewStatisticsCountsCommand");
        this._http.get<ReviewStatisticsCountsCommand>(this._baseUrl + 'api/ReviewStatistics/ExcecuteReviewStatisticsCountCommand').subscribe(
           data => {
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
                        this.GetReviewSetsCodingCounts(true);
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
                            this.GetReviewSetsCodingCounts(true);
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
            },
            () => {
                this.RemoveBusy("GetReviewStatisticsCountsCommand");
            }
        );
    }
    public GetReviewSetsCodingCounts(completed: boolean) {
        this.SkippedFullStats = false;
        this._BusyMethods.push("GetReviewSetsCodingCounts");
        let body = JSON.stringify({ Value: completed });
        this._http.post<ReviewStatisticsCodeSet[]>(this._baseUrl + 'api/ReviewStatistics/FetchCounts',
            body).subscribe(

            result => {

                this.CompletedCodesets = result;
                this.GetReviewSetsIncompleteCodingCounts(false);

            }, error => {
                this.modalService.SendBackHomeWithError(error);
                this.RemoveBusy("GetReviewSetsCodingCounts");
            }
            , () => {
                this.RemoveBusy("GetReviewSetsCodingCounts");
            }

        );

    }
    public GetReviewSetsIncompleteCodingCounts(completed: boolean) {
        this._BusyMethods.push("GetReviewSetsIncompleteCodingCounts");
        let body = JSON.stringify({ Value: completed });
        this._http.post<ReviewStatisticsCodeSet[]>(this._baseUrl + 'api/ReviewStatistics/FetchCounts',
            body).subscribe(result => {

                this.IncompleteCodesets = result;
                //console.log('incomplete sets' + JSON.stringify(result.map((x) => x.setName + ' ' + x.numItems)) + '\n');
                //this.SaveIncompleteSets();
                //this.GetIncompleteSetsEmit.emit(result);
                this.formateSets();
                //console.log(this._tmpCodesets);
                this.RemoveBusy("GetReviewSetsIncompleteCodingCounts");

            }, error => {
                this.RemoveBusy("GetReviewSetsIncompleteCodingCounts");
                this.modalService.SendBackHomeWithError(error);
            }, () => {
                this.RemoveBusy("GetReviewSetsIncompleteCodingCounts");
            }
        );

        
    }
    formateSets(): any {
        this.tmpCodesets = [];
        let ind: number = 0;
        for (var i = 0; i < this.reviewSetsService.ReviewSets.length; i++) {

            //console.log(this.reviewSetsService.ReviewSets[i].set_name + '\n');
            

            var tempSetName = this.reviewSetsService.ReviewSets[i].set_name;
            var tempSetId = this.reviewSetsService.ReviewSets[i].set_id;
            let index1: number = this.CompletedCodesets.findIndex(x => x.setId == tempSetId);
            let index2: number = this.IncompleteCodesets.findIndex(x => x.setId == tempSetId);
            let tmpSet = new StatsCompletion();
            tmpSet.codingIsFinal = this.reviewSetsService.ReviewSets[i].codingIsFinal;
            tmpSet.order = this.reviewSetsService.ReviewSets[i].order;
            tmpSet.subTypeName = this.reviewSetsService.ReviewSets[i].subTypeName;
            tmpSet.setName = tempSetName;
            tmpSet.setId = tempSetId;
            if (index1 != -1 && index2 != -1) {

                ind += 1;
                let tmp: ReviewStatisticsCodeSet | undefined = this.CompletedCodesets.find(x => x.setId == tempSetId);
                if (tmp) {
                    //console.log("found this:", tmp, tmp.reviewerStatistics);
                    tmpSet.countCompleted = tmp.numItems;
                    tmpSet.CompletedByReviewer = tmpSet.CompletedByReviewer.concat(tmp.reviewerStatistics);
                    let tmpI: ReviewStatisticsCodeSet | undefined = this.IncompleteCodesets.find(x => x.setId == tempSetId);
                    if (tmpI) {
                        tmpSet.IncompleteByReviewer = tmpSet.IncompleteByReviewer.concat(tmpI.reviewerStatistics);
                            tmpSet.countIncomplete = tmpI.numItems;
                            this.tmpCodesets.push(tmpSet);
                            continue;
                        }
                }
               
            }
            if (index1 != -1) {

                ind += 1;
                let tmp: ReviewStatisticsCodeSet | undefined = this.CompletedCodesets.find(x => x.setId == tempSetId);
                
                if (tmp) {
                    tmpSet.CompletedByReviewer = tmpSet.CompletedByReviewer.concat(tmp.reviewerStatistics);
                    tmpSet.countCompleted = tmp.numItems;
                    tmpSet.countIncomplete = 0;
                    this.tmpCodesets.push(tmpSet);
                    continue;
                }
                
            }
            if (index2 != -1) {

                ind += 1;
                //console.log('single find in incomplete');
                let tmpI: ReviewStatisticsCodeSet | undefined = this.IncompleteCodesets.find(x => x.setId == tempSetId);

                if (tmpI) {
                    tmpSet.IncompleteByReviewer = tmpSet.IncompleteByReviewer.concat(tmpI.reviewerStatistics);
                    tmpSet.countCompleted = 0;
                    tmpSet.countIncomplete = tmpI.numItems;
                    this.tmpCodesets.push(tmpSet);
                    continue;
                }
               
            }
            tmpSet.countCompleted = 0;
            tmpSet.countIncomplete = 0;
            this.tmpCodesets.push(tmpSet);

        }
        this.tmpCodesets.sort(function (a, b) { return a.order - b.order });
        //this.SaveFormattedSets();
	}
	SendToItemBulkCompleteCommand(setID: number, contactID: number, completeOrNot: string) : void {
			   
		this._BusyMethods.push("SendToItemBulkCompleteCommand");
		let MVCcmd: ItemSetBulkCompleteCommand = new ItemSetBulkCompleteCommand();
		MVCcmd.setID = setID;
		MVCcmd.contactID = contactID;
		MVCcmd.completeOrNot = completeOrNot;
		this._http.post<ItemSetBulkCompleteCommand>(this._baseUrl + 'api/ReviewStatistics/ExcecuteItemSetBulkCompleteCommand',
			MVCcmd).subscribe(() => {

                this.GetReviewSetsCodingCounts(true);//refresh coding counts - we just changed completion states is bulk, so we'd better!
				this.RemoveBusy("SendToItemBulkCompleteCommand");
	
				}, error => {
					this.RemoveBusy("SendToItemBulkCompleteCommand");
					this.modalService.SendBackHomeWithError(error);
				}, () => {
					this.RemoveBusy("SendToItemBulkCompleteCommand");
				}
			);
	}
	SendItemsToBulkCompleteOrNotCommand(attributeId: number, isCompleting: string,
		setId: number, isPreview: string, reviewerId?: number): Promise<BulkCompleteUncompleteCommand>  {

		this._BusyMethods.push("ItemsToBulkCompleteOrNotCommand");
		let MVCcmd: BulkCompleteUncompleteCommand = new BulkCompleteUncompleteCommand();
		MVCcmd.attributeId = attributeId;
		MVCcmd.isCompleting = isCompleting;
		MVCcmd.setId = setId;
		MVCcmd.reviewerId = reviewerId == null? 0 : reviewerId;
		MVCcmd.isPreview = isPreview;
    return lastValueFrom(this._http.post<BulkCompleteUncompleteCommand>(this._baseUrl + 'api/ReviewStatistics/PreviewCompleteUncompleteCommand',
			MVCcmd)).then(
                (result) => {
                    if (isPreview == 'false') {
                        this.GetReviewSetsCodingCounts(true);//refresh coding counts - we just changed completion states is bulk, so we'd better!
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

    public Clear() {
        console.log("Clear on CodesetStatisticsService");
        this._ReviewStats = {
            itemsIncluded: -1,
            itemsExcluded: -1,
            itemsDeleted: -1,
            duplicateItems: -1
        };
        this._tmpCodesets = [];
        this._CompletedCodesets = [];
        this._IncompleteCodesets = [];
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

	public attributeId: number= 0;
	public isCompleting: string = '';
	public setId: number =0;
	public reviewerId?: number=0;
	public isPreview: string = '';
	public potentiallyAffectedItems : number = 0;
	public affectedItems: number =0;

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
