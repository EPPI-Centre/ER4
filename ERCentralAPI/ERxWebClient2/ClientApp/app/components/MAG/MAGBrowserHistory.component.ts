import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import {  Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagBrowseHistoryItem, topicInfo, MagPaper } from '../services/MAGClasses.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { of } from 'rxjs';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { BasicMAGService } from '../services/BasicMAG.service';

@Component({
    selector: 'MAGBrowserHistory',
    templateUrl: './MAGBrowserHistory.component.html',
    providers: []
})

export class MAGBrowserHistory implements OnInit {

    constructor(
        private _location: Location,
        public _MAGBrowserHistoryService: MAGBrowserHistoryService,
        public _magAdvancedService: MAGAdvancedService,
        public _magBasicService: BasicMAGService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private router: Router,
        public _eventEmitterService: EventEmitterService,
        private _magBrowserService: MAGBrowserService
    ) {

    }

    //public MAGBrowsingHistory: NavigationEnd[] = [];
    public magBrowseHistoryList: MagBrowseHistoryItem[] = [];
    ngOnInit() {

        //this.MAGBrowsingHistory = this._MAGBrowserHistoryService.getHistory();
        this.fetchMAGHistory();
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    GoToUrl(index: number) {

        console.log('go to url', index);
        //this.router.navigate([url]);
        this.NavigateToThisPoint(index);
    }
    //RemoveUrl(item: NavigationEnd) {

    //    let id: number = item.id;
    //    let index: number = this.MAGBrowsingHistory.findIndex(x => x.id == id);
    //    if (index != -1) {
    //    this.MAGBrowsingHistory.splice(index,1);
    //    }
    //}
    public ClearHistory() {
        this._MAGBrowserHistoryService.ClearHistory();
    }
    public get IsServiceBusy(): boolean {

        return false;
    }
    Back() {
        this._location.back();
    }
    fetchMAGHistory() {

        this._MAGBrowserHistoryService.FetchMAGBrowserHistory();
    }
    public NavigateToThisPoint(browsePosition: number) {
      
        if (browsePosition > -1) {

            this._MAGBrowserHistoryService.currentBrowsePosition = browsePosition;
            console.log('browsePosition', browsePosition);
            console.log('this.magBrowseHistoryList.length', this.magBrowseHistoryList.length);
            if (this._MAGBrowserHistoryService._MAGBrowserHistoryList != null && browsePosition <= this._MAGBrowserHistoryService._MAGBrowserHistoryList.length) {
                console.log('inside navigate to this point second if');
                let mbh: MagBrowseHistoryItem = this._MAGBrowserHistoryService._MAGBrowserHistoryList[browsePosition];
                console.log('inside navigate to this point object:', JSON.stringify(mbh));
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
                            this.ShowPaperDetailsPage(mbh.paperId, mbh.paperFullRecord, mbh.paperAbstract, mbh.uRLs,
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
                        case "RelatedPapers":
                            this.ShowRelatedPapersPage();
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
    public ShowPaperDetailsPage(paperId: number, paperFullRecord: string, paperAbstract: string, urls: string,
        findOnWeb: string, linkedITEM_ID: number) {

        this._magAdvancedService.FetchMagPaperId(paperId).then(

            (result: MagPaper) => {
                if (result.paperId != null && result.paperId > 0) {
                    let magBrowseItem: MagBrowseHistoryItem = new MagBrowseHistoryItem("Go to specific Paper Id: " + result.fullRecord, "PaperDetail", result.paperId, result.fullRecord,
                        result.abstract, result.linkedITEM_ID, result.urls, result.findOnWeb, 0, "", "", 0);
                    this._MAGBrowserHistoryService.IncrementHistoryCount();
                    this._MAGBrowserHistoryService.AddToBrowseHistory(magBrowseItem);
                    this._magAdvancedService.PostFetchMagPaperCalls(result);
                } else {
                    this._magBasicService.showMAGRunMessage('Microsoft academic could not find the paperId!');
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
    public ShowAllWithThisCode(attributeIds: string) {

        this._eventEmitterService.getAttributeIdsEvent.emit(attributeIds);
    }
    public ShowAutoIdentifiedMatches(magRelatedRunId: number) {
        //this._magBasicService.FetchMagRelatedPapersRunList();
    }
    public ShowRelatedPapers() {
        this._magBasicService.FetchMagRelatedPapersRunList();
        this.router.navigate(['BasicMAGFeatures']);
    }
    public ShowTopicPage(fieldOfStudyId: number, fieldOfStudy: string) {

        this._magAdvancedService.currentMagPaper = new MagPaper();
        this._magBrowserService.WPChildTopics = [];
        this._magBrowserService.WPParentTopics = [];
        this._magBrowserService.ParentTopic = '';
        this.router.navigate(['MAGBrowser']);
        this.GetParentAndChildRelatedPapers(fieldOfStudy, fieldOfStudyId);

    }
    public ShowSelectedPapersPage() {

        alert('not implemented yet');
    }
    public ShowRelatedPapersPage() {

        alert('not implemented yet');
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
