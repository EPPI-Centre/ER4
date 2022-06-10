import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { SourcesService, IncomingItemsList, ImportFilter, SourceForUpload, Source, ReadOnlySource } from '../services/sources.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { Helpers } from '../helpers/HelperMethods';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ModalService } from '../services/modal.service';
import { ReviewService } from '../services/review.service';
import { NotificationService } from '@progress/kendo-angular-notification';


@Component({
    selector: 'reviewerList',
    templateUrl: './reviewerList.component.html',
    providers: []
})

export class ReviewerListComponent implements OnInit {
    //some inspiration taken from: https://malcoded.com/posts/angular-file-upload-component-with-express
    constructor( 
        private ItemListService: ItemListService,
        private _eventEmitter: EventEmitterService,
        private SourcesService: SourcesService,
        private ConfirmationDialogService: ConfirmationDialogService,
        private ReviewerIdentityService: ReviewerIdentityService,
        private reviewInfoService: ReviewInfoService,
        private modalService: ModalService,
        private AccountManagerService: ReviewService,
        private notificationService: NotificationService,
        @Inject('BASE_URL') private _baseUrl: string,
    ) {    }
    ngOnInit() {
        //this.RefreshData();
        //this.reviewInfoService.FetchReviewMembers();
    }
    public isAddReviewerExpanded: boolean = false;
    private _ReviewContacts: Contact[] = [];
    public get Contacts(): Contact[] {
        //return this._ReviewContacts;
        return this.reviewInfoService.Contacts;
    }
    public reviewerEmail: string = '';

    ExpandAddReviewer() {
        if (this.isAddReviewerExpanded == true) {
            this.reviewerEmail = "";
            this.CloseInviteReviewer();
        }
        else {
            this.isAddReviewerExpanded = true;
        }        
    }

    CloseInviteReviewer() {
        this.reviewerEmail = "";
        this.isAddReviewerExpanded = false;
    }

    public get HasWriteRights(): boolean {
        return this.ReviewerIdentityService.HasWriteRights;
    }


    CanInviteReviewer() {
        if (this.CheckEmail) {
            return true;
        }
    }


    private email = "^[a-zA-Z0-9_!#$%&'*+/=?`{|}~^.-]+@[a-zA-Z0-9.-]+$";
    // from https://howtodoinjava.com/java/regex/java-regex-validate-email-address/

    get CheckEmail(): boolean {
        if (this.reviewerEmail.match(this.email)) {
            return true;
        }
        else { // incorrect email syntax
            return false;
        }
    }


    InviteReviewer() {
        // check that the email isn't already in the list of reviewers
        const reviewMembers = this.reviewInfoService.Contacts;
        var match = 0;
        for (var i = 0; i < reviewMembers.length; i++) {
            if (reviewMembers[i].email == this.reviewerEmail.trim()) {
                match = 1;
                i = reviewMembers.length
                this.modalService.GenericErrorMessage("This email address is already in the list of reviewers.");
            }
        }
        if (match == 0) {
            this.InviteReviewerIntoReview()
        }
    }

    async InviteReviewerIntoReview() {
        let result = await this.AccountManagerService.InviteReviewer(this.reviewerEmail.trim());
            if (result == true) {
                // close the invite area and reload the list to show the new reviewer 
                this.isAddReviewerExpanded = false;
                this.reviewInfoService.FetchReviewMembers();
            }
    }

    async RemoveReviewerFromReview(member: Contact) {
        let result = await this.AccountManagerService.RemoveReviewer(member.contactId);
        if (result == true) {
            // reload the list to show updated list 
            this.reviewInfoService.FetchReviewMembers();
        }
    }


    public RefreshData() {
        this.getMembers();
    }

    getMembers() {
        if (!this.reviewInfoService.IsBusy && (!this.reviewInfoService.ReviewInfo || this.reviewInfoService.ReviewInfo.reviewId < 1)) {
            this.reviewInfoService.Fetch();
        }
        if (this.reviewInfoService.Contacts.length == 0)
            this.reviewInfoService.FetchReviewMembers();
    }


    RemoveReviewer(member: Contact) {
        if (member.contactId == this.ReviewerIdentityService.reviewerIdentity.userId) {
            this.modalService.GenericErrorMessage("You cannot remove yourself from a review.<br>Another review admin must do this for you.");
        }
        else {
            let msg: string;
            msg = "Are you sure you want to remove<br> <b>\"" + member.contactName + "\"</b> from this review?"
            this.openConfirmationDialogRemoveReviewer(member, msg);
        }
    }

    public openConfirmationDialogRemoveReviewer(member: Contact, msg: string) {
        this.ConfirmationDialogService.confirm('Please confirm', msg, false, '')
            .then(
                (confirmed: any) => {
                    //console.log('User confirmed source (un/)delete:', confirmed);
                    if (confirmed) {
                        this.RemoveReviewerFromReview(member);
                    } else {
                        //alert('did not confirm');
                    }
                }
            )
            .catch(() => {
                //console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)');
            });
    }

    SetRelevantDropDownValues(selection: number, reviewer: Contact) {

        if (reviewer.contactId == this.ReviewerIdentityService.reviewerIdentity.userId) {

            // if the user removes their admin access then all sorts of things already on the screen shouldn't be there.
            // Rather than having to go through and hide those bits the cleaner (easier) solution is to just not allow the reviewer to 
            // remove their own admin access. Someone else will need to do that.
            // This also means that we don't need to check that there is still at least one admin in the review.
            this.modalService.GenericErrorMessage("You cannot remove yourself as a review admin.<br>Another review admin must do this for you.");

            // change the reviewer back to review admin (how do I do that??)
            // This reloads all of the reviewers. Not ideal but works for now.
            // I would prefer to just reset the dropdown in the correct row
            this.reviewInfoService.FetchReviewMembers();
        }
        else {
            // translate selection to a role
            var friendlyRoleName = "";
            var databaseRoleName = "";
            switch (true) {
                case (selection == 0):
                    friendlyRoleName = "Review admin";
                    databaseRoleName = "AdminUser";
                    break;
                case (selection == 1):
                    friendlyRoleName = "Reviewer";
                    databaseRoleName = "RegularUser";
                    break;
                case (selection == 2):
                    friendlyRoleName = "Coding only";
                    databaseRoleName = "Coding only";
                    break;
                default:
                    friendlyRoleName = "Read only";
                    databaseRoleName = "ReadOnlyUser";
                    break;
            }
            this.ChangeRole(databaseRoleName, reviewer.contactId);
        }
    }


    async ChangeRole(role: string, reviewerID: number) {
        let result = await this.AccountManagerService.UpdateReviewerRole(role, reviewerID);
            if (result == true) {
                // put up a message saying the account was updated?
                this.showAccountRoleUpdatedNotification();
            }
    }

    private showAccountRoleUpdatedNotification(): void {
        let contentSt: string = "Account role updated";
        this.notificationService.show({
            content: contentSt,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "success", icon: true },
            closable: true
        });
    }


    private _roleOptions: kvSelectFrom[] = 
    [{ key: 0, value: 'Review admin' }, 
    { key: 1, value: 'Reviewer' },
    { key: 2, value: 'Coding only' },
    { key: 3, value: 'Read only' }];
    public get RoleOptions(): kvSelectFrom[] {
        return this._roleOptions;
    }


    public set RoleOptions(value: kvSelectFrom[]) {

        this._roleOptions = value;

    }


}

export interface kvSelectFrom {
    key: number;
    value: string;
}




