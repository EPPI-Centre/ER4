import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ConfigService } from './config.service';
import { lastValueFrom } from 'rxjs';
import { Helpers } from '../helpers/HelperMethods';

@Injectable({
  providedIn: 'root',
})

export class ReviewerTermsService extends BusyAwareService {

  constructor(
    private _httpC: HttpClient,
    private modalService: ModalService,
    configService: ConfigService
  ) {
    super(configService);
  }
  @Output() setHighlights: EventEmitter<boolean> = new EventEmitter();
  private _TermsList: ReviewerTerm[] = [];
  public get TermsList(): ReviewerTerm[] {
    return this._TermsList;
  }
  public _ShowHideTermsList: boolean = false;

  public Fetch() {
    //console.log("Fetching terms");
    this._BusyMethods.push("Fetch");
    return lastValueFrom(this._httpC.get<iReviewerTerm[]>(this._baseUrl + 'api/ReviewerTermList/Fetch')).then(result => {
      this._TermsList = [];
      for (let iT of result) {
        this._TermsList.push(new ReviewerTerm(iT));
      }
      this.setHighlights.emit();
      this.RemoveBusy("Fetch");
    },
      error => {
        this.modalService.GenericError(error);
        this.RemoveBusy("Fetch");
      }
    );
  }

  public CreateTerm(trt: ReviewerTerm) {
    if (trt.reviewerTerm.trim().length < 1) return;
    this._BusyMethods.push("CreateTerm");
    let body = {
      trainingReviewerTermId: trt.trainingReviewerTermId,
      itemTermDictionaryId: trt.itemTermDictionaryId,
      reviewerTerm: trt.reviewerTerm,
      included: trt.included,
      term: trt.term
    };
    return this._httpC.post<ReviewerTerm>(this._baseUrl + 'api/ReviewerTermList/CreateReviewerTerm',
      body)
      .subscribe(result => {
        //this._TermsList.push(result)
        this.Fetch();
        this.RemoveBusy("CreateTerm");

      },
        error => {
          this.modalService.GenericError(error);
          this.RemoveBusy("CreateTerm");
        },
        () => {
          this.RemoveBusy("CreateTerm");
        }
      );
  }

  public DeleteTerm(termId: number) {

    this._BusyMethods.push("DeleteTerm");
    let body = JSON.stringify({ Value: termId });
    return this._httpC.post<ReviewerTerm>(this._baseUrl + 'api/ReviewerTermList/DeleteReviewerTerm',
      body)
      .subscribe(result => {

        let ind: number = this._TermsList.findIndex(x => x.trainingReviewerTermId == result.trainingReviewerTermId);
        //this._TermsList.splice(ind, 1);
        this.Fetch();
        this.RemoveBusy("DeleteTerm");

      },
        error => {

          this.modalService.GenericError(error);
          this.RemoveBusy("DeleteTerm");
        }, () => {
          this.RemoveBusy("DeleteTerm");
        }
      );
  }

  public UpdateTerm(term: ReviewerTerm) {
    if (term.reviewerTerm.trim().length < 1) return;
    this._BusyMethods.push("UpdateTerm");

    let body = {
      trainingReviewerTermId: term.trainingReviewerTermId,
      itemTermDictionaryId: term.itemTermDictionaryId,
      reviewerTerm: term.reviewerTerm,
      included: term.included,
      term: term.term };
    return this._httpC.post<ReviewerTerm>(this._baseUrl + 'api/ReviewerTermList/UpdateReviewerTerm',
      body)
      .subscribe(result => {

        //let ind: number = this._TermsList.findIndex(x => x.trainingReviewerTermId == term.trainingReviewerTermId);
        //this._TermsList[ind] = result;
        this.Fetch();
        this.RemoveBusy("UpdateTerm");
        //return this.TermsList;
      },
        error => {

          this.modalService.GenericError(error);
          this.RemoveBusy("UpdateTerm");
        }, () => {
          this.RemoveBusy("UpdateTerm");
        }
      );
  }
}
export interface iReviewerTerm {
  trainingReviewerTermId: number;
  itemTermDictionaryId: number;
  reviewerTerm: string;
  included: boolean;
  term: string;
}
export class ReviewerTerm {
  private static spanStrings: string[] = [
    "<span class='RelevantTerm'>",
    "<span class='IrrelevantTerm'>",
    "<span class='RelevantTermER4'>",
    "<span class='IrrelevantTermER4'>",
    "<span class='RelevantTermBW'>",
    "<span class='IrrelevantTermBW'>",
    "<span class='RelevantTermFainter'>",
    "<span class='IrrelevantTermFainter'>",
    "/span>"
  ];
  constructor(iTerm: iReviewerTerm | string = '') {
    if (typeof iTerm == 'string') {
      iTerm = {
        trainingReviewerTermId: 0,
        itemTermDictionaryId: 0,
        reviewerTerm: iTerm,
        included: true,
        term: iTerm
      };
    }

    this.trainingReviewerTermId = iTerm.trainingReviewerTermId;
    this.itemTermDictionaryId = iTerm.itemTermDictionaryId;
    this._reviewerTerm = iTerm.reviewerTerm;
    this.OriginalTerm = iTerm.reviewerTerm;
    this.Originalincluded = iTerm.included;
    this.included = iTerm.included;
    this.term = iTerm.term;
    this.SetHighlightSearchString();
  }
  private SetHighlightSearchString() {

    if (this.reviewerTerm.length == 0) {
      this.highlightSearchString = "";
      return;
    }
    //we can assume our actual term isn't an empty string from now on
    let index: number = -1;
    let SafeReviewerTerm = Helpers.cleanSpecialRegexChars(this.reviewerTerm);
    if (SafeReviewerTerm.substring(0, 1).toUpperCase() !== SafeReviewerTerm.substring(0, 1).toLowerCase()) {
      //we do this if the first char of our term has upper/lower case variants
      //this part of the final regex makes sure we match Uppercase and lowercase versions the same term (thus matching both "Term" and "term"):
      SafeReviewerTerm = "[" + SafeReviewerTerm.substring(0, 1).toUpperCase() + SafeReviewerTerm.substring(0, 1).toLowerCase() + "]" + SafeReviewerTerm.substring(1);
    }
    //now it gets complicated, as we NEED to ensure the spans we add to produce the highlights will NOT match when we'll process the next terms
    let spansToDealWith: string[] = [];
    let safetyCounter: number = 0;
    for (let spanSt of ReviewerTerm.spanStrings) {
      index = spanSt.search(SafeReviewerTerm);
      while (index != -1 && safetyCounter < 100) {
        safetyCounter++;
        const toAdd = spanSt.substring(0, index);
        //console.log("check for adding: ", spansToDealWith.find(f => f == toAdd));
        if (spansToDealWith.find(f => f == toAdd) == undefined) {
          spansToDealWith.push(toAdd);//given this term, if this span appears in the "highlighted title/abstract" the term would be found within the span
          //we can't allow this, so we'll build a regex that uses negative lookbehind to make sure we won't match such spans.
        }
        index = spanSt.indexOf(this.reviewerTerm, index + 1);
      }
    }
    if (spansToDealWith.length == 0) {
      if (SafeReviewerTerm.endsWith('<')) {
        //extra special case - a string ending in ' <' would break the start|end of a span if it's preceded by a space and the rest of the string (if any)
        SafeReviewerTerm += "(?!(span class=')|(/span>)|(br />))"
      }
      this.highlightSearchString = SafeReviewerTerm; //no spans would match, so nothing special to do.
    }
    else if (spansToDealWith.length == 1) {//only one span may match, easy Regex to build
      this.highlightSearchString = "(?<!" + spansToDealWith[0] + ")" + SafeReviewerTerm;
    }
    else {//not so easy, we use a negative lookbehind with all spans found, combined with "OR"
      let tempSt = "(?<!(";
      for (let tSpan of spansToDealWith) {
        if (tempSt.length == 5) {
          tempSt += tSpan;
        }
        else {
          tempSt += ")|(" + tSpan;
        }
      }
      this.highlightSearchString = tempSt + "))" + SafeReviewerTerm;
    }
    console.log("Term search string: ", this.highlightSearchString);
  }
  public trainingReviewerTermId: number;
  public itemTermDictionaryId: number;
  private _reviewerTerm: string;
  public get reviewerTerm(): string {
    return this._reviewerTerm;
  }
  public set reviewerTerm(val: string) {
    this._reviewerTerm = val;
    this.term = val;
    this.SetHighlightSearchString();
  }
  public highlightSearchString: string = "";
  public included: boolean;
  public term: string;
  private OriginalTerm: string = "";
  private Originalincluded: boolean = false;

  public get IsValid(): boolean {
    return this.reviewerTerm.trim().length > 0;
  }
  public get CanSave(): boolean {
    if (!this.IsValid) return false;
    return this.reviewerTerm != this.OriginalTerm || this.included != this.Originalincluded;
  }
}
