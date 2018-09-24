import { Component, Inject, OnInit, Input} from '@angular/core';

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
    selector: 'armsComp',
    templateUrl: './arms.component.html',
    providers: []
})

export class armsComp implements OnInit{

    public selectedArm?: arm ;
    public arms: arm[] = [];
    @Input() itemID: number = 0;
    private subscription: Subscription = new Subscription;

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
        
        if (filterVal == "0") {
            alert('sdfg: ' + filterVal);
        }
        else {
            alert('sdfg: ' + filterVal);
            this.selectedArm = this.arms.filter((x) => x.itemArmId == filterVal)[0];
            this.reviewSetsServ.clearItemData();
            this.reviewSetsServ.AddItemData(this.itemCodingServ.ItemCodingList, this.selectedArm.itemArmId);

        }
    }   
    public CurrentItem?: Item;
    ngOnInit() {
        console.log('init armsComp');
        this.PriorityScreeningService.gotItem.subscribe(() => this.GetArmsScreening());
        this.itemListServ.ItemChanged.subscribe((res: Item) => this.GetArmsItem(res));
        //this.armsService.gotArms.subscribe(

        //    (res: arm[]) => {
        //         this.arms = res;
        //    }
        //);

        //this.itemListServ.tmpItemIDChange

        //    .subscribe(itemR => {

        //        if (itemR > 0) {
                    
        //            this.itemID = itemR;
        //            this.armsService.FetchArms(itemR);
        //        }

        //});

    }
    GetArmsScreening() {
        this.CurrentItem = this.PriorityScreeningService.CurrentItem;
        this.armsService.FetchArms(this.PriorityScreeningService.CurrentItem);
    }
    GetArmsItem(item: Item) {
        console.log('GetArmsItem(item: Item)');
        this.CurrentItem = item;
        this.armsService.FetchArms(item);
    }
}






