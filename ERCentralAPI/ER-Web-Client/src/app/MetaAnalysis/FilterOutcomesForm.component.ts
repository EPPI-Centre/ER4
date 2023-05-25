import { Component, OnInit,Input, ViewChild, OnDestroy, EventEmitter, Output } from '@angular/core';
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
    if (!this.MetaAnalysisService.CurrentMetaAnalysis || !this.CurrentFilterSetting.columnName ) return [];
    const key = MetaAnalysisService.FieldNameFromER4ColName(this.CurrentFilterSetting.columnName) as keyof ExtendedOutcome;
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
  public ChangingColumn(event: Event) {
    const ColFieldName: string = (event.target as HTMLOptionElement).value;
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
    console.log(event);
  }

  public ValIsSelected(val: string): boolean {
    const separator = "{" + String.fromCharCode(0x00AC) + "}";
    const SelectedVals = this.CurrentFilterSetting.selectedValues.split(separator);//.filter(f => f != '');
    if (SelectedVals.find(f => f == val)) return true;
    return false;
  }
  public ChangeSelected(val: string, event: Event) {
    //split by "{¬}" see if
    //if (!this.CurrentFilterSetting) return;
    console.log("changeSel", event, val);
    const separator = "{" + String.fromCharCode(0x00AC) + "}";
    let SelectedVals = this.CurrentFilterSetting.selectedValues.split(separator);//.filter(f => f != '');
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
    SelectedVals.sort();
    this.CurrentFilterSetting.selectedValues = SelectedVals.join(separator);
    this.MetaAnalysisService.ApplyFilters();
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






