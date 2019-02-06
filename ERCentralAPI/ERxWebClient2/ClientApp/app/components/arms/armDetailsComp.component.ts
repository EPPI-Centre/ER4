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

					(res: any) => {

						let key = this.armsList.length;
						this.armsList.splice(key, 0, res);
					}
				);
			}
			this.title = '';
		}
	}

}


export class Arm {

	[key: number]: any;
	itemArmId: number =0;
	itemId: number = 0;
	ordering: number = 0;
	title: string = '';

}