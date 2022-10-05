import { EventEmitter, Inject, Injectable, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import {
  ApiKeyInfo, Group, IERWebANDZoteroReviewItem, IERWebObjects, IZoteroReviewItem, iZoteroJobject, ZoteroItem, ZoteroAttachment, ZoteroReviewCollection, ZoteroReviewCollectionList,
  iZoteroERWebReviewItem,
  ZoteroERWebReviewItem,
  SyncState
} from './ZoteroClasses.service';
import { ConfigService } from './config.service';

@Injectable({
    providedIn: 'root',
})

export class ZoteroService extends BusyAwareService {
   
    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
      configService: ConfigService
    ) {
      super(configService);
    }
    
    public currentGroupBeingSynced: number = 0;
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

  private _ZoteroItems: ZoteroItem[] = [];
  public get ZoteroItems(): ZoteroItem[] {
    return this._ZoteroItems;
  }

  private _zoteroERWebReviewItemList: ZoteroERWebReviewItem[] = [];
  public get ZoteroERWebReviewItemList() {
    return this._zoteroERWebReviewItemList;
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
    return this._httpC.get<ApiKeyInfo|string>(this._baseUrl + 'api/Zotero/CheckApiKey')
      .toPromise().then(async result => {
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
    return this._httpC.get<ApiKeyInfo>(this._baseUrl + 'api/Zotero/GetApiKey')
      .toPromise().then(result => {
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
      return this._httpC.post<boolean>(this._baseUrl + 'api/Zotero/UpdateGroupToReview?deleteLink=' + deleteLink.toString(), groupId.toString() )
            .toPromise().then(result => {
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

    public async FetchGroupToReviewLinks(): Promise<ZoteroReviewCollectionList> {
      
        this._BusyMethods.push("FetchGroupToReviewLinks");
        return this._httpC.get<ZoteroReviewCollection[]>(this._baseUrl + 'api/Zotero/FetchGroupToReviewLinks')
            .toPromise().then(result => {
                let zoteroReviewCollectionList = new ZoteroReviewCollectionList();
                for (var i = 0; i < result.length; i++) {
                    zoteroReviewCollectionList.ZoteroReviewCollectionList.push(result[i]);
                }                
                this.RemoveBusy("FetchGroupToReviewLinks");
                return zoteroReviewCollectionList;
            },
                error => {
                    this.RemoveBusy("FetchGroupToReviewLinks");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
  }

    public async GroupMemberGet(groupId: string): Promise<boolean> {
        this._BusyMethods.push("GroupMemberGet");
        return this._httpC.get<boolean>(this._baseUrl + 'api/Zotero/GroupMember?groupId=' + groupId )
            .toPromise().then(result => {
                this.RemoveBusy("GroupMemberGet");
                return result;
            },
                error => {
                    this.RemoveBusy("GroupMemberGet");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public fetchGroupMetaData(): Promise<Group[]> {
        
        this._BusyMethods.push("fetchGroupMetaData");
      this.groupMeta = []
        return this._httpC.get<Group[]>(this._baseUrl + 'api/Zotero/GroupMetaData')
            .toPromise().then(result => {
                if (result.length === 0) {
                    console.log('this is zero even though controller returns data!!');
                }
              this.RemoveBusy("fetchGroupMetaData");
              this.groupMeta = result;
                return result;
            },
                error => {
                    this.RemoveBusy("fetchGroupMetaData");
                    this.modalService.GenericError(error);
                    return error;
                }
        ).catch(
            (error) => {

                console.log('some error!');
                this.RemoveBusy("fetchGroupMetaData");
            });
    }

    public async MarkGroupForSync(groupId: number): Promise<boolean> {
      this._BusyMethods.push("MarkGroupForSync");     

      return this._httpC.post<number>(this._baseUrl + 'api/Zotero/MarkGroupForSync', groupId
        )
            .toPromise().then(result => {
              this.RemoveBusy("MarkGroupForSync");
                return result;
            },
                error => {
                  this.RemoveBusy("MarkGroupForSync");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public async fetchERWebAndZoteroAlreadySyncedItems(): Promise<IERWebANDZoteroReviewItem[]> {
        this._BusyMethods.push("fetchERWebAndZoteroAlreadySyncedItems");
        return this._httpC.get<IERWebANDZoteroReviewItem[]>(this._baseUrl + 'api/Zotero/ItemsERWebAndZotero')
            .toPromise().then(result => {
                this.RemoveBusy("fetchERWebAndZoteroAlreadySyncedItems");
                console.log('got the synced items back: ', result);
                return result;
            },
                error => {
                    this.RemoveBusy("fetchERWebAndZoteroAlreadySyncedItems");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public async deleteMiddleMan(itemKey: string): Promise<string> {
        this._BusyMethods.push("deleteMiddleMan");
        return this._httpC.post<string>(this._baseUrl + 'api/Zotero/DeleteMiddleMan?itemKey=' + itemKey, itemKey)
            .toPromise().then(result => {
                this.RemoveBusy("deleteMiddleMan");
                return result;
            },
                error => {
                    this.RemoveBusy("deleteMiddleMan");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public  fetchZoteroObjectVersionsAsync() {
      this._BusyMethods.push("fetchZoteroObjectVersionsAsync");
      this._ZoteroItems = [];
      return this._httpC.get<iZoteroJobject[]>(this._baseUrl + 'api/Zotero/ZoteroItems')
        .subscribe(result => {
          //we're getting what Zotero provides at /groups/{zrc.LibraryId}/items?sort=title
          //which is jumble of refs and attachments, so we'll digest this result
          //might be better to digest on the server side, or not. Hard to say for now (03 Oct 2022)
          const References = result.filter(f => f.data.itemType !== 'attachment');//ugh, hopefully this is only references and not also other stuff!
          const Attachments = result.filter(f => f.data.itemType == 'attachment');
          for (let iref of References) {//create the ZoteroItem, add it to our list of zoteroItems
            let ref = new ZoteroItem(iref);
            this._ZoteroItems.push(ref);
          }
          //now we have all ZoteroItems, but we need to put the attachments in them;
          for (let iAtt of Attachments) {
            let Att = new ZoteroAttachment(iAtt);
            //now find its parent...
            const ind = this._ZoteroItems.findIndex(f => f.key == iAtt.data.parentItem);
            if (ind > -1) {
              //OK, found it, otherwise it's an attachment with no parent and we don't know what to do with it.
              this._ZoteroItems[ind].attachments.push(Att);
            }
          }

          this.RemoveBusy("fetchZoteroObjectVersionsAsync");
        },
          error => {
            this.RemoveBusy("fetchZoteroObjectVersionsAsync");
            this.modalService.GenericError(error);
            return error;
          }
        );
    }

    public async fetchERWebObjectsNotInZoteroAsync(): Promise<IERWebObjects[]> {
        this._BusyMethods.push("fetchERWebObjectNotInZoteroAsync");
  
        return this._httpC.get<IERWebObjects[]>(this._baseUrl + 'api/Zotero/ItemReviewIdsLocal')
            .toPromise().then(result => {
                this.RemoveBusy("fetchERWebObjectNotInZoteroAsync");
                return result;
            },
                error => {
                    this.RemoveBusy("fetchERWebObjectNotInZoteroAsync");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

  public async getVersionOfItemInErWebAsync(item: iZoteroJobject) {
        this._BusyMethods.push("getVersionOfItemInErWeb");
        return this._httpC.get<IZoteroReviewItem>(this._baseUrl + 'api/Zotero/ItemKeyVersionLocal?ItemKey=' + item.key + '&ItemType=' + item.data.itemType)
            .toPromise().then(result => {
                this.RemoveBusy("getVersionOfItemInErWeb");
                
                return result;
            },
                error => {
                    this.RemoveBusy("getVersionOfItemInErWeb");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

  public async fetchZoteroObjectAsync(itemKey: string): Promise<iZoteroJobject> {
        this._BusyMethods.push("fetchZoteroObjectAsync");

    return this._httpC.get<iZoteroJobject>(this._baseUrl + 'api/Zotero/ItemsItemKey?itemKey=' + itemKey)
            .toPromise().then(result => {
                this.RemoveBusy("fetchZoteroObjectAsync");
                return result;
            },
                error => {
                    this.RemoveBusy("fetchZoteroObjectAsync");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
  }

  public async fetchZoteroERWebReviewItemListAsync(attributeId: string) {
    this._BusyMethods.push("fetchZoteroERWebReviewItemListAsync");
    this._zoteroERWebReviewItemList = [];
    return this._httpC.get<iZoteroERWebReviewItem[]>(this._baseUrl +
      'api/Zotero/FetchZoteroERWebReviewItemList?attributeId=' + attributeId)
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
              if (dateER > dateZT) {
                zri.syncState = SyncState.canPush;
                ZI.syncState = SyncState.canPull;
              }
              else if (dateER < dateZT) {
                zri.syncState = SyncState.canPull;
                ZI.syncState = SyncState.canPush;
              }
              else {
                zri.syncState = SyncState.upToDate;
                ZI.syncState = SyncState.upToDate;
              }
              console.log("state0", zri.syncState, ind);
            } 
          }
          else {
              console.log("state1", zri.syncState);
              zri.syncState = SyncState.canPush;
            }
          console.log("state2", zri.syncState);
          this._zoteroERWebReviewItemList.push(zri);
        }
        this.RemoveBusy("fetchZoteroERWebReviewItemListAsync");
      },
        error => {
          this.RemoveBusy("fetchZoteroERWebReviewItemListAsync");
          this.modalService.GenericError(error);
        }
      );
  }

  public async updateZoteroObjectInERWebAsync(item: iZoteroJobject): Promise<boolean> {
        this._BusyMethods.push("updateZoteroObjectInERWebAsync");

    return this._httpC.post<iZoteroJobject>(this._baseUrl + 'api/Zotero/ItemsItemsIdLocal', item)
            .toPromise().then(result => {
                this.RemoveBusy("updateZoteroObjectInERWebAsync");
                return result;
            },
                error => {
                    this.RemoveBusy("updateZoteroObjectInERWebAsync");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

  public async insertZoteroObjectIntoERWebAsync(items: iZoteroJobject[]): Promise<boolean> {
      this._BusyMethods.push("insertZoteroObjectInERWebAsync");       
    return this._httpC.post<iZoteroJobject[]>(this._baseUrl + 'api/Zotero/ItemsLocal', items)
            .toPromise().then(result => {
                this.RemoveBusy("insertZoteroObjectInERWebAsync");
                return result;
            },
                error => {
                    this.RemoveBusy("insertZoteroObjectInERWebAsync");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public async fetchERWebAttachmentState(itemReviewId: string): Promise<boolean> {
        this._BusyMethods.push("fetchERWebAttachmentState");

        return this._httpC.get<boolean>(this._baseUrl + 'api/Zotero/ErWebDocumentExists?itemReviewId=' + itemReviewId)
            .toPromise().then(result => {
                this.RemoveBusy("fetchERWebAttachmentState");
                return result;
            },
                error => {
                    this.RemoveBusy("fetchERWebAttachmentState");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public async fetchERWebItemsToPushToZotero(itemIds: string): Promise<string[]> {
        this._BusyMethods.push("fetchERWebItemsToPushToZotero");

        return this._httpC.get<string[]>(this._baseUrl + 'api/Zotero/ItemReviewIds?itemIds=' + itemIds)
            .toPromise().then(result => {
                this.RemoveBusy("fetchERWebItemsToPushToZotero");
                return result;
            },
                error => {
                    this.RemoveBusy("fetchERWebItemsToPushToZotero");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public async fetchItemIDPerItemReviewID(itemReviewID: string): Promise<string> {
        this._BusyMethods.push("fetchItemIDPerItemReviewID");

        return this._httpC.get<string>(this._baseUrl + 'api/Zotero/ItemIdLocal?itemReviewID=' + itemReviewID)
            .toPromise().then(result => {
                this.RemoveBusy("fetchItemIDPerItemReviewID");
                return result;
            },
                error => {
                    this.RemoveBusy("fetchItemIDPerItemReviewID");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public async postERWebItemsToZotero(items: string[]): Promise<string> {
        this._BusyMethods.push("postERWebItemsToZotero");

        return this._httpC.post<string>(this._baseUrl + 'api/Zotero/GroupsGroupIdItems', items)
            .toPromise().then(result => {
                this.RemoveBusy("postERWebItemsToZotero");
                return result;
            },
                error => {
                    this.RemoveBusy("postERWebItemsToZotero");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public async updateERWebItemsInZotero(items: IERWebObjects[]): Promise<boolean> {
        this._BusyMethods.push("updateERWebItemsInZotero");
      console.log('data in the service is like so: ' + JSON.stringify(items));
        return this._httpC.put<IERWebObjects[]>(this._baseUrl + 'api/Zotero/UpdateERWebItemsInZoteroAsync', items)
            .toPromise().then(result => {
                this.RemoveBusy("updateERWebItemsInZotero");
                return result;
            },
                error => {
                    this.RemoveBusy("updateERWebItemsInZotero");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public OauthProcessGet(erWebUserID: number, reviewId: number): Promise<any> {
        this._BusyMethods.push("OauthProcessGet");

        return this._httpC.get<any>(this._baseUrl + 'api/Zotero/StartOauthProcess')
            .toPromise().then(result => {
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

    return this._httpC.get<boolean>(this._baseUrl + 'api/Zotero/DeleteZoteroApiKey')
      .toPromise().then(result => {
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

    //public async CollectionPost(collection: string): Promise<any> {
    //  this._BusyMethods.push("Collection");

    //    return this._httpC.post<any>(this._baseUrl + 'api/Zotero/Collection', collection)
    //        .toPromise().then(result => {
    //          this.RemoveBusy("Collection");
    //            return result;
    //        },
    //            error => {
    //              this.RemoveBusy("Collection");
    //                this.modalService.GenericError(error);
    //                return error;
    //            }
    //        );
    //}

    public async GetZoteroCollections(zoteroApiKey: string):Promise<number> {
        this._BusyMethods.push("GetZoteroCollections");
        return this._httpC.get<number>(this._baseUrl + 'api/Zotero/GetZoteroCollectionsAsync?key=' + zoteroApiKey)
            .toPromise().then(result => {
                this.RemoveBusy("GetZoteroCollections");
                return result;
            },
                error => {
                    this.RemoveBusy("GetZoteroCollections");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

  public Clear() {
    this.groupMeta = [];
    this._errorMessage = "data not fetched";
    this.hasPermissions = false;
    }
}

