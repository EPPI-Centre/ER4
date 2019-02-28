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

export class WorkAllocationListService extends BusyAwareService {
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

	private _ContactWorkAllocations: WorkAllocation[] = [];
	private _allWorkAllocationsForReview: WorkAllocation[] = [];
    public CurrentlyLoadedWorkAllocationSublist: WorkAllocationSublist = new WorkAllocationSublist();

    public get ContactWorkAllocations(): WorkAllocation[] {
        
		return this._ContactWorkAllocations;

	}

	public get AllWorkAllocationsForReview(): WorkAllocation[] {

		return this._allWorkAllocationsForReview;
	}

	public set ContactWorkAllocations(wa: WorkAllocation[]) {
		this._ContactWorkAllocations = wa;
    }
 
    
    public Fetch() {

		this._httpC.get<WorkAllocation[]>(this._baseUrl + 'api/WorkAllocationContactList/WorkAllocationContactList')
			.subscribe(result => {

			this._ContactWorkAllocations = result;
            this.ListLoaded.emit();
        }, error => { this.modalService.SendBackHomeWithError(error); }
        );

	}

	public FetchAll() {

		this._httpC.get<WorkAllocation[]>(this._baseUrl + 'api/WorkAllocationContactList/WorkAllocations')
			.subscribe(result => {

			this._allWorkAllocationsForReview = result;

		}, error => { this.modalService.SendBackHomeWithError(error); }
		);

	}

	public AssignWorkAllocation(wa: WorkAllocation) {

		// is there a need for busy methods here I would say yes...
		this._BusyMethods.push("AssignWorkAllocation");

		this._httpC.post<WorkAllocation>(this._baseUrl +
			'api/WorkAllocationContactList/AssignWorkAllocation', wa)
			.subscribe(() => {

				this.FetchAll();
				this.RemoveBusy("AssignWorkAllocation");

			},
				error => {
					this.modalService.GenericError(error);
					this.RemoveBusy("AssignWorkAllocation");
				}
				, () => {
					this.RemoveBusy("AssignWorkAllocation");
				}
			);


	}

	

	public DeleteWorkAllocation(workAllocationId: number) {

		let body = JSON.stringify({ Value: workAllocationId });
		this._httpC.post<WorkAllocation>(this._baseUrl + 'api/WorkAllocationContactList/DeleteWorkAllocation', body)
			.subscribe(() => {

				this.FetchAll();
			},
			error => { this.modalService.SendBackHomeWithError(error); }
		 );

	}

	public Clear() {
		this._allWorkAllocationsForReview = [];
		this._ContactWorkAllocations = [];
		this.CurrentlyLoadedWorkAllocationSublist = new WorkAllocationSublist();
	}

	//public FetchAll(itemId: number) {

	//	let itemID: number = itemId;
	//	alert('This is what we have: ' + itemID);
	//	let body = JSON.stringify({ Value: itemID });
	//	this._httpC.post<WorkAllocation[]>(this._baseUrl + 'api/WorkAllocationContactList/WorkAllocationsAll', body).subscribe(result => {

	//		this.AllWorkAllocationsForReview = result;//also saves to local storage
	//		this.ListLoaded.emit();
	//	}, error => { this.modalService.SendBackHomeWithError(error); }
	//	);

	//}

}

export class WorkAllocation {
    workAllocationId: number = 0;
    contactName: string = "";
    contactId: string = "";
    setName: string = "";
    setId: number = 0;
    attributeName: string = "";
    attributeId: number = 0;
    totalAllocation: number = 0;
    totalStarted: number = 0;
    totalRemaining: number = 0;
}
export class WorkAllocationSublist {
    workAllocationId: number = 0;
    listSubtype: string = "";
}
