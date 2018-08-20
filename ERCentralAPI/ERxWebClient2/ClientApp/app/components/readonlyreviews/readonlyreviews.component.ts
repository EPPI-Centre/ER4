import { Component, Inject, OnInit, Output, EventEmitter } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { readonlyreviewsService } from '../services/readonlyreviews.service';


@Component({
    selector: 'readonlyreviews',
    templateUrl: './readonlyreviews.component.html',
    providers: []
})
export class FetchReadOnlyReviewsComponent implements OnInit {

    constructor(private router: Router,
                private _httpC: HttpClient,
                @Inject('BASE_URL') private _baseUrl: string,
                private ReviewerIdentityServ: ReviewerIdentityService,
                private _readonlyreviewsService: readonlyreviewsService) {

                console.log('rOr constructor: ' + this.ReviewerIdentityServ.reviewerIdentity.userId);
    }
    @Output() OpeningNewReview = new EventEmitter();

    onSubmit(f: string) {
            let RevId: number = parseInt(f, 10);
            this.ReviewerIdentityServ.LoginToReview(RevId).subscribe(ri => {
            this.ReviewerIdentityServ.reviewerIdentity = ri;
            console.log('login to Review: ' + this.ReviewerIdentityServ.reviewerIdentity.userId);
            if (this.ReviewerIdentityServ.reviewerIdentity.userId > 0 && this.ReviewerIdentityServ.reviewerIdentity.reviewId === RevId) {
                this.ReviewerIdentityServ.Save();
                this.router.onSameUrlNavigation = "reload";
                this.OpeningNewReview.emit();
                this.router.navigate(['main']);
            }
        })
        //this.ReviewerIdentityServ.reviewerIdentity.reviewId = +f;
        //this.router.navigate(['fetch-reviewsets'])
    }

    //getReviews() {

    //    let body2 = "contactId=" + this.ReviewerIdentityServ.reviewerIdentity.userId;
    //    console.log('Calling get reviews...!');
    //     this._httpC.post<ReadOnlyReview[]>(this._baseUrl + 'api/review/reviewsbycontact',
    //         body2).subscribe(result => this.ReviewList = result);
    //}

    //ngOnInit() {

    //    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
    //        console.log("didn't work!");
    //        this.router.navigate(['home']);
    //    }
    //    else {

    //        this.ReviewerIdentityServ.Report();
    //        this.getReviews();//we don't want this sort of thing in the constructor, API calls should be done after...
    //    }

      
    //}

    getReviews() {
        console.log('inside get reviews');
        if (this._readonlyreviewsService.ReadOnlyReviews.length == 0) {
            this._readonlyreviewsService.Fetch().subscribe(result => {
                this._readonlyreviewsService.ReadOnlyReviews = result;
                console.log("got " + this._readonlyreviewsService.ReadOnlyReviews.length + " ReadOnlyReviews.");
            });
        }
    }

    ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            console.log('user is empty...');
            this.router.navigate(['home']);
        }
        else {

            console.log("getting ReadOnlyReviews");
            this.ReviewerIdentityServ.Report();
            this.getReviews();
        }
    }


}
