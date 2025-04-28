import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import { ClassifierService } from '../services/classifier.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { SetAttribute, singleNode } from '../services/ReviewSets.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

@Component({
  selector: 'CheckScreening',
  templateUrl: './CheckScreening.component.html',
  providers: []
})

export class CheckScreening implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private classifierService: ClassifierService,
    private _reviewerIdentityServ: ReviewerIdentityService,
    private confirmationDialogService: ConfirmationDialogService,
    private _eventEmitterService: EventEmitterService,
    private notificationService: NotificationService,
    private modalService: ModalService
  ) { }

  @Output() PleaseCloseMe = new EventEmitter();
  ngOnInit() {

  }
  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;

  CanWrite(): boolean {
    return this._reviewerIdentityServ.HasWriteRights;
  }
  public get ClassifierServiceIsBusy(): boolean {
    return this.classifierService.IsBusy;
  }


  public get nodeSelected(): singleNode | null | undefined {
    return this._eventEmitterService.nodeSelected;
  }
  SetAttrOn(node: singleNode | null | undefined) {
    //alert(JSON.stringify(node));
    if (node != null && node.nodeType == "SetAttribute") {
      let a = node as SetAttribute;
      this.selectedModelDropDown1 = node.name;
      this.DD1 = a.attribute_id;
      this._eventEmitterService.nodeSelected = undefined;
    }
    else {
      this.selectedModelDropDown1 = "";
      this.DD1 = 0;
    }
  }
  SetAttrNotOn(node: singleNode | null | undefined) {
    //alert(JSON.stringify(node));
    if (node != null && node != undefined && node.nodeType == "SetAttribute") {
      let a = node as SetAttribute;
      this.selectedModelDropDown2 = node.name;
      this.DD2 = a.attribute_id;
      this._eventEmitterService.nodeSelected = undefined;
    }
    else {
      this.selectedModelDropDown2 = "";
      this.DD2 = 0;
    }
  }
  public isCollapsedCheckScreening: boolean = false;
  public isCollapsed2CheckScreening: boolean = false;
  public selectedModelDropDown1: string = '';
  public selectedModelDropDown2: string = '';
  public DD1: number = 0;
  public DD2: number = 0;

  CloseBMDropDown1() {

    this.isCollapsedCheckScreening = false;
  }
  CloseBMDropDown2() {

    this.isCollapsed2CheckScreening = false;
  }

  CanCheckScreening() {
    if (this.CanWrite() == false) return false;
    if (this.selectedModelDropDown1 && this.selectedModelDropDown2
      && this.DD1 > 0 && this.DD2 > 0 && this.DD1 != this.DD2) {
      return true;
    }
    return false;
  }
  public get SameCodesAreSelected(): boolean {
    if (this.selectedModelDropDown1 && this.selectedModelDropDown2
      && this.DD1 > 0 && this.DD1 == this.DD2) {
      return true;
    }

    return false;
  }

  public openConfirmationDialogCheckScreening() {
    this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to check screening with these codes ?', false, '')
      .then(
        (confirmed: any) => {
          console.log('User confirmed:', confirmed);
          if (confirmed) {
            this.CheckScreening();
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
  }

  async CheckScreening() {

    if (this.DD1 != null && this.DD2 != null) {

      let res = await this.classifierService.CheckScreening("¬¬CheckScreening", this.DD1, this.DD2);

      if (res != false) { //we get "false" if an error happened...
        if (res == "Starting...") {
          this.notificationService.show({
            content: 'Screening check started. Results will appear as search results when finished (click Refresh List periodically)',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            hideAfter: 5000
          });
          this.Close();
        }
        else if (res == "Already running") {
          this.notificationService.show({
            content: 'Screening check could not be run. A check is already running for this review',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "error", icon: true },
            closable: true
          });
        }
        else if ((res as string).startsWith("Insufficient Data")) {
          this.modalService.GenericErrorMessage((res as string));
        }
        else {
          this.modalService.GenericErrorMessage("Unexpected result:\r\n" + res.toString());
        }
      }
    }

  }
  Close() {
    this.PleaseCloseMe.emit();
  }


  ngOnDestroy() {

  }
}
