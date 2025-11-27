import { Component, Inject, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { timer, Subject, Subscription, Observable } from 'rxjs';
import { take, map, takeUntil } from 'rxjs/operators';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { WorkAllocationListService } from '../services/WorkAllocationList.service';
import { OnlineHelpService, FeedbackAndClientError4Create } from '../services/onlinehelp.service';
import { trigger, state, style, transition, animate, keyframes } from '@angular/animations';
import { NotificationService } from '@progress/kendo-angular-notification';



@Component({
  selector: 'HeaderComponent',
  templateUrl: './header.component.html',
  providers: [],
  animations: [
    trigger('HelpAppear', [
      state('hidden', style({
        transform: 'scaleY(0)',
        'height': '0px',
        transformOrigin: 'top',
        'overflow': 'hidden',
        opacity: 0
      })),
      state('visible', style({
        transform: 'scaleY(1)',
        'height': '*',
        transformOrigin: 'top',
        'overflow': 'auto',
        opacity: 1
      })),
      transition('hidden => visible', [
        animate('0.5s')
      ]),
      transition('visible => hidden', [
        animate('0.2s')
      ]),
    ]),
    trigger('PanelAppear', [
      state('hidden', style({
        'height': '0px',
        'overflow': 'hidden',
        opacity: 0
      })),
      state('visible', style({
        'height': '*',
        'overflow': 'auto',
        opacity: 1
      })),
      transition('hidden => visible', [
        animate('0.5s')
      ]),
      transition('visible => hidden', [
        animate('0.2s')
      ]),
    ]),
    trigger('PanelFeedbackAppear', [
      state('collapse', style({
        'height': '0px',
        'overflow': 'hidden',
        opacity: 0
      })),
      state('expand', style({
        'height': '*',
        'overflow': 'auto',
        opacity: 1
      })),
      transition('collapse => expand', [
        animate('0.5s')
      ]),
      transition('expand => collapse', [
        animate('0.2s')
      ]),
    ]),
  ],
})

export class HeaderComponent implements OnInit {


  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    public ReviewerIdentityServ: ReviewerIdentityService,
    private OnlineHelpService: OnlineHelpService,
    private ReviewSetsService: ReviewSetsService,
    private ItemListService: ItemListService,
    private workAllocationListService: WorkAllocationListService,
    private notificationService: NotificationService
  ) { }


  @Input() PageTitle: string | undefined;
  @Input() Context: string | undefined;

  private _ActivePanel: string = "";
  private cachedDisplayFriendlyIndex0Name: string = "";
  public set ActivePanel(val: string) {
    this._ActivePanel = val;
  }
  public get ActivePanel(): string {
    //console.log("closing help?:", this.OnlineHelpService.CurrentContext, this.Context);

    if (this._ActivePanel == "Help" && !this.IsServiceBusy && this.OnlineHelpService.CurrentContext != this.Context) {
      if (this.cachedDisplayFriendlyIndex0Name == ""
        || this.OnlineHelpService.CurrentHelp.parentContext != this.Context) this._ActivePanel = "";
    }
    return this._ActivePanel;
  }
  public UserFeedback: string = "";

  public ShowDropDown: boolean = false;

  public get ShowHelpDropDown(): boolean {
    return this.OnlineHelpService.ShowHelpDropDown;
  }

  public get NameOfUser(): string {
    return this.ReviewerIdentityServ.reviewerIdentity.name;
  }

  public get IsReadOnly(): boolean {
    if (this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated && this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0
      && this.ReviewerIdentityServ.reviewerIdentity.roles.indexOf('ReadOnlyUser') == -1) {
      //special case: user is authenticated, but not in a review and does not have the ReadOnlyUser role
      return false;
    }
    else {
      return !this.ReviewerIdentityServ.HasWriteRights;
    }
  }
  public get IsServiceBusy(): boolean {
    if (this.OnlineHelpService.IsBusy) return true;
    else return false;
  }
  public get CurrentContextHelp(): string {
    //console.log("CurrentContextHelp", this.OnlineHelpService.CurrentHTMLHelp);
    if (this.OnlineHelpService.IsBusy) return "";
    else {
      return this.OnlineHelpService.CurrentHTMLHelp;
    }
  }
  ngOnDestroy() {
  }
  Clear() {
    //      this.ItemListService.SaveItems(new ItemList(), new Criteria());
    //      this.ReviewSetsService.Clear();
    //this.workAllocationListService.Clear();
    //this.workAllocationContactListService.Save();
  }
  Logout() {
    this.Clear();
    this.ReviewerIdentityServ.LogOut();
    this.router.navigate(['home']);
  }
  ShowHideFeedback() {
    if (this.ActivePanel == "Feedback"
      || !this.Context
      || this.Context == '') {
      this._ActivePanel = "";
    }
    else if (this.Context && this.Context !== '') {
      //console.log("Feedback");
      this._ActivePanel = "Feedback";
    }
  }
  ShowHideHelp() {
    this.cachedDisplayFriendlyIndex0Name = "";
    if (this.ActivePanel == "Help"
      || !this.Context
      || this.Context == '') {
      this.ActivePanel = "";
      this.ShowDropDown = false;
    }
    else if (this.Context && this.Context !== '') {
      this._ActivePanel = "Help";
      this.OnlineHelpService.FetchHelpContent(this.Context);
      this.OnlineHelpService.FetchHelpPageList(this.Context);
      this.ShowDropDown = true;
    }
  }
  SendFeedback() {
    let fb: FeedbackAndClientError4Create = new FeedbackAndClientError4Create();
    fb.context = (this.Context ? this.Context : "");
    fb.contactId = this.ReviewerIdentityServ.reviewerIdentity.userId;
    fb.isError = false;
    fb.message = this.UserFeedback;
    this.OnlineHelpService.CreateFeedbackMessage(fb);
    this.UserFeedback = "";
    this._ActivePanel = "";
    this.notificationService.show({
      content: "Thanks for your feed-back!",
      animation: { type: 'slide', duration: 800 },
      position: { horizontal: 'center', vertical: 'top' },
      type: { style: 'success', icon: true },
      hideAfter: 2000
    });
  }
  public get EmailString(): string {
    return "mailto:EPPISupport@ucl.ac.uk?Subject=ER-Web support request from page \""
      + this.Context
      + "\" (Review Id:"
      + this.ReviewerIdentityServ.reviewerIdentity.reviewId
      + ")&Body=Hello,%0D%0A[Please type your message here]%0D%0A %0D%0A %0D%0A[Context details, please do not edit]: %0D%0APage: "
      + this.Context + "%0D%0AReview Id: " + this.ReviewerIdentityServ.reviewerIdentity.reviewId + "%0D%0A";
  }

  public selectedContext: string = "Select a topic";

  DisplayFriendlyHelpPageNames(helpPageItem: ReadOnlyHelpPage): string {
    this.selectedContext = helpPageItem.context_Name;
    //return helpPageItem.context_Name;
    return helpPageItem.context_SectionName;
  }
  DisplayFriendlyIndex0Name(): string {
    if (this.cachedDisplayFriendlyIndex0Name != "") return this.cachedDisplayFriendlyIndex0Name;
    return this.OnlineHelpService.CurrentHelp.sectionName;
  }

  public get HelpPages(): ReadOnlyHelpPage[] {
    return this.OnlineHelpService.HelpPages;
  }
  public selected?: ReadOnlyHelpPage;


  public RetrieveHelpNew(event: Event) {

    let TmpContext = (event.target as HTMLOptionElement).value;
    if (TmpContext) {
      if (TmpContext.indexOf("--> ") != -1) {
        const pageToGet = this.OnlineHelpService.HelpPages.find(f => f.context_SectionName == TmpContext.replace("--> ", ""));
        if (pageToGet) {
          if (this.cachedDisplayFriendlyIndex0Name == "") this.cachedDisplayFriendlyIndex0Name = this.OnlineHelpService.CurrentHelp.sectionName;
          TmpContext = pageToGet.context_Name;
        }
        else return;
      }
      else if (this.cachedDisplayFriendlyIndex0Name != "" && TmpContext == this.cachedDisplayFriendlyIndex0Name) {
        if (this.Context) this.OnlineHelpService.FetchHelpContent(this.Context);
        else return;
      }
      this.OnlineHelpService.FetchHelpContent(TmpContext);
      this._ActivePanel = "Help";
    }
  }


  ngOnInit() {
  }
}

export interface ReadOnlyHelpPage {
  helpPage_ID: number;
  context_Name: string;
  context_SectionName: string;
}
