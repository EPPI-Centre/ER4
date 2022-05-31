import { Component,  OnInit,  OnDestroy, AfterViewInit } from '@angular/core';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { crosstabService, iWebDbItemAttributeCrosstabList, iWebDbItemAttributeCrosstabRow } from '../services/crosstab.service';
import { singleNode, ReviewSetsService, SetAttribute } from '../services/ReviewSets.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { frequenciesService, Frequency } from '../services/frequencies.service';
import { ExcelService } from '../services/excel.service';


@Component({
   
    selector: 'FreqXtabMaps',
    templateUrl: './FreqXtabMaps.component.html',
    providers: [],

})
export class FreqXtabMapsComp implements OnInit, OnDestroy, AfterViewInit {

	constructor(
		private ItemListService: ItemListService,
		private reviewSetsService: ReviewSetsService,
		private crosstabService: crosstabService,
        private _eventEmitter: EventEmitterService,
        private frequenciesService: frequenciesService,
        private ExcelService: ExcelService
    ) { }

    ngOnInit() {

    }
     
    ngAfterViewInit() {
		//this.crossTabResult = this.crosstabService.Result;
	}
    
    public crosstbShowWhat: string = 'table';
    //public crossTabResult = this.crosstabService.Result;
    public crosstbIncEx: string = 'bothIandE';
    public selectedFilterAttribute: singleNode | null = null;
    public selectedNodeDataX: singleNode | null = null;
    public selectedNodeDataY: singleNode | null = null;
    public FreqShowWhat: string = 'table';
    public NoneOfTheAboveCB: boolean = true;


    public get selectedNode(): singleNode | null {
        return this.reviewSetsService.selectedNode;
    }
    //public get CrossTab() {
    //    return this.crosstabService.CrossTab;
    //}
    public get xTab(): iWebDbItemAttributeCrosstabList | null {
        return this.crosstabService.xTab;
    }
    public get Frequencies(): Frequency[] {
        return this.frequenciesService.Frequencies;
    }

    canSetCode(): boolean {
        if (this.reviewSetsService.selectedNode
            && this.reviewSetsService.selectedNode.attributes
            && this.reviewSetsService.selectedNode.attributes.length > 0) return true;
        return false;
    }
    canSetFilter(): boolean {
        if (this.reviewSetsService.selectedNode
            && this.reviewSetsService.selectedNode.nodeType == "SetAttribute") return true;
        return false;
    }


    xTabItemsList(xTab: iWebDbItemAttributeCrosstabList, row: iWebDbItemAttributeCrosstabRow, ColumnIndex: number) {
        console.log("xTabItemsList...", xTab, row, ColumnIndex);
        let crit: Criteria = new Criteria();
        crit.pageSize = this.ItemListService.ItemList.pagesize;
        crit.listType = "ErWithWithoutCodes";
        //the included and "filterAtt" values are taken from the xTab object, not the UI, as user may change them there...
        if (xTab.included == "") crit.onlyIncluded = null;
        else if (xTab.included == "true") crit.onlyIncluded = true;
        else if (xTab.included == "false") crit.onlyIncluded = false;
        else return;
        const RowIndex = xTab.rows.findIndex(f => f == row);
        if (RowIndex == -1) return;
        if (RowIndex == xTab.rows.length - 1) {
            //last row, with the column code AND without all codes in the rows
            crit.withAttributesIds = xTab.columnAttIDs[ColumnIndex].toString();
            crit.withSetIdsList = xTab.setIdX.toString();
            for (let i = 0; i < xTab.rows.length - 1; i++) {
                crit.withOutAttributesIdsList += xTab.rows[i].attributeId.toString() + ",";
                crit.withOutSetIdsList += xTab.setIdY + ",";
            }
            crit.withOutAttributesIdsList = crit.withOutAttributesIdsList.substring(0, crit.withOutAttributesIdsList.length - 1);
            crit.withOutSetIdsList = crit.withOutSetIdsList.substring(0, crit.withOutSetIdsList.length - 1);
            crit.description = "Crosstab results, last cell for column: '" + xTab.columnAttNames[ColumnIndex] +"'";
        }
        else if (ColumnIndex == row.counts.length - 1) {
            //last column: with the row code AND without all codes in columns
            crit.withAttributesIds = row.attributeId.toString();
            crit.withSetIdsList = xTab.setIdY.toString();
            for (let i = 0; i < row.counts.length - 1; i++) {
                crit.withOutAttributesIdsList += xTab.columnAttIDs[i].toString() + ",";
                crit.withOutSetIdsList += xTab.setIdX + ",";
            }
            crit.withOutAttributesIdsList = crit.withOutAttributesIdsList.substring(0, crit.withOutAttributesIdsList.length - 1);
            crit.withOutSetIdsList = crit.withOutSetIdsList.substring(0, crit.withOutSetIdsList.length - 1);
            crit.description = "Crosstab results, last cell for row: '" + row.attributeName + "'";
        }
        else {
            //normal: with the 2 codes from column, row, (optional filter)
            //NOTE: the last cell of the last row always counts "0", so these are the only 3 possible cases
            crit.withAttributesIds = xTab.columnAttIDs[ColumnIndex] + "," + row.attributeId;
            crit.withSetIdsList = xTab.setIdX + "," + xTab.setIdY;
            crit.description = "Crosstab results, with codes: '" + xTab.columnAttNames[ColumnIndex] + "' and '" + row.attributeName + "'";
        }
        if (xTab.filterAttributeId != null && xTab.filterAttributeId > 0 && crit.withAttributesIds.length > 0) {
            const FilterAtt = this.reviewSetsService.FindAttributeById(xTab.filterAttributeId);
            if (FilterAtt != null) {
                crit.withAttributesIds += "," + FilterAtt.attribute_id;
                crit.withSetIdsList += "," + FilterAtt.set_id;
            }
        }

        this.ItemListService.FetchWithCrit(crit, crit.description);
        this._eventEmitter.PleaseSelectItemsListTab.emit();
    }
	
    setXaxis() {
        if (
            this.reviewSetsService.selectedNode
            && this.reviewSetsService.selectedNode.attributes
            && this.reviewSetsService.selectedNode.attributes.length > 0
        ) this.selectedNodeDataX = this.reviewSetsService.selectedNode;
    }
    setYaxis() {
        if (
            this.reviewSetsService.selectedNode
            && this.reviewSetsService.selectedNode.attributes
            && this.reviewSetsService.selectedNode.attributes.length > 0
        ) this.selectedNodeDataY = this.reviewSetsService.selectedNode;
    }

    setFilter() {
        if (
            this.reviewSetsService.selectedNode
            && this.reviewSetsService.selectedNode.nodeType == "SetAttribute"
        ) this.selectedFilterAttribute = this.reviewSetsService.selectedNode;
    }
    clearFilter() {
        this.selectedFilterAttribute = null;
    }
    fetchCrossTabs() {
        if (!this.selectedNodeDataX || this.selectedNodeDataX == undefined || !this.selectedNodeDataY
            || this.selectedNodeDataY == undefined) {

            alert('Please select both sets from the code tree');

        } else {
            this.frequenciesService.Frequencies = [];
            this.crosstabService.Fetch(this.selectedNodeDataX, this.selectedNodeDataY, this.selectedFilterAttribute, this.crosstbIncEx);
        }
    }

    fetchFrequencies(selectedNodeDataF: any, selectedFilter: singleNode | null) {
        //console.log('NoneOfTheAboveCB:' + this.NoneOfTheAboveCB);
        if (!selectedNodeDataF || selectedNodeDataF == undefined) {

            alert('Please select a code from the tree');

        } else {

            //console.log(selectedNodeDataF.name);
            // need to filter data before calling the below Fetch
            if (this.crosstbIncEx == 'included') {
                this.frequenciesService.crit.Included = true;
            } else if (this.crosstbIncEx == 'excluded') {
                this.frequenciesService.crit.Included = false;
            } else if (this.crosstbIncEx == 'bothIandE') {
                this.frequenciesService.crit.Included = null;
            } else {
                return;//just in case
            }
            this.crosstabService.Clear();
            let filterAttId: number | null = null;
            if (selectedFilter != null && selectedFilter.nodeType == "SetAttribute") {
                filterAttId = (selectedFilter as SetAttribute).attribute_id;
            }
            this.frequenciesService.Fetch(selectedNodeDataF, filterAttId);

        }
    }
    public FreqToExcel() {
        let res: any[] = [];
        //res.push(["Code", "CodeId", "Count"]);
        for (let row of this.frequenciesService.Frequencies) {
            let rrow = { Code: row.attribute, CodeId: row.attributeId, Count: row.itemCount };
            res.push(rrow);
        }
        this.ExcelService.exportAsExcelFile(res, 'FrequenciesReport');
    }

    public CrosstabToExcel() {
        if (this.crosstabService.xTab) {
            let res: any[] = [];
            
            for (let i = 0; i < this.crosstabService.xTab.rows.length; i++) {
                let rrow: any = { Codes: this.crosstabService.xTab.rows[i].attributeName };
                for (let ii = 0; ii < this.crosstabService.xTab.columnAttNames.length; ii++) {
                    rrow[this.crosstabService.xTab.columnAttNames[ii]] = this.crosstabService.xTab.rows[i].counts[ii];
                }
                res.push(rrow);
            }
            this.ExcelService.exportAsExcelFile(res, 'CrosstabReport');
        }
    }

    public Clear() {
        this.clearFilter();
        this.crosstbShowWhat = 'table';
        this.crosstbIncEx = 'bothIandE';
        this.selectedFilterAttribute = null;
        this.selectedNodeDataX = null;
        this.selectedNodeDataY = null;
        this.crosstabService.Clear();
        this.frequenciesService.Clear();
    }
    ngOnDestroy() {
     
    }

	
}






