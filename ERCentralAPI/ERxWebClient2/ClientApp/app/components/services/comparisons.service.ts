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

	private _Statistics: ComparisonStatistics[] = [];

	public get Statistics(): ComparisonStatistics[] {
		if (this._Statistics) {
			return this._Statistics;
		} else {
			return [];
		}
	}

	public set Statistics(stats: ComparisonStatistics[]) {
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

	public FetchStats(ComparisonId: number ) {

		let body = JSON.stringify({ Value: ComparisonId });
		this._httpC.post<ComparisonStatistics[]>(this._baseUrl + 'api/Comparisons/ComparisonStats', body)
			.subscribe(result => {

				this._Statistics = result;
				this.currentComparison = this.Comparisons.filter(x => x.comparisonId == ComparisonId)[0];
				console.log(this._Statistics);

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

	comparisonId: number = 0;
	isScreening: boolean = false;
	reviewId: number = 0;
	inGroupAttributeId: number = 0;
	setId: number = 0;
	comparisonDate: string = "";
	contactId1: number = 0;
	contactId2: number = 0;
	contactId3: number = 0;
	contactName1: string = '';
	contactName2: string = '';
	contactName3: string = '';
	attributeName: string = '';
	setName: string = '';

}

export class ComparisonStatistics {

	comparisonId: number = 0;
	N1vs2: number = 0;
	N2vs3: number = 0;
	N1vs3: number = 0;
	disagreements1vs2: number = 0;
	disagreements2vs3: number = 0;
	disagreements1vs3: number = 0;
	Ncoded1: number = 0;
	Ncoded2: number = 0;
	Ncoded3: number = 0;
	CanComplete1vs2: number = 0;
	CanComplete1vs3: number = 0;
	CanComplete2vs3: number = 0;
	Scdisagreements1vs2: number = 0;
	Scdisagreements2vs3: number = 0;
	Scdisagreements1vs3: number = 0;
	isScreening: boolean = false;

}