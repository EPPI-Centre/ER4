import { Component, Inject, OnInit, Input, ViewChild, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { singleNode, ReviewSet } from '../services/ReviewSets.service';
import { TreeItem } from '@progress/kendo-angular-treeview';


@Component({
    selector: 'codesetTree4Copy',
    styles: [],
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

  NodeSelected(event: TreeItem) {
    const node: singleNode = event.dataItem;
    this.SelectedCodeDescription = node.description;
	}

    ngOnDestroy() {
       
    }
}





