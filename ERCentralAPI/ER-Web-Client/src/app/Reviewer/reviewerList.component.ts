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
import { RobotsService } from '../services/Robots.service';


@Component({
  selector: 'reviewerList',
  templateUrl: './reviewerList.component.html',
  providers: []
})

export class ReviewerListComponent implements OnInit {
  //some inspiration taken from: https://malcoded.com/posts/angular-file-upload-component-with-express
  constructor(
    private robotsService: RobotsService,
    private ConfirmationDialogService: ConfirmationDialogService,
    private ReviewerIdentityService: ReviewerIdentityService,
    private reviewInfoService: ReviewInfoService,
    private modalService: ModalService,
    private AccountManagerService: ReviewService,
    private notificationService: NotificationService,
    @Inject('BASE_URL') private _baseUrl: string,
  ) { }
  ngOnInit() {
    if (this.robotsService.RobotsList.length == 0) {
      this.robotsService.GetRobotsList().then(()=> this.DoFilterContacts());
    }
    else this.DoFilterContacts();
    //this.RefreshData();
    //this.reviewInfoService.FetchReviewMembers();
  }
  public isAddReviewerExpanded: boolean = false;
  private _FilteredContacts: Contact[] = [];
  public get isCochraneReview(): boolean {
    return this.reviewInfoService.ReviewInfo.isCochrane;
  };
  public get Contacts(): Contact[] {
    return this.reviewInfoService.Contacts;
  }
  public get FilteredContacts(): Contact[] {
    return this._FilteredContacts;
  }
  public DoFilterContacts(): void {
    const theFullList = this.robotsService.RobotsList.concat(this.robotsService.RobotsExpiredList);
    this._FilteredContacts = this.reviewInfoService.Contacts.filter(
      f => {
        if (theFullList.findIndex(ff => ff.robotContactId == f.contactId) != -1) return false;
        if (f.contactName == "Auto-Reconcile User" && f.expiry == "") return false;
        return true;
      });
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
    else {
      return false;
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
      this.reviewInfoService.FetchReviewMembers().then(()=> this.DoFilterContacts());
    }
  }

  async RemoveReviewerFromReview(member: Contact) {



    let result = await this.AccountManagerService.RemoveReviewer(member.contactId);
    if (result == true) {
      // reload the list to show updated list 
      this.reviewInfoService.FetchReviewMembers().then(() => this.DoFilterContacts());
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
      this.reviewInfoService.FetchReviewMembers().then(()=> this.DoFilterContacts());
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

  SetRelevantDropDownValues(event: Event, reviewer: Contact) {

    if (reviewer.contactId == this.ReviewerIdentityService.reviewerIdentity.userId) {

      // if the user removes their admin access then all sorts of things already on the screen shouldn't be there.
      // Rather than having to go through and hide those bits the cleaner (easier) solution is to just not allow the reviewer to 
      // remove their own admin access. Someone else will need to do that.
      // This also means that we don't need to check that there is still at least one admin in the review.
      this.modalService.GenericErrorMessage("You cannot change your own role as a review admin.<br>Another review admin must do this for you.");

      // change the reviewer back to review admin (how do I do that??)
      // This reloads all of the reviewers. Not ideal but works for now.
      // I would prefer to just reset the dropdown in the correct row
      this.reviewInfoService.FetchReviewMembers();
    }
    else {
      // translate selection to a role
      var friendlyRoleName = "";
      var databaseRoleName = "";
      let selection = parseInt((event.target as HTMLOptionElement).value);
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
      let roles = reviewer.roles;
      // we need to be sure we are not removing the only role they have
      // Actually......., this check is unnecessay as you can select the already selected item in a dropdown
      // Maybe I should leave it here anyway just so I know I thought about it?
      const numberOfRoleSplits = roles.split(",").length - 1;
      if ((numberOfRoleSplits == 0) && (roles == friendlyRoleName)) {
        // they only have one role and we can't remove the only role they have...
        this.ConfirmationDialogService.ShowInformationalModal(
          "You cannot remove the only role this user has been assigned."
          , "Unable to remove role.")
      }
      else {
        // we can add/remove the role
        if (reviewer.roles.includes(friendlyRoleName)) {
          // we are removing a role
          this.ConfirmationDialogService.confirm("Removing a role",
            "Are you sure you want to remove the <strong>" + friendlyRoleName + "</strong> role from <strong>" + reviewer.contactName + "</strong>?", false, '')
            .then((confirm: any) => {
              if (confirm) {
                this.ChangeRole(databaseRoleName, reviewer.contactId);
              }
              this.reviewInfoService.FetchReviewMembers();
            });
        }
        else {
          // we are adding a role
          this.ConfirmationDialogService.confirm("Adding a role",
            "Are you sure you want to add the <strong>" + friendlyRoleName + "</strong> role to <strong>" + reviewer.contactName + "</strong>?", false, '')
            .then((confirm: any) => {
              if (confirm) {
                this.ChangeRole(databaseRoleName, reviewer.contactId);
              }
              this.reviewInfoService.FetchReviewMembers();
            });
        }
      }
    }
  }

  public RemoveRoleFromReviewer(controllingRole: string, reviewer: Contact) {
    // we need a delete button to delete the role when it is at the top of the hierarchy
    let roles = reviewer.roles;
    const numberOfRoleSplits = roles.split(",").length - 1;
    if ((numberOfRoleSplits == 0) && (roles == controllingRole)) {
      // they only have one role and we can't remove the only role they have...
      this.ConfirmationDialogService.ShowInformationalModal(
        "You cannot remove the only role this user has been assigned."
        , "Unable to remove role.")
    }
    else {
      var databaseRoleName = "";
      switch (controllingRole) {
        case ("Review admin"):
          databaseRoleName = "AdminUser";
          break;
        case ("Reviewer"):
          databaseRoleName = "RegularUser";
          break;
        case ("Coding only"):
          databaseRoleName = "Coding only";
          break;
        default:
          databaseRoleName = "ReadOnlyUser";
          break;
      }
      // we are removing a role
      this.ConfirmationDialogService.confirm("Removing a role",
        "Are you sure you want to remove the <strong>" + controllingRole + "</strong> role from <strong>" + reviewer.contactName + "</strong>?", false, '')
        .then((confirm: any) => {
          if (confirm) {
            this.ChangeRole(databaseRoleName, reviewer.contactId);
          }
          this.reviewInfoService.FetchReviewMembers();
        });
    }

  }


  public AddCheckMarkIfRole(role: string, controllingRole: string, contactId: number): string  {
    let index = this.FilteredContacts.findIndex(f => f.contactId == contactId);
    if (index != -1) {
      let roles = this.FilteredContacts[index].roles;
      if (roles.includes(role)) {
        if (role == controllingRole) {
          return "X" + role; // controlling role
        }
        return "x " + role; // other assigned role
      }
      return "-  " + role; // unassigned role
    }
    else return "";
  }

  async ChangeRole(role: string, reviewerID: number) {
    let result = await this.AccountManagerService.UpdateReviewerRole(role, reviewerID);
    if (result == true) {
      // put up a message saying the account was updated?
      this.showAccountRoleUpdatedNotification();
      let user = this.FilteredContacts.find(f => f.contactId == reviewerID);
      if (user) {
        let roles = user.roles.split(',');
        const index = roles.findIndex(f => f == role);
        if (index == -1) {//we were adding a role
          roles.push(role);
        }
        else {//we're removing the role
          roles.splice(index, 1);
        }
        user.roles = roles.join(',');
      }
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




