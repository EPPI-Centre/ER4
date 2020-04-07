import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Item } from './ItemList.service';
//import { MagList } from './BasicMAG.service';

@Injectable({
    providedIn: 'root',
})

export class MAGAdvancedService extends BusyAwareService {
    
    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
   
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

    private _MagCurrentInfo: MagCurrentInfo = new MagCurrentInfo();

    public get MagCurrentInfo(): MagCurrentInfo{

        return this._MagCurrentInfo;

    }

    public set MagCurrentInfo(magInfo: MagCurrentInfo) {
        this._MagCurrentInfo = magInfo;

    }

    private _MagSimulationList: MagSimulation[] = [];

    public get MagSimulationList(): MagSimulation[] {

        return this._MagSimulationList;
    }

    public set MagSimulationList(classifierContactModelList: MagSimulation[]) {
        this._MagSimulationList = classifierContactModelList;

    }

    private _ClassifierContactModelList: ClassifierContactModel[] = [];

    public get ClassifierContactModelList(): ClassifierContactModel[] {

        return this._ClassifierContactModelList;

    }

    public set ClassifierContactModelList(classifierContactModelList: ClassifierContactModel[]) {
        this._ClassifierContactModelList = classifierContactModelList;

    }
    FetchClassifierContactModelList() {
        console.log('advanced mag service 1');
        this._BusyMethods.push("FetchClassifierContactModelList");
        this._httpC.get<ClassifierContactModel[]>(this._baseUrl + 'api/MagClassifierContact/FetchClassifierContactList')
            .subscribe(result => {
                this.RemoveBusy("FetchClassifierContactModelList");
                this.ClassifierContactModelList = result;
                //console.log(result);
            },
                error => {
                    this.RemoveBusy("FetchClassifierContactModelList");
                    this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("FetchClassifierContactModelList");
            });
    }
    FetchMagSimulationList() {

        console.log('advanced mag service 2');
        this._BusyMethods.push("FetchMagSimulationList");
        this._httpC.get<MagSimulation[]>(this._baseUrl + 'api/MagSimulationList/GetMagSimulationList')
            .subscribe(result => {
                this.RemoveBusy("FetchMagSimulationList");
                this.MagSimulationList = result;
                //console.log('mag simulation list: ', result);
            },
            error => {
                this.RemoveBusy("FetchMagSimulationList");
                this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("FetchMagSimulationList");
            });
    }
    FetchMagReviewMagInfo() {

        console.log('advanced mag service 3');
        this._BusyMethods.push("FetchMagReviewMagInfo");
        this._httpC.get<MagReviewMagInfo>(this._baseUrl + 'api/MagCurrentInfo/GetMagReviewMagInfo')
            .subscribe(result => {
                this.RemoveBusy("FetchMagReviewMagInfo");
                this.AdvancedReviewInfo = result;
                //console.log('numbers in question: ',JSON.stringify(result));
            },
                error => {
                    this.RemoveBusy("FetchMagReviewMagInfo");
                    this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("FetchMagReviewMagInfo");
            });
    }
    FetchMagCurrentInfo() {

        console.log('advanced mag service 4');
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
    FetchMagPaperId(Id: number) : Promise<void> {

        console.log('advanced mag service 5');
        this._BusyMethods.push("FetchMagPaperId");
        let body = JSON.stringify({ Value: Id });
        return this._httpC.post<MagPaper>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaper', body)
            .toPromise().then(result => {
                this.RemoveBusy("FetchMagPaperId");
                this.currentMagPaper = result;
                // should call the relevant methods after the above
                if (result.paperId != null && result.paperId > 0) {
                    this.FetchMagPaperListId(result.paperId);
                }
                //console.log(result)
            },
                error => {
                    this.RemoveBusy("FetchMagPaperId");
                    this.modalService.GenericError(error);
                    
                }
            ).catch (
                    (error) => {

                        this.modalService.GenericErrorMessage("error with FetchMagPaperId");
                        this.RemoveBusy("FetchMagPaperId");
            });
    }

    //UpdateCurrentPaper(paperId : number) {

    //    this._BusyMethods.push("UpdateCurrentPaper");
    //    let body = JSON.stringify({ Value: paperId });
    //    return this._httpC.post<MagPaper>(this._baseUrl + 'api/MagCurrentInfo/UpdateMagPaper', body)
    //        .toPromise().then(result => {

    //            this.RemoveBusy("UpdateCurrentPaper");
    //            //this.currentMagPaper = result;
    //            //// should call the relevant methods after the above
    //            //if (result.paperId != null && result.paperId > 0) {
    //            //    this.FetchMagPaperListId(result.paperId);
    //            //}
    //            //console.log(result)
    //        },
    //            error => {
    //                this.RemoveBusy("UpdateCurrentPaper");
    //                this.modalService.GenericError(error);

    //            }
    //        );

    //}

    //CHECK IF THERE SHOULD BE A RETURN HERE
    RunMatchingAlgorithm() {

        console.log('advanced mag service 6');
        this._BusyMethods.push("RunMatchingAlgorithm");
        this._httpC.get<any>(this._baseUrl + 'api/MagMatchAll/RunMatchingAlgorithm')
            .subscribe(result => {
                this.RemoveBusy("RunMatchingAlgorithm");
                
            },
                error => {
                    this.RemoveBusy("RunMatchingAlgorithm");
                    this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("RunMatchingAlgorithm");
            });
    }
    public MagList: MagList = new MagList();
    public FetchMagPaperListId(paperId: number): Promise<void> {

        console.log('advanced mag service 7');
        let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        //test here but need a switch based on listtype
        //crit.included = 'Included';
        crit.listType = "CitationsList";
        crit.pageSize = 20;
        crit.magPaperId = paperId;
        
        this._BusyMethods.push("FetchMagPaperListId");
        return this._httpC.post<MagList>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaperList', crit)
            .toPromise().then(
            (result: MagList) => {
                this.RemoveBusy("FetchMagPaperListId");
                this.MagList = result;
                this.PaperIds = '';
                for (var i = 0; i < result.papers.length; i++) {
                    this.PaperIds += result.papers[i].paperId.toString() + ',';
                }
                this.FetchMagFieldOfStudyList(this.PaperIds);

            },
                error => {
                    this.RemoveBusy("FetchMagPaperListId");
                    this.modalService.GenericError(error);
                }
                ).catch (
                        (error) => {

                            this.modalService.GenericErrorMessage("error with FetchMagPaperListId");
                            this.RemoveBusy("FetchMagPaperListId");
            });
    }
    public FetchMagPaperList(crit: MVCMagPaperListSelectionCriteria): Promise<void> {

        console.log('advanced mag service 8');
        //console.log(crit);
        this._BusyMethods.push("FetchMagPaperList");
        return this._httpC.post<MagPaper[]>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaperList', crit)
            .toPromise().then(

            (result: MagPaper[]) => {
                this.RemoveBusy("FetchMagPaperList");
                if (crit.listType == 'CitationsList') {
                    //this.PaperIds = "";
                    this.MagCitationsPaperList = result;
                    //for (var i = 0; i < this.MagCitationsPaperList.length; i++) {
                    //    this.PaperIds += this.MagCitationsPaperList[i].paperId.toString() + ',';
                    //}
                    //this.PaperIds = this.PaperIds.substr(0, this.PaperIds.length - 1)
                    console.log(result);
                } else if (crit.listType == 'CitedByList') {

                    //console.log('got in here');
                    this.MagCitationsPaperList = result;
                    console.log(result);
                } else if (crit.listType == 'ReviewMatchedPapers') {
                    this.PaperIds = "";
                    this.ReviewMatchedPapersList = result;
                    //this._magListService.MAGList.papers = result;
                    //this._magListService.ListCriteria.listType = "ReviewMatchedPapers";
                    //this._magListService.ListCriteria.pageSize = 20;
                    for (var i = 0; i < this.ReviewMatchedPapersList.length; i++) {
                        this.PaperIds += this.ReviewMatchedPapersList[i].paperId.toString() + ',';
                    }
                    this.PaperIds = this.PaperIds.substr(0, this.PaperIds.length - 1)
                    //this._magListService.ListCriteria.paperIds = this.PaperIds;
                    console.log(result);
                    //console.log('rvm list: ', this.ReviewMatchedPapersList);
                    //console.log('rvm papers: ', this.PaperIds);
                } else if (crit.listType == 'ReviewMatchedPapersWithThisCode') {
                    this.PaperIds = "";
                    this.ReviewMatchedPapersList = result;
                    for (var i = 0; i < this.ReviewMatchedPapersList.length; i++) {
                        this.PaperIds += this.ReviewMatchedPapersList[i].paperId.toString() + ',';
                    }
                    this.PaperIds = this.PaperIds.substr(0, this.PaperIds.length - 1)
                    //console.log('rvm list: ', this.ReviewMatchedPapersList);
                    //console.log('rvm papers: ', this.PaperIds);
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
    // I THINK THIS IS NO LONGER NEEDED AFTER THE REFACTOR...WILL CHECK LATER ON...
    FetchMagFieldOfStudyList(paperIds: string): Promise<void>{

        console.log('advanced mag service 9');
        let crit: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
        crit.fieldOfStudyId = 0;
        crit.listType = "PaperFieldOfStudyList";
        crit.paperIdList = paperIds;
        //THIS SEARCH TEXT NEEDS TO COME IN FROM THE FRONT
        crit.searchText = ''; //searchText;

        this._BusyMethods.push("FetchMagPaperList");
        return this._httpC.post<MagPaper[]>(this._baseUrl + 'api/MagCurrentInfo/GetMagFieldOfStudyList', crit)
            .toPromise().then(
            (result: MagPaper[]) => {
                this.RemoveBusy("MagPaperFieldsList");
                this.MagPaperFieldsList = result;
                console.log('paper field list: ', result);
            },
                error => {
                    this.RemoveBusy("MagPaperFieldsList");
                    this.modalService.GenericError(error);
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with MagPaperFieldsList");
                    this.RemoveBusy("MagPaperFieldsList");
                });
    }
    public DeleteSimulation(item: MagSimulation) {

        console.log('advanced mag service 10');
        //console.log(item.magSimulationId);
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

                    this.modalService.GenericErrorMessage("error with DeleteSimulation");
                    this.RemoveBusy("DeleteSimulation");
            });
    }
    public AddMagSimulation(newMagSimulation: MagSimulation) {

        console.log('advanced mag service 11');
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

export class MagReviewMagInfo {

    reviewId: number = 0;
    nInReviewIncluded: number = 0;
    nInReviewExcluded: number = 0;
    nMatchedAccuratelyIncluded: number = 0;
    nMatchedAccuratelyExcluded: number = 0;
    nRequiringManualCheckIncluded: number = 0;
    nRequiringManualCheckExcluded: number = 0;
    nNotMatchedIncluded: number = 0;
    nNotMatchedExcluded: number = 0;
    
}

export class MagPaperList {

    pageIndex : number = 0;
    totalItemCount: number = 0;
    pageSize: number = 0;
    isPageChanging: boolean = false;
    fieldOfStudyId: number = 0;
    paperId: number = 0;
    authorId: number = 0;
    magRelatedRunId: number = 0;
    paperIds: string = '';
    includedOrExcluded: string = '';
    attributeIds: string = '';
}



export class MVCMagFieldOfStudyListSelectionCriteria {

   fieldOfStudyId: number = 0;
   listType: string = '';
   paperIdList: string = '';
   searchText: string = '';

}

export class MVCMagPaperListSelectionCriteria {
       
    magPaperId: number = 0;
    iTEM_ID: number = 0;
    listType: string = '';
    fieldOfStudyId: number = 0;
    authorId: number = 0;
    magRelatedRunId: number = 0;
    paperIds: string = '';
    attributeIds: string = '';
    included: string = '';
    pageNumber: number = 0;
    pageSize: number = 0;
    numResults: number = 0;

}

export class MvcMagFieldOfStudyListSelectionCriteria {

    fieldOfStudyId: number = 0;
    listType: string = '';
    paperIdList: string = '';
    searchText: string = '';

}

export class ClassifierContactModel {

    modelId: number = 0;
    modelTitle: string = '';
    contactId: number = 0;
    reviewId: number = 0;
    reviewName: string = '';
    contactName: string = '';
    attributeOn: string = '';
    attributeNotOn: string = '';
    accuracy: number = 0;
    auc: number = 0;
    precision: number = 0;
    recall: number = 0;

}

export class MagFieldOfStudy {

    fieldOfStudyId : number  = 0;
    rank: number = 0;
    normalizedName: string = '';
    displayName: string = '';
    mainType: string = '';
    level: number = 0;
    paperCount: number = 0;
    citationCount: number = 0;
    createdDate: Date = new Date();
    num_times: number = 0;
    externalMagLink: string = '';

}

export class MagPaper {

    externalMagLink: string = '';
    fullRecord: string = '';
    paperId: number = 0;
    doi: string = '';
    docType: string = '';
    paperTitle: string = '';
    originalTitle: string = '';
    bookTitle: string = '';
    year: number = 0;
    smartDate: Date = new Date();
    journalId: number = 0;
    journal: string = '';
    conferenceSeriesId: number = 0;
    conferenceInstanceId: number = 0;
    volume: string = '';
    issue: string = '';
    firstPage: string = '';
    lastPage: string = '';
    referenceCount: number = 0;
    references: number = 0;
    citationCount: number = 0;
    estimatedCitationCount: number = 0;
    createdDate: number = 0;
    authors: string = '';
    urls: string = '';
    pdfLinks: string = '';
    linkedITEM_ID: number = 0;
    isSelected: boolean = false;
    canBeSelected: boolean = false;
    abstract: string = '';
    autoMatchScore: number = 0;
    manualTrueMatch: boolean = false;
    manualFalseMatch: boolean = false;
    findOnWeb: string = '';
}

export class MagCurrentInfo {

    currentAvailability: string = '';
    lastUpdated: Date = new Date(2000, 2, 10);

}

export class MagSimulation {

    magSimulationId: number = 0;
    reviewId: number = 0;
    year: number = 0;
    createdDate: Date = new Date();
    withThisAttributeId: number = 0;
    filteredByAttributeId: number = 0;
    searchMethod: string = '';
    networkStatistic: string = '';
    studyTypeClassifier: string = '';
    userClassifierModelId: number = 0;
    status: string = '';
    withThisAttribute: string = '';
    filteredByAttribute: string = '';
    userClassifierModel: string = '';
    tp: number = 0;
    fp: number = 0;
    fn: number = 0;
    tn: number = 0;
}

export class MagList {

    pagesize: number = 0;
    paperIds: string = '';
    pagecount: number = 0;
    pageindex: number = 0;
    totalItemCount: number = 0;
    papers: MagPaper[] = [];

}