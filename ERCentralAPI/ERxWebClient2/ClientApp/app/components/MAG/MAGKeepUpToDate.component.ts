import { Location } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import {  Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagBrowseHistoryItem, MagPaper, MVCMagPaperListSelectionCriteria, MagRelatedPapersRun, MagAutoUpdateRun, MagAutoUpdate, MagAutoUpdateVisualise, MagAutoUpdateVisualiseSelectionCriteria, ClassifierContactModel, MagAddClassifierScoresCommand } from '../services/MAGClasses.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MAGAdminService } from '../services/MAGAdmin.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { SetAttribute, singleNode } from '../services/ReviewSets.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { timeInRange } from '@progress/kendo-angular-dateinputs/dist/es2015/util';
import { timeout } from 'rxjs/operators';
import { Helpers } from '../helpers/HelperMethods';

@Component({
    selector: 'MAGKeepUpToDate',
    templateUrl: './MAGKeepUpToDate.component.html',
    providers: []
})

export class MAGKeepUpToDate implements OnInit {

    constructor(
        private ConfirmationDialogService: ConfirmationDialogService,
        //public _MAGBrowserHistoryService: MAGBrowserHistoryService,
        private MAGAdvancedService: MAGAdvancedService,
        //private _magBasicService: MAGRelatedRunsService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private router: Router,
        //public _eventEmitterService: EventEmitterService,
        private _magBrowserService: MAGBrowserService,
        //public _magAdminService: MAGAdminService
        private MAGRelatedRunsService: MAGRelatedRunsService,
        private NotificationService: NotificationService
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
    public CurrentMagAutoUpdateRun: MagAutoUpdateRun | null = null;

    //refine and import variables, as MVCMagPaperListSelectionCriteria, as it contains all the details we need, aslo for "importing", save for the "filter out" strings
    public ListCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    public ListCriteriaTotPapers: number = 0;
    public FilterOutJournal: string = "";
    public FilterOutURL: string = "";
    public FilterOutDOI: string = "";
    public comboAutoUpdateImportOptions: string = "AutoUpdate";

    private _ShowDist: string = "AutoUpdate";
    public get ShowDist(): string {
        return this._ShowDist;
    }
    public set ShowDist(val: string) {
        if (val != this._ShowDist) {
            this._ShowDist = val;
            this.LoadGraph();
        } else this._ShowDist = val;
    }

    public get MagAutoUpdatesList(): MagAutoUpdate[] {
        return this.MAGRelatedRunsService.MagAutoUpdatesList;
    }
    public get MagAutoUpdateRunList(): MagAutoUpdateRun[] {
        return this.MAGRelatedRunsService.MagAutoUpdateRunList;
    }
    public get MagAutoUpdateVisualise(): MagAutoUpdateVisualise[] {
        return this.MAGRelatedRunsService.MagAutoUpdateVisualise;
    }
    public get StudyClassifiers(): string[] {
        return [
            "[Please select...]"
            , "RCT"
            , "Cochrane RCT"
            , "Economic evaluation"
            , "Systematic review"];
    }
    public SelectedStudyClassifier = "[Please select...]";
    public get ClassifierContactModelList(): ClassifierContactModel[] {
        return this.MAGAdvancedService.ClassifierContactModelList;
    }
    public SelectedClassifierContactModel: ClassifierContactModel | null = null;

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
    RefineAndImport(taskRun: MagAutoUpdateRun) {
        console.log("RefineAndImport");
        this._ShowDist = 'AutoUpdate';
        let crit: MagAutoUpdateVisualiseSelectionCriteria = { field: this.ShowDist, magAutoUpdateRunId: taskRun.magAutoUpdateRunId };
        this.CurrentMagAutoUpdateRun = taskRun;
        this.MAGRelatedRunsService.GetMagAutoVisualiseList(crit);
        this.ListCriteria = new MVCMagPaperListSelectionCriteria();
        this.ListCriteria.magAutoUpdateRunId = taskRun.magAutoUpdateRunId;
        this.ListCriteria.autoUpdateUserTopN = taskRun.nPapers;
        this.ListCriteriaTotPapers = taskRun.nPapers;
        this.comboAutoUpdateImportOptions = 'AutoUpdate';
        if (this.MAGAdvancedService.ClassifierContactModelList.length == 0) {
            //wait 100ms and then get this list, I don't like sending many server requests all concurrent
            setTimeout(() => { this.MAGAdvancedService.FetchClassifierContactModelList(); }, 100);
        }
    }
    public LoadGraph() {
        console.log("LoadGraph", this.ShowDist);
        if (this.CurrentMagAutoUpdateRun != null && this.ShowDist != '') {
            let crit: MagAutoUpdateVisualiseSelectionCriteria = { field: this.ShowDist, magAutoUpdateRunId: this.CurrentMagAutoUpdateRun.magAutoUpdateRunId };
            this.MAGRelatedRunsService.GetMagAutoVisualiseList(crit);
        }
    }
    async RunStudyClassifier() {
        if (this.CurrentMagAutoUpdateRun != null && this.StudyClassifiers.indexOf(this.SelectedStudyClassifier) > 0) {
            let cmd: MagAddClassifierScoresCommand = new MagAddClassifierScoresCommand();
            cmd.magAutoUpdateRunId = this.CurrentMagAutoUpdateRun.magAutoUpdateRunId;
            cmd.studyTypeClassifier = this.SelectedStudyClassifier;
            cmd.topN = this.CurrentMagAutoUpdateRun.nPapers;
            await this.MAGRelatedRunsService.RunMagAddClassifierScoresCommand(cmd).then(async res => {
                if (res) {
                    this.NotificationService.show({
                        content: 'Model is being applied, should be ready in 5m (will check every 30s).',
                        animation: { type: 'slide', duration: 400 },
                        position: { horizontal: 'center', vertical: 'top' },
                        type: { style: "info", icon: true },
                        closable: true,
                        hideAfter: 3000
                    });
                    let i: number = 0;
                    while (i < 10) {
                        await Helpers.Sleep(1000 * 30);
                        this.MAGRelatedRunsService.GetMagAutoUpdateRunList(); 
                        i++;
                    }
                    
                }
            })
        }
    }
    async RunContactModelClassifier() {
        if (this.CurrentMagAutoUpdateRun != null && this.SelectedClassifierContactModel && this.SelectedClassifierContactModel.modelId > 0) {
            let cmd: MagAddClassifierScoresCommand = new MagAddClassifierScoresCommand();
            cmd.magAutoUpdateRunId = this.CurrentMagAutoUpdateRun.magAutoUpdateRunId;
            cmd.userClassifierModelId = this.SelectedClassifierContactModel.modelId;
            cmd.userClassifierReviewId = this.SelectedClassifierContactModel.reviewId;
            cmd.studyTypeClassifier = "None";
            cmd.topN = this.CurrentMagAutoUpdateRun.nPapers;
            await this.MAGRelatedRunsService.RunMagAddClassifierScoresCommand(cmd).then(async res => {
                if (res) {
                    this.NotificationService.show({
                        content: 'Model is being applied, should be ready in 5m (will check every 30s).',
                        animation: { type: 'slide', duration: 400 },
                        position: { horizontal: 'center', vertical: 'top' },
                        type: { style: "info", icon: true },
                        closable: true,
                        hideAfter: 3000
                    });
                    let i: number = 0;
                    while (i < 10) {
                        await Helpers.Sleep(1000 * 30);
                        this.MAGRelatedRunsService.GetMagAutoUpdateRunList();
                        i++;
                    }

                }
            })
        }
    }
    CloseCodeDropDown() {
        //console.log(this.WithOrWithoutCodeSelector);
        let node: SetAttribute = this.WithOrWithoutCodeSelector.SelectedNodeData as SetAttribute;
        this.CurrentDropdownSelectedCode = node;
        this.isCollapsed = false;
    }
}
