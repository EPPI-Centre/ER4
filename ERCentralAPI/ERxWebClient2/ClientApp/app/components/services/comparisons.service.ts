import {  Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewSetsService } from './ReviewSets.service';

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
	public Agreements1: number = 0;
	public Agreements2: number = 0;
	public Agreements3: number = 0;
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

	private _Statistics: ComparisonStatistics = new ComparisonStatistics();

	public get Statistics(): ComparisonStatistics {
		if (this._Statistics) {
			return this._Statistics;
		} else {
			return new ComparisonStatistics();
		}
	}
	public calculateStats() {

		let stats: ComparisonStatistics = this._Statistics;
		//console.log(stats.canComplete1vs2 + 'true');
		this.Agreements1 = stats.n1vs2 - stats.disagreements1vs2;
		this.Agreements2 = stats.n1vs3 - stats.disagreements1vs3;
		this.Agreements3 = stats.n2vs3 - stats.disagreements2vs3;
		//alert('asdf ' + test.n2vs3);

	}
	public set Statistics(stats: ComparisonStatistics) {
		if (stats) {
			this._Statistics = stats;
		}
	}

    
    public FetchAll() {

		this._httpC.get<Comparison[]>(this._baseUrl + 'api/Comparisons/ComparisonList')
			.subscribe(result => {

				this._Comparisons = result;
				//this.ListLoaded.emit();

        }, error => { this.modalService.SendBackHomeWithError(error); }
        );
	}

	public  FetchStatsAsync(ComparisonId: number ) {

		let body = JSON.stringify({ Value: ComparisonId });
		 this._httpC.post<ComparisonStatistics>(this._baseUrl + 'api/Comparisons/ComparisonStats', body)
			.subscribe(result => {
				this._Statistics = result;
				this.currentComparison = this.Comparisons.filter(x => x.comparisonId == ComparisonId)[0];
				//console.log(this._Statistics);
				this.calculateStats();

			}, error => { this.modalService.SendBackHomeWithError(error); }
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

		let body = JSON.stringify({ Value: ComparisonId });

		this._httpC.post<Comparison>(this._baseUrl + 'api/Comparisons/DeleteComparison', body)
			.subscribe(() => {

				this.FetchAll();
			},
			error => { this.modalService.SendBackHomeWithError(error); }
		 );

	}

	public Clear() {
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

	public comparisonId: number = 0;
	public n1vs2: number = 0;
	public n2vs3: number = 0;
	public n1vs3: number = 0;
	public disagreements1vs2: number = 0;
	public disagreements2vs3: number = 0;
	public disagreements1vs3: number = 0;
	public ncoded1: number = 0;
	public ncoded2: number = 0;
	public ncoded3: number = 0;
	public canComplete1vs2: boolean = false;
	public canComplete1vs3: boolean = false;
	public canComplete2vs3: boolean = false;
	public scdisagreements1vs2: number = 0;
	public scdisagreements2vs3: number = 0;
	public scdisagreements1vs3: number = 0;
	public isScreening: boolean = false;

}