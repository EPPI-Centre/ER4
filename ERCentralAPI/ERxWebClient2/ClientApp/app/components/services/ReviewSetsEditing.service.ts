import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { OK } from 'http-status-codes';
import { error } from '@angular/compiler/src/util';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { iSetType, ReviewSetsService, ReviewSet } from './ReviewSets.service';
import { BusyAwareService } from '../helpers/BusyAwareService';

@Injectable({
    providedIn: 'root',
})

export class ReviewSetsEditingService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        private ReviewSetsService: ReviewSetsService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }

    private _SetTypes: iSetType[] = [];
    public get SetTypes(): iSetType[]
    {
        return this._SetTypes;
    }
    public FetchSetTypes() {
        this._BusyMethods.push("FetchSetTypes");
        this._httpC.get<iSetType[]>(this._baseUrl + 'api/Codeset/SetTypes').subscribe(
            (res) => {
                this._SetTypes = res;
            }
            , error => { this.modalService.GenericError(error); }
            , () => {
                this.RemoveBusy("FetchSetTypes");
            }
        );
    }
    public SaveReviewSet(rs: ReviewSet) {
        this._BusyMethods.push("SaveReviewSet");
        let rsC: ReviewSetUpdateCommand = {
            ReviewSetId: rs.reviewSetId,
            SetId: rs.set_id,
            AllowCodingEdits: rs.allowEditingCodeset,
            CodingIsFinal: rs.codingIsFinal,
            SetName: rs.set_name,
            setOrder: rs.order,
            setDescription: rs.description
        }
        //console.log("saving reviewSet via command", rs, rsC);
        this._httpC.post<ReviewSetUpdateCommand>(this._baseUrl + 'api/Codeset/SaveReviewSet', rsC).subscribe(
            data => {

                //this.ItemCodingItemAttributeSaveCommandExecuted.emit(data);
                //this._IsBusy = false;
            }, error => {
                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
            }, () => { this.RemoveBusy("SaveReviewSet");}
        );
    }
    public MoveSetAttribute(attributeSetId: number,
        fromId: number,
        toId: number,
        attributeorder: number) {
        this._BusyMethods.push("MoveSetAttribute");
        let rsC: AttributeSetMoveCommand = {
            FromId: fromId,
            ToId: toId,
            AttributeSetId: attributeSetId,
            attributeOrder: attributeorder
        }
        //console.log("saving reviewSet via command", rs, rsC);
        this._httpC.post<ReviewSetUpdateCommand>(this._baseUrl + 'api/Codeset/AttributeSetMove', rsC).subscribe(
            data => {
                this.RemoveBusy("MoveSetAttribute");
                //this.ItemCodingItemAttributeSaveCommandExecuted.emit(data);
                //this._IsBusy = false;
            }, error => {
                this.RemoveBusy("MoveSetAttribute");
                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
            }, () => {
                
            }
        );
    }
}
export interface ReviewSetUpdateCommand
    //(int reviewSetId, int setId, bool allowCodingEdits, bool codingIsFinal, string setName, int SetOrder, string setDescription)
{
    ReviewSetId: number;
    SetId: number;
    AllowCodingEdits: boolean;
    CodingIsFinal: boolean;
    SetName: string;
    setOrder: number;
    setDescription: string;
}

export interface AttributeSetMoveCommand {
    FromId: number;
    ToId: number;
    AttributeSetId: number;
    attributeOrder: number;
}