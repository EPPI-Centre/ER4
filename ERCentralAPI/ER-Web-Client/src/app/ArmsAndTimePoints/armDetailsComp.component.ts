import { Component, OnInit, Input, ViewChild, ElementRef, Renderer2 } from '@angular/core';
import { ArmTimepointLinkListService, iArm, Arm, ItemArmDeleteWarningCommandJSON } from '../services/ArmTimepointLinkList.service';
import { Item } from '../services/ItemList.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { NgForm } from '@angular/forms';
import { Helpers } from '../helpers/HelperMethods';

@Component({
	selector: 'armDetailsComp',
	templateUrl: './armDetails.component.html'
})
export class armDetailsComp implements OnInit {

	constructor(
		private _armsService: ArmTimepointLinkListService,
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
	@ViewChild('ArmsForm') ArmsForm!: NgForm;
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

		this.editTitle = true;
		this.currentKey = key;
		this.title = arm.title;
		this.currentArm = arm;
	}

	editField!: string;
	
    updateList(arm: iArm) {

		this.editTitle = false;
		this.armsList[this.currentKey].title = this.title;
		this.item!.arms[this.currentKey].title = this.title;
		this._armsService.UpdateArm(this.item!.arms[this.currentKey]);
		this.Clear();
	}

	Clear() {
		//console.log("Clear in ArmDetails");
		this.editTitle = false;
		//this.titleModel = '';
		this.title = '';
		if (this.ArmsForm) this.ArmsForm.resetForm({n1: this.title});
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

  public openConfirmationDialogDeleteArmsWithText(key: number, numCodings: number, numOutcomes: number) {
    let contents: string = "Deleting an Arm is a permanent operation and will delete all coding associated with the Arm.";

    if (numCodings == 1) contents += '<br /><b>This Arm is associated with one code.</b>';
    else if (numCodings > 1) contents += '<br /><b>This Arm is associated with ' + numCodings + ' codes.</b>';

    if (numOutcomes == 1) contents += '<br /><b>This Arm is associated with one outcome.</b>';
    else if (numOutcomes > 1) contents += '<br /><b>This Arm is associated with ' + numOutcomes + ' outcomes.</b>';

    contents += '<br />Please type \'I confirm\' in the box below if you are sure you want to proceed.';
    this.confirmationDialogService.confirm('Please confirm', contents, true, 'I confirm')
      .then(
        (confirm: any) => {

          //console.log('Text entered is the following: ' + confirm + ' ' + this.eventsService.UserInput );

          if (confirm && this.eventsService.UserInput.toLowerCase().trim() == 'i confirm') {
            this.ActuallyRemove(key);
          } else {

          }
        }
      )
      .catch(() => { 
        //console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)')
      });
  }

	removeWarning(key: number) {
		// first call the dialog then call this part
		this._armsService.DeleteWarningArm(this.armsList[key]).then(

      (res: ItemArmDeleteWarningCommandJSON) => {
				if (res.numCodings == 0 && res.numOutcomes == 0) {
					this.openConfirmationDialogDeleteArms(key);
				} else if (res.numCodings == -1) {
					return;
				}
				else {
					this.openConfirmationDialogDeleteArmsWithText(key, res.numCodings, res.numOutcomes);
				} 
			}
		);
		this.editTitle = false;
	}

  async ActuallyRemove(key: number) {
    let ToRemove = this.armsList[key];
    if (ToRemove && this.item) {
      let SelectedId = this._armsService.SelectedArm ? this._armsService.SelectedArm.itemArmId : -1;
      let something = await this._armsService.DeleteArm(ToRemove);
      let toKeep = this.item.arms;
      this.item.arms = [];
      //for some reason, if we don't await, the arms dropdowns show the first arm as "selected", when in fact it isn't...
      await Helpers.Sleep(5);
      toKeep.splice(key, 1);
      this.item.arms = toKeep;
      if (SelectedId == ToRemove.itemArmId) this._armsService.SetSelectedArm(0);
      
    }
  }
  

	add(title: string) {
		//console.log("Add arm:", title);
		if (title != '') {
			if (this.item != undefined) {
				let newArm: Arm = new Arm();
				newArm.title = title;
				newArm.itemId = this.item.itemId;
				this._armsService.CreateArm(newArm).then(
          (res: Arm) => {
            let key = this.armsList.length;
            let ires: iArm = {
              [key]: key,  // Add index signature
              itemArmId: res.itemArmId,
              itemId: res.itemId,
              ordering: res.ordering,
              title: res.title
            };
            this.armsList.splice(key, 0, ires);
          }
        );
      }
      this.title = '';
    }
    this.Clear();
  }

}


