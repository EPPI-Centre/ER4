import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ItemCodingComp } from '../coding/coding.component';



@Injectable({
    providedIn: 'root',
})

export class ItemCodingService {

    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string
        ) { }


    private _ItemCodingList: ItemSet[] = [];

    public get ItemCodingList(): ItemSet[] {
        if (this._ItemCodingList.length == 0) {

            const ItemSetsJson = localStorage.getItem('ItemCodingList');
            let ReadOnlyReviews: ItemSet[] = ItemSetsJson !== null ? JSON.parse(ItemSetsJson) : [];
            if (ReadOnlyReviews == undefined || ReadOnlyReviews == null || ReadOnlyReviews.length == 0) {
                return this._ItemCodingList;
            }
            else {
                console.log("Got ItemSets from LS");
                this._ItemCodingList = ReadOnlyReviews;
            }
        }
        return this._ItemCodingList;

    }
    
    public set ItemCodingList(icl: ItemSet[]) {
        this._ItemCodingList = icl;
        this.Save();
    }
        
    public Fetch(ItemId: number) {
        let body = JSON.stringify({ Value: ItemId });
        return this._httpC.post<ItemSet[]>(this._baseUrl + 'api/ItemSetList/Fetch',
            body);
    }

    public Save() {
        if (this._ItemCodingList.length > 0)
            localStorage.setItem('ItemCodingList', JSON.stringify(this._ItemCodingList));
        else if (localStorage.getItem('ItemCodingList'))//to be confirmed!! 
            localStorage.removeItem('ItemCodingList');
    }
}

export class ItemSet {
    itemSetId: number = 0;
    setId: number = 0;
    itemId: number = 0;
    contactId: number = 0;
    contactName: string = "";
    setName: string = "";
    isCompleted: boolean = false;
    isLocked: boolean = true;
    itemAttributesList: ReadOnlyItemAttribute[] = [];
    isSelected: boolean = false;
    outcomeItemList: Outcome[] = [];
}
export class ReadOnlyItemAttribute {
    attributeId: number = 0;
    additionalText: string = "";
    armId: number = 0;
    armTitle: string = "";
    itemAttributeFullTextDetails: ItemAttributeFullTextDetails[] = [];
}
export class Outcome {

}
export class ItemAttributeFullTextDetails {

}