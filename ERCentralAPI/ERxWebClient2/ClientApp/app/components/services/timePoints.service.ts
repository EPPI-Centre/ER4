import { Inject, Injectable, EventEmitter, Output, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Item } from './ItemList.service';
import { iTimePoint } from '../timePoints/timePointsComp.component';

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
    private _timepoints: iTimePoint[] | null = null;//null when service has been instantiated, empty array when the item in question had no timepoints.
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
    @Output() gottimepoints = new EventEmitter();
    @Output() timepointChangedEE = new EventEmitter();
	public get Selectedtimepoint(): iTimePoint | null {

        return this._selectedtimepoint;
    }
	private _selectedtimepoint: iTimePoint | null = null;

    public SetSelectedtimepoint(timepointID: number) {
		let Oldid = this._selectedtimepoint ? this._selectedtimepoint.itemTimepointId : 0;
        for (let timepoint of this.timepoints) {
            if (timepoint.itemTimepointId == timepointID) {
                this._selectedtimepoint = timepoint;
                if (Oldid !== timepointID) this.timepointChangedEE.emit();
                return;
            }
        }
        this._selectedtimepoint = null;
        if (Oldid !== timepointID) this.timepointChangedEE.emit();
    }
    public Fetchtimepoints(currentItem: Item) {

		this._BusyMethods.push("Fetchtimepoints");
		this._currentItem = currentItem;
        let body = JSON.stringify({ Value: currentItem.itemId });

		this._http.post<iTimePoint[]>(this._baseUrl + 'api/ItemtimepointList/Gettimepoints',

			   body).subscribe(result => {
				   this.timepoints = result;
				   currentItem.timepoints = this.timepoints;
				   this._selectedtimepoint = null;
				   this.gottimepoints.emit(this.timepoints);
				   this.RemoveBusy("Fetchtimepoints");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("Fetchtimepoints");
			}
			);
			return currentItem.timepoints;
	}

	public FetchPromisetimepoints(currentItem: Item): Promise<iTimePoint[]> {

		this._BusyMethods.push("FetchPromisetimepoints");
		this._currentItem = currentItem;
		let body = JSON.stringify({ Value: currentItem.itemId });

		return this._http.post<iTimePoint[]>(this._baseUrl + 'api/ItemtimepointList/Gettimepoints',

			body).toPromise().then(

				result => {
					this.timepoints = result;
					currentItem.timepoints = this.timepoints;
					this.RemoveBusy("FetchPromisetimepoints");
					return result;
					
			},

			(error) => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("Fetchtimepoints");
				return error;
			}
		);
	}

	public Createtimepoint(currenttimepoint: iTimePoint): Promise<iTimePoint> {

		this._BusyMethods.push("Createtimepoint");
		let ErrMsg = "Something went wrong when creating an timepoint. \r\n If the problem persists, please contact EPPISupport.";

		return this._http.post<iTimePoint>(this._baseUrl + 'api/ItemtimepointList/Createtimepoint',

			currenttimepoint).toPromise()
						.then(
						(result) => {
	
							if (!result) this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("Createtimepoint");
							return result;
						}
						, (error) => {
							this.timepoints = this.Fetchtimepoints(this._currentItem);		
							this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("Createtimepoint");
							return error;
						}
						)
						.catch(
							(error) => {
								this.timepoints = this.Fetchtimepoints(this._currentItem);		
								this.modalService.GenericErrorMessage(ErrMsg);
								this.RemoveBusy("Createtimepoint");
								return error;
							}
		);

	}


	public Updatetimepoint(currenttimepoint: iTimePoint) {

		this._BusyMethods.push("Updatetimepoint");
		let ErrMsg = "Something went wrong when updating an timepoint. \r\n If the problem persists, please contact EPPISupport.";

		this._http.post<iTimePoint[]>(this._baseUrl + 'api/ItemtimepointList/Updatetimepoint',

			currenttimepoint).subscribe(

				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("Updatetimepoint");
					return result;
				}
				, (error) => {
					this.timepoints = this.Fetchtimepoints(this._currentItem);		
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("Updatetimepoint");
					return error;
				});
	}

	public DeleteWarningtimepoint(timepoint: iTimePoint) {
		this._BusyMethods.push("DeleteWarningtimepoint");
		let ErrMsg = "Something went wrong when warning of deleting a timepoint. \r\n If the problem persists, please contact EPPISupport.";
		let cmd: ItemtimepointDeleteWarningCommandJSON = new ItemtimepointDeleteWarningCommandJSON();
		cmd.timepointId = timepoint.itemTimepointId;
		cmd.itemId = timepoint.itemId;

		return this._http.post<ItemtimepointDeleteWarningCommandJSON>(this._baseUrl + 'api/ItemtimepointList/DeleteWarningtimepoint',

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
			
		this._http.post<iTimePoint>(this._baseUrl + 'api/ItemtimepointList/Deletetimepoint',

			timepoint).subscribe(
				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("Deletetimepoint");
					return result;
				}
				, (error) => {

					this.timepoints = this.Fetchtimepoints(this._currentItem);					
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("Deletetimepoint");
					return error;
				}
				);

	}

}

export class ItemtimepointDeleteWarningCommandJSON {

	numCodings: number = 0;
    itemId: number = 0;
	timepointId: number = 0;
	numOutcomes: number = 0;
}


