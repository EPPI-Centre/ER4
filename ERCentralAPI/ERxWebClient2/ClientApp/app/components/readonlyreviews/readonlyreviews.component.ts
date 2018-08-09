import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';



@Component({
    selector: 'readonlyreviews',
    templateUrl: './readonlyreviews.component.html',
    //providers: [ReviewerIdentityService]
    providers: []
})
export class FetchReadOnlyReviewsComponent implements OnInit {
    public ReviewList: ReadOnlyReview[] = [];
    constructor(private router: Router, private _httpC: HttpClient, @Inject('BASE_URL') private _baseUrl: string, private ReviewerIdentityServ: ReviewerIdentityService) {
        console.log('rOr constructor: ' + this.ReviewerIdentityServ.reviewerIdentity.userId);
    }
    onSubmit(f: string) {
        let RevId: number = parseInt(f, 10);
        this.ReviewerIdentityServ.LoginToReview(RevId).subscribe(ri => {
            this.ReviewerIdentityServ.reviewerIdentity = ri;
            console.log('login to Review: ' + this.ReviewerIdentityServ.reviewerIdentity.userId);
            if (this.ReviewerIdentityServ.reviewerIdentity.userId > 0 && this.ReviewerIdentityServ.reviewerIdentity.reviewId === RevId) {
                this.ReviewerIdentityServ.Save();
                this.router.navigate(['fetch-reviewsets']);
            }
        })
        //this.ReviewerIdentityServ.reviewerIdentity.reviewId = +f;
        //this.router.navigate(['fetch-reviewsets'])
    }

    getReviews() {

        let body2 = "contactId=" + this.ReviewerIdentityServ.reviewerIdentity.userId;
        console.log('Calling get reviews...!');
         this._httpC.post<ReadOnlyReview[]>(this._baseUrl + 'api/review/reviewsbycontact',
             body2).subscribe(result => this.ReviewList = result);
    }
    ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            console.log("didn't work!");
            this.router.navigate(['home']);
        }
        else {

            //const userJson = localStorage.getItem('currentErUser');

            //console.log('On intiation this should have th user from the localstorage ' + userJson);

            //let currentUser: any = userJson !== null ? JSON.parse(userJson) : '';

            //console.log('rOr init: ' + currentUser.userId);
            this.ReviewerIdentityServ.Report();
            this.getReviews();//we don't want this sort of thing in the constructor, API calls should be done after...
        }

      
    }
}


interface ReadOnlyReview {
    reviewId: number;
    reviewName: string;
    contactReviewRoles: string;
    reviewOwner: string;
    lastAccess: string;
}

