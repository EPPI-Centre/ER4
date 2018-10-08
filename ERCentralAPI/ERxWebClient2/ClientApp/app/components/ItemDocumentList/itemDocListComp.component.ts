import { Component, Inject, OnInit, EventEmitter, Output, Input, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Http, RequestOptions, URLSearchParams } from '@angular/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscribable, Subscription } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';
import { FormGroup, FormControl, FormBuilder, Validators, ReactiveFormsModule  } from '@angular/forms';
import { Subject } from 'rxjs/index';
import { debounceTime, startWith, merge, switchMap, share  } from 'rxjs/operators/index';
import { pipe } from 'rxjs'
import { style } from '@angular/animations';
import { ItemCodingService } from '../services/ItemCoding.service'
import { ItemDocsService } from '../services/itemdocs.service'
import { map } from 'rxjs/operators';
import { ResponseContentType } from '@angular/http';
import { PriorityScreeningService } from '../services/PriorityScreening.service';


@Component({
    selector: 'ItemDocListComp',
    templateUrl: './ItemDocListComp.component.html',
    providers: []
})
export class ItemDocListComp implements OnInit, OnDestroy {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        private itemListService: ItemListService,
        private priorityScreeningService: PriorityScreeningService,
        public ItemDocsService: ItemDocsService,
        @Inject('BASE_URL') private _baseUrl: string

    ) {
    }
    public me: string = "I don't know";
    public sub: Subscription | null = null;
 
    @Input() itemID: number = 0;
    
    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {

                this.router.navigate(['home']);
        }
        else {

            this.sub = this.itemListService.ItemChanged.subscribe(
                    () => {
                        //console.log('inside the component doc stuff: ' + this.itemID);
                        this.ItemDocsService.FetchDocList(this.itemID);
                    }
            );
            this.sub = this.priorityScreeningService.gotItem.subscribe(
                () => {
                    //console.log('inside the component doc stuff: ' + this.itemID);
                    this.ItemDocsService.FetchDocList(this.itemID);
                }
            );
        }
    }
    
    DownloadDoc(itemDocumentId: number) {

        this.ItemDocsService.GetItemDocument(itemDocumentId);

    }

    ngOnDestroy() {

        if (this.sub) this.sub.unsubscribe();
    }

}



export class ItemDocumentList {

    ItemDocuments: ItemDocument[] = [];
}


export class ItemDocument {

    public itemDocumentId: number = 0;
    public itemId: number = 0;
    public shortTitle: string = '';
    public extension: string = '';
    public title: string = '';
    public text: string = "";
    public binaryExists: boolean = false;
    public textFrom: number = 0;
    public textTo: number = 0;
    public freeNotesStream: string = "";
    public freeNotesXML: string = '';
    public isBusy: boolean = false;
    public isChild: boolean = false;
    public isDeleted: boolean = false;
    public isDirty: boolean = false;
    public isNew: boolean = false;
    public isSavable: boolean = false;
    public isSelfBusy: boolean = false;
    public isSelfDirty: boolean = false;
    public isSelfValid: boolean = false;
    public isValid: boolean = false;

}



