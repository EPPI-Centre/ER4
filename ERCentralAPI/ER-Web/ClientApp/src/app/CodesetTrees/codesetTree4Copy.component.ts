import { Component, Inject, OnInit, Input, ViewChild, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { singleNode, ReviewSet } from '../services/ReviewSets.service';
import { ITreeOptions, TreeComponent } from '@circlon/angular-tree-component';


@Component({
    selector: 'codesetTree4Copy',
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
    templateUrl: './codesetTree4Copy.component.html'
})

export class codesetTree4CopyComponent implements OnInit, OnDestroy {
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
    @Input() SelectedCodeset: ReviewSet | null = null;
    @Input() MaxHeight: number = 600;
	options: ITreeOptions = {
        childrenField: 'attributes', 
        displayField: 'name',
		allowDrag: false,
		
	}

	@ViewChild('tree') treeComponent!: TreeComponent;
	
    get nodes(): singleNode[] | null {
        if (this.SelectedCodeset) {
            const res: singleNode[] = [];
            res.push(this.SelectedCodeset);
            return res;
        }
        else {
            return null;
        }
    }
	

	
	    
    public SelectedNodeData: singleNode | null = null;
	public SelectedCodeDescription: string = "";

	NodeSelected(node: singleNode) {
        this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
	}

    ngOnDestroy() {
       
    }
}





