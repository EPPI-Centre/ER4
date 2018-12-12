import { Component, Inject, Injectable } from '@angular/core';
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

    public _itemDocs: ItemDocument[] = []; 
   

    public FetchDocList(itemID: number) {

        let body = JSON.stringify({ Value: itemID });
        return this._httpC.post<ItemDocument[]>(this._baseUrl + 'api/ItemDocumentList/GetDocuments', body).subscribe(
            (res) => { this._itemDocs = res }
            , error => { this.modalService.GenericError(error); }
        );
    }


    public GetItemDocument(itemDocumentId: number) {
        
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
                            let url = URL.createObjectURL(blob);
                            if (url) window.open(url);
                        });
                }
            });

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
    public isBusy: boolean = false;
    public isChild: boolean = false;
    public isDeleted: boolean = false;
    public isDirty: boolean = false;
    public isNew: boolean = false;
    public isSavable: boolean = false;
    public isSelfBusy: boolean = false;
    public isSelfDirty: boolean = false;
    public isSelfValid: boolean = false;
    public isValid: boolean = false;

}