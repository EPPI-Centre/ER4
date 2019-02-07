import { Component, OnInit, Input, ViewChild, ElementRef, Renderer2 } from '@angular/core';
import { ArmsService } from '../services/arms.service';
import { arm, Item } from '../services/ItemList.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { Observable } from 'rxjs';

@Component({
	selector: 'armDetailsComp',
	templateUrl: './armDetails.component.html'
})
export class armDetailsComp implements OnInit {

	constructor(
		private _armsService: ArmsService,
		private _renderer: Renderer2,
		private confirmationDialogService: ConfirmationDialogService,
		private elRef: ElementRef
	) { }

	public get armsList(): arm[] {

		if (!this.item || !this.item.arms) return [];
		else return this.item.arms;
	}

	public title: string = '';
	
	public currentItem!: Item;
	
	@Input() item!: Item | undefined;

	@ViewChild("editTitle", { read: ElementRef }) tref!: ElementRef;

	ngOnInit() {

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
	

	public openConfirmationDialogDeleteSearches(key: number) {
		this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to delete the selected arm ?')
			.then(
				(confirmed) => {
					console.log('User confirmed:', confirmed);
					if (confirmed) {
						this.remove(key);
					} else {
						//alert('did not confirm');
					}
				}
			)
			.catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
	}

	remove(key: number) {

		// first call the dialog then call this part
		this._armsService.DeleteArm(this.armsList[key]).then(

			(res: numCodings) => {

				//alert(JSON.stringify(res));
				this.armsList.splice(key, 1);
				// Show a particular warning based upon this result
				if (res.numCodings == 0) {
					alert('need the normal 0 codings alert here');

				} else {
					alert('need the complicated user enters text warning alert here');

				}
			}
		);

		
	}

	add(title: string) {

		if (title != '') {

			if (this.item != undefined) {
				let newArm: arm = new Arm();
				newArm.title = title;
				newArm.itemId = this.item.itemId;


				//let res: any = this._armsService.CreateArm(newArm).; 

				//let key = this.armsList.length;
				//this.armsList.splice(key, 0, res);

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