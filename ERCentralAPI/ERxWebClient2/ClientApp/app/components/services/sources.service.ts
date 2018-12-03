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
import { formatDate } from '@angular/common';

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
    private _ImportFilters: ImportFilter[] = [];
    public get ImportFilters(): ImportFilter[] {
        return this._ImportFilters;
    }
    @Output() gotItems4Checking = new EventEmitter();
    @Output() SourceUploaded = new EventEmitter();
    private _ReviewSources: ReadOnlySource[] = [];
    public get ReviewSources(): ReadOnlySource[] {
        return this._ReviewSources;
    }
    public FetchSources() {
        this._http.get<ReadOnlySourcesList>(this._baseUrl + 'api/Sources/GetSources').subscribe(result => {
            this._ReviewSources = result.sources;
        }, error => { this.modalService.GenericErrorMessage(error); }
        );
    }
    public FetchImportFilters() {
        this._http.get<ImportFilter[]>(this._baseUrl + 'api/Sources/GetImportFilters').subscribe(result => {
            this._ImportFilters = result;
             }, error => { this.modalService.GenericErrorMessage(error); }
         );
    }
    
    public CheckUpload(data: SourceForUpload) {
        console.log('CheckUpload');
        this._IncomingItems4Checking = null;
        let body = JSON.stringify(data);
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/VerifyFile',
            body).subscribe(
                result => {
                    this._IncomingItems4Checking = result;
            }, error => {
                this.modalService.GenericErrorMessage(error);
            },
            () => { this.gotItems4Checking.emit(); }
            );
    }
    public Upload(data: SourceForUpload) {
        console.log('CheckUpload');
        this._IncomingItems4Checking = null;
        let body = JSON.stringify(data);
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/UploadSource',
            body).subscribe(result => {
                 this.FetchSources()
                 this.SourceUploaded.emit();
            }, error => {
                this.modalService.GenericErrorMessage(error);
            },
                () => {  }
            );
    }
    
    public DeleteUndeleteSource(ros: ReadOnlySource) {
        let body = JSON.stringify({ Value: ros.source_ID });
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/DeleteUndeleteSource',
        body).subscribe(result => {
            this.FetchSources()
        }, error => {
            this.modalService.GenericErrorMessage(error);
        },
            () => { }
        );
    }
    newSourceForUpload(FileContent: string, Source_Name: string, ImportFilterName: string,
                        DateOfSerach?: string, DateOfImport?: string, SourceDataBase?: string,
                        SearchDescription?: string, SearchString?: string, Notes?: string): SourceForUpload {
        if (!DateOfImport) DateOfImport = new Date().toJSON().slice(0, 10);
        if (!DateOfSerach) DateOfSerach = new Date().toJSON().slice(0, 10);
        if (!SourceDataBase) SourceDataBase = "";
        if (!SearchDescription) SearchDescription = "";
        if (!SearchString) SearchString = "";
        if (!Notes) Notes = "";
        let result: SourceForUpload = {
            fileContent: FileContent,
            source_ID: -1,
            source_Name: Source_Name,
            dateOfSerach: DateOfSerach,
            dateOfImport: DateOfImport,
            sourceDataBase: SourceDataBase,
            searchDescription: SearchDescription,
            searchString: SearchString,
            notes: Notes,
            importFilter: ImportFilterName, //the name of the filter!
            total_Items: -1,
            deleted_Items: -1,
            isFlagDeleted: false,
            codes: -1,
            inductiveCodes: -1,
            attachedFiles: -1,
            duplicates: -1,
            isMasterOf: -1,
            outcomes: -1,
            isDeleted: false
        };
        return result;
    }
    
}
export interface Source {
    source_ID: number;
    source_Name: string;
    dateOfSerach: string;
    dateOfImport: string;
    sourceDataBase: string;
    searchDescription: string;
    searchString: string;
    notes: string;
    importFilter: string; //the name of the filter!
    total_Items: number;
    deleted_Items: number;
    isFlagDeleted: boolean;
    codes: number;
    inductiveCodes: number;
    attachedFiles: number;
    duplicates: number;
    isMasterOf: number;
    outcomes: number;
    isDeleted: boolean;
}
export interface SourceForUpload extends Source {
    //we use the source interface to upload items as it allows to pass the source description fields
    //we can't really use an IncomingItemsList-like object (used in ER4), because the parsing now happens on server side,
    //making this kind of object impractical. Instead, we create it on the fly on the server side.
    fileContent: string
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
export interface ImportFilter {
    filterID: number;
    ruleName: string;
    startOfNewRec: string;//we might use this to split the file and send only the first 100 refs for checking (import step1)...
}
export interface ReadOnlySource {
    source_ID: number;
    source_Name: string;
    total_Items: number;
    deleted_Items: number;
    duplicates: number;
    isDeleted: boolean;
}
export interface ReadOnlySourcesList {
    sources: ReadOnlySource[];
}