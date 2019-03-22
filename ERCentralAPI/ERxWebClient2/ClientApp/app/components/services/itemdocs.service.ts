import { Component, Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { OK } from 'http-status-codes';
import { error } from '@angular/compiler/src/util';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';

@Injectable({
    providedIn: 'root',
})

export class ItemDocsService {

    constructor(
        private _httpC: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
       
    }
    @Output() GotDocument = new EventEmitter();
    public _itemDocs: ItemDocument[] = []; 
    private currentItemId: number = 0;
    private currentDocBin: Blob | null = null;
    private currentDocBinId: number = 0;
    public get CurrentDoc() {
        return this.currentDocBin;
    }

    public FetchDocList(itemID: number) {
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
        this._httpC.post<ItemDocument[]>(this._baseUrl + 'api/ItemDocumentList/GetDocuments', body).subscribe(
            (res) => { this._itemDocs = res }
            , error => { this.modalService.GenericError(error); }
        );
    }

    public GetItemDocument(itemDocumentId: number, ForView:boolean = false) {
        this.currentDocBin = null;
        this.currentDocBinId = 0;
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
                                if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                                    window.navigator.msSaveOrOpenBlob(blob);
                                }
                                else {
                                    URL.createObjectURL(blob);
                                    let url = URL.createObjectURL(blob);
                                    if (url) window.open(url);
                                }
                            }
                        });
                }
            });
    }

    public DeleteDocWarning(DocId: number) {

        let ErrMsg = "Something went wrong when checking if it's safe to delete this document. \r\n If the problem persists, please contact EPPISupport.";
        let body = JSON.stringify({ Value: DocId });

        return this._httpC.post<number>(this._baseUrl + 'api/ItemDocumentList/DeleteDocWarning', body).toPromise()
            .then(
                (result) => {
                    return result;
                }
                , (error) => {
                    console.log('error in DeleteDocWarning() rejected', error);
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return -1;
                }
            )
            .catch(
                (error) => {
                    console.log('error in DeleteDocWarning() catch', error);
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return -1;
                }
            );

    }

    DeleteItemDoc(ID: number) {

        let ErrMsg = "Something went wrong when deleting the document. \r\n If the problem persists, please contact EPPISupport.";
        let body = JSON.stringify({ Value: ID });
        this._httpC.post(this._baseUrl + 'api/ItemDocumentList/DeleteDoc', body).subscribe(
                (result) => {
                    console.log(result);
                    this.Refresh();
                }
                , (error) => {
                    this.modalService.GenericErrorMessage(ErrMsg);
                    console.log(error);
                    this.Refresh();
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