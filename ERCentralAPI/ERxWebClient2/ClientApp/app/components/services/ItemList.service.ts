import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient,  } from '@angular/common/http';
import { WorkAllocationListService } from './WorkAllocationList.service';
import { PriorityScreeningService } from './PriorityScreening.service';
import { ModalService } from './modal.service';
import { error } from '@angular/compiler/src/util';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { ArmsService } from './arms.service';
import { Subject } from 'rxjs';
import { Helpers } from '../helpers/HelperMethods';
import { ReadOnlySource } from './sources.service';

@Injectable({
    providedIn: 'root',
    }
)

export class ItemListService extends BusyAwareService {
    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
		private _WorkAllocationService: WorkAllocationListService,
        private _PriorityScreeningService: PriorityScreeningService,
		private ModalService: ModalService
    ) {
        super();
        //this.timerObj = timer(5000, 5000).pipe(
        //    takeUntil(this.killTrigger));
        //this.timerObj.subscribe(() => console.log("ItemListServID:", this.ID));
	}


    private _IsInScreeningMode: boolean | null = null;
    public get IsInScreeningMode(): boolean {
        if (this._IsInScreeningMode !== null) return this._IsInScreeningMode;
        else return false;
    }
    public set IsInScreeningMode(state: boolean) {
        this._IsInScreeningMode = state;
        //this.Save();
	}
    private _ItemList: ItemList = new ItemList();
    private _Criteria: Criteria = new Criteria();
    private _currentItem: Item = new Item();
    private _ItemTypes: any[] = [];
    public get ItemTypes(): any[] {
        //console.log("Get ItemTypes");
        return this._ItemTypes;
    }
    public ListDescription: string = "";
    @Output() ItemChanged = new EventEmitter();
	public get ItemList(): ItemList {
        return this._ItemList;
    }
    public get ListCriteria(): Criteria {
        return this._Criteria;
    }
    public get currentItem(): Item {
        return this._currentItem;
    }
    private _CurrentItemAdditionalData: iAdditionalItemDetails | null = null;
    public get CurrentItemAdditionalData(): iAdditionalItemDetails | null {
        if (!this._currentItem || !this._CurrentItemAdditionalData) return null;
        else if (this._currentItem.itemId !== this._CurrentItemAdditionalData.itemID) return null;
        else return this._CurrentItemAdditionalData;
    }
    public FetchWithCrit(crit: Criteria, listDescription: string) {
        this._BusyMethods.push("FetchWithCrit");
        this._Criteria = crit;
        if (this._ItemList && this._ItemList.pagesize > 0
            && this._ItemList.pagesize <= 4000
            && this._ItemList.pagesize != crit.pageSize
        ) {
            crit.pageSize = this._ItemList.pagesize;
        }
        console.log("FetchWithCrit", this._Criteria.listType);
        this.ListDescription = listDescription;
        this._httpC.post<ItemList>(this._baseUrl + 'api/ItemList/Fetch', crit)
            .subscribe(
                list => {
                    this._Criteria.totalItems = this.ItemList.totalItemCount;
                    this.SaveItems(list, this._Criteria);
                }, error => {
                    this.ModalService.GenericError(error);
                    this.RemoveBusy("FetchWithCrit");
                }
                , () => { this.RemoveBusy("FetchWithCrit"); }
            );
    }
    public Refresh() {
        if (this._Criteria && this._Criteria.listType && this._Criteria.listType != "") {
            //we have something to do
            this.FetchWithCrit(this._Criteria, this.ListDescription);
        }
    }
    public FetchItemTypes() {
        this._BusyMethods.push("FetchItemTypes");
        this._httpC.get<KeyValue[]>(this._baseUrl + 'api/ItemList/ItemTypes')
            .subscribe(
            (res) => {
                this.RemoveBusy("FetchItemTypes"); 
                //putting the "journal" type close to the top...
                let i = res.findIndex(found => found.key == '14');
                if (i > -1) {
                    let j = res.splice(i, 1);
                    res.splice(1,0, j[0]);
                }
                this._ItemTypes = res;
                //console.log(res);
            }
            , (err) => {
                this.RemoveBusy("FetchItemTypes");
                this.ModalService.GenericError(err);
            }
            );
    }
    public FetchAdditionalItemDetails() {
        console.log("FetchAdditionalItemDetails");
        if (this._currentItem.itemId !== 0) {
            this._BusyMethods.push("FetchAdditionalItemDetails");
            let body = JSON.stringify({ Value: this._currentItem.itemId });
            this._httpC.post<iAdditionalItemDetails>(this._baseUrl + 'api/ItemList/FetchAdditionalItemData', body)
                .subscribe(
                    result => {
                        this._CurrentItemAdditionalData = result;
                    }, error => {
                        this.ModalService.GenericError(error);
                        this.RemoveBusy("FetchAdditionalItemDetails");
                    }
                    , () => { this.RemoveBusy("FetchAdditionalItemDetails"); }
                );
        }
    }
    
    public UpdateItem(item: Item) {
        this._BusyMethods.push("UpdateItem");
        this._httpC.post<Item>(this._baseUrl + 'api/ItemList/UpdateItem', item)
            .subscribe(
                result => {
                   //if we get an item back, put it in the list substituting it via itemID
                    if (item.itemId == 0) {
                        //we created a new item, add to current list, so users can see it immediately...
                        //this._currentItem = result;//not sure we need this...
                        this.ItemList.items.push(result);
                    }
                    else {
                        //try to replace item in current list. We use the client side object 'cause the typename might otherwise be wrong.
                        let i = this.ItemList.items.findIndex(found => found.itemId == item.itemId);
                        if (i !== -1) {
                            //console.log("replacing updated item.", this.ItemList.items[i]);
                            this.ItemList.items[i] = item;
                            console.log("replaced updated item.");//, this.ItemList.items[i]);
                        }
                        else {
                            console.log("updated item not replaced: could not find it...");
                        }
					}
					this.RemoveBusy("UpdateItem");
                }, error => {
                    this.ModalService.GenericError(error);
                    this.RemoveBusy("UpdateItem");
                }
            , () => { this.RemoveBusy("UpdateItem"); }
            );
    }


    public GetIncludedItems() {
        let cr: Criteria = new Criteria();
        cr.listType = 'StandardItemList';
        this.FetchWithCrit(cr, "Included Items");
    }
    public GetExcludedItems() {
        let cr: Criteria = new Criteria();
        cr.listType = 'StandardItemList';
        cr.onlyIncluded = false;
        this.FetchWithCrit(cr, "Excluded Items");
    }
    public GetDeletedItems() {
        let cr: Criteria = new Criteria();
        cr.listType = 'StandardItemList';
        cr.onlyIncluded = false;
        cr.showDeleted = true;
        this.FetchWithCrit(cr, "Excluded Items");
    }
    public static GetCitation(Item: Item): string {
        let retVal: string = "";
        switch (Item.typeId) {
            case 1: //Report
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". <i>" + Item.title.replace(/</g, "&lt;") + "</i>. " + Item.city.replace(/</g, "&lt;") + ": " + Item.publisher.replace(/</g, "&lt;") + ". ";
                break;
            case 2: //Book, Whole
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". <i>" + Item.title.replace(/</g, "&lt;") + "</i>. " + Item.city.replace(/</g, "&lt;") + ": " + Item.publisher.replace(/</g, "&lt;") + ".";
                break;
            case 3: //Book, Chapter
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". " + Item.title.replace(/</g, "&lt;") + ". In <i>" + Item.parentTitle.replace(/</g, "&lt;") + "</i>, edited by " + ItemListService.CleanAuthors(Item.parentAuthors) + ", " +
                    Item.pages + ". " + Item.city + ": " + Item.publisher + ".";
                break;
            case 4: //Dissertation
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". " + Item.edition.replace(/</g, "&lt;") + ", " + Item.institution.replace(/</g, "&lt;") + ".";
                break;
            case 5: //Conference Proceedings
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". " + Item.title.replace(/</g, "&lt;") + ". Paper presented at " + Item.parentTitle.replace(/</g, "&lt;") + ", " + Item.city.replace(/</g, "&lt;") + ": " + Item.publisher.replace(/</g, "&lt;") + ".";
                break;
            case 6: //Document From Internet Site
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". " + Item.publisher.replace(/</g, "&lt;") + ". " + URL +
                    (Item.availability == "" ? "" : " [Accessed " + Item.availability.replace(/</g, "&lt;") + "] ") + ".";
                break;
            case 7: //Web Site
                retVal = ItemListService.CleanAuthors(Item.authors) + ". (" + Item.year + "). <i>" + Item.title.replace(/</g, "&lt;") + "</i>. " + Item.publisher.replace(/</g, "&lt;") + ". " + URL +
                    (Item.availability == "" ? "" : " [Accessed " + Item.availability.replace(/</g, "&lt;") + "] ") + ".";
                break;
            case 8: //DVD, Video, Media
                retVal = "\"" + Item.title.replace(/</g, "&lt;") + "\". " + Item.year + ". " + (Item.availability == "" ? "" : " [" + Item.availability.replace(/</g, "&lt;") + "] ") +
                    Item.city + ": " + ItemListService.CleanAuthors(Item.authors) + ".";
                break;
            case 9: //Research project
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". " + Item.city.replace(/</g, "&lt;") + ": " + Item.publisher.replace(/</g, "&lt;") + ".";
                break;
            case 10: //Article In A Periodical
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". <i>" + Item.parentTitle.replace(/</g, "&lt;") + "</i> " + Item.volume + (Item.issue != "" ? "(" + Item.issue + ")" : "") + ":" + Item.pages + ".";
                break;
            case 11: //Interview
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". ";
                break;
            case 12: //Generic
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". " + Item.city.replace(/</g, "&lt;") + ": " + Item.publisher.replace(/</g, "&lt;") + ".";
                break;
            case 14: //Journal, Article
                retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title + "\". <i>" + Item.parentTitle + "</i> " + Item.volume + (Item.issue != "" ? "(" + Item.issue + ")" : "") + ":" + Item.pages + ".";
                break;
        }
        //console.log("GetCitation for Item: ", Item, retVal);
        return retVal;
    }
    public static CleanAuthors(inputAuthors: string): string {
        if (inputAuthors != "") {
            inputAuthors = inputAuthors.replace(" ;", ",");
            inputAuthors = inputAuthors.replace(";", ",");
            inputAuthors = inputAuthors.replace(/</g, "&lt;");
            inputAuthors = inputAuthors.trim();
            if (inputAuthors.endsWith(',')) inputAuthors = inputAuthors.substring(0, inputAuthors.length -1);
        }
        let commaCount = 0;
        for (let i = 0; i < inputAuthors.length; i++) if (inputAuthors[i] == ',') commaCount++;
        if (commaCount > 0) {
            let cI = inputAuthors.lastIndexOf(',');
            inputAuthors = inputAuthors.substring(0, cI) + " and" + inputAuthors.substring(cI + 1);//.(inputAuthors.LastIndexOf(",") + 1, " and");
        }
        return inputAuthors;
    }
    public SaveItems(items: ItemList, crit: Criteria) {
        //console.log('saving items');
        items.items = orderBy(items.items, this.sort); 
        this._ItemList = items;
        this._Criteria = crit;
        //this.Save();
    }
    private ChangingItem(newItem: Item) {
        //console.log('ChangingItem');
        this._currentItem = newItem;
        this._CurrentItemAdditionalData = null;
        this.FetchAdditionalItemDetails();
        //this.SaveCurrentItem();
		//console.log('This is when this is emitted actually');
		this.ItemChanged.emit(newItem);
    }
	public getItem(itemId: number): Item {

        console.log('getting item');
        let ff = this.ItemList.items.find(found => found.itemId == itemId);
        if (ff != undefined && ff != null) {
            console.log('first emit');
            this.ChangingItem(ff);
            return ff;
        }
        else {
            this.ChangingItem(new Item());
            return new Item();
        }
    }
    public hasPrevious(itemId: number): boolean {
        //if (!this.IsInScreeningMode && this._PriorityScreeningService && this._PriorityScreeningService.TrainingList && this._PriorityScreeningService.TrainingList.length > 0) {
        //    return this._PriorityScreeningService.HasPrevious();
        //}
        //else {
            let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
            if (ff != undefined && ff != null && ff > 0 && ff != -1) {
                //console.log('Has prev (yes)' + ff);
                return true;
            }
            else {
                //console.log('Has prev (no)' + ff);
                return false;
            }
        //}
    }
    public getFirst(): Item {
        let ff = this.ItemList.items[0];
        if (ff != undefined && ff != null) {
            //this.ChangingItem(ff);
            return ff;
        }
        else {
            //this.ChangingItem(new Item());
            return new Item();
        }
    }
    public getPrevious(itemId: number): Item {
        
        let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
        if (ff != undefined && ff != null && ff > -1 && ff < this._ItemList.items.length) {
            //this.ChangingItem(this._ItemList.items[ff - 1]);
            return this._ItemList.items[ff - 1];
        }
        else {
            //this.ChangingItem(new Item());
            return new Item();
        }
        
    }
    public hasNext(itemId: number): boolean {
        //if (!this.IsInScreeningMode && this._PriorityScreeningService && this._PriorityScreeningService.TrainingList && this._PriorityScreeningService.TrainingList.length > 0) {
        //    return this._PriorityScreeningService.HasNext();
        //}
        //else {
            let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
            if (ff != undefined && ff != null && ff > -1 && ff + 1 < this._ItemList.items.length) return true;
            else return false;
        //}
    }
    public getNext(itemId: number): Item {
        //console.log('getNext');
        let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
        //console.log(ff);
        if (ff != undefined && ff != null && ff > -1 && ff + 1 < this._ItemList.items.length) {
            //console.log('I am emitting');
            //this.ChangingItem(this._ItemList.items[ff + 1]);
            return this._ItemList.items[ff + 1];
        }
        else {
            //this.ChangingItem(new Item());
            return new Item();
        }
	}
    public getLast(): Item {
        let ff = this.ItemList.items[this._ItemList.items.length - 1];
        if (ff != undefined && ff != null) {
            //this.ChangingItem(ff);
            return ff;
        }
        else {
            //this.ChangingItem(new Item());
            return new Item();
        }
    }

    
    public FetchNextPage() {
        
        if (this.ItemList.pageindex < this.ItemList.pagecount-1) {
            this._Criteria.pageNumber += 1;
        } else {
        }
        this.FetchWithCrit(this._Criteria, this.ListDescription)
    }
    public FetchPrevPage() {
        if (this.ItemList.pageindex == 0 ) {
            return this.FetchWithCrit(this._Criteria, this.ListDescription);
        } else {
            this._Criteria.pageNumber -= 1;
            return this.FetchWithCrit(this._Criteria, this.ListDescription);
        }
    }
    public FetchLastPage() {
        this._Criteria.pageNumber = this.ItemList.pagecount - 1;
        return this.FetchWithCrit(this._Criteria, this.ListDescription);
    }
    public FetchFirstPage() {
        this._Criteria.pageNumber = 0;
        return this.FetchWithCrit(this._Criteria, this.ListDescription);
    }
    public FetchParticularPage(pageNum: number) {
        this._Criteria.pageNumber = pageNum;
        return this.FetchWithCrit(this._Criteria, this.ListDescription);
    }

    public sort: SortDescriptor[] = [{
        field: 'shortTitle',
        dir: 'asc'
    }];
    public sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        console.log('sorting items by ' + this.sort[0].field + " ");
        this._ItemList.items = orderBy(this._ItemList.items, this.sort);
    }
    public get HasSelectedItems(): boolean {
        //return true;
        //console.log("HasSelectedItems?", this._ItemList.items[0].isSelected, this._ItemList.items[1].isSelected);
        if (this._ItemList.items.findIndex(found => found.isSelected == true) > -1) return true;
        else return false;
    }
    public get SelectedItems(): Item[] {
        return this._ItemList.items.filter(found => found.isSelected == true);
    }
    public SelectedItemsToRIStext(): string {
        let res: string = "";
        for (let Itm of this.SelectedItems) {
            res += ItemListService.ExportItemToRIS(Itm);
        }
        console.log("SelectedItemsToRIStext", res);
        return res;
    }

    public static ExportItemToRIS(it: Item): string {
        const calend: string[]  = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul",
            "Aug", "Sep", "Oct", "Nov", "Dec"];
        const newLine: string = "\r\n";
        let res: string = "TY  - ";
        let tmp: string = "";
        switch (it.typeId) {
            case 14:
                res += "JOUR" + newLine;
                break;
            case 1:
                res += "RPRT" + newLine;
                break;
            case 2:
                res += "BOOK" + newLine;
                break;
            case 3:
                res += "CHAP" + newLine;
                break;
            case 4:
                res += "THES" + newLine;
                break;
            case 5:
                res += "CONF" + newLine;
                break;
            case 6:
                res += "ELEC" + newLine;
                break;
            case 7:
                res += "ELEC" + newLine;
                break;
            case 8:
                res += "ADVS" + newLine;
                break;
            case 10:
                res += "MGZN" + newLine;
                break;
            default:
                res += "GEN" + newLine;
                break;
        }
        res += "T1  - " + it.title + newLine;
        if (it.typeId == 10 || it.typeId == 14)
            res += "JF  - " + it.parentTitle + newLine;
        else
            res += "T2  - " + it.parentTitle + newLine;
        for(let au of it.authors.split(';'))
        {
            tmp = au.trim();
            if (tmp != "") res += "A1  - " + tmp + newLine;
        }
        for(let au of it.parentAuthors.split(';'))
        {
            tmp = au.trim();
            if (tmp != "") res += "A2  - " + tmp + newLine;
        }
        res += "KW  - eppi-reviewer4" + newLine
            + ((it.keywords != null && it.keywords.length > 2) ? it.keywords.trim() + newLine : "");
        let Month: number | null, Yr: number | null;
        let tmpDate: string = "";
        Month = Helpers.SafeParseInt(it.month);
        if (!Month || (Month < 1 || Month > 12)) {
            Month = 1 + it.month.length > 2 ? calend.indexOf(it.month.substring(0, 3)) + 1 : 0;
        }
        Yr = Helpers.SafeParseInt(it.year);
        if (it.year !== "" && Yr) {
            if (Yr > 0) {
                if (Yr < 20) Yr += 1900;
                else if (Yr < 100) Yr += 2000;
                if ((Yr.toString()).length == 4) {
                    res += "PY  - " + Yr.toString() + newLine;
                    if (Month != 0) {

                        tmpDate += it.year + "/" +
                            ((Month.toString().length == 1 ? "0" + Month.toString() : Month.toString()))
                            + "//";
                    }
                    else {
                        tmpDate += it.year + "///" + it.month;//"Y1  - " 
                    }
                }
            }
        }
        if (tmpDate.length > 0) {
            res += "DA  - " + tmpDate + newLine;
            res += "Y1  - " + tmpDate;


            //little trick: edition information is supposed to be the additional info at the end of the 
            //Y1 filed. For Thesis pubtype (4) we use the edition field to hold the thesys type,
            //the following finishes up the Y1 field keeping all this into account

            if (it.typeId == 4 && it.edition.length > 0)
                res += newLine + "KW  - " + it.edition + newLine;
            else if (it.edition.length > 0)
                res += " " + it.edition + newLine;
            else res += newLine;

        }
        else if (it.typeId == 4 && it.edition.length > 0) {
            res += newLine + "KW  - " + it.edition + newLine;
        }//end of little trick

        //res += "N2  - " + it.abstract + newLine;
        res += "AB  - " + it.abstract + newLine;
        if (it.doi.length > 0) res += "DO  - " + it.doi + newLine;
        res += "VL  - " + it.volume + newLine;
        res += "IS  - " + it.issue + newLine;
        let split = '-';
        Yr = it.pages.indexOf(split);
        if (Yr > 0) {
            let pgs = it.pages.split(split);
            res += "SP  - " + pgs[0] + newLine;
            res += "EP  - " + pgs[1] + newLine;
        }
        else if (it.pages.length > 0) res += "SP  - " + it.pages + newLine;
        res += "CY  - " + it.city + (it.country.length > 0 ? " " + it.country : "") + newLine;
        if (it.url.length > 0)
            res += "UR  - " + it.url + newLine;
        if (it.availability.length > 0)
            res += "AV  - " + it.availability + newLine;
        if (it.publisher.length > 0)
            res += "PB  - " + it.publisher + newLine;
        if (it.standardNumber.length > 0)
            res += "SN  - " + it.standardNumber + newLine;
        res += "U1  - " + it.itemId.toString() + newLine;
        if (it.oldItemId.length > 0)
            res += "U2  - " + it.oldItemId + newLine;


        res += "N1  - " + it.comments + newLine;

        res += "ER  - " + newLine + newLine;

        res = res.replace("     ", " ");
        res = res.replace("    ", " ");
        res = res.replace("   ", " ");
        res = res.replace("   ", " ");
        return res;
    }
    


    //public Save() {
    //    if (this._ItemList.items.length > 0) {
    //        localStorage.setItem('ItemsList', JSON.stringify(this._ItemList));
    //    }
    //    else if (localStorage.getItem('ItemsList')) {
    //        localStorage.removeItem('ItemsList');
    //    }
    //    if (this._Criteria.listType != "") {
    //        localStorage.setItem('ItemsListCriteria', JSON.stringify(this._Criteria));
    //    }
    //    else if (localStorage.getItem('ItemsListCriteria')) {
    //        localStorage.removeItem('ItemsListCriteria');
    //    }
    //    if (this._IsInScreeningMode !== null) localStorage.setItem('ItemListIsInScreeningMode', JSON.stringify(this._IsInScreeningMode));
    //    else if (localStorage.getItem('ItemListIsInScreeningMode')) {
    //        localStorage.removeItem('ItemListIsInScreeningMode');
    //    }
    //    this.SaveCurrentItem();
    //}

}


export class ItemList {
    pagesize: number = 0;
    pageindex: number = 1;
    pagecount: number = 0;
    totalItemCount: number = 0;
    items: Item[] = [];
}
export class Item {
    itemId: number = 0;
    masterItemId: number = 0;
    isDupilcate: boolean= false;
    typeId: number = 0;
    title: string = "";
    parentTitle: string = "";
    shortTitle: string = "";
    dateCreated: string = "";
    createdBy: string = "";
    dateEdited: string = "";
    editedBy: string = "";
    year: string = "";
    month: string = "";
    standardNumber: string = "";
    city: string = "";
    country: string = "";
    publisher: string = "";
    institution: string = "";
    volume: string = "";
    pages: string = "";
    edition: string = "";
    issue: string = "";
    isLocal: string = "";
    availability: string = "";
    url: string = "";
    oldItemId: string = "";
    abstract: string = "";
    comments: string = "";
    typeName: string = "";
    authors: string = "";
    parentAuthors: string = "";
    doi: string = "";
    keywords: string = "";
    attributeAdditionalText: string = "";
    rank: number = 0;
    isItemDeleted: boolean = false;
    isIncluded: boolean = true;
    isSelected: boolean = false;
    itemStatus: string = "";
    itemStatusTooltip: string = "";
    arms: iArm[] = [];
}

export class Criteria {
    onlyIncluded: boolean = true;
    showDeleted: boolean = false;
    sourceId: number = 0;
    searchId: number = 0;
    xAxisSetId: number = 0;
    xAxisAttributeId: number = 0;
    yAxisSetId: number = 0;
    yAxisAttributeId: number = 0;
    filterSetId: number = 0;
    filterAttributeId: number = 0;
    attributeSetIdList: string = "";
	listType: string = "";
	attributeid: number = 0;

    pageNumber: number = 0;
    pageSize: number = 100;
    totalItems: number = 0;
    startPage: number= 0;
    endPage: number = 0;
    startIndex: number = 0;
    endIndex: number = 0;

    workAllocationId: number = 0;
    comparisonId: number = 0;
    description: string = "";
    contactId: number = 0;
    setId: number = 0;
    showInfoColumn: boolean = true;
    showScoreColumn: boolean = true;
}

export interface iArm {
	[key: number]: any;  // Add index signature
	itemArmId: number;
    itemId: number;
    ordering: number;
    title: string;
}

export class Arm {
    
    itemArmId: number = 0;
    itemId: number = 0;
    ordering: number = 0;
    title: string = '';

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
export interface iAdditionalItemDetails {
    itemID: number;
    duplicates: iItemDuplicatesReadOnly[];
    readonlysource: ReadOnlySource;
}
export interface iItemDuplicatesReadOnly {
    itemId: number;
    shortTitle: string;
    sourceName: string;
}
export class KeyValue {//used in more than one place...
    constructor(k: string, v: string) {
        this.key = k;
        this.value = v;
    }
    key: string;
    value: string;
}

