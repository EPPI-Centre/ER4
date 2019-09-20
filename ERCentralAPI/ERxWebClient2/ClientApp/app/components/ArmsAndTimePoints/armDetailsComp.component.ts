import { Component, OnInit, Input, ViewChild, ElementRef, Renderer2 } from '@angular/core';
import { ArmsService, iArm, Arm } from '../services/arms.service';
import {  Item } from '../services/ItemList.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { Observable } from 'rxjs';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

@Component({
	selector: 'armDetailsComp',
	templateUrl: './armDetails.component.html'
})
export class armDetailsComp implements OnInit {

	constructor(
		private _armsService: ArmsService,
		private _renderer: Renderer2,
		private confirmationDialogService: ConfirmationDialogService,
        private eventsService: EventEmitterService,
        private ReviewerIdentityServ: ReviewerIdentityService
	) {
		
	}

    public get armsList(): iArm[] {

		if (!this.item || !this.item.arms) return [];
		else return this.item.arms;
	}

	public title: string = '';
	public ShowArms: boolean = true;

	public get ShowArmsBtnText(): string {
		if (this.ShowArms) return "Collapse";
		else return "Expand";
	}

	

	@Input() item!: Item | undefined;

	//@ViewChild("editTitle", { read: ElementRef }) tref!: ElementRef;

	ngOnInit() {

		//this.confirmationDialogService.
		//	.subscribe((user) => {
		//		this.user = user
		//	})
	}

	swap: boolean = false;
    public currentArm!: iArm;
	public currentTitle!: string;
	public currentKey: number = 0;
	public editTitle: boolean = false;
	public titleModel: string = '';
    public get HasWriteRights(): boolean {
        return this.ReviewerIdentityServ.HasWriteRights;
    }
    setArm(arm: iArm, key: number) {

		this.currentKey = key;
		this.currentTitle = arm.title;
		this.currentArm = arm;
	}

	editField!: string;
	
    updateList(arm: iArm) {

		this.editTitle = false;
		this.armsList[this.currentKey].title = this.currentTitle;
		this.item!.arms[this.currentKey].title = this.currentTitle;
		this._armsService.UpdateArm(this.item!.arms[this.currentKey]);
		this.ClearAndCancelEdit();
	}

	ClearAndCancelEdit() {

		this.editTitle = false;
		this.currentTitle = '';

	}

	ClearAndCancelAdd() {

		this.title = '';

	}
	
	public openConfirmationDialogDeleteArms(key: number) {
		this.confirmationDialogService.confirm('Please confirm', 'Deleting an Arm is a permanent operation and will delete all coding associated with the Arm.' +
			'<br />This Arm is associated with 0 codes.', false, '')
			.then(
				(confirmed: any) => {
					console.log('User confirmed:');
					if (confirmed ) {

						this.ActuallyRemove(key);

					} else {
						//alert('did not confirm');
					}
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}

	public openConfirmationDialogDeleteArmsWithText(key: number, numCodings: number) {
		
		this.confirmationDialogService.confirm('Please confirm', 'Deleting an Arm is a permanent operation and will delete all coding associated with the Arm.' +
			'<br /><b>This Arm is associated with ' + numCodings + ' codes.</b>' +
			'<br />Please type \'I confirm\' in the box below if you are sure you want to proceed.', true,this.confirmationDialogService.UserInputTextArms)
			.then(
			(confirm: any) => {
								
				//console.log('Text entered is the following: ' + confirm + ' ' + this.eventsService.UserInput );
			
				if (confirm && this.eventsService.UserInput  == 'I confirm') {
						
						this.ActuallyRemove(key);

					} else {
					
					}
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}

	removeWarning(key: number) {

		
		// first call the dialog then call this part
		this._armsService.DeleteWarningArm(this.armsList[key]).then(

			(res: numCodings) => {

				if (res.numCodings == 0) {

					this.openConfirmationDialogDeleteArms(key);

				
				} else if (res.numCodings == -1) {
					return;
				}
				else {
					
					this.openConfirmationDialogDeleteArmsWithText(key, res.numCodings);
				} 

			}
		);

		this.editTitle = false;
	}

	ActuallyRemove(key: number) {
        let ToRemove = this.armsList[key];
        if (ToRemove) {
            let SelectedId = this._armsService.SelectedArm ? this._armsService.SelectedArm.itemArmId : -1;
            this._armsService.DeleteArm(ToRemove);
            this.armsList.splice(key, 1);
            if (SelectedId == ToRemove.itemArmId) this._armsService.SetSelectedArm(0);
        }
	}

	add(title: string) {

		if (title != '') {
			if (this.item != undefined) {
				let newArm: Arm = new Arm();
				newArm.title = title;
				newArm.itemId = this.item.itemId;
				this._armsService.CreateArm(newArm).then(
                    (res: Arm) => {

						let key = this.armsList.length;
						this.armsList.splice(key, 0, res);
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