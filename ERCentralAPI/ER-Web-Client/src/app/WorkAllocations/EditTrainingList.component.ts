import { Component, Inject, OnInit, EventEmitter, Output, AfterContentInit, OnDestroy, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subscription, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocationListService, WorkAllocation } from '../services/WorkAllocationList.service';
import { ItemListService } from '../services/ItemList.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { PriorityScreeningService, Training } from '../services/PriorityScreening.service';
import { GridDataResult, RowClassArgs } from '@progress/kendo-angular-grid';
import { Helpers } from '../helpers/HelperMethods';

@Component({
  selector: 'EditTrainingListComp',
  templateUrl: './EditTrainingList.component.html',
  styles: ['.UsedWorkAllocation { font-weight: bold; background-color: lightblue;}'],
  providers: []
})

export class EditTrainingListComp implements OnInit, OnDestroy {
  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private reviewInfoService: ReviewInfoService,
    private PriorityScreeningService: PriorityScreeningService
  ) { }

  ngOnInit() {
    if (this.PriorityScreeningService.TrainingList.length == 0) {
      this.PriorityScreeningService.Fetch().then(() => { this.BuildLocalList(); });
    } else {
      this.BuildLocalList();
    }
  }
  
  @Output() PleaseCloseMe = new EventEmitter();
  public editingProgressList: Training[] = [];

  public get TrainingList(): Training[] {
    return this.PriorityScreeningService.TrainingList;
  }
  public get TrainingListData(): GridDataResult {
    return {
      data: this.editingProgressList,
      total: this.editingProgressList.length,
    };
  }

  public get HasChanges(): boolean {
    for (let i = 0; i < this.editingProgressList.length; i++) {
      if (this.editingProgressList[i].hidden != this.PriorityScreeningService.TrainingList[i].hidden) return true;
    }
    return false;
  }

  private BuildLocalList() {
    this.editingProgressList = [];
    for (const el of this.PriorityScreeningService.UnfilteredTrainingList) {
      let newEl: Training = {
        trainingId: el.trainingId,
        contactId: el.contactId,
        startTime: el.startTime,
        endTime: el.endTime,
        iteration: el.iteration,
        nTrainingItemsInc: el.nTrainingItemsInc,
        nTrainingItemsExc: el.nTrainingItemsExc,
        contactName: el.contactName,
        c: el.c,
        tp: el.tp,
        tn: el.tn,
        fp: el.fp,
        fn: el.fn,
        totalN: el.totalN,
        totalIncludes: el.totalIncludes,
        totalExcludes: el.totalExcludes,
        hidden: el.hidden
      }
      this.editingProgressList.push(newEl);
    }
  }
  FormatDate2(DateSt: string): string {
    return Helpers.FormatDate2(DateSt);
  }

  public rowCallback = (context: RowClassArgs) => {
    const row: Training = context.dataItem;
    if (row.hidden == true) {
      return { HiddenTrainingRecord: true };
    }
    else return { };
  };


  HasScreeningList(): boolean {
    if (this.reviewInfoService
      && this.reviewInfoService.ReviewInfo
      && this.reviewInfoService.ReviewInfo.showScreening
      && this.reviewInfoService.ReviewInfo.screeningCodeSetId != 0
      && this.reviewInfoService.ReviewInfo.screeningListIsGood
      && this.ReviewerIdentityServ.HasWriteRights)
      return true;
    else return false;
  }

  SelfClose() {
    this.PleaseCloseMe.emit();
  }
  async SaveAndClose() {
    await this.PriorityScreeningService.ShowHideTrainingRecords(this.editingProgressList);
    this.SelfClose();
  }




  Clear() {
    //alert('put stuff in here');
    //this._workAllocationListService.Clear();
    //this._workAllocationContactListService.Save();
  }

  ngOnDestroy() {
    
  }
}





