import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient,  } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
//import { MagPaper, MVCMagPaperListSelectionCriteria,  MVCMagFieldOfStudyListSelectionCriteria, MagFieldOfStudy } from './magAdvanced.service';
import { MagList } from './BasicMAG.service';


@Injectable({

	providedIn: 'root',

    }
)

export class MAGBrowserService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private modalService: ModalService,
    ) {
		super();
		
	}
    	
    private _MAGList: MagList = new MagList();
    private _Criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    private _currentPaper: MagPaper = new MagPaper();
 
    public ListDescription: string = "";
    @Output() PaperChanged = new EventEmitter();

    public get MAGList(): MagList {
        return this._MAGList;
    }
    public set MAGList(value: MagList) {
        this._MAGList = value;
    }
    public get ListCriteria(): MVCMagPaperListSelectionCriteria {
        return this._Criteria;
    }
    public set ListCriteria(value: MVCMagPaperListSelectionCriteria) {
        this._Criteria = value;
    }
    public get currentPaper(): MagPaper {
        return this._currentPaper;
    }
    FetchMAGRelatedPaperRunsListId(Id: number): Promise<void> {

        console.log('MAGList service FetchMAGRelatedPaperRunsListId 1');
        this._BusyMethods.push("FetchMAGRelatedPaperRunsListId");
        this.ListCriteria.listType = "MagRelatedPapersRunList";
        this.ListCriteria.pageSize = 20;
        this.ListCriteria.magRelatedRunId = Id;

        return this._httpC.post<MagList>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRunsId',
            this.ListCriteria)
            .toPromise().then(
                (result) => {

                    console.log('', result);
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                    this.MAGList = result;
                    this.ListCriteria.paperIds = '';
                    for (var i = 0; i < result.papers.length; i++) {
                       
                        this.ListCriteria.paperIds += result.papers[i].paperId.toString() + ',';
                    }
                    this.ListCriteria.paperIds = this.ListCriteria.paperIds.substr(0, this.ListCriteria.paperIds.length - 1);

                    this.ListCriteria.pageNumber += 1;
                    this.SavePapers(result, this.ListCriteria);
                    let FieldsListcriteria: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
                    FieldsListcriteria.fieldOfStudyId = 0;
                    FieldsListcriteria.listType = "PaperFieldOfStudyList";
                    FieldsListcriteria.paperIdList = this.ListCriteria.paperIds;
                    //THIS SEARCH TEXT NEEDS TO COME IN FROM THE FRONT
                    FieldsListcriteria.searchText = ''; //searchText;
                    this.FetchMagFieldOfStudyList(FieldsListcriteria);

                },
                error => {
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                    //this.modalService.GenericError(error);
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchMAGRelatedPaperRunsListId");
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                });
    }
    public MagPaperFieldsList: MagFieldOfStudy[] = [];
    FetchMagFieldOfStudyList(criteria: MVCMagFieldOfStudyListSelectionCriteria): Promise<MagFieldOfStudy[] | void> {

        console.log('MAGList service 2');

        this._BusyMethods.push("FetchMagPaperList");
        return this._httpC.post<MagFieldOfStudy[]>(this._baseUrl + 'api/MagCurrentInfo/GetMagFieldOfStudyList', criteria)
            .toPromise().then(
            (result: MagFieldOfStudy[]) => {

                this.RemoveBusy("FetchMagPaperList");
                    this.MagPaperFieldsList = result;
                    console.log('paper field list: ', result);
                    return result;
                },
                error => {
                    this.RemoveBusy("FetchMagPaperList");
                    //this.modalService.GenericError(error);
                    
                }
            ).catch(
            (error) => {

                this.modalService.GenericErrorMessage("error with FetchMagPaperList");
                this.RemoveBusy("FetchMagPaperList");
            });
    }
    public FetchWithCrit(crit: MVCMagPaperListSelectionCriteria, listDescription: string): Promise<void> {

        console.log('MAGList service 3');
        this._BusyMethods.push("FetchWithCrit");
        this._Criteria = crit;
        if (this._MAGList && this._MAGList.pagesize > 0
            && this._MAGList.pagesize <= 4000
            && this._MAGList.pagesize != crit.pageSize
        ) {
            crit.pageSize = this._MAGList.pagesize;
        }
        console.log('criteria are: ', crit);
        this.ListDescription = listDescription;
        return this._httpC.post<MagList>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaperList', crit)
            .toPromise().then(

            (list: MagList) => {
                    this.RemoveBusy("FetchWithCrit");
                    console.log('list result from controller are: ', list);
                    console.log('resultant crtiteria: ',this._Criteria);
                    this.SavePapers(list, this._Criteria);

                    //console.log('aksdjh: CHEKC: ', JSON.stringify(this.MAGList.papers.length));

                }, error => {
                    this.modalService.GenericError(error);
                    this.RemoveBusy("FetchWithCrit");
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchWithCrit");
                    this.RemoveBusy("FetchWithCrit");
            });
	}

	
    public Refresh() {
        if (this._Criteria && this._Criteria.listType && this._Criteria.listType != "") {
            //we have something to do
            this.FetchWithCrit(this._Criteria, this.ListDescription);
        }
    }


    public GetIncludedPapers() {
        let cr: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        cr.listType = 'ReviewMatchedPapers';
        cr.included = 'true';
        this.FetchWithCrit(cr, "Included papers");
    }
    public GetExcludedPapers() {
        let cr: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        cr.listType = 'ReviewMatchedPapers';
        cr.included = 'false';
        this.FetchWithCrit(cr, "Excluded papers");
    }


    public SavePapers(list: MagList, crit: MVCMagPaperListSelectionCriteria) {

        console.log('Inside savepapers list detail is: ', list);

        //papers = orderBy(papers, this.sort); 

        this._MAGList = list;
        this._Criteria = crit;
  
    }
    private ChangingPaper(newPaper: MagPaper) {

        this._currentPaper = newPaper;
		this.PaperChanged.emit(newPaper);
    }
	public getPaper(paperId: number): MagPaper {

        console.log('getting MagPaper');
        let ff = this.MAGList.papers.find(found => found.paperId == paperId);
        if (ff != undefined && ff != null) {
            console.log('first emit');
            this.ChangingPaper(ff);
            return ff;
        }
        else {
            this.ChangingPaper(new MagPaper());
            return new MagPaper();
        }
    }
    public hasPrevious(paperId: number): boolean {
        //if (!this.IsInScreeningMode && this._PriorityScreeningService && this._PriorityScreeningService.TrainingList && this._PriorityScreeningService.TrainingList.length > 0) {
        //    return this._PriorityScreeningService.HasPrevious();
        //}
        //else {
            let ff = this.MAGList.papers.findIndex(found => found.paperId == paperId);
            if (ff != undefined && ff != null && ff > 0 && ff != -1) {
                //console.log('Has prev (yes)' + ff);
                return true;
            }
            else {
                //console.log('Has prev (no)' + ff);
                return false;
            }
        //}
    }
    public getFirst(): MagPaper {
        let ff = this.MAGList.papers[0];
        if (ff != undefined && ff != null) {
            //this.ChangingPaper(ff);
            return ff;
        }
        else {
            //this.ChangingPaper(new MagPaper());
            return new MagPaper();
        }
    }
    public getPrevious(paperId: number): MagPaper {
        
        let ff = this.MAGList.papers.findIndex(found => found.paperId == paperId);
        if (ff != undefined && ff != null && ff > -1 && ff < this._MAGList.papers.length) {
            //this.ChangingPaper(this._MAGList.papers[ff - 1]);
            return this._MAGList.papers[ff - 1];
        }
        else {
            //this.ChangingPaper(new MagPaper());
            return new MagPaper();
        }
        
    }
    public hasNext(paperId: number): boolean {
        //if (!this.IsInScreeningMode && this._PriorityScreeningService && this._PriorityScreeningService.TrainingList && this._PriorityScreeningService.TrainingList.length > 0) {
        //    return this._PriorityScreeningService.HasNext();
        //}
        //else {
            let ff = this.MAGList.papers.findIndex(found => found.paperId == paperId);
            if (ff != undefined && ff != null && ff > -1 && ff + 1 < this._MAGList.papers.length) return true;
            else return false;
        //}
    }
    public getNext(paperId: number): MagPaper {
        //console.log('getNext');
        let ff = this.MAGList.papers.findIndex(found => found.paperId == paperId);
        //console.log(ff);
        if (ff != undefined && ff != null && ff > -1 && ff + 1 < this._MAGList.papers.length) {
            //console.log('I am emitting');
            //this.ChangingPaper(this._MAGList.papers[ff + 1]);
            return this._MAGList.papers[ff + 1];
        }
        else {
            //this.ChangingPaper(new MagPaper());
            return new MagPaper();
        }
	}
    public getLast(): MagPaper {
        let ff = this.MAGList.papers[this._MAGList.papers.length - 1];
        if (ff != undefined && ff != null) {
            //this.ChangingPaper(ff);
            return ff;
        }
        else {
            //this.ChangingPaper(new MagPaper());
            return new MagPaper();
        }
    }

    
    public FetchNextPage() {
        
        if (this.MAGList.pageindex < this.MAGList.pagecount-1) {
            this.MAGList.pageindex += 1;
        } else {
        }
        console.log('inside fetchnextPage: ', this.MAGList);
        this._Criteria.pageNumber = this.MAGList.pageindex;
        //this.FetchMAGRelatedPaperRunsListId(this._Criteria.magRelatedRunId);
        this.FetchWithCrit(this._Criteria, this.ListDescription)
    }
    public FetchPrevPage() {
        if (this.MAGList.pageindex == 0 ) {
            return this.FetchWithCrit(this._Criteria, this.ListDescription);
        } else {
            this._Criteria.pageNumber -= 1;
            return this.FetchWithCrit(this._Criteria, this.ListDescription);
        }
    }
    public FetchLastPage() {
        this._Criteria.pageNumber = this.MAGList.pagecount - 1;
        return this.FetchWithCrit(this._Criteria, this.ListDescription);
    }
    public FetchFirstPage() {
        this._Criteria.pageNumber = 0;
        return this.FetchWithCrit(this._Criteria, this.ListDescription);
    }
    public FetchParticularPage(pageNum: number) {
        this._Criteria.pageNumber = pageNum;
        return this.FetchWithCrit(this._Criteria, this.ListDescription);
    }

    public sort: SortDescriptor[] = [{
        field: 'paperId',
        dir: 'asc'
    }];
    public sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        console.log('sorting papers by ' + this.sort[0].field + " ");
        this._MAGList.papers = orderBy(this._MAGList.papers, this.sort);
    }
    public get HasSelectedPapers(): boolean {
        //return true;
        //console.log("HasSelectedPapers?", this._MAGList.papers[0].isSelected, this._MAGList.papers[1].isSelected);
        if (this._MAGList.papers.findIndex(found => found.isSelected == true) > -1) return true;
        else return false;
    }
    public get SelectedPapers(): MagPaper[] {
        return this._MAGList.papers.filter(found => found.isSelected == true);
    }

}


export class KeyValue {//used in more than one place...
    constructor(k: string, v: string) {
        this.key = k;
        this.value = v;
    }
    key: string;
    value: string;
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

    pageIndex: number = 0;
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

    fieldOfStudyId: number = 0;
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



