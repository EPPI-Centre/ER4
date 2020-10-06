import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Item } from './ItemList.service';
import { MAGBrowserService } from './MAGBrowser.service';
import { MagPaper, MagReviewMagInfo, MVCMagPaperListSelectionCriteria,  
    ClassifierContactModel, MVCMagFieldOfStudyListSelectionCriteria, MagList,
    MagCheckContReviewRunningCommand, MagFieldOfStudy, MagCurrentInfo} from './MAGClasses.service';
import { Router } from '@angular/router';


@Injectable({
    providedIn: 'root',
})

export class MAGAdvancedService extends BusyAwareService {
    
    constructor(
        private _httpC: HttpClient,
        private _magBrowserService: MAGBrowserService,
        private modalService: ModalService,
        private router: Router,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
    public _RunAlgorithmFirst: boolean = false;
    public ReviewMatchedPapersList: MagPaper[] = [];
    public AdvancedReviewInfo: MagReviewMagInfo = new MagReviewMagInfo();
    public currentMagPaper: MagPaper = new MagPaper();
    public ListDescription: string = '';
    public TotalNumberOfMatchedPapers: number = 0;
    public MagPapersMatchedList: Item[] = [];
    public MagReferencesPaperList: MagList = new MagList();
    public MagCitationsPaperList: MagPaper[] = [];
    public MagRelatedPaperList: MagPaper[] = [];
    public MagPaperFieldsList: MagPaper[] = [];
    public CurrentCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    public CurrentMagSimId: number = 0;
    public PaperIds: string = '';
    public MagList: MagList = new MagList();
    private _MagCurrentInfo: MagCurrentInfo = new MagCurrentInfo();
    private _ClassifierContactModelList: ClassifierContactModel[] = [];
    public get MagCurrentInfo(): MagCurrentInfo{
        return this._MagCurrentInfo;
    }
    public set MagCurrentInfo(magInfo: MagCurrentInfo) {
        this._MagCurrentInfo = magInfo;
    }
    public get ClassifierContactModelList(): ClassifierContactModel[] {
        return this._ClassifierContactModelList;
    }
    public set ClassifierContactModelList(classifierContactModelList: ClassifierContactModel[]) {
        this._ClassifierContactModelList = classifierContactModelList;
    }

    public UpdateMagPaper(matchCorrect: boolean, paperId: number, itemId: number): Promise<MagPaper[]> {

        this._BusyMethods.push("UpdateMagPaper");
        let body = JSON.stringify({ manualTrueMatchProperty: matchCorrect, magPaperId:  paperId, itemId: itemId});
        return this._httpC.post<MagPaper>(this._baseUrl + 'api/MagPaperList/UpdateMagPaper', body)
            .toPromise().then((result: MagPaper) => {
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
        return this._httpC.post<MagList>(this._baseUrl + 'api/MagPaperList/GetMagPaperList', crit)
            .toPromise().then(

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
    public FetchMagPaperListMagPaper(crit: MVCMagPaperListSelectionCriteria): Promise<MagList> {
        this._BusyMethods.push("FetchMagPaperListMagPaper");
        return this._httpC.post<MagList>(this._baseUrl + 'api/MagPaperList/GetMagPaperList', crit)
            .toPromise().then(

                (result: MagList) => {

                    this.RemoveBusy("FetchMagPaperListMagPaper");

                    if (crit.listType == 'ReviewMatchedPapers' || crit.listType == 'ReviewMatchedPapersWithThisCode') {
                        this.ReviewMatchedPapersList = result.papers;
                        for (var i = 0; i < this.ReviewMatchedPapersList.length; i++) {
                            this.PaperIds += this.ReviewMatchedPapersList[i].paperId.toString() + ',';
                        }
                        this.PaperIds = this.PaperIds.substr(0, this.PaperIds.length - 1)
                        this.CurrentCriteria = crit;

                    } 
                    return result.papers;
                },
                error => {
                    this.RemoveBusy("FetchMagPaperListMagPaper");
                    this.modalService.GenericError(error);
                    return error;
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchMagPaperListMagPaper");
                    this.RemoveBusy("FetchMagPaperListMagPaper");
                    return error;
                });
    }
    public FetchMagPaperId(Id: number): Promise<MagPaper> {

        this._magBrowserService.ClearTopics();
        this._BusyMethods.push("FetchMagPaperId");
        let body = JSON.stringify({ Value: Id });
        return this._httpC.post<MagPaper>(this._baseUrl + 'api/MagPaperList/GetMagPaper', body)
            .toPromise().then(result => {

                this.RemoveBusy("FetchMagPaperId");
                this.currentMagPaper = result;

                return result;
                
            },
                error => {
                    this.RemoveBusy("FetchMagPaperId");
                    this.modalService.GenericError(error);
                    return error;

                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchMagPaperId");
                    this.RemoveBusy("FetchMagPaperId");
                });
    }
    public PostFetchMagPaperCalls(result: MagPaper, listType: string) {
           if (result.paperId != null && result.paperId > 0) {

                let criteriaCitationsList: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
                criteriaCitationsList.listType = "CitationsList";
                criteriaCitationsList.magPaperId = result.paperId;
                criteriaCitationsList.pageSize = 20;

                this._magBrowserService.FetchWithCrit(criteriaCitationsList, "CitationsList").then(

                    (res: boolean) => {

                        if (res) {
                            this.PaperIds = this._magBrowserService.ListCriteria.paperIds;
                            let criteriaCitedBy: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
                            criteriaCitedBy.listType = "CitedByList";
                            criteriaCitedBy.magPaperId = result.paperId;
                            criteriaCitedBy.pageSize = 20;
                            this._magBrowserService.FetchWithCrit(criteriaCitedBy, "CitedByList").then(

                                (res: boolean) => {

                                    if (this.currentMagPaper.paperId > -1) {

                                            this.PaperIds = this._magBrowserService.ListCriteria.paperIds;
                                            let criteriaFOS: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
                                            criteriaFOS.fieldOfStudyId = 0;
                                            criteriaFOS.listType = 'PaperFieldOfStudyList';
                                            criteriaFOS.paperIdList = result.paperId.toString();
                                            criteriaFOS.SearchTextTopics = ''; //TODO this will be populated by the user..
                                            this._magBrowserService.FetchMagFieldOfStudyList(criteriaFOS, listType).then(

                                                (res: MagFieldOfStudy[]) => {
                                                    this.router.navigate(['MAGBrowser']);

                                                }
                                        );

                                    } else {

                                         this._magBrowserService.FetchOrigWithCrit(criteriaCitedBy, "CitedByList").then(

                                         () => {

                                                if (res) {
                                                    this.PaperIds = this._magBrowserService.ListCriteria.paperIds;
                                                    let criteriaFOS: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
                                                    criteriaFOS.fieldOfStudyId = 0;
                                                    criteriaFOS.listType = 'PaperFieldOfStudyList';
                                                    criteriaFOS.paperIdList = result.paperId.toString();
                                                    criteriaFOS.SearchTextTopics = ''; //TODO this will be populated by the user..
                                                    this._magBrowserService.FetchMagFieldOfStudyList(criteriaFOS, listType).then(

                                                        (res: MagFieldOfStudy[]) => {
                                                            this.router.navigate(['MAGBrowser']);

                                                        }
                                                    );
                                                }
                                            }
                                        )
                                    }
                                });
                        }                       
                    }
                );
            }
    }
    public CheckContReviewPipelineState(): Promise<boolean> {

        this._BusyMethods.push("CheckContReviewPipelineState");
        return this._httpC.get<MagCheckContReviewRunningCommand>(this._baseUrl + 'api/MagCurrentInfo/MagCheckContReviewRunningCommand')
            .toPromise().then(
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
    public FetchClassifierContactModelList() {
        this._BusyMethods.push("FetchClassifierContactModelList");
        this._httpC.get<ClassifierContactModel[]>(this._baseUrl + 'api/MagClassifierContact/FetchClassifierContactList')
            .subscribe(result => {
                this.RemoveBusy("FetchClassifierContactModelList");
                if (result != null) {
                    this.ClassifierContactModelList = result;
                }
            },
                error => {
                    this.RemoveBusy("FetchClassifierContactModelList");
                    this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("FetchClassifierContactModelList");
            });
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
        return this._httpC.post<string>(this._baseUrl + 'api/MagMatchAll/RunMatchingAlgorithm', body)
            .toPromise().then(

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
        return this._httpC.post<MagList>(this._baseUrl + 'api/MagMatchAll/MagMatchItemsToPapers', body)
            .toPromise().then(() => {
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
        return this._httpC.post<string>(this._baseUrl + 'api/MagMatchAll/ClearAllMAGMatches', body)
            .toPromise().then((res) => {
                this.RemoveBusy("ClearAllMAGMatches");
                return res;
            },
                error => {
                    this.RemoveBusy("ClearAllMAGMatches");
                    this.modalService.GenericError(error);
                    return error;
                });
    }
    public ClearMagMatchItemsToPapers(itemId: number): Promise<string>{

        this._BusyMethods.push("ClearMagMatchItemsToPapers");
        let body = JSON.stringify({ Value: itemId });
        return this._httpC.post<string>(this._baseUrl + 'api/MagMatchAll/ClearMagMatchItemsToPapers', body)
            .toPromise().then((res) => {
                this.RemoveBusy("ClearMagMatchItemsToPapers");
                return res;
            },
                error => {
                    this.RemoveBusy("ClearMagMatchItemsToPapers");
                    this.modalService.GenericError(error);
                    return error;
                });
    }

}

