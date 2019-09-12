import { Component, Inject, OnInit, Input, EventEmitter, Output} from '@angular/core';

import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { ItemCodingComp } from '../coding/coding.component';
import {  Item } from '../services/ItemList.service';
import { ArmsService } from '../services/arms.service';

@Component({
    selector: 'ArmsComp',
    templateUrl: './arms.component.html',
    providers: []
})

export class armsComp implements OnInit{
    
    
    //@Output() armChangedEE = new EventEmitter();
    public CurrentItem: Item = new Item();

    constructor(
        private armsService: ArmsService
        ) {
    }
    ArmChanged(armId: number) {
        console.log("ArmChanged...");
        this.armsService.SetSelectedArm(armId);
        //this.armChangedEE.emit();
    }
    ngOnInit() {
    }
    
}






