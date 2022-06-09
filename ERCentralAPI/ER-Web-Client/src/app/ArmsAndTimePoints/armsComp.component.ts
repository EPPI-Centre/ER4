import { Component, OnInit } from '@angular/core';
import {  Item } from '../services/ItemList.service';
import { ArmTimepointLinkListService } from '../services/ArmTimepointLinkList.service';

@Component({
    selector: 'ArmsComp',
    templateUrl: './arms.component.html',
    providers: []
})

export class armsComp implements OnInit{
    
    
    //@Output() armChangedEE = new EventEmitter();
    public CurrentItem: Item = new Item();

    constructor(
        private armsService: ArmTimepointLinkListService
        ) {
    }
  ArmChanged(event: Event) {
    let armId = parseInt((event.target as HTMLOptionElement).value);
    //console.log("ArmChanged...");
    if (!isNaN(armId)) this.armsService.SetSelectedArm(armId);
    //this.armChangedEE.emit();
  }
    ngOnInit() {
    }
    
}






