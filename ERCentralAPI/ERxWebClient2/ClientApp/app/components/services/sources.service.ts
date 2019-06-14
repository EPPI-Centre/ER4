import { Component, Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';

@Injectable({
    providedIn: 'root',
})

export class SourcesService extends BusyAwareService {

	constructor(
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
    private _ImportFilters: ImportFilter[] = [];
    public get ImportFilters(): ImportFilter[] {
        return this._ImportFilters;
    }
    @Output() gotSource = new EventEmitter();
    @Output() gotItems4Checking = new EventEmitter();
    @Output() SourceUploaded = new EventEmitter();
    @Output() SourceUpdated = new EventEmitter();
    @Output() SourceDeleted = new EventEmitter<number>();
    @Output() gotPmSearchToCheck = new EventEmitter();
    @Output() PubMedSearchImported = new EventEmitter();

    private _ReviewSources: ReadOnlySource[] = [];
    public get ReviewSources(): ReadOnlySource[] {
        return this._ReviewSources;
    }
    private _Source: Source | null = null;
    public get CurrentSourceDetail(): Source | null {
        return this._Source;
    }
    private _LastUploadOrUpdateStatus: string = "";
    public get LastUploadOrUpdateStatus(): string {
        return this._LastUploadOrUpdateStatus;
    }
    private _LastDeleteForeverStatus: string = "";
    public get LastDeleteForeverStatus(): string {
        return this._LastDeleteForeverStatus;
    }
    private _CurrentPMsearch: PubMedSearch | null = null;
    public get CurrentPMsearch(): PubMedSearch | null {
        return this._CurrentPMsearch;
    }

    public ClearIncomingItems4Checking() {
        this._IncomingItems4Checking = null;
        this._LastUploadOrUpdateStatus = "";
    }
    public ClearPMsearchState() {
        this._CurrentPMsearch = null;
        this._LastUploadOrUpdateStatus = "";
    }
    
    public FetchSources() {
        this._BusyMethods.push("FetchSources");
        return this._httpC.get<ReadOnlySourcesList>(this._baseUrl + 'api/Sources/GetSources').subscribe(result => {
            this._ReviewSources = result.sources;
            this.RemoveBusy("FetchSources");
        }, error => {
            this.RemoveBusy("FetchSources");
            this.modalService.GenericErrorMessage(error);
        }
        );
    }
    public FetchSource(SourceId: number) {
        this._BusyMethods.push("FetchSource");
        let body = JSON.stringify({ Value: SourceId });
        this._httpC.post<Source>(this._baseUrl + 'api/Sources/GetSource', body).subscribe(result => {
            this._Source = result;
            this.gotSource.emit();
        }, error => {
            this.RemoveBusy("FetchSource");
            this.modalService.GenericErrorMessage(error);
        }
            , () => {
                this.RemoveBusy("FetchSource");
            }
        );
    }
    public FetchNewPubMedSearch(SearchString: string) {
        if (SearchString.trim().length < 2) return;
        this._BusyMethods.push("FetchNewPubMedSearch");
        let body = JSON.stringify({ Value: SearchString.trim() });
        this._httpC.post<PubMedSearch>(this._baseUrl + 'api/Sources/NewPubMedSearchPreview', body).subscribe(result => {
            this._CurrentPMsearch = result;
            //this.gotSource.emit();
        }, error => {
            this.RemoveBusy("FetchNewPubMedSearch");
            this.modalService.GenericErrorMessage(error);
        }
            , () => {
                this.gotPmSearchToCheck.emit();
                this.RemoveBusy("FetchNewPubMedSearch");
            }
        );
    }
    public ActOnPubMedSearch(PmSearch: PubMedSearch) {
        this._BusyMethods.push("ActOnPubMedSearch");
        //same logic as ER4 to figure if we're doing the import or just getting some results to show.
        let IsGettingAPreview: boolean = (PmSearch.showEnd != 0 && PmSearch.showStart < PmSearch.showEnd && PmSearch.saveEnd == 0 && PmSearch.saveStart == 0);
        let body = JSON.stringify(PmSearch);
        this._httpC.post<PubMedSearch>(this._baseUrl + 'api/Sources/ActOnPubMedSearchPreview', body).subscribe(result => {
            this._CurrentPMsearch = result;
            this._LastUploadOrUpdateStatus = "Success";
            this.RemoveBusy("ActOnPubMedSearch");
            if (!IsGettingAPreview) this.FetchSources();
        }, error => {
            console.log("something went wrong: ", error);
            this.RemoveBusy("ActOnPubMedSearch");
            this.PubMedSearchImported.emit();
            this._LastUploadOrUpdateStatus = "Error";
        }
            , () => {
                if (IsGettingAPreview) {
                    //we're getting a different preview
                    this.gotPmSearchToCheck.emit();
                } else {//importing
                    this.PubMedSearchImported.emit();
                }
                this.RemoveBusy("ActOnPubMedSearch");
            }
        );
    }
    public DeleteSourceForever(SourceId: number) {
        this._LastDeleteForeverStatus = "";
        this._BusyMethods.push("DeleteSourceForever");
        let body = JSON.stringify({ Value: SourceId });
        this._httpC.post<number>(this._baseUrl + 'api/Sources/DeleteSourceForever', body).subscribe(result => {
            if (result == SourceId && this._Source && SourceId == this._Source.source_ID) this._Source = null;//we wipe it here only if user has not changed source in the mean time!!
            this._LastDeleteForeverStatus = "Success";
        },
            error => { 
                //this.modalService.GenericErrorMessage(error); 
                this._LastDeleteForeverStatus = "Error";
                this.RemoveBusy("DeleteSourceForever");
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
        this._httpC.get<ImportFilter[]>(this._baseUrl + 'api/Sources/GetImportFilters').subscribe(result => {
            this._ImportFilters = result;
        }, error => {
            this.modalService.GenericErrorMessage(error);
            this.RemoveBusy("FetchImportFilters");
        }, () => {
                this.RemoveBusy("FetchImportFilters");
            }
         );
    }
    
    public CheckUpload(data: SourceForUpload) {
        this._LastUploadOrUpdateStatus = ""; //reset this as we're starting over
        this._BusyMethods.push("CheckUpload");
        //console.log('CheckUpload');
        this._IncomingItems4Checking = null;
        let body = JSON.stringify(data);
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/VerifyFile',
            body).subscribe(
                result => {
                    this._IncomingItems4Checking = result;
            }, error => {
                this.RemoveBusy("CheckUpload");
                this.modalService.GenericErrorMessage(error);
            },
            () => {
                this.gotItems4Checking.emit();
                this.RemoveBusy("CheckUpload");
            });
    }
    public Upload(data: SourceForUpload) {
        this._BusyMethods.push("Upload");
        this._LastUploadOrUpdateStatus = "Uploading";//probably redundant, we only use this value when API call is finished.
        console.log('Upload Source started');
        let body = JSON.stringify(data);
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/UploadSource',
            body).subscribe(result => {
                 this.FetchSources()
                this._LastUploadOrUpdateStatus = "Success";
            }, error => {
                //this.modalService.GenericErrorMessage();
                this.RemoveBusy("Upload");
                this._LastUploadOrUpdateStatus = "Error";
            },
            () => {
                this.RemoveBusy("Upload");
                this.SourceUploaded.emit();
            }
            );
    }
    
    public DeleteUndeleteSource(ros: ReadOnlySource) {
        this._Source = null;//we may be deleting/undeleting this, so catch all solution: just forget...
        this._BusyMethods.push("DeleteUndeleteSource");
        let body = JSON.stringify({ Value: ros.source_ID });
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/DeleteUndeleteSource',
        body).subscribe(result => {
            this.FetchSources()
            }, error => {
                this.RemoveBusy("DeleteUndeleteSource");
                this.modalService.GenericErrorMessage(error);
            },
            () => {
                this.RemoveBusy("DeleteUndeleteSource");
            }
        );
    }

    public UpdateSource(source: Source) {
        this._BusyMethods.push("UpdateSource");
        this._LastUploadOrUpdateStatus = "Updating";//probably redundant, we only use this value when API call is finished.
        console.log('UpdateSource Source started');
        let body = JSON.stringify(source);
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/UpdateSource',
            body).subscribe(result => {
                this._LastUploadOrUpdateStatus = "Success";
            }, error => {
                //this.modalService.GenericErrorMessage();
                this._LastUploadOrUpdateStatus = "Error";
                this.RemoveBusy("UpdateSource");
            },
            () => {
                    this.FetchSource(source.source_ID);
                    this.FetchSources();
                    this.RemoveBusy("UpdateSource");//service remains busy because of the two calls above
                    this.SourceUpdated.emit();//makes the component wait for service to stop being busy and then update itself (gotSource subscr) and show the notify.
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
    public Clear() {
        this._ReviewSources = [];
        this._Source = null;
        this._LastDeleteForeverStatus = "";
        this.ClearIncomingItems4Checking();
        this.ClearPMsearchState();
    }
    public static LimitedAuthorsString(IncomingItemAuthors: IncomingItemAuthor[]): string {
        //[LAST] + ' ' + [FIRST] + ' ' + [SECOND]
        let res: string = "";
        if (IncomingItemAuthors) {
            for (let IncomingItemAuthor of IncomingItemAuthors) {
                res += IncomingItemAuthor.lastName + ' ' + IncomingItemAuthor.firstName +
                    (IncomingItemAuthor.middleName.length > 0 ? ' ' + IncomingItemAuthor.middleName : '') + '; ';
                if (res.length > 60) {
                    res += " [et al.]";
                    break;
                }
            }
        }
        return res.trim();
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

export interface PubMedSearch {
    queryStr: string;
    webEnv: string;
    queMax: number;
    showStart: number;
    showEnd: number;
    saveStart: number;
    saveEnd: number;
    summary: string;
    //public MobileList<string> SavedIndexes = null;
    itemsList: PMincomingItems;
    queryKey: number;
}
export interface PMincomingItems {
    sourceName: string;
    incomingItems: IncomingItem[];
    notes: string;
    searchDescr: string;
    sourceDB: string;
    dateOfSerach: string;
    dateOfImport: string;
}