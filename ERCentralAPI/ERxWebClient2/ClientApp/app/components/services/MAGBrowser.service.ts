import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient  } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import {
    MagList, MagPaper, MVCMagFieldOfStudyListSelectionCriteria,
    MVCMagPaperListSelectionCriteria, MagFieldOfStudy, MvcMagFieldOfStudyListSelectionCriteria, TopicLink
} from '../services/MAGClasses.service';

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

    public MagCitationsByPaperList: MagList = new MagList();
    public MagPaperFieldsList: MagFieldOfStudy[] = [];
    private _MAGList: MagList = new MagList();
    private _Criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    private _currentPaper: MagPaper = new MagPaper();
    public ParentTopic: string = '';
    public WPParentTopics: TopicLink[] = [];
    public WPChildTopics: TopicLink[] = [];
    public ListDescription: string = '';
    public selectedPapers: MagPaper[] = [];
    public SelectedPaperIds: number[] = [];
    public pageSize: number = 20;
    @Output() PaperChanged = new EventEmitter();
    public currentFieldOfStudy: MagFieldOfStudy = new MagFieldOfStudy();
    public ClearSelected() {
        for (var i = 0; i < this.MAGList.papers.length; i++) {
            this.MAGList.papers[i].isSelected = false;
        }
        this.SelectedPaperIds = [];
        this.selectedPapers = [];
    }
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
    public GetPaperListForTopic(FieldOfStudyId: number): boolean | undefined {

        if (FieldOfStudyId != null) {

            let id = this.ListCriteria.magRelatedRunId;
            this.ListCriteria = new MVCMagPaperListSelectionCriteria();
            this.ListCriteria.magRelatedRunId = id;
            this.ListCriteria.fieldOfStudyId = FieldOfStudyId;
            this.ListCriteria.listType = "PaperFieldsOfStudyList";
            this.ListCriteria.pageNumber = 0;
            this.ListCriteria.pageSize = this.pageSize;
            this.FetchWithCrit(this.ListCriteria, "PaperFieldsOfStudyList").then(

                    (res: boolean) => { return res; }
                );

        } else {
            return false;
        }
    }
    public GetParentAndChildFieldsOfStudy(FieldOfStudy: string, FieldOfStudyId: number): Promise<boolean> {

        let selectionCriteria: MvcMagFieldOfStudyListSelectionCriteria = new MvcMagFieldOfStudyListSelectionCriteria();
        selectionCriteria.listType = FieldOfStudy;
        selectionCriteria.fieldOfStudyId = FieldOfStudyId;
        selectionCriteria.SearchTextTopics = '';
        return this.FetchMagFieldOfStudyList(selectionCriteria, 'CitationsList').then(

            (result: MagFieldOfStudy[]) => {
                if (result != null && result.length > 0) {
                    return true;
                } else {
                    return false;
                }
            }, ((error) => {
 
                this.modalService.GenericError(error);
                return false;})
        );
    }
    FetchMAGRelatedPaperRunsListId(Id: number) {

        this._BusyMethods.push("FetchMAGRelatedPaperRunsListId");
        let body = JSON.stringify({ Value: Id });
        return this._httpC.post<MagList>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRunsId',
            body)
            .subscribe(
                (result) => {

                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                    this.MAGList = result;
                    this.ListCriteria.listType = "MagRelatedPapersRunList";
                    this.ListCriteria.pageSize = 20;
                    this.ListCriteria.magRelatedRunId = Id;
                },
                error => {
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                    this.modalService.GenericErrorMessage('an api call error with FetchMAGRelatedPaperRunsListId: ' + error);
                }
            );
    }
    public FetchMAGRelatedPaperRunsListById(Id: number): Promise<boolean> {
        var goBackListType: string = 'MagRelatedPapersRunList';
        this._BusyMethods.push("FetchMAGRelatedPaperRunsListId");
        this.ListCriteria.listType = "MagRelatedPapersRunList";
        this.ListCriteria.pageSize = this.pageSize;
        this.ListCriteria.magRelatedRunId = Id;

        return this._httpC.post<MagList>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRunsId',
            this.ListCriteria)
            .toPromise().then(
                (result) => {

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
                    //TODO THIS SEARCH TEXT NEEDS TO COME IN FROM THE FRONT
                    FieldsListcriteria.SearchTextTopics = ''; //searchText;
                    return this.FetchMagFieldOfStudyList(FieldsListcriteria, goBackListType).then(

                        (res: MagFieldOfStudy[]) => {
                            if (res != null) {
                                return true;
                            } else {
                                return false;
                            }
                        }
                    );
                },
                error => {
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                    this.modalService.GenericError(error);
                    return false;
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchMAGRelatedPaperRunsListId");
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                    return false;
                });
    }
    public FetchMagFieldOfStudyList(criteria: MVCMagFieldOfStudyListSelectionCriteria, goBackListType: string): Promise<MagFieldOfStudy[]> {
        this._BusyMethods.push("FetchMagFieldOfStudyList");
        return this._httpC.post<MagFieldOfStudy[]>(this._baseUrl + 'api/MagFieldOfStudyList/GetMagFieldOfStudyList', criteria)
            .toPromise().then(
            (result: MagFieldOfStudy[]) => {

                    this.RemoveBusy("FetchMagFieldOfStudyList");
                        if (result != null) {

                            let FosList: MagFieldOfStudy[] = result;
                            let i: number = 2;
                            let j: number = 2;
                            for (var fos of FosList) {

                                let item: TopicLink = new TopicLink();
                                item.displayName = fos.displayName;
                                item.fieldOfStudyId = fos.fieldOfStudyId;

                                if (criteria.listType == 'FieldOfStudyParentsList') {
                                    if (i > 0.1) {
                                        i -= 0.008;
                                    }
                                    item.fontSize = i;
                                    this.WPParentTopics.push(item);
                                    

                                } else {
                                    if (j > 0.1) {
                                        j -= 0.008;
                                    }
                                    item.fontSize = j;
                                    this.WPChildTopics.push(item);
                                   
                                }
                            }
                        }
                    this.ListCriteria.listType = goBackListType;
                    return result;
                },
                error => {
                    this.RemoveBusy("FetchMagFieldOfStudyList");
                    this.modalService.GenericError(error);
                    return error;
                }
            ).catch(
            (error) => {

                    this.modalService.GenericErrorMessage("error with FetchMagFieldOfStudyList: " + error);
                    this.RemoveBusy("FetchMagFieldOfStudyList");
                return error;
            });
    }
    public FetchWithCrit(crit: MVCMagPaperListSelectionCriteria, listDescription: string): Promise<boolean> {

        this._BusyMethods.push("FetchWithCrit");
        this._Criteria = crit;

        if (this._MAGList && this._MAGList.pagesize > 0
            && this._MAGList.pagesize <= 4000
            && this._MAGList.pagesize != crit.pageSize
        ) {
            crit.pageSize = this._MAGList.pagesize;
        }

        this.ListCriteria.paperIds = crit.paperIds;
        this.ListDescription = listDescription;

        return this._httpC.post<MagList>(this._baseUrl + 'api/MagPaperList/GetMagPaperList', crit)
            .toPromise().then(

            (list: MagList) => {

                    this.RemoveBusy("FetchWithCrit");
                    this.SavePapers(list, this._Criteria);
                    return true;
                                    
                }, error => {
                    this.modalService.GenericError(error);
                    this.RemoveBusy("FetchWithCrit");
                    return false;
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchWithCrit: " + error);
                    this.RemoveBusy("FetchWithCrit");
                    return false;
            });
    }
    public SavePapers(list: MagList, crit: MVCMagPaperListSelectionCriteria) {

        if (crit.listType == 'CitationsList' || crit.listType == 'ReviewMatchedPapers') {

            this._Criteria.paperIds = '';
            for (var i = 0; i < list.papers.length; i++) {
                this._Criteria.paperIds += list.papers[i].paperId + ',';
            }
            this._Criteria.paperIds = this._Criteria.paperIds.substr(0, this._Criteria.paperIds.length - 2);
            this._MAGList = list;
            this._Criteria = crit;
           
        } else if (crit.listType == 'CitedByList') {

            this._Criteria.paperIds = '';
            for (var i = 0; i < list.papers.length; i++) {
                this._Criteria.paperIds += list.papers[i].paperId + ',';
            }
            this._Criteria.paperIds = this._Criteria.paperIds.substr(0, this._Criteria.paperIds.length - 2);
            this.MagCitationsByPaperList = list;
            this._Criteria = crit;
           
        } else if (crit.listType == 'MagRelatedPapersRunList') {

            this._MAGList = list;
            this._Criteria = crit;
        } else if (crit.listType == 'PaperFieldsOfStudyList') {

            this._MAGList = list;
            this._Criteria = crit;
        }     
       
    }
    private ChangingPaper(newPaper: MagPaper) {

        this._currentPaper = newPaper;
		this.PaperChanged.emit(newPaper);
    }
	public getPaper(paperId: number): MagPaper {

        let ff = this.MAGList.papers.find(found => found.paperId == paperId);
        if (ff != undefined && ff != null) {
            this.ChangingPaper(ff);
            return ff;
        }
        else {
            this.ChangingPaper(new MagPaper());
            return new MagPaper();
        }
    }
    //Paging methods
    public FetchNextPage() {
        if (this.MAGList.pageindex < this.MAGList.pagecount-1) {
            this.MAGList.pageindex += 1;
        } 
        this._Criteria.pageNumber = this.MAGList.pageindex;
        this._Criteria.pageSize = this.pageSize;
        this.FetchWithCrit(this._Criteria, this._Criteria.listType)
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
    public get SelectedPapers(): MagPaper[] {
        return this._MAGList.papers.filter(found => found.isSelected == true);
    }
    public Clear() {

        this._currentPaper = new MagPaper();
        this.MAGList = new MagList();
        this.MagCitationsByPaperList = new MagList();
        this.ClearTopics();
    }
    public ClearTopics() {
        this.ParentTopic = '';
        this.WPChildTopics = [];
        this.WPParentTopics = [];
    }
}


