import { Inject, Injectable, EventEmitter, Output, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Item } from './ItemList.service';
import { COMPOSITION_BUFFER_MODE } from '@angular/forms';

@Injectable({
    providedIn: 'root',
})

export class ArmTimepointLinkListService extends BusyAwareService implements OnInit  {

    constructor(
		private _http: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
	ngOnInit() {

	}
	@Output() armChangedEE = new EventEmitter();
	private _currentItem: Item = new Item();
	private _timepoints: iTimePoint[] | null = null;
	private _selectedtimepoint: iTimePoint | null = null;
	private _links: iItemLink[] | null = null;
	private _selectedLink: iItemLink | null = null;
	private _arms: iArm[] | null = null;//null when service has been instantiated, empty array when the item in question had no arms.
	private _selectedArm: iArm | null = null;
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
	public get arms(): iArm[] {
		if (this._arms) return this._arms;
		else {
			this._arms = [];
			return this._arms;
		}
	}
	public set arms(arms: iArm[]) {
		this._arms = arms;
	}
	public get links(): iItemLink[] {
		if (this._links) return this._links;
		else {
			this._links = [];
			return this._links;
		}
	}
	public set links(links: iItemLink[]) {
		this._links = links;
	}

   //@Output() gotNewTimepoints = new EventEmitter();
   // @Output() timepointChangedEE = new EventEmitter();
	public get Selectedtimepoint(): iTimePoint | null {
        return this._selectedtimepoint;
    }

	public SetSelectedtimepoint(timepoint: TimePoint) {
		this._selectedtimepoint = timepoint;
	}
	public get SelectedLink(): iItemLink | null {
		return this._selectedLink;
	}

	public SetSelectedLink(link: iItemLink) {
		this._selectedLink = link;
	}
	public get SelectedArm(): iArm | null {
		return this._selectedArm;
	}

	public SetSelectedArm(armID: number) {
		let Oldid = this._selectedArm ? this._selectedArm.itemArmId : 0;
		for (let arm of this.arms) {
			if (arm.itemArmId == armID) {
				this._selectedArm = arm;
				if (Oldid !== armID) this.armChangedEE.emit();
				return;
			}
		}
		this._selectedArm = null;
		if (Oldid !== armID) this.armChangedEE.emit();
	}

	public FetchAll(currentItem: Item) {

		this._BusyMethods.push("FetchAll");
		this._currentItem = currentItem;
		let body = JSON.stringify({ Value: currentItem.itemId });

		this._http.post<iItemArmTimepointLinkLists>(this._baseUrl + 'api/ArmTimepointLinkList/GetArmTimepointLinkLists',

			body).subscribe(result => {
				this.arms = result.arms;
				currentItem.arms = this.arms;
				this._selectedArm = null;
				this.timepoints = result.timePoints;
				currentItem.timepoints = this.timepoints;
				this._selectedtimepoint = null;
				this._links = result.links;
				this._selectedLink = null;
				//this.gotArms.emit(this.arms);
				this.RemoveBusy("FetchAll");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
					this.RemoveBusy("FetchAll");
			}
			);
	}


	public GetItemLinks(Id: number): Promise<iItemLink[] | boolean> {
		this._BusyMethods.push("GetLinkLists");
		let ErrMsg = "Something went wrong when getting the Links. \r\n If the problem persists, please contact EPPISupport.";
		let body = JSON.stringify({ Value: Id });

		return this._http.post<iItemLink[]>(this._baseUrl + 'api/ArmTimepointLinkList/GetLinkLists',
			body).toPromise()
			.then(
				(result) => {
					//if (!result || result.length < 1) this.modalService.GenericErrorMessage(ErrMsg);
					// a false result just means there aren't any links (and we want to know that)
					this.RemoveBusy("GetLinkLists");
					return result;
				}
				, (error) => {
					this.modalService.GenericError(error);
					this.RemoveBusy("GetLinkLists");
					return false;
				}
			).catch((caught) => {
				this.modalService.GenericError(caught);
				this.RemoveBusy("GetLinkLists");
				return false;
			});
	}

	public async GetLinksForThisItem(Id: number): Promise<iItemLink[] | boolean>  {
		let res = await this.GetItemLinks(Id);
		if (res != false) {
			let res = await this.GetItemLinks(Id);
		}
		return res;
	}


    public Fetchtimepoints(currentItem: Item) {

		this._BusyMethods.push("Fetchtimepoints");
		this._currentItem = currentItem;
        let body = JSON.stringify({ Value: currentItem.itemId });

		this._http.post<iTimePoint[]>(this._baseUrl + 'api/ArmTimepointLinkList/GetTimePoints', body).subscribe(result => {
			this.timepoints = result;
			//console.log('got inside the timepoints service: ' + this.timepoints.length);
				   currentItem.timepoints = this.timepoints;
				   this._selectedtimepoint = null;
				   //this.gotNewTimepoints.emit(this.timepoints);
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

		return this._http.post<iTimePoint[]>(this._baseUrl + 'api/ArmTimepointLinkList/CreateTimePoint',

			currenttimepoint).toPromise()
						.then(
							(result) => {
								this.timepoints = result;
								//console.log('got inside the timepoints service: ' + this.timepoints.length);
								this._currentItem.timepoints = this.timepoints;
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

		this._http.post<iTimePoint>(this._baseUrl + 'api/ArmTimepointLinkList/UpdateTimePoint',

			currenttimepoint).subscribe(

				(result) => {
					let tpi = this._currentItem.timepoints.findIndex(f => f.itemTimepointId == currenttimepoint.itemTimepointId);
					if (tpi > -1) {
						this._currentItem.timepoints[tpi].timepointMetric = result.timepointMetric;
						this._currentItem.timepoints[tpi].timepointValue = result.timepointValue;
                    }
					this.RemoveBusy("Updatetimepoint");
				}
				, (error) => {
					this.Fetchtimepoints(this._currentItem);		
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("Updatetimepoint");
				});
	}

	public DeleteWarningtimepoint(timepoint: iTimePoint) {
		this._BusyMethods.push("DeleteWarningtimepoint");
		let ErrMsg = "Something went wrong when warning of deleting a timepoint. \r\n If the problem persists, please contact EPPISupport.";
		let cmd: ItemTimepointDeleteWarningCommandJSON = new ItemTimepointDeleteWarningCommandJSON();
		cmd.itemTimepointId = timepoint.itemTimepointId;
		cmd.itemId = timepoint.itemId;

		return this._http.post<ItemTimepointDeleteWarningCommandJSON>(this._baseUrl + 'api/ArmTimepointLinkList/DeleteWarningTimePoint',

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

	public Deletetimepoint(timepoint: iTimePoint) {
		this._BusyMethods.push("Deletetimepoint");
			let ErrMsg = "Something went wrong when deleting an timepoint. \r\n If the problem persists, please contact EPPISupport.";
			
		this._http.post<iTimePoint[]>(this._baseUrl + 'api/ArmTimepointLinkList/DeleteTimePoint',

			timepoint).subscribe(
				(result) => {
					this.timepoints = result;
					this._currentItem.timepoints = result;
					this._selectedtimepoint = null;
					this.RemoveBusy("Deletetimepoint");
				}
				, (error) => {
					this.Fetchtimepoints(this._currentItem);					
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("Deletetimepoint");
				}
				);

	}

	public FetchArms(currentItem: Item) {

		this._BusyMethods.push("FetchArms");
		this._currentItem = currentItem;
		let body = JSON.stringify({ Value: currentItem.itemId });

		this._http.post<iArm[]>(this._baseUrl + 'api/ArmTimepointLinkList/GetArms',

			body).subscribe(result => {
				this.arms = result;
				currentItem.arms = this.arms;
				this._selectedArm = null;
				//this.gotArms.emit(this.arms);
				this.RemoveBusy("FetchArms");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchArms");
			}
			);
		return currentItem.arms;
	}

	public FetchPromiseArms(currentItem: Item): Promise<iArm[]> {

		this._BusyMethods.push("FetchPromiseArms");
		this._currentItem = currentItem;
		let body = JSON.stringify({ Value: currentItem.itemId });

		return this._http.post<iArm[]>(this._baseUrl + 'api/ArmTimepointLinkList/GetArms',

			body).toPromise().then(

				result => {
					this.arms = result;
					currentItem.arms = this.arms;
					this.RemoveBusy("FetchPromiseArms");
					return result;

				},

				(error) => {
					this.modalService.SendBackHomeWithError(error);
					this.RemoveBusy("FetchArms");
					return error;
				}
			);
	}

	public CreateArm(currentArm: Arm): Promise<Arm> {

		this._BusyMethods.push("CreateArm");
		let ErrMsg = "Something went wrong when creating an arm. \r\n If the problem persists, please contact EPPISupport.";

		return this._http.post<Arm>(this._baseUrl + 'api/ArmTimepointLinkList/CreateArm',

			currentArm).toPromise()
			.then(
				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("CreateArm");
					return result;
				}
				, (error) => {
					this.FetchArms(this._currentItem);
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("CreateArm");
					return error;
				}
			)
			.catch(
				(error) => {
					this.FetchArms(this._currentItem);
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("CreateArm");
					return error;
				}
			);

	}

	public UpdateArm(currentArm: iArm) {

		this._BusyMethods.push("UpdateArm");
		let ErrMsg = "Something went wrong when updating an arm. \r\n If the problem persists, please contact EPPISupport.";

		this._http.post<iArm[]>(this._baseUrl + 'api/ArmTimepointLinkList/UpdateArm',

			currentArm).subscribe(

				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("UpdateArm");
					return result;
				}
				, (error) => {
					this.FetchArms(this._currentItem);
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("UpdateArm");
					return error;
				});
	}

	public DeleteWarningArm(arm: iArm) {
		this._BusyMethods.push("DeleteWarningArm");
		let ErrMsg = "Something went wrong when warning of deleting an arm. \r\n If the problem persists, please contact EPPISupport.";
		let cmd: ItemArmDeleteWarningCommandJSON = new ItemArmDeleteWarningCommandJSON();
		cmd.itemArmId = arm.itemArmId;
		cmd.itemId = arm.itemId;

		return this._http.post<ItemArmDeleteWarningCommandJSON>(this._baseUrl + 'api/ArmTimepointLinkList/DeleteWarningArm',

			cmd).toPromise()
			.then(
				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					// Logic here to show various error messages...
					this.RemoveBusy("DeleteWarningArm");
					return result;
				}
				, (error) => {

					cmd.numCodings = -1;
					console.log('error in DeleteWarningArm() rejected', error);
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteWarningArm");
					return cmd;
				}
			)
			.catch(
				(error) => {

					cmd.numCodings = -1;
					console.log('error in DeleteWarningArm() catch', error);
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteWarningArm");
					return cmd;
				}
			);

	}

	public DeleteArm(arm: iArm) {
		this._BusyMethods.push("DeleteArm");
		let ErrMsg = "Something went wrong when deleting an arm. \r\n If the problem persists, please contact EPPISupport.";

		this._http.post<iArm>(this._baseUrl + 'api/ArmTimepointLinkList/DeleteArm',

			arm).subscribe(
				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteArm");
					return result;
				}
				, (error) => {

					this.FetchArms(this._currentItem);
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteArm");
					return error;
				}
			);

	}

	public CreateItemLink(link: iItemLink): Promise<boolean> {

		this._BusyMethods.push("CreateItemLink");
		let ErrMsg = "Something went wrong when Creating the Link. \r\n If the problem persists, please contact EPPISupport.";

		return this._http.post<iItemLink[]>(this._baseUrl + 'api/ArmTimepointLinkList/CreateItemLink',

			link).toPromise().then(
				(result) => {

					if (!result || result.length < 1) this.modalService.GenericErrorMessage(ErrMsg);
					else this.links = result;
					this.RemoveBusy("CreateItemLink");
					return true;
				}
				, (error) => {
					this.FetchAll(this._currentItem);
					this.modalService.GenericError(error);
					this.RemoveBusy("CreateItemLink");
					return false;
				}
		).catch((caught) => {
			this.FetchAll(this._currentItem);
			this.modalService.GenericError(caught);
			this.RemoveBusy("CreateItemLink");
			return false;
		});

	}

	public UpdateItemLink(link: iItemLink): Promise<boolean> {

		this._BusyMethods.push("UpdateItemLink");
		let ErrMsg = "Something went wrong when updating the link. \r\n If the problem persists, please contact EPPISupport.";

		return this._http.post<iItemLink[]>(this._baseUrl + 'api/ArmTimepointLinkList/UpdateItemLink',

			link).toPromise().then(
				(result) => {
					if (!result || result.length < 1) this.modalService.GenericErrorMessage(ErrMsg);
					else this.links = result;
					this.RemoveBusy("UpdateItemLink");
					return true
				}
				, (error) => {
					this.FetchAll(this._currentItem);
					this.modalService.GenericError(error);
					this.RemoveBusy("UpdateItemLink");
					return false;
				}).catch((caught) => {
					this.FetchAll(this._currentItem);
					this.modalService.GenericError(caught);
					this.RemoveBusy("UpdateItemLink");
					return false;
				});
	}

	public DeleteItemLink(link: iItemLink) {
		this._BusyMethods.push("DeleteItemLink");
		let ErrMsg = "Something went wrong when deleting the link. \r\n If the problem persists, please contact EPPISupport.";

		this._http.post<iItemLink[]>(this._baseUrl + 'api/ArmTimepointLinkList/DeleteItemLink',

			link).subscribe(
				(result) => {
					if (!result || this._links == null || result.length < (this._links.length -1) ) this.modalService.GenericErrorMessage(ErrMsg);
					else this.links = result;
					this.RemoveBusy("DeleteItemLink");
					return result;
				}
				, (error) => {
					this.FetchAll(this._currentItem);
					this.modalService.GenericError(error);
					this.RemoveBusy("DeleteItemLink");
				}
			);

	}
}
interface iItemArmTimepointLinkLists {
	arms: iArm[];
	timePoints: iTimePoint[];
	links: iItemLink[];
}
export class ItemTimepointDeleteWarningCommandJSON {

	numOutcomes: number = 0;
    itemId: number = 0;
	itemTimepointId: number = 0;
}

export interface iTimePoint {
	//[key: number]: any;  // Add index signature
	itemId: number;
	timepointValue: string;
	timepointMetric: string;
	itemTimepointId: number;
}

export class TimePoint {

	constructor(ItemId: number, TimepointValue: string, TimepointMetric: string,
		ItemTimepointId: number) {
		this.itemId = ItemId;
		this.timepointValue = TimepointValue;
		this.timepointMetric = TimepointMetric;
		this.itemTimepointId = ItemTimepointId;
	}
	itemId: number = 0;
	timepointValue: string = '';
	timepointMetric: string = '';
	itemTimepointId: number = 0;

}
export interface iArm {
	[key: number]: any;  // Add index signature
	itemArmId: number;
	itemId: number;
	ordering: number;
	title: string;
}
export class Arm {

	itemArmId: number = 0;
	itemId: number = 0;
	ordering: number = 0;
	title: string = '';

}
export class ItemArmDeleteWarningCommandJSON {

	itemId: number = 0;
	itemArmId: number = 0;
	numCodings: number = 0;

}
export interface iItemLink {
	itemLinkId: number;
	itemIdPrimary: number;
	itemIdSecondary: number;
	title: string;
	shortTitle: string;
	description: string;
}