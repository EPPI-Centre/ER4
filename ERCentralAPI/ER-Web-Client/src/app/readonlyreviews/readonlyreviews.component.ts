import { Component, Inject, OnInit, OnDestroy, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { readonlyreviewsService, ReadOnlyReview, ReadOnlyArchieReview } from '../services/readonlyreviews.service';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { Helpers } from '../helpers/HelperMethods';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

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
                @Inject('BASE_URL') private _baseUrl: string,
                private ReviewerIdentityServ: ReviewerIdentityService,
        public _readonlyreviewsService: readonlyreviewsService,
        private confirmationDialogService: ConfirmationDialogService,
		private _eventEmitter: EventEmitterService
	) {
		
    }
    ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
         
            this.router.navigate(['home']);
        }
        else {

			//this.loadReviews();
            //this._readonlyreviewsService.Fetch(this.dtTrigger);
            //this.getReviews();
        }
	}
    allowUnsort: boolean = true;

    public get IsServiceBusy(): boolean {
        return this._readonlyreviewsService.IsBusy;
    }

    public get DataSource(): GridDataResult {
        return {
            data: orderBy(this._readonlyreviewsService.ReadOnlyReviews, this.sort),
            total: this._readonlyreviewsService.ReadOnlyReviews.length
        };
    }
    public get ArchieReviewsData(): GridDataResult {
        return {
            data: orderBy(this._readonlyreviewsService.ArchieReviews, this.ArchieSort),
            total: this._readonlyreviewsService.ArchieReviews.length
        };
    }
    public HasCodingOnlyRole(RoR: ReadOnlyReview): boolean {
        if (RoR.contactReviewRoles.toLowerCase().includes("coding only")) return true;
        else return false;
    }
    public get IsCochraneUser(): boolean {
        if (this.ReviewerIdentityServ.reviewerIdentity.roles.indexOf("CochraneUser") == -1) return false;
        else return true;
    }
    public get singleTableHeight(): number {
        if (this.IsCochraneUser) return 160;
        else return 240;
    }
    public sort: SortDescriptor[] = [{
        field: 'lastAccess',
        dir: 'desc'
    }];
	public sortChange(sort: SortDescriptor[]): void {
		//console.log(sort);
        this.sort = sort;
    }
    public ArchieSort: SortDescriptor[] = [{
        field: 'reviewName',
        dir: 'asc'
    }];
    public ArchieSortChange(sort: SortDescriptor[]): void {
        //console.log(sort);
        this.ArchieSort = sort;
    }
    FormatDate(DateSt: string): string {
        return Helpers.FormatDate2(DateSt);
    }

    onSubmit(f: string) {
            let RevId: number = parseInt(f, 10);
        this.ReviewerIdentityServ.LoginToReview(RevId);
    }

	onFullSubmit(Ror: ReadOnlyReview) {
        console.log('onFullSubmit: ', Ror);
        if (this.HasCodingOnlyRole(Ror)) return;
        let RevId: number = Ror.reviewId;
		//console.log('all the way to here: ' +  RevId);
        this.ReviewerIdentityServ.LoginToFullReview(RevId);
    }
    UndoCheckout(Ror: ReadOnlyArchieReview) {
        console.log('UndoCheckout: ', Ror);
        if (Ror.isCheckedOutHere) this._readonlyreviewsService.ArchieReviewUndoCheckout(Ror.archieReviewId);
    }
    ConfirmActivate(Ror: ReadOnlyArchieReview) {
        this.confirmationDialogService.confirm('Activate Review?',
            'Activation will create the review in EPPI-Reviewer. <br />No data will be transferred from RevMan/Archie.<br /><br />'
            + 'You will be given the <em>Review Administrator</em> role in this review.'
            , false, '')
            .then(
                (confirmed: any) => {
                    //console.log('User confirmed:', confirmed);
                    if (confirmed) {
                        this.Checkout(Ror);
                    } else {
                        //alert('did not confirm');
                    }
                }
            )
            .catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
    }
    Checkout(Ror: ReadOnlyArchieReview) {
        console.log('Checkout: ', Ror);
        if (!Ror.isCheckedOutHere || !Ror.isLocal) this._readonlyreviewsService.ArchieReviewPrepare(Ror.archieReviewId);
    }

    getReviews() {
        //console.log('fetching reviews');
        this._readonlyreviewsService.Fetch();
        if (this.IsCochraneUser) this._readonlyreviewsService.FetchArchieReviews();
        ////console.log('inside get reviews');
        ////when we're not in a review, we want the fresh list! otherwise we're OK with the existing one
        //if (this._readonlyreviewsService.ReadOnlyReviews.length == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {

        //    this._readonlyreviewsService.Fetch();
            
        //}
	}
	//loadReviews() {

	//	lastValueFrom(this._readonlyreviewsService.Fetch()).then(

	//			(result) => {

	//				this._readonlyreviewsService.ReadOnlyReviews = result;
	//				this.createTable();

	//			}, error => {
	//				this.modalService.GenericError(error);

	//			}

	//		);
	//}
	//createTable() {
	//	this.dataSource = new MatTableDataSource(this._readonlyreviewsService.ReadOnlyReviews);
	//	this.dataSource.sort = this.sort1;
	//}
    Clear() {
        this._readonlyreviewsService.Clear();
    }
	ngOnDestroy() {

        //this._readonlyreviewsService.ReadOnlyReviews = [];
        //this.dtTrigger.unsubscribe();
        this.Clear();
    }

}
