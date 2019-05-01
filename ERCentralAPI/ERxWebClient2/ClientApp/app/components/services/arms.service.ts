import { Inject, Injectable, EventEmitter, Output, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { iArm, Item,  Arm } from './ItemList.service';
import { BusyAwareService } from '../helpers/BusyAwareService';

@Injectable({
    providedIn: 'root',
})

export class ArmsService extends BusyAwareService implements OnInit  {

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
    private _arms: iArm[] | null = null;//null when service has been instantiated, empty array when the item in question had no arms.
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
    @Output() gotArms = new EventEmitter();
    @Output() armChangedEE = new EventEmitter();
    public get SelectedArm(): iArm | null {
        //if this is happening in a new tab (new instance of the app)
        //we need to retreive data from local storage. We know this is the case, because this.arms is empty.
        //selected arm is NULL when no arm is selected (i.e. the whole study is, which isn't an arm!)
        //if (!this._arms) {//null => we need local storage
        //    const SelectedArmJson = localStorage.getItem('selectedArm');
        //    if (!SelectedArmJson) this._selectedArm = null;
        //    else {
        //        let tSelectedArm: arm = JSON.parse(SelectedArmJson);
        //        if (tSelectedArm == undefined || tSelectedArm == null || tSelectedArm.itemArmId == 0) {
        //            this._selectedArm = null;
        //        }
        //        else {
        //            //console.log("Got workAllocations from LS");
        //            this._selectedArm = tSelectedArm;
        //        }
        //    }
        //}
        return this._selectedArm;
    }
    private _selectedArm: iArm | null = null;

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
    public FetchArms(currentItem: Item) {

		this._BusyMethods.push("FetchArms");
		this._currentItem = currentItem;
        let body = JSON.stringify({ Value: currentItem.itemId });

        this._http.post<iArm[]>(this._baseUrl + 'api/ItemArmList/GetArms',

			   body).subscribe(result => {
				   this.arms = result;
				   currentItem.arms = this.arms;
				   this._selectedArm = null;
				   this.gotArms.emit(this.arms);
				   this.RemoveBusy("FetchArms");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchArms");
			}
			);
			return currentItem.arms;
	}

	public FetchPromiseArms(currentItem: Item): iArm[] {

		this._BusyMethods.push("FetchPromiseArms");
		this._currentItem = currentItem;
		let body = JSON.stringify({ Value: currentItem.itemId });

		this._http.post<iArm[]>(this._baseUrl + 'api/ItemArmList/GetArms',

			body).toPromise().then(

				result => {
					this.arms = result;
					currentItem.arms = this.arms;
					this.RemoveBusy("FetchPromiseArms");
					
			}

			//, error => {
			//	this.modalService.SendBackHomeWithError(error);
			//	this.RemoveBusy("FetchArms");
			//}
		);
		return this.arms;
	}


	public CreateArm(currentArm: Arm): Promise<Arm> {

		this._BusyMethods.push("CreateArm");
		let ErrMsg = "Something went wrong when creating an arm. \r\n If the problem persists, please contact EPPISupport.";

		return this._http.post<Arm>(this._baseUrl + 'api/ItemArmList/CreateArm',

			currentArm).toPromise()
						.then(
						(result) => {
	
							if (!result) this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("CreateArm");
							return result;
						}
						, (error) => {
							this.arms = this.FetchArms(this._currentItem);		
							this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("CreateArm");
							return error;
						}
						)
						.catch(
							(error) => {
								this.arms = this.FetchArms(this._currentItem);		
								this.modalService.GenericErrorMessage(ErrMsg);
								this.RemoveBusy("CreateArm");
								return error;
							}
		);

	}


	public UpdateArm(currentArm: iArm) {

		this._BusyMethods.push("UpdateArm");
		let ErrMsg = "Something went wrong when updating an arm. \r\n If the problem persists, please contact EPPISupport.";

		this._http.post<iArm[]>(this._baseUrl + 'api/ItemArmList/UpdateArm',

			currentArm).subscribe(

				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("UpdateArm");
					return result;
				}
				, (error) => {
					this.arms = this.FetchArms(this._currentItem);		
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

		return this._http.post<ItemArmDeleteWarningCommandJSON>(this._baseUrl + 'api/ItemArmList/DeleteWarningArm',

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

	DeleteArm(arm: iArm) {
		this._BusyMethods.push("DeleteArm");
			let ErrMsg = "Something went wrong when deleting an arm. \r\n If the problem persists, please contact EPPISupport.";
			
			this._http.post<iArm>(this._baseUrl + 'api/ItemArmList/DeleteArm',

			arm).subscribe(
				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteArm");
					return result;
				}
				, (error) => {

					this.arms = this.FetchArms(this._currentItem);					
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteArm");
					return error;
				}
				);

	}

}


export class ItemArmDeleteWarningCommandJSON {

    itemId: number = 0;
	itemArmId: number = 0;
	numCodings: number = 0;

}


