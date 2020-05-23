import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Item } from './ItemList.service';
import { MAGBrowserService } from './MAGBrowser.service';
import { MagPaper, MagReviewMagInfo, MVCMagPaperListSelectionCriteria, MagCurrentInfo, MagSimulation,
        ClassifierContactModel, MVCMagFieldOfStudyListSelectionCriteria, MagList } from './MAGClasses.service';

@Injectable({
    providedIn: 'root',
})

export class MAGAdvancedService extends BusyAwareService {
    
    constructor(
        private _httpC: HttpClient,
        private _magBrowserService: MAGBrowserService,
        private modalService: ModalService,
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
    public MagReferencesPaperList: MagPaper[] = [];
    public MagCitationsPaperList: MagPaper[] = [];
    public MagRelatedPaperList: MagPaper[] = [];
    public MagPaperFieldsList: MagPaper[] = [];
    public CurrentCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    public CurrentMagSimId: number = 0;
    public PaperIds: string = '';
    public MagList: MagList = new MagList();
    private _MagCurrentInfo: MagCurrentInfo = new MagCurrentInfo();
    private _ClassifierContactModelList: ClassifierContactModel[] = [];
    private _MagSimulationList: MagSimulation[] = [];

    public get MagCurrentInfo(): MagCurrentInfo{
        return this._MagCurrentInfo;
    }
    public set MagCurrentInfo(magInfo: MagCurrentInfo) {
        this._MagCurrentInfo = magInfo;
    }
    public get MagSimulationList(): MagSimulation[] {
        return this._MagSimulationList;
    }
    public set MagSimulationList(classifierContactModelList: MagSimulation[]) {
        this._MagSimulationList = classifierContactModelList;
    }
    public get ClassifierContactModelList(): ClassifierContactModel[] {
        return this._ClassifierContactModelList;
    }
    public set ClassifierContactModelList(classifierContactModelList: ClassifierContactModel[]) {
        this._ClassifierContactModelList = classifierContactModelList;
    }
    public FetchClassifierContactModelList() {
        this._BusyMethods.push("FetchClassifierContactModelList");
        this._httpC.get<ClassifierContactModel[]>(this._baseUrl + 'api/MagClassifierContact/FetchClassifierContactList')
            .subscribe(result => {
                this.RemoveBusy("FetchClassifierContactModelList");
                this.ClassifierContactModelList = result;
            },
                error => {
                    this.RemoveBusy("FetchClassifierContactModelList");
                    this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("FetchClassifierContactModelList");
            });
    }
    public FetchMagSimulationList() {
        this._BusyMethods.push("FetchMagSimulationList");
        this._httpC.get<MagSimulation[]>(this._baseUrl + 'api/MagSimulationList/GetMagSimulationList')
            .subscribe(result => {
                this.RemoveBusy("FetchMagSimulationList");
                this.MagSimulationList = result;
            },
            error => {
                this.RemoveBusy("FetchMagSimulationList");
                this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("FetchMagSimulationList");
            });
    }
    public FetchMagReviewMagInfo() {
        this._BusyMethods.push("FetchMagReviewMagInfo");
        this._httpC.get<MagReviewMagInfo>(this._baseUrl + 'api/MagCurrentInfo/GetMagReviewMagInfo')
            .subscribe(result => {
                this.RemoveBusy("FetchMagReviewMagInfo");
                this.AdvancedReviewInfo = result;
            },
                error => {
                    this.RemoveBusy("FetchMagReviewMagInfo");
                    this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("FetchMagReviewMagInfo");
            });
    }
    public FetchMagCurrentInfo() {
        this._BusyMethods.push("FetchMagCurrentInfo");
        this._httpC.get<MagCurrentInfo>(this._baseUrl + 'api/MagCurrentInfo/GetMagCurrentInfo')
			.subscribe(result => {
                this.RemoveBusy("FetchMagCurrentInfo");
                this.MagCurrentInfo = result;
			},
				error => {
                    this.RemoveBusy("FetchMagCurrentInfo");
					this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("FetchMagCurrentInfo");
            });
    }
    public FetchMagPaperId(Id: number) : Promise<string> {

        this._magBrowserService.WPChildTopics = [];
        this._magBrowserService.WPParentTopics = [];
        this._magBrowserService.ParentTopic = '';
        this._BusyMethods.push("FetchMagPaperId");
        let body = JSON.stringify({ Value: Id });
        return this._httpC.post<MagPaper>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaper', body)
            .toPromise().then(result => {
                this.RemoveBusy("FetchMagPaperId");
                this.currentMagPaper = result;
                
                if (result.paperId != null && result.paperId > 0) {

                    let criteriaCitationsList: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
                    criteriaCitationsList.listType = "CitationsList";
                    criteriaCitationsList.magPaperId = result.paperId;
                    criteriaCitationsList.pageSize = 20;

                    this._magBrowserService.FetchWithCrit(criteriaCitationsList, "CitationsList").then(

                        () => {

                            this.PaperIds = this._magBrowserService.ListCriteria.paperIds;
                            let criteriaCitedBy: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
                            criteriaCitedBy.listType = "CitedByList";
                            criteriaCitedBy.magPaperId = result.paperId;
                            criteriaCitedBy.pageSize = 20;
                            this._magBrowserService.FetchWithCrit(criteriaCitedBy, "CitedByList").then(

                                () => {
                                    this.PaperIds = this._magBrowserService.ListCriteria.paperIds;
                                    let criteriaFOS: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
                                    criteriaFOS.fieldOfStudyId = 0;
                                    criteriaFOS.listType = 'PaperFieldOfStudyList';
                                    criteriaFOS.paperIdList = this.PaperIds;
                                    criteriaFOS.SearchTextTopics = ''; //TODO this will be populated by the user..
                                    this._magBrowserService.FetchMagFieldOfStudyList(criteriaFOS, 'CitationsList').then(

                                        () => { return; }
                                    );

                            });
                        }
                    );
                }
            },
                error => {
                    this.RemoveBusy("FetchMagPaperId");
                    this.modalService.GenericError(error);
                    return error;
                    
                }
            ).catch (
                    (error) => {

                        this.modalService.GenericErrorMessage("error with FetchMagPaperId");
                        this.RemoveBusy("FetchMagPaperId");
            });
    }
    public RunMatchingAlgorithm(attributeId: number) : string {

        this._BusyMethods.push("RunMatchingAlgorithm");
        let body = JSON.stringify({ Value: attributeId});
        this._httpC.post<boolean>(this._baseUrl + 'api/MagMatchAll/RunMatchingAlgorithm', body )
            .subscribe(result => {
                this.RemoveBusy("RunMatchingAlgorithm");
                console.log('returned form matching algo' + result);
                return result;
            },
                error => {
                    this.RemoveBusy("RunMatchingAlgorithm");
                    this.modalService.GenericError(error);
                    return error;
            },
            () => {
                this.RemoveBusy("RunMatchingAlgorithm");
                return ;
            });
        return "error";
    }
    public FetchMagPaperList(crit: MVCMagPaperListSelectionCriteria): Promise<void> {
        this._BusyMethods.push("FetchMagPaperList");
        return this._httpC.post<MagPaper[]>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaperList', crit)
            .toPromise().then(

            (result: MagPaper[]) => {
                this.RemoveBusy("FetchMagPaperList");
                if (crit.listType == 'CitationsList') {
                    this.MagCitationsPaperList = result;
                } else if (crit.listType == 'CitedByList') {
                    this.MagCitationsPaperList = result;
                } else if (crit.listType == 'ReviewMatchedPapers') {
                    this.ReviewMatchedPapersList = result;
                    for (var i = 0; i < this.ReviewMatchedPapersList.length; i++) {
                        this.PaperIds += this.ReviewMatchedPapersList[i].paperId.toString() + ',';
                    }
                    this.PaperIds = this.PaperIds.substr(0, this.PaperIds.length - 1)
                } else if (crit.listType == 'ReviewMatchedPapersWithThisCode') {
                    this.PaperIds = "";
                    this.ReviewMatchedPapersList = result;
                    for (var i = 0; i < this.ReviewMatchedPapersList.length; i++) {
                        this.PaperIds += this.ReviewMatchedPapersList[i].paperId.toString() + ',';
                    }
                    this.PaperIds = this.PaperIds.substr(0, this.PaperIds.length - 1)
                    this.CurrentCriteria = crit;
                }           

            },
                error => {
                    this.RemoveBusy("FetchMagPaperList");
                    this.modalService.GenericError(error);
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchMagPaperList");
                    this.RemoveBusy("FetchMagPaperList");
                });
    }
    public DeleteSimulation(item: MagSimulation) {

        this._BusyMethods.push("DeleteSimulation");
        let body = JSON.stringify({Value: item.magSimulationId });
        return this._httpC.post<MagSimulation>(this._baseUrl + 'api/MAGSimulationList/DeleteMagSimulation', body)
            .toPromise().then(
                (result: MagSimulation) => {

                    this.RemoveBusy("DeleteSimulation");
                    if (result != null && result.magSimulationId > 0) {

                        let ind: number = this.MagSimulationList.findIndex((x) => x.magSimulationId == item.magSimulationId);
                            if (ind > 0) {
                                this.MagSimulationList.splice(ind, 1);
                            }
                    }
                },
                error => {
                    this.RemoveBusy("DeleteSimulation");
                    this.modalService.GenericError(error);
                }
                ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with DeleteSimulation: " + error);
                    this.RemoveBusy("DeleteSimulation");
            });
    }
    public AddMagSimulation(newMagSimulation: MagSimulation) {
        this._BusyMethods.push("AddMagSimulation");
        return this._httpC.post<MagSimulation>(this._baseUrl + 'api/MAGSimulationList/CreateMagSimulation', newMagSimulation)
            .toPromise().then(
            (result: MagSimulation) => {

                this.RemoveBusy("AddMagSimulation");
                    if (this.MagSimulationList != null && this.MagSimulationList.length > 0) {

                        this.MagSimulationList.push(result);
                    }
                },
                error => {
                    this.RemoveBusy("AddMagSimulation");
                    this.modalService.GenericError(error);
                }
            ).catch(
            (error) => {

                this.modalService.GenericErrorMessage("error with AddMagSimulation");
                this.RemoveBusy("AddMagSimulation");
            });
    }

}
