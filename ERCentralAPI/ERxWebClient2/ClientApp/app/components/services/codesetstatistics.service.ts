import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { arm, Item } from './ItemList.service';

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
    public GetReviewStatisticsCountsCommand() {

       this._http.get<ReviewStatisticsCountsCommand>(this._baseUrl + 'api/Review/ExcecuteReviewStatisticsCountCommand').subscribe(
           data => {    
               this._ReviewStats = data;
               this.Save();
               console.log('checking the stats data' + JSON.stringify(this._ReviewStats));
               return data;
            }
        );
       
    }
    public GetReviewSetsCompletedCodingCounts(RevId: boolean) {

        let body = JSON.stringify({ Value: RevId });
        this._http.post<ReviewStatisticsCodeSet[]>(this._baseUrl + 'api/Review/FetchCounts',
            body).subscribe(result => {


                console.log(JSON.stringify(result));
                
                //this.Save();
            }, error => { this.modalService.SendBackHomeWithError(error); }
        );

    }


    private Save() {
        if (this.ReviewStats != undefined && this.ReviewStats != null ) //{ }
            localStorage.setItem('ReviewStats', JSON.stringify(this.ReviewStats));
        else if (localStorage.getItem('ReviewStats')) localStorage.removeItem('ReviewStats');
    }
       
}
export interface ReviewStatisticsCountsCommand {
    itemsIncluded: number;
    itemsExcluded: number;
    itemsDeleted: number;
    duplicateItems: number;
}

export class ReviewStatisticsCodeSet {

    public SetName: string = '';
    public title: string = '';
    public setId: number = 0;
    public numItems: number = 0;
    public completed: boolean = false;

}
