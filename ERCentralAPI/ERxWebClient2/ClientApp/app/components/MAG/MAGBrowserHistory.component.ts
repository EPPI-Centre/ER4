import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import {  Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagBrowseHistoryItem, MagPaper, MVCMagPaperListSelectionCriteria } from '../services/MAGClasses.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MAGAdminService } from '../services/MAGAdmin.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

@Component({
    selector: 'MAGBrowserHistory',
    templateUrl: './MAGBrowserHistory.component.html',
    providers: []
})

export class MAGBrowserHistory implements OnInit {

    constructor(
        private notificationService: ConfirmationDialogService,
        public _MAGBrowserHistoryService: MAGBrowserHistoryService,
        public _magAdvancedService: MAGAdvancedService,
        public _magBasicService: MAGRelatedRunsService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private router: Router,
        public _eventEmitterService: EventEmitterService,
        private _magBrowserService: MAGBrowserService,
        public _magAdminService: MAGAdminService
    ) {

    }

    public magBrowseHistoryList: MagBrowseHistoryItem[] = [];
    ngOnInit() {
        this.fetchMAGHistory();
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    GoToUrl(index: number) {

        console.log('go to url', index);
        this.NavigateToThisPoint(index);
    }
    RemoveUrl(index: number) {

        if (index != -1) {
            this._MAGBrowserHistoryService._MAGBrowserHistoryList.splice(index,1);
        }
    }
    public ClearHistory() {
        this._MAGBrowserHistoryService._MAGBrowserHistoryList = [];
    }
    public get IsServiceBusy(): boolean {

        return false;
    }
    Back() {
        this.router.navigate(['Main']);
    }
    fetchMAGHistory() {

        this._MAGBrowserHistoryService.FetchMAGBrowserHistory();
    }
    public NavigateToThisPoint(browsePosition: number) {
      
        if (browsePosition > -1) {

            this._MAGBrowserHistoryService.currentBrowsePosition = browsePosition;
            if (this._MAGBrowserHistoryService._MAGBrowserHistoryList != null && browsePosition <= this._MAGBrowserHistoryService._MAGBrowserHistoryList.length) {
                let mbh: MagBrowseHistoryItem = this._MAGBrowserHistoryService._MAGBrowserHistoryList[browsePosition];
                    switch (mbh.browseType) {
                        case "History":
                            this.ShowHistoryPage();
                            break;
                        case "Advanced":
                            this.ShowAdvancedPage();
                            break;
                        case "Admin":
                            this.ShowAdminPage();
                            break;
                        case "RelatedPapers":
                            this.ShowRelatedPapers();
                            break;
                        case "matching":
                            this.ShowMatching();
                            break;
                        case "PaperDetail":
                            this.ShowPaperDetailsPage(mbh.paperId, mbh.paperFullRecord, mbh.paperAbstract, mbh.allLinks,
                                mbh.findOnWeb, mbh.linkedITEM_ID);
                            break;
                        case "MatchesIncluded":
                            this.ShowMAGMatchesPage("included");
                            break;
                        case "MatchesExcluded":
                            this.ShowMAGMatchesPage("excluded");
                            break;
                        case "MatchesIncludedAndExcluded":
                            this.ShowMAGMatchesPage("all");
                            break;
                        case "ReviewMatchedPapersWithThisCode":
                            this.ShowAllWithThisCode(mbh.attributeIds);
                            break;
                        case "MagRelatedPapersRunList":
                            this.ShowAutoIdentifiedMatches(mbh.magRelatedRunId);
                            break;
                        case "BrowseTopic":
                            this.ShowTopicPage(mbh.fieldOfStudyId, mbh.fieldOfStudy);
                            break;
                        case "SelectedPapers":
                            this.ShowSelectedPapersPage();
                            break;
                        case "KeepUpdated":
                            this.ShowKeepUpToDate();
                            break; 
                    }
            }
        }
    }
    public ShowHistoryPage() {
        this.router.navigate(['MAGBrowserHistory']);
    }
    public ShowAdvancedPage() {
        this.router.navigate(['AdvancedMAGFeatures']);
    }
    public ShowAdminPage() {
        this.router.navigate(['MAGAdmin']);
    }
    public ShowMatching() {
        this.router.navigate(['MatchingMAGItems']);
    }
    public ShowKeepUpToDate() {
        this.router.navigate(['MAGKeepUpToDate']);
    } 
    public ShowPaperDetailsPage(paperId: number, paperFullRecord: string, paperAbstract: string, urls: string,
        findOnWeb: string, linkedITEM_ID: number) {

        this._magAdvancedService.FetchMagPaperId(paperId).then(

            (result: MagPaper) => {
                if (result.paperId != null && result.paperId > 0) {
                    this._MAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Go to specific Paper Id: " + result.fullRecord, "PaperDetail", result.paperId, result.fullRecord,
                        result.abstract, result.linkedITEM_ID, result.allLinks, result.findOnWeb, 0, "", "", 0));
                    this._magAdvancedService.PostFetchMagPaperCalls(result,'');
                } else {
                    this.notificationService.showMAGRunMessage('Microsoft academic could not find the paperId!');
                }
            });
    }
    public ShowMAGMatchesPage(incOrExc: string) {

        if (incOrExc == 'included') {
            this._eventEmitterService.getMatchedIncludedItemsEvent.emit();
        } else if (incOrExc == 'excluded') {
            this._eventEmitterService.getMatchedExcludedItemsEvent.emit();
        } else if (incOrExc == 'all') {
            this._eventEmitterService.getMatchedAllItemsEvent.emit();
        } else {
            //          there is an error
        }
        
    }
    public ShowAllWithThisCode(attributeId: string) {
        
            let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            criteria.listType = "ReviewMatchedPapersWithThisCode";
            criteria.attributeIds = attributeId;
            criteria.pageSize = 20;
            this._magAdvancedService.FetchMagPaperListMagPaper(criteria).then(
                () => {
                    this.router.navigate(['MAGBrowser']);
                }
            );
    }
    public ShowAutoIdentifiedMatches(magRelatedRunId: number) {
        this._magBrowserService.FetchMAGRelatedPaperRunsListById(magRelatedRunId)
            .then(
                () => {
                    this.router.navigate(['MAGBrowser']);
                }
            );
    }
    public ShowRelatedPapers() {
        this._magBasicService.FetchMagRelatedPapersRunList();
        this.router.navigate(['BasicMAGFeatures']);
    }
    public ShowTopicPage(fieldOfStudyId: number, fieldOfStudy: string) {

        this._magBrowserService.currentMagPaper = new MagPaper();
        this._magBrowserService.WPChildTopics = [];
        this._magBrowserService.WPParentTopics = [];
        this._magBrowserService.ParentTopic = '';
        this.router.navigate(['MAGBrowser']);
        this.GetParentAndChildRelatedPapers(fieldOfStudy, fieldOfStudyId);

    }
    public ShowSelectedPapersPage() {
        this.router.navigate(['MAGBrowser']);
        //this._magBrowserService.onTabSelect(2);
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
}
