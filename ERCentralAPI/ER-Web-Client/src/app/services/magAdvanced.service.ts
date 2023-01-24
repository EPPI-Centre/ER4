import { Inject, Injectable, OnDestroy} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { MAGBrowserService } from './MAGBrowser.service';
import { MagPaper, MagReviewMagInfo, MVCMagPaperListSelectionCriteria,  
      MagList,
    MagCheckContReviewRunningCommand,  MagCurrentInfo} from './MAGClasses.service';
import { Router } from '@angular/router';
import { EventEmitterService } from './EventEmitter.service';
import { MAGTopicsService } from './MAGTopics.service';
import { lastValueFrom, Subscription } from 'rxjs';
import { ConfigService } from './config.service';


@Injectable({
    providedIn: 'root',
})

export class MAGAdvancedService extends BusyAwareService implements OnDestroy {
    
    constructor(
        private _httpC: HttpClient,
        private _magBrowserService: MAGBrowserService,
        private modalService: ModalService,
        private router: Router,
        private EventEmitterService: EventEmitterService,
        private _magTopicsService: MAGTopicsService,
      configService: ConfigService
    ) {
      super(configService);
        //console.log("On create MAGAdvancedService");
        this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
    }

    ngOnDestroy() {
        //console.log("Destroy MAGAdvancedService");
        if (this.clearSub != null) this.clearSub.unsubscribe();
    }
    private clearSub: Subscription | null = null;
    //public firstVisitToMAGBrowser: boolean = true;
    public _RunAlgorithmFirst: boolean = false;
    public ReviewMatchedPapersList: MagPaper[] = [];
    public AdvancedReviewInfo: MagReviewMagInfo = new MagReviewMagInfo();
    //public currentMagPaper: MagPaper = new MagPaper();
    public ListDescription: string = '';
    public MagReferencesPaperList: MagList = new MagList();
    public MagPaperFieldsList: MagPaper[] = [];
    public CurrentMagSimId: number = 0;
    private _MagCurrentInfo: MagCurrentInfo = new MagCurrentInfo();
    public get MagCurrentInfo(): MagCurrentInfo{
        return this._MagCurrentInfo;
    }
    public set MagCurrentInfo(magInfo: MagCurrentInfo) {
        this._MagCurrentInfo = magInfo;
    }

    public UpdateMagPaper(matchCorrect: boolean, paperId: number, itemId: number): Promise<MagPaper[]> {

        this._BusyMethods.push("UpdateMagPaper");
        let body = JSON.stringify({ manualTrueMatchProperty: matchCorrect, magPaperId:  paperId, itemId: itemId});
      return lastValueFrom(this._httpC.post<MagPaper>(this._baseUrl + 'api/MagPaperList/UpdateMagPaper', body)
            ).then((result: MagPaper) => {
                this.RemoveBusy("UpdateMagPaper");
                let ind = this.ReviewMatchedPapersList.findIndex(f => f.paperId == result.paperId);
                if (ind >= -1) {
                    this.ReviewMatchedPapersList[ind] = result;
                }
                    return result;    
                },
                (error: any) => {
                    this.RemoveBusy("UpdateMagPaper");
                    this.modalService.GenericError(error);
                    return error;
                })
        
    }
    public FetchMagPaperMagList(crit: MVCMagPaperListSelectionCriteria): Promise<MagList> {
        this._BusyMethods.push("FetchMagPaperMagList");
      return lastValueFrom(this._httpC.post<MagList>(this._baseUrl + 'api/MagPaperList/GetMagPaperList', crit)
            ).then(

                (result: MagList) => {
                    this.RemoveBusy("FetchMagPaperMagList");

                    if (crit.listType == 'ItemMatchedPapersList') {
                        
                        this.MagReferencesPaperList = result;
                    }
                    return result;
                },
                error => {
                    this.RemoveBusy("FetchMagPaperMagList");
                    this.modalService.GenericError(error);
                    return error;
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchMagPaperMagList");
                    this.RemoveBusy("FetchMagPaperMagList");
                    return error;
                });
    }
    //public FetchMagPaperListMagPaper(crit: MVCMagPaperListSelectionCriteria): Promise<MagList> {
    //    this._BusyMethods.push("FetchMagPaperListMagPaper");
    //    return this._httpC.post<MagList>(this._baseUrl + 'api/MagPaperList/GetMagPaperList', crit)
    //        ).then(

    //            (result: MagList) => {

    //                this.RemoveBusy("FetchMagPaperListMagPaper");

    //                if (crit.listType == 'ReviewMatchedPapers' || crit.listType == 'ReviewMatchedPapersWithThisCode') {
    //                    this.ReviewMatchedPapersList = result.papers;
    //                    for (var i = 0; i < this.ReviewMatchedPapersList.length; i++) {
    //                        this.PaperIds += this.ReviewMatchedPapersList[i].paperId.toString() + ',';
    //                    }
    //                    this.PaperIds = this.PaperIds.substr(0, this.PaperIds.length - 1)
    //                    this.CurrentCriteria = crit;

    //                } 
    //                return result.papers;
    //            },
    //            error => {
    //                this.RemoveBusy("FetchMagPaperListMagPaper");
    //                this.modalService.GenericError(error);
    //                return error;
    //            }
    //        ).catch(
    //            (error) => {

    //                this.modalService.GenericErrorMessage("error with FetchMagPaperListMagPaper");
    //                this.RemoveBusy("FetchMagPaperListMagPaper");
    //                return error;
    //            });
    //}
    
    //try making below async await
    //public async PostFetchCitationsList(result: MagPaper): Promise<boolean> {

    //    let criteriaCitationsList: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    //    criteriaCitationsList.listType = "CitationsList";
    //    criteriaCitationsList.magPaperId = result.paperId;
    //    criteriaCitationsList.pageSize = 20;
    //    console.log('calling FetchWithCrit: CitationsList');
    //    return await this._magBrowserService.FetchWithCrit(criteriaCitationsList, "CitationsList");

    //}

    //public async PostFetchCitedByListList(result: MagPaper): Promise<boolean> {

    //    this.PaperIds = this._magBrowserService.ListCriteria.paperIds;
    //    let criteriaCitedBy: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    //    criteriaCitedBy.listType = "CitedByList";
    //    criteriaCitedBy.magPaperId = result.paperId;
    //    criteriaCitedBy.pageSize = 20;
    //    console.log('calling PostFetchCitedByListList: CitedByList');
    //    return await this._magBrowserService.FetchWithCrit(criteriaCitedBy, "CitedByList")

    //}

    //public async PostFetchMagFieldOfStudyList(result: MagPaper, listType: string): Promise<MagFieldOfStudy[] | boolean> {

    //    this.PaperIds = this._magBrowserService.ListCriteria.paperIds;
    //    let criteriaFOS: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
    //    criteriaFOS.fieldOfStudyId = 0;
    //    criteriaFOS.listType = 'PaperFieldOfStudyList';
    //    criteriaFOS.paperIdList = result.paperId.toString();
    //    criteriaFOS.SearchTextTopics = ''; 
    //    console.log('calling FetchMagFieldOfStudyList: ', listType);
    //    return await this._magTopicsService.FetchMagFieldOfStudyList(criteriaFOS, listType);

    //}

    //public async PostFetchOriginalMagPaperList(result: MagPaper, listType: string): Promise<boolean> {

    //    let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    //    crit.listType = listType;
    //    crit.magPaperId = result.paperId;
    //    crit.pageSize = 20;
    //    console.log('calling FetchOrigWithCrit: ', crit.listType);
    //    return await this._magBrowserService.FetchOrigWithCrit(crit, listType);

    //}

    ////Should the promises below be made into observables??
    //private async PostFetchMagPaperCalls(result: MagPaper, listType: string) {

    //    if (result.paperId != null && result.paperId > 0) {

    //        await this.PostFetchCitationsList(result);
    //        await this.PostFetchCitedByListList(result);
    //        if (this._magBrowserService.currentMagPaper.paperId > -1) {

    //            await this.PostFetchMagFieldOfStudyList(result, listType);
    //        } else {
    //            await this.PostFetchOriginalMagPaperList(result, listType);
    //            await this.PostFetchMagFieldOfStudyList(result, listType);
    //        }

    //        //refactoring navigation: we don't navigate anymore...
    //        //this.router.navigate(['MAGBrowser']);
    //    }
    //}

    public CheckContReviewPipelineState(): Promise<boolean> {

        this._BusyMethods.push("CheckContReviewPipelineState");
      return lastValueFrom(this._httpC.get<MagCheckContReviewRunningCommand>(this._baseUrl + 'api/MagCurrentInfo/MagCheckContReviewRunningCommand')
            ).then(
                (result: MagCheckContReviewRunningCommand) => {
                    this.RemoveBusy("CheckContReviewPipelineState");
                    if (result != null) {
                        if (result.isRunningMessage == 'running') {
                            return true;
                        } else if (result.isRunningMessage == 'failed')
                        {
                            return true;
                        } else {
                            return false;
                        }
                    }
                    return true;
                },
                (error) => {
                    this.RemoveBusy("CheckContReviewPipelineState");
                    this.modalService.GenericError(error);
                    return error;
                }
        );
    }
    
    public FetchMagReviewMagInfo() {
        this._BusyMethods.push("FetchMagReviewMagInfo");
        this._httpC.get<MagReviewMagInfo>(this._baseUrl + 'api/MagCurrentInfo/GetMagReviewMagInfo')
            .subscribe(result => {
                this.RemoveBusy("FetchMagReviewMagInfo");
                if (result != null) {
                    this.AdvancedReviewInfo = result;
                }
            },
                error => {
                    this.RemoveBusy("FetchMagReviewMagInfo");
                    this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("FetchMagReviewMagInfo");
            });
    }
    public RunMatchingAlgorithm(attributeId: number) : Promise<string> {

        this._BusyMethods.push("RunMatchingAlgorithm");
        let body = JSON.stringify({ Value: attributeId});
      return lastValueFrom(this._httpC.post<string>(this._baseUrl + 'api/MagMatchAll/RunMatchingAlgorithm', body)
            ).then(

                result => {
                    this.RemoveBusy("RunMatchingAlgorithm");
                    return result;
                },
                error => {
                    this.RemoveBusy("RunMatchingAlgorithm");
                    this.modalService.GenericError(error);
                    return error;
                });
    }
    public MagMatchItemsToPapers(itemId: number): Promise<MagPaper[]> {

        this._BusyMethods.push("MagMatchItemsToPapers");
        let body = JSON.stringify({ Value: itemId });
      return lastValueFrom(this._httpC.post<MagList>(this._baseUrl + 'api/MagMatchAll/MagMatchItemsToPapers', body)
            ).then(() => {
                this.RemoveBusy("MagMatchItemsToPapers");
                let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
                crit.listType = 'ItemMatchedPapersList';
                crit.iTEM_ID = itemId;

                this.FetchMagPaperMagList(crit).then(
                    (result: MagList) => {
                        console.log('the result from fetching the list from MagMatchItemsToPapers: ', JSON.stringify(result.papers));
                        this.MagReferencesPaperList = result;
                        return result.papers;
                    }
                );
            },
                error => {
                    this.RemoveBusy("MagMatchItemsToPapers");
                    this.modalService.GenericError(error);
                    return error;
                });
    }
    public ClearAllMAGMatches(attributeId: number): Promise<string> {

        this._BusyMethods.push("ClearAllMAGMatches");
        let body = JSON.stringify({ Value: attributeId });
      return lastValueFrom(this._httpC.post<string>(this._baseUrl + 'api/MagMatchAll/ClearAllMAGMatches', body)
            ).then((res) => {
                this.RemoveBusy("ClearAllMAGMatches");
                return res;
            },
                error => {
                    this.RemoveBusy("ClearAllMAGMatches");
                    this.modalService.GenericError(error);
                    return error;
                });
    }
    public ClearAllNonManualMAGMatches(attributeId: number): Promise<string> {

        this._BusyMethods.push("ClearAllNonManualMAGMatches");
        let body = JSON.stringify({ Value: attributeId });
      return lastValueFrom(this._httpC.post<string>(this._baseUrl + 'api/MagMatchAll/ClearAllNonManualMAGMatches', body)
            ).then((res) => {
                this.RemoveBusy("ClearAllNonManualMAGMatches");
                return res;
            },
                error => {
                    this.RemoveBusy("ClearAllNonManualMAGMatches");
                    this.modalService.GenericError(error);
                    return error;
                });
    }
    public ClearMagMatchItemsToPapers(itemId: number): Promise<string>{

        this._BusyMethods.push("ClearMagMatchItemsToPapers");
        let body = JSON.stringify({ Value: itemId });
      return lastValueFrom(this._httpC.post<string>(this._baseUrl + 'api/MagMatchAll/ClearMagMatchItemsToPapers', body)
            ).then((res) => {
                this.RemoveBusy("ClearMagMatchItemsToPapers");
                return res;
            },
                error => {
                    this.RemoveBusy("ClearMagMatchItemsToPapers");
                    this.modalService.GenericError(error);
                    return error;
                });
    }

    public Clear() {
        this.ListDescription = "";
        this._MagCurrentInfo = new MagCurrentInfo();
        this.AdvancedReviewInfo = new MagReviewMagInfo();
        this.CurrentMagSimId = 0;
        this._RunAlgorithmFirst = false;
    }
}

