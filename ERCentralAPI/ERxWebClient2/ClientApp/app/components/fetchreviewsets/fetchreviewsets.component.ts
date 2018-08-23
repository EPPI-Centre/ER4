import { Component, Inject, OnInit, Output, EventEmitter } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet } from '../services/ReviewSets.service';
import { ITreeOptions } from 'angular-tree-component';

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
    ) { }

    

    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else {
            console.log("Review Ticket: " + this.ReviewerIdentityServ.reviewerIdentity.ticket);
            this.GetReviewSets();
        }
    }
    options: ITreeOptions = {
        childrenField: 'attributes', 
        displayField: 'name',
        allowDrag: false,
    }
    //nodes: singleNode[] = [];
    get nodes(): singleNode[] | null {
        //console.log('Getting codetree nodes');
        if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) 
        {
            //console.log('found my nodes');
            return this.ReviewSetsService.ReviewSets;
        }
        else {
            //console.log('NO nodes');
            return null;
        }
    }
    GetReviewSets() {
        if (this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) return;
        this.ReviewSetsService.GetReviewSets();
            //.subscribe(
            //result => {
            //    this.ReviewSetsService.ReviewSets = result;
            //    this.nodes = this.ReviewSetsService.ReviewSets;// as singleNode[];
            //}, error => {
            //    console.error(error);
            //    this.router.navigate(['main']);
            //}
            //);
    }
    CheckBoxClicked(event: any, AttId: number, additionalText: string, armId: number) {
        
        let evdata: CheckBoxClickedEventData = new CheckBoxClickedEventData();
        evdata.event = event;
        evdata.armId = armId;
        evdata.AttId = AttId;
        evdata.additionalText = additionalText;
        this.ReviewSetsService.PassItemCodingCeckboxChangedEvent(evdata);
    }

}
export class CheckBoxClickedEventData {
    event: any | null = null;
    AttId: number = 0;
    additionalText: string = "";
    armId: number = 0;
}




