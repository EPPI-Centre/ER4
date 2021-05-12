import { Component, Inject, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ClassifierService } from '../services/classifier.service';
import { ReviewSetsService, singleNode, SetAttribute } from '../services/ReviewSets.service';
//import { BuildModelService, MVCClassifierCommand } from '../services/buildmodel.service';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { NotificationService } from '@progress/kendo-angular-notification';


@Component({
    selector: 'BuildModelComp',
    templateUrl: './buildmodel.component.html',
    providers: []
})

export class BuildModelComponent implements OnInit, OnDestroy {
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
		private _classifierService: ClassifierService,
		public _reviewSetsService: ReviewSetsService,
		//private _buildModelService: BuildModelService,
		private _eventEmitterService: EventEmitterService,
		private _confirmationDialogService: ConfirmationDialogService,
		private _ReviewerIdentityServ: ReviewerIdentityService,
		private _notificationService: NotificationService
	) { }

	public selectedModelDropDown1: string = '';
	public selectedModelDropDown2: string = '';
	public modelNameText: string = '';
	public DD1: number = 0;
	public DD2: number = 0;
	public get DataSource(): GridDataResult {
		return {
			data: orderBy(this._classifierService.ClassifierModelList, this.sort),
			total: this._classifierService.ClassifierModelList.length,
		};
    }
    public get selectedNode(): singleNode | null {
        return this._reviewSetsService.selectedNode;
    }
    public get nodeSelected(): singleNode | null | undefined {
        return this._eventEmitterService.nodeSelected;//SG note: not sure this is a good idea, how is this better than this.reviewSetsService.selectedNode?
    }
	CanOnlySelectRoots() {

		return true;

	}
	public get HasWriteRights(): boolean {
		return this._ReviewerIdentityServ.HasWriteRights;
	}
	CanBuildModel() {

		if (this.selectedModelDropDown1 && this.selectedModelDropDown2 && this.modelNameText != ''
			&& (this.selectedModelDropDown1 != this.selectedModelDropDown2) ) {
			return true;
		}
		return false;
	}
	private canDelete: boolean = false;
	public CanDeleteModel(): boolean {

		for (var i = 0; i < this.DataSource.data.length; i++) {
			if (this.DataSource.data[i].add == true) {
				return true;
			}
		}
		return false;
	}
    removeHandler(event: any) {

        alert("Not implemented!");
    }
	public sort: SortDescriptor[] = [{
		field: 'modelId',
		dir: 'desc'
	}];
	public async openConfirmationDialogDeleteModels() {

		let counter: number = 0;
		for (var i = 0; i < this.DataSource.data.length; i++) {
			if (this.DataSource.data[i].add == true) {
				counter += 1;
			}
		}
		this._confirmationDialogService.confirm('Please confirm', 'Are you sure you want to ' +
		 'delete the ' + counter + ' selected model(s) ? ', false, '')
			.then(
				(confirmed: any) => {
					console.log('User confirmed:', confirmed);
					if (confirmed) {
						this.DeleteModelSelected().then(
                            (res) => {
                                if (res == true) {
                                    this._notificationService.show({
                                        content: this.modelsToBeDeleted.length + " models have been deleted",
                                        animation: { type: 'slide', duration: 400 },
                                        position: { horizontal: 'center', vertical: 'top' },
                                        type: { style: "info", icon: true },
                                        closable: true
                                    });
                                }
                                this.modelsToBeDeleted = [];
								this._classifierService.Fetch();//we refresh data in all branches, as it's not costly and we like getting a reliable list from the server side.
                                this.Clear();
							},
                            (error) => {
                                this.modelsToBeDeleted = [];
								this._classifierService.Fetch();
                                console.log("Error deleting models (controller side)", error);
                                this.Clear();
                            }
                        ).catch(
                            (caught) => {
                                this.modelsToBeDeleted = [];
								this._classifierService.Fetch();
                                console.log("Error deleting models (controller side, catch)", caught);
                                this.Clear();
                            }
                        );
					} 
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}
	public modelsToBeDeleted: number[] = [];
	async DeleteModelSelected(): Promise<boolean> {

		let lstStrModelIds = '';
        let res: boolean = false;
		let modelID: number = 0;
		console.log(this.DataSource);
		//alert('number in the list is: ' + this.DataSource.data.length)
		
		for (var i = 0; i < this.DataSource.data.length; i++) {

			if (this.DataSource.data[i].add != undefined && this.DataSource.data[i].add == true) {

				this.modelsToBeDeleted.push(this.DataSource.data[i].modelId);
			}
		}
		for (var j = 0; j < this.modelsToBeDeleted.length; j++) {
            //this.canDelete = true;
            //lstStrModelIds += this.DataSource.data[j].modelId;
            modelID = this.modelsToBeDeleted[j];
            //console.log('trying to delete this model: ' + modelID);
			res = await this._classifierService.Delete(modelID);
            if (res == null || res == undefined || res == false) {
                //an error happened. Let's stop here.
                res = false;
                break;
            }
            //else {
            //    let tmpIndex: any = this._buildModelService.ClassifierModelList.findIndex(x => x.modelId == modelID);
            //    this._buildModelService.ClassifierModelList.splice(tmpIndex, 1);
            //}
		}
		return res;
	}
	public checkBoxSelected: boolean = false;
	public checkboxClicked(dataItem: any) {

		if (dataItem.add == undefined || dataItem.add == null) {
			dataItem.add = true;
		} else {
			dataItem.add = !dataItem.add;
		}
		//console.log('trying to delete=' + dataItem.add + ' this data item(model): ' + dataItem.modelId);
		//if (dataItem.add == true) {
		//	this._buildModelService.modelToBeDeleted = dataItem.modelId;
		//}
		if (dataItem.add == true) {
			this.checkBoxSelected = true;
			//this.canDelete = true;
		} else {
			//this.canDelete = false;
		}
		//
	};
	public allModelsSelected: boolean = false;
	public selectAllModelsChange() {

		if (this.allModelsSelected == true) {
			for (var i = 0; i < this.DataSource.data.length; i++) {
				this.DataSource.data[i].add = false;
			}
			this.allModelsSelected = false;
			return;
		} else {
			for (var i = 0; i < this.DataSource.data.length; i++) {
				this.DataSource.data[i].add = true;
			}
		}
		this.allModelsSelected = true;

	}
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
		if (this._reviewSetsService.ReviewSets.length == 0) this._reviewSetsService.GetReviewSets(false);
		this._classifierService.Fetch();
	}
	ngOnDestroy() {

		this._reviewSetsService.selectedNode = null;
	}
	IamVerySorryRefresh() {

		this._classifierService.Fetch();

	}
	SetAttrOn(node: singleNode | null | undefined) {
		//alert(JSON.stringify(node));
		if (node != null && node.nodeType == "SetAttribute") {
			let a = node as SetAttribute;
			this.selectedModelDropDown1 = node.name;
			this.DD1 = a.attribute_id;
		}
		
	}
	SetAttrNotOn(node: singleNode | null | undefined) {
		//alert(JSON.stringify(node));
		if (node != null && node != undefined && node.nodeType == "SetAttribute") {
			let a = node as SetAttribute;
			this.selectedModelDropDown2 = node.name;
			this.DD2 = a.attribute_id;
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
	async BuildModel(title: any) {

		if (this.DD1 != null && this.DD2 != null && this.modelNameText != '') {

			await this._classifierService.CreateAsync(title.model, this.DD1, this.DD2, -1);
		}
		
	}
    ngAfterViewInit() {

	}
	Clear() {
		
		this.selectedModelDropDown1 = '';
		this.selectedModelDropDown2 = '';
		this.modelNameText = '';
		this.DD1 = 0;
		this.DD2 = 0;

	}

	 
}
