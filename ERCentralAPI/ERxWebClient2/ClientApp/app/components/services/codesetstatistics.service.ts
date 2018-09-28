import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { arm, Item } from './ItemList.service';
import { filter } from 'rxjs/operators';

@Injectable({
    providedIn: 'root',
})

export class CodesetStatisticsService {

    constructor(
        private _http: HttpClient,
        private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
       
    }

    private _CompletedCodesets: ReviewStatisticsCodeSet[] = [];
    private _IncompleteCodesets: ReviewStatisticsCodeSet[] = [];

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
                //console.log("Got User from LS");
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
               //console.log('checking the stats data' + JSON.stringify(this._ReviewStats));
               return data;
            }
        );
    }

    public GetReviewSetsCompletedCodingCounts(completed: boolean) {

        let body = JSON.stringify({ Value: completed });
        this._http.post<ReviewStatisticsCodeSet[]>(this._baseUrl + 'api/ReviewStatistics/FetchCounts',
            body).subscribe(result => {

                this._CompletedCodesets = result;
                //console.log(result);
                this.SaveCompletedSets();
                return result;

            }, error => { this.modalService.SendBackHomeWithError(error);}
        );

    }

    public GetReviewSetsIncompleteCodingCounts(completed: boolean) {

        let body = JSON.stringify({ Value: completed });
        this._http.post<ReviewStatisticsCodeSet[]>(this._baseUrl + 'api/ReviewStatistics/FetchCounts',
            body).subscribe(result => {

                this._IncompleteCodesets = result;
                console.log(this._IncompleteCodesets);
                this.SaveIncompleteSets();
                //return result;

            }, error => { this.modalService.SendBackHomeWithError(error); }
        );

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
    public reviewStatistics: ReviewStatistics[] = [];
}


export interface ReviewStatistics {

    contactId: number;
    contactName: string;
    isBusy: boolean;
    isChild: boolean;
    isCompleted: boolean;
    isDeleted: boolean;
    isDirty: boolean;
    isNew: boolean;
    isSavable: boolean;
    isSelfBusy: boolean;
    isSelfDirty: boolean;
    isSelfValid: boolean;
    isValid: boolean;
    numItems: number;
    setId: number;
    setName: string;
    title: string;

}