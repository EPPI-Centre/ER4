import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { searchService } from '../services/search.service';
import { singleNode } from '../services/ReviewSets.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { BasicMAGService } from '../services/BasicMAG.service';
import { MagSearch } from '../services/MAGClasses.service';
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
    ngOnInit() {

        this.FetchMagSearches();
         
    }
    FetchMagSearches() {

        this._magSearchService.FetchMAGSearchList();

    }

    public dropdownBasic2: boolean = false;
    public isCollapsed2: boolean = false;
    public dropdownBasic1: boolean = false;
    public isCollapsed1: boolean = false;
    public dropdownBasic3: boolean = false;
    public isCollapsed3: boolean = false;
    public dropdownBasic4: boolean = false;
    public isCollapsed4: boolean = false;
    public WordsIn: string = '';
    public LogicalOperator: string = '';
    public DateLimit: string = '';
    public PublicationType: string = '';
    public MagSearchList: MagSearch[] = [];

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
    public RunMAGSearch() {


    }
    public CombineSearch(){

    }
}

