import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import {
  ApiKeyInfo, Group, ZoteroItem, ZoteroAttachment, iZoteroERWebReviewItem,
  ZoteroERWebReviewItem,
  SyncState,
  iZoteroItemsResult,
  ZoteroERWebItemDoc
} from './ZoteroClasses.service';
import { ConfigService } from './config.service';
import { lastValueFrom, Subscription } from 'rxjs';
import { EventEmitterService } from './EventEmitter.service';
import { CustomSorting, LocalSort } from '../helpers/CustomSorting';

@Injectable({
    providedIn: 'root',
})

export class ZoteroService extends BusyAwareService implements OnDestroy {
   
  constructor(
    private _httpC: HttpClient,
    private modalService: ModalService,
    private EventEmitterService: EventEmitterService,
    configService: ConfigService
  ) {
    super(configService);
    this.clearSub = this.EventEmitterService.OpeningNewReview.subscribe(() => { this.Clear(); });
  }
  private clearSub: Subscription | null = null;
  //public editApiKeyPermissions: boolean = false;
  public hasPermissions: boolean = false;//overall flag. When True, user can pull/push.
  private _errorMessage: string = "data not fetched";//should be empty when no error is present
  public get ErrorMessage(): string {
    if (this.hasPermissions == true) return "";//no error: the overall flag is in control.
    else return this._errorMessage;
  }
  public SetError(errorMsg: string) {
    if (errorMsg == "") {
      this._errorMessage = "";
      this.hasPermissions = true;
    }
    else {
      this._errorMessage = errorMsg;
      this.hasPermissions = false;
    }
  }
  public groupMeta: Group[] = [];
  private userKeyInfo: ApiKeyInfo = <ApiKeyInfo>{};
  public get ZoteroPermissions(): ApiKeyInfo{
            return this.userKeyInfo;
    }
  public set ZoteroPermissions(value: ApiKeyInfo) {
        this.userKeyInfo = value;
  }
  private _LibraryName: string = "[Unknown]";
  public get NameOfCurrentLibrary(): string {
    return this._LibraryName;
  }
  private _ZoteroItems: ZoteroItem[] = [];
  public get ZoteroItems(): ZoteroItem[] {
    return this._ZoteroItems;
  }

  private _zoteroERWebReviewItemList: ZoteroERWebReviewItem[] = [];
  public get ZoteroERWebReviewItemList() {
    return this._zoteroERWebReviewItemList;
  }

  private _BusyMessage = "";
  public get BusyMessage(): string {
    if (this._BusyMethods.length == 0) {
      return "";
    }
    return this._BusyMessage;
  }

  public async PushZoteroErWebReviewItemList(): Promise<boolean> {

    //we handle 2 cases: user has selected some (pushable) items, OR push all that's pushable...
    let itemsToPush: ZoteroERWebReviewItem[] = this.ZoteroERWebReviewItemList.filter(f => f.ClientSelected == true && (f.syncState == SyncState.canPush || f.HasPdfToPush));
    if (itemsToPush.length == 0) itemsToPush = this.ZoteroERWebReviewItemList.filter(f => f.syncState == SyncState.canPush || f.HasPdfToPush);
    let batches = [];

    while (itemsToPush.length) {
      let singleBatch = itemsToPush.splice(0, 20);
      if (singleBatch.length > 0) batches.push(singleBatch);
    }
    let res: boolean = false;
    for (let i = 0; i < batches.length; i++) {
      this._BusyMessage = "Pushing items, batch " + (i + 1).toString() + " of " + batches.length.toString();
      res = await this.PushTheseItemsToZotero(batches[i]);
      if (res == false) {
        //an error happened, we'll stop
        this._BusyMessage = "";
        return false;
      }
    }
    this._BusyMessage = "";
    return true; //new Promise(() => { return true; }); 
  }

  private async PushTheseItemsToZotero(itemsToPush: ZoteroERWebReviewItem[]): Promise<boolean> {
    this._BusyMethods.push("PushZoteroErWebReviewItemList");
    return lastValueFrom(this._httpC.post<boolean>(this._baseUrl + 'api/Zotero/PushZoteroErWebReviewItemList', itemsToPush )
      ).then(result => {
        this.RemoveBusy("PushZoteroErWebReviewItemList");
        return true;
      },
        error => {
          this.RemoveBusy("PushZoteroErWebReviewItemList");
          this.modalService.GenericError(error);
          return false;
        }
      );
  }

  public async PullTheseItems(Items: ZoteroERWebReviewItem[]): Promise<boolean> {

    this._BusyMethods.push("PullTheseItems");
    return lastValueFrom(this._httpC.post<boolean>(this._baseUrl + 'api/Zotero/PullZoteroErWebReviewItemList', Items)
      ).then(result => {
        this.RemoveBusy("PullTheseItems");
        return true;
      },
        error => {
          this.RemoveBusy("PullTheseItems");
          this.modalService.GenericError(error);
          return false;
        }
    ).catch(caught => {
      this.RemoveBusy("PullTheseItems");
      this.modalService.GenericError(caught);
      return false;
    }
    );
  }

  public async CheckZoteroPermissions(): Promise<boolean> {
    this._errorMessage = "data not fetched";
    this.hasPermissions = false;
    const WeHaveTheAPIKey = await this.CheckZoteroApiKey();
    if (WeHaveTheAPIKey == true) {
      this._errorMessage = "";
      this.hasPermissions = true;
    } else this._errorMessage = WeHaveTheAPIKey.toString();
    return this.hasPermissions;
  }
  private CheckZoteroApiKey(): Promise<boolean | string> {
    this._BusyMethods.push("GetZoteroApiKey");
    this.userKeyInfo = <ApiKeyInfo>{};
    this.groupMeta = [];
    return lastValueFrom(this._httpC.get<ApiKeyInfo|string>(this._baseUrl + 'api/Zotero/CheckApiKey')
      ).then(async result => {
        this.RemoveBusy("GetZoteroApiKey");
        if (typeof result == "string") {
          //ApiKeyInfo could not be collected: something is wrong with it!
          await this.GetApiKey();
          return result;
        }
        else {//All is good if the Status value == "OK"
          const aki = result as ApiKeyInfo;
          this.userKeyInfo = result;
          if (aki.status && aki.status != "OK") return aki.status;
          else if (aki.status == undefined || aki.status == null) {
            return "unexpected error";
          }
          else {
            return true;
          }
        }
      },
        error => {
          this.RemoveBusy("GetZoteroApiKey");
          this.modalService.GenericError(error);
          return "CheckZoteroApiKey failed";
        }
      );
  }
  public async GetApiKey(): Promise<void> {
    //used only when we need to get the API Key in order to understand what the problem is!
    this._BusyMethods.push("GetApiKey");
    this.userKeyInfo = <ApiKeyInfo>{};
    this.groupMeta = [];
    return lastValueFrom(this._httpC.get<ApiKeyInfo>(this._baseUrl + 'api/Zotero/GetApiKey')
      ).then(result => {
        this.RemoveBusy("GetApiKey");
        this.userKeyInfo = result;
      }, error => {
        this.RemoveBusy("GetApiKey");
        this.modalService.GenericError(error);
      }).catch(caught => {
        this.RemoveBusy("GetApiKey");
        this.modalService.GenericError(caught);
      });
  }

    public async UpdateGroupToReview(groupId: string, deleteLink: boolean): Promise<boolean> {
    
        this._BusyMethods.push("UpdateGroupToReview");
      return lastValueFrom(this._httpC.post<boolean>(this._baseUrl + 'api/Zotero/UpdateGroupToReview', groupId.toString() )
            ).then(result => {
                this.RemoveBusy("UpdateGroupToReview");
                return true;
            },
                error => {
                    this.RemoveBusy("UpdateGroupToReview");
                    this.modalService.GenericError(error);
                    return false;
                }
            );
  }

  public async fetchGroupMetaData(): Promise<boolean> {
    this._BusyMethods.push("fetchGroupMetaData");
      this.groupMeta = []
    return lastValueFrom(this._httpC.get<Group[]>(this._baseUrl + 'api/Zotero/GroupMetaData')
          ).then(result => {
            if (result.length === 0) {
              console.log('this is zero even though controller returns data!!');
            }
            this.RemoveBusy("fetchGroupMetaData");
            this.groupMeta = result;
            return true;
          },
            error => {
              this.RemoveBusy("fetchGroupMetaData");
              this.modalService.GenericError(error);
              return false;
            }
          ).catch(
            (error) => {
              this.modalService.GenericError(error);
              this.RemoveBusy("fetchGroupMetaData");
              return false;
            });
  }

  public async CheckAndFetchZoteroItems(force:boolean = false): Promise<boolean> {
    if ((!this._BusyMethods.includes("fetchZoteroItems")) && this._ZoteroItems.length == 0 || force == true) {
      return this.fetchZoteroItems();
    }
    else return new Promise(() => { return true; });
  }

  private async fetchZoteroItems(): Promise<boolean> {
    this._BusyMethods.push("fetchZoteroItems");
    this._BusyMessage = "<div class='mx-0 px-0'><div>Getting the Full list of references in the Zotero library. This can take minutes, when there are thousands of references.</div>"
      + "<div class='row'><div class='bg-white text-dark font-weight-bold small px-2 mx-auto'>While this happens, you can go back to the main ER-Web window and return here in a few minutes.</div></div></div>";
    this._ZoteroItems = [];
    return lastValueFrom(this._httpC.get<iZoteroItemsResult>(this._baseUrl + 'api/Zotero/ZoteroItems')
      ).then(result => {
        this.RemoveBusy("fetchZoteroItems");
        this._BusyMessage = "";
        let AttachKeys: string[] = [];//used to figure out what Attachments are in TB_ZOTERO_ITEM_DOCUMENT, but not present on Zotero end, anymore. We'll delete them
        let ToDeleteItems: ZoteroERWebReviewItem[] = [];//used to track what references are in TB_ZOTERO_ITEM_REVIEW, but not present on Zotero end, anymore. We'll delete them
        let ToDeleteAttachments: ZoteroERWebItemDoc[] = [];//used to track what PDFs/Attachments are in TB_ZOTERO_ITEM_DOCUMENT, but not present on Zotero end, anymore. We'll delete them

        //First: digest what's in Zotero
        //in result.zoteroItems we're getting what Zotero provides at /groups/{zrc.LibraryId}/items?sort=title
        //which is jumble of refs and attachments, so we'll digest this result
        //might be better to digest on the server side, or not. Hard to say for now (03 Oct 2022)
        const References = result.zoteroItems.filter(f => f.data.itemType !== 'attachment');//ugh, hopefully this is only references and not also other stuff!
        const Attachments = result.zoteroItems.filter(f => f.data.itemType == 'attachment');
        if (result.zoteroItems.length > 0) {
          this._LibraryName = result.zoteroItems[0].library.name;
        }
        for (let iref of References) {//create the ZoteroItem, add it to our list of zoteroItems
          let ref = new ZoteroItem(iref);
          ref.syncState = SyncState.canPull;//we default to canPull, will change it if/when an item exists in ER-Web (via result.pairedItems)
          this._ZoteroItems.push(ref);
        }
        //now we have all ZoteroItems, but we need to put the attachments in them;
        for (let iAtt of Attachments) {
          AttachKeys.push(iAtt.key);
          let Att = new ZoteroAttachment(iAtt);
          //now find its parent...
          const ind = this._ZoteroItems.findIndex(f => f.key == iAtt.data.parentItem);
          if (ind > -1) {
            //OK, found it, otherwise it's an attachment with no parent and we don't know what to do with it.
            Att.syncState = SyncState.canPull;//default to canPull, will change it if we can't...
            this._ZoteroItems[ind].attachments.push(Att);
          }
        }

        //SECOND, find out all the syncStates for what we have in this._ZoteroItems
        //to do this, we use result.ZoteroERWebReviewItemList, which contain all (review) data from TB_ZOTERO_ITEM_REVIEW and TB_ZOTERO_ITEM_DOCUMENT
        for (let rr of result.pairedItems) {
          let zri = new ZoteroERWebReviewItem(rr);
          let ind = this._ZoteroItems.findIndex(f => f.key == zri.itemKey);
          if (ind > -1) {
            //OK, this item exists on both ends... Can we push, pull or is it uptodate?
            let ZI = this._ZoteroItems[ind];

            ZI.itemId = zri.itemID; //we will need to know the ItemId, when pulling...

            let dateER = new Date(zri.lasT_MODIFIED);
            let dateZT = new Date(ZI.dateModified);
            let dateZTwithOffset = this.AddOffsetTimeToDate(dateZT, 5);//zoteroTime comes with precision to Seconds, ER time to milliseconds.
            //Hence, zoterotime is ~always (999 times out of 1000) less than ER time. We add 5 seconds to zoterotime and then look at intervalse

            if (dateER > dateZTwithOffset) {
              //zri.syncState = SyncState.canPush;
              ZI.syncState = SyncState.canPush;
            }
            else if (dateER < dateZT) {
              //zri.syncState = SyncState.canPull;
              ZI.syncState = SyncState.canPull;
            }
            else {
              //zri.syncState = SyncState.upToDate;
              ZI.syncState = SyncState.upToDate;
            }
            //console.log("state0", zri.syncState, ind);

            //THIRD: given the PDFs in this zri , do they still exist on the Zotero end? If not, we'll delete them
            //otherwise, if they do exist, we can't pull them!
            for (let att of zri.pdfList) {
              if (att.docZoteroKey == "") {
                //the attachment exists on the ER side, BUT NOT on the Zot side
                //nothing to do, but keeping this clause to make the logic "visible"
              }
              else {
                let attInd = AttachKeys.findIndex(f => f == att.docZoteroKey);
                if (attInd == -1) ToDeleteAttachments.push(att);
                else {//attachments in elements of _ZoteroItems are created as canPull, but we can't pull this one, as it exists on both sides
                  for (const zit of this._ZoteroItems) {
                    let zatt = zit.FindAttachmentByZoteroKey(att.docZoteroKey);
                    if (zatt !== null) {
                      zatt.syncState = SyncState.upToDate;
                      break;
                    }
                  }
                }
              }
            }
          }
          else {
            //this one exists in TB_ZOTERO_ITEM_REVIEW but not in Zotero, has been deleted there, we need to delete it from TB_ZOTERO_ITEM_REVIEW
            //console.log("state1", zri.syncState);
            ToDeleteItems.push(zri);
            for (let att of zri.pdfList) {
              ToDeleteAttachments.push(att);
            }
          }
          //console.log("state2", zri.syncState);
          //this._zoteroERWebReviewItemList.push(zri);
        }

        //FINALLY: call the next "fire and forget" method which will DELETE records in TB_ZOTERO_ITEM_REVIEW and TB_ZOTERO_ITEM_DOCUMENT
        //these are records of things that were "paired" (existed both in ER and Zotero sides), but has disappeared on the Zotero end
        //so we want to make sure the ER side is updated accordingly
        if (ToDeleteAttachments.length > 0 || ToDeleteItems.length > 0) {
          this.DeleteEntitiesThatWereDeletedOnZoteroSide(ToDeleteAttachments, ToDeleteItems);
        }
        return true;
      },
        error => {
          this.modalService.GenericError(error);
          this.RemoveBusy("fetchZoteroItems");
          return false;
        }
      ).catch(caught => {
        this.modalService.GenericError(caught);
        this.RemoveBusy("fetchZoteroItems");
        return false;
      });
  }

  private DeleteEntitiesThatWereDeletedOnZoteroSide(ToDeleteAttachments: ZoteroERWebItemDoc[], ToDeleteItems: ZoteroERWebReviewItem[]) {
    //DELETE records in TB_ZOTERO_ITEM_REVIEW and TB_ZOTERO_ITEM_DOCUMENT
    let ItemKeys: string = "", DocKeys: string = "";
    for (let itm of ToDeleteItems) {
      ItemKeys += itm.itemKey + ",";
    }
    ItemKeys = ItemKeys.substring(0, ItemKeys.length - 1);
    for (let doc of ToDeleteAttachments) {
      DocKeys += doc.docZoteroKey + ",";
    }
    DocKeys = DocKeys.substring(0, DocKeys.length - 1);
    this.deleteERZoterolinkstoItemsAndDocs(ItemKeys, DocKeys);
  }
  private deleteERZoterolinkstoItemsAndDocs(ItemKeys: string, DocKeys: string) {
    this._BusyMethods.push("deleteERZoterolinkstoItemsAndDocs");
    const body = { itemKeys: ItemKeys, docKeys: DocKeys };
    return this._httpC.post<boolean>(this._baseUrl + 'api/Zotero/DeleteLinkedDocsAndItems', body).subscribe(
      result => {
        this.RemoveBusy("deleteERZoterolinkstoItemsAndDocs");
      },
      error => {
        this.RemoveBusy("deleteERZoterolinkstoItemsAndDocs");
        this.modalService.GenericError(error);
      }
    );
  }
  private AddOffsetTimeToDate(indate: Date, secsToAdd: number) {
    //snatched from: https://code.tutsplus.com/tutorials/how-to-add-and-subtract-time-from-a-date-in-javascript--cms-37207
    var numberOfMlSeconds = indate.getTime();
    var addMlSeconds = secsToAdd * 1000;
    var newDateObj = new Date(numberOfMlSeconds + addMlSeconds);
    return newDateObj;
  }

  public async fetchZoteroERWebReviewItemListAsync(attributeId: string, sortResultsBy: LocalSort) {
    this._BusyMethods.push("fetchZoteroERWebReviewItemListAsync");
    this._zoteroERWebReviewItemList = [];
    return this._httpC.post<iZoteroERWebReviewItem[]>(this._baseUrl +
      'api/Zotero/FetchZoteroERWebReviewItemList', attributeId)
      .subscribe(result => {
        for (let rr of result) {
          let zri = new ZoteroERWebReviewItem(rr);
          if (zri.itemKey != "") {
            let ind = this._ZoteroItems.findIndex(f => f.key == zri.itemKey);
            if (ind > -1) {
              //OK, this item exists on both ends... Can we push, pull or is it uptodate?
              let ZI = this._ZoteroItems[ind];
              let dateER = new Date(zri.lasT_MODIFIED);
              let dateZT = new Date(ZI.dateModified);
              let dateZTwithOffset = this.AddOffsetTimeToDate(dateZT, 5);//zoteroTime comes with precision to Seconds, ER time to milliseconds.
              //Hence, zoterotime is ~always (999 times out of 1000) less than ER time. We add 5 seconds to zoterotime and then look at intervals

              if (dateER > dateZTwithOffset) {
                zri.syncState = SyncState.canPush;
                zri.version = ZI.version;
                //ZI.syncState = SyncState.canPush;
              }
              else if (dateER < dateZT) {
                zri.syncState = SyncState.canPull;
                //ZI.syncState = SyncState.canPull;
              }
              else {
                zri.syncState = SyncState.upToDate;
                //ZI.syncState = SyncState.upToDate;
              }
              //console.log("state0", zri.syncState, ind);

              //do the same for PDFs/attachments in this zri...
              for (const erDoc of zri.pdfList) {
                if (erDoc.docZoteroKey != "") {
                  for (const zit of this._ZoteroItems) {
                    let zatt = zit.FindAttachmentByZoteroKey(erDoc.docZoteroKey);
                    if (zatt !== null) {
                      erDoc.syncState = SyncState.upToDate;
                      break;
                    }
                  }
                } else {
                  erDoc.syncState = SyncState.canPush;
                }
              }
            } 
          }
          else {
            for (const erDoc of zri.pdfList) {
              erDoc.syncState = SyncState.canPush;
            }
              //console.log("state1", zri.syncState);
              zri.syncState = SyncState.canPush;
            }
          //console.log("state2", zri.syncState);
          this._zoteroERWebReviewItemList.push(zri);
        }
        CustomSorting.DoSort(this._zoteroERWebReviewItemList, sortResultsBy);
        this.RemoveBusy("fetchZoteroERWebReviewItemListAsync");
      },
        error => {
          this.RemoveBusy("fetchZoteroERWebReviewItemListAsync");
          this.modalService.GenericError(error);
        }
      );
  }

  public OauthProcessGet(erWebUserID: number, reviewId: number): Promise<any> {
    this._BusyMethods.push("OauthProcessGet");

    return lastValueFrom(this._httpC.get<any>(this._baseUrl + 'api/Zotero/StartOauthProcess')
      ).then(result => {
        this.RemoveBusy("OauthProcessGet");
        return result;
      },
        error => {
          this.RemoveBusy("OauthProcessGet");
          this.modalService.GenericError(error);
          return error;
        }
      );
  }
  public DeleteZoteroApiKey(): Promise<boolean> {
    this._BusyMethods.push("DeleteZoteroApiKey");

    return lastValueFrom(this._httpC.get<boolean>(this._baseUrl + 'api/Zotero/DeleteZoteroApiKey')
      ).then(result => {
        this.RemoveBusy("DeleteZoteroApiKey");
        if (result == true) {
          this.groupMeta = [];
          this._errorMessage = "No API Key";
          this.hasPermissions = false;
        }
        return result;
      },
        error => {
          this.RemoveBusy("DeleteZoteroApiKey");
          this.modalService.GenericError(error);
          return false;
        }
      );
  }

  ngOnDestroy() {
    console.log("Destroy ZoteroService");
    if (this.clearSub != null) this.clearSub.unsubscribe();
  }
  public PartialClear() {
    console.log("PartialClear in ZoteroService");
    this._zoteroERWebReviewItemList = [];
  }
  public Clear() {
    console.log("Clear in ZoteroService");
    this.PartialClear();
    this._ZoteroItems = [];
    this.groupMeta = [];
    this._errorMessage = "data not fetched";
    this.hasPermissions = false;
    }
}

