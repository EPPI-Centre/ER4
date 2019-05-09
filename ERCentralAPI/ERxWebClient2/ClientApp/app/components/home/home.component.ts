import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService, ReviewerIdentity } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';

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
    }
    GoToArchie() {
        //
        let url = "";
        let redirectUri = this._baseUrl + "ArchieCallBack";
        redirectUri = "https://ssru38.ioe.ac.uk/WcfHostPortal/ArchieCallBack.aspx";//temporary!!!!!!!
        if (this._baseUrl.indexOf("://eppi.ioe.ac.uk") != -1) {
            //this is the production environment, go there
            url = "https://vno-account.cochrane.org/auth/realms/cochrane/protocol/openid-connect/auth?client_id=eppi&response_type=code&redirect_uri=";
        }
        else {
            //go to test env
            url = "https://test-login.cochrane.org/auth/realms/cochrane/protocol/openid-connect/auth?client_id=eppi&response_type=code&redirect_uri=";
        }
        url += redirectUri + "&scope=document person&state=";
        var state = '';
        const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        const charactersLength = characters.length;
        for (var i = 0; i < 12; i++) {//generate a random string of 12 chars...
            state += characters.charAt(Math.floor(Math.random() * charactersLength));
        }
        url += state + "&access_type=offline";
        url = encodeURI(url);
        console.log("Trying this URL:", url);
        window.location.href = url;
    }
    
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
        return Helpers.FormatDate(DateSt);
    }
}
class versionInfo {
    date: string = "";
    description: string = "";
    url: string = "";
    versionN: string = "";
}
