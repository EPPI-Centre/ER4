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

@Injectable({
    providedIn: 'root',
})

export class ArmsService {

    constructor(
        private _http: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
       
    }

    public arms: arm[] = [];
   
    public Fetch(ItemId: number) {

        let body = JSON.stringify({ Value: ItemId });

        this._http.post<arm[]>(this._baseUrl + 'api/ItemSetList/GetArms',

            body).subscribe(result => {

                console.log('got inside subscription');
                this.arms = result;
                const armsJson = JSON.stringify(this.arms)
                console.log('jsonified: ' + armsJson);

            }, error => { this.modalService.SendBackHomeWithError(error); }
        );

        return this.arms;
    }

       
}

export class arm {

    itemId: number = 0;
    title: string = '';
    itemArmId: number = 0;
}