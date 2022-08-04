import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import {  Item } from '../services/ItemList.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ArmTimepointLinkListService, ItemTimepointDeleteWarningCommandJSON, iTimePoint, TimePoint } from '../services/ArmTimepointLinkList.service';
import { NgModel, NgForm } from '@angular/forms';
import { Helpers } from '../helpers/HelperMethods';

@Component({
	selector: 'timePointsComp',
	templateUrl: './timePoints.component.html',
    providers: []
})

export class timePointsComp  implements OnInit {

	constructor(
		private _timePointsService: ArmTimepointLinkListService,
		private confirmationDialogService: ConfirmationDialogService,
		private eventsService: EventEmitterService,
		private ReviewerIdentityServ: ReviewerIdentityService
	) {
		
	}

	@ViewChild('timePointModel') timePointModel!: NgModel;
	@ViewChild('timePointForm') timePointForm!: NgForm;

	public ShowTimePoints: boolean = true;

	public get ShowTimePointsBtnText(): string {
		if (this.ShowTimePoints) return "Collapse";
		else return "Expand";
	}

	public get timePointsList(): iTimePoint[] {

		if (!this.item || !this.item.timepoints) {
			//console.log('empty:!!! ');
			return [];
		}
		else {
			//console.log('timepoints: ' + this.item.timepoints.length);
			return this.item.timepoints;
		}
	}

	@Input() item!: Item | undefined;
	
	ngOnInit() {

		if (this.item) {
			console.log('got in here servuce time points');
			//this._timePointsService.Fetchtimepoints(this.item);
		}
	}

	public Units: any[] = ['seconds','minutes','hours','days','weeks','months','years'];	
	public SelectedUnit: string = '';
	public EditingTimepointId: number = 0;
	public TimePointValue: number = 0;

	public get HasWriteRights(): boolean {
		return this.ReviewerIdentityServ.HasWriteRights;
	}
	public get TimePointTheSame(): boolean {
		if (this.item == undefined || this.item.timepoints == undefined) return true;
		else if (this.item.timepoints.findIndex(f => f.timepointMetric == this.SelectedUnit && f.timepointValue == this.TimePointValue.toString()) > -1) return true;
		return false;
	}

	setTimePointForEdit(timepoint: iTimePoint) {
		let val = Helpers.SafeParseInt(timepoint.timepointValue.toString());
		//the toString() above is because timepointValue should be a number, as it arrives from the API as such, and thus, iTimePoint objects actually contain numbers :-(
		console.log("setTimePointForEdit", val);
		if (val != null) {
			this.EditingTimepointId = timepoint.itemTimepointId;
			this.SelectedUnit = timepoint.timepointMetric;
			this.TimePointValue = val;
        }
	}

	//editField!: string;

	


	Clear() {
		this.SelectedUnit = '';
		this.EditingTimepointId = 0;
		this.TimePointValue = 0;
		if (this.timePointForm) this.timePointForm.resetForm({ n1: this.TimePointValue });
	}

	public openConfirmationDialogDeletetimepoints(tp: iTimePoint) {

		this.confirmationDialogService.confirm('Please confirm',
			'Deleting an timepoint is a permanent operation and will delete'
			+'all outcome(s) associated with the timepoint.' +
			'<br />This timepoint is associated with 0 outcome.', false, '')
			.then(
				(confirmed: any) => {
					console.log('User confirmed:');
					if (confirmed) {

						this.ActuallyRemove(tp);

					} else {
						//alert('did not confirm');
					}
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}

	public openConfirmationDialogDeletetimepointsWithText(tp: iTimePoint, numCodings: number) {

		this.confirmationDialogService.confirm('Please confirm',
			'Deleting a timepoint is a permanent operation '
			+ 'which will remove the timepoint from all outcomes associated with it.' +
			'<br /><b>This timepoint is associated with ' + numCodings + ' outcomes(s).</b>' +
			'<br />Please type \'I confirm\' in the box below if you are sure you want to proceed.',
			true, 'i confirm')
			.then(
				(confirm: any) => {

                    if (confirm && this.eventsService.UserInput.toLowerCase().trim() == 'i confirm') {

						this.ActuallyRemove(tp);
					}
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}

	removeWarning(tp: iTimePoint) {

		// first call the dialog then call this part
		this._timePointsService.DeleteWarningtimepoint(tp).then(

			(res: ItemTimepointDeleteWarningCommandJSON) => {

				if (res == undefined || res.numOutcomes == 0  ) {

					this.openConfirmationDialogDeletetimepoints(tp);

				} else if (res.numOutcomes == -1) {
					return;
				}
				else {

					this.openConfirmationDialogDeletetimepointsWithText(tp, res.numOutcomes);
				}
			}
		);
		//this.timepointFreq = "";
		//this.unitModel = "";
		//this.edit = false;
	}

	ActuallyRemove(tp: iTimePoint) {
	
		let ToRemove = tp;
		if (ToRemove) {
			let SelectedId = this._timePointsService.Selectedtimepoint ? this._timePointsService.Selectedtimepoint.itemTimepointId : -1;
			this._timePointsService.Deletetimepoint(ToRemove);
			if (SelectedId == ToRemove.itemTimepointId) this._timePointsService.SetSelectedtimepoint(new TimePoint(0, '', '', 0));
		}
	}

	CreateTimepoint() {
		if (!this.HasWriteRights || this.TimePointTheSame || this.item == undefined || this.item.itemId < 1) return;
		else {
			let newtimepoint: TimePoint = new TimePoint(this.item.itemId, this.TimePointValue.toString(), this.SelectedUnit, 0);
			this._timePointsService.Createtimepoint(newtimepoint);
			this.Clear()
        }
	}
	UpdateTimepoint() {
		if (!this.HasWriteRights || this.TimePointTheSame
			|| this.item == undefined || this.item.itemId < 1
			|| this.item.timepoints.length == 0 || this.EditingTimepointId < 0) return;
		else {
			let tpi = this.item.timepoints.findIndex(f => f.itemTimepointId == this.EditingTimepointId);
			if (tpi == -1) return;
			let tp = this.item.timepoints[tpi];
			tp.timepointValue = this.TimePointValue.toString();
			tp.timepointMetric = this.SelectedUnit;
			this._timePointsService.Updatetimepoint(tp);
			this.Clear();
		}
	}
}

