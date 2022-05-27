import { Location } from '@angular/common';
import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import {  Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import {
    MagBrowseHistoryItem, MagPaper, MVCMagPaperListSelectionCriteria, MagRelatedPapersRun,
    MagAutoUpdateRun, MagAutoUpdate, MagAutoUpdateVisualise, MagAutoUpdateVisualiseSelectionCriteria, MagAddClassifierScoresCommand, MagItemPaperInsertCommand
} from '../services/MAGClasses.service';
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
import { NgForm } from '@angular/forms';
import { ModalService } from '../services/modal.service';
import { ClassifierModel, ClassifierService } from '../services/classifier.service';

@Component({
    selector: 'MAGKeepUpToDate',
    templateUrl: './MAGKeepUpToDate.component.html',
    providers: []
})

export class MAGKeepUpToDate implements OnInit {

    constructor(
        private ConfirmationDialogService: ConfirmationDialogService,
        private MAGBrowserHistoryService: MAGBrowserHistoryService,
        private MAGAdvancedService: MAGAdvancedService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _magBrowserService: MAGBrowserService,
        private MAGRelatedRunsService: MAGRelatedRunsService,
        private NotificationService: NotificationService,
        private ModalService: ModalService,
        private _classifierService: ClassifierService
    ) {

    }


    
    ngOnInit() {
        //this.MAGRelatedRunsService.GetMagAutoUpdateList(true);
        console.log("MAGKeepUpToDate init");
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
        //this.router.navigate(['Main']);
    }

    public Refresh() {
        this.CurrentMagAutoUpdateRun = null;
        this.MAGRelatedRunsService.GetMagAutoUpdateList(true);
    }
    public basicSearchPanel: boolean = false;
    public description: string = '';
    public magMode: string = 'New items in MAG';
    @ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
    @ViewChild('ThreshodsForm') ThreshodsForm!: NgForm;
    public CurrentDropdownSelectedCode: singleNode | null = null;
    public isCollapsed: boolean = false;//the "pick a code" dropdown...
    public searchAll: string = 'all';
    public CurrentMagAutoUpdateRun: MagAutoUpdateRun | null = null;
    @Output() PleaseGoTo = new EventEmitter<string>();
    @Output() IHaveImportedSomething = new EventEmitter<void>();
    //refine and import variables, as MVCMagPaperListSelectionCriteria, as it contains all the details we need, aslo for "importing", save for the "filter out" strings
    public ListCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    public ListCriteriaTotPapers: number = 0;
    public ListCriteriaFilteredPapers: number = 0;
    public FilterOutJournal: string = "";
    public FilterOutURL: string = "";
    public FilterOutDOI: string = "";
    public FilterOutTitle: string = "";
    public comboAutoUpdateImportOptions: string = "AutoUpdate";
    public basicPanel: boolean = false;

    public get currentlyApplyingModelToThisRunId(): number {
        return this.MAGRelatedRunsService.currentlyApplyingModelToThisRunId;
    }
    public get AutoRefreshIsOn(): boolean {
        return this.MAGRelatedRunsService.AutoRefreshIsOn;
    }
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

    public FormatDate(date: string): string {
        return Helpers.FormatDate(date);
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
    public get ClassifierContactModelList(): ClassifierModel[] {
        return this._classifierService.ClassifierContactAllModelList;
    }
    public SelectedClassifierContactModel: ClassifierModel | null = null;

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
        if (this.CurrentDropdownSelectedCode != null && this.searchAll == 'specified') {
            att = this.CurrentDropdownSelectedCode as SetAttribute;
            magRun.attributeId = att.attribute_id;
            magRun.attributeName = att.name;
        }
        else if (this.searchAll !== 'all') return;
        magRun.mode = this.magMode;
        magRun.userDescription = this.description;
        this.MAGRelatedRunsService.CreateAutoUpdate(magRun);

    }
    DeleteAutoUpdate(magAutoUpdate: MagAutoUpdate) {
        this.ConfirmationDialogService.confirm("Deleting the selected auto update task",
            "Are you sure you want to delete auto update task: \"<em>" + magAutoUpdate.userDescription + "\"</em>?", false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this.MAGRelatedRunsService.DeleteMAGAutoUpdate(magAutoUpdate.magAutoUpdateId);
                }
            });
    }
    BrowseAllItems(taskRun: MagAutoUpdateRun) {
        //console.log("BrowseAllItems", taskRun);
        let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();

        crit.magAutoUpdateRunId = taskRun.magAutoUpdateRunId;
        crit.listType = "MagAutoUpdateRunPapersList";
        crit.pageSize = 20;
        crit.autoUpdateOrderBy = "AutoUpdate";
        crit.autoUpdateAutoUpdateScore = 0.20;//can't be less than 0.20!
        crit.autoUpdateStudyTypeClassifierScore = 0;
        crit.autoUpdateUserClassifierScore = 0;
        crit.autoUpdateUserTopN = taskRun.nPapers;
        this._magBrowserService.GetMagOrigList(crit).then(
            (res) => {
                this.MAGBrowserHistoryService.AddHistory(MagBrowseHistoryItem.MakeFromAutoUpdateListCrit(crit));
                if (res == true) this.PleaseGoTo.emit("MagAutoUpdateRunPapersList");// this.router.navigate(['MAGBrowser']);
            }
        );
    }
    RefineAndImport(taskRun: MagAutoUpdateRun) {
        //console.log("RefineAndImport");
        this._ShowDist = 'AutoUpdate';
        let crit: MagAutoUpdateVisualiseSelectionCriteria = { field: this.ShowDist, magAutoUpdateRunId: taskRun.magAutoUpdateRunId };
        this.CurrentMagAutoUpdateRun = taskRun;
        this.MAGRelatedRunsService.GetMagAutoVisualiseList(crit);
        this.ListCriteria = new MVCMagPaperListSelectionCriteria();
        this.ListCriteria.magAutoUpdateRunId = taskRun.magAutoUpdateRunId;
        this.ListCriteria.autoUpdateUserTopN = taskRun.nPapers;
        this.ListCriteriaTotPapers = taskRun.nPapers;
        this.ListCriteriaFilteredPapers = taskRun.nPapers;
        this.comboAutoUpdateImportOptions = 'AutoUpdate';
        if ((this._classifierService.ClassifierContactAllModelList.length == 0
            && (
            this._classifierService.CurrentUserId4ClassifierContactModelList < 1
            || this._classifierService.CurrentUserId4ClassifierContactModelList != this._ReviewerIdentityServ.reviewerIdentity.userId
            )) || (this._classifierService.CurrentUserId4ClassifierContactModelList < 1
            || this._classifierService.CurrentUserId4ClassifierContactModelList != this._ReviewerIdentityServ.reviewerIdentity.userId)
        ) {
            //only fetch this if it's empty or if it contains a list of models that belongs to someone else. 
            //the second checks on userId prevent leaking when one user logs off, another logs in and finds the list belonging to another user, very ugly, but should work.
            //wait 100ms and then get this list, I don't like sending many server requests all concurrent
            setTimeout(() => { this._classifierService.FetchClassifierContactModelList(this._ReviewerIdentityServ.reviewerIdentity.userId); }, 100);
        }
    }
    public LoadGraph() {
        //console.log("LoadGraph", this.ShowDist);
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
            this.MAGRelatedRunsService.currentlyApplyedModelToRunId = this.CurrentMagAutoUpdateRun.studyTypeClassifier;
            this.MAGRelatedRunsService.currentlyApplyingModelToThisRunId = this.CurrentMagAutoUpdateRun.magAutoUpdateRunId;
            this.MAGRelatedRunsService.RunMagAddClassifierScoresCommand(cmd).then(async res => {
                if (res) {
                    this.NotificationService.show({
                        content: 'Model is being applied, should be ready in 10m (will check every 30s).',
                        animation: { type: 'slide', duration: 400 },
                        position: { horizontal: 'center', vertical: 'top' },
                        type: { style: "info", icon: true },
                        closable: false,
                        hideAfter: 3000
                    });
                    let now = new Date();
                    this.MAGRelatedRunsService.RefreshUntil = new Date(now.getTime() + 10 * 60000); //10 minutes into the future!
                    this.MAGRelatedRunsService.refreshForStudyClassifier = true;
                    this.MAGRelatedRunsService.RefreshAutoUpdateRunsOnTimer();
                    this.CancelImportRefine();
                    //let i: number = 0;
                    //while (i < 20) {
                    //    await Helpers.Sleep(1000 * 25);
                    //    this.MAGRelatedRunsService.GetMagAutoUpdateRunList();
                    //    await Helpers.Sleep(1000 * 5);//we give GetMagAutoUpdateRunList 5s to complete, not a big deal if it's not enough
                    //    i++;
                    //    let newRun = this.MAGRelatedRunsService.MagAutoUpdateRunList.find(f => f.magAutoUpdateRunId == this.MAGRelatedRunsService.currentlyApplyingModelToThisRunId);
                    //    if (newRun != undefined) {
                    //        //we'll check if the model has been applied
                    //        if (newRun.studyTypeClassifier !== this.MAGRelatedRunsService.currentlyApplyedModelToRunId) {
                    //            //yay! it's in, so we can stop looping
                    //            this.MAGRelatedRunsService.currentlyApplyingModelToThisRunId = 0;
                    //            this.MAGRelatedRunsService.currentlyApplyedModelToRunId = "";
                    //            this.CurrentMagAutoUpdateRun = newRun;//update what we have in here!!
                    //            this.LoadGraph();//get update histogram, while we're there...
                    //            i = 100;
                    //            return;
                    //        }
                    //    }
                    //}
                    ////bad luck, either we were re-applying the same model or it's taking more than 5m, we give up
                    //this.MAGRelatedRunsService.currentlyApplyingModelToThisRunId = 0;
                    //this.MAGRelatedRunsService.currentlyApplyedModelToRunId = "";
                }
            });
            this.SelectedStudyClassifier = this.StudyClassifiers[0];
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
            this.MAGRelatedRunsService.currentlyApplyedModelToRunId = this.CurrentMagAutoUpdateRun.userClassifierModelId.toString();
            this.MAGRelatedRunsService.currentlyApplyingModelToThisRunId = this.CurrentMagAutoUpdateRun.magAutoUpdateRunId;
            this.MAGRelatedRunsService.RunMagAddClassifierScoresCommand(cmd).then(async res => {
                if (res) {
                    this.NotificationService.show({
                        content: 'Model is being applied, should be ready in 10m (will check every 30s).',
                        animation: { type: 'slide', duration: 400 },
                        position: { horizontal: 'center', vertical: 'top' },
                        type: { style: "info", icon: true },
                        closable: false,
                        hideAfter: 3000
                    });
                    let now = new Date();
                    this.MAGRelatedRunsService.RefreshUntil = new Date(now.getTime() + 10 * 60000); //10 minutes into the future!
                    this.MAGRelatedRunsService.refreshForStudyClassifier = false;
                    this.MAGRelatedRunsService.RefreshAutoUpdateRunsOnTimer();
                    this.CancelImportRefine();
                    //while (i < 20) {
                    //    await Helpers.Sleep(1000 * 25);
                    //    this.MAGRelatedRunsService.GetMagAutoUpdateRunList();
                    //    await Helpers.Sleep(1000 * 5);//we give GetMagAutoUpdateRunList 5s to complete, not a big deal if it's not enough
                    //    i++;
                    //    let newRun = this.MAGRelatedRunsService.MagAutoUpdateRunList.find(f => f.magAutoUpdateRunId == this.MAGRelatedRunsService.currentlyApplyingModelToThisRunId);
                    //    if (newRun != undefined) {
                    //        //we'll check if the model has been applied
                    //        if (newRun.userClassifierModelId.toString() !== this.MAGRelatedRunsService.currentlyApplyedModelToRunId) {
                    //            //yay! it's in, so we can stop looping
                    //            this.MAGRelatedRunsService.currentlyApplyingModelToThisRunId = 0;
                    //            this.MAGRelatedRunsService.currentlyApplyedModelToRunId = "";
                    //            this.CurrentMagAutoUpdateRun = newRun;//update what we have in here!!
                    //            this.LoadGraph();//get update histogram, while we're there...
                    //            i = 100;
                    //            return;
                    //        }
                    //    }
                    //}
                    ////bad luck, either we were re-applying the same model or it's taking more than 5m, we give up
                    //this.MAGRelatedRunsService.currentlyApplyingModelToThisRunId = 0;
                    //this.MAGRelatedRunsService.currentlyApplyedModelToRunId = "";
                }
            });
            this.SelectedClassifierContactModel = null;
        }
    }
    
    async AutoUpdateCountResultsCommand() {
        //console.log("AAAAh: ", this.ThreshodsForm);
        if (this.CurrentMagAutoUpdateRun != null) {
            let cmd: MagItemPaperInsertCommand = new MagItemPaperInsertCommand();
            //here we use this data structure (MagItemPaperInsertCommand) to exchange this data, because it fits, even if it's not its original purpose
            cmd.magAutoUpdateRunId = this.CurrentMagAutoUpdateRun.magAutoUpdateRunId;
            cmd.autoUpdateScore = this.ListCriteria.autoUpdateAutoUpdateScore;
            cmd.studyTypeClassifierScore = this.ListCriteria.autoUpdateStudyTypeClassifierScore;
            cmd.userClassifierScore = this.ListCriteria.autoUpdateUserClassifierScore;

            let res = await this.MAGRelatedRunsService.AutoUpdateCountResultsCommand(cmd);//returns the number of filtered results or -1 if an error occurred.
            if (res >= 0) {
                if (this.ThreshodsForm) this.ThreshodsForm.resetForm({
                    n1: this.ListCriteria.autoUpdateAutoUpdateScore,
                    n2: this.ListCriteria.autoUpdateStudyTypeClassifierScore,
                    n3: this.ListCriteria.autoUpdateUserClassifierScore
                });
                this.ListCriteriaFilteredPapers = res;
                if (this.ListCriteria.autoUpdateUserTopN > this.ListCriteriaFilteredPapers
                    || this.ListCriteria.autoUpdateUserTopN < this.ListCriteriaFilteredPapers) {
                    this.ListCriteria.autoUpdateUserTopN = this.ListCriteriaFilteredPapers;
                }
            }
        }
    }
    public GetItems() {
        if (this.CurrentMagAutoUpdateRun && this.CurrentMagAutoUpdateRun.magAutoUpdateRunId) {
            this.ListCriteria.magAutoUpdateRunId = this.CurrentMagAutoUpdateRun.magAutoUpdateRunId;
            this.ListCriteria.listType = "MagAutoUpdateRunPapersList";
            this.ListCriteria.pageSize = 20;
            this.ListCriteria.autoUpdateOrderBy = this.comboAutoUpdateImportOptions;
            this._magBrowserService.GetMagOrigList(this.ListCriteria).then(
                (res) => {
                    if (res == true) {
                        this.MAGBrowserHistoryService.AddHistory(MagBrowseHistoryItem.MakeFromAutoUpdateListCrit(this.ListCriteria));
                        this.PleaseGoTo.emit("MagAutoUpdateRunPapersList");//this.router.navigate(['MAGBrowser']);
                    }
                }
            );
        }
    }
    public ImportMagAutoUpdateRun() {
        //mr.magAutoUpdateRunId, mr.orderBy, mr.autoUpdateScore
        //    , mr.studyTypeClassifierScore, mr.userClassifierScore
        //    , mr.TopN, mr.filterJournal, mr.filterDOI, mr.filterURL
        if (this.ListCriteria.autoUpdateUserTopN > 20000) {
            this.ModalService.GenericErrorMessage('Sorry, imports are restricted to 20,000 records.<br /> You can specify the "<strong>Import top</strong>" value and order.');
        }
        else if (this.CurrentMagAutoUpdateRun != null) {
            let cmd: MagItemPaperInsertCommand = new MagItemPaperInsertCommand();
            cmd.magAutoUpdateRunId = this.CurrentMagAutoUpdateRun.magAutoUpdateRunId;
            this.ListCriteria.autoUpdateOrderBy = this.comboAutoUpdateImportOptions;
            cmd.orderBy = this.ListCriteria.autoUpdateOrderBy;
            cmd.autoUpdateScore = this.ListCriteria.autoUpdateAutoUpdateScore;
            cmd.studyTypeClassifierScore = this.ListCriteria.autoUpdateStudyTypeClassifierScore;
            cmd.userClassifierScore = this.ListCriteria.autoUpdateUserClassifierScore;
            cmd.topN = this.ListCriteria.autoUpdateUserTopN;
            cmd.filterJournal = this.FilterOutJournal;
            cmd.filterDOI = this.FilterOutDOI;
            cmd.filterURL = this.FilterOutURL;
            cmd.filterTitle = this.FilterOutTitle;
            this.ConfirmationDialogService.confirm("Importing papers for this auto-update task run",
                "Are you sure you want to import the items as per current settings?", false, '')
                .then((confirm: any) => {
                    if (confirm) {
                        this.MAGRelatedRunsService.ImportAutoUpdateRun(cmd);
                        this.IHaveImportedSomething.emit();
                    }
                });
        }
    }
    CancelImportRefine() {
        this.CurrentMagAutoUpdateRun = null;
    }
    DeleteAutoUpdateRun(task: MagAutoUpdateRun) {
        if (task.magAutoUpdateRunId < 1 || !this.HasWriteRights) return;
        this.ConfirmationDialogService.confirm("Deleting the selected auto update task results",
            "Are you sure you want to delete auto update task results: \"<em>" + task.userDescription + "\"</em>?"
            + "<br> Mag version: <em>"+ task.magVersion + "</em>."
            + "<br> This operation <strong>cannot be undone</strong>!", false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this.MAGRelatedRunsService.DeleteMAGAutoUpdateRun(task.magAutoUpdateRunId);
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
