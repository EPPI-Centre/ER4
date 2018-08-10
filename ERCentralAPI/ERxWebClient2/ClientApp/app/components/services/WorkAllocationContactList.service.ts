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

export class WorkAllocationContactListService {

    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string
        ) { }

    private _workAllocations: WorkAllocation[] = [];

    public get workAllocations(): WorkAllocation[] {
        if (this._workAllocations.length == 0) {

            const workAllocationsJson = localStorage.getItem('WorkAllocationContactList');
            let workAllocations: WorkAllocation[] = workAllocationsJson !== null ? JSON.parse(workAllocationsJson) : [];
            if (workAllocations == undefined || workAllocations == null || workAllocations.length == 0) {
                return this._workAllocations;
            }
            else {
                console.log("Got workAllocations from LS");
                this._workAllocations = workAllocations;
            }
        }
        return this._workAllocations;

    }


    public set workAllocations(wa: WorkAllocation[]) {
        this._workAllocations = wa;
        this.Save();
    }
    
    
    public Fetch() {
        return this._httpC.get<WorkAllocation[]>(this._baseUrl + 'api/WorkAllocationContactList/WorkAllocationContactList');
    }

    public Save() {
        if (this._workAllocations.length > 0)
            localStorage.setItem('WorkAllocationContactList', JSON.stringify(this._workAllocations));
        else if (localStorage.getItem('WorkAllocationContactList'))//to be confirmed!! 
            localStorage.removeItem('WorkAllocationContactList');
    }
}
export class WorkAllocation {
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
