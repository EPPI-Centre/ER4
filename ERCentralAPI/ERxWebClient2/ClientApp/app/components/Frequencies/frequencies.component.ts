import { Component,  OnInit,  OnDestroy, AfterViewInit, Output, EventEmitter, ViewChild, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { frequenciesService, Frequency } from '../services/frequencies.service';
import { singleNode } from '../services/ReviewSets.service';
import { ItemListComp } from '../ItemList/itemListComp.component';
import { EventEmitterService } from '../services/EventEmitter.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';



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
		private _eventEmitter: EventEmitterService,
		private httpService: HttpClient,
		@Inject('BASE_URL') private _baseUrl: string

    ) { }
     
    ngAfterViewInit() {
        // child is set
	}

	@ViewChild('ItemList') ItemListComponent!: ItemListComp;


	public CheckBoxAutoAdvanceVal: boolean = false;
	public selectedNodeData: any | null = null;

	onSubmit(f: string) {

    }

	NodeDataChange(nodeData: singleNode) {

		this.selectedNodeData = nodeData;
	}

	// ADD CHART OPTIONS. 
	pieChartOptions = {
		responsive: true
	}

	pieChartLabels = ['JAN', 'FEB', 'MAR', 'APR', 'MAY'];

	// CHART COLOR.
	pieChartColor: any = [
		{
			backgroundColor: ['rgba(30, 169, 224, 0.8)',
				'rgba(255,165,0,0.9)',
				'rgba(139, 136, 136, 0.9)',
				'rgba(255, 161, 181, 0.9)',
				'rgba(255, 102, 0, 0.9)'
			]
		}
	]

	pieChartData: any = [
		{
			data: []
		}
	];

	onChartClick(event: any) {
		console.log(event);
	}

	FrequencyNoneOfTheAboveItemsList(item: Frequency) {

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
			//this.httpService.get('./assets/sales.json', { responseType: 'json' }).subscribe(

			//	data => {
			//		this.pieChartData = data as any[];

			//		console.log(this.pieChartData);
			//		// FILL THE CHART ARRAY WITH DATA.
			//	},
			//	(err: HttpErrorResponse) => {
			//		console.log(err.message);
			//	}
			//);

			this.pieChartData = [{ "data": [47, 9, 28, 54, 77] }];

			this._eventEmitter.dataStr.subscribe(

				(data: any) => {

					console.log('this is being emitted freq');
					this.selectedNodeData = data;
				}
			)
		}
    }
    
    ngOnDestroy() {
     
    }

}






