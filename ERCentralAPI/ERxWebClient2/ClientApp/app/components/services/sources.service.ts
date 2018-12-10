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
import { BusyAwareService } from '../helpers/BusyAwareService';

@Injectable({
    providedIn: 'root',
})

export class SourcesService extends BusyAwareService {

    constructor(
        private _http: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
    private _IncomingItems4Checking: IncomingItemsList | null = null;
    public get IncomingItems4Checking(): IncomingItemsList | null {
        return this._IncomingItems4Checking;
    }
    public ClearIncomingItems4Checking() {
        this._IncomingItems4Checking = null;
        this._LastUploadStatus = "";
    }
    private _ImportFilters: ImportFilter[] = [];
    public get ImportFilters(): ImportFilter[] {
        return this._ImportFilters;
    }
    @Output() gotSource = new EventEmitter();
    @Output() gotItems4Checking = new EventEmitter();
    @Output() SourceUploaded = new EventEmitter();
    @Output() SourceDeleted = new EventEmitter<number>();
    private _ReviewSources: ReadOnlySource[] = [];
    public get ReviewSources(): ReadOnlySource[] {
        return this._ReviewSources;
    }
    private _Source: Source | null = null;
    public get CurrentSourceDetail(): Source | null {
        return this._Source;
    }
    private _LastUploadStatus: string = "";
    public get LastUploadStatus(): string {
        return this._LastUploadStatus;
    }
    private _LastDeleteForeverStatus: string = "";
    public get LastDeleteForeverStatus(): string {
        return this._LastDeleteForeverStatus;
    }
    
    public FetchSources() {
        this._BusyMethods.push("FetchSources");
        return this._http.get<ReadOnlySourcesList>(this._baseUrl + 'api/Sources/GetSources').subscribe(result => {
            this._ReviewSources = result.sources;
            if (this._Source == null && this._ReviewSources.length > 0) {
                //let's go and get the first source:
                this.FetchSource(this._ReviewSources[0].source_ID);
            }
        }, error => { this.modalService.GenericErrorMessage(error); }
            , () => {
                this.gotSource.emit();
                this.RemoveBusy("FetchSources");
            }
        );
    }
    public FetchSource(SourceId: number) {
        this._BusyMethods.push("FetchSource");
        let body = JSON.stringify({ Value: SourceId });
        this._http.post<Source>(this._baseUrl + 'api/Sources/GetSource', body).subscribe(result => {
            this._Source = result;
            this.gotSource.emit();
        }, error => { this.modalService.GenericErrorMessage(error); }
            , () => {
                this.RemoveBusy("FetchSource");
            }
        );
    }
    //UpdateSource
    public DeleteSourceForever(SourceId: number) {
        this._LastDeleteForeverStatus = "";
        this._BusyMethods.push("DeleteSourceForever");
        let body = JSON.stringify({ Value: SourceId });
        this._http.post<number>(this._baseUrl + 'api/Sources/DeleteSourceForever', body).subscribe(result => {
            if (result == SourceId && this._Source && SourceId == this._Source.source_ID) this._Source = null;//we wipe it here only if user has not changed source in the mean time!!
            this._LastDeleteForeverStatus = "Success";
        },
            error => { 
                //this.modalService.GenericErrorMessage(error); 
                this._LastDeleteForeverStatus = "Error";
            },
            () => {
                this.FetchSources();
                this.SourceDeleted.emit(SourceId);
                this.RemoveBusy("DeleteSourceForever");
            }
        );
    }
    public FetchImportFilters() {
        this._BusyMethods.push("FetchImportFilters");
        this._http.get<ImportFilter[]>(this._baseUrl + 'api/Sources/GetImportFilters').subscribe(result => {
            this._ImportFilters = result;
            }, error => { this.modalService.GenericErrorMessage(error); }
            , () => {
                this.RemoveBusy("FetchImportFilters");
            }
         );
    }
    
    public CheckUpload(data: SourceForUpload) {
        this._LastUploadStatus = ""; //reset this as we're starting over
        this._BusyMethods.push("CheckUpload");
        //console.log('CheckUpload');
        this._IncomingItems4Checking = null;
        let body = JSON.stringify(data);
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/VerifyFile',
            body).subscribe(
                result => {
                    this._IncomingItems4Checking = result;
            }, error => {
                this.modalService.GenericErrorMessage(error);
            },
            () => {
                this.gotItems4Checking.emit();
                this.RemoveBusy("CheckUpload");
            });
    }
    public Upload(data: SourceForUpload) {
        this._BusyMethods.push("Upload");
        this._LastUploadStatus = "Uploading";//probably redundant, we only use this value when API call is finished.
        console.log('Upload Source started');
        let body = JSON.stringify(data);
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/UploadSource',
            body).subscribe(result => {
                 this.FetchSources()
                this._LastUploadStatus = "Success";
            }, error => {
                //this.modalService.GenericErrorMessage();
                this._LastUploadStatus = "Error";
            },
            () => {
                this.RemoveBusy("Upload");
                this.SourceUploaded.emit();
            }
            );
    }
    
    public DeleteUndeleteSource(ros: ReadOnlySource) {
        this._BusyMethods.push("DeleteUndeleteSource");
        let body = JSON.stringify({ Value: ros.source_ID });
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/DeleteUndeleteSource',
        body).subscribe(result => {
            this.FetchSources()
        }, error => {
            this.modalService.GenericErrorMessage(error);
        },
            () => {
                this.RemoveBusy("DeleteUndeleteSource");
            }
        );
    }
    public IsSourceNameValid(sourceName: string, sourceID?: number): number {
        // zero if it's fine, 1 if empty, 2 if name-clash (we don't want 2 sources with the same name)
        //if (this.WizPhase != 2) return 1;
        if (sourceName.trim() == "") return 1;
        else if (sourceID) {
            let IndexbyName = this.ReviewSources.findIndex(found => found.source_Name == sourceName);
            if (IndexbyName == -1) return 0;
            let IndexById = this.ReviewSources.findIndex(found => found.source_ID == sourceID);
            if (IndexById == IndexbyName) return 0;//name is in use, but it's the same source!
            else return 2;
        }
        else {
            if (
                this.ReviewSources.findIndex(found => found.source_Name == sourceName) == -1
            ) return 0;
            else return 2;
        };
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
    parent_title: string;
    authorsLi: IncomingItemAuthor[];
    year: string;
    pages: string;
}
export interface IncomingItemAuthor {
    firstName: string;
    lastName: string;
    middleName: string;
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