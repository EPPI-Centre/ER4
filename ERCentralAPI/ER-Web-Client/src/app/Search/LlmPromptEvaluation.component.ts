import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import { ClassifierService, PriorityScreeningSimulation } from '../services/classifier.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ReviewSetsService, ReviewSet, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ChartComponent } from '@progress/kendo-angular-charts';
import { iRobotSettings, RobotsService } from '../services/Robots.service';
import { saveAs } from '@progress/kendo-file-saver';
import 'hammerjs';
import { NgModel } from '@angular/forms';

@Component({
  selector: 'LlmPromptEvaluation',
  templateUrl: './LlmPromptEvaluation.component.html',
  providers: []
})

export class LlmPromptEvaluation implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private classifierService: ClassifierService,
    private _reviewerIdentityServ: ReviewerIdentityService,
    private confirmationDialogService: ConfirmationDialogService,
    private _eventEmitterService: EventEmitterService,
    private notificationService: NotificationService,
    private modalService: ModalService,
    private _robotsService: RobotsService,
    public reviewSetsService: ReviewSetsService
  ) { }

  @Output() PleaseCloseMe = new EventEmitter();

  public selectedCodeSet: ReviewSet = new ReviewSet();
  public n_iterations: number = 5;
  public n_in_train_set: number = 100;
  public SelectedGoldStandardEvaluationAttribute: number = 0;
  public SelectedGoldStandardTrainTestAttribute: number = 0;
  public SelectedTrainTestBelowHereAttribute: number = 0;

  public get AllCodeSets(): ReviewSet[] {

    return this.reviewSetsService.ReviewSets;
  }

  @ViewChild('evaluationNameInput') evaluationNameInput!: NgModel;
  ngOnInit() {
    if (this.reviewSetsService.ReviewSets.length == 0) this.reviewSetsService.GetReviewSets();
    if (this._robotsService.RobotsList.length == 0) this._robotsService.GetRobotsList();

  }

  DropdownSelectCodingToolPromptEvaluation() {
    //this.BulkDeleteCodingCommand = this.GetNewBulkDeleteCodingCommand();
    //this.showMessage = false;
  }




  @ViewChild('VisualiseChart')
  private VisualiseChart!: ChartComponent;
  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;

  CanWrite(): boolean {
    if (this.ClassifierServiceIsBusy) return false;
    return this._reviewerIdentityServ.HasWriteRights;
  }
  public get ClassifierServiceIsBusy(): boolean {
    return this.classifierService.IsBusy;
  }

  public get nodeSelected(): singleNode | null | undefined {
    return this._eventEmitterService.nodeSelected;
  }
  SetSelectedGoldStandardEvaluationAttribute(node: singleNode | null | undefined) {
    //alert(JSON.stringify(node));
    if (node != null && node.nodeType == "SetAttribute") {
      let a = node as SetAttribute;
      this.selectedModelDropDown1 = node.name;
      this.SelectedGoldStandardEvaluationAttribute = a.attribute_id;
      this._eventEmitterService.nodeSelected = undefined;
    }
    else {
      this.selectedModelDropDown1 = "";
      this.SelectedGoldStandardEvaluationAttribute = 0;
    }
  }
  SetSelectedGoldStandardTrainTestAttribute(node: singleNode | null | undefined) {
    //alert(JSON.stringify(node));
    if (node != null && node != undefined && node.nodeType == "SetAttribute") {
      let a = node as SetAttribute;
      this.selectedModelDropDown2 = node.name;
      this.SelectedGoldStandardTrainTestAttribute = a.attribute_id;
      this._eventEmitterService.nodeSelected = undefined;
    }
    else {
      this.selectedModelDropDown2 = "";
      this.SelectedGoldStandardTrainTestAttribute = 0;
    }
  }
  SetSelectedTrainTestBelowHereAttribute(node: singleNode | null | undefined) {
    //alert(JSON.stringify(node));
    if (node != null && node != undefined && node.nodeType == "SetAttribute") {
      let a = node as SetAttribute;
      this.selectedModelDropDown3 = node.name;
      this.SelectedTrainTestBelowHereAttribute = a.attribute_id;
      this._eventEmitterService.nodeSelected = undefined;
    }
    else {
      this.selectedModelDropDown2 = "";
      this.SelectedTrainTestBelowHereAttribute = 0;
    }
  }
  public isCollapsedSetSelectedGoldStandardEvaluationAttribute: boolean = false;
  public isCollapsed2SetSelectedGoldStandardEvaluationAttribute: boolean = false;
  public isCollapsedSetSelectedGoldStandardTrainTestAttribute: boolean = false;
  public isCollapsed2SetSelectedGoldStandardTrainTestAttribute: boolean = false;
  public isCollapsedSetSelectedTrainTestBelowHereAttribute: boolean = false;
  public isCollapsed2SetSelectedTrainTestBelowHereAttribute: boolean = false;
  public selectedModelDropDown1: string = '';
  public selectedModelDropDown2: string = '';
  public selectedModelDropDown3: string = '';
  
  public evaluationNameText: string = '';

  CloseBMDropDown1() {

    this.isCollapsedSetSelectedGoldStandardEvaluationAttribute = false;
  }
  CloseBMDropDown2() {

    this.isCollapsedSetSelectedGoldStandardTrainTestAttribute = false;
  }
  CloseBMDropDown3() {

    this.isCollapsedSetSelectedTrainTestBelowHereAttribute = false;
  }
  RobotChanged(event: Event) {
    let name = (event.target as HTMLOptionElement).value;
    this._robotsService.RobotSetting.robotName = name;
  }
  public get RobotSettings(): iRobotSettings {
    return this._robotsService.RobotSetting;
  }
  public get RobotsList() {
    return this._robotsService.RobotsList;
  }

  ngOnDestroy() {

  }

  BackToMain() {
    this.router.navigate(['Main']);
  }
}
