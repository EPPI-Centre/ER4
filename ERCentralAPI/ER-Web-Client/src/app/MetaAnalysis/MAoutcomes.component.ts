import { Component, OnInit, OnDestroy, EventEmitter, Output, Inject } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { Item, ItemListService, StringKeyValue } from '../services/ItemList.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
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
.OutcomesTable th {border: 1px dotted Silver; min-width:3vw;}
.OutcomesTable thead th {background-color: #fbfbfb; box-shadow: inset 0px -0.8px #222222, 0 0 #000; }
.OutcomesTable td {border: 1px dotted Silver;}
.sortableTH { cursor:pointer;}
.QuestionCol { background: Khaki !important;  cursor:pointer;}
.AnswerCol { background: LemonChiffon !important; cursor:pointer;}
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
    //1. remove name/id from 2 fields in CurrentMetaAnalysis
    //2. remove column from this.ColumnVisibility
    //3. check "sortBy", react if we're sorting by the column that's about to disappear.
    //4. save all changes! (this is to match the behaviour of "add column" where we HAVE to save the whole MA)

    if (this.MetaAnalysisService.CurrentMetaAnalysis == null) return;
    const separator = String.fromCharCode(0x00AC); //the "not" simbol, or inverted pipe
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
    } else {
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
    }
    if (this.MetaAnalysisService.CurrentMetaAnalysis.sortedBy == colname) {
      this.MetaAnalysisService.UnSortOutcomes();
    }
    this.PleaseSaveTheCurrentMA.emit();
  }
  public ExportOutcomes() {
    if (this.ExportTo == "Excel" || this.ExportTo == "Html"
      || this.ExportTo == "CSV" || this.ExportTo == "TSV") this.ExportTable();
    else if (this.ExportTo == "ExcelRD") this.ExportToExcel();
    else if (this.ExportTo == "HtmlRD" || this.ExportTo == "CSVRD" || this.ExportTo == "TSVRD") this.ExportRawData();
  }
  public ExportTable() {
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
  public ExportToExcel() {
    if (this.MetaAnalysisService.CurrentMetaAnalysis) {
      let res: any[] = [];
      //res.push(["Code", "CodeId", "Count"]);
      for (let row of this.Outcomes) {
        res.push(row);
      }
      this.ExcelService.exportAsExcelFile(res, this.MetaAnalysisService.CurrentMetaAnalysis.title + ' - Outcomes');
    }
  }
  private ExportRawData() {
    if (this.MetaAnalysisService.CurrentMetaAnalysis && this.Outcomes.length > 0) {
      const data = this.Outcomes;
      //1st the headers
      const row1 = data[0] as any;
      let headerRow: any = {};
      for (var prop in row1) {
        if (Object.prototype.hasOwnProperty.call(row1, prop) && prop.toString() != "outcomeCodes" && prop.toString() != "manuallyEnteredOutcomeTypeId" &&  prop.toString() != "outcomeTimePoint") {
          headerRow[prop] = prop.toString();
        }
      }
      let ToSend = [headerRow];
      ToSend = ToSend.concat(data);
      if (this.ExportTo == "HtmlRD") this.ExportThisDataToHTML(ToSend);
      else if (this.ExportTo == "CSVRD") this.ExportThisDataToCSV(ToSend);
      else if (this.ExportTo == "TSVRD") this.ExportThisDataToTSV(ToSend);


      //let title = this.MetaAnalysisService.CurrentMetaAnalysis.title + ' - Outcomes';
      //let report = "<table border='1' style='border-collapse:collapse'><thead><tr>";
      //const data = this.Outcomes;
      ////1st the headers
      //const row1 = data[0] as any;
      //for (var prop in row1) {
      //  if (Object.prototype.hasOwnProperty.call(row1, prop)) {
      //    report += "<th>" + Helpers.htmlEncode( prop.toString()) + "</th>";
      //  }
      //}
      //report += "</tr></thead><tbody>";
      //for (let i = 1; i < data.length; i++) {
      //  const row = data[i] as any;
      //  for (var prop in row) {
      //    if (Object.prototype.hasOwnProperty.call(row, prop) && prop.toString() != "outcomeCodes" && prop.toString() != "manuallyEnteredOutcomeTypeId" &&  prop.toString() != "outcomeTimePoint") {
      //      let val = "";
      //      if (row[prop] != undefined) val = "<td>" + Helpers.htmlEncode(row[prop].toString()) + "</td>";
      //      else val = "<td></td>"
      //      report += val;
      //    }
      //  }
      //  report += "</tr>";
      //}
      //report += "</tbody></table>";
      //const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(report, this._baseUrl, title));
      //saveAs(dataURI, title + ".html");
    }
  }


  ngOnDestroy() { }
}
class NameValuePair {
  name: string = "";
  value: string = "";
}
