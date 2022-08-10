import { Component,  OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationService } from '@progress/kendo-angular-notification';
import { CodesetStatisticsService, StatsCompletion } from '../services/codesetstatistics.service';
import { ConfigurableReportService } from '../services/configurablereport.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { Criteria, Item, ItemListService } from '../services/ItemList.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, singleNode } from '../services/ReviewSets.service';
import { ZoteroService } from '../services/Zotero.service';
import {
    Collection, Group, GroupData, Groups, IERWebANDZoteroReviewItem,
    IERWebObjects, IERWebZoteroObjects, IObjectsInERWebNotInZotero,
    IObjectSyncState, IZoteroReviewItem, PerGroupPermissions, SyncState, UserKey, ZoteroReviewCollectionList
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
        private _ItemListService: ItemListService,
        private _configurablereportServ: ConfigurableReportService,
        private _reviewSetsService: ReviewSetsService
    ) {

    }

    private maxAllowedItemsToBePushedToZotero: number = 10;
    private objectKeysNotExistsNeedingSyncing: Collection[] = [];
    private objectKeysExistsNeedingSyncing: string[] = [];
    private objectSyncState: IObjectSyncState[] = [];
    public currentReview: number = 0;
    public groupMeta: Group[] = [];
  public zoteroUserID: number = 0;
    public totalItemsInCurrentReview: number = 0;
    public zoteroUserName: string = '';
  public zoteroUserKey: string = '';
    private ObjectZoteroList: Collection[] = [];
    private ObjectERWebList: IERWebObjects[] = [];
    private ObjectERWebMetaDataAheadOfZoteroList: IERWebZoteroObjects[] = [];
    private hasPermissions: boolean = false;
    private _hasCodeSelected: boolean = false;
    private attachmentKeyToSync: string = '';

    public CurrentDropdownSelectedCode: singleNode | null = null;
    public ObjectsInERWebNotInZotero: IObjectsInERWebNotInZotero[] = [];
    public ItemsInERWebANDInZotero: Collection[] = [];
    public codingSetData: StatsCompletion[] = [];
    public itemsWithThisCode: Item[] = [];
    public isCollapsed = false;
    public zoteroLink: string = '';
    public zoteroEditKeyLink: string = '';
    public reviewLinkText: string[] = [];
    public currentLinkedReviewId: string = '';
    public zoteroCollectionList: ZoteroReviewCollectionList = new ZoteroReviewCollectionList();
    public ZoteroObjectsThatAreNotAttachmentsLength: number = 0;
    public ObjectsInERWebAndZotero: IERWebANDZoteroReviewItem[] = [];
    public DetailsForSetId: number = 0;
    private Pushing: boolean = false;
    private Pulling: boolean = false;
    public apiKeys: UserKey[] = [];
    private currentApiKey: string = "";

    ngOnDestroy() {
    }

    ngOnInit() {
        this._zoteroService.editApiKeyPermissions = false;
        this.zoteroUserName = this._ReviewerIdentityServ.reviewerIdentity.name;
        this.zoteroUserID = this._ReviewerIdentityServ.reviewerIdentity.userId;
        this.currentLinkedReviewId = this._ReviewerIdentityServ.reviewerIdentity.reviewId.toString();
        this.currentReview = this._ReviewerIdentityServ.reviewerIdentity.reviewId;
        var groupId = 0;
        this._zoteroService.GetZoteroApiKey().then(
                (zoteroApiKey) => {
                    this.currentApiKey = zoteroApiKey;
                    if (this.currentApiKey && this.currentApiKey.length > 0) {
                        
                       this._zoteroService.fetchApiKeys().then(
                               async (userKeys) => {

                                   this.apiKeys = userKeys;
                                });


                        // ALREADY HAS A ZOTERO API KEY
                        this.FetchLinkedReviewID();
                        this.zoteroEditKeyLink = 'https://www.zotero.org/oauth/authorize?oauth_token=' + zoteroApiKey;
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
                                                await this.FetchLinkedReviewID();
                                                this._zoteroService.fetchGroupMetaData().then(
                                                    async (meta) => {
                                                        this.groupMeta = meta;
                                                        if (!!this.groupMeta[0]) {
                                                            groupId = this.groupMeta[0].id;

                                                            let cr: Criteria = new Criteria();
                                                            cr.showDeleted = false;
                                                            cr.pageNumber = 0;
                                                            cr.searchId = 44982;
                                                            let ListDescription: string = 'Coded with: Include';
                                                            cr.listType = 'GetItemSearchList';
                                                            this._ItemListService.FetchWithCrit(cr, ListDescription);
                                                            this.currentReview = this._ReviewerIdentityServ.reviewerIdentity.reviewId;
                                                            this._codesetStatsServ.GetReviewStatisticsCountsCommand(true, true);
                                                            this._zoteroService.fetchERWebItemsToPushToZotero(this._ItemListService.ItemList.items.map(x => x.itemId).join(',')).then(
                                                                (itemReviewIDs: string[]) => {

                                                                }
                                                            );

                                                            this.totalItemsInCurrentReview = this._codesetStatsServ.ReviewStats.itemsIncluded;
                                                            await this.FetchLinkedReviewID();
                                                            for (var i = 0; i < this.groupMeta.length; i++) {
                                                                var group = this.groupMeta[i];
                                                                if (group.groupBeingSynced > 0) {
                                                                    this._zoteroService.currentGroupBeingSynced = group.groupBeingSynced;
                                                                    this._zoteroService.editApiKeyPermissions = true;
                                                                }
                                                            }

                                                            this.changeGroupBeingSynced(this.groupMeta[0]);

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

    public CountZoteroObjectsThatAreNotAttachments(): void {
        let objects = this.ObjectZoteroList.map(x => x.data.itemType !== 'attachment');
        this.ZoteroObjectsThatAreNotAttachmentsLength = objects.length;
    }

    public ObjectTypeValidity(object: Collection): boolean {
        return object.data.itemType !== 'attachment';
    }

    public get hasItems(): boolean {
        return (this.itemsWithThisCode.length > 0);
    }

    async getErWebObjects() {
        this.ObjectZoteroList = [];
        this.ObjectsInERWebAndZotero = [];
        if (this.Pushing || this.Pulling) {
            return;
        }

        if (this._zoteroService.currentGroupBeingSynced === 0) {
            this._notificationService.show({
                content: "You must select a group in order to check the current sync status",
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "info", icon: true },
                closable: true
            });
            this.Pulling = false;
            this.Pushing = false;
            return;

        } else {
            this.Clear();
            this.itemsWithThisCode = this._ItemListService.ItemList.items;
            await this.fetchERWebObjectVersionsAsync().then(
                async () => {
                    await this.fetchZoteroObjectVersionsAsync();
                    this.Pulling = false;
                    this.Pushing = false;
                }
            );
        }
    }

    async fetchZoteroObjectVersionsAsync(): Promise<void> {
        this.ObjectZoteroList = [];
        this._zoteroService.fetchZoteroObjectVersionsAsync().then(
            (objects) => {
                this.ObjectZoteroList = objects;
                this.ObjectZoteroList = this.ObjectZoteroList.sort((a, b) => {
                    if (a.data.key > b.data.key) {
                        return 1;
                    }

                    if (a.data.key < b.data.key) {
                        return -1;
                    }

                    return 0;
                });

                this.objectSyncState = [...objects.filter(x => x.data.itemType !== 'attachment').map(x => <IObjectSyncState>{ objectKey: x.key, syncState: SyncState.doesNotExist })];
            }
        ).then(
            async () => {
                await this.fetchERWebAndZoteroAlreadySyncedItems();
                await this.checkSyncState();
            }
        )
    }

    async fetchERWebAndZoteroAlreadySyncedItems(): Promise<void> {
        this.ObjectsInERWebAndZotero = [];
        await this._zoteroService.fetchERWebAndZoteroAlreadySyncedItems().then(
            async (objects) => {
                for (var i = 0; i < objects.length; i++) {
                    if (objects[i].libraryID == this._zoteroService.currentGroupBeingSynced.toString()) {
                        let itemKey = objects[i].itemKey;
                        if (this.ObjectsInERWebAndZotero.findIndex(x => x.itemKey === itemKey) > -1) {
                        } else {
                            if (objects[i].libraryID === this._zoteroService.currentGroupBeingSynced.toString()) {
                                this.ObjectsInERWebAndZotero.push(objects[i]);
                            }
                        }
                    }
                }
                this.ObjectsInERWebAndZotero = this.ObjectsInERWebAndZotero.sort((a, b) => a.itemKey.localeCompare(b.itemKey));

                // TODO when logic correct for pushing will call
                await this.DeleteLocallyItemsRemovedFromZotero();
            }
        );
    }
    public async StateOfMiddleMan(itemKeys: string[]): Promise<void> {

        for (var i = 0; i < itemKeys.length; i++) {
            var itemKey = itemKeys[i];
            await this._zoteroService.deleteMiddleMan(itemKey).then(
                (result) => {
                    let indexSecond = this.ObjectsInERWebAndZotero.findIndex(x => x.itemKey === itemKey);
                    if (indexSecond > -1) {
                        this.ObjectsInERWebAndZotero.splice(indexSecond, 1);
                    }
                    this.ObjectsInERWebAndZotero = this.ObjectsInERWebAndZotero.sort((a, b) => a.itemKey.localeCompare(b.itemKey));
                    this._notificationService.show({
                        content: "ERWeb has deleted the following object: " + itemKey + ", as it has been removed from Zotero",
                        animation: { type: 'slide', duration: 400 },
                        position: { horizontal: 'center', vertical: 'top' },
                        type: { style: "warning", icon: true },
                        closable: true
                    });
                }
            );
        }
    }


    public async DeleteLocallyItemsRemovedFromZotero(): Promise<void> {
        var itemKeys: string[] = [];
        if (!this.Pushing && !this.Pulling) {
            if (this.ObjectZoteroList.length !== this.ObjectsInERWebAndZotero.length) {
                for (var i = 0; i < this.ObjectsInERWebAndZotero.length; i++) {
                    let itemKey = this.ObjectsInERWebAndZotero[i].itemKey;

                    let index = this.ObjectZoteroList.findIndex(x => x.key === itemKey);
                    if (index === -1) {
                        if (this.ObjectsInERWebAndZotero[i].libraryID === this._zoteroService.currentGroupBeingSynced.toString()) {
                            itemKeys.push(itemKey)
                        }
                    }
                }
                // PUT BACK WHEN PUSHING FIXED
                await this.StateOfMiddleMan(itemKeys);
            }
        }
    }

    async checkSyncState(): Promise<void> {
        this.ItemsInERWebANDInZotero = [];

        this.ObjectZoteroList.forEach(
            async (item) => {
                if (item.key.length > 0) {

                    let zoteroReviewItemResult: IZoteroReviewItem = await this._zoteroService.getVersionOfItemInErWebAsync(item);
                    if (zoteroReviewItemResult === null) return;
                    let localItemVersion = zoteroReviewItemResult.version;
                    let zoteroItemVersion = this.ObjectZoteroList.find(x => x == item);
                    let localVersion = parseInt(localItemVersion) || -1;
                    var stateRow = this.objectSyncState.find(x => x.objectKey === item.key);
                    if (localVersion == -1) {
                        this.objectKeysNotExistsNeedingSyncing.push(item);
                        var stateRow = this.objectSyncState.find(x => x.objectKey === item.key);
                        if (stateRow) {
                            stateRow.syncState = SyncState.doesNotExist;
                        }
                    } else {
                        if (zoteroItemVersion !== undefined) {
                            if (zoteroItemVersion.version == localVersion) {
                                if (stateRow !== undefined) {
                                    // check for an attachment, if so check its state separately                                    
                                    if (zoteroItemVersion.links.attachment === null) {
                                        stateRow.syncState = SyncState.upToDate
                                    } else {

                                        stateRow.syncState = SyncState.upToDate
                                        // TODO currently this is incorrect
                                        // it needs to check that the stored itemreviewId in middle man table
                                        // has a linked doc in itemDocuments...!!

                                        //await this.CheckAttachmentExistsAsync(zoteroReviewItemResult).then(
                                        //    (result: boolean) => {
                                        //        var syncState: SyncState;
                                        //        if (result === true) {
                                        //            syncState = SyncState.upToDate;
                                        //        }
                                        //        else {
                                        //            syncState = SyncState.attachmentDoesNotExist;

                                        //        }
                                        //        console.log('what!! syncstate ' + syncState);
                                        //        if (stateRow !== undefined && syncState != undefined) {
                                        //            stateRow.syncState = syncState;
                                        //            // TODO when changed to use versions
                                        //            //if (syncState === SyncState.doesNotExist ||
                                        //            //    syncState === SyncState.attachmentDoesNotExist) {
                                        //            //    console.log('what!!');
                                        //            //    var attachmentToInsert = this.ObjectZoteroList.filter(x => x.key ===
                                        //            //        this.attachmentKeyToSync)[0];
                                        //            //    this.objectKeysNotExistsNeedingSyncing.push(attachmentToInsert);
                                        //            //}
                                        //        }
                                        //    }
                                        //);
                                    }
                                }
                            } else if (localVersion < zoteroItemVersion.version) {
                                if (stateRow !== undefined) {
                                    stateRow.syncState = SyncState.behind;
                                }
                                this.objectKeysExistsNeedingSyncing.push(item.key);
                            } else if (localVersion > zoteroItemVersion.version) {
                                if (stateRow !== undefined) {
                                    stateRow.syncState = SyncState.ahead;
                                    if (zoteroReviewItemResult.iteM_REVIEW_ID) {
                                        await this._zoteroService.fetchItemIDPerItemReviewID(zoteroReviewItemResult.iteM_REVIEW_ID.toString()).then(
                                            (itemID: string) => {
                                                var itemLocallyAhead = <IERWebZoteroObjects>{ itemID: parseInt(itemID), itemReviewID: zoteroReviewItemResult.iteM_REVIEW_ID, itemkey: item.key, version: zoteroReviewItemResult.version, itemDocumentID: -1 };  //TODO need to populate correctly when necessary
                                                this.ObjectERWebMetaDataAheadOfZoteroList.push(itemLocallyAhead);
                                            }
                                        )
                                    }
                                }
                                this.objectKeysExistsNeedingSyncing.push(item.key);
                            }
                        }
                    }
                }
            }
        );


        var tempListOfDocumentsFromZotero = this.ObjectZoteroList.filter(x => x.data.itemType === 'attachment');
        var tempListOfNonDocumentsFromZotero = this.ObjectZoteroList.filter(x => x.data.itemType !== 'attachment');
        this.ObjectZoteroList = [];
        this.ObjectZoteroList = tempListOfNonDocumentsFromZotero.concat(tempListOfDocumentsFromZotero);

        this.Pushing = false;
        this.Pulling = false;
        this.ObjectsInERWebAndZotero = this.ObjectsInERWebAndZotero.sort((a, b) => a.itemKey.localeCompare(b.itemKey));
        this.ObjectZoteroList = this.ObjectZoteroList.sort((a, b) => {
            if (a.data.key > b.data.key) {
                return 1;
            }
            if (a.data.key < b.data.key) {
                return -1;
            }
            return 0;
        });
    }


    async fetchERWebObjectVersionsAsync(): Promise<void> {
        await this._zoteroService.fetchERWebObjectsNotInZoteroAsync().then(
            (result) => {
                this.ObjectERWebList = [];
                this.ObjectsInERWebNotInZotero = [];
                for (var i = 0; i < result.length; i++) {
                    var item = result[i];
                    // TODO only ever do the first ten items
                    for (var j = 0; j < this.itemsWithThisCode.length; j++) {
                        var itemId = this.itemsWithThisCode[j].itemId;

                        if (itemId === item.itemID) {
                            const index: number = j;
                            if (index > -1) {
                                if (this.ObjectsInERWebNotInZotero.length > this.maxAllowedItemsToBePushedToZotero) {
                                    break;
                                }
                                if (this.ObjectERWebList.map(x => x.itemID).indexOf(item.itemID) == -1) {
                                    this.ObjectERWebList.push(item);
                                }
                                var newItem = <IObjectsInERWebNotInZotero>{
                                    itemId: item.itemID,
                                    itemReviewId: item.itemReviewID,
                                    shortTitle: this.itemsWithThisCode[index].shortTitle,
                                    typeName: this.itemsWithThisCode[index].typeName,
                                    documentAttached: item.itemDocumentID === 0 ? false : true
                                };
                                if (this.ObjectsInERWebNotInZotero.map(x => x.itemId).indexOf(newItem.itemId) == -1 &&
                                    this.ObjectsInERWebAndZotero.map(x => x.itemID).indexOf(newItem.itemId) == -1) {
                                    this.ObjectsInERWebNotInZotero.push(newItem);
                                }
                            }
                        }
                    }
                }
            }
        ).then(
            async () => {
                this.ObjectsInERWebAndZotero = this.ObjectsInERWebAndZotero.sort((a, b) => a.itemKey.localeCompare(b.itemKey));
                await this.checkSyncState();
            }
        )
    }


    Clear() {
        this.objectKeysExistsNeedingSyncing = [];
        this.objectKeysNotExistsNeedingSyncing = [];
        this.ObjectERWebList = [];
        this.ObjectsInERWebNotInZotero = [];
        this.ObjectsInERWebAndZotero = [];
        this.ObjectZoteroList = [];
        this.ItemsInERWebANDInZotero = [];
        this.ObjectERWebMetaDataAheadOfZoteroList = [];
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
        if (this.groupMeta.length == 0) {
           
        }
    }

    public HasLinkedReviewID(group: GroupData): boolean {
        let index = this.zoteroCollectionList.ZoteroReviewCollectionList.findIndex(x => x.libraryID === group.id.toString());
        return index > -1;
    }

    public async AddLinkedReviewID(group: GroupData) {
        await this._zoteroService.UpdateGroupToReview(group.id.toString(), false).then(
                async () => {
                    await this.FetchLinkedReviewID();
                });
        this._notificationService.show({
            content: " You have added Zotero authenticaiton for group: " + group.id.toString(),
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

    public async changeGroupBeingSynced(group: Group) {
        await this.FetchLinkedReviewID();
        let index = this.zoteroCollectionList.ZoteroReviewCollectionList.findIndex(x => x.libraryID === group.id.toString());
        if (index === -1) {
            this._notificationService.show({
                content: "You must add this group in order to sync with it",
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "warning", icon: true },
                closable: true
            });
            return;
        } else {
            
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
            await this.getErWebObjects();            
        }
    }

    async UpdateGroupMetaData(groupId: number, userId: number, currentReview: number) {
        await this._zoteroService.postGroupMetaData(groupId);
    }

    public get IsReportsServiceBusy(): boolean {
        return this._configurablereportServ.IsBusy;
    }
    public get ScreeningSets(): StatsCompletion[] {
        return this._codesetStatsServ.tmpCodesets.filter((found: StatsCompletion) => found.subTypeName == 'Screening');
    }
    public get StandardSets(): StatsCompletion[] {
        return this._codesetStatsServ.tmpCodesets.filter((found: StatsCompletion) => found.subTypeName == 'Standard');
    }
    public get AdminSets(): StatsCompletion[] {
        return this._codesetStatsServ.tmpCodesets.filter((found: StatsCompletion) => found.subTypeName == 'Administration');
    }

    ShowDetailsForSetId(SetId: number) {
        if (this.DetailsForSetId == SetId) this.DetailsForSetId = 0;
        else this.DetailsForSetId = SetId;
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

    public async GetObjectsDeletedFromZotero(object: IERWebANDZoteroReviewItem): Promise<boolean> {
        let index: number = this.ObjectZoteroList.findIndex(x => x.key === object.itemKey);
        if (index > -1) {
            return false;
        } else {
            return true;
        }
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
        this.currentApiKey = "";
        this._zoteroService.editApiKeyPermissions = false;
        this._zoteroService.currentGroupBeingSynced = 0;
        this._router.navigate(['Main']);
    }
}
