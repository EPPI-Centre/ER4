//import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
//import { HttpClient } from '@angular/common/http';
//import { Router } from '@angular/router';
//import { ReviewerIdentityService } from '../services/revieweridentity.service';
//import { CodesetStatisticsService, ReviewStatisticsCountsCommand } from '../services/codesetstatistics.service';
//import { ReviewSetsService } from '../services/ReviewSets.service';

//@Component({
//    selector: 'reviewStatistics',
//    templateUrl: './mainfull.component.html',
//    providers: []
//})
//export class CodesetStatisticsComponent implements OnInit, OnDestroy {

//    constructor(private router: Router,
//                private _httpC: HttpClient,
//                @Inject('BASE_URL') private _baseUrl: string,
//        private ReviewerIdentityServ: ReviewerIdentityService,
//                private ReviewSetsServ: ReviewSetsService,
//        public _codesetStatisticsServ: CodesetStatisticsService) {
 
//    }

   
//    getCodesetStatistics() {

//        if (this.ReviewSetsServ.ReviewSets.length == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {

//            let cmd = new ReviewStatisticsCountsCommand();
//            this._codesetStatisticsServ.GetReviewStatisticsCountsCommand(cmd);
//        }
//    }

//    ngOnInit() {

//        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
//            console.log('user is empty...');
//            this.router.navigate(['home']);
//        }
//        else {

//            this.getCodesetStatistics();
//        }
//    }
//    ngOnDestroy() {

//    }

//}
