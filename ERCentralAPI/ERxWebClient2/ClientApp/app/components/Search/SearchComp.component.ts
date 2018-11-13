import { Component, Inject, OnInit, EventEmitter, Output, Input, OnDestroy, AfterViewInit, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';
import { FormGroup, FormControl, FormBuilder, Validators, ReactiveFormsModule  } from '@angular/forms';
import { Subject } from 'rxjs/index';
import { debounceTime, startWith, merge, switchMap, share  } from 'rxjs/operators/index';
import { pipe } from 'rxjs'
import { style } from '@angular/animations';
import { searchService, Search } from '../services/search.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MatInputModule, MatTableModule, MatToolbarModule, MatTableDataSource } from '@angular/material';
import {  ViewChild } from '@angular/core';
import { MatSort } from '@angular/material';
import { DataSource } from '@angular/cdk/table';
import { SelectionModel } from '@angular/cdk/collections';

@Component({
	selector: 'SearchComp',
	templateUrl: './SearchComp.component.html',
	providers: [],
	encapsulation: ViewEncapsulation.None
})

export class SearchComp implements OnInit, OnDestroy {
	displayedColumns = ['selected', 'searchId', 'title', 'contactName', 'searchDate', 'hitsNo'];
	constructor(private router: Router,
		private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService,
		private _searchService: searchService,
		private _eventEmitter: EventEmitterService
	) { }

    onSubmit(f: string) {

    }
		
    @Input() Context: string | undefined;

	public selectedAll: boolean = false;
	dataSource: any | undefined;
	//displayedColumns: string [] | undefined;

	@ViewChild(MatSort) sort!: MatSort;

    value = 1;
    onEnter(value: number) {
        this.value = value ;
        this.ItemListService.FetchParticularPage(value-1);
    }

	dataTable: any;


	OpenItems(item: number) {

		let cr: Criteria = new Criteria();

		//cr.setId = item.setId;
		//cr.attributeid = item.attributeId;
		//// the below should get its value from the view radio buttons
		//cr.onlyIncluded = item.isIncluded;
		cr.filterAttributeId = -1;
		cr.searchId = item;
		cr.listType = 'GetItemSearchList';
		//cr.attributeSetIdList = item.attributeSetId;
		console.log('searchid is: ' + item);
		this.ItemListService.FetchWithCrit(cr, "GetItemSearchList");
		this._eventEmitter.selectTabItems();
	}

	ngOnDestroy() {

	}


	public tableArr: Search[] = [];
	public testArr: Search[] = [];
	public selectAll: boolean =false;

	public initialSelection = [];
	public allowMultiSelect = true;
	public selection = new SelectionModel<Search>(this.allowMultiSelect, this.initialSelection);

	public columnNames = [
	{
		id: "selected",
		value: "Selected"
	},
	{
		id: "searchId",
		value: "SearchId"
	},
	{
		id: "title",
		value: "Title"
	},
	{
		id: "contactName",
		value: "ContactName"
	},
	{
		id: "searchDate",
		value: "SearchDate"
	},
	{
		id: "hitsNo",
		value: "HitsNo"

	}];

	testAlert() {

		alert('Not implemented');
	}

	updateCheck() {

		

		if (this.selectAll === true) {
			this.selectAll = false;
			this.tableArr.map((r) => {
				r.selected = false;
			});

		} else {
			this.selectAll = true;
			this.tableArr.map((r) => {
				r.selected = true;
			});
		}
	}

	createTable() {

		this.dataSource = new MatTableDataSource(this.tableArr);
		this.dataSource.sort = this.sort;
	}


	ngOnInit() {
				
		if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
			this.router.navigate(['home']);
		}
		else {


			 this._searchService.Fetch().toPromise().then(
				 
				 (res) => {

					 this.displayedColumns = this.columnNames.map(
						 (x) => {

							 if (x != undefined) {
								 return x.id;
							 } else {
								 return '';
							 }
						 }
					 );

					 this.testArr = res;
					 this.tableArr = res;

					 for (var i = 0; i < res.length; i++) {

						 this.tableArr[i].contactName = this.testArr[i].contactName;
						 this.tableArr[i].hitsNo = this.testArr[i].hitsNo;
						 this.tableArr[i].title = this.testArr[i].title;
						 this.tableArr[i].searchDate = this.testArr[i].searchDate;
						 this.tableArr[i].searchId = this.testArr[i].searchId;
						 this.tableArr[i].selected = false;

						// console.log(this.tableArr[i]);

					 }

					 this.tableArr = res;
					 this.createTable();
				 }

			);


				//.map(

				//	(res) => {


				//		this.displayedColumns = this.columnNames.map(

				//			(x) => {

				//				if (x != undefined) {
				//					return x.id;
				//				} else {
				//					return '';
				//				}
				//			}
				//		);

				//		this.tableArr.forEach(y => y = res);
				//		console.log('testing here: ' + res);
				//		this.createTable();
				//	}

				//);

		}
	}

}




