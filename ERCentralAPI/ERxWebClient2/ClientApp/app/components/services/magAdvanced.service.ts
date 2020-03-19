import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ItemList, Criteria, Item } from './ItemList.service';

@Injectable({
    providedIn: 'root',
})

export class MAGAdvancedService extends BusyAwareService {
    
    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        private notificationService: NotificationService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
    private _magMatchedIncluded: number = 0;
    private _magMatchedExcluded: number = 0;
    private _magMatchedAll: number = 0;
    private _magMatchedWithThisCode: number = 0;

    public get magMatchedIncluded(): number {

        return this._magMatchedIncluded;
        
    }

    public set magMatchedIncluded(value: number) {

        this._magMatchedExcluded = value;
    }

    public get magMatchedExcluded(): number {

        return this._magMatchedExcluded;

    }

    public set magMatchedExcluded(value: number) {

        this._magMatchedExcluded = value;
    }

    public get magMatchedAll(): number {

        return this._magMatchedAll;

    }

    public set magMatchedAll(value: number) {

        this._magMatchedAll = value;
    }

    public get magMatchedWithThisCode(): number {

        return this._magMatchedWithThisCode;

    }

    public set magMatchedWithThisCode(value: number) {

        this._magMatchedWithThisCode = value;
    }

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

        this._BusyMethods.push("FetchClassifierContact");

        this._httpC.get<ClassifierContactModel[]>(this._baseUrl + 'api/MagClassifierContact/FetchClassifierContactList')
            .subscribe(result => {
                this.RemoveBusy("FetchClassifierContact");
                this.ClassifierContactModelList = result;
                console.log(result);
            },
                error => {
                    this.RemoveBusy("FetchClassifierContact");
                    this.modalService.GenericError(error);
                }
            );
    }

    FetchMagSimulationList() {

        this._BusyMethods.push("GetMagSimulationList");

        this._httpC.get<MagSimulation[]>(this._baseUrl + 'api/MagSimulationList/GetMagSimulationList')
            .subscribe(result => {
                this.RemoveBusy("GetMagSimulationList");
                this.MagSimulationList = result;
            },
                error => {
                    this.RemoveBusy("GetMagSimulationList");
                    this.modalService.GenericError(error);
                }
            );
    }


    FetchCurrentInfo() {

        this._BusyMethods.push("MagRelatedPapersRunFetch");

        this._httpC.get<MagCurrentInfo>(this._baseUrl + 'api/MagCurrentInfo/GetMagCurrentInfo')
			.subscribe(result => {
                this.RemoveBusy("MagRelatedPapersRunFetch");
                this.MagCurrentInfo = result;
			},
				error => {
                    this.RemoveBusy("MagRelatedPapersRunFetch");
					this.modalService.GenericError(error);
				}
			);
    }

    public currentMagPaper: MagPaper = new MagPaper();

    FetchMagPaper(Id: number) : Promise<void> {

        this._BusyMethods.push("FetchMagPaper");
        let body = JSON.stringify({ Value: Id });
        return this._httpC.post<MagPaper>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaper', body)
            .toPromise().then(result => {
                this.RemoveBusy("FetchMagPaper");
                this.currentMagPaper = result;
                // should call the relevant methods after the above
                //this.FetchMagPaperList(result.paperId);


                //console.log(result)
            },
                error => {
                    this.RemoveBusy("FetchMagPaper");
                    this.modalService.GenericError(error);
                    
                }
            );
    }

    RunMatchingAlgorithm() {

        this._BusyMethods.push("RunMatchingAlgorithm");
        this._httpC.get<any>(this._baseUrl + 'api/MagMatchAll/RunMatchingAlgorithm')
            .subscribe(result => {
                this.RemoveBusy("RunMatchingAlgorithm");
                
            },
                error => {
                    this.RemoveBusy("RunMatchingAlgorithm");
                    this.modalService.GenericError(error);
                }
            );
    }
    public ListDescription: string = '';
    public TotalNumberOfMatchedPapers: number = 0;
    private _Criteria: Criteria = new Criteria();


    FetchMAGMatchesWithCrit( listDescription: string): any {

        let SelectionCritieraItemList: Criteria = new Criteria();
        SelectionCritieraItemList.listType = "MagMatchesMatched";
        SelectionCritieraItemList.onlyIncluded = true;
        SelectionCritieraItemList.showDeleted = false;
        SelectionCritieraItemList.attributeSetIdList = "";
        SelectionCritieraItemList.pageNumber = 0;

        this._BusyMethods.push("FetchMAGMatchesWithCrit");

        this.ListDescription = listDescription;
        this._httpC.post<ItemList>(this._baseUrl + 'api/ItemList/Fetch', SelectionCritieraItemList)
            .toPromise().then(
                (list: ItemList) => {
                    this.RemoveBusy("FetchMAGMatchesWithCrit");
                    this._magMatchedIncluded = list.totalItemCount;
                    this.MagPapersMatchedList = list.items;
                    this.TotalNumberOfMatchedPapers = list.totalItemCount
                }
            );
    }

    FetchMAGMatchesWithCritEx(listDescription: string): any {

        let SelectionCritieraItemList: Criteria = new Criteria();
        SelectionCritieraItemList.listType = "MagMatchesMatched";
        SelectionCritieraItemList.onlyIncluded = false;
        SelectionCritieraItemList.showDeleted = false;
        SelectionCritieraItemList.attributeSetIdList = "";
        SelectionCritieraItemList.pageNumber = 0;

        this._BusyMethods.push("FetchMAGMatchesWithCritEx");

        this.ListDescription = listDescription;
        this._httpC.post<ItemList>(this._baseUrl + 'api/ItemList/Fetch', SelectionCritieraItemList)
            .toPromise().then(
                (list: ItemList) => {
                    this.RemoveBusy("FetchMAGMatchesWithCritEx");
                    this._magMatchedExcluded = list.totalItemCount;
                    this.MagPapersMatchedList = list.items;
                    this.TotalNumberOfMatchedPapers = list.totalItemCount
                }
            );
    }

    public MagPapersMatchedList: Item[] = [];
    public MagReferencesPaperList: MagPaperList = new MagPaperList();
    public MagCitationsPaperList: MagPaperList = new MagPaperList();
    public MagRelatedPaperList: MagPaperList = new MagPaperList();
    public MagPaperFieldsList: MagPaperList = new MagPaperList();

    public FetchMagPaperList(paperId: number, listType: string) {

        let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        //test here but need a switch based on listtype
        crit.included = 'Included';
        crit.listType = listType;
        crit.pageSize = 20;
        crit.magPaperId = paperId;


        this._BusyMethods.push("FetchMagPaperList");
        this._httpC.post<MagPaperList>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaperList', crit)
            .subscribe(result => {
                this.RemoveBusy("FetchMagPaperList");

                if (listType == 'PaperFieldsOfStudyList') {

                    this.MagPaperFieldsList = result;

                } else if (listType == 'CitedByList') {

                    this.MagCitationsPaperList = result;

                } else if (listType == 'CitationsList') {

                    this.MagReferencesPaperList = result;

                } else if (listType == 'PaperListById'){

                    //this.MagPapersMatchedList = result;

                }
                //this.MagRIDMatchedPaperList = result;
                console.log(result)
            },
                error => {
                    this.RemoveBusy("FetchMagPaperList");
                    this.modalService.GenericError(error);
                }
            );
    }


    FetchMagFieldOfStudyList(paperId: string) {

        let crit: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
        crit.fieldOfStudyId = 0;
        crit.listType = "PaperFieldOfStudyList";
        crit.paperIdList = paperId;
        crit.searchText = "";

        this._BusyMethods.push("FetchMagPaperList");
        this._httpC.post<MagPaperList>(this._baseUrl + 'api/MagCurrentInfo/GetMagFieldOfStudyList', crit)
            .subscribe(result => {
                this.RemoveBusy("MagPaperFieldsList");

                console.log(result)
                this.MagPaperFieldsList = result;

            },
                error => {
                    this.RemoveBusy("MagPaperFieldsList");
                    this.modalService.GenericError(error);
                }
            );
    }

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
    createdDate: Date = new Date;
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
    TP: number = 0;
    FP: number = 0;
    FN: number = 0;
    TN: number = 0;
}

