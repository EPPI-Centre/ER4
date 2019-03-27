import {  Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewSetsService } from './ReviewSets.service';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root',
})

export class ComparisonsService extends BusyAwareService {
    private sub: any;
    @Output() ListLoaded = new EventEmitter();
    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
		private _reviewSetsService: ReviewSetsService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
	
	private _Comparisons: Comparison[] = [];
	public currentComparison: Comparison = new Comparison();

	public get Comparisons(): Comparison[] {
		if (this._Comparisons) {
			return this._Comparisons;
		} else {
			return [];
		}
	}

	public set Comparisons(comparisons: Comparison[]) {
		if (comparisons) {
			this._Comparisons = comparisons;
		}
    }

	private _Statistics: ComparisonStatistics | null = null;

    public get Statistics(): ComparisonStatistics | null {
		return this._Statistics;		
	}
	//public calculateStats() {

	//	let stats: ComparisonStatistics = this._Statistics;
	//	//console.log(stats.canComplete1vs2 + 'true');
	//	this.Agreements1 = stats.RawStats.n1vs2 - stats.disagreements1vs2;
	//	this.Agreements2 = stats.n1vs3 - stats.disagreements1vs3;
	//	this.Agreements3 = stats.n2vs3 - stats.disagreements2vs3;
	//	//alert('asdf ' + test.n2vs3);

	//}
	

    
    public FetchAll() {
        this._BusyMethods.push("FetchAll");
		this._httpC.get<Comparison[]>(this._baseUrl + 'api/Comparisons/ComparisonList')
			.subscribe(result => {
				this._Comparisons = result;
				//this.ListLoaded.emit();
                this.RemoveBusy("FetchAll");
            }, error => {
                this.RemoveBusy("FetchAll");
                this.modalService.SendBackHomeWithError(error);
            }
        );
	}

	public  FetchStats(ComparisonId: number ) {
        this._BusyMethods.push("FetchStats");
		let body = JSON.stringify({ Value: ComparisonId });
		 this._httpC.post<iComparisonStatistics>(this._baseUrl + 'api/Comparisons/ComparisonStats', body)
			.subscribe(result => {
                this._Statistics = new ComparisonStatistics(result, ComparisonId);
				this.currentComparison = this.Comparisons.filter(x => x.comparisonId == ComparisonId)[0];//consider a get
				console.log(this._Statistics);
				//this.calculateStats();
                this.RemoveBusy("FetchStats");
             }, error => {
                 this.RemoveBusy("FetchStats");
                 this.modalService.SendBackHomeWithError(error);
             }
             );
	}

	public CreateComparison(comparison: Comparison) {

		this._BusyMethods.push("CreateComparison");
		//console.log('inside the service now' + JSON.stringify(comparison));
		this._httpC.post<Comparison>(this._baseUrl +
			'api/Comparisons/CreateComparison', comparison)
			.subscribe(() => {

				this.FetchAll();
				this.RemoveBusy("CreateComparison");

			},
				error => {
					this.modalService.GenericError(error);
					this.RemoveBusy("CreateComparison");
				}
				, () => {
					this.RemoveBusy("CreateComparison");
				}
			);
	}
	
	public DeleteComparison(ComparisonId: number) {
        this._BusyMethods.push("DeleteComparison");
		let body = JSON.stringify({ Value: ComparisonId });

		this._httpC.post<Comparison>(this._baseUrl + 'api/Comparisons/DeleteComparison', body)
			.subscribe(() => {
                this.RemoveBusy("DeleteComparison");
				this.FetchAll();
			},
            error => {
                this.RemoveBusy("DeleteComparison");
                this.modalService.SendBackHomeWithError(error);
            }
		 );
	}

    public Clear() {
        //clear current stats details AS Well!
		this._Comparisons = [];
	}

}

export class Comparison {

	public comparisonId: number = 0;
	public isScreening: boolean = false;
	public reviewId: number = 0;
	public inGroupAttributeId: number = 0;
	public setId: number = 0;
	public comparisonDate: string = "";
	public contactId1: number = 0;
	public contactId2: number = 0;
	public contactId3: number = 0;
	public contactName1: string = '';
	public contactName2: string = '';
	public contactName3: string = '';
	public attributeName: string = '';
	public setName: string = '';

}

export class ComparisonStatistics {
    public constructor(data: iComparisonStatistics, comparisonID: number) {
        this.RawStats = data;
        this.comparisonID = comparisonID;
    }
    public RawStats: iComparisonStatistics;
    public comparisonID: number;
	//public comparisonId: number = 0;
	//public n1vs2: number = 0;
	//public n2vs3: number = 0;
	//public n1vs3: number = 0;
	//public disagreements1vs2: number = 0;
	//public disagreements2vs3: number = 0;
 //   public disagreements1vs3: number = 0;

	//public ncoded1: number = 0;
	//public ncoded2: number = 0;
	//public ncoded3: number = 0;
	//public canComplete1vs2: boolean = false;
	//public canComplete1vs3: boolean = false;
	//public canComplete2vs3: boolean = false;
	//public scdisagreements1vs2: number = 0;
	//public scdisagreements2vs3: number = 0;
	//public scdisagreements1vs3: number = 0;
    //public isScreening: boolean = false;
    
    public get Agreements1(): number {
        return this.RawStats.n1vs2 - this.RawStats.disagreements1vs2;
    };
    public get Agreements2(): number {
        return this.RawStats.n1vs3 - this.RawStats.disagreements1vs3;
    };
    public get Agreements3(): number {
        return this.RawStats.n2vs3 - this.RawStats.disagreements2vs3;
    };
}
export interface iComparisonStatistics {
    comparisonId: number;
    n1vs2: number;
    n2vs3: number;
    n1vs3: number;
    disagreements1vs2: number;
    disagreements2vs3: number;
    disagreements1vs3: number;
    ncoded1: number;
    ncoded2: number;
    ncoded3: number;
    canComplete1vs2: boolean;
    canComplete1vs3: boolean;
    canComplete2vs3: boolean;
    scdisagreements1vs2: number;
    scdisagreements2vs3: number;
    scdisagreements1vs3: number;
    isScreening: boolean;
}