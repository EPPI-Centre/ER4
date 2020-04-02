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
        private ReviewSetsEditingService: ReviewSetsEditingService
    ) { }
    ngOnInit() {
        
        if (this.ReviewSetsService.ReviewSets.length == 0) {
            this.ReviewSetsService.GetReviewSets();
        } else {
            this.CheckReviewSetsOrder();
        }
    }
    @ViewChild('tree') treeComponent!: TreeComponent;
    @Input() CanChangeSelectedCode: boolean = true;
    @Input() CanWriteAndServicesIdle: boolean = false;
    options: ITreeOptions = {
        childrenField: 'attributes',
        displayField: 'name',
        allowDrag: false,

    }
    public CanWrite(): boolean {
        if (this.CanChangeSelectedCode) {
            console.log("1: ", this.CanWriteAndServicesIdle);
            return this.CanWriteAndServicesIdle;
        }
        else {
            console.log("2: ", false);
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
        this.treeComponent.treeModel.update();
    }
    async MoveUpNode(node: singleNode) {
        await this.ReviewSetsEditingService.MoveUpNode(node);
        //and notify the tree:
        this.treeComponent.treeModel.update();
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
        this.treeComponent.treeModel.update();
        //if (node.nodeType == 'ReviewSet') {
        //    let MySet = node as ReviewSet;
        //    if (MySet) this.MoveDownSet(MySet);
        //}
        //else {
        //    let MyAtt = node as SetAttribute;
        //    if (MyAtt) this.MoveDownAttribute(MyAtt);
        //}
    }
    

    NodeSelected(node: singleNode) {
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
