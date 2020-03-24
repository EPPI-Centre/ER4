import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, ViewContainerRef } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet, SetAttribute, iAttributeSet } from '../services/ReviewSets.service';
import { ITreeOptions, TreeModel, TreeComponent } from 'angular-tree-component';
import { ITreeNode } from 'angular-tree-component/dist/defs/api';

import { Subscription } from 'rxjs';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
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
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
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
export interface singleNode4move extends singleNode {
    CanMoveBranchInHere: boolean;
}
export class ReviewSet4Move extends ReviewSet implements singleNode4move {
    constructor(reviewSet: ReviewSet, movingBrach: singleNode) {
        super();
        this.MovingBrach = movingBrach;
        this.set_id = reviewSet.set_id;
        this.reviewSetId = reviewSet.reviewSetId;
        this.set_name = reviewSet.set_name;
        this.order = reviewSet.order;
        this.codingIsFinal = reviewSet.codingIsFinal;
        this.allowEditingCodeset = reviewSet.allowEditingCodeset;
        this.description = reviewSet.description;
        this.setType = reviewSet.setType;
        this.attributes = [];
        this.MovingBrachDepth = this.FindDepthOfBranch(this.MovingBrach, 1);
        this._canMoveBranchInHere = (this.MovingBrachDepth <= this.setType.maxDepth);
        this._alreadyIsTheParent = movingBrach.parent == 0;
        for (let att of reviewSet.attributes) {
            let newA = new SetAttribute4Move(att, 1, this.MovingBrachDepth, this.setType.maxDepth, false, movingBrach.id, movingBrach.parent);
            this.attributes.push(newA);
        }
        

    }
    private FindDepthOfBranch(branch: singleNode, startingDepth: number): number {//RECURSIVE!!!
        if (branch.attributes.length > 0) {
            startingDepth++;
            let tmpMax = startingDepth;
            for (let a of branch.attributes) {
                let Al = this.FindDepthOfBranch(a, startingDepth);
                if (Al > tmpMax) tmpMax = Al;
                startingDepth = tmpMax;
            }
        }
        console.log('FindDepthOfBranch', startingDepth);
        return startingDepth;
    }
    private MovingBrach: singleNode;
    private MovingBrachDepth: number;
    private _canMoveBranchInHere: boolean;
    private _alreadyIsTheParent: boolean;
    get CanMoveBranchInHere(): boolean {
        if (this._alreadyIsTheParent) return false;
        return this._canMoveBranchInHere;
    }
}
export class SetAttribute4Move extends SetAttribute implements singleNode4move {
    constructor(setAttribute: SetAttribute, currentDepth: number, movingBranchDepth: number, maxDepth:number, alreadyCant:boolean, movingBranchRootId:string, movingBranchParentId: number) {
        super();
        this.attribute_id = setAttribute.attribute_id;
        this.attribute_name = setAttribute.attribute_name;
        this.order = setAttribute.order;
        this.attribute_type = setAttribute.attribute_type;
        this.attribute_type_id = setAttribute.attribute_type_id;
        this.attribute_set_desc = setAttribute.attribute_set_desc;
        this.attributeSetId = setAttribute.attributeSetId;
        this.parent_attribute_id = setAttribute.parent_attribute_id;
        this.attribute_desc = setAttribute.attribute_desc;
        this.set_id = setAttribute.set_id;
        this.attribute_order = setAttribute.attribute_order;
        if (this.attribute_id == movingBranchParentId) this._alreadyIsTheParent = true;
        else this._alreadyIsTheParent = false;
        if (alreadyCant) this._canMoveBranchInHere = false;
        else if (currentDepth + movingBranchDepth > maxDepth) this._canMoveBranchInHere = false;
        else if ("A" + this.attribute_id == movingBranchRootId) this._canMoveBranchInHere = false;
        else this._canMoveBranchInHere = true;
        
        for (let att of setAttribute.attributes) {
            let newA = new SetAttribute4Move(att, currentDepth + 1, movingBranchDepth, maxDepth, !this._canMoveBranchInHere, movingBranchRootId, movingBranchParentId);
            this.attributes.push(newA);
        }
    }
    private _canMoveBranchInHere: boolean;
    private _alreadyIsTheParent: boolean;
    get CanMoveBranchInHere(): boolean {
        //console.log('can move in here:', this.name, this._canMoveBranchInHere, this._alreadyIsTheParent);
        if (this._alreadyIsTheParent) return false;
        return this._canMoveBranchInHere;
    }
}





