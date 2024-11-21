import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { FeedbackAndClientError, OnlineHelpContent, OnlineHelpContent1, OnlineHelpService } from '../services/onlinehelp.service';
import { GridDataResult, PageChangeEvent, DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, process, CompositeFilterDescriptor, State } from '@progress/kendo-data-query';
import { Subscription } from 'rxjs';
import { EventEmitterService } from '../services/EventEmitter.service';
import { SelectEvent, TabStripComponent } from '@progress/kendo-angular-layout';


@Component({
    selector: 'siteadmin',
    templateUrl: './siteadmin.component.html',
    providers: []
})

export class SiteAdminComponent implements OnInit {

  @ViewChild('content') private content: any;
  @ViewChild('tabstrip') public tabstrip!: TabStripComponent;

    constructor(private router: Router,
        private _httpC: HttpClient,
        private OnlineHelpService: OnlineHelpService,
        @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService,
        public eventEmitters: EventEmitterService
  ) { }



    ngOnInit() {
        this.subOpeningReview = this.eventEmitters.OpeningNewReview.subscribe(() => this.BackToMain());
        if (!this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) this.router.navigate(['home']);
        else this.OnlineHelpService.GetFeedbackMessageList();
    }
    public Uname: string = "";
    public Pw: string = "";
  public revId: string = "";
  public LogTypeSelection: number = 0;


  private _ActivePanel: string = "Help";
  public ActivePanel: string = "Help";






    //public HelpAndFeebackContext: string = "main\\reviewhome";
    //public LoggedStatus: string = "";
    subOpeningReview: Subscription | null = null;
    public get FeedbackMessageList(): FeedbackAndClientError[] {
        return this.OnlineHelpService.FeedbackMessageList;
    }
    SelectTab(i: number) {
      if (!this.tabstrip) return;
      else {
        let t = this.tabstrip.tabs.get(i);
        if (!t) return;
        let e = new SelectEvent(i, t.title);
        this.tabstrip.selectTab(i);
        this.onTabSelect(e);
      }
    }
    public get DataSource(): GridDataResult {
        return process(this.OnlineHelpService.FeedbackMessageList, this.state);
        //return {
        //    data: orderBy(this.OnlineHelpService.FeedbackMessageList.slice(this.skip, this.skip + this.pageSize), this.sort),
        //    total: this.OnlineHelpService.FeedbackMessageList.length,
        //};
    }
    public state: State = {
        skip: 0,
        take: 10,
        
    };
    public dataStateChange(state: DataStateChangeEvent): void {
        this.state = state;
        this.DataSource; //= process(sampleProducts, this.state);
    }
    
    ShowDBSettingById(event: Event) {
      let helpId = parseInt((event.target as HTMLOptionElement).value);
      //console.log("Changing DB (id): ", dbId);
      this.OnlineHelpService.FetchHelpContent(helpId.toString());
    }
    
    public get IsSiteAdmin(): boolean {
        //console.log("Is it?", this.ReviewerIdentityServ.reviewerIdentity
        //    , this.ReviewerIdentityServ.reviewerIdentity.userId > 0
        //    , this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated
        //    , this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin);
        if (this.ReviewerIdentityServ
            && this.ReviewerIdentityServ.reviewerIdentity
            && this.ReviewerIdentityServ.reviewerIdentity.userId > 0
            && this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated
            && this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) return true;
        else return false;
    }
    public get CanOpenRev(): boolean {
        if (this.Uname.trim().length < 2) return false;
        else if (this.Pw.trim().length < 6) return false;
        else if (this.revId.trim().length > 0) {
            let rid = parseInt(this.revId, 10)
            if (!isNaN(rid) && rid > 0) return true;
            else return false;
        }
        else return false;
    }
    OpenRev() {
        //this.LoggedStatus == "";
        let rid = parseInt(this.revId, 10)
        if (!isNaN(rid) && rid > 0) {
            //this.BackToMain();
            this.ReviewerIdentityServ.LoginReqSA(this.Uname, this.Pw, rid);
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

  UpdateHelp() {
    let help: OnlineHelpContent1 = new OnlineHelpContent1();
    help.context = this.context;
    help.helpHTML = "";// this.UserFeedback;
    //this.OnlineHelpService.UpdateHelpContent(help);
  }
  /*
  public CloneHelpforEdit(toClone: OnlineHelpContent): OnlineHelpContent {
    let res = {
      onlineHelpContentId: toClone.onlineHelpContentId,
      context: this.context,
      helpHTML: toClone.helpHTML
    }
    return res;
  }
  */

  public model = {
    //editorData: this.CurrentContextHelp,
    editorData: "",
  };

  //public EditingHelp: OnlineHelpContent | null = null;
  public helpContent: string | null = null;
  public ContextSelection: number = 0;
  public context = "";
  //public test = "";
  public editingHelp = "";
  public RetrieveHelp() {
    switch (this.ContextSelection) {
      case 1: this.context = "(codingui)itemdetails"; break;
      case 2: this.context = "(codingui)itemdetails\pdf"; break;
      case 3: this.context = "(codingui)main"; break;
      case 4: this.context = "buildmodel"; break;
      case 5: this.context = "duplicates"; break;
      case 6: this.context = "editcodesets"; break;
      case 7: this.context = "editref"; break;
      case 8: this.context = "importcodesets"; break;
      case 9: this.context = "intropage"; break;
      case 10: this.context = "itemdetails"; break;
      case 11: this.context = "itemdetails\arms"; break;
      case 12: this.context = "itemdetails\codingrecord"; break;
      case 13: this.context = "itemdetails\pdf"; break;
      case 14: this.context = "main\collaborate"; break;
      case 15: this.context = "main\crosstabs"; break;
      case 16: this.context = "main\frequencies"; break;
      case 17: this.context = "main\references"; break;
      case 18: this.context = "main\reports"; break;
      case 19: this.context = "main\reviewhome"; break;
      case 20: this.context = "main\search"; break;
      case 21: this.context = "metaanalysis"; break;
      case 22: this.context = "metaanalysis\run"; break;
      case 23: this.context = "metaanalysis\runnetwork"; break;
      case 24: this.context = "reconciliation"; break;
      case 25: this.context = "reconciliation\treesview"; break;
      case 26: this.context = "sources\file"; break;
      case 27: this.context = "sources\managesources"; break;
      case 28: this.context = "sources\pubmed"; break;
      case 29: this.context = "webdbs"; break;
      case 30: this.context = "ZoteroSetup"; break;
      case 31: this.context = "ZoteroSync"; break;
      default: this.context = "0";
    }
    this.showEdit = false;
    if (this.context != "0") {
      this.OnlineHelpService.FetchHelpContent(this.context);
      if (this.CurrentContextHelp == null) {
        // no data so close the view area?
        this.OnlineHelpService.FetchHelpContent("");
      }
    }
    else {
      this.OnlineHelpService.FetchHelpContent("");
    }

  }

  Edit() {
    if (this.showEdit == true) {
      this.showEdit = false;
    }
    else {
      this.showEdit = true;
      this.model.editorData = this.CurrentContextHelp;
    }    
  }

  Preview() {
    var test = this.model.editorData;
  }


  Save() {
    // update the database with the edited help
  }


  public showEdit: boolean = false;
  public get ShowEdit(): boolean {
    return this.showEdit;
  }

    //public  CheckLoggedStatus() {
    //    if (this.ReviewerIdentityServ.reviewerIdentity && this.ReviewerIdentityServ.reviewerIdentity.reviewId != 0) {
    //        this.LoggedStatus =  "Rid=" + this.ReviewerIdentityServ.reviewerIdentity.reviewId + " " + this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated;
    //    }
    //    else this.LoggedStatus = "Nope: " + this.ReviewerIdentityServ.reviewerIdentity.reviewId;
    //}
    //protected pageChange({ skip, take }: PageChangeEvent): void {
    //    this.skip = skip;
    //    this.pageSize = take;
    //    this.DataSource;
    //}//
    //protected filterChange($event: CompositeFilterDescriptor) {
    //    console.log($event);
    //    this.filter = $event;
    //}
    //public sortChange(sort: SortDescriptor[]): void {
    //    this.sort = sort;
    //    this.DataSource;
    //}
    BackToMain() {
        this.router.navigate(['Main']);
    }
    ngOnDestroy() {
    }


  onTabSelect(e: SelectEvent) {

    if (e.title == 'Help') {
      //this.OnlineHelpService.FetchHelpContentList();
      this.OnlineHelpService.FetchHelpContent("0");
      this.ContextSelection = 0;
    }
    else {

    }
  }




}






