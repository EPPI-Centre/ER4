import { Component, OnInit,Input, ViewChild, OnDestroy, EventEmitter, Output, Inject } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { iMetaAnalysisRunInRCommand, iReference, MetaAnalysis, MetaAnalysisService, Moderator, NMAmatrixRow } from '../services/MetaAnalysis.service';
import { FilterOutcomesFormComp } from './FilterOutcomesForm.component';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';
import { Helpers } from '../helpers/HelperMethods';

@Component({
  selector: 'MetaAnalysisRunNetworkComp',
  templateUrl: './MetaAnalysisRunNetwork.component.html',
  providers: [],
  styles: []
})
export class MetaAnalysisRunNetworkComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private MetaAnalysisService: MetaAnalysisService,
    private sanitizer: DomSanitizer,
    @Inject('BASE_URL') private _baseUrl: string
  ) { }

  @Output() PleaseCloseMe = new EventEmitter();
  ngOnInit() {
    this.BuildReferences();
  }
  public HideReport: boolean = false;

  public get CurrentMA(): MetaAnalysis | null {
    return this.MetaAnalysisService.CurrentMetaAnalysis;
  }
  public get MAreportSource(): iMetaAnalysisRunInRCommand | null {
    return this.MetaAnalysisService.MAreportSource;
  }
  public get MAreportHTML(): SafeHtml | null {
    if (this.MetaAnalysisService.MAreport) return this.sanitizer.bypassSecurityTrustHtml(this.MetaAnalysisService.MAreport);
    else return null;
  }
  private _references: iReference[] = [];
  public get References(): iReference[] {
    if (this._references.length == 0) this.BuildReferences();
    return this._references;
  }
  private _NMAmatrixRows: NMAmatrixRow[] = [];
  public get NMAmatrixRows(): NMAmatrixRow[] {
    return this._NMAmatrixRows;
  }

  private BuildReferences() {
    if (!this.CurrentMA) return;
    this.CurrentMA.analysisType = 1;
    if (this.MAreportSource && this.MAreportSource.metaAnalaysisObject.analysisType == 0) this.MetaAnalysisService.Clear(true);
    this._references = [];
    for (let mam of this.CurrentMA.metaAnalysisModerators) {
      if (mam.fieldName == "InterventionText") this._references = mam.references;
      break;
    }
    if (this._references.length > 0) this.CurrentMA.nmaReference = this._references[0].name;
  }
  public SaveReport() {
    let tmp = document.getElementById('MAreportContent');
    if (tmp) {
      let partialFilename = " - ";
      if (this.MAreportSource) partialFilename += this.MAreportSource.metaAnalaysisObject.title;
      const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(tmp.outerHTML, this._baseUrl, "Network Meta Analysis"));
      saveAs(dataURI, "Network Meta Analysis Report" + partialFilename + ".html");
    }
  }
  
  public Run() {
    if (this.CurrentMA) {
      this.HideReport = false;
      this.MetaAnalysisService.RunMetaAnalysis(this.CurrentMA);
    }
  }

  public BuildNMAmatrix() {
    this._NMAmatrixRows = [];
    if (this.CurrentMA) {
      const outcomes = this.MetaAnalysisService.FilteredOutcomes.filter(f => f.isSelected == true);
      let interventions: string[] = [];
      for (let o of outcomes) {
        if (interventions.indexOf(o.interventionText) == -1) {
          interventions.push(o.interventionText);
          const Fouts = outcomes.filter(f => f.interventionText == o.interventionText);
          for (let oo of Fouts) {
            const IndexOfRelevantRow = this._NMAmatrixRows.findIndex(f => f.intervention == o.interventionText); 
            if (IndexOfRelevantRow == -1) {
              let row = new NMAmatrixRow()
              row.intervention = o.interventionText;
              row.comparator.push({ Id: 1, Name: oo.controlText });
              this._NMAmatrixRows.push(row);
            } else {
              const IndexOfRelevantComparator = this._NMAmatrixRows[IndexOfRelevantRow].comparator.findIndex(f => f.Name == oo.controlText);
              if (IndexOfRelevantComparator == -1) {
                this._NMAmatrixRows[IndexOfRelevantRow].comparator.push({ Id: 1, Name: oo.controlText });
              } else {
                this._NMAmatrixRows[IndexOfRelevantRow].comparator[IndexOfRelevantComparator].Id = this._NMAmatrixRows[IndexOfRelevantRow].comparator[IndexOfRelevantComparator].Id + 1;
              }
            }
          }
        }
      }

    }
  }

  public CloseMe() {
    this.PleaseCloseMe.emit();
  }
  ngOnDestroy() { }
}






