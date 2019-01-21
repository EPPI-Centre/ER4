import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, AfterViewInit, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode } from '../services/ReviewSets.service';
import { ITreeOptions, TreeModel, TreeComponent, IActionMapping, TREE_ACTIONS, KEYS } from 'angular-tree-component';
import { ITreeNode } from 'angular-tree-component/dist/defs/api';
import { EventEmitterService } from '../services/EventEmitter.service';


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
	   private _eventEmitterService: EventEmitterService

	) { }

	@Input() MaxHeight: number = 400;
	@Input() rootsOnly: boolean = false;//obsolete
	@Input() IsMultiSelect: boolean = false;
	@Input() WhatIsSelectable: string = "All";
	//"All": can select any type of node
	//"AttributeSet":Codes(AttributeSet) only
	//"ReviewSet":Codesets(ReviewSet) only
	//"NodeWithChildren":Anything that does have children
	//"CanHaveChildren": any node that is allowed to contain children(future)


	public SelectedNodeData: singleNode | null = null;
	public SelectedNodesData: singleNode[] = [];
	public SelectedCodeDescription: string = "";
	@ViewChild('tree') treeComponent!: TreeComponent;
	@Output() selectedNodeInTree: EventEmitter<null> = new EventEmitter();
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
		// use this when a multi select is needed
		//actionMapping: this.actionMapping

	}


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

	NodeSelected(node: singleNode) {

		//console.log(JSON.stringify(node));
		
		//@Input() rootsOnly: boolean = false;//obsolete
		//@Input() IsMultiSelect: boolean = false;
		//@Input() WhatIsSelectable: string = "All";
		//"All": can select any type of node
		//"AttributeSet":Codes(AttributeSet) only
		//"ReviewSet":Codesets(ReviewSet) only
		//"NodeWithChildren":Anything that does have children
		//"CanHaveChildren": any node that is allowed to contain children(future)

		//alert(this.IsMultiSelect);
		//alert(this.WhatIsSelectable);

		// So far six possible paths of logic
		if (this.WhatIsSelectable == "SetAttribute" && this.IsMultiSelect==false) {
			if (node.nodeType == "SetAttribute") {
				console.log(JSON.stringify(node));
				this.SelectedNodeData = node;
	
				this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
				// and raise event to close the drop down
				this.selectedNodeInTree.emit();
				this._eventEmitterService.nodeSelected = node;
			}

		} else if (node.nodeType == "ReviewSet" && this.IsMultiSelect == false) {
			// it must be a root node hence we should do nothing...
			alert('you cannot select these roots here!');

		} else if (this.IsMultiSelect == false) {
			// ALL

		} else if (node.nodeType == "SetAttribute" && this.IsMultiSelect == true) {
			alert('you cannot use multiselect here 1');

		} else if (node.nodeType == "ReviewSet" && this.IsMultiSelect == true) {
			alert('you cannot use multiselect here 2');

		} else if ( this.IsMultiSelect == true) {
			alert('you cannot use multiselect here 3');

		}

    }

    ngOnDestroy() {

    }
}





