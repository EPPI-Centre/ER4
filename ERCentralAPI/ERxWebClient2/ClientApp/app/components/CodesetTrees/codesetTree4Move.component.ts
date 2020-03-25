import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, ViewContainerRef } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { singleNode, ReviewSet, SetAttribute, iAttributeSet } from '../services/ReviewSets.service';
import { ITreeOptions, TreeModel, TreeComponent } from 'angular-tree-component';
import { ITreeNode } from 'angular-tree-component/dist/defs/api';

import { Subscription } from 'rxjs';
import { ReviewSetsEditingService, singleNode4move, ReviewSet4Move } from '../services/ReviewSetsEditing.service';
import { interceptingHandler } from '@angular/common/http/src/module';

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
        @Inject('BASE_URL') 
        private ReviewerIdentityServ: ReviewerIdentityService
	) { }
	

	ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
		else {
        }
    }
    ngAfterViewInit() {
        if (this.treeComponent) {
            for (let node of this.treeComponent.treeModel.getVisibleRoots())
                this.treeComponent.treeModel.setExpandedNode(node, true);
        }
    }
    @Input() SelectedCodeset: ReviewSet | null = null;
    @Input() SelectedNode: singleNode | null = null;
    @Input() MaxHeight: number = 600;
	options: ITreeOptions = {
        childrenField: 'attributes', 
        displayField: 'name',
		allowDrag: false,
		
	}

	@ViewChild('tree') treeComponent!: TreeComponent;
    private _SelectedCodeset4move: singleNode4move[] | null = null;
    get nodes(): singleNode4move[] | null {
        if (this.SelectedCodeset && this._SelectedCodeset4move == null && this.SelectedNode) {
            const res: singleNode4move[] = [];
            res.push(new ReviewSet4Move(this.SelectedCodeset, this.SelectedNode));
            this._SelectedCodeset4move = res;
        }
        return this._SelectedCodeset4move;
    }
    
	public SelectedCodeDescription: string = "";

    NodeSelected(node: ITreeNode) {
        let data = node.data as singleNode4move;
        if (data) {
            if (!data.CanMoveBranchInHere) {
                console.log("Current node can't accept branch to be moved...");
            }
            this.SelectedCodeDescription = data.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
        }
	}
    public get CanMoveBranchHere(): boolean {
        if (!this.treeComponent) return false;
        else {
            let node = this.treeComponent.treeModel.getActiveNode();
            if (!node) return false;
            else {
                let data = node.data as singleNode4move;
                if (!data) return false;
                else {
                    return data.CanMoveBranchInHere;
                }
            }
        }
    }
    public get DestinationBranch(): singleNode | null {
        if (!this.treeComponent) return null;
        else {
            let node = this.treeComponent.treeModel.getActiveNode();
            if (!node) return null;
            else {
                let data = node.data as singleNode;
                if (!data) return null;
                else {
                    return data;
                }
            }
        }
    }
    public get DestinationBranchName(): string {
        if (!this.treeComponent) return "No Valid Selection";
        else {
            let node = this.treeComponent.treeModel.getActiveNode();
            if (!node) return "No Valid Selection";
            else {
                let data = node.data as singleNode4move;
                if (!data || !data.CanMoveBranchInHere) return "No Valid Selection";
                else {
                    return data.name;
                }
            }
        }
    }

    ngOnDestroy() {
       
    }
}




