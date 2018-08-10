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

    private _Items: Item[] = [];
    private _Criteria: Criteria = new Criteria();
    public get Items(): Item[] {
        if (this._Items.length == 0) {

            const workAllocationsJson = localStorage.getItem('ItemsList');
            let workAllocations: Item[] = workAllocationsJson !== null ? JSON.parse(workAllocationsJson) : [];
            if (workAllocations == undefined || workAllocations == null || workAllocations.length == 0) {
                return this._Items;
            }
            else {
                console.log("Got workAllocations from LS");
                this._Items = workAllocations;
            }
        }
        return this._Items;

    }


    public SaveItems(items: Item[], criteria: Criteria) {
        this._Items = items;
        this._Criteria = criteria;
        this.Save();
    }
    
    
    public Fetch() {

        return this._httpC.get<Item[]>(this._baseUrl + 'api/WorkAllocationContactList/WorkAllocationContactList');
    }

    private Save() {
        if (this._Items.length > 0) {
            localStorage.setItem('ItemsList', JSON.stringify(this._Items));
            localStorage.setItem('ItemsListCriteria', JSON.stringify(this._Criteria));
        }
        else if (localStorage.getItem('ItemsList')) {
            localStorage.removeItem('ItemsList');
            localStorage.removeItem('ItemsListCriteria');
        }
    }
}

export class ItemList {
    workAllocationId: number = 0;
    contactName: string = "";
    contactId: string = "";
    setName: string = "";
    setId: number = 0;
    attributeName: string = "";
    attributeId: number = 0;
    totalAllocation: number = 0;
    totalStarted: number = 0;
    totalRemaining: number = 0;
}


export class Item {
    workAllocationId: number = 0;
    contactName: string = "";
    contactId: string = "";
    setName: string = "";
    setId: number = 0;
    attributeName: string = "";
    attributeId: number = 0;
    totalAllocation: number = 0;
    totalStarted: number = 0;
    totalRemaining: number = 0;
}
export class Criteria {

}
