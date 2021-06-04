import { Component,  OnInit,  OnDestroy, AfterViewInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { crosstabService, iWebDbItemAttributeCrosstabList, iWebDbItemAttributeCrosstabRow } from '../services/crosstab.service';
import { singleNode, ReviewSetsService, SetAttribute } from '../services/ReviewSets.service';
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
		private ItemListService: ItemListService,
		public reviewSetsService: ReviewSetsService,
		public crosstabService: crosstabService,
		private _eventEmitter: EventEmitterService

    ) { }

    ngOnInit() {


        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            //this._eventEmitter.dataStr.subscribe(

            //	(data: any) => {

            //		console.log('this is being emitted CT');
            //		this.selectedNodeData = data;
            //	}
            //)
        }
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


	public selectedNodeData: any | null = null;
    public get selectedNode(): singleNode | null {
        return this.reviewSetsService.selectedNode;
    }
    //public get CrossTab() {
    //    return this.crosstabService.CrossTab;
    //}
    public get xTab(): iWebDbItemAttributeCrosstabList | null {
        return this.crosstabService.xTab;
    }
	onSubmit(f: string) {

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



	NodeDataChange(nodeData: singleNode) {

		this.selectedNodeData = nodeData;
	}

	//CrossTabItemsList(item: CrossTab, attributeidY: number, attributenameY: string, field: string) {

 //       console.log("1: " + item + " 2: " + attributeidY + " 3: " + attributenameY + " 4: " + field);
	//	//this.crosstabService.CrossTab.attributeIdXAxis = item.xHeadersID[Number(field.substr(5)) - 1];
	//	//this.crosstabService.CrossTab.attributeIdYAxis = attributeid;

	//	//console.log('hello \n' + item.xHeadersID + '\n' + field + '\n'
	//	//	+ item.xHeadersID[Number(field.substr(5)) - 1] + '\n'
	//	//	+ Number(field.substr(field.length - 1, field.length)) + '\n'
	//	//	+ attributeid + '\n ' 
	//	//	+ ' BLAH :  ' + 
	//	//	' set x: ' + this.crosstabService.CrossTab.setIdXAxis + ' \n '
 //       //	 + ' set y: ' +  this.crosstabService.CrossTab.setIdYAxis + ' \n '
	//	//	+ ' att x: ' +  this.crosstabService.CrossTab.attributeIdXAxis + ' \n '
	//	//	+ ' att x: ' +  this.crosstabService.CrossTab.attributeIdYAxis
	//	//);

	//	let cr: Criteria = new Criteria();
	
	//	cr.xAxisSetId = this.crosstabService.CrossTab.setIdXAxis; 
 //       cr.yAxisSetId = this.crosstabService.CrossTab.setIdYAxis; 
 //       cr.xAxisAttributeId = item.xHeadersID[Number(field.substr(5)) - 1]; 
 //       cr.yAxisAttributeId = attributeidY ; 
 //       cr.onlyIncluded = true;

 //       if (this.crosstabService.crit.attributeIdFilter > 0) {
 //           cr.filterSetId = this.crosstabService.filterSetId;
 //           cr.filterAttributeId = this.crosstabService.crit.attributeIdFilter;
 //       }

	//	//cr.onlyIncluded = this.crosstabService.crit.onlyIncluded;
        
	//	cr.listType = 'CrosstabsList';
	//	//cr.attributeSetIdList = this.crosstabService.crit.attributeSetIdList;
 //       let descr: string = "Crosstab of '" + item.xHeaders[Number(field.substr(5)) - 1] + "' against '" + attributenameY + "'";
 //       if (cr.filterSetId > 0) {
 //           descr += ". Filtered by '" + this.crosstabService.filterName + "'";
 //       }
 //       this.ItemListService.FetchWithCrit(cr, descr);
 //       this._eventEmitter.PleaseSelectItemsListTab.emit();

	//}

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

            //if (selectedNodeDataX.nodeType == 'ReviewSet') {
            //	let test = JSON.stringify(selectedNodeDataX.attributes);
            //	console.log('testing here1: ' + test);
            //}
            //if (selectedNodeDataY.nodeType == 'ReviewSet') {
            //	let test2 = JSON.stringify(selectedNodeDataY.attributes);
            //	console.log('testing here2: ' + test2);
            //}		
            //console.log('hmmm');
            //console.log(this.selectedNodeDataX);
            //console.log(this.selectedNodeDataY);
            //console.log(this.selectedFilterAttribute);
            this.crosstabService.Fetch(this.selectedNodeDataX, this.selectedNodeDataY, this.selectedFilterAttribute, this.crosstbIncEx);

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
    }
    ngOnDestroy() {
     
    }

	
}






