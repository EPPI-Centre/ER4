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
import { iRobotInvestigate, iRobotSettings, RobotsService } from '../services/Robots.service';
import { encodeBase64, saveAs } from '@progress/kendo-file-saver';
import { Helpers } from '../helpers/HelperMethods';


@Component({
  selector: 'RobotInvestigate',
  templateUrl: './robotInvestigate.component.html',
  providers: []
})


export class RobotInvestigate implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private _reviewerIdentityServ: ReviewerIdentityService,
    private _reviewInfoService: ReviewInfoService,
    private _robotsService: RobotsService,
    private _reviewSetsService: ReviewSetsService,
    private _eventEmitterService: EventEmitterService,
    private _notificationService: NotificationService,
  ) { }

  @Input() Context: string = "";
  ngOnInit() {
    this._reviewSetsService.selectedNode = null;
    //this block allows to reload the page from scratch AND ensure the current user has all the rights they need
    //it's especially useful in dev
    if (this._reviewInfoService.ReviewInfo.reviewId == 0) {
      this._reviewInfoService.Fetch().then(() => { this.CheckUserRights(); });
    }
    else this.CheckUserRights();
    if (this._reviewSetsService.ReviewSets.length == 0) this._reviewSetsService.GetReviewSets(true);
    if (this.ResultsFromRobot.length > 0) this.CurrentIndexInResults = this.ResultsFromRobot.length - 1;
    if (this._robotsService.RobotsList.length == 0) this._robotsService.GetRobotsList();
  }
  private CheckUserRights() {
    if (!this._reviewerIdentityServ.UserCanGPTinvestigate || !this._reviewInfoService.ReviewInfo.hasCreditForRobots) {
      this.router.navigate(['home']);
    }
  }
  public isCollapsedRobotInvestigate: boolean = false;
  public isCollapsedRobotInvestigate2: boolean = false;
  public selectedRobotInvestigateTextOption: string = "title";
  public DD1: number = 0;
  public DD2: number = 0;
  public selectedAttributeDropDown1: string = '';
  public selectedAttributeDropDown2: string = '';
  public queryInputForRobot: string = '';
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

  public get ResultsFromRobot(): iRobotInvestigate[] {
    return this._robotsService.RobotInvestigateResults;
  }
  public CurrentIndexInResults: number = -1;
  public get CurrentResult(): iRobotInvestigate | null {
    if (this.CurrentIndexInResults == -1) return null;
    else {
      const res = this.ResultsFromRobot[this.CurrentIndexInResults];
      if (res) return res;
      else return null;
    }
  }
  public get CanGoToPrevious(): boolean {
    if (this.ResultsFromRobot.length == 0) return false;
    if (this.CurrentIndexInResults == 0) return false;
    return true;
  }
  public get CanGoToNext(): boolean {
    if (this.ResultsFromRobot.length == 0) return false;
    if (this.CurrentIndexInResults >= this.ResultsFromRobot.length - 1) return false;
    return true;
  }
  public TextFromAttributeId(AttId: number): string {
    return this._robotsService.TextFromAttributeId(AttId);
  }
  public TextFromInvestigateTextOption(OptionVal: string): string {
    return this._robotsService.TextFromInvestigateTextOption(OptionVal);
  }

  public get RobotSettings(): iRobotSettings {
    return this._robotsService.RobotSetting;
  }
  public get RobotsList() {
    return this._robotsService.RobotsList;
  }

  RobotChanged(event: Event) {
    let name = (event.target as HTMLOptionElement).value;
    this._robotsService.RobotSetting.robotName = name;
  }

  SaveCurrentResult() {
    if (!this.CurrentResult) return;
    const repHTML = this._robotsService.InvestigateReportHTML(this.CurrentResult);
    const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(repHTML, this._baseUrl, "Investigate (with GPT) report"));
    //console.log("Savign report:", dataURI)
    saveAs(dataURI, "Investigate (with GPT) report.html");
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
  public get WhatToSubmitPartialText(): string {
    if (this.selectedRobotInvestigateTextOption == "info") return "info box";
    else return "highlighted text";
  }

  CanWrite(): boolean {
    return this._reviewerIdentityServ.HasWriteRights;
  }

 
  ngOnDestroy() {

  }
  ngAfterViewInit() {

  }

  public get CannotRunRobotInvestigate(): boolean { // this feels a bit backwards, as it returns TRUE to disable the control
    if (!this.HasWriteRights) return true;
    else if (!this._reviewInfoService.ReviewInfo.canUseRobots) return true;
    else if (this.selectedAttributeDropDown1 && this.queryInputForRobot.trim().length > 15 && this.busyInvestigating == false) {
      if (this.selectedRobotInvestigateTextOption == 'title') return false;
      else if (this.selectedRobotInvestigateTextOption != 'title' && this.selectedAttributeDropDown2) return false;
    }
    return true;
  }

  public async RunRobotInvestigateCommand() {
    if (this.selectedRobotInvestigateTextOption == 'title') {
      this.sampleSize = Math.min(this.sampleSize, 150)
    };
    let cmd: iRobotInvestigate = {
      robotName: this._robotsService.RobotSetting.robotName,
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
      this.CurrentIndexInResults = this._robotsService.RobotInvestigateResults.length - 1;
    }
    this.busyInvestigating = false;
  }
  BackToMain() {
    this.router.navigate(['Main']);
  }
}

