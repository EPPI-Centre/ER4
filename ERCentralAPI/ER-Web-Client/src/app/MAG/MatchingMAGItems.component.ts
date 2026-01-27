import { Component, OnInit, ViewChild, OnDestroy, Output, EventEmitter, Input, Inject } from '@angular/core';
import { searchService } from '../services/search.service';
import { singleNode, SetAttribute, ReviewSetsService } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MVCMagPaperListSelectionCriteria, MagFieldOfStudy, MagPaper, TopicLink, MagBrowseHistoryItem, OpenAlexOriginReportCommand }
  from '../services/MAGClasses.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MVCMagFieldOfStudyListSelectionCriteria } from '../services/MAGClasses.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MAGTopicsService } from '../services/MAGTopics.service';
import { faSpinner } from '@fortawesome/free-solid-svg-icons';
import { Helpers } from '../helpers/HelperMethods';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';
import { Workbook, WorkbookSheet, WorkbookSheetColumn, WorkbookSheetRow, WorkbookSheetRowCell, WorkbookSheetRowCellBorderBottom, WorkbookSheetRowCellBorderLeft, WorkbookSheetRowCellBorderRight, WorkbookSheetRowCellBorderTop } from '@progress/kendo-angular-excel-export';

@Component({
  selector: 'MatchingMAGItems',
  templateUrl: './MatchingMAGItems.component.html',
  providers: []
})

export class MatchingMAGItemsComponent implements OnInit, OnDestroy {

  constructor(@Inject('BASE_URL') private _baseUrl: string,
    private ConfirmationDialogService: ConfirmationDialogService,
    public _magBasicService: MAGRelatedRunsService,
    public _magAdvancedService: MAGAdvancedService,
    private _magBrowserService: MAGBrowserService,
    public _searchService: searchService,
    private _ReviewerIdentityServ: ReviewerIdentityService,
    private _eventEmitterService: EventEmitterService,
    private router: Router,
    public _mAGBrowserHistoryService: MAGBrowserHistoryService,
    private _magTopicsService: MAGTopicsService,
    private reviewSetsService: ReviewSetsService
  ) {

  }
  public SearchTextTopic: string = '';
  ngOnInit() {

    if (this._ReviewerIdentityServ.reviewerIdentity.userId == 0 ||
      this._ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
      this.router.navigate(['home']);
    }
    else if (!this._ReviewerIdentityServ.HasWriteRights) {
      this.router.navigate(['Main']);
    }
    else if (this.reviewSetsService.ReviewSets.length == 0) {
      this.reviewSetsService.GetReviewSets();
    }
    //else {
    //    this.GetMagReviewMagInfoCommand();
    //}
  }
  ngOnDestroy() {
    this.CancelAutoRefresh();
  }
  @ViewChild('WithOrWithoutCodeSelector2') WithOrWithoutCodeSelector2!: codesetSelectorComponent;

  @Output() PleaseGoTo = new EventEmitter<string>();
  @Input() MustMatchItems: boolean = true;
  public CurrentDropdownSelectedCode2: singleNode | null = null;
  public dropdownBasic2: boolean = false;
  public isCollapsed2: boolean = false;
  public ListSubType: string = '';
  public SearchTextTopics: TopicLink[] = [];
  public SearchTextTopicsResults: TopicLink[] = [];
  public SearchTextTopicResultsPageNumber: number = 1;
  public SearchTextTopicResultsLength: number = 0;
  public SearchTextTopicResultsTotalPages: number = 0;
  public magPaperId: number = 0;
  public basicPanel: boolean = false;
  public OAoriginReport: OpenAlexOriginReportCommand | null = null;
  faSpinner = faSpinner;
  public Back() {
    //this.router.navigate(['Main']);
  }

  public AutoRefreshTimeoutId = setTimeout(() => { }, 1);
  public AutoRefreshIsRunning: boolean = false;

  public AutoRefreshOnTimer(seconds: number = 30) {
    const Msecs = seconds * 1000;
    this.AutoRefreshIsRunning = true;
    clearTimeout(this.AutoRefreshTimeoutId);//cancelling the timeout in case there is another instance running - for safety
    this.AutoRefreshTimeoutId = setTimeout(async () => {
      await this._magAdvancedService.FetchMagReviewMagInfo();
      if (this._magAdvancedService.AdvancedReviewInfo.matchingTaskIsRunning) {
        this.AutoRefreshOnTimer(seconds);
      }
      else this.CancelAutoRefresh();
    }, Msecs);
  }
  public CancelAutoRefresh() {
    clearTimeout(this.AutoRefreshTimeoutId);
    this.AutoRefreshIsRunning = false;
  }

  public ClearAllMatching() {

    this.ConfirmationDialogService.confirm("Are you sure you wish to clear all matching in your review?", "", false, "")
      .then(
        (confirm: any) => {
          if (confirm) {
            this.ConfirmationDialogService.showMAGDelayMessage("Clearing all matches!");
            this._magAdvancedService.ClearAllMAGMatches(0).then((res) => {
              this._magAdvancedService.FetchMagReviewMagInfo();
            });
          }
        }
      )
      .catch(() => { });


  }
  public ClearAllNonManualMatching() {

    this.ConfirmationDialogService.confirm("Are you sure you wish to clear all non-manual matching in your review?", "", false, "")
      .then(
        (confirm: any) => {
          if (confirm) {
            this.ConfirmationDialogService.showMAGDelayMessage("Clearing all non-manual matches!");
            this._magAdvancedService.ClearAllNonManualMAGMatches(0).then((res) => {
              this._magAdvancedService.FetchMagReviewMagInfo();
            });
          }
        }
      )
      .catch(() => { });


  }
  public ClearMatches() {

    this.ConfirmationDialogService.confirm("Are you sure you want to clear matches from items with this code?", "", false, "")
      .then(
        (confirm: any) => {
          if (confirm) {
            let attribute = this.CurrentDropdownSelectedCode2 as SetAttribute;
            if (attribute != null) {
              this._magAdvancedService.ClearAllMAGMatches(attribute.attribute_id).then((res) => {
                this._magAdvancedService.FetchMagReviewMagInfo();
              });
            }
            this.ConfirmationDialogService.showMAGDelayMessage("Clearing all matches for specific code!");
          }
        }
      )
  }
  public ClearNonManualMatches() {

    this.ConfirmationDialogService.confirm("Are you sure you want to clear non-manual matches from items with this code?", "", false, "")
      .then(
        (confirm: any) => {
          if (confirm) {
            let attribute = this.CurrentDropdownSelectedCode2 as SetAttribute;
            if (attribute != null) {
              this._magAdvancedService.ClearAllNonManualMAGMatches(attribute.attribute_id).then((res) => {
                this._magAdvancedService.FetchMagReviewMagInfo();
              });
            }
            this.ConfirmationDialogService.showMAGDelayMessage("Clearing all non-manual matches for specific code!");
          }
        }
      )
  }
  public get HasWriteRights(): boolean {
    return this._ReviewerIdentityServ.HasWriteRights;
  }
  public get IsServiceBusy(): boolean {

    return this._magBrowserService.IsBusy || this._magAdvancedService.IsBusy;
  }
  GetMagReviewMagInfoCommand() {
    this._magAdvancedService.FetchMagReviewMagInfo();
  }

  public UpdateTopicResultsPrevious(step: string) {
    this.SearchTextTopicResultsPageNumber -= 1;
    this.SearchTextTopicsResults = [];
    this.UpdateTopicResults(step);
  }

  public UpdateTopicResultsNext(step: string) {
    this.SearchTextTopicResultsPageNumber += 1;
    this.SearchTextTopicsResults = [];
    this.UpdateTopicResults(step);
  }

  public UpdateTopicResults(caller: string) {
    if (this.SearchTextTopic.length > 2) {
      if (caller == "new") {
        // this is a new call so initialise
        this.SearchTextTopicsResults = [];
        this.SearchTextTopicResultsLength = 0;
        this.SearchTextTopicResultsTotalPages = 1;
        this.SearchTextTopicResultsPageNumber = 1;
      }
      else {
        // this is a step call

      }

      let criteriaFOSL: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
      criteriaFOSL.fieldOfStudyId = 0;
      criteriaFOSL.listType = 'FieldOfStudySearchList';
      criteriaFOSL.paperIdList = '';
      criteriaFOSL.SearchTextTopics = this.SearchTextTopic;
      criteriaFOSL.pageNumber = this.SearchTextTopicResultsPageNumber;
      this._magTopicsService.FetchMagFieldOfStudyList(criteriaFOSL, '').then(

        (results: MagFieldOfStudy[] | boolean) => {
          if (results != false) {

            let FosList: MagFieldOfStudy[] = results as MagFieldOfStudy[];
            this.SearchTextTopicResultsLength = FosList[0].totalCount;

            this.SearchTextTopicResultsPageNumber = criteriaFOSL.pageNumber;

            if (FosList[0].totalCount > 50) {
              this.SearchTextTopicResultsTotalPages = Math.trunc(FosList[0].totalCount / 50);
              const intoNextPage = FosList[0].totalCount % 50;
              if (intoNextPage > 0) {
                this.SearchTextTopicResultsTotalPages += 1;
              }
            }
            else {
              this.SearchTextTopicResultsTotalPages = 1;
              this.SearchTextTopicResultsPageNumber = 1;
            }

            let i: number = 1.7;
            let cnt: number = 0;

            for (var fos of FosList) {
              let item: TopicLink = new TopicLink();
              item.displayName = fos.displayName;
              item.fontSize = i;
              item.fieldOfStudyId = fos.fieldOfStudyId;

              this.SearchTextTopicsResults[cnt] = item;

              cnt += 1;
              if (i > 0.1) {
                i -= 0.01;
              }
            }
            return;
          }
          else {
            this.SearchTextTopicResultsLength = -1;
          }
        }
      );

    } else {
      this.SearchTextTopics = [];
      this.SearchTextTopicsResults = [];
    }
  }
  public async FOSMAGBrowserNavigate(displayName: string, fieldOfStudyId: number) {

    let res: boolean = await this._magBrowserService.GetParentAndChildRelatedPapers(displayName, fieldOfStudyId);
    if (res == true) {
      this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem(displayName, "BrowseTopic", 0,
        "", "", 0, "", "", fieldOfStudyId, displayName, "", 0));
      this.PleaseGoTo.emit("BrowseTopic");
    }

    //this.router.navigate(['MAGBrowser']);
  }

  public RunMatchingAlgo(matchType: number) {

    var att = this.CurrentDropdownSelectedCode2 as SetAttribute;
    let msg: string = '';
    //if (att != null && att.attribute_id > 0) {
    if (matchType == 1) {
      msg = 'Are you sure you want to match all items with this code to OpenAlex records?';
    } else {
      msg = 'Are you sure you want to match all items to OpenAlex records?';
    }
    this.ConfirmationDialogService.confirm('Run matching algorithm', msg, false, '')
      .then((confirm: any) => {
        if (confirm) {
          //if (att != null && att.attribute_id > 0) {
          if (matchType == 1) {
            this._magAdvancedService.RunMatchingAlgorithm(att.attribute_id).then(
              (result) => {
                //msg = 'Are you sure you want to match all items with this code to Microsoft Academic records?';
                this.RunMatchingAlgoResultHandling(result);
              }
            );
          } else {
            this._magAdvancedService.RunMatchingAlgorithm(0).then(
              (result) => {
                //msg = 'Are you sure you want to match all items to Microsoft Academic records?';
                this.RunMatchingAlgoResultHandling(result);
              }
            );
          }
          this._magAdvancedService._RunAlgorithmFirst = true;
        }
      });
  }
  private RunMatchingAlgoResultHandling(result: string) {
    if (result == "error") {
      this.ConfirmationDialogService.showMAGRunMessage('Matching has returned an error please try again. Contact EPPISupport if the problem repeats.');
    }
    else if (result == "Another matching job is already running for this review.") {
      this.ConfirmationDialogService.showWarningMessageInStrip('Another matching job is already running for this review.');
      this._magAdvancedService.AdvancedReviewInfo.matchingTaskIsRunning = true;
      if (!this.AutoRefreshIsRunning) this.AutoRefreshOnTimer(30);
    }
    else {
      this.ConfirmationDialogService.showMAGRunMessage('Matching records to OpenAlex. This can take a while...');
      this._magAdvancedService.AdvancedReviewInfo.matchingTaskIsRunning = true;
      if (!this.AutoRefreshIsRunning) this.AutoRefreshOnTimer(30);
    }
  }
  public OpenMatchesInReview(listType: string) {

    if (listType != null) {
      this.ListSubType = listType;
      this._eventEmitterService.criteriaMAGChange.emit(listType);
    }
  }
  public MAGBrowser(listType: string) {
    this._magBrowserService.currentMagPaper = new MagPaper();
    //this._magBrowserService.WPChildTopics = [];
    //this._magBrowserService.WPParentTopics = [];
    this._magBrowserService.ParentTopic = '';
    if (listType == 'MatchedIncluded') {
      this.GetMatchedMagIncludedList();

    } else if (listType == 'MatchedExcluded') {
      this.GetMatchedMagExcludedList();

    } else if (listType == 'MatchedAll') {
      this.GetMatchedMagAllList();

    } else if (listType == 'MatchedWithThisCode') {
      this.GetMatchedMagWithCodeList();
    }
  }
  public async GetMatchedMagIncludedList() {
    let res = await this._magBrowserService.GetMatchedMagIncludedList();
    if (res == true) {
      this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("List all included matches", "MatchesIncluded", 0, "", "", 0, "", "", 0, "", "", 0));
      this.PleaseGoTo.emit("MatchesIncluded");
    }
  }
  public async GetMatchedMagExcludedList() {
    let res = await this._magBrowserService.GetMatchedMagExcludedList();
    if (res == true) {
      this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("List all excluded matches", "MatchesExcluded", 0, "", "", 0, "", "", 0, "", "", 0));
      this.PleaseGoTo.emit("MatchesExcluded");
    }
  }
  public async GetMatchedMagAllList() {
    let res = await this._magBrowserService.GetMatchedMagAllList();
    if (res == true) {
      this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("List of all matches in review (included and excluded)", "MatchesIncludedAndExcluded",
        0, "", "", 0, "", "", 0, "", "", 0));
      this.PleaseGoTo.emit("MatchesIncludedAndExcluded");
    }
  }
  public CanGetCodeMatches(): boolean {
    if (this.CurrentDropdownSelectedCode2 != null) {
      return true;
    } else {
      return false;
    }
  }
  public async GetMatchedMagWithCodeList() {
    if (this.CurrentDropdownSelectedCode2 != null && this.CurrentDropdownSelectedCode2.nodeType == "SetAttribute") {
      let att = this.CurrentDropdownSelectedCode2 as SetAttribute;
      let res: boolean = await this._magBrowserService.GetMatchedMagWithCodeList(att);

      if (res == true) {
        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("List of all item matches with this code", "ReviewMatchedPapersWithThisCode", 0,
          "", "", 0, "", "", 0, "", att.attribute_id.toString(), 0));
        this.PleaseGoTo.emit("ReviewMatchedPapersWithThisCode");
      }



    }
  }
  public CanGetMagPaper(): boolean {

    if (this.magPaperId != null && this.magPaperId > 0) {
      return true;
    } else {
      return false;
    }

  }
  public CanGetTopics(): boolean {

    if (this.SearchTextTopic.trim().length > 2) {
      return true;
    } else {
      return false;
    }
  }
  public CanGetNotMatchedExcluded(): boolean {

    if (this._magAdvancedService.AdvancedReviewInfo.nNotMatchedExcluded > 0) {
      return true;
    } else {
      return false;
    }
  }
  public CanGetNotMatchedIncluded(): boolean {

    if (this._magAdvancedService.AdvancedReviewInfo.nNotMatchedIncluded > 0) {
      return true;
    } else {
      return false;
    }
  }
  public CanGetMatchesNeedingCheckingIncluding(): boolean {

    if (this._magAdvancedService.AdvancedReviewInfo.nRequiringManualCheckIncluded > 0) {
      return true;
    } else {
      return false;
    }
  }
  public CanGetMatchesNeedingCheckingExcluding(): boolean {

    if (this._magAdvancedService.AdvancedReviewInfo.nRequiringManualCheckExcluded > 0) {
      return true;
    } else {
      return false;
    }
  }
  public CanGetMatchedAll(): boolean {

    if ((this._magAdvancedService.AdvancedReviewInfo.nMatchedAccuratelyIncluded +
      this._magAdvancedService.AdvancedReviewInfo.nMatchedAccuratelyExcluded) > 0) {
      return true;
    } else {
      return false;
    }
  }
  public CanGetMatchedIncluded(): boolean {

    if (this._magAdvancedService.AdvancedReviewInfo.nMatchedAccuratelyIncluded > 0) {
      return true;
    } else {
      return false;
    }
  }
  public CanGetMatchedExcluded(): boolean {

    if (this._magAdvancedService.AdvancedReviewInfo.nMatchedAccuratelyExcluded > 0) {
      return true;
    } else {
      return false;
    }
  }
  public CanGetNPreviouslyMatched(): boolean {

    if (this._magAdvancedService.AdvancedReviewInfo.nPreviouslyMatched > 0) {
      return true;
    } else {
      return false;
    }
  }
  public GetMagPaper() {

    this._magBrowserService.GetCompleteMagPaperById(this.magPaperId).then(
      (result: boolean) => {
        if (result == true && this._magBrowserService.currentMagPaper.paperId > 0) {
          const p = this._magBrowserService.currentMagPaper;
          this._magTopicsService.ShowingParentAndChildTopics = false;
          this._magTopicsService.ShowingChildTopicsOnly = true;
          this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Go to specific Paper Id: " + p.fullRecord, "PaperDetail", p.paperId, p.fullRecord,
            p.abstract, p.linkedITEM_ID, p.allLinks, p.findOnWeb, 0, "", "", 0));
          this.PleaseGoTo.emit("PaperDetail");
        } else {
          this.ConfirmationDialogService.showMAGRunMessage('OpenAlex could not find the paperId!');
        }
      });
  }
  CloseCodeDropDown2() {
    if (this.WithOrWithoutCodeSelector2) {
      this.CurrentDropdownSelectedCode2 = this.WithOrWithoutCodeSelector2.SelectedNodeData;
    }
    this.isCollapsed2 = false;
    this.OAoriginReport = null;
  }
  async GetReport() {
    if (this.CurrentDropdownSelectedCode2 != null && this.CurrentDropdownSelectedCode2.nodeType == "SetAttribute") {
      let att = this.CurrentDropdownSelectedCode2 as SetAttribute;
      const check = await this._magAdvancedService.GetOpenAlexOriginReportCommand(att.attribute_id);
      if (check != false) {
        this.OAoriginReport = check as OpenAlexOriginReportCommand;
        const res = this.MakeHTMLReport();
        Helpers.OpenInNewWindow(res, this._baseUrl, "Open Alex Origin Report");
      }
    }
  }
  MakeHTMLReport(): string {
    if (this.OAoriginReport == null) return "";
    const rep = this.OAoriginReport;
    const att = this.CurrentDropdownSelectedCode2 as SetAttribute;
    let res = "<h4>Open Alex Origin Report</h4>"
      + "<p>Generated in ReviewId = " + this._ReviewerIdentityServ.reviewerIdentity.reviewId.toString()
      + ", on items with \"" + att.attribute_name + "\" (id = " + att.attribute_id.toString() +").</p>"
      + rep.SummaryHTMLtable;
    res += "<Br />" + rep.AutoUpdatesHTMLTable;
    res += "<Br />" + rep.RelatedRunsHTMLTable;
    res += "<Br />" + rep.TextSearchesHTMLTable;
    res += "<Br />" + rep.ItemsHTMLTable;
    return res;
  }
  SaveReport() {
    if (this.OAoriginReport == null) return ;
    const rep = this.MakeHTMLReport();
    const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(rep, this._baseUrl, "Open Alex Origin Report", true));
    saveAs(dataURI, "Open Alex Origin Report.html");
  }
  SaveReportToExcel() {
    if (this.OAoriginReport == null) return;
    const rep = this.OAoriginReport;
    let SheetsToAdd: WorkbookSheet[] = [{
      name: "Summary",
      columns: <WorkbookSheetColumn[]>[],
      rows: []
    }, {
      name: "Items",
      columns: <WorkbookSheetColumn[]>[],
      rows: []
    }];
    const workbook: Workbook = new Workbook({
      sheets: <WorkbookSheet[]>SheetsToAdd
    });
    const SummarySheet = SheetsToAdd[0];
    const ItemsSheet = SheetsToAdd[1];
    ItemsSheet.frozenRows = 1;

    const Cmedium: WorkbookSheetColumn = { autoWidth: false, width: 170 };
    SummarySheet.columns?.push(Cmedium);
    SummarySheet.columns?.push(Cmedium);
    SummarySheet.columns?.push(Cmedium);
    SummarySheet.columns?.push(Cmedium);
    SummarySheet.columns?.push(Cmedium);
    SummarySheet.columns?.push(Cmedium);
    SummarySheet.columns?.push(Cmedium);
    SummarySheet.columns?.push(Cmedium);
    SummarySheet.columns?.push(Cmedium);
    SummarySheet.columns?.push(Cmedium);
    SummarySheet.columns?.push(Cmedium);
    
    const titleCell: WorkbookSheetRowCell = { value: "Open Alex Origin Report", bold: true, colSpan:7 };
    const emptyCell: WorkbookSheetRowCell = { value: "" };
    SummarySheet.rows?.push({ cells: [titleCell] });
    SummarySheet.rows?.push({ cells: [emptyCell] });
    const att = this.CurrentDropdownSelectedCode2 as SetAttribute;
    const descrCell: WorkbookSheetRowCell = {
      value: "Generated in ReviewId = " + this._ReviewerIdentityServ.reviewerIdentity.reviewId.toString()
        + ", on items with \"" + att.attribute_name + "\" (id = " + att.attribute_id.toString() + ")."
        , colSpan:7
    };
    SummarySheet.rows?.push({ cells: [descrCell] });
    SummarySheet.rows?.push({ cells: [emptyCell] });

    const summH1: WorkbookSheetRowCell = { value: "Total Items", bold:true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH2: WorkbookSheetRowCell = { value: "Matched", bold: true,borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH3: WorkbookSheetRowCell = { value: "Not Matched", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH4: WorkbookSheetRowCell = { value: "In Auto Updates", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH5: WorkbookSheetRowCell = { value: "In Related Searches", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH6: WorkbookSheetRowCell = { value: "In both AU & RS", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    //2nd row
    const summH7: WorkbookSheetRowCell = { value: "In Text Searches", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH8: WorkbookSheetRowCell = { value: "In both TS & AU", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH9: WorkbookSheetRowCell = { value: "In both TS & RS", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH10: WorkbookSheetRowCell = { value: "In all 3", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH11: WorkbookSheetRowCell = { value: "Matched but in none", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };

    const summD1: WorkbookSheetRowCell = { value: rep.summary.totalItems, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summD2: WorkbookSheetRowCell = { value: rep.summary.matched,borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summD3: WorkbookSheetRowCell = { value: rep.summary.notMatched, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summD4: WorkbookSheetRowCell = { value: rep.summary.inAutoUpdateResults, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summD5: WorkbookSheetRowCell = { value: rep.summary.inRelatedSearches, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summD6: WorkbookSheetRowCell = { value: rep.summary.inBothAuAndRs, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    //2nd row
    const summD7: WorkbookSheetRowCell = { value: rep.summary.inTextSearches, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summD8: WorkbookSheetRowCell = { value: rep.summary.inBothAuAndTs, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summD9: WorkbookSheetRowCell = { value: rep.summary.inBothTsAndRs, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summD10: WorkbookSheetRowCell = { value: rep.summary.inAll3, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summD11: WorkbookSheetRowCell = { value: rep.summary.otherMatched, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };

    SummarySheet.rows?.push({ cells: [summH1, summH2, summH3, summH4, summH5, summH6] });
    SummarySheet.rows?.push({ cells: [summD1, summD2, summD3, summD4, summD5, summD6] });

    SummarySheet.rows?.push({ cells: [emptyCell, summH7, summH8, summH9, summH10, summH11] });
    SummarySheet.rows?.push({ cells: [emptyCell, summD7, summD8, summD9, summD10, summD11] });

    SummarySheet.rows?.push({ cells: [emptyCell] });
    SummarySheet.rows?.push(...this.MakeAURexcel(rep));
    SummarySheet.rows?.push({ cells: [emptyCell] });
    SummarySheet.rows?.push(...this.MakeRsExcel(rep));
    SummarySheet.rows?.push({ cells: [emptyCell] });
    SummarySheet.rows?.push(...this.MakeTsExcel(rep));
    this.MakeItemsExecl(rep, ItemsSheet);
    workbook.toDataURL().then((dataUrl) => {
      saveAs(dataUrl, "OpenAlex Origin Report.xlsx");
      //this.saving = false;
    });
  }
  private MakeAURexcel(rep: OpenAlexOriginReportCommand): WorkbookSheetRow[] {
    let res: WorkbookSheetRow[] = [];
    const summH: WorkbookSheetRowCell = { value: "Relevant Auto Updates:", colSpan: 3 };
    res.push({ cells: [summH] });
    const summH1: WorkbookSheetRowCell = { value: "Name", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH2: WorkbookSheetRowCell = { value: "Hits N", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH3: WorkbookSheetRowCell = { value: "Id", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH4: WorkbookSheetRowCell = { value: "Study Type Classifier", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH5: WorkbookSheetRowCell = { value: "User Classifier", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH6: WorkbookSheetRowCell = { value: "Date", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH7: WorkbookSheetRowCell = { value: "Items #", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    res.push({ cells: [summH1, summH2, summH3, summH4, summH5, summH6, summH7] });

    let index: number = 0;
    for (let aur of rep.magAutoUpdateRunList) {
      const summD1: WorkbookSheetRowCell = { value: aur.userDescription, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD2: WorkbookSheetRowCell = { value: aur.nPapers, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD3: WorkbookSheetRowCell = { value: aur.magAutoUpdateRunId, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD4: WorkbookSheetRowCell = { value: aur.studyTypeClassifier, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD5: WorkbookSheetRowCell = { value: aur.userClassifierDescription, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD6: WorkbookSheetRowCell = { value: Helpers.FormatDate(aur.dateRun), borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD7: WorkbookSheetRowCell = { value: rep.magAutoUpdateRunListCounts[index], borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      res.push({ cells: [summD1, summD2, summD3, summD4, summD5, summD6, summD7] });
      index++;
    }
    return res;
  }
  private MakeRsExcel(rep: OpenAlexOriginReportCommand): WorkbookSheetRow[] {
    let res: WorkbookSheetRow[] = [];
    const summH: WorkbookSheetRowCell = { value: "Relevant Auto Updates:", colSpan: 3 };
    res.push({ cells: [summH] });
    const summH1: WorkbookSheetRowCell = { value: "Name", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH2: WorkbookSheetRowCell = { value: "Hits N", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH3: WorkbookSheetRowCell = { value: "Id", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH4: WorkbookSheetRowCell = { value: "Mode", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH5: WorkbookSheetRowCell = { value: "With this code", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH6: WorkbookSheetRowCell = { value: "Date run", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH7: WorkbookSheetRowCell = { value: "Items #", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    res.push({ cells: [summH1, summH2, summH3, summH4, summH5, summH6, summH7] });

    let index: number = 0;
    for (let rs of rep.magRelatedSearches) {
      const summD1: WorkbookSheetRowCell = { value: rs.userDescription, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD2: WorkbookSheetRowCell = { value: rs.nPapers, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD3: WorkbookSheetRowCell = { value: rs.magRelatedRunId, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD4: WorkbookSheetRowCell = { value: rs.mode, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD5: WorkbookSheetRowCell = { value: rs.attributeName, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD6: WorkbookSheetRowCell = { value: Helpers.FormatDate(rs.dateRun), borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD7: WorkbookSheetRowCell = { value: rep.magRelatedSearchesCounts[index], borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      res.push({ cells: [summD1, summD2, summD3, summD4, summD5, summD6, summD7] });
      index++;
    }
    return res;
  }

  private MakeTsExcel(rep: OpenAlexOriginReportCommand): WorkbookSheetRow[] {
    let res: WorkbookSheetRow[] = [];
    const summH: WorkbookSheetRowCell = { value: "Relevant Text Searches:", colSpan: 3 };
    res.push({ cells: [summH] });

    let summHs: WorkbookSheetRowCell = {
      value: "Results for these only contain searches for which the list of results is stored (and fixed) in EPPI Reviewer. These are \"combined searches\" and searches that have been imported at least once (since V.6.17.2)."
      , colSpan: 7, fontSize: 13, wrap: true
    };
    res.push({ cells: [summHs] });
    
    //Results for these only contain searches for which the list of results is stored (and fixed)
    const summH1: WorkbookSheetRowCell = { value: "Name", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH2: WorkbookSheetRowCell = { value: "Hits N", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH3: WorkbookSheetRowCell = { value: "Id", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH4: WorkbookSheetRowCell = { value: "Search N.", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH5: WorkbookSheetRowCell = { value: "Date run", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH6: WorkbookSheetRowCell = { value: "Items #", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    res.push({ cells: [summH1, summH2, summH3, summH4, summH5, summH6] });

    let index: number = 0;
    for (let rs of rep.magTextSearches) {
      const summD1: WorkbookSheetRowCell = { value: rs.searchText, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD2: WorkbookSheetRowCell = { value: rs.hitsNo, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD3: WorkbookSheetRowCell = { value: rs.magSearchId, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD4: WorkbookSheetRowCell = { value: rs.searchNo, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD5: WorkbookSheetRowCell = { value: Helpers.FormatDate2(rs.searchDate), borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD6: WorkbookSheetRowCell = { value: rep.magTextSearchesCounts[index], borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      res.push({ cells: [summD1, summD2, summD3, summD4, summD5, summD6] });
      index++;
    }
    return res;
  }

  private MakeItemsExecl(rep: OpenAlexOriginReportCommand, ItemsSheet: WorkbookSheet) {
    const Cshort: WorkbookSheetColumn = { autoWidth: false, width: 70 };
    const Cmedium: WorkbookSheetColumn = { autoWidth: false, width: 150 };
    const Cwide: WorkbookSheetColumn = { autoWidth: false, width: 350 };
    ItemsSheet.columns?.push(Cshort);
    ItemsSheet.columns?.push(Cmedium);
    ItemsSheet.columns?.push(Cwide);
    ItemsSheet.columns?.push(Cmedium);
    ItemsSheet.columns?.push(Cwide);
    ItemsSheet.columns?.push(Cmedium);
    ItemsSheet.columns?.push(Cshort);
    ItemsSheet.columns?.push(Cshort);
    ItemsSheet.columns?.push(Cshort);
    ItemsSheet.columns?.push(Cwide);
    ItemsSheet.columns?.push(Cshort);
    ItemsSheet.columns?.push(Cwide);
    ItemsSheet.columns?.push(Cshort);
    const summH1: WorkbookSheetRowCell = { value: "ItemId", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH2: WorkbookSheetRowCell = { value: "Short Title", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH3: WorkbookSheetRowCell = { value: "Title", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH4: WorkbookSheetRowCell = { value: "In source:", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH5: WorkbookSheetRowCell = { value: "AutoUpdate Runs", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH6: WorkbookSheetRowCell = { value: "AU date:", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH7: WorkbookSheetRowCell = { value: "AU score:", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH8: WorkbookSheetRowCell = { value: "Study Type score:", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH9: WorkbookSheetRowCell = { value: "User Classif. score:", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH10: WorkbookSheetRowCell = { value: "AU Runs Count", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH11: WorkbookSheetRowCell = { value: "Related Searches:", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH12: WorkbookSheetRowCell = { value: "RS count:", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH13: WorkbookSheetRowCell = { value: "Text Searches:", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    const summH14: WorkbookSheetRowCell = { value: "TS count:", bold: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
    ItemsSheet.rows?.push({
      cells: [summH1, summH2, summH3, summH4, summH5, summH6, summH7
        , summH8, summH9, summH10, summH11, summH12, summH13, summH14]
    });
    const minHeight = 20;
    for (let i of rep.items) {
      //i.itemId + "</td><td>" + i.shortTitle + "</td><td class='small'>" + i.title
      const summD1: WorkbookSheetRowCell = { value: i.itemId, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD2: WorkbookSheetRowCell = { value: i.shortTitle, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD3: WorkbookSheetRowCell = { value: i.title, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD4: WorkbookSheetRowCell = { value: i.sourceName, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      let CellVal5 = "";
      let CellVal6 = "";
      let CellVal7 = "";
      let CellVal8 = "";
      let CellVal9 = "";
      let aurCount = 0;
      for (let AuId of i.autoUpdateResults) {
        const aur = rep.GetAutoUpdate(AuId.autoUpdateId);
        if (aur) {
          aurCount++;
          if (CellVal5 != "") {
            CellVal5 += "\r\n";
            CellVal6 += "\r\n";
            CellVal7 += "\r\n";
            CellVal8 += "\r\n";
            CellVal9 += "\r\n";
          }
          CellVal5 += aur.userDescription + " (" + aur.magAutoUpdateRunId + ")";
          CellVal6 += Helpers.FormatDate2(AuId.dateRun);
          CellVal7 += AuId.contReviewScore.toLocaleString("en-GB", { maximumFractionDigits: 3 });
          CellVal8 += AuId.studyTypeClassifierScore.toLocaleString("en-GB", { maximumFractionDigits: 3 });
          CellVal9 += AuId.userClassifierScore.toLocaleString("en-GB", { maximumFractionDigits: 3 });
        }
      }
      const summD5: WorkbookSheetRowCell = { value: CellVal5, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD6: WorkbookSheetRowCell = { value: CellVal6, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD7: WorkbookSheetRowCell = { value: CellVal7, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD8: WorkbookSheetRowCell = { value: CellVal8, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD9: WorkbookSheetRowCell = { value: CellVal9, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD10: WorkbookSheetRowCell = { value: aurCount, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      let CellVal = "";
      let rsCount = 0;
      for (let rsId of i.relatedSearches) {
        const rs = rep.GetRelatedSearch(rsId);
        if (rs) {
          rsCount++;
          if (CellVal != "") CellVal += "\r\n";
          CellVal += rs.userDescription + " (" + rs.magRelatedRunId + ")";
        }
      }
      const summD11: WorkbookSheetRowCell = { value: CellVal, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD12: WorkbookSheetRowCell = { value: rsCount, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      CellVal = "";
      let tsCount = 0;
      for (let tsId of i.textSearches) {
        const rs = rep.GetTextSearch(tsId);
        if (rs) {
          tsCount++;
          if (CellVal != "") CellVal += "\r\n";
          CellVal += rs.searchText + " (#" + rs.searchNo + ")";
        }
      }
      const summD13: WorkbookSheetRowCell = { value: CellVal, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      const summD14: WorkbookSheetRowCell = { value: tsCount, wrap: true, borderBottom: this.borderBottom, borderLeft: this.borderLeft, borderRight: this.borderRight, borderTop: this.borderTop };
      ItemsSheet.rows?.push({
        cells: [summD1, summD2, summD3, summD4, summD5, summD6, summD7
          , summD8, summD9, summD10, summD11, summD12, summD13, summD14]
      });

      if (rsCount > 0 || aurCount > 0 || tsCount > 0) {
        let rCount = 0;
        if (ItemsSheet.rows) {
          rsCount = Math.max(rsCount,aurCount,tsCount);
          rCount = ItemsSheet.rows.length;
          const height = minHeight * rsCount;
          ItemsSheet.rows[rCount - 1].height = height;
        }
      }
    }
  }
  private readonly borderBottom: WorkbookSheetRowCellBorderBottom = { size: 1 };
  private readonly borderLeft: WorkbookSheetRowCellBorderLeft = { size: 1 };
  private readonly borderTop: WorkbookSheetRowCellBorderTop = { size: 1 };
  private readonly borderRight: WorkbookSheetRowCellBorderRight = { size: 1 };
}

