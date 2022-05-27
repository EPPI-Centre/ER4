import { Inject, Injectable, EventEmitter, Output, OnDestroy } from '@angular/core';
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
import { MAGTopicsService } from './MAGTopics.service';
import { SetAttribute } from './ReviewSets.service';
import { Subscription } from 'rxjs';


@Injectable({
    providedIn: 'root',
}
)

export class MAGBrowserService extends BusyAwareService implements OnDestroy {


    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private EventEmitterService: EventEmitterService,
        private _magTopicsService: MAGTopicsService,
        private modalService: ModalService,
        private datePipe: DatePipe
    ) {
        super();
        //console.log("On create MAGBrowserService");
        this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
        this.clearSub2 = this.EventEmitterService.OpeningNewReview.subscribe(() => { this.Clear(); });
    }
    ngOnDestroy() {
        console.log("Destroy MAGRelatedRunsService");
        if (this.clearSub != null) this.clearSub.unsubscribe();
        if (this.clearSub2 != null) this.clearSub2.unsubscribe();
    }
    private clearSub: Subscription | null = null;
    private clearSub2: Subscription | null = null;

    public currentMagPaper: MagPaper = new MagPaper();
    private _MAGList: MagList = new MagList();//the "references" list
    private _Criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();//criteria used for _MAGList
    private _MAGOriginalList: MagList = new MagList();//any list generated when reaching the browser
    private _OrigCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();//criteria used for _MAGOriginalList
    private _CitedCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();//criteria used for MagCitationsByPaperList 
    public MagCitationsByPaperList: MagList = new MagList();
    public selectedPapers: MagPaper[] = [];
    public ParentTopic: string = '';
    //public WPParentTopics: TopicLink[] = [];
    //public WPChildTopics: TopicLink[] = [];
    private pageSize: number = 20;

    public get MAGOriginalList(): MagList {
        return this._MAGOriginalList;
    }
    public set MAGOriginalList(value: MagList) {
        this._MAGOriginalList = value;
    }
    public get MAGList(): MagList {//the "references" list, when present. Otherwise the OrigList
        if (this.currentMagPaper.paperId > 0) return this._MAGList;
        else return this._MAGOriginalList;
    }
    public set MAGList(value: MagList) {
        this._MAGList = value;
    }

    public get SelectedPaperIds(): number[] {
        return this.selectedPapers.map(f => f.paperId);
    }

    public get OrigCriteria(): MVCMagPaperListSelectionCriteria {
        return this._OrigCriteria;
    }

    private ClearTopics() {
        this.ParentTopic = '';
        //this.WPChildTopics = [];
        //this.WPParentTopics = [];
    }

    private FetchWithCrit(crit: MVCMagPaperListSelectionCriteria): Promise<MagList | boolean> {
        this._BusyMethods.push("FetchWithCrit");

        return this._httpC.post<MagList>(this._baseUrl + 'api/MagPaperList/GetMagPaperList', crit)
            .toPromise().then(
                (list: MagList) => {
                    this.RemoveBusy("FetchWithCrit");
                    return list;

                }, error => {
                    this.modalService.GenericError(error);
                    this.RemoveBusy("FetchWithCrit");
                    return false;
                }
            ).catch(
                (error) => {
                    this.modalService.GenericErrorMessage("Sorry, could not fetch the requested list of papers: " + error);
                    this.RemoveBusy("FetchWithCrit");
                    return false;
                });
    }
    private GetTopicsForCurrentOrigList(): Promise<boolean> {
        let FieldsListcriteria: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
        FieldsListcriteria.fieldOfStudyId = 0;
        FieldsListcriteria.listType = "PaperFieldOfStudyList";
        FieldsListcriteria.paperIdList = this._MAGOriginalList.papers.map(f => f.paperId).join(',');
        this._magTopicsService.ShowingParentAndChildTopics = false;
        this._magTopicsService.ShowingChildTopicsOnly = true;
        FieldsListcriteria.SearchTextTopics = '';
        return this._magTopicsService.FetchMagFieldOfStudyList(FieldsListcriteria, this._OrigCriteria.listType).then(

            (res: MagFieldOfStudy[] | boolean) => {

                //this._eventEmitterService.firstVisitMAGBrowserPage = false;

                if (typeof res != "boolean") {
                    return true;
                } else {
                    return false;
                }
            }
        );
    }
    private GetTopicsForCurrentPaper(): Promise<boolean> {
        let FieldsListcriteria: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
        FieldsListcriteria.fieldOfStudyId = 0;
        FieldsListcriteria.listType = "PaperFieldOfStudyList";
        FieldsListcriteria.paperIdList = this.currentMagPaper.paperId.toString();

        FieldsListcriteria.SearchTextTopics = '';
        return this._magTopicsService.FetchMagFieldOfStudyList(FieldsListcriteria, this._OrigCriteria.listType).then(

            (res: MagFieldOfStudy[] | boolean) => {

                //this._eventEmitterService.firstVisitMAGBrowserPage = false;

                if (typeof res != "boolean") {
                    return true;
                } else {
                    return false;
                }
            }
        );
    }

    public async GetMagRelatedRunsListById(magRelatedRunId: number): Promise<boolean> {
        let crit = new MVCMagPaperListSelectionCriteria();
        crit.listType = "MagRelatedPapersRunList";
        crit.pageSize = this.pageSize;
        crit.magRelatedRunId = magRelatedRunId;
        crit.pageNumber = 0;
        let res = await this.FetchWithCrit(crit);
        if (typeof res != "boolean") {
            //it worked, so let's replace things accordingly...
            this.currentMagPaper = new MagPaper();
            this._MAGList = new MagList();//
            this._Criteria = new MVCMagPaperListSelectionCriteria();
            this._OrigCriteria = crit;
            this._magTopicsService.WPParentTopics = [];
            this._magTopicsService.WPChildTopics = [];
            this._MAGOriginalList = res;
            //all is well, now get the topics for these papers... We don't wait for it, fire and forget!
            this.GetTopicsForCurrentOrigList();
            return true;
        }
        else {
            //didn't work change nothing and signal the fact
            return false;
        }
    }
    public async GetParentAndChildRelatedPapers(displayName: string, fieldOfStudyId: number): Promise<boolean> {

        //this._eventEmitterService.firstVisitMAGBrowserPage = false;
        //this.currentRefreshListType = 'PaperFieldsOfStudyList';
        //this.currentListType = "PaperFieldsOfStudyList";
        //this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem(displayName, "BrowseTopic", 0,
        //    "", "", 0, "", "", fieldOfStudyId, displayName, "", 0));

        this.currentMagPaper = new MagPaper();
        this._magTopicsService.WPChildTopics = [];
        this._magTopicsService.WPParentTopics = [];
        this.ParentTopic = '';

        this.ParentTopic = displayName;
        this.GetParentAndChildFieldsOfStudy("FieldOfStudyParentsList", fieldOfStudyId).then(
            () => { this.GetParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", fieldOfStudyId); }
        );
        return await this.GetPaperListForTopic(fieldOfStudyId);
    }

    public async GetMagOrigList(crit: MVCMagPaperListSelectionCriteria): Promise<boolean> {
        if (crit.pageSize == 0) crit.pageSize = this._OrigCriteria.pageSize > 0 ? this._OrigCriteria.pageSize : this.pageSize;
        let res = await this.FetchWithCrit(crit);
        if (typeof res != "boolean") {
            //it worked, so let's replace things accordingly...
            this.currentMagPaper = new MagPaper();
            this._MAGList = new MagList();//
            this._Criteria = new MVCMagPaperListSelectionCriteria();
            this._OrigCriteria = crit;
            this._magTopicsService.WPParentTopics = [];
            this._magTopicsService.WPChildTopics = [];
            this._MAGOriginalList = res;
            //all is well, now get the topics for these papers... We don't wait for it, fire and forget!
            this.GetTopicsForCurrentOrigList();
            return true;
        }
        else {
            //didn't work change nothing and signal the fact
            return false;
        }
    }
    public async GetMagItemsForSearch(item: MagSearch): Promise<boolean> {
        let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        crit.pageSize = 20;
        crit.pageNumber = 0;
        crit.listType = "MagSearchResultsList";
        crit.magSearchText = item.magSearchText;
        return this.GetMagOrigList(crit);
    }
    public GetPaperListForTopic(FieldOfStudyId: number): Promise<boolean> {

        //let id = this._Criteria.magRelatedRunId;
        this._OrigCriteria = new MVCMagPaperListSelectionCriteria();
        //this._Criteria.magRelatedRunId = id;
        this._OrigCriteria.fieldOfStudyId = FieldOfStudyId;
        this._OrigCriteria.listType = "PaperFieldsOfStudyList";
        this._OrigCriteria.pageNumber = 0;
        this._OrigCriteria.pageSize = this.pageSize;
        return this.FetchWithCrit(this._OrigCriteria).then(

            (res: boolean | MagList) => {
                if (typeof res == "boolean") return res;
                else {
                    this._MAGOriginalList = res;
                    return true;
                }
            });
    }
    public async FetchJustMagPaperRecordById(Id: number): Promise<boolean> {
        return this.FetchMagPaperById(Id);
    }
    private FetchMagPaperById(Id: number): Promise<boolean> {
        this.currentMagPaper = new MagPaper();
        this.ClearTopics();
        this._BusyMethods.push("FetchMagPaperId");
        let body = JSON.stringify({ Value: Id });
        return this._httpC.post<MagPaper>(this._baseUrl + 'api/MagPaperList/GetMagPaper', body)
            .toPromise().then(result => {

                this.RemoveBusy("FetchMagPaperId");
                this.currentMagPaper = result;
                //this.currentMagPaper = result;
                return true;
            },
                error => {
                    this.RemoveBusy("FetchMagPaperId");
                    this.modalService.GenericError(error);
                    return false;
                }
            ).catch(
                (error) => {
                    this.modalService.GenericErrorMessage("error with FetchMagPaperId");
                    this.RemoveBusy("FetchMagPaperId");
                    return false;
                });
    }

    public async GetCompleteMagPaperById(Id: number): Promise<boolean> {
        this.MagCitationsByPaperList = new MagList();
        this._MAGList = new MagList();
        let res = await this.FetchMagPaperById(Id);
        if (res == true) {
            return await this.pGetAdditionalPaperDataById(Id);
            //res = await this.GetPaperCitationsList(Id);
            //if (res == true) {
            //    res = await this.GetPaperCitedByList(Id);
            //    if (res == true) {
            //        //as in other places, we'll return before fetching the topics...
            //        this.GetTopicsForCurrentPaper();
            //        return true;
            //    }
            //}
        }
        return false;
    }
    public async GetAdditionalPaperDataForCurrentPaper(): Promise<boolean> {
        this.MagCitationsByPaperList = new MagList();
        this._MAGList = new MagList();
        this.ClearTopics();
        if (this.currentMagPaper.paperId > 0) return await this.pGetAdditionalPaperDataById(this.currentMagPaper.paperId);
        else return false;
    }
    private async pGetAdditionalPaperDataById(Id: number): Promise<boolean> {
        let res = await this.GetPaperCitationsList(Id);
        if (res == true) {
            res = await this.GetPaperCitedByList(Id);
            if (res == true) {
                //as in other places, we'll return before fetching the topics...
                this.GetTopicsForCurrentPaper();
                return true;
            }
        }
        return res;
    }
    private async GetPaperCitationsList(refId: number): Promise<boolean> {

        let criteriaCitationsList: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteriaCitationsList.listType = "CitationsList";
        criteriaCitationsList.magPaperId = refId;
        criteriaCitationsList.pageSize = 20;
        //console.log('calling FetchWithCrit: CitationsList');
        let res = await this.FetchWithCrit(criteriaCitationsList);
        if (typeof res !== "boolean") {
            this._MAGList = res;
            this._Criteria = criteriaCitationsList;
            return true;
        } else return res;
    }

    private async GetPaperCitedByList(refId: number): Promise<boolean> {
        this._CitedCriteria = new MVCMagPaperListSelectionCriteria();
        let criteriaCitedBy: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteriaCitedBy.listType = "CitedByList";
        criteriaCitedBy.magPaperId = refId;
        criteriaCitedBy.pageSize = 20;
        //console.log('calling PostFetchCitedByListList: CitedByList');
        let res = await this.FetchWithCrit(criteriaCitedBy);
        if (typeof res !== "boolean") {
            this._CitedCriteria = criteriaCitedBy;
            this.MagCitationsByPaperList = res;
            return true;
        } else {
            return res;
        }
    }

    private GetParentAndChildFieldsOfStudy(FieldOfStudy: string, FieldOfStudyId: number): Promise<boolean> {
        this._magTopicsService.ShowingParentAndChildTopics = true;
        this._magTopicsService.ShowingChildTopicsOnly = false;
        let selectionCriteria: MvcMagFieldOfStudyListSelectionCriteria = new MvcMagFieldOfStudyListSelectionCriteria();
        selectionCriteria.listType = FieldOfStudy;
        selectionCriteria.fieldOfStudyId = FieldOfStudyId;
        selectionCriteria.SearchTextTopics = '';

        return this._magTopicsService.FetchMagFieldOfStudyList(selectionCriteria, 'CitationsList').then(

            (result: MagFieldOfStudy[] | boolean) => {
                if (typeof result === "boolean") return false;
                else {
                    if (result != null && result.length > 0) {
                        
                        return true;
                    } else {
                        return false;
                    }
                }
            }

            , ((error) => {

                this.modalService.GenericError(error);
                return false;
            })
        );
    }


    private async FetchOrig(): Promise<boolean> {
        let res = await this.FetchWithCrit(this._OrigCriteria);
        if (typeof res != "boolean") {
            this._MAGOriginalList = res;
            if (this._OrigCriteria.listType != "PaperFieldsOfStudyList") {
                //we don't refresh the topics when we're browsing a topic!
                this.GetTopicsForCurrentOrigList();
            }
            return true;
        } else { return false; }
    }
    public FetchOrigParticularPage(pageN: number) {
        this._OrigCriteria.pageNumber = pageN;
        this.FetchOrig();
    }
    public FetchOrigNextPage() {
        if (this._OrigCriteria.pageNumber < this._MAGOriginalList.pagecount - 1) {
            this._OrigCriteria.pageNumber++;
            this.FetchOrig();
        }
    }
    public FetchOrigPrevPage() {
        if (this._OrigCriteria.pageNumber > 0) {
            this._OrigCriteria.pageNumber--;
            this.FetchOrig();
        }
    }
    public FetchOrigLastPage() {
        this._OrigCriteria.pageNumber = this._MAGOriginalList.pagecount - 1;
        this.FetchOrig();
    }
    public FetchOrigFirstPage() {
        this._OrigCriteria.pageNumber = 0;
        this.FetchOrig();
    }


    private async FetchReferences(): Promise<boolean> {
        let res = await this.FetchWithCrit(this._Criteria);
        if (typeof res != "boolean") {
            this._MAGList = res;
            return true;
        } else { return false; }
    }
    public FetchParticularPage(pageN: number) {
        this._Criteria.pageNumber = pageN;
        this.FetchReferences();
    }
    public FetchNextPage() {
        if (this._Criteria.pageNumber < this._MAGList.pagecount - 1) {
            this._Criteria.pageNumber++;
            this.FetchReferences();
        }
    }
    public FetchPrevPage() {
        if (this._Criteria.pageNumber > 0) {
            this._Criteria.pageNumber--;
            this.FetchReferences();
        }
    }
    public FetchLastPage() {
        this._Criteria.pageNumber = this._MAGList.pagecount - 1;
        this.FetchReferences();
    }
    public FetchFirstPage() {
        this._Criteria.pageNumber = 0;
        this.FetchReferences();
    }

    private async FetchCitedByList() {
        let res = await this.FetchWithCrit(this._CitedCriteria);
        if (typeof res != "boolean") {
            this.MagCitationsByPaperList = res;
            return true;
        } else { return false; }
    }
    public FetchCitedParticularPage(pageN: number) {
        this._CitedCriteria.pageNumber = pageN;
        this.FetchCitedByList();
    }
    public FetchCitedNextPage() {
        if (this._CitedCriteria.pageNumber < this.MagCitationsByPaperList.pagecount - 1) {
            this._CitedCriteria.pageNumber++;
            this.FetchCitedByList();
        }
    }
    public FetchCitedPrevPage() {
        if (this._CitedCriteria.pageNumber > 0) {
            this._CitedCriteria.pageNumber--;
            this.FetchCitedByList();
        }
    }
    public FetchCitedLastPage() {
        this._CitedCriteria.pageNumber = this.MagCitationsByPaperList.pagecount - 1;
        this.FetchCitedByList();
    }
    public FetchCitedFirstPage() {
        this._CitedCriteria.pageNumber = 0;
        this.FetchCitedByList();
    }

    public async GetMatchedMagIncludedList(): Promise<boolean> {

        this._magTopicsService.ShowingParentAndChildTopics = false;
        this._magTopicsService.ShowingChildTopicsOnly = true;

        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapers";
        criteria.included = "Included";
        criteria.pageSize = 20;

        let res = await this.GetMagOrigList(criteria);
        return res;
    }
    public async GetMatchedMagExcludedList(): Promise<boolean> {
        this._magTopicsService.ShowingParentAndChildTopics = false;
        this._magTopicsService.ShowingChildTopicsOnly = true;

        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapers";
        criteria.included = "Excluded";
        criteria.pageSize = 20;

        let res = await this.GetMagOrigList(criteria);
        return res;
    }
    public async GetMatchedMagAllList(): Promise<boolean>{
        this._magTopicsService.ShowingParentAndChildTopics = false;
        this._magTopicsService.ShowingChildTopicsOnly = true;
        
        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapers";
        criteria.included = "all";
        criteria.pageSize = 20;

        let res = await this.GetMagOrigList(criteria);
        return res;
    }

    public async GetMatchedMagWithCodeList(att: SetAttribute): Promise<boolean> {
        this._magTopicsService.ShowingParentAndChildTopics = false;
        this._magTopicsService.ShowingChildTopicsOnly = true;
        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapersWithThisCode";
        criteria.attributeIds = att.attribute_id.toString();
        criteria.pageSize = 20;
        let res = await this.GetMagOrigList(criteria);
        return res;
    }

    public ImportMagRelatedSelectedPapers(selectedPapers: MagPaper[]): Promise<MagItemPaperInsertCommand | void> {

        let selectedPapersStr: string = selectedPapers.map(f=> f.paperId).join(',');
        this._BusyMethods.push("ImportMagRelatedSelectedPapers");
        let body = JSON.stringify({ Value: selectedPapersStr });
        return this._httpC.post<MagItemPaperInsertCommand>(this._baseUrl + 'api/MagRelatedPapersRunList/ImportMagRelatedSelectedPapers',
            body)
            .toPromise().then(
                (result: MagItemPaperInsertCommand) => {
                    this.RemoveBusy("ImportMagRelatedSelectedPapers");
                    //this.selectedPapers = []; if you do this here, then we don't know how many papers *should* be selected
                    return result;
                },
                error => {
                    this.RemoveBusy("ImportMagRelatedSelectedPapers");
                    this.modalService.GenericError(error);
                }
            );
    }
    public ClearSelected() {
        for (var i = 0; i < this._MAGList.papers.length; i++) {
            this._MAGList.papers[i].isSelected = false;
        }
        for (var i = 0; i < this._MAGOriginalList.papers.length; i++) {
            this._MAGOriginalList.papers[i].isSelected = false;
        }
        for (var i = 0; i < this.MagCitationsByPaperList.papers.length; i++) {
            this.MagCitationsByPaperList.papers[i].isSelected = false;
        }
        this.selectedPapers = [];
    }
    public Clear() {
        this._Criteria = new MVCMagPaperListSelectionCriteria();
        this._OrigCriteria = new MVCMagPaperListSelectionCriteria();
        this._MAGOriginalList = new MagList();
        this._MAGList = new MagList();
        this.currentMagPaper = new MagPaper();
        this.selectedPapers = [];
        //testing lines below...
        //this.selectedPapers = [new MagPaper()];
        //this.selectedPapers[0].paperTitle = "Oooooh!";
        //this.selectedPapers[0].originalTitle = "it works!";
        //this.selectedPapers[0].fullRecord = "Oooooh! it works! :-)";
    }
}
class junk extends BusyAwareService {
    //constructor(
    //    private _httpC: HttpClient,
    //    @Inject('BASE_URL') private _baseUrl: string,
    //    private _eventEmitterService: EventEmitterService,
    //    private _magTopicsService: MAGTopicsService,
    //    private modalService: ModalService,
    //    private datePipe: DatePipe,
    //    private _mAGBrowserHistoryService: MAGBrowserHistoryService
    //) {
    //    super();
    //}
    //public currentMagRelatedRun: MagRelatedPapersRun = new MagRelatedPapersRun();
    //public currentMagSearch: MagSearch = new MagSearch();
    //public currentTopicSearch: MagFieldOfStudy = new MagFieldOfStudy();
    //public currentMagPaper: MagPaper = new MagPaper();
    //public currentRefreshListType: string = '';
    //public currentListType: string = '';
    //public firstVisitToMAGBrowser: boolean = true;
    //public MagCitationsByPaperList: MagList = new MagList();
    //public MagPaperFieldsList: MagFieldOfStudy[] = [];
    //private _MAGList: MagList = new MagList();
    //private _MAGOriginalList: MagList = new MagList();
    //private _Criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    //private _OrigCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    //private _currentPaper: MagPaper = new MagPaper();
    //public ParentTopic: string = '';
    //public WPParentTopics: TopicLink[] = [];
    //public WPChildTopics: TopicLink[] = [];
    //public ListDescription: string = '';
    //public OrigListDescription: string = '';
    //public selectedPapers: MagPaper[] = [];
    //public SelectedPaperIds: number[] = [];
    //public pageSize: number = 20;
    //@Output() PaperChanged = new EventEmitter();
    //public currentFieldOfStudy: MagFieldOfStudy = new MagFieldOfStudy();
    //public ClearSelected() {
    //    for (var i = 0; i < this.MAGList.papers.length; i++) {
    //        this.MAGList.papers[i].isSelected = false;
    //    }
    //    this.SelectedPaperIds = [];
    //    this.selectedPapers = [];
    //}
    //public get MAGOriginalList(): MagList {
    //    return this._MAGOriginalList;
    //}
    //public set MAGOriginalList(value: MagList) {
    //    this._MAGOriginalList = value;
    //}
    //public get MAGList(): MagList {
    //    if (this.currentMagPaper.paperId > 0) return this._MAGList;
    //    else return this._MAGOriginalList;
    //}
    //public set MAGList(value: MagList) {
    //    this._MAGList = value;
    //}
    //public get ListCriteria(): MVCMagPaperListSelectionCriteria {
    //    return this._Criteria;
    //}
    //public set ListCriteria(value: MVCMagPaperListSelectionCriteria) {
    //    this._Criteria = value;
    //}
    //public get OrigListCriteria(): MVCMagPaperListSelectionCriteria {
    //    return this._OrigCriteria;
    //}
    //public set OrigListCriteria(value: MVCMagPaperListSelectionCriteria) {
    //    this._OrigCriteria = value;
    //}
    //public get currentPaper(): MagPaper {
    //    return this._currentPaper;
    //}
    //public GetPaperListForTopic(FieldOfStudyId: number): Promise<boolean>  {

    //        let id = this.ListCriteria.magRelatedRunId;
    //        this.ListCriteria = new MVCMagPaperListSelectionCriteria();
    //        this.ListCriteria.magRelatedRunId = id;
    //        this.ListCriteria.fieldOfStudyId = FieldOfStudyId;
    //        this.ListCriteria.listType = "PaperFieldsOfStudyList";
    //        this.ListCriteria.pageNumber = 0;
    //        this.ListCriteria.pageSize = this.pageSize;
    //        return this.FetchWithCrit(this.ListCriteria, "PaperFieldsOfStudyList").then(

    //                    (res1: boolean) => {
                        
    //                            if (this._eventEmitterService.firstVisitMAGBrowserPage) {
    //                                this.FetchOrigWithCrit(this.ListCriteria, "PaperFieldsOfStudyList").then(
    //                                    (res2: boolean) => {
    //                                        console.log('got in here!!!!!!!!!!!!');
    //                                        this.firstVisitToMAGBrowser = false;
    //                                        this._eventEmitterService.firstVisitMAGBrowserPage = false;
    //                                        return res2;
    //                                    })
    //                        };
    //                        return res1;
    //                    }
    //            );
    //}

    //public async GetTopicsAndRelatedPapers(item: MagFieldOfStudy) {

    //    this.currentTopicSearch = item;
    //    this.currentRefreshListType = 'PaperFieldsOfStudyList';
    //    //this._eventEmitterService.firstVisitMAGBrowserPage = false;
    //    this.OrigListCriteria.listType = "PaperFieldsOfStudyList";
    //    this._magTopicsService.ShowingParentAndChildTopics = true;
    //    this._magTopicsService.ShowingChildTopicsOnly = false;
    //    this.currentFieldOfStudy = item;
    //    let fieldOfStudyId: number = item.fieldOfStudyId;

    //    await this.GetParentAndChildRelatedPapers(item.displayName, fieldOfStudyId);
    //}

    //public async GetParentAndChildRelatedPapers(displayName: string, fieldOfStudyId: number): Promise<boolean>  {

    //    //this._eventEmitterService.firstVisitMAGBrowserPage = false;
    //    this.currentRefreshListType = 'PaperFieldsOfStudyList';
    //    this.currentListType = "PaperFieldsOfStudyList";
    //    this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem(displayName, "BrowseTopic", 0,
    //        "", "", 0, "", "", fieldOfStudyId, displayName, "", 0));

    //    this.currentMagPaper = new MagPaper();
    //    this._magTopicsService.WPChildTopics = [];
    //    this._magTopicsService.WPParentTopics = [];
    //    this.ParentTopic = '';

    //    this.ParentTopic = displayName;
    //    await this.GetParentAndChildFieldsOfStudy("FieldOfStudyParentsList", fieldOfStudyId)

    //    await this.GetParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", fieldOfStudyId);
        
    //    return await this.GetPaperListForTopic(fieldOfStudyId);

    //}

    //public GetPaperListForTopicsAfterRefresh(fieldOfStudy: MagFieldOfStudy, dateFrom: Date, dateTo: Date): boolean | undefined {

    //    let dateFormattedFrom: string | null = this.datePipe.transform(dateFrom, 'yyyy-MM-dd'); 
    //    let dateFormattedTo: string | null = this.datePipe.transform(dateTo, 'yyyy-MM-dd'); 

    //    if (fieldOfStudy.fieldOfStudyId != null) {

    //        let id = this.ListCriteria.magRelatedRunId;
    //        this.ListCriteria = new MVCMagPaperListSelectionCriteria();
    //        this.ListCriteria.magRelatedRunId = id;
    //        this.ListCriteria.fieldOfStudyId = fieldOfStudy.fieldOfStudyId;
    //        this.ListCriteria.listType = "PaperFieldsOfStudyList";
    //        this.ListCriteria.pageNumber = 0;
    //        this.ListCriteria.pageSize = this.pageSize;
    //        if (dateFormattedFrom != null) {
    //            this.ListCriteria.dateFrom = dateFormattedFrom;
    //        }
    //        if (dateFormattedTo != null) {
    //            this.ListCriteria.dateTo = dateFormattedTo;
    //        }
    //        this.FetchWithCrit(this.ListCriteria, "PaperFieldsOfStudyList").then(

    //            (res: boolean) => { return res; }
    //        );

    //    } else {
    //        return false;
    //    }
    //}
    //public ImportMagRelatedSelectedPapers(selectedPapers: number[]): Promise<MagItemPaperInsertCommand | void> {

    //    let selectedPapersStr: string = selectedPapers.join(',');
    //    this._BusyMethods.push("ImportMagRelatedSelectedPapers");
    //    let body = JSON.stringify({ Value: selectedPapersStr });
    //    return this._httpC.post<MagItemPaperInsertCommand>(this._baseUrl + 'api/MagRelatedPapersRunList/ImportMagRelatedSelectedPapers',
    //        body)
    //        .toPromise().then(
    //            (result: MagItemPaperInsertCommand) => {
    //                this.RemoveBusy("ImportMagRelatedSelectedPapers");
    //                return result;
    //            },
    //            error => {
    //                this.RemoveBusy("ImportMagRelatedSelectedPapers");
    //                this.modalService.GenericErrorMessage('an api call error with ImportMagRelatedSelectedPapers: ' + error);
    //            }
    //        );
    //}

    //public GetParentAndChildFieldsOfStudy(FieldOfStudy: string, FieldOfStudyId: number): Promise<boolean> {
    //    this._magTopicsService.ShowingParentAndChildTopics = true;
    //    this._magTopicsService.ShowingChildTopicsOnly = false;
    //    let selectionCriteria: MvcMagFieldOfStudyListSelectionCriteria = new MvcMagFieldOfStudyListSelectionCriteria();
    //    selectionCriteria.listType = FieldOfStudy;
    //    selectionCriteria.fieldOfStudyId = FieldOfStudyId;
    //    selectionCriteria.SearchTextTopics = '';
        
    //    return this._magTopicsService.FetchMagFieldOfStudyList(selectionCriteria, 'CitationsList').then(

    //        (result: MagFieldOfStudy[] | boolean) => {
    //            if (typeof result === "boolean" ) return false;
    //            else {
    //                if (result != null && result.length > 0) {
    //                    return true;
    //                } else {
    //                    return false;
    //                }
    //            }
    //        }

    //        , ((error) => {
 
    //                    this.modalService.GenericError(error);
    //                    return false;})
    //            );
    //}


    //public FetchMAGRelatedPaperRunsListId(Id: number) {
        
    //    this._BusyMethods.push("FetchMAGRelatedPaperRunsListId");
    //    let body = JSON.stringify({ Value: Id });
    //    return this._httpC.post<MagList>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRunsId',
    //        body)
    //        .subscribe(
    //            (result) => {
    //                this.firstVisitToMAGBrowser = false;
    //                this._eventEmitterService.firstVisitMAGBrowserPage = false;
    //                this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
    //                this.MAGList = result;
    //                this.MAGOriginalList = result;
    //                this.ListCriteria.listType = "MagRelatedPapersRunList";
    //                this.ListCriteria.pageSize = 20;
    //                this.ListCriteria.magRelatedRunId = Id;
    //            },
    //            error => {
    //                this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
    //                this.modalService.GenericErrorMessage('an api call error with FetchMAGRelatedPaperRunsListId: ' + error);
    //            }
    //        );
    //}

    //public GetMagRelatedRunsListById(item: MagRelatedPapersRun) : Promise<boolean> {

    //    this.currentMagRelatedRun = item;
    //    this.currentRefreshListType = 'MagRelatedPapersRunList';
    //    this.currentMagPaper = new MagPaper();
    //    this.MagCitationsByPaperList.papers = [];
    //    this.MAGOriginalList.papers = [];
    //    this.currentListType = "MagRelatedPapersRunList";
    //    this.currentMagPaper = new MagPaper();
    //    this._magTopicsService.ShowingParentAndChildTopics = false;
    //    this._magTopicsService.ShowingChildTopicsOnly = true;
    //    this._magTopicsService.WPParentTopics = [];
    //    this._magTopicsService.WPChildTopics = [];
    //    this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Papers identified from auto-identification run", "MagRelatedPapersRunList", 0,
    //        "", "", 0, "", "", 0, "", "", item.magRelatedRunId));

    //    return this.FetchMAGRelatedPaperRunsListById(item.magRelatedRunId);
 
    //}


    //public FetchMAGRelatedPaperRunsListById(Id: number): Promise<boolean> {

    //    var goBackListType: string = 'MagRelatedPapersRunList';
    //    this._BusyMethods.push("FetchMAGRelatedPaperRunsListById");
    //    this.ListCriteria.listType = "MagRelatedPapersRunList";
    //    this.ListCriteria.pageSize = this.pageSize;
    //    this.ListCriteria.magRelatedRunId = Id;
    //    this.ListCriteria.pageNumber = 0;

    //    this.OrigListCriteria.listType = "MagRelatedPapersRunList";
    //    this.OrigListCriteria.pageSize = this.pageSize;
    //    this.OrigListCriteria.magRelatedRunId = Id;
    //    this.OrigListCriteria.pageNumber = 0;


    //    return this._httpC.post<MagList>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRunsId',
    //        this.ListCriteria)
    //        .toPromise().then(
    //            (result) => {

                   
    //                this.RemoveBusy("FetchMAGRelatedPaperRunsListById");
    //                //this.MAGList = result;
    //                this.MAGOriginalList = result;//new MagList();
    //                //this.MAGOriginalList.pagecount = result.pagecount;
    //                //this.MAGOriginalList.pageindex = result.pageindex;
    //                //this.MAGOriginalList.pagesize = result.pagesize;
    //                //this.MAGOriginalList.totalItemCount = result.totalItemCount;
    //                this.currentListType = "MagRelatedPapersRunList";

    //                if (this.MAGList.papers != null && this.MAGList.papers.length > 0) {

    //                    this.MAGList.papers.forEach((item: any) => {
    //                        this.MAGOriginalList.papers.push(item);
    //                    });
    //                }
    //                this.ListCriteria.paperIds = '';
    //                for (var i = 0; i < result.papers.length; i++) {
                       
    //                    this.ListCriteria.paperIds += result.papers[i].paperId.toString() + ',';
    //                }
    //                this.ListCriteria.paperIds = this.ListCriteria.paperIds.substr(0, this.ListCriteria.paperIds.length - 1);
    //                this.ListCriteria.pageNumber += 1;
                   
    //                let FieldsListcriteria: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
    //                FieldsListcriteria.fieldOfStudyId = 0;
    //                FieldsListcriteria.listType = "PaperFieldOfStudyList";
    //                FieldsListcriteria.paperIdList = this.ListCriteria.paperIds;
    //                this.SavePapers(result, this.ListCriteria, "NormalList");
    //                this.SaveOrigPapers(result, this.OrigListCriteria, "PaperFieldOfStudyList");
    //                FieldsListcriteria.SearchTextTopics = ''; 
    //                return this._magTopicsService.FetchMagFieldOfStudyList(FieldsListcriteria, goBackListType).then(

    //                    (res: MagFieldOfStudy[] | boolean) => {

    //                        this._eventEmitterService.firstVisitMAGBrowserPage = false;

    //                        if (typeof res != "boolean") {
    //                            return true;
    //                        } else {
    //                            return false;
    //                        }
    //                    }
    //                );
    //            },
    //            error => {
    //                this.RemoveBusy("FetchMAGRelatedPaperRunsListById");
    //                this.modalService.GenericError(error);
    //                return false;
    //            }
    //        ).catch(
    //            (error) => {

    //                this.modalService.GenericErrorMessage("error with FetchMAGRelatedPaperRunsListById");
    //                this.RemoveBusy("FetchMAGRelatedPaperRunsListById");
    //                return false;
    //            });
    //}

    //public async GetMagItemsForSearch(item: MagSearch): Promise<boolean> {

    //    this.currentMagSearch = item;
    //    this.currentMagPaper = new MagPaper();
    //    this.MagCitationsByPaperList.papers = [];
    //    this.MAGOriginalList.papers = [];
    //    this.currentRefreshListType = 'MagSearchResultsList';
    //    this.currentListType = "MagSearchResultsList";
    //    this._magTopicsService.ShowingParentAndChildTopics = false;
    //    this._magTopicsService.ShowingChildTopicsOnly = true;
    //    this._magTopicsService.WPChildTopics = [];
    //    this._magTopicsService.WPParentTopics = [];
    //    this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Papers identified from Mag Search run", "MagSearchPapersList", 0,
    //        "", "", 0, "", "", 0, "", "", item.magSearchId));
    //    let selectionCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    //    selectionCriteria.pageSize = 20;
    //    selectionCriteria.pageNumber = 0;
    //    selectionCriteria.listType = "MagSearchResultsList";
    //    selectionCriteria.magSearchText = item.magSearchText;
    //    return await this.FetchMagPapersFromSearch(selectionCriteria, "MagSearchResultsList");
    //}


    //public async FetchMagPapersFromSearch(criteria: MVCMagPaperListSelectionCriteria, goBackListType: string) : Promise<boolean> {
    //    let res: boolean = false;
    //    res = await this.FetchOrigWithCrit(criteria, "MagSearchResultsList");
    //    if (res == true) {
    //        this.MAGList = this.MAGOriginalList;
    //        let FieldsListcriteria: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
    //        FieldsListcriteria.fieldOfStudyId = 0;
    //        FieldsListcriteria.listType = "PaperFieldOfStudyList";
    //        FieldsListcriteria.paperIdList = this.ListCriteria.paperIds;
    //        let res2: MagFieldOfStudy[] | boolean = await this._magTopicsService.FetchMagFieldOfStudyList(FieldsListcriteria, 'MagSearchResultsList')
    //        if (res2 !== false) return true;
    //        else return false;
    //        //return await this.FetchOrigWithCrit(criteria, "OrigList");
    //    } else return false;

    //}
    //public async GoToListOfAutoUpdatePapers(crit: MVCMagPaperListSelectionCriteria, listDescription: string) {
    //    this._mAGBrowserHistoryService.AddHistory(MagBrowseHistoryItem.MakeFromAutoUpdateListCrit(crit));
    //    let res: boolean = await this.FetchOrigWithCrit(crit, listDescription);
    //    if (res == true) this.MAGList = this.MAGOriginalList;
    //    return res;
    //}
    //public FetchWithCrit(crit: MVCMagPaperListSelectionCriteria, listDescription: string): Promise<boolean> {

    //    this._BusyMethods.push("FetchWithCrit");
    //    this.ListCriteria = crit;
    //    if (this.MAGList && this._MAGList.pagesize > 0
    //        && this.MAGList.pagesize <= 4000
    //        && this.MAGList.pagesize != crit.pageSize
    //    ) {
    //        crit.pageSize = this.MAGList.pagesize;
    //    }
    //    this.ListCriteria.paperIds = crit.paperIds;
    //    this.ListDescription = listDescription;

    //    return this._httpC.post<MagList>(this._baseUrl + 'api/MagPaperList/GetMagPaperList', crit)
    //        .toPromise().then(

    //        (list: MagList) => {

    //                this.RemoveBusy("FetchWithCrit");
    //                this.SavePapers(list, this.ListCriteria, "NormalList");
    //                return true;
                                    
    //            }, error => {
    //                this.modalService.GenericError(error);
    //                this.RemoveBusy("FetchWithCrit");
    //                return false;
    //            }
    //        ).catch(
    //            (error) => {

    //                this.modalService.GenericErrorMessage("error with FetchWithCrit: " + error);
    //                this.RemoveBusy("FetchWithCrit");
    //                return false;
    //        });
    //}

    //public FetchOrigWithCrit(crit: MVCMagPaperListSelectionCriteria, listDescription: string): Promise<boolean> {

    //    this._BusyMethods.push("FetchOrigWithCrit");
    //    this.OrigListCriteria = crit;
    //    if (this.MAGOriginalList && this._MAGOriginalList.pagesize > 0
    //        && this.MAGOriginalList.pagesize <= 4000
    //        && this.MAGOriginalList.pagesize != crit.pageSize
    //    ) {
    //        crit.pageSize = this._MAGOriginalList.pagesize;
    //    }

    //    this.OrigListCriteria.paperIds = crit.paperIds;
    //    return this._httpC.post<MagList>(this._baseUrl + 'api/MagPaperList/GetMagPaperList', crit)
    //        .toPromise().then(

    //            (list: MagList) => {

    //                this.RemoveBusy("FetchOrigWithCrit");
    //                this._eventEmitterService.firstVisitMAGBrowserPage = false;
    //                this.SaveOrigPapers(list, this.OrigListCriteria, 'OrigList');
    //                return true;

    //            }, error => {
    //                this.modalService.GenericError(error);
    //                this.RemoveBusy("FetchOrigWithCrit");
    //                return false;
    //            }
    //        ).catch(
    //            (error) => {

    //                this.modalService.GenericErrorMessage("error with FetchOrigWithCrit: " + error);
    //                this.RemoveBusy("FetchOrigWithCrit");
    //                return false;
    //            });
    //}
    //public SaveOrigPapers(list: MagList, crit: MVCMagPaperListSelectionCriteria, referenceList: string) {


    //    if (crit.listType == 'CitationsList' || crit.listType == 'ReviewMatchedPapers' || crit.listType == 'MagSearchResultsList'
    //        || crit.listType == '"MagRelatedPapersRunList"' || crit.listType == '"PaperFieldsOfStudyList"'
    //        || crit.listType == 'MagSearchResultsList') {

    //        if (referenceList == 'OrigList') {

    //            this._OrigCriteria.paperIds = '';
    //            for (var i = 0; i < list.papers.length; i++) {
    //                this._OrigCriteria.paperIds += list.papers[i].paperId + ',';
    //            }
    //            this._OrigCriteria.paperIds = this._OrigCriteria.paperIds.substr(0, this._OrigCriteria.paperIds.length - 2);
    //        }

    //    } else if (crit.listType == 'CitedByList') {

    //        this._Criteria.paperIds = '';
    //        for (var i = 0; i < list.papers.length; i++) {
    //            this._Criteria.paperIds += list.papers[i].paperId + ',';
    //        }
    //        this._Criteria.paperIds = this._Criteria.paperIds.substr(0, this._Criteria.paperIds.length - 2);
    //        this.MagCitationsByPaperList = list;
    //        this._Criteria = crit;
    //        return;
    //    }

    //    if (referenceList == 'OrigList') {
    //        this._MAGOriginalList = list;
    //        this._OrigCriteria = crit;
    //        this._MAGOriginalList.totalItemCount = list.totalItemCount;
    //        this._MAGOriginalList.pagecount = list.pagecount;
    //    } 

    //}
    //public SavePapers(list: MagList, crit: MVCMagPaperListSelectionCriteria, referenceList: string ) {

    //    if (crit.listType == 'CitationsList' || crit.listType == 'ReviewMatchedPapers' || crit.listType == 'MagSearchResultsList'
    //        || crit.listType == '"MagRelatedPapersRunList"' || crit.listType == '"PaperFieldsOfStudyList"'
    //    || crit.listType =='MagSearchResultsList') {
                       
    //            this._Criteria.paperIds = '';
    //            for (var i = 0; i < list.papers.length; i++) {
    //                this._Criteria.paperIds += list.papers[i].paperId + ',';
    //            }
    //                this._Criteria.paperIds = this._Criteria.paperIds.substr(0, this._Criteria.paperIds.length - 2);
                       
    //    } else if (crit.listType == 'CitedByList') {

    //        this._Criteria.paperIds = '';
    //        for (var i = 0; i < list.papers.length; i++) {
    //            this._Criteria.paperIds += list.papers[i].paperId + ',';
    //        }
    //        this._Criteria.paperIds = this._Criteria.paperIds.substr(0, this._Criteria.paperIds.length - 2);
    //        this.MagCitationsByPaperList = list;
    //        this._Criteria = crit;
    //        return;
    //    } 
    //    this._MAGList = list;
    //    this._Criteria = crit;

    //}

    //public FetchNextPage() {
    //    if (this.MAGList.pageindex < this.MAGList.pagecount - 1) {
    //        this.MAGList.pageindex += 1;
    //    } 
    //    this.ListCriteria.pageNumber = this.MAGList.pageindex;
    //    this.ListCriteria.pageSize = this.pageSize;
    //    this.FetchWithCrit(this.ListCriteria, this.ListCriteria.listType);
    //}
    //public FetchPrevPage() {
        
    //    if (this.MAGList.pageindex == 0 ) {
    //        return this.FetchWithCrit(this._Criteria, this.ListDescription);
    //    } else {
    //        this._Criteria.pageNumber -= 1;
    //        return this.FetchWithCrit(this._Criteria, this.ListDescription);
    //    }
    //}
    //public FetchLastPage() {
    //    this._Criteria.pageNumber = this.MAGList.pagecount - 1;
    //    return this.FetchWithCrit(this._Criteria, this.ListDescription);
    //}
    //public FetchFirstPage() {
    //    this._Criteria.pageNumber = 0;
    //    return this.FetchWithCrit(this._Criteria, this.ListDescription);
    //}
    //public FetchParticularPage(pageNum: number) {
    //    this._Criteria.pageNumber = pageNum;
    //    return this.FetchWithCrit(this._Criteria, this.ListDescription);
    //}

    //public FetchOrigNextPage() {
    //    this.OrigListCriteria.pageNumber = this.MAGOriginalList.pageindex;
    //    this.OrigListCriteria.listType = this.currentListType;
    //    if (this.MAGOriginalList.pageindex < this.MAGOriginalList.pagecount - 1) {
    //        this.MAGOriginalList.pageindex += 1;
    //    }
    //    this.OrigListCriteria.pageNumber = this.MAGOriginalList.pageindex;
    //    this.OrigListCriteria.pageSize = this.pageSize;
    //    this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType)
    //}
    //public FetchOrigPrevPage() {
    //    console.log('curentList: ', this.currentListType);
    //    this.OrigListCriteria.listType = this.currentListType;
    //    this.OrigListCriteria.pageNumber = this.MAGOriginalList.pageindex;
    //    if (this.MAGOriginalList.pageindex == 0) {
    //        return this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType);
    //    } else {
    //        this.OrigListCriteria.pageNumber -= 1;
    //        return this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType);
    //    }
    //}
    //public FetchOrigLastPage() {
    //    this._OrigCriteria.pageNumber = this.MAGOriginalList.pagecount - 1;
    //    return this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType);
    //}
    //public FetchOrigFirstPage() {
    //    this.OrigListCriteria.pageNumber = 0;
    //    return this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType);
    //}
    //public FetchOrigParticularPage(pageNum: number) {
    //    this._OrigCriteria.pageNumber = pageNum;
    //    return this.FetchOrigWithCrit(this.OrigListCriteria, this.OrigListCriteria.listType);
    //}
    //public get SelectedPapers(): MagPaper[] {
    //    return this._MAGList.papers.filter(found => found.isSelected == true);
    //}
    //public Clear() {

    //    this._currentPaper = new MagPaper();
    //    this.MAGList = new MagList();
    //    this.MAGOriginalList = new MagList();
    //    this.MagCitationsByPaperList = new MagList();
    //    this.selectedPapers = [];
    //    this.ClearTopics();
    //}
    //public ClearTopics() {
    //    this.ParentTopic = '';
    //    this.WPChildTopics = [];
    //    this.WPParentTopics = [];
    //}
}


