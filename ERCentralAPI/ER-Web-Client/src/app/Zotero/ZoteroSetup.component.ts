import { Component,  Input,  OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationService } from '@progress/kendo-angular-notification';
import { CodesetStatisticsService, StatsCompletion } from '../services/codesetstatistics.service';
import { ConfigurableReportService } from '../services/configurablereport.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { Item } from '../services/ItemList.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { singleNode } from '../services/ReviewSets.service';
import { ZoteroService } from '../services/Zotero.service';
import {
    Collection, Group, GroupData, Groups, IERWebANDZoteroReviewItem,
    IObjectsInERWebNotInZotero, PerGroupPermissions, UserKey, ZoteroReviewCollectionList
} from '../services/ZoteroClasses.service';

@Component({
    selector: 'ZoteroSetup',
    templateUrl: './ZoteroSetup.component.html',
    providers: []
})

export class ZoteroSetupComponent implements OnInit {
    constructor(
        private route: ActivatedRoute,
        private _notificationService: NotificationService,
        private _zoteroService: ZoteroService,
        private _router: Router,
        private _ActivatedRoute: ActivatedRoute,
        private _confirmationDialogService: ConfirmationDialogService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _codesetStatsServ: CodesetStatisticsService,
        private _configurablereportServ: ConfigurableReportService,
    ) {

    }

    @Input() ZoteroApiKeyResult: boolean = true;
    public currentReview: number = 0;
    public groupMeta: Group[] = [];
    public zoteroUserID: number = 0;
    public totalItemsInCurrentReview: number = 0;
    public zoteroUserName: string = '';
    public zoteroUserKey: string = '';
    private hasPermissions: boolean = false;

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
    public apiKeys: UserKey[] = [];

    ngOnDestroy() {
    }

    ngOnInit() {
        this._zoteroService.editApiKeyPermissions = false;
        this.zoteroUserName = this._ReviewerIdentityServ.reviewerIdentity.name;
        this.zoteroUserID = this._ReviewerIdentityServ.reviewerIdentity.userId;
        this.currentLinkedReviewId = this._ReviewerIdentityServ.reviewerIdentity.reviewId.toString();
        this.currentReview = this._ReviewerIdentityServ.reviewerIdentity.reviewId;
        var groupId = 0;
        this._zoteroService.CheckZoteroApiKey().then(
                (zoteroApiKeyResult) => {
                    if (zoteroApiKeyResult === true) {
                        
                       this._zoteroService.fetchApiKeys().then(
                               async (userKeys) => {

                                   this.apiKeys = userKeys;
                                });

                        // ALREADY HAS A ZOTERO API KEY
                        this.FetchLinkedReviewID();
                        //this.zoteroEditKeyLink = 'https://www.zotero.org/oauth/authorize?oauth_token=' + zoteroApiKey;
                        this._zoteroService.fetchUserZoteroPermissions().then(
                                async () => {

                                    await this._zoteroService.fetchGroupMetaData().then(
                                        async (meta) => {
                                            this.groupMeta = meta;
                                        });

                                    if (this.groupMeta.length === 0) {
                                        this._notificationService.show({
                                            content: " You have no available groups; please create an accessible/shared group within Zotero",
                                            animation: { type: 'slide', duration: 400 },
                                            position: { horizontal: 'center', vertical: 'top' },
                                            type: { style: "info", icon: true },
                                            closable: true
                                        });
                                        await this._zoteroService.DeleteZoteroApiKey(-1);

                                        return;
                                    }

                                    var perGroup: PerGroupPermissions;
                                    var groupsAll: Groups = this._zoteroService.ZoteroPermissions.access.groups as Groups;
                                    if (groupsAll.all === undefined) {
                                        perGroup = this._zoteroService.ZoteroPermissions.access.groups as PerGroupPermissions;

                                        var firstGroup = perGroup[this.groupMeta[0].id];
                                        if (firstGroup === undefined) {
                                            this._notificationService.show({
                                                content: " You have no available groups; please create an accessible/shared group within Zotero",
                                                animation: { type: 'slide', duration: 400 },
                                                position: { horizontal: 'center', vertical: 'top' },
                                                type: { style: "info", icon: true },
                                                closable: true
                                            });
                                            await this._zoteroService.DeleteZoteroApiKey(-1);

                                            return;
                                        } else {
                                            // PER GROUP LOGIC GOES HERE
                                            if (firstGroup.library === false || firstGroup.write === false) {
                                                this._notificationService.show({
                                                    content: " You have no available groups; please create an accessible/shared group within Zotero",
                                                    animation: { type: 'slide', duration: 400 },
                                                    position: { horizontal: 'center', vertical: 'top' },
                                                    type: { style: "info", icon: true },
                                                    closable: true
                                                });
                                                await this._zoteroService.DeleteZoteroApiKey(-1);

                                                return;
                                            }

                                            this._zoteroService.hasPermissions = true;
                                            for (var i = 0; i < this.groupMeta.length; i++) {
                                                var group = this.groupMeta[i];
                                                if (group.groupBeingSynced > 0) {
                                                    this._zoteroService.currentGroupBeingSynced = group.groupBeingSynced;
                                                    this._zoteroService.editApiKeyPermissions = true;
                                               }
                                          }
                                          this.changeGroupBeingSynced(this.groupMeta[0]);
                                            return;
                                        }
                                    }

                                    if (this._zoteroService.ZoteroPermissions.access.user.library === undefined ||
                                        this._zoteroService.ZoteroPermissions.access.groups === undefined ||
                                        groupsAll.all === undefined) {
                                        this._notificationService.show({
                                            content: " You have no available groups; please create an accessible/shared group within Zotero",
                                            animation: { type: 'slide', duration: 400 },
                                            position: { horizontal: 'center', vertical: 'top' },
                                            type: { style: "info", icon: true },
                                            closable: true
                                        });
                                        await this._zoteroService.DeleteZoteroApiKey(-1);

                                        return;
                                    }

                                    if (this._zoteroService.ZoteroPermissions.access.user.library || groupsAll.all.library) {
                                        this.zoteroUserID = this._zoteroService.ZoteroPermissions.userID;
                                        this.zoteroUserKey = this._zoteroService.ZoteroPermissions.key;

                                        if (this.zoteroUserKey.length > 0) {

                                            if (this._zoteroService.ZoteroPermissions.access.user.files &&
                                                this._zoteroService.ZoteroPermissions.access.user.library &&
                                                groupsAll.all.write &&
                                                groupsAll.all.library) {
                                                this._zoteroService.hasPermissions = true;
                                                for (var i = 0; i < this.groupMeta.length; i++) {
                                                    var group = this.groupMeta[i];
                                                    if (group.groupBeingSynced > 0) {
                                                        this._zoteroService.currentGroupBeingSynced = group.groupBeingSynced;
                                                        this._zoteroService.editApiKeyPermissions = true;
                                                    }
                                              }
                                              this.changeGroupBeingSynced(this.groupMeta[0]);
                                                await this.FetchLinkedReviewID();
                                                this._zoteroService.fetchGroupMetaData().then(
                                                    async (meta) => {
                                                        this.groupMeta = meta;
                                                        if (!!this.groupMeta[0]) {
                                                            groupId = this.groupMeta[0].id;                                                                                                                     

                                                            //TODO CHECK
                                                            ////this.changeGroupBeingSynced(this.groupMeta[0]);

                                                        }
                                                        else {
                                                            this._notificationService.show({
                                                                content: " You have no available groups; please create an accessible group within Zotero",
                                                                animation: { type: 'slide', duration: 400 },
                                                                position: { horizontal: 'center', vertical: 'top' },
                                                                type: { style: "info", icon: true },
                                                                closable: true
                                                            });
                                                            await this._zoteroService.DeleteZoteroApiKey(-1);

                                                        }
                                                    }

                                                );
                                            } else {
                                                var contentError: string = ' You have not selected read/write permissions on your relevant Zotero group/s';
                                                this._notificationService.show({
                                                    content: contentError,
                                                    animation: { type: 'slide', duration: 400 },
                                                    position: { horizontal: 'center', vertical: 'top' },
                                                    type: { style: "error", icon: true },
                                                    closable: true
                                                });
                                                await this._zoteroService.DeleteZoteroApiKey(-1);


                                            }
                                        } else {
                                            await this._zoteroService.DeleteZoteroApiKey(-1);

                                        }
                                    }
                                });
                    } else {
                        // NEEDS TO AUTHORISE
                        let title = "Authorise Zotero"
                        let msg = 'Please confirm whether you wish to authorise Zotero with your ERWeb account (you must already have created a Zotero account and have at least one group created)';
                        if (this._ActivatedRoute.snapshot.queryParamMap.get('error') == 'Unauthorized') {
                            title = "Authorise Zotero: please retry"
                            msg = 'You need to try again: the authorisation with Zotero sometimes fails at the "verification" step. <br />Most of the times, trying again solves the problem.'
                        }
                        this._confirmationDialogService.confirm(title, msg, false, '')
                            .then(
                                async (confirmed: any) => {
                                    if (confirmed) {
                                        this._confirmationDialogService.confirm('IMPORTANT', 'When authorising Zotero you must set the key permissions to read/write for a specific group; do not just click ACCEPT DEFAULTS, (you must already have created a Zotero account and have at least one group created) ', false, '')
                                            .then(
                                                async (confirmedMsg: any) => {
                                                    if (confirmedMsg) {
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
                                                        await this._zoteroService.DeleteZoteroApiKey(-1);

                                                    }

                                                });
                                    } else {
                                        this._notificationService.show({
                                            content: " You have cancelled Zotero authorisation",
                                            animation: { type: 'slide', duration: 400 },
                                            position: { horizontal: 'center', vertical: 'top' },
                                            type: { style: "info", icon: true },
                                            closable: true
                                        });
                                        await this._zoteroService.DeleteZoteroApiKey(-1);

                                    }
                                }).catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
                    }
                });
    }

    public CodingSets(set_id: number): StatsCompletion[] {
        return this._codesetStatsServ.tmpCodesets.filter(x => x.setId == set_id);
    }

    public get IsServiceBusy(): boolean {

        return this._zoteroService.IsBusy || this._codesetStatsServ.IsBusy;
    }

    Clear() {
      this.apiKeys = [];
      this.groupMeta = [];
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
      await this.FetchLinkedReviewID();
     

        await this.AddLinkedReviewID(group.data);

        await this.UpdateGroupMetaData(group.id, this._ReviewerIdentityServ.reviewerIdentity.userId,
          this.currentReview);

        for (var i = 0; i < this.groupMeta.length; i++) {
          if (this.groupMeta[i].id === group.id) {
            this.groupMeta[i].groupBeingSynced = group.id;
            this._zoteroService.currentGroupBeingSynced = this.groupMeta[i].id;

          } else {
            this.groupMeta[i].groupBeingSynced = 0;
          }
        }
        this._zoteroService.editApiKeyPermissions = true;
   
    }

    public async AddLinkedReviewID(group: GroupData) {
        await this._zoteroService.UpdateGroupToReview(group.id.toString(), false).then(
                async () => {
                    await this.FetchLinkedReviewID();
                });
        this._notificationService.show({
            content: " You currently have Zotero authenticaiton for group: " + group.id.toString(),
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
        });
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
        await this._zoteroService.postGroupMetaData(groupId);
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

    public get HasReviewStats(): boolean {
        return this._codesetStatsServ.ReviewStats.itemsIncluded != -1;
    }

    public async RemoveApiKey(apiKey: UserKey) {

        var result = await this._zoteroService.RemoveApiKey(apiKey, this._ReviewerIdentityServ.reviewerIdentity.userId, this.currentReview);
		if (result) {
            this.apiKeys.splice(0, 1);
            this._notificationService.show({
                content: " You have deleted your apiKey for Zotero you will need to re-start Oauth",
                animation: { type: 'slide', duration: 100 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "info", icon: true },
                closable: true
            });
		} else {
            this._notificationService.show({
                content: " The specified key could not be deleted",
                animation: { type: 'slide', duration: 100 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "error", icon: true },
                closable: true
            });
        }
        this._zoteroService.editApiKeyPermissions = false;
        this._zoteroService.currentGroupBeingSynced = 0;
        this._router.navigate(['Main']);
    }
}
