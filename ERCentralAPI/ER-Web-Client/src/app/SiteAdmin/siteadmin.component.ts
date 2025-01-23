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
import { CKEditor4 } from 'ckeditor4-angular/ckeditor';
import { ModalService } from '../services/modal.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { SourcesService, iJSONreport4upolad } from '../services/sources.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';


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
      public eventEmitters: EventEmitterService,
      private modalService: ModalService,
      public _notificationService: NotificationService,
      private sourcesService: SourcesService,
      private reviewInfo: ReviewInfoService
  ) { }



    ngOnInit() {
        this.subOpeningReview = this.eventEmitters.OpeningNewReview.subscribe(() => this.BackToMain());
        if (!this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) this.router.navigate(['home']);
      else this.OnlineHelpService.GetFeedbackMessageList();
      this.reader.onload = (e) => this.fileRead();
    }
    public Uname: string = "";
    public Pw: string = "";
    public revId: string = "";
    public LogTypeSelection: number = 0;


    private _ActivePanel: string = "Help";
    public ActivePanel: string = "Help";



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
      this.OnlineHelpService.FetchHelpContent(helpId.toString());
    }
    
    public get IsSiteAdmin(): boolean {
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


/////////////////////////////////////////////////////////////////////////

  public TmpCurrentContextHelp: string = "";
  public OrigCurrentContextHelp: string = "";
  public enableSave: boolean = false;


  CanSaveHelp() {
    if (this.enableSave) {
      return true;
    } else return false;
  }


  public get CurrentContextHelp(): string {
    //console.log("CurrentContextHelp", this.OnlineHelpService.CurrentHTMLHelp);
    if (this.OnlineHelpService.IsBusy) return "";
    else {
      return this.OnlineHelpService.CurrentHTMLHelp;
    }
  }


  public model = {
    editorData: "",
  };


  public helpContent: string | null = null;
  public ContextSelection: number = 0;
  public context = "";
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
    this.enableSave = false;
    this.TmpCurrentContextHelp = "";
    if (this.context != "0") {
      this.OnlineHelpService.FetchHelpContent(this.context);
      if (this.CurrentContextHelp == null) {
        // there is no data
        this.OnlineHelpService.FetchHelpContent("");
      }
    }
    else {
      // user selected '0' again so no data
      this.OnlineHelpService.FetchHelpContent("");
    }
  }


  public onDataChange(event: CKEditor4.EventInfo) {
    var test = event.editor.getData();
    //this.TmpCurrentContextHelp = this.model.editorData;
    this.TmpCurrentContextHelp = event.editor.getData();
    if (this.OrigCurrentContextHelp == event.editor.getData()) {
      // things are unchanged from origninal...
      this.enableSave = false;
    }
    else {
      // things are different...
      this.enableSave = true;
    }
  }


  Edit() {
    if (this.showEdit == true) {
      // this is a 'cancel'
      this.showEdit = false;
      this.TmpCurrentContextHelp = "";
      this.OrigCurrentContextHelp = "";
      this.showEdit = false;
      this.OnlineHelpService.FetchHelpContent(this.context);
    }
    else {
      // this is an 'edit'
      this.enableSave = false;
      this.showEdit = true;     
      this.OrigCurrentContextHelp = this.CurrentContextHelp;
      this.model.editorData = this.CurrentContextHelp;
      this.TmpCurrentContextHelp = this.model.editorData;

    }    
  }


  public GetText(): string {
    if (this.showEdit == true) {
      return this.model.editorData;
    }
    else {
      return this.CurrentContextHelp;
    }      
  }


  Save() {
    let help: OnlineHelpContent1 = new OnlineHelpContent1();
    help.context = this.context;
    help.helpHTML = this.model.editorData;
    this.OnlineHelpService.UpdateHelpContent(help);
    this.showEdit = false;
    this.TmpCurrentContextHelp = "";

    this.OnlineHelpService.FetchHelpContent("");
    this.ContextSelection = 0;

    //this.OrigCurrentContextHelp = "";
    //this.OnlineHelpService.FetchHelpContent(this.context);
  }


  public showEdit: boolean = false;
  public get ShowEdit(): boolean {
    return this.showEdit;
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////

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


  @ViewChild('file') file: any;
  private currentFileName: string = "";
  public importCommand: iJSONreport4upolad = this.EmptyImportCommand;
  public ItemsCount4Import: number = 0;
  public CodingTools4Import: string[] = [];
  public get canImport(): boolean {
    if (this.importCommand.content == "") return false;
    if (this.importCommand.fileName == "") return false;
    return true;
  }
  public get CurrentReviewName(): string {
    return this.reviewInfo.ReviewInfo.reviewName;
  }

  public ClearJSONCache() {
    this.ItemsCount4Import = 0;
    this.CodingTools4Import = [];
    this.importCommand = this.EmptyImportCommand;
  }
  private get EmptyImportCommand(): iJSONreport4upolad {
    return {
      content: "",
      fileName: "",
      importWhat: ""
    }; 
  }
  addFile() {
    //console.log('oo');
    if (this.file) this.file.nativeElement.click();
  }
  private reader = new FileReader();
  onFilesAdded() {
    const files: { [key: string]: File } = this.file.nativeElement.files;
    const file: File = files[0];
    console.log("onFilesAdded", file.size, file.name);
    if (file) {
      //if (file.size > 31457280 && 1 !== 1) {
      if (file.size > 52428800) {
        //console.log("onFilesAdded", file.size, file.name);
        this.modalService.GenericErrorMessage("Sorry, the <strong>maximum</strong> file size is <strong>50MB</strong>. Please select a smaller file.");
      }
      else {
        this.currentFileName = file.name;
        //reader.onload = function (e) {
        //    if (reader.result) {
        //        fileContent = reader.result as string;
        //        console.log(fileContent.length);
        //    }
        //}
        this.reader.readAsText(file);
        this.fileRead();
      }
    }
  }

  private fileRead() {
    this.ClearJSONCache();
    if (this.reader.result) {
      const fileContent: string = this.reader.result as string;
      //console.log("fileRead: " + fileContent.length);
      let filename = "";
      if (this.currentFileName) filename = this.currentFileName.trim();
      else {
        return;
      }
      this.importCommand = {
        content: fileContent,
        importWhat: "",
        fileName: filename
      };
      const parsed = JSON.parse(fileContent);
      if (parsed) {
        if (parsed.CodeSets && parsed.CodeSets.length > 0) {
          for (let Cset of parsed.CodeSets) {
            if (Cset && Cset.SetName) {
              this.CodingTools4Import.push(Cset.SetName);
            }
          }
        }
        if (parsed.References && parsed.References.length > 0) {
          this.ItemsCount4Import = parsed.References.length;
        }
      }
    }
  }
  public doImport() {
    if (!this.canImport) return;
    this.sourcesService.ImportJsonReport(this.importCommand).then(
      (result) => {
        if (result) {
          this._notificationService.show({
            content: "Imported! Please go back, refresh Items and Coding tools to check.",
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "warning", icon: true },
            hideAfter: 10000
          })
        }
      }
    );
  }
}






