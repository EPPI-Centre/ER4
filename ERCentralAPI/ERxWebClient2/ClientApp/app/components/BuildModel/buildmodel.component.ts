import { Component, Inject, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ClassifierService } from '../services/classifier.service';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { BuildModelService } from '../services/buildmodel.service';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { anyChanged } from '@progress/kendo-angular-grid/dist/es2015/utils';


@Component({
    selector: 'BuildModelComp',
    templateUrl: './buildmodel.component.html',
    providers: []
})

export class BuildModelComponent implements OnInit, OnDestroy {
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
		private _classifierService: ClassifierService,
		private _reviewSetsService: ReviewSetsService,
		public reviewSetsService: ReviewSetsService,
		public _buildModelService: BuildModelService
	) { }

	public selectedModelDropDown1: string = '';
	public selectedModelDropDown2: string = '';
	public modelName: string = '';
	public DD1: string = '0';
	public DD2: string = '0';

	public get DataSource(): GridDataResult {
		return {
			data: orderBy(this._buildModelService.ClassifierModelList, this.sort),
			total: this._buildModelService.ClassifierModelList.length,
		};
	}
	
	CanOnlySelectRoots() {

		return true;

	}
    removeHandler(event: any) {

        alert("Not implemented!");
    }
	public sort: SortDescriptor[] = [{
		field: 'modelId',
		dir: 'desc'
	}];

	public sortChange(sort: SortDescriptor[]): void {
		this.sort = sort;
		console.log('sorting?' + this.sort[0].field + " ");
	}
	BackToMain() {
		this.router.navigate(['Main']);
	}

	ngOnInit() {

		this.selectedModelDropDown1 = '';
		this.selectedModelDropDown2 = '';
		this.reviewSetsService.GetReviewSets();
		this._buildModelService.Fetch();

	}
	ngOnDestroy() {

		this._reviewSetsService.selectedNode = null;
	}
	IamVerySorryRefresh() {

		this._buildModelService.Fetch();

	}
	SetAttrOn(node: any) {


		if (node != null) {
			this.selectedModelDropDown1 = node.name;
			let id: string = node.id;
			let a: number = id.indexOf('A');
			if (a != -1) {
				let tmp: string = id.substr(a+1, id.length - a);
				this.DD1 = tmp;

				}
		}
		
	}
	SetAttrNotOn(node: any) {

		if (node != null) {
			//alert(node.name);
			this.selectedModelDropDown2 = node.name;
			let id: string = node.id;
			let a: number = id.indexOf('A');
			if (a != -1) {
				let tmp: string = id.substr(a+1, id.length - a);
				this.DD2 = tmp;
				//alert(tmp);
			}
		}
	}
	public isCollapsed: boolean = false;
	public isCollapsed2: boolean = false;
	CloseBMDropDown1() {

		this.isCollapsed = false;
	}
	CloseBMDropDown2() {

		this.isCollapsed2 = false;
	}
	async BuildModel(title: string) {

		if (this.DD1 != null && this.DD2 != null && this.modelName != '') {

			await this._classifierService.CreateAsync(title, this.DD1, this.DD2);
		}
		
	}


    ngAfterViewInit() {

	}


	 
}
