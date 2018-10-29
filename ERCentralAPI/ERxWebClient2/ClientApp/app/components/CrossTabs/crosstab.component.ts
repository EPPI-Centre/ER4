import { Component,  OnInit,  OnDestroy, AfterViewInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { crosstabService, CrossTab, CrossTabCriteria } from '../services/crosstab.service';
import { singleNode } from '../services/ReviewSets.service';
import { CodesetTreeComponent } from '../CodesetTree/codesets.component';
import { NgbTabset } from '@ng-bootstrap/ng-bootstrap';
import { ItemListComp } from '../ItemList/itemListComp.component';
import { EventEmitterService } from '../services/EventEmitter.service';


@Component({
   
    selector: 'crosstab',
	templateUrl: './crosstab.component.html',
    providers: [],

})
export class CrossTabsComp implements OnInit, OnDestroy, AfterViewInit {

	constructor(private router: Router,
		private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService,
		private route: ActivatedRoute,
        public PriorityScreeningService: PriorityScreeningService,
		public ItemDocsService: ItemDocsService,
		private crosstabService: crosstabService,
		private _eventEmitter: EventEmitterService

    ) { }
     
    ngAfterViewInit() {

		this.crossTabResult = this.crosstabService.testResult;
	}

	@ViewChild('ItemList') ItemListComponent!: ItemListComp;
	public crossTabResult: any | null;

	public CheckBoxAutoAdvanceVal: boolean = false;
	public selectedNodeData: any | null = null;

	onSubmit(f: string) {

    }
	tester() {
		alert('hello');
	}

	NodeDataChange(nodeData: singleNode) {

		this.selectedNodeData = nodeData;
	}

	CrossTabItemsList(item: CrossTabCriteria) {

		let cr: Criteria = new Criteria();
	
		cr.setId = item.setId; 
		cr.attributeid = item.attributeId; 
		// the below should get its value from the view radio buttons
		cr.onlyIncluded = item.isIncluded;
		cr.filterAttributeId = -1;
		cr.listType = 'StandardItemList';
		cr.attributeSetIdList = item.attributeSetId;
		
		this.ItemListService.FetchWithCrit(cr, "StandardItemList");
		this._eventEmitter.selectTabItems();
	}

    ngOnInit() {

        
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
		else
		{
			this._eventEmitter.dataStr.subscribe(

				(data: any) => {
			
					this.selectedNodeData = data;
				}
			)
		}
    }
    
    ngOnDestroy() {
     
    }

	
}






