import { Component, OnInit, Input, ViewChild, ElementRef, Renderer2 } from '@angular/core';
import { ArmsService } from '../services/arms.service';
import { arm, Item } from '../services/ItemList.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

@Component({
	selector: 'armDetailsComp',
	templateUrl: './armDetails.component.html'
})
export class armDetailsComp implements OnInit {

	constructor(
		private _armsService: ArmsService,
		private _renderer: Renderer2,
		private confirmationDialogService: ConfirmationDialogService
	) { }


	public armsList: Array<arm> = [];
	public title: string = '';

	@Input() item: Item | undefined;

	@ViewChild("editTitle", { read: ElementRef }) tref!: ElementRef;

	ngOnInit() {

		if (this.item != null ) {
			this.armsList = this._armsService.FetchArms(this.item);
		}
	}

	ItemChanged() {
		alert('something happened');
		//if (this.item != null) {
		//	this.armsList = this._armsService.FetchArms(this.item);
		//}
	}

	swap: boolean = false;
	EditField() {

		if (this.swap == false) {
			this._renderer.setAttribute(this.tref.nativeElement, 'contenteditable', 'true');
			this.swap = true;
		} else {
			this._renderer.setAttribute(this.tref.nativeElement, 'contenteditable', 'false');
		}
		
		
	}

	editField!: string;


	updateList(key: number, property: any, event: any) {
		const editField = event.target.textContent;
		this.armsList[key][property] = editField;
		this._armsService.UpdateArm(this.armsList[key]);
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

	changeValue(ItemArmId: number, property: string, event: any) {
		this.editField = event.target.textContent;
	}
}


export class Arm {

	[key: number]: any;
	itemArmId: number =0;
	itemId: number = 0;
	ordering: number = 0;
	title: string = '';

}