import { Component,  ElementRef,  EventEmitter, Inject, OnInit, Output, ViewChild } from '@angular/core';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Helpers } from '../helpers/HelperMethods';
import { ConfigurableReportService } from '../services/configurablereport.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { ZoteroService } from '../services/Zotero.service';
import {Group, GroupData } from '../services/ZoteroClasses.service';

@Component({
    selector: 'ZoteroSetup',
    templateUrl: './ZoteroSetup.component.html',
    providers: []
})

export class ZoteroSetupComponent implements OnInit {
    constructor(
        private _notificationService: NotificationService,
        private _zoteroService: ZoteroService,
        private _confirmationDialogService: ConfirmationDialogService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
      private _configurablereportServ: ConfigurableReportService,
      private reviewInfoService: ReviewInfoService,
      @Inject('BASE_URL') private _baseUrl: string
    ) {

  }
  ngOnInit() {
    if (this.reviewInfoService.Contacts.length == 0) this.reviewInfoService.FetchReviewMembers();
    this.CheckForStatus();
  }

  @ViewChild('InstructionsForNewWindow') InstructionsForNewWindow!: ElementRef;
  @Output() PleaseGoTo = new EventEmitter<string>();
  private _UIphase = "Get Key";//used to decide what to show on the UI, this is the top level variable for the main "switch" in the UI.
  public get UIphase(): string {
    return this._UIphase;
  }
  public InstructionsShown: string = "";
  public SafetyCheck1 = false;
  public SafetyCheck2 = false;

  public get CurrentUserOwnsTheApiKey(): boolean {
    if (!this._zoteroService.ZoteroPermissions
      || this._zoteroService.ZoteroPermissions.zoteroConnectionId == undefined
      || this._zoteroService.ZoteroPermissions.zoteroConnectionId == null
      || this._zoteroService.ZoteroPermissions.zoteroConnectionId < 1
    ) {
      return false;
    } else if (this._zoteroService.ZoteroPermissions.erUserId == this._ReviewerIdentityServ.reviewerIdentity.userId) {
      return true;
    }
    return false;
  }
  public get NameOfAPIKeyOwner(): string {
    if (!this._zoteroService.ZoteroPermissions || !this._zoteroService.ZoteroPermissions.erUserId) return "[Unknown: unspecified error]";
    else return this.reviewInfoService.ContactNameById(this._zoteroService.ZoteroPermissions.erUserId);
  }
  public get ErrorMessage(): string {
    return this._zoteroService.ErrorMessage;
  }
  
  public get groupMeta(): Group[] {
    return this._zoteroService.groupMeta;
  }

  //main "gateway" method, checks for many things, so to decide what the UI will show.
  private async CheckForStatus() {
    if (this._zoteroService.IsBusy) await this.WaitforNotBusyService();
    if (!this._zoteroService.hasPermissions) {
      if (this._zoteroService.ErrorMessage == "data not fetched" && this._zoteroService.IsBusy == false) {
        let res = await this._zoteroService.CheckZoteroPermissions();
        let eM = this._zoteroService.ErrorMessage;
        if (res == false && eM == "No API Key") {
          this._UIphase = "BeginOauthProcess";
        } else this._zoteroService.fetchGroupMetaData();
      } else if (this._zoteroService.ErrorMessage == "No API Key" || this._zoteroService.ErrorMessage == "CheckZoteroApiKey failed") {
        this._UIphase = "BeginOauthProcess";
      } else if (this._zoteroService.ErrorMessage == "Invalid API Key") {
        this._UIphase = "ResetAndRestartOauthProcess";
      } else if (this._zoteroService.ErrorMessage == "unauthorised") {
        this._UIphase = "TryAgain";
      } else if (this._zoteroService.ErrorMessage == "nogroups") {
        this._UIphase = "NoGroups";//similar to try again! 
      } else if (this._zoteroService.ErrorMessage == "nodictvals") {
        this._UIphase = "TryAgain";
      } else if (this._zoteroService.ErrorMessage == "library_clash") {
        this._UIphase = "library_clash";
      } else if (this._zoteroService.ErrorMessage == "No write access to Group Library") {
        this._UIphase = "ViewSettings";//"ResetAndRestartOauthProcess";
        this._zoteroService.fetchGroupMetaData().then(
          () => {
            if (this._zoteroService.groupMeta.length == 0) this._UIphase = "ResetAndRestartOauthProcess";
          }
        );
      } else if (this._zoteroService.ErrorMessage == "No write access") {
        this._UIphase = "ResetAndRestartOauthProcess";//"ResetAndRestartOauthProcess";
      }
      else {
        this._UIphase = "PickGroup";
        this._zoteroService.fetchGroupMetaData().then(
          () => {
            if (this._zoteroService.groupMeta.length == 0) this._UIphase = "ResetAndRestartOauthProcess";
          }
        );
      }
    } else {
      this._UIphase = "ViewSettings";
      setTimeout(() => { this._zoteroService.fetchGroupMetaData(); }, 20);
    }
  }
  public get IsServiceBusy(): boolean {
    return this._zoteroService.IsBusy;
  }

  private async WaitforNotBusyService() {
    const maxTries = 62;//2 tries per sec 31 sec in total
    let tries: number = 0;
    let done: boolean = false;
    while (tries <= maxTries) {
      tries++;
      await setTimeout(() => { done = !this.IsServiceBusy; }, 500);//2 tries per sec!
      if (done) {
        tries = maxTries + 10;
        break;
      }
    }
  }

  public ShowInstructionsInNewWindow(step: string) {
    this.InstructionsShown = step;
    setTimeout(() => {
      if (this.InstructionsForNewWindow) this.InnerShowInstructionsInNewWindow();
      else setTimeout(() => {
        if (this.InstructionsForNewWindow) this.InnerShowInstructionsInNewWindow();
      }, 300);
    }, 200);
  }
  private InnerShowInstructionsInNewWindow() {
    let content = this.InstructionsForNewWindow.nativeElement as HTMLElement;
    if (content) {
      const res = content.children[0];
      //console.log(content, this.InstructionsForNewWindow.nativeElement, res);
      if (res && res.innerHTML && res.innerHTML != "") {
        Helpers.OpenInNewWindow(res.innerHTML, this._baseUrl);
      }
    }
  }

  public get CanAskForApiKey():boolean {
    if (!this.HasWriteRights) return false;
    else {
      if (this.SafetyCheck1 == false || this.SafetyCheck2 == false) return false;
    }
    return true;
  }
   
  public DeleteApiKey() {
    this._zoteroService.DeleteZoteroApiKey().then(() => { this._UIphase = "BeginOauthProcess"; });
  }
  public BeginOauthProcess() {
    let title = "Authorise Zotero"
    let msg = 'Please confirm whether you wish to <strong>authorise</strong> EPPI-Reviewer to exchange data with Zotero (for <strong>this review</strong> only).';
    if (this._zoteroService.ErrorMessage == "unauthorised") {
      title = "Authorise Zotero: please retry"
      msg = 'You need to try again: the authorisation with Zotero sometimes fails at the "verification" step. <br />Most of the times, trying again solves the problem.'
    }
    this._confirmationDialogService.confirm(title, msg, false, '')
      .then(
        async (confirmed: any) => {
          if (confirmed) {
            this._zoteroService.OauthProcessGet(this._ReviewerIdentityServ.reviewerIdentity.userId, this._ReviewerIdentityServ.reviewerIdentity.reviewId)
              .then(
                (token: any) => {
                  window.location.href = 'https://www.zotero.org/oauth/authorize?oauth_token=' + token.toString();
                });
          } else {
            this._notificationService.show({
              content: " You have cancelled Zotero authorisation",
              animation: { type: 'slide', duration: 400 },
              position: { horizontal: 'center', vertical: 'top' },
              type: { style: "info", icon: true },
              closable: true
            });
          }
        }).catch(() => { });
  }
  
    

  public async changeGroupBeingSynced(group: Group) {
    const res = await this._zoteroService.UpdateGroupToReview(group.id.toString(), false);
    if (this._zoteroService.hasPermissions == true && res == true) {
      //console.log(1);
      for (var i = 0; i < this.groupMeta.length; i++) {
        if (this.groupMeta[i].id === group.id) {
          this.groupMeta[i].groupBeingSynced = true;
        } else {
          this.groupMeta[i].groupBeingSynced = false;
        }
      }
    } else if (this._zoteroService.hasPermissions == false) {
      //console.log(2);
      //things _might_ be good now, so we'll make very sure and get our API to check again.
      this._zoteroService.SetError("data not fetched");//makes the "checkForStatus()" method try again from scratch...
      this.CheckForStatus().then(() => {
        //console.log(3);
        if (this._zoteroService.hasPermissions == true) this.PleaseGoTo.emit("ZoteroSync");
      });
    }
    //this._zoteroService.editApiKeyPermissions = true;
  }

    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }

    public get HasAdminRights(): boolean {
        return this._ReviewerIdentityServ.HasAdminRights;
    }
}
