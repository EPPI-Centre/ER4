import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

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
}
class versionInfo {
    date: string = "";
    description: string = "";
    uRL: string = "";
    versionN: string = "";
}
