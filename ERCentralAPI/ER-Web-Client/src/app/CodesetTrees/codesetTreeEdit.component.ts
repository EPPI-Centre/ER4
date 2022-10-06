import { Component, Inject, OnInit, Input, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { TreeItem } from '@progress/kendo-angular-treeview';
import { faCaretDown, faCaretUp, faAngleDoubleDown, faAngleDoubleUp, faEject } from '@fortawesome/free-solid-svg-icons';

@Component({
    selector: 'codesetTreeEdit',
    templateUrl: './codesetTreeEdit.component.html',
    providers: []
})

export class CodesetTreeEditComponent implements OnInit, OnDestroy {


  constructor(
    public ReviewerIdentityServ: ReviewerIdentityService,
        private ReviewSetsService: ReviewSetsService,
        private ReviewSetsEditingService: ReviewSetsEditingService
    ) { }
    ngOnInit() {
        if (this.ReviewSetsService.ReviewSets.length == 0) {
            this.ReviewSetsService.GetReviewSets(false);
        } else {
            this.CheckReviewSetsOrder();
        }
    }
    @Input() CanChangeSelectedCode: boolean = true;
    @Input() CanWriteAndServicesIdle: boolean = false;
  faCaretDown = faCaretDown;
  faCaretUp = faCaretUp;
  faAngleDoubleDown = faAngleDoubleDown;
  faAngleDoubleUp = faAngleDoubleUp;
  faEject = faEject;
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

    ngOnDestroy() {
    }
}
