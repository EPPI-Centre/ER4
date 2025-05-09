import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { Item, ItemListService, StringKeyValue } from '../services/ItemList.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';



@Component({
    selector: 'editItemDetails',
    templateUrl: './editItemDetails.component.html',
    providers: [],
    styles: []
})
export class editItemDetailsComp implements OnInit, OnDestroy {

    constructor(
        private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemDocsService: ItemDocsService,
        private router: Router,
        private route: ActivatedRoute,
        public ItemListService: ItemListService,
        private priorityScreeningService: PriorityScreeningService
    ) {}
    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            if (!this.ReviewerIdentityServ.HasWriteRights) this.GoBack();
            if (this.ItemListService.ItemTypes.length == 0) this.ItemListService.FetchItemTypes();
            this.subReturnTo = this.route.queryParams.subscribe((params:any) => {
                if (params['return']) this.returnTo = params['return'];
                else this.returnTo = "Main";
                if (this.subReturnTo) this.subReturnTo.unsubscribe();
                this.route.queryParams = new Observable<Params>();
            });
            this.subItemIDinPath = this.route.params.subscribe((params:any) => {
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
  private _ItemTypes: StringKeyValue[] = [];
  public get ItemTypes(): StringKeyValue[] {
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
  private _ItemFlagOptions: StringKeyValue[] = [new StringKeyValue('I', 'Included'), new StringKeyValue('E', 'Excluded')];//, new KeyValueState('D', 'Deleted') can't do deleted 'cause BO doesn't save this state...
  public get ItemFlagOptions(): StringKeyValue[] {
        return this._ItemFlagOptions;
    }
   
  public get ItemFlagStatus(): StringKeyValue {
        let i = this._ItemFlagOptions.findIndex(found => (this.item != null && found.key == this.item.itemStatus));
        if (i == -1) return this._ItemFlagOptions[0];
        else return this._ItemFlagOptions[i];
    }
  public SetItemFlagStatus(event: Event) {
    let val = (event.target as HTMLOptionElement).value;

        if (val && this.item)
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
        if (this.item) {
            this.ItemListService.UpdateItem(this.item);
            if (this.returnTo == "itemcoding/PriorityScreening2") this.priorityScreeningService.CurrentItem = this.item;//so that my changes will be visible
        }
    }
    private GetItem() {
        if (this.itemString == "0") {
            this.item = new Item();
          this.OriginalItem = new Item();
          this.ItemDocsService.Clear();
        }
        else if (this.itemString == "FromPrioritySc") {
            if (this.priorityScreeningService.CurrentItem.itemId < 1) this.GoBack();
            this.OriginalItem = this.priorityScreeningService.CurrentItem;
            if (!this.OriginalItem || this.OriginalItem.itemId == 0) this.router.navigate(['Main']);
            this.item = (JSON.parse(JSON.stringify(this.OriginalItem)));//WARNING: this works ONLY as long as Item class has no methods!!!!
        }
        else {
            //this.itemID = +this.itemString;
            this.OriginalItem = this.ItemListService.getItem(+this.itemString);
            if (!this.OriginalItem || this.OriginalItem.itemId == 0) this.GoBack();
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
    ngOnDestroy() {
        if (this.subReturnTo) this.subReturnTo.unsubscribe();
        if (this.subItemIDinPath) this.subItemIDinPath.unsubscribe();
    }
}







