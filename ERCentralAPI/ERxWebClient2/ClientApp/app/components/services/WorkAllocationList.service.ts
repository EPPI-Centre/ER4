import {  Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';


@Injectable({
    providedIn: 'root',
})

export class WorkAllocationListService extends BusyAwareService {
    @Output() ListLoaded = new EventEmitter();
    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
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

		this._BusyMethods.push("FetchWorkAllocation");
		this._httpC.get<WorkAllocation[]>(this._baseUrl + 'api/WorkAllocationContactList/WorkAllocationContactList')
			.subscribe(result => {

			this._ContactWorkAllocations = result;
				this.ListLoaded.emit();
				this.RemoveBusy("FetchWorkAllocation");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchWorkAllocation");
			}
        );

	}

	public FetchAll() {

		this._BusyMethods.push("FetchAllWorkAllocation");
		this._httpC.get<WorkAllocation[]>(this._baseUrl + 'api/WorkAllocationContactList/WorkAllocations')
			.subscribe(result => {

			this._allWorkAllocationsForReview = result;
				this.RemoveBusy("FetchAllWorkAllocation");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchAllWorkAllocation");
			}
		);

	}

	public AssignWorkAllocation(wa: WorkAllocation) {

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

		this._BusyMethods.push("DeleteWorkAllocation");
		let body = JSON.stringify({ Value: workAllocationId });
		this._httpC.post<WorkAllocation>(this._baseUrl +
			'api/WorkAllocationContactList/DeleteWorkAllocation', body)
			.subscribe(() => {

				this.FetchAll();
				this.RemoveBusy("DeleteWorkAllocation");
			},
			error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("DeleteWorkAllocation");
			}
		 );

	}

	public Clear() {
		this._allWorkAllocationsForReview = [];
		this._ContactWorkAllocations = [];
		this.CurrentlyLoadedWorkAllocationSublist = new WorkAllocationSublist();
	}

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
