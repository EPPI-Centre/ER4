import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { NotificationService } from '@progress/kendo-angular-notification';

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

        this._httpC.get<ClassifierContactModel[]>(this._baseUrl + 'api/MagClassifierContactController/FetchClassifierContactList')
            .subscribe(result => {
                this.RemoveBusy("FetchClassifierContact");
                //this.ClassifierContactModelList = result;
                //console.log(result);
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

    FetchMagPaper(Id: number) {

        console.log(Id)
        this._BusyMethods.push("FetchMagPaper");
        let body = JSON.stringify({ Value: Id });
        this._httpC.post<MagPaper>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaper', body)
            .subscribe(result => {
                this.RemoveBusy("FetchMagPaper");
                this.currentMagPaper = result;
                console.log(result)
            },
                error => {
                    this.RemoveBusy("FetchMagPaper");
                    this.modalService.GenericError(error);
                }
            );
    }
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

