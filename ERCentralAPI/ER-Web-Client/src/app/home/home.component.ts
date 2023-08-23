import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService, ReviewerIdentity } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfigService } from '../services/config.service';
import { faSpinner } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'home',
  templateUrl: './home.component.html'
  , providers: []
  , styles: [
    `@keyframes glowingText {
      0% { background-color:#FFFADD; margin-left:1rem; }
    50% { background-color:#BAEEBA; margin-left:1rem;}
    100% { background-color:#FFFADD; margin-left:1rem; } 
        }`
  ]

  //providers: [ReviewerIdentityService]
})
export class HomeComponent implements OnInit {
  constructor(private router: Router,
    private ReviewerIdentityServ: ReviewerIdentityService,
    //@Inject('BASE_URL') private _baseUrl: string,
    configService: ConfigService,
    protected EventEmitterService: EventEmitterService,
    private _httpC: HttpClient,
  ) {
    this.ReviewerIdentityServ.LoginFailed.subscribe(() => this.LoginFailed());
    this._baseUrl = configService.baseUrl;
  }
  vInfo: versionInfo = new versionInfo();
  private _baseUrl = "";
  public ShowLoginFailed: boolean = false;
  public ShowUsernameRequired: boolean = false;
  public ShowPasswordRequired: boolean = false;
  public showCochraneHelp: boolean = false;
  public HasConnectionError: boolean = false;
  public IsBusy: boolean = false;
  public IsGettingVersionInfo: boolean = false;
  public faSpinner = faSpinner;
  public versionIsNew: boolean = false;
  public get baseUrl(): string {
    return this._baseUrl;
  }
 
  onLogin(u: string, p: string) {
    //this.ReviewerIdentityServ.Login(u, p);
    //localStorage.clear();
    this.ReviewerIdentityServ.LogOut();
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
    this.IsBusy = true;
    this.ReviewerIdentityServ.LoginReq(u, p);
  }
  GoToArchie() {
    this.ReviewerIdentityServ.GoToArchie();
  }
  FilckShowCochraneHelp() {
    this.showCochraneHelp = !this.showCochraneHelp;
  }
  LoginFailed() {
    this.IsBusy = false;
    this.ShowLoginFailed = true;
  }
  ngOnInit() {
    //localStorage.clear();
    this.EventEmitterService.PleaseClearYourDataAndState.emit();
    this.ReviewerIdentityServ.LogOut();
    //this.ReviewerIdentityServ.reviewerIdentity = new ReviewerIdentity();
    this.getVinfo();
  }
  getVinfo() {
    this.HasConnectionError = false;
    this.IsGettingVersionInfo = true;
    this._httpC.get<versionInfo>(this._baseUrl + 'api/Login/VersionInfo').subscribe(
      result => {
        this.IsGettingVersionInfo = false;
        this.vInfo = result;
        if (this.vInfo.versionN.startsWith("4.")) {
          this.vInfo.versionN = "6" + this.vInfo.versionN.substring(1);
        }
        this.CheckIfVersionIsNew();
      }, error => {
        this.HasConnectionError = true;
        this.IsGettingVersionInfo = false;
        console.error(error);
      }
    );
  }
  FormatDate(DateSt: string): string {
    return Helpers.FormatDate(DateSt);
  }
  private CheckIfVersionIsNew() {
    const date = new Date();
    date.setDate(date.getDate() - 10);
    const vDT = this.vInfo.date.split(' ');
    if (!vDT || vDT.length != 2) {
      this.versionIsNew = false;
      return;
    }
    const dmy = vDT[0].split('/');
    if (!dmy || dmy.length != 3) {
      this.versionIsNew = false;
      return;
    }
    const vD = new Date(parseInt(dmy[2]), parseInt(dmy[1]) - 1, parseInt(dmy[0]));
    if (vD > date) this.versionIsNew = true;
    else this.versionIsNew = false;
  }
}
class versionInfo {
  date: string = "";
  description: string = "";
  url: string = "";
  versionN: string = "";
}
