import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, ViewContainerRef } from '@angular/core';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet, SetAttribute, iAttributeSet } from '../services/ReviewSets.service';
import { ITreeOptions, TreeModel, TreeComponent } from 'angular-tree-component';
import { ITreeNode } from 'angular-tree-component/dist/defs/api';
import { Subscription } from 'rxjs';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';

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

export class CodesetTreeMainComponent implements OnInit, OnDestroy, AfterViewInit {
   constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
       private ReviewSetsService: ReviewSetsService,
       private ReviewSetsEditingService: ReviewSetsEditingService
	) { }
	
	//@ViewChild('ManualModal') private ManualModal: any;

    @Input() tabSelected: string = '';
    @Input() MaxHeight: number = 800;

	//@ViewChild('tabset') tabset!: NgbTabset;

	//@ViewChild(NgbTabset) set content(content: ViewContainerRef) {
	//	this.tabSet = content;
	//};

	public showManualModal: boolean = false;

	subRedrawTree: Subscription | null = null;

	public smallTree: string = '';

	ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else {
            this.subRedrawTree = this.ReviewSetsEditingService.PleaseRedrawTheTree.subscribe(
                () => { this.RefreshLocalTree(); }
            );
			//this._eventEmitter.tabChange.subscribe(

			//	(res: any) => {

			//		if (res.nextId == 'SearchListTab') {

			//			this.smallTree = 'true';
			//			this._eventEmitter.codingTreeVar = true;

			//		};
			//	}

			//);
        }
	}
    public get IsServiceBusy(): boolean {
        return this.ReviewSetsService.IsBusy;
    }
	options: ITreeOptions = {
        childrenField: 'attributes', 
        displayField: 'name',
		allowDrag: false,
		
	}

	@ViewChild('tree') treeComponent!: TreeComponent;
	
	ngAfterViewInit() {
		//alert(this.tabSet);
	}
    RefreshLocalTree() {
        //console.log("RefreshLocalTree (mainfull)", this.treeComponent);
        if (this.treeComponent && this.treeComponent.treeModel) {
            console.log("RefreshLocalTree (mainfull)");
            this.treeComponent.treeModel.update();
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

	rootsCollect() {

		const treeModel: TreeModel = this.treeComponent.treeModel;
		const firstNode: any = treeModel.getFirstRoot();

		var rootsArr: Array<ITreeNode> = [];

		for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

			rootsArr[i] = this.treeComponent.treeModel.roots[i];
			//console.log(rootsArr[i]);
		}

	}

	nodesNotRootsCollect(node: ITreeNode) {

		const treeModel: TreeModel = this.treeComponent.treeModel;
		const firstNode: any = treeModel.getFirstRoot();

		var childrenArr: Array<any> = [];

		for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

			var test = this.treeComponent.treeModel.roots[i];

			childrenArr[i] = test.getVisibleChildren();
			console.log(childrenArr[i]);
			
		}
	}

	onEvent($event: any) {

		alert($event);

	}

	selectAllRoots() {

		const treeModel: TreeModel = this.treeComponent.treeModel;

		const firstNode: any = treeModel.getFirstRoot();

		for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

			this.treeComponent.treeModel.roots[i].setIsActive(false, true);

				//.setIsActive(true, true);

			//if (i == 0) {
			//}
		}
		//console.log('A root: ' + this.treeComponent.treeModel.roots[0].doForAll(x => x.expand()))
	}
	    
    public SelectedNodeData: singleNode | null = null;
    public get SelectedCodeDescription(): string {
        return this.ReviewSetsService.SelectedCodeDescription;
    }

	NodeSelected(node: singleNode) {

		//alert(JSON.stringify(stuff));
		//console.log(JSON.stringify(node));

		//if (this._eventEmitter.codingTreeVar == true) {

		//	if (node.nodeType != 'ReviewSet') {

		//		//this._eventEmitter.nodeSelected = true;
		//		//this.SelectedNodeData = node;
		//		//this._eventEmitter.nodeName = node.name;
		//		//alert('this has the correct number: ' + this._eventEmitter.nodeName);
		//		//this._eventEmitter.sendMessage(node);
  //              this.ReviewSetsService.selectedNode = node;
		//		this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
		//	}
			
		//} else {

		//	console.log(node.name + ' =====> ' + node.nodeType + ' blah ' + this.smallTree);
		//	//this.SelectedNodeData = node;
		//	//this._eventEmitter.sendMessage(node);
        //  this.ReviewSetsService.selectedNode = node;
		//	this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');

		//}

       this.ReviewSetsService.selectedNode = node;
        //this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
	}

    ngOnDestroy() {
        //this.ReviewerIdentityServ.reviewerIdentity = new ReviewerIdentity();
		if (this.subRedrawTree) this.subRedrawTree.unsubscribe();
        //console.log('killing reviewSets comp');
    }
}





