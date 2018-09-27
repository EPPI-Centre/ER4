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

    public stats: ReviewStatisticsCountsCommand = new ReviewStatisticsCountsCommand();

    public GetReviewStatisticsCountsCommand(cmd: ReviewStatisticsCountsCommand) {

       this._http.post<ReviewStatisticsCountsCommand>(this._baseUrl + 'api/Review/ExcecuteReviewStatisticsCountCommand', cmd).subscribe(

           data => {
               cmd = data;                
               this.stats = data;
               this.Save();
               console.log('checking the stats data' + JSON.stringify(this.stats));
               return cmd;
            }
        );
       
    }
    private Save() {
        if (this.stats != undefined && this.stats != null ) //{ }
            localStorage.setItem('stats', JSON.stringify(this.stats));
        else if (localStorage.getItem('stats')) localStorage.removeItem('stats');
    }
       
}
export class ReviewStatisticsCountsCommand {

    public itemsIncluded: number = 0;
    public itemsExcluded: number = 0;
    public itemsDeleted: number = 0;
    public duplicateItems: number = 0;
}
