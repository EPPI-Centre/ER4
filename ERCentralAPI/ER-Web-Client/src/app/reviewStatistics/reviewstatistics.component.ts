import { Component, Inject, OnInit, ViewChild, OnDestroy, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Criteria, ItemList } from '../services/ItemList.service';
import { ItemListService, StringKeyValue } from '../services/ItemList.service';
import { ReviewSetsService, ReviewSet, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { CodesetStatisticsService, ReviewStatisticsCountsCommand, StatsCompletion, StatsByReviewer, BulkCompleteUncompleteCommand, ReviewStatisticsCodeSet2, iReviewStatisticsReviewer2, iReviewStatisticsCodeSet2 } from '../services/codesetstatistics.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { encodeBase64, saveAs } from '@progress/kendo-file-saver';
import { ConfigurableReportService, iReportAllCodingCommand } from '../services/configurablereport.service';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import { Helpers } from '../helpers/HelperMethods';
import {
  Workbook, WorkbookSheetColumn, WorkbookSheetRow, WorkbookSheetRowCell, WorkbookSheet
  , WorkbookSheetRowCellBorderBottom, WorkbookSheetRowCellBorderLeft, WorkbookSheetRowCellBorderRight, WorkbookSheetRowCellBorderTop
} from "@progress/kendo-ooxml";
import { OutcomesService } from '../services/outcomes.service';


@Component({
  selector: 'reviewStatisticsComp',
  templateUrl: './reviewstatistics.component.html',
  providers: []
})

export class ReviewStatisticsComp implements OnInit, OnDestroy {
  constructor(private router: Router,
    public ReviewerIdentityServ: ReviewerIdentityService,
    private reviewSetsService: ReviewSetsService,
    @Inject('BASE_URL') private _baseUrl: string,
    private _httpC: HttpClient,
    private ItemListService: ItemListService,
    public codesetStatsServ: CodesetStatisticsService,
    private confirmationDialogService: ConfirmationDialogService,
    private _reviewInfoService: ReviewInfoService,
    private _notificationService: NotificationService,
    private _OutcomesService: OutcomesService,
    private configurablereportServ: ConfigurableReportService
  ) {

  }

  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;
  //fas = fas;
  //faSpin = faSpin;
  @ViewChild('CodeStudiesTreeOne') CodeStudiesTreeOne!: codesetSelectorComponent;
  @Output() tabSelectEvent = new EventEmitter();

  public stats: ReviewStatisticsCountsCommand | null = null;
  public countDown: any | undefined;
  public count: number = 60;
  public DetailsForSetId: number = 0;
  public isReviewPanelCollapsed = false;
  public isWorkAllocationsPanelCollapsed = false;
  public msg: string = '';
  public PreviewMsg: string = '';
  public canBulkComplete: boolean = false;
  public isBulkCompleting: boolean = false;
  public showMessage: boolean = false;
  public showPreviewMessage: boolean = true;
  public DropdownSelectedCodeStudies: singleNode | null = null;
  public isCollapsedCodeStudies: boolean = false;
  public selectedCodeSet: ReviewSet = new ReviewSet();
  public PanelName: string = '';
  public complete: string = '';
  public selectedReviewer1: Contact = new Contact();
  //public ImportOrNewDDData: Array<any> = [{
  //	text: 'New Reference',
  //	click: () => {
  //		this.NewReference();
  //	}
  //}];

  //dtOptions: DataTables.Settings = {};
  //dtTrigger: Subject<any> = new Subject();

  public get Contacts(): Contact[] {

    return this._reviewInfoService.Contacts;
  }
  public get CodeSets(): ReviewSet[] {

    return this.reviewSetsService.ReviewSets.filter(x => x.setType.allowComparison != false);
  }
  public get IsServiceBusy(): boolean {
    return this.codesetStatsServ.IsBusy;
  }
  public get IsReportsServiceBusy(): boolean {
    return this.configurablereportServ.IsBusy || this.saving;
  }
  public get ScreeningSets(): ReviewStatisticsCodeSet2[] {
    return this.codesetStatsServ.CodingProgressStats.filter(found => found.subTypeName == 'Screening');
  }
  public get StandardSets(): ReviewStatisticsCodeSet2[] {
    return this.codesetStatsServ.CodingProgressStats.filter(found => found.subTypeName == 'Standard');
  }
  public get AdminSets(): ReviewStatisticsCodeSet2[] {
    return this.codesetStatsServ.CodingProgressStats.filter(found => found.subTypeName == 'Administration');
  }

  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }
  public get HasAdminRights(): boolean {
    return this.ReviewerIdentityServ.HasAdminRights;
  }
  public get HasReviewStats(): boolean {
    return this.codesetStatsServ.ReviewStats.itemsIncluded != -1;
  }
  public get ReviewIsMagEnabled(): boolean {
    if (this._reviewInfoService.ReviewInfo.magEnabled
      //&& this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin
    ) return true;
    return false;
  }
  public get ReviewIsZoteroEnabled(): boolean {
    //if (this._reviewInfoService.ReviewInfo.magEnabled
    //	//&& this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin
    //) return true;
    return true;
  }
  public get HasSkippedFullStats(): boolean {
    if (this.reviewSetsService.ReviewSets.length == 0) return false;
    else return this.codesetStatsServ.SkippedFullStats;
  }
  public get WillNotAutoRefreshCodingStats(): boolean {
    return this.codesetStatsServ.WouldSkipFullStats;
  }
  public get AllCodingReportOptions() {
    return this.configurablereportServ.reportAllCodingCommandOptions;
  }
  private _showAllCodingReportOptions: boolean = false;
  public get showAllCodingReportOptions(): boolean {
    if (this.DetailsForSetId) return this._showAllCodingReportOptions;
    else return false;
  }
  public set showAllCodingReportOptions(val: boolean) {
    this._showAllCodingReportOptions = val;
  }
  public get showAllCodingReportOptionsText(): string {
    if (this._showAllCodingReportOptions) return "Close Excel Options";
    else return "More...";
  }
  ngOnInit() {

    console.log('inititating stats');

    //this.subOpeningReview = this.ReviewerIdentityServ.OpeningNewReview.subscribe(() => this.Reload());
    //this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(

    //	() => {
    //		console.log('gettign the stats');
    //		this.GetStats()
    //	}
    //		);
    //if (this.codesetStatsServ.ReviewStats.itemsIncluded == -1
    //	|| (this.reviewSetsService.ReviewSets.length > 0 && this.codesetStatsServ.tmpCodesets.length == 0)
    //) this.Reload();
  }
  public ImportOrNewDDData: Array<any> = [{
    text: 'New Reference',
    click: () => {
      this.NewReference();
    }
  },
  {
    text: 'Manage Sources',
    click: () => {
      this.GoToManageSources();
    }
  }
  ];

  public CodingToolsDDData: Array<any> = [{
    text: 'Import Coding Tools',
    click: () => {
      this.ImportCodesetClick();
    }
  }];
  ShowDetailsForSetId(SetId: number) {
    if (this.DetailsForSetId == SetId) this.DetailsForSetId = 0;
    else this.DetailsForSetId = SetId;
  }
  RefreshStats() {
    //this.reviewSetsService.GetReviewStatsEmit.emit();
    this.codesetStatsServ.GetReviewStatisticsCountsCommand(true, true);
  }
  Reload() {
    this.Clear();
  }
  GetStats() {
    //console.log('getting stats...');
    //this.codesetStatsServ.GetReviewStatisticsCountsCommand();
    //this.codesetStatsServ.GetReviewSetsCodingCounts(true, this.dtTrigger);
  }
  Clear() {
    this.ItemListService.SaveItems(new ItemList(), new Criteria());
    this.reviewSetsService.Clear();

  }
  EditCodeSets() {
    this.router.navigate(['EditCodeSets']);
  }
  ImportCodesetClick() {
    this.router.navigate(['ImportCodesets']);
  }
  GoToSources() {
    this.router.navigate(['sources']);
  }
  GoToManageSources() {
    this.router.navigate(['sources'], { queryParams: { tabby: 'ManageSources' } });
  }
  GoToDuplicates() {
    this.router.navigate(['Duplicates']);
  }
  GoToMetaAnalysis() {
    this.router.navigate(['MetaAnalysis']);
  }
  public OpenMAG() {
    this.router.navigate(['MAG']);
  }
  public OpenZotero() {
    this.router.navigate(['Zotero']);
  }
  IncludedItemsList() {
    this.ItemListService.GetIncludedItems();
    this.tabSelectEvent.emit();
    //this.tabset.select('ItemListTab');
  }
  ExcludedItemsList() {
    this.ItemListService.GetExcludedItems();
    this.tabSelectEvent.emit();
    //this.tabset.select('ItemListTab');
  }
  DeletedItemsList() {
    this.ItemListService.GetDeletedItems();
    this.tabSelectEvent.emit();
    //this.tabset.select('ItemListTab');
  }
  CompletedBySetAndContact(statsByContact: iReviewStatisticsReviewer2, setName: string) {
    let cri: Criteria = new Criteria();
    cri.contactId = statsByContact.contactId;
    cri.setId = statsByContact.setId;
    cri.pageSize = this.ItemListService.ListCriteria.pageSize;
    cri.listType = "ReviewerCodingCompleted";
    this.ItemListService.FetchWithCrit(cri, statsByContact.contactName + ": documents with completed coding using '" + setName + "'");
    this.tabSelectEvent.emit();
  }
  IncompleteBySetAndContact(statsByContact: iReviewStatisticsReviewer2, setName: string) {
    let cri: Criteria = new Criteria();
    cri.contactId = statsByContact.contactId;
    cri.setId = statsByContact.setId;
    cri.pageSize = this.ItemListService.ListCriteria.pageSize;
    cri.listType = "ReviewerCodingIncomplete";
    this.ItemListService.FetchWithCrit(cri, statsByContact.contactName + ": documents with incomplete (but started) coding using '" + setName + "'");
    this.tabSelectEvent.emit();
  }

  NewReference() {
    this.router.navigate(['EditItem'], { queryParams: { return: 'Main' } });
  }
  CompleteCoding(contactName: string, setName: string, setId: number, contactId: number, completeOrNot: string) {
    if (!this.HasWriteRights) return;
    if (setId != null && contactId != null && completeOrNot != null) {

      let tmpComplete: string = '';
      let tmpStrItemVisible: string = '';
      if (completeOrNot == 'true') {
        tmpComplete = 'Completed';
        tmpStrItemVisible = ' items will be visible in searches and reports.';
      } else {
        tmpComplete = 'Uncompleted';
        tmpStrItemVisible = ' items will no longer be visible in searches and reports.';
      }
      this.confirmationDialogService.confirm(completeOrNot == 'true' ? 'Complete this coding?' : 'Un-complete this coding?', 'Are you sure you want to change the codings by <em>' + contactName + '</em> for the "<em>' + setName + '</em>" coding tool to <b>' + tmpComplete + '</b>?' +
        '<br />' +
        '<br />Please check the (full) manual if you are unsure about the implications.' +
        '<br /><b>' + tmpComplete + '</b> ' + tmpStrItemVisible, false, '', undefined, undefined, 'lg')
        .then(
          (confirmed: any) => {
            console.log('User confirmed:');
            if (confirmed) {

              this.codesetStatsServ.SendToItemBulkCompleteCommand(
                setId,
                contactId,
                completeOrNot);//stats are refreshed here, overriding review-size check.

            } else {
              //alert('did not confirm');
            }
          }
        )
        .catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
    }
  }
  CloseCodeDropDownStudies() {

    if (this.CodeStudiesTreeOne) {

      this.DropdownSelectedCodeStudies = this.CodeStudiesTreeOne.SelectedNodeData;
    }
    this.isCollapsedCodeStudies = false;
    this.DropdownChange();
  }
  public DropdownChange() {
    this.canBulkComplete = false;
    this.showMessage = false;
    this.msg = '';
    this.CanPreview();
  }
  public CanPreview() {
    this.showPreviewMessage = true;
    if (this.complete == 'Complete') {
      this.isBulkCompleting = true;
    } else {
      this.isBulkCompleting = false;
    }
    let compORuncomp: string = this.isBulkCompleting ? "completed" : "un-completed";
    this.PreviewMsg = "Please click \"Preview\" to continue.";

    if (this.selectedCodeSet.name == '') {
      this.PreviewMsg = "Please select the codeset to be " + compORuncomp + ".";
      //console.log(msg);
      return false;
    }
    if (this.isBulkCompleting && this.selectedReviewer1.contactName == ''
      || this.selectedReviewer1.contactName == ' ') {
      this.PreviewMsg = "Please select whose codings should be " + compORuncomp + ".";
      return false;
    }
    if (this.DropdownSelectedCodeStudies == null) {
      this.PreviewMsg = "Please select the code used to specify what items are to be " + compORuncomp + ".";
      //console.log(msg);
      return false;
    }

    let setId: number = this.selectedCodeSet.set_id;
    let node: SetAttribute = this.DropdownSelectedCodeStudies as SetAttribute;

    if (node.set_id == setId) {
      this.PreviewMsg = "This can't be done: the selected code belongs to the Codeset you wish to act on. </br> Please select a different Code/Codeset combination.";
      //console.log(msg);
      return false;
    }
    this.showPreviewMessage = false;
    return true;
  }
  public CompleteOrUncomplete() {

    if (this.DropdownSelectedCodeStudies == null || this.DropdownSelectedCodeStudies.name == ''
      || this.selectedCodeSet == null) {
      return;
    }
    let setId: number = this.selectedCodeSet.set_id;
    let node: SetAttribute = this.DropdownSelectedCodeStudies as SetAttribute;
    let attId: number = node.attribute_id;
    let reviewerId: number = this.selectedReviewer1.contactId;
    let apiResult: Promise<BulkCompleteUncompleteCommand> | any;

    if (this.isBulkCompleting) {

      apiResult = this.codesetStatsServ.SendItemsToBulkCompleteOrNotCommand(
        attId,
        this.isBulkCompleting.toString(),
        setId,
        'false',
        reviewerId
      ).then(
        () => {
          this._notificationService.show({
            content: 'finished the bulk complete',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
          });
        }
      );
    }
    else {

      apiResult = this.codesetStatsServ.SendItemsToBulkCompleteOrNotCommand(
        attId,
        this.isBulkCompleting.toString(),
        setId,
        'false'
      ).then(
        () => {
          this._notificationService.show({
            content: 'finished the bulk uncomplete',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
          });
        }
      );
    }
    this.ClearBulkFields();
    this.changePanel('');

  }
  ClearBulkFields() {

    this.selectedReviewer1 = new Contact();
    this.DropdownSelectedCodeStudies = null;
    this.selectedCodeSet = new ReviewSet();
    this.canBulkComplete = false;
    this.CanPreview();
    this.showMessage = false;
    this.showPreviewMessage = true;

  }
  public Preview(isCompleting: string) {

    let completing: string = '';
    if (isCompleting == 'Complete') {

      completing = 'true';
    } else {

      completing = 'false';
    }

    if (this.DropdownSelectedCodeStudies == null || this.DropdownSelectedCodeStudies.name == ''
      || this.selectedCodeSet == null) {
      return;
    }

    let setId: number = this.selectedCodeSet.set_id;
    let node: SetAttribute = this.DropdownSelectedCodeStudies as SetAttribute;
    let attId: number = node.attribute_id;
    let reviewerId: number = this.selectedReviewer1.contactId;
    let apiResult: Promise<BulkCompleteUncompleteCommand> | any;

    if (attId != null) {

      apiResult = this.codesetStatsServ.SendItemsToBulkCompleteOrNotCommand(
        attId,
        this.isBulkCompleting.toString(),
        setId,
        'true',
        reviewerId

      ).then(

        (result: BulkCompleteUncompleteCommand) => {

          this.msg = "Your selected code (" + node.attribute_name + ") is associated with ";
          this.msg += "<b>" + result.potentiallyAffectedItems + " Items. </b>";

          if (result.potentiallyAffectedItems > 0) {

            this.msg += "<br\> Of these, "
              + (result.isCompleting ? "un-completed" : "completed")
              + " codings in the chosen Codeset (\"" + this.selectedCodeSet.set_name + "\") will be "
              + (result.isCompleting ? "completed, if they belong to " + this.selectedReviewer1.contactName : "un-completed");
            + "." + "<br\>";

            this.msg += "<br\> As a result, <b> the coding of ";
            this.msg += result.affectedItems + " Items ";
            this.msg += "will be " + (result.isCompleting ? "completed" : "un-completed") + "</b>.";

            if (result.affectedItems > 0) {
              this.msg += "<br\>" + "If this looks ok, you may now press the "
                + (result.isCompleting ? "\"Complete!\"" : "\"Uncomplete!\"") + " button.";
              this.msg += "<br\>" + "<b>Warning: this action does not have a direct \"Undo\" function so please use with care!</b>";

              this.canBulkComplete = true;

            } else {

              this.msg += "<br\>" + "<b>Nothing to be " + (result.isCompleting ? "completed" : "un-completed") + "</b>!";
            }


          } else {

            this.msg += "Nothing to be " + (result.isCompleting ? "completed" : "un-completed") + "!";
          }
          this.showMessage = true;
        });
    }
  }

  public CanCompleteOrNot() {
    return this.canBulkComplete;
  }

  changePanel(completeOrNot: string) {

    this.isBulkCompleting = true;

    if (this.PanelName == '') {

      this.PanelName = 'BulkCompleteSection';

    } else if (this.PanelName == 'BulkCompleteSection' && completeOrNot == 'true' && this.complete == 'Complete') {

      this.PanelName = '';

    } else if (this.PanelName == 'BulkCompleteSection' && completeOrNot == 'true' && this.complete == 'Uncomplete') {

      this.PanelName = 'BulkCompleteSection';

    } else if (this.PanelName == 'BulkCompleteSection' && completeOrNot == 'false' && this.complete == 'Complete') {

      this.PanelName = 'BulkCompleteSection';

    } else if (this.PanelName == 'BulkCompleteSection' && completeOrNot == 'false' && this.complete == 'Uncomplete') {

      this.PanelName = '';
    }
    else {

      this.PanelName = '';
    }
    if (completeOrNot == 'true') {

      this.complete = 'Complete';

    } else {

      this.complete = 'Uncomplete';
    }
    this.ClearBulkFields();
  }


  public async GetAndSaveCodingReport(setStats: ReviewStatisticsCodeSet2) {
    console.log("Asking for the report");
    let stringReport = await this.configurablereportServ.FetchAllCodingReportBySet(setStats.setId);
    if (stringReport != 'error') {
      //console.log("Got the report");
      this.SaveAsHtml(stringReport, setStats.setName);
    }
  }
  public async GetAndSaveJsonCodingReport(setStats: ReviewStatisticsCodeSet2) {
    console.log("Asking for the report");
    let jsonVal = await this.configurablereportServ.FetchAllCodingReportDataBySet(setStats.setId);
    if (jsonVal != false) {
      //console.log("Got the report");
      this.SaveAsJson(JSON.stringify(jsonVal), setStats.setName);
    }
  }
  private saving: boolean = false;
  public SaveAsHtml(reportHTML: string, CodesetName: string) {
    if (reportHTML.length < 1) return;
    this.saving = true;
    const b = new Blob([reportHTML], { type: 'text/html' });
    saveAs(b, CodesetName + " full coding report.html");
    this.saving = false;
  }
  private SaveAsJson(JsonReport: string, CodesetName: string) {
    //console.log("Save as Json, codesets: " + this.ItemCodingService.jsonReport.CodeSets.length + "; refs: " + this.ItemCodingService.jsonReport.References.length);
    if (!JsonReport) {
      console.log("Save as Json. Return (not jsonreport)");
      return;
    }
    this.saving = true;
    console.log("Save as Json. Encoding");
    const dataURI = "data:text/plain;base64," + encodeBase64(JsonReport);
    const blob = Helpers.dataURItoBlob(dataURI);
    console.log("Savign json report...");//, dataURI)
    saveAs(blob, "All coding for " + CodesetName + ".json");
    this.saving = false;
  }

  private readonly borderBottom: WorkbookSheetRowCellBorderBottom = { size: 1 };
  private readonly borderLeft: WorkbookSheetRowCellBorderLeft = { size: 1 };
  private readonly borderTop: WorkbookSheetRowCellBorderTop = { size: 1 };
  private readonly borderRight: WorkbookSheetRowCellBorderRight = { size: 1 };

  public GetAndSaveXLSCodingReport(setStats: ReviewStatisticsCodeSet2) {
    //safety measures, let's check the size of the job...
    let Size: string = "small"; //medium, big, very big, massive
    let FormattedMsg: string = ""; //medium, big, very big, massive
    const ItemsEstimatedCount = setStats.numItemsCompleted + setStats.numItemsIncomplete +
      (Math.min(setStats.numItemsCompleted, setStats.numItemsIncomplete) * setStats.reviewerStatistics.length);
    const CS = this.reviewSetsService.FindSetById(setStats.setId);
    const CodesCount = (CS ? CS.NumberOfChildren : 0);
    const reviewersCount = setStats.reviewerStatistics.length;
    if (CodesCount == 0) return;//shouldn't happen, but we can cut our losses if it does
    let SheetsCount = 3;
    if (this.AllCodingReportOptions.includeArms) SheetsCount += 3;

    let estimatedCells: number = SheetsCount * (this.AllCodingReportOptions.showFullTitle ? 4 : 3) * ItemsEstimatedCount;
    estimatedCells += ItemsEstimatedCount * reviewersCount * CodesCount * SheetsCount;
    if (this.AllCodingReportOptions.includeOutcomes) {
      estimatedCells += (this.AllCodingReportOptions.showFullTitle ? 4 : 3) * ItemsEstimatedCount;
      estimatedCells += 20 * ItemsEstimatedCount;
    }
    if (estimatedCells < 5000) {
      this.DoGetAndSaveXLSCodingReport(setStats, Size);
      return;
    }
    else if (estimatedCells >= 5000 && estimatedCells < 20000) {
      Size = "medium";
      FormattedMsg = "This report is expected to be (relatively) medium in size and should take a few seconds to arrive.<br />Proceed?"
    }
    else if (estimatedCells >= 20000 && estimatedCells < 100000) {
      Size = "big";
      FormattedMsg = "This report is expected to be big in size and may take many seconds to arrive.<br />Proceed?"
    }
    else if (estimatedCells >= 100000 && estimatedCells < 1200000) {
      Size = "very big";
      FormattedMsg = "This report is expected to be <strong>very big</strong> in size and may take minutes to arrive.<br />Proceed?"
    }
    else if (estimatedCells >= 1200000) {
      Size = "massive";
      FormattedMsg = "This report is expected to be <strong>massive</strong> in size and <strong>may take many minutes</strong> to arrive.<br />"
        + "Moreover, <strong>it may crash your browser</strong> by running out of available memory. <br />Proceed?"
    }
    this.confirmationDialogService.confirm("Get full coding Report?"
      , FormattedMsg, false, '').then(
        (confirmed: any) => {
          if (confirmed == true) this.DoGetAndSaveXLSCodingReport(setStats, Size);
        }
    ).catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
  }

  public async DoGetAndSaveXLSCodingReport(setStats: ReviewStatisticsCodeSet2, size: string) {
    console.log("Asking for the report");
    let jsonVal = await this.configurablereportServ.FetchAllCodingReportDataBySet(setStats.setId);
    if (jsonVal != false) {
      this.saving = true;
      this.ProduceXLSreport(jsonVal as iReportAllCodingCommand, setStats);
      //this.saving = false;
    }
  }
  private ProduceXLSreport(jsonData: iReportAllCodingCommand, setStats: ReviewStatisticsCodeSet2) {

    let SheetsToAdd: WorkbookSheet[] = [{
      name: "Codes (whole study)",
      columns: <WorkbookSheetColumn[]>[],
      rows: []
    }, {
      name: "InfoBox (whole study)",
      columns: <WorkbookSheetColumn[]>[],
      rows: []
    }, {
      name: "PDF coding (whole study)",
      columns: <WorkbookSheetColumn[]>[],
      rows: []
      }];

    if (this.AllCodingReportOptions.includeArms) {
      //we need 3 more sheets!
      SheetsToAdd.push({
        name: "Codes (arms)",
        columns: <WorkbookSheetColumn[]>[],
        rows: []
      });
      SheetsToAdd.push({
        name: "InfoBox (arms)",
        columns: <WorkbookSheetColumn[]>[],
        rows: []
      });
      SheetsToAdd.push({
        name: "PDF coding (arms)",
        columns: <WorkbookSheetColumn[]>[],
        rows: []
      });
    }

    if (this.AllCodingReportOptions.includeOutcomes) {
      SheetsToAdd.push({
        name: "Outcomes",
        columns: <WorkbookSheetColumn[]>[],
        rows: []
      });
    }
    const workbook: Workbook = new Workbook({
      sheets: <WorkbookSheet[]>SheetsToAdd
    });

    const ReviewerStats = setStats.reviewerStatistics;
    const NamesCells: WorkbookSheetRowCell[] = [];
    const Reviewers: StringKeyValue[] = [];
    for (let p of ReviewerStats) {
      NamesCells.push({ value: p.contactName, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
      Reviewers.push(new StringKeyValue(p.contactId.toString(), p.contactName));
    }
    let sheets = workbook.options.sheets;
    if (!sheets) return;
    let sheet1 = sheets[0];
    let sheet2 = sheets[1];
    let sheet3 = sheets[2];
    let sheet4: WorkbookSheet = {};
    let sheet5: WorkbookSheet = {};
    let sheet6: WorkbookSheet = {};
    let outcomesSheet: WorkbookSheet = {};

    if (this.AllCodingReportOptions.includeArms) {
      sheet4 = sheets[3];
      sheet5 = sheets[4];
      sheet6 = sheets[5];
    }
    const sheet3ColCount = 4 + (NamesCells.length * jsonData.attributes.length);
    
    for (let i = 0; i < sheet3ColCount; i++) {
      const AutoColumn: WorkbookSheetColumn = { autoWidth: false, width : 70};
      sheet3.columns?.push(AutoColumn);
    }

    let tmpRow = this.BuildFirstRow(jsonData, setStats);
    sheet1.rows?.push(tmpRow);
    sheet2.rows?.push(tmpRow);
    sheet3.rows?.push(tmpRow);
    if (this.AllCodingReportOptions.includeArms) {
      sheet4.rows?.push(tmpRow);
      sheet5.rows?.push(tmpRow);
      sheet6.rows?.push(tmpRow);
    }
    tmpRow = this.BuildSecondRow(jsonData, setStats);
    sheet1.rows?.push(tmpRow);
    sheet2.rows?.push(tmpRow);
    sheet3.rows?.push(tmpRow);
    if (this.AllCodingReportOptions.includeArms) {
      sheet4.rows?.push(tmpRow);
      sheet5.rows?.push(tmpRow);
      sheet6.rows?.push(tmpRow);
    }

    tmpRow = this.BuildThirdRow(jsonData, setStats, NamesCells);
    sheet1.rows?.push(tmpRow);
    sheet2.rows?.push(tmpRow);
    sheet3.rows?.push(tmpRow);
    if (this.AllCodingReportOptions.includeArms) {
      sheet4.rows?.push(tmpRow);
      sheet5.rows?.push(tmpRow);
      sheet6.rows?.push(tmpRow);
    }

    sheet1.frozenColumns = 2;
    sheet1.frozenRows = 3;
    sheet2.frozenColumns = 2;
    sheet2.frozenRows = 3;
    sheet3.frozenColumns = 2;
    sheet3.frozenRows = 3;
    if (this.AllCodingReportOptions.includeArms) {
      sheet4.frozenColumns = 2;
      sheet4.frozenRows = 3;
      sheet5.frozenColumns = 2;
      sheet5.frozenRows = 3;
      sheet6.frozenColumns = 2;
      sheet6.frozenRows = 3;
    }
    
    if (!this.AllCodingReportOptions.includeArms) {//check for how many sheets only once, then do the work "efficiently"
      for (let i = 0; i < jsonData.items.length; i++) {
        sheet1.rows?.push(
          this.BuildWholeStudyDataRow(jsonData, setStats, i, Reviewers)
        );
        sheet2.rows?.push(
          this.BuildWholeStudyInfoBoxRow(jsonData, setStats, i, Reviewers)
        );
        sheet3.rows?.push(
          this.BuildWholeStudyPDFRow(jsonData, setStats, i, Reviewers)
        );
      }
    }
    else {
      for (let i = 0; i < jsonData.items.length; i++) {
        sheet1.rows?.push(
          this.BuildWholeStudyDataRow(jsonData, setStats, i, Reviewers)
        );
        sheet2.rows?.push(
          this.BuildWholeStudyInfoBoxRow(jsonData, setStats, i, Reviewers)
        );
        sheet3.rows?.push(
          this.BuildWholeStudyPDFRow(jsonData, setStats, i, Reviewers)
        );
        sheet4.rows?.push(
          this.BuildArmsDataRow(jsonData, setStats, i, Reviewers)
        );
        sheet5.rows?.push(
          this.BuildArmsInfoBoxDataRow(jsonData, setStats, i, Reviewers)
        );
        sheet6.rows?.push(
          this.BuildArmsPDFDataRow(jsonData, setStats, i, Reviewers)
        ); 
      }
    }
    if (workbook && workbook.options && workbook.options.sheets && workbook.options.sheets[2] && workbook.options.sheets[2].columns) {
      for (let i = 3; i < workbook.options.sheets[2].columns.length; i++) {
        if (workbook
          && workbook.options
          && workbook.options.sheets
          && workbook.options.sheets[2]
          && workbook.options.sheets[2].rows
          && workbook.options.sheets[2].rows[2]
          && workbook.options.sheets[2].rows[2].cells
          && workbook.options.sheets[2].rows[2].cells[i])
          if (workbook.options.sheets[2].rows[2].cells[i].value != undefined) {
            const val = workbook.options.sheets[2].rows[2].cells[i].value;
            if (val) workbook.options.sheets[2].columns[i].width = val.toString().length * 8.2;
          }
      }
    }
    if (this.AllCodingReportOptions.includeOutcomes) {
      outcomesSheet = sheets[sheets.length - 1];
      outcomesSheet.frozenRows = 1;
      outcomesSheet.frozenColumns = 3;
      this.BuildOutcomesSheet(jsonData, outcomesSheet);
    }
    workbook.toDataURL().then((dataUrl) => {
      saveAs(dataUrl, "All coding for " + setStats.setName + ".xlsx");
      this.saving = false;
    });
  }

  private BuildFirstRow(jsonData: iReportAllCodingCommand, setStats: ReviewStatisticsCodeSet2): WorkbookSheetRow {
    const cell1: WorkbookSheetRowCell = { value: "ITEM_ID", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const cell2: WorkbookSheetRowCell = { value: "Short Title", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const cell3: WorkbookSheetRowCell = { value: "Full title", wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const cell4: WorkbookSheetRowCell = { value: "I/E/D/S flag", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const res: WorkbookSheetRow = { cells: [cell1, cell2] };
    if (this.AllCodingReportOptions.showFullTitle) res.cells?.push(cell3);
    res.cells?.push(cell4);
    const NofReviewer = setStats.reviewerStatistics.length;
    for (let code of jsonData.attributes) {
      res.cells?.push({ value: code.attName, colSpan: NofReviewer, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    }
    return res;
  }

  private BuildSecondRow(jsonData: iReportAllCodingCommand, setStats: ReviewStatisticsCodeSet2): WorkbookSheetRow {
    let cSpan: number = 3;
    if (this.AllCodingReportOptions.showFullTitle) cSpan = 4;
    const cell1: WorkbookSheetRowCell = { value: "Full code path", colSpan: cSpan, textAlign: "right", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const res: WorkbookSheetRow = { cells: [cell1] };
    const NofReviewer = setStats.reviewerStatistics.length;
    for (let code of jsonData.attributes) {
      res.cells?.push({ value: code.fullPath, colSpan: NofReviewer, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    }
    return res;
  }

  private BuildThirdRow(jsonData: iReportAllCodingCommand, setStats: ReviewStatisticsCodeSet2, NamesCells: WorkbookSheetRowCell[]): WorkbookSheetRow {
    let cSpan: number = 3;
    if (this.AllCodingReportOptions.showFullTitle) cSpan = 4;
    const cell1: WorkbookSheetRowCell = { value: "Reviewers:", colSpan: cSpan, textAlign: "right", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const res: WorkbookSheetRow = { cells: [cell1] };
    
    for (let code of jsonData.attributes) {
      res.cells?.push(...NamesCells);//spread operator! https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Spread_syntax
    }
    return res;
  }

  private BuildWholeStudyDataRow(jsonData: iReportAllCodingCommand, setStats: ReviewStatisticsCodeSet2, index: number, reviewers: StringKeyValue[]): WorkbookSheetRow {
    const res: WorkbookSheetRow = { cells: [] };
    const item = jsonData.items[index];
    let CompCodingVal: string | number = this.AllCodingReportOptions.labelForCompletedCoding;
    let IncompCodingVal: string | number = this.AllCodingReportOptions.labelForIncompleteCoding;
    let NoCodingVal: string | number = this.AllCodingReportOptions.labelForNoCoding;
    if (this.AllCodingReportOptions.saveLabelsAsNumbers) {
      CompCodingVal = +CompCodingVal;
      IncompCodingVal = +IncompCodingVal;
      NoCodingVal = +NoCodingVal;
    }
    res.cells?.push({ value: item.itemId });
    res.cells?.push({ value: item.shortTitle });
    if (this.AllCodingReportOptions.showFullTitle) res.cells?.push({ value: item.title });
    res.cells?.push({ value: item.state });

    for (let code of jsonData.attributes) {
      const CodesFound = jsonData.items[index].codingsList.find(f => f.key.attId == code.attId);
      if (CodesFound) {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          const CodingFound = CodesFound.value.find(f => f.contactId.toString() == rId.key && f.armName == "");
          if (CodingFound) {
            if (CodingFound.isComplete) res.cells?.push({
              value: CompCodingVal, background: "#d0ffd0"
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)});
            else res.cells?.push({
              value: IncompCodingVal, background: "#dddddd"
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)});
          }
          else res.cells?.push({
            value: NoCodingVal
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)});
        }
      }
      else {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          res.cells?.push({
            value: NoCodingVal
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)});
        }
      }
    }
    return res;
  }

  private BuildWholeStudyInfoBoxRow(jsonData: iReportAllCodingCommand, setStats: ReviewStatisticsCodeSet2, index: number, reviewers: StringKeyValue[]): WorkbookSheetRow {
    const res: WorkbookSheetRow = { cells: [] };
    const item = jsonData.items[index];
    res.cells?.push({ value: item.itemId });
    res.cells?.push({ value: item.shortTitle });
    if (this.AllCodingReportOptions.showFullTitle) res.cells?.push({ value: item.title });
    res.cells?.push({ value: item.state });

    for (let code of jsonData.attributes) {
      const CodesFound = jsonData.items[index].codingsList.find(f => f.key.attId == code.attId);
      if (CodesFound) {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          const CodingFound = CodesFound.value.find(f => f.contactId.toString() == rId.key);
          if (CodingFound && CodingFound.armName == "") {
            if (CodingFound.isComplete) res.cells?.push({
              value: CodingFound.infoBox, background: "#d0ffd0"
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
            });
            else res.cells?.push({
              value: CodingFound.infoBox, background: "#dddddd"
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
            });
          }
          else res.cells?.push({
            value: ""
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
          });
        }
      }
      else {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          res.cells?.push({
            value: ""
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
          });
        }
      }
    }
    return res;
  }

  private readonly rx: RegExp = /<br \/>/g;
  private readonly rx2: RegExp = /\r\n|\r|\n/g;

  private BuildWholeStudyPDFRow(jsonData: iReportAllCodingCommand, setStats: ReviewStatisticsCodeSet2, index: number, reviewers: StringKeyValue[]): WorkbookSheetRow {
    const res: WorkbookSheetRow = { cells: [], height: 22 };
    const LinesSep = "\r\n" + this.AllCodingReportOptions.linesSeparator + "\r\n";
    const item = jsonData.items[index];
    res.cells?.push({ value: item.itemId });
    res.cells?.push({ value: item.shortTitle });
    if (this.AllCodingReportOptions.showFullTitle) res.cells?.push({ value: item.title });
    res.cells?.push({ value: item.state });
    let maxLines: number = 0; let cellLines = 0;
    for (let code of jsonData.attributes) {
      const CodesFound = jsonData.items[index].codingsList.find(f => f.key.attId == code.attId);
      if (CodesFound) {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          cellLines = 0;
          const CodingFound = CodesFound.value.find(f => f.contactId.toString() == rId.key && f.armName == "");
          if (CodingFound
            && CodingFound.pdf && CodingFound.pdf.length > 0) {
            let cellString = "";
            for (const p of CodingFound.pdf) {
              cellString += "[" + p.docName + ", pg: " + p.page + "] " + p.text.replace(this.rx, "\r\n");
              if (cellString != "") cellString += LinesSep;
            }
            if (cellString.endsWith(LinesSep)) cellString = cellString.substring(0, cellString.length - LinesSep.length);
            if (CodingFound.isComplete) res.cells?.push({
              value: cellString, wrap: true, background: "#d0ffd0"
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
            });
            else res.cells?.push({
              value: cellString, wrap: true, background: "#dddddd"
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
            });

            cellLines = cellString.split(this.rx2).length;
            if (cellLines > maxLines) maxLines = cellLines;
          }
          else if (CodingFound) {
            if (CodingFound.isComplete) res.cells?.push({
              value: "", background: "#d0ffd0"
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
            });
            else res.cells?.push({
              value: "", background: "#dddddd"
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
            });
          }
          else res.cells?.push({
            value: ""
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
          });
        }
      }
      else {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          res.cells?.push({
            value: ""
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
          });
        }
      }
    }
    if (maxLines > 0 && res.height) res.height = (res.height + 8) * maxLines;
    return res;
  }

  private BuildArmsDataRow(jsonData: iReportAllCodingCommand, setStats: ReviewStatisticsCodeSet2, index: number, reviewers: StringKeyValue[]): WorkbookSheetRow {
    const res: WorkbookSheetRow = { cells: [], height: 22 };
    const item = jsonData.items[index];
    const LinesSep = "\r\n" + this.AllCodingReportOptions.linesSeparator + "\r\n";
    let CompCodingVal: string | number = this.AllCodingReportOptions.labelForCompletedCoding;
    let IncompCodingVal: string | number = this.AllCodingReportOptions.labelForIncompleteCoding;
    let NoCodingVal: string | number = this.AllCodingReportOptions.labelForNoCoding;
    if (this.AllCodingReportOptions.UseOnlyColourCodingForCompletion) {
      CompCodingVal = ""; IncompCodingVal = "";
    }
    else if (this.AllCodingReportOptions.saveLabelsAsNumbers) {
      CompCodingVal = +CompCodingVal;
      IncompCodingVal = +IncompCodingVal;
      NoCodingVal = +NoCodingVal;
    }
    res.cells?.push({ value: item.itemId });
    res.cells?.push({ value: item.shortTitle });
    if (this.AllCodingReportOptions.showFullTitle) res.cells?.push({ value: item.title });
    res.cells?.push({ value: item.state });

    let maxLines: number = 0; let cellLines = 0;

    for (let code of jsonData.attributes) {
      const CodesFound = jsonData.items[index].codingsList.find(f => f.key.attId == code.attId);
      if (CodesFound) {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          cellLines = 0;
          const CodingFound = CodesFound.value.filter(f => f.contactId.toString() == rId.key && f.armName != "");
          if (CodingFound && CodingFound.length > 0) {
            let CellContent: string = "";
            let BgString: string = "";
            if (CodingFound[0].isComplete) {
              if (CompCodingVal != "") CellContent += CompCodingVal + LinesSep;
              BgString = "#d0ffd0";
            } else {
              if (IncompCodingVal != "") CellContent += IncompCodingVal + LinesSep;
              BgString = "#dddddd";
            }

            for (const ArmCoding of CodingFound) {
              CellContent += ArmCoding.armName + LinesSep;
            }

            cellLines = CellContent.split(this.rx2).length;
            if (CellContent.endsWith(LinesSep)) CellContent = CellContent.substring(0, CellContent.length - LinesSep.length);
            if (cellLines > maxLines) maxLines = cellLines;
            res.cells?.push({
              value: CellContent, background: BgString, wrap: true
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
            })
          }
          else res.cells?.push({
            value: (this.AllCodingReportOptions.UseOnlyColourCodingForCompletion ? "" : NoCodingVal)
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
          });
        }
      }
      else {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          res.cells?.push({
            value: (this.AllCodingReportOptions.UseOnlyColourCodingForCompletion ? "" : NoCodingVal)
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
          });
        }
      }
    }
    if (maxLines > 0 && res.height) res.height = (res.height) * maxLines;
    return res;
  }

  private BuildArmsInfoBoxDataRow(jsonData: iReportAllCodingCommand, setStats: ReviewStatisticsCodeSet2, index: number, reviewers: StringKeyValue[]): WorkbookSheetRow {
    const res: WorkbookSheetRow = { cells: [], height: 22 };
    const item = jsonData.items[index];
    const LinesSep = "\r\n" + this.AllCodingReportOptions.linesSeparator + "\r\n";
    let CompCodingVal: string | number = this.AllCodingReportOptions.labelForCompletedCoding;
    let IncompCodingVal: string | number = this.AllCodingReportOptions.labelForIncompleteCoding;
    let NoCodingVal: string | number = this.AllCodingReportOptions.labelForNoCoding;
    if (this.AllCodingReportOptions.UseOnlyColourCodingForCompletion) {
      CompCodingVal = ""; IncompCodingVal = "";
    }
    else if (this.AllCodingReportOptions.saveLabelsAsNumbers) {
      CompCodingVal = +CompCodingVal;
      IncompCodingVal = +IncompCodingVal;
      NoCodingVal = +NoCodingVal;
    }
    res.cells?.push({ value: item.itemId });
    res.cells?.push({ value: item.shortTitle });
    if (this.AllCodingReportOptions.showFullTitle) res.cells?.push({ value: item.title });
    res.cells?.push({ value: item.state });

    let maxLines: number = 0; let cellLines = 0;

    for (let code of jsonData.attributes) {
      const CodesFound = jsonData.items[index].codingsList.find(f => f.key.attId == code.attId);
      if (CodesFound) {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          cellLines = 0;
          const CodingFound = CodesFound.value.filter(f => f.contactId.toString() == rId.key && f.armName != "" && f.infoBox.trim() != "");
          if (CodingFound && CodingFound.length > 0) {
            let CellContent: string = "";
            let BgString: string = "";
            if (CodingFound[0].isComplete) {
              if (CompCodingVal != "") CellContent += CompCodingVal + LinesSep;
              BgString = "#d0ffd0";
            } else {
              if (IncompCodingVal != "") CellContent += IncompCodingVal + LinesSep;
              BgString = "#dddddd";
            }

            for (const ArmCoding of CodingFound) {
              CellContent += ArmCoding.armName + ":\r\n" + ArmCoding.infoBox + LinesSep;
            }

            cellLines = CellContent.split(this.rx2).length;
            if (CellContent.endsWith(LinesSep)) CellContent = CellContent.substring(0, CellContent.length - LinesSep.length);
            if (cellLines > maxLines) maxLines = cellLines;
            res.cells?.push({
              value: CellContent, background: BgString, wrap: true
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
            })
          }
          else res.cells?.push({
            value: (this.AllCodingReportOptions.UseOnlyColourCodingForCompletion ? "" : NoCodingVal)
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
          });
        }
      }
      else {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          res.cells?.push({
            value: (this.AllCodingReportOptions.UseOnlyColourCodingForCompletion ? "" : NoCodingVal)
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
          });
        }
      }
    }
    if (maxLines > 0 && res.height) res.height = (res.height) * maxLines;
    return res;
  }
  private BuildArmsPDFDataRow(jsonData: iReportAllCodingCommand, setStats: ReviewStatisticsCodeSet2, index: number, reviewers: StringKeyValue[]): WorkbookSheetRow {
    const res: WorkbookSheetRow = { cells: [], height: 22 };
    const item = jsonData.items[index];
    const LinesSep = "\r\n" + this.AllCodingReportOptions.linesSeparator + "\r\n";
    let CompCodingVal: string | number = this.AllCodingReportOptions.labelForCompletedCoding;
    let IncompCodingVal: string | number = this.AllCodingReportOptions.labelForIncompleteCoding;
    let NoCodingVal: string | number = this.AllCodingReportOptions.labelForNoCoding;
    if (this.AllCodingReportOptions.UseOnlyColourCodingForCompletion) {
      CompCodingVal = ""; IncompCodingVal = "";
    }
    else if (this.AllCodingReportOptions.saveLabelsAsNumbers) {
      CompCodingVal = +CompCodingVal;
      IncompCodingVal = +IncompCodingVal;
      NoCodingVal = +NoCodingVal;
    }
    res.cells?.push({ value: item.itemId });
    res.cells?.push({ value: item.shortTitle });
    if (this.AllCodingReportOptions.showFullTitle) res.cells?.push({ value: item.title });
    res.cells?.push({ value: item.state });

    let maxLines: number = 0; let cellLines = 0;

    for (let code of jsonData.attributes) {
      const CodesFound = jsonData.items[index].codingsList.find(f => f.key.attId == code.attId);
      if (CodesFound) {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          cellLines = 0;
          const CodingFound = CodesFound.value.filter(f => f.contactId.toString() == rId.key && f.armName != "");
          if (CodingFound && CodingFound.length > 0) {
            let CellContent: string = "";
            let perPdfContent: string = "";
            let BgString: string = "";
            if (CodingFound[0].isComplete) {
              if (CompCodingVal != "") CellContent += CompCodingVal + LinesSep;
              BgString = "#d0ffd0";
            } else {
              if (IncompCodingVal != "") CellContent += IncompCodingVal + LinesSep;
              BgString = "#dddddd";
            }
            for (const ArmCoding of CodingFound) {
              if (ArmCoding && ArmCoding.pdf && ArmCoding.pdf.length > 0) {
                perPdfContent = "";
                perPdfContent += "Arm name: " + ArmCoding.armName + "\r\n";
                for (const p of ArmCoding.pdf) {
                  perPdfContent += "[" + p.docName + ", pg: " + p.page + "] " + p.text.replace(this.rx, "\r\n");
                  if (perPdfContent != "") perPdfContent += LinesSep;
                }
                if (perPdfContent != "") CellContent += perPdfContent;
              }
            }

            cellLines = CellContent.split(this.rx2).length;
            if (CellContent.endsWith(LinesSep)) CellContent = CellContent.substring(0, CellContent.length - LinesSep.length);
            if (cellLines > maxLines) maxLines = cellLines;
            res.cells?.push({
              value: CellContent, background: BgString, wrap: true
              , borderLeft: (i == 0 ? { size: 1 } : undefined)
              , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
            });
          }
          else res.cells?.push({
            value: (this.AllCodingReportOptions.UseOnlyColourCodingForCompletion ? "" : NoCodingVal)
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
          });
        }
      }
      else {
        for (let i = 0; i < reviewers.length; i++) {
          let rId = reviewers[i];
          res.cells?.push({
            value: (this.AllCodingReportOptions.UseOnlyColourCodingForCompletion ? "" : NoCodingVal)
            , borderLeft: (i == 0 ? { size: 1 } : undefined)
            , borderRight: (i == reviewers.length - 1 ? { size: 1 } : undefined)
          });
        }
      }
    }
    if (maxLines > 0 && res.height) res.height = (res.height) * maxLines;
    return res;
  }

  private BuildOutcomesSheet(jsonData: iReportAllCodingCommand, sheet: WorkbookSheet) {
    const ItemsWithOutcomes = jsonData.items.filter(f => f.outcomesLists.length > 0);
    if (ItemsWithOutcomes.length == 0) return;

    //we have data to show, so we do...
    const cell1: WorkbookSheetRowCell = { value: "ITEM_ID", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const cell2: WorkbookSheetRowCell = { value: "Short Title", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const cell3: WorkbookSheetRowCell = { value: "Reviewer", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const cell4: WorkbookSheetRowCell = { value: "IsCompleted", wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const cell5: WorkbookSheetRowCell = { value: "Full title", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const headers: WorkbookSheetRow = { cells: [cell1, cell2, cell3, cell4] };
    if (this.AllCodingReportOptions.showFullTitle) headers.cells?.push(cell5);
    
    headers.cells?.push({ value: "I/E/D/S flag", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    
    headers.cells?.push({ value: "Outcome title", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Outcome description", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Timepoint", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Outcome", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Intervention", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Comparison", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Arm 1", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Arm 2", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Outcome type", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 1", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 2", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 3", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 4", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 5", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 6", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 7", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 8", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 9", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 10", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 11", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 12", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 13", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Data 14", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "ES", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "SE", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    headers.cells?.push({ value: "Outcome Codes", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
    sheet.rows?.push(headers);
    
    for (const item of ItemsWithOutcomes) {
      for (const PerUserOutcome of item.outcomesLists) {
        for (const val of PerUserOutcome.value) {
          const row: WorkbookSheetRow = { cells: [], height: 22 };
          row.cells?.push({ value: item.itemId, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: item.shortTitle, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.contactName, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.isComplete, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          if (this.AllCodingReportOptions.showFullTitle) row.cells?.push({ value: item.title, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });

          row.cells?.push({ value: item.state, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });

          row.cells?.push({ value: val.outcome.title, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.outcomeDescription, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.timepointDisplayValue, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.outcomeText, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.interventionText, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.controlText, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.grp1ArmName, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.grp2ArmName, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });

          const oType = this._OutcomesService.OutcomeTypeList.find(f => f.outcomeTypeId == val.outcome.outcomeTypeId);
          if (oType) {
            row.cells?.push({ value: oType.outcomeTypeName, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          } else {//shouldn't happen, but for safety reasons, we add a cell anyway
            row.cells?.push({ value: "undefined", borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          }

          row.cells?.push({ value: val.outcome.data1, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data2, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data3, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data4, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data5, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data6, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data7, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data8, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data9, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data10, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data11, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data12, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data13, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.data14, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.es, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          row.cells?.push({ value: val.outcome.sees, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          let cellVal: string = "";
          for (const oCode of val.outcome.outcomeCodes.outcomeItemAttributesList) {
            cellVal += oCode.attributeName + "\r\n";
          }
          if (cellVal != "") cellVal = cellVal.substring(0, cellVal.length - 2);
          const cellLines = cellVal.split(this.rx2).length;
          if (row.height) row.height = (row.height + 4) * cellLines;
          row.cells?.push({ value: cellVal, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop });
          sheet.rows?.push(row);

        }

      }
    }
  }

  ngOnDestroy() {
    //if (this.subOpeningReview) {
    //	this.subOpeningReview.unsubscribe();
    //}
  }
}
