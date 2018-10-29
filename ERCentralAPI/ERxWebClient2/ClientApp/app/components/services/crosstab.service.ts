import { Inject, Injectable, EventEmitter, Output} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { ReviewSet, SetAttribute, iAttributesList } from './ReviewSets.service';


@Injectable({

    providedIn: 'root',

})

export class crosstabService {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
        ) { }
    
	private _CrossTabList: CrossTab[] = [];
	@Output() codeSelectedChanged = new EventEmitter();
	public testResult: CrossTab = new CrossTab();

	public get CrossTabs(): CrossTab[] {
		if (this._CrossTabList.length == 0) {

			const CrossTabJson = localStorage.getItem('CrossTabs');
			let CrossTabs: CrossTab[] = CrossTabJson !== null ? JSON.parse(CrossTabJson) : [];
			if (CrossTabs == undefined || CrossTabs == null || CrossTabs.length == 0) {
				return this._CrossTabList;
            }
            else {
				this._CrossTabList = CrossTabs;
            }
        }
		return this._CrossTabList;

    }
    
	public set CrossTabs(cs: CrossTab[]) {
		
		this._CrossTabList = cs;

        //this.Save();
    }

	public Fetch(selectedNodeDataX: any, selectedNodeDataY: any, ) {
		


		let asX: SetAttribute = new SetAttribute();
		let rsY: ReviewSet = new ReviewSet();
		let asY: SetAttribute = new SetAttribute();

		if (selectedNodeDataX.nodeType == 'ReviewSet') {

			//let rsX: ReviewSet = new ReviewSet();

			let AttributeIdXaxis: number = 0;
			let SetIdXaxis: number = selectedNodeDataX.SetId;
			let NXaxis: number = selectedNodeDataX.attributes.length;
			let xAxisAttributes: SetAttribute[] = selectedNodeDataX.attributes;

			this.testResult.xHeaders = xAxisAttributes.map(x => x.attribute_name);
			this.testResult.xHeadersID = xAxisAttributes.map(x => x.attribute_id);

			//JSON.stringify(selectedNodeDataX.attributes);
			console.log('crosstabcheck \n  ' + AttributeIdXaxis + ' \n  ' + SetIdXaxis + '\n  '
				+ NXaxis + '\n ' + xAxisAttributes.map(x => x.attribute_name));

		}

		if (selectedNodeDataY.nodeType == 'ReviewSet') {

			let AttributeIdYaxis: number = 0;
			let SetIdYaxis: number = selectedNodeDataY.SetId;
			let NYaxis: number = selectedNodeDataY.attributes.length;
			let yAxisAttributes: SetAttribute[] = selectedNodeDataY.attributes;



			//JSON.stringify(selectedNodeDataX.attributes);
			console.log('crosstabcheck \n  ' + AttributeIdYaxis + ' \n  ' + SetIdYaxis + '\n  '
				+ NYaxis + '\n ' + yAxisAttributes.map(x => x.attribute_name));

			//let yAxisAttributes = JSON.stringify(selectedNodeDataY.attributes);
			//console.log('testing here2: ' + test2);
		}
		
		
		//for (var i = 0; i < Math.min(NXaxis, 50); i++) {

		//	ItemAttributeCrossTabColumns[i] = xAxisAttributes[i].AttributeName;

		//}

		alert('got in cross tab alerting' );

		//this.codeSelectedChanged.emit(selectedNodeData);
		//attributeIdXAxis = 0
		//int setIdXAxis, 27
		//Int64 attributeIdYAxis, 0
		//int setIdYAxis, 58
		//Int64 attributeIdFilter, 0
		//int setIdFilter, 0
		//int nxaxis, 10

		let crit: Criteria = new Criteria();

		crit.attributeIdXAxis = 0;
		crit.setIdXAxis = 27;
		crit.attributeIdYAxis = 0;
		crit.setIdYAxis = 58;
		crit.attributeIdFilter = 0;
		crit.setIdFilter = 0;
		crit.nxaxis = 10;
		
		return this._httpC.post<any[]>(this._baseUrl + 'api/CrossTab/GetCrossTabs',
			crit).subscribe(result => {

				//this.CrossTabs = result;

					this.testResult.yRows = result;

					console.log(this.testResult);

				}
			);

    }

    //public Save() {
    //    if (this._ReviewList.length > 0)
    //        localStorage.setItem('ReadOnlyReviews', JSON.stringify(this._ReviewList));
    //    else if (localStorage.getItem('ReadOnlyReviews'))//to be confirmed!! 
    //        localStorage.removeItem('ReadOnlyReviews');
    //}
}

export class CrossTabCriteria {

	attributeId: number = 0;
	itemCount: string = "";
	attribute: string="";
	attributeSetId: string = "";
	setId: number = 0;
	filterAttributeId: string = "";
	isIncluded: boolean = false;
}

export class CrossTab {
	
	xHeaders: string[] = [];
	xHeadersID: number[] = [];
	yRows: any[] = [];

	//yRowsID: string[] = [];


}

export class Criteria {

	attributeIdXAxis: number = 0;
	setIdXAxis: number = 0;
	attributeIdYAxis: number = 0;
	setIdYAxis: number = 0;
	attributeIdFilter: number = 0;
	setIdFilter: number = 0;
	nxaxis: number = 0;
	
}
