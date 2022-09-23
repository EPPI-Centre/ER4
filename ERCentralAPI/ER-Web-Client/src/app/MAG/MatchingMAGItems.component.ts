import { Component, OnInit, ViewChild, OnDestroy, Output, EventEmitter, Input } from '@angular/core';
import { searchService } from '../services/search.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MVCMagPaperListSelectionCriteria, MagFieldOfStudy, MagPaper, TopicLink, MagBrowseHistoryItem }
    from '../services/MAGClasses.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MVCMagFieldOfStudyListSelectionCriteria } from '../services/MAGClasses.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MAGTopicsService } from '../services/MAGTopics.service';

@Component({
    selector: 'MatchingMAGItems',
    templateUrl: './MatchingMAGItems.component.html',
    providers: []
})

export class MatchingMAGItemsComponent implements OnInit, OnDestroy {

    constructor(private ConfirmationDialogService: ConfirmationDialogService,
        public _magBasicService: MAGRelatedRunsService,
        public _magAdvancedService: MAGAdvancedService,
        private _magBrowserService: MAGBrowserService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _eventEmitterService: EventEmitterService,
        private _notificationService: ConfirmationDialogService,
        private router: Router,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService,
        private _magTopicsService: MAGTopicsService
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
        //else {
        //    this.GetMagReviewMagInfoCommand();
        //}
    }
    ngOnDestroy() {
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

    public Back() {
        //this.router.navigate(['Main']);
    }
    public ClearAllMatching() {

        this.ConfirmationDialogService.confirm("Are you sure you wish to clear all matching in your review?", "", false, "")
            .then(
                (confirm: any) => {
                    if (confirm) {
                        this._notificationService.showMAGDelayMessage("Clearing all matches!");
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
                        this._notificationService.showMAGDelayMessage("Clearing all non-manual matches!");
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
                        this._notificationService.showMAGDelayMessage("Clearing all matches for specific code!");
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
                        this._notificationService.showMAGDelayMessage("Clearing all non-manual matches for specific code!");
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
                    let res: string = '';
                    //if (att != null && att.attribute_id > 0) {
                    if (matchType == 1) {
                        this._magAdvancedService.RunMatchingAlgorithm(att.attribute_id).then(
                            (result) => {
                               //msg = 'Are you sure you want to match all items with this code to Microsoft Academic records?';
                                res = result;
                            }
                            );
                    } else {
                        this._magAdvancedService.RunMatchingAlgorithm(0).then(
                            (result) => {
                                //msg = 'Are you sure you want to match all items to Microsoft Academic records?';
                                res = result;
                            }
                        );
                    }
                    this._magAdvancedService._RunAlgorithmFirst = true;

                    if (res != "error") {
                        this._notificationService.showMAGRunMessage('Matching records to OpenAlex. This can take a while...');
                    } else {
                        this._notificationService.showMAGRunMessage('Matching has returned an error please contact your administrator');
                    }
                }
            });
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
            this._magAdvancedService.AdvancedReviewInfo.nMatchedAccuratelyExcluded) > 0 ) {
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
                    this._notificationService.showMAGRunMessage('OpenAlex could not find the paperId!');
                }
            });
    }
    CloseCodeDropDown2() {
        if (this.WithOrWithoutCodeSelector2) {
            this.CurrentDropdownSelectedCode2 = this.WithOrWithoutCodeSelector2.SelectedNodeData;
        }
        this.isCollapsed2 = false;
    }
}

