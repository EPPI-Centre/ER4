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
  public IntervAndCompNetworks: string[][] = [];
  public connectedOutcomes: ExtendedOutcome[][] = [];
  public incompleteOutcomes: ExtendedOutcome[] = [];
  private _MappedOutcomes: ExtendedOutcome[] = [];

  public showIncompleteOutcomesHelp: boolean = false;

  public get DataIsMapped(): boolean {
    //console.log("DataIsMapped?", this.incompleteOutcomes.length);
    if (this._MappedOutcomes) {
      const ToCompare0 = this.MetaAnalysisService.FilteredOutcomes.filter(f => f.isSelected == true && (f.interventionText == '' || f.controlText == ''));
      if (this.incompleteOutcomes.length != ToCompare0.length) {
        if (this.incompleteOutcomes.length > 0 || ToCompare0.length > 0) this.ClearMap();
        return false;
      }
      for (let i = 0; i < this.incompleteOutcomes.length; i++) {
        if (this.incompleteOutcomes[i] != ToCompare0[i]) {
          if (this.incompleteOutcomes.length > 0 || ToCompare0.length > 0) this.ClearMap();
          return false;
        }
      }
      const ToCompare = this.MetaAnalysisService.FilteredOutcomes.filter(f => f.isSelected == true && f.interventionText != '' && f.controlText != '');
      if (this._MappedOutcomes.length != ToCompare.length) {
        this.ClearMap(); return false;
      }
      for (let i = 0; i < this._MappedOutcomes.length; i++) {
        if (this._MappedOutcomes[i] != ToCompare[i]) {
          this.ClearMap(); return false;
        }
      }
    }
    if (this._NMAmatrixRows.length > 0 || this.IntervAndCompNetworks.length > 0
      || this.connectedOutcomes.length > 0 || this.incompleteOutcomes.length > 0) return true;
    else {
      this.ClearMap(); return false;
    }
  }

  public get canRunNMA(): number {
    // -1: not enough outcomes selected; 0: NMA can run,
    // 1: need to "map outcomes" 1st, 2: please unselect incomplete outcomes, 3 please unselect extra networks, only one network can be analysed at a given time
    //4: there is no network to analyse, 5: not enough elements in the one network that exists; 6: the "reference" selected is not in the network
    if (this.MetaAnalysisService.FilteredOutcomes.filter(f => f.isSelected == true).length < 2) return -1;
    else if (this.DataIsMapped == false) return 1;
    else if (this.incompleteOutcomes.length > 0) return 2;
    else if (this.IntervAndCompNetworks.length > 1) return 3;
    else if (this.IntervAndCompNetworks.length == 0) return 4;
    else if (this.IntervAndCompNetworks[0].length < 3) return 5;
    else if (this.CurrentMA && this.IntervAndCompNetworks[0].indexOf(this.CurrentMA.nmaReference) == -1) return 6;
    else return 0;
  }
  private ClearMap() {
    //console.log("ClearMap?", this.incompleteOutcomes.length);
    this._MappedOutcomes = [];
    this._NMAmatrixRows = [];
    this.IntervAndCompNetworks = [];
    this.connectedOutcomes = [];
    this.incompleteOutcomes = [];
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
    //console.log("BuildNMAmatrix", this.incompleteOutcomes.length);
    this._NMAmatrixRows = [];
    this.incompleteOutcomes = [];
    this.connectedOutcomes = [];
    this._MappedOutcomes = [];
    if (this.CurrentMA) {
      //identify all outcomes without an intervention or a control
      const PartialOutcomes = this.MetaAnalysisService.FilteredOutcomes.filter(f => f.isSelected == true && (f.interventionText == '' || f.controlText == ''));
      this.incompleteOutcomes = PartialOutcomes;
      const outcomes = this.MetaAnalysisService.FilteredOutcomes.filter(f => f.isSelected == true && f.interventionText != '' && f.controlText != '');
      this._MappedOutcomes = outcomes;
      //build the 1st table matching Interventions with Comparators
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
      this.IntervAndCompNetworks = [];
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
            this.IntervAndCompNetworks[indexOfControlNetw].push(o.interventionText);
            this.connectedOutcomes[indexOfControlNetw].push(o);
          }
          else if (indexOfInterventionNetw != -1 && indexOfControlNetw == -1) {
            //add control to network where intervention is
            this.IntervAndCompNetworks[indexOfInterventionNetw].push(o.controlText);
            this.connectedOutcomes[indexOfInterventionNetw].push(o);
          }
          else if (indexOfInterventionNetw != -1 && indexOfControlNetw != -1) {
            //both sides are already somewhere
            if (indexOfControlNetw != indexOfInterventionNetw) {
              //merge these two networks!
              const biggerI = indexOfControlNetw > indexOfInterventionNetw ? indexOfControlNetw : indexOfInterventionNetw;
              const smallerI = indexOfControlNetw < indexOfInterventionNetw ? indexOfControlNetw : indexOfInterventionNetw;
              for (let el of this.IntervAndCompNetworks[biggerI]) {
                if (this.IntervAndCompNetworks[smallerI].indexOf(el) == -1) this.IntervAndCompNetworks[smallerI].push(el);
              }
              this.IntervAndCompNetworks.splice(biggerI, 1);
              for (let el of this.connectedOutcomes[biggerI]) {
                if (this.connectedOutcomes[smallerI].indexOf(el) == -1) this.connectedOutcomes[smallerI].push(el);
              }
              if (this.connectedOutcomes[smallerI].indexOf(o) == -1) this.connectedOutcomes[smallerI].push(o);
              this.connectedOutcomes.splice(biggerI, 1);
            }
            else {
              //just put the outcome in the correct "network"...
              this.connectedOutcomes[indexOfControlNetw].push(o);
            }
          }
          else {
            //both sides are in no existing network:
            //new network
            let NN: string[] = [];
            NN.push(o.interventionText, o.controlText);
            this.IntervAndCompNetworks.push(NN);
            let NNo: ExtendedOutcome[] = [];
            NNo.push(o);
            this.connectedOutcomes.push(NNo);
          }
        }
      }


      for (let sing of singletons) {
        let NewN: string[] = [];
        NewN.push(sing.interventionText, sing.controlText);
        this.IntervAndCompNetworks.push(NewN);
        let NewCO: ExtendedOutcome[] = [];
        NewCO.push(sing);
        this.connectedOutcomes.push(NewCO);
      }
    }

    //console.log("BuildNMAmatrix 2", this.incompleteOutcomes.length);
  }
  private IndexofNetwork(val: string): number {
    if (this.IntervAndCompNetworks.length == 0) return -1;
    for (let i = 0; i < this.IntervAndCompNetworks.length; i++) {
      if (this.IntervAndCompNetworks[i].indexOf(val) != -1) return i;
    }
    return -1;
  }

  public UnselectIncompleteOutcomes() {
    for (let o of this.incompleteOutcomes) {
      o.isSelected = false;
    }
    this.BuildNMAmatrix();
  }

  public UnselectThisNetwork(index: number) {
    const ToUnselect = this.connectedOutcomes[index];
    if (ToUnselect) {
      for (let o of ToUnselect) {
        o.isSelected = false;
      }
      this.BuildNMAmatrix();
    }
  }

  public CloseMe() {
    this.PleaseCloseMe.emit();
  }
  ngOnDestroy() { }
}






