import { Component, Inject, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ClassifierService } from '../services/classifier.service';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { BuildModelService } from '../services/buildmodel.service';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';

@Component({
    selector: 'BuildModelComp',
    templateUrl: './buildmodel.component.html',
    providers: []
})

export class BuildModelComponent implements OnInit, OnDestroy {
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
		private _classifierService: ClassifierService,
  //      private notificationService: NotificationService,
		private _reviewSetsService: ReviewSetsService,
		//private ReviewerIdentityServ: ReviewerIdentityService,
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

		// Load the nodes here
		this.reviewSetsService.GetReviewSets();
		this._buildModelService.Fetch();

	}
	ngOnDestroy() {


	}
	IamVerySorryRefresh() {

		this._buildModelService.Fetch();

	}
	SetAttrOn(node: any) {

		//alert(node.name);
		if (node != null) {
			this.selectedModelDropDown1 = node.name;
			let id: string = node.id;
			let a: number = id.indexOf('A');
			if (a != -1) {
				let tmp: string = id.substr(a+1, id.length - a);
				this.DD1 = tmp;
				//alert(tmp);
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

	BuildModel(title: string, attrOn: string, attrNotOn: string) {


		if (this.DD1 != null && this.DD2 != null && this.modelName != '') {

			this._classifierService.Create(title, this.DD1, this.DD2);
		}
	}

    ngAfterViewInit() {

    }
 
  
	 //   private _CurrentModelDateofSearch: Date | null = null;
		//public get CurrentModelDateofSearch(): Date | null{

	 //       if (true) {
	 //           return null;
	 //       }
	 //       else if (this._CurrentModelDateofSearch == null) {
	 //           try {
	 //               //this._CurrentModelDateofSearch = new Date(this._CurrentModel.dateOfSerach);
	 //           } catch { this._CurrentModelDateofSearch = null;}
	 //       }
	 //       return this._CurrentModelDateofSearch;
	 //   }



	 //   public set CurrentModelDateofSearch(newDate: Date | null) {
	 //       this._CurrentModelDateofSearch = newDate;
	 //   }

}
