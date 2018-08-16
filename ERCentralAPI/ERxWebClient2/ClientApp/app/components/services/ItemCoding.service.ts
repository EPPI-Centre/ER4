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
    outcomeItemList: OutcomeItemList = new OutcomeItemList();
}
export class ReadOnlyItemAttribute {
    attributeId: number = 0;
    additionalText: string = "";
    armId: number = 0;
    armTitle: string = "";
    itemAttributeFullTextDetails: ItemAttributeFullTextDetails[] = [];
}
export class OutcomeItemList {
    outcomesList: Outcome[] = [];
}
export class Outcome {
    outcomeId: number = 0;
    itemSetId: number = 0;
    outcomeTypeName: string = "";
    itemAttributeIdIntervention: number = 0;
    itemAttributeIdControl: number = 0;
    itemAttributeIdOutcome: number = 0;
    title: string = "";
    shortTitle: string = "";
    outcomeDescription: string = "";
    data1: number = 0;
    data2: number = 0;
    data3: number = 0;
    data4: number = 0;
    data5: number = 0;
    data6: number = 0;
    data7: number = 0;
    data8: number = 0;
    data9: number = 0;
    data10: number = 0;
    data11: number = 0;
    data12: number = 0;
    data13: number = 0;
    data14: number = 0;
    interventionText: string = "";
    controlText: string = "";
    outcomeText: string = "";
    feWeight: number = 0;
    reWeight: number = 0;
    smd: number = 0;
    sesmd: number = 0;
    r: number = 0;
    ser: number = 0;
    oddsRatio: number = 0;
    seOddsRatio: number = 0;
    riskRatio: number = 0;
    seRiskRatio: number = 0;
    ciUpperSMD: number = 0;
    ciLowerSMD: number = 0;
    ciUpperR: number = 0;
    ciLowerR: number = 0;
    ciUpperOddsRatio: number = 0;
    ciLowerOddsRatio: number = 0;
    ciUpperRiskRatio: number = 0;
    ciLowerRiskRatio: number = 0;
    ciUpperRiskDifference: number = 0;
    ciLowerRiskDifference: number = 0;
    ciUpperPetoOddsRatio: number = 0;
    ciLowerPetoOddsRatio: number = 0;
    ciUpperMeanDifference: number = 0;
    ciLowerMeanDifference: number = 0;
    riskDifference: number = 0;
    seRiskDifference: number = 0;
    meanDifference: number = 0;
    seMeanDifference: number = 0;
    petoOR: number = 0;
    sePetoOR: number = 0;
    es: number = 0;
    sees: number = 0;
    nRows: number = 0;
    ciLower: number = 0;
    ciUpper: number = 0;
    esDesc: string = "";
    seDesc: string = "";
    data1Desc: string = "";
    data2Desc: string = "";
    data3Desc: string = "";
    data4Desc: string = "";
    data5Desc: string = "";
    data6Desc: string = "";
    data7Desc: string = "";
    data8Desc: string = "";
    data9Desc: string = "";
    data10Desc: string = "";
    data11Desc: string = "";
    data12Desc: string = "";
    data13Desc: string = "";
    data14Desc: string = "";
}
export class ItemAttributeFullTextDetails {

}