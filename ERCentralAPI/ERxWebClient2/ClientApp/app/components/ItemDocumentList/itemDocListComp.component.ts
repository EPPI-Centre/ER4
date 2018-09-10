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


@Component({
    selector: 'ItemDocListComp',
    templateUrl: './ItemDocListComp.component.html',
    providers: []
})
export class ItemDocListComp implements OnInit, OnDestroy {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        private ItemCodingService: ItemCodingService,
        public ItemDocsService: ItemDocsService,
        @Inject('BASE_URL') private _baseUrl: string

    ) {
    }
    public me: string = "I don't know";
    public sub: Subscription | null = null;
    //public _itemDocs: ItemDocument[] = [];

    // testing
    @Input() itemID: number = 0;
    //public itemID: number = 0;

    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {

                this.router.navigate(['home']);
        }
        else {

                this.sub = this.ItemCodingService.DataChanged.subscribe(
                
                    () => {
                        console.log('inside the component doc stuff: ' + this.itemID);
                        //this.itemID = itemID;
                        this.ItemDocsService.FetchDocList(this.itemID);
                    }
                );
           
            //this.ItemDocsService.FetchDocList(this.itemID);
              //this.ItemDocsService.FetchDocList(this.itemID).subscribe(

              //      (res) => {
              //          this._itemDocs = res;
              //          for (var i = 0; i < res.length; i++) {

              //              console.log(this._itemDocs[i].title);
              //          }
              //      }

              //  );

        }
    }
    
    DownloadDoc(itemDocumentId: number) {

        this.ItemDocsService.GetItemDocument(itemDocumentId);

    }
        //return this._httpC
        //    .get((this._baseUrl + 'api/ItemDocumentList/GetItemDocument', body
        //    ), {
               
        //    }).pipe(
        //        map(res => {
        //        return {
                                       
        //            filename: 'filename.pdf',
        //            data: res
        //        };
        //    })
        //    )
        //    .subscribe(res => {

        //        console.log('start download:', res);
        //        var url = window.URL.createObjectURL(res.data);
        //        var a = document.createElement('a');
        //        document.body.appendChild(a);
        //        a.setAttribute('style', 'display: none');
        //        a.href = url;
        //        a.download = res.filename;
        //        a.click();
        //        window.URL.revokeObjectURL(url);
        //        a.remove(); // remove the element
        //    }, error => {
        //        console.log('download error:', JSON.stringify(error));
        //    }, () => {
        //        console.log('Completed file download.')
        //    });


    ngOnDestroy() {
        //console.log('killing itemDocListComp');
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



