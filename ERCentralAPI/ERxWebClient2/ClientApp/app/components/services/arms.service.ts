import { Component, Inject, Injectable, EventEmitter, Output, OnInit } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { OK } from 'http-status-codes';
import { error } from '@angular/compiler/src/util';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { arm, Item, ItemListService } from './ItemList.service';

@Injectable({
    providedIn: 'root',
})

export class ArmsService implements OnInit{

    constructor(
        private _http: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        private ItemListService: ItemListService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
       
	}
	ngOnInit() {


	}

    private _arms: arm[] | null = null;//null when service has been instantiated, empty array when the item in question had no arms.
    public get arms(): arm[] {
        if (this._arms) return this._arms;
        else {
            this._arms = [];
            return this._arms;
        }

        //if (!this._arms) {//null => we need local storage
        //    const ArmsJson = localStorage.getItem('ItemArms');
        //    if (!ArmsJson) this._arms = new Array<arm>();
        //    else {
        //        let tArms: arm[] = JSON.parse(ArmsJson);
        //        if (tArms == undefined || tArms == null || tArms.length == 0) {
        //            this._arms = new Array<arm>();
        //        }
        //        else {
        //            //console.log("Got workAllocations from LS");
        //            this._arms = tArms;
        //        }
        //    }
        //}
        //return this._arms;
    }
    public set arms(arms: arm[]) {
        this._arms = arms;
    }
    @Output() gotArms = new EventEmitter();
    public get SelectedArm(): arm | null {
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
	private _selectedArm: arm | null = null;

    public SetSelectedArm(armID: number) {
        
        for (let arm of this.arms) {
			if (arm.itemArmId == armID) {
                this._selectedArm = arm;
                return;
            }
        }
        this._selectedArm = null;
    }
    public FetchArms(currentItem: Item) {

        let body = JSON.stringify({ Value: currentItem.itemId });

      this._http.post<arm[]>(this._baseUrl + 'api/ItemArmList/GetArms',

           body).subscribe(result => {
               this.arms = result;
               currentItem.arms = this.arms;
               this._selectedArm = null;
               this.gotArms.emit(this.arms);

            }, error => { this.modalService.SendBackHomeWithError(error); }
        );
        return currentItem.arms;
	}

	public CreateArm(currentArm: arm): Promise<arm> {

		let ErrMsg = "Something went wrong when creating an arm. \r\n If the problem persists, please contact EPPISupport.";

		return this._http.post<arm>(this._baseUrl + 'api/ItemArmList/CreateArm',

			currentArm).toPromise()
						.then(
						(result) => {
	
							if (!result) this.modalService.GenericErrorMessage(ErrMsg);
							return result;
						}
						, (error) => {

							this.modalService.GenericErrorMessage(ErrMsg);
							return error;
						}
						)
						.catch(
							(error) => {

								this.modalService.GenericErrorMessage(ErrMsg);
								return error;
							}
		);

	}


	public UpdateArm(currentArm: arm) {

		this._http.post<arm[]>(this._baseUrl + 'api/ItemArmList/UpdateArm',

			currentArm).subscribe(result => {

			},
				error => { this.modalService.SendBackHomeWithError(error); }
			);
	}

	public DeleteArm(arm: arm) {
		
		let ErrMsg = "Something went wrong when deleting an arm. \r\n If the problem persists, please contact EPPISupport.";
		let cmd: ItemArmDeleteWarningCommandJSON = new ItemArmDeleteWarningCommandJSON();
		cmd.itemArmId = arm.itemArmId;
		cmd.itemId = arm.itemId;

		return this._http.post<ItemArmDeleteWarningCommandJSON>(this._baseUrl + 'api/ItemArmList/DeleteWarningArm',

			cmd).toPromise()
			.then(
				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					// Logic here to show various error messages...

					return result;
				}
				, (error) => {

					this.modalService.GenericErrorMessage(ErrMsg);
					return error;
				}
			)
			.catch(
				(error) => {

					this.modalService.GenericErrorMessage(ErrMsg);
					return error;
				}
			);

		//this._http.post<arm[]>(this._baseUrl + 'api/ItemArmList/DeleteArm',

		//	arm).subscribe(result => {

		//	},
		//		error => { this.modalService.SendBackHomeWithError(error); }
		//	);
			   
	}

}


export class ItemArmDeleteWarningCommandJSON {

    itemId: number = 0;
	itemArmId: number = 0;
	numCodings: number = 0;


}


