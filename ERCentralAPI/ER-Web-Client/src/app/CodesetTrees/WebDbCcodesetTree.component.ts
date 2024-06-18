import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode } from '../services/ReviewSets.service';
import { Subscription } from 'rxjs';
import { WebDBService } from '../services/WebDB.service';
import { TreeItem } from '@progress/kendo-angular-treeview';

@Component({
  selector: 'WebDbCcodesetTree',
  templateUrl: './WebDbCcodesetTree.component.html'
})

export class WebDbCcodesetTreeComponent implements OnInit, OnDestroy {
  constructor(private router: Router,
    private ReviewerIdentityServ: ReviewerIdentityService,
    private ReviewSetsService: ReviewSetsService,
    private WebDBService: WebDBService
  ) { }


  @Input() tabSelected: string = '';
  @Input() MaxHeight: number = 800;
  @Input() CanChangeSelectedCode: boolean = true;


  subRedrawTree: Subscription | null = null;

  public smallTree: string = '';

  ngOnInit() {

    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
      this.router.navigate(['home']);
    }
    else {
      this.subRedrawTree = this.WebDBService.PleaseRedrawTheTree.subscribe(
        () => { this.RefreshLocalTree(); }
      );
    }
  }
  public get IsServiceBusy(): boolean {
    return this.ReviewSetsService.IsBusy;
  }

  RefreshLocalTree() {
    this.WebDBService.CurrentSets = this.WebDBService.CurrentSets.slice();
  }
  get nodes(): singleNode[] | null {


    if (this.WebDBService && this.WebDBService.CurrentSets && this.WebDBService.CurrentSets.length > 0) {

      return this.WebDBService.CurrentSets as singleNode[];
    }
    else {
      return null;
    }
  }

  public get SelectedCodeDescription(): string {
    if (this.WebDBService.SelectedNodeData == null) return "";
    else return this.WebDBService.SelectedNodeData.description;
  }
  public TreeClass(): string {
    if (!this.CanChangeSelectedCode) return "disableThisAndChildren";
    else return "AAaaAA";
  }
  NodeSelected(event: TreeItem) {
    let node = event.dataItem as singleNode;
    this.WebDBService.SelectedNodeData = node;
  }

  ngOnDestroy() {
    //this.ReviewerIdentityServ.reviewerIdentity = new ReviewerIdentity();
    if (this.subRedrawTree) this.subRedrawTree.unsubscribe();
    //console.log('killing reviewSets comp');
  }
}
