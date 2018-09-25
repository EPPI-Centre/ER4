import { Component, Inject, OnInit, Input, EventEmitter, Output} from '@angular/core';

import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { ItemCodingComp } from '../coding/coding.component';
import { ItemListService, Item, arm } from '../services/ItemList.service';
import { Subscription, BehaviorSubject } from 'rxjs';
import { ArmsService } from '../services/arms.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';

@Component({
    selector: 'ArmsComp',
    templateUrl: './arms.component.html',
    providers: []
})

export class armsComp implements OnInit{

    public selectedArm?: arm ;
    //public arms: arm[] = [];
    //@Input() itemID: number = 0;
    //private subscription: Subscription = new Subscription;
    @Output() armChangedEE = new EventEmitter<number>();
    public CurrentItem: Item = new Item();

    constructor(
        @Inject('BASE_URL') private _baseUrl: string,
        private modalService: ModalService,
        private itemCodingComp: ItemCodingComp,
        private itemListServ: ItemListService,
        private ReviewerIdentityServ: ReviewerIdentityService, 
        private PriorityScreeningService: PriorityScreeningService,
        private itemCodingServ: ItemCodingService,
        private reviewSetsServ: ReviewSetsService,
        private armsService: ArmsService,
        ) {
    }

    filterArms(filterVal: any) {
        
        //if (filterVal == "0") {
        //    console.log('sdfg0: ' + filterVal);
        //    this.reviewSetsServ.clearItemData();
        //    this.reviewSetsServ.AddItemData(this.itemCodingServ.ItemCodingList, 0);
        //}
        //else if (this.CurrentItem) {

        //    console.log('sdfg: ' + filterVal + this.CurrentItem.arms);

        //    this.selectedArm = this.CurrentItem.arms.filter((x) => x.itemArmId == filterVal)[0];
        //    this.reviewSetsServ.clearItemData();
        //    if (this.selectedArm) this.reviewSetsServ.AddItemData(this.itemCodingServ.ItemCodingList, this.selectedArm.itemArmId);
        //    else {//do something! it's not supposed to happen...
        //    }
        //}
    }   
    ArmChanged(armId: number) {
        //console.log(event);
        this.armChangedEE.emit(armId);
    }
    ngOnInit() {

        //console.log('init armsComp');

        //this.CurrentItem = this.itemListServ.currentItem;
        //this.PriorityScreeningService.gotItem.subscribe(() => this.GetArmsScreening());
        //this.itemListServ.ItemChanged.subscribe(() => this.GetArmsItem());

    }

    //GetArmsScreening() {
    //    this.CurrentItem = this.PriorityScreeningService.CurrentItem;
    //    //this.armsService.FetchArms(this.PriorityScreeningService.CurrentItem);
    //}

    //GetArmsItem() {
    //    //console.log('Calling GetArmsItem(item: Item)');
    //    this.CurrentItem = this.itemListServ.currentItem;
    //    this.CurrentItem = this.itemCodingComp.item;
    //    //console.log('arms look like this inside the item: ' + JSON.stringify(this.CurrentItem));
    //    //if (this.CurrentItem) this.armsService.FetchArms(this.CurrentItem);
    //}
}






