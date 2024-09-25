import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { readonlyreviewsService } from '../services/readonlyreviews.service';
import { ReviewInfo, ReviewInfoService } from '../services/ReviewInfo.service';
import { NotificationService } from '@progress/kendo-angular-notification';

@Component({
  selector: 'EditReviewComp',
  templateUrl: './editReview.component.html',
  providers: []
})

export class EditReviewComponent implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    public _reviewService: ReviewService,
    private readonlyreviewsService: readonlyreviewsService,
    public _reviewerIdentityServ: ReviewerIdentityService,
    public modalService: ModalService,
    public ReviewInfoService: ReviewInfoService,
    private notificationService: NotificationService,
  ) { }

  //@Output() onCloseClick = new EventEmitter();
  public isExpanded: boolean = false;
  public EditingRevInfo: ReviewInfo = this.ReviewInfoService.ReviewInfo.Clone();
  public EditingReviewName: string = "";
  ngOnInit() {

  }
  CanWrite(): boolean {
    //console.log("create rev check:", this._reviewerIdentityServ.reviewerIdentity);
    if (!this._reviewService.IsBusy) {
      if (!this._reviewerIdentityServ.HasWriteRights) {
        //one more check: is the user in the first screen?
        if (this._reviewerIdentityServ.reviewerIdentity.reviewId == 0
          && this._reviewerIdentityServ.reviewerIdentity.ticket === ""
          && this._reviewerIdentityServ.reviewerIdentity.token
          && this._reviewerIdentityServ.reviewerIdentity.token.length > 100
          && this._reviewerIdentityServ.reviewerIdentity.roles
          && this._reviewerIdentityServ.reviewerIdentity.roles.length > 0
          && this._reviewerIdentityServ.reviewerIdentity.roles.indexOf('ReadOnlyUser') == -1
          && this._reviewerIdentityServ.reviewerIdentity.daysLeftAccount >= 0
        )
          return true;
        else return false;
      }
      else return true;
    } else {
      return false;
    }
  }
  Expand() {
    this.isExpanded = true;
    this.EditingRevInfo = this.ReviewInfoService.ReviewInfo.Clone();
    this.EditingReviewName = this.EditingRevInfo.reviewName;
  }
  CloseEditReview() {
    this.EditingRevInfo = new ReviewInfo();
    this.isExpanded = false;
  }
  public get CanSaveReviewOptions() {
    if (!this.CanWrite()) return false;
    if (this.EditingRevInfo.reviewName.trim() == '') return false;
    const rinfo = this.ReviewInfoService.ReviewInfo;
    if (this.EditingRevInfo.comparisonsInCodingOnly != rinfo.comparisonsInCodingOnly
      || this.EditingRevInfo.showScreening != rinfo.showScreening
    ) {
      return true;
    }
    else return false;
  }

  async SaveReviewOptions() {
    if (this.CanSaveReviewOptions) {
      let result = await this.ReviewInfoService.Update(this.EditingRevInfo);
      if (result == true) {
        // close div and put up a message saying the review was updated 
        this.isExpanded = false;
        this.showAccountUpdatedNotification();
      }
    }
  }
  public get CanSaveReviewName(): boolean {
    if (this.CanWrite()) {
      const rn = this.EditingReviewName.trim();
      if (rn.length > 0 && rn != this.EditingRevInfo.reviewName) return true;
    }
    return false;
  }
  async SaveReviewName() {
    if (this.EditingReviewName.trim().length > 0) {
      let result = await this._reviewService.UpdateReviewName(this.EditingReviewName);
      if (result == true) {
        // close div and put up a message saying the account was updated 
        this.showAccountUpdatedNotification();
        this.ReviewInfoService.ReviewInfo.reviewName = this.EditingReviewName.trim();
        this.isExpanded = false;
      }
    }
  }

  private showAccountUpdatedNotification(): void {
    let contentSt: string = "Review updated";
    this.notificationService.show({
      content: contentSt,
      animation: { type: 'slide', duration: 400 },
      position: { horizontal: 'center', vertical: 'top' },
      type: { style: "success", icon: true },
      hideAfter: 3000
    });
  }


  
  onFullSubmit(revId: number) {
    //this.readonlyreviewsService.Fetch();
    this.CloseEditReview();
    this._reviewerIdentityServ.LoginToFullReview(revId);
  }
  BackToMain() {
    this.router.navigate(['Main']);
  }
  ngOnDestroy() {

  }
  ngAfterViewInit() {

  }
}
