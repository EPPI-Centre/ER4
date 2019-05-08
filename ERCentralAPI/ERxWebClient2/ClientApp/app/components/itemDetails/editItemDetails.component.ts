import { Component, Inject, OnInit, EventEmitter, Output, Input } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Params } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subject, Subscription } from 'rxjs';
import { Item, ItemListService, KeyValue } from '../services/ItemList.service';
import { ReviewerTermsService } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ModalService } from '../services/modal.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { isString } from '@progress/kendo-angular-grid/dist/es2015/utils';
import { Helpers } from '../helpers/HelperMethods';



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
    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            if (!this.ReviewerIdentityServ.HasWriteRights) this.GoBack();
            if (this.ItemListService.ItemTypes.length == 0) this.ItemListService.FetchItemTypes();
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
    public item: Item | null = null;
    private OriginalItem: Item | null = null;//used to support "cancel".
    private subItemIDinPath: Subscription | null = null;
    private subReturnTo: Subscription | null = null;
    private itemString: string = '0';
    public showOptionalFields = false;
    private returnTo: string = "Main";
    private _ItemTypes: KeyValue[] = [];
    public get ItemTypes(): KeyValue[] {
        //looking at this.ItemListService.ItemTypes makes the service fetch the data if it's not already there
        //the below is a system to make it ask only once
        if (this._ItemTypes.length == 0 && this.ItemListService.ItemTypes.length > 0) this._ItemTypes = this.ItemListService.ItemTypes;
        else if (this._ItemTypes.length == 0 && !this.ItemListService.IsBusy) this.ItemListService.FetchItemTypes();
        return this._ItemTypes;
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
    public get CanSave(): boolean {
        if (!this.ReviewerIdentityServ.HasWriteRights) return false;
        else if (!this.item) return false;
        else if (this.item.typeId == 0) return false;
        else return true;
    }
    public get IsServiceBusy(): boolean {
        if (this.ItemListService.IsBusy) return true;
        else return false;
    }
    public FieldsByType(typeId: number) {
        return Helpers.FieldsByPubType(typeId);
    }
    private _ItemFlagOptions: KeyValue[] = [new KeyValue('I', 'Included'), new KeyValue('E', 'Excluded')];//, new KeyValueState('D', 'Deleted') can't do deleted 'cause BO doesn't save this state...
    public get ItemFlagOptions(): KeyValue[] {
        return this._ItemFlagOptions;
    }
   
    public get ItemFlagStatus(): KeyValue {
        let i = this._ItemFlagOptions.findIndex(found => (this.item != null && found.key == this.item.itemStatus));
        if (i == -1) return this._ItemFlagOptions[0];
        else return this._ItemFlagOptions[i];
    }
    public SetItemFlagStatus(val: string) {
        if (this.item)
        {
            this.item.itemStatus = val;
            if (val == "I") {
                this.item.itemStatusTooltip = "Included in review";
            }
            else if (val == "E") {
                this.item.itemStatusTooltip = "Excluded from review";
            }
            //else if (val == "D") {
            //    this.item.itemStatusTooltip = "Deleted";
            //}
        }
    }
    TypeChanged() {
        console.log("type changed: ", this.item);
        let i = this.ItemTypes.findIndex(found => this.item != null && found.key == this.item.typeId.toString())
        if (i >= -1 && this.item) {
            this.item.typeName = this.ItemTypes[i].value;
        }
    }
    
    async SaveAndClose() {
        this.Save();
        let i = 1;
        while (this.ItemListService.IsBusy && i < 3 * 120) {
            i++;
            await Helpers.Sleep(200);
            console.log("waiting, cycle n: " + i);
        }
        this.GoBack();
    }
    
    Save() {
        if (this.item) this.ItemListService.UpdateItem(this.item);
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
}







