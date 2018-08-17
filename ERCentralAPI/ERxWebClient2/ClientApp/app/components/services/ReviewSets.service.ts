import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { ItemSet } from './ItemCoding.service';


//EVERYTHING here is wrong! see: https://stackoverflow.com/questions/34031448/typescript-typeerror-myclass-myfunction-is-not-a-function
@Injectable({
    providedIn: 'root',
})

export class ReviewSetsService {
    constructor(private router: Router, //private _http: Http, 
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string) { }

    private _ReviewSets: ReviewSet[] = [];
    GetReviewSets() {
        return this._httpC.get<ReviewSet[]>(this._baseUrl + 'api/Codeset/CodesetsByReview');
    }
    public get ReviewSets(): ReviewSet[] {
        if (this._ReviewSets.length == 0) {

            const ReviewSetsJson = localStorage.getItem('ReviewSets');
            let ReviewSets: ReviewSet[] = ReviewSetsJson !== null ? JSON.parse(ReviewSetsJson) : [];
            if (ReviewSets == undefined || ReviewSets == null || ReviewSets.length == 0) {
                return this._ReviewSets;
            }
            else {
                console.log("Got ReviewSets from LS");
                this._ReviewSets = ReviewSets;
            }
        }
        return this._ReviewSets;
    }
    public set ReviewSets(sets: ReviewSet[]) {
        this._ReviewSets = sets;
        this.Save();
    }
    private Save() {
        if (this._ReviewSets != undefined && this._ReviewSets != null && this._ReviewSets.length > 0)
            localStorage.setItem('ReviewSets', JSON.stringify(this._ReviewSets));
        else if (localStorage.getItem('ReviewSets')) localStorage.removeItem('ReviewSets');
    }

    public AddItemData(ItemCodingList: ItemSet[]) {
        for (let itemset of ItemCodingList) {
            let destSet = this._ReviewSets.find(d => d.id == itemset.setId );
            if (destSet) {
                for (let itemAttribute of itemset.itemAttributesList) {
                    console.log('.' + destSet.name);
                    if (destSet.children) {
                        let dest = this.FindAttributeById(destSet.children, itemAttribute.attributeId);
                        console.log('.');
                        if (dest) {
                            dest.isSelected = true;
                            console.log("found destination attr, id: " + itemAttribute.attributeId + "name: " + dest.name);
                        }
                    }
                }
            }
        }
    }
    private FindAttributeById(list: SetAttribute[], AttributeId: number): SetAttribute | null {
        let result: SetAttribute | null = null;
        for (let candidate of list) {
            if (AttributeId == candidate.attribute_id) {
                result = candidate;
                break;
            }
            else if (candidate.children) {
                result = this.FindAttributeById(candidate.children, AttributeId);
            }
        }
        return result;
    }
}

export interface singleNode {
    id: number;
    name: string;
    children: singleNode[];

    nodeType: string;
    isSelected: boolean;
    additionalText: string;
    armId: number;
    armTitle: string;
    
}

export class ReviewSet implements singleNode {
    set_id: number = -1;
    id: number = this.set_id;
    set_name: string = "";
    name: string = this.set_name;
    set_type: string = "";
    set_order: number = -1;
    attributes: SetAttribute[] = [];
    ShowCheckBox: boolean = false;
    children: SetAttribute[] = this.attributes;

    nodeType: string = "ReviewSet";

    
    isSelected: boolean = false;
    additionalText: string = "";
    armId: number = 0;
    armTitle: string = "";

    public FindAttributeById(AttributeId: number): SetAttribute | null {
        let result: SetAttribute | null = null;
        for (let candidate of this.attributes) {
            if (AttributeId == candidate.attribute_id) {
                result = candidate;
                break;
            }
            else {
                result = this.FindAttributeById(AttributeId);
            }
        }
        return result;
    }
}
export class SetAttribute implements singleNode {
    attribute_id: number = -1;
    id: number = this.attribute_id;
    attribute_name: string = "";
    name: string = this.attribute_name;
    attribute_order: number = -1;;
    attribute_type: string = "";
    attribute_set_desc: string = "";
    attribute_desc: string = "";
    showCheckBox: boolean = false;
    parent_attribute_id: number = -1;;
    attribute_type_id: number = -1;;
    attributes: SetAttribute[] = [];
    children: SetAttribute[] = this.attributes;

    nodeType: string = "SetAttribute";

    
    isSelected: boolean = false;
    additionalText: string = "";
    armId: number = 0;
    armTitle: string = "";

    public FindAttributeById(AttributeId: number): SetAttribute | null {
        let result: SetAttribute | null = null;
        for (let candidate of this.attributes) {
            if (AttributeId == candidate.attribute_id) {
                result = candidate;
                break;
            }
            else {
                result = this.FindAttributeById(AttributeId);
            }
        }
        return result;
    }

}