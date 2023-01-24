import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MagSearch, TopicLink, MVCMagFieldOfStudyListSelectionCriteria, MagFieldOfStudy, MagBrowseHistoryItem, MagSearchBuilder } from '../services/MAGClasses.service';
import { magSearchService } from '../services/MAGSearch.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { MAGTopicsService } from '../services/MAGTopics.service';
import { Helpers } from '../helpers/HelperMethods';
import { MAGAdminService } from '../services/MAGAdmin.service';
import { ModalService } from '../services/modal.service';
import { formatDate } from '@angular/common';

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
          private MAGAdminService: MAGAdminService,
          private NotificationService: NotificationService
    ) {

    }

    @Output() PleaseGoTo = new EventEmitter<string>();
    @Output() IHaveImportedSomething = new EventEmitter<void>();
    public dropdownBasic2: boolean = false;
    public isCollapsed2: boolean = false;
    public dropdownBasic1: boolean = false;
    public isCollapsed1: boolean = false;
    public dropdownBasic3: boolean = false;
    public isCollapsed3: boolean = false;
    public dropdownBasic4: boolean = false;
    public isCollapsed4: boolean = false;
    public WordsInSelection: string = "0";
    public LogicalOperator: string = 'Select operator';
    public DateLimitSelection: number = 0;
    public PublicationTypeSelection: number = 0;
    public magSearchInput: string = '';
    public SearchedTopic: string = "";
    public valueKendoDatepicker1 : Date = new Date();
    public valueKendoDatepicker2: Date = new Date();
    public valueKendoDatepicker3: Date = new Date();
    private _maxyear: number = (new Date()).getFullYear() + 1;
    public get maxyear(): number {
        return this._maxyear;
    }

    public valueYearPicker3: number = this.maxyear - 11;
    public valueYearPicker4: number = this.maxyear - 11;
    public ShowTextImportFilters: boolean = false;
    public FilterOutJournal: string = "";
    public FilterOutURL: string = "";
    public FilterOutDOI: string = "";
    public FilterOutTitle: string = "";
    public magSearchDate1: Date = new Date();
    public magSearchDate2: Date = new Date();
    public SearchTextTopics: TopicLink[] = [];
    public SearchTextTopicsResults: TopicLink[] = [];
    public SearchTextTopic: string = '';
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
        this.SearchedTopic = this.magSearchInput;
        this.SearchTextTopicDisplayName = "";
        this.SearchTextTopic = "";
        this.SearchTextTopicsResults = [];
        if (this.magSearchInput.length > 2) {

            let criteriaFOSL: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
            criteriaFOSL.fieldOfStudyId = 0;
            criteriaFOSL.listType = 'FieldOfStudySearchList';
            criteriaFOSL.paperIdList = '';
            criteriaFOSL.SearchTextTopics = this.magSearchInput;
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
   
    public SelectTopic(topic: TopicLink)
    {
        this.SearchTextTopicDisplayName = topic.displayName;
        this.SearchTextTopic = topic.fieldOfStudyId.toString();
    }
    public CanImportMagPapers(item: MagSearch): boolean {
        if (!item.isOASearch) return false;
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
                    this._magSearchService.ImportMagSearches(magSearch.magSearchId.toString()
                        , magSearch.searchText
                        , this.basicFilterPanel ? this.FilterOutJournal : ""
                        , this.basicFilterPanel ? this.FilterOutURL : ""
                        , this.basicFilterPanel ? this.FilterOutDOI : ""
                        , this.basicFilterPanel ? this.FilterOutTitle : ""
                    ).then(

                        (result: number) => {

                            let num_in_run: number = magSearch.hitsNo;
                            let msg: string = '';
                            if (result != undefined || result != null) {

                            if (result == num_in_run) {
                                msg = "Imported " + result.toString() + " out of " +
                                    num_in_run.toString() + " items";
                                this.IHaveImportedSomething.emit();
                            }
                            else if (result != 0) {
                                msg = "Some of these items were already in your review.\n\nImported " +
                                    result.toString() + " out of " + num_in_run.toString() +
                                    " search hits";
                                this.IHaveImportedSomething.emit();
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
          if (item.isOASearch == false) {
            this.modalService.GenericErrorMessage("This search refers to an outdated version of OpenAlex, so results can't be retrieved or imported. <br /> Please recreate the search.")
            return;
          }
          if (item.hitsNo < 1) {
                this.modalService.GenericErrorMessage("No results to import.")
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
                            this._confirmationDialogService.showMAGDelayMessage(msg);
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
                this._confirmationDialogService.showMAGDelayMessage(msg);
            }
       );

    }
    public RunMAGSearch() {

        //if (this.DateLimitSelection == 4 ) {

        //    this.magSearchDate1 = this.valueKendoDatepicker1;
        //    this.magSearchDate2 = this.valueKendoDatepicker2;

        //} else if (this.DateLimitSelection == 5 || this.DateLimitSelection == 6 ||
        //    this.DateLimitSelection == 7) {

        //    this.magSearchDate1 = new Date(this.valueYearPicker3+1,0 ,0,0,0,0,0);
  
        //} else if (this.DateLimitSelection == 8) {

        //    this.magSearchDate1 = new Date(this.valueYearPicker3+1, 0, 0, 0, 0, 0, 0);
        //    this.magSearchDate2 = new Date(this.valueYearPicker4+1, 0, 0, 0, 0, 0, 0);        

        //}else{

        //    this.magSearchDate1 = this.valueKendoDatepicker3;
        //}

        //let title: string = "";
        //if (this.WordsInSelection != 3) title = this.magSearchInput;
        //else title = this.SearchTextTopicDisplayName;
        //this._magSearchService.CreateMagSearch(this.WordsInSelection, this.DateLimitSelection, this.PublicationTypeSelection,
        //    title, this.magSearchDate1, this.magSearchDate2, this.SearchTextTopic).then(

        //        () => {
        //            this.FetchMagSearches();
        //            this.DateLimitSelection = 0;

        //            if (this.WordsInSelection == 3) {
        //                //cleanup the topics...
        //                this.SearchTextTopicsResults = [];
        //                this.SearchTextTopic = "";
        //                this.SearchedTopic = "";
        //                this.SearchTextTopicDisplayName = "";
        //            }
        //            //let msg: string = 'You have created a new search';
        //            //this._confirmationDialogService.showMAGRunMessage(msg);
        //        }
        //    );
    }
  public RunOpenAlexSearch() {
    let newSearch: MagSearchBuilder = new MagSearchBuilder();
    switch (this.WordsInSelection) {
      case "0":
        newSearch.magSearchText = this.magSearchInput;
        newSearch.searchText = '\u00AC' + "Title: " + this.magSearchInput;
        break;
      case "1":
        newSearch.magSearchText = this.magSearchInput;
        newSearch.searchText = '\u00AC' + "Title and abstract: " + this.magSearchInput;
        break;
      case "2":
        newSearch.magSearchText = this.SearchTextTopic;
        newSearch.searchText = '\u00AC' + "Topic: " + this.SearchTextTopicDisplayName;
        break;
      case "3":
        newSearch.magSearchText = this.GetSearchTextMagIds(this.magSearchInput);
        if (newSearch.magSearchText.length < 1) {
          //maybe show an error notification??
          return;
        }
        newSearch.searchText = '\u00AC' + "OpenAlex ID(s): " + newSearch.magSearchText;
        break;
      case "4":
        newSearch.magSearchText = this.magSearchInput;
        newSearch.searchText = '\u00AC' + "Custom filter: " + this.magSearchInput;
        break;
      case "5":
        newSearch.magSearchText = this.magSearchInput;
        newSearch.searchText = '\u00AC' + "Custom search: " + this.magSearchInput;
        break;
      default:
        return;
    }
    if (this.DateLimitSelection > 0 && this.WordsInSelection != '1') {
      switch (this.DateLimitSelection) {
        case 1:
          newSearch.date1 = formatDate(this.valueKendoDatepicker3, "yyyy-MM-dd", 'en-GB');
          newSearch.dateFilter = "Created after";
          break;
        case 2:
          newSearch.date1 = formatDate(this.valueKendoDatepicker3, "yyyy-MM-dd", 'en-GB');
          newSearch.dateFilter = "Published on";
          break;
        case 3:
          newSearch.date1 = formatDate(this.valueKendoDatepicker3, "yyyy-MM-dd", 'en-GB');
          newSearch.dateFilter = "Published before";
          break;
        case 4:
          newSearch.date1 = formatDate(this.valueKendoDatepicker3, "yyyy-MM-dd", 'en-GB');
          newSearch.dateFilter = "Published after";
          break;
        case 5:
          newSearch.date1 = formatDate(this.valueKendoDatepicker1, "yyyy-MM-dd", 'en-GB');
          newSearch.date2 = formatDate(this.valueKendoDatepicker2, "yyyy-MM-dd", 'en-GB');
          newSearch.dateFilter = "Published between";
          break;
        case 6:
          newSearch.date1 = this.valueYearPicker3.toString();
          newSearch.dateFilter = "Publication year";
          break;
      }
    }
    else {
      newSearch.dateFilter = "";
    }

    if (newSearch.magSearchText.length > 2000) {
      this.modalService.GenericErrorMessage("Sorry, the search string is too long.<br />Please consider dividing it in two and then combining the results.");
      return;
    }
    this._magSearchService.CreateMagSearch(newSearch).then(

      () => {
        //this.FetchMagSearches();
        this.DateLimitSelection = 0;

        if (this.WordsInSelection == "2") {
          //cleanup the topics...
          this.SearchTextTopicsResults = [];
          this.SearchTextTopic = "";
          this.SearchedTopic = "";
          this.SearchTextTopicDisplayName = "";
        }
        this.NotificationService.show({
          content: "New search was created",
          animation: { type: 'slide', duration: 400 },
          hideAfter: 2500,
          position: { horizontal: 'center', vertical: 'top' },
          type: { style: "info", icon: true }
        });
      }
    );
  }
      public GetSearchTextMagIds(searchText: string): string {
            let res:string = "";
            const numbers = this.magSearchInput.split(',');
            if (!numbers || numbers.length == 0) return '';
            else {
                  for (const numTx of numbers) {
                        let test = Number.parseInt(numTx);
                        if (!isNaN(test) && test > 0) {
                              if (res == "") res = "W" + numTx;
                              else res += "|W" + numTx;
                        } 
                  }
            }
            return res;
      }

    public Back() {
        this.router.navigate(['Main']);
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get IsServiceBusy(): boolean {

        return this._magSearchService.IsBusy || this._magBrowserService.IsBusy || this._magTopicsService.IsBusy;
    }
      public CombineSearches() {
            const MaxHitCount = 40000;//might move to a config file!!
        const Ns = this.AllSelectedItems.length;
          if (Ns > 20) {
                this.modalService.GenericErrorMessage("Sorry, can't combine that many searches...")
                return;
          }
          else if (Ns <= 1) {
                this.modalService.GenericErrorMessage("Sorry, there is nothing to 'combine'...")
                return;
          }
            let search: MagSearch = new MagSearch();
            let hitCount = 0;
            let combined = "";
            let searchDesc = "";
          for (let src of this.AllSelectedItems) {
                if (!src.isOASearch) {
                      this.modalService.GenericErrorMessage("Sorry, old searches cannot be combined now we have moved to OpenAlex");
                      return;
                }
                if (!src.searchIdsStored) {
                      hitCount += src.hitsNo;
                }
                if (combined == "") {
                      combined = src.magSearchId.toString();
                      searchDesc = src.searchNo.toString();
                }
                else {
                      combined += this.LogicalOperator + src.magSearchId.toString();
                      searchDesc += this.LogicalOperator + src.searchNo.toString();
                }
            }
            if (hitCount > MaxHitCount) {
                  this.modalService.GenericErrorMessage("Sorry, too many hits. Please combine fewer records. <br /> (Limit is " + MaxHitCount.toString() + ")");
                  return;
            }
            this.LogicalOperator = 'Select operator';
            search.magSearchText = combined;
            search.searchText = '\u00AC' + "COMBINE SEARCHES" + searchDesc;
        this._magSearchService.RunMagSearch(search).then(
            (res) => {
                if (res == true) {
                    let msg: string = 'Searches have been combined.';
                    this._confirmationDialogService.showMAGDelayMessage(msg);
                    //this.DateLimitSelectionCombine = 0;
                }
                //this.FetchMagSearches();
            }
        );
    }
    
    

    

    public AddSearchIdToList(magSearch: MagSearch) {

    }
    public CanRunSearch(): boolean {
        if (!this.HasWriteRights) return false;
        
        else if (this.WordsInSelection == "2" && this.SearchTextTopic == "") {
            return false;
        }
        else if (this.WordsInSelection != "2" && this.magSearchInput == "") {
            return false;
        }
        else if (this.WordsInSelection == "3") {
              const numbers = this.magSearchInput.split(',');
              if (!numbers || numbers.length == 0) return false;
              else {
                    for (const numTx of numbers) {
                          let test = Number.parseInt(numTx);
                          if (isNaN(test)) return false;
                          else if (test < 1) return false;
                    }
              }
          }
          return true;
      }

    public CanDeleteSearch(): boolean {
        if (this.AllSelectedItems.length == 0 || !this.HasWriteRights) {
            return false;
        } else {
            return true;
        }
    }

    public CanCombineSearches(): boolean {
        if (!this.HasWriteRights) return false;
        else if (this.AllSelectedItems.length > 1 && this.AllSelectedItems.length <= 20
            //Seriously? Combine more than 20 searches in one go? NOPE, not doing it.
            && (this.LogicalOperator == "AND" || this.LogicalOperator == "OR")) return true;//combining searches
        else {
            return false;
        }
    }
}

