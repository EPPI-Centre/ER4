import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdminService } from '../services/MAGAdmin.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { MAGLog, MAGReview, MagCurrentInfo } from '../services/MAGClasses.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Router } from '@angular/router';

@Component({
    selector: 'MAGAdmin',
    templateUrl: './MAGAdmin.component.html',
    providers: []
})

export class MAGAdminComp implements OnInit {

    constructor(
        public _MAGBrowserHistoryService: MAGBrowserHistoryService,
        public _magAdminService: MAGAdminService,
        public _confirmationDialogService: ConfirmationDialogService,
        private _magAdvancedService: MAGAdvancedService,
        public _notificationService: NotificationService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        public router: Router
    ) {

    }
    public previousMAG: string = '';
    public latestMag: string = '';
    public ScoreThreshold: number = 0.90;
    public FoSThreshold: number = 0.20;
    public SampleSize: number = 20;
    public stepScore: number = 0.01;
    public stepFoS: number = 0.05;
    public stepSampleSize: number = 1;
    public isRunning: boolean = false;
    public reviewId: number = 0;
    public newMagEndPoint: string = '';
    public newMagVersion: string = '';

    // public DoCheckChangedPaperIds() {

    //     let msg: string = "Are you sure?\nPlease check it is not already running first!\nOld: "
    //         + this._magAdminService.previousMAGName + " new: " + this._magAdminService.latestMAGName;
    //     this._confirmationDialogService.confirm('MAG Admin', msg, false, '')
    //         .then((confirm: any) => {
    //             if (confirm) {
    //                 this._magAdminService.DoCheckChangedPaperIds(this._magAdminService.latestMAGName);
    //             }
    //         });
        
    // }
    public RefreshLogTable() {
        this._magAdminService.GetMAGLogList();
    }
    public Back() {
        this.router.navigate(['Main']);
    }
    public get MagLogList(): MAGLog[] {
        return this._magAdminService.MAGLogList;
    }
    public get MagReviewList(): MAGReview[] {
        return this._magAdminService.MAGReviewList;
    }
    public get MagCurrentInfoList(): MagCurrentInfo[] {
      return this._magAdminService.MagCurrentInfoList;
    }
    public isValidDate(value: string | null | undefined): boolean {
      if (!value) return false;

      // Strict format: YYYY-MM-DD
      const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
      if (!m) return false;

      const year = Number(m[1]);
      const month = Number(m[2]);
      const day = Number(m[3]);

      // Basic range checks
      if (month < 1 || month > 12) return false;
      if (day < 1 || day > 31) return false;

      // Real calendar date check (handles leap years, invalid days, etc.)
      const d = new Date(Date.UTC(year, month - 1, day));
      return (
        d.getUTCFullYear() === year &&
        d.getUTCMonth() === month - 1 &&
        d.getUTCDate() === day
      );
    }
    public UpdateMagInfo() {

        if (this.newMagEndPoint != '' && this.newMagVersion != '') {
            this._magAdminService.UpdateMagCurrentInfo(this.newMagEndPoint, this.newMagVersion);
        }
    }
    public AddReviewWithThisId() {

        if (this.reviewId != null && this.reviewId >0) {
            this._magAdminService.AddReview(this.reviewId);
        }
    } 
    public DeleteReview(magReview: MAGReview) {
        let message = "Are you sure you want to delete this row?";
    this._confirmationDialogService.confirm('Please confirm', message, false, '')
      .then(
        (confirmed: any) => {
          if (confirmed) {
            this.ReallyDeleteReview(magReview);
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
    }
    public ReallyDeleteReview(magReview: MAGReview){
      if (magReview.reviewId != null ) {
            this._magAdminService.DeleteReview(magReview.reviewId);
        }
    }
    public RefreshReviewList() {

        this._magAdminService.GetMAGReviewList();
    }
    public RefreshMagCurrentInfoList() {

        this._magAdminService.GetMagCurrentInfoList();
    }

    public CheckContReviewPipeLine() {
    let message = "Are you sure you want to run the pipeline from: " + this._magAdminService.MagCurrentInfo.magFolder + " to: " + this.newMagVersion;
    this._confirmationDialogService.confirm('Please confirm', message, false, '')
      .then(
        (confirmed: any) => {
          if (confirmed) {
            this.CheckContReviewPipeLineAlreadyRunning();
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
  }

    public CheckContReviewPipeLineAlreadyRunning() {

        let running: boolean = false;
        let msg: string = '';
        this._magAdvancedService.CheckContReviewPipelineState().then(
            (result) => { running = result; }
            );
        if (running) {
            msg = 'There is a MAG pipeline already running!';
        } else {
            msg = 'Running pipline...';
            this.DoRunContReviewPipeline("", 0, "Pipeline running...", this.FoSThreshold, this.SampleSize, this.ScoreThreshold);
        }
        this._confirmationDialogService.showMAGRunMessage(msg);
    }
    public MakeLive(selectedMagCurrentInfo: MagCurrentInfo){
      if (selectedMagCurrentInfo.makesDeploymentStatus != "LIVE"){
        selectedMagCurrentInfo.makesDeploymentStatus = "LIVE";
        this._magAdminService.UpdateThisMagCurrentInfo(selectedMagCurrentInfo);
      }
      else
      {
        this._confirmationDialogService.showMAGRunMessage("This line cannot be set to LIVE");
      }
    }
    public MakePending(selectedMagCurrentInfo: MagCurrentInfo){
      if (selectedMagCurrentInfo.makesDeploymentStatus != "PENDING"){
        selectedMagCurrentInfo.makesDeploymentStatus = "PENDING";
        this._magAdminService.UpdateThisMagCurrentInfo(selectedMagCurrentInfo);
      }
      else
      {
        this._confirmationDialogService.showMAGRunMessage("This line cannot be set to PENDING");
      }

    }
    public DeleteCurrentInfo(selectedMagCurrentInfo: MagCurrentInfo){
       let message = "Are you sure you want to delete this row?";
    this._confirmationDialogService.confirm('Please confirm', message, false, '')
      .then(
        (confirmed: any) => {
          if (confirmed) {
            this.ReallyDeleteCurrentInfo(selectedMagCurrentInfo);
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
    }
    public ReallyDeleteCurrentInfo(selectedMagCurrentInfo: MagCurrentInfo){
      this._magAdminService.DeleteMagCurrentInfo(selectedMagCurrentInfo.magCurrentInfoId);
    }

    public DownloadResults(selectedMagLog: MAGLog){
      let message = "Are you sure you want to download these results?";
    this._confirmationDialogService.confirm('Please confirm', message, false, '')
      .then(
        (confirmed: any) => {
          if (confirmed) {
            this.ReallyDownloadResults(selectedMagLog);
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
    }
    public ReallyDownloadResults(selectedMagLog: MAGLog){
      const i = selectedMagLog.jobMessage.indexOf("Folder:");
      const folder = i >= 0 ? selectedMagLog.jobMessage.substring(i).replace("Folder:", "") : "";
      if (folder != ""){
        this.DoRunContReviewPipeline(folder, selectedMagLog.magLogId, "Downloading data from folder: " + folder, 0, 0, 0);
      }
      else
      {
        this._confirmationDialogService.showMAGRunMessage("Cannot locate folder");
      }
    }

    public DoRunContReviewPipeline(specificFolder: string, magLogId: number, alertText: string, editFoSThreshold: number,
        editReviewSampleSize: number, editScoreThreshold: number ) : void {
        console.log('editFoSThreshold:', editFoSThreshold);
        this._magAdminService.DoRunContReviewPipeline(specificFolder, magLogId, alertText, editFoSThreshold,
            editReviewSampleSize, editScoreThreshold, this._magAdminService.MagCurrentInfo.magFolder, this.newMagVersion );
    }
    public get IsServiceBusy(): boolean {

        return this._magAdminService.IsBusy;
    }
    ngOnInit() {
        if (!this._ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) this.Back();
        if (this._magAdminService != null) {

            //this._magAdminService.GetMAGBlobCommand();
            this._magAdminService.GetMAGReviewList();
            this._magAdminService.GetMagCurrentInfoList();
            this.RefreshLogTable();
            //this._magAdminService.FetchMagCurrentInfo();
        }
    }  

}
