import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, AfterViewInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode } from '../services/ReviewSets.service';
import { ITreeOptions, TreeModel, TreeComponent, IActionMapping, TREE_ACTIONS, KEYS } from 'angular-tree-component';
import { ITreeNode } from 'angular-tree-component/dist/defs/api';


@Component({
    selector: 'codesetSelector',
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
    templateUrl: './codesetSelector.component.html'
})

export class codesetSelectorComponent implements OnInit, OnDestroy, AfterViewInit {
   constructor(private router: Router,
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
       private ReviewSetsService: ReviewSetsService,

	) { }
	
	@Input() rootsOnly: boolean = false;
	//@Input() attributesOnly: boolean = false;

	ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
		else {

        }
	}


	optionsRoot: ITreeOptions = {
        //childrenField: 'attributes', 
        displayField: 'name',
		allowDrag: false,
		
	}

	actionMapping: IActionMapping = {
		mouse: {
			click: TREE_ACTIONS.TOGGLE_ACTIVE_MULTI
		},
		keys: {
			[KEYS.ENTER]: (tree, node, $event) => alert(`This is ${node.data.name}`)
		}
	}

	options: ITreeOptions = {
		childrenField: 'attributes', 
		displayField: 'name',
		allowDrag: false,
		actionMapping: this.actionMapping

	}
		

	@ViewChild('tree') treeComponent!: TreeComponent;
	
	ngAfterViewInit() {

	
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

			console.log(rootsArr[i]);
		}

		return rootsArr;

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
	public SelectedCodeDescription: string = "";

	NodeSelected(node: singleNode) {

		//alert(JSON.stringify(this.treeComponent.treeModel.getActiveNodes()));

		//alert(JSON.stringify(stuff));
		console.log(JSON.stringify(node));

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
        this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
	}

    ngOnDestroy() {

    }
}





