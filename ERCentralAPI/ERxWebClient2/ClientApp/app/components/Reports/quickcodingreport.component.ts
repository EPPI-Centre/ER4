import { Component, Input, OnInit, OnDestroy, Inject, Output, EventEmitter } from '@angular/core';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet, ReviewSetsService } from '../services/ReviewSets.service';
import { encodeBase64, saveAs } from '@progress/kendo-file-saver';

@Component({
    selector: 'quickcodingreport',
    templateUrl: './quickcodingreport.component.html',
})
export class QuickCodingReportComponent implements OnInit, OnDestroy {

	

    constructor(
        private ItemCodingService: ItemCodingService,
        private ItemListService: ItemListService,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewSetsService: ReviewSetsService
    ) { }

	ngOnInit() {
    }
    @Output() PleaseCloseMe = new EventEmitter();
    public get GettingReport(): boolean {
        return this.ItemCodingService.QuickCodingReportIsRunning;
    }
    public get ReportProgress(): string {
        return this.ItemCodingService.ProgressOfQuickCodingReport;
    }
    public get ReportHTML(): string {
        return this.ItemCodingService.CodingReport;
    }
    public get CodeSets(): ReviewSet[] {
        return this.ReviewSetsService.ReviewSets;
    }
    public Close() {
        this.ReviewSetsService.clearItemData();
        this.ItemCodingService.Clear();
        this.PleaseCloseMe.emit();
    }
    public StartQuickReport() {
        if (!this.CanStartReport) return;
        else {
            this.ItemCodingService.FetchCodingReport(this.ItemListService.SelectedItems, this.ReviewSetsService.ReviewSets.filter(found => found.isSelected == true));
        }
    }
    public OpenInNewWindow() {
        if (this.ReportHTML.length < 1 && !this.CanStartReport) return;
        else if (this.ReportHTML.length < 1 && this.CanStartReport) {
            this.StartQuickReport();
        }
        else {//do the magic
            let Pagelink = "about:blank";
            let pwa = window.open(Pagelink, "_new");
            //let pwa = window.open("data:text/plain;base64," + btoa(this.AddHTMLFrame(this.ReportHTML)), "_new");
            if (pwa) {
                pwa.document.open();

                pwa.document.write(this.AddHTMLFrame(this.ReportHTML));
                pwa.document.close();
            }
        }
    }
    public get CanStartReport(): boolean {
        if (this.HasSelectedCodesets && this.ItemListService.HasSelectedItems) return true;
        else return false;
    }
    private get HasSelectedCodesets(): boolean {
        for (let Set of this.ReviewSetsService.ReviewSets) if (Set.isSelected) return true;
        return false;
    }
    public SaveAsHtml() {
        if (this.ReportHTML.length < 1 && !this.CanStartReport) return;
        const dataURI = "data:text/plain;base64," + encodeBase64(this.AddHTMLFrame(this.ReportHTML));
        console.log("ToRis", dataURI)
        saveAs(dataURI, "Report.html");
    }
    private AddHTMLFrame(report: string): string {
        let res = "<HTML id='content'><HEAD><title>EPPI-Reviewer Coding Report</title><link rel='stylesheet' href='" + this._baseUrl+ "/dist/vendor.css' /></HEAD><BODY class='m-2' id='body'>" + report;
        //res += "<br /><a download='report.html' href='data:text/html;charset=utf-8," + report + "'>Save...</a></BODY></HTML>";
        //res += "<br />" + this.AddSaveMe() + "</BODY></HTML>";
        res += "</BODY></HTML>";
        return res;
    }
    private AddSaveMe(): string {
        //see: https://stackoverflow.com/questions/29702758/html-button-to-save-div-content-using-javascript#answer-29702870
        let rep = "<script>function download(){";
        rep += "var butt = document.getElementById('saveB');";
        rep += "var body = document.getElementById('body');";
        rep += "butt.parentNode.removeChild(butt);";
        //rep += "butt.style.display = 'none';";
        rep += "var a = document.body.appendChild(";
        rep += "document.createElement('a')";
        rep += ");";
        rep += "a.download = 'export.html';";
        rep += "a.id = 'temp';";
        rep += "a.href = 'data:text/html,' + document.getElementById('content').innerHTML;";
        rep += "a.click();";
        rep += "a.parentNode.removeChild(a);";
        rep += "body.appendChild(butt);";
        rep += "}</script>";
        rep += "<button type='button' class='btn btn-success' id='saveB' onclick='download()' style='position:fixed;top:10px;right:10px;'>Save</button>";
        return rep;
    }
    ngOnDestroy() {
        console.log("Destroy in QuickCodingReportComponent");
        this.ReviewSetsService.clearItemData();//because we are hijacking the "isSelected" field of reviewSets;
    }
}
interface SelectableReviewSet {
    isSelected: boolean;
    reviewSet: ReviewSet;
}