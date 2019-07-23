import { Inject, Injectable, EventEmitter, Output, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Item, TimePoint } from './ItemList.service';
import { iTimePoint } from '../timePoints/timePointsComp.component';
import { COMPOSITION_BUFFER_MODE } from '@angular/forms';

@Injectable({
    providedIn: 'root',
})

export class timePointsService extends BusyAwareService implements OnInit  {

    constructor(
		private _http: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
	ngOnInit() {

	}
	private _currentItem: Item = new Item();
    private _timepoints: iTimePoint[] | null = null;
	public get timepoints(): iTimePoint[] {
        if (this._timepoints) return this._timepoints;
        else {
            this._timepoints = [];
            return this._timepoints;
        }
    }
	public set timepoints(timepoints: iTimePoint[]) {
        this._timepoints = timepoints;
    }
   @Output() gotNewTimepoints = new EventEmitter();
   // @Output() timepointChangedEE = new EventEmitter();
	public get Selectedtimepoint(): iTimePoint | null {

        return this._selectedtimepoint;
    }
	private _selectedtimepoint: iTimePoint | null = null;

	public SetSelectedtimepoint(timepoint: TimePoint) {

		this._selectedtimepoint = timepoint;
		//if (this._selectedtimepoint != timepoint) {
			this.gotNewTimepoints.emit();
		//}

	}

	public IsServiceBusy(): boolean {

		if (this._BusyMethods.length > 0) {
			return true;
		} else {
			return false;
		}
	}

    public Fetchtimepoints(currentItem: Item) {

		this._BusyMethods.push("Fetchtimepoints");
		this._currentItem = currentItem;
        let body = JSON.stringify({ Value: currentItem.itemId });

		this._http.post<iTimePoint[]>(this._baseUrl + 'api/ItemtimepointList/GetTimePoints', body).subscribe(result => {
			this.timepoints = result;
			console.log('got inside the timepoints service: ' + this.timepoints.length);
				   currentItem.timepoints = this.timepoints;
				   this._selectedtimepoint = null;
				   this.gotNewTimepoints.emit(this.timepoints);
				   this.RemoveBusy("Fetchtimepoints");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("Fetchtimepoints");
			}
        );
	}


	public Createtimepoint(currenttimepoint: iTimePoint): Promise<iTimePoint> {

		this._BusyMethods.push("Createtimepoint");
		let ErrMsg = "Something went wrong when creating an timepoint. \r\n If the problem persists, please contact EPPISupport.";

		return this._http.post<iTimePoint>(this._baseUrl + 'api/ItemTimepointList/CreateTimePoint',

			currenttimepoint).toPromise()
						.then(
						(result) => {
	
							if (!result) this.modalService.GenericErrorMessage(ErrMsg);
							this.Fetchtimepoints(this._currentItem);
							this.RemoveBusy("Createtimepoint");
							return result;
						}
						, (error) => {
							this.Fetchtimepoints(this._currentItem);		
							this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("Createtimepoint");
							return error;
						}
						)
						.catch(
							(error) => {
								this.Fetchtimepoints(this._currentItem);		
								this.modalService.GenericErrorMessage(ErrMsg);
								this.RemoveBusy("Createtimepoint");
								return error;
						}
		);

	}


	public Updatetimepoint(currenttimepoint: iTimePoint) {

		this._BusyMethods.push("Updatetimepoint");
		let ErrMsg = "Something went wrong when updating an timepoint. \r\n If the problem persists, please contact EPPISupport.";

		this._http.post<iTimePoint[]>(this._baseUrl + 'api/ItemTimepointList/UpdateTimePoint',

			currenttimepoint).subscribe(

				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("Updatetimepoint");
					return result;
				}
				, (error) => {
					 this.Fetchtimepoints(this._currentItem);		
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("Updatetimepoint");
					return error;
				});
	}

	public DeleteWarningtimepoint(timepoint: iTimePoint) {
		this._BusyMethods.push("DeleteWarningtimepoint");
		let ErrMsg = "Something went wrong when warning of deleting a timepoint. \r\n If the problem persists, please contact EPPISupport.";
		let cmd: ItemTimepointDeleteWarningCommandJSON = new ItemTimepointDeleteWarningCommandJSON();
		cmd.itemTimepointId = timepoint.itemTimepointId;
		cmd.itemId = timepoint.itemId;

		return this._http.post<ItemTimepointDeleteWarningCommandJSON>(this._baseUrl + 'api/ItemTimepointList/DeleteWarningTimePoint',

			cmd).toPromise()
			.then(
				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteWarningtimepoint");
					return result;
				}
				, (error) => {

					
					console.log('error in DeleteWarningtimepoint() rejected', error);
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteWarningtimepoint");
					return cmd;
				}
			)
			.catch(
			(error) => {

					
					console.log('error in DeleteWarningtimepoint() catch', error);
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteWarningtimepoint");
					return cmd;
				}
			);

	}

	Deletetimepoint(timepoint: iTimePoint) {
		this._BusyMethods.push("Deletetimepoint");
			let ErrMsg = "Something went wrong when deleting an timepoint. \r\n If the problem persists, please contact EPPISupport.";
			
		this._http.post<iTimePoint>(this._baseUrl + 'api/ItemTimepointList/DeleteTimePoint',

			timepoint).subscribe(
				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("Deletetimepoint");
					return result;
				}
				, (error) => {

					this.Fetchtimepoints(this._currentItem);					
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("Deletetimepoint");
					return error;
				}
				);

	}

}

export class ItemTimepointDeleteWarningCommandJSON {

	numOutcomes: number = 0;
    itemId: number = 0;
	itemTimepointId: number = 0;
}


