import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService, ReviewerIdentity } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfigService, versionInfo } from '../services/config.service';
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
    private configService: ConfigService,
    protected EventEmitterService: EventEmitterService,
    
  ) {
    this.ReviewerIdentityServ.LoginFailed.subscribe(() => this.LoginFailed());
    this._baseUrl = configService.baseUrl;
  }
  public get vInfo(): versionInfo {
    return this.configService.vInfo;
  }
  public set vInfo(val: versionInfo) {
    this.configService.vInfo = val;
  }
  public get versionIsNew(): boolean {
    return this.configService.versionIsNew;
  }

  private _baseUrl = "";
  public ShowLoginFailed: boolean = false;
  public ShowUsernameRequired: boolean = false;
  public ShowPasswordRequired: boolean = false;
  public showCochraneHelp: boolean = false;
  public IsBusy: boolean = false;
  public faSpinner = faSpinner;
  public get baseUrl(): string {
    return this._baseUrl;
  }
  public get HasConnectionError(): boolean{
    return this.configService.HasConnectionError;
  }
  public get IsGettingVersionInfo(): boolean {
    return this.configService.IsGettingVersionInfo;
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
    this.configService.getVinfo();
  }
  FormatDate(DateSt: string): string {
    return Helpers.FormatDate(DateSt);
  }
  
}

