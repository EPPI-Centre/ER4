import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { arm, Item } from './ItemList.service';
import { filter } from 'rxjs/operators';
import { ReviewSetsService, ReviewSet } from './ReviewSets.service';

@Injectable({
    providedIn: 'root',
})

export class CodesetStatisticsService {

    constructor(
        private _http: HttpClient,
        private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        private reviewSetsService: ReviewSetsService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
       
    }

    public _CompletedCodesets: ReviewStatisticsCodeSet[] = [];
    public _IncompleteCodesets: ReviewStatisticsCodeSet[] = [];
    public _tmpCodesets: StatsCompletion[] = [];

    @Output() GetCompletedSetsEmit: EventEmitter<any> = new EventEmitter<any>();
    @Output() GetIncompleteSetsEmit: EventEmitter<any> = new EventEmitter<any>();

    private _ReviewStats: ReviewStatisticsCountsCommand = {
        itemsIncluded: -1,
        itemsExcluded: -1,
        itemsDeleted: -1,
        duplicateItems: -1
    };
    public get ReviewStats(): ReviewStatisticsCountsCommand {
        if (this._ReviewStats.itemsIncluded != -1 ) {
            return this._ReviewStats;
        }
        else {
            const ReviewStatsJson = localStorage.getItem('ReviewStats');
            let rev_Stats: ReviewStatisticsCountsCommand = ReviewStatsJson !== null ? JSON.parse(ReviewStatsJson) : null;
            if (rev_Stats == undefined || rev_Stats == null || rev_Stats.itemsIncluded == -1) {

                return this._ReviewStats;
            }
            else {
                this._ReviewStats = rev_Stats;
            }
        }
        return this._ReviewStats;
    }
    public get CompletedCodesets(): ReviewStatisticsCodeSet[]  {
        if (this._CompletedCodesets != null ) {
            return this._CompletedCodesets;
        }
        else {
            const CompletedCodesetsJson = localStorage.getItem('CompletedCodesets');
            let CompletedSets: ReviewStatisticsCodeSet[]  = CompletedCodesetsJson !== null ? JSON.parse(CompletedCodesetsJson) : null;
            if (CompletedSets == undefined || CompletedSets == null  ) {

                return this._CompletedCodesets;
            }
            else {
                this._CompletedCodesets = CompletedSets;
            }
        }
        return this._CompletedCodesets;
    }

    public get IncompleteCodesets(): ReviewStatisticsCodeSet[] {
        if (this._IncompleteCodesets != null) {
            return this._IncompleteCodesets;
        }
        else {
            const IncompleteCodesetsJson = localStorage.getItem('IncompleteCodesets');
            let IncompleteSets: ReviewStatisticsCodeSet[] = IncompleteCodesetsJson !== null ? JSON.parse(IncompleteCodesetsJson) : null;
            if (IncompleteSets == undefined || IncompleteSets == null) {

                return this._IncompleteCodesets;
            }
            else {
                this._IncompleteCodesets = IncompleteSets;
            }
        }
        return this._IncompleteCodesets;
    }

    public GetReviewStatisticsCountsCommand() {

       this._http.get<ReviewStatisticsCountsCommand>(this._baseUrl + 'api/ReviewStatistics/ExcecuteReviewStatisticsCountCommand').subscribe(
           data => {    
               
               this._ReviewStats = data;
               this.Save();
               this.GetCompletedSetsEmit.emit(data);
               
               return data;
            }
        );
    }

    public GetReviewSetsCodingCounts(completed: boolean) {

        let body = JSON.stringify({ Value: completed });
        this._http.post<ReviewStatisticsCodeSet[]>(this._baseUrl + 'api/ReviewStatistics/FetchCounts',
            body).subscribe(

            result => {

                this._CompletedCodesets = result;
                //console.log('complete sets: ' + JSON.stringify(result.map((x) => x.setName + ' ' + x.numItems)));
                this.SaveCompletedSets();
                this.GetCompletedSetsEmit.emit(result);
               
                this.GetReviewSetsIncompleteCodingCounts(false);

            }, error => { this.modalService.SendBackHomeWithError(error); }

        );

    }

    public GetReviewSetsIncompleteCodingCounts(completed: boolean) {

        let body = JSON.stringify({ Value: completed });
        this._http.post<ReviewStatisticsCodeSet[]>(this._baseUrl + 'api/ReviewStatistics/FetchCounts',
            body).subscribe(result => {

                this._IncompleteCodesets = result;
                //console.log('incomplete sets' + JSON.stringify(result.map((x) => x.setName + ' ' + x.numItems)) + '\n');
                this.SaveIncompleteSets();
                this.GetIncompleteSetsEmit.emit(result);
                
                this.formateSets();

            }, error => { this.modalService.SendBackHomeWithError(error); }
        );

        
    }

    formateSets(): any {

        let ind: number = 0;
        for (var i = 0; i < this.reviewSetsService.ReviewSets.length; i++) {

            //console.log(this.reviewSetsService.ReviewSets[i].set_name + '\n');
            

            var tempSetName = this.reviewSetsService.ReviewSets[i].set_name;
            let index1: number = this._CompletedCodesets.findIndex(x => x.setName == tempSetName);
            let index2: number = this._IncompleteCodesets.findIndex(x => x.setName == tempSetName);

            if (index1 != -1 && index2 != -1) {

                ind += 1;
                let tmp: ReviewStatisticsCodeSet | undefined = this._CompletedCodesets.find(x => x.setName == tempSetName);
                if (tmp) {
                        let tmpSet = new StatsCompletion();
                        tmpSet.setName = tempSetName;
                        tmpSet.countCompleted = tmp.numItems;
                
                        let tmpI: ReviewStatisticsCodeSet | undefined = this._IncompleteCodesets.find(x => x.setName == tempSetName);

                    if (tmpI) {
                        tmpSet.ID = ind;
                            tmpSet.countIncomplete = tmpI.numItems;
                            this._tmpCodesets.push(tmpSet);
                            continue;
                        }
                }
               
            }
            if (index1 != -1) {

                ind += 1;
                let tmp: ReviewStatisticsCodeSet | undefined = this._CompletedCodesets.find(x => x.setName == tempSetName);
                
                if (tmp) {
                    let tmpSet = new StatsCompletion();

                    tmpSet.ID = ind;
                    tmpSet.setName = tempSetName;
                    tmpSet.countCompleted = tmp.numItems;
                    tmpSet.countIncomplete = 0;
                    this._tmpCodesets.push(tmpSet);
                    continue;
                }
                
            }
            if (index2 != -1) {

                ind += 1;
                //console.log('single find in incomplete');
                let tmpI: ReviewStatisticsCodeSet | undefined = this._IncompleteCodesets.find(x => x.setName == tempSetName);

                if (tmpI) {
                    let tmpSet = new StatsCompletion();
                    tmpSet.ID = ind;
                    tmpSet.setName = tempSetName;
                    tmpSet.countCompleted = 0;
                    tmpSet.countIncomplete = tmpI.numItems;
                    this._tmpCodesets.push(tmpSet);
                    continue;
                }
               
            }

            let tmpSet = new StatsCompletion();
            tmpSet.setName = tempSetName;
            tmpSet.countCompleted = 0;
            tmpSet.countIncomplete = 0;
            this._tmpCodesets.push(tmpSet);

        }

        this.SaveFormattedSets();
    }
    
    private Save() {
        if (this.ReviewStats != undefined && this.ReviewStats != null )
            localStorage.setItem('ReviewStats', JSON.stringify(this.ReviewStats));
        else if (localStorage.getItem('ReviewStats')) localStorage.removeItem('ReviewStats');
    }
    private SaveCompletedSets() {
        if (this._CompletedCodesets != undefined && this._CompletedCodesets != null) 
            localStorage.setItem('CompletedSets', JSON.stringify(this._CompletedCodesets));
        else if (localStorage.getItem('CompletedSets')) localStorage.removeItem('CompletedSets');
    }
    private SaveIncompleteSets() {
        if (this._IncompleteCodesets != undefined && this._IncompleteCodesets != null) 
            localStorage.setItem('IncompleteSets', JSON.stringify(this._IncompleteCodesets));
        else if (localStorage.getItem('IncompleteSets')) localStorage.removeItem('IncompleteSets');
    }
    private SaveFormattedSets() {
        if (this._tmpCodesets != undefined && this._tmpCodesets != null)
            localStorage.setItem('tmpCodesets', JSON.stringify(this._tmpCodesets));
        else if (localStorage.getItem('tmpCodesets')) localStorage.removeItem('tmpCodesets');
    }
       
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
    public ReviewStatisticsReviewer: ReviewStatisticsReviewer[] = [];
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

    ID: number = 0;
    setName: string = '';
    countCompleted: number = 0;
    countIncomplete: number = 0;

}