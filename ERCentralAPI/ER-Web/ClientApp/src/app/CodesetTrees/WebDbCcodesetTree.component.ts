import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, ViewContainerRef } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode } from '../services/ReviewSets.service';
import { ITreeOptions, TreeComponent } from 'angular-tree-component';
import { Subscription } from 'rxjs';
import { WebDBService } from '../services/WebDB.service';

@Component({
	selector: 'WebDbCcodesetTree',
	templateUrl: './WebDbCcodesetTree.component.html'
})

export class WebDbCcodesetTreeComponent implements OnInit, OnDestroy, AfterViewInit {
   constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
       private ReviewSetsService: ReviewSetsService,
	   private WebDBService: WebDBService
	) { }
	
	//@ViewChild('ManualModal') private ManualModal: any;

    @Input() tabSelected: string = '';
    @Input() MaxHeight: number = 800;
    @Input() CanChangeSelectedCode: boolean = true;
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
            this.subRedrawTree = this.WebDBService.PleaseRedrawTheTree.subscribe(
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
            //console.log("RefreshLocalTree (webDB)");
            this.treeComponent.treeModel.update();
        }
    }
    get nodes(): singleNode[] | null {


		if (this.WebDBService && this.WebDBService.CurrentSets && this.WebDBService.CurrentSets.length > 0) {

			return this.WebDBService.CurrentSets as singleNode[];
        }
        else {
            return null;
        }
    }

    public get SelectedCodeDescription(): string {
        if (this.WebDBService.SelectedNodeData == null) return "";
        else return this.WebDBService.SelectedNodeData.description;
    }

	NodeSelected(node: singleNode) {
        this.WebDBService.SelectedNodeData = node;
	}

    ngOnDestroy() {
        //this.ReviewerIdentityServ.reviewerIdentity = new ReviewerIdentity();
		if (this.subRedrawTree) this.subRedrawTree.unsubscribe();
        //console.log('killing reviewSets comp');
    }
}





