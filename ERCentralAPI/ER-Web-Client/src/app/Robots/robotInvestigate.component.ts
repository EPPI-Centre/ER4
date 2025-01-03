import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { ModalService } from '../services/modal.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewSetsService, ReviewSet, singleNode, SetAttribute } from '../services/ReviewSets.service'
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { NotificationService } from '@progress/kendo-angular-notification';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import { iRobotInvestigate, RobotsService } from '../services/Robots.service';


@Component({
  selector: 'RobotInvestigate',
  templateUrl: './robotInvestigate.component.html',
  providers: []
})


export class RobotInvestigate implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private _reviewService: ReviewService,
    private _reviewerIdentityServ: ReviewerIdentityService,
    private _reviewInfoService: ReviewInfoService,
    private _robotsService: RobotsService,
    private modalService: ModalService,
    private _reviewSetsService: ReviewSetsService,
    private _eventEmitterService: EventEmitterService,
    private _notificationService: NotificationService,
  ) { }

  public isCollapsedRobotInvestigate: boolean = false;
  public isCollapsedRobotInvestigate2: boolean = false;
  public selectedRobotInvestigateTextOption: string = "title";
  public DD1: number = 0;
  public DD2: number = 0;
  public selectedAttributeDropDown1: string = '';
  public selectedAttributeDropDown2: string = '';
  public queryInputForRobot: string = 'Enter query here...';
  public resultTextFromRobot: string = "No results yet";
  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;
  public busyInvestigating: boolean = false;
  public sampleSize: number = 20;

  public get HasWriteRights(): boolean {
    return this._reviewerIdentityServ.HasWriteRights
  }

  CloseBMDropDown1() {

    this.isCollapsedRobotInvestigate = false;
  }
  CloseBMDropDown2() {

    this.isCollapsedRobotInvestigate2 = false;
  }

  CanOnlySelectRoots() {
    return true;
  }

  public get nodeSelected(): singleNode | null | undefined {
    return this._eventEmitterService.nodeSelected;//SG note: not sure this is a good idea, how is this better than this.reviewSetsService.selectedNode?
  }
  SetAttr1(node: singleNode | null | undefined) {
    if (node != null && node.nodeType == "SetAttribute") {
      let a = node as SetAttribute;
      this.selectedAttributeDropDown1 = node.name;
      this.DD1 = a.attribute_id;
    }
  }
  SetAttr2(node: singleNode | null | undefined) {
    if (node != null && node.nodeType == "SetAttribute") {
      let a = node as SetAttribute;
      this.selectedAttributeDropDown2 = node.name;
      this.DD2 = a.attribute_id;
    }
  }
  public IsServiceBusyInvestigating(): boolean {
    if (this.busyInvestigating) return true;
    else return false;
  }

  @Input() Context: string = "";
  public isExpanded: boolean = false;
  ngOnInit() {
    this._reviewSetsService.selectedNode = null;
  }
  CanWrite(): boolean {
    return this._reviewerIdentityServ.HasWriteRights;
  }

  public get RobotInvestigate(): iRobotInvestigate {
    return this._robotsService.RobotInvestigate;
  }

  ngOnDestroy() {

  }
  ngAfterViewInit() {

  }

  public get CanRunRobotInvestigate(): boolean { // this feels a bit backwards, as it returns TRUE to disable the control
    if (!this.HasWriteRights) return true;
    if (!this._reviewInfoService.ReviewInfo.canUseRobots) return true;
    if (this.selectedAttributeDropDown1 && this.queryInputForRobot != '' &&
      this.queryInputForRobot != 'Enter query here...' && this.busyInvestigating == false && this.selectedRobotInvestigateTextOption == 'title') return false;
    if (this.selectedAttributeDropDown1 && this.queryInputForRobot != '' &&
      this.queryInputForRobot != 'Enter query here...' && this.busyInvestigating == false && this.selectedRobotInvestigateTextOption != 'title' && this.selectedAttributeDropDown2) return false;
    return true;
  }

  public async RunRobotInvestigateCommand() {
    if (this.selectedRobotInvestigateTextOption == 'title') {
      this.sampleSize = Math.min(this.sampleSize, 150)
    };
    let cmd: iRobotInvestigate = {
      queryForRobot: this.queryInputForRobot,
      getTextFrom: this.selectedRobotInvestigateTextOption,
      itemsWithThisAttribute: this.DD1,
      textFromThisAttribute: this.DD2,
      sampleSize: this.sampleSize,
      returnMessage: "",
      returnResultText: "",
      returnItemIdList: ""
    };
    this._notificationService.show({
      content: "Query submitted to server",
      position: { horizontal: 'center', vertical: 'top' },
      animation: { type: 'fade', duration: 500 },
      type: { style: 'success', icon: true },
      hideAfter: 4500
    });
    this.busyInvestigating = true;
    let res = await this._robotsService.RunRobotInvestigateCommand(cmd);
    if (res.returnMessage != "Error") {
      //no need to handle errors here - we do that in the service as usual
      //this.confirmationDialogService..ShowInformationalModal(res.returnMessage, "GPT4 result");
      this._notificationService.show({
        content: "GPT4 result: " + res.returnMessage,
        position: { horizontal: 'center', vertical: 'top' },
        animation: { type: 'fade', duration: 500 },
        type: { style: 'success', icon: true },
        hideAfter: 4500
      });
      this.resultTextFromRobot = "<p><h4>Query: <i>" + this.queryInputForRobot + "</i></h4></p><p> " + res.returnResultText + "</p>";
    }
    this.busyInvestigating = false;
  }

}

