import { Component, Input, OnInit, OnDestroy, Inject, Output, EventEmitter, ViewChild } from '@angular/core';
import { ItemCodingService, QuickQuestionReportOptions } from '../services/ItemCoding.service';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet, ReviewSetsService } from '../services/ReviewSets.service';
import { encodeBase64, saveAs } from '@progress/kendo-file-saver';
import { CodesetTree4QuickQuestionReportComponent } from '../CodesetTrees/codesetTree4QuickQuestionReport.component';
import { Helpers } from '../helpers/HelperMethods';
import { NotificationService } from '@progress/kendo-angular-notification';

@Component({
    selector: 'quickcodingreport',
    templateUrl: './quickcodingreport.component.html',
})
export class QuickCodingReportComponent implements OnInit, OnDestroy {

	

    constructor(
        private ItemCodingService: ItemCodingService,
        private ItemListService: ItemListService,
        @Inject('BASE_URL') private _baseUrl: string,
		private ReviewSetsService: ReviewSetsService,
		private _notificationService: NotificationService
    ) { }

	ngOnInit() {
    }
    @Output() PleaseCloseMe = new EventEmitter();
    @Input() Aim:string = "";
    @ViewChild('QuestionSelector') QuestionSelector!: CodesetTree4QuickQuestionReportComponent;
    public QuickQuestionReportOptions: QuickQuestionReportOptions = new QuickQuestionReportOptions();
    public JsonReport = false;
    public get GettingReport(): boolean {
        return this.ItemCodingService.QuickCodingReportIsRunning;
    }
    public get ReportProgress(): string {
        return this.ItemCodingService.ProgressOfQuickCodingReport;
    }
    public get ReportHTML(): string {
        return this.ItemCodingService.CodingReport;
    }
    public get JsonReportContent(): string {
        if (!this.JsonReport) return "";
        if (this.ItemCodingService.jsonReport.CodeSets.length > 0 || this.ItemCodingService.jsonReport.References.length > 0) {
            let step = JSON.stringify(this.ItemCodingService.jsonReport);
            step = step.replace(/,"Attributes":\{"AttributesList":\[]\}/g, "");
            return step;
        }
        return "";
    }
    public get CanSaveReport(): boolean {
        if (this.GettingReport) return false;
        if (!this.JsonReport && this.ReportHTML.length > 0) return true;
        if (this.JsonReport && (this.ItemCodingService.jsonReport.CodeSets.length > 0 || this.ItemCodingService.jsonReport.References.length > 0)) return true;
        return false;
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
        else if (this.Aim == '') {
            this.ItemCodingService.FetchCodingReport(this.ItemListService.SelectedItems, this.ReviewSetsService.ReviewSets.filter(found => found.isSelected == true), this.JsonReport);
        }
        else if (this.Aim == 'QuickQuestionReport' && this.QuestionSelector) {
           this.ItemCodingService.FetchQuickQuestionReport(this.ItemListService.SelectedItems, this.QuestionSelector.SelectedNodes, this.QuickQuestionReportOptions);
        }
	}

	public CancelQuickReport() {

		this.ItemCodingService.stopQuickReport = true;
		this._notificationService.show({
			content: "Quick Report Cancelled!",
			position: { horizontal: 'left', vertical: 'bottom' },
			animation: { type: 'fade', duration: 500 },
			type: { style: 'none', icon: false },
			hideAfter: 3000
		});
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

                pwa.document.write(Helpers.AddHTMLFrame(this.ReportHTML, this._baseUrl));
                pwa.document.close();
            }
        }
    }
    public get CanStartReport(): boolean {
        if (this.Aim == '') {
            if (this.HasSelectedCodesets && this.ItemListService.HasSelectedItems) return true;
            else return false;
        }
        else if (this.Aim == 'QuickQuestionReport') {
            if (this.HasSelectedQuestions && this.ItemListService.HasSelectedItems) return true;
            else return false;
        }
        else return false;
    }
    private get HasSelectedQuestions(): boolean {
        if (!this.QuestionSelector) return false;
        else {
            return this.QuestionSelector.SelectedNodes.length > 0;
        }
    }
    private get HasSelectedCodesets(): boolean {
        for (let Set of this.ReviewSetsService.ReviewSets) if (Set.isSelected) return true;
        return false;
    }
    public SaveReport() {
        if (this.JsonReport) this.SaveAsJson();
        else this.SaveAsHtml();
    }
    public SaveAsHtml() {
        if (this.ReportHTML.length < 1 && !this.CanStartReport) return;
        const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(this.ReportHTML, this._baseUrl));
        //console.log("Savign report:", dataURI)
        saveAs(dataURI, "Report.html");
    }

    public SaveAsJson() {
        console.log("Save as Json, codesets: " + this.ItemCodingService.jsonReport.CodeSets.length + "; refs: " + this.ItemCodingService.jsonReport.References.length);
        if (!this.JsonReport) {
            console.log("Save as Json. Return (not jsonreport)");
            return;
        }
        if ((this.ItemCodingService.jsonReport.CodeSets.length < 1 && this.ItemCodingService.jsonReport.References.length < 1) && !this.CanStartReport) {
            console.log("Save as Json. Return", this.ItemCodingService.jsonReport.CodeSets.length
                , this.ItemCodingService.jsonReport.References.length < 1
                , this.CanStartReport);
            return;
        }
        console.log("Save as Json. Encoding");
        const dataURI = "data:text/plain;base64," + encodeBase64(this.JsonReportContent);
        const blob = this.dataURItoBlob(dataURI);
        console.log("Savign json report...");//, dataURI)
        saveAs(blob, "Report.json");
    }
    private dataURItoBlob(dataURI: string): Blob {
        // convert base64/URLEncoded data component to raw binary data held in a string
        var byteString;
        if (dataURI.split(',')[0].indexOf('base64') >= 0)
            byteString = atob(dataURI.split(',')[1]);
        else
            byteString = unescape(dataURI.split(',')[1]);

        // separate out the mime component
        var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];

        // write the bytes of the string to a typed array
        var ia = new Uint8Array(byteString.length);
        for (var i = 0; i < byteString.length; i++) {
            ia[i] = byteString.charCodeAt(i);
        }

        let res = new Blob([ia], { type: mimeString });
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
        this.ItemCodingService.Clear();
        this.ReviewSetsService.clearItemData();//because we are hijacking the "isSelected" field of reviewSets;
    }
}
interface SelectableReviewSet {
    isSelected: boolean;
    reviewSet: ReviewSet;
}