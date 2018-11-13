import { Component, Inject, OnInit, Output, EventEmitter, OnDestroy, ViewChild, ChangeDetectorRef, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subject, BehaviorSubject, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { readonlyreviewsService, ReadOnlyReview } from '../services/readonlyreviews.service';
import { timer } from 'rxjs'; // (for rxjs < 6) use 'rxjs/observable/timer'
import { take, map } from 'rxjs/operators';
import { DataTableDirective, DataTablesModule } from 'angular-datatables';
import { CollectionViewer, DataSource } from '@angular/cdk/collections';
import { ModalService } from '../services/modal.service';
import { MatTableDataSource, MatSort } from '@angular/material';

@Component({
    selector: 'readonlyreviews',
    templateUrl: './readonlyreviews.component.html',
	providers: [],
	encapsulation: ViewEncapsulation.None
    
})
export class FetchReadOnlyReviewsComponent implements OnInit, OnDestroy{
	dataSource: any | undefined;
	displayedColumns = ["Full Reviewing", "reviewId", "reviewName", "lastAccess"];
    constructor(private router: Router,
                private _httpC: HttpClient,
                @Inject('BASE_URL') private _baseUrl: string,
                private ReviewerIdentityServ: ReviewerIdentityService,
		public _readonlyreviewsService: readonlyreviewsService,
		private modalService: ModalService

	) {
		
    }


	@ViewChild(MatSort) sort1!: MatSort;

   // dtOptions: DataTables.Settings = {};
    reviews: ReadOnlyReview[] = [];
    // We use this trigger because fetching the list of persons can be quite long,
    // thus we ensure the data is fetched before rendering
    //dtTrigger: Subject<any> = new Subject();

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
		console.log('all the way to here: ' +  RevId);
        this.ReviewerIdentityServ.LoginToFullReview(RevId);
        
    }

    getReviews() {
        //console.log('inside get reviews');
        //when we're not in a review, we want the fresh list! otherwise we're OK with the existing one
        if (this._readonlyreviewsService.ReadOnlyReviews.length == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {

            this._readonlyreviewsService.Fetch();
            
        }
	}
	loadReviews() {

		this._readonlyreviewsService.Fetch().toPromise().then(

				(result) => {

					this._readonlyreviewsService.ReadOnlyReviews = result;
					this.createTable();

				}, error => {
					this.modalService.GenericError(error);

				}

			);
	}
	createTable() {
		this.dataSource = new MatTableDataSource(this._readonlyreviewsService.ReadOnlyReviews);
		this.dataSource.sort = this.sort1;
	}
    ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
         
            this.router.navigate(['home']);
        }
        else {

			this.loadReviews();
            //this._readonlyreviewsService.Fetch(this.dtTrigger);

        }
	}

	ngOnDestroy() {

        //this._readonlyreviewsService.ReadOnlyReviews = [];
        //this.dtTrigger.unsubscribe();

    }

}