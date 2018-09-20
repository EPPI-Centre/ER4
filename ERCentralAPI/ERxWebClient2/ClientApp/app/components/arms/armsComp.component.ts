import { Component, Inject, OnInit} from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Http, RequestOptions, URLSearchParams } from '@angular/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReviewSetsService } from '../services/ReviewSets.service';

@Component({
    selector: 'armsComp',
    templateUrl: './arms.component.html',
    providers: []
})

export class armsComp implements OnInit{

    public arms: arm[] = [];
    public cacheArms: arm[] = [];

    constructor(private _http: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private modalService: ModalService,
        private router: Router,
        private ReviewerIdentityServ: ReviewerIdentityService, 
        private itemCodingServ: ItemCodingService,
        private reviewSetsServ: ReviewSetsService,
        private route: ActivatedRoute
        ) {
        
    }

    public Fetch(ItemId: number) {

        let body = JSON.stringify({ Value: ItemId });

        this._http.post<arm[]>(this._baseUrl + 'api/ItemSetList/GetArms',

            body).subscribe(result => {

                console.log('got inside subscription');
                this.arms = result;
                this.cacheArms = result;
                const armsJson = JSON.stringify(this.arms)
                console.log('jsonified: ' + armsJson );

            }, error => { this.modalService.SendBackHomeWithError(error); }
            );
    }
   

    //public Fetch(ItemId: number) {

    //    this.itemCodingSrv..filter(x => x. == this.personId)[0];
        
    //}

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

        console.log('initiated component...');

        this.Fetch(1848769);

    }
}

export class arm {

    itemId: number = 0;
    title: string = '';
    itemArmId: number = 0;
}




//export class ItemDocumentList {

//    ItemDocuments: ItemDocument[] = [];
//}


//export class ItemDocument {

//    public itemDocumentId: number = 0;
//    public itemId: number = 0;
//    public shortTitle: string = '';
//    public extension: string = '';
//    public title: string = '';
//    public text: string = "";
//    public binaryExists: boolean = false;
//    public textFrom: number = 0;
//    public textTo: number = 0;
//    public freeNotesStream: string = "";
//    public freeNotesXML: string = '';
//    public isBusy: boolean = false;
//    public isChild: boolean = false;
//    public isDeleted: boolean = false;
//    public isDirty: boolean = false;
//    public isNew: boolean = false;
//    public isSavable: boolean = false;
//    public isSelfBusy: boolean = false;
//    public isSelfDirty: boolean = false;
//    public isSelfValid: boolean = false;
//    public isValid: boolean = false;

//}



