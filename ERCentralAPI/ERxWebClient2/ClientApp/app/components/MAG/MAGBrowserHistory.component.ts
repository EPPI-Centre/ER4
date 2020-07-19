import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { NavigationEnd, Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagBrowseHistoryItem } from '../services/MAGClasses.service';

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
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private router: Router

    ) {

    }
    public currentBrowsePosition: number = 0;
    //public MAGBrowsingHistory: NavigationEnd[] = [];
    public magBrowseHistoryList: MagBrowseHistoryItem[] = [];

    ngOnInit() {

        //this.MAGBrowsingHistory = this._MAGBrowserHistoryService.getHistory();
        this.fetchMAGHistory();
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    GoToUrl(url: string) {

        console.log(url);
        this.router.navigate([url]);
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

        if (browsePosition > 0) {

            this.currentBrowsePosition = browsePosition;
           
            if (this.magBrowseHistoryList != null && browsePosition <= this.magBrowseHistoryList.length) {
                                                
                let mbh: MagBrowseHistoryItem = this.magBrowseHistoryList[browsePosition - 1];
                    switch (mbh.browseType) {
                        case "History":
                            this.ShowHistoryPage();
                            break;
                        case "Advanced":
                            this.ShowAdvancedPage();
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
    public ShowPaperDetailsPage(paperId: number, paperFullRecord: string, paperAbstract: string, urls: string,
        findOnWeb: string, linkedITEM_ID: number) {

        alert('not implemented yet');
    }
    public ShowMAGMatchesPage(incOrExc: string) {
        alert('not implemented yet');
    }
    public ShowAllWithThisCode(attributeIds: string) {
        alert('not implemented yet');
    }
    public ShowAutoIdentifiedMatches(magRelatedRunId: number) {
        alert('not implemented yet');
    }
    public ShowTopicPage(fieldOfStudyId: number, fieldOfStudy: string) {
        alert('not implemented yet');
    }
    public ShowSelectedPapersPage() {
        alert('not implemented yet');
    }
    public ShowRelatedPapersPage() {
        alert('not implemented yet');
    }
}
