import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ModalService } from './modal.service';
import { ArchieIdentity, ReviewerIdentityService } from './revieweridentity.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ConfirmationDialogService } from './confirmation-dialog.service';


@Injectable({
    providedIn: 'root',
})

export class readonlyreviewsService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        private ConfirmationDialogService: ConfirmationDialogService,
        private ReviewerIdentityService: ReviewerIdentityService,
        @Inject('BASE_URL') private _baseUrl: string
    ) { super(); }

    private _ReviewList: ReadOnlyReview[] = [];

    public get ReadOnlyReviews(): ReadOnlyReview[] {
        //if (this._ReviewList.length == 0) {

        //    const ReadOnlyReviewsJson = localStorage.getItem('ReadOnlyReviews');
        //    let ReadOnlyReviews: ReadOnlyReview[] = ReadOnlyReviewsJson !== null ? JSON.parse(ReadOnlyReviewsJson) : [];
        //    if (ReadOnlyReviews == undefined || ReadOnlyReviews == null || ReadOnlyReviews.length == 0) {
        //        return this._ReviewList;
        //    }
        //    else {
        //        //console.log("Got workAllocations from LS");
        //        this._ReviewList = ReadOnlyReviews;
        //    }
        //}
        return this._ReviewList;

    }

    public set ReadOnlyReviews(ror: ReadOnlyReview[]) {
        this._ReviewList = ror;
        //this.Save();
    }
    private _ArchieReviews: ReadOnlyArchieReview[] = [];
    public get ArchieReviews(): ReadOnlyArchieReview[] {
        return this._ArchieReviews;
    }
    
    public Fetch() {
        this._BusyMethods.push("Fetch");
        return this._httpC.get<ReadOnlyReview[]>(this._baseUrl + 'api/review/readonlyreviews').subscribe(result => {
            this.ReadOnlyReviews = result;
            this.RemoveBusy("Fetch");
        }, error => {
            this.RemoveBusy("Fetch");
            this.modalService.GenericError(error);
        }
        );
    }
    public FetchArchieReviews() {
        this._BusyMethods.push("FetchArchieReviews");
        return this._httpC.get<ReadOnlyArchieReview[] | iErrorFromArchie>(this._baseUrl + 'api/review/ReadOnlyArchieReviews').subscribe(result => {
            if (this.checkForArchieError(result)) {
                this.ConfirmationDialogService.confirm("Re-Authenticate in Archie?",
                    "Sorry. Could not fetch your list of Archie Reviews.\nUsually this means you need to Re-Authenticate via Archie", false, "")
                    .then(
                        (confirmed: any) => {
                            if (confirmed) {
                                this.ReviewerIdentityService.GoToArchie();
                            } else {
                                //alert('did not confirm');
                            }
                        }
                    )
                    .catch();
                this._ArchieReviews = [];
            }
            else { this._ArchieReviews = result; }//it worked, keep the data!
            console.log("Handling FetchArchieReviews: ", result);

            this.RemoveBusy("FetchArchieReviews");

        }, error => {
            this.RemoveBusy("FetchArchieReviews");
            console.log("Handling FetchArchieReviews ERR:", error);
            this.modalService.GenericError(error);
        }
        );
    }
    public ArchieReviewPrepare(ArchieID: string) {
        //will checkout review as well.
        this._BusyMethods.push("ArchieReviewPrepare");
        //this.itemID.next(ItemId); 
        //console.log('FetchCoding');
        let body = JSON.stringify({ Value: ArchieID });
        this._httpC.post<iArchieReviewPrepCheckInOutCommand>(this._baseUrl + 'api/review/ArchieReviewPrepare',
            body).subscribe(result => {
                if (result.result == "Done") {
                    //all is well! update local data and return
                    let index = this._ArchieReviews.findIndex(res => res.archieReviewId == ArchieID);
                    if (index == -1) this.FetchArchieReviews();//uh? Don't know what review was actioned, fetch data again...
                    else {
                        if (this._ArchieReviews[index].reviewId == 0) this._ArchieReviews[index].reviewId = result.reviewID;
                        this._ArchieReviews[index].isLocal = true;
                        this._ArchieReviews[index].isCheckedOutHere = true;
                    }
                }
                else {
                    this.FetchArchieReviews();
                    this.modalService.GenericErrorMessage("Sorry. The operation failed. " + result.result
                        + " If the problem persists please contact EPPISupport.");
                }
            }, error => {
                this.RemoveBusy("ArchieReviewPrepare");
                this.modalService.SendBackHomeWithError(error);
            }
            , () => { this.RemoveBusy("ArchieReviewPrepare"); }
            );
    }
    public ArchieReviewUndoCheckout(ArchieID: string) {
        this._BusyMethods.push("ArchieReviewUndoCheckout");
        //this.itemID.next(ItemId); 
        //console.log('FetchCoding');
        let body = JSON.stringify({ Value: ArchieID });
        this._httpC.post<iArchieReviewPrepCheckInOutCommand>(this._baseUrl + 'api/review/ArchieReviewUndoCheckout',
            body).subscribe(result => {
                if (result.result == "Done") {
                    //all is well! update local data and return
                    let index = this._ArchieReviews.findIndex(res => res.archieReviewId == ArchieID);
                    if (index == -1) this.FetchArchieReviews();//uh? Don't know what review was actioned, fetch data again...
                    else {
                        this._ArchieReviews[index].isCheckedOutHere = false;
                    }
                }
                else {
                    this.FetchArchieReviews();
                    this.modalService.GenericErrorMessage("Sorry. The operation failed. " + result.result
                        + " If the problem persists please contact EPPISupport.")
                }
            }, error => {
                this.RemoveBusy("ArchieReviewUndoCheckout");
                this.modalService.SendBackHomeWithError(error);
            }
            , () => { this.RemoveBusy("ArchieReviewUndoCheckout"); }
            );
    }

    checkForArchieError(toBeDetermined: any): toBeDetermined is iErrorFromArchie {
        if ((toBeDetermined as iErrorFromArchie).reason && (toBeDetermined as iErrorFromArchie).error) {
        return true
    }
    return false
}

    //public Save() {
    //    if (this._ReviewList.length > 0)
    //        localStorage.setItem('ReadOnlyReviews', JSON.stringify(this._ReviewList));
    //    else if (localStorage.getItem('ReadOnlyReviews'))//to be confirmed!! 
    //        localStorage.removeItem('ReadOnlyReviews');
    //}
}

export class ReadOnlyReview {
    reviewId: number = 0;
    reviewName: string = "";
    contactReviewRoles: string = "";
    reviewOwner: string = "";
    lastAccess: string = "";
}
export interface ReadOnlyArchieReview {
    archieReviewCD: string;
    archieReviewId: string;
    identity: ArchieIdentity;
    checkedOutInArchie: boolean;
    contactReviewRoles: string;
    isCheckedOutHere: boolean;
    isLocal: boolean;
    lastAccess: string;
    reviewId: number;
    reviewName: string;
    reviewOwner: string;
    stage: string;
    status: string;
    userIsInReview: boolean;
}
interface iErrorFromArchie {
    error: string;
    reason: string;
}
interface iArchieReviewPrepCheckInOutCommand {
    archieReviewID: string;
    result: string;
    reviewID: number;
}
