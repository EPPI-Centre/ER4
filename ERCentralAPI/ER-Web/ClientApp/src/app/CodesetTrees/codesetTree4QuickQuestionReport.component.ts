import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, ViewContainerRef } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet, SetAttribute, iAttributeSet } from '../services/ReviewSets.service';
import { ITreeOptions, TreeModel, TreeComponent } from 'angular-tree-component';


@Component({
    selector: 'codesetTree4QuickQuestionReport',
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
    templateUrl: './codesetTree4QuickQuestionReport.component.html'
})

export class CodesetTree4QuickQuestionReportComponent implements OnInit, OnDestroy, AfterViewInit {
   constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
       private ReviewSetsService: ReviewSetsService,

	) { }

    @Input() tabSelected: string = '';
    @Input() MaxHeight: number = 800;
    
    private _selectedNodes: singleNode[] = [];
    public get SelectedNodes(): singleNode[] {
        return this._selectedNodes;
    }

	ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
		else {
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
	}

    get nodes(): singleNode[] | null {
        if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) {

            return this.ReviewSetsService.ReviewSets;
        }
        else {
            return null;
        }
    }
    
    
    CheckBoxClicked(event: any, node: singleNode ) {
        //console.log("question sel event:", node.id, this._selectedNodes, this._selectedNodes.length);
        if (event.target.checked) {
            //checkbox was checked
            if (this._selectedNodes.findIndex(found => found.id == node.id) == -1) {
                //we only add it if it isn't there: this should always happen...
                this._selectedNodes.push(node);
            }
        }
        else {
            //un-checked
            const i = this._selectedNodes.findIndex(found => found.id == node.id);
            //console.log("trying to remove selection...", i, this._selectedNodes.length);
            if (i != -1) {
                //array contains the node, get it out...
                //console.log("normal remove selection...");
                this._selectedNodes.splice(i, 1);
            }
        }
        //console.log("question sel event (end):", node.id, this._selectedNodes, this._selectedNodes.length);
    }
	

    ngOnDestroy() {
        //this.ReviewerIdentityServ.reviewerIdentity = new ReviewerIdentity();
		//this.sub.unsubscribe();
        //console.log('killing reviewSets comp');
    }
}





