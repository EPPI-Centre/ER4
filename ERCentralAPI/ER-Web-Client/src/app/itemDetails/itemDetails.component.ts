import { Component, OnInit, Input, ViewChild, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Item, ItemListService, iAdditionalItemDetails } from '../services/ItemList.service';
import { ReviewerTermsService, ReviewerTerm } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ModalService } from '../services/modal.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { TextSelectEvent } from "../helpers/text-select.directive";
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReviewerIdentityService, PersistingOptions } from '../services/revieweridentity.service';
import { Subscription } from 'rxjs';
import { ItemDocListComp } from '../ItemDocumentList/itemDocListComp.component';
import { faBook } from '@fortawesome/free-solid-svg-icons';

// COPYRIGHTS BELONG TO THE FOLLOWING FOR ABILITY TO SELECT TEXT AND CAPTURE EVENT
// https://www.bennadel.com/blog/3439-creating-a-medium-inspired-text-selection-directive-in-angular-5-2-10.htm

@Component({
  selector: 'itemDetailsComp',
  templateUrl: './itemDetails.component.html',
  providers: [],
  styles: []
})
export class itemDetailsComp implements OnInit, OnDestroy {

  constructor(
    private router: Router,
    private ReviewerTermsService: ReviewerTermsService,
    public ItemDocsService: ItemDocsService,
    private PriorityScreeningService: PriorityScreeningService,
    private ItemListService: ItemListService,
    private ModalService: ModalService,
    private ItemCodingService: ItemCodingService,
    private ReviewerIdentityServ: ReviewerIdentityService
  ) { }

  @Input() item: Item | undefined;
  @Input() ShowHighlights: boolean = false;
  @Input() CanEdit: boolean = false;
  @Input() IsScreening: boolean = false;
  @Input() ShowDocViewButton: boolean = true;
  @Input() Context: string = "CodingFull";
  @ViewChild('ItemDocListComp')
  private ItemDocListComp!: ItemDocListComp;
  ngOnInit() {
    this.subscr = this.ReviewerTermsService.setHighlights.subscribe(
      () => { this.SetHighlights(); }

    );
    this.selectedText = "";
  }
  public HAbstract: string = "";
  public brAbstract: string = "";
  public HTitle: string = "";

  public showOptionalFields = false;
  public NoTextSelected: boolean = true;
  public data: Array<any> = [{
    text: 'Current',
    icon: 'paste-plain-text',
    click: () => {
      this.RelevantTermClass = 'RelevantTerm';
      this.IrrelevantTermClass = 'IrrelevantTerm';
      this.RefreshHighlights();
      //console.log('Keep Text Only');
    }
  }, {
    text: 'ER4 style',
    icon: 'paste-as-html',
    click: () => {
      this.RelevantTermClass = 'RelevantTermER4';
      this.IrrelevantTermClass = 'IrrelevantTermER4';
      this.RefreshHighlights();
      //console.log('Paste as HTML');
    }
  }, {
    text: 'B & W',
    icon: 'paste-markdown',
    click: () => {
      this.RelevantTermClass = 'RelevantTermBW';
      this.IrrelevantTermClass = 'IrrelevantTermBW';
      this.RefreshHighlights();
      //console.log('Paste Markdown');
    }
  }, {
    text: 'Current: fainter',
    click: () => {
      this.RelevantTermClass = 'RelevantTermFainter';
      this.IrrelevantTermClass = 'IrrelevantTermFainter';
      this.RefreshHighlights();
      //console.log('Set Default Paste');
    }
  }];
  public faBook = faBook;
  public get HighlightsStyle(): string {
    if (this.ReviewerIdentityServ.userOptions.persistingOptions) return this.ReviewerIdentityServ.userOptions.persistingOptions.HighlightsStyle;
    return "";
  }
  public RelevantTermClass: string = 'RelevantTerm';
  public IrrelevantTermClass: string = 'IrrelevantTerm';
  public changeTermsColours() {

    this.ShowHighlightsClicked();
    alert('Change term colours here...');
  }

  public get CurrentItemAdditionalData(): iAdditionalItemDetails | null {
    if (this.IsScreening) {
      return this.PriorityScreeningService.CurrentItemAdditionalData;
    }
    else {
      return this.ItemListService.CurrentItemAdditionalData;
    }
  }


  private selectedText!: string;
  private subscr: Subscription = new Subscription();

  public get DOILink(): string {
    if (this.item == undefined) return "";
    else {
      return Helpers.DOILink(this.item.doi);
    }
  }
  //adapted from:https://stackoverflow.com/a/43467144 
  public get URLLink(): string {
    if (this.item == undefined) return "";
    else {
      return Helpers.URLLink(this.item.url);
    }
  }

  public get HasPDF(): boolean {
    if (this.ItemDocsService._itemDocs.findIndex((found) => found.extension.toLowerCase() == ".pdf") == -1) return false;
    else return true;
  }

  public renderRectangles(event: TextSelectEvent): void {

    console.groupEnd();

    if (event.hostRectangle && event.text.length > 0) {
      this.selectedText = event.text;
      this.NoTextSelected = false;

    } else {
      this.selectedText = "";
      this.NoTextSelected = true;
    }
  }

  public WipeHighlights() {
    this.HAbstract = "";
    this.HTitle = "";
  }
  ShowHighlightsClicked() {
    if (this.item && this.ShowHighlights && this.HAbstract == '' && !(this.item.abstract == '')) {
      this.SetHighlights();
    }
    this.ShowHighlights = !this.ShowHighlights;
    if (!this.ShowHighlights) {
      this.ReviewerTermsService._ShowHideTermsList = false;
    }
  }
  ItemChanged() {
    alert('item changed!!');
  }
  EditItem() {
    if (this.item) {
      if (!this.IsScreening)
        this.router.navigate(['EditItem', this.item.itemId], { queryParams: { return: 'itemcoding/' + this.item.itemId.toString() } });
      else {//we're in priority screening... but which kind?
        if (this.PriorityScreeningService.UsingListFromSearch == false)
          this.router.navigate(['EditItem', "FromPrioritySc"], { queryParams: { return: 'itemcoding/PriorityScreening2' } });
        else this.router.navigate(['EditItem', "FromPrioritySc"], { queryParams: { return: 'itemcoding/ScreeningFromList2' } });
      }
    }
  }
  OpenFirstPDF() {
    console.log("Try to open first Doc");
    if (this.ItemDocListComp) this.ItemDocListComp.OpenFirstPDF();
  }
  public FindReferenceOnMicrosoftAcademic(item: Item) {
    if (item != null) {
      let searchString: string = "\"" + item.title + "\" " + item.authors;
      window.open("https://academic.microsoft.com/search?q=" +
        encodeURIComponent(searchString));
    }
  }
  public FindReferenceOnGoogle(item: Item) {
    if (item != null) {
      let searchString: string = "\"" + item.title + "\" " + item.authors;
      window.open("https://www.google.com/search?q=" + searchString);
    }
  }

  public FindReferenceOnGoogleScholar(item: Item) {
    if (item != null) {
      let searchString: string = "\"" + item.title + "\" " + item.authors;
      window.open("https://scholar.google.com/scholar?q=" + searchString);
    }
  }
  public RemoveTerm() {

    if (this.selectedText != null && this.selectedText != ''
      && this.ReviewerTermsService.TermsList.length > 0) {

      var findTerm = this.ReviewerTermsService.TermsList
        .find(x => x.reviewerTerm == this.selectedText);

      if (findTerm) {
        this.ReviewerTermsService.DeleteTerm(findTerm.trainingReviewerTermId);
      }
    }

  }
  public AddRelevantTerm(addRemoveBtn: boolean) {

    if (this.selectedText != null && this.selectedText != '') {

      let s: string = this.selectedText.trim().toLowerCase();
      if (s == null || s.length == 0) return;
      if (s.length > 50) return;
      let terms: string[] = s.split(" ", 50);
      for (var i = 0; i < terms.length; i++) {

        var term = terms[i];
        //console.log(term + '\n');
        let cTrt: ReviewerTerm | null = this.FindTerm(term);
        //console.log('CTrt not null: ', cTrt);
        if (cTrt == null) {
          let trt: ReviewerTerm = new ReviewerTerm({
            reviewerTerm: term,
            included: addRemoveBtn,
            itemTermDictionaryId: 0,
            trainingReviewerTermId: 0,
            term: term,
          });
          if (this.ReviewerTermsService.TermsList != null) {
            //this.ReviewerTermsService.TermsList.push(trt as ReviewerTerm);
            this.ReviewerTermsService.CreateTerm(trt);
          }
        }
        else {//term is already there, see if we need to flip the Included flag
          if (

            (cTrt.included && !addRemoveBtn)//adding as negative, but it's already there as positive
            ||
            (!cTrt.included && addRemoveBtn)
          ) {
            cTrt.included = !cTrt.included;
            this.ReviewerTermsService.UpdateTerm(cTrt);
          }
        }
        this.RefreshHighlights();
      }
    }
  }

  public FindTerm(term: string): ReviewerTerm | null {

    var ind = this.ReviewerTermsService.TermsList.findIndex(x => x.reviewerTerm == term);
    if (ind != -1) {
      return this.ReviewerTermsService.TermsList[ind];
    } else {
      return null;
    }
  }

  public ShowHideTermsList() {

    this.ReviewerTermsService._ShowHideTermsList = !this.ReviewerTermsService._ShowHideTermsList;
  }

  public RefreshHighlights() {
    this.selectedText = '';
    this.NoTextSelected = true;
    this.SetHighlights();
  }
  SetHighlightStyle(style: string) {
    if (!this.ReviewerIdentityServ.userOptions.persistingOptions) {
      this.ReviewerIdentityServ.userOptions.persistingOptions = new PersistingOptions();
    }
    this.ReviewerIdentityServ.userOptions.persistingOptions.HighlightsStyle = style;
    this.ReviewerIdentityServ.SaveOptions();//otherwise they won't persist...
    this.SetHighlights();
  }

  public SetHighlights() {
    if (this.item) {
      const reg0 = new RegExp('\r\n|\r|\n', "g");
      this.HTitle = Helpers.htmlEncode(this.item.title);
      this.HAbstract = Helpers.htmlEncode(this.item.abstract);
      //two values below are used to check if a given term matches the *original* (before adding <spans>) version of our text
      //if it does not, we do not "replace", thus greatly reducing the risk of replacing text coming the bits we have already added to highlight something else
      const tempHAbstract: string = this.HAbstract;
      const tempHTitle: string = this.HTitle;
      this.brAbstract = this.item.abstract;
      //console.log('set highlights called (0): ' + this.brAbstract);
      if (this.ReviewerTermsService && this.ReviewerTermsService.TermsList.length > 0) {
        //console.log('set highlights called: ' + this.HAbstract);
        for (let term of this.ReviewerTermsService.TermsList) {
          //console.log('something to do with the terms list here: ' + this.ReviewerTermsService.TermsList);
          try {
            if (term.reviewerTerm) {//minimum term length is 1 char
              const reg = new RegExp(term.highlightSearchString, "g");
              if (term.included) {
                if (tempHTitle.match(reg))
                  this.HTitle = this.HTitle.replace(reg, "<span class='" + this.ReviewerIdentityServ.userOptions.RelevantTermClass + "'>" + term.reviewerTerm + "</span>");
                if (tempHAbstract.match(reg))
                this.HAbstract = this.HAbstract.replace(reg, "<span class='" + this.ReviewerIdentityServ.userOptions.RelevantTermClass + "'>" + Helpers.htmlEncode(term.reviewerTerm) + "</span>");
              }
              else {
                if (tempHTitle.match(reg))
                  this.HTitle = this.HTitle.replace(reg, "<span class='" + this.ReviewerIdentityServ.userOptions.IrrelevantTermClass + "'>" + term.reviewerTerm + "</span>");
                if (tempHAbstract.match(reg))
                  this.HAbstract = this.HAbstract.replace(reg, "<span class='" + this.ReviewerIdentityServ.userOptions.IrrelevantTermClass + "'>" + Helpers.htmlEncode(term.reviewerTerm) + "</span>");
              }
            }
          }
          catch (error) {
            console.log(error);
            this.ModalService.GenericErrorMessage("Sorry, the terms-highlighting system has encountered a problem. Please inform EPPI-Support.");
          }
        }
      }
      //this.HAbstract = this.HAbstract.replace(reg0, "<br />");
      //this.brAbstract = this.brAbstract.replace(reg0, "<br />");
    }
  }
  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }

  toHTML(text: string): string {
    return text.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
  }
  public FieldsByType(typeId: number) {
    return Helpers.FieldsByPubType(typeId);
  }

  ngOnDestroy() {
    this.selectedText = "";
    if (this.subscr) {
      this.subscr.unsubscribe();
    }
  }
}







