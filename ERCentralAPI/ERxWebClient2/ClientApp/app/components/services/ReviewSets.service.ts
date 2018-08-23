import { Component, Inject, Injectable, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { ItemSet } from './ItemCoding.service';


//see: https://stackoverflow.com/questions/34031448/typescript-typeerror-myclass-myfunction-is-not-a-function
//JSON object is consumed via interfaces, but we immediately digest those and put them into actual classes.
//this is NECESSARY to consume the CSLA (server side) objects as produced by the API in a way that makes 
//then suitable for the angular-tree-component used to show codetrees to users.
@Injectable({
    providedIn: 'root',
})

export class ReviewSetsService {
    constructor(private router: Router, //private _http: Http, 
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string) { }

    private _ReviewSets: ReviewSet[] = [];
    GetReviewSets() {
        this._httpC.get<iReviewSet[]>(this._baseUrl + 'api/Codeset/CodesetsByReview').subscribe(
            data => {
                this.ReviewSets = ReviewSetsService.digestJSONarray(data);
            }
        );
    }
    public get ReviewSets(): ReviewSet[] {
        if (this._ReviewSets.length == 0) {

            //this.GetReviewSets();

            const ReviewSetsJson = localStorage.getItem('ReviewSets');
            let ReviewSets: ReviewSet[] = ReviewSetsJson !== null ? ReviewSetsService.digestLocalJSONarray(JSON.parse(ReviewSetsJson)) : [];
            if (ReviewSets == undefined || ReviewSets == null || ReviewSets.length == 0) {
                let first: ReviewSet = ReviewSets[0];
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
        if (this._ReviewSets != undefined && this._ReviewSets != null && this._ReviewSets.length > 0) //{ }
            localStorage.setItem('ReviewSets', JSON.stringify(this._ReviewSets));
        else if (localStorage.getItem('ReviewSets')) localStorage.removeItem('ReviewSets');
    }
    public static digestJSONarray(data: iReviewSet[]): ReviewSet[] {
        let result: ReviewSet[] = [];
        console.log('digest JSON');
        for (let iItemset of data) {
            //console.log('+');
            let newSet: ReviewSet = new ReviewSet();
            newSet.set_id = iItemset.setId;
            newSet.set_name = iItemset.setName;
            newSet.order = iItemset.setOrder;
            newSet.codingIsFinal = iItemset.codingIsFinal;
            newSet.setType = iItemset.setType;
            newSet.attributes = ReviewSetsService.childrenFromJSONarray(iItemset.attributes.attributesList);
            result.push(newSet);
        }
        return result;
    }
    public static digestLocalJSONarray(data: any[]): ReviewSet[] {
        let result: ReviewSet[] = [];
        console.log('digest local JSON');
        for (let iItemset of data) {
            //console.log('+');
            let newSet: ReviewSet = new ReviewSet();
            newSet.set_id = iItemset.set_id;
            newSet.set_name = iItemset.set_name;
            newSet.order = iItemset.order;
            newSet.codingIsFinal = iItemset.codingIsFinal;
            newSet.setType = iItemset.setType;
            newSet.attributes = ReviewSetsService.childrenFromLocalJSONarray(iItemset.attributes);
            result.push(newSet);
        }
        return result;
    }
    public static childrenFromJSONarray(data: iAttributeSet[]): SetAttribute[] {
        let result: SetAttribute[] = [];
        for (let iAtt of data) {
            //console.log('.');
            let newAtt: SetAttribute = new SetAttribute();
            newAtt.attribute_id = iAtt.attributeId;
            newAtt.attribute_name = iAtt.attributeName;
            newAtt.order = iAtt.attributeOrder;
            newAtt.attribute_type = iAtt.attributeType;
            newAtt.attribute_type_id = iAtt.AttributeTypeId;
            newAtt.attribute_set_desc = iAtt.attributeSetDescription;
            newAtt.attribute_desc = iAtt.attributeDescription;
            //console.log(newAtt.isSelected);
            newAtt.attributes = ReviewSetsService.childrenFromJSONarray(iAtt.attributes.attributesList);
            result.push(newAtt);
        }
        return result;
    }
    public static childrenFromLocalJSONarray(data: any[]): SetAttribute[] {
        let result: SetAttribute[] = [];
        if (data && data != undefined) {
            
            for (let iAtt of data) {
                //console.log('.');
                let newAtt: SetAttribute = new SetAttribute();
                newAtt.attribute_id = iAtt.attribute_id;
                newAtt.attribute_name = iAtt.attribute_name;
                newAtt.order = iAtt.order;
                newAtt.attribute_type = iAtt.attribute_type;
                newAtt.attribute_type_id = iAtt.attribute_type_id;
                newAtt.attribute_set_desc = iAtt.attribute_set_desc;
                newAtt.attribute_desc = iAtt.attribute_desc;
                //console.log(newAtt.isSelected);
                if (iAtt.attributes) newAtt.attributes = ReviewSetsService.childrenFromLocalJSONarray(iAtt.attributes);
                else newAtt.attributes = []; 
                result.push(newAtt);
            }
        }
        return result;
    }
    public AddItemData(ItemCodingList: ItemSet[]) {
        for (let itemset of ItemCodingList) {
            let destSet = this._ReviewSets.find(d => d.set_id == itemset.setId );
            if (destSet) {
                for (let itemAttribute of itemset.itemAttributesList) {
                    //console.log('.' + destSet.set_name);
                    if (destSet.attributes) {
                        let dest = this.FindAttributeById(destSet.attributes, itemAttribute.attributeId);
                        //console.log('.');
                        if (dest) {
                            dest.isSelected = true;
                            //console.log("found destination attr, id: " + itemAttribute.attributeId + "name: " + dest.attribute_name);
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
            else if (candidate.attributes) {
                result = this.FindAttributeById(candidate.attributes, AttributeId);
            }
        }
        return result;
    }
    public clearItemData() {
        for (let set of this._ReviewSets) {
            this.clearItemDataInChildren(set.attributes);
        }
    }
    private clearItemDataInChildren(children: SetAttribute[]) {
        for (let att of children) {
            if (att.isSelected) att.isSelected = false;
            if (att.attributes && att.attributes.length > 0) this.clearItemDataInChildren(att.attributes);
        }
    }
}

export interface singleNode {
    id: string;
    name: string;
    attributes: singleNode[];
    showCheckBox: boolean;
    nodeType: string;
    isSelected: boolean;
    additionalText: string;
    armId: number;
    armTitle: string;
    order: number;
    
}

export class ReviewSet implements singleNode {
    set_id: number = -1;
    public get id(): string { return "C_" + this.set_id; }
    set_name: string = "";
    public get name(): string { return this.set_name; }
    set_order: number = -1;
    attributes: SetAttribute[] = [];
    showCheckBox: boolean = false;
   
    setType: iSetType | null = null ;
    nodeType: string = "ReviewSet";
    order: number = 0;
    codingIsFinal: boolean = true;

    
    isSelected: boolean = false;
    additionalText: string = "";
    armId: number = 0;
    armTitle: string = "";

}
export class SetAttribute implements singleNode {
    attribute_id: number = -1;
    public get id(): string { return "A" + this.attribute_id; };
    attribute_name: string = "";
    public get name(): string { return this.attribute_name; };
    attribute_order: number = -1;
    attribute_type: string = "";
    attribute_set_desc: string = "";
    attribute_desc: string = "";
    public get showCheckBox(): boolean {
        if (this.attribute_type == 'Not selectable (no checkbox)') return false;
        else return true;
    }
    parent_attribute_id: number = -1;;
    attribute_type_id: number = -1;;
    attributes: SetAttribute[] = [];
    
    
    nodeType: string = "SetAttribute";
    //private _isSelected: boolean = false;
    //public get isSelected(): boolean {
    //    console.log(this._isSelected); 
    //    return this._isSelected;
    //}
    //public set isSelected(val: boolean) {
    //    console.log('setting is selected: [' + this._isSelected + '] to ' + val);
    //    this._isSelected = val;
    //}
    isSelected: boolean = false; 
    additionalText: string = "";
    armId: number = 0;
    armTitle: string = "";
    order: number = 0;
}

export interface iReviewSet {
    reviewSetId: number;
    setId: number;
    setType: iSetType,
    setName: string;
    setDescription: string;
    setOrder: number;
    codingIsFinal: boolean;
    attributes: iAttributesList;
}
export interface iAttributesList
{  
    attributesList: iAttributeSet[];
}
export interface iAttributeSet {
    attributeSetId: number;
    attributeId: number;
    attributeSetDescription: string;
    attributeType: string;
    AttributeTypeId: number;
    attributeName: string;
    attributeDescription: string;
    attributes: iAttributesList;
    isSelected: boolean;
    attributeOrder: number;
}
export interface iSetType {
    setTypeName: string;
    setTypeDescription: string;
}




