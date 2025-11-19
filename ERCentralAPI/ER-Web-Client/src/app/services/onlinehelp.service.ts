import { Component, Inject, Injectable, EventEmitter, Output, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { ItemListService } from './ItemList.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { EventEmitterService } from './EventEmitter.service';
import { lastValueFrom, Subscription } from 'rxjs';
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
  public ShowHelpDropDown: boolean = false;
  private clearSub: Subscription | null = null;
  private _CurrentHelp: OnlineHelpContent = new OnlineHelpContent();
  public get CurrentHelp(): OnlineHelpContent {
    return this._CurrentHelp;
  }
  public get CurrentHTMLHelp(): string {
    if (this.IsBusy) return "";
    else return this._CurrentHelp.helpHTML;
  }
  public get CurrentContext(): string {
    if (this._CurrentHelp.IsExtension) {
      return this._CurrentHelp.parentContext;
    }
    else {
      //return this._CurrentHelp.context;
      if (this.ShowHelpDropDown) {
        return this._CurrentHelp.sectionName
      }
      else
        return this._CurrentHelp.context;
    }
  }
  
  private _FeedbackMessageList: FeedbackAndClientError[] = [];
  public get FeedbackMessageList(): FeedbackAndClientError[] {
    return this._FeedbackMessageList;
  }
  public ClearCurrentContext() {
    this._CurrentHelp = new OnlineHelpContent();
  }
  public FetchHelpContent(context: string) {
    if (this.CurrentHelp.context == context) return; //no need to re-fetch the help we have already.
    else {
      this._BusyMethods.push("FetchHelpContent");
      let body = { Value: context };

      lastValueFrom(this._http.post<iOnlineHelpContent>(this._baseUrl + 'api/Help/FetchHelpContent',
        body))
        .then(result => {
          this._CurrentHelp = new OnlineHelpContent(result);
          //console.log("gethelp:", body, result);
          //if ((result.parentContext == null) || (result.parentContext == "")) {
          //  this._CurrentContext = result.context;
          //}
          //else {
          //  this._CurrentContext = result.parentContext;
          //}
          //this._FullContextExtensionName = result.context;

          //this._EditingSectionName = result.sectionName.trim();
          //this._ContextExtensionUserFriendlyName = result.sectionName.trim();

          ////this._Index0SectionName = result.sectionName;
          //if (fistContext == true) {
          //  this._Index0SectionName = result.sectionName;
          //}
          //this._CurrentHTMLHelp = result.helpHTML;
          this.RemoveBusy("FetchHelpContent");
        },
          (error) => {
            //console.log("FetchHelpContent error:", error);
            this.RemoveBusy("FetchHelpContent");
            this.modalService.GenericError(error);
          }
        );
    }
  }



  public UpdateHelpContent(message: OnlineHelpContent,) {

    this._BusyMethods.push("UpdateHelpContent");

    lastValueFrom(this._http.post<OnlineHelpContent>(this._baseUrl + 'api/Help/UpdateHelpcontent', message))
      .then(result => {
        this.RemoveBusy("UpdateHelpContent");
        this.FetchHelpPageList("0");
        this.FetchHelpContent("");
      },     
        (error) => {
          console.log("UpdateHelpContent error:", error);
          this.RemoveBusy("UpdateHelpContent");
          this.modalService.GenericError(error);
        }
      );
  }

  public AddEmptyHelpPage(message: OnlineHelpContent,) {

    this._BusyMethods.push("UpdateHelpContent");
    // note!! UpdateHelpcontent is actually a Create or Edit routine so we can that 
    lastValueFrom( this._http.post<OnlineHelpContent>(this._baseUrl + 'api/Help/UpdateHelpcontent', message))
      .then(result => {
        this.RemoveBusy("UpdateHelpContent");
        this.FetchHelpPageList("0");
        this.FetchHelpContent("")
      },
        (error) => {
          console.log("UpdateHelpContent error:", error);
          this.RemoveBusy("UpdateHelpContent");
          this.modalService.GenericError(error);
        }
      );
  }

  public DeleteContextExtension(message: OnlineHelpContent) {

    this._BusyMethods.push("DeleteHelpContent");
    // note!! UpdateHelpcontent is actually a Create or Edit routine so we can that 
    lastValueFrom( this._http.post<OnlineHelpContent>(this._baseUrl + 'api/Help/DeleteHelpcontent', message))
      .then(result => {
        this.RemoveBusy("DeleteHelpContent");
        //reload the list of context
        this.FetchHelpPageList("0");
        this.FetchHelpContent("");
      },
        (error) => {
          console.log("DeleteHelpContent error:", error);
          this.RemoveBusy("DeleteHelpContent");
          this.modalService.GenericError(error);
        }
      );
  }

  private _OnlineHelpPages: ReadOnlyHelpPage[] = [];
  public get HelpPages(): ReadOnlyHelpPage[] {
    return this._OnlineHelpPages;
  }


  public FetchHelpPageList(context: string) {
    if ((this.CurrentContext == context) && (context != "0")) return;
    else {
      this._BusyMethods.push("FetchHelpPageList");
      let body = { Value: context };
      lastValueFrom(this._http.post<ReadOnlyHelpPageList>(this._baseUrl + 'api/Help/GetHelpPageList',
        body))
        .then(result => {
          this._OnlineHelpPages = result.helpPages;
          this.ShowHelpDropDown = false;
          if (result.helpPages.length > 0) {
            this.ShowHelpDropDown = true;
          }
          this.RemoveBusy("FetchHelpPageList");
        }, error => {
          this.RemoveBusy("FetchHelpPageList");
          this.modalService.GenericError(error);
        }
        );
    }
  }


  public CreateFeedbackMessage(message: FeedbackAndClientError4Create) {

    this._BusyMethods.push("CreateFeedbackMessage");

    lastValueFrom(this._http.post<FeedbackAndClientError>(this._baseUrl + 'api/Help/CreateFeedbackMessage', message))
      .then(result => {
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
    lastValueFrom(this._http.get<FeedbackAndClientError[]>(this._baseUrl + 'api/Help/FeedbackMessageList')).then(
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
    this.ClearCurrentContext();
  }
}
export interface iOnlineHelpContent {
  context: string;
  helpHTML: string;
  sectionName: string;
  parentContext: string;
}
export class OnlineHelpContent implements iOnlineHelpContent {
  constructor(iOnlineHelp: iOnlineHelpContent | undefined = undefined) {
    if (iOnlineHelp) {
      this.context = iOnlineHelp.context;
      this.helpHTML = iOnlineHelp.helpHTML;
      this.sectionName = iOnlineHelp.sectionName;
      this.parentContext = iOnlineHelp.parentContext;
      const splitted = this.context.split('\\');
      if (this.parentContext == "") this._leafContext = "";
      else this._leafContext = splitted[splitted.length - 1];
    } else {
      this.context = "";
      this.helpHTML = "";
      this.sectionName = "";
      this.parentContext = "";
      this._leafContext = "";
    }
  }
  clone(): OnlineHelpContent {
    return new OnlineHelpContent(this);
  }
  private _leafContext;
  public get leafContext(): string { return this._leafContext; }
  public set leafContext(val: string) {
    this._leafContext = val;
    this.context = this.parentContext + "\\" + this._leafContext;
  }
  public get IsExtension(): boolean {
    if (this.parentContext == "") return false;
    return true;
  }
  public get IsValid():boolean {
    if (this.IsExtension) {
      if (this.leafContext.trim() == "" || this.sectionName.trim() == "") return false;
    }
    if (this.context.trim() == "") return false;
    return true;
  }
  context: string;
  helpHTML: string;
  sectionName: string;
  parentContext: string;
}



export class FeedbackAndClientError4Create {
  public contactId: number = -1;
  public context: string = "";
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
  context_SectionName: string;
}

export interface ReadOnlyHelpPageList {
  helpPages: ReadOnlyHelpPage[];
}


