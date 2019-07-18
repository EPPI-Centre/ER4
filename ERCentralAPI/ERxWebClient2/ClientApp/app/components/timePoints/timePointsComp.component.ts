import { Component, OnInit, Input } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import {  Item, TimePoint} from '../services/ItemList.service';
import { _localeFactory } from '@angular/core/src/application_module';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { timePointsService, ItemtimepointDeleteWarningCommandJSON } from '../services/timePoints.service';

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
	
	ngOnInit() {

		if (this.item) {
			this._timePointsService.Fetchtimepoints(this.item);
		}
	}

	public Units: any[] = [{
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
	public currentTimePoint: TimePoint = new TimePoint();
	//public currentTitle!: string;
	public currentKey: number = 0;
	public edit: boolean = false;
	public timepointModel: string = '';
	public unitModel: string = '';
	public timepointFreq: string = '';
	public selected: string = '';

	public get HasWriteRights(): boolean {
		return this.ReviewerIdentityServ.HasWriteRights;
	}
	setTimePoint(timepoint: iTimePoint, key: number) {

		this.currentKey = key;
		this.currentTimePoint = timepoint;
		//console.log(JSON.stringify(this.currentTimePoint));
		this.edit = true;
		this.unit = this.currentTimePoint.timepointMetric;
		this.unitModel = this.Units.filter(x => x.name == this.currentTimePoint.timepointMetric)[0];
		this.timepointModel = this.currentTimePoint.timepointMetric;
		this.timepointFreq = this.currentTimePoint.timepointValue;
		this.selected = 'selected';
	}

	//editField!: string;

	UpdateList() {

		this.currentTimePoint.timepointMetric = this.timepointModel;
		this.currentTimePoint.timepointValue = this.timepointFreq;
		this.edit = false;
		this.item!.timepoints[this.currentKey] = this.currentTimePoint;
		//console.log(JSON.stringify(this.currentTimePoint));
		this._timePointsService.Updatetimepoint(this.item!.timepoints[this.currentKey]);
		this.Clear();
	}


	Clear() {

		this.title = '';
		this.unit = '';
		this.timepointFreq = '';
		this.currentTimePoint = new TimePoint();
		this._timePointsService.SetSelectedtimepoint(new TimePoint());
		this.unitModel = '';
		this.edit = false;
	}

	public openConfirmationDialogDeletetimepoints(key: number) {

		this.confirmationDialogService.confirm('Please confirm',
			'Deleting an timepoint is a permanent operation and will delete'
			+'all outcome(s) associated with the timepoint.' +
			'<br />This timepoint is associated with 0 outcome.', false, '')
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

		this.confirmationDialogService.confirm('Please confirm',
			'Deleting an timepoint is a permanent operation and will '
			+ 'delete all outcome(s) associated with the timepoint.' +
			'<br /><b>This timepoint is associated with ' + numCodings + ' outcomes(s).</b>' +
			'<br />Please type \'I confirm\' in the box below if you are sure you want to proceed.',
			true, 'who knows here')
			.then(
				(confirm: any) => {

					if (confirm && this.eventsService.UserInput == 'I confirm') {

						this.ActuallyRemove(key);
					}
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}

	removeWarning(key: number) {

		// first call the dialog then call this part
		this._timePointsService.DeleteWarningtimepoint(this.timePointsList[key]).then(

			(res: ItemtimepointDeleteWarningCommandJSON) => {

				if (res == undefined || res.numOutcomes == 0  ) {

					this.openConfirmationDialogDeletetimepoints(key);

				} else if (res.numOutcomes == -1) {
					return;
				}
				else {

					this.openConfirmationDialogDeletetimepointsWithText(key, res.numOutcomes);
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
			if (SelectedId == ToRemove.itemTimepointId) this._timePointsService.SetSelectedtimepoint(new TimePoint());
		}
	}

	TimePointChanged(unit: any) {

		this.currentTimePoint.itemId = this.item != null? this.item.itemId: 0;
		this.currentTimePoint.timepointMetric = unit.name;
		this.currentTimePoint.timepointValue = this.timepointFreq;
		this._timePointsService.SetSelectedtimepoint(this.currentTimePoint);

	}

	add(timepointFreq: string) {

		if (timepointFreq != '' && this._timePointsService.Selectedtimepoint
			&& this.currentTimePoint.timepointMetric != '') {
			if (this.item != undefined) {

				let newtimepoint: TimePoint = new TimePoint();
				newtimepoint.itemId = this.item.itemId;
				newtimepoint.timepointValue = timepointFreq;
				if (this._timePointsService.Selectedtimepoint) {
					newtimepoint.timepointMetric = this.currentTimePoint.timepointMetric;
				}
				// check for the same timepoint
				if (this.timePointsList.filter( x=> x.timepointMetric == newtimepoint.timepointMetric)[0]
					&& this.timePointsList.filter(x => x.timepointValue == newtimepoint.timepointValue)[0])
				{
					// Will ask Sergio if he wants a notification here
					// or the alert errors panel to appear
					alert('The list already contains');
					return;
				}

				this._timePointsService.Createtimepoint(newtimepoint).then(
					(res: TimePoint) => {

						let key = this.timePointsList.length;
						this.timePointsList.splice(key, 0, res);
					}
				);
			}
			this.title = '';
		}
		this.Clear();
	}
}

export interface iTimePoint {

	itemId: number ;
	timepointValue: string ;
	timepointMetric: string ;
    itemTimepointId: number ;

}