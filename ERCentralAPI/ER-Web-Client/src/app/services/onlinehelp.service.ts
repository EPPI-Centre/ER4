import { Component, Inject, Injectable, EventEmitter, Output, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { ItemListService } from './ItemList.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { EventEmitterService } from './EventEmitter.service';
import { Subscription } from 'rxjs';
import { ConfigService } from './config.service';

@Injectable({
    providedIn: 'root',
})

export class OnlineHelpService extends BusyAwareService implements OnDestroy {

    constructor(
        private _http: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        private ItemListService: ItemListService,
        private EventEmitterService: EventEmitterService,
        private _httpC: HttpClient,
        configService: ConfigService
    ) {
      super(configService);
        //console.log("On create OnlineHelpService");
        this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
    }
    ngOnDestroy() {
        console.log("Destroy search service");
        if (this.clearSub != null) this.clearSub.unsubscribe();
    }
    private clearSub: Subscription | null = null;
    private _CurrentHTMLHelp: string = "";
    public get CurrentHTMLHelp(): string {
        if (this.IsBusy) return "";
        else return this._CurrentHTMLHelp;
    }
    private _CurrentContext: string = ""
    public get CurrentContext(): string {
        return this._CurrentContext;
    }
    private _FeedbackMessageList: FeedbackAndClientError[] = [];
    public get FeedbackMessageList(): FeedbackAndClientError[] {
        return this._FeedbackMessageList;
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

  public UpdateHelpContent(message: OnlineHelpContent1) {

    this._BusyMethods.push("UpdateHelpContent");

    this._http.post<OnlineHelpContent>(this._baseUrl + 'api/Help/UpdateHelpcontent', message)
        .subscribe(result => {
          this.RemoveBusy("UpdateHelpContent");
        },
          (error) => {
            console.log("UpdateHelpContent error:", error);
            this.RemoveBusy("UpdateHelpContent");
            this.modalService.GenericError(error);
          }
        );
    }



  private _OnlineHelpPages: ReadOnlyHelpPage[] = [];
  public get HelpPages(): ReadOnlyHelpPage[] {
    return this._OnlineHelpPages;
  }

  public FetchHelpPageList() {
    this._BusyMethods.push("FetchHelpPageList");
    return this._httpC.get<ReadOnlyHelpPageList>(this._baseUrl + 'api/Help/GetHelpPageList').subscribe(result => {
      this._OnlineHelpPages = result.helpPages;
      this.RemoveBusy("FetchHelpPageList");
    }, error => {
      this.RemoveBusy("FetchHelpPageList");
      this.modalService.GenericError(error);
    }
    );
  }
/*
  public FetchSources() {
    this._BusyMethods.push("FetchSources");
    return this._httpC.get<ReadOnlySourcesList>(this._baseUrl + 'api/Sources/GetSources').subscribe(result => {
      this._ReviewSources = result.sources;
      this._SomeSourceIsBeingDeleted = result.someSourceIsBeingDeleted;
      this.RemoveBusy("FetchSources");
    }, error => {
      this.RemoveBusy("FetchSources");
      this.modalService.GenericError(error);
    }
    );
  }
*/
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
    public GetFeedbackMessageList() {
        //console.log("GetFeedbackMessageList");
        this._BusyMethods.push("GetFeedbackMessageList");
        this._http.get<FeedbackAndClientError[]>(this._baseUrl + 'api/Help/FeedbackMessageList').subscribe(
            data => {
                this._FeedbackMessageList = data;
                this.RemoveBusy("GetFeedbackMessageList");
            },
            error => {
                this.modalService.GenericError(error);
                console.log("GetFeedbackMessageList", error);
                this.RemoveBusy("GetFeedbackMessageList");
            }
        );
    }
    public Clear() {
        this._FeedbackMessageList = [];
    }
}
export interface OnlineHelpContent{
    context: string;
    helpHTML: string;
}

export class OnlineHelpContent1 {
  public context: string = "";
  public helpHTML: string = "";
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
    public contactEmail: string = "";
    public dateCreated: string = "";
    public reviewId: number = 0;
}

export interface ReadOnlyHelpPage {
  isSelected: boolean;
  helpPage_ID: number;
  context_Name: string;
}

export interface ReadOnlyHelpPageList {
  helpPages: ReadOnlyHelpPage[];
}


