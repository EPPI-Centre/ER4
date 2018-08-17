import { Component, Inject, OnInit, EventEmitter, Output, AfterContentInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';
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
export class WorkAllocationContactListComp implements OnInit, AfterContentInit {
    constructor(
    private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        private _workAllocationContactListService: WorkAllocationContactListService,
        private ItemListService: ItemListService
    ) { }

    onSubmit(f: string) {
    }

    @Output() criteriaChange = new EventEmitter();
    public ListSubType: string = "";

    public clickedIndex: string = 'waRemaining-0';
    public allocTotal: number = 0;
    public allocStarted: number = 0;
    public allocRem: number = 0;

    setClickedIndex(i: string) {
        this.clickedIndex = i;
        this._workAllocationContactListService.clickedIndex = i;
    }

    getWorkAllocationContactList() {
        this._workAllocationContactListService.Fetch().subscribe(result => {
            this._workAllocationContactListService.workAllocations = result;
            console.log("got " + this._workAllocationContactListService.workAllocations.length + " Work Allocs.");
            for (let workAll of this._workAllocationContactListService.workAllocations) {
                console.log("WA_Id: " + workAll.workAllocationId);
            }
            this.LoadDefaultItemList();
        });
    }

    LoadDefaultItemList() {
        if (!this._workAllocationContactListService.workAllocations) return;
        for (let workAll of this._workAllocationContactListService.workAllocations) {
            if (workAll.totalRemaining > 0) {
                console.log(this.clickedIndex);
                console.log("emitting: " + workAll.attributeId);
                console.log("WA_Id: " + workAll.workAllocationId);
                this.ListSubType = "GetItemWorkAllocationListRemaining";
                this.criteriaChange.emit(workAll);
                return;
            }
        }
        for (let workAll of this._workAllocationContactListService.workAllocations) {
            if (workAll.totalAllocation > 0) {
                console.log("emitting: " + workAll.attributeId);
                console.log(this.clickedIndex);
                console.log("blah the blah: " + workAll.workAllocationId);
                this.ListSubType = "GetItemWorkAllocationList";
                this.criteriaChange.emit(workAll);

                return;
            }
        }
    }
    LoadGivenList(workAllocationId: number, subtype: string) {
        for (let workAll of this._workAllocationContactListService.workAllocations) {
            if (workAll.workAllocationId == workAllocationId) {
                console.log(this.clickedIndex);
                console.log("emitting: " + workAll.attributeId);
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

    }
    ngAfterContentInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            console.log("getting WorkAllocs");
            //this.allItems = data;
            this.getWorkAllocationContactList();
        }
    }
}





