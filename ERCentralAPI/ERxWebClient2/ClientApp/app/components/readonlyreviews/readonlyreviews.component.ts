import { Component, Inject, OnInit } from '@angular/core';
import { Http, Response, Headers, RequestOptions, RequestMethod } from '@angular/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';
import { ReviewerIdentityService } from '../app/revieweridentity.service';
import 'rxjs/add/operator/toPromise';



@Component({
    selector: 'readonlyreviews',
    templateUrl: './readonlyreviews.component.html',
    //providers: [ReviewerIdentityService]
    providers: []
})
export class FetchReadOnlyReviewsComponent implements OnInit {
    public ReviewList: ReadOnlyReview[] = [];
    private _http: Http;
    private _baseUrl: string;
    private headers: Headers = new Headers({ "Content-Type": 'application/x-www-form-urlencoded'  });
    constructor(private router: Router,http: Http, @Inject('BASE_URL') baseUrl: string, private ReviewerIdentity: ReviewerIdentityService) {//,@Inject(ReviewerIdentityService) private ReviewerIdentity: ReviewerIdentityService) {
        this._http = http;
        this._baseUrl = baseUrl;
        console.log('rOr constructor: ' + this.ReviewerIdentity.userId);
        this.ReviewerIdentity.Report();
        this.getReviews();
        
    }
    onSubmit(f: string) {
        this.ReviewerIdentity.reviewId = +f;
        this.router.navigate(['fetch-reviewsets'])
    }

    getReviews() {
        //console.log('rOr getReviews: ' + this.ReviewerIdentity.ContactId);
        //this.ReviewerIdentity.Report();
        let body = "contactId="+ this.ReviewerIdentity.userId;
        //let body = JSON.stringify({ 'contactId': 1 });
        let requestoptions = new RequestOptions({
            method: RequestMethod.Post,
            url: this._baseUrl + 'api/review/reviewsbycontact',
            headers: this.headers,
            body: body
        });

        this._http.request(this._baseUrl + 'api/review/reviewsbycontact', requestoptions).subscribe(result => {
            this.ReviewList = result.json() as ReadOnlyReview[];
            console.log(this.ReviewList.length);
        }, error => console.error(error));


        
        //var body = { contactId: " + this.ReviewerIdentity.ContactId + };//{ contactId: this.ReviewerIdentity.ContactId };//JSON.stringify({name: name}), {headers: this.headers}
        //this._http.post(this._baseUrl + 'api/review/reviewsbycontact', body, { headers: this.headers }).subscribe(result => {
        //this._http.post(this._baseUrl + 'api/review/reviewsbycontact', body, { headers: this.headers }).subscribe(result => {
        //    this.ReviewList = result.json() as ReadOnlyReview[];
        //}, error => console.error(error));
    }
    ngOnInit() {
        //console.log('rOr init: ' + this.ReviewerIdentity.ContactId);
        //this.ReviewerIdentity.Report();
        //if (this.ReviewerIdentity.ContactId != 0 && !this.ReviewList) {
        //    //this.getReviews();
        //} else {
        //    this.ReviewerIdentity.ContactId = 1214;
        //    //this.getReviews();
        //}
        //alert(something);
        //this._http.post(this._baseUrl + 'api/review/reviewsbycontact', something.).subscribe(result => {
        //        this.ReviewList = result.json() as ReadOnlyReview[];
        //}, error => console.error(error));
    }
}


interface ReadOnlyReview {
    reviewId: number;
    reviewName: string;
    contactReviewRoles: string;
    reviewOwner: string;
    lastAccess: string;
}

