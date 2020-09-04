import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { BasicMAGService } from '../services/BasicMAG.service';
import { MagSearch, TopicLink, MVCMagFieldOfStudyListSelectionCriteria, MagFieldOfStudy } from '../services/MAGClasses.service';
import { magSearchService } from '../services/MAGSearch.service';

@Component({
    selector: 'MAGSearch',
    templateUrl: './MAGSearch.component.html',
    providers: []
})

export class MAGSearchComponent implements OnInit {

    constructor(private ConfirmationDialogService: ConfirmationDialogService,
        public _magBasicService: BasicMAGService,
        public _magAdvancedService: MAGAdvancedService,
        private _magBrowserService: MAGBrowserService,
        public _magSearchService: magSearchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _location: Location,
        private router: Router,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService

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
    public LogicalOperator: string = '';
    public DateLimitSelection: number = 0;
    public PublicationTypeSelection: number = 0;
    public MagSearchList: MagSearch[] = [];
    public MagSearchListCombine: MagSearch[] = [];
    public magSearchInput: string = '';
    public valueKendoDatepicker1 : Date = new Date();
    public valueKendoDatepicker2: Date = new Date();
    public valueKendoDatepicker3: Date = new Date();
    public magSearchDate1: Date = new Date();
    public magSearchDate2: Date = new Date();
    public SearchTextTopics: TopicLink[] = [];
    public SearchTextTopicsResults: TopicLink[] = [];
    public SearchTextTopic: string = '';

    ngOnInit() {

        this.FetchMagSearches();
         
    }
    FetchMagSearches() {

        this._magSearchService.FetchMAGSearchList();

    }
    public UpdateTopicResults() {


        if (this.SearchTextTopic.length > 2) {

            let criteriaFOSL: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
            criteriaFOSL.fieldOfStudyId = 0;
            criteriaFOSL.listType = 'FieldOfStudySearchList';
            criteriaFOSL.paperIdList = '';
            criteriaFOSL.SearchTextTopics = this.SearchTextTopic;
            this._magBrowserService.FetchMagFieldOfStudyList(criteriaFOSL, '').then(

                (results: MagFieldOfStudy[]) => {

                    //this.WPFindTopics = [];
                    let FosList: MagFieldOfStudy[] = results;
                    let i: number = 1;
                    let cnt: number = 0;
                    for (var fos of FosList) {
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
    public DeleteMagSearch(magSearchId: number) {

        this._magSearchService.Delete(magSearchId);

    }
    public RunMAGSearch() {

        if (this.DateLimitSelection == 4 || this.DateLimitSelection == 8 ) {
            this.magSearchDate1 = this.valueKendoDatepicker1;
            this.magSearchDate2 = this.valueKendoDatepicker2;
        } else {
            this.magSearchDate1 = this.valueKendoDatepicker3;
        }
        this._magSearchService.CreateMagSearch(this.WordsInSelection, this.DateLimitSelection, this.PublicationTypeSelection,
            this.magSearchInput, this.magSearchDate1, this.magSearchDate2, this.SearchTextTopic).then(

                () => { this.FetchMagSearches(); }
            );
    }
    public AdvancedFeatures() {

        this.router.navigate(['AdvancedMAGFeatures']);

    }
    public Back() {
        this._location.back();
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get IsServiceBusy(): boolean {

        return this._magBrowserService.IsBusy || this._magAdvancedService.IsBusy;
    }
    public CombineSearches(){

        this._magSearchService.CombineSearches(this.MagSearchListCombine, this.LogicalOperator);
    }
    public AddSearchIdToList(magSearch: MagSearch) {
        if (magSearch.magSearchId > -1) {
            let index: number = this.MagSearchListCombine.findIndex(x => x.magSearchId == magSearch.magSearchId);
            if ( index != -1) {
                this.MagSearchListCombine.splice(index, 1);
            } else {
                this.MagSearchListCombine.push(magSearch)
            }
        }
    }
}

