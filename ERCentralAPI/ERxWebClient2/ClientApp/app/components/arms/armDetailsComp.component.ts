import { Component, OnInit, Input } from '@angular/core';
import { ArmsService, Arm } from '../services/arms.service';
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
	@Input() currentItem: Item | undefined;

	ngOnInit() {

		if (this.currentItem != null ) {
			this.armsList = this._armsService.FetchArms(this.currentItem);

			//alert(JSON.stringify(this.armsList));
		}
	}

	editField!: string;

	//armsList: Array<any> = [
	//	{ ItemArmId: 1, Title: 'Aurelia Vega', ItemId: 30 },
	//	{ ItemArmId: 2, Title: 'Guerra Cortez', ItemId: 30},
	//	{ ItemArmId: 3, Title: 'Guadalupe House', ItemId: 30 },
	//	{ ItemArmId: 4, Title: 'Aurelia Vega', ItemId: 30 },
	//	{ ItemArmId: 5, Title: 'Elisa Gallagher', ItemId: 30 },
	//];

	awaitingArmsList: Array<any> = [
		{ itemArmId: 6, title: 'Aurelia Vega', itemId: this.currentItem },
		{ itemArmId: 7, title: 'Elisa Gallagher', itemId: this.currentItem }
	];

	updateList(ItemArmId: number, property: any, event: any) {
		const editField = event.target.textContent;
		this.armsList[ItemArmId][property] = editField;

	}

	remove(ItemArmId: number) {

		//this.awaitingArmsList.push(this.armsList[ItemArmId]);

		alert(JSON.stringify(this.armsList.find(x => x.itemArmId == ItemArmId)));
		this._armsService.DeleteArm(this.armsList.find(x => x.itemArmId == ItemArmId));
		this.armsList.splice(ItemArmId, 1);
	}

	add() {
		if (this.awaitingArmsList.length > 0) {
			const newArm = this.awaitingArmsList[0];
			this.armsList.push(newArm);
			this.awaitingArmsList.splice(0, 1);
		}
	}

	changeValue(ItemArmId: number, property: string, event: any) {
		this.editField = event.target.textContent;
	}
}