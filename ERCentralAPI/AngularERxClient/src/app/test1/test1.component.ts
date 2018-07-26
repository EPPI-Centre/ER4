import { Component, OnInit, Inject } from '@angular/core';
//import { Response, Headers, RequestOptions, RequestMethod } from '@angular/http';
import { HttpClient } from '@angular/common/http'; 
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../Services/revieweridentity.service';

@Component({
  selector: 'app-test1',
  templateUrl: './test1.component.html',
  styleUrls: ['./test1.component.css']
  , providers: []
})
export class Test1Component implements OnInit {

  private _http: HttpClient ;
  private _baseUrl: string;
  private headers: Headers = new Headers({ "Content-Type": 'application/x-www-form-urlencoded' });
  constructor(
      router: Router
      ,http: HttpClient 
    , private ReviewerIdentity: ReviewerIdentityService
    , @Inject('BASE_URL') baseUrl: string
  ) {
    this._http = http;
    this._baseUrl = baseUrl;
  }

  ngOnInit() {
  }

}
