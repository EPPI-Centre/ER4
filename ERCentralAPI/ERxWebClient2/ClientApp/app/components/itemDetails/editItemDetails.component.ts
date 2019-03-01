import { Component, Inject, OnInit, EventEmitter, Output, Input } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Params } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subject, Subscription } from 'rxjs';
import { Item, ItemListService } from '../services/ItemList.service';
import { ReviewerTermsService } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ModalService } from '../services/modal.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { isString } from '@progress/kendo-angular-grid/dist/es2015/utils';



@Component({
    selector: 'editItemDetails',
    templateUrl: './editItemDetails.component.html',
    providers: [],
    styles: []
})
export class editItemDetailsComp implements OnInit {

    constructor(
        private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemDocsService: ItemDocsService,
        private router: Router,
        private route: ActivatedRoute,
        public ItemListService: ItemListService,
        private ModalService: ModalService
    ) {}

    private item: Item | null = null;
    private OriginalItem: Item | null = null;//used to support "cancel".
    private subItemIDinPath: Subscription | null = null;
    private subReturnTo: Subscription | null = null;
    private itemString: string = '0';
    public showOptionalFields = false;
    private returnTo: string = "Main";
    private _ItemTypes: any[] = [];
    public get ItemTypes(): any {
        //looking at this.ItemListService.ItemTypes makes the service fetch the data if it's not already there
        //the below is a system to make it ask only once
        if (this._ItemTypes.length == 0 && this.ItemListService.ItemTypes.length > 0) this._ItemTypes = this.ItemListService.ItemTypes;
        else if (this._ItemTypes.length == 0) this.ItemListService.FetchItemTypes();
        return this._ItemTypes;
    }
    public FieldsByType(typeId: number) {
        if (typeId == 0) return null;
        else if (typeId == 14) {
            return {
                parentTitle: { txt: 'Journal', optional: false }
                , parentAuthors: { txt: 'Parent Authors', optional: true }
                , standardNumber: { txt: 'ISSN', optional: false }

            };
        }
        else if (typeId == 2) {
            return {
                parentTitle: { txt: 'Parent Title', optional: true }
                , parentAuthors: { txt: 'Parent Authors', optional: true }
                , standardNumber: { txt: 'ISBN', optional: false }
            };
        }
        else if (typeId == 3) {
            return {
                parentTitle: { txt: 'Book Title', optional: false }
                , parentAuthors: { txt: 'Parent Authors', optional: false }
                , standardNumber: { txt: 'ISBN', optional: true }
            };
        }
        else if (typeId == 4) {
            return {
                parentTitle: { txt: 'Publ. Title', optional: false }
                , parentAuthors: { txt: 'Parent Authors', optional: true }
                , standardNumber: { txt: 'ISSN/ISBN', optional: false }
            };
        }
        else if (typeId == 5) {
            return {
                parentTitle: { txt: 'Conference', optional: false }
                , parentAuthors: { txt: 'Parent Authors', optional: true }
                , standardNumber: { txt: 'ISSN/ISBN', optional: false }
            };
        }
        else if (typeId == 10) {
            return {
                parentTitle: { txt: 'Periodical', optional: false }
                , parentAuthors: { txt: 'Parent Authors', optional: true }
                , standardNumber: { txt: 'ISSN/ISBN', optional: true }
            };
        }
        else {
            return {
                parentTitle: { txt: 'Parent Title', optional: true }
                , parentAuthors: { txt: 'Parent Authors', optional: true }
                , standardNumber: { txt: 'Standard Number', optional: true }
            };
        }
    }
	ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            if (!this.ReviewerIdentityServ.HasWriteRights) this.GoBack();
            this.subReturnTo = this.route.queryParams.subscribe(params => {
                if (params['return']) this.returnTo = params['return'];
                else this.returnTo = "Main";
                if (this.subReturnTo) this.subReturnTo.unsubscribe();
                this.route.queryParams = new Observable<Params>();
            });
            this.subItemIDinPath = this.route.params.subscribe(params => {
                if (params['itemId']) this.itemString = params['itemId'];
                else this.itemString = "0";
                this.GetItem();
                //console.log('coding full sajdhfkjasfdh: ' + this.itemID);
            });
        }
    }
    private wasEdited: boolean = false;
    public get Edited(): boolean {
        if (this.item == null || this.OriginalItem == null) return false;
        if (this.wasEdited) return true;
        else {
            if (JSON.stringify(this.OriginalItem) !== JSON.stringify(this.item)) {
                this.wasEdited = true;
                return true;
            }
        }
        return false;
    }
    SaveAndClose() {
        this.Save();
        this.GoBack();
    }
    Save() {

    }
    private GetItem() {
        if (this.itemString == "0") {
            this.item = new Item();
            this.OriginalItem = new Item();
        }
        else {
            //this.itemID = +this.itemString;
            this.OriginalItem = this.ItemListService.getItem(+this.itemString);
            if (!this.OriginalItem || this.OriginalItem.itemId == 0) this.router.navigate(['Main']);
            this.item = (JSON.parse(JSON.stringify(this.OriginalItem)));//WARNING: this works ONLY as long as Item class has no methods!!!!
            //should the above fail, we might use iterationCopy(...), below.
        }
        
    }
    iterationCopy(src: any): any {
        //adapted from: https://medium.com/@Farzad_YZ/3-ways-to-clone-objects-in-javascript-f752d148054d
        //not tested, included here for future (01 march 2019) ref.
        let target: any = {};
        for (let prop in src) {
            if (src.hasOwnProperty(prop)) {
                target[prop] = src[prop];
            }
        }
        return target;
    }
    GoBack() {
        this.router.navigate([this.returnTo]);
    }
    toHTML(text: string): string {
        return text.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
    }
}






