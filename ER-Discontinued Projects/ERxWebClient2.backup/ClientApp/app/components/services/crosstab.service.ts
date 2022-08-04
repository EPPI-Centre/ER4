import { Inject, Injectable, EventEmitter, Output} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { ReviewSet, SetAttribute, iAttributesList } from './ReviewSets.service';
import { BusyAwareService } from '../helpers/BusyAwareService';


@Injectable({

    providedIn: 'root',

})

export class crosstabService extends BusyAwareService  {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) { super(); }
    
	//private _CrossTab: CrossTab = new CrossTab();

	private _xTab: iWebDbItemAttributeCrosstabList | null = null;
	@Output() codeSelectedChanged = new EventEmitter();
	//public Result: CrossTab = new CrossTab();

    //fields below were needed to get the correct data when clicking through to obtain and ItemList, they complement what's in the Criteria object.
    //public NXaxis: number = 0;
    //public filterSetId: number = 0;
    //public filterName: string = "";
    //public attributeNameXAxis: string = "";
    //public attributeNameYAxis: string = "";

    private _Crit: CriteriaCrosstab = new CriteriaCrosstab();

	//private _fieldNames: string[] = [];

	//public get CrossTab(): CrossTab {
	//	return this._CrossTab;
	//}
	public get xTab(): iWebDbItemAttributeCrosstabList | null {
		return this._xTab;
    }
	//public get fieldNames(): string[] {
	//	return this._fieldNames;
	//}

    public get crit(): CriteriaCrosstab {
		return this._Crit;
	}

	//public set fieldNames(fn: string[]) {
	//	this._fieldNames = fn;
	//}
    
	//public set CrossTab(cs: CrossTab) {
	//	this._CrossTab = cs;
	//}

    public set crit(cr: CriteriaCrosstab) {

		this._Crit = cr;

	}
    //we need to get data as "any" because first two params can be of types ReviewSet or SetAttribute, so it's hard to keep things strongly typed
    //(unless we're happy to refactor by overloading/multiplying Fetch method)
	public Fetch(selectedNodeDataX: any, selectedNodeDataY: any, selectedFilter: any, IncEx: string) {
		if (IncEx == "included") {
			this.crit.onlyIncluded = "true";
		} else if (IncEx == "excluded") {
			this.crit.onlyIncluded = "false";
		} else if (IncEx == "bothIandE") {
			this.crit.onlyIncluded = "";
		}
		else return;
        this._BusyMethods.push("Fetch");
		let AttributeIdXaxis: number = 0;
		let xAxisAttributes: SetAttribute[] = [];
		let SetIdXaxis: number = 0;
		let AttributeIdYaxis: number = 0;
		let SetIdYaxis: number = 0;
		let yAxisAttributes: SetAttribute[] = [];
        //this._CrossTab = new CrossTab();
		//this._fieldNames = [];
		if (selectedNodeDataX.nodeType == 'ReviewSet') {

			AttributeIdXaxis = 0;
			SetIdXaxis = selectedNodeDataX.set_id;
			//this.NXaxis = selectedNodeDataX.attributes.length;
			xAxisAttributes = selectedNodeDataX.attributes;
            //this._CrossTab.xHeaders = xAxisAttributes.map(x => x.attribute_name);
            //this._CrossTab.xHeadersID = xAxisAttributes.map(x => x.attribute_id);
			//console.log('crosstabcheck2 \n  ' + AttributeIdXaxis + ' \n  ' + SetIdXaxis + '\n  '
			//	+ this.NXaxis + '\n ' + xAxisAttributes.map(x => x.attribute_name));

		} else {

			AttributeIdXaxis = selectedNodeDataX.attribute_id;
			SetIdXaxis = selectedNodeDataX.set_id;
			//this.NXaxis = selectedNodeDataX.attributes.length;
			xAxisAttributes = selectedNodeDataX.attributes;
            //this._CrossTab.xHeaders = xAxisAttributes.map(x => x.attribute_name);
            //this._CrossTab.xHeadersID = xAxisAttributes.map(x => x.attribute_id);

        }
        //console.log('logging xheaders:');
        //for (let xh of this._CrossTab.xHeaders) console.log(xh);
		if (selectedNodeDataY.nodeType == 'ReviewSet') {

			AttributeIdYaxis = 0;
			SetIdYaxis = selectedNodeDataY.set_id;
			yAxisAttributes 
		} else {

			AttributeIdYaxis = selectedNodeDataY.attribute_id;
			SetIdYaxis = selectedNodeDataY.set_id;
			yAxisAttributes = selectedNodeDataY.attributes;

		}

		//console.log('crosstabcheck2345 \n  ' + this.crit      + ' ewrt \n ' + this.crit.nxaxis);

		this.crit.attributeIdYAxis = AttributeIdYaxis;
        this.crit.setIdYAxis = SetIdYaxis;
        //this.attributeNameYAxis = selectedNodeDataY.name;
		this.crit.attributeIdXAxis = AttributeIdXaxis;
        this.crit.setIdXAxis = SetIdXaxis;
        //this.attributeNameXAxis = selectedNodeDataX.name;

        if (selectedFilter && selectedFilter.attribute_id ) {
            this.crit.attributeIdFilter = selectedFilter.attribute_id;
            //this.filterSetId = selectedFilter.set_id;
            //this.filterName = selectedFilter.name;
		} else {
			this.crit.attributeIdFilter = 0;
		}
		
		this.crit.setIdFilter = 0;
		//this.crit.nxaxis = this.NXaxis;

		
		//return this._httpC.post<ReadOnlyItemAttributeCrosstab[]>(this._baseUrl + 'api/CrossTab/GetCrossTabs',
		return this._httpC.post<iWebDbItemAttributeCrosstabList>(this._baseUrl + 'api/CrossTab/GetCrossTabs',
			this.crit).subscribe(result => {

                //this._CrossTab.yRows = result;
                ////this.CrossTab = this.Result;
                //this._CrossTab.attributeIdXAxis = this.crit.attributeIdXAxis;
                //this._CrossTab.attributeIdYAxis = this.crit.attributeIdYAxis;
                //this._CrossTab.setIdXAxis = this.crit.setIdXAxis;
                //this._CrossTab.setIdYAxis = this.crit.setIdYAxis;
                ////console.log('n axis len: ' + this.NXaxis);
				//for (var i = 1; i <= Math.min(this.NXaxis, 50); i++)
				//{
				//	this._fieldNames[i-1] = "field" + i;
                //    //console.log("field" + i);
				//}
				this._xTab = result;
                this.RemoveBusy("Fetch");
				//console.log('fieldnames len: ' + this._fieldNames.length);
				},
            (error) => {
                console.log("Crosstab fetch error:", error);
                this.RemoveBusy("Fetch");
                this.modalService.GenericError(error);
            }
			);
    }

    public Clear() {
        this._xTab = null;
        //this.NXaxis = 0;
        //this.filterSetId = 0;
        //this.filterName = "";
        //this.attributeNameXAxis = "";
        //this.attributeNameYAxis = "";
        this._Crit = new CriteriaCrosstab();
        //this._fieldNames = [];
    }
}



//export class CrossTab {
	
//	xHeaders: string[] = [];
//	xHeadersID: number[] = [];
//	yRows: ReadOnlyItemAttributeCrosstab[] = [];
//	attributeIdXAxis: number = 0;
//	setIdXAxis: number = 0;
//	attributeIdYAxis: number = 0;
//	setIdYAxis: number = 0;

//}

export class CriteriaCrosstab {

	attributeIdXAxis: number = 0;
	setIdXAxis: number = 0;
	attributeIdYAxis: number = 0;
	setIdYAxis: number = 0;
	attributeIdFilter: number = 0;//want
	setIdFilter: number = 0;
	nxaxis: number = 0;
	onlyIncluded: string = "both";
	//showDeleted: boolean = false;
	//sourceId: number = 0;
	//searchId: number = 0;
	//xAxisSetId: number = 0;
	//xAxisAttributeId: number = 0;
	//yAxisSetId: number = 0;
	//yAxisAttributeId: number = 0;
	//filterSetId: number = 0;
	//filterAttributeId: number = 0;
	//attributeSetIdList: string = "";
	//listType: string = "";
	//attributeid: number = 0;

}


//export class ReadOnlyItemAttributeCrosstab {

//	attributeId: number = 0;
//	attributeName: string = '';
//	field1: number = 0;
//	field2: number = 0;
//	field3: number = 0;
//	field4: number = 0;
//	field5: number = 0;
//	field6: number = 0;
//	field7: number = 0;
//	field8: number = 0;
//	field9: number = 0;
//	field10: number = 0;
//	field11: number = 0;
//	field12: number = 0;
//	field13: number = 0;
//	field14: number = 0;
//	field15: number = 0;
//	field16: number = 0;
//	field17: number = 0;
//	field18: number = 0;
//	field19: number = 0;
//	field20: number = 0;
//	field21: number = 0;
//	field22: number = 0;
//	field23: number = 0;
//	field24: number = 0;
//	field25: number = 0;
//	field26: number = 0;
//	field27: number = 0;
//	field28: number = 0;
//	field29: number = 0;
//	field30: number = 0;
//	field31: number = 0;
//	field32: number = 0;
//	field33: number = 0;
//	field34: number = 0;
//	field35: number = 0;
//	field36: number = 0;
//	field37: number = 0;
//	field38: number = 0;
//	field39: number = 0;
//	field40: number = 0;
//	field41: number = 0;
//	field42: number = 0;
//	field43: number = 0;
//	field44: number = 0;
//	field45: number = 0;
//	field46: number = 0;
//	field47: number = 0;
//	field48: number = 0;
//	field49: number = 0;
//	field50: number = 0;

//}


export interface iWebDbItemAttributeCrosstabList {
	rows: iWebDbItemAttributeCrosstabRow[];
	columnAttIDs: number[];
	columnAttNames: string[];
	filterAttributeId: number;
	setIdX: number;
	setIdY: number;
	setIdXName: string;
	setIdYName: string;
	attibuteIdX: number;
	attibuteIdY: number;
	attibuteIdXName: string;
	attibuteIdYName: string;
	graphic: string;
	included: string;
	//webDbId: number;
}
export interface iWebDbItemAttributeCrosstabRow {
	attributeId: number;
	attributeName: string;
	counts: number[];
}