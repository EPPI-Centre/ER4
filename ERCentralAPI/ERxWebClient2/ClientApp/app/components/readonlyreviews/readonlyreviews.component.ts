import { Component, Inject, OnInit, Output, EventEmitter, OnDestroy, ViewChild, ChangeDetectorRef } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subject, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { readonlyreviewsService, ReadOnlyReview } from '../services/readonlyreviews.service';
import { timer } from 'rxjs'; // (for rxjs < 6) use 'rxjs/observable/timer'
import { take, map } from 'rxjs/operators';
import { DataTableDirective, DataTablesModule } from 'angular-datatables';

@Component({
    selector: 'readonlyreviews',
    templateUrl: './readonlyreviews.component.html',
    providers: [],
    
})
export class FetchReadOnlyReviewsComponent implements OnInit, OnDestroy {

    constructor(private router: Router,
                private _httpC: HttpClient,
                @Inject('BASE_URL') private _baseUrl: string,
                private ReviewerIdentityServ: ReviewerIdentityService,
        public _readonlyreviewsService: readonlyreviewsService,
        private changeDetectorRef: ChangeDetectorRef) {

                //console.log('rOr constructor: ' + this.ReviewerIdentityServ.reviewerIdentity.userId);
    }

    //@ViewChild(DataTableDirective)
    //dtElement: DataTableDirective = new DataTableDirective();
    //dtOptions: DataTables.Settings = {};
    //dtTrigger: Subject<any> = new Subject();

    dtOptions: DataTables.Settings = {};
    reviews: ReadOnlyReview[] = [];
    // We use this trigger because fetching the list of persons can be quite long,
    // thus we ensure the data is fetched before rendering
    dtTrigger: Subject<any> = new Subject();

    FormatDate(DateSt: string): string {
        let date: Date = new Date(DateSt);
        return date.toLocaleDateString();
    }

    onSubmit(f: string) {
            let RevId: number = parseInt(f, 10);
        this.ReviewerIdentityServ.LoginToReview(RevId);

        
    }

    onFullSubmit(f: string) {
        let RevId: number = parseInt(f, 10);
        this.ReviewerIdentityServ.LoginToFullReview(RevId);
        
    }

    getReviews() {
        //console.log('inside get reviews');
        //when we're not in a review, we want the fresh list! otherwise we're OK with the existing one
        if (this._readonlyreviewsService.ReadOnlyReviews.length == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {

            this._readonlyreviewsService.Fetch(this.dtTrigger);
             
                //Complete
                //this.changeDetectorRef.detectChanges();
                //const table: any = $("#user-table").DataTable();
                //table.DataTable();
            
        }
    }

    ngOnInit() {

        //this.changeDetectorRef.detectChanges();
        //const table: any = $("#user-table").DataTable();
        
        //table.DataTable();

        //this.chRef.detectChanges();
        //const table: any = $('table');
        //this.dataTable = table.DataTable(); 

        this.dtOptions = {
            pagingType: 'full_numbers',
            columnDefs: [
                { "type": "num", "targets": 1 }
            ]
            //pageLength: 2
        };

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            console.log('user is empty...');
            this.router.navigate(['home']);
        }
        else {

            //console.log("getting ReadOnlyReviews");
            //this.ReviewerIdentityServ.Report();

            this._readonlyreviewsService.Fetch(this.dtTrigger);

        }
    }
    ngOnDestroy() {
        //console.log('killing ROR comp');
        //this._readonlyreviewsService.ReadOnlyReviews = [];
        this.dtTrigger.unsubscribe();
    }

}

//export class WithOptionsComponent implements OnInit {
//    dtOptions: DataTables.Settings = {};

//    ngOnInit(): void {
//        this.dtOptions = {
//            pagingType: 'full_numbers'
//        };
//    }
//}