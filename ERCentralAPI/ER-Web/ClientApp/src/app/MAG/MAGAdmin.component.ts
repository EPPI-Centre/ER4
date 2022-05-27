import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MAGAdminService } from '../services/MAGAdmin.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { MAGLog, MAGReview } from '../services/MAGClasses.service';
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
    public ScoreThreshold: number = 0.20;
    public FoSThreshold: number = 0.20;
    public SampleSize: number = 20;
    public stepScore: number = 0.01;
    public stepFoS: number = 0.05;
    public stepSampleSize: number = 1;
    public isRunning: boolean = false;
    public reviewId: number = 0;
    public newMagEndPoint: string = '';
    public newMagVersion: string = '';
    public DoCheckChangedPaperIds() {

        let msg: string = "Are you sure?\nPlease check it is not already running first!\nOld: "
            + this._magAdminService.previousMAGName + " new: " + this._magAdminService.latestMAGName;
        this._confirmationDialogService.confirm('MAG Admin', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magAdminService.DoCheckChangedPaperIds(this._magAdminService.latestMAGName);
                }
            });
        
    }
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
        console.log("got in here");
        if (magReview.reviewId != null ) {
            this._magAdminService.DeleteReview(magReview.reviewId);
        }
    }
    public RefreshReviewList() {

        this._magAdminService.GetMAGReviewList();
    }
    public CheckContReviewPipeLine() {

        let running: boolean = false;
        let msg: string = '';
        this._magAdvancedService.CheckContReviewPipelineState().then(
            (result) => { running = result; }
            );
        if (running) {
            msg = 'There is a MAG pipeline already running!';
        } else {
            msg = 'Running pipline...';
            this.DoRunContReviewPipeline("", 0, "Pipeline running...", this.FoSThreshold, this.SampleSize,
            this.ScoreThreshold);
        }
        this._confirmationDialogService.showMAGRunMessage(msg);
    }
    public DoRunContReviewPipeline(specificFolder: string, magLogId: number, alertText: string, editFoSThreshold: number,
        editReviewSampleSize: number, editScoreThreshold: number ) : void {
        console.log('editFoSThreshold:', editFoSThreshold);
        this._magAdminService.DoRunContReviewPipeline(specificFolder, magLogId, alertText, editFoSThreshold,
            editReviewSampleSize, editScoreThreshold );
    }
    public get IsServiceBusy(): boolean {

        return this._magAdminService.IsBusy;
    }
    ngOnInit() {
        if (!this._ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) this.Back();
        if (this._magAdminService != null) {

            this._magAdminService.GetMAGBlobCommand();
            this._magAdminService.GetMAGReviewList();
            this.RefreshLogTable();
            //this._magAdminService.FetchMagCurrentInfo();
        }
    }  

}
