import { Component,  OnInit,  OnDestroy, AfterViewInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { frequenciesService, Frequency } from '../services/frequencies.service';
import { singleNode } from '../services/ReviewSets.service';
import { CodesetTreeComponent } from '../CodesetTree/codesets.component';
import { NgbTabset } from '@ng-bootstrap/ng-bootstrap';
import { ItemListComp } from '../ItemList/itemListComp.component';
import { EventEmitterService } from '../services/EventEmitter.service';


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
		private frequenciesService: frequenciesService,
		private _eventEmitter: EventEmitterService

    ) { }
     
    ngAfterViewInit() {
        // child is set
	}
	//@ViewChild('tabset') tabset!: NgbTabset;
	@ViewChild('ItemList') ItemListComponent!: ItemListComp;


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

	FrequencyNoneOfTheAboveItemsList(item: Frequency) {

		//console.log('\n' + item.attributeId + 
		//	'\n' + item.itemCount +
		//	'\n' + item.attribute +
		//	'\n' + item.attributeSetId +
		//	'\n' + item.setId +
		//	'\n' + item.filterAttributeId +
		//	'\n' + item.isIncluded);

		let cr: Criteria = new Criteria();
	
		cr.setId = item.setId; 
		cr.attributeid = item.attributeId; 
		// the below should get its value from the view radio buttons
		cr.onlyIncluded = item.isIncluded;
		cr.filterAttributeId = -1;
		cr.listType = 'StandardItemList';
		cr.attributeSetIdList = item.attributeSetId;

		console.log('test again: \n' + cr);

		this.ItemListService.FetchWithCrit(cr, "StandardItemList");
		this._eventEmitter.selectTabItems();
	}

    ngOnInit() {

        
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
		else
		{
			//this.frequenciesService.codeSelectedChanged.subscribe(

			//	(res: any) => { alert(res); }
			//);
			this._eventEmitter.dataStr.subscribe(

				(data: any) => {
					//console.log(data);
					this.selectedNodeData = data;
				}
			)
		}
    }
    
    ngOnDestroy() {
     
    }

}






