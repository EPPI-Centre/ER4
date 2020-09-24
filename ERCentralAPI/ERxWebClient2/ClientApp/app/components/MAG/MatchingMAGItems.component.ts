import { Component, OnInit, ViewChild, OnDestroy, EventEmitter } from '@angular/core';
import { Location } from '@angular/common';
import { searchService } from '../services/search.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router, NavigationEnd } from '@angular/router';
import { MVCMagPaperListSelectionCriteria, MagFieldOfStudy, MagPaper, TopicLink, MagBrowseHistoryItem }
    from '../services/MAGClasses.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MVCMagFieldOfStudyListSelectionCriteria } from '../services/MAGClasses.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { BasicMAGService } from '../services/BasicMAG.service';
import { NotificationService } from '@progress/kendo-angular-notification';

@Component({
    selector: 'MatchingMAGItems',
    templateUrl: './MatchingMAGItems.component.html',
    providers: []
})

export class MatchingMAGItemsComponent implements OnInit, OnDestroy {

    //history: NavigationEnd[] = [];
    constructor(private ConfirmationDialogService: ConfirmationDialogService,
        public _magBasicService: BasicMAGService,
        public _magAdvancedService: MAGAdvancedService,
        private _magBrowserService: MAGBrowserService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _eventEmitterService: EventEmitterService,
        private _routingStateService: MAGBrowserHistoryService,
        private _location: Location,
        private _notificationService: NotificationService,
        private router: Router,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService

    ) {

        //this.history = this._routingStateService.getHistory();
    }
    //public sub: Subscription = new Subscription();
    public SearchTextTopic: string = '';
    ngOnInit() {
         this._eventEmitterService.getMatchedIncludedItemsEvent.subscribe(
            () => {
                this.GetMatchedMagIncludedList();
            }
        );
        this._eventEmitterService.getMatchedExcludedItemsEvent.subscribe(
            () => {
                this.GetMatchedMagExcludedList();
            }
        );
        this._eventEmitterService.getMatchedAllItemsEvent.subscribe(
            () => {
                this.GetMatchedMagAllList();
            }
        );
        if (this._ReviewerIdentityServ.reviewerIdentity.userId == 0 ||
            this._ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else if (!this._ReviewerIdentityServ.HasWriteRights) {
            this.router.navigate(['Main']);
        }
        else {
            this.GetMagReviewMagInfoCommand();
        }
    }
    ngOnDestroy() {
        //if (this.sub != null) {
        //    this.sub.unsubscribe();
        //}
    }
    @ViewChild('WithOrWithoutCodeSelector2') WithOrWithoutCodeSelector2!: codesetSelectorComponent;

    public CurrentDropdownSelectedCode2: singleNode | null = null;
    public dropdownBasic2: boolean = false;
    public isCollapsed2: boolean = false;
    public ListSubType: string = '';
    public SearchTextTopics: TopicLink[] = [];
    public SearchTextTopicsResults: TopicLink[] = [];
    public magPaperId: number = 0;
    public AdvancedFeatures() {

        this.router.navigate(['AdvancedMAGFeatures']);

    }
    public Back() {
        this._location.back();
    }
    public ClearAllMatching() {

        this.ConfirmationDialogService.confirm("Are you sure you wish to clear all matching in your review?", "", false, "")
            .then(
                (confirm: any) => {
                    if (confirm) {
                        this._magAdvancedService.ClearAllMAGMatches(0);
                        this._notificationService.show({
                            content: "Clearing all matches!",
                            animation: { type: 'slide', duration: 400 },
                            position: { horizontal: 'center', vertical: 'top' },
                            type: { style: "warning", icon: true },
                            hideAfter: 20000
                        });
                    }
                }
            )
            .catch(() => { });
     
           
    }
    public ClearMatches() {

        this.ConfirmationDialogService.confirm("Are you sure you want to match all items with this code to Microsoft Academic records?", "", false, "")
            .then(
                (confirm: any) => {
                    if (confirm) {
                        let attribute = this.CurrentDropdownSelectedCode2 as SetAttribute;
                        if (attribute != null) {
                            this._magAdvancedService.ClearAllMAGMatches(attribute.attribute_id);
                        }
                        this._notificationService.show({
                            content: "Clearing all matches for specific attribute!",
                            animation: { type: 'slide', duration: 400 },
                            position: { horizontal: 'center', vertical: 'top' },
                            type: { style: "warning", icon: true },
                            hideAfter: 20000
                        });
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
    public UpdateTopicResults() {

        
        if (this.SearchTextTopic.length > 2 ) {
 
            let criteriaFOSL: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
            criteriaFOSL.fieldOfStudyId = 0;
            criteriaFOSL.listType = 'FieldOfStudySearchList';
            criteriaFOSL.paperIdList = '';
            criteriaFOSL.SearchTextTopics = this.SearchTextTopic;
            this._magBrowserService.FetchMagFieldOfStudyList(criteriaFOSL, '').then(

                (results: MagFieldOfStudy[]) => {

                    //this.WPFindTopics = [];
                    let FosList: MagFieldOfStudy[] = results;
                    let i: number = 1.7;
                    let cnt: number = 0;
                    for (var fos of FosList)
                    {
                        console.log('got in here');
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
            );

        } else {

            this.SearchTextTopics = [];
            this.SearchTextTopicsResults = [];
        }
    }
    public FOSMAGBrowserNavigate(displayName: string, fieldOfStudyId: number) {

        let magBrowseItem: MagBrowseHistoryItem = new MagBrowseHistoryItem(displayName, "BrowseTopic", 0,
            "", "", 0, "", "", fieldOfStudyId, displayName, "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(magBrowseItem);
        this._magAdvancedService.currentMagPaper = new MagPaper();
        this._magBrowserService.WPChildTopics = [];
        this._magBrowserService.WPParentTopics = [];
        this._magBrowserService.ParentTopic = '';
        this.router.navigate(['MAGBrowser']);
        this.GetParentAndChildRelatedPapers(displayName, fieldOfStudyId);
    }
    public GetParentAndChildRelatedPapers(FieldOfStudy: string, FieldOfStudyId: number) {

        this._magBrowserService.ParentTopic = FieldOfStudy;

        this._magBrowserService.GetParentAndChildFieldsOfStudy("FieldOfStudyParentsList", FieldOfStudyId).then(
            () => {
                this._magBrowserService.GetParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", FieldOfStudyId).then(
                    () => {
                        this._magBrowserService.GetPaperListForTopic(FieldOfStudyId);
                    });
            });
    }
    public RunMatchingAlgo() {

        var att = this.CurrentDropdownSelectedCode2 as SetAttribute;
        let msg: string = ''; 
        if (att != null && att.attribute_id > 0) {
            msg = 'Are you sure you want to match all items with this code to Microsoft Academic records?';
        } else {
            msg = 'Are you sure you want to match all items to Microsoft Academic records?';
        }
        this.ConfirmationDialogService.confirm('Run matching algorithm', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    let res: string = '';
                    if (att != null && att.attribute_id > 0) {
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
                        this._magBasicService.showMAGRunMessage('MAG Matching can take a while...');
                    } else {
                        this._magBasicService.showMAGRunMessage('MAG Matching has returned an error please contact your administrator');
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
        this._magAdvancedService.currentMagPaper = new MagPaper();
        this._magBrowserService.WPChildTopics = [];
        this._magBrowserService.WPParentTopics = [];
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
    public GetMatchedMagIncludedList(): void {

        this._magBrowserService.ShowingParentAndChildTopics = false;
        this._magBrowserService.ShowingChildTopicsOnly = true;
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("List of all included matches", "MatchesIncluded", 0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);

        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapers";
        criteria.included = "Included";
        criteria.pageSize = 20;
        this._magBrowserService.FetchWithCrit(criteria, "ReviewMatchedPapers").then(
           
            () => {

                let criteria2: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
                criteria2.fieldOfStudyId = 0;
                criteria2.listType = 'PaperFieldOfStudyList';
                criteria2.paperIdList = this._magBrowserService.ListCriteria.paperIds;
                criteria2.SearchTextTopics = ''; //TODO this will be populated by the user..
                this._magBrowserService.FetchMagFieldOfStudyList(criteria2, 'ReviewMatchedPapers').then(

                    () => { this.router.navigate(['MAGBrowser']); }
                );
            }
        );
    }
    public GetMatchedMagExcludedList() {
        this._magBrowserService.ShowingParentAndChildTopics = false;
        this._magBrowserService.ShowingChildTopicsOnly = true;
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("List of all excluded matches", "MatchesExcluded", 0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);

        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapers";
        criteria.included = "Excluded";
        criteria.pageSize = 20;

        this._magBrowserService.FetchWithCrit(criteria, "ReviewMatchedPapers").then(

            () => {

                let criteria2: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
                criteria2.fieldOfStudyId = 0;
                criteria2.listType = 'PaperFieldOfStudyList';
                criteria2.paperIdList = this._magBrowserService.ListCriteria.paperIds;
                criteria2.SearchTextTopics = ''; //TODO this will be populated by the user..
                this._magBrowserService.FetchMagFieldOfStudyList(criteria2, 'ReviewMatchedPapers').then(

                    () => { this.router.navigate(['MAGBrowser']); }
                );
            }
        );
    }
    public GetMatchedMagAllList() {
        this._magBrowserService.ShowingParentAndChildTopics = false;
        this._magBrowserService.ShowingChildTopicsOnly = true;
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("List of all matches in review (included and excluded)", "MatchesIncludedAndExcluded",
            0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapers";
        criteria.included = "all";
        criteria.pageSize = 20;

        this._magBrowserService.FetchWithCrit(criteria, "ReviewMatchedPapers").then(

            () => {

                let criteria2: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
                criteria2.fieldOfStudyId = 0;
                criteria2.listType = 'PaperFieldOfStudyList';
                criteria2.paperIdList = this._magBrowserService.ListCriteria.paperIds;
                criteria2.SearchTextTopics = ''; //TODO this will be populated by the user..
                this._magBrowserService.FetchMagFieldOfStudyList(criteria2, 'ReviewMatchedPapers').then(

                    () => { this.router.navigate(['MAGBrowser']); }
                );
            }
        );
    }
    public CanGetCodeMatches(): boolean {
        if (this.CurrentDropdownSelectedCode2 != null) {
            return true;
        } else {
            return false;
        }
    }
    public GetMatchedMagWithCodeList() {
        if (this.CurrentDropdownSelectedCode2 != null) {
            this._magBrowserService.ShowingParentAndChildTopics = false;
            this._magBrowserService.ShowingChildTopicsOnly = true;
            let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            criteria.listType = "ReviewMatchedPapersWithThisCode";
            var att = this.CurrentDropdownSelectedCode2 as SetAttribute;
            criteria.attributeIds = att.attribute_id.toString();
            criteria.pageSize = 20;
            let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("List of all item matches with this code", "ReviewMatchedPapersWithThisCode", 0,
                "", "", 0, "", "", 0, "", criteria.attributeIds, 0);
            this._mAGBrowserHistoryService.IncrementHistoryCount();
            this._mAGBrowserHistoryService.AddToBrowseHistory(item);

            this._magAdvancedService.FetchMagPaperListMagPaper(criteria).then(
                () => {
                    this.router.navigate(['MAGBrowser']);
                }
            );
        }
    }
    public CanGetMagPaper(): boolean {

        if (this.magPaperId != null && this.magPaperId > 0) {
            return true;
        } else {
            return false;
        }

    }
    public GetMagPaper() {

        this._magAdvancedService.FetchMagPaperId(this.magPaperId).then(
            
            (result: MagPaper) => {
                if (result.paperId != null && result.paperId > 0) {
                    this._magBrowserService.ShowingParentAndChildTopics = false;
                    this._magBrowserService.ShowingChildTopicsOnly = true;
                    let magBrowseItem: MagBrowseHistoryItem = new MagBrowseHistoryItem("Go to specific Paper Id: " + result.fullRecord, "PaperDetail", result.paperId, result.fullRecord,
                        result.abstract, result.linkedITEM_ID, result.urls, result.findOnWeb, 0, "", "", 0);
                    this._mAGBrowserHistoryService.IncrementHistoryCount();
                    this._mAGBrowserHistoryService.AddToBrowseHistory(magBrowseItem);
                    this._magAdvancedService.PostFetchMagPaperCalls(result, '');
                } else {
                    this._magBasicService.showMAGRunMessage('Microsoft academic could not find the paperId!');
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

