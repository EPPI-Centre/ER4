import { Component, OnInit, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationService } from '@progress/kendo-angular-notification';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { CodesetStatisticsService, StatsCompletion } from '../services/codesetstatistics.service';
import { ConfigurableReportService } from '../services/configurablereport.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { Criteria, Item, ItemListService } from '../services/ItemList.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSet, ReviewSetsService, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { ZoteroService } from '../services/Zotero.service';
import {
  Collection, GroupData, IERWebANDZoteroReviewItem, IERWebObjects, IERWebZoteroObjects,
  IObjectsInERWebNotInZotero, IObjectSyncState, IZoteroReviewItem, SyncState, ZoteroReviewCollectionList
} from '../services/ZoteroClasses.service';

@Component({
  selector: 'ZoteroSync',
  templateUrl: './ZoteroSync.component.html',
  providers: []
})

export class ZoteroSyncComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private _notificationService: NotificationService,
    public _zoteroService: ZoteroService,
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
  @ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
  CanOnlySelectRoots() {
    return true;
  }

  private objectKeysNotExistsNeedingSyncing: Collection[] = [];
  private objectKeysExistsNeedingSyncing: string[] = [];
  private objectSyncState: IObjectSyncState[] = [];
  private currentReview: number = 0;
  private groupMeta: GroupData[] = [];
  private zoteroUserID: number = 0;
  private totalItemsInCurrentReview: number = 0;
  private zoteroUserName: string = '';
  private zoteroUserKey: string = '';
  public ObjectZoteroList: Collection[] = [];
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
  //public zoteroEditKeyLink: string = '';
  public reviewLinkText: string[] = [];
  public currentLinkedReviewId: string = '';
  public zoteroCollectionList: ZoteroReviewCollectionList = new ZoteroReviewCollectionList();
  public ZoteroObjectsThatAreNotAttachmentsLength: number = 0;
  public ObjectsInERWebAndZotero: IERWebANDZoteroReviewItem[] = [];
  public DetailsForSetId: number = 0;
  private Pushing: boolean = false;
  private Pulling: boolean = false;
  public dropdownBasic1: boolean = false;

  ngOnInit() {
    this.zoteroUserName = this._ReviewerIdentityServ.reviewerIdentity.name;
    this.zoteroUserID = this._ReviewerIdentityServ.reviewerIdentity.userId;
    this.currentLinkedReviewId = this._ReviewerIdentityServ.reviewerIdentity.reviewId.toString();
    this.currentReview = this._ReviewerIdentityServ.reviewerIdentity.reviewId;

    if (this._reviewSetsService.ReviewSets.length == 0) this._reviewSetsService.GetReviewSets(false);
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
        //this.FetchLinkedReviewID();
    }

  public CodingSets(set_id: number): StatsCompletion[] {
    return this._codesetStatsServ.tmpCodesets.filter(x => x.setId == set_id);
  }

  public get IsServiceBusy(): boolean {
    return this._zoteroService.IsBusy || this._codesetStatsServ.IsBusy;
  }

  public Back() {
    this._router.navigate(['Main']);
  }

  public CloseCodeDropDown() {
    this.codingSetData = [];
    this._ItemListService.Clear();

    if (this.WithOrWithoutCodeSelector !== null) {
      let CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData as SetAttribute;
      if (CurrentDropdownSelectedCode !== null) {
        this.codingSetData = this.CodingSets(CurrentDropdownSelectedCode.set_id);
        let cr: Criteria = new Criteria();
        cr.attributeid = CurrentDropdownSelectedCode.attribute_id;
        cr.attributeSetIdList = CurrentDropdownSelectedCode.attributeSetId.toString();
        cr.listType = 'StandardItemList';
        this._ItemListService.FetchWithCrit(cr, "Included Items");
      }
      this.CurrentDropdownSelectedCode = CurrentDropdownSelectedCode;
    }
    this.isCollapsed = false;
    this._hasCodeSelected = true;
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

  public getSyncStateOfObject(key: string): SyncState {
    let index = this.objectSyncState.findIndex(x => x.objectKey === key);
    if (index > -1) {
      return this.objectSyncState[index].syncState;
    } else {
      return SyncState.doesNotExist;
    }
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

  public async RemoveCurrentReviewAT(group: GroupData) {

    await this._zoteroService.UpdateGroupToReview(group.id.toString(), true).then(
      async () => {
        let indexG = this.groupMeta.indexOf(group);
        this.groupMeta[indexG].groupBeingSynced = false;
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
          this.Back();
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
      this.Back();
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
  

  async getErWebObjects() {
    this.ObjectZoteroList = [];
    this.ObjectsInERWebAndZotero = [];
    if (this.Pushing || this.Pulling) {
      return;
    }

      if (!this._zoteroService.hasPermissions) {
            this._notificationService.show({
                content: "You must select a group on the setup page in order to check the current sync status",
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
      await this.fetchZoteroObjectVersionsAsync();     
    }
  }

  public get CodeSets(): ReviewSet[] {

    return this._reviewSetsService.ReviewSets.filter((x: ReviewSet) => x.setType.allowComparison != false);
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
               
      }
    );
  }

  public async DeleteLocallyItemsRemovedFromZotero(): Promise<void> {
    var itemKeys: string[] = [];
    if (!this.Pushing && !this.Pulling) {
      
      var correctLengthOfRightHandTable = this.ObjectZoteroList.filter(x => x.data.itemType !== 'attachment'
        && x.library.id === this._zoteroService.currentGroupBeingSynced).length;
      if (correctLengthOfRightHandTable !== this.ObjectsInERWebAndZotero.length) {
        for (var i = 0; i < this.ObjectsInERWebAndZotero.length; i++) {
          let itemKey = this.ObjectsInERWebAndZotero[i].itemKey;

          let index = this.ObjectZoteroList.findIndex(x => x.key === itemKey);
          if (index === -1) {
            if (this.ObjectsInERWebAndZotero[i].libraryID === this._zoteroService.currentGroupBeingSynced.toString()) {
              itemKeys.push(itemKey)
            }
          }
        }
        await this.StateOfMiddleMan(itemKeys);
      }
    }
  }

  public async GetObjectsDeletedFromZotero(object: IERWebANDZoteroReviewItem): Promise<boolean> {
    let index: number = this.ObjectZoteroList.findIndex(x => x.key === object.itemKey);
    if (index > -1) {
      return false;
    } else {
      return true;
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

        this.objectSyncState = [...objects.filter(x => x.data.itemType !== 'attachment').map(x => <IObjectSyncState>{ objectKey: x.key, syncState: SyncState.upToDate })];
      }
    )
      .then(
      async () => {
          await this.fetchERWebAndZoteroAlreadySyncedItems().then(
            async () => {
              await this.checkSyncState();
              await this.fetchERWebObjectVersionsAsync(); 
              await this.DeleteLocallyItemsRemovedFromZotero();
              this.Pulling = false;
              this.Pushing = false;
            });
      }
    )
  }

  async fetchERWebObjectVersionsAsync(): Promise<void> {
    this.ObjectERWebList = [];
    this.ObjectsInERWebNotInZotero = [];
    for (var i = 0; i < this._ItemListService.ItemList.items.length; i++) {
      var item = this._ItemListService.ItemList.items[i];
      // TODO only ever do the first ten items
      for (var j = 0; j < this.itemsWithThisCode.length; j++) {
        var itemId = this.itemsWithThisCode[j].itemId;

        if (itemId === item.itemId) {
            if (this.ObjectsInERWebNotInZotero.length > this.maxAllowedItemsToBePushedToZotero) {
              break;
            }

            if (this.ObjectERWebList.map(x => x.itemID).indexOf(item.itemId) == -1) {
              let ErWebObject = <IERWebObjects>{
                itemID : item.itemId,
                itemDocumentID: 0,
                itemReviewID: 0
              };
              this.ObjectERWebList.push(ErWebObject);
            }
            var newItem = <IObjectsInERWebNotInZotero>{
              itemId: item.itemId,
              itemReviewId: 0, //item.itemReviewID,
              shortTitle: this.itemsWithThisCode[j].shortTitle,
              typeName: this.itemsWithThisCode[j].typeName,
              documentAttached: false // item.itemDocumentID === 0 ? false : true
            };

          if (this.ObjectsInERWebAndZotero.length === 0) {
            console.log('newItem not in zotero: ' + JSON.stringify(newItem));
            this.ObjectsInERWebNotInZotero.push(newItem);
          }
          if (this.ObjectsInERWebAndZotero.map(x => x.itemID).indexOf(newItem.itemId) === -1) {
            console.log('newItem not in zotero: ' + JSON.stringify(newItem));
            this.ObjectsInERWebNotInZotero.push(newItem);
          }
          
          }
        }
    }

    this.ObjectsInERWebAndZotero = this.ObjectsInERWebAndZotero.sort((a, b) => a.itemKey.localeCompare(b.itemKey));
    await this.checkSyncState();
  }

  async PullConfirmZoteroItems(): Promise<void> {
    this.Pulling = true;
    this._zoteroService.fetchZoteroObjectVersionsAsync().then(
      async (objects) => {
        console.log('objects: ' + JSON.stringify(objects));
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
      }
    ).then(
      async () => {

        var updateOrInsertIntoErWeb: boolean = false;
        for (var i = 0; i < this.objectSyncState.length; i++) {
          if (this.objectSyncState[i].syncState !== SyncState.upToDate) {
            updateOrInsertIntoErWeb = true;
          }
        }

        if (updateOrInsertIntoErWeb === false) {
          this._notificationService.show({
            content: "No Zotero object/s to pull",
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
          });
          return;
        }
        let msg: string = 'Are you sure you want to pull these object/s into ERWeb? ';
        this._confirmationDialogService.confirm('Zotero Sync', msg, false, '', 'Pull Data')
          .then(async (confirm: boolean) => {
            if (confirm === true) {
              await this.PullZoteroItems();

              async () => {
                await this.fetchERWebAndZoteroAlreadySyncedItems();
                await this.checkSyncState();
                await this.getErWebObjects();

              }

            } else {
              this.Clear();
              this.itemsWithThisCode = this._ItemListService.ItemList.items;
              await this.fetchERWebObjectVersionsAsync().then(
                async () => {
                  await this.fetchZoteroObjectVersionsAsync();
                  this.Pulling = false;
                }
              );

              return;
            }
          });
      }
    )
  }

  async PullZoteroItems(): Promise<void> {

    var arrayOfItemsToPullIntoErWeb: Collection[] = [];
    console.log('we will pull these: ' + JSON.stringify(this.objectKeysExistsNeedingSyncing));
    for (var i = 0; i < this.objectKeysExistsNeedingSyncing.length; i++) {
      var itemKey = this.objectKeysExistsNeedingSyncing[i];
      await this._zoteroService.fetchZoteroObjectAsync(itemKey).then(
        (itemCollection) => {
          arrayOfItemsToPullIntoErWeb.push(itemCollection);
        });
    }
    let errCount = 0;
    if (arrayOfItemsToPullIntoErWeb.length > 0) {
      console.log('we will pull these collectionOfItemsToInsert: ' + JSON.stringify(arrayOfItemsToPullIntoErWeb));
      await this._zoteroService.insertZoteroObjectIntoERWebAsync(arrayOfItemsToPullIntoErWeb).then(
        async (result: boolean) => {
          if (!result) {
            if (keyResults.find(x => x === item.key)) return;
            console.log('There was an error with this one: ' + JSON.stringify(item));
            errCount++
          } else {
            console.log('There was NOT an error with this one: ' + JSON.stringify(item));
          }
        });

      if (errCount > 0) {
        var errMsg = "Zotero object/s insertion into ERWeb has failed";
        this._notificationService.show({
          content: errMsg,
          animation: { type: 'slide', duration: 400 },
          position: { horizontal: 'center', vertical: 'top' },
          type: { style: "error", icon: true },
          closable: true
        });
      } else {
        this._notificationService.show({
          content: " Zotero object/s has been pulled into ERWeb",
          animation: { type: 'slide', duration: 400 },
          position: { horizontal: 'center', vertical: 'top' },
          type: { style: "info", icon: true },
          closable: true
        });
        this.ObjectERWebList = [];
        this.objectKeysNotExistsNeedingSyncing = [];
        this.objectKeysExistsNeedingSyncing = [];
        await this.fetchZoteroObjectVersionsAsync();
      }
      this.Pulling = false;
      await this.getErWebObjects();
    }


    if (this.objectKeysNotExistsNeedingSyncing.length > 0) {
      arrayOfItemsToPullIntoErWeb = [];
      for (var i = 0; i < this.objectKeysNotExistsNeedingSyncing.length; i++) {
        var item = this.objectKeysNotExistsNeedingSyncing[i];
        if (item === null || item.data === null || item.key.length === 0) return;
        await this._zoteroService.fetchZoteroObjectAsync(item.key)
          .then(
            (itemCollection) => {
              arrayOfItemsToPullIntoErWeb.push(itemCollection);
            });
      }
      errCount = 0;
      var keyResults: string[] = [];
      var itemsToInsertIntoErWeb = [];
      for (var i = 0; i < arrayOfItemsToPullIntoErWeb.length; i++) {
        var item = arrayOfItemsToPullIntoErWeb[i];
        itemsToInsertIntoErWeb.push(item);
      }

      await this._zoteroService.insertZoteroObjectIntoERWebAsync(itemsToInsertIntoErWeb).then(
        async (result: boolean) => {
          if (!result) {
            if (keyResults.find(x => x === item.key)) return;
            console.log('There was an error with this one: ' + JSON.stringify(item));
            errCount++
          } else {
            console.log('There was NOT an error with this one: ' + JSON.stringify(item));
          }
        });

      if (errCount > 0) {
        var errMsg = "Zotero object/s insertion into ERWeb has failed";
        this._notificationService.show({
          content: errMsg,
          animation: { type: 'slide', duration: 400 },
          position: { horizontal: 'center', vertical: 'top' },
          type: { style: "error", icon: true },
          closable: true
        });
      } else {
        this._notificationService.show({
          content: " Zotero object/s has been pulled into ERWeb",
          animation: { type: 'slide', duration: 400 },
          position: { horizontal: 'center', vertical: 'top' },
          type: { style: "info", icon: true },
          closable: true
        });
        this.ObjectERWebList = [];
        this.objectKeysNotExistsNeedingSyncing = [];
        this.objectKeysExistsNeedingSyncing = [];
        await this.fetchZoteroObjectVersionsAsync();
      }
      this.Pulling = false;
      await this.getErWebObjects();
    }

  }

  async PushERWebItems() {
    this.Pushing = true;
    await this.fetchERWebAndZoteroAlreadySyncedItems();
    await this.PushAndCallStatus();

  }

  async PushAndCallStatus(): Promise<void> {
    if (this.ObjectERWebMetaDataAheadOfZoteroList.length > 0) {
      // TODO check this just does ten items at a time
      this.ObjectERWebMetaDataAheadOfZoteroList = this.ObjectERWebMetaDataAheadOfZoteroList.slice(0, 10);
      let msg: string = 'Are you sure you wish to PUSH these items in Zotero? (Only the first ten items will be pushed given Zotero limitations) ' + this.ObjectERWebMetaDataAheadOfZoteroList.map(x => x.itemID.toString()).join(',');
      this._confirmationDialogService.confirm('Zotero Sync', msg, false, '')
        .then(async (confirm: boolean) => {
          if (confirm === true) {
            await this._zoteroService.updateERWebItemsInZotero(this.ObjectERWebMetaDataAheadOfZoteroList).then(
              async (result) => {
                if (result) {
                  this._notificationService.show({
                    content: " Zotero object/s have been updated in Zotero",
                    animation: { type: 'slide', duration: 400 },
                    position: { horizontal: 'center', vertical: 'top' },
                    type: { style: "info", icon: true },
                    closable: true
                  });
                  await this.fetchZoteroObjectVersionsAsync();
                  await this.checkSyncState();
                  await this.getErWebObjects();
                } else {
                  this._notificationService.show({
                    content: " Zotero object/s update has failed",
                    animation: { type: 'slide', duration: 400 },
                    position: { horizontal: 'center', vertical: 'top' },
                    type: { style: "error", icon: true },
                    closable: true
                  });
                }
                this.Pushing = false;


              });
          } else {
            this.Pushing = false;

            return;
          }
        });
    }

    if (this.ObjectsInERWebNotInZotero.length > 0) {
      var listItemIDsToPush = this.ObjectsInERWebNotInZotero.map(x => x.itemId.toString());
      let msg: string = 'Are you sure you wish to PUSH these items to Zotero? ' +
        '(Only the first ten will be pushed given Zotero limitations) ';
      await this._confirmationDialogService.confirm('Zotero Sync', msg, false, '')
        .then(async (confirm: any) => {
          if (confirm === true) {

            await this._zoteroService.postERWebItemsToZotero(listItemIDsToPush)
              .then(
                async (result) => {
                  if (result.toString() === "true") {
                    this._notificationService.show({
                      content: " ERWeb object/s have been pushed to Zotero",
                      animation: { type: 'slide', duration: 400 },
                      position: { horizontal: 'center', vertical: 'top' },
                      type: { style: "info", icon: true },
                      closable: true
                    });
                  } else {
                    this._notificationService.show({
                      content: " ERWeb object/s push has failed",
                      animation: { type: 'slide', duration: 400 },
                      position: { horizontal: 'center', vertical: 'top' },
                      type: { style: "error", icon: true },
                      closable: true
                    });
                  }
                  await this.checkSyncState();
                  await this.getErWebObjects();
                  this.Pushing = false;
                });
          } else {
            this.Pushing = false;
          }
        });
    }
  }

  async checkSyncState(): Promise<void> {
    this.ItemsInERWebANDInZotero = [];
    let href = '';
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
                  console.log('Got here:' + JSON.stringify(zoteroItemVersion.links.attachment));
                  if (zoteroItemVersion.links.attachment !== null && zoteroItemVersion.links.attachment !== undefined) {
                    href = zoteroItemVersion.links.attachment.href;
                    console.log('testing href: ' + JSON.stringify(href));
                  }

                  if (zoteroItemVersion.links.attachment === null || zoteroItemVersion.links.attachment === undefined) {
                    stateRow.syncState = SyncState.upToDate
                  } else {

                    // TODO currently this is incorrect
                    // it needs to check that the stored itemreviewId in middle man table
                    // has a linked doc in itemDocuments...!!
                    console.log('Check itemreviewId has a linked doc: ' + JSON.stringify(zoteroReviewItemResult));
                    await this.CheckAttachmentExistsAsync(zoteroReviewItemResult).then(
                      (result: boolean) => {
                        var syncState: SyncState;
                        if (result === true) {
                          syncState = SyncState.upToDate;
                        }
                        else {
                          syncState = SyncState.attachmentDoesNotExist;
                          if (zoteroItemVersion !== undefined) {
                            this.attachmentKeyToSync = this.GetAttachmentKey(href);
                            var attachmentToInsert = this.ObjectZoteroList.filter(x => x.key ===
                              this.attachmentKeyToSync)[0];
                            this.objectKeysNotExistsNeedingSyncing.push(attachmentToInsert);
                          }
                        }
                        console.log('what!! syncstate ' + syncState);
                        if (stateRow !== undefined && syncState != undefined) {
                          stateRow.syncState = syncState;
                          // TODO when changed to use versions
                          //if (syncState === SyncState.doesNotExist ||
                          //    syncState === SyncState.attachmentDoesNotExist) {
                          //    console.log('what!!');
                          //    var attachmentToInsert = this.ObjectZoteroList.filter(x => x.key ===
                          //        this.attachmentKeyToSync)[0];
                          //    this.objectKeysNotExistsNeedingSyncing.push(attachmentToInsert);
                          //}
                        }
                      }
                    );
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

  async CheckAttachmentExistsAsync(zoteroReviewItemResult: IZoteroReviewItem): Promise<boolean> {

    //TODO this needs to check the relevant itemDocument locally exists
    // 1 - from the itemReviewID get the itemDocID
    var parentToAttachment_REVIEW_ID = zoteroReviewItemResult.iteM_REVIEW_ID;
    if (parentToAttachment_REVIEW_ID > -1) {
      // have to get the item id first
      return await this._zoteroService.fetchERWebAttachmentState(parentToAttachment_REVIEW_ID.toString());
    }
    return false;
  }

  private GetAttachmentKey(attachment: string): string {
    if (attachment === null) throw Error();
    var indexSlash = attachment.lastIndexOf('/');
    var attachmentKey = attachment.substr(indexSlash + 1, attachment.length - indexSlash - 1);
    console.log('attachmentKey' + attachmentKey);
    return attachmentKey;
  }

  ngOnDestroy() {
  }

  get HasRunMetaInfo(): boolean {
    var objectsExist: boolean = this.objectSyncState.length > 0;
    var hasPermission: boolean = this._zoteroService.hasPermissions;
    var objectsCanBeSynced: boolean = false;
    for (var i = 0; i < this.objectSyncState.length; i++) {
      var syncState = this.objectSyncState[i].syncState;
      if (syncState !== SyncState.upToDate) {
        objectsCanBeSynced = true;
      }
    }
    return (objectsExist && hasPermission && objectsCanBeSynced);
  }

  get HasCodeSelected(): boolean {
    return this._hasCodeSelected;
  }

  get HasERWebMetaInfo(): boolean {

    var objectsNeedPushingToZotero: boolean = this.objectSyncState.findIndex(x => x.syncState === SyncState.ahead) !== -1 ||
      this.ObjectERWebList.length > 0;

    var hasPermission: boolean = this._zoteroService.hasPermissions;

    return (objectsNeedPushingToZotero && hasPermission);
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

}
