import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, ViewContainerRef } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet, SetAttribute, iAttributeSet } from '../services/ReviewSets.service';
//import { ITreeOptions, TreeModel, TreeComponent, TreeNode } from '@circlon/angular-tree-component';
import { Subscription } from 'rxjs';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { TreeItem, TreeViewComponent } from "@progress/kendo-angular-treeview";

@Component({
    selector: 'codesetTreeMain',
    styles: [`.bt-infoBox {    
                    padding: .08rem .12rem .12rem .12rem;
                    margin-bottom: .12rem;
                    font-size: .875rem;
                    line-height: 1.2;
                    border-radius: .2rem;
                }
			.no-select{    
				-webkit-user-select: none;
				cursor:not-allowed; /*makes it even more obvious*/
				}
        `],
    templateUrl: './codesetTreeMain.component.html'
})

export class CodesetTreeMainComponent implements OnInit, OnDestroy {
   constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
       private ReviewSetsService: ReviewSetsService,
       private ReviewSetsEditingService: ReviewSetsEditingService
	) { }
  ngOnInit() {
    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
      this.router.navigate(['home']);
    }
    else {
      this.subRedrawTree = this.ReviewSetsEditingService.PleaseRedrawTheTree.subscribe(
        () => { this.RefreshLocalTree(); }
      );
    }
  }
  @Input() tabSelected: string = '';
  @Input() MaxHeight: number = 800;
  @ViewChild('tree') treeComponent!: TreeViewComponent;
  public showManualModal: boolean = false;
  subRedrawTree: Subscription | null = null;
  public smallTree: string = '';

  public get IsServiceBusy(): boolean {
    return this.ReviewSetsService.IsBusy;
  }
  //options: ITreeOptions = {
  //  childrenField: 'attributes',
  //  displayField: 'name',
  //  idField: 'id',
  //  allowDrag: false,
  //}

  RefreshLocalTree() {
    //console.log("RefreshLocalTree (mainfull)", this.treeComponent);
    //if (this.treeComponent && this.treeComponent.treeModel) {
    //  console.log("RefreshLocalTree (mainfull)");
    //  this.treeComponent.treeModel.update();
    //}
    if (this.treeComponent) {
      console.log("RefreshLocalTree (mainfull)");
      const tmp = this.ReviewSetsService.ReviewSets;
      this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.slice();
    }
  }
  get nodes(): singleNode[] | null {
    if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) {
      return this.ReviewSetsService.ReviewSets;
    }
    else {
      return null;
    }
  }
  public get SelectedCodeDescription(): string {
        return this.ReviewSetsService.SelectedCodeDescription;
  }
  NodeSelected(node: singleNode) {
    this.ReviewSetsService.selectedNode = node;
  }
  onSelectionChange(event: TreeItem) {
    //console.log(event);
    let node: singleNode = event.dataItem;
    this.ReviewSetsService.selectedNode = node;
  }
    ngOnDestroy() {
        //this.ReviewerIdentityServ.reviewerIdentity = new ReviewerIdentity();
		if (this.subRedrawTree) this.subRedrawTree.unsubscribe();
        //console.log('killing reviewSets comp');
    }
}





