import { Injectable, Inject } from "@angular/core";
import { ModalService } from "./modal.service";
import { HttpClient } from "@angular/common/http";
import { BusyAwareService } from "../helpers/BusyAwareService";
import { MAGBlobCommand, MAGLog, MAGLogList, MAGReview, MagCurrentInfo } from "./MAGClasses.service";

@Injectable({

    providedIn: 'root',

})
export class MAGAdminService extends BusyAwareService {

    
    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
    public releaseNotes: string = '';
    public latestMagSasUri: string = '';
    public latestMAGName: string = '';
    public previousMAGName: string = '';
    public MAGLogList: MAGLog[] = [];
    public MAGReviewList: MAGReview[] = [];
    public MagCurrentInfo: MagCurrentInfo = new MagCurrentInfo();
    public DoCheckChangedPaperIds(magLatest: string ) {

        this._BusyMethods.push("DoCheckChangedPaperIds");
        let body = JSON.stringify({ Value: magLatest });
        this._httpC.post<boolean>(this._baseUrl + 'api/MagCurrentInfo/DoCheckChangedPaperIds', body)
            .subscribe(result => {
                this.RemoveBusy("DoCheckChangedPaperIds");
                if (result != null ) {
                    this.SwitchOnAutoRefreshLogList();
                }
            },
                error => {
                    this.RemoveBusy("DoCheckChangedPaperIds");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("DoCheckChangedPaperIds");
                });
    }
    private SwitchOnAutoRefreshLogList() {
        this.GetMAGLogList();
    }
    public GetMAGBlobCommand() {
        this._BusyMethods.push("GetMAGBlobCommand");

        this._httpC.get<MAGBlobCommand>(this._baseUrl + 'api/MagCurrentInfo/GetMAGBlobCommand')
            .subscribe(result => {
                this.RemoveBusy("GetMAGBlobCommand");
                if (result != null) {
                    this.previousMAGName = result.previousMAGName;
                    this.latestMAGName = result.latestMAGName;
                    this.releaseNotes = result.releaseNotes;
                    this.latestMagSasUri = result.latestMagSasUri;
                }
            },
                error => {
                    this.RemoveBusy("GetMAGBlobCommand");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("GetMAGBlobCommand");
                });
    }
    public GetMAGLogList() {
        this._BusyMethods.push("GetMAGLogList");

        this._httpC.get<MAGLog[]>(this._baseUrl + 'api/MagLogList/GetMagLogList')
            .subscribe(result => {
                this.RemoveBusy("GetMAGLogList");
                if (result != null) {
                    console.log(result)
                    this.MAGLogList = result;
                }
            },
                error => {
                    this.RemoveBusy("GetMAGLogList");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("GetMAGLogList");
                });

    }
    GetMAGReviewList() {
        this._BusyMethods.push("MAGReviewList");

        this._httpC.get<MAGReview[]>(this._baseUrl + 'api/MagReviewList/GetMagReviewList')
            .subscribe(result => {
                this.RemoveBusy("MAGReviewList");
                if (result != null) {
                    console.log(result)
                    this.MAGReviewList = result;
                }
            },
                error => {
                    this.RemoveBusy("MAGReviewList");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("MAGReviewList");
                });


    }
    public UpdateMagCurrentInfo() {
        this._BusyMethods.push("UpdateMagCurrentInfo");

        this._httpC.get<MagCurrentInfo>(this._baseUrl + 'api/MagCurrentInfo/UpdateMagCurrentInfo')
            .subscribe(result => {
                this.RemoveBusy("UpdateMagCurrentInfo");
                if (result != null) {
                    console.log(result)
                    this.MagCurrentInfo = result;
                }
            },
                error => {
                    this.RemoveBusy("UpdateMagCurrentInfo");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("UpdateMagCurrentInfo");
                });
    }
   
   
}