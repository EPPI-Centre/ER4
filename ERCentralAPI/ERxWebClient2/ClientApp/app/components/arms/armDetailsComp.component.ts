import { Component, OnInit, Input, ViewChild, ElementRef, Renderer2 } from '@angular/core';
import { ArmsService } from '../services/arms.service';
import { arm, Item } from '../services/ItemList.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { Observable } from 'rxjs';
import { EventEmitterService } from '../services/EventEmitter.service';

@Component({
	selector: 'armDetailsComp',
	templateUrl: './armDetails.component.html'
})
export class armDetailsComp implements OnInit {

	constructor(
		private _armsService: ArmsService,
		private _renderer: Renderer2,
		private confirmationDialogService: ConfirmationDialogService,
		private eventsService: EventEmitterService
	) { }

	public get armsList(): arm[] {

		if (!this.item || !this.item.arms) return [];
		else return this.item.arms;
	}

	public title: string = '';
	
	//public currentItem!: Item;
	
	@Input() item!: Item | undefined;

	//@ViewChild("editTitle", { read: ElementRef }) tref!: ElementRef;

	ngOnInit() {

		//this.confirmationDialogService.
		//	.subscribe((user) => {
		//		this.user = user
		//	})
	}

	swap: boolean = false;
	public currentArm!: arm;
	public currentTitle!: string;
	public currentKey: number = 0;
	public editTitle: boolean = false;
	public titleModel: string = '';

	setArm(arm: arm, key: number) {

		this.currentKey = key;
		this.currentTitle = arm.title;
		this.currentArm = arm;
	}

	editField!: string;
	
	updateList(arm: arm) {

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
			' This Arm is associated with 0 codes.', false, '')
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
			' This Arm is associated with ' + numCodings + ' codes.' +
			'Please type \'I confirm\' in the box below if you are sure you want to proceed.', true,this.confirmationDialogService.UserInputTextArms)
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
	}

	ActuallyRemove(key: number) {
		
		this._armsService.DeleteArm(this.armsList[key]);
		this.armsList.splice(key, 1);
	}

	add(title: string) {

		if (title != '') {
			if (this.item != undefined) {
				let newArm: arm = new Arm();
				newArm.title = title;
				newArm.itemId = this.item.itemId;
				this._armsService.CreateArm(newArm).then(
					(res: arm) => {

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


export class Arm {

	[key: number]: any;
	itemArmId: number =0;
	itemId: number = 0;
	ordering: number = 0;
	title: string = '';

}

export interface numCodings {

	numCodings: number;

}