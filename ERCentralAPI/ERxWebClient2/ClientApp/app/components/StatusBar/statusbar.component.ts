import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationContactList.service'
import { Criteria, ItemList } from '../services/ItemList.service'
import { WorkAllocationContactListComp } from '../WorkAllocationContactList/workAllocationContactListComp.component';
import { ItemListService } from '../services/ItemList.service'
import { ItemListComp } from '../ItemList/itemListComp.component';
import { FetchReadOnlyReviewsComponent } from '../readonlyreviews/readonlyreviews.component';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { timer, Subject } from 'rxjs';
import { take, map, takeUntil } from 'rxjs/operators';

@Component({
    selector: 'statusbar',
    templateUrl: './statusbar.component.html',
    providers: []
})

export class StatusBarComponent implements OnInit {

    constructor(private router: Router,
                private _httpC: HttpClient,
                @Inject('BASE_URL') private _baseUrl: string,
                private ReviewerIdentityServ: ReviewerIdentityService,
                private ReviewInfoService: ReviewInfoService,) {

    }

    private killTrigger: Subject<void> = new Subject();
    public countDown: any | undefined;
    public count: number = 60;

    timerServerCheck(u: string, g: string) {
        console.log(u + '+' + g);
        this.countDown = timer(0, 8000).pipe(
            takeUntil(this.killTrigger),
            map(() => {
                console.log('+');
                this.ReviewerIdentityServ.LogonTicketCheckExpiration(u, g);
            })
        );
        this.countDown.subscribe(console.log('AHA!'));
    }

    getDaysLeftAccount() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftAccount;
    }
    getDaysLeftReview() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftReview;
    }

    ngOnDestroy() {

        if (this.countDown) this.killTrigger.next();
    }

    ngOnInit() {

        this.ReviewInfoService.Fetch();
        let guid = this.ReviewerIdentityServ.reviewerIdentity.ticket;
        let uu = String(this.ReviewerIdentityServ.reviewerIdentity.userId);
        console.log('init main');
        if (guid != undefined && uu != '') {
            console.log('init main: timer');
            this.timerServerCheck(uu, guid);
        }
    }


}
