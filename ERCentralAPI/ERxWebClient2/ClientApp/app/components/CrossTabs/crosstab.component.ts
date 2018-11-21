import { Component,  OnInit,  OnDestroy, AfterViewInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { crosstabService, CrossTab, ReadOnlyItemAttributeCrosstab } from '../services/crosstab.service';
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

	CrossTabItemsList(item: CrossTab, attributeid: any, field: string) {


		this.crosstabService.CrossTab.attributeIdXAxis = item.xHeadersID[Number(field.substr(field.length - 1, field.length)) - 1];
		this.crosstabService.CrossTab.attributeIdYAxis = attributeid;

		console.log('hello \n' + item.xHeadersID + '\n' + 

			+ item.xHeadersID[Number(field.substr(field.length - 1, field.length)) - 1] + '\n'

			+ Number(field.substr(field.length - 1, field.length)) + '\n'

			+ attributeid + '\n ' 

			+ ' BLAH :  ' + 

			' set x: ' + this.crosstabService.CrossTab.setIdXAxis + ' \n '

			 + ' set y: ' +  this.crosstabService.CrossTab.setIdYAxis + ' \n '

			+ ' att x: ' +  this.crosstabService.CrossTab.attributeIdXAxis + ' \n '

			+ ' att x: ' +  this.crosstabService.CrossTab.attributeIdYAxis

		);

		let cr: Criteria = new Criteria();
	
		cr.xAxisSetId = this.crosstabService.CrossTab.setIdXAxis; 
		cr.yAxisSetId = this.crosstabService.CrossTab.setIdYAxis; 
		cr.xAxisAttributeId = this.crosstabService.CrossTab.attributeIdXAxis; 
		cr.yAxisAttributeId = this.crosstabService.CrossTab.attributeIdYAxis ; 

		cr.onlyIncluded = this.crosstabService.crit.onlyIncluded;
		cr.filterAttributeId = this.crosstabService.crit.filterAttributeId;
		cr.listType = 'CrosstabsList';
		cr.attributeSetIdList = this.crosstabService.crit.attributeSetIdList;

		this.ItemListService.FetchWithCrit(cr, "CrosstabsList");
        this._eventEmitter.PleaseSelectItemsListTab.emit();

	}

	
    ngOnInit() {

        
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
		else
		{
			//this._eventEmitter.dataStr.subscribe(

			//	(data: any) => {

			//		console.log('this is being emitted CT');
			//		this.selectedNodeData = data;
			//	}
			//)
		}
    }
    
    ngOnDestroy() {
     
    }

	
}






