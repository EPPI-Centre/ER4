import { Component, OnInit } from '@angular/core';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MagSearch, TopicLink, MVCMagFieldOfStudyListSelectionCriteria, MagFieldOfStudy } from '../services/MAGClasses.service';
import { magSearchService } from '../services/MAGSearch.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { MAGTopicsService } from '../services/MAGTopics.service';

@Component({
    selector: 'MAGSearch',
    templateUrl: './MAGSearch.component.html',
    providers: []
})

export class MAGSearchComponent implements OnInit {

    constructor(private _confirmationDialogService: ConfirmationDialogService,
        public _magBasicService: MAGRelatedRunsService,
        public _magAdvancedService: MAGAdvancedService,
        private _magBrowserService: MAGBrowserService,
        public _magSearchService: magSearchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private router: Router,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService,
        public _notificationService: NotificationService,
        private _magTopicsService: MAGTopicsService

    ) {

    }

    public dropdownBasic2: boolean = false;
    public isCollapsed2: boolean = false;
    public dropdownBasic1: boolean = false;
    public isCollapsed1: boolean = false;
    public dropdownBasic3: boolean = false;
    public isCollapsed3: boolean = false;
    public dropdownBasic4: boolean = false;
    public isCollapsed4: boolean = false;
    public WordsInSelection: number = 0;
    public LogicalOperator: string = 'Select operator';
    public DateLimitSelection: number = 0;
    public PublicationTypeSelection: number = 0;
    public MagSearchList: MagSearch[] = [];
    public magSearchInput: string = '';
    public valueKendoDatepicker1 : Date = new Date();
    public valueKendoDatepicker2: Date = new Date();
    public valueKendoDatepicker3: Date = new Date();
    public magSearchDate1: Date = new Date();
    public magSearchDate2: Date = new Date();
    public SearchTextTopics: TopicLink[] = [];
    public SearchTextTopicsResults: TopicLink[] = [];
    public SearchTextTopic: string = '';
    public OpenTopics: boolean = false;
    public SearchTextTopicDisplayName: string = '';

    ngOnInit() {

        this.FetchMagSearches();
         
    }
    FetchMagSearches() {

        this._magSearchService.FetchMAGSearchList();

    }
    public UpdateTopicResults() {


        if (this.SearchTextTopicDisplayName.length > 2) {

            let criteriaFOSL: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
            criteriaFOSL.fieldOfStudyId = 0;
            criteriaFOSL.listType = 'FieldOfStudySearchList';
            criteriaFOSL.paperIdList = '';
            criteriaFOSL.SearchTextTopics = this.SearchTextTopicDisplayName;
            this._magTopicsService.FetchMagFieldOfStudyList(criteriaFOSL, '').then(

                (results: MagFieldOfStudy[]) => {

                    //this.WPFindTopics = [];
                    let FosList: MagFieldOfStudy[] = results;
                    let i: number = 1;
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
            );

        } else {

            this.SearchTextTopics = [];
            this.SearchTextTopicsResults = [];
        }
    }
    public OpenTopicsPanel(event: any) {
        var dropDownValue = event.target.value;
        console.log(dropDownValue);
        if (dropDownValue == 3) {
            this.OpenTopics = true;
        } else {
            this.OpenTopics = false;
        }
    }
    public SelectTopic(topic: TopicLink)
    {
        this.OpenTopics = false;
        this.SearchTextTopicDisplayName = topic.displayName;
        this.SearchTextTopic = topic.fieldOfStudyId.toString();
    }
    public CanImportMagPapers(item: MagSearch): boolean {

        if (item != null && item.magSearchId > 0 && item.hitsNo > 0 && this.HasWriteRights) {
            return true;
        } else {
            return false;
        }
    }
    public ImportMagSearchPapers(item: MagSearch) {

        let msg: string = '';
        if (item.magSearchId == 0) {
            this._confirmationDialogService.showMAGRunMessage('There are no papers to import');

        } else {

            if (item.hitsNo > 20000) {
                 msg ="Sorry. You can't import more than 20k records at a time.\nYou could try breaking up your search e.g. by date?";
            }
            else {
                msg = "Are you sure you want to import this search result?";
            }
            this.ImportMagRelatedPapersRun(item, msg);
        }
    }
    public ImportMagRelatedPapersRun(magSearch: MagSearch, msg: string) {

        this._confirmationDialogService.confirm("Importing papers for the selected MAG search",
            msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magSearchService.ImportMagSearches(magSearch.magSearchText, magSearch.searchText).then(

                        (result: number) => {

                            let num_in_run: number = magSearch.hitsNo;
                            let msg: string = '';
                            if (result != undefined || result != null) {

                            if (result == num_in_run) {
                                msg = "Imported " + result.toString() + " out of " +
                                    num_in_run.toString() + " items";
                            }
                            else if (result != 0) {
                                msg = "Some of these items were already in your review.\n\nImported " +
                                    result.toString() + " out of " + num_in_run.toString() +
                                    " new items";
                            }
                            else {
                                msg = "All of these records were already in your review.";
                                }

                            } else {
                                msg = 'results are undefined';
                            }
                            this._confirmationDialogService.showMAGRunMessage(msg);
                        }
                        
                   );
                }
            });
    }

    public GetItems(item: MagSearch) {

        if (item.magSearchId > 0) {

            this._magBrowserService.GetMagItemsForSearch(item)            
                .then(
                    () => {
                        this.router.navigate(['MAGBrowser']);
                    }
                );
        }
    }

    public get AllItemsAreSelected(): boolean {
        const ind = this._magSearchService.MagSearchList.findIndex(f => f.add == false);
        if (ind == -1 && this._magSearchService.MagSearchList.length > 0) return true;
        return false;
    }
    public get AllSelectedItems(): MagSearch[] {
        return this._magSearchService.MagSearchList.filter(f => f.add == true);
    }
    public ToggleAllItemsSelected() {
        if (this.AllItemsAreSelected) {
            for (let i = 0; i < this._magSearchService.MagSearchList.length; i++) {
                this._magSearchService.MagSearchList[i].add = false;
            }
        }
        else {
            for (let i = 0; i < this._magSearchService.MagSearchList.length; i++) {
                this._magSearchService.MagSearchList[i].add = true;
            }
        }
    }
    
    public DeleteSearches() {
        const count = this.AllSelectedItems.length.toString();
        this._confirmationDialogService.confirm("Are you sure you want to delete " + this.AllSelectedItems.length.toString()
            + " selected MAG searches",
            '', false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magSearchService.Delete(this.AllSelectedItems).then(

                        (res: any) => {
                            let msg: string = 'Deleted: ' + count + ' items';
                            this._confirmationDialogService.showMAGRunMessage(msg);
                        }
                    );
                }
            });
    }
    public ReRunMAGSearch(magSearch: MagSearch) {

        this._magSearchService.ReRunMagSearch(magSearch.searchText, magSearch.magSearchText).then(

            () => {
                this.FetchMagSearches();
                let msg: string = 'You have ReRun a MAG search';
                this._confirmationDialogService.showMAGRunMessage(msg);
            }
       );

    }
    public RunMAGSearch() {

        if (this.DateLimitSelection == 4 || this.DateLimitSelection == 8 ) {
            this.magSearchDate1 = this.valueKendoDatepicker1;
            this.magSearchDate2 = this.valueKendoDatepicker2;
        } else {
            this.magSearchDate1 = this.valueKendoDatepicker3;
        }
        if (this.SearchTextTopicDisplayName != '') {
            this.magSearchInput = this.SearchTextTopicDisplayName;
        }
        this._magSearchService.CreateMagSearch(this.WordsInSelection, this.DateLimitSelection, this.PublicationTypeSelection,
            this.magSearchInput, this.magSearchDate1, this.magSearchDate2, this.SearchTextTopic).then(

                () => {
                    this.FetchMagSearches();
                    let msg: string = 'You have created a new MAG search';
                    this._confirmationDialogService.showMAGRunMessage(msg);
                }
            );
    }
    public AdvancedFeatures() {
        this.router.navigate(['AdvancedMAGFeatures']);
    }

    public Back() {
        this.router.navigate(['Main']);
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get IsServiceBusy(): boolean {

        return this._magSearchService.IsBusy || this._magBrowserService.IsBusy;
    }
    public CombineSearches(){
        if (this.LogicalOperator == 'Select operator') return;
        this._magSearchService.CombineSearches(this.AllSelectedItems, this.LogicalOperator).then(

            () => {
                let msg: string = 'You have Combined MAG searches using : ' + this.LogicalOperator;
                this._confirmationDialogService.showMAGRunMessage(msg);
                this.LogicalOperator = 'Select operator';
                this.FetchMagSearches();
            }
        );
    }
    public AddSearchIdToList(magSearch: MagSearch) {

    }
    public CanRunSearch(): boolean {
        if (this.magSearchInput == "") {
            return false;
        } else {
            return true;
        }
    }

    public CanDeleteSearch(): boolean {
        if (this.AllSelectedItems.length == 0) {
            return false;
        } else {
            return true;
        }
    }

    public CanCombineSearches(): boolean {
        if (this.AllSelectedItems.length <= 1
            || this.AllSelectedItems.length > 50 //Seriously? Combine more than 50 searches in one go? NOPE, not doing it.
            //should not be doing this client side, if so must be done on both but server side more important...
            //but yes did not think of this I bet there are more as we do not have a think about stage along with
            //understanding the spec from set out cards ?
        ) {
            return false;
        } else {
            return true;
        }
    }
}

