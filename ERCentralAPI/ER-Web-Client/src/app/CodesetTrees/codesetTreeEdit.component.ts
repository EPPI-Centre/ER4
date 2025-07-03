import { Component, Inject, OnInit, Input, OnDestroy, ViewChild, AfterViewInit, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, ReviewSet, singleNode, SetAttribute } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { TreeItem } from '@progress/kendo-angular-treeview';
import { faCaretDown, faCaretUp, faAngleDoubleDown, faAngleDoubleUp, faEject, faPlaneDeparture } from '@fortawesome/free-solid-svg-icons';
import { EditCodeComp } from '../CodesetTrees/editcode.component';

@Component({
  selector: 'codesetTreeEdit',
  templateUrl: './codesetTreeEdit.component.html',
  providers: []
})

export class CodesetTreeEditComponent implements OnInit, OnDestroy, AfterViewInit {


  constructor(
    public ReviewerIdentityServ: ReviewerIdentityService,
    private ReviewSetsService: ReviewSetsService,
    private ReviewSetsEditingService: ReviewSetsEditingService
  ) { }
  //@ViewChild('EditCodeComp') EditCodeComp!: EditCodeComp;

  ngOnInit() {
    if (this.ReviewSetsService.ReviewSets.length == 0) {
      this.ReviewSetsService.GetReviewSets(false);
    } else {
      this.CheckReviewSetsOrder();
    }
    
  }
  ngAfterViewInit() {
    this.ShowSelectedCode();
  }
  @Input() CanChangeSelectedCode: boolean = true;
  @Input() CanWriteAndServicesIdle: boolean = false;
  @Output() PleaseShowTheMoveCodePanel = new EventEmitter();
  faCaretDown = faCaretDown;
  faCaretUp = faCaretUp;
  faAngleDoubleDown = faAngleDoubleDown;
  faAngleDoubleUp = faAngleDoubleUp;
  faEject = faEject;
  faPlaneDeparture = faPlaneDeparture;
  public CanWrite(): boolean {
    if (this.CanChangeSelectedCode) {
      return this.CanWriteAndServicesIdle;
    }
    else {
      return false;
    }
  }
  public get ReviewSets(): ReviewSet[] {
    return this.ReviewSetsService.ReviewSets;
  }
  public get selectedNode(): singleNode | null {
    return this.ReviewSetsService.selectedNode;
  }
  private _CurrentReviewSet: ReviewSet | null = null;
  public get CurrentReviewSet(): ReviewSet | null {
    return this._CurrentReviewSet;
  }
  SelectReviewSet(rs: ReviewSet) {
    this._CurrentReviewSet = rs;
  }

  //used as input (not 2-way binding) by the kendo-treeview
  public get selectedKeys(): string[] {
    if (this.selectedNode) return [this.selectedNode.id];
    else return [];
  }
  

  private ShowSelectedCode() {
    if (this.ReviewSetsService.selectedNode == null || this.ReviewSetsService.selectedNode.nodeType != "SetAttribute") return;
    else {
      const selected = this.ReviewSetsService.selectedNode as SetAttribute;
      const Set = this.ReviewSetsService.FindSetById(selected.set_id);
      if (Set) {
        const parents = Set.ParentsListByAttId(selected.attribute_id);
        let toExpand = parents.map(x => x.id);
        toExpand.push(Set.id);
        for (const id of toExpand) {
          if (this.ReviewSetsService.ExpandedNodeKeys.findIndex(f => f == id) == -1) {
            this.ReviewSetsService.ExpandedNodeKeys.push(id);
          }
        }
        //scroll the node in view
        if (document) {
          setTimeout(() => {
            const element = document.getElementById("nodeID-" + selected.id);
            if (element) element.scrollIntoView(false);
          }, 250);
        }
      }
    }
  }

  //IsServiceBusy(): boolean {
  //    if (this.ReviewSetsService.IsBusy || this.ReviewSetsEditingService.IsBusy) return true;
  //    else return false;
  //}
  //CanWrite(): boolean {
  //    if (this.ReviewerIdentityServ.HasWriteRights && !this.ReviewSetsService.IsBusy && !this.ReviewSetsEditingService.IsBusy) return true;
  //    else return false;
  //}
  CheckReviewSetsOrder() {
    let i: number = 0;
    let changedSomething: boolean = false;
    for (let rs of this.ReviewSetsService.ReviewSets) {
      if (rs.order != i) {
        rs.order = i;
        changedSomething = true;
        //do something to save change!
        this.ReviewSetsEditingService.SaveReviewSet(rs);
      }
      i++;
    }
    if (changedSomething) {
      //for sanity, sort by order;
      this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
        return s1.order - s2.order;
      });
    }
  }
  RefreshLocalTree() {
    this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.slice();
  }
  async MoveUpNode(node: singleNode) {
    await this.ReviewSetsEditingService.MoveUpNode(node);
    //and notify the tree:
    this.RefreshLocalTree();
    //if (node.nodeType == 'ReviewSet') {
    //    let MySet = node as ReviewSet;
    //    if (MySet) this.MoveUpSet(MySet);
    //}
    //else {
    //    let MyAtt = node as SetAttribute;
    //    if (MyAtt) {
    //        this.MoveUpAttribute(MyAtt);
    //    }
    //}
  }
  async MoveDownNode(node: singleNode) {
    await this.ReviewSetsEditingService.MoveDownNode(node);
    //and notify the tree:
    this.RefreshLocalTree();
    //if (node.nodeType == 'ReviewSet') {
    //    let MySet = node as ReviewSet;
    //    if (MySet) this.MoveDownSet(MySet);
    //}
    //else {
    //    let MyAtt = node as SetAttribute;
    //    if (MyAtt) this.MoveDownAttribute(MyAtt);
    //}
  }
  async MoveUpNodeFull(node: singleNode) {
    await this.ReviewSetsEditingService.MoveUpNodeFull(node);
    //and notify the tree:
    this.RefreshLocalTree();
    //if (node.nodeType == 'ReviewSet') {
    //    let MySet = node as ReviewSet;
    //    if (MySet) this.MoveUpSet(MySet);
    //}
    //else {
    //    let MyAtt = node as SetAttribute;
    //    if (MyAtt) {
    //        this.MoveUpAttribute(MyAtt);
    //    }
    //}
  }
  async MoveDownNodeFull(node: singleNode) {
    await this.ReviewSetsEditingService.MoveDownNodeFull(node);
    //and notify the tree:
    this.RefreshLocalTree();
    //if (node.nodeType == 'ReviewSet') {
    //    let MySet = node as ReviewSet;
    //    if (MySet) this.MoveDownSet(MySet);
    //}
    //else {
    //    let MyAtt = node as SetAttribute;
    //    if (MyAtt) this.MoveDownAttribute(MyAtt);
    //}
  }

  MoveCode(node: singleNode) {
    this.ReviewSetsService.selectedNode = node;
    this.PleaseShowTheMoveCodePanel.emit();   
  }

  NodeSelected(event: TreeItem) {
    let node: singleNode = event.dataItem;
    this.ReviewSetsService.selectedNode = node;
    //this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
  }
  CanMoveDown(node: singleNode): boolean {
    return this.ReviewSetsEditingService.CanMoveDown(node);
  }
  CanMoveUp(node: singleNode): boolean {
    return this.ReviewSetsEditingService.CanMoveUp(node);
  }
  IsACode(node: singleNode): boolean {
    return this.ReviewSetsEditingService.IsACode(node);
  }
  ngOnDestroy() {
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
  /*END REGION: retain isExpanded data across all trees...*/


}
