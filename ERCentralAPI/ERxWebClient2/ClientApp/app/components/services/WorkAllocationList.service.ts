import {  Inject, Injectable, EventEmitter, Output, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { EventEmitterService } from './EventEmitter.service';
import { Subscription } from 'rxjs';


@Injectable({
    providedIn: 'root',
})

export class WorkAllocationListService extends BusyAwareService implements OnDestroy {
    @Output() ListLoaded = new EventEmitter();
    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        private EventEmitterService: EventEmitterService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
        console.log("On create WorkAllocationListService");
        this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
    }
    ngOnDestroy() {
        console.log("Destroy search service");
        if (this.clearSub != null) this.clearSub.unsubscribe();
    }
    private clearSub: Subscription | null = null;
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

	public AssignWorkAllocation(wa: WorkAllocation): Promise<any> {

		this._BusyMethods.push("AssignWorkAllocation");
		
		return this._httpC.post<WorkAllocation>(this._baseUrl +
			'api/WorkAllocationContactList/AssignWorkAllocation', wa)
			.toPromise().then(

				() => {

					this.FetchAll();
					this.RemoveBusy("AssignWorkAllocation");

			},
				error => {
					this.modalService.GenericError(error);
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

    public RunAllocationFromWizardCommand(cmd: WorkAllocationFromWizardCommand): Promise<WorkAllocWizardResult> {
        this._BusyMethods.push("RunAllocationFromWizardCommand");
        return this._httpC.post<WorkAllocWizardResult>(this._baseUrl +
            'api/WorkAllocationContactList/ExecuteWorkAllocationFromWizardCommand', cmd)
            .toPromise().then((result) => {
                this.RemoveBusy("RunAllocationFromWizardCommand");
                if (cmd.isPreview == 1) {
                    //we're only getting how many items...
                    cmd.numberOfItemsToAssign = result.numberOfAffectedItems;
                }
                return result;
            }, (error) => {
                this.modalService.GenericError(error);
                this.RemoveBusy("RunAllocationFromWizardCommand");
                let res: WorkAllocWizardResult = {
                    preview: null,
                    numberOfAffectedItems: -1,
                    isSuccess: false
                };
                return res;
            })
            .catch((err) => {
                this.modalService.GenericError(err);
                this.RemoveBusy("RunAllocationFromWizardCommand");
                let res: WorkAllocWizardResult = {
                    preview: null,
                    numberOfAffectedItems: -1,
                    isSuccess: false
                }
                return res;
            });

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
export class WorkAllocationFromWizardCommand {
    //partially based on PerformRandomAllocateCommandJSON 
    filterType: string = "";
    attributeIdFilter: number = 0;
    setIdFilter: number = 0;
    destination_Attribute_ID: number = 0;
    destination_Set_ID: number = 0;
    percentageOfWholePot: number = 100;
    included: boolean = true;

    isPreview:number = 1;
    work_to_do_setID: number = 0;; //"assign codes from this set"
    oneGroupPerPerson: boolean = false;
    peoplePerItem: number = 1;
    reviewersIds: string = "";
    reviewerNames: string = "";
    itemsPerEachReviewer: string = "";
    groupsPrefix: string = "";
    numberOfItemsToAssign: number = -1;
}
export interface WorkAllocWizardResult {
    preview: any;
    numberOfAffectedItems: number;
    isSuccess: boolean;
}