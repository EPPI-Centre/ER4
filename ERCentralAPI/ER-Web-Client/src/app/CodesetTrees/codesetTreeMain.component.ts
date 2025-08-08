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
  //used as input (not 2-way binding) by the kendo-treeview
  public get selectedKeys(): string[] {
    if (this.ReviewSetsService.selectedNode) return [this.ReviewSetsService.selectedNode.id];
    else return [];
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

  /*REGION: retain isExpanded data across all trees...*/
  public get ExpandedNodeKeys(): string[] {
    return this.ReviewSetsService.ExpandedNodeKeys;
  }
  public isExpanded = (dataItem: any, index: string) => {
    //console.log("IsExpanded", dataItem, index);
    const sn = dataItem as singleNode;
    return this.ReviewSetsService.isExpanded(dataItem as singleNode, sn.id);
  };
  public handleCollapse(node: any) {
    //console.log("hCollapse", node);
    const sn = node.dataItem as singleNode;
    this.ReviewSetsService.handleCollapse(sn);
  }
  public handleExpand(node: any) {
    //console.log("hExpand", node);
    const sn = node.dataItem as singleNode;
    //console.log("hExpand2", sn);
    this.ReviewSetsService.handleExpand(sn);
  }
  public IsCollapsed(data: singleNode): boolean {
    return this.ReviewSetsService.ExpandedNodeKeys.findIndex(f => f == data.id) == -1;
  }
  public ExpandAllFromHere(data: singleNode) {
    //console.log("EAFH", data);
    this.ReviewSetsService.ExpandAllFromHere(data);
  }
  public CollapseAllFromHere(data: singleNode) {
    //console.log("EAFH", data);
    this.ReviewSetsService.CollapseAllFromHere(data);
  }
  /*END REGION: retain isExpanded data across all trees...*/
}

