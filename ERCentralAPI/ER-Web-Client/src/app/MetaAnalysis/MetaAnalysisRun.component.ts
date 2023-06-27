import { Component, OnInit,Input, ViewChild, OnDestroy, EventEmitter, Output, Inject } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { iMetaAnalysisRunInRCommand, iReference, MetaAnalysis, MetaAnalysisService, Moderator } from '../services/MetaAnalysis.service';
import { FilterOutcomesFormComp } from './FilterOutcomesForm.component';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';
import { Helpers } from '../helpers/HelperMethods';

@Component({
  selector: 'MetaAnalysisRunComp',
  templateUrl: './MetaAnalysisRun.component.html',
  providers: [],
  styles: []
})
export class MetaAnalysisRunComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private MetaAnalysisService: MetaAnalysisService,
    private sanitizer: DomSanitizer,
    @Inject('BASE_URL') private _baseUrl: string
  ) { }

  @Output() PleaseCloseMe = new EventEmitter();
  ngOnInit() {
    if (this.MAreportSource && this.MAreportSource.metaAnalaysisObject.analysisType == 1) this.MetaAnalysisService.Clear(true);
    if (this.CurrentMA) this.CurrentMA.analysisType = 0;
  }
  public HideReport: boolean = false;
  public ActivePanel: string = "";
  public SigLevMin: number = 75;
  public SigLevMax: number = 99;
  public decPlacesMin: number = 2;
  public decPlacesMax: number = 8;

  
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

  public CloseActivePanel() {
    this.ActivePanel = "";
  }
  public ShowPanel(PanelName: string) {
    if (this.ActivePanel == PanelName) this.CloseActivePanel();
    else this.ActivePanel = PanelName;
  }

  public SaveReport() {
    let tmp = document.getElementById('MAreportContent');
    if (tmp) {
      let partialFilename = " - ";
      if (this.MAreportSource) partialFilename += this.MAreportSource.metaAnalaysisObject.title;
      const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(tmp.outerHTML, this._baseUrl, "Items Table"));
      saveAs(dataURI, "Meta Analysis Report" + partialFilename + ".html");
    }
  }
  ChangingModel(event: Event) {
    this.EnableDisableKNHA();
  }
   EnableDisableKNHA() {
    //if (cbModel != null && cbModel.SelectedIndex == 0) {
    //  cbKNHA.IsChecked = false;
    //  cbKNHA.IsEnabled = false;
    //}
    //else {
    //  if (cbKNHA != null)
    //    cbKNHA.IsEnabled = true;
    //}
  }
  public get knhaIsDisabled(): boolean {
    if (!this.CurrentMA) return true;
    else if (this.CurrentMA.statisticalModel == 0) {
      if (this.CurrentMA.knha == true) setTimeout(() => { if (this.CurrentMA) this.CurrentMA.knha = false; }, 5);
      return true;
    }
    else return false;
  }
  public Run() {
    if (this.CurrentMA) {
      this.CurrentMA.analysisType = 0;
      this.HideReport = false;
      this.MetaAnalysisService.RunMetaAnalysis(this.CurrentMA);
    }
  }

  public SigLevLostFocus() {
    if (this.CurrentMA) {
      const check = parseInt(this.CurrentMA.significanceLevel.toString());
      if (check == NaN) this.CurrentMA.significanceLevel = 95;
      else if (check > this.SigLevMax) this.CurrentMA.significanceLevel = this.SigLevMax;
      else if (check < this.SigLevMin) this.CurrentMA.significanceLevel = this.SigLevMin;
      else if (check != this.CurrentMA.significanceLevel) this.CurrentMA.significanceLevel = check;
    }
  }
  public DecPlacesLostFocus() {
    if (this.CurrentMA) {
      const check = parseInt(this.CurrentMA.decPlaces.toString());
      if (check == NaN) this.CurrentMA.decPlaces = 4;
      else if (check > this.decPlacesMax) this.CurrentMA.decPlaces = this.decPlacesMax;
      else if (check < this.decPlacesMin) this.CurrentMA.decPlaces = this.decPlacesMin;
      else if (check != this.CurrentMA.decPlaces) this.CurrentMA.decPlaces = check;
    }
  }

  public CloseMe() {
    this.PleaseCloseMe.emit();
  }
  ngOnDestroy() { }
}






