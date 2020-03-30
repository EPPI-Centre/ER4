import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient,  } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';

import { ReadOnlySource } from './sources.service';
import { EventEmitterService } from './EventEmitter.service';
import { MagPaper, MVCMagPaperListSelectionCriteria, MagPaperList, MVCMagFieldOfStudyListSelectionCriteria } from './magAdvanced.service';
import { MagList } from './BasicMAG.service';


@Injectable({

	providedIn: 'root',

    }
)

export class MAGListService extends BusyAwareService {

	//private _MAGListOptions: MAGListOptions = new MAGListOptions();
    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
		private _eventEmitterService: EventEmitterService,
		private ModalService: ModalService
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
    public get currentPaper(): MagPaper {
        return this._currentPaper;
    }
    FetchMAGRelatedPaperRunsListId(Id: number): Promise<void> {

        this._BusyMethods.push("FetchMAGRelatedPaperRunsListId");
        this.ListCriteria.listType = "MagRelatedPapersRunList";
        this.ListCriteria.pageSize = 20;
        this.ListCriteria.magRelatedRunId = Id;

        return this._httpC.post<MagList>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRunsId',
            this.ListCriteria)
            .toPromise().then(
                (result) => {

                    console.log(result);
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                    this.MAGList = result;
                    this.ListCriteria.paperIds = '';
                    for (var i = 0; i < result.papers.length; i++) {
                       
                        this.ListCriteria.paperIds += result.papers[i].paperId.toString() + ',';
                    }
                    this.ListCriteria.paperIds = this.ListCriteria.paperIds.substr(0, this.ListCriteria.paperIds.length - 1);
                    this.SavePapers(result.papers, this._Criteria);
                    this.ListCriteria.pageNumber += 1;
                    this.FetchMagFieldOfStudyList(this.ListCriteria.paperIds);

                },
                error => {
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                    //this.modalService.GenericError(error);
                }
            );
    }
    public MagPaperFieldsList: MagPaper[] = [];
    FetchMagFieldOfStudyList(paperIds: string): Promise<void> {

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
                    //this.modalService.GenericError(error);
                }
            );
    }
    public FetchWithCrit(crit: MVCMagPaperListSelectionCriteria, listDescription: string) {
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
        this._httpC.post<MagPaper[]>(this._baseUrl + 'api/MagCurrentInfo/GetMagPaperList', crit)
            .subscribe(
                list => {
					//this._Criteria.numResults = this.MAGList.totalItemCount;
                    console.log('papers result from controller are: ', list);

                    this.SavePapers(list, this._Criteria);

                    console.log('aksdjh: CHEKC: ', JSON.stringify(this.MAGList.papers.length));

                }, error => {
                    this.ModalService.GenericError(error);
                    this.RemoveBusy("FetchWithCrit");
                }
                , () => { this.RemoveBusy("FetchWithCrit"); }
            );
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


    public SavePapers(papers: MagPaper[], crit: MVCMagPaperListSelectionCriteria) {

        console.log('Inside savepapers sort descriptor is: ', this.sort);

        //papers = orderBy(papers, this.sort); 

        this._MAGList.papers = papers;
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
        this.FetchMAGRelatedPaperRunsListId(this._Criteria.magRelatedRunId);
        //this.FetchWithCrit(this._Criteria, this.ListDescription)
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

