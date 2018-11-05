import { Component,  OnInit,  OnDestroy, AfterViewInit, Output, EventEmitter, ViewChild, Inject, Input } from '@angular/core';
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
import { BaseChartDirective } from 'ng2-charts';



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
	@ViewChild('testChart') testChart!: BaseChartDirective;
	@ViewChild('ItemList') ItemListComponent!: ItemListComp;

	public show: string = 'table';


	test() {

		alert('showing again...');
	}

	public CheckBoxAutoAdvanceVal: boolean = false;
	public selectedNodeData: any | null = null;

	onSubmit(f: string) {

    }


	NodeDataChange(nodeData: singleNode) {

		this.selectedNodeData = nodeData;
	}

	// ADD CHART OPTIONS. 

	public pieChartOptions: any = {
		responsive: true
	};

	public pieChartLabels: Array<string> =
		['January', 'February', 'March', 'April', 'May', 'June', 'July'];

	// CHART COLOR.
	public pieChartColor: any = [
		{
			backgroundColor: ['rgba(30, 169, 224, 0.8)',
				'rgba(255,165,0,0.9)',
				'rgba(139, 136, 136, 0.9)',
				'rgba(255, 161, 181, 0.9)',
				'rgba(255, 102, 0, 0.9)'
			]
		}
	]

	public chartColours: string[] = [];	

	public dynamicColours(dataLength: number) {
		
		for (var i = 0; i < dataLength; i++) {

			var r = Math.floor(Math.random() * 255);
			var g = Math.floor(Math.random() * 255);
			var b = Math.floor(Math.random() * 255);

			this.chartColours[i] = 'rgba(' + r + ', 100 ,' + b + ', 1)';

		}
		
	};

	//public pieChartColor: Array<any> = [
	//	{ // grey
	//		backgroundColor: 'rgba(148,159,177,0.2)',
	//		borderColor: 'rgba(148,159,177,1)',
	//		pointBackgroundColor: 'rgba(148,159,177,1)',
	//		pointBorderColor: '#fff',
	//		pointHoverBackgroundColor: '#fff',
	//		pointHoverBorderColor: 'rgba(148,159,177,0.8)'
	//	}
	//];

	public pieChartLegend: boolean = true;
	public pieChartType: string = 'pie';

	// events
	public chartClicked(e: any): void {
		console.log(e);
	}

	public chartHovered(e: any): void {
		console.log(e);
	}

	public pieChartData: any = [
		{
			data: []
		}
	];

	//onChartClick(event: any) {

	//	//this.testChart.labels = this.frequenciesService.Frequencies.map(

	//	//	(x) => {

	//	//		//console.log(x.attribute.toString());
	//	//		return x.attribute;
	//	//	}

	//	//);

	//	//console.log(event);
	//	console.log(this.pieChartData + '\n');

	//	this.removeData(this.testChart);

	//	//console.log(this.testChart.data + '\n');
	//	console.log(this.testChart.options + '\n');
	//}

	//removeData(chart: any) {

	//	chart.labels.pop();

	//	chart.datasets.forEach((dataset: any) => {
	//		dataset.data.pop();
	//	});

	//	this.testChart.getChartBuilder(this.testChart.ctx);
	//}


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

	public pieData: number[] = [];
	public pieLabel: string[] = [];

	ngOnInit() {

		this.pieChartData = [{ "data": [47, 9, 28, 54, 77] }];

		if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {

			this.router.navigate(['home']);

		}
		else {

				this._eventEmitter.showFreqView.subscribe(

					(x: any) => {

						//clear them and start again..
						//this.pieData = [];
						//this.pieLabel = [];
						//this.chartColours = [];

						////this.pieChartData.forEach((dataset: any) => {
						////	console.log('do we get here...' + dataset.data);
						////});
						
						//this.frequenciesService.Frequencies.forEach(

						//	(y, i) => {

						//		this.pieData[i] = Number(y.itemCount);
						//		this.pieLabel[i] = y.attribute;
							
						//	});

						//this.dynamicColours(this.frequenciesService.Frequencies.length);
						//this.pieChartColors = this.chartColours;
						//this.pieChartData = [{ "data": this.pieData }];
						//this.pieChartLabels = this.pieLabel ;
						//console.log('do we get here...' + this.pieData);
						this.show = x;
					}
				)						

			this._eventEmitter.dataStr.subscribe(

				(data: any) => {

					console.log('this is being emitted freq');
					this.selectedNodeData = data;

				}
			)

			this.frequenciesService.frequenciesChanged.subscribe(

				() => {

					this.pieData = [];
					this.pieLabel = [];
					this.chartColours = [];

					this.frequenciesService.Frequencies.forEach(

						(y, i) => {

							this.pieData[i] = Number(y.itemCount);
							this.pieLabel[i] = y.attribute;

						});

					this.dynamicColours(this.frequenciesService.Frequencies.length);
					this.pieChartData = [{ "data": this.pieData }];
					this.pieChartColor = [{ "backgroundColor": this.chartColours }];
					this.pieChartLabels = this.pieLabel;
					console.log('do we get here...' + this.chartColours);

				});
		}
	}
    
	ngOnDestroy() {

     
	}

}






