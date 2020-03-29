import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient,  } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';

import { ReadOnlySource } from './sources.service';
import { EventEmitterService } from './EventEmitter.service';
import { MagPaper } from './magAdvanced.service';


@Injectable({

	providedIn: 'root',

    }
)

export class MAGListService extends BusyAwareService {

	private _MAGListOptions: MAGListOptions = new MAGListOptions();
    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
		private eventEmitterService: EventEmitterService,
		private ModalService: ModalService
    ) {
		super();
		
	}

	
    private _MAGList: MAGList = new MAGList();
    private _Criteria: Criteria = new Criteria();
    private _currentPaper: MagPaper = new MagPaper();
 
    public ListDescription: string = "";
    @Output() PaperChanged = new EventEmitter();
    @Output() ListChanged = new EventEmitter();

	public get MAGList(): MAGList {
        return this._MAGList;
    }
    public get ListCriteria(): Criteria {
        return this._Criteria;
    }
    public get currentPaper(): MagPaper {
        return this._currentPaper;
    }
  
    public FetchWithCrit(crit: Criteria, listDescription: string) {
        this._BusyMethods.push("FetchWithCrit");
        this._Criteria = crit;
        if (this._MAGList && this._MAGList.pagesize > 0
            && this._MAGList.pagesize <= 4000
            && this._MAGList.pagesize != crit.pageSize
        ) {
            crit.pageSize = this._MAGList.pagesize;
        }

        this.ListDescription = listDescription;
        this._httpC.post<MAGList>(this._baseUrl + 'api/MAGList/Fetch', crit)
            .subscribe(
                list => {
					this._Criteria.totalPapers = this.MAGList.totalPaperCount;
					console.log();
                    this.SavePapers(list, this._Criteria);
                    this.ListChanged.emit();
					console.log('aksdjh: CHEKC: ', JSON.stringify(this.MAGList.Papers.length));
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
  
    
    public UpdatePaper(MagPaper: MagPaper) {
        this._BusyMethods.push("UpdatePaper");
        this._httpC.post<MagPaper>(this._baseUrl + 'api/MAGList/UpdatePaper', MagPaper)
            .subscribe(
                result => {
                   //if we get an MagPaper back, put it in the list substituting it via paperId
                    if (MagPaper.paperId == 0) {
                        //we created a new MagPaper, add to current list, so users can see it immediately...
                        //this._currentPaper = result;//not sure we need this...
                        this.MAGList.Papers.push(result);
                    }
                    else {
                        //try to replace MagPaper in current list. We use the client side object 'cause the typename might otherwise be wrong.
                        let i = this.MAGList.Papers.findIndex(found => found.paperId == MagPaper.paperId);
                        if (i !== -1) {
                            //console.log("replacing updated MagPaper.", this.MAGList.Papers[i]);
                            this.MAGList.Papers[i] = MagPaper;
                            console.log("replaced updated MagPaper.");//, this.MAGList.Papers[i]);
                        }
                        else {
                            console.log("updated MagPaper not replaced: could not find it...");
                        }
					}
					this.RemoveBusy("UpdatePaper");
                }, error => {
                    this.ModalService.GenericError(error);
                    this.RemoveBusy("UpdatePaper");
                }
            , () => { this.RemoveBusy("UpdatePaper"); }
        );

    }


    public GetIncludedPapers() {
        let cr: Criteria = new Criteria();
        cr.listType = 'StandardMAGList';
        this.FetchWithCrit(cr, "Included Papers");
    }
    public GetExcludedPapers() {
        let cr: Criteria = new Criteria();
        cr.listType = 'StandardMAGList';
        cr.onlyIncluded = false;
        this.FetchWithCrit(cr, "Excluded Papers");
    }
    public GetDeletedPapers() {
        let cr: Criteria = new Criteria();
        cr.listType = 'StandardMAGList';
        cr.onlyIncluded = false;
        cr.showDeleted = true;
        this.FetchWithCrit(cr, "Excluded Papers");
	}


    public SavePapers(Papers: MAGList, crit: Criteria) {
        //console.log('saving Papers');
        Papers.Papers = orderBy(Papers.Papers, this.sort); 
        this._MAGList = Papers;
        this._Criteria = crit;
        //this.Save();
    }
    private ChangingPaper(newPaper: MagPaper) {

        this._currentPaper = newPaper;
		this.PaperChanged.emit(newPaper);
    }
	public getPaper(paperId: number): MagPaper {

        console.log('getting MagPaper');
        let ff = this.MAGList.Papers.find(found => found.paperId == paperId);
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
            let ff = this.MAGList.Papers.findIndex(found => found.paperId == paperId);
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
        let ff = this.MAGList.Papers[0];
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
        
        let ff = this.MAGList.Papers.findIndex(found => found.paperId == paperId);
        if (ff != undefined && ff != null && ff > -1 && ff < this._MAGList.Papers.length) {
            //this.ChangingPaper(this._MAGList.Papers[ff - 1]);
            return this._MAGList.Papers[ff - 1];
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
            let ff = this.MAGList.Papers.findIndex(found => found.paperId == paperId);
            if (ff != undefined && ff != null && ff > -1 && ff + 1 < this._MAGList.Papers.length) return true;
            else return false;
        //}
    }
    public getNext(paperId: number): MagPaper {
        //console.log('getNext');
        let ff = this.MAGList.Papers.findIndex(found => found.paperId == paperId);
        //console.log(ff);
        if (ff != undefined && ff != null && ff > -1 && ff + 1 < this._MAGList.Papers.length) {
            //console.log('I am emitting');
            //this.ChangingPaper(this._MAGList.Papers[ff + 1]);
            return this._MAGList.Papers[ff + 1];
        }
        else {
            //this.ChangingPaper(new MagPaper());
            return new MagPaper();
        }
	}
    public getLast(): MagPaper {
        let ff = this.MAGList.Papers[this._MAGList.Papers.length - 1];
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
            this._Criteria.pageNumber += 1;
        } else {
        }
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
        field: 'shortTitle',
        dir: 'asc'
    }];
    public sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        console.log('sorting Papers by ' + this.sort[0].field + " ");
        this._MAGList.Papers = orderBy(this._MAGList.Papers, this.sort);
    }
    public get HasSelectedPapers(): boolean {
        //return true;
        //console.log("HasSelectedPapers?", this._MAGList.Papers[0].isSelected, this._MAGList.Papers[1].isSelected);
        if (this._MAGList.Papers.findIndex(found => found.isSelected == true) > -1) return true;
        else return false;
    }
    public get SelectedPapers(): MagPaper[] {
        return this._MAGList.Papers.filter(found => found.isSelected == true);
    }


	DeleteSelectedPapers(paperIds: MagPaper[]) {
		
        this._BusyMethods.push("DeleteSelectedPapers");
        let Ids = paperIds.map(x => x.paperId);
        //console.log("IDs:", Ids);
		//var strpaperIds = paperIds.map(x => x.paperId).toString();

		//let body = JSON.stringify({ paperIds: strpaperIds });

		this._httpC.post<any>(this._baseUrl + 'api/MAGList/DeleteSelectedPapers',
            Ids)
			.subscribe(
			list => {

					//var paperIdStr = list.toString().split(",");
					//var wholListpaperIdStr = this.MAGList.Papers.map(x => x.paperId);
					//for (var i = 0; i < paperIdStr.length; i++) {
					//	var id = Number(paperIdStr[i]);
					//	var ind = wholListpaperIdStr.indexOf(id);
					//	this.MAGList.Papers.slice(ind, 1);
					//}
					//this._Criteria.totalPapers = this.MAGList.totalPaperCount;
					//this.SavePapers(this.MAGList, this._Criteria);
					//this.ListChanged.emit();
					this.Refresh();
					//this.FetchWithCrit(this._Criteria, "StandardMAGList");
				

				}, error => {
					this.ModalService.GenericError(error);
					this.RemoveBusy("DeleteSelectedPapers");
				}
				, () => { this.RemoveBusy("DeleteSelectedPapers"); }
			);
	}


  

}

export class MAGListOptions {

	public showId: boolean = true;
	public showImportedId: boolean = false;
	public showShortTitle: boolean = true;
	public showTitle: boolean = true;
	public showYear: boolean = true;
	public showAuthors: boolean = false;
	public showJournal: boolean = false;
	public showDocType: boolean = false;
	public showInfo: boolean = false;
	public showScore: boolean = false;

}


export class MAGList {
    pagesize: number = 0;
    pageindex: number = 1;
    pagecount: number = 0;
    totalPaperCount: number = 0;
    Papers: MagPaper[] = [];
}

export class Criteria {
    onlyIncluded: boolean = true;
    showDeleted: boolean = false;
    sourceId: number = 0;
    searchId: number = 0;
    xAxisSetId: number = 0;
    xAxisAttributeId: number = 0;
    yAxisSetId: number = 0;
    yAxisAttributeId: number = 0;
    filterSetId: number = 0;
    filterAttributeId: number = 0;
    attributeSetIdList: string = "";
	listType: string = "";
	attributeid: number = 0;

    pageNumber: number = 0;
    pageSize: number = 100;
    totalPapers: number = 0;
    startPage: number= 0;
    endPage: number = 0;
    startIndex: number = 0;
    endIndex: number = 0;
    magSimulationId: number = 0;
    workAllocationId: number = 0;
    comparisonId: number = 0;
    description: string = "";
    contactId: number = 0;
    setId: number = 0;
    showInfoColumn: boolean = true;
    showScoreColumn: boolean = true;
}

export class KeyValue {//used in more than one place...
    constructor(k: string, v: string) {
        this.key = k;
        this.value = v;
    }
    key: string;
    value: string;
}

