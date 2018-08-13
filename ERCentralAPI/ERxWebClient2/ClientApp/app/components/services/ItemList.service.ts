import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';

@Injectable({
    providedIn: 'root',
    }
)

export class ItemListService {

    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string
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
    public SaveItems(items: ItemList) {
        this._ItemList = items;
        this.Save();
    }
    
    
    
    public Fetch() {

        return this._httpC.get<Item[]>(this._baseUrl + 'api/WorkAllocationContactList/WorkAllocationContactList');
    }
    public FetchWithCrit(crit: Criteria) {
        //let body = "Username=" + u + "&Password=" + p;
        //return this._httpC.post<ItemList>(this._baseUrl + 'api/Login/Login',
        //    body);
    }
    public FetchWorkAlloc(AllocationId: number, allocSubtype: string, pageSize: number, pageNumber: number) {
        let body = "AllocationId=" + AllocationId + "&ListType=" + allocSubtype
            + "&pageSize=" + pageSize + "&pageNumber=" + pageNumber;
        return this._httpC.post<ItemList>(this._baseUrl + 'api/ItemList/WorkAllocation', body);
    }

    private Save() {
        if (this._ItemList.items.length > 0) {
            localStorage.setItem('ItemsList', JSON.stringify(this._ItemList));
            localStorage.setItem('ItemsListCriteria', JSON.stringify(this._Criteria));
        }
        else if (localStorage.getItem('ItemsList')) {
            localStorage.removeItem('ItemsList');
            localStorage.removeItem('ItemsListCriteria');
        }
    }
}

export class ItemList {
    pagesize: number = 0;
    pageindex: number = 0;
    pagecount: number = 0;
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
    pageSize: number = 0;
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