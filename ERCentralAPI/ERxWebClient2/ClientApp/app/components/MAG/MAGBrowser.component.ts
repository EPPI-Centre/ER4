import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { searchService } from '../services/search.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router, NavigationEnd } from '@angular/router';
import { MagPaper,  MagFieldOfStudy, MagBrowseHistoryItem } from '../services/MAGClasses.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { TabStripComponent } from '@progress/kendo-angular-layout';
import { EventEmitterService } from '../services/EventEmitter.service';
import { Subscription } from 'rxjs';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { MAGTopicsService } from '../services/MAGTopics.service';


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
        public _notificationService: ConfirmationDialogService,
        public _eventEmitterService: EventEmitterService,
        private router: Router,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService,
        public _magTopicsService: MAGTopicsService
    ) {

    }
    @ViewChild('tabSelectedPapers') public tabstrip!: TabStripComponent;
    public ClickedOnTopic: string = '';
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
    public currentPaperIsSelected: boolean = false;
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

                    this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Browse topic: SelectedPapers "
                        , "SelectedPapers", 0, "", "", 0, "", "",
                        0, "", "", 0));
                }
            }
        );

    }

    ngOnInit() {

        this._eventEmitterService.firstVisitMAGBrowserPage = true;

        this._eventEmitterService.OpeningNewReview.subscribe(
            () => {
                this.Clear();
                this._magBrowserService.Clear();
            }
        );        
        this.getTopicsSub = this._eventEmitterService.getTopicsEvent.subscribe(
            (topicInfo: any) => {

                this._magBrowserService.GetParentAndChildFieldsOfStudy(topicInfo.fieldOfStudy, topicInfo.fieldOfStudyId).then(
                    () => {

                        this.router.navigate(['MAGBrowser']);
                    }
                );
            }
        );
    }

    public AddCurrentPaperToSelectedList() {

        this._magBrowserService.currentMagPaper.isSelected = false; 

        if (this._magBrowserService.selectedPapers != null ) {
            let paper: MagPaper = this._magBrowserService.currentMagPaper;
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
            this.InOutReview(this._magBrowserService.currentMagPaper, this.currentMagPaperList);
        }
    }

    public RemoveCurrentPaperToSelectedList() {

        if (this._magBrowserService.selectedPapers != null) {
            this._magBrowserService.currentMagPaper.isSelected = true;
            let paper: MagPaper = this._magBrowserService.currentMagPaper;
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
            } else {
                // means it is the current Mag Paper
                let tmp: number = this._magBrowserService.selectedPapers.findIndex(x => x.paperId == this._magBrowserService.currentMagPaper.paperId);
                if (tmp != -1) {
                    this._magBrowserService.selectedPapers[tmp].isSelected = true;
                }
            }
            

            this.InOutReview(this._magBrowserService.currentMagPaper, this.currentMagPaperList);
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

        this._magAdvancedService.firstVisitToMAGBrowser = false;

    }
    public toggleDisplayDivIf() {
        this.isShowDivIf = !this.isShowDivIf;
    }
    public GetPDFLinks() : string[] {
        let links: string = this._magBrowserService.currentMagPaper.pdfLinks;
        if (links != null && links != '') {
            var pdfLinks = links.split(';');
            return pdfLinks;
        } else {
            return [];      
        }
    }

    public GetAllLinks(): string[] {
        let links: string = this._magBrowserService.currentMagPaper.allLinks;
        if (links != null && links != '') {
            var allLinks = links.split(';');
            return allLinks;
        } else {
            return [];
        }
    }

    public IsCurrentPaperSelected(paperId: number): boolean {

        if (this._magBrowserService.selectedPapers != null && 
            this._magBrowserService.selectedPapers.length > 0) {

            let pos: number = this._magBrowserService.selectedPapers.findIndex(x => x.paperId == paperId);
            if (pos > -1) {
                this.currentPaperIsSelected = true;
                return this.currentPaperIsSelected ;
            } else {
                this.currentPaperIsSelected = false;
                return this.currentPaperIsSelected ;
            }
        } else {
            this.currentPaperIsSelected = false;
            return this.currentPaperIsSelected ;
        }
    }

    public GetMagPaperRef(magPaperRefId: number, list: MagPaper[]) {

        this._magBrowserService.currentRefreshListType = 'GetMagPaperRef';
        this.currentMagPaperList = list;
        this._magTopicsService.ShowingParentAndChildTopics = false;
        this._magTopicsService.ShowingChildTopicsOnly = true;
        this._magTopicsService.WPParentTopics = [];
        this._magTopicsService.WPChildTopics = [];
        this._magAdvancedService.FetchMagPaperId(magPaperRefId).then(
            (result: MagPaper) => {

                this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Browse paper: " + result.fullRecord, "PaperDetail",
                    result.paperId, result.fullRecord,
                    result.abstract, result.linkedITEM_ID, result.allLinks, result.findOnWeb, 0, "", "", 0));
                this._magAdvancedService.PostFetchMagPaperCalls(result, "CitationsList");
            });
    }

    public Back() {
        this.router.navigate(['Main']);
    }
    private AddPaperToSelectedList(paperId: number, list: MagPaper[]) {

        if (!this.IsInSelectedList(paperId)) {
        
            this._magBrowserService.SelectedPaperIds.push(paperId);
            console.log(this._magBrowserService.SelectedPaperIds);
            console.log(this._magBrowserService.selectedPapers);
            this.UpdateSelectedCount();
            this.AddToSelectedList(paperId, list);
        }
    }

    public AddToSelectedList(paperId: number, list: MagPaper[]) {

        let IdsListPos: number = this._magBrowserService.SelectedPaperIds.indexOf(paperId);
        let PapersListPos: number = this._magBrowserService.selectedPapers.findIndex(x => x.paperId == paperId);
        if (IdsListPos != -1 && PapersListPos != -1) {
            if (this._magBrowserService.currentMagPaper.paperId > 0) {
                this._magBrowserService.selectedPapers.push(this._magBrowserService.currentMagPaper);
                let foundPaper: number = this._magBrowserService.MAGList.papers.findIndex(x => x == this._magBrowserService.currentMagPaper);
                if (foundPaper > -1) {
                    
                    //._magBrowserService.MAGList.papers[foundPaper].isSelected = true;
                }
                let foundPaperOrig: number = this._magBrowserService.MAGOriginalList.papers.findIndex(x => x == this._magBrowserService.currentMagPaper);
                if (foundPaperOrig > -1) {
                    //this._magBrowserService.MAGOriginalList.papers[foundPaperOrig].isSelected = true;
                }
                let foundPaperCit: number = this._magBrowserService.MagCitationsByPaperList.papers.findIndex(x => x == this._magBrowserService.currentMagPaper);
                if (foundPaperCit > -1) {
                    //this._magBrowserService.MagCitationsByPaperList.papers[foundPaperCit].isSelected = true;
                }
                console.log(this._magBrowserService.currentMagPaper);
            }
        } else {

            var itemPos = list.findIndex(x => x.paperId == paperId);
            if (itemPos > -1) {
                var item = list[itemPos];
                this._magBrowserService.selectedPapers.push(item);
                let foundPaper: number = this._magBrowserService.MAGList.papers.findIndex(x => x == item);
                if (foundPaper > -1) {
                    //this._magBrowserService.MAGList.papers[foundPaper].isSelected = true;
                }
                let foundPaperOrig: number =  this._magBrowserService.MAGOriginalList.papers.findIndex(x => x == item);
                if (foundPaperOrig > -1) {
                    //this._magBrowserService.MAGOriginalList.papers[foundPaperOrig].isSelected = true;
                }
                let foundPaperCit: number = this._magBrowserService.MagCitationsByPaperList.papers.findIndex(x => x == item);
                if (foundPaperCit > -1) {
                    //this._magBrowserService.MagCitationsByPaperList.papers[foundPaperCit].isSelected = true;
                }
                console.log(item);
            }
        }
    }

    private RemovePaperFromSelectedList(paperId: number, list: MagPaper[]): any {

        if (this.IsInSelectedList(paperId)) {
            let pos: number = this._magBrowserService.SelectedPaperIds.indexOf(paperId);
            if (pos > -1) {
                this._magBrowserService.SelectedPaperIds.splice(pos, 1);
                this.UpdateSelectedCount();
            }
            let pos2: number = this._magBrowserService.selectedPapers.findIndex( x => x.paperId == paperId);
            if (pos2 > -1) {
                this._magBrowserService.selectedPapers.splice(pos2, 1);
            }
        }
    }

    public async GetParentAndChildRelatedPapers(item: MagFieldOfStudy) {

        this._magBrowserService.MagCitationsByPaperList.papers = [];
        this._eventEmitterService.firstVisitMAGBrowserPage = false;
        this.ClickedOnTopic = item.displayName;

        await this._magBrowserService.GetTopicsAndRelatedPapers(item);

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

            if (this.IsCurrentPaperSelected(paper.paperId)) {
                console.log('is selected making not selected');
                this.RemovePaperFromSelectedList(paper.paperId, list);
                paper.isSelected = false;
            }
            else {
                console.log('is not selected making selected');
                paper.isSelected = true;
                console.log('this is the paper I clicked on at the top: ', paper);
                this.AddPaperToSelectedList(paper.paperId, list);
                
            }            
        }
        else {
            this._notificationService.showMAGRunMessage("This paper is already in your review");
        }
    }

    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get IsServiceBusy(): boolean {

        return this._magBrowserService.IsBusy || this._magAdvancedService.IsBusy;
    }
    public Clear() {
        this._magBrowserService.currentMagPaper = new MagPaper();
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

        if (item.linkedITEM_ID > 0 ) {
            return false;
        } else {
            return true;
        }
    }


}
