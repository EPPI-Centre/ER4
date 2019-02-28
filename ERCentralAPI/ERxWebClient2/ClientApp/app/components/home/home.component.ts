import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService, ReviewerIdentity } from '../services/revieweridentity.service';

@Component({
    selector: 'home',
    templateUrl: './home.component.html'
     ,providers: []

    //providers: [ReviewerIdentityService]
})
export class HomeComponent implements OnInit {
    constructor(private router: Router,
        private ReviewerIdentityServ: ReviewerIdentityService,
        @Inject('BASE_URL') private _baseUrl: string,
        private _httpC: HttpClient,
    ) {
        this.ReviewerIdentityServ.LoginFailed.subscribe(() => this.LoginFailed());
    }
    vInfo: versionInfo = new versionInfo();
    public ShowLoginFailed: boolean = false;
    public ShowUsernameRequired: boolean = false;
    public ShowPasswordRequired: boolean = false;
    onLogin(u: string, p:string) {
        //this.ReviewerIdentityServ.Login(u, p);
        localStorage.clear();
        this.ShowLoginFailed = false;
        this.ShowUsernameRequired = false;
        this.ShowPasswordRequired = false;
        if (u.length < 2) {
            this.ShowUsernameRequired = true;
            return;
        }
        if (p.length < 6) {
            this.ShowPasswordRequired = true;
            return;
        }
        this.ReviewerIdentityServ.LoginReq(u, p);
    };
    
    LoginFailed() {
        this.ShowLoginFailed = true;
    }
    ngOnInit() {
        localStorage.clear();
        this.ReviewerIdentityServ.reviewerIdentity = new ReviewerIdentity();
        this.getVinfo();
    }
    getVinfo() {
        this._httpC.get<versionInfo>(this._baseUrl + 'api/Login/VersionInfo').subscribe(
            result => {
                this.vInfo = result;
            }, error => {
                console.error(error);
            }
        );
    }
    FormatDate(DateSt: string): string {
        if (DateSt.length < 10) return "";
        else {
            const year = parseInt(DateSt.substr(6, 4));
            const month = parseInt(DateSt.substr(3, 2));
            const day = parseInt(DateSt.substr(0, 2));
            const date: Date = new Date(year, month, day);
            return date.toLocaleDateString();
        }
    }
}
class versionInfo {
    date: string = "";
    description: string = "";
    url: string = "";
    versionN: string = "";
}
