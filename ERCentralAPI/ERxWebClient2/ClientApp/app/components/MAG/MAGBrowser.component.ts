import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Location } from '@angular/common';
import { searchService } from '../services/search.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router, NavigationEnd } from '@angular/router';
import { MagPaper,  MagFieldOfStudy, MagBrowseHistoryItem, topicInfo, MagList } from '../services/MAGClasses.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { TabStripComponent } from '@progress/kendo-angular-layout';
import { EventEmitterService } from '../services/EventEmitter.service';
import { Subscription } from 'rxjs';


@Component({
    selector: 'MAGBrowser',
    templateUrl: './MAGBrowser.component.html',
    providers: []
})

export class MAGBrowser implements OnInit, OnDestroy {

    constructor(
        public _magAdvancedService: MAGAdvancedService,
        public _magBrowserService: MAGBrowserService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _routingStateService: MAGBrowserHistoryService,
        private _location: Location,
        public _notificationService: NotificationService,
        public _eventEmitterService: EventEmitterService,
        private router: Router,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService
    ) {

    }
    @ViewChild('tabSelectedPapers') public tabstrip!: TabStripComponent;
    public browsingHistory: NavigationEnd[] = [];
    public MAGPapers: MagPaper[] = [];
    public description: string = '';
    public ShowSelectedPapers: string = '';
    public isShowDivIf = false;
    public kendoAfterDateValue: Date = new Date();
    public kendoBeforeDateValue: Date = new Date();
    public isCurrentSelected: boolean = false;
    public ShowOriginalPapers: boolean = false;
    public ShowingTopics: boolean = true;
    public getTopicsSub: Subscription | null = null;
    public getAttriubteIdsSub: Subscription | null = null;
    public basicOrigPanel: boolean = false;
    public currentMagPaperList: MagPaper[] = [];
    public currenPaperIsSelected: boolean = false;
    public get SelectedPapersTitle(): string {
        let ret: string = "Selected Papers (" + this._magBrowserService.selectedPapers.length + ")";
        return ret;
    }

    public ShowOrigPanel() {

        this.basicOrigPanel = !this.basicOrigPanel;
        this.ShowOriginalPapers = !this.ShowOriginalPapers;
    }
    ngAfterViewChecked () {

        this._eventEmitterService.selectedButtonPressed.subscribe(
            () => {
                if (this.tabstrip != null) {

                    this.tabstrip.selectTab(2);

                    let magBrowseItem: MagBrowseHistoryItem = new MagBrowseHistoryItem("Browse topic: SelectedPapers "
                        , "SelectedPapers", 0, "", "", 0, "", "",
                        0, "", "", 0);
                    this._mAGBrowserHistoryService.IncrementHistoryCount();
                    this._mAGBrowserHistoryService.AddToBrowseHistory(magBrowseItem);

                }
            }
        );

    }
    ngOnInit() {

        this._eventEmitterService.OpeningNewReview.subscribe(
            () => {
                //each time a new review is opened reset all data
                this.Clear();
                this._magBrowserService.Clear();
            }
        );

        
        this._magBrowserService.ShowingParentAndChildTopics = false;
        this._magBrowserService.ShowingChildTopicsOnly = true;
        this.getTopicsSub = this._eventEmitterService.getTopicsEvent.subscribe(
            (topicInfo: any) => {

                this._magBrowserService.GetParentAndChildFieldsOfStudy(topicInfo.fieldOfStudy, topicInfo.fieldOfStudyId).then(
                    () => { this.router.navigate(['MAGBrowser']); }
                );
            }
        );
     
        this._magBrowserService.MAGOriginalList.papers = this._magBrowserService.MAGList.papers;
        this._magBrowserService.OrigListCriteria = this._magBrowserService.ListCriteria;

    }
    public AddCurrentPaperToSelectedList() {

        this._magAdvancedService.currentMagPaper.isSelected = false; 
        if (this._magBrowserService.selectedPapers.length > 0 ) {
            console.log('inside here again....: ');
            let paper: MagPaper = this._magAdvancedService.currentMagPaper;
           
            let paperIndex: number = -1;
            paperIndex = this._magBrowserService.MAGList.papers.findIndex(x => x.paperId == paper.paperId) 
            if (paperIndex != -1) {
                this.currentMagPaperList = this._magBrowserService.MAGList.papers;
                this.currentMagPaperList[paperIndex].isSelected = true;
            } else if (this._magBrowserService.MagCitationsByPaperList.papers.findIndex(y => y.paperId == paper.paperId) != -1) {
                paperIndex = this._magBrowserService.MagCitationsByPaperList.papers.findIndex(y => y.paperId == paper.paperId);
                this.currentMagPaperList = this._magBrowserService.MagCitationsByPaperList.papers;
                this.currentMagPaperList[paperIndex].isSelected = true;
            } else if (this._magBrowserService.MAGOriginalList.papers.findIndex(z => z.paperId == paper.paperId) != -1) {
                paperIndex = this._magBrowserService.MAGOriginalList.papers.findIndex(z => z.paperId == paper.paperId)
                this.currentMagPaperList = this._magBrowserService.MAGOriginalList.papers;
                this.currentMagPaperList[paperIndex].isSelected = true;
            }
            console.log('gotta here', this._magAdvancedService.currentMagPaper);
            this.InOutReview(this._magAdvancedService.currentMagPaper, this.currentMagPaperList);
        }
    }
    public RemoveCurrentPaperToSelectedList() {

        if (this._magBrowserService.selectedPapers != null) {

            let paper: MagPaper = this._magAdvancedService.currentMagPaper;
            this._magAdvancedService.currentMagPaper.isSelected = true;
            let paperIndex: number = -1;
            paperIndex = this._magBrowserService.MAGList.papers.findIndex(x => x.paperId == paper.paperId)
            if (paperIndex != -1) {
                this.currentMagPaperList = this._magBrowserService.MAGList.papers;
                this.currentMagPaperList[paperIndex].isSelected = false;
            } else if (this._magBrowserService.MagCitationsByPaperList.papers.findIndex(y => y.paperId == paper.paperId) != -1) {
                paperIndex = this._magBrowserService.MagCitationsByPaperList.papers.findIndex(y => y.paperId == paper.paperId);
                this.currentMagPaperList = this._magBrowserService.MagCitationsByPaperList.papers;
                this.currentMagPaperList[paperIndex].isSelected = false;
            } else if (this._magBrowserService.MAGOriginalList.papers.findIndex(z => z.paperId == paper.paperId) != -1) {
                paperIndex = this._magBrowserService.MAGOriginalList.papers.findIndex(z => z.paperId == paper.paperId)
                this.currentMagPaperList = this._magBrowserService.MAGOriginalList.papers;
                this.currentMagPaperList[paperIndex].isSelected = false;
            }
            this.InOutReview(this._magAdvancedService.currentMagPaper, this.currentMagPaperList);
        }
    }
    public RefreshPapersBetweenDates() {

        this._magBrowserService.GetPaperListForTopicsAfterRefresh(this._magBrowserService.currentFieldOfStudy,
            this.kendoAfterDateValue, this.kendoBeforeDateValue);

    }
    onTabSelect(e: any) {

        //this.tabstrip.selectTab(e.index);
    }
    ngOnDestroy() {

        //this._magBrowserService.Clear();
        //this.Clear();
    }
    public UpdatePageSize(pageSize: number) {

        if (pageSize != null && pageSize > 0) {
            this._magBrowserService.pageSize = pageSize;
            this.GetParentAndChildRelatedPapers(this._magBrowserService.currentFieldOfStudy);
        }
    }
    public toggleDisplayDivIf() {
        this.isShowDivIf = !this.isShowDivIf;
    }
    public GetPDFLinks() : string[] {
        let links: string = this._magAdvancedService.currentMagPaper.pdfLinks;
        if (links != null && links != '') {
            var pdfLinks = links.split(';');
            return pdfLinks;
        } else {
            return [];      
        }
    }
    showMAGRunMessage(notifyMsg: string) {

        this._notificationService.show({
            content: notifyMsg,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
        });
    }
    public IsCurrentPaperSelected(): boolean {

        if (this._magBrowserService.selectedPapers != null && 
            this._magBrowserService.selectedPapers.length > 0) {

            let found: MagPaper =
                this._magBrowserService.selectedPapers.filter(x => x.paperId == this._magAdvancedService.currentMagPaper.paperId)[0];
            if (found != null && found != undefined && found.paperId != null && found.paperId > -1) {
                this.currenPaperIsSelected = true;
                return this.currenPaperIsSelected ;
            } else {
                this.currenPaperIsSelected = false;
                return this.currenPaperIsSelected ;
                }
        } else {
            this.currenPaperIsSelected = false;
            return this.currenPaperIsSelected ;
        }
    }
    public GetMagPaperRef(magPaperRefId: number, list: MagPaper[]) {

       
        this.currentMagPaperList = list;
        this._magBrowserService.ShowingParentAndChildTopics = false;
        this._magBrowserService.ShowingChildTopicsOnly = true;
        this._magAdvancedService.FetchMagPaperId(magPaperRefId).then(
            (result: MagPaper) => {

                let magBrowseItem: MagBrowseHistoryItem = new MagBrowseHistoryItem("Browse paper: " + result.fullRecord, "PaperDetail",
                    result.paperId, result.fullRecord,
                    result.abstract, result.linkedITEM_ID, result.urls, result.findOnWeb, 0, "", "", 0);
                this._mAGBrowserHistoryService.IncrementHistoryCount();
                this._mAGBrowserHistoryService.AddToBrowseHistory(magBrowseItem);

                this._magAdvancedService.PostFetchMagPaperCalls(result, "CitationsList");
            });
    }
    public Back() {
        this.router.navigate(['Main']);
    }
    private AddPaperToSelectedList(paperId: number, list: MagPaper[]) {
        console.log('paper we need: ', paperId);
        if (!this.IsInSelectedList(paperId)) {
        
            this._magBrowserService.SelectedPaperIds.push(paperId);
            console.log(this._magBrowserService.SelectedPaperIds);
            console.log(this._magBrowserService.selectedPapers);
            this.UpdateSelectedCount();
            this.AddToSelectedList(paperId, list);
        }
    }
    public AddToSelectedList(paperId: number, list: MagPaper[]) {

        for (var i = 0; i < this._magBrowserService.SelectedPaperIds.length; i++) {
            var item = list.filter(x => x.paperId == paperId)[0];
            if (item != null && this._magBrowserService.selectedPapers.findIndex(x => x.paperId == paperId) == -1 && item.paperId > 0) {
                console.log('inside the push part, pushing: ', item.paperId);
                this._magBrowserService.selectedPapers.push(item);
                item = new MagPaper();
            }
        }
    }
    private RemovePaperFromSelectedList(paperId: number, list: MagPaper[]): any {

        if (this.IsInSelectedList(paperId)) {
            let pos: number = this._magBrowserService.SelectedPaperIds.indexOf(paperId);
            if (pos > -1) {

                this._magBrowserService.SelectedPaperIds.splice(pos, 1);
                this._magBrowserService.selectedPapers.splice(pos, 1);
                this.UpdateSelectedCount();
            }
        }
    }
    public ClickedOnTopic: string = '';
    public GetParentAndChildRelatedPapers(item: MagFieldOfStudy) {

        this.ClickedOnTopic = item.displayName;
        this._magBrowserService.ShowingParentAndChildTopics = true;
        this._magBrowserService.ShowingChildTopicsOnly = false;
        let magBrowseItem: MagBrowseHistoryItem = new MagBrowseHistoryItem("Browse topic: " +
            item.displayName, "BrowseTopic", 0, "", "", 0, "", "",
            item.fieldOfStudyId, item.displayName, "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(magBrowseItem);
        this._magBrowserService.currentFieldOfStudy = item;
        let FieldOfStudyId: number = item.fieldOfStudyId;
        this._magBrowserService.ParentTopic = item.displayName;
        this._magBrowserService.WPChildTopics = [];
        this._magBrowserService.WPParentTopics = [];
        //this._magBrowserService.Clear();
        this._magAdvancedService.currentMagPaper = new MagPaper();
        this._magBrowserService.MagCitationsByPaperList = new MagList();
        this._magBrowserService.GetParentAndChildFieldsOfStudy("FieldOfStudyParentsList", FieldOfStudyId).then(
            () => {
                this._magBrowserService.GetParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", FieldOfStudyId).then(
                    () => {
                        this._magBrowserService.GetPaperListForTopic(FieldOfStudyId);
                    });
        });
    }
    
    public get HideSelectedPapers(): boolean {
        const len: number = this._magBrowserService.SelectedPaperIds.length;
        if (len > 0) {
            return false;
        } else {
            return true;
        }

    }
    public get DoesNotHaveCitations(): boolean {
        const len: number = this._magBrowserService.MagCitationsByPaperList.papers.length;
        if (len > 0) {
            return false;
        } else {
            return true;
        }
    }
    private IsInSelectedList(paperId: number): boolean {

        if (this._magBrowserService.SelectedPaperIds.indexOf(paperId) > -1)
            return true;
        else
            return false;
    }
    private UpdateSelectedCount(): any {
        this.ShowSelectedPapers = "Selected (" + this._magBrowserService.SelectedPaperIds.length.toString() + ")";
    }
    public InOutReview(paper: MagPaper, list: MagPaper[]) {
        if (paper.linkedITEM_ID == 0) {
            if (paper.isSelected) {

                this.RemovePaperFromSelectedList(paper.paperId, list);
                paper.isSelected = false;
            }
            else {
                paper.isSelected = true;
                this.AddPaperToSelectedList(paper.paperId, list);
                
            }            
        }
        else {
            this.showMAGRunMessage("This paper is already in your review");
        }
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get IsServiceBusy(): boolean {

        return this._magBrowserService.IsBusy || this._magAdvancedService.IsBusy;
    }
    public Clear() {
        //this._magBrowserService.SelectedPaperIds = [];
        this._magAdvancedService.currentMagPaper = new MagPaper();
        this.MAGPapers = [];

    }
    public CanDeleteMAGRun(): boolean {

        return this.HasWriteRights;
    }
    public CanAddNewMAGSearch(): boolean {

        if (this.description != '' && this.description != null && this.HasWriteRights
        ) {
            return true;
        } else {
            return false;
        }
    }
    public CanSelectMagItem(item: MagPaper): boolean {
        if (item.linkedITEM_ID > 0) {
            return false;
        } else {
            return true;
        }
    }
}
