import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ConfigService } from './config.service';
import { first, forEach } from 'lodash';
import { iExtendedOutcome, ExtendedOutcome } from './outcomes.service';
import { CustomSorting, LocalSort } from '../helpers/CustomSorting';
import { lastValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})

export class MetaAnalysisService extends BusyAwareService {

  constructor(
    private _httpC: HttpClient,
    private modalService: ModalService,
    configService: ConfigService
  ) {
    super(configService);
  }

  public MetaAnalysisList: MetaAnalysis[] = [];

  public CurrentMetaAnalysis: MetaAnalysis | null = null;
  private CurrentMetaAnalysisUnchanged: MetaAnalysis | null = null;

  public ColumnVisibility: DynamicColumnsOutcomes = new DynamicColumnsOutcomes();

  public LocalSort: LocalSort = new LocalSort();
  //private UnsortedOutcomesList: ExtendedOutcome[] = [];

  private _FilteredOutcomes: ExtendedOutcome[] = [];
  public get FilteredOutcomes(): ExtendedOutcome[] {
    return this._FilteredOutcomes;
  }

  public UnSortOutcomes(): void {
    if (this.CurrentMetaAnalysis == null || this.CurrentMetaAnalysisUnchanged == null) return;
    
    const tArr = this._FilteredOutcomes.concat();
    this._FilteredOutcomes = [];
    for (let iO of this.CurrentMetaAnalysis.outcomes) {
      //let Oc: ExtendedOutcome = new ExtendedOutcome(iO);
      if (tArr.findIndex(f => f.outcomeId == iO.outcomeId) > -1) this._FilteredOutcomes.push(iO);
    }
    this.CurrentMetaAnalysis.sortDirection = this.CurrentMetaAnalysisUnchanged.sortDirection;
    this.CurrentMetaAnalysis.sortedBy = "";
    this.LocalSort = new LocalSort();
  }
  public SortOutcomesBy(fieldName: string) {
    if (this.CurrentMetaAnalysis == null) return;
    if (this.LocalSort.SortBy == fieldName && this.LocalSort.Direction == false) this.UnSortOutcomes();
    else {
      this._FilteredOutcomes = CustomSorting.SortBy(fieldName, this._FilteredOutcomes, this.LocalSort);
      if (this.LocalSort.Direction) this.CurrentMetaAnalysis.sortDirection = "Ascending";
      else this.CurrentMetaAnalysis.sortDirection = "Descending";
      this.CurrentMetaAnalysis.sortedBy = MetaAnalysisService.ER4ColNameFromFieldName(fieldName, true);
    }
  }


  public get CurrentMAhasChanges(): boolean {
    //console.log("CurrentMA:", this.CurrentMetaAnalysis?.metaAnalysisTypeId ,this.CurrentMetaAnalysis?.metaAnalysisTypeTitle);
    if (this.CurrentMetaAnalysis == null || this.CurrentMetaAnalysisUnchanged == null) return false;
    else if (this.CurrentMetaAnalysis.title !== this.CurrentMetaAnalysisUnchanged.title
      || this.CurrentMetaAnalysis.metaAnalysisTypeId !== this.CurrentMetaAnalysisUnchanged.metaAnalysisTypeId
      || this.CurrentMetaAnalysis.sortDirection !== this.CurrentMetaAnalysisUnchanged.sortDirection
      || this.CurrentMetaAnalysis.sortedBy !== this.CurrentMetaAnalysisUnchanged.sortedBy
      || this.CurrentMetaAnalysis.attributeIdQuestion !== this.CurrentMetaAnalysisUnchanged.attributeIdQuestion
      || this.CurrentMetaAnalysis.attributeIdAnswer !== this.CurrentMetaAnalysisUnchanged.attributeIdAnswer
      || this.CurrentMetaAnalysis.title !== this.CurrentMetaAnalysisUnchanged.title
    ) return true;
    if (this.CurrentMetaAnalysis.filterSettingsList.filter(f => f.isClear == false).length != this.CurrentMetaAnalysisUnchanged.filterSettingsList.filter(f => f.isClear == false).length) return true;
    for (let i = 0; i < this.CurrentMetaAnalysis.filterSettingsList.length; i++) {
      if (this.CurrentMetaAnalysis.filterSettingsList[i].columnName != this.CurrentMetaAnalysisUnchanged.filterSettingsList[i].columnName
        || this.CurrentMetaAnalysis.filterSettingsList[i].filter1 != this.CurrentMetaAnalysisUnchanged.filterSettingsList[i].filter1
        || this.CurrentMetaAnalysis.filterSettingsList[i].filter1CaseSensitive != this.CurrentMetaAnalysisUnchanged.filterSettingsList[i].filter1CaseSensitive
        || this.CurrentMetaAnalysis.filterSettingsList[i].filter1Operator != this.CurrentMetaAnalysisUnchanged.filterSettingsList[i].filter1Operator
        || this.CurrentMetaAnalysis.filterSettingsList[i].filter2 != this.CurrentMetaAnalysisUnchanged.filterSettingsList[i].filter2
        || this.CurrentMetaAnalysis.filterSettingsList[i].filter2CaseSensitive != this.CurrentMetaAnalysisUnchanged.filterSettingsList[i].filter2CaseSensitive
        || this.CurrentMetaAnalysis.filterSettingsList[i].filter2Operator != this.CurrentMetaAnalysisUnchanged.filterSettingsList[i].filter2Operator
        || this.CurrentMetaAnalysis.filterSettingsList[i].selectedValues != this.CurrentMetaAnalysisUnchanged.filterSettingsList[i].selectedValues
        || this.CurrentMetaAnalysis.filterSettingsList[i].metaAnalysisFilterSettingId != this.CurrentMetaAnalysisUnchanged.filterSettingsList[i].metaAnalysisFilterSettingId
      ) return true;
    }
    for (let i = 0; i < this.CurrentMetaAnalysis.outcomes.length; i++) {
      if (this.CurrentMetaAnalysis.outcomes[i].isSelected != this.CurrentMetaAnalysisUnchanged.outcomes[i].isSelected) return true;
    }

    return false;
  }
  public DiscardMAChanges() {
    if (this.CurrentMetaAnalysisUnchanged) this.CurrentMetaAnalysis = new MetaAnalysis(this.CurrentMetaAnalysisUnchanged);
  }
  public FetchMAsList() {
    this._BusyMethods.push("FetchMAsList");
    this.MetaAnalysisList = [];
    this._httpC.get<iMetaAnalysis[]>(this._baseUrl + 'api/MetaAnalysis/GetMAList')
      .subscribe(
        (res) => {
          this.RemoveBusy("FetchMAsList");
          for (let iMa of res) {
            let MA: MetaAnalysis = new MetaAnalysis(iMa);
            this.MetaAnalysisList.push(MA);
          }
        }
        , (err) => {
          this.RemoveBusy("FetchMAsList");
          this.modalService.GenericError(err);
        }
      );
  }
  public FetchMetaAnalysis(crit: MetaAnalysisSelectionCrit) {
    this._BusyMethods.push("FetchMetaAnalysis");
    this._httpC.post<iMetaAnalysis>(this._baseUrl + 'api/MetaAnalysis/FetchMetaAnalysis',
      crit).subscribe(result => {
        this.CurrentMetaAnalysis = new MetaAnalysis(result);
        this.CurrentMetaAnalysisUnchanged = new MetaAnalysis(result);
        if (crit.GetAllDetails == true) {
          this.CalculateColsVisibility();
          this.ApplyFilters();
          this.ApplySavedSorting();
        }
        this.RemoveBusy("FetchMetaAnalysis");
      }, error => {
        this.RemoveBusy("FetchMetaAnalysis");
        this.modalService.GenericError(error);
      });
  }
  
  public FetchEmptyMetaAnalysis() {
    this._BusyMethods.push("FetchEmptyMetaAnalysis");
    this._httpC.get<iMetaAnalysis>(this._baseUrl + 'api/MetaAnalysis/FetchEmptyMetaAnalysis',).subscribe(result => {
        this.CurrentMetaAnalysis = new MetaAnalysis(result);
        this.CurrentMetaAnalysisUnchanged = new MetaAnalysis(result);
        this.CalculateColsVisibility();
        this.ApplyFilters();
        this.ApplySavedSorting();
        this.RemoveBusy("FetchEmptyMetaAnalysis");
      }, error => {
        this.RemoveBusy("FetchEmptyMetaAnalysis");
        this.modalService.GenericError(error);
      });
  }
  
  public SaveMetaAnalysis(MA: MetaAnalysis): Promise<MetaAnalysis | boolean> {
    this._BusyMethods.push("SaveMetaAnalysis");
    const ToSend: iMetaAnalysis = MA.ToiMetaAnalysis();
    return lastValueFrom(this._httpC.post<iMetaAnalysis>(this._baseUrl + 'api/MetaAnalysis/SaveMetaAnalysis', ToSend)
    ).then((res) => {
      let returned = new MetaAnalysis(res);
      let ind = this.MetaAnalysisList.findIndex(f => f.metaAnalysisId == returned.metaAnalysisId)
      if (ind == -1) {
        this.MetaAnalysisList.push(returned);
      }
      else {
        this.MetaAnalysisList.splice(ind, 1, returned);
      }
      this.MetaAnalysisList = this.MetaAnalysisList.concat();//ensures the UI notices we've changed something...
      this.CurrentMetaAnalysis = returned;
      this.CurrentMetaAnalysisUnchanged = new MetaAnalysis(res);
      this.CalculateColsVisibility();
      this.ApplyFilters();
      this.ApplySavedSorting();
      this.RemoveBusy("SaveMetaAnalysis");
      return returned;
    },
      (err) => {
        console.log("Error SaveMetaAnalysis:", err);
        this.RemoveBusy("SaveMetaAnalysis");
        this.modalService.GenericError(err);
        return false;
      }).catch(
        (error) => {
          this.modalService.GenericErrorMessage("Saving this Meta Analysis produced this error: " + error);
          this.RemoveBusy("SaveMetaAnalysis");
          return false;
        });
  }


  public DeleteMetaAnalysis(Id: number) {
    const crit: MetaAnalysisSelectionCrit = { MetaAnalysisId: Id, GetAllDetails: false };
    this._BusyMethods.push("DeleteMetaAnalysis");
    this._httpC.post<void>(this._baseUrl + 'api/MetaAnalysis/DeleteMetaAnalysis',
      crit).subscribe(() => {
        if (this.CurrentMetaAnalysis != null && this.CurrentMetaAnalysis.metaAnalysisId == Id) {
          this.CurrentMetaAnalysis = null;
          this.CurrentMetaAnalysisUnchanged = null;
        }
        let ind = this.MetaAnalysisList.findIndex(f => f.metaAnalysisId == Id)
        if (ind !== -1) {
          this.MetaAnalysisList.splice(ind, 1);
        }
        this.RemoveBusy("DeleteMetaAnalysis");
      }, error => {
        this.RemoveBusy("DeleteMetaAnalysis");
        this.modalService.GenericError(error);
      });
  }

  private CalculateColsVisibility() {
    if (this.CurrentMetaAnalysis == null) return;
    this.ColumnVisibility = new DynamicColumnsOutcomes();
    for(let otc of this.CurrentMetaAnalysis.outcomes)
    {
      for(let oia of otc.outcomeCodes.outcomeItemAttributesList)
      {
        if (this.ColumnVisibility.ClassificationHeaders.findIndex(f => f.Id == oia.attributeId) == -1) {
          let minicode: IdAndNamePair = new IdAndNamePair();
          minicode.Id = oia.attributeId; minicode.Name = oia.attributeName;
          this.ColumnVisibility.ClassificationHeaders.push(minicode);
        }
      }
    }
    const separator = String.fromCharCode(0x00AC); //the "not" simbol, or inverted pipe
    let AnswersColIds: string[] = [];
    if (this.CurrentMetaAnalysis.attributeIdAnswer) AnswersColIds = this.CurrentMetaAnalysis.attributeIdAnswer.split(",");
    let AnswersColNames: string[] = [];
    if (this.CurrentMetaAnalysis.attributeAnswerText) AnswersColNames = this.CurrentMetaAnalysis.attributeAnswerText.split(separator);
    if (AnswersColIds.length > 0 //nothing to do if we don't have answers cols to show
      && AnswersColIds.length + 1 == AnswersColNames.length //can't do anything reliable if we get 2 arrays of diff length! (AnswersColNames ends with an empty string)
    ) {
      for (let i = 0; i < AnswersColIds.length; i++) {
        if (AnswersColNames[i] && AnswersColNames[i] != "") {
          let minicode: IdAndNamePair = new IdAndNamePair();
          minicode.Id = Number.parseInt(AnswersColIds[i], 10);
          minicode.Name = AnswersColNames[i];
          this.ColumnVisibility.AnswerHeaders.push(minicode);
        }
      }
    }
    let QuestionColIds = this.CurrentMetaAnalysis.attributeIdQuestion.split(",");
    let QuestionColNames = this.CurrentMetaAnalysis.attributeQuestionText.split(separator);
    if (QuestionColIds.length > 0 && QuestionColIds.length + 1 == QuestionColNames.length) {
      //can't do anything reliable if we get 2 arrays of diff length! (QuestionColNames ends with an empty string)
      for (let i = 0; i < QuestionColIds.length; i++) {
        if (QuestionColNames[i] && QuestionColNames[i] != "") {
          let minicode: IdAndNamePair = new IdAndNamePair();
          minicode.Id = Number.parseInt(QuestionColIds[i], 10);
          minicode.Name = QuestionColNames[i];
          this.ColumnVisibility.QuestionHeaders.push(minicode);
        }
      }
    }
  }

  public ApplySavedSorting() {
    if (this.CurrentMetaAnalysis == null || this.CurrentMetaAnalysisUnchanged == null) return;
    if (this.CurrentMetaAnalysis.sortedBy != "") {
      let booleanDir: boolean = this.CurrentMetaAnalysis.sortDirection == "Ascending" ? true : false;
      this.LocalSort = {
        SortBy: MetaAnalysisService.FieldNameFromER4ColName(this.CurrentMetaAnalysis.sortedBy) ,
        Direction: booleanDir
      };
      this._FilteredOutcomes = CustomSorting.DoSort(this._FilteredOutcomes, this.LocalSort);
    }
    else { this.LocalSort = new LocalSort(); }
  }
  public ApplyFilters() {
    if (this.CurrentMetaAnalysis == null) return;
    let res = this.CurrentMetaAnalysis.outcomes.concat();
    for (let FF of this.CurrentMetaAnalysis.filterSettingsList) {
      res = this.ProcessSingleFilter(res, FF);
    }
    this._FilteredOutcomes = res;
    for (let oc of this.CurrentMetaAnalysis.outcomes.filter(f => f.isSelected == true)) {
      if (this._FilteredOutcomes.findIndex(f => f.outcomeId == oc.outcomeId) == -1) {
        //we have filtered out this outcome, so we also un-select it as otherwise it will be included in forest plots et al.
        oc.isSelected = false;
      }
    }
  }
  private ProcessSingleFilter(outcomes: ExtendedOutcome[], filter: iFilterSettings): ExtendedOutcome[] {
    const separator = "{" + String.fromCharCode(0x00AC) + "}";
    let res: ExtendedOutcome[] = [];
    const key = MetaAnalysisService.FieldNameFromER4ColName(filter.columnName) as keyof ExtendedOutcome;
    if (filter.selectedValues && filter.selectedValues != "") {
      const selectors = filter.selectedValues.split(separator);
      res = outcomes.filter(f => {
        for (const sel of selectors) {
          if (f[key] == sel) return true;
        }
        return false;
      });
    } else res = res.concat(outcomes);
    let FirstFilterSet: boolean = !(filter.filter1 == "" && filter.filter1Operator == "IsEqualTo");
    let SecondFilterSet: boolean = false;
    if (!FirstFilterSet) return res;
    SecondFilterSet = !(filter.filter2 == "" && filter.filter2Operator == "IsEqualTo");
    if (!SecondFilterSet) {
      //easy case - only one filter to deal with...
      res = this.FilterByNumberedFilter(res, filter.filter1, filter.filter1Operator, filter.filter1CaseSensitive, key);
    }
    else if (filter.filtersLogicalOperator == "And") {
      //fairly easy, filter by filter1 then filter the result by filter2
      res = this.FilterByNumberedFilter(res, filter.filter1, filter.filter1Operator, filter.filter1CaseSensitive, key);
      res = this.FilterByNumberedFilter(res, filter.filter2, filter.filter2Operator, filter.filter2CaseSensitive, key);
    }
    else {
      //ouch: filter by "OR" across filter1 and filter2, not so easy!
      let interim1 = this.FilterByNumberedFilter(res, filter.filter1, filter.filter1Operator, filter.filter1CaseSensitive, key);
      let interim2 = this.FilterByNumberedFilter(res, filter.filter2, filter.filter2Operator, filter.filter2CaseSensitive, key);
      res = interim1.concat(interim2.filter((f) => interim1.indexOf(f) < 0));
    }
    //console.log("sub-filtering result: ", res);
    return res;
  }
  private FilterByNumberedFilter(outcomes: ExtendedOutcome[], FilterBy: string, Operator: string, CaseSensitive: boolean, field: keyof ExtendedOutcome): ExtendedOutcome[] {
    let res: ExtendedOutcome[] = [];
    res = res.concat(outcomes);
    //possible filtering Operators are:
    //Is equal to
    //Is not equal to
    //Starts with
    //Ends with
    //Contains
    //Does not contain
    //Is contained in
    //Is not contained in
    //Is empty
    //Is not empty
    //Is less than
    //Is less than or equal to
    //Is greater than
    //Is greater than or equal to
    //Is null
    //Is not null
    if (field == "es" || field == "sees" || field.startsWith("aa") || field.startsWith("occ")) {
      //field holds numeric vals, so we use ad-hoc logic
      res = this.FilterByNumberedFilterAgainstNumericValues(res, FilterBy, Operator, field);
      return res;
    }
    if (!CaseSensitive) FilterBy = FilterBy.toLowerCase();
    if (Operator == "IsEqualTo") {
      if (CaseSensitive) res = res.filter(f => f[field] == FilterBy);
      else res = res.filter(f => f[field].toString().toLowerCase() == FilterBy);
    }
    else if (Operator == "IsNotEqualTo") {
      if (CaseSensitive) res = res.filter(f => f[field] != FilterBy);
      else res = res.filter(f => f[field].toString().toLowerCase() != FilterBy);
    }
    else if (Operator == "StartsWith") {
      if (CaseSensitive) res = res.filter(f => f[field].toString().startsWith(FilterBy));
      else res = res.filter(f => f[field].toString().toLowerCase().startsWith(FilterBy));
    }
    else if (Operator == "EndsWith") {
      if (CaseSensitive) res = res.filter(f => f[field].toString().endsWith(FilterBy));
      else res = res.filter(f => f[field].toString().toLowerCase().endsWith(FilterBy));
    }
    else if (Operator == "Contains") {
      if (CaseSensitive) res = res.filter(f => f[field].toString().indexOf(FilterBy) != -1);
      else res = res.filter(f => f[field].toString().toLowerCase().indexOf(FilterBy) != -1);
    }
    else if (Operator == "DoesNotContain") {
      if (CaseSensitive) res = res.filter(f => !(f[field].toString().indexOf(FilterBy) != -1));
      else res = res.filter(f => !(f[field].toString().toLowerCase().indexOf(FilterBy) != -1));
    }
    else if (Operator == "IsContainedIn") {
      if (CaseSensitive) res = res.filter(f => FilterBy.indexOf(f[field].toString()) != -1);
      else res = res.filter(f => FilterBy.indexOf(f[field].toString().toLowerCase()) != -1);
    }
    else if (Operator == "IsNotContainedIn") {
      if (CaseSensitive) res = res.filter(f => !(FilterBy.indexOf(f[field].toString()) != -1));
      else res = res.filter(f => !(FilterBy.indexOf(f[field].toString().toLowerCase()) != -1));
    }
    else if (Operator == "IsEmpty") {
      res = res.filter(f => f[field] == "");
    }
    else if (Operator == "IsNotEmpty") {
      res = res.filter(f => f[field] != "");
    }
    else if (Operator == "IsLessThan") {
      if (CaseSensitive) res = res.filter(f => f[field].toString() < FilterBy);
      else res = res.filter(f => f[field].toString().toLowerCase() < FilterBy);
    }
    else if (Operator == "IsLessThanOrEqualTo") {
      if (CaseSensitive) res = res.filter(f => f[field].toString() <= FilterBy);
      else res = res.filter(f => f[field].toString().toLowerCase() <= FilterBy);
    }
    else if (Operator == "IsGreaterThan") {
      if (CaseSensitive) res = res.filter(f => f[field].toString() > FilterBy);
      else res = res.filter(f => f[field].toString().toLowerCase() > FilterBy);
    }
    else if (Operator == "IsGreaterThanOrEqualTo") {
      if (CaseSensitive) res = res.filter(f => f[field].toString() >= FilterBy);
      else res = res.filter(f => f[field].toString().toLowerCase() >= FilterBy);
    }
    else if (Operator == "IsNull") {
      res = res.filter(f => f[field] == "");
    }
    else if (Operator == "IsNotNull") {
      res = res.filter(f => f[field] != "");
    } 
    return res;
  }
  private FilterByNumberedFilterAgainstNumericValues(res: ExtendedOutcome[], FilterByString: string, Operator: string, field: keyof ExtendedOutcome): ExtendedOutcome[] {
    const FilterBy = Number.parseFloat(FilterByString);
    //possible filtering Operators are:
    //Is equal to
    //Is not equal to
    //Starts with
    //Ends with
    //Contains
    //Does not contain
    //Is contained in
    //Is not contained in
    //Is empty
    //Is not empty
    //Is less than
    //Is less than or equal to
    //Is greater than
    //Is greater than or equal to
    //Is null
    //Is not null
    if (Operator == "IsEqualTo") {
      res = res.filter(f => f[field] == FilterBy);
    }
    else if (Operator == "IsNotEqualTo") {
      res = res.filter(f => f[field] != FilterBy);
    }
    else if (Operator == "StartsWith") {
      res = res.filter(f => f[field].toString().startsWith(FilterByString));
    }
    else if (Operator == "EndsWith") {
      res = res.filter(f => f[field].toString().endsWith(FilterByString));
    }
    else if (Operator == "Contains") {
      res = res.filter(f => f[field].toString().indexOf(FilterByString) != -1);
    }
    else if (Operator == "DoesNotContain") {
      res = res.filter(f => !(f[field].toString().indexOf(FilterByString) != -1));
    }
    else if (Operator == "IsContainedIn") {
      res = res.filter(f => FilterByString.indexOf(f[field].toString()) != -1);
    }
    else if (Operator == "IsNotContainedIn") {
      res = res.filter(f => !(FilterByString.indexOf(f[field].toString()) != -1));
    }
    else if (Operator == "IsEmpty") {
      res = res.filter(f => f[field] == "");
    }
    else if (Operator == "IsNotEmpty") {
      res = res.filter(f => f[field] != "");
    }
    else if (Operator == "IsLessThan") {
      res = res.filter(f => f[field] < FilterBy);
    }
    else if (Operator == "IsLessThanOrEqualTo") {
      res = res.filter(f => f[field] <= FilterBy);
    }
    else if (Operator == "IsGreaterThan") {
      res = res.filter(f => f[field] > FilterBy);
    }
    else if (Operator == "IsGreaterThanOrEqualTo") {
      res = res.filter(f => f[field] >= FilterBy);
    }
    else if (Operator == "IsNull") {
      res = res.filter(f => f[field] == "");
    }
    else if (Operator == "IsNotNull") {
      res = res.filter(f => f[field] != "");
    }
    return res;
  }

  public static FieldNameFromER4ColName(ColName: string): string {
    switch (ColName) {
      case "ESColumn": return "es";
      case "SEESColumn": return "sees";
      case "ES": return "es";//case for sorting
      case "SEES": return "sees";//case for sorting
      case "titleColumn": return "shortTitle";
      case "ShortTitle": return "shortTitle";
      case "DescColumn": return "title";
      case "Title": return "title";// sorting
      case "TimepointColumn": return "timepointDisplayValue";
      case "TimepointDisplayValue": return "timepointDisplayValue"; //sorting
      case "OutcomeTypeName": return "outcomeTypeName";
      case "OutcomeColumn": return "outcomeText";
      case "OutcomeText": return "outcomeText";//sorting
      case "InterventionColumn": return "interventionText";
      case "InterventionText": return "interventionText";//sorting
      case "ComparisonColumn": return "controlText";
      case "ControlText": return "controlText";//sorting 
      case "Arm1Column": return "grp1ArmName";
      case "Arm2Column": return "grp2ArmName";
      default: return ColName;
    }
  }
  public static ER4ColNameFromFieldName(ColName: string, ForSorting: boolean): string {
    //we need to do slightly different things if we are sorting or filtering.

    switch (ColName) {
      case "es": return ForSorting ? "ES" : "ESColumn";
      case "sees": return ForSorting ? "SEES" : "SEESColumn";
      case "shortTitle": return "titleColumn";
      case "title": return ForSorting ? "Title" : "DescColumn";
      case "timepointDisplayValue": return ForSorting ? "TimepointDisplayValue": "TimepointColumn";
      case "outcomeTypeName": return ForSorting ? "OutcomeText" : "OutcomeTypeName";
      case "outcomeText": return "OutcomeColumn";
      case "interventionText": return ForSorting ? "InterventionText" : "InterventionColumn"; 
      case "controlText": return ForSorting ? "ControlText" : "ComparisonColumn"; 
      case "grp1ArmName": return ForSorting ? "grp1ArmName" : "Arm1Column";
      case "grp2ArmName": return ForSorting ? "grp2ArmName" : "Arm2Column";
      default: return ColName;
    }
  }

  Clear() {
    this.MetaAnalysisList = [];
    this.CurrentMetaAnalysis = null;
    this.CurrentMetaAnalysisUnchanged = null;
    this.ColumnVisibility = new DynamicColumnsOutcomes();
  }
}
interface iMetaAnalysis {
  analysisType: number;
  title: string;
  knha: boolean;
  fitStats: boolean;
  confint: boolean;
  egger: boolean;
  rankCorr: boolean;
  trimFill: boolean;
  statisticalModel: number;
  verbose: number;
  significanceLevel: number;
  decPlaces: number;
  xAxisTitle: string;
  summaryEstimateTitle: string;
  showAnnotations: boolean;
  showAnnotationWeights: boolean;
  fittedVals: boolean;
  credInt: boolean;
  showFunnel: boolean;
  showBoxplot: boolean;
  sortedBy: string;
  sortDirection: string;
  nmaStatisticalModel: number;
  largeValuesGood: boolean;
  nmaReference: string;
  exponentiated: boolean;
  allTreatments: boolean;
  metaAnalysisId: number;
  attributeId: number;
  setId: number;
  attributeIdIntervention: number;
  attributeIdControl: number;
  attributeIdOutcome: number;
  randomised: number;
  roB: number;
  incon: number;
  indirect: number;
  imprec: number;
  pubBias: number;
  certaintyLevel: number;
  roBComment: string;
  roBSequence: boolean;
  roBConcealment: boolean;
  roBBlindingParticipants: boolean;
  roBBlindingAssessors: boolean;
  roBIncomplete: boolean;
  roBSelective: boolean;
  roBNoIntention: boolean;
  roBCarryover: boolean;
  roBStopped: boolean;
  roBUnvalidated: boolean;
  roBOther: boolean;
  inconComment: string;
  inconPoint: boolean;
  inconCIs: boolean;
  inconDirection: boolean;
  inconStatistical: boolean;
  inconOther: boolean;
  indirectComment: string;
  indirectPopulation: boolean;
  indirectOutcome: boolean;
  indirectNoDirect: boolean;
  indirectIntervention: boolean;
  indirectTime: boolean;
  indirectOther: boolean;
  imprecComment: string;
  imprecWide: boolean;
  imprecFew: boolean;
  imprecOnlyOne: boolean;
  imprecOther: boolean;
  pubBiasComment: string;
  pubBiasCommercially: boolean;
  pubBiasAsymmetrical: boolean;
  pubBiasLimited: boolean;
  pubBiasMissing: boolean;
  pubBiasDiscontinued: boolean;
  pubBiasDiscrepancy: boolean;
  pubBiasOther: boolean;
  upgradeComment: string;
  upgradeLarge: boolean;
  upgradeVeryLarge: boolean;
  upgradeAllPlausible: boolean;
  upgradeClear: boolean;
  upgradeNone: boolean;
  certaintyLevelComment: string;
  metaAnalysisTypeId: number;
  metaAnalysisTypeTitle: string;
  interventionText: string;
  controlText: string;
  outcomeText: string;
  outcomes: iExtendedOutcome[];
  metaAnalysisModerators: iModerator[];
  attributeIdQuestion: string;
  attributeQuestionText: string;
  attributeIdAnswer: string;
  attributeAnswerText: string;
  gridSettings: string;
  filterSettingsList: iFilterSettings[];
  feForestPlot: null;
  reForestPlot: null;
  feFunnelPlot: null;
  feSumWeight: number;
  reSumWeight: number;
  feEffect: number;
  feSE: number;
  feCiUpper: number;
  feCiLower: number;
  reEffect: number;
  reSE: number;
  reCiUpper: number;
  reCiLower: number;
  tauSquared: number;
  q: number;
  reQ: number;
  numStudies: number;
  fileDrawerZ: number;
  sumWeightsSquared: number;
  reSumWeightsTimesOutcome: number;
  wY_squared: number;
}
export class MetaAnalysis implements iMetaAnalysis {
  constructor(iMA: iMetaAnalysis) {
    this.analysisType = iMA.analysisType;
    this.title = iMA.title;
    this.knha = iMA.knha;
    this.fitStats = iMA.fitStats;
    this.confint = iMA.confint;
    this.egger = iMA.egger;
    this.rankCorr = iMA.rankCorr;
    this.trimFill = iMA.trimFill;
    this.statisticalModel = iMA.statisticalModel;
    this.verbose = iMA.verbose;
    this.significanceLevel = iMA.significanceLevel;
    this.decPlaces = iMA.decPlaces;
    this.xAxisTitle = iMA.xAxisTitle;
    this.summaryEstimateTitle = iMA.summaryEstimateTitle;
    this.showAnnotations = iMA.showAnnotations;
    this.showAnnotationWeights = iMA.showAnnotationWeights;
    this.fittedVals = iMA.fittedVals;
    this.credInt = iMA.credInt;
    this.showFunnel = iMA.showFunnel;
    this.showBoxplot = iMA.showBoxplot;
    this.sortedBy = iMA.sortedBy;
    this.sortDirection = iMA.sortDirection;
    this.nmaStatisticalModel = iMA.nmaStatisticalModel;
    this.largeValuesGood = iMA.largeValuesGood;
    this.nmaReference = iMA.nmaReference;
    this.exponentiated = iMA.exponentiated;
    this.allTreatments = iMA.allTreatments;
    this.metaAnalysisId = iMA.metaAnalysisId;
    this.attributeId = iMA.attributeId;
    this.setId = iMA.setId;
    this.attributeIdIntervention = iMA.attributeIdIntervention;
    this.attributeIdControl = iMA.attributeIdControl;
    this.attributeIdOutcome = iMA.attributeIdOutcome;
    this.randomised = iMA.randomised;
    this.roB = iMA.roB;
    this.incon = iMA.incon;
    this.indirect = iMA.indirect;
    this.imprec = iMA.imprec;
    this.pubBias = iMA.pubBias;
    this.certaintyLevel = iMA.certaintyLevel;
    this.roBComment = iMA.roBComment;
    this.roBSequence = iMA.roBSequence;
    this.roBConcealment = iMA.roBConcealment;
    this.roBBlindingParticipants = iMA.roBBlindingParticipants;
    this.roBBlindingAssessors = iMA.roBBlindingAssessors;
    this.roBIncomplete = iMA.roBIncomplete;
    this.roBSelective = iMA.roBSelective;
    this.roBNoIntention = iMA.roBNoIntention;
    this.roBCarryover = iMA.roBCarryover;
    this.roBStopped = iMA.roBStopped;
    this.roBUnvalidated = iMA.roBUnvalidated;
    this.roBOther = iMA.roBOther;
    this.inconComment = iMA.inconComment;
    this.inconPoint = iMA.inconPoint;
    this.inconCIs = iMA.inconCIs;
    this.inconDirection = iMA.inconDirection;
    this.inconStatistical = iMA.inconStatistical;
    this.inconOther = iMA.inconOther;
    this.indirectComment = iMA.indirectComment;
    this.indirectPopulation = iMA.indirectPopulation;
    this.indirectOutcome = iMA.indirectOutcome;
    this.indirectNoDirect = iMA.indirectNoDirect;
    this.indirectIntervention = iMA.indirectIntervention;
    this.indirectTime = iMA.indirectTime;
    this.indirectOther = iMA.indirectOther;
    this.imprecComment = iMA.imprecComment;
    this.imprecWide = iMA.imprecWide;
    this.imprecFew = iMA.imprecFew;
    this.imprecOnlyOne = iMA.imprecOnlyOne;
    this.imprecOther = iMA.imprecOther;
    this.pubBiasComment = iMA.pubBiasComment;
    this.pubBiasCommercially = iMA.pubBiasCommercially;
    this.pubBiasAsymmetrical = iMA.pubBiasAsymmetrical;
    this.pubBiasLimited = iMA.pubBiasLimited;
    this.pubBiasMissing = iMA.pubBiasMissing;
    this.pubBiasDiscontinued = iMA.pubBiasDiscontinued;
    this.pubBiasDiscrepancy = iMA.pubBiasDiscrepancy;
    this.pubBiasOther = iMA.pubBiasOther;
    this.upgradeComment = iMA.upgradeComment;
    this.upgradeLarge = iMA.upgradeLarge;
    this.upgradeVeryLarge = iMA.upgradeVeryLarge;
    this.upgradeAllPlausible = iMA.upgradeAllPlausible;
    this.upgradeClear = iMA.upgradeClear;
    this.upgradeNone = iMA.upgradeNone;
    this.certaintyLevelComment = iMA.certaintyLevelComment;
    this.metaAnalysisTypeTitle = iMA.metaAnalysisTypeTitle;
    this.interventionText = iMA.interventionText;
    this.controlText = iMA.controlText;
    this.outcomeText = iMA.outcomeText;
    this.metaAnalysisModerators = iMA.metaAnalysisModerators;
    this.attributeIdQuestion = iMA.attributeIdQuestion;
    this.attributeQuestionText = iMA.attributeQuestionText;
    this.attributeIdAnswer = iMA.attributeIdAnswer;
    this.attributeAnswerText = iMA.attributeAnswerText;
    this.gridSettings = iMA.gridSettings;
    this.feForestPlot =null;
    this.reForestPlot =null;
    this.feFunnelPlot =null;
    this.feSumWeight = iMA.feSumWeight;
    this.reSumWeight = iMA.reSumWeight;
    this.feEffect = iMA.feEffect;
    this.feSE = iMA.feSE;
    this.feCiUpper = iMA.feCiUpper;
    this.feCiLower = iMA.feCiLower;
    this.reEffect = iMA.reEffect;
    this.reSE = iMA.reSE;
    this.reCiUpper = iMA.reCiUpper;
    this.reCiLower = iMA.reCiLower;
    this.tauSquared = iMA.tauSquared;
    this.q = iMA.q;
    this.reQ = iMA.reQ;
    this.numStudies = iMA.numStudies;
    this.fileDrawerZ = iMA.fileDrawerZ;
    this.sumWeightsSquared = iMA.sumWeightsSquared;
    this.reSumWeightsTimesOutcome = iMA.reSumWeightsTimesOutcome;
    this.wY_squared = iMA.wY_squared;
    this.outcomes = [];
    for (let iO of iMA.outcomes) {
      let Oc: ExtendedOutcome = new ExtendedOutcome(iO);
      this.outcomes.push(Oc);
    }
    this.filterSettingsList = []; 
    for (let inFS of iMA.filterSettingsList) {
      let FS: FilterSettings = new FilterSettings(inFS);
      this.filterSettingsList.push(FS);
    }
    this.metaAnalysisModerators = [];
    for (let iMod of iMA.metaAnalysisModerators) {
      let Mod: Moderator = new Moderator(iMod);
      this.metaAnalysisModerators.push(Mod);
    }
    this.metaAnalysisTypeId = iMA.metaAnalysisTypeId;
  }
  public analysisType: number;
  public title: string;
  public knha: boolean;
  public fitStats: boolean;
  public confint: boolean;
  public egger: boolean;
  public rankCorr: boolean;
  public trimFill: boolean;
  public statisticalModel: number;
  public verbose: number;
  public significanceLevel: number;
  public decPlaces: number;
  public xAxisTitle: string;
  public summaryEstimateTitle: string;
  public showAnnotations: boolean;
  public showAnnotationWeights: boolean;
  public fittedVals: boolean;
  public credInt: boolean;
  public showFunnel: boolean;
  public showBoxplot: boolean;
  public sortedBy: string;
  public sortDirection: string;
  public nmaStatisticalModel: number;
  public largeValuesGood: boolean;
  public nmaReference: string;
  public exponentiated: boolean;
  public allTreatments: boolean;
  public metaAnalysisId: number;
  public attributeId: number;
  public setId: number;
  public attributeIdIntervention: number;
  public attributeIdControl: number;
  public attributeIdOutcome: number;
  public randomised: number;
  public roB: number;
  public incon: number;
  public indirect: number;
  public imprec: number;
  public pubBias: number;
  public certaintyLevel: number;
  public roBComment: string;
  public roBSequence: boolean;
  public roBConcealment: boolean;
  public roBBlindingParticipants: boolean;
  public roBBlindingAssessors: boolean;
  public roBIncomplete: boolean;
  public roBSelective: boolean;
  public roBNoIntention: boolean;
  public roBCarryover: boolean;
  public roBStopped: boolean;
  public roBUnvalidated: boolean;
  public roBOther: boolean;
  public inconComment: string;
  public inconPoint: boolean;
  public inconCIs: boolean;
  public inconDirection: boolean;
  public inconStatistical: boolean;
  public inconOther: boolean;
  public indirectComment: string;
  public indirectPopulation: boolean;
  public indirectOutcome: boolean;
  public indirectNoDirect: boolean;
  public indirectIntervention: boolean;
  public indirectTime: boolean;
  public indirectOther: boolean;
  public imprecComment: string;
  public imprecWide: boolean;
  public imprecFew: boolean;
  public imprecOnlyOne: boolean;
  public imprecOther: boolean;
  public pubBiasComment: string;
  public pubBiasCommercially: boolean;
  public pubBiasAsymmetrical: boolean;
  public pubBiasLimited: boolean;
  public pubBiasMissing: boolean;
  public pubBiasDiscontinued: boolean;
  public pubBiasDiscrepancy: boolean;
  public pubBiasOther: boolean;
  public upgradeComment: string;
  public upgradeLarge: boolean;
  public upgradeVeryLarge: boolean;
  public upgradeAllPlausible: boolean;
  public upgradeClear: boolean;
  public upgradeNone: boolean;
  public certaintyLevelComment: string;
  private _metaAnalysisTypeId: number = -1;
  public set metaAnalysisTypeId(val: number) {
    this._metaAnalysisTypeId = val;
    for(let o of this.outcomes)
    {
      o.SetESForThisOutcomeType(this._metaAnalysisTypeId);
      o.updateCanSelect(this._metaAnalysisTypeId);
      if (!o.canSelect) {
        o.isSelected = false;
      }
    }
  }
  public get metaAnalysisTypeId(): number {
    return this._metaAnalysisTypeId;
  }
  public metaAnalysisTypeTitle: string;
  public interventionText: string;
  public controlText: string;
  public outcomeText: string;
  public outcomes: ExtendedOutcome[];
  public metaAnalysisModerators: Moderator[];
  public attributeIdQuestion: string;
  public attributeQuestionText: string;
  public attributeIdAnswer: string;
  public attributeAnswerText: string;
  public gridSettings: string;
  public filterSettingsList: iFilterSettings[];
  public feForestPlot: null;
  public reForestPlot: null;
  public feFunnelPlot: null;
  public feSumWeight: number;
  public reSumWeight: number;
  public feEffect: number;
  public feSE: number;
  public feCiUpper: number;
  public feCiLower: number;
  public reEffect: number;
  public reSE: number;
  public reCiUpper: number;
  public reCiLower: number;
  public tauSquared: number;
  public q: number;
  public reQ: number;
  public numStudies: number;
  public fileDrawerZ: number;
  public sumWeightsSquared: number;
  public reSumWeightsTimesOutcome: number;
  public wY_squared: number;

  public get CanTrimFill(): boolean {
    const selectedModerators = this.metaAnalysisModerators.filter(f => f.isSelected == true);
    if (selectedModerators.length == 1) return false;
    else return true;
  }
  public ToiMetaAnalysis(): iMetaAnalysis {
    //we need this because "get" methods don't get in the JSON string by default, so we have to do it explictly for any "property" that is implemented with get/set...
    const ResString = JSON.stringify(this);
    let res: iMetaAnalysis = JSON.parse(ResString);
    res.metaAnalysisTypeId = this.metaAnalysisTypeId;
    return res;
  }
}

export interface iFilterSettings {
  isClear: boolean;
  metaAnalysisFilterSettingId: number;
  metaAnalysisId: number;
  columnName: string;
  selectedValues: string;
  filter1: string;
  filter1Operator: string;
  filter1CaseSensitive: boolean;
  filtersLogicalOperator: string;
  filter2: string;
  filter2Operator: string;
  filter2CaseSensitive: boolean;
}
export class FilterSettings implements iFilterSettings{
  constructor(iF: iFilterSettings) {
    //this.isClear = iF.isClear;
    this.metaAnalysisFilterSettingId = iF.metaAnalysisFilterSettingId;
    this.metaAnalysisId = iF.metaAnalysisId;
    this.columnName = iF.columnName;
    this.selectedValues = iF.selectedValues;
    this.filter1 = iF.filter1;
    this.filter1Operator = iF.filter1Operator;
    this.filter1CaseSensitive = iF.filter1CaseSensitive;
    this.filtersLogicalOperator = iF.filtersLogicalOperator;
    this.filter2 = iF.filter2;
    this.filter2Operator = iF.filter2Operator;
    this.filter2CaseSensitive = iF.filter2CaseSensitive;
  }
  public get isClear(): boolean {
    if (this.selectedValues == ""
      && this.filter1 == ""
      && this.filter1CaseSensitive == false
      && this.filter1Operator == "IsEqualTo"
      && this.filter2 == ""
      && this.filter2CaseSensitive == false
      && this.filter2Operator == "IsEqualTo"
      && this.filtersLogicalOperator == "And") return true;
    else return false;
  }
  metaAnalysisFilterSettingId: number;
  metaAnalysisId: number;
  columnName: string;
  selectedValues: string;
  filter1: string;
  filter1Operator: string;
  filter1CaseSensitive: boolean;
  filtersLogicalOperator: string;
  filter2: string;
  filter2Operator: string;
  filter2CaseSensitive: boolean;
}

export interface iModerator {
  name: string;
  fieldName: string;
  attributeID: number;
  reference: string;
  references: iReference[];
  isSelected: boolean;
  isFactor: boolean;
}

export class Moderator implements iModerator {
  constructor(incoming: iModerator) {
    this.name = incoming.name;
    this.fieldName = incoming.fieldName;
    this.attributeID = incoming.attributeID;
    this.reference = incoming.reference;
    this.references = incoming.references;
    this.isSelected = incoming.isSelected;
    this.isFactor = incoming.isFactor;
  }
  name: string;
  fieldName: string;
  attributeID: number;
  reference: string;
  references: iReference[];
  isSelected: boolean;
  isFactor: boolean;
}

export interface iReference {
  "name": string,
  "attributeID": number,
}

export class MetaAnalysisSelectionCrit {
  GetAllDetails: boolean = false;
  MetaAnalysisId: number = 0;
}
export class DynamicColumnsOutcomes {
  //ClassificationsCount: number = 0;
  //AnswersCount: number = 0;
  //QuestionsCount: number = 0;
  ClassificationHeaders: IdAndNamePair[] = [];
  AnswerHeaders: IdAndNamePair[] = [];
  QuestionHeaders: IdAndNamePair[] = [];
}
export class IdAndNamePair {
  Id: number = -1;
  Name: string = "";
}
