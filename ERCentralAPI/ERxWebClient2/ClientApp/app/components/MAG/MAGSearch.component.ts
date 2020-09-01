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
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _location: Location,
        private router: Router,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService

    ) {

    }
    ngOnInit() {
         
    }

    public CurrentDropdownSelectedCode2: singleNode | null = null;
    public dropdownBasic2: boolean = false;
    public isCollapsed2: boolean = false;

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
}

