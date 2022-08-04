import { Component, Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { OK } from 'http-status-codes';
import { error } from '@angular/compiler/src/util';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ConfigService } from './config.service';

@Injectable({
    providedIn: 'root',
})

export class ItemDocsService extends BusyAwareService   {

    constructor(
        private _httpC: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
      configService: ConfigService
    ) {
      super(configService);
    }
    @Output() GotDocument = new EventEmitter();
    public _itemDocs: ItemDocument[] = []; 
    private currentItemId: number = 0;
    private currentDocBin: Blob | null = null;
    private currentDocBinId: number = 0;
    public get CurrentDoc() {
        return this.currentDocBin;
    }
    public get CurrentDocId(): number {
        return this.currentDocBinId;
    }
    public get CurrentItemId(): number {
        return this.currentItemId;
    }
    public FetchDocList(itemID: number) {
        console.log("FetchDocList");
        if (this.currentItemId != itemID) {
            this.currentDocBin = null;
            this.currentDocBinId = 0;
            this.currentItemId = itemID;
        }
        this.Refresh();
    }
    public Refresh() {
        if (this.currentItemId == 0) return;
        let body = JSON.stringify({ Value: this.currentItemId });
        this._BusyMethods.push("Refresh");
        this._httpC.post<ItemDocument[]>(this._baseUrl + 'api/ItemDocumentList/GetDocuments', body).subscribe(
            (res) => {
                this._itemDocs = res;
                this.RemoveBusy("Refresh");
            }
            , error => {
                this.modalService.GenericError(error);
                this.RemoveBusy("Refresh");
            }
        );
    }
    public Clear() {
        console.log("ItemDocs Clear");
        this._itemDocs = [];
        this.currentDocBin = null;
        this.currentDocBinId = 0;
        this.currentItemId = 0;
    }
    public GetItemDocument(itemDocumentId: number, ForView:boolean = false) {
        this.currentDocBin = null;
        this.currentDocBinId = 0;
        this._BusyMethods.push("GetItemDocument");
        let params = new HttpParams();
        params = params.append('itemDocumentId', itemDocumentId.toString());
        //console.log(this.ReviewerIdentityService.reviewerIdentity.token);
        let requestHeaders: any = { Authorization: `Bearer ${this.ReviewerIdentityService.reviewerIdentity.token}` };

        fetch(this._baseUrl + 'api/ItemDocumentList/GetItemDocument?ItemDocumentId=' + itemDocumentId, {
            
            headers: requestHeaders
            
        })
            .then(response => {
                
                if (response.status >= 200 && response.status < 300) {
                    response.blob().then(
                        blob => {
                            if (ForView) {
                                this.currentDocBin = blob;
                                this.currentDocBinId = itemDocumentId;
                                this.GotDocument.emit();
                            }
                            else {
                                //if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                                //    window.navigator.msSaveOrOpenBlob(blob);
                                //}
                                //else {
                                    URL.createObjectURL(blob);
                                    let url = URL.createObjectURL(blob);
                                    if (url) window.open(url);
                                //}
                            }
                        });
                    this.RemoveBusy("GetItemDocument");
                }
			});
		this.RemoveBusy("GetItemDocument");
    }

    public DeleteDocWarning(DocId: number) {

		this._BusyMethods.push("DeleteDocWarning");
        let ErrMsg = "Something went wrong when checking if it's safe to delete this document. \r\n If the problem persists, please contact EPPISupport.";
        let body = JSON.stringify({ Value: DocId });

        return this._httpC.post<number>(this._baseUrl + 'api/ItemDocumentList/DeleteDocWarning', body).toPromise()
            .then(
			(result) => {
				this.RemoveBusy("DeleteDocWarning");
                    return result;
                }
                , (error) => {
                    console.log('error in DeleteDocWarning() rejected', error);
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteDocWarning");
                    return -1;
                }
            )
            .catch(
                (error) => {
                    console.log('error in DeleteDocWarning() catch', error);
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteDocWarning");
                    return -1;
                }
            );

    }

    DeleteItemDoc(ID: number) {

		this._BusyMethods.push("DeleteItemDoc");
        let ErrMsg = "Something went wrong when deleting the document. \r\n If the problem persists, please contact EPPISupport.";
        let body = JSON.stringify({ Value: ID });
        this._httpC.post(this._baseUrl + 'api/ItemDocumentList/DeleteDoc', body).subscribe(
                (result) => {
					console.log(result);
					this.Refresh();
					this.RemoveBusy("DeleteItemDoc");
                }
                , (error) => {
                    this.modalService.GenericErrorMessage(ErrMsg);
                    console.log(error);
					this.Refresh();
					this.RemoveBusy("DeleteItemDoc");
                }
            );

    }
    
    //public Save() {
    //    if (this._itemDocs != null)
    //        localStorage.setItem('ItemDocumentList', JSON.stringify(this._itemDocs));
    //    else if (localStorage.getItem('ItemDocumentList'))//to be confirmed!! 
    //        localStorage.removeItem('ItemDocumentList');
    //}
    
}


export class ItemDocumentList {

    ItemDocuments: ItemDocument[] = [];
}

export class ItemDocument {

    public itemDocumentId: number = 0;
    public itemId: number = 0;
    public shortTitle: string = '';
    public extension: string = '';
    public title: string = '';
    public text: string = "";
    public binaryExists: boolean = false;
    public textFrom: number = 0;
    public textTo: number = 0;
    public freeNotesStream: string = "";
    public freeNotesXML: string = '';
    //public isBusy: boolean = false;
    //public isChild: boolean = false;
    //public isDeleted: boolean = false;
    //public isDirty: boolean = false;
    //public isNew: boolean = false;
    //public isSavable: boolean = false;
    //public isSelfBusy: boolean = false;
    //public isSelfDirty: boolean = false;
    //public isSelfValid: boolean = false;
    //public isValid: boolean = false;

}
