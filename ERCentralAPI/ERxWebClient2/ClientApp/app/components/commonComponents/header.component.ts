import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { timer, Subject, Subscription, Observable } from 'rxjs';
import { take, map, takeUntil } from 'rxjs/operators';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';


@Component({
    selector: 'HeaderComponent',
    templateUrl: './header.component.html',
    providers: []
})

export class HeaderComponent implements OnInit {

   

    constructor(private router: Router,
                @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService,
        public ReviewInfoService: ReviewInfoService,
        private ReviewSetsService: ReviewSetsService,
        private ItemListService: ItemListService,
        private workAllocationContactListService: WorkAllocationContactListService
    ) {    }

    
    ngOnDestroy() {
    }
    Clear() {
        this.ItemListService.SaveItems(new ItemList(), new Criteria());
        this.ReviewSetsService.Clear();
        this.workAllocationContactListService.workAllocations = [];
        //this.workAllocationContactListService.Save();
    }
    Logout() {
        this.Clear();
        this.router.navigate(['home']);
    }


    ngOnInit() {
    }
}
