import { Location } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import {  Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagBrowseHistoryItem, MagPaper, MVCMagPaperListSelectionCriteria, MagRelatedPapersRun, MagAutoUpdateRun, MagAutoUpdate } from '../services/MAGClasses.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MAGAdminService } from '../services/MAGAdmin.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { SetAttribute, singleNode } from '../services/ReviewSets.service';

@Component({
    selector: 'MAGKeepUpToDate',
    templateUrl: './MAGKeepUpToDate.component.html',
    providers: []
})

export class MAGKeepUpToDate implements OnInit {

    constructor(
        private ConfirmationDialogService: ConfirmationDialogService,
        //public _MAGBrowserHistoryService: MAGBrowserHistoryService,
        //public _magAdvancedService: MAGAdvancedService,
        //private _magBasicService: MAGRelatedRunsService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private router: Router,
        //public _eventEmitterService: EventEmitterService,
        private _magBrowserService: MAGBrowserService,
        //public _magAdminService: MAGAdminService
        private MAGRelatedRunsService: MAGRelatedRunsService
    ) {

    }

    public magBrowseHistoryList: MagBrowseHistoryItem[] = [];
    ngOnInit() {
        this.MAGRelatedRunsService.GetMagAutoUpdateList(true);
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    
    public get IsServiceBusy(): boolean {
        if (this.MAGRelatedRunsService.IsBusy
            || this._magBrowserService.IsBusy) return true;
        return false;
    }
    Back() {
        this.router.navigate(['Main']);
    }
    public basicSearchPanel: boolean = false;
    public description: string = '';
    public magMode: string = 'New items in MAG';
    @ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
    public CurrentDropdownSelectedCode: singleNode | null = null;
    public isCollapsed: boolean = false;//the "pick a code" dropdown...
    public searchAll: string = 'all';

    public get MagAutoUpdatesList(): MagAutoUpdate[] {
        return this.MAGRelatedRunsService.MagAutoUpdatesList;
    }
    public get MagAutoUpdateRunList(): MagAutoUpdateRun[] {
        return this.MAGRelatedRunsService.MagAutoUpdateRunList;
    }

    public ToggleSearchPanel() {
        this.basicSearchPanel = !this.basicSearchPanel;
    }
    public CanAddNewMAGSearch(): boolean {
        if (this.description != '' && this.description != null && this.HasWriteRights) {
            return true;
        } else {
            return false;
        }
    }

    public CreateAutoUpdate() {

        let magRun: MagRelatedPapersRun = new MagRelatedPapersRun();
        if (this.searchAll == 'all') {
            magRun.allIncluded = true;
        } else if (this.searchAll == 'specified') {
            magRun.allIncluded = false;
        } else {
            //children of this code...
        }
        let att: SetAttribute = new SetAttribute();
        if (this.CurrentDropdownSelectedCode != null) {
            att = this.CurrentDropdownSelectedCode as SetAttribute;
            magRun.attributeId = att.attribute_id;
            magRun.attributeName = att.name;
        }
        else if (this.searchAll !== 'specified') return;
        magRun.mode = this.magMode;
        magRun.userDescription = this.description;
        this.MAGRelatedRunsService.CreateAutoUpdate(magRun);

    }
    DeleteAutoUpdate(magAutoUpdate: MagAutoUpdate) {
        this.ConfirmationDialogService.confirm("Deleting the selected MAG Auto Update task",
            "Are you sure you want to delete MAG Auto Update task: \"<em>" + magAutoUpdate.userDescription + "\"</em>?", false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this.MAGRelatedRunsService.DeleteMAGAutoUpdate(magAutoUpdate.magAutoUpdateId);;
                }
            });
    }
    CloseCodeDropDown() {
        //console.log(this.WithOrWithoutCodeSelector);
        let node: SetAttribute = this.WithOrWithoutCodeSelector.SelectedNodeData as SetAttribute;
        this.CurrentDropdownSelectedCode = node;
        this.isCollapsed = false;
    }
}
