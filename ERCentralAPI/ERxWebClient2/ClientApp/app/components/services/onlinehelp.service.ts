import { Component, Inject, Injectable, EventEmitter, Output } from '@angular/core';
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
import { arm, Item, ItemListService } from './ItemList.service';
import { BusyAwareService } from '../helpers/BusyAwareService';

@Injectable({
    providedIn: 'root',
})

export class OnlineHelpService extends BusyAwareService  {

    constructor(
        private _http: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        private ItemListService: ItemListService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
    private _CurrentHTMLHelp: string = ""
    public get CurrentHTMLHelp(): string {
        if (this.IsBusy) return "";
        else return this._CurrentHTMLHelp;
    }
    private _CurrentContext: string = ""
    public get CurrentContext(): string {
        return this._CurrentContext;
    }
    public FetchHelpContent(context: string) {
        if (this._CurrentContext == context) return; //no need to re-fetch the help we have already.
        else {
            this._BusyMethods.push("FetchHelpContent");
            let body = { Value: context };
            
            this._http.post<OnlineHelpContent>(this._baseUrl + 'api/Help/FetchHelpContent',
                body)
                .subscribe(result => {
                    //console.log("gethelp:", body, result);
                    this._CurrentContext = result.context;
                    this._CurrentHTMLHelp = result.helpHTML;
                    this.RemoveBusy("FetchHelpContent");
                },
                    (error) => {
                        console.log("FetchHelpContent error:", error);
                        this.RemoveBusy("FetchHelpContent");
                        this.modalService.GenericError(error);
                    }
                );
        }
    }
    public CreateFeedbackMessage(message: FeedbackAndClientError4Create) {

        this._BusyMethods.push("CreateFeedbackMessage");

        this._http.post<FeedbackAndClientError>(this._baseUrl + 'api/Help/CreateFeedbackMessage', message)
            .subscribe(result => {
                //console.log("gethelp:", body, result);
                //we could check if this worked, by looking at the messageId in result...
                this.RemoveBusy("CreateFeedbackMessage");
            },
                (error) => {
                    console.log("CreateFeedbackMessage error:", error);
                    this.RemoveBusy("CreateFeedbackMessage");
                    this.modalService.GenericError(error);
                }
            );
    }
    
}
export interface OnlineHelpContent{
    onlineHelpContentId: number;
    context: string;
    helpHTML: string;
}

export class FeedbackAndClientError4Create {
    public contactId: number = -1;
    public context: string ="";
    public isError: boolean = false;
    public message: string = "";
}
export class FeedbackAndClientError extends FeedbackAndClientError4Create {
    public messageId: number = -1;
    public contactName: string = "";
    public dateCreated: string = "";
}