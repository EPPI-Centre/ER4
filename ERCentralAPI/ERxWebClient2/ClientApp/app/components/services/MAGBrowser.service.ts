import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient  } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import {
    MagList, MagPaper, MVCMagFieldOfStudyListSelectionCriteria,
    MVCMagPaperListSelectionCriteria, MagFieldOfStudy, MvcMagFieldOfStudyListSelectionCriteria,
    TopicLink, MagItemPaperInsertCommand, MVCMagOrigPaperListSelectionCriteria, MagRelatedPapersRun,
    MagSearch, MagBrowseHistoryItem
} from '../services/MAGClasses.service';
import { DatePipe } from '@angular/common';
import { EventEmitterService } from './EventEmitter.service';
import { MAGBrowserHistoryService } from './MAGBrowserHistory.service';
import { MAGTopicsService } from './MAGTopics.service';
import { MAGAdvancedService } from './magAdvanced.service';


@Injectable({
    providedIn: 'root',
    }
)

export class MAGBrowserService extends BusyAwareService {


    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private _eventEmitterService: EventEmitterService,
        private _magTopicsService: MAGTopicsService,
        private modalService: ModalService,
        private datePipe: DatePipe,
        private _mAGBrowserHistoryService: MAGBrowserHistoryService
    ) {
		super();
    }
    public currentMagRelatedRun: MagRelatedPapersRun = new MagRelatedPapersRun();
    public currentMagSearch: MagSearch = new MagSearch();
    public currentTopicSearch: MagFieldOfStudy = new MagFieldOfStudy();
    public currentMagPaper: MagPaper = new MagPaper();
    public currentRefreshListType: string = '';
    public currentListType: string = '';
    public firstVisitToMAGBrowser: boolean = true;
    public MagCitationsByPaperList: MagList = new MagList();
    public MagPaperFieldsList: MagFieldOfStudy[] = [];
    private _MAGList: MagList = new MagList();
    private _MAGOriginalList: MagList = new MagList();
    private _Criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    private _OrigCriteria: MVCMagOrigPaperListSelectionCriteria = new MVCMagOrigPaperListSelectionCriteria();
    private _currentPaper: MagPaper = new MagPaper();
    public ParentTopic: string = '';
    public WPParentTopics: TopicLink[] = [];
    public WPChildTopics: TopicLink[] = [];
    public ListDescription: string = '';
    public OrigListDescription: string = '';
    public selectedPapers: MagPaper[] = [];
    public SelectedPaperIds: number[] = [];
    public ShowingParentAndChildTopics: boolean = false;
    public ShowingChildTopicsOnly: boolean = false;
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
    public get MAGOriginalList(): MagList {
        return this._MAGOriginalList;
    }
    public set MAGOriginalList(value: MagList) {
        this._MAGOriginalList = value;
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
    public get OrigListCriteria(): MVCMagOrigPaperListSelectionCriteria {
        return this._OrigCriteria;
    }
    public set OrigListCriteria(value: MVCMagOrigPaperListSelectionCriteria) {
        this._OrigCriteria = value;
    }
    public get currentPaper(): MagPaper {
        return this._currentPaper;
    }
    public GetPaperListForTopic(FieldOfStudyId: number): Promise<boolean>  {

            let id = this.ListCriteria.magRelatedRunId;
            this.ListCriteria = new MVCMagPaperListSelectionCriteria();
            this.ListCriteria.magRelatedRunId = id;
            this.ListCriteria.fieldOfStudyId = FieldOfStudyId;
            this.ListCriteria.listType = "PaperFieldsOfStudyList";
            this.ListCriteria.pageNumber = 0;
            this.ListCriteria.pageSize = this.pageSize;
            return this.FetchWithCrit(this.ListCriteria, "PaperFieldsOfStudyList").then(

                        (res1: boolean) => {
                        
                                if (this._eventEmitterService.firstVisitMAGBrowserPage) {
                                    this.FetchOrigWithCrit(this.ListCriteria, "PaperFieldsOfStudyList").then(
                                        (res2: boolean) => {
                                            this.firstVisitToMAGBrowser = false;
                                            this._eventEmitterService.firstVisitMAGBrowserPage = false;
                                            return res2;
                                        })
                            };
                            return res1;
                        }
                );
    }

    public GetTopicsAndRelatedPapers(item: MagFieldOfStudy) {

        this.currentTopicSearch = item;
        this.currentRefreshListType = 'PaperFieldsOfStudyList';
        this._eventEmitterService.firstVisitMAGBrowserPage = false;
        this.OrigListCriteria.listType = "PaperFieldsOfStudyList";
        this.ShowingParentAndChildTopics = true;
        this.ShowingChildTopicsOnly = false;
        this.currentFieldOfStudy = item;
        let fieldOfStudyId: number = item.fieldOfStudyId;

        this.GetParentAndChildRelatedPapers(item.displayName, fieldOfStudyId);
    }

    public async GetParentAndChildRelatedPapers(displayName: string, fieldOfStudyId: number): Promise<boolean>  {

        this._eventEmitterService.firstVisitMAGBrowserPage = false;
        this.currentRefreshListType = 'PaperFieldsOfStudyList';
        //this.currentListType = "PaperFieldsOfStudyList";
        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem(displayName, "BrowseTopic", 0,
            "", "", 0, "", "", fieldOfStudyId, displayName, "", 0));

        this.currentMagPaper = new MagPaper();
        this._magTopicsService.WPChildTopics = [];
        this._magTopicsService.WPParentTopics = [];
        this.ParentTopic = '';

        this.ParentTopic = displayName;

        await this.GetParentAndChildFieldsOfStudy("FieldOfStudyParentsList", fieldOfStudyId)

        await this.GetParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", fieldOfStudyId);
        
        return await this.GetPaperListForTopic(fieldOfStudyId);

    }

    public GetPaperListForTopicsAfterRefresh(fieldOfStudy: MagFieldOfStudy, dateFrom: Date, dateTo: Date): boolean | undefined {

        let dateFormattedFrom: string | null = this.datePipe.transform(dateFrom, 'yyyy-MM-dd'); 
        let dateFormattedTo: string | null = this.datePipe.transform(dateTo, 'yyyy-MM-dd'); 

        if (fieldOfStudy.fieldOfStudyId != null) {

            let id = this.ListCriteria.magRelatedRunId;
            this.ListCriteria = new MVCMagPaperListSelectionCriteria();
            this.ListCriteria.magRelatedRunId = id;
            this.ListCriteria.fieldOfStudyId = fieldOfStudy.fieldOfStudyId;
            this.ListCriteria.listType = "PaperFieldsOfStudyList";
            this.ListCriteria.pageNumber = 0;
            this.ListCriteria.pageSize = this.pageSize;
            if (dateFormattedFrom != null) {
                this.ListCriteria.dateFrom = dateFormattedFrom;
            }
            if (dateFormattedTo != null) {
                this.ListCriteria.dateTo = dateFormattedTo;
            }
            this.FetchWithCrit(this.ListCriteria, "PaperFieldsOfStudyList").then(

                (res: boolean) => { return res; }
            );

        } else {
            return false;
        }
    }
    public ImportMagRelatedSelectedPapers(selectedPapers: number[]): Promise<MagItemPaperInsertCommand | void> {

        let selectedPapersStr: string = selectedPapers.join(',');
        this._BusyMethods.push("ImportMagRelatedSelectedPapers");
        let body = JSON.stringify({ Value: selectedPapersStr });
        return this._httpC.post<MagItemPaperInsertCommand>(this._baseUrl + 'api/MagRelatedPapersRunList/ImportMagRelatedSelectedPapers',
            body)
            .toPromise().then(
                (result: MagItemPaperInsertCommand) => {
                    this.RemoveBusy("ImportMagRelatedSelectedPapers");
                    return result;
                },
                error => {
                    this.RemoveBusy("ImportMagRelatedSelectedPapers");
                    this.modalService.GenericErrorMessage('an api call error with ImportMagRelatedSelectedPapers: ' + error);
                }
            );
    }

    public GetParentAndChildFieldsOfStudy(FieldOfStudy: string, FieldOfStudyId: number): Promise<boolean> {
        this.ShowingParentAndChildTopics = true;
        this.ShowingChildTopicsOnly = false;
        let selectionCriteria: MvcMagFieldOfStudyListSelectionCriteria = new MvcMagFieldOfStudyListSelectionCriteria();
        selectionCriteria.listType = FieldOfStudy;
        selectionCriteria.fieldOfStudyId = FieldOfStudyId;
        selectionCriteria.SearchTextTopics = '';

        return this._magTopicsService.FetchMagFieldOfStudyList(selectionCriteria, 'CitationsList').then(

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


    public FetchMAGRelatedPaperRunsListId(Id: number) {
        
        this._BusyMethods.push("FetchMAGRelatedPaperRunsListId");
        let body = JSON.stringify({ Value: Id });
        return this._httpC.post<MagList>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRunsId',
            body)
            .subscribe(
                (result) => {
                    this.firstVisitToMAGBrowser = false;
                    this._eventEmitterService.firstVisitMAGBrowserPage = false;
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                    this.MAGList = result;
                    this.MAGOriginalList = result;
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

    public GetMagRelatedRunsListById(item: MagRelatedPapersRun) : Promise<boolean> {

        this.currentMagRelatedRun = item;
        this.currentRefreshListType = 'MagRelatedPapersRunList';
        this.currentMagPaper = new MagPaper();
        this.MagCitationsByPaperList.papers = [];
        this.MAGOriginalList.papers = [];
        this.currentListType = "MagRelatedPapersRunList";
        this.currentMagPaper = new MagPaper();
        this.ShowingParentAndChildTopics = false;
        this.ShowingChildTopicsOnly = true;
        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Papers identified from auto-identification run", "MagRelatedPapersRunList", 0,
            "", "", 0, "", "", 0, "", "", item.magRelatedRunId));

        return this.FetchMAGRelatedPaperRunsListById(item.magRelatedRunId);
 
    }


    public FetchMAGRelatedPaperRunsListById(Id: number): Promise<boolean> {

        var goBackListType: string = 'MagRelatedPapersRunList';
        this._BusyMethods.push("FetchMAGRelatedPaperRunsListById");
        this.ListCriteria.listType = "MagRelatedPapersRunList";
        this.ListCriteria.pageSize = this.pageSize;
        this.ListCriteria.magRelatedRunId = Id;
        this.ListCriteria.pageNumber = 0;

        this.OrigListCriteria.listType = "MagRelatedPapersRunList";
        this.OrigListCriteria.pageSize = this.pageSize;
        this.OrigListCriteria.magRelatedRunId = Id;
        this.OrigListCriteria.pageNumber = 0;


        return this._httpC.post<MagList>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRunsId',
            this.ListCriteria)
            .toPromise().then(
                (result) => {

                    this.firstVisitToMAGBrowser = false;
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListById");
                    this.MAGList = result;
                    this.MAGOriginalList = new MagList();
                    this.MAGOriginalList.pagecount = result.pagecount;
                    this.MAGOriginalList.pageindex = result.pageindex;
                    this.MAGOriginalList.pagesize = result.pagesize;
                    this.MAGOriginalList.totalItemCount = result.totalItemCount;
                    this.currentListType = "MagRelatedPapersRunList";

                    if (this.MAGList.papers != null && this.MAGList.papers.length > 0) {

                        this.MAGList.papers.forEach((item: any) => {
                            this.MAGOriginalList.papers.push(item);
                        });
                    }
                    this.ListCriteria.paperIds = '';
                    for (var i = 0; i < result.papers.length; i++) {
                       
                        this.ListCriteria.paperIds += result.papers[i].paperId.toString() + ',';
                    }
                    this.ListCriteria.paperIds = this.ListCriteria.paperIds.substr(0, this.ListCriteria.paperIds.length - 1);
                    this.ListCriteria.pageNumber += 1;
                   
                    let FieldsListcriteria: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
                    FieldsListcriteria.fieldOfStudyId = 0;
                    FieldsListcriteria.listType = "PaperFieldOfStudyList";
                    FieldsListcriteria.paperIdList = this.ListCriteria.paperIds;
                    this.SavePapers(result, this.ListCriteria, "NormalList");
                    this.SaveOrigPapers(result, this.OrigListCriteria, "PaperFieldOfStudyList");
                    FieldsListcriteria.SearchTextTopics = ''; 
                    return this._magTopicsService.FetchMagFieldOfStudyList(FieldsListcriteria, goBackListType).then(

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
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListById");
                    this.modalService.GenericError(error);
                    return false;
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchMAGRelatedPaperRunsListById");
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListById");
                    return false;
                });
    }

    public async GetMagItemsForSearch(item: MagSearch): Promise<boolean> {

        this.currentMagSearch = item;
        this.currentMagPaper = new MagPaper();
        this.MagCitationsByPaperList.papers = [];
        this.MAGOriginalList.papers = [];
        this.currentRefreshListType = 'MagSearchResultsList';
        this.currentListType = "MagSearchResultsList";
        this.ShowingParentAndChildTopics = false;
        this.ShowingChildTopicsOnly = true;
        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Papers identified from Mag Search run", "MagSearchPapersList", 0,
            "", "", 0, "", "", 0, "", "", item.magSearchId));
        let selectionCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        selectionCriteria.pageSize = 20;
        selectionCriteria.pageNumber = 0;
        selectionCriteria.listType = "MagSearchResultsList";
        selectionCriteria.magSearchText = item.magSearchText;
        return await this.FetchMagPapersFromSearch(selectionCriteria, "MagSearchResultsList");
    }


    public async FetchMagPapersFromSearch(criteria: MVCMagPaperListSelectionCriteria, goBackListType: string) : Promise<any> {

        await this.FetchWithCrit(criteria, "MagSearchResultsList");
        let FieldsListcriteria: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
        FieldsListcriteria.fieldOfStudyId = 0;
        FieldsListcriteria.listType = "PaperFieldOfStudyList";
        FieldsListcriteria.paperIdList = this.ListCriteria.paperIds;
        await this._magTopicsService.FetchMagFieldOfStudyList(FieldsListcriteria, 'MagSearchResultsList')
        return await this.FetchOrigWithCrit(criteria, "OrigList");

    }
    public FetchWithCrit(crit: MVCMagPaperListSelectionCriteria, listDescription: string): Promise<boolean> {

        this._BusyMethods.push("FetchWithCrit");
        this.ListCriteria = crit;
        if (this.MAGList && this._MAGList.pagesize > 0
            && this.MAGList.pagesize <= 4000
            && this.MAGList.pagesize != crit.pageSize
        ) {
            crit.pageSize = this.MAGList.pagesize;
        }
        this.ListCriteria.paperIds = crit.paperIds;
        this.ListDescription = listDescription;

        return this._httpC.post<MagList>(this._baseUrl + 'api/MagPaperList/GetMagPaperList', crit)
            .toPromise().then(

            (list: MagList) => {

                    this.RemoveBusy("FetchWithCrit");
                    this.SavePapers(list, this.ListCriteria, "NormalList");
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

    public FetchOrigWithCrit(crit: MVCMagOrigPaperListSelectionCriteria, listDescription: string): Promise<boolean> {

        this._BusyMethods.push("FetchOrigWithCrit");
        this.OrigListCriteria = crit;
        if (this.MAGOriginalList && this._MAGOriginalList.pagesize > 0
            && this.MAGOriginalList.pagesize <= 4000
            && this.MAGOriginalList.pagesize != crit.pageSize
        ) {
            crit.pageSize = this._MAGOriginalList.pagesize;
        }

        this.OrigListCriteria.paperIds = crit.paperIds;
        return this._httpC.post<MagList>(this._baseUrl + 'api/MagPaperList/GetMagPaperList', crit)
            .toPromise().then(

                (list: MagList) => {

                    this.RemoveBusy("FetchOrigWithCrit");

                    this.SaveOrigPapers(list, this.OrigListCriteria, 'OrigList');
                    return true;

                }, error => {
                    this.modalService.GenericError(error);
                    this.RemoveBusy("FetchOrigWithCrit");
                    return false;
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchOrigWithCrit: " + error);
                    this.RemoveBusy("FetchOrigWithCrit");
                    return false;
                });
    }
    public SaveOrigPapers(list: MagList, crit: MVCMagOrigPaperListSelectionCriteria, referenceList: string) {


        if (crit.listType == 'CitationsList' || crit.listType == 'ReviewMatchedPapers' || crit.listType == 'MagSearchResultsList'
            || crit.listType == '"MagRelatedPapersRunList"' || crit.listType == '"PaperFieldsOfStudyList"'
            || crit.listType == 'MagSearchResultsList') {

            if (referenceList == 'OrigList') {

                this._OrigCriteria.paperIds = '';
                for (var i = 0; i < list.papers.length; i++) {
                    this._OrigCriteria.paperIds += list.papers[i].paperId + ',';
                }
                this._OrigCriteria.paperIds = this._OrigCriteria.paperIds.substr(0, this._OrigCriteria.paperIds.length - 2);
            }

        } else if (crit.listType == 'CitedByList') {

            this._Criteria.paperIds = '';
            for (var i = 0; i < list.papers.length; i++) {
                this._Criteria.paperIds += list.papers[i].paperId + ',';
            }
            this._Criteria.paperIds = this._Criteria.paperIds.substr(0, this._Criteria.paperIds.length - 2);
            this.MagCitationsByPaperList = list;
            this._Criteria = crit;
            return;
        }

        if (referenceList == 'OrigList') {
            this._MAGOriginalList = list;
            this._OrigCriteria = crit;
            this._MAGOriginalList.totalItemCount = list.totalItemCount;
            this._MAGOriginalList.pagecount = list.pagecount;
        } 

    }
    public SavePapers(list: MagList, crit: MVCMagPaperListSelectionCriteria, referenceList: string ) {

        if (crit.listType == 'CitationsList' || crit.listType == 'ReviewMatchedPapers' || crit.listType == 'MagSearchResultsList'
            || crit.listType == '"MagRelatedPapersRunList"' || crit.listType == '"PaperFieldsOfStudyList"'
        || crit.listType =='MagSearchResultsList') {
                       
                this._Criteria.paperIds = '';
                for (var i = 0; i < list.papers.length; i++) {
                    this._Criteria.paperIds += list.papers[i].paperId + ',';
                }
                    this._Criteria.paperIds = this._Criteria.paperIds.substr(0, this._Criteria.paperIds.length - 2);
                       
        } else if (crit.listType == 'CitedByList') {

            this._Criteria.paperIds = '';
            for (var i = 0; i < list.papers.length; i++) {
                this._Criteria.paperIds += list.papers[i].paperId + ',';
            }
            this._Criteria.paperIds = this._Criteria.paperIds.substr(0, this._Criteria.paperIds.length - 2);
            this.MagCitationsByPaperList = list;
            this._Criteria = crit;
            return;
        } 
        this._MAGList = list;
        this._Criteria = crit;

    }

    public FetchNextPage() {
        if (this.MAGList.pageindex < this.MAGList.pagecount - 1) {
            this.MAGList.pageindex += 1;
        } 
        this.ListCriteria.pageNumber = this.MAGList.pageindex;
        this.ListCriteria.pageSize = this.pageSize;
        this.FetchWithCrit(this.ListCriteria, this.ListCriteria.listType);
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

    public FetchOrigNextPage() {
        this.OrigListCriteria.pageNumber = this.MAGOriginalList.pageindex;
        this.OrigListCriteria.listType = this.currentListType;
        if (this.MAGOriginalList.pageindex < this.MAGOriginalList.pagecount - 1) {
            this.MAGOriginalList.pageindex += 1;
        }
        this.OrigListCriteria.pageNumber = this.MAGOriginalList.pageindex;
        this.OrigListCriteria.pageSize = this.pageSize;
        this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType)
    }
    public FetchOrigPrevPage() {
        console.log('curentList: ', this.currentListType);
        this.OrigListCriteria.listType = this.currentListType;
        this.OrigListCriteria.pageNumber = this.MAGOriginalList.pageindex;
        if (this.MAGOriginalList.pageindex == 0) {
            return this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType);
        } else {
            this.OrigListCriteria.pageNumber -= 1;
            return this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType);
        }
    }
    public FetchOrigLastPage() {
        this._OrigCriteria.pageNumber = this.MAGOriginalList.pagecount - 1;
        return this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType);
    }
    public FetchOrigFirstPage() {
        this.OrigListCriteria.pageNumber = 0;
        return this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType);
    }
    public FetchOrigParticularPage(pageNum: number) {
        this._OrigCriteria.pageNumber = pageNum;
        return this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType);
    }
    public get SelectedPapers(): MagPaper[] {
        return this._MAGList.papers.filter(found => found.isSelected == true);
    }
    public Clear() {

        this._currentPaper = new MagPaper();
        this.MAGList = new MagList();
        this.MAGOriginalList = new MagList();
        this.MagCitationsByPaperList = new MagList();
        this.selectedPapers = [];
        this.ClearTopics();
    }
    public ClearTopics() {
        this.ParentTopic = '';
        this.WPChildTopics = [];
        this.WPParentTopics = [];
    }
}


