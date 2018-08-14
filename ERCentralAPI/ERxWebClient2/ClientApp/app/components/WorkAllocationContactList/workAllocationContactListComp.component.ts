import { Component, Inject, OnInit, EventEmitter, Output } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocationContactListService, WorkAllocation } from '../services/WorkAllocationContactList.service';



@Component({
    selector: 'WorkAllocationContactListComp',
    templateUrl: './workAllocationContactListComp.component.html',
    //providers: [ReviewerIdentityService]
    providers: []
})
export class WorkAllocationContactListComp implements OnInit {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        private _workAllocationContactListService: WorkAllocationContactListService
    ) { }

    onSubmit(f: string) {
    }
    @Output() criteriaChange = new EventEmitter();
    public ListSubType: string = "";

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
                console.log("emitting: " + workAll.attributeId);
                // Sergio I do not know where this comes from!
                this.ListSubType = subtype;
                //this.ListSubType = "GetItemWorkAllocationList";
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

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            console.log("getting WorkAllocs");
            this.getWorkAllocationContactList();
        }
    }
}





