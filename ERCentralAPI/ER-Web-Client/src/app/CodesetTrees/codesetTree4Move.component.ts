import { Component, OnInit, Input, ViewChild, OnDestroy, AfterViewInit } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { singleNode, ReviewSet } from '../services/ReviewSets.service';
//import { ITreeNode } from '@circlon/angular-tree-component/
import { singleNode4move, ReviewSet4Move } from '../services/ReviewSetsEditing.service';
import { TreeItem, TreeViewComponent } from '@progress/kendo-angular-treeview';

@Component({
  selector: 'codesetTree4Move',
  styles: [`
			.no-select{    
				-webkit-user-select: none;
				cursor:not-allowed; /*makes it even more obvious*/
                opacity: 0.5;
                text-decoration: line-through;
				}
        `],
  templateUrl: './codesetTree4Move.component.html'
})

export class codesetTree4Move implements OnInit, AfterViewInit, OnDestroy {
  constructor(private router: Router,
    private ReviewerIdentityServ: ReviewerIdentityService
  ) { }


  ngOnInit() {
    if (!this.ReviewerIdentityServ.reviewerIdentity || this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
      this.router.navigate(['home']);
    }
    else {
    }
  }
  ngAfterViewInit() {
    
  }

  //@ViewChild("treeview", { static: true })
  //public treeview: TreeViewComponent;
  @ViewChild("Tree") public treeComponent!: TreeViewComponent;
  @Input() SelectedCodeset: ReviewSet | null = null;
  @Input() SelectedNode: singleNode | null = null;
  @Input() MaxHeight: number = 600;

  public SelectedCodeDescription: string = "";
  private innerSelectedNode: singleNode4move | null = null;

  private _SelectedCodeset4move: singleNode4move[]  = [];
  get nodes(): singleNode4move[] {
    if (this.SelectedCodeset && this._SelectedCodeset4move.length == 0 && this.SelectedNode) {
      const res: singleNode4move[] = [];
      res.push(new ReviewSet4Move(this.SelectedCodeset, this.SelectedNode));
      this._SelectedCodeset4move = res;
      setTimeout(() => {
        if (!this.treeComponent) console.log("viewchild didn't work...");
        if (this.treeComponent && this.nodes && this.nodes.length > 0) {
          this.treeComponent.expandNode(this.nodes[0], '0');// setExpandedNode(node, true);
        }
      }, 90);
    }
    return this._SelectedCodeset4move;
  }
  //used as input (not 2-way binding) by the kendo-treeview
  public get selectedKeys(): string[] {
    if (this.innerSelectedNode) return [this.innerSelectedNode.id];
    else return [];
  }
  NodeSelected(event: TreeItem) {
    let data = event.dataItem as singleNode4move;
    if (data) {
      this.innerSelectedNode = data;
      if (!data.CanMoveBranchInHere) {
        console.log("Current node can't accept branch to be moved...");
      }
      this.SelectedCodeDescription = data.description;
    }
  }
  public get CanMoveBranchHere(): boolean {
      let node = this.innerSelectedNode;
      if (!node) return false;
      else return node.CanMoveBranchInHere;
  }
  public CanMoveBranchBelowthisNode(node: singleNode4move | null): boolean {
    if (!node) {
      return false;
    }
    else if (node.parent === -1) {
      return false;
    }
    else {
      if (node.id === this.SelectedNode?.id) {
        return false;
      }
      else if (node.parent === this.SelectedNode?.parent) {
        return true;
      }
      else {
        return false;
      }
    }
  }
  public get CanMoveBranchBelowHere(): boolean {
    return this.CanMoveBranchBelowthisNode(this.innerSelectedNode);
  }
  public get DestinationBranch(): singleNode | null {
    let node = this.innerSelectedNode;
      if (!node) return null;
      else {
        let data = node as singleNode;
        if (!data) return null;
        else {
          return data;
        }
      }
  }
  public DestinationBranchName(isMoveInto: boolean): string {
    let node = this.innerSelectedNode;
    if (!node) return "No Valid Selection";
    else if (isMoveInto) { 
       if (!node.CanMoveBranchInHere) return "No Valid Selection";
      else {
        return node.name;
      }
    } else {
      if (!this.CanMoveBranchBelowHere) return "No Valid Selection";
      else {
        return node.name;
      }
    }
  }

  ngOnDestroy() {
  }
}
