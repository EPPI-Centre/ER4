import { Component, OnInit,Input, ViewChild, OnDestroy, EventEmitter, Output, Inject } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { IdAndNamePair, iMetaAnalysisRunInRCommand, iReference, MetaAnalysis, MetaAnalysisService, Moderator, NMAmatrixRow } from '../services/MetaAnalysis.service';
import { FilterOutcomesFormComp } from './FilterOutcomesForm.component';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';
import { Helpers } from '../helpers/HelperMethods';
import { ExtendedOutcome, Outcome } from '../services/outcomes.service';
import { last, single } from 'rxjs';

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
  public networks: string[][] = [];
  public connectedOutcomes: ExtendedOutcome[][] = [];
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
      //here we try to check if each outcome is part of a unique network, which is the same as finding out how many distinct network we have
      //for each outcome, see if you can connect it to something
      //if you can't it goes in its own solitary network
      //if you can, see if one of its elements is in an existing network, but the other isn't
      //if it is, add the other to existing network and check if you need to merge this network
      //ELSE if both elements are in the SAME existing network, move on
      //ELSE, if one element is in one network, and the other in a different network, merge the networks
      //else, it's connected to something, but not in an already known network, so add a new network
      this.networks = [];
      let singletons: ExtendedOutcome[] = [];
      let DoneOutcomes: ExtendedOutcome[] = [];
      let DoneInterventions: string[] = [];
      let DoneControls: string[] = [];

      for (let o of outcomes) {
        DoneOutcomes.push(o);
        let cnt = outcomes.filter(f => o.interventionText == f.interventionText || o.controlText == f.controlText).length;
        if (cnt == 1) {
          singletons.push(o);//it's the same outcome we're dealing with
        }
        else {
          let indexOfInterventionNetw = this.IndexofNetwork(o.interventionText);
          let indexOfControlNetw = this.IndexofNetwork(o.controlText);
          if (indexOfInterventionNetw == -1 && indexOfControlNetw != -1) {
            //add intervention to network where control is
            this.networks[indexOfControlNetw].push(o.interventionText);
          }
          else if (indexOfInterventionNetw != -1 && indexOfControlNetw == -1) {
            //add control to network where intervention is
            this.networks[indexOfInterventionNetw].push(o.controlText);
          }
          else if (indexOfInterventionNetw != -1 && indexOfControlNetw != -1) {
            //both sides are already somewhere
            if (indexOfControlNetw != indexOfInterventionNetw) {
              //merge these two networks!
              const biggerI = indexOfControlNetw > indexOfInterventionNetw ? indexOfControlNetw : indexOfInterventionNetw;
              const smallerI = indexOfControlNetw < indexOfInterventionNetw ? indexOfControlNetw : indexOfInterventionNetw;
              for (let el of this.networks[biggerI]) {
                if (this.networks[smallerI].indexOf(el) == -1) this.networks[smallerI].push(el);
              }
              this.networks.splice(biggerI, 1);
            }
            else {
              //nothing to do
            }
          }
          else {
            //both sides are in no existing network:
            //new network
            let NN: string[] = [];
            NN.push(o.interventionText, o.controlText);
            this.networks.push(NN);
          }
        }
      }


      for (let sing of singletons) {
        let NewN: string[] = [];
        NewN.push(sing.interventionText, sing.controlText);
        this.networks.push(NewN);
        let NewCO: ExtendedOutcome[] = [];
        NewCO.push(sing);
        this.connectedOutcomes.push(NewCO);
      }
    }
  }
  private IndexofNetwork(val: string): number {
    if (this.networks.length == 0) return -1;
    for (let i = 0; i < this.networks.length; i++) {
      if (this.networks[i].indexOf(val) != -1) return i;
    }
    return -1;
  }

  public CloseMe() {
    this.PleaseCloseMe.emit();
  }
  ngOnDestroy() { }
}






