import { Component, Inject, Injectable, EventEmitter, Output, Input } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { ModalService } from './modal.service';
import { Item } from './ItemList.service';
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

	public get Comparisons(): Comparison[] {

		return this._Comparisons;
	}

	public set Comparisons(comparisons: Comparison[]) {
		this._Comparisons = comparisons;
    }
 
    
    public FetchAll() {

		this._httpC.get<Comparison[]>(this._baseUrl + 'api/Comparisons/ComparisonList')
			.subscribe(result => {

				this._Comparisons = result;
				this.ListLoaded.emit();

        }, error => { this.modalService.SendBackHomeWithError(error); }
        );

	}


	public CreateComparison(comparison: Comparison) {

		this._BusyMethods.push("CreateComparison");
		alert(comparison.contactName1);
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