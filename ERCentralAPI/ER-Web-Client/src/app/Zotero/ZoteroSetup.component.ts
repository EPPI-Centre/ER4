import { Component,  ElementRef,  EventEmitter,  Inject,  Input,  OnInit, Output, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Helpers } from '../helpers/HelperMethods';
import { CodesetStatisticsService, StatsCompletion } from '../services/codesetstatistics.service';
import { ConfigurableReportService } from '../services/configurablereport.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { Item } from '../services/ItemList.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { singleNode } from '../services/ReviewSets.service';
import { ZoteroService } from '../services/Zotero.service';
import {
    Collection, Group, GroupData, IERWebANDZoteroReviewItem,
    IObjectsInERWebNotInZotero, ZoteroReviewCollectionList
} from '../services/ZoteroClasses.service';

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

  public get currentReview(): number {
    return this._ReviewerIdentityServ.reviewerIdentity.reviewId;
  }
  private _UIphase = "Get Key";//used to decide what to show on the UI, this is the top level variable for the main "switch" in the UI.
  public get UIphase(): string {
    return this._UIphase;
  }
  public InstructionsShown: string = "";
  @ViewChild('InstructionsForNewWindow') InstructionsForNewWindow!: ElementRef;
  public SafetyCheck1 = false;
  public SafetyCheck2 = false;

  public get CurrentUserOwnsTheApiKey(): boolean {
    if (!this._zoteroService.ZoteroPermissions
      || this._zoteroService.ZoteroPermissions.zoteroConnectionId == undefined
      || this._zoteroService.ZoteroPermissions.zoteroConnectionId == null
      || this._zoteroService.ZoteroPermissions.zoteroConnectionId < 1
    ) {
      return false;
    } else {
      if (this._zoteroService.ZoteroPermissions.erUserId == this._ReviewerIdentityServ.reviewerIdentity.userId) return true;
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
  @Output() PleaseGoTo = new EventEmitter<string>();


    public zoteroUserID: number = 0;
    public totalItemsInCurrentReview: number = 0;
    public zoteroUserName: string = '';
    public zoteroUserKey: string = '';

    public CurrentDropdownSelectedCode: singleNode | null = null;
    public ObjectsInERWebNotInZotero: IObjectsInERWebNotInZotero[] = [];
    public ItemsInERWebANDInZotero: Collection[] = [];
    public codingSetData: StatsCompletion[] = [];
    public itemsWithThisCode: Item[] = [];
    public isCollapsed = false;
    public zoteroLink: string = '';
    public reviewLinkText: string[] = [];
    public currentLinkedReviewId: string = '';
    public zoteroCollectionList: ZoteroReviewCollectionList = new ZoteroReviewCollectionList();
    public ZoteroObjectsThatAreNotAttachmentsLength: number = 0;
    public ObjectsInERWebAndZotero: IERWebANDZoteroReviewItem[] = [];
    public DetailsForSetId: number = 0;
  
  public get groupMeta(): Group[] {
    return this._zoteroService.groupMeta;
  }


    ngOnDestroy() {
    }

    ngOnInit() {
        this.zoteroUserName = this._ReviewerIdentityServ.reviewerIdentity.name;
        this.zoteroUserID = this._ReviewerIdentityServ.reviewerIdentity.userId;
      this.currentLinkedReviewId = this._ReviewerIdentityServ.reviewerIdentity.reviewId.toString();
      if (this.reviewInfoService.Contacts.length == 0) this.reviewInfoService.FetchReviewMembers();
      this.CheckForStatus();
    }

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
      } else if (this._zoteroService.ErrorMessage == "No write access to Group Library") {
        this._UIphase = "ResetAndRestartOauthProcess";
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
      setTimeout(() => { this._zoteroService.fetchGroupMetaData(); }, 50);
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
    

    Clear() {
      //this.apiKeys = [];
      //this.groupMeta = [];
  }
  public ResetAndRestart() {
    this._zoteroService.DeleteZoteroApiKey().then(() => {
      this._UIphase = "BeginOauthProcess";
      //this.BeginOauthProcess();
    });
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
                  this.zoteroLink = 'https://www.zotero.org/oauth/authorize?oauth_token=' + token.toString();
                  window.location.href = this.zoteroLink;
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
  
    public async RemoveCurrentReviewAT(group: Group) {

        await this._zoteroService.UpdateGroupToReview( group.id.toString(), true).then(
                async () => {
                    let indexG = this.groupMeta.indexOf(group);
                    this.groupMeta[indexG].data.groupBeingSynced = false;
                    await this.FetchLinkedReviewID();
                    let index = this.zoteroCollectionList.ZoteroReviewCollectionList.findIndex(x => x.libraryID === group.id.toString());
                    if (index > -1) {
                        this.zoteroCollectionList.ZoteroReviewCollectionList.splice(index, 1);
                    }
                    if (this.zoteroCollectionList.ZoteroReviewCollectionList.length === 0) {
                        this._notificationService.show({
                            content: "You have no syncable groups remaining please reauthorise with Zotero",
                            animation: { type: 'slide', duration: 400 },
                            position: { horizontal: 'center', vertical: 'top' },
                            type: { style: "info", icon: true },
                            closable: true
                        });                      
                    }
                });
        this._notificationService.show({
            content: " You have removed the Zotero authentication for group: " + group.id.toString(),
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
        });
    }

    public HasLinkedReviewID(group: GroupData): boolean {
        let index = this.zoteroCollectionList.ZoteroReviewCollectionList.findIndex(x => x.libraryID === group.id.toString());
        return index > -1;
    }

   // Logic in here should change depending on what we want to happen
    public async changeGroupBeingSynced(group: Group) {
      //await this.FetchLinkedReviewID();
     

        await this.AddLinkedReviewID(group.data);

        //await this.UpdateGroupMetaData(group.id, this._ReviewerIdentityServ.reviewerIdentity.userId,
        //  this.currentReview);
      if (this._zoteroService.hasPermissions == true) {
        //console.log(1);
        for (var i = 0; i < this.groupMeta.length; i++) {
          if (this.groupMeta[i].id === group.id) {
            this.groupMeta[i].groupBeingSynced = true;
            this._zoteroService.currentGroupBeingSynced = this.groupMeta[i].id;

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

    public async AddLinkedReviewID(group: GroupData) {
      await this._zoteroService.UpdateGroupToReview(group.id.toString(), false);
    }

    public async FetchLinkedReviewID(): Promise<void> {
        await this._zoteroService.FetchGroupToReviewLinks().then(
                async (zoteroReviewCollectionList: ZoteroReviewCollectionList) => {
                    this.zoteroCollectionList = zoteroReviewCollectionList;
                    if (zoteroReviewCollectionList.ZoteroReviewCollectionList.length > 0) {
                        this.currentLinkedReviewId = zoteroReviewCollectionList.ZoteroReviewCollectionList[0].revieW_ID.toString();
                    }
                });
    }

    public CanGetCurrentStatus(): boolean {
        let index = this.zoteroCollectionList.ZoteroReviewCollectionList.findIndex(x => x.libraryID === this._zoteroService.currentGroupBeingSynced.toString());
        if (index === -1) {
            return false;
        }
        return true;
    }
   
    async UpdateGroupMetaData(groupId: number, userId: number, currentReview: number) {
      await this._zoteroService.MarkGroupForSync(groupId);
    }

    public get IsReportsServiceBusy(): boolean {
        return this._configurablereportServ.IsBusy;
    }
  
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }

    public get HasAdminRights(): boolean {
        return this._ReviewerIdentityServ.HasAdminRights;
    }
}
