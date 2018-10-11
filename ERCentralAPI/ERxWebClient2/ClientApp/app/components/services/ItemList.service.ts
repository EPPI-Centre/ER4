import { Component, Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of, Subscription, Subject, BehaviorSubject } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { WorkAllocationContactListService } from './WorkAllocationContactList.service';
import { forEach } from '@angular/router/src/utils/collection';
import { PriorityScreeningService } from './PriorityScreening.service';
import { ModalService } from './modal.service';
import { error } from '@angular/compiler/src/util';

@Injectable({
    providedIn: 'root',
    }
)

export class ItemListService {
    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private _WorkAllocationService: WorkAllocationContactListService,
        private _PriorityScreeningService: PriorityScreeningService,
        private ModalService: ModalService
    ) { }
    private _IsInScreeningMode: boolean | null = null;
    public get IsInScreeningMode(): boolean {
        //return this._IsInScreeningMode;
        if (this._IsInScreeningMode === null) {
            const tIsInScreeningMode = localStorage.getItem('ItemListIsInScreeningMode');
            let iism: boolean | null = tIsInScreeningMode !== null ? JSON.parse(tIsInScreeningMode) : null;
            if (iism === null ) {
                return false;
            }
            else {
                //console.log("Got ItemsList from LS");
                this.IsInScreeningMode = iism;
            }
        }
        if (this._IsInScreeningMode !== null) return this._IsInScreeningMode;
        else return false;
    }
    public set IsInScreeningMode(state: boolean) {
        this._IsInScreeningMode = state;
        this.Save();
    }
    private _ItemList: ItemList = new ItemList();
    private _Criteria: Criteria = new Criteria();
    private _currentItem: Item = new Item();
    @Output() ItemChanged = new EventEmitter();
    public get ItemList(): ItemList {
        if (this._ItemList.items.length == 0) {
            const listJson = localStorage.getItem('ItemsList');
            let list: ItemList = listJson !== null ? JSON.parse(listJson) : new ItemList();
            if (list == undefined || list == null || list.items.length == 0) {
                return this._ItemList;
            }
            else {
                //console.log("Got ItemsList from LS");
                this._ItemList = list;
            }
        }
        return this._ItemList;
    }
    public get ListCriteria(): Criteria {
        if (this._Criteria.listType == "") {
            const critJson = localStorage.getItem('ItemsListCriteria');
            let crit: Criteria = critJson !== null ? JSON.parse(critJson) : new Criteria();
            if (crit == undefined || crit == null) {
                return this._Criteria;
            }
            else {
                //console.log("Got Criteria from LS");
                this._Criteria = crit;
            }
        }
        return this._Criteria;
    }
    public get currentItem(): Item {
        if (this._currentItem) return this._currentItem;
        else {
            const currentItemJson = localStorage.getItem('currentItem');
            this._currentItem = currentItemJson !== null ? JSON.parse(currentItemJson) : new Item();
        }
        return this._currentItem;
    }
    private SaveCurrentItem() {
        if (this._currentItem) {
            localStorage.setItem('currentItem', JSON.stringify(this._currentItem));
        }
        else if (localStorage.getItem('currentItem')) {
            localStorage.removeItem('currentItem');
        }
    }

   
    public SaveItems(items: ItemList, crit: Criteria) {
        //console.log('saving items');
        this._ItemList = items;
        this._Criteria = crit;
        this.Save();
    }
    private ChangingItem(newItem: Item) {
        this._currentItem = newItem;
        this.SaveCurrentItem();
        this.ItemChanged.emit();
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
            if (ff != undefined && ff != null && ff > 0) {
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
            this.ChangingItem(ff);
            return ff;
        }
        else {
            this.ChangingItem(new Item());
            return new Item();
        }
    }
    public getPrevious(itemId: number): Item {
        
        let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
        if (ff != undefined && ff != null && ff > -1 && ff < this._ItemList.items.length) {
            this.ChangingItem(this._ItemList.items[ff - 1]);
            return this._ItemList.items[ff - 1];
        }
        else {
            this.ChangingItem(new Item());
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
      
        let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
        //console.log(ff);
        if (ff != undefined && ff != null && ff > -1 && ff + 1 < this._ItemList.items.length) {
            //console.log('I am emitting');
            this.ChangingItem(this._ItemList.items[ff + 1]);
            return this._ItemList.items[ff + 1];
        }
        else {
            this.ChangingItem(new Item());
            return new Item();
        }
    }
    public getLast(): Item {
        let ff = this.ItemList.items[this._ItemList.items.length - 1];
        if (ff != undefined && ff != null) {
            this.ChangingItem(ff);
            return ff;
        }
        else {
            this.ChangingItem(new Item());
            return new Item();
        }
    }
    public ListDescription: string = "";
    public FetchWithCrit(crit: Criteria, listDescription: string) {
        console.log(crit.listType);
        this._Criteria = crit;
        console.log(this._Criteria.listType);
        this.ListDescription = listDescription;
            this._httpC.post<ItemList>(this._baseUrl + 'api/ItemList/Fetch', crit)
            .subscribe(list => {
                this._Criteria.totalItems = this.ItemList.totalItemCount;
                //console.log('Got item list');
                this.SaveItems(list, this._Criteria);
            }, error => { this.ModalService.GenericError(error);}
            );
    }
    public Refresh() {
        if (this._Criteria && this._Criteria.listType && this._Criteria.listType != "") {
            //we have something to do
            this.FetchWithCrit(this._Criteria, this.ListDescription);
        }
    }
    public FetchNextPage() {
        console.log('np');
        if (this.ItemList.pageindex < this.ItemList.pagecount-1) {
            this._Criteria.pageNumber += 1;
        } else {
        }
        this.FetchWithCrit(this._Criteria, this.ListDescription)
    }
    public FetchPrevPage() {
        //console.log('total items are: ' + this._Criteria.totalItems);
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
    public Save() {
        if (this._ItemList.items.length > 0) {
            localStorage.setItem('ItemsList', JSON.stringify(this._ItemList));
        }
        else if (localStorage.getItem('ItemsList')) {
            localStorage.removeItem('ItemsList');
        }
        if (this._Criteria.listType != "") {
            localStorage.setItem('ItemsListCriteria', JSON.stringify(this._Criteria));
        }
        else if (localStorage.getItem('ItemsListCriteria')) {
            localStorage.removeItem('ItemsListCriteria');
        }
        if (this._IsInScreeningMode !== null) localStorage.setItem('ItemListIsInScreeningMode', JSON.stringify(this._IsInScreeningMode));
        else if (localStorage.getItem('ItemListIsInScreeningMode')) {
            localStorage.removeItem('ItemListIsInScreeningMode');
        }
        this.SaveCurrentItem();
    }

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
    uRL: string = "";
    oldItemId: string = "";
    abstract: string = "";
    comments: string = "";
    typeName: string = "";
    authors: string = "";
    parentAuthors: string = "";
    dOI: string = "";
    keywords: string = "";
    attributeAdditionalText: string = "";
    rank: number = 0;
    isItemDeleted: boolean = false;
    isIncluded: boolean = true;
    isSelected: boolean = false;
    itemStatus: string = "";
    itemStatusTooltip: string = "";
    arms: arm[] = [];
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
export interface arm {
    itemArmId: number;
    itemId: number;
    ordering: number;
    title: string;
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

