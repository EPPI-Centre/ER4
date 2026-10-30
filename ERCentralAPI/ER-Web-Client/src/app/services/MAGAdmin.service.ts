import { Injectable, Inject, OnDestroy } from "@angular/core";
import { ModalService } from "./modal.service";
import { HttpClient } from "@angular/common/http";
import { BusyAwareService } from "../helpers/BusyAwareService";
import { MAGBlobCommand, MAGLog, MAGReview, MagCurrentInfo, ContReviewPipeLineCommand } from "./MAGClasses.service";
import { EventEmitterService } from './EventEmitter.service';
import { lastValueFrom, Subscription } from "rxjs";
import { ConfigService } from "./config.service";

@Injectable({

    providedIn: 'root',

})
export class MAGAdminService extends BusyAwareService implements OnDestroy {


    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        private EventEmitterService: EventEmitterService,
      configService: ConfigService
    ) {
      super(configService);
        //console.log("On create MAGAdminService");
        this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
    }

    ngOnDestroy() {
        console.log("Destroy MAGAdminService");
        if (this.clearSub != null) this.clearSub.unsubscribe();
    }
    private clearSub: Subscription | null = null;

    public releaseNotes: string = '';
    //public latestMagSasUri: string = '';
    public latestMAGName: string = '';
    public previousMAGName: string = '';
    public MAGLogList: MAGLog[] = [];
    public MAGReviewList: MAGReview[] = [];
    public MagCurrentInfo: MagCurrentInfo = new MagCurrentInfo();
    public MagCurrentInfoList: MagCurrentInfo[] = [];


    public DoRunContReviewPipeline(specificFolder: string, magLogId: number, alertText: string, editFoSThreshold: number,
        editReviewSampleSize: number, editScoreThreshold: number, fromDate: string, toDate: string
    ) {
        this._BusyMethods.push("DoRunContReviewPipeline");

        let pipelineParams: ContReviewPipeLineCommand = new ContReviewPipeLineCommand();
        pipelineParams.editFoSThreshold = editFoSThreshold;
        pipelineParams.editReviewSampleSize = editReviewSampleSize;
        pipelineParams.editScoreThreshold = editScoreThreshold;
        pipelineParams.magLogId = magLogId;
        pipelineParams.currentVersion = toDate;
        pipelineParams.previousVersion = fromDate;
        pipelineParams.specificFolder = specificFolder;
        this._httpC.post<boolean>(this._baseUrl + 'api/MagCurrentInfo/DoRunContReviewPipeline', pipelineParams)
            .subscribe(result => {
                this.RemoveBusy("DoRunContReviewPipeline");
                if (result != null) {
                    this.SwitchOnAutoRefreshLogList();
                }
            },
                error => {
                    this.RemoveBusy("DoRunContReviewPipeline");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("DoRunContReviewPipeline");
                });
    }
    // public DoCheckChangedPaperIds(magLatest: string ) {

    //     this._BusyMethods.push("DoCheckChangedPaperIds");
    //     let body = JSON.stringify({ Value: magLatest });
    //     this._httpC.post<boolean>(this._baseUrl + 'api/MagCurrentInfo/DoCheckChangedPaperIds', body)
    //         .subscribe(result => {
    //             this.RemoveBusy("DoCheckChangedPaperIds");
    //             if (result != null ) {
    //                 this.SwitchOnAutoRefreshLogList();
    //             }
    //         },
    //             error => {
    //                 console.log(JSON.stringify(error));
    //                 this.RemoveBusy("DoCheckChangedPaperIds");
    //                 this.modalService.GenericError(error);
    //             },
    //             () => {
    //                 this.RemoveBusy("DoCheckChangedPaperIds");
    //             });
    // }
    public AddReview(reviewId: number) {

        this._BusyMethods.push("AddReview");
        let body = JSON.stringify({ Value: reviewId });
      lastValueFrom(this._httpC.post<MAGReview>(this._baseUrl + 'api/MagReviewList/AddReviewToMagList', body)
            ).then( (result) => {
                this.RemoveBusy("AddReview");
                if (result != null) {
                    this.MAGReviewList.push(result);
                    return true;

                } else {
                    return false;
                }
            },
            (error) => {
                this.RemoveBusy("AddReview");
                this.modalService.GenericError(error);
            });
    }
    public DeleteReview(reviewId: number) {

        this._BusyMethods.push("DeleteReview");
        let body = JSON.stringify({ Value: reviewId });
      lastValueFrom(this._httpC.post<boolean>(this._baseUrl + 'api/MagReviewList/DeleteReview', body)
            ).then(() => {
                this.RemoveBusy("DeleteReview");
                let index: number = this.MAGReviewList.findIndex(x => x.reviewId == reviewId);
                if (index > -1) {
                    this.MAGReviewList.splice(index, 1);
                    return true;
                } else {
                    return false;
                }
                
            },
                (error) => {
                    this.RemoveBusy("DeleteReview");
                    this.modalService.GenericError(error);
                });
    }
    private SwitchOnAutoRefreshLogList() {
        this.GetMAGLogList();
    }

    // public GetMAGBlobCommand() {
    //     this._BusyMethods.push("GetMAGBlobCommand");

    //     this._httpC.get<MAGBlobCommand>(this._baseUrl + 'api/MagCurrentInfo/GetMAGBlobCommand')
    //         .subscribe(result => {
    //             this.RemoveBusy("GetMAGBlobCommand");
    //             if (result != null) {
    //                 this.previousMAGName = result.previousMAGName;
    //                 this.latestMAGName = result.latestMAGName;
    //                 this.releaseNotes = result.releaseNotes;
    //                 //this.latestMagSasUri = result.latestMagSasUri;
    //             }
    //         },
    //             error => {
    //                 this.RemoveBusy("GetMAGBlobCommand");
    //                 this.modalService.GenericError(error);
    //             },
    //             () => {
    //                 this.RemoveBusy("GetMAGBlobCommand");
    //             });
    // }

    public GetMAGLogList() {
        this._BusyMethods.push("GetMAGLogList");

        this._httpC.get<MAGLog[]>(this._baseUrl + 'api/MagLogList/GetMagLogList')
            .subscribe(result => {
                this.RemoveBusy("GetMAGLogList");
                if (result != null) {
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
    GetMagCurrentInfoList() {
        this._BusyMethods.push("MAGCurrentInfoList");
        this._httpC.get<MagCurrentInfo[]>(this._baseUrl + 'api/MagCurrentInfo/GetMagCurrentInfoList')
            .subscribe(result => {
                this.RemoveBusy("MAGCurrentInfoList");
                if (result != null) {
                    this.MagCurrentInfoList = result;
                }
            },
            error => {
                this.RemoveBusy("MAGCurrentInfoList");
                this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("MAGCurrentInfoList");
            });
    }
    public UpdateMagCurrentInfo(newMagEndPoint: string , newMagVersion: string ) {

        this._BusyMethods.push("UpdateMagCurrentInfo");
        let magCurrentInfo: MagCurrentInfo = this.MagCurrentInfo;
        magCurrentInfo.magOnline = true;
        magCurrentInfo.matchingAvailable = true;
        magCurrentInfo.makesEndPoint = newMagEndPoint;
        magCurrentInfo.magFolder = newMagVersion;
        this._httpC.post<MagCurrentInfo>(this._baseUrl + 'api/MagCurrentInfo/UpdateMagCurrentInfo', magCurrentInfo)
            .subscribe(result => {
                this.RemoveBusy("UpdateMagCurrentInfo");
                if (result != null) {
                    this.MagCurrentInfo = result;
                }
                this.FetchMagCurrentInfo();
            },
                error => {
                    this.RemoveBusy("UpdateMagCurrentInfo");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("UpdateMagCurrentInfo");
                });
    }

    public UpdateThisMagCurrentInfo(thisMagCurrentInfo: MagCurrentInfo ) {

        this._BusyMethods.push("UpdateThisMagCurrentInfo");
        this._httpC.post<MagCurrentInfo>(this._baseUrl + 'api/MagCurrentInfo/UpdateThisMagCurrentInfo', thisMagCurrentInfo)
            .subscribe(result => {
                this.RemoveBusy("UpdateThisMagCurrentInfo");
                if (result != null) {
                    this.MagCurrentInfo = result;
                }
                this.FetchMagCurrentInfo();
            },
                error => {
                    this.RemoveBusy("UpdateThisMagCurrentInfo");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("UpdateThisMagCurrentInfo");
                });
    }

    public FetchMagCurrentInfo() {
        this._BusyMethods.push("FetchMagCurrentInfo");
        this._httpC.get<MagCurrentInfo>(this._baseUrl + 'api/MagCurrentInfo/GetMagCurrentInfo')
            .subscribe(result => {
                this.RemoveBusy("FetchMagCurrentInfo");
                if (result != null) {
                    this.MagCurrentInfo = result;
                }
            },
                error => {
                    this.RemoveBusy("FetchMagCurrentInfo");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("FetchMagCurrentInfo");
                });
    }
    public DeleteMagCurrentInfo(magCurrentInfoId: number) {

        this._BusyMethods.push("DeleteMagCurrentInfo");
        let body = JSON.stringify({ Value: magCurrentInfoId });
      lastValueFrom(this._httpC.post<boolean>(this._baseUrl + 'api/MagCurrentInfo/DeleteMagCurrentInfo', body)
            ).then(() => {
                this.RemoveBusy("DeleteMagCurrentInfo");
                let index: number = this.MagCurrentInfoList.findIndex(x => x.magCurrentInfoId == magCurrentInfoId);
                if (index > -1) {
                    this.MagCurrentInfoList.splice(index, 1);
                    return true;
                } else {
                    return false;
                }
                
            },
                (error) => {
                    this.RemoveBusy("DeleteMagCurrentInfo");
                    this.modalService.GenericError(error);
                });
    }


    public Clear() {
        this.releaseNotes = '';
        //this.latestMagSasUri = '';
        this.latestMAGName = '';
        this.previousMAGName = '';
        this.MAGLogList = [];
        this.MAGReviewList = [];
        this.MagCurrentInfo = new MagCurrentInfo();
    }
}
