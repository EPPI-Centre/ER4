import { Component, Inject, OnInit, Output, Input, OnDestroy, } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, ReviewSet } from '../services/ReviewSets.service';
import { EventEmitter, HostListener } from '@angular/core';
import { ReviewSetsEditingService, ReadOnlyTemplateReview } from '../services/ReviewSetsEditing.service';
import { Helpers } from '../helpers/HelperMethods';

@Component({
  selector: 'importCodesetsWizard',
  styles: [``],
  templateUrl: './importCodesetsWizard.component.html'
})

export class ImportCodesetsWizardComponent implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private ReviewerIdentityServ: ReviewerIdentityService,
    private ReviewSetsService: ReviewSetsService,
    private ReviewSetsEditingService: ReviewSetsEditingService
  ) { }


  //@Input() attributesOnly: boolean = false;

  ngOnInit() {

    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 ||
      this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
      this.router.navigate(['home']);
    }
    else if (!this.ReviewerIdentityServ.HasWriteRights) {
      this.router.navigate(['Main']);
    }
    else {
      this.getScreenSize(null);
      this.ReviewSetsEditingService.FetchReviewTemplates();
    }
  }
  @Input() IsStandalone: boolean = true;
  @Output()
  PleaseCloseMe = new EventEmitter();
  private screenHeight: number = 0;
  @HostListener('window:resize', ['$event'])
  getScreenSize(event: any) {
    this.screenHeight = window.innerHeight;
  }
  public get codingtoolsMaxHeight(): number {
    return this.screenHeight * 0.7;
  }
  public WizStep: number = 1;
  public get TemplateReviews(): ReadOnlyTemplateReview[] {
    return this.ReviewSetsEditingService.ReadOnlyTemplateReviews;
  }
  private _selectedTemplate: ReadOnlyTemplateReview | null = null;
  public get SelectedTemplate(): ReadOnlyTemplateReview | null {
    if (this._selectedTemplate == null &&
      this.ReviewSetsEditingService.ReadOnlyTemplateReviews && this.ReviewSetsEditingService.ReadOnlyTemplateReviews.length > 0) {
      this._selectedTemplate = this.ReviewSetsEditingService.ReadOnlyTemplateReviews[0];
    }
    return this._selectedTemplate;
  }
  private _SelectedSet4Copy: ReviewSet | null = null;
  public get SelectedSet4Copy(): ReviewSet | null {
    return this._SelectedSet4Copy;
  }
  IsServiceBusy(): boolean {
    if (this.ReviewSetsEditingService.IsBusy || this.ReviewSetsService.IsBusy) return true;
    else return false;
  }
  CanWrite(): boolean {
    if (!this.ReviewerIdentityServ.HasWriteRights) return false;
    else return !this.IsServiceBusy();
  }
  CancelActivity() {
    if (this.WizStep == 1) {
      if (this.IsStandalone) this.BackToMain();
      else {
        //somehow close itself...
        this.PleaseCloseMe.emit();
      }
    }
    if (this.WizStep >= 2) {
      this.WizStep = 1;
      this.ReviewSetsEditingService.clearReviewSets4Copy();
      this._SelectedSet4Copy = null;
    }
  }
  async ProceedClicked() {
    let roTr = this._selectedTemplate;
    if (!roTr) return;
    if (
      (roTr.templateName == "Manually pick from Public codesets..." && roTr.templateId == 1000)
      ||
      (roTr.templateName == "Manually pick from your own codesets..." && roTr.templateId == 2000)
    ) {
      this.OpenListOfSets(roTr);
    }
    else if (roTr.templateName.length > 0 && roTr.templateId > 0 && roTr.reviewSetIds && roTr.reviewSetIds.length > 0) {
      //do the copy thing, selfclose when done
      await this.ReviewSetsEditingService.ImportReviewTemplate(roTr);
      console.log("finished waiting for codesets import...")
      let cycle: number = 0;
      //SUPER UGLY: donkey alert!!!
      await Helpers.Sleep(500);
      while (this.ReviewSetsEditingService.IsBusy && cycle < 600) {
        cycle++;
        await Helpers.Sleep(100);
      }
      this.BackToMain();
    }
  }

  private _codingTools: string = 'All';

  public get codingTools(): string {
    return this._codingTools;
  }
  public set codingTools(val: string) {
    this._codingTools = val;
    this.CheckSelectedTool();
  }

  private _FilterByName: string = "";
  public get FilterByName(): string {
    return this._FilterByName;
  }
  public set FilterByName(val: string) {
    this._FilterByName = val;
    this.CheckSelectedTool();
  }
  private CheckSelectedTool() {
    const tmp = this.getRelevantTools();
    if (this._SelectedSet4Copy) {
      if (!tmp.find(f => f == this._SelectedSet4Copy)) {
        if (tmp.length > 0) this._SelectedSet4Copy = tmp[0];
        else this._SelectedSet4Copy = null;
      }
    } else if (tmp.length > 0) {
      this._SelectedSet4Copy = tmp[0];
    }
  }

  public get AllReviewSets4Copy(): ReviewSet[] {
    return this.ReviewSetsEditingService.ReviewSets4Copy;
  }
  private _SceeningSets4Copy: ReviewSet[] = [];
  private _StandardSets4Copy: ReviewSet[] = [];
  private _AdminSets4Copy: ReviewSet[] = [];


  public getRelevantTools() {
    let res = this.AllReviewSets4Copy;
    if (this._codingTools == "Standard") {
      res = this._StandardSets4Copy;
    } else if (this._codingTools == "Screening") {
      res = this._SceeningSets4Copy;
    } else if (this._codingTools == "Administration") {
      res = this._AdminSets4Copy;
    }
    if (this.FilterByName != "") {
      const lowerCase = this.FilterByName.toLowerCase();
      res = res.filter(f => (f.set_name.toLowerCase().indexOf(lowerCase) != -1));
    }
    
    return res;
  }


  public async OpenListOfSets(roTr: ReadOnlyTemplateReview) {
    let res: boolean = false;
    if (roTr.templateName == "Manually pick from Public codesets..." && roTr.templateId == 1000) {
      this.WizStep = 2.1;
      res = await this.ReviewSetsEditingService.FetchReviewSets4Copy(false);
    }
    else if (roTr.templateName == "Manually pick from your own codesets..." && roTr.templateId == 2000) {
      this.WizStep = 2.2;
      res = await this.ReviewSetsEditingService.FetchReviewSets4Copy(true);
    }
    if (res == true) {
      this._StandardSets4Copy = this.AllReviewSets4Copy.filter(f => f.subTypeName == "Standard");
      this._SceeningSets4Copy = this.AllReviewSets4Copy.filter(f => f.subTypeName == "Screening");
      this._AdminSets4Copy = this.AllReviewSets4Copy.filter(f => f.subTypeName == "Administration");
    } else {
      this._StandardSets4Copy = [];
      this._SceeningSets4Copy = [];
      this._AdminSets4Copy = [];
    }
  }
  SelectTemplate(ID: number) {
    if (!this.ReviewSetsEditingService.ReadOnlyTemplateReviews || this.ReviewSetsEditingService.ReadOnlyTemplateReviews.length == 0) {
      this._selectedTemplate = null;
    }
    else {
      let res = this.ReviewSetsEditingService.ReadOnlyTemplateReviews.find(res => res.templateId == ID);
      if (res) {
        this._selectedTemplate = res;
      }
      else {
        this._selectedTemplate = this.ReviewSetsEditingService.ReadOnlyTemplateReviews[0];
      }
    }
  }
  SelectSet4Copy(set: ReviewSet) {
    this._SelectedSet4Copy = set;
  }




  async ImportSelectedSet() {
    if (!this._SelectedSet4Copy) return;
    else {
      this.ReviewSetsEditingService.ReviewSetCopy(this._SelectedSet4Copy.reviewSetId, this.ReviewSetsService.ReviewSets.length).then(
        (result) => {
          if (result.reviewSetId < 0) {
            console.log("Copy single codeset failed (in service):", this._SelectedSet4Copy, result);
          }
          else this.ReviewSetsService.GetReviewSets();
        }
        , (reject) => {
          console.log("Copy single codeset failed (reject):", this._SelectedSet4Copy, reject);
        }

      ).catch(error => {
        console.log("Copy single codeset failed (catch):", this._SelectedSet4Copy, error);
      });
    }
  }
  BackToMain() {
    this.router.navigate(['Main']);
  }
  ngOnDestroy() {
    this.ReviewSetsEditingService.clearReviewSets4Copy();
  }
}
