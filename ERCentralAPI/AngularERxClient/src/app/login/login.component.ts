import { Component, OnInit, Inject } from '@angular/core';
//import { Response, Headers, RequestOptions, RequestMethod } from '@angular/http';
import { HttpClient, HttpParams } from '@angular/common/http'; 
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../Services/revieweridentity.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
  , providers: []
})
export class LoginComponent implements OnInit {

  private _http: HttpClient;
  private _baseUrl: string;
  private headers: Headers = new Headers({ "Content-Type": 'application/x-www-form-urlencoded' });
  constructor(private router: Router, http: HttpClient
    , private ReviewerIdentity: ReviewerIdentityService
    , @Inject('BASE_URL') baseUrl: string) {
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
  ngOnInit() {
  }
  onLogin(u: string, p: string) {
    let body = "Username=" + u + "&Password=" + p;
    //let body = JSON.stringify({ 'contactId': 1 });

    let httpParams = new HttpParams()
      .append("Username", u)
      .append("Password", "p");

    let options = { params: httpParams };
    

    this._http.post<ReviewerIdentityService>(this._baseUrl + 'api/Login/Login', options).subscribe(result => {
      
      if (result.userId > 0) {
        this.ReviewerIdentity.userId = result.userId;
        this.ReviewerIdentity.name = result.name;
        this.ReviewerIdentity.accountExpiration = result.accountExpiration;
        this.ReviewerIdentity.reviewExpiration = result.reviewExpiration;
        this.ReviewerIdentity.isSiteAdmin = result.isSiteAdmin;
        this.ReviewerIdentity.loginMode = result.loginMode;
        this.ReviewerIdentity.isAuthenticated = result.isAuthenticated;
        this.ReviewerIdentity.roles = result.roles;
        this.ReviewerIdentity.token = result.token;
      }
      console.log('home login: ' + this.ReviewerIdentity.userId);
      if (this.ReviewerIdentity.userId > 0) this.router.navigate(['readonlyreviews']);

    }, error => console.error(error));
  }

}
