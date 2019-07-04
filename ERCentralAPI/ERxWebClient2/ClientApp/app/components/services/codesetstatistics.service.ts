import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { ReviewSetsService, ReviewSet } from './ReviewSets.service';
import { Subject, Subscription } from 'rxjs';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ItemArmDeleteWarningCommandJSON } from './arms.service';

@Injectable({
    providedIn: 'root',
})

export class CodesetStatisticsService extends BusyAwareService {

    constructor(
        private _http: HttpClient,
        private modalService: ModalService,
        private reviewSetsService: ReviewSetsService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }

    private _CompletedCodesets: ReviewStatisticsCodeSet[] = [];
	private _IncompleteCodesets: ReviewStatisticsCodeSet[] = [];
	private _tmpCodesets: StatsCompletion[] = [];

    @Output() GetCompletedSetsEmit: EventEmitter<any> = new EventEmitter<any>();
    @Output() GetIncompleteSetsEmit: EventEmitter<any> = new EventEmitter<any>();

    private _ReviewStats: ReviewStatisticsCountsCommand = {
        itemsIncluded: -1,
        itemsExcluded: -1,
        itemsDeleted: -1,
        duplicateItems: -1
    };
    public get ReviewStats(): ReviewStatisticsCountsCommand {
        //if (this._ReviewStats.itemsIncluded != -1 ) {
        //    return this._ReviewStats;
        //}
        //else {
        //    const ReviewStatsJson = localStorage.getItem('ReviewStats');
        //    let rev_Stats: ReviewStatisticsCountsCommand = ReviewStatsJson !== null ? JSON.parse(ReviewStatsJson) : null;
        //    if (rev_Stats == undefined || rev_Stats == null || rev_Stats.itemsIncluded == -1) {

        //        return this._ReviewStats;
        //    }
        //    else {
        //        this._ReviewStats = rev_Stats;
        //    }
        //}
        return this._ReviewStats;
    }
    public get CompletedCodesets(): ReviewStatisticsCodeSet[]  {
        //if (this._CompletedCodesets != null ) {
        //    return this._CompletedCodesets;
        //}
        //else {
        //    const CompletedCodesetsJson = localStorage.getItem('CompletedCodesets');
        //    let CompletedSets: ReviewStatisticsCodeSet[]  = CompletedCodesetsJson !== null ? JSON.parse(CompletedCodesetsJson) : null;
        //    if (CompletedSets == undefined || CompletedSets == null  ) {

        //        return this._CompletedCodesets;
        //    }
        //    else {
        //        this._CompletedCodesets = CompletedSets;
        //    }
        //}
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

  //      if (this._tmpCodesets != null) {
  //          return this._tmpCodesets;
  //      }
  //      else {
		//	const tmpCodesetsJson = localStorage.getItem('tmpCodesets');
		//	let tmpCodesets: StatsCompletion[] = tmpCodesetsJson !== null ? JSON.parse(tmpCodesetsJson) : null;
		//	if (tmpCodesets == undefined || tmpCodesets == null) {

  //              return this._tmpCodesets;
  //          }
  //          else {
		//		this._tmpCodesets = tmpCodesets;
  //          }
		//}
		//console.log('blah ' + this._tmpCodesets);
		return this._tmpCodesets;
    }
	public get IncompleteCodesets(): ReviewStatisticsCodeSet[] {
		//if (this._IncompleteCodesets != null) {
		//	return this._IncompleteCodesets;
		//}
		//else {
		//	const IncompleteCodesetsJson = localStorage.getItem('IncompleteCodesets');
		//	let IncompleteSets: ReviewStatisticsCodeSet[] = IncompleteCodesetsJson !== null ? JSON.parse(IncompleteCodesetsJson) : null;
		//	if (IncompleteSets == undefined || IncompleteSets == null) {

		//		return this._IncompleteCodesets;
		//	}
		//	else {
		//		this._IncompleteCodesets = IncompleteSets;
		//	}
		//}
		return this._IncompleteCodesets;
	}
    public GetReviewStatisticsCountsCommand() {
        this._BusyMethods.push("GetReviewStatisticsCountsCommand");
        this._http.get<ReviewStatisticsCountsCommand>(this._baseUrl + 'api/ReviewStatistics/ExcecuteReviewStatisticsCountCommand').subscribe(
           data => {
               this._ReviewStats = data;
               //this.Save();
               this.GetCompletedSetsEmit.emit(data);
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
    public GetReviewSetsCodingCounts(completed: boolean, trigger: Subject<any>) {
        this._BusyMethods.push("GetReviewSetsCodingCounts");
        let body = JSON.stringify({ Value: completed });
        this._http.post<ReviewStatisticsCodeSet[]>(this._baseUrl + 'api/ReviewStatistics/FetchCounts',
            body).subscribe(

            result => {

                this.CompletedCodesets = result;
                console.log('complete sets: ', result);
                //console.log('complete sets: ' + JSON.stringify(result.map((x) => x.setName + ' ' + x.numItems)));
                //this.SaveCompletedSets();
                this.GetCompletedSetsEmit.emit(result);
                this.GetReviewSetsIncompleteCodingCounts(false, trigger);

            }, error => {
                this.modalService.SendBackHomeWithError(error);
                this.RemoveBusy("GetReviewSetsCodingCounts");
            }
            , () => {
                this.RemoveBusy("GetReviewSetsCodingCounts");
            }

        );

    }
    public GetReviewSetsIncompleteCodingCounts(completed: boolean, trigger: Subject<any>) {
        this._BusyMethods.push("GetReviewSetsIncompleteCodingCounts");
        let body = JSON.stringify({ Value: completed });
        this._http.post<ReviewStatisticsCodeSet[]>(this._baseUrl + 'api/ReviewStatistics/FetchCounts',
            body).subscribe(result => {

                this.IncompleteCodesets = result;
                //console.log('incomplete sets' + JSON.stringify(result.map((x) => x.setName + ' ' + x.numItems)) + '\n');
                //this.SaveIncompleteSets();
                this.GetIncompleteSetsEmit.emit(result);
                this.formateSets();
                //console.log(this._tmpCodesets);
                this.RemoveBusy("GetReviewSetsIncompleteCodingCounts");
                trigger.next();

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
                    console.log("found this:", tmp, tmp.reviewerStatistics);
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
                let tmp: ReviewStatisticsCodeSet | undefined = this.CompletedCodesets.find(x => x.setName == tempSetName);
                
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
                let tmpI: ReviewStatisticsCodeSet | undefined = this.IncompleteCodesets.find(x => x.setName == tempSetName);

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

				this.GetReviewStatisticsCountsCommand();
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
		return this._http.post<BulkCompleteUncompleteCommand>(this._baseUrl + 'api/ReviewStatistics/PreviewCompleteUncompleteCommand',
			MVCcmd).toPromise().then(

			(result) => {

				this.RemoveBusy("ItemsToBulkCompleteOrNotCommand");
				return result;
				
			}, error => {
				this.RemoveBusy("ItemsToBulkCompleteOrNotCommand");
				this.modalService.SendBackHomeWithError(error);
				return error;
			}
		);
	}
    //private Save() {
    //    if (this.ReviewStats != undefined && this.ReviewStats != null )
    //        localStorage.setItem('ReviewStats', JSON.stringify(this.ReviewStats));
    //    else if (localStorage.getItem('ReviewStats')) localStorage.removeItem('ReviewStats');
    //}
    //private SaveCompletedSets() {
    //    if (this._CompletedCodesets != undefined && this._CompletedCodesets != null) 
    //        localStorage.setItem('CompletedSets', JSON.stringify(this._CompletedCodesets));
    //    else if (localStorage.getItem('CompletedSets')) localStorage.removeItem('CompletedSets');
    //}
    //private SaveIncompleteSets() {
    //    if (this._IncompleteCodesets != undefined && this._IncompleteCodesets != null) 
    //        localStorage.setItem('IncompleteSets', JSON.stringify(this._IncompleteCodesets));
    //    else if (localStorage.getItem('IncompleteSets')) localStorage.removeItem('IncompleteSets');
    //}
    //private SaveFormattedSets() {
    //    console.log('saving formatted sets')
    //    if (this._tmpCodesets != undefined && this._tmpCodesets != null)
    //        localStorage.setItem('tmpCodesets', JSON.stringify(this._tmpCodesets));
    //    else if (localStorage.getItem('tmpCodesets')) localStorage.removeItem('tmpCodesets');
    //}
    public Clear() {
        this._ReviewStats = {
            itemsIncluded: -1,
            itemsExcluded: -1,
            itemsDeleted: -1,
            duplicateItems: -1
        };
        this._tmpCodesets = [];
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