import { Component, Inject, OnInit, Input, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, iSetType, ReviewSet, singleNode, SetAttribute } from '../services/ReviewSets.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ITreeOptions, TreeComponent } from 'angular-tree-component';

@Component({
    selector: 'codesetTreeEdit',
    templateUrl: './codesetTreeEdit.component.html',
    providers: []
})

export class CodesetTreeEditComponent implements OnInit, OnDestroy {

   

    constructor(private router: Router,
                @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService,
        private ReviewSetsService: ReviewSetsService,
        private ReviewSetsEditingService: ReviewSetsEditingService,
    ) { }
    ngOnInit() {
        
        if (this.ReviewSetsService.ReviewSets.length == 0) {
            this.ReviewSetsService.GetReviewSets();
        } else {
            this.CheckReviewSetsOrder();
        }
    }
    @ViewChild('tree') treeComponent!: TreeComponent;
    @Input() CanWrite: boolean = false;
    @Input() IsServiceBusy: boolean = false;
    options: ITreeOptions = {
        childrenField: 'attributes',
        displayField: 'name',
        allowDrag: false,

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
                //recursive CheckAttributeSet of attributes?
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

    AddCodeSet() {
        alert("Not Yet!");
    }
    MoveUpNode(node: singleNode) {
        if (node.nodeType == 'ReviewSet') {
            let MySet = node as ReviewSet;
            if (MySet) this.MoveUpSet(MySet);
        }
        else {
            let MyAtt = node as SetAttribute;
            if (MyAtt) {
                this.MoveUpAttribute(MyAtt);
            }
        }
    }
    MoveDownNode(node: singleNode) {
        if (node.nodeType == 'ReviewSet') {
            let MySet = node as ReviewSet;
            if (MySet) this.MoveDownSet(MySet);
        }
        else {
            let MyAtt = node as SetAttribute;
            if (MyAtt) this.MoveDownAttribute(MyAtt);
        }
    }
    MoveDownSet(rs: ReviewSet) {
        let index = this.ReviewSetsService.ReviewSets.findIndex(found => found.set_id == rs.set_id);
        if (index == -1 || index == this.ReviewSetsService.ReviewSets.length - 1) {
            //oh! should not happen... do nothing?
            return;
        }
        let swapper: ReviewSet = this.ReviewSetsService.ReviewSets[index + 1];
        rs.order++;
        this.ReviewSetsEditingService.SaveReviewSet(rs);
        if (swapper) {
            swapper.order--;
            this.ReviewSetsEditingService.SaveReviewSet(swapper);
        }
        //now sort by order;
        this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
            return s1.order - s2.order;
        });
        //and notify the tree:
        this.treeComponent.treeModel.update();
    }
    MoveUpSet(rs: ReviewSet) {
        //console.log("before:", rs, rs.order);
        let index = this.ReviewSetsService.ReviewSets.findIndex(found => found.set_id == rs.set_id);
        if (index <= 0) {
            //oh! should not happen... do nothing?
            return;
        }
        let swapper: ReviewSet = this.ReviewSetsService.ReviewSets[index -1];
        //console.log("mid1:", rs, rs.order);
        rs.order = rs.order - 1;
        //console.log("mid2 :", rs, rs.order);
        this.ReviewSetsEditingService.SaveReviewSet(rs);
        if (swapper) {
            swapper.order = swapper.order + 1;
            this.ReviewSetsEditingService.SaveReviewSet(swapper);
        }
        //now sort by order;
        this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
            return s1.order - s2.order;
        });
        //and notify the tree:
        this.treeComponent.treeModel.update();
        //console.log("after:", rs, rs.order);
    }
    MoveDownAttribute(Att: SetAttribute) {
        //silently does nothing if data doesn't make sense
        let swapper: SetAttribute | null = null;
        let SortingParent: ReviewSet | SetAttribute | null = null;//used to update what user sees
        let index: number = -1;
        if (Att.parent_attribute_id == 0) {
            let Set: ReviewSet | null = this.ReviewSetsService.FindSetById(Att.set_id);
            if (!Set) return;
            index = Set.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
            if (index <= 0) {
                //oh! should not happen... do nothing?
                return;
            }
            swapper = Set.attributes[index + 1];
            SortingParent = Set;
        }
        else {
            let Parent: SetAttribute | null = this.ReviewSetsService.FindAttributeById(Att.parent_attribute_id);
            if (!Parent) return;
            index = Parent.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
            if (index <= 0) {
                //oh! should not happen... do nothing?
                return;
            }
            swapper = Parent.attributes[index + 1];
            if (!swapper) return;
            SortingParent = Parent;
        }
        if (!swapper || index < 0) return;
        //all is good: do changes
        swapper.order = swapper.order - 1;
        Att.order = Att.order + 1;
        this.ReviewSetsEditingService.MoveSetAttribute(Att.attributeSetId, Att.parent_attribute_id, Att.parent_attribute_id, Att.order);
        SortingParent.attributes.sort((s1, s2) => {
            return s1.order - s2.order;
        });
        this.treeComponent.treeModel.update();
    }
    MoveUpAttribute(Att: SetAttribute) {
        let swapper: SetAttribute | null = null;
        let SortingParent: ReviewSet | SetAttribute | null = null;//used to update what user sees
        let index: number = -1;
        if (Att.parent_attribute_id == 0) {
            let Set: ReviewSet | null = this.ReviewSetsService.FindSetById(Att.set_id);
            if (!Set) return;
            index = Set.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
            if (index <= 0) {
                //oh! should not happen... do nothing?
                return;
            }
            swapper = Set.attributes[index - 1];
            SortingParent = Set;
        }
        else {
            let Parent: SetAttribute | null = this.ReviewSetsService.FindAttributeById(Att.parent_attribute_id);
            if (!Parent) return;
            index = Parent.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
            if (index <= 0) {
                //oh! should not happen... do nothing?
                return;
            }
            swapper = Parent.attributes[index - 1];
            if (!swapper) return;
            SortingParent = Parent;
        }
        if (!swapper || index < 0) return;
        //all is good: do changes
        swapper.order = swapper.order + 1;
        Att.order = Att.order - 1;
        this.ReviewSetsEditingService.MoveSetAttribute(Att.attributeSetId, Att.parent_attribute_id, Att.parent_attribute_id, Att.order);
        SortingParent.attributes.sort((s1, s2) => {
            return s1.order - s2.order;
        });
        this.treeComponent.treeModel.update();
    }
    NodeSelected(node: singleNode) {
        this.ReviewSetsService.selectedNode = node;
        //this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
    }
    CanMoveDown(node: singleNode): boolean {
        if (!this.ReviewSetsService.ReviewSets || this.ReviewSetsService.ReviewSets.length < 1) return false;
        else if (node.nodeType == 'ReviewSet') {
            //console.log("AAAAAAAAA", node);
            if (node.order != this.ReviewSetsService.ReviewSets.length - 1) return true;
            else return false;
        }
        else {//this is an attribute, more work needed...
            let SetAtt = node as SetAttribute;
            if (SetAtt) {
                if (SetAtt.parent == 0) {
                    //att is in the root.
                    let MySet = this.ReviewSetsService.FindSetById(SetAtt.set_id);
                    if (MySet) {
                        if (SetAtt.order != MySet.attributes.length - 1) return true;
                        else return false;
                    }
                }
                else {
                    //att is inside the tree
                    let MyParent = this.ReviewSetsService.FindAttributeById(SetAtt.parent_attribute_id);
                    if (MyParent) {
                        //console.log("CanMoveDown", SetAtt.order, "PA_ID:" + MyParent.attribute_id, MyParent.attributes.length);
                        if (SetAtt.order != MyParent.attributes.length - 1) return true;
                        else return false;
                    }
                }
            }
        }
        return false;
    }
    CanMoveUp(node: singleNode): boolean {
        if (!this.ReviewSetsService.ReviewSets || this.ReviewSetsService.ReviewSets.length < 1) return false;
        else if (node.nodeType == 'ReviewSet') {
            if (node.order != 0 && this.ReviewSetsService.ReviewSets.length > 1) return true;
            else return false;
        }
        else {//this is an attribute, more work needed...
            let SetAtt = node as SetAttribute;
            if (SetAtt) {
                if (SetAtt.parent == 0) {
                    //att is in the root.
                    let MySet = this.ReviewSetsService.FindSetById(SetAtt.set_id);
                    if (MySet) {
                       // console.log(MySet, SetAtt);
                        if (SetAtt.order != 0 && MySet.attributes.length > 1) return true;
                        else return false;
                    }
                }
                else {
                    //att is inside the tree
                    let MyParent = this.ReviewSetsService.FindAttributeById(SetAtt.parent_attribute_id);
                    if (MyParent) {
                        if (SetAtt.order != 0 && MyParent.attributes.length > 1) return true;
                        else return false;
                    }
                }
            }
        }
        return false;
    }
    ngOnDestroy() {
    }
}
