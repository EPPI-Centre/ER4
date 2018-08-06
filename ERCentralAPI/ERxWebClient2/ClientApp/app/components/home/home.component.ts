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
    constructor(private router: Router, http: Http, private ReviewerIdentityServ: ReviewerIdentityService,
        @Inject('BASE_URL') baseUrl: string) {
        this._http = http;
        this._baseUrl = baseUrl;
    }//, @Inject(ReviewerIdentityService) private ReviewerIdentity: ReviewerIdentityService) { }
    
    onLogin(u: string, p:string) {
        //this.ReviewerIdentityServ.Login(u, p);

        this.ReviewerIdentityServ.LoginReq2(u, p).subscribe(ri => {
            this.ReviewerIdentityServ.reviewerIdentity = ri;
            console.log('home login: ' + this.ReviewerIdentityServ.reviewerIdentity.userId);
            if (this.ReviewerIdentityServ.reviewerIdentity.userId > 0) this.router.navigate(['readonlyreviews']);
        })
        
    };
    
}
