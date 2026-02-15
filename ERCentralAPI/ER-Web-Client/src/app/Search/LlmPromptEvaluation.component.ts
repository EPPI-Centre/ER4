import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ModalService } from '../services/modal.service';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import { ClassifierService, PriorityScreeningSimulation } from '../services/classifier.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ReviewSetsService, ReviewSet, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ChartComponent } from '@progress/kendo-angular-charts';
//import { iRobotOpenAICommand, RobotsService, iRobotSettings } from '../services/Robots.service';
import { iRobotOpenAiQueueBatchJobEvaluationCommand, iRobotCoderReadOnly, iRobotSettings, RobotsService, iRobotOpenAiCancelQueuedBatchJobEvaluationCommand, RobotOpenAiPromptEvaluation, RobotOpenAiPromptEvaluationData } from '../services/Robots.service';
import { saveAs } from '@progress/kendo-file-saver';
import 'hammerjs';
import { NgModel } from '@angular/forms';
import { Helpers } from '../helpers/HelperMethods';


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
    public reviewSetsService: ReviewSetsService,
    private ReviewerIdentityServ: ReviewerIdentityService,
    private reviewInfoService: ReviewInfoService,
  ) { }

  @Output() PleaseCloseMe = new EventEmitter();

  public selectedCodeSet: ReviewSet = new ReviewSet();
  public n_iterations: number = 5;
  public n_in_train_set: number = 100;
  public SelectedGoldStandardEvaluationAttribute: number = 0;
  public SelectedGoldStandardTrainTestAttribute: number = 0;
  public SelectedTrainTestBelowHereAttribute: number = 0;
  public showManualModalRobotOptions: boolean = false;
  public showManualModalUncompleteWarning: boolean = false;
  public showRobotDetails = true;



  public get AllCodeSets(): ReviewSet[] {

    return this.reviewSetsService.ReviewSets;
  }
  FormatDate(DateSt: string): string {
    if (DateSt == "0001-01-01T00:00:00") return "None";
    return Helpers.FormatDate2(DateSt);
  }

  @ViewChild('evaluationNameInput') evaluationNameInput!: NgModel;
  ngOnInit() {
    if (this.reviewSetsService.ReviewSets.length == 0) this.reviewSetsService.GetReviewSets();
    if (this._robotsService.RobotsList.length == 0) this._robotsService.GetRobotsList();
    this.refreshRobotOpenAiPromptEvaluationList();
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
    // alert(JSON.stringify(node));
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
      this.selectedModelDropDown3 = "";
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

  public get RobotOpenAiPromptEvaluationList(): RobotOpenAiPromptEvaluation[] {
    return this._robotsService.RobotOpenAiPromptEvaluationList;
  }
  refreshRobotOpenAiPromptEvaluationList() {
    this._robotsService.FetchRobotOpenAiPromptEvaluationList();
  }

  public get RobotOpenAiPromptEvaluationDataList(): RobotOpenAiPromptEvaluationData[] {
    return this._robotsService.CurrentRobotOpenAiPromptEvaluationDataList;
  }

  showEvaluation(item: RobotOpenAiPromptEvaluation) {
    this._robotsService.FetchRobotOpenAiPromptEvaluationDataList(item.openAiPromptEvaluationId);
  }

  confirmDeleteEvaluation(item: RobotOpenAiPromptEvaluation) {

  }

  public get HasWriteRights(): boolean {

    return this.ReviewerIdentityServ.HasWriteRights;
  }

  public get CanRunOpenAIrobot(): boolean {
    if (!this.HasWriteRights) return false;
    else if (!this.reviewInfoService.ReviewInfo.canUseRobots) return false;
    return true;
  }

  public get SelectedRobot(): iRobotCoderReadOnly | null {
    if (this.RobotSettings.robotName == "") return null;
    const selected = this.RobotsList.find(f => f.robotName == this.RobotSettings.robotName);
    if (!selected) return null;
    return selected;
  }

  public RunRobotOpenAICommandEvaluation() {
    this.ActuallyRunRobotOpenAICommandEvaluation();
  }

  public async ActuallyRunRobotOpenAICommandEvaluation() {
    let rname = this.RobotSettings.robotName;

    const node = this.reviewSetsService.selectedNode as ReviewSet;

    let data: iRobotOpenAiQueueBatchJobEvaluationCommand = {
      evaluationName: this.evaluationNameText,
      robotName: this.RobotSettings.robotName,
      reviewSetId: this.selectedCodeSet.reviewSetId,
      reviewSetHtml: this.selectedCodeSet.printHtml(false, false, true),
      goldStandardAttributeId: this.SelectedGoldStandardEvaluationAttribute,
      useFullTextDocument: this.RobotSettings.useFullTextDocument,
      returnMessage: "",
      nIterations: this.n_iterations
    };
    this._robotsService.EnqueueRobotOpenAIBatchJobEvaluationCommand(data).then(
      (res: boolean) => {
        if (res) {
          this.notificationService.show({
            content: "Your Batch Coding request has been Queued.",
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            hideAfter: 4000
          });
          this._robotsService.GetCurrentQueue();
          //this.ShowQueue = true;
        }
      }

    );
  }

  public RobotDDData: Array<any> = [
    {
      text: 'LLM coding options...',
      click: () => {
        this.showManualModalRobotOptions = true;
      }
    }
  ];
  



}
