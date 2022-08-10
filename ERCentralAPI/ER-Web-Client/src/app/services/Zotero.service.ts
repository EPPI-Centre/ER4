import { EventEmitter, Inject, Injectable, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Collection, Group, IERWebANDZoteroReviewItem, IERWebObjects, IZoteroReviewItem, TypeCollection, UserKey, UserSubscription, ZoteroReviewCollection, ZoteroReviewCollectionList } from './ZoteroClasses.service';
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
    @Output() SendGroupSelectedToZoteroSync = new EventEmitter<string>();
    public currentGroupBeingSynced: number = 0;
    public editApiKeyPermissions: boolean = false;
    public hasPermissions: boolean = false;
    public get ZoteroPermissions() {
            return this.userKeyInfo;
    }
    public set ZoteroPermissions(value: UserKey) {
        this.userKeyInfo = value;
    }
    private userKeyInfo: UserKey = <UserKey>{};
    public async RemoveApiKey(apiKey: UserKey, userId: number, currentReview: number): Promise<boolean> {

        this._BusyMethods.push("RemoveApiKey");
        return this._httpC.delete<boolean>(this._baseUrl + 'api/Zotero/DeleteZoteroApiKey?userId='
            + userId.toString() + '&reviewId=' + currentReview)
            .toPromise().then(result => {                
                this.RemoveBusy("RemoveApiKey");
                return result;
            },
                error => {
                    this.RemoveBusy("RemoveApiKey");
                    this.modalService.GenericError(error);
                    return error;
                }
            );

  }

    public async UpdateGroupToReview(reviewId: string, groupId: string, userId: string, deleteLink: boolean): Promise<ZoteroReviewCollection> {
    
        this._BusyMethods.push("UpdateGroupToReview");
        return this._httpC.post<ZoteroReviewCollection>(this._baseUrl + 'api/Zotero/UpdateGroupToReview?reviewId=' + reviewId + '&userId=' + userId.toString() + '&deleteLink=' + deleteLink.toString(), groupId.toString() )
            .toPromise().then(result => {
                this.RemoveBusy("UpdateGroupToReview");
                return true;
            },
                error => {
                    this.RemoveBusy("UpdateGroupToReview");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }
    public async FetchGroupToReviewLinks(reviewId: string,  userId: string): Promise<ZoteroReviewCollectionList> {
      
        this._BusyMethods.push("FetchGroupToReviewLinks");
        return this._httpC.get<ZoteroReviewCollection[]>(this._baseUrl + 'api/Zotero/FetchGroupToReviewLinks?reviewId=' + reviewId + '&userId=' + userId.toString())
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


    public async fetchApiKeys(reviewId: number, userId: number): Promise<UserKey[]>  {

        this._BusyMethods.push("fetchApiKeys");
        return this._httpC.get<UserKey[]>(this._baseUrl + 'api/Zotero/FetchApiKeys?reviewId=' + reviewId + '&userId=' + userId.toString())
            .toPromise().then(result => {
                this.RemoveBusy("fetchApiKeys");
                return result;
            },
                error => {
                    this.RemoveBusy("fetchApiKeys");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public FetchUserZoteroSubscriptions(userId: number, currentReview: number) {
        this._BusyMethods.push("FetchUserZoteroSubscriptions");
        return this._httpC.get<UserSubscription>(this._baseUrl + 'api/Zotero/Usersubscription?userId=' + userId.toString() + '&reviewId=' + currentReview)
            .toPromise().then(result => {
                
                this.RemoveBusy("FetchUserZoteroSubscriptions");
                return result;
            },
                error => {
                    this.RemoveBusy("FetchUserZoteroSubscriptions");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public async fetchUserZoteroPermissions(userID: number, reviewId: number): Promise<UserKey>  {
        this._BusyMethods.push("fetchUserZoteroPermissions");
        return this._httpC.get<UserKey>(this._baseUrl + 'api/Zotero/UserPermissions?userId='+userID.toString() + '&reviewId=' + reviewId)
            .toPromise().then(result => {
                this.RemoveBusy("fetchUserZoteroPermissions");
                this.userKeyInfo = result;
                return result;
            },
                error => {
                    this.RemoveBusy("fetchUserZoteroPermissions");
                    this.modalService.GenericError(error);
                    return error;
                }
        );
    }

    public async GroupMemberGet(groupId: string, reviewId: number): Promise<boolean> {
        this._BusyMethods.push("GroupMemberGet");
        return this._httpC.get<boolean>(this._baseUrl + 'api/Zotero/GroupMember?groupId=' + groupId + '&userId=' + this.userKeyInfo.userID
            + '&reviewId=' + reviewId)
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

    public fetchGroupMetaData(userId: number, reviewId: number): Promise<Group[]> {
        
        this._BusyMethods.push("fetchGroupMetaData");

        return this._httpC.get<Group[]>(this._baseUrl + 'api/Zotero/GroupMetaData?zoteroUserId=' + this.userKeyInfo.userID + '&userId=' + userId.toString()
            + '&reviewId=' + reviewId )
            .toPromise().then(result => {
                if (result.length === 0) {
                    console.log('this is zero even though controller returns data!!');
                }
                this.RemoveBusy("fetchGroupMetaData");
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

    public async postGroupMetaData(groupId: number, userId: number, reviewId: number): Promise<boolean> {
      this._BusyMethods.push("GroupId");
      var GroupPayload = { 'groupId': groupId.toString(), 'userId': userId.toString(), 'reviewId': reviewId.toString() };

      return this._httpC.post<number>(this._baseUrl + 'api/Zotero/GroupId', GroupPayload
        )
            .toPromise().then(result => {
              this.RemoveBusy("GroupId");
                return result;
            },
                error => {
                  this.RemoveBusy("GroupId");
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

    public async fetchZoteroObjectVersionsAsync(userId: number, reviewId: number): Promise<TypeCollection[]> {
        this._BusyMethods.push("fetchZoteroObjectVersionsAsync");
 
        return this._httpC.get<TypeCollection[]>(this._baseUrl + 'api/Zotero/Items?userId=' + userId.toString() + '&reviewId=' + reviewId)
            .toPromise().then(result => {
                this.RemoveBusy("fetchZoteroObjectVersionsAsync");
                console.log('zotero items in service: ', result);
                return result;
            },
                error => {
                    this.RemoveBusy("fetchZoteroObjectVersionsAsync");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public async fetchERWebObjectsNotInZoteroAsync(reviewID: number): Promise<IERWebObjects[]> {
        this._BusyMethods.push("fetchERWebObjectNotInZoteroAsync");
  
        return this._httpC.get<IERWebObjects[]>(this._baseUrl + 'api/Zotero/ItemReviewIdsLocal?reviewID=' + reviewID)
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

    public async getVersionOfItemInErWebAsync(item: Collection) {
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

    public async fetchZoteroObjectAsync(itemKey: string, userId: number, reviewId: number): Promise<TypeCollection> {
        this._BusyMethods.push("fetchZoteroObjectAsync");

        return this._httpC.get<TypeCollection>(this._baseUrl + 'api/Zotero/ItemsItemKey?itemKey=' + itemKey + '&userId=' + userId.toString()
            + '&reviewId=' + reviewId)
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

    public async updateZoteroObjectInERWebAsync(item: Collection): Promise<boolean> {
        this._BusyMethods.push("updateZoteroObjectInERWebAsync");

        return this._httpC.post<Collection>(this._baseUrl + 'api/Zotero/ItemsItemsIdLocal', item)
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

    public async insertZoteroObjectIntoERWebAsync(items: TypeCollection[], userId: string, reviewId: number): Promise<boolean> {
        this._BusyMethods.push("insertZoteroObjectInERWebAsync");

        return this._httpC.post<TypeCollection[]>(this._baseUrl + 'api/Zotero/ItemsLocal?userId=' + userId.toString() + '&reviewId=' + reviewId + '', items)
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

    public async postERWebItemsToZotero(items: IERWebObjects[], userId: number, reviewId: number): Promise<string> {
        this._BusyMethods.push("postERWebItemsToZotero");

        if (items.length === 0) {
            console.log('The number of items to post is zero!');
            return 'The number of items to post is zero!';
        }

        return this._httpC.post<string>(this._baseUrl + 'api/Zotero/GroupsGroupIdItems?userId=' + userId.toString() + '&reviewId=' + reviewId, items)
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

        return this._httpC.get<any>(this._baseUrl + 'api/Zotero/OauthProcess?erWebUserID=' + erWebUserID + "&reviewID=" + reviewId )
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

    public GetZoteroApiKey(reviewId: number, userId: number): Promise<string> {
        this._BusyMethods.push("GetZoteroApiKey");

        return this._httpC.get<string>(this._baseUrl + 'api/Zotero/ApiKey?reviewId=' + reviewId + '&userId=' + userId )
            .toPromise().then(result => {
                this.RemoveBusy("GetZoteroApiKey");
                return result;
            },
                error => {
                    this.RemoveBusy("GetZoteroApiKey");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public DeleteZoteroApiKey(reviewId: number, userId: number, groupId:number): Promise<string> {
        this._BusyMethods.push("DeleteZoteroApiKey");

        return this._httpC.get<string>(this._baseUrl + 'api/Zotero/ApiKey?reviewId=' + reviewId + '&userId=' + userId
            + '&deleteApiKey=true' + '&groupId=' + groupId)
            .toPromise().then(result => {
                this.RemoveBusy("DeleteZoteroApiKey");
                return result;
            },
                error => {
                    this.RemoveBusy("DeleteZoteroApiKey");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

    public async CollectionPost(collection: string, userId: number, reviewId: number): Promise<any> {
      this._BusyMethods.push("Collection");

        return this._httpC.post<any>(this._baseUrl + 'api/Zotero/Collection?userId=' + userId + 'reviewId=' + reviewId +'', collection)
            .toPromise().then(result => {
              this.RemoveBusy("Collection");
                return result;
            },
                error => {
                  this.RemoveBusy("Collection");
                    this.modalService.GenericError(error);
                    return error;
                }
            );
    }

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
        //this._ZoteroList = [];
    }
}

