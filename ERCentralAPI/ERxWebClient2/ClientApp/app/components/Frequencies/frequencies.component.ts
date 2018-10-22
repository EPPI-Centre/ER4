import { Component,  OnInit,  OnDestroy, AfterViewInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService } from '../services/ItemList.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { frequenciesService } from '../services/frequencies.service';
import { singleNode } from '../services/ReviewSets.service';
import { CodesetTreeComponent } from '../CodesetTree/codesets.component';


@Component({
   
    selector: 'frequencies',
	templateUrl: './frequencies.component.html',
    providers: [],

})
export class frequenciesComp implements OnInit, OnDestroy, AfterViewInit {

	constructor(private router: Router,
		private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService,
		private route: ActivatedRoute,
        public PriorityScreeningService: PriorityScreeningService,
		public ItemDocsService: ItemDocsService,
		private frequenciesService: frequenciesService

    ) { }
     
    ngAfterViewInit() {
        // child is set
    }

	public CheckBoxAutoAdvanceVal: boolean = false;
	public selectedNodeData: singleNode | null = null;

	onSubmit(f: string) {

    }
	tester() {
		alert('hello');
	}

	NodeDataChange(nodeData: singleNode) {

		this.selectedNodeData = nodeData;
	}

    ngOnInit() {

        
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
		else
		{
			this.frequenciesService.codeSelectedChanged.subscribe(

				(res: any) => { alert(res); }
			);
		}
    }
    
    ngOnDestroy() {
     
    }

}






