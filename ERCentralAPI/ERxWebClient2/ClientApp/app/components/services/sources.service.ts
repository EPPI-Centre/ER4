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
import { arm, Item, ItemListService } from './ItemList.service';

@Injectable({
    providedIn: 'root',
})

export class SourcesService {

    constructor(
        private _http: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
       
    }
    private _IncomingItems4Checking: IncomingItemsList | null = null;
    public get IncomingItems4Checking(): IncomingItemsList | null {
        return this._IncomingItems4Checking;
    }
    @Output() gotItems4Checking = new EventEmitter();
    public Fetchsources() {
       // let body = JSON.stringify({ Value: currentItem.itemId });

       //this._http.post<arm[]>(this._baseUrl + 'api/ItemSetList/GetArms',

       //    body).subscribe(result => {
       //        this.arms = result;
       //        currentItem.arms = this.arms;
       //        this._selectedArm = null;
       //        this.gotArms.emit(this.arms);
       //        //this.Save();
       //     }, error => { this.modalService.SendBackHomeWithError(error); }
       // );
       // return currentItem.arms;
    }
    public CheckUpload(Filter_Name: string, File_Content: string) {
        console.log('CheckUpload');
        this._IncomingItems4Checking = null;
        let body = JSON.stringify({
            FilterName: Filter_Name,
            FileContent: File_Content
        });
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/UploadSource/VerifyFile',
            body).subscribe(result => {
                this._IncomingItems4Checking = result;
            }, error => {
                this.modalService.SendBackHomeWithError(error);
            },
            () => { this.gotItems4Checking.emit();}
            );
    }
       
}
export interface IncomingItemsList {
    totalReferences: number;
    incomingItems: IncomingItem[];
}
export interface IncomingItem {
    title: string;
    parentTitle: string;
    authors: string;
    year: string;
}