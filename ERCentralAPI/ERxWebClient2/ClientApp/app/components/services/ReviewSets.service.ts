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
        else if (localStorage.getItem('ReviewSets'))//to be confirmed!! 
            localStorage.removeItem('ReviewSets');
    }

}
export interface singleNode {
    id: number;
    name: string;
    children: singleNode[];
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
}