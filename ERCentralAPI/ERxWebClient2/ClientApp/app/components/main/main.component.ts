import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

@Component({
    selector: 'main',
    templateUrl: './main.component.html'
     ,providers: []

    //providers: [ReviewerIdentityService]
})
export class MainComponent implements OnInit {
    constructor(private router: Router,
        private ReviewerIdentityServ: ReviewerIdentityService,
        @Inject('BASE_URL') private _baseUrl: string,
        private _httpC: HttpClient,
    ) { }
    vInfo: versionInfo = new versionInfo();
    onLogin(u: string, p:string) {
        //this.ReviewerIdentityServ.Login(u, p);

        this.ReviewerIdentityServ.LoginReq(u, p).subscribe(ri => {
            this.ReviewerIdentityServ.reviewerIdentity = ri;
            console.log('home login: ' + this.ReviewerIdentityServ.reviewerIdentity.userId);
            if (this.ReviewerIdentityServ.reviewerIdentity.userId > 0) {
                this.ReviewerIdentityServ.Save();
                this.router.navigate(['readonlyreviews']);
            }
        })
        
    };
    ngOnInit() {
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
