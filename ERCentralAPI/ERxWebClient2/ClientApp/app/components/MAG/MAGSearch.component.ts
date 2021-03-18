import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MagSearch, TopicLink, MVCMagFieldOfStudyListSelectionCriteria, MagFieldOfStudy, MagBrowseHistoryItem } from '../services/MAGClasses.service';
import { magSearchService } from '../services/MAGSearch.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { MAGTopicsService } from '../services/MAGTopics.service';
import { Helpers } from '../helpers/HelperMethods';
import { MAGAdminService } from '../services/MAGAdmin.service';
import { ModalService } from '../services/modal.service';

@Component({
    selector: 'MAGSearch',
    templateUrl: './MAGSearch.component.html',
    providers: []
})

export class MAGSearchComponent implements OnInit {

    constructor(private _confirmationDialogService: ConfirmationDialogService,
        private modalService: ModalService,
        private _magBrowserService: MAGBrowserService,
        private _magSearchService: magSearchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private router: Router,
        private _mAGBrowserHistoryService: MAGBrowserHistoryService,
        private _magTopicsService: MAGTopicsService,
        private MAGAdminService: MAGAdminService

    ) {

    }

    @Output() PleaseGoTo = new EventEmitter<string>();
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
    public DateLimitSelectionCombine: number = 0;
    public PublicationTypeSelection: number = 0;
    public magSearchInput: string = '';
    public valueKendoDatepicker1 : Date = new Date();
    public valueKendoDatepicker2: Date = new Date();
    public valueKendoDatepicker3: Date = new Date();
    public valueKendoDatepickerCombine1: Date = new Date();
    public valueKendoDatepickerCombine2: Date = new Date();
    private _maxyear: number = (new Date()).getFullYear() + 1;
    public get maxyear(): number {
        return this._maxyear;
    }
    public valueYearPickerCombine1: number = this.maxyear - 11;
    public valueYearPickerCombine2: number = this.valueYearPickerCombine1;
    public ShowTextImportFilters: boolean = false;
    public FilterOutJournal: string = "";
    public FilterOutURL: string = "";
    public FilterOutDOI: string = "";
    public magSearchDate1: Date = new Date();
    public magSearchDate2: Date = new Date();
    public SearchTextTopics: TopicLink[] = [];
    public SearchTextTopicsResults: TopicLink[] = [];
    public SearchTextTopic: string = '';
    public OpenTopics: boolean = false;
    public SearchTextTopicDisplayName: string = '';
    public basicFilterPanel: boolean = false;

    public get MagFolder(): string {
        return this.MAGAdminService.MagCurrentInfo.magFolder;
    }

    ngOnInit() {

        //this.FetchMagSearches();
         
    }
    public get MagSearchList(): MagSearch[] {
        return this._magSearchService.MagSearchList;
    }
    public FetchMagSearches() {
        this._magSearchService.FetchMAGSearchList();
    }

    FormatDate(DateSt: string): string {
        return Helpers.FormatDate(DateSt);
    }
    FormatDate2(DateSt: string): string {
        return Helpers.FormatDate2(DateSt);
    }
    public UpdateTopicResults() {


        if (this.SearchTextTopicDisplayName.length > 2) {

            let criteriaFOSL: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
            criteriaFOSL.fieldOfStudyId = 0;
            criteriaFOSL.listType = 'FieldOfStudySearchList';
            criteriaFOSL.paperIdList = '';
            criteriaFOSL.SearchTextTopics = this.SearchTextTopicDisplayName;
            this._magTopicsService.FetchMagFieldOfStudyList(criteriaFOSL, '').then(

                (results: MagFieldOfStudy[] | boolean) => {

                    //this.WPFindTopics = [];
                    if (results != false) {
                        let FosList: MagFieldOfStudy[] = results as MagFieldOfStudy[];
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
        if (item.magFolder != this.MagFolder) return false;
        else if (item != null && item.magSearchId > 0 && item.hitsNo > 0 && this.HasWriteRights) {
            return true;
        } else {
            return false;
        }
    }
    public ShowFilterPanel() {
        this.basicFilterPanel = !this.basicFilterPanel;
    }
    public ImportMagSearchPapers(item: MagSearch) {

        let msg: string = '';
        if (item.magSearchId == 0 || item.hitsNo == 0) {
            this.modalService.GenericErrorMessage('Sorry, there are no papers to import');

        } else if (item.hitsNo > 20000) {
            msg = "Sorry, imports are restricted to 20,000 records.<br />You could try breaking up your search (eg. by date).";
            this.modalService.GenericErrorMessage(msg);
        } else {
            if (item.hitsNo == 1) {
                msg = "Are you sure you want to import this search result? <br />This will import up to " + item.hitsNo.toString() + " items in your review.";
            }
            else {
                msg = "Are you sure you want to import these search results? <br />This will import up to " + item.hitsNo.toString() + " items in your review.";
            }
            this.ImportMagRelatedPapersRun(item, msg);
        }
    }
    public ImportMagRelatedPapersRun(magSearch: MagSearch, msg: string) {

        this._confirmationDialogService.confirm("Importing papers for the selected search",
            msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magSearchService.ImportMagSearches(magSearch.magSearchText
                        , magSearch.searchText
                        , this.basicFilterPanel ? this.FilterOutJournal : ""
                        , this.basicFilterPanel ? this.FilterOutURL : ""
                        , this.basicFilterPanel ? this.FilterOutDOI : ""
                    ).then(

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
        if (item.magFolder != this.MagFolder) {
            this.modalService.GenericErrorMessage("This search refers to an outdated version of MAG, results might be outdated as well. <br /> Please re-run the search.")
            return;
        }

        if (item.magSearchId > 0) {

            this._magBrowserService.GetMagItemsForSearch(item)            
                .then(
                    () => {
                        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Papers List from search #: " + item.searchNo.toString(), "MagSearchPapersList", item.magSearchId,
                            item.magSearchText, "", 0, "", "", 0, "", "", 0));
                        this.PleaseGoTo.emit("MagSearchPapersList");
                        //this.router.navigate(['MAGBrowser']);
                    }
                );
        }
    }

    public get AllItemsAreSelected(): boolean {
        const ind = this._magSearchService.MagSearchList.findIndex(f => f.add == false);
        //console.log("AllItemsAreSelected", ind, this._magSearchService.MagSearchList.length);
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
            + " selected searches",
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
                let msg: string = 'You have rerun the search';
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
                    //let msg: string = 'You have created a new search';
                    //this._confirmationDialogService.showMAGRunMessage(msg);
                }
            );
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
        if ((this.LogicalOperator !== 'AND' && this.LogicalOperator !== 'OR') || this.AllSelectedItems.length < 2) return;
        let search = this.CombineSearchesString();
        this.AddDateAndStringLimits(search);
        if (search.magSearchText.length > 2000) {
            let msg = "Sorry, we can't combine these searches, the resulting search string is too long.";
            this._confirmationDialogService.showErrorMessageInStrip(msg);
            this.LogicalOperator = 'Select operator';
            return;
        }
        this.LogicalOperator = 'Select operator';
        this._magSearchService.RunMagSearch(search).then(
            (res) => {
                if (res == true) {
                    let msg: string = 'You have combined searches using : ' + this.LogicalOperator;
                    this._confirmationDialogService.showMAGRunMessage(msg);
                }
                //this.FetchMagSearches();
            }
        );
        //this._magSearchService.CombineSearches(this.AllSelectedItems, this.LogicalOperator).then(

        //    () => {
        //        let msg: string = 'You have Combined MAG searches using : ' + this.LogicalOperator;
        //        this._confirmationDialogService.showMAGRunMessage(msg);
        //        this.LogicalOperator = 'Select operator';
        //        this.FetchMagSearches();
        //    }
        //);
    }
    private CombineSearchesString(): MagSearch {
        let combinedMagSearch: string = "";
        let combinedSearchText: string = this.LogicalOperator + "(";
        for (let ms of this.AllSelectedItems)
        {
            if (combinedMagSearch == "") {
                combinedMagSearch = ms.magSearchText;
                combinedSearchText += "#" + ms.searchNo.toString();
            }
            else {
                combinedMagSearch += "," + ms.magSearchText;
                combinedSearchText += ", #" + ms.searchNo.toString();
            }
        }
        let res: MagSearch = new MagSearch();
        res.searchText = combinedSearchText + ")";
        res.magSearchText = this.LogicalOperator + "(" + combinedMagSearch + ")";
        return res;
    } 
    private AddDateAndStringLimits(newSearch: MagSearch) {
        if (this.DateLimitSelectionCombine > 0) {
            switch (this.DateLimitSelectionCombine) {
                case 1:
                    newSearch.magSearchText = "AND(" + newSearch.magSearchText + "," +
                        this.GetSearchTextPubDateExactly(this.valueKendoDatepickerCombine1) + ")";
                    newSearch.searchText += " AND published on: " + this.valueKendoDatepickerCombine1.toISOString().substring(0, 10);
                    break;
                case 2:
                    newSearch.magSearchText = "AND(" + newSearch.magSearchText + "," +
                        this.GetSearchTextPubDateBefore(this.valueKendoDatepickerCombine1) + ")";
                    newSearch.searchText += " AND published before: " + this.valueKendoDatepickerCombine1.toISOString().substring(0, 10);
                    break;
                case 3:
                    newSearch.magSearchText = "AND(" + newSearch.magSearchText + "," +
                        this.GetSearchTextPubDateFrom(this.valueKendoDatepickerCombine1) + ")";
                    newSearch.searchText += " AND published after: " + this.valueKendoDatepickerCombine1.toISOString().substring(0, 10);
                    break;
                case 4:
                    newSearch.magSearchText = "AND(" + newSearch.magSearchText + "," +
                        this.GetSearchTextPubDateBetween(this.valueKendoDatepickerCombine1,
                            this.valueKendoDatepickerCombine2) + ")";
                    newSearch.searchText += " AND published between: " + this.valueKendoDatepickerCombine1.toISOString().substring(0, 10) + " and " +
                        this.valueKendoDatepickerCombine2.toISOString().substring(0, 10);
                    break;
                case 5:
                    newSearch.magSearchText = "AND(" + newSearch.magSearchText + "," +
                        this.GetSearchTextYearExactly(this.valueYearPickerCombine1.toString()) + ")";
                    newSearch.searchText += " AND year of publication: " + this.valueYearPickerCombine1.toString();
                    break;
                case 6:
                    newSearch.magSearchText = "AND(" + newSearch.magSearchText + "," +
                        this.GetSearchTextYearBefore(this.valueYearPickerCombine1.toString()) + ")";
                    newSearch.searchText += " AND year of publication before: " + this.valueYearPickerCombine1.toString();
                    break;
                case 7:
                    newSearch.magSearchText = "AND(" + newSearch.magSearchText + "," +
                        this.GetSearchTextYearAfter(this.valueYearPickerCombine1.toString()) + ")";
                    newSearch.searchText += " AND year of publication after: " + this.valueYearPickerCombine1.toString();
                    break;
                case 8:
                    newSearch.magSearchText = "AND(" + newSearch.magSearchText + "," +
                        this.GetSearchTextYearBetween(this.valueYearPickerCombine1.toString(),
                            this.valueYearPickerCombine2.toString()) + ")";
                    newSearch.searchText += " AND year of publication between: " + this.valueYearPickerCombine1.toString() + " and " +
                        this.valueYearPickerCombine1.toString();
                    break;
            }
        }
        return newSearch;
    }
    //format for dates: 2021-03-01
    public GetSearchTextPubDateExactly(date: Date):string {
        return "D='" + date.toISOString().substring(0, 10) + "'";
    }
    private GetSearchTextPubDateFrom(date: Date): string {
        return "D>'" + date.toISOString().substring(0,10) + "'";
    }

    private GetSearchTextPubDateBefore(date: Date): string  {
        return "D<'" + date.toISOString().substring(0, 10) + "'";
    }

    private GetSearchTextPubDateBetween(date1: Date, date2: Date): string  {
        return "D=['" + date1.toISOString().substring(0, 10) + "','" + date2.toISOString().substring(0, 10) + "']";
    }
    private GetSearchTextYearExactly(year: string): string  {
        return "Y=" + year;
    }
    private GetSearchTextYearBefore(year: string): string  {
        return "Y<" + year;
    }

    private GetSearchTextYearAfter(year: string): string  {
        return "Y>" + year;
    }

    private GetSearchTextYearBetween(year1: string, year2: string): string  {
        return "Y=[" + year1 + "," + year2 + "]";
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

