import { Component, Inject, OnInit, ViewChild, OnDestroy, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Criteria, ItemList } from '../services/ItemList.service';
import { ItemListService } from '../services/ItemList.service'
import { ReviewSetsService, ReviewSet, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { CodesetStatisticsService, ReviewStatisticsCountsCommand, StatsCompletion, StatsByReviewer, BulkCompleteUncompleteCommand, ReviewStatisticsCodeSet2, iReviewStatisticsReviewer2 } from '../services/codesetstatistics.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { saveAs } from '@progress/kendo-file-saver';
import { ConfigurableReportService } from '../services/configurablereport.service';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';


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


  public async GetAndSaveCodingReport(setStats: StatsCompletion) {
    console.log("Asking for the report");
    let stringReport = await this.configurablereportServ.FetchAllCodingReportBySet(setStats.setId);
    if (stringReport != 'error') {
      console.log("Got the report");
      this.SaveAsHtml(stringReport, setStats.setName);
    }
  }
  private saving: boolean = false;
  public SaveAsHtml(reportHTML: string, CodesetName: string) {
    if (reportHTML.length < 1) return;
    this.saving = true;
    //console.log("Encoding the report, length:", reportHTML.length);
    //let dataURI: string = "";
    //if (reportHTML.length < 1000000) dataURI = "data:text/plain;base64," + encodeBase64(reportHTML);
    //else {
    //	let remaining = reportHTML.length;
    //	let i: number = 0; const ChunkLength = 999999;
    //	dataURI = "data:text/plain;base64,";
    //	while (remaining > 0 && i < 10000) {
    //		const end = remaining < ChunkLength ? reportHTML.length : (i + 1) * ChunkLength;  //((i + 1) * ChunkLength > reportHTML.length)
    //		const tmp = reportHTML.substring(i * ChunkLength, end);
    //		dataURI += encodeBase64(tmp);
    //		console.log("Datauri l:", dataURI.length, i);
    //		i++;
    //		remaining = reportHTML.length - end;
    //	}
    //	console.log("in chunks", dataURI);
    //	console.log("in one", "data:text/plain;base64," + encodeBase64(reportHTML));
    //      }
    //console.log("Saving the report");

    //saveAs(dataURI, CodesetName + " full coding report.html");
    const b = new Blob([reportHTML], { type: 'text/html' });
    saveAs(b, CodesetName + " full coding report.html");
    this.saving = false;
  }


  ngOnDestroy() {
    //if (this.subOpeningReview) {
    //	this.subOpeningReview.unsubscribe();
    //}
  }
}
