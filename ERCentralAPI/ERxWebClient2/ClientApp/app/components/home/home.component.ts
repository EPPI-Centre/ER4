import { Component, Inject } from '@angular/core';
import { Http, Response, Headers, RequestOptions, RequestMethod } from '@angular/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../app/revieweridentity.service';

@Component({
    selector: 'home',
    templateUrl: './home.component.html'
     ,providers: []

    //providers: [ReviewerIdentityService]
})
export class HomeComponent {
    private _http: Http;
    private _baseUrl: string;
    private headers: Headers = new Headers({ "Content-Type": 'application/x-www-form-urlencoded' });
    constructor(private router: Router, http: Http, private ReviewerIdentity: ReviewerIdentityService,
        @Inject('BASE_URL') baseUrl: string) {
        this._http = http;
        this._baseUrl = baseUrl;
    }//, @Inject(ReviewerIdentityService) private ReviewerIdentity: ReviewerIdentityService) { }
    onSubmit(f: string) {
        this.ReviewerIdentity.userId = +f;
        this.router.navigate(['readonlyreviews'])
        console.log(f);  
        //console.log(this.ReviewerIdentity.ContactId); 
        //this.ReviewerIdentity.Report();
    }
    onLogin(u: string, p:string) {
        let body = "Username=" + u + "&Password="+ p;
        //let body = JSON.stringify({ 'contactId': 1 });
        let requestoptions = new RequestOptions({
            method: RequestMethod.Post,
            url: this._baseUrl + 'api/Login/Login',
            headers: this.headers,
            body: body
        });

        this._http.request(this._baseUrl + 'api/Login/Login', requestoptions).subscribe(result => {
            let tmpReviewerIdentity: ReviewerIdentityService = result.json() as ReviewerIdentityService;
            if (tmpReviewerIdentity.userId > 0) {
                this.ReviewerIdentity.userId = tmpReviewerIdentity.userId;
                this.ReviewerIdentity.name = tmpReviewerIdentity.name;
                this.ReviewerIdentity.accountExpiration = tmpReviewerIdentity.accountExpiration;
                this.ReviewerIdentity.reviewExpiration = tmpReviewerIdentity.reviewExpiration;
                this.ReviewerIdentity.isSiteAdmin = tmpReviewerIdentity.isSiteAdmin;
                this.ReviewerIdentity.loginMode = tmpReviewerIdentity.loginMode;
                this.ReviewerIdentity.isAuthenticated = tmpReviewerIdentity.isAuthenticated;
                this.ReviewerIdentity.roles = tmpReviewerIdentity.roles;
                this.ReviewerIdentity.token = tmpReviewerIdentity.token;
            }
            console.log('home login: ' + this.ReviewerIdentity.userId);
            if (this.ReviewerIdentity.userId > 0) this.router.navigate(['readonlyreviews']);

        }, error => console.error(error));
    }
}
