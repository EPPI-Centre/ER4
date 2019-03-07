import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, ViewContainerRef } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet, SetAttribute, iAttributeSet } from '../services/ReviewSets.service';
import { ITreeOptions, TreeModel, TreeComponent } from 'angular-tree-component';
import { NgbModal, NgbActiveModal, NgbTabset } from '@ng-bootstrap/ng-bootstrap';
import { ArmsService } from '../services/arms.service';
import { ITreeNode } from 'angular-tree-component/dist/defs/api';
import { frequenciesService } from '../services/frequencies.service';
import { Injectable, EventEmitter } from '@angular/core';
import { Subscription } from 'rxjs';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewSetsEditingService, ReadOnlyTemplateReview } from '../services/ReviewSetsEditing.service';
import { Helpers } from '../helpers/HelperMethods';

@Component({
    selector: 'importCodesetsWizard',
    styles: [``],
    templateUrl: './importCodesetsWizard.component.html'
})

export class ImportCodesetsWizardComponent implements OnInit, OnDestroy {
   constructor(private router: Router,
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
       private ReviewSetsService: ReviewSetsService,
       private ReviewSetsEditingService: ReviewSetsEditingService
	) { }
	
   
	//@Input() attributesOnly: boolean = false;
    
	ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 ||
            this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else if (!this.ReviewerIdentityServ.HasWriteRights) {
            this.router.navigate(['Main']);
        }
		else {
            this.ReviewSetsEditingService.FetchReviewTemplates();
        }
    }
    @Input() IsStandalone: boolean = true;
    @Output()
    PleaseCloseMe = new EventEmitter();
    public WizStep: number = 1;
    public get TemplateReviews(): ReadOnlyTemplateReview[] {
        return this.ReviewSetsEditingService.ReadOnlyTemplateReviews;
    }
    private _selectedTemplate: ReadOnlyTemplateReview | null = null;
    public get SelectedTemplate(): ReadOnlyTemplateReview | null {
        if (this._selectedTemplate == null &&
            this.ReviewSetsEditingService.ReadOnlyTemplateReviews && this.ReviewSetsEditingService.ReadOnlyTemplateReviews.length > 0) {
            this._selectedTemplate = this.ReviewSetsEditingService.ReadOnlyTemplateReviews[0];
        }
        return this._selectedTemplate;
    }
    private _SelectedSet4Copy: ReviewSet | null = null;
    public get SelectedSet4Copy(): ReviewSet | null {
        if (this._SelectedSet4Copy == null &&
            this.ReviewSetsEditingService.ReviewSets4Copy && this.ReviewSetsEditingService.ReviewSets4Copy.length > 0) {
            this._SelectedSet4Copy = this.ReviewSetsEditingService.ReviewSets4Copy[0];
        }
        return this._SelectedSet4Copy;
    }
    public get ReviewSets4Copy(): ReviewSet[] {
        return this.ReviewSetsEditingService.ReviewSets4Copy;
    }
    IsServiceBusy(): boolean {
        if (this.ReviewSetsEditingService.IsBusy || this.ReviewSetsService.IsBusy) return true;
        else return false;
    }
    CanWrite(): boolean {
        if (!this.ReviewerIdentityServ.HasWriteRights) return false;
        else return !this.IsServiceBusy();
    }
    CancelActivity() {
        if (this.WizStep == 1) {
            if (this.IsStandalone) this.BackToMain();
            else {
                //somehow close itself...
                this.PleaseCloseMe.emit();
            }
        }
        if (this.WizStep >= 2) {
            this.WizStep = 1;
            this.ReviewSetsEditingService.clearReReviewSets4Copy();
            this._SelectedSet4Copy = null;
        }
    }
    async ProceedClicked() {
        let roTr = this._selectedTemplate;
        if (!roTr) return;
        if (
            (roTr.templateName == "Manually pick from Public codesets..." && roTr.templateId == 1000)
            ||
            (roTr.templateName == "Manually pick from your own codesets..." && roTr.templateId == 2000)
        )
        {
            this.OpenListOfSets(roTr);
        }
        else if (roTr.templateName.length > 0 && roTr.templateId > 0 && roTr.reviewSetIds && roTr.reviewSetIds.length > 0) {
            //do the copy thing, selfclose when done
            await this.ReviewSetsEditingService.ImportReviewTemplate(roTr);
            console.log("finished waiting for codesets import...")
            let cycle: number = 0;
            //SUPER UGLY: donkey alert!!!
            await Helpers.Sleep(500);
            while (this.ReviewSetsEditingService.IsBusy && cycle < 600) {
                cycle++;
                await Helpers.Sleep(100);
            }
            this.BackToMain();
        }
    }
    
    OpenListOfSets(roTr: ReadOnlyTemplateReview) {
        if (roTr.templateName == "Manually pick from Public codesets..." && roTr.templateId == 1000) {
            this.WizStep = 2.1;
            this.ReviewSetsEditingService.FetchReviewSets4Copy(false);
        }
        else if (roTr.templateName == "Manually pick from your own codesets..." && roTr.templateId == 2000) {
            this.WizStep = 2.2;
            this.ReviewSetsEditingService.FetchReviewSets4Copy(true);
        }
        
    }
    SelectTemplate(ID: number) {
        if (!this.ReviewSetsEditingService.ReadOnlyTemplateReviews || this.ReviewSetsEditingService.ReadOnlyTemplateReviews.length == 0) {
            this._selectedTemplate = null;
        }
        else {
            let res = this.ReviewSetsEditingService.ReadOnlyTemplateReviews.find(res => res.templateId == ID);
            if (res) {
                this._selectedTemplate = res;
            }
            else {
                this._selectedTemplate = this.ReviewSetsEditingService.ReadOnlyTemplateReviews[0];
            }
        }
    }
    SelectSet4Copy(set: ReviewSet) {
        this._SelectedSet4Copy = set;
    }
    async ImportSelectedSet() {
        if (!this._SelectedSet4Copy) return;
        else {
            this.ReviewSetsEditingService.ReviewSetCopy(this._SelectedSet4Copy.reviewSetId, this.ReviewSetsService.ReviewSets.length).then(
                (result) => {
                    if (result.reviewSetId < 0) {
                        console.log("Copy single codeset failed (in service):", this._SelectedSet4Copy, result);
                    }
                    else this.ReviewSetsService.GetReviewSets();
                }
                , (reject) => {
                    console.log("Copy single codeset failed (reject):", this._SelectedSet4Copy, reject);
                }

            ).catch(error => {
                console.log("Copy single codeset failed (catch):", this._SelectedSet4Copy, error);
                });
        }
    }
    BackToMain() {
        this.router.navigate(['Main']);
    }
    ngOnDestroy() {

    }
}





