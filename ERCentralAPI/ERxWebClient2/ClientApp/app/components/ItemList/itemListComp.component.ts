import { Component, Inject, OnInit, EventEmitter, Output } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
//import { DataSource } from '@angular/cdk/collections';

@Component({
    selector: 'ItemListComp',
    templateUrl: './ItemListComp.component.html',
    //providers: [ReviewerIdentityService]
    providers: []
})
export class ItemListComp implements OnInit {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService, private ItemListService: ItemListService
    ) { }

    onSubmit(f: string) {
    }
    //@Output() criteriaChange = new EventEmitter();
    //public ListSubType: string = "";

    //dataSource = this.ItemListService.ItemList.items;
    //displayedColumns = ['Authors'];

    public LoadWorkAllocList(workAlloc: WorkAllocation, ListSubType: string) {
        let crit = new Criteria();
        crit.listType = ListSubType;
        crit.workAllocationId = workAlloc.workAllocationId;
        this.ItemListService.FetchWithCrit(crit)
            .subscribe(list => {
                console.log("Got ItemList, lenght = " + list.items.length);
                this.ItemListService.SaveItems(list, crit);
            })
    }

    LoadDefaultItemList() {
        
    }



    ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            
        }
    }
}





