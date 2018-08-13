import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode } from '../services/ReviewSets.service';

@Component({
    selector: 'reviewsets',
    templateUrl: './fetchreviewsets.component.html'
})
export class ReviewSetsComponent implements OnInit {
   constructor(private router: Router,
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
        private ReviewSetsService: ReviewSetsService
    ) {}
    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else {
            console.log("Review Ticket: " + this.ReviewerIdentityServ.reviewerIdentity.ticket);
            this.GetReviewSets();
        }
    }

    nodes: singleNode[] = [];

    GetReviewSets() {
        this.ReviewSetsService.GetReviewSets().subscribe(
            result => {
                this.ReviewSetsService.ReviewSets = result;
                this.nodes = this.ReviewSetsService.ReviewSets as singleNode[];
            }, error => {
                console.error(error);
                this.router.navigate(['main']);
            }
            );
    }
}




