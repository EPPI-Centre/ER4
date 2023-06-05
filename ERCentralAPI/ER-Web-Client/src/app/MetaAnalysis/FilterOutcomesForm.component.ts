import { Component, OnInit,Input, ViewChild, OnDestroy, EventEmitter, Output } from '@angular/core';
import { sep } from 'path';
import { StringKeyValue } from '../services/ItemList.service';
import { FilterSettings, MetaAnalysis, MetaAnalysisService } from '../services/MetaAnalysis.service';
import { ExtendedOutcome } from '../services/outcomes.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

@Component({
  selector: 'FilterOutcomesFormComp',
  templateUrl: './FilterOutcomesForm.component.html',
  providers: [],
  styles: []
})
export class FilterOutcomesFormComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private MetaAnalysisService: MetaAnalysisService
  ) { }

  ngOnInit() {
    //we build "not changing" data here, this is things that drive the UI and can only remain constant while the "edit filters" panel is open
    if (this.MetaAnalysisService.CurrentMetaAnalysis) {
      //1st: set the initial "current filter" user may decide to change it...
      if (this.IncomingFilterFieldName !== undefined && this.IncomingFilterFieldName !== '') {
        let ER4ColName = MetaAnalysisService.ER4ColNameFromFieldName(this.IncomingFilterFieldName, false);
        let ColFilter = this.MetaAnalysisService.CurrentMetaAnalysis.filterSettingsList.find(f => f.columnName == ER4ColName);
        if (ColFilter === undefined) {
          this._CurrentFilterSetting = new FilterSettings({
            isClear: false, metaAnalysisFilterSettingId: 0
            , metaAnalysisId: this.MetaAnalysisService.CurrentMetaAnalysis.metaAnalysisId
            , columnName: ER4ColName, selectedValues: "", filter1: "", filter1Operator: "IsEqualTo", filter1CaseSensitive: false, filtersLogicalOperator: "And"
            , filter2: "", filter2Operator: "IsEqualTo", filter2CaseSensitive: false
          });
          this.MetaAnalysisService.CurrentMetaAnalysis.filterSettingsList.push(this._CurrentFilterSetting);
        }
        else this._CurrentFilterSetting = ColFilter;
      } else {
        this._CurrentFilterSetting = new FilterSettings({
          isClear: false, metaAnalysisFilterSettingId: 0
          , metaAnalysisId: this.MetaAnalysisService.CurrentMetaAnalysis.metaAnalysisId
          , columnName: "", selectedValues: "", filter1: "", filter1Operator: "IsEqualTo", filter1CaseSensitive: false, filtersLogicalOperator: "And"
          , filter2: "", filter2Operator: "IsEqualTo", filter2CaseSensitive: false
        });
      }
      //2nd set up the list of columns that can be filtered
      this._CurrentColumns = this._FixedColumns.concat();
      let i: number = 1;
      for (let col of this.MetaAnalysisService.ColumnVisibility.AnswerHeaders) {
        this._CurrentColumns.push(new StringKeyValue("aa" + i.toString(), col.Name));
        i++;
      }
      i = 1;
      for (let col of this.MetaAnalysisService.ColumnVisibility.QuestionHeaders) {
        this._CurrentColumns.push(new StringKeyValue("aq" + i.toString(), col.Name));
        i++;
      }
      i = 1;
      for (let col of this.MetaAnalysisService.ColumnVisibility.ClassificationHeaders) {
        this._CurrentColumns.push(new StringKeyValue("occ" + i.toString(), col.Name));
        i++;
      }
    }
    else {//can't do anything here if we don't have a CurrentMetaAnalysis, so we close the component
      this.PleaseCloseMe.emit();
    }
  }


  @Input() IncomingFilterFieldName: string | undefined;
  @Output() PleaseCloseMe = new EventEmitter();

  private _CurrentFilterSetting: FilterSettings = new FilterSettings({
    isClear: false, metaAnalysisFilterSettingId: 0
    , metaAnalysisId: (this.MetaAnalysisService.CurrentMetaAnalysis) ? this.MetaAnalysisService.CurrentMetaAnalysis.metaAnalysisId : 0
    , columnName: "", selectedValues: "", filter1: "", filter1Operator: "IsEqualTo", filter1CaseSensitive: false, filtersLogicalOperator: "And"
    , filter2: "", filter2Operator: "IsEqualTo", filter2CaseSensitive: false
  });;
  public get CurrentFilterSetting(): FilterSettings {
    return this._CurrentFilterSetting;
  }

  private _SelectableValues: string[] = [];
  public get SelectableValues(): string[] {
    if (!this.MetaAnalysisService.CurrentMetaAnalysis || !this.CurrentFilterSetting.columnName) return [];
    let AdjustedColName = this.CurrentFilterSetting.columnName;
    if (AdjustedColName == "ESColumn") AdjustedColName = "esRounded";
    else if (AdjustedColName == "SEESColumn") AdjustedColName = "seesRounded";
    const key = MetaAnalysisService.FieldNameFromER4ColName(AdjustedColName) as keyof ExtendedOutcome;
    let res = this.MetaAnalysisService.CurrentMetaAnalysis.outcomes.filter((value, index, array) => {
      return array.findIndex(f => value[key] === f[key]) === index;
    });
    return res.map(f =>  f[key].toString()).sort();
  }

  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }
  public get CurrentMA(): MetaAnalysis | null {
    return this.MetaAnalysisService.CurrentMetaAnalysis;
  }
  public TextFilterOperators: StringKeyValue[] = [
    new StringKeyValue("IsEqualTo", "Is Equal To"),
    new StringKeyValue("IsNotEqualTo", "Is Not Equal To"),
    new StringKeyValue("StartsWith", "Starts With"),
    new StringKeyValue("EndsWith", "Ends With"),
    new StringKeyValue("Contains", "Contains"),
    new StringKeyValue("DoesNotContain", "Does Not Contain"),
    new StringKeyValue("IsContainedIn", "Is Contained In"),
    new StringKeyValue("IsNotContainedIn", "Is Not Contained In"),
    new StringKeyValue("IsEmpty", "Is Empty"),
    new StringKeyValue("IsNotEmpty", "Is Not Empty"),
    new StringKeyValue("IsLessThan", "Is Less Than"),
    new StringKeyValue("IsLessThanOrEqualTo", "Is Less Than Or Equal To"),
    new StringKeyValue("IsGreaterThan", "Is Greater Than"),
    new StringKeyValue("IsGreaterThanOrEqualTo", "Is Greater Than Or Equal To"),
    new StringKeyValue("IsNull", "Is Null"),
    new StringKeyValue("IsNotNull", "Is Not Null")
  ];

  public IsThisTheCurrentOperator(operatorString: string, isFirstFilter: boolean): boolean {
    if (isFirstFilter) return this.CurrentFilterSetting.filter1Operator == operatorString;
    else return this.CurrentFilterSetting.filter2Operator == operatorString;
  }

  public ChangingFilterOperator(event: Event, isFirstFilter: boolean) {
    const operat: string = (event.target as HTMLOptionElement).value;
    if (isFirstFilter) this.CurrentFilterSetting.filter1Operator = operat;
    else this.CurrentFilterSetting.filter2Operator = operat;
    this.MetaAnalysisService.ApplyFilters();
  }
  public ApplyFilter() {
    this.MetaAnalysisService.ApplyFilters();
  }
  private _FixedColumns: StringKeyValue[] = [
    new StringKeyValue("es", "Effect Size"),
    new StringKeyValue("sees", "Standard Error"),
    new StringKeyValue("shortTitle", "Study"),
    new StringKeyValue("title", "Outc. Desc."),
    new StringKeyValue("timepointDisplayValue", "Timepoint"),
    new StringKeyValue("outcomeTypeName", "Type"),
    new StringKeyValue("outcomeText", "Outcome"),
    new StringKeyValue("interventionText", "Intervention"),
    new StringKeyValue("controlText", "Comparison"),
    new StringKeyValue("grp1ArmName", "Arm 1"),
    new StringKeyValue("grp2ArmName","Arm 2")
  ];
  private _CurrentColumns: StringKeyValue[] = [];
  public get CurrentColumns(): StringKeyValue[] {
    return this._CurrentColumns;
  }

  IsColTheCurrentFilter(col: StringKeyValue | string) {
    if (typeof col == "string") {
      if (this.CurrentFilterSetting.columnName == "") return true;
      else return false;
    }
    //console.log("IsColTheCurrentFilter", col);
    else {
      return this.CurrentFilterSetting.columnName == MetaAnalysisService.ER4ColNameFromFieldName(col.key, false);
    }
    //return false;
  }
  public get CurrentColDisplayName(): string {
    let res = this.CurrentColumns.find(f => MetaAnalysisService.ER4ColNameFromFieldName(f.key, false) == this.CurrentFilterSetting.columnName);
    if (res) return res.value;
    return "N/A";
  }

  public get HasSelections(): number {
    console.log("HasSelections1", this.SelectableValues.length, this.SelectableValues.filter(f => this.ValIsSelected(f) == true).length);
    const selectedCount = this.SelectableValues.filter(f => this.ValIsSelected(f) == true).length;
    if (selectedCount == 0) return 0;
    const selectableCount = this.SelectableValues.length;
    console.log("HasSelections2", this.SelectableValues.length, this.SelectableValues.filter(f => this.ValIsSelected(f) == true).length);
    if (selectedCount != selectableCount) return 1; //partial selection
    else return 2;
  }



  BringFilteringColIntoView() {
    setTimeout(() => {
      const element = document.getElementById("col-" + MetaAnalysisService.FieldNameFromER4ColName(this.CurrentFilterSetting.columnName));
      if (element) element.scrollIntoView(false);
    }, 50);
  }


  public get CanSave(): boolean {
    return this.HasWriteRights && this.MetaAnalysisService.CurrentMAhasChanges;
  }

  
  public DropDownColumnDisplayName(col: StringKeyValue): string {
    if (this.ColumnIsFiltered(col.key)) return col.value + " (*)";
    else return col.value;
  }
  public ColumnIsFiltered(fieldName: string): boolean {
    const ER4ColName = MetaAnalysisService.ER4ColNameFromFieldName(fieldName, false);
    if (this.CurrentMA) {
      return (this.CurrentMA.filterSettingsList.findIndex(f => !f.isClear && f.columnName == ER4ColName) > -1);
    }
    else return false;
  }
  public ChangingColumnFromDropdown(event: Event) {
    const ColFieldName: string = (event.target as HTMLOptionElement).value;
    if (ColFieldName != '') this.ChangingColumn(ColFieldName);
  }
  public ChangingColumn(ColFieldName: string) {
    //const ColFieldName: string = (event.target as HTMLOptionElement).value;
    const Col = this._CurrentColumns.find(f => f.key == ColFieldName);
    if (Col && this.CurrentMA) {
      const CurrFil = this._CurrentFilterSetting;
      //what do we do with the current filter, if any? We'll remove it if it's empty, do nothing otherwise
      if (CurrFil.isClear) {
        let i = this.CurrentMA.filterSettingsList.findIndex(f => f.columnName == CurrFil.columnName);
        if (i> -1) this.CurrentMA.filterSettingsList.splice(i, 1);
      }
      //now set the new "current filter"
      //we can do 2 things: create a new filter (and add it to the MA) if we're going to a column that isn't currently filtered OR use the existing filter
      let i = this.CurrentMA.filterSettingsList.findIndex(f => f.columnName == MetaAnalysisService.ER4ColNameFromFieldName(Col.key, false));
      if (i > -1) this._CurrentFilterSetting = this.CurrentMA.filterSettingsList[i];
      else {
        this._CurrentFilterSetting = new FilterSettings({
          isClear: false, metaAnalysisFilterSettingId: 0
          , metaAnalysisId: this.CurrentMA.metaAnalysisId
          , columnName: MetaAnalysisService.ER4ColNameFromFieldName(Col.key, false)
          , selectedValues: "", filter1: "", filter1Operator: "IsEqualTo", filter1CaseSensitive: false, filtersLogicalOperator: "And"
          , filter2: "", filter2Operator: "IsEqualTo", filter2CaseSensitive: false
        });
        this.CurrentMA.filterSettingsList.push(this._CurrentFilterSetting);
      }
    }
    //console.log(event);
  }

  public ValIsSelected(val: string): boolean {
    const separator = "{" + String.fromCharCode(0x00AC) + "}";
    const SelectedVals = this.CurrentFilterSetting.selectedValues.split(separator);//.filter(f => f != '');
    if ((this.CurrentFilterSetting.columnName == "ESColumn" || this.CurrentFilterSetting.columnName == "SEESColumn")
      && this.CurrentMA && this.CurrentMA.outcomes.length > 0) //these we use to find the "ShowSignificantDigits" val in outcomes
    {
      const currMA = this.CurrentMA;
      if (
        SelectedVals.find(
          f => {
            const multiplier = 10 ** currMA.outcomes[0].ShowSignificantDigits;
            //return Math.round((this.sees + Number.EPSILON) * multiplier) / multiplier;
            return (Math.round((Number.parseFloat(f) + Number.EPSILON) * multiplier) / multiplier).toString() == val;
          }
        )
      ) return true;
    }
    else if (SelectedVals.find(f => f == val)) return true;
    return false;
  }
  public ChangeSelected(val: string, event: Event) {
    //split by "{¬}" see if
    //if (!this.CurrentFilterSetting) return;
    //console.log("changeSel", event, val);

    //we show only up to 3 decimal places for ER and SEES, so to find the real filter-by vals we need to do some work...
    if (this.CurrentFilterSetting.columnName == "ESColumn") {
      if (this.CurrentMA) {
        let res = this.CurrentMA.outcomes.filter(f => f.esRounded.toString() == val).map(m => m.es);
        for (let fullVal of res) {
          this.innerChangeSelected(fullVal.toString(), event);
        }
      }
    }
    else if (this.CurrentFilterSetting.columnName == "SEESColumn") {
      if (this.CurrentMA) {
        let res = this.CurrentMA.outcomes.filter(f => f.seesRounded.toString() == val).map(m => m.sees);
        for (let fullVal of res) {
          this.innerChangeSelected(fullVal.toString(), event);
        }
      }
    }
    else this.innerChangeSelected(val, event);
  }
  private innerChangeSelected(val: string, event: Event) {
    const separator = "{" + String.fromCharCode(0x00AC) + "}";
    let SelectedVals: string[] = [];
    //console.log("innerChangeSelected START:", val, JSON.stringify(this.CurrentFilterSetting.selectedValues));
    //selectedValues.split(separator) will retun an array with 1 empty string if selectedValues is empty
    //which then means we're "filtering in" outcomes with no value in the col
    //so we actually handle 4 possible cases:
    if (this.CurrentFilterSetting.selectedValues == separator) {
      //only the separator: special case where ONLY the "...no value" option is ticked
      SelectedVals.push('');
    }
    else if(this.CurrentFilterSetting.selectedValues.indexOf(separator) != -1) {
      //separator is in here, we have 2 or more selected vals
      SelectedVals = this.CurrentFilterSetting.selectedValues.split(separator);
    }
    else if (this.CurrentFilterSetting.selectedValues !== '') {
      //we have something in this.CurrentFilterSetting.selectedValues, but it's only one value
      SelectedVals.push(this.CurrentFilterSetting.selectedValues);
    }
    //console.log("innerChangeSelected 1:", JSON.stringify(this.CurrentFilterSetting.selectedValues), SelectedVals);
    //else, this.CurrentFilterSetting.selectedValues is empty, thus SelectedVals is an empty array, which it's how we initialised
    const chbx = event.target as HTMLInputElement;
    if (chbx.checked) {
      //adding this val
      SelectedVals.push(val);
    }
    else {
      //removing this val
      const i = SelectedVals.findIndex(f => f == val);
      if (i > -1) SelectedVals.splice(i, 1);
    }
    //console.log("innerChangeSelected 2:", JSON.stringify(this.CurrentFilterSetting.selectedValues), SelectedVals);
    SelectedVals.sort();
    if (SelectedVals.length == 1 && SelectedVals[0] == '') this.CurrentFilterSetting.selectedValues = separator;//otherwise it's empty, meaning nothing is selected
    else this.CurrentFilterSetting.selectedValues = SelectedVals.join(separator);
    //console.log("innerChangeSelected END:", JSON.stringify(this.CurrentFilterSetting.selectedValues));
    this.MetaAnalysisService.ApplyFilters();
    this.MetaAnalysisService.ApplySavedSorting();
  }

  
  ClearFilter() {
    let fil = this.CurrentFilterSetting;
    fil.selectedValues = "";
    fil.filter1 = "";
    fil.filter1CaseSensitive = false;
    fil.filter1Operator = "IsEqualTo";
    fil.filtersLogicalOperator = "And";
    fil.filter2 = "";
    fil.filter2CaseSensitive = false;
    fil.filter2Operator = "IsEqualTo";
    this.MetaAnalysisService.ApplyFilters();
  }
  ClearFreeTextFilter(isFilter1: boolean) {
    let fil = this.CurrentFilterSetting;
    if (isFilter1) {
      fil.filter1 = "";
      fil.filter1CaseSensitive = false;
      fil.filter1Operator = "IsEqualTo";
    } else {
      fil.filter2 = "";
      fil.filter2CaseSensitive = false;
      fil.filter2Operator = "IsEqualTo";
    }
    this.MetaAnalysisService.ApplyFilters();
  }

  public SelectAll() {
    let fil = this.CurrentFilterSetting;
    fil.selectedValues = "";
    const separator = "{" + String.fromCharCode(0x00AC) + "}";
    let SelectedVals: string[] = [];
    
    if (this.CurrentFilterSetting.columnName == "ESColumn") {
      if (this.CurrentMA) {
        for (let val of this.SelectableValues) {
          let res = this.CurrentMA.outcomes.filter(f => f.esRounded.toString() == val).map(m => m.es);
          for (let fullVal of res) {
            SelectedVals.push(fullVal.toString());
          }
        }
      }
    }
    else if (this.CurrentFilterSetting.columnName == "SEESColumn") {
      if (this.CurrentMA) {
        for (let val of this.SelectableValues) {
          let res = this.CurrentMA.outcomes.filter(f => f.seesRounded.toString() == val).map(m => m.sees);
          for (let fullVal of res) {
            SelectedVals.push(fullVal.toString());
          }
        }
      }
    }
    else {
      for (let val of this.SelectableValues) {
        SelectedVals.push(val);
      }
    }

    SelectedVals.sort();
    if (SelectedVals.length == 1 && SelectedVals[0] == '') this.CurrentFilterSetting.selectedValues = separator;//otherwise it's empty, meaning nothing is selected
    else this.CurrentFilterSetting.selectedValues = SelectedVals.join(separator);
  }

  public UnSelectAll() {
    let fil = this.CurrentFilterSetting;
    fil.selectedValues = "";
    this.MetaAnalysisService.ApplyFilters();
  }


  public CloseMe() {
    const CurrFil = this._CurrentFilterSetting;
    //what do we do with the current filter, if any? We'll remove it if it's empty, do nothing otherwise
    if (this.CurrentMA && CurrFil.isClear) {
      let i = this.CurrentMA.filterSettingsList.findIndex(f => f.columnName == CurrFil.columnName);
      if (i > -1) this.CurrentMA.filterSettingsList.splice(i, 1);
    }
    this.MetaAnalysisService.ApplyFilters();
    this.PleaseCloseMe.emit();
  }
  ngOnDestroy() { }
}






