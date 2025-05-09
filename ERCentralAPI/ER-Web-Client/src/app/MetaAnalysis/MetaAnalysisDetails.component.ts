import { Component, OnInit,Input, ViewChild, OnDestroy } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MetaAnalysis, MetaAnalysisService } from '../services/MetaAnalysis.service';
import { FilterOutcomesFormComp } from './FilterOutcomesForm.component';
import { MetaAnalysisRunNetworkComp } from './MetaAnalysisRunNetwork.component';

@Component({
  selector: 'MetaAnalysisDetailsComp',
  templateUrl: './MetaAnalysisDetails.component.html',
    providers: [],
  styles: [`
option:disabled {color: rgb(199, 199, 199);}
`]
})
export class MetaAnalysisDetailsComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private MetaAnalysisService: MetaAnalysisService
  ) { }

  
  @ViewChild('FilterOutcomesFormComp') FilterOutcomesFormComp!: FilterOutcomesFormComp;
  @ViewChild('MetaAnalysisRunNetworkComp') MetaAnalysisRunNetworkComp!: MetaAnalysisRunNetworkComp;
  ngOnInit() { }
  public ActivePanel: string = "";
  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }

  public FilterToBeEdited: string = "";

  public get CurrentMA(): MetaAnalysis | null {
    return this.MetaAnalysisService.CurrentMetaAnalysis;
  }
  public get CanSave(): boolean {
    return this.HasWriteRights && this.MetaAnalysisService.CurrentMAhasChanges && this.CurrentMAIsValid;
  }

  public get CurrentMAIsValid(): boolean {
    if (this.CurrentMA == null) return false;
    else if (this.CurrentMA.title == "") return false;
    return true;
  }
  public get CurrentMAIsInvalidMsg(): string {
    if (this.CurrentMAIsValid == true) return "";
    else if (this.CurrentMA && this.CurrentMA.title == "") return "Please give this Meta Analysis a name";
    else return "This Meta Analysis is invalid for unknown reasons";
  }
  public CloseActivePanel() {
    this.FilterToBeEdited = "";
    this.ActivePanel = "";
  }
  public ShowPanel(PanelName: string) {
    if (this.ActivePanel == PanelName) this.CloseActivePanel();
    else this.ActivePanel = PanelName;
  }

  public PleaseEditThisFilter(fieldName: string) {
    if (this.ActivePanel != "EditFilters") {
      this.FilterToBeEdited = fieldName;
      this.ActivePanel = "EditFilters";
    } else {
      if (this.FilterOutcomesFormComp) {
        //if the panel is already open, we ask the child-component to do the change
        this.FilterToBeEdited = fieldName;
        this.FilterOutcomesFormComp.ChangingColumn(fieldName);
      }
    }
  }
  public ChangingMAtype(event: Event) {
    if (this.CurrentMA) {
      const target: HTMLSelectElement = (event.target as HTMLSelectElement);
      let val = target.options[target.options.selectedIndex];
      this.CurrentMA.metaAnalysisTypeTitle = val.text;
      this.MetaAnalysisService.ApplyFilters();
      this.MetaAnalysisService.ApplySavedSorting();
    }
  }

  public async Save() {
    if (!this.CanSave) return;
    if (this.CurrentMA) {
      const ReturnToFilter = this.FilterToBeEdited;
      if (ReturnToFilter != '' && this.ActivePanel == "EditFilters") this.PleaseEditThisFilter('');
      let CurrentNMAReferenceVal = this.CurrentMA.nmaReference;
      let res = await this.MetaAnalysisService.SaveMetaAnalysis(this.CurrentMA);
      if (this.ActivePanel == "EditFilters") {
        if (res != false) {
          this.CurrentMA.nmaReference = CurrentNMAReferenceVal;
          this.PleaseEditThisFilter(ReturnToFilter);
        } else {
          this.PleaseEditThisFilter('');
        }
      }
      else if (this.ActivePanel == "RunNetwork" && res != false) {
        if (this.MetaAnalysisRunNetworkComp) {
          this.MetaAnalysisRunNetworkComp.BuildReferences();
          this.CurrentMA.nmaReference = CurrentNMAReferenceVal;
        }
      }
    }
  }

  ngOnDestroy() { }
}






