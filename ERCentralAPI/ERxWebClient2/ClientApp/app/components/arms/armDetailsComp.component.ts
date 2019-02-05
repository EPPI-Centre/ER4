import { Component, OnInit, Input } from '@angular/core';
import { ArmsService } from '../services/arms.service';
import { arm, Item } from '../services/ItemList.service';

@Component({
	selector: 'armDetailsComp',
	templateUrl: './armDetails.component.html'
})
export class armDetailsComp implements OnInit {

	constructor(
		private _armsService: ArmsService
	) { }


	private armsList: Array<arm> = [];
	public title: string = '';

	@Input() currentItem: Item | undefined;

	ngOnInit() {

		if (this.currentItem != null ) {
			this.armsList = this._armsService.FetchArms(this.currentItem);
		}
	}

	editField!: string;


	updateList(key: number, property: any, event: any) {
		const editField = event.target.textContent;
		this.armsList[key][property] = editField;
		this._armsService.UpdateArm(this.armsList[key]);
	}

	remove(key: number) {
				
		this._armsService.DeleteArm(this.armsList[key]);
		this.armsList.splice(key, 1);
	}

	add(title: string) {

		if (this.currentItem != undefined) {
			let newArm: arm  = new Arm();
			newArm.title = title;
			newArm.itemId = this.currentItem.itemId;
				
				this._armsService.CreateArm(newArm).then(

				(res: any) => {

					//alert('SHOULD WAIT AND THEN: ' + JSON.stringify(res));
					let key = this.armsList.length;
					this.armsList.splice(key, 0, res);
				}
			);
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