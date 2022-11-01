import { Inject, Injectable, EventEmitter, Output, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { EventEmitterService } from './EventEmitter.service';
import { lastValueFrom, Subscription } from 'rxjs';
import { Helpers } from '../helpers/HelperMethods';
import { ConfigService } from './config.service';

@Injectable({
    providedIn: 'root',
})

export class SourcesService extends BusyAwareService implements OnDestroy {

	constructor(
		private _httpC: HttpClient,
        private modalService: ModalService,
        private EventEmitterService: EventEmitterService,
    configService: ConfigService
    ) {
    super(configService);
        //console.log("On create SourcesService");
        this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
    }
    ngOnDestroy() {
        console.log("Destroy SourcesService");
        if (this.clearSub != null) this.clearSub.unsubscribe();
    }
    private clearSub: Subscription | null = null;

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
    @Output() SourceUpdated = new EventEmitter();
    @Output() SourceDeleted = new EventEmitter<number>();
    @Output() gotPmSearchToCheck = new EventEmitter();
    @Output() PubMedSearchImported = new EventEmitter();

    private _PerSourceReport: boolean = true;
    private _ReviewSources: ReadOnlySource[] = [];
    public get ReviewSources(): ReadOnlySource[] {
        return this._ReviewSources;
    }
    private _SomeSourceIsBeingDeleted: boolean = false;
    public get SomeSourceIsBeingDeleted(): boolean {
        return this._SomeSourceIsBeingDeleted;
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

    public get SourceReportIsRunning(): boolean {
        if (this._CurrentSourceIndex4SourceReport == -1) {
            return false;
        }
        else {
            return this._NumberSourcesInReport > this._CurrentSourceIndex4SourceReport;
        }
    }


    private _CurrentSourceIndex4SourceReport: number = -1;// -1 also means: report is NOT running
    private _NumberSourcesInReport: number = -1;

    public get ProgressOfSourcesReport(): string {
        return "Retreiving Item " + (this._CurrentSourceIndex4SourceReport + 1).toString()
            + " of " + this._NumberSourcesInReport;

    }

    public FetchSources() {
        this._BusyMethods.push("FetchSources");
        return this._httpC.get<ReadOnlySourcesList>(this._baseUrl + 'api/Sources/GetSources').subscribe(result => {
            this._ReviewSources = result.sources;
            this._SomeSourceIsBeingDeleted = result.someSourceIsBeingDeleted;
            this.RemoveBusy("FetchSources");
        }, error => {
            this.RemoveBusy("FetchSources");
            this.modalService.GenericError(error);
        }
        );
    }
    /*
    public FetchSource1(SourceId: number) {
        this._BusyMethods.push("FetchSource");
        let body = JSON.stringify({ Value: SourceId });
        this._httpC.post<Source>(this._baseUrl + 'api/Sources/GetSource', body).subscribe(result => {
            this._Source = result;
            this.gotSource.emit();
        }, error => {
            this.RemoveBusy("FetchSource");
            this.modalService.GenericError(error);
        }
            , () => {
                this.RemoveBusy("FetchSource");
            }
        );
    }
    */


    public FetchNewPubMedSearch(SearchString: string) {
        if (SearchString.trim().length < 2) return;
        this._BusyMethods.push("FetchNewPubMedSearch");
        let body = JSON.stringify({ Value: SearchString.trim() });
        this._httpC.post<PubMedSearch>(this._baseUrl + 'api/Sources/NewPubMedSearchPreview', body).subscribe(result => {
            this._CurrentPMsearch = result;
            //this.gotSource.emit();
        }, error => {
            this.RemoveBusy("FetchNewPubMedSearch");
            this.modalService.GenericError(error);
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
        let IsGettingAPreview: boolean = (PmSearch.showEnd != 0 && PmSearch.showStart <= PmSearch.showEnd && PmSearch.saveEnd == 0 && PmSearch.saveStart == 0);
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
        this._httpC.post<string>(this._baseUrl + 'api/Sources/DeleteSourceForever', body).subscribe(result => {
            if (this._Source && SourceId == this._Source.source_ID) this._Source = null;//we wipe it here only if user has not changed source in the mean time!!
            this._LastDeleteForeverStatus = result;
            this.SourceDeleted.emit(SourceId);
            this.FetchSources();
            this.RemoveBusy("DeleteSourceForever");
        },
            error => { 
                //this.modalService.GenericErrorMessage(error); 
                this._LastDeleteForeverStatus = "Error";
                this.RemoveBusy("DeleteSourceForever");
                this.modalService.GenericError(error);//best way to show the error, as it will include the error details, no matter what!
                this.FetchSources();
                //this.SourceDeleted.emit(SourceId);
            }
            
        );
    }
    public FetchImportFilters() {
        this._BusyMethods.push("FetchImportFilters");
        this._httpC.get<ImportFilter[]>(this._baseUrl + 'api/Sources/GetImportFilters').subscribe(result => {
            this._ImportFilters = result;
        }, error => {
            this.modalService.GenericError(error);
            this.RemoveBusy("FetchImportFilters");
        }, () => {
                this.RemoveBusy("FetchImportFilters");
            }
         );
    }
    
    public CheckUpload(data: SourceForUpload) : Promise<boolean> {
        this._LastUploadOrUpdateStatus = ""; //reset this as we're starting over
        this._BusyMethods.push("CheckUpload");
        //console.log('CheckUpload');
        this._IncomingItems4Checking = null;
        let body = JSON.stringify(data);
      return lastValueFrom(this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/VerifyFile',
            body)).then(
            result => {
                this.RemoveBusy("CheckUpload");
                this._IncomingItems4Checking = result;
                this.gotItems4Checking.emit();
                return true;
            }, error => {
                this.RemoveBusy("CheckUpload");
                console.log("Error in CheckUpload source:", error);
                this.modalService.GenericError(error);
                return false;
            }
        ).catch(
            caught => {
                this.RemoveBusy("CheckUpload");
                console.log("Catch in CheckUpload source:", caught);
                this.modalService.GenericError(caught);
                return false;
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
                console.log("Error in Upload source:", error);
                this.modalService.GenericError(error);
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
        if (this._Source &&  this._Source.source_ID == ros.source_ID) this._Source = null;//we are deleting/undeleting this, so catch all solution: just forget it...
        this._BusyMethods.push("DeleteUndeleteSource");
        let body = JSON.stringify({ Value: ros.source_ID });
        this._httpC.post<IncomingItemsList>(this._baseUrl + 'api/Sources/DeleteUndeleteSource',
            body).subscribe(result => {
                this.FetchSources();
            }, error => {
                this.RemoveBusy("DeleteUndeleteSource");
                this.modalService.GenericError(error);
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
                    this._Source = null;//resets the source also on the UI - this ensures the "cancel" buttons will disappear in the component
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
        this._SomeSourceIsBeingDeleted = false;
        this.ClearIncomingItems4Checking();
        this.ClearPMsearchState();
        this.StopSourcesReport();
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


    public GetSourceData(Id: number): Promise<Source | boolean> {
        this._BusyMethods.push("GetSource");
        let ErrMsg = "Something went wrong when getting the source data. \r\n If the problem persists, please contact EPPISupport.";
        let body = JSON.stringify({ Value: Id });

      return lastValueFrom(this._httpC.post<Source>(this._baseUrl + 'api/Sources/GetSource',
            body))
            .then(
                (result) => {
                    //if (!result || result.length < 1) this.modalService.GenericErrorMessage(ErrMsg);
                    // a false result just means there aren't any links (and we want to know that)
                    this.RemoveBusy("GetSource");
                    return result;
                }
                , (error) => {
                    this.modalService.GenericError(error);
                    this.RemoveBusy("GetSource");
                    return false;
                }
            ).catch((caught) => {
                this.modalService.GenericError(caught);
                this.RemoveBusy("GetSource");
                return false;
            });
    }

    

    public async FetchSource(SourceId: number) {
        let res = await this.GetSourceData(SourceId);
        if (res != false) {
            if (res != true) {
                this._Source = res;
            }
        }
    }
    
    public async GetSourceReport(reportParameter: Source | string): Promise<string> {

        let report: string = "";
        if (typeof reportParameter === 'string') {
            // the parameter is a string indicating the type of report
            if (reportParameter == "allSources") {
                // this is a summary report of all non-deleted sources

                this._CurrentSourceIndex4SourceReport = 0;

                report += "<h3>Search sources report</h3>(undeleted sources only)";
                report += "<table border='1' cellspacing='0' cellpadding='2'>";

                let sourceList: ReadOnlySource[] = this.ReviewSources.filter(f=> f.isDeleted == false && f.source_ID > 0);//filter to get only not-deleted sources, and exclude the "manually created" source

                // we need the the number of sources that will be in the report, this also helps "cancelling" the process on user's demand
                this._NumberSourcesInReport += sourceList.length; // counting the manually created source

                // order the source array by source name
                let orderedSourceList = sourceList.sort((a, b) => (a.source_Name < b.source_Name) ? -1 : 1);
                
                for (this._CurrentSourceIndex4SourceReport = 0; this._NumberSourcesInReport > -1 && this._CurrentSourceIndex4SourceReport < orderedSourceList.length; this._CurrentSourceIndex4SourceReport++) {
                    //condition this._NumberSourcesInReport > -1 becomes false when user clicks on "Cancel".
                    let currentSource: ReadOnlySource = orderedSourceList[this._CurrentSourceIndex4SourceReport];
                        
                    report += "<tr>"
                    report += "<td>Source name</td>";
                    report += "<td><b>" + currentSource.source_Name + "</b></td>";
                    report += "</tr>"

                    let res = await this.GetSourceData(currentSource.source_ID);

                    if (res != false) {
                        if (res != true) {
                            
                            let currentSourceData: Source = res;

                            let searchString: string = currentSourceData.searchString;
                            searchString = searchString.replace(/(?:\r\n|\r|\n)/g, '<br>');

                            report += "<tr>"
                            report += "<td>Database name/platform</td>";
                            report += "<td><b>" + currentSourceData.sourceDataBase + "</b></td>";
                            report += "</tr>"
                            report += "<tr>"
                            report += "<td>Date of search</td>";
                            report += "<td>" + Helpers.FormatDate2(currentSourceData.dateOfSerach) + "</td>";
                            report += "</tr>"
                            report += "<tr>"
                            report += "<tr>"
                            report += "<td>Date of import</td>";
                            report += "<td>" + Helpers.FormatDate2(currentSourceData.dateOfImport) + "</td>";
                            report += "</tr>"
                            report += "<tr>"
                            report += "<td>Number items</td>";
                            report += "<td>" + currentSourceData.total_Items + "</td>";
                            report += "</tr>"
                            report += "<tr>"
                            report += "<td>Duplicates</td>";
                            report += "<td>" + currentSourceData.duplicates + "</td>";
                            report += "</tr>"
                            report += "<tr>"
                            report += "<td>Description</td>";
                            report += "<td>" + currentSourceData.searchDescription + "</td>";
                            report += "</tr>"
                            report += "<tr>"
                            report += "<td>Notes</td>";
                            report += "<td>" + currentSourceData.notes + "</td>";
                            report += "</tr>"
                            report += "<tr>"
                            report += "<td>Search string</td>";
                            report += "<td>" + searchString + "</td>";
                            report += "</tr>"
                            report += "<tr>"
                            report += "<td colspan='2' style='border:0px'>&nbsp;</td>";
                            report += "</tr>"
                        }
                    }
                    else {
                        //on error, we abort the whole thing, otherwise would be returning a malformed report with at least one source data missing.
                        this.StopSourcesReport();//fully mark the process as finished
                        return "";//also ends the for cycle
                    }
                    
                }

                // add the manually create items source at the end
                report += "<tr>"
                report += "<td>Source name</td>";
                report += "<td><b>Manually created items</b></td>";
                report += "</tr>"
                report += "<tr>"
                report += "<td>Database name/platform</td>";
                report += "<td>N/A</td>";
                report += "</tr>"
                report += "<tr>"
                report += "<td>Date of search</td>";
                report += "<td>N/A</td>";
                report += "</tr>"
                report += "<tr>"
                report += "<td>Date of import</td>";
                report += "<td>N/A</td>";
                report += "</tr>"
                report += "<tr>"
                report += "<td>Number items</td>";
                report += "<td>" + this.ReviewSources[this.ReviewSources.length - 1].total_Items + "</td>";
                report += "</tr>"
                report += "<tr>"
                report += "<td>Duplicates</td>";
                report += "<td>" + this.ReviewSources[this.ReviewSources.length - 1].duplicates + "</td>";
                report += "</tr>"
                report += "<tr>"
                report += "<td>Description</td>";
                report += "<td>N/A</td>";
                report += "</tr>"
                report += "<tr>"
                report += "<td>Notes</td>";
                report += "<td>N/A</td>";
                report += "</tr>"
                report += "<tr>"
                report += "<td>Search string</td>";
                report += "<td>N/A</td>";
                report += "</tr>"
                report += "<tr>"
                report += "<td colspan='2' style='border:0px'>&nbsp;</td>";
                report += "</tr>"
                report += "</table>"
                if (this._NumberSourcesInReport == -1) report = "";//this happens if the user clicked on "Cancel"...
                this.StopSourcesReport(); // report generation is done
            }
        }
        else {
            // this is a detailed report of a single source
            let currentSource: Source = reportParameter;

            let searchString: string = currentSource.searchString;
            searchString = searchString.replace(/(?:\r\n|\r|\n)/g, '<br>');

            report +=  "<h3>Source report</h3>";
            report += "<table border='1' cellspacing='0' cellpadding='2'>";

            report += "<tr>"
            report += "<td>Source name</td>";
            report += "<td><b>" + currentSource.source_Name + "</b></td>";
            report += "</tr>"

            report += "<tr>"
            report += "<td>Database name/platform</td>";
            report += "<td><b>" + currentSource.sourceDataBase + "</b></td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Date of search</td>";
            report += "<td>" + Helpers.FormatDate2(currentSource.dateOfSerach) + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Date of import</td>";
            report += "<td>" + Helpers.FormatDate2(currentSource.dateOfImport) + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Number items</td>";
            report += "<td>" + currentSource.total_Items + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Duplicates</td>";
            report += "<td>" + currentSource.duplicates + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Description</td>";
            report += "<td>" + currentSource.searchDescription + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Notes</td>";
            report += "<td>" + currentSource.notes + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Search string</td>";
            report += "<td>" + searchString + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Items coded</td>";
            report += "<td>" + currentSource.codes + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Uploaded documents</td>";
            report += "<td>" + currentSource.attachedFiles + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Masters of duplicates</td>";
            report += "<td>" + currentSource.isMasterOf + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Deleted items</td>";
            report += "<td>" + currentSource.deleted_Items + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Outcomes</td>";
            report += "<td>" + currentSource.outcomes + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Import filter</td>";
            report += "<td>" + currentSource.importFilter + "</td>";
            report += "</tr>"
            report += "<tr>"
            report += "<td>Is deleted?</td>";
            report += "<td>" + currentSource.isDeleted + "</td>";
            report += "</tr>";
            report += "</table>"           
        }
        
        return report;
    }
    public StopSourcesReport() {
        this._CurrentSourceIndex4SourceReport = -1;
        this._NumberSourcesInReport = -1;
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
    isSelected: boolean;
    source_ID: number;
    source_Name: string;
    total_Items: number;
    deleted_Items: number;
    duplicates: number;
    isDeleted: boolean;
    isBeingDeleted: boolean;
}
export interface ReadOnlySourcesList {
    someSourceIsBeingDeleted: boolean;
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
