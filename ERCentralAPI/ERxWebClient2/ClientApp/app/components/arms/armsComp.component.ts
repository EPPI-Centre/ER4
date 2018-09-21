import { Component, Inject, OnInit, Input} from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Http, RequestOptions, URLSearchParams } from '@angular/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { ItemCodingComp } from '../coding/coding.component';
import { ItemListComp } from '../ItemList/itemListComp.component';
import { ItemListService } from '../services/ItemList.service';
import { Subscription } from 'rxjs';
import {  ArmsService } from '../services/arms.service';

@Component({
    selector: 'armsComp',
    templateUrl: './arms.component.html',
    providers: []
})

export class armsComp implements OnInit{

    public arms: arm[] = [];
    public cacheArms: arm[] = [];
    @Input() itemID: number = 0;
    private subscription: Subscription = new Subscription;

    constructor(private _http: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private modalService: ModalService,
        private router: Router,
        private itemCodingComp: ItemCodingComp,
        private itemListServ: ItemListService,
        private ReviewerIdentityServ: ReviewerIdentityService, 
        private itemCodingServ: ItemCodingService,
        private reviewSetsServ: ReviewSetsService,
        private armsService: ArmsService,
        ) {
        
    }




    filterArms(filterVal: any) {

        if (filterVal == "0") {
            console.log('filter value is 0!!!!');
            this.arms = this.cacheArms;
        }
        else {
            console.log('filter value is: ' + filterVal);
            this.arms = this.cacheArms.filter((item) => item.itemArmId == filterVal);


            // NOW SIMPLY Loop through LocalStorage and select all those attributes 
            // where the armid is equal to the selection

            // fire off another fetch for the page as the ItemSets have possible changed status
            // WHERE itemArmId = a new value
        }
    }   

    ngOnInit() {

        //this.subscription = 
        this.itemListServ.tmpItemIDChange
            .subscribe(itemR => {
                this.arms =this.armsService.Fetch(itemR);
            });

        this.itemCodingComp.valueChange.subscribe(

            (res2: number) => {
                console.log('ItemID IS: ' + res2);

               this.arms = this.armsService.Fetch(res2);
            }
        );

        //hardcoded how do we find the pages itemId...1848769
        

    }
}

export class arm {

    itemId: number = 0;
    title: string = '';
    itemArmId: number = 0;
}




