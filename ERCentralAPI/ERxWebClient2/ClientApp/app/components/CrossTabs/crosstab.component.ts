import { Component,  OnInit,  OnDestroy, AfterViewInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { crosstabService, CrossTab, ReadOnlyItemAttributeCrosstab } from '../services/crosstab.service';
import { singleNode, ReviewSetsService } from '../services/ReviewSets.service';
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
        private reviewSetsService: ReviewSetsService,
		private crosstabService: crosstabService,
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
		this.crossTabResult = this.crosstabService.testResult;
	}
    
	public crossTabResult: any | null;
    public lookAtIncludeExclude: string = 'included';
    public selectedFilterAttribute: any | 'none';
    public selectedNodeDataX: any | 'none';
    public selectedNodeDataY: any | 'none';


	public selectedNodeData: any | null = null;

	onSubmit(f: string) {

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

    setXFilter() {
        if (
            this.reviewSetsService.selectedNode
            && this.reviewSetsService.selectedNode.nodeType == "SetAttribute"
        ) this.selectedNodeDataY = this.reviewSetsService.selectedNode;
    }
    clearXFilter() {
        this.selectedFilterAttribute = null;
    }
    fetchCrossTabs(selectedNodeDataX: any, selectedNodeDataY: any, selectedFilter: any) {

        if (!selectedNodeDataX || selectedNodeDataX == undefined || !selectedNodeDataY
            || selectedNodeDataY == undefined) {

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

            this.crossTabResult = this.crosstabService.Fetch(selectedNodeDataX, selectedNodeDataY, selectedFilter);

        }
    }

    ngOnDestroy() {
     
    }

	
}






