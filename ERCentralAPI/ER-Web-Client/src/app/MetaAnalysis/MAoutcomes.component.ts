import { Component, OnInit, OnDestroy, EventEmitter, Output, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { DynamicColumnsOutcomes, IdAndNamePair, MetaAnalysis, MetaAnalysisSelectionCrit, MetaAnalysisService } from '../services/MetaAnalysis.service';
import { ExtendedOutcome } from '../services/outcomes.service';
import { CustomSorting } from '../helpers/CustomSorting';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ExcelService } from '../services/excel.service';
import { encodeBase64, saveAs } from '@progress/kendo-file-saver';



@Component({
  selector: 'MAoutcomes',
  templateUrl: './MAoutcomes.component.html',
  providers: [],
  styles: [
`
.OutcomesTableContainer {border-top: 1px solid DarkBlue; border-bottom: 1px solid DarkBlue; max-height: 55vh; overflow:auto; max-width:95vw;}
.OutcomesTable table { max-height: 50vh; max-width: 90vm; }
.OutcomesTable th {border: 0 1px dotted Silver; min-width:3vw;}

.OutcomesTable td {border: 1px dotted Silver;}
.sortableTH { cursor:pointer;}
.QuestionCol { background: Khaki !important; border: 1px dotted white !important; cursor:pointer;}
.QuestionColOutcome { background: LightSkyBlue !important; border: 1px dotted white !important; cursor:pointer;}
.AnswerCol { background: LemonChiffon !important; border: 1px dotted white !important; cursor:pointer;}
.AnswerColOutcome { background: PowderBlue !important; border: 1px dotted white !important; cursor:pointer;}
.ConstantCol{ border-left: 1px dotted silver !important;  border-right: 1px dotted silver !important;}
.ClassifCol { background: LightGray !important; border: 1px dotted white !important; cursor:pointer;}
.FirstQuestion, .FirstAnswer, .FirstClassif {border-left:1px solid DarkBlue !important;}
.clickableIcon {padding: 6px 8px 8px 8px ; border: 1px solid #00000000; border-radius: 3px;}
.clickableIcon:hover {border: 1px solid blue; border-radius: 3px; color:blue;}
.text-danger.clickableIcon:hover {border: 1px solid red; border-radius: 3px; color:red;}
.DisabledClickableIcon { color:Gray !important;}
.DisabledClickableIcon:hover {border: 1px solid Gray !important; color:Gray !important;}

`]
})
  //see https://stackoverflow.com/a/47923622 for how the "ticky" thing works for tableFixHead!!
  //.OutcomesTable thead th { box-shadow: inset 0px -0.8px #222222, 0 0 #000; }
export class MAoutcomesComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private router: Router,
    private MetaAnalysisService: MetaAnalysisService,
    private ConfirmationDialogService: ConfirmationDialogService,
    private ExcelService: ExcelService,
    @Inject('BASE_URL') private _baseUrl: string
  ) { }
  ngOnInit() {
    
  }

  @Output() PleaseEditThisFilter = new EventEmitter<string>();
  @Output() PleaseSaveTheCurrentMA = new EventEmitter<void>();

  public ExportTo: string = "Excel";
  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }
  public get Outcomes(): ExtendedOutcome[] {
    if (this.MetaAnalysisService.CurrentMetaAnalysis == null) return [];
    else return this.MetaAnalysisService.FilteredOutcomes;
  }
  public get ColumnVisibility(): DynamicColumnsOutcomes {
    return this.MetaAnalysisService.ColumnVisibility;
  }

  public get HasSelections(): number {
    //console.log("HasSelections o1", this.Outcomes.length, this.Outcomes.filter(f => f.isSelected == true).length);
    const selectedCount = this.Outcomes.filter(f => f.isSelected == true).length;
    if (selectedCount == 0) return 0;
    const selectableCount = this.Outcomes.filter(f => f.canSelect == true).length;
    //console.log("HasSelections o2", this.Outcomes.length, this.Outcomes.filter(f => f.canSelect == true).length);
    if (selectedCount != selectableCount) return 1; //partial selection
    else return 2;
  }
  public get SelectedCount(): number {
    return this.Outcomes.filter(f => f.isSelected == true).length;
  }

  public get ColSpanOnOutcomes(): number {
    return this.ColumnVisibility.AnswerOutcomeHeaders.length + this.ColumnVisibility.QuestionOutcomeHeaders.length;
  }
  public get ColSpanOnItems(): number {
    return this.ColumnVisibility.AnswerHeaders.length + this.ColumnVisibility.QuestionHeaders.length;
  }
  public get ColSpanOnOCC(): number {
    return this.ColumnVisibility.ClassificationHeaders.length;
  }


  public SortingSymbol(fieldName: string): string {
    return CustomSorting.SortingSymbol(fieldName, this.MetaAnalysisService.LocalSort);
  }
  public Sort(fieldname: string) {
    this.MetaAnalysisService.SortOutcomesBy(fieldname);
  }
  public IsFilteringOnThisCol(ER4Colname: string): boolean {
    if (this.MetaAnalysisService.CurrentMetaAnalysis == null) return false;
    if (this.MetaAnalysisService.CurrentMetaAnalysis.filterSettingsList.findIndex(f => !f.isClear && f.columnName == ER4Colname) > -1) return true;
    return false;
  }

  public DoEditThisFilter(fieldName: string, event: Event) {
    event.stopPropagation();
    this.PleaseEditThisFilter.emit(fieldName);
  }
  public SelectAll(event: Event) {
    event.stopPropagation();
    for (let o of this.Outcomes) {
      if (o.canSelect == true) o.isSelected = true;
    }
  }
  public UnSelectAll(event: Event) {
    event.stopPropagation();
    if (this.MetaAnalysisService.CurrentMetaAnalysis && this.MetaAnalysisService.CurrentMetaAnalysis.sortedBy == "isSelected") {
      this.MetaAnalysisService.UnSortOutcomes();
    }
    for (let o of this.Outcomes) {
      o.isSelected = false;
    }
  }
  public DeleteColumn(colToDelete: IdAndNamePair, event: Event) {
    event.stopPropagation();
    if (!this.HasWriteRights) return;
    this.ConfirmationDialogService.confirm("Delete column?"
      , "Are you sure you want to delete this column? "
      + "<div class='w-100 p-0 mx-0 my-2 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
      + colToDelete.Name + "</strong></div>"
      + "Removing this column will <strong>save</strong> the whole Meta Analysis."
      , false, '')
      .then((confirm: any) => {
        if (confirm) {
          this.DoDeleteColumn(colToDelete);
        }
      });
  }
  private DoDeleteColumn(colToDelete: IdAndNamePair) {
    //need to:
    //0. Find which of the 4 types of cols we're deleting
    //1. remove name/id from 2 fields in CurrentMetaAnalysis
    //2. remove column from this.ColumnVisibility
    //3. check "sortBy", react if we're sorting by the column that's about to disappear.
    //4. save all changes! (this is to match the behaviour of "add column" where we HAVE to save the whole MA)
    //column tags: aa1-20, aq1-20, ao1-20, aqo1-20
    if (this.MetaAnalysisService.CurrentMetaAnalysis == null) return;
    const separator = String.fromCharCode(0x00AC); //the "not" simbol, or inverted pipe

    //answers for entire item
    let ind: number = this.ColumnVisibility.AnswerHeaders.indexOf(colToDelete);
    let colname = "";
    if (ind != -1) {
      //answer col
      //attributeIdAnswer only has commas between elements, so there is some discerning to do...
      if (this.ColumnVisibility.AnswerHeaders.length == 1) {//only element
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeAnswerText = "";
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswer = "";
      } else {
        if (ind == 0) {//first element of many
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswer =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswer.replace(colToDelete.Id.toString() + ',', '');
        } else {
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswer =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswer.replace(',' + colToDelete.Id.toString(), '');
        }
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeAnswerText =
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeAnswerText.replace(colToDelete.Name + separator, '');
      }
      colname = "aa" + (ind + 1).toString();
      this.ColumnVisibility.AnswerHeaders.splice(ind, 1);
    }

    //questions for entire item
    ind = this.ColumnVisibility.QuestionHeaders.indexOf(colToDelete);
    if (ind != -1) {
      //question col
      //attributeIdQuestion only has commas between elements, so there is some discerning to do...
      if (this.ColumnVisibility.QuestionHeaders.length == 1) {//only element
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeQuestionText = "";
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestion = "";
      } else {
        if (ind == 0) {//first element of many 
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestion =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestion.replace(colToDelete.Id.toString() + ',', '');
        } else {
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestion =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestion.replace(',' + colToDelete.Id.toString(), '');
        }
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeQuestionText =
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeQuestionText.replace(colToDelete.Name + separator, '');
      }
      colname = "aq" + (ind + 1).toString();
      this.ColumnVisibility.QuestionHeaders.splice(ind, 1);
    }

    //answers for codes on outcomes
    ind = this.ColumnVisibility.AnswerOutcomeHeaders.indexOf(colToDelete);
    colname = "";
    if (ind != -1) {
      //answer col
      //attributeIdAnswer only has commas between elements, so there is some discerning to do...
      if (this.ColumnVisibility.AnswerOutcomeHeaders.length == 1) {//only element
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeAnswerOutcomeText = "";
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswerOutcome = "";
      } else {
        if (ind == 0) {//first element of many
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswerOutcome =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswerOutcome.replace(colToDelete.Id.toString() + ',', '');
        } else {
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswerOutcome =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswerOutcome.replace(',' + colToDelete.Id.toString(), '');
        }
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeAnswerOutcomeText =
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeAnswerOutcomeText.replace(colToDelete.Name + separator, '');
      }
      colname = "ao" + (ind + 1).toString();
      this.ColumnVisibility.AnswerOutcomeHeaders.splice(ind, 1);
    }

    //questions for entire item
    ind = this.ColumnVisibility.QuestionOutcomeHeaders.indexOf(colToDelete);
    if (ind != -1) {
      //question col
      //attributeIdQuestion only has commas between elements, so there is some discerning to do...
      if (this.ColumnVisibility.QuestionOutcomeHeaders.length == 1) {//only element
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeQuestionOutcomeText = "";
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestionOutcome = "";
      } else {
        if (ind == 0) {//first element of many 
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestionOutcome =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestionOutcome.replace(colToDelete.Id.toString() + ',', '');
        } else {
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestionOutcome =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestionOutcome.replace(',' + colToDelete.Id.toString(), '');
        }
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeQuestionOutcomeText =
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeQuestionOutcomeText.replace(colToDelete.Name + separator, '');
      }
      colname = "aqo" + (ind + 1).toString();
      this.ColumnVisibility.QuestionOutcomeHeaders.splice(ind, 1);
    }
    

    if (this.MetaAnalysisService.CurrentMetaAnalysis.sortedBy == colname) {
      this.MetaAnalysisService.UnSortOutcomes();
    }
    this.PleaseSaveTheCurrentMA.emit();
  }

  public ExportOutcomes() {
    if (this.ExportTo == "Excel" || this.ExportTo == "Html"
      || this.ExportTo == "CSV" || this.ExportTo == "TSV") this.ExportTable();
    else if (this.ExportTo == "ExcelRD" || this.ExportTo == "HtmlRD" || this.ExportTo == "CSVRD" || this.ExportTo == "TSVRD") this.ExportRawData();
  }
  private ExportTable() {
    //first build the list of columns
    let Cols: NameValuePair[] = [
      { name: "Is Selected", value: "isSelected" }
      , { name: "ES", value: "esRounded" }
      , { name: "SE", value: "seesRounded" }
      , { name: "ID", value: "outcomeId" }
      , { name: "Study", value: "shortTitle" }
      , { name: "Outc. Desc.", value: "title" }
      , { name: "Timepoint", value: "timepointDisplayValue" }
      , { name: "Type", value: "outcomeTypeName" }
      , { name: "Outcome", value: "outcomeText" }
      , { name: "Intervention", value: "interventionText" }
      , { name: "Comparison", value: "controlText" }
      , { name: "Arm 1", value: "grp1ArmName" }
      , { name: "Arm 2", value: "grp2ArmName" }
    ];
    let i = 1;
    for (let col of this.ColumnVisibility.AnswerOutcomeHeaders) {
      Cols.push({ name: col.Name, value: "ao" + i.toString() });
      i++;
    }
    i = 1;
    for (let col of this.ColumnVisibility.QuestionOutcomeHeaders) {
      Cols.push({ name: col.Name, value: "aoq" + i.toString() });
      i++;
    }
    i = 1;
    for (let col of this.ColumnVisibility.AnswerHeaders) {
      Cols.push({ name: col.Name, value: "aa" + i.toString() });
      i++;
    }
    i = 1;
    for (let col of this.ColumnVisibility.QuestionHeaders) {
      Cols.push({ name: col.Name, value: "aq" + i.toString() });
      i++;
    }
    i = 1;
    for (let col of this.ColumnVisibility.ClassificationHeaders) {
      let count = Cols.filter(f => f.name == col.Name).length;
      let colName = col.Name;
      if (count > 0) colName += " (" + (count + 1).toString() + ")";
      Cols.push({ name: colName, value: "occ" + i.toString() });
      i++;
    }
    //second get the data in the desired "digested format".
    let ExportingData: any[] = [];
    if (this.ExportTo != "Excel") {
      let header: any = {};
      for (let NVP of Cols) {
        const key = NVP.value as keyof ExtendedOutcome;
        header[key] = NVP.name;
      }
      ExportingData.push(header);
    }
    for (let o of this.Outcomes) {
      let row: any = {};
      for (let NVP of Cols) {
        const key = NVP.value as keyof ExtendedOutcome;
        const OutKey = NVP.name as keyof any;
        row[OutKey] = o[key];
      }
      ExportingData.push(row);
    }
    //last if we have data, send it to the correct output method
    if (ExportingData.length > 1) {
      if (this.ExportTo == "Excel") { this.ExportThisDataToExcel(ExportingData); }
      else if (this.ExportTo == "Html") { this.ExportThisDataToHTML(ExportingData); }
      else if (this.ExportTo == "CSV") { this.ExportThisDataToCSV(ExportingData);}
      else if (this.ExportTo == "TSV") { this.ExportThisDataToTSV(ExportingData); }
    }
  }
  private ExportThisDataToExcel(data: any[]) {
    if (this.MetaAnalysisService.CurrentMetaAnalysis) {
      this.ExcelService.exportAsExcelFile(data, this.MetaAnalysisService.CurrentMetaAnalysis.title + ' - Outcomes');
    }
  }
  private ExportThisDataToHTML(data: any[]) {
    if (this.MetaAnalysisService.CurrentMetaAnalysis && data.length > 1) {
      let title = this.MetaAnalysisService.CurrentMetaAnalysis.title + ' - Outcomes';
      let report = "<table border='1' style='border-collapse:collapse'><thead><tr>";
      for (var prop in data[0]) {
        if (Object.prototype.hasOwnProperty.call(data[0], prop) && prop.toString() != "outcomeCodes" && prop.toString() != "manuallyEnteredOutcomeTypeId" &&  prop.toString() != "outcomeTimePoint") {
          report += "<TH>" + Helpers.htmlEncode(data[0][prop].toString()) + "</TH>";
        }
      }
      report += "</tr></thead><tbody>";
      for (let i = 1; i < data.length; i++) {
        report += "<tr>";
        const row = data[i];
        for (var prop in row) {
          if (Object.prototype.hasOwnProperty.call(row, prop) && prop.toString() != "outcomeCodes" && prop.toString() != "manuallyEnteredOutcomeTypeId" &&  prop.toString() != "outcomeTimePoint") {
            let val = "";
            if (row[prop] != undefined) val = "<td>" + Helpers.htmlEncode(row[prop].toString()) + "</td>";
            else val = "<td></td>"
            report += val;
          }
        }

        report += "</tr>";
      }
      report += "</tbody></table>";
      const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(report, this._baseUrl, title));
      saveAs(dataURI, title + ".html");
      
    }
  }
  private ExportThisDataToCSV(data: any[]) {
    if (this.MetaAnalysisService.CurrentMetaAnalysis && data.length > 1) {
      let title = this.MetaAnalysisService.CurrentMetaAnalysis.title + ' - Outcomes';
      let report = "";
      for (let i = 0; i < data.length; i++) {
        const row = data[i];
        for (var prop in row) {
          if (Object.prototype.hasOwnProperty.call(row, prop) && prop.toString() != "outcomeCodes" && prop.toString() != "manuallyEnteredOutcomeTypeId" &&  prop.toString() != "outcomeTimePoint") {
            let val = "";
            if (row[prop] != undefined) val = row[prop].toString().replace(/,/g, '') + ","
            else val = ",";
            report += val;
          }
        }
        report = report.substring(0, report.length - 1) + "\r\n";
      }
      report = report.substring(0, report.length - 2);
      const dataURI = "data:text/plain;base64," + encodeBase64(report);
      saveAs(dataURI, title + ".csv");

    }
  }
  private ExportThisDataToTSV(data: any[]) {
    if (this.MetaAnalysisService.CurrentMetaAnalysis && data.length > 1) {
      let title = this.MetaAnalysisService.CurrentMetaAnalysis.title + ' - Outcomes';
      let report = "";
      
      for (let i = 0; i < data.length; i++) {
        const row = data[i];
        for (var prop in row) {
          if (Object.prototype.hasOwnProperty.call(row, prop) && prop.toString() != "outcomeCodes" && prop.toString() != "manuallyEnteredOutcomeTypeId" &&  prop.toString() != "outcomeTimePoint") {
            let val = "";
            if (row[prop] != undefined) val = row[prop].toString().replace(/\t/g, ' ') + "\t";
            else val = ",";
            report += val;
          }
        }
        report = report.substring(0, report.length - 1) + "\r\n";
      }
      report = report.substring(0, report.length - 2);
      const dataURI = "data:text/plain;base64," + encodeBase64(report);
      saveAs(dataURI, title + ".tsv");
    }
  }

  private ExportRawData() {
    if (this.MetaAnalysisService.CurrentMetaAnalysis && this.Outcomes.length > 0) {
      const data = this.Outcomes;
      let ToSend: any[] = [];
      
      const ColVis = this.ColumnVisibility;
      //1st the data
      for (const outc of data) {
        ToSend.push(new RawDataOutcome(outc, ColVis));
      }
      //2nd the headers
      if (this.ExportTo !== "ExcelRD") {
        const row1 = ToSend[0] as any;
        let headerRow: any = {};
        for (var prop in row1) {
          if (Object.prototype.hasOwnProperty.call(row1, prop) && prop.toString() != "outcomeCodes" && prop.toString() != "manuallyEnteredOutcomeTypeId" && prop.toString() != "outcomeTimePoint") {
            headerRow[prop] = prop.toString();
          }
        }
        ToSend.unshift(headerRow);
      }

      if (this.ExportTo == "ExcelRD") this.ExportThisDataToExcel(ToSend);
      else if (this.ExportTo == "HtmlRD") this.ExportThisDataToHTML(ToSend);
      else if (this.ExportTo == "CSVRD") this.ExportThisDataToCSV(ToSend);
      else if (this.ExportTo == "TSVRD") this.ExportThisDataToTSV(ToSend);

    }
  }


  ngOnDestroy() { }
}
class NameValuePair {
  name: string = "";
  value: string = "";
}
class RawDataOutcome {
  public static OutcomeTypeMapper(TypeId: number): string { return ""; }
  constructor(outc: ExtendedOutcome, ColVis: DynamicColumnsOutcomes) {
    this.isSelected = outc.isSelected;
    this.outcomeId = outc.outcomeId;
    this.shortTitle = outc.shortTitle;
    this.itemId = outc.itemId;
    this.title = outc.title;
    this.outcomeDescription = outc.outcomeDescription;
    this.outcomeTypeName = outc.outcomeTypeName; //continuous,binary,correlation??
    this.outcomeTypeDetail = RawDataOutcome.OutcomeTypeMapper(outc.outcomeTypeId);
    this.timepointDisplayValue = outc.timepointDisplayValue;
    this.itemTimepointValue = Number(outc.itemTimepointValue);
    this.itemTimepointMetric = outc.itemTimepointMetric;
    this.outcomeText = outc.outcomeText; //(name of the outcome code, if any)
    this.itemAttributeIdOutcome = outc.itemAttributeIdOutcome;
    this.interventionText = outc.interventionText;
    this.itemAttributeIdIntervention = outc.itemAttributeIdIntervention;
    this.controlText = outc.controlText;
    this.itemAttributeIdControl = outc.itemAttributeIdControl;
    this.grp1ArmName = outc.grp1ArmName;
    this.itemArmIdGrp1 = outc.itemArmIdGrp1;
    this.grp2ArmName = outc.grp2ArmName;
    this.itemArmIdGrp2 = outc.itemArmIdGrp2;
    this.Data1 = outc.data1;
    this.Data1Desc = outc.data1Desc;
    this.Data2 = outc.data2;
    this.Data2Desc = outc.data2Desc;
    this.Data3 = outc.data3;
    this.Data3Desc = outc.data3Desc;
    this.Data4 = outc.data4;
    this.Data4Desc = outc.data4Desc;
    this.Data5 = outc.data5;
    this.Data5Desc = outc.data5Desc;
    this.Data6 = outc.data6;
    this.Data6Desc = outc.data6Desc;
    this.Data7 = outc.data7;
    this.Data7Desc = outc.data7Desc;
    this.Data8 = outc.data8;
    this.Data8Desc = outc.data8Desc;
    this.Data9 = outc.data9;
    this.Data9Desc = outc.data9Desc;
    this.Data10 = outc.data10;
    this.Data10Desc = outc.data10Desc;
    this.Data11 = outc.data11;
    this.Data11Desc = outc.data11Desc;
    this.Data12 = outc.data12;
    this.Data12Desc = outc.data12Desc;
    this.Data13 = outc.data13;
    this.Data13Desc = outc.data13Desc;
    this.Data14 = outc.data14;
    this.Data14Desc = outc.data14Desc;
    this.feWeight = outc.feWeight;
    this.reWeight = outc.reWeight;
    this.smd = outc.smd;
    this.sesmd = outc.sesmd;
    this.r = outc.r;
    this.ser = outc.ser;
    this.oddsRatio = outc.oddsRatio;
    this.seOddsRatio = outc.seOddsRatio;
    this.riskRatio = outc.riskRatio;
    this.seRiskRatio = outc.seRiskRatio;
    this.ciUpperSMD = outc.ciUpperSMD;
    this.ciLowerSMD = outc.ciLowerSMD;
    this.ciUpperR = outc.ciUpperR;
    this.ciLowerR = outc.ciLowerR;
    this.ciUpperOddsRatio = outc.ciUpperOddsRatio;
    this.ciLowerOddsRatio = outc.ciLowerOddsRatio;
    this.ciUpperRiskRatio = outc.ciUpperRiskRatio;
    this.ciLowerRiskRatio = outc.ciLowerRiskRatio;
    this.ciUpperRiskDifference = outc.ciUpperRiskDifference;
    this.ciLowerRiskDifference = outc.ciLowerRiskDifference;
    this.ciUpperPetoOddsRatio = outc.ciUpperPetoOddsRatio;
    this.ciLowerPetoOddsRatio = outc.ciLowerPetoOddsRatio;
    this.ciUpperMeanDifference = outc.ciUpperMeanDifference;
    this.ciLowerMeanDifference = outc.ciLowerMeanDifference;
    this.riskDifference = outc.riskDifference;
    this.seRiskDifference = outc.seRiskDifference;
    this.meanDifference = outc.meanDifference;
    this.seMeanDifference = outc.seMeanDifference;
    this.petoOR = outc.petoOR;
    this.sePetoOR = outc.sePetoOR;
    this.es = outc.es;
    this.esDesc = outc.esDesc;
    this.sees = outc.sees;
    this.seDesc = outc.seDesc;
    this.ciLower = outc.ciLower;
    this.ciUpper = outc.ciUpper;
    let OptionalPropName: string = "";
    let index = 0;
    for (const oA of ColVis.AnswerOutcomeHeaders) {
      OptionalPropName = oA.Name + " (Outc. Lev. A." + (index+1).toString() + ")";
      (this as any)[OptionalPropName] = (outc as any)["ao" + (index + 1).toString()];
      console.log("testing1: ", (this as any)[OptionalPropName]);
      index++;
    }
    index = 0;
    for (const oA of ColVis.QuestionOutcomeHeaders) {
      OptionalPropName = oA.Name + " (Outc. Lev. Q." + (index + 1).toString() + ")";
      (this as any)[OptionalPropName] = (outc as any)["aqo" + (index + 1).toString()];
      console.log("testing2: ", (this as any)[OptionalPropName]);
      index++;
    }
    index = 0;
    for (const oA of ColVis.AnswerHeaders) {
      OptionalPropName = oA.Name + " (Item Lev. A." + (index + 1).toString() + ")";
      (this as any)[OptionalPropName] = (outc as any)["aa" + (index + 1).toString()];
      console.log("testing3: ", (this as any)[OptionalPropName]);
      index++;
    }
    index = 0;
    for (const oA of ColVis.QuestionHeaders) {
      OptionalPropName = oA.Name + " (Item Lev. Q." + (index + 1).toString() + ")";
      (this as any)[OptionalPropName] = (outc as any)["aq" + (index + 1).toString()];
      console.log("testing4: ", (this as any)[OptionalPropName]);
      index++;
    }
    index = 0;
    for (const oA of ColVis.ClassificationHeaders) {
      OptionalPropName = oA.Name + " (Outc. Classif." + (index + 1).toString() + ")";
      (this as any)[OptionalPropName] = (outc as any)["occ" + (index + 1).toString()];
      console.log("testing5: ", (this as any)[OptionalPropName]);
      index++;
      if (index == 30) break;
    }
  }
  isSelected: boolean = false;
  outcomeId: number = 0;
  shortTitle: string = "";
  itemId: number = 0;
  title: string = "";
  outcomeDescription: string = "";
  outcomeTypeName: string = ""; //continuous,binary,correlation??
  outcomeTypeDetail: string = ""; //see bottom list
  timepointDisplayValue: string = "";
  itemTimepointValue: number = 0;
  itemTimepointMetric: string = "";
  outcomeText: string = ""; //(name of the outcome code, if any)
  itemAttributeIdOutcome: number = 0;
  interventionText: string = "";
  itemAttributeIdIntervention: number = 0;
  controlText: string = "";
  itemAttributeIdControl: number = 0;
  grp1ArmName: string = "";
  itemArmIdGrp1: number = 0;
  grp2ArmName: string = "";
  itemArmIdGrp2: number = 0;
  Data1: number = 0;
  Data1Desc: string = "";
  Data2: number = 0;
  Data2Desc: string = "";
  Data3: number = 0;
  Data3Desc: string = "";
  Data4: number = 0;
  Data4Desc: string = "";
  Data5: number = 0;
  Data5Desc: string = "";
  Data6: number = 0;
  Data6Desc: string = "";
  Data7: number = 0;
  Data7Desc: string = "";
  Data8: number = 0;
  Data8Desc: string = "";
  Data9: number = 0;
  Data9Desc: string = "";
  Data10: number = 0;
  Data10Desc: string = "";
  Data11: number = 0;
  Data11Desc: string = "";
  Data12: number = 0;
  Data12Desc: string = "";
  Data13: number = 0;
  Data13Desc: string = "";
  Data14: number = 0;
  Data14Desc: string = "";
  es: number = 0;
  esDesc: string = "";
  sees: number = 0;
  seDesc: string = "";
  ciLower: number = 0;
  ciUpper: number = 0;
  feWeight: number = 0;
  reWeight: number = 0;
  smd: number = 0;
  sesmd: number = 0;
  r: number = 0;
  ser: number = 0;
  oddsRatio: number = 0;
  seOddsRatio: number = 0;
  riskRatio: number = 0;
  seRiskRatio: number = 0;
  ciUpperSMD: number = 0;
  ciLowerSMD: number = 0;
  ciUpperR: number = 0;
  ciLowerR: number = 0;
  ciUpperOddsRatio: number = 0;
  ciLowerOddsRatio: number = 0;
  ciUpperRiskRatio: number = 0;
  ciLowerRiskRatio: number = 0;
  ciUpperRiskDifference: number = 0;
  ciLowerRiskDifference: number = 0;
  ciUpperPetoOddsRatio: number = 0;
  ciLowerPetoOddsRatio: number = 0;
  ciUpperMeanDifference: number = 0;
  ciLowerMeanDifference: number = 0;
  riskDifference: number = 0;
  seRiskDifference: number = 0;
  meanDifference: number = 0;
  seMeanDifference: number = 0;
  petoOR: number = 0;
  sePetoOR: number = 0;

  //OutcomeLevelAnswer1: boolean = false;
  //OutcomeLevelAnswerName1: string = "";
  //OutcomeLevelAnswer2: boolean = false;
  //OutcomeLevelAnswerName2: string = "";
  //OutcomeLevelAnswer3: boolean = false;
  //OutcomeLevelAnswerName3: string = "";
  //OutcomeLevelAnswer4: boolean = false;
  //OutcomeLevelAnswerName4: string = "";
  //OutcomeLevelAnswer5: boolean = false;
  //OutcomeLevelAnswerName5: string = "";
  //OutcomeLevelAnswer6: boolean = false;
  //OutcomeLevelAnswerName6: string = "";
  //OutcomeLevelAnswer7: boolean = false;
  //OutcomeLevelAnswerName7: string = "";
  //OutcomeLevelAnswer8: boolean = false;
  //OutcomeLevelAnswerName8: string = "";
  //OutcomeLevelAnswer9: boolean = false;
  //OutcomeLevelAnswerName9: string = "";
  //OutcomeLevelAnswer10: boolean = false;
  //OutcomeLevelAnswerName10: string = "";
  //OutcomeLevelAnswer11: boolean = false;
  //OutcomeLevelAnswerName11: string = "";
  //OutcomeLevelAnswer12: boolean = false;
  //OutcomeLevelAnswerName12: string = "";
  //OutcomeLevelAnswer13: boolean = false;
  //OutcomeLevelAnswerName13: string = "";
  //OutcomeLevelAnswer14: boolean = false;
  //OutcomeLevelAnswerName14: string = "";
  //OutcomeLevelAnswer15: boolean = false;
  //OutcomeLevelAnswerName15: string = "";
  //OutcomeLevelAnswer16: boolean = false;
  //OutcomeLevelAnswerName16: string = "";
  //OutcomeLevelAnswer17: boolean = false;
  //OutcomeLevelAnswerName17: string = "";
  //OutcomeLevelAnswer18: boolean = false;
  //OutcomeLevelAnswerName18: string = "";
  //OutcomeLevelAnswer19: boolean = false;
  //OutcomeLevelAnswerName19: string = "";
  //OutcomeLevelAnswer20: boolean = false;
  //OutcomeLevelAnswerName20: string = "";

  //OutcomeLevelQuestion1: boolean = false;
  //OutcomeLevelQuestionName1: string = "";
  //OutcomeLevelQuestion2: boolean = false;
  //OutcomeLevelQuestionName2: string = "";
  //OutcomeLevelQuestion3: boolean = false;
  //OutcomeLevelQuestionName3: string = "";
  //OutcomeLevelQuestion4: boolean = false;
  //OutcomeLevelQuestionName4: string = "";
  //OutcomeLevelQuestion5: boolean = false;
  //OutcomeLevelQuestionName5: string = "";
  //OutcomeLevelQuestion6: boolean = false;
  //OutcomeLevelQuestionName6: string = "";
  //OutcomeLevelQuestion7: boolean = false;
  //OutcomeLevelQuestionName7: string = "";
  //OutcomeLevelQuestion8: boolean = false;
  //OutcomeLevelQuestionName8: string = "";
  //OutcomeLevelQuestion9: boolean = false;
  //OutcomeLevelQuestionName9: string = "";
  //OutcomeLevelQuestion10: boolean = false;
  //OutcomeLevelQuestionName10: string = "";
  //OutcomeLevelQuestion11: boolean = false;
  //OutcomeLevelQuestionName11: string = "";
  //OutcomeLevelQuestion12: boolean = false;
  //OutcomeLevelQuestionName12: string = "";
  //OutcomeLevelQuestion13: boolean = false;
  //OutcomeLevelQuestionName13: string = "";
  //OutcomeLevelQuestion14: boolean = false;
  //OutcomeLevelQuestionName14: string = "";
  //OutcomeLevelQuestion15: boolean = false;
  //OutcomeLevelQuestionName15: string = "";
  //OutcomeLevelQuestion16: boolean = false;
  //OutcomeLevelQuestionName16: string = "";
  //OutcomeLevelQuestion17: boolean = false;
  //OutcomeLevelQuestionName17: string = "";
  //OutcomeLevelQuestion18: boolean = false;
  //OutcomeLevelQuestionName18: string = "";
  //OutcomeLevelQuestion19: boolean = false;
  //OutcomeLevelQuestionName19: string = "";
  //OutcomeLevelQuestion20: boolean = false;
  //OutcomeLevelQuestionName20: string = "";

  //ItemLevelAnswers: NameValuePair[] = [];//0-20
  //ItemLevelQuestions: NameValuePair[] = [];//0-20
  //OutcomeClassificationCodes: NameValuePair[] = [];//0-30
}
