import { Component, Inject, Injectable, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ItemCodingComp } from '../coding/coding.component';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Subject } from 'rxjs';
import { ModalService } from './modal.service';

@Injectable({
    providedIn: 'root',
})

export class ItemCodingService  {
    @Output() DataChanged = new EventEmitter();
    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private modalService: ModalService,
        private ReviewerIdentityService: ReviewerIdentityService 
        ) { }


    private _ItemCodingList: ItemSet[] = [];
    //public itemID = new Subject<number>();


    public get ItemCodingList(): ItemSet[] {
        if (this._ItemCodingList.length == 0) {
            const ItemSetsJson = localStorage.getItem('ItemCodingList');
            let ReadOnlyReviews: ItemSet[] = ItemSetsJson !== null ? JSON.parse(ItemSetsJson) : [];
            if (ReadOnlyReviews == undefined || ReadOnlyReviews == null || ReadOnlyReviews.length == 0) {
                return this._ItemCodingList;
            }
            else {
                //not sure we should do anything here
            }
        }
        return this._ItemCodingList;
    }
    
    public set ItemCodingList(icl: ItemSet[]) {
        this._ItemCodingList = icl;
        this.Save();
    }
        
    public Fetch(ItemId: number) {

        //this.itemID.next(ItemId); 

        let body = JSON.stringify({ Value: ItemId });
        this._httpC.post<ItemSet[]>(this._baseUrl + 'api/ItemSetList/Fetch',
            body).subscribe(result => {
                this.ItemCodingList = result;
                this.DataChanged.emit();
                //this.ReviewSetsService.AddItemData(result);
                //this.Save();
            }, error => { this.modalService.SendBackHomeWithError(error); }
            );
    }

    public Save() {
        //nope! We're not saving this to localstorage:
        //item coding comes and goes (as we change items), so best not to keep it, if needed we'll re-fetch it.

        //if (this._ItemCodingList.length > 0)
        //    localStorage.setItem('ItemCodingList', JSON.stringify(this._ItemCodingList));
        //else if (localStorage.getItem('ItemCodingList'))//to be confirmed!! 
        //    localStorage.removeItem('ItemCodingList');
    }
    public FindItemSetBySetId(DestSetId: number): ItemSet | null {
        //this is where somewhat complicated logic needs to happen. We need to replicate here the logic that decides if a new itemset is needed or not...
        let result: ItemSet | null = null;
        for (let itemSet of this._ItemCodingList) {
            if (itemSet.setId == DestSetId) {
                //we have an itemSet in the desired set: if complete, we'll use it. Otherwise, check that it belongs to current user.
                //if itemset to be used is locked, we should not even have tried, so tricky case...
                if (itemSet.isCompleted) {
                    if (itemSet.isLocked) {
                        alert('Coding is locked! We shouldn\'t be doing this...');
                        throw new Error('Coding is locked! We shouldn\'t be doing this...');
                    }
                    result = itemSet;
                    break;
                }
                else if (itemSet.contactId == this.ReviewerIdentityService.reviewerIdentity.userId) {
                    if (itemSet.isLocked) {
                        alert('Coding is locked! We shouldn\'t be doing this...');
                        throw new Error('Coding is locked! We shouldn\'t be doing this...');
                    }
                    result = itemSet;
                    break;
                }
            }
        }
        return result;
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
    itemAttributeId: number = 0;
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