import { Component,  OnInit, ViewChild, EventEmitter, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MagItemPaperInsertCommand, MagBrowseHistoryItem, MVCMagPaperListSelectionCriteria, MagRelatedPapersRun, MagSearch, MagPaper, MagFieldOfStudy, MagList } from '../services/MAGClasses.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { Helpers } from '../helpers/HelperMethods';
import { MAGAdvancedService } from '../services/magAdvanced.service';

@Component({
    selector: 'MAGHeaderBar',
    templateUrl: './MAGHeaderBar.component.html',
    providers: []
})
export class MAGHeaderBarComp implements OnInit {

    constructor(private router: Router,
        private _location: Location,
        private _magBrowserService: MAGBrowserService,
        private _magAdvancedService: MAGAdvancedService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        public _notificationService: NotificationService,
        public _eventEmitterService: EventEmitterService,
        public _confirmationDialogService: ConfirmationDialogService,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService
    ) {

    }

    
    ngOnInit() {
	
    }
    @Input() Context: string | undefined;
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get isSiteAdmin(): boolean {
        return this._ReviewerIdentityServ.reviewerIdentity.isSiteAdmin;
    }
    public ShowSelectedPapers: string = "Selected (" + this._magBrowserService.SelectedPaperIds.length.toString() + ")";
    public  get SelectedItems() : boolean {

        if (this._magBrowserService.selectedPapers != null && 
            this._magBrowserService.selectedPapers.length >0 ) {
            return true;
        } else {
            return false;
        }
    }
    public DisableButton(destination: string) {
        if (this.Context == undefined || !this.HasWriteRights) return false;
        else if (this.Context == destination) return true;
        else return false;
    }
    public Forward() {
        
        this._location.forward();
    }
    public Back() {
        
        this._location.back();
    }
    public AdvancedFeatures() {

        this.router.navigate(['AdvancedMAGFeatures']);
    }
    public Selected() {
        if (this.Context == "MAGBrowser") {
            this._eventEmitterService.selectedButtonPressed.emit();
        } else {
            let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("Browse topic: SelectedPapers "
                , "SelectedPapers", 0, "", "", 0, "", "",
                0, "", "", 0);
            this._mAGBrowserHistoryService.IncrementHistoryCount();
            this._mAGBrowserHistoryService.AddToBrowseHistory(item);
            
            this.router.navigate(['MAGBrowser']).then(
                async (res) =>  {
                    if (res) {
                        await Helpers.Sleep(50);
                        this._eventEmitterService.selectedButtonPressed.emit();
                    }
                    //this._eventEmitterService.tool = true;
                }
            );
        }
    }
    public ClearSelected() {
        let msg: string = 'Are you sure you want to clear the ' + this._magBrowserService.selectedPapers.length + '  selected MAG papers into your review?';
        this._confirmationDialogService.confirm('MAG Selected Papers', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magBrowserService.ClearSelected();
                }
            });
        
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
    public ImportSelected() {

        let msg: string = 'Are you sure you want to import the ' + this._magBrowserService.selectedPapers.length + '  selected MAG papers into your review?';
        this._confirmationDialogService.confirm('MAG Import', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this.ConfirmedImport();
                }
            });
    }
    public ConfirmedImport() {

        let notificationMsg: string = '';
        this._magBrowserService.ImportMagRelatedSelectedPapers(this._magBrowserService.SelectedPaperIds).then(

            (result: MagItemPaperInsertCommand | void) => {
                if (result != null && result.nImported != null) {
                    if (result.nImported == this._magBrowserService.SelectedPaperIds.length) {

                        notificationMsg += "Imported " + result.nImported + " out of " +
                            this._magBrowserService.SelectedPaperIds.length + " items";
                        
                    } else if (result.nImported != 0) {

                        notificationMsg += "Some of these items were already in your review.\n\nImported " +
                            result.nImported + " out of " + this._magBrowserService.SelectedPaperIds.length +
                            " new items";
                    }
                    else {
                        notificationMsg += "All of these records were already in your review.";
                    }
                    this.showMAGRunMessage(notificationMsg);
                    this.RefreshLists(this._magBrowserService.SelectedPaperIds);
                }
            });
       
    }

    public RefreshLists(SelectedPaperIds: number[]) {

        if (this._magBrowserService.currentRefreshListType == 'MagRelatedPapersRunList') {

            console.log('MagRelatedPapersRunList');
            let item: MagRelatedPapersRun = this._magBrowserService.currentMagRelatedRun;
            if (item.magRelatedRunId > 0) {

                this._magBrowserService.currentRefreshListType = 'MagRelatedPapersRunList';
                  this._magBrowserService.MagCitationsByPaperList.papers = [];
                this._magBrowserService.MAGOriginalList.papers = [];
                this._magBrowserService.currentListType = "MagRelatedPapersRunList";
                this._magBrowserService.ShowingParentAndChildTopics = false;
                this._magBrowserService.ShowingChildTopicsOnly = true;
                let magBrowseItem: MagBrowseHistoryItem = new MagBrowseHistoryItem("Papers identified from auto-identification run", "MagRelatedPapersRunList", 0,
                    "", "", 0, "", "", 0, "", "", item.magRelatedRunId);
                this._mAGBrowserHistoryService.IncrementHistoryCount();
                this._mAGBrowserHistoryService.AddToBrowseHistory(magBrowseItem);
                this._magBrowserService.FetchMAGRelatedPaperRunsListById(item.magRelatedRunId)
                    .then(
                        () => {
                            this.router.navigate(['MAGBrowser']);
                        }
                    );
            }

        } else if (this._magBrowserService.currentRefreshListType == 'MagSearchResultsList') {


            let itemSearch: MagSearch = this._magBrowserService.currentMagSearch;
            if (itemSearch.magSearchId > 0) {
                console.log('MagSearchResultsList');
                this._magBrowserService.currentMagSearch = itemSearch;
                this._magBrowserService.MagCitationsByPaperList.papers = [];
                this._magBrowserService.MAGOriginalList.papers = [];
                this._magBrowserService.currentRefreshListType = 'MagSearchResultsList';
                this._magBrowserService.currentListType = "MagSearchResultsList";
                this._magBrowserService.ShowingParentAndChildTopics = false;
                this._magBrowserService.ShowingChildTopicsOnly = true;
                let magBrowseItem: MagBrowseHistoryItem = new MagBrowseHistoryItem("Papers identified from Mag Search run", "MagSearchPapersList", 0,
                    "", "", 0, "", "", 0, "", "", itemSearch.magSearchId);
                this._mAGBrowserHistoryService.IncrementHistoryCount();
                this._mAGBrowserHistoryService.AddToBrowseHistory(magBrowseItem);
                let selectionCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
                selectionCriteria.pageSize = 20;
                selectionCriteria.pageNumber = 0;
                selectionCriteria.listType = "MagSearchResultsList";
                selectionCriteria.magSearchText = itemSearch.magSearchText;
                this._magBrowserService.FetchMagPapersFromSearch(selectionCriteria, "MagSearchResultsList")
                    .then(
                        () => {
                            this.router.navigate(['MAGBrowser']);
                        }
                    );
            }

        } else if (this._magBrowserService.currentRefreshListType == 'GetMagPaperRef') {


            let magPaperRefId: number = this._magBrowserService.currentPaper.paperId;
            if (magPaperRefId > 0) {
                console.log('GetMagPaperRef');
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

        } else if (this._magBrowserService.currentRefreshListType == 'PaperFieldsOfStudyList') {

            let item: MagFieldOfStudy = this._magBrowserService.currentTopicSearch;
            if (item.fieldOfStudyId > 0) {
                console.log('PaperFieldsOfStudyList');
                this._magBrowserService.currentRefreshListType = 'PaperFieldsOfStudyList';
                this._eventEmitterService.firstVisitMAGBrowserPage = false;
                this._magBrowserService.OrigListCriteria.listType = "PaperFieldsOfStudyList";
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

                this._magAdvancedService.currentMagPaper = new MagPaper();
                this._magBrowserService.MagCitationsByPaperList = new MagList();
                this._magBrowserService.GetParentAndChildFieldsOfStudy("FieldOfStudyParentsList", FieldOfStudyId).then(
                    () => {
                        this._magBrowserService.GetParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", FieldOfStudyId).then(
                            () => {
                                this._eventEmitterService.firstVisitMAGBrowserPage = false;
                                this._magBrowserService.GetPaperListForTopic(FieldOfStudyId);
                            });
                    });

            }
        }        
        this._magBrowserService.SelectedPaperIds = [];
        this._magBrowserService.selectedPapers = [];
    }

    public MagSearch() {

        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("MagSearch", "Search", 0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this.router.navigate(['MagSearch']);

    }
    public MagAdmin() {
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("MAGAdmin", "Admin", 0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this.router.navigate(['MAGAdmin']);
    }
    public MatchingMAGItems() {
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("Matching page", "matching", 0, "", "", 0, "", "", 0, "", "", 0)
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this.router.navigate(['MatchingMAGItems']);
    }
    public AutoUpdateHome() {
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("Manage review updates / find related papers", "RelatedPapers", 0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this.router.navigate(['BasicMAGFeatures']);
    }
    public ShowHistory() {
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("View browse history", "History", 0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this.router.navigate(['MAGBrowserHistory']);
    }
    public Admin() {
        this.router.navigate(['MAGAdmin']);
    }
  
}




