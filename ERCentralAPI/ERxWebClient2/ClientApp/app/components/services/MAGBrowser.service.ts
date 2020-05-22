import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient  } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import {
    MagList, MagPaper, MVCMagFieldOfStudyListSelectionCriteria,
    MVCMagPaperListSelectionCriteria, MagFieldOfStudy, MvcMagFieldOfStudyListSelectionCriteria
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
    public GetPaperListForTopic(FieldOfStudyId: number): any {

        //console.log('we need to get in here...');
        let id = this.ListCriteria.magRelatedRunId;
        this.ListCriteria = new MVCMagPaperListSelectionCriteria();
        this.ListCriteria.magRelatedRunId = id;
        this.ListCriteria.fieldOfStudyId = FieldOfStudyId;
        this.ListCriteria.listType = "PaperFieldsOfStudyList";
        this.ListCriteria.pageNumber = 0;
        this.ListCriteria.pageSize = 20;
        this.FetchWithCrit(this.ListCriteria, "PaperFieldsOfStudyList");

    }
    public GetParentAndChildFieldsOfStudy(FieldOfStudy: string, FieldOfStudyId: number, ParentOrChild: string): Promise<void> {


        console.log(' ' + FieldOfStudy + ' ' + FieldOfStudyId + ' ' + ParentOrChild);

        let selectionCriteria: MvcMagFieldOfStudyListSelectionCriteria = new MvcMagFieldOfStudyListSelectionCriteria();
        selectionCriteria.listType = FieldOfStudy;
        selectionCriteria.fieldOfStudyId = FieldOfStudyId;
        selectionCriteria.SearchTextTopics = '';
        return this.FetchMagFieldOfStudyList(selectionCriteria, 'CitationsList').then(

            () => { }
            //(result: MagFieldOfStudy[] | void) => {

            //    if (result != null) {

            //        let FosList: MagFieldOfStudy[] = result;
            //        let i: number = 1.7;
            //        let j: number = 1.7;
            //        for (var fos of FosList) {
                       
            //            let item: TopicLink = new TopicLink();
            //            item.displayName = fos.displayName;
            //            item.fontSize = i;
            //            item.fieldOfStudyId = fos.fieldOfStudyId;

            //            if (ParentOrChild == 'Parent topics') {
            //                this.WPParentTopics.push(item);
            //                if (i > 0.1) {
            //                    i -= 0.05;
            //                }

            //            } else {
            //                this.WPChildTopics.push(item);
            //                if (j > 0.1) {
            //                    j -= 0.05;
            //                }
            //            }
            //        }
            //    }
            //}
        );
    }
    public FetchMAGRelatedPaperRunsListById(Id: number): Promise<boolean> {

        var goBackListType: string = 'MagRelatedPapersRunList';
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
                    FieldsListcriteria.SearchTextTopics = ''; //searchText;
                    this.FetchMagFieldOfStudyList(FieldsListcriteria, goBackListType);
                    return true;
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

        console.log('MAGList service 2' + criteria);

        this._BusyMethods.push("FetchMagPaperList");
        return this._httpC.post<MagFieldOfStudy[]>(this._baseUrl + 'api/MagCurrentInfo/GetMagFieldOfStudyList', criteria)
            .toPromise().then(
            (result: MagFieldOfStudy[]) => {

                    this.RemoveBusy("FetchMagPaperList");
                    // try something here

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
                                        i -= 0.01;
                                    }
                                    item.fontSize = i;
                                    this.WPParentTopics.push(item);
                                    

                                } else {
                                    if (j > 0.1) {
                                        j -= 0.01;
                                    }
                                    item.fontSize = j;
                                    this.WPChildTopics.push(item);
                                   
                                }
                            }
                        }

                    // end try
                    console.log('paper field list: ', result);
                    this.ListCriteria.listType = goBackListType;
                    return result;
                },
                error => {
                    this.RemoveBusy("FetchMagPaperList");
                    this.modalService.GenericError(error);
                    return error;
                }
            ).catch(
            (error) => {

                this.modalService.GenericErrorMessage("error with FetchMagPaperList: " + error);
                this.RemoveBusy("FetchMagPaperList");
                return error;
            });
    }
    public FetchWithCrit(crit: MVCMagPaperListSelectionCriteria, listDescription: string): Promise<boolean> {

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

        this.ListCriteria.paperIds = crit.paperIds;
        this.ListDescription = listDescription;

        return this._httpC.post<MagList>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaperList', crit)
            .toPromise().then(

            (list: MagList) => {

                    this.RemoveBusy("FetchWithCrit");
                    this.SavePapers(list, this._Criteria);

                console.log('paperIds after save papers: ', this.ListCriteria.paperIds);
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
            console.log('inside savepapers: ', this._Criteria.paperIds);
            
            this._MAGList = list;
            this._Criteria = crit;
           
        } else if (crit.listType == 'CitedByList') {

            this._Criteria.paperIds = '';
            for (var i = 0; i < list.papers.length; i++) {
                this._Criteria.paperIds += list.papers[i].paperId + ',';
            }
            this._Criteria.paperIds = this._Criteria.paperIds.substr(0, this._Criteria.paperIds.length - 2);
            console.log('inside savepapers: ', this._Criteria.paperIds);
            this.MagCitationsByPaperList = list;
            this._Criteria = crit;
           
        } else if (crit.listType == 'MagRelatedPapersRunList') {

            //too many if conditions there should be an abstract class that they all inherit from
            console.log('new and used to solve bug...');

            this._MAGList = list;
            this._Criteria = crit;
        } else if (crit.listType == 'PaperFieldsOfStudyList') {

            console.log('PaperFieldsOfStudyList call inside if else');
            this._MAGList = list;
            this._Criteria = crit;
        }     
       
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

    public FetchNextPage() {
        if (this.MAGList.pageindex < this.MAGList.pagecount-1) {
            this.MAGList.pageindex += 1;
        } 
        this._Criteria.pageNumber = this.MAGList.pageindex;
        this._Criteria.pageSize = 20;
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
        this.WPChildTopics = [];
        this.WPParentTopics = [];
        this.MAGList = new MagList();
        this.MagCitationsByPaperList = new MagList();

    }

}

export class TopicLink {

    displayName: string = '';
    fontSize: number = 0;
    callToFOS: string = '';
    fieldOfStudyId: number = 0;
}


