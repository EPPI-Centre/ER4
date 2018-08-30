/// <reference path="../services/itemlist.service.ts" />
import { Component, Inject, OnInit, EventEmitter, Output, AfterContentInit, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocationContactListService, WorkAllocation } from '../services/WorkAllocationContactList.service';
import { ItemListService } from '../services/ItemList.service'

@Component({
    selector: 'WorkAllocationContactListComp',
    templateUrl: './workAllocationContactListComp.component.html',
    styles: ['.UsedWorkAllocation { font-weight: bold; background-color: lightblue;}'],
    providers: []
})
export class WorkAllocationContactListComp implements OnInit, AfterContentInit, OnDestroy {
    constructor(
    private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        public _workAllocationContactListService: WorkAllocationContactListService,
        private ItemListService: ItemListService
    ) { }

    onSubmit(f: string) {
    }

    @Output() criteriaChange = new EventEmitter();
    //@Output() dataChange = new EventEmitter();
    private subWorkAllocationsLoaded: Subscription | null = null;
    public ListSubType: string = "GetItemWorkAllocationList";

    public get clickedIndex(): string {
        if (this.ItemListService && this.ItemListService.ListCriteria && this.ItemListService.ListCriteria.workAllocationId != 0) {

            if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationListRemaining") {
                //console.log('waStarted-' + this.ItemListService.ListCriteria.workAllocationId);
                return 'waRemaining-' + this.ItemListService.ListCriteria.workAllocationId;
            }
            else if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationListStarted") {
                //console.log('waStarted-' + this.ItemListService.ListCriteria.workAllocationId);
                return 'waStarted-' + this.ItemListService.ListCriteria.workAllocationId;
            }
            else if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationList") {
                //console.log('waAll-' + this.ItemListService.ListCriteria.workAllocationId);
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
        if (!this._workAllocationContactListService.workAllocations) return;
        if (this.ItemListService && this.ItemListService.ListCriteria && this.ItemListService.ListCriteria.workAllocationId != 0) {
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
        for (let workAll of this._workAllocationContactListService.workAllocations) {
            if (workAll.totalRemaining > 0) {

                this.ListSubType = "GetItemWorkAllocationListRemaining";
                this.criteriaChange.emit(workAll);
                return;
            }
        }
        for (let workAll of this._workAllocationContactListService.workAllocations) {
            if (workAll.totalAllocation > 0) {
                this.ListSubType = "GetItemWorkAllocationList";
                this.criteriaChange.emit(workAll);
                return;
            }
        }
    }

    getWorkAllocationContactList() {

        this._workAllocationContactListService.Fetch();

    }
      
    LoadGivenList(workAllocationId: number, subtype: string) {
        for (let workAll of this._workAllocationContactListService.workAllocations) {
            if (workAll.workAllocationId == workAllocationId) {
                this.ListSubType = subtype;
                this.criteriaChange.emit(workAll);
                return;
            }
        }
    }

    Clear() {
        this._workAllocationContactListService.workAllocations = [];
        this._workAllocationContactListService.Save();
    }

    log(blah: string) {
        console.log(blah)
    }

    ngOnInit() {
        console.log('initWorkAlloc');
        if (this.ItemListService && this.ItemListService.ListCriteria && this.ItemListService.ListCriteria.workAllocationId != 0) {
            
            if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationListRemaining") {
                console.log('got inside1');
                this.setClickedIndex('waRemaining-' + this.ItemListService.ListCriteria.workAllocationId);
            }
            else if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationListStarted") {
                this.setClickedIndex('waStarted-' + this.ItemListService.ListCriteria.workAllocationId);
                console.log('got inside2');
            }
            else if (this.ItemListService.ListCriteria.listType == "GetItemWorkAllocationList") {
                this.setClickedIndex('waAll-' + this.ItemListService.ListCriteria.workAllocationId);
                console.log('got inside3');
            }
        }
        
    }
    ngAfterContentInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {

            this.subWorkAllocationsLoaded = this._workAllocationContactListService.ListLoaded.subscribe(
                () => this.LoadDefaultItemList()
            );
            this.getWorkAllocationContactList();
        }
    }
    ngOnDestroy() {
        console.log('killing work alloc comp');
        if (this.subWorkAllocationsLoaded) this.subWorkAllocationsLoaded;
        //if (this.subCodingCheckBoxClickedEvent) this.subCodingCheckBoxClickedEvent.unsubscribe();
    }
}





