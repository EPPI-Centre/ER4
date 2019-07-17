import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationListService } from '../services/WorkAllocationList.service';
import {  Item, TimePoint} from '../services/ItemList.service';
import { _localeFactory } from '@angular/core/src/application_module';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { timePointsService, ItemtimepointDeleteWarningCommandJSON } from '../services/timePoints.service';
import { currencyDisplay } from '@telerik/kendo-intl';


@Component({
	selector: 'timePointsComp',
	templateUrl: './timePoints.component.html',
    providers: []
})

export class timePointsComp implements OnInit {

	constructor(
		private _timePointsService: timePointsService,
		private confirmationDialogService: ConfirmationDialogService,
		private eventsService: EventEmitterService,
		private ReviewerIdentityServ: ReviewerIdentityService
	) { }

	public get timePointsList(): iTimePoint[] {

		if (!this.item || !this.item.timepoints) return [];
		else return this.item.timepoints;
	}

	public title: string = '';
	@Input() item!: Item | undefined;

	//@ViewChild("editTitle", { read: ElementRef }) tref!: ElementRef;

	ngOnInit() {

		if (this.item) {
			this._timePointsService.Fetchtimepoints(this.item);
		}
	}

	public Units: any = [{
		id: '8f8c6e98',
		name: 'seconds'
	},
	{
		id: '169f8e1a',
		name: 'minutes'
	},
	{
		id: '169f9e1a',
		name: 'hours'
	},
	{
		id: '169fe001a',
		name: 'days'
	}
		,
	{
		id: '3466fdghfgh',
		name: 'weeks'
	}
		,
	{
		id: '8756sdfg',
		name: 'months'
	}
		,
	{
		id: '7564fdgh',
		name: 'years'
	}];
	
	public unit: string = '';
	swap: boolean = false;
	public currentTimePoint!: iTimePoint;
	public currentTitle!: string;
	public currentKey: number = 0;
	public editTitle: boolean = false;
	public titleModel: string = '';
	public timepointFreq: string = '';

	public get HasWriteRights(): boolean {
		return this.ReviewerIdentityServ.HasWriteRights;
	}
	setTimePoint(timepoint: iTimePoint, key: number) {

		this.currentKey = key;
		this.currentTimePoint = timepoint;
	}

	editField!: string;

	updateList(timepoint: iTimePoint) {

		this.editTitle = false;
		this.item!.timepoints[this.currentKey] = timepoint;
		this._timePointsService.Updatetimepoint(this.item!.timepoints[this.currentKey]);
		this.ClearAndCancelEdit();
	}


	ClearAndCancelEdit() {

		this.editTitle = false;
		this.currentTitle = '';

	}

	ClearAndCancelAdd() {

		this.title = '';

	}

	public openConfirmationDialogDeletetimepoints(key: number) {
		this.confirmationDialogService.confirm('Please confirm', 'Deleting an timepoint is a permanent operation and will delete all coding associated with the timepoint.' +
			'<br />This timepoint is associated with 0 codes.', false, '')
			.then(
				(confirmed: any) => {
					console.log('User confirmed:');
					if (confirmed) {

						this.ActuallyRemove(key);

					} else {
						//alert('did not confirm');
					}
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}

	public openConfirmationDialogDeletetimepointsWithText(key: number, numCodings: number) {

		this.confirmationDialogService.confirm('Please confirm', 'Deleting an timepoint is a permanent operation and will delete all coding associated with the timepoint.' +
			'<br /><b>This timepoint is associated with ' + numCodings + ' codes.</b>' +
			'<br />Please type \'I confirm\' in the box below if you are sure you want to proceed.', true, 'who knows here')
			.then(
				(confirm: any) => {

					//console.log('Text entered is the following: ' + confirm + ' ' + this.eventsService.UserInput );

					if (confirm && this.eventsService.UserInput == 'I confirm') {

						this.ActuallyRemove(key);

					} else {

					}
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}

	removeWarning(key: number) {


		// first call the dialog then call this part
		this._timePointsService.DeleteWarningtimepoint(this.timePointsList[key]).then(

			(res: ItemtimepointDeleteWarningCommandJSON) => {

				if (res.numCodings == 0) {

					this.openConfirmationDialogDeletetimepoints(key);


				} else if (res.numCodings == -1) {
					return;
				}
				else {

					this.openConfirmationDialogDeletetimepointsWithText(key, res.numCodings);
				}

			}
		);
	}

	ActuallyRemove(key: number) {
		let ToRemove = this.timePointsList[key];
		if (ToRemove) {
			let SelectedId = this._timePointsService.Selectedtimepoint ? this._timePointsService.Selectedtimepoint.itemTimepointId : -1;
			this._timePointsService.Deletetimepoint(ToRemove);
			this.timePointsList.splice(key, 1);
			if (SelectedId == ToRemove.itemTimepointId) this._timePointsService.SetSelectedtimepoint(0);
		}
	}

	TimePointChanged(timepointMetric: string) {
		console.log("timepointId changed: ..." + timepointMetric);
		let currentTimePoint: TimePoint = new TimePoint();
		currentTimePoint.itemId = this.item != null? this.item.itemId: 0;
		currentTimePoint.timepointMetric = timepointMetric;
		currentTimePoint.timepointValue = this.timepointFreq;
		this._timePointsService.SetSelectedtimepoint(currentTimePoint);

	}

	add(timepointFreq: string) {

		if (timepointFreq != '') {
			if (this.item != undefined) {

				let newtimepoint: TimePoint = new TimePoint();
				newtimepoint.itemId = this.item.itemId;
				newtimepoint.timepointValue = timepointFreq;
				newtimepoint.timepointMetric = this.unit;
				this._timePointsService.Createtimepoint(newtimepoint).then(
					(res: TimePoint) => {

						let key = this.timePointsList.length;
						this.timePointsList.splice(key, 0, res);
					}
				);
			}
			this.title = '';
		}
		this.ClearAndCancelAdd();
	}

}



export interface numCodings {

	numCodings: number;
}

export interface iTimePoint {

	itemId: number ;
	timepointValue: string ;
	timepointMetric: string ;
    itemTimepointId: number ;

}