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
    
	private _CrossTab: CrossTab = new CrossTab();
	@Output() codeSelectedChanged = new EventEmitter();
	public testResult: CrossTab = new CrossTab();
	public NXaxis: number = 0;

	private _fieldNames: string[] = [];

	public get CrossTab(): CrossTab {
		
		if (this._CrossTab ) {

			console.log('got in here 2');
			const CrossTabJson = localStorage.getItem('CrossTab');

			let CrossTab: CrossTab = CrossTabJson !== null ? JSON.parse(CrossTabJson) : [];
			if (CrossTab == undefined || CrossTab == null) {

				console.log('got in here ct');
				return this._CrossTab;
            }
			else {

				console.log('got in here ct2');
				this._CrossTab = CrossTab;
            }
        }
		return this._CrossTab;
	}

	public get fieldNames(): string[] {


		if (this._fieldNames.length == 0) {
			console.log('got in here field');
			const fieldNamesJson = localStorage.getItem('fieldNames');

			let fieldNames: string[] = fieldNamesJson !== null ? JSON.parse(fieldNamesJson) : [];
			if (fieldNames.length == 0 || fieldNames == null) {

				return this._fieldNames;
			}
			else {
				this._fieldNames = fieldNames;
			}
		}
		return this._fieldNames;

	}

	public set fieldNames(fn: string[]) {

		this._fieldNames = fn;

		//this.Save();
	}
    
	public set CrossTab(cs: CrossTab) {
		
		this._CrossTab = cs;

        this.Save();
    }

	public Fetch(selectedNodeDataX: any, selectedNodeDataY: any ) {

		let crit: Criteria = new Criteria();
		let AttributeIdXaxis: number = 0;
		let xAxisAttributes: SetAttribute[] = [];
		let SetIdXaxis: number = 0;
		let AttributeIdYaxis: number = 0;
		let SetIdYaxis: number = 0;
		let yAxisAttributes: SetAttribute[] = [];

		if (selectedNodeDataX.nodeType == 'ReviewSet') {

			AttributeIdXaxis = 0;
			SetIdXaxis = selectedNodeDataX.set_id;
			this.NXaxis = selectedNodeDataX.attributes.length;
			xAxisAttributes = selectedNodeDataX.attributes;
			this.testResult.xHeaders = xAxisAttributes.map(x => x.attribute_name);
			this.testResult.xHeadersID = xAxisAttributes.map(x => x.attribute_id);

			console.log('crosstabcheck \n  ' + AttributeIdXaxis + ' \n  ' + SetIdXaxis + '\n  '
				+ this.NXaxis + '\n ' + xAxisAttributes.map(x => x.attribute_name));

		} else {

			AttributeIdXaxis = selectedNodeDataX.attribute_id;
			SetIdXaxis = selectedNodeDataX.set_id;
			this.NXaxis = selectedNodeDataX.attributes.length;
			xAxisAttributes = selectedNodeDataX.attributes;
			this.testResult.xHeaders = xAxisAttributes.map(x => x.attribute_name);
			this.testResult.xHeadersID = xAxisAttributes.map(x => x.attribute_id);

		}

		if (selectedNodeDataY.nodeType == 'ReviewSet') {

			AttributeIdYaxis = 0;
			SetIdYaxis = selectedNodeDataY.set_id;
			yAxisAttributes = selectedNodeDataY.attributes;

			//console.log('crosstabcheck \n  ' + AttributeIdYaxis + ' \n  ' + SetIdYaxis + '\n  '
			//	+ NYaxis + '\n ' + yAxisAttributes.map(x => x.attribute_name));

		} else {

			AttributeIdYaxis = selectedNodeDataY.attribute_id;
			SetIdYaxis = selectedNodeDataY.set_id;
			yAxisAttributes = selectedNodeDataY.attributes;

		}
		
		crit.attributeIdXAxis = AttributeIdXaxis;
		crit.setIdXAxis = SetIdXaxis;
		crit.attributeIdYAxis = AttributeIdYaxis;
		crit.setIdYAxis = SetIdYaxis;
		crit.attributeIdFilter = 0;
		crit.setIdFilter = 0;
		crit.nxaxis = this.NXaxis;
		
		return this._httpC.post<ReadOnlyItemAttributeCrosstab[]>(this._baseUrl + 'api/CrossTab/GetCrossTabs',
			crit).subscribe(result => {
									
					this.testResult.yRows = result;
					this.CrossTab = this.testResult;

					for (var i = 1; i <= Math.min(this.NXaxis, 50); i++)
					{
						this.fieldNames[i-1] = "field" + i;
						
					}
					this.Save();
					console.log(result);
				}
			);
    }

	public Save() {

		if (this._CrossTab)
			localStorage.setItem('CrossTab', JSON.stringify(this._CrossTab));

		else if (localStorage.getItem('CrossTab'))
			localStorage.removeItem('CrossTab');

		if (this._fieldNames != null)
			localStorage.setItem('fieldNames', JSON.stringify(this._fieldNames));
		else if (localStorage.getItem('fieldNames'))
			localStorage.removeItem('fieldNames');
    }
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
	yRows: ReadOnlyItemAttributeCrosstab[] = [];


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


export class ReadOnlyItemAttributeCrosstab {

	AttributeId: number = 0;
	AttributeName: string = '';
	Field1: number = 0;
	Field2: number = 0;
	Field3: number = 0;
	Field4: number = 0;
	Field5: number = 0;
	Field6: number = 0;
	Field7: number = 0;
	Field8: number = 0;
	Field9: number = 0;
	Field10: number = 0;
	Field11: number = 0;
	Field12: number = 0;
	Field13: number = 0;
	Field14: number = 0;
	Field15: number = 0;
	Field16: number = 0;
	Field17: number = 0;
	Field18: number = 0;
	Field19: number = 0;
	Field20: number = 0;
	Field21: number = 0;
	Field22: number = 0;
	Field23: number = 0;
	Field24: number = 0;
	Field25: number = 0;
	Field26: number = 0;
	Field27: number = 0;
	Field28: number = 0;
	Field29: number = 0;
	Field30: number = 0;
	Field31: number = 0;
	Field32: number = 0;
	Field33: number = 0;
	Field34: number = 0;
	Field35: number = 0;
	Field36: number = 0;
	Field37: number = 0;
	Field38: number = 0;
	Field39: number = 0;
	Field40: number = 0;
	Field41: number = 0;
	Field42: number = 0;
	Field43: number = 0;
	Field44: number = 0;
	Field45: number = 0;
	Field46: number = 0;
	Field47: number = 0;
	Field48: number = 0;
	Field49: number = 0;
	Field50: number = 0;

}
