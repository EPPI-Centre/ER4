import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter, ElementRef, ChangeDetectorRef } from '@angular/core';
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
import {
  iRobotOpenAiQueueBatchJobEvaluationCommand, iRobotCoderReadOnly, iRobotSettings, RobotsService,
  iRobotOpenAiCancelQueuedBatchJobEvaluationCommand, iRobotOpenAiPromptEvaluation, RobotOpenAiPromptEvaluationData,
  ConfusionMatrixSummary, ConfusionMatrixRow, AttributeLookup
} from '../services/Robots.service';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';
import 'hammerjs';
import { NgModel } from '@angular/forms';
import { Helpers } from '../helpers/HelperMethods';
import { NONE_TYPE } from '@angular/compiler';
import { Criteria, ItemListService } from '../services/ItemList.service';


@Component({
  selector: 'LlmPromptEvaluation',
  templateUrl: './LlmPromptEvaluation.component.html',
  providers: []
})

export class LlmPromptEvaluation implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private confirmationDialogService: ConfirmationDialogService,
    private _eventEmitterService: EventEmitterService,
    private notificationService: NotificationService,
    private modalService: ModalService,
    private _robotsService: RobotsService,
    public reviewSetsService: ReviewSetsService,
    private ReviewerIdentityServ: ReviewerIdentityService,
    private reviewInfoService: ReviewInfoService,
    private itemListService: ItemListService
  ) { }

  @Output() PleaseCloseMe = new EventEmitter();

  public selectedCodeSet: ReviewSet = new ReviewSet();
  public n_iterations: number = 3;
  public n_in_train_set: number = 50;
  public SelectedGoldStandardEvaluationAttribute: number = 0;
  public SelectedGoldStandardTrainTestAttribute: number = 0;
  public SelectedTrainTestBelowHereAttribute: number = 0;
  public showManualModalRobotOptions: boolean = false;
  public showManualModalUncompleteWarning: boolean = false;
  public showRobotDetails = true;

  public CreateTrainTestSplitsSection: boolean = false;



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
  OpenCreateTrainTestSplitsSection() {
    this.CreateTrainTestSplitsSection = this.CreateTrainTestSplitsSection;
  }

  @ViewChild('VisualiseChart')
  private VisualiseChart!: ChartComponent;
  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;

  CanWrite(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
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
  public currentSelectedEvaluationCodeSetName: string = '';
  public currentSelectedEvaluationDateRun: Date | null = null;
  public currentSelectedEvaluationTitle: string = '';
  public currentSelectedEvaluationContactName: string = '';
  public currentSelectedEvaluationRobotName: string = '';
  public currentSelectedEvaluationNCodes: string = '';
  public currentSelectedEvaluationNRecords: string = '';
  public currentSelectedEvaluationNIterations: string = '';

  
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

  public get RobotOpenAiPromptEvaluationList(): iRobotOpenAiPromptEvaluation[] {
    return this._robotsService.RobotOpenAiPromptEvaluationList;
  }
  refreshRobotOpenAiPromptEvaluationList() {
    this._robotsService.FetchRobotOpenAiPromptEvaluationList();
  }

  public get RobotOpenAiPromptEvaluationDataList(): RobotOpenAiPromptEvaluationData[] {
    return this._robotsService.CurrentRobotOpenAiPromptEvaluationDataList;
  }

  showEvaluation(item: iRobotOpenAiPromptEvaluation) {
    this.currentSelectedEvaluationCodeSetName = this.getCodeSetName(item);
    this.currentSelectedEvaluationDateRun = item.whenRun;
    this.currentSelectedEvaluationTitle = item.title;
    this.currentSelectedEvaluationContactName = item.contactName;
    this.currentSelectedEvaluationRobotName = item.robotName;
    this.currentSelectedEvaluationNCodes = item.nCodes.toString();
    this.currentSelectedEvaluationNRecords = item.nRecords.toString();
    this.currentSelectedEvaluationNIterations = item.nIterations.toString();
    this._robotsService.FetchRobotOpenAiPromptEvaluationDataList(item);
  }

  public get metrics(): ConfusionMatrixSummary[] {
    return this._robotsService.metrics;
  }
  public get uniqueAttributeIds(): number[] {
    return this._robotsService.uniqueAttributeIds;
  }
  public getRowsForAttribute(attributeId: number): ConfusionMatrixRow[] { 
    return this._robotsService.getRowsForAttribute(attributeId);
  }
  public getMetricsForAttribute(attributeId: number): ConfusionMatrixSummary | undefined {
    return this._robotsService.getMetricsForAttribute(attributeId);
  }
  getAttributeName(attributeId: number): string {
    return this._robotsService.getAttributeName(this._robotsService.attributeLookup, attributeId);
  }  
  public getCodeSetName(item: iRobotOpenAiPromptEvaluation): string {
    let html = item.reviewSetHtml;
    const start = html.indexOf("<h2>") + 4;
    const end = html.indexOf("</h2>");
    return html.substring(start, end);
  }
  public listItems(thisRow: ConfusionMatrixRow, trueOrFalseHuman: boolean) {
    if (!this._robotsService.CurrentRobotOpenAiPromptEvaluation) return;
    const r = thisRow.attributeId;
    let cr: Criteria = new Criteria();
    cr.showDeleted = false;
    cr.pageNumber = 0;
    cr.attributeid = r;
    cr.openAiPromptEvaluationId = this._robotsService.CurrentRobotOpenAiPromptEvaluation.openAiPromptEvaluationId;
    cr.llmTrue = thisRow.llmClassification;
    cr.goldTrue = thisRow.llm_human_true > 0;
    cr.listType = "RobotOpenAIPromptEvaluationResults";
    this.itemListService.FetchWithCrit(cr, "LLM eval list");
    this._eventEmitterService.criteriaMAGChange.emit("");
    // grab: attributeId, trueOrFalseHuman, and the LLM true / false from the first field in the row
    // then put this information through the system into the searchlist object.

  }
  public DownloadSelectedEvaluationCodingTool() {
    if (this._robotsService.CurrentRobotOpenAiPromptEvaluation == null || this._robotsService.CurrentRobotOpenAiPromptEvaluation.reviewSetHtml == "") return;
    const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(this._robotsService.CurrentRobotOpenAiPromptEvaluation.reviewSetHtml, this._baseUrl, "Coding Tool Printout"));
    //console.log("Savign report:", dataURI)
    saveAs(dataURI, "Coding Tool printout.html");
  }
  
  public confirmDeleteEvaluation(item: iRobotOpenAiPromptEvaluation) {
    this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to delete this evaluation?', false, '')
      .then(
        (confirmed: any) => {
          //console.log('User confirmed:', confirmed);
          if (confirmed) {
            this.deleteEvaluation(item);
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
  }

  deleteEvaluation(item: iRobotOpenAiPromptEvaluation) {
    this._robotsService.DeleteRobotOpenAiPromptEvaluation(item.openAiPromptEvaluationId);
  }

  public get HasWriteRights(): boolean {

    return this.ReviewerIdentityServ.HasWriteRights;
  }
  public get SimulationNameAlreadyExists() {
    for (let evaluation of this._robotsService.RobotOpenAiPromptEvaluationList) {
      if (evaluation.title.toLowerCase() == this.evaluationNameText.toLowerCase()) {
        return true;
      }
    }
    return false;
  }
  private readonly pattern = /^[A-Za-z0-9\-_ ]+$/;
  public get evaluationNameIsInvalid(): number {
    if (this.evaluationNameText.length == 0) return 1;
    else if (this.evaluationNameText.length < 4) return 2;
    else if (this.pattern.test(this.evaluationNameText) == false) return 3;
    else if (this.SimulationNameAlreadyExists) return 4;
    return 0;
  }
  public get CanRunOpenAIrobot(): boolean {
    if (!this.HasWriteRights) return false;
    if (!this.reviewInfoService.ReviewInfo.canUseRobots) return false;
    if (this.selectedCodeSet == null) return false;
    if (this.SelectedGoldStandardEvaluationAttribute == null) return false;
    if (this.RobotSettings.robotName == "") return false;
    if (this.evaluationNameIsInvalid) return false;
    return true;
  }

  public get SelectedRobot(): iRobotCoderReadOnly | null {
    if (this.RobotSettings.robotName == "") return null;
    const selected = this.RobotsList.find(f => f.robotName == this.RobotSettings.robotName);
    if (!selected) return null;
    return selected;
  }
  public openConfirmationDialogPromptEvaluation() {
    this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to run the evaluation with these codes?', false, '')
      .then(
        (confirmed: any) => {
          //console.log('User confirmed:', confirmed);
          if (confirmed) {
            this.ActuallyRunRobotOpenAICommandEvaluation();
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
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
