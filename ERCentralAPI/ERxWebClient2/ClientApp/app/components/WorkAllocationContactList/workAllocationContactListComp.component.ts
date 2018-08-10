import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';


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

    getWorkAllocationContactList() {
        this._workAllocationContactListService.Fetch().subscribe(result => {
            this._workAllocationContactListService.workAllocations = result;
            console.log("got " + this._workAllocationContactListService.workAllocations.length + " Work Allocs.");
        });
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





