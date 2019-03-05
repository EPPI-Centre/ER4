import { Component, Inject, OnInit, EventEmitter, Output, AfterContentInit, OnDestroy, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subscription, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocationListService, WorkAllocation } from '../services/WorkAllocationList.service';
import { ItemListService } from '../services/ItemList.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';

@Component({
	selector: 'WorkAllocationContactListComp',
	templateUrl: './WorkAllocationContactListComp.component.html',
    styles: ['.UsedWorkAllocation { font-weight: bold; background-color: lightblue;}'],
    providers: []
})

export class WorkAllocationContactListComp implements OnInit, AfterContentInit, OnDestroy {
    constructor(
    private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
		public _workAllocationListService: WorkAllocationListService,
        public reviewInfoService: ReviewInfoService,
        private ItemListService: ItemListService,
        private PriorityScreeningService: PriorityScreeningService
    ) { }

    ngOnInit() {
        if (this.ItemListService && this.ItemListService.ListCriteria && this.ItemListService.ListCriteria.workAllocationId != 0) {

            if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationListRemaining") {

                this.setClickedIndex('waRemaining-' + this.ItemListService.ListCriteria.workAllocationId);
            }
            else if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationListStarted") {
                this.setClickedIndex('waStarted-' + this.ItemListService.ListCriteria.workAllocationId);

            }
            else if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationList") {
                this.setClickedIndex('waAll-' + this.ItemListService.ListCriteria.workAllocationId);
            }
        }
    }
    ngAfterContentInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {

			this.subWorkAllocationsLoaded = this._workAllocationListService.ListLoaded.subscribe(
                () => this.LoadDefaultItemList()
            );
            this.getWorkAllocationContactList();
        }
    }

    @Output() criteriaChange = new EventEmitter();
    @Output() AllocationClicked = new EventEmitter();
    private subWorkAllocationsLoaded: Subscription | null = null;
    @Input() Context: string | undefined;
    public ListSubType: string = "GetItemWorkAllocationList";

    public get clickedIndex(): string {
        if (this.ItemListService && this.ItemListService.ListCriteria && this.ItemListService.ListCriteria.workAllocationId != 0) {

            if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationListRemaining") {
                return 'waRemaining-' + this.ItemListService.ListCriteria.workAllocationId;
            }
            else if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationListStarted") {
                return 'waStarted-' + this.ItemListService.ListCriteria.workAllocationId;
            }
            else if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationList") {
                return 'waAll-' + this.ItemListService.ListCriteria.workAllocationId;
            }
        }
        return "";
    };
    public allocTotal: number = 0;
    public allocStarted: number = 0;
    public allocRem: number = 0;

    setClickedIndex(i: string) {
        //this.clickedIndex = i;
        //this._workAllocationContactListService.clickedIndex = i;
    }
    //will pick the ItemList to load. If one was 
    LoadDefaultItemList() {
        console.log("load def item list " + this.JustCheckInstance);
        console.log(this.ItemListService.ListCriteria.workAllocationId + " | " + this.ItemListService.ListCriteria.listType);
		if (!this._workAllocationListService.ContactWorkAllocations) return;
        
        if (this.ItemListService
            && this.ItemListService.ListCriteria
            && this.ItemListService.ListCriteria.workAllocationId != 0
            && this.ItemListService.ListCriteria.listType.startsWith('GetItemWorkAllocation')
        ) {
            //we want to reload this list, no matter what it is...
            // waStarted-921 :GetItemWorkAllocationListStarted
            //waRemaining-921:GetItemWorkAllocationListRemaining
            //waAll-921: GetItemWorkAllocationList
            //if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationListRemaining") {
            //    this.setClickedIndex('waRemaining-' + this.ItemListService.ListCriteria.workAllocationId);
            //}
            //else if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationListStarted") {
            //    this.setClickedIndex('waStarted-' + this.ItemListService.ListCriteria.workAllocationId);
            //}
            //else if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationList") {
            //    this.setClickedIndex('waAll-' + this.ItemListService.ListCriteria.workAllocationId);
            //    }
            this.ItemListService.Refresh();
            return;
        }
        if (this.ItemListService
            && this.ItemListService.ListCriteria
            && !this.ItemListService.ListCriteria.listType.startsWith('GetItemWorkAllocation')
            && this.Context !== "CodingOnly"
        ) {

            return;
        }//current list is not a work allocation: don't reload it (applies to main interface)

        //see last condition  [&& this.Context !== "CodingOnly] if there is no list and we ARE in coding only,
        //we'll get one...
		for (let workAll of this._workAllocationListService.ContactWorkAllocations) {
            if (workAll.totalRemaining > 0) {

                this.ListSubType = "GetItemWorkAllocationListRemaining";
                this.criteriaChange.emit(workAll);
                return;
            }
        }
		for (let workAll of this._workAllocationListService.ContactWorkAllocations) {
            if (workAll.totalAllocation > 0) {
                this.ListSubType = "GetItemWorkAllocationList";
                this.criteriaChange.emit(workAll);
                return;
            }
        }
    }

    getWorkAllocationContactList() {
        if (!this.reviewInfoService.ReviewInfo || this.reviewInfoService.ReviewInfo.reviewId < 1) {
            //we have reloaded the whole app and need to get the missing info
            //this happens here because both coding only and main UI will call this method on reload and similar conditions.
            this.reviewInfoService.Fetch();
        }
		this._workAllocationListService.Fetch();

    }
      
    LoadGivenList(workAllocationId: number, subtype: string) {
		for (let workAll of this._workAllocationListService.ContactWorkAllocations) {
            if (workAll.workAllocationId == workAllocationId) {
                this.ListSubType = subtype;
                this.criteriaChange.emit(workAll);
                this.AllocationClicked.emit();
                return;
            }
        }
    }

    Clear() {
		this._workAllocationListService.Clear();
        //this._workAllocationContactListService.Save();
    }

    log(blah: string) {

    }
    JustCheckInstance: number = Math.random();

    HasScreeningList(): boolean {
        if (this.reviewInfoService
            && this.reviewInfoService.ReviewInfo
            && this.reviewInfoService.ReviewInfo.screeningCodeSetId != 0
            && this.reviewInfoService.ReviewInfo.screeningListIsGood
            && this.ReviewerIdentityServ.HasWriteRights)
            return true;
        else return false;
    }
    
    private subGotPriorityScreeningData: Subscription | null = null;
    StartScreening() {
        //alert('Start Screening: not implemented');
        this.ItemListService.IsInScreeningMode = true;
        this.subGotPriorityScreeningData = this.PriorityScreeningService.gotList.subscribe(this.ContinueStartScreening());
        this.PriorityScreeningService.Fetch();

    }
    ContinueStartScreening() {
        if (this.subGotPriorityScreeningData) this.subGotPriorityScreeningData.unsubscribe();
        if (this.Context == 'FullUI') this.router.navigate(['itemcoding', 'PriorityScreening']);
        else if (this.Context == 'CodingOnly') this.router.navigate(['itemcodingOnly', 'PriorityScreening']);
    }
    ngOnDestroy() {
        if (this.subWorkAllocationsLoaded) this.subWorkAllocationsLoaded.unsubscribe();
        //if (this.subCodingCheckBoxClickedEvent) this.subCodingCheckBoxClickedEvent.unsubscribe();
    }
}





