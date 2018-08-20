import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { WorkAllocationContactListService } from './WorkAllocationContactList.service';
import { forEach } from '@angular/router/src/utils/collection';

@Injectable({
    providedIn: 'root',
    }
)

export class ItemListService {

    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private _WorkAllocationService: WorkAllocationContactListService
        ) { }

    private _ItemList: ItemList = new ItemList();
    private _Criteria: Criteria = new Criteria();
    public get ItemList(): ItemList {
        if (this._ItemList.items.length == 0) {

            const listJson = localStorage.getItem('ItemsList');
            let list: ItemList = listJson !== null ? JSON.parse(listJson) : new ItemList();
            if (list == undefined || list == null || list.items.length == 0) {
                return this._ItemList;
            }
            else {
                console.log("Got ItemsList from LS");
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
                console.log("Got Criteria from LS");
                this._Criteria = crit;
            }
        }
        return this._Criteria;
    }


    public SaveItems(items: ItemList, crit: Criteria) {
        this._ItemList = items;
        this._Criteria = crit;
        this.Save();
    }
    public getItem(itemId: number): Item {
        let ff = this.ItemList.items.find(found => found.itemId == itemId);
        if (ff != undefined && ff != null) return ff;
        return new Item();
    }
    public getPrevious(itemId: number): Item {
        let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
        if (ff != undefined && ff != null && ff > -1 && ff < this._ItemList.items.length) return this._ItemList.items[ff-1];
        return new Item();
    }
    public getNext(itemId: number): Item {
        let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
        if (ff != undefined && ff != null && ff > -1 && ff + 1 < this._ItemList.items.length) return this._ItemList.items[ff + 1];
        return new Item();
    }
    
    public FetchWithCrit(crit: Criteria) {
        this._Criteria = crit;
        this._httpC.post<ItemList>(this._baseUrl + 'api/ItemList/Fetch', crit)
            .subscribe(list => {

                this.SaveItems(list, this._Criteria);
            });
    }
    public FetchNextPage() {
        if (this.ItemList.pageindex < this.ItemList.pagecount-1) {
            this._Criteria.pageNumber += 1;
        } else {
        }
        this.FetchWithCrit(this._Criteria)
    }
    public FetchPrevPage() {
        //console.log('total items are: ' + this._Criteria.totalItems);
        if (this.ItemList.pageindex == 0 ) {
            return this.FetchWithCrit(this._Criteria);
        } else {
            this._Criteria.pageNumber -= 1;
            return this.FetchWithCrit(this._Criteria);
        }
    }
    public FetchLastPage() {
        this._Criteria.pageNumber = this.ItemList.pagecount - 1;
        return this.FetchWithCrit(this._Criteria);
    }
    public FetchFirstPage() {
        this._Criteria.pageNumber = 0;
        return this.FetchWithCrit(this._Criteria);
    }
    public FetchParticularPage(pageNum: number) {
        this._Criteria.pageNumber = pageNum;
        return this.FetchWithCrit(this._Criteria);
    }
    public FetchWorkAlloc(AllocationId: number, allocSubtype: string, pageSize: number, pageNumber: number) {
        let body = "AllocationId=" + AllocationId + "&ListType=" + allocSubtype
            + "&pageSize=" + pageSize + "&pageNumber=" + pageNumber;
        return this._httpC.post<ItemList>(this._baseUrl + 'api/ItemList/WorkAllocation', body);
    }

    private Save() {
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
    arms: Arm[] = [];
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
export class Arm {
    itemArmId: number = 0;
    itemId: number = 0;
    ordering: number = 0;
    title: string = "";
}