import { Component, Inject, Injectable, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of, Subscription } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ItemCodingComp } from '../coding/coding.component';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Subject } from 'rxjs';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Item, ItemListService } from './ItemList.service';
import { ReviewSet, SetAttribute, ReviewSetsService, singleNode } from './ReviewSets.service';
import { Review } from './review.service';

@Injectable({
    providedIn: 'root',
})

export class ItemCodingService extends BusyAwareService {
    @Output() DataChanged = new EventEmitter();
    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private modalService: ModalService,
        private ReviewerIdentityService: ReviewerIdentityService
    ) { super(); }


    private _ItemCodingList: ItemSet[] = [];
    //public itemID = new Subject<number>();


    public get ItemCodingList(): ItemSet[] {
        //if (this._ItemCodingList.length == 0) {
        //    const ItemSetsJson = localStorage.getItem('ItemCodingList');
        //    let ReadOnlyReviews: ItemSet[] = ItemSetsJson !== null ? JSON.parse(ItemSetsJson) : [];
        //    if (ReadOnlyReviews == undefined || ReadOnlyReviews == null || ReadOnlyReviews.length == 0) {
        //        return this._ItemCodingList;
        //    }
        //    else {
        //        //not sure we should do anything here
        //    }
        //}
        return this._ItemCodingList;
    }
    
    public set ItemCodingList(icl: ItemSet[]) {
        this._ItemCodingList = icl;
        this.Save();
    }
        
    public Fetch(ItemId: number) {
        this._BusyMethods.push("Fetch");
        //this.itemID.next(ItemId); 
        //console.log('FetchCoding');
        let body = JSON.stringify({ Value: ItemId });
        this._httpC.post<ItemSet[]>(this._baseUrl + 'api/ItemSetList/Fetch',
            body).subscribe(result => {
                this.ItemCodingList = result;
                this.DataChanged.emit();
                //this.ReviewSetsService.AddItemData(result);
                //this.Save();
            }, error => {
                this.RemoveBusy("Fetch");
                if (this.SelfSubscription4QuickCodingReport) {
                    this.SelfSubscription4QuickCodingReport.unsubscribe();
                    this.SelfSubscription4QuickCodingReport = null;
                }
                this.modalService.SendBackHomeWithError(error);
            }
            , () => { this.RemoveBusy("Fetch");}
            );
    }
    private SelfSubscription4QuickCodingReport: Subscription | null = null;
    private _CodingReport: string = "";
    public get CodingReport(): string {
        return this._CodingReport;
    }
    public Clear() {
        this._ItemsToReport = [];
        this._CodingReport = "";
        this._CurrentItemIndex4QuickCodingReport = 0;
        if (this.SelfSubscription4QuickCodingReport) {
            this.SelfSubscription4QuickCodingReport.unsubscribe();
            this.SelfSubscription4QuickCodingReport = null;
        }
    }
    private _ItemsToReport: Item[] = [];
    private _ReviewSetsToReportOn: ReviewSet[] = [];
    private _CurrentItemIndex4QuickCodingReport: number = 0;
    public get QuickCodingReportIsRunning(): boolean {
        return this._ItemsToReport.length > this._CurrentItemIndex4QuickCodingReport;
    }
    public get ProgressOfQuickCodingReport(): string {
        return "Retreiving Item " + (this._CurrentItemIndex4QuickCodingReport + 1).toString() + " of " + this._ItemsToReport.length;
    }
    public FetchCodingReport(Items: Item[], ReviewSetsToReportOn: ReviewSet[]) {
        this._ItemsToReport = [];
        this._ReviewSetsToReportOn = [];
        if (this.SelfSubscription4QuickCodingReport) {
            this.SelfSubscription4QuickCodingReport.unsubscribe();
            this.SelfSubscription4QuickCodingReport = null;
        }
        this._CurrentItemIndex4QuickCodingReport = 0;
        this._CodingReport = "";
        if (!Items || Items.length < 1) {
            return;
        }
        this._BusyMethods.push("FetchCodingReport");
        this._ItemsToReport = Items;
        this._ReviewSetsToReportOn = ReviewSetsToReportOn;
        this.InterimGetItemCodingForReport();
        this.RemoveBusy("FetchCodingReport");
    }
    private InterimGetItemCodingForReport() {
        if (!this.SelfSubscription4QuickCodingReport) {
            //initiate recursion, ugh!
            this.SelfSubscription4QuickCodingReport = this.DataChanged.subscribe(
                () => {
                    this.AddToQuickCodingReport();
                    this._CurrentItemIndex4QuickCodingReport++;
                    this.InterimGetItemCodingForReport();
                }//no error handling: any error in this.Fetch(...) sends back home!!
            );
        }
        if (!this.QuickCodingReportIsRunning)  {
            if (this.SelfSubscription4QuickCodingReport) {
                this.SelfSubscription4QuickCodingReport.unsubscribe();
                this.SelfSubscription4QuickCodingReport = null;
            }
            return;
        }
        //passing negative item IDs make the ItemList object grab the full text as well as "normal coding"
        else this.Fetch(-this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport].itemId);
    }
    private AddToQuickCodingReport() {
        if (this._CodingReport == "") {
           // this._CodingReport = "Start of report<br />";
        }
        const currentItem = this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport];
        //console.log("AddToQuickCodingReport", currentItem);
        if (!currentItem || currentItem.itemId == 0) return;
        //this._CodingReport += "Item: "
        //    + this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport].itemId.toString()
        //    + " contains " + this.ItemCodingList.length + " item sets. <br />";
        this._CodingReport += "<h4>ID " + currentItem.itemId.toString() + ": " + currentItem.shortTitle.replace(/</g, "&lt;") + "</h4><br />" +
            ItemListService.GetCitation(currentItem) + "<br />";
        if (currentItem.oldItemId != "") {
            this._CodingReport += "<b>Your ID:</b> " + currentItem.oldItemId + "<br />";
        }
        if (currentItem.abstract != "") {
            this._CodingReport += '<div class="small mt-1"><b>Abstract:</b> ' + currentItem.abstract.replace(/</g, "&lt;") + "</div>";
        }
        this.AddCodingToReport();
        this._CodingReport += "<hr />";
    }
    private AddCodingToReport() {
        //console.log("AddCodingToReport", this._ReviewSetsToReportOn.length);
        for (let i = 0; i < this._ReviewSetsToReportOn.length; i++)
        {
            let reviewSet: ReviewSet = this._ReviewSetsToReportOn[i];
            for (let itemSet of this._ItemCodingList)
            {
                if (itemSet.setId == this._ReviewSetsToReportOn[i].set_id && itemSet.isCompleted == true) {
                    this._CodingReport += "<br /><h6>Reviewer: " + itemSet.contactName + "</h6>" ;
                    
                    if (reviewSet != null) {
                        this._CodingReport += "<p><h4>" + reviewSet.set_name + "</h4></p><p><ul>";
                        for(let attributeSet of reviewSet.attributes)
                        {
                            //console.log("about to go into writeCodingReportAttributesWithArms", itemSet, attributeSet);
                            this._CodingReport += this.writeCodingReportAttributesWithArms(itemSet, attributeSet);
                        }
                        this._CodingReport += "</ul></p>";
                        //console.log("about to go into OutcomesTable", itemSet.outcomeItemList.outcomesList);
                        this._CodingReport += "<p>" + this.OutcomesTable(itemSet.outcomeItemList.outcomesList) + "</p>";
                    }

                }
            }
        }
    }
    private writeCodingReportAttributesWithArms(itemSet: ItemSet, attributeSet: SetAttribute) :string {
        let report: string = "";
        let roias = itemSet.itemAttributesList.filter(found => found.attributeId == attributeSet.attribute_id);
        //console.log("roias", roias, itemSet.itemAttributesList, attributeSet.attribute_id);
        if (roias != null && roias.length > 0) {
            for(let roia of roias)
            {
                let AttributeName = attributeSet.attribute_name;
                if (roia.armId != 0) {
                    AttributeName += " [" + roia.armTitle + "]";
                }

                report += '<li class="text-success"><span class="font-weight-bold">' + AttributeName + "</span>";
                if (roia.additionalText.length > 0) report += "<br /><i>" + roia.additionalText.replace("\n", "<br />") + "</i>";
                if (roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {
                    
                    report += this.addFullTextToComparisonReport(roia.itemAttributeFullTextDetails);
                }
                report += "</li>";
            }
            if (this.CodingReportCheckChildSelected(itemSet, attributeSet) == true) // ie an attribute below this is selected, even though this one isn't
            {
                report += "<ul>";
                for (let child of attributeSet.attributes)
                {
                    report += this.writeCodingReportAttributesWithArms(itemSet, child);
                }
                report += "</ul>";
            }
        }
        else {
            if (this.CodingReportCheckChildSelected(itemSet, attributeSet) == true) // ie an attribute below this is selected, even though this one isn't
            {
                report += '<li class="text-muted">' + attributeSet.attribute_name + "</li>";
                report += "<ul>";
                for(let child of attributeSet.attributes)
                {
                    report += this.writeCodingReportAttributesWithArms(itemSet, child);
                }
                report += "</ul>";
            }
        }
        return report;
    }
    public addFullTextToComparisonReport(list: ItemAttributeFullTextDetails[]): string {
        //console.log("addFullTextToComparisonReport", list);
        let result: string = "";
        for (let ftd of list) {
            result += "<br style='mso-data-placement:same-cell;'  />" + ftd.docTitle + ": ";
            if (ftd.isFromPDF) {
                let rres = ftd.text.replace(/\[\u00ACs\]/g, '');//"\u00AC" is "¬", wouldn't match it otherwise
                rres = rres.replace(/\[\u00ACe\]/g, "");
                result += "<span class='small text-info'>" + rres + "</span><br style='mso-data-placement:same-cell;'  />";//.replace(/\[¬s\]/g, '').replace(/\[¬e\/]/g, "") + "</span>";
            }
            else {
                result += "<code class='small'>" + ftd.text + "(from char " + ftd.textFrom.toString() + " to char " + ftd.textTo.toString()
                    + ")</code><br style='mso-data-placement:same-cell;'  />";
            }
        }
        //console.log("addFullTextToComparisonReport", list, result);
        return result;
    }
    public OutcomesTable(Outcomes: Outcome[]): string {
        let retVal: string = "";
        let i: number = -1;

        let sortedOutcomes = Outcomes.sort(function (a, b) { return a.outcomeTypeId - b.outcomeTypeId });
        for(let o of sortedOutcomes)
        {
            if (i != o.outcomeTypeId) {
                if (retVal == "") {
                    retVal = "<p><b>Outcomes</b></p><table class='m-1' border='1'>";
                }
                else {
                    retVal += "</table><table class='m-1' border='1'>";
                }
                i = o.outcomeTypeId;
                retVal += this.GetOutcomeHeaders(o);
            }
            retVal += this.GetOutcomeInnerTable(o);
        }
        return retVal + "</table>";
    }
    private GetOutcomeHeaders(o: Outcome): string {
        let retVal = "<tr bgcolor='silver'><td>Title</td><td>Description</td><td>Outcome</td><td>Intervention</td><td>Control</td><td>Type</td>";
        switch (o.outcomeTypeId) {
            case 0: // manual entry
                retVal += "<td>SMD</td><td>SE</td><td>r</td><td>SE</td><td>Odds ratio</td><td>SE</td><td>Risk ratio</td><td>SE</td><td>Risk difference</td><td>SE</td><td>Mean difference</td><td>SE</td>";
                break;

            case 1: // n, mean, SD
                retVal += "<td>Group 1 N</td><td>Group 2 N</td><td>Group 1 mean</td><td>Group 2 mean</td><td>Group 1 SD</td>" +
                    "<td>Group 2 SD</td><td>SMD</td><td>SE</td>";
                break;

            case 2: // binary 2 x 2 table
                retVal += "<td>Group 1 events</td><td>Group 2 events</td><td>Group 1 no events</td><td>Group 2 no events</td><td>Odds ratio</td><td>SE (log OR)</td>";
                break;

            case 3: //n, mean SE
                retVal += "<td>Group 1 N</td><td>Group 2 N</td><td>Group 1 mean</td><td>Group 2 mean</td><td>Group 1 SE</td>" +
                    "<td>Group 2 SE</td><td>SMD</td><td>SE</td>";
                break;

            case 4: //n, mean CI
                retVal += "<td>Group 1 N</td><td>Group 2 N</td><td>Group 1 mean</td><td>Group 2 mean</td><td>Group 1 CI lower</td>" +
                    "<td>Group 1 CI upper</td><td>Group 2 CI lower</td><td>Group 2 CI upper</td><td>SMD</td><td>SE</td>";
                break;

            case 5: //n, t or p value
                retVal += "<td>Group 1 N</td><td>Group 2 N</td><td>Group 1 mean</td><td>Group 2 mean</td><td>t-value</td>" +
                    "<td>p-value</td><td>SMD</td><td>SE</td>";
                break;

            case 6: // diagnostic test 2 x 2 table
                retVal += "<td>True positive</td><td>False positive</td><td>False negative</td><td>True negative</td><td>Diagnostic odds ratio</td><td>SE (log dOR)</td>";
                break;

            case 7: // correlation coeffiecient r
                retVal += "<td>Group size</td><td>r</td><td>SE (Z transformed)</td>";
                break;

            default:
                break;
        }
        return retVal + "<td>Outcome Classifications</td></tr>";
    }

    private GetOutcomeInnerTable(o: Outcome): string {
        let retVal = "<tr><td>" + o.title + "</td><td>" + o.outcomeDescription.replace("\r", "<br style='mso-data-placement:same-cell;'  />") + "</td><td>" + o.outcomeText + "</td><td>" + o.interventionText +
            "</td><td>" + o.controlText + "</td>";
        switch (o.outcomeTypeId) {
            case 0: // manual entry
                retVal += "<td>Manual entry</td>" +
                    "<td>" + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data3 === 'number' ?  o.data3.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data4 === 'number' ?  o.data4.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data5 === 'number' ?  o.data5.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data6 === 'number' ?  o.data6.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data7 === 'number' ? o.data7.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data8 === 'number' ? o.data8.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data11 === 'number' ? o.data11.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data12 === 'number' ? o.data12.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data13 === 'number' ? o.data13.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data14 === 'number' ? o.data14.toFixed(3) : "NaN") + "</td>";
                break;

            case 1: // n, mean, SD
                retVal += "<td>Continuous: Ns, means and SD</td>" +
                    "<td>" + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data3 === 'number' ?  o.data3.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data4 === 'number' ?  o.data4.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data5 === 'number' ?  o.data5.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data6 === 'number' ?  o.data6.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td><td>"
                    + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
                break;

            case 2: // binary 2 x 2 table
                retVal += "<td>Binary: 2 x 2 table</td>" +
                    "<td>" + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data3 === 'number' ?  o.data3.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data4 === 'number' ?  o.data4.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td><td>"
                    + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
                break;

            case 3: //n, mean SE
                retVal += "<td>Continuous: N, Mean, SE</td>" +
                    "<td>" + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data3 === 'number' ?  o.data3.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data4 === 'number' ?  o.data4.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data5 === 'number' ?  o.data5.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data6 === 'number' ?  o.data6.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td><td>"
                    + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
                break;

            case 4: //n, mean CI
                retVal += "<td>Continuous: N, Mean, CI</td>" +
                    "<td>" + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data3 === 'number' ?  o.data3.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data4 === 'number' ?  o.data4.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data5 === 'number' ?  o.data5.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data6 === 'number' ?  o.data6.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data7 === 'number' ? o.data7.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data8 === 'number' ? o.data8.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td><td>"
                    + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
                break;

            case 5: //n, t or p value
                retVal += "<td>Continuous: N, t- or p-value</td>" +
                    "<td>" + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data3 === 'number' ?  o.data3.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data4 === 'number' ?  o.data4.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td><td>"
                    + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
                break;

            case 6: // binary 2 x 2 table
                retVal += "<td>Diagnostic test: 2 x 2 table</td>" +
                    "<td>" + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data3 === 'number' ?  o.data3.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data4 === 'number' ?  o.data4.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td><td>"
                    + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
                break;

            case 7: // correlation coefficient r
                retVal += "<td>Correlation coefficient r</td>" +
                    "<td>" + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
                    "<td>" + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
                break;

            default:
                break;
        }

        retVal += "<td>";
        for(let OIA of o.outcomeCodes)
        {
            retVal += OIA.attributeName + "<br style='mso-data-placement:same-cell;' >";
        }
        return retVal + "</td></tr>";
    }

    private CodingReportCheckChildSelected(itemSet: ItemSet, attributeSet: SetAttribute): boolean {
        if (itemSet) {
            for (let roia of itemSet.itemAttributesList) {
                if (roia.attributeId == attributeSet.attribute_id) return true;
            }
            for(let child of attributeSet.attributes)
            {
                if (this.CodingReportCheckChildSelected(itemSet, child) == true) {
                    return true;
                }
            }
        }
        return false;
    }

    public FetchQuickQuestionReport(Items: Item[], nodesToReportOn: singleNode[], options: QuickQuestionReportOptions) {
        if (this.SelfSubscription4QuickCodingReport) {
            this.SelfSubscription4QuickCodingReport.unsubscribe();
            this.SelfSubscription4QuickCodingReport = null;
        }
        this._CurrentItemIndex4QuickCodingReport = 0;
        this._CodingReport = "";
        if (!Items || Items.length < 1) {
            return;
        }
        //this._BusyMethods.push("FetchQuickQuestionReport");
        this._ItemsToReport = Items;
        this.InterimGetItemCodingForQuestionReport(nodesToReportOn, options);
        //this.RemoveBusy("FetchQuickQuestionReport");
    }
    private InterimGetItemCodingForQuestionReport(nodesToReportOn: singleNode[], options: QuickQuestionReportOptions) {
        if (!this.SelfSubscription4QuickCodingReport) {
            //initiate recursion, ugh!
            this.SelfSubscription4QuickCodingReport = this.DataChanged.subscribe(
                () => {
                    this.AddToQuickQuestionReport(nodesToReportOn, options);
                    this._CurrentItemIndex4QuickCodingReport++;
                    this.InterimGetItemCodingForQuestionReport(nodesToReportOn, options);
                }//no error handling: any error in this.Fetch(...) sends back home!!
            );
        }
        if (!this.QuickCodingReportIsRunning) {
            if (this.SelfSubscription4QuickCodingReport) {
                this.SelfSubscription4QuickCodingReport.unsubscribe();
                this.SelfSubscription4QuickCodingReport = null;
                this._CodingReport += "</table>";
            }
            return;
        }
        //passing negative item IDs make the ItemList object grab the full text as well as "normal coding"
        else this.Fetch(-this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport].itemId);
    }
    private AddToQuickQuestionReport(nodesToReportOn: singleNode[], options: QuickQuestionReportOptions) {
        if (this._CodingReport == "") {
            //this._CodingReport = "Quick Question Report:<br />";
            this._CodingReport += "<table class='border border-dark'><tr><th class='border border-dark'>Item</th>";
            if (options.IncludeFullTitle) {
                this._CodingReport += "<th class='border border-dark'>Title</th>";
            }
            for (let node of nodesToReportOn) {
                this._CodingReport += "<th class='border border-dark'>" + node.name;
                if (options.ShowCodeIds) {
                    if (node.nodeType == "ReviewSet") this._CodingReport += " (" + node.id.substring(2) + ")";
                    else if (node.nodeType == "SetAttribute") this._CodingReport += " ("+ node.id.substring(1)+")";
                }
                this._CodingReport += "</th>";
            }
            this._CodingReport += "</tr>";
        }
        const currentItem = this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport];
        //console.log("AddToQuickCodingReport", currentItem);
        if (!currentItem || currentItem.itemId == 0) return;
        this._CodingReport += "<tr><td class='border border-dark'>" + currentItem.shortTitle + " (ID:" + currentItem.itemId + ")</td>";
        if (options.IncludeFullTitle) {
            this._CodingReport += "<td class='border border-dark'>"+ currentItem.title + "</td>";
        }
        this.AddQuestionCodingToReport(nodesToReportOn, options);
        this._CodingReport += "</tr>";
    }
    AddQuestionCodingToReport(nodesToReportOn: singleNode[], options: QuickQuestionReportOptions) {
        for (let node of nodesToReportOn) {
            this._CodingReport += "<td class='border border-dark'>";
            let ChildrenIds: number[] = [];
            for (let aNode of node.attributes) {
                ChildrenIds.push((aNode as SetAttribute).attribute_id);
            }
            for (let itemSet of this._ItemCodingList.filter(found => found.isCompleted)) {
                for (let roia of itemSet.itemAttributesList) {
                    if (ChildrenIds.indexOf(roia.attributeId) > -1) {
                        //this itemSet contains a child of this node, report it:
                        let fNode = node.attributes.find(found => found.id == "A" + roia.attributeId);
                        if (fNode) {
                            this._CodingReport += "-";
                            if (roia.armId > 0) {
                                this._CodingReport += fNode.name
                                    + (options.ShowCodeIds ? "(" + roia.attributeId + ")" : "")
                                    + " [<span class='alert-info small'>" + roia.armTitle + "</span>]";
                            }
                            else this._CodingReport += fNode.name + (options.ShowCodeIds ? "(" + roia.attributeId + ")" : "");
                            if (options.ShowInfobox && roia.additionalText && roia.additionalText.length > 0)
                                this._CodingReport += "<br style='mso-data-placement:same-cell;' /><i class='small'>" + roia.additionalText.replace("\n", "<br style='mso-data-placement:same-cell;' />") + "</i>";
                            if (options.ShowCodedText && roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {
                                this._CodingReport += this.addFullTextToComparisonReport(roia.itemAttributeFullTextDetails);
                            }
                            this._CodingReport += "<br style='mso-data-placement:same-cell;' />";
                        }
                    }
                }
            }
            if (this._CodingReport.endsWith("<br style='mso-data-placement:same-cell;' />")) this._CodingReport = this._CodingReport.substring(0, this._CodingReport.length - 44);
            this._CodingReport += "</ul></td>";
        }
    }

    public Save() {
        //nope! We're not saving this to localstorage:
        //item coding comes and goes (as we change items), so best not to keep it, if needed we'll re-fetch it.

        //if (this._ItemCodingList.length > 0)
        //    localStorage.setItem('ItemCodingList', JSON.stringify(this._ItemCodingList));
        //else if (localStorage.getItem('ItemCodingList'))//to be confirmed!! 
        //    localStorage.removeItem('ItemCodingList');
    }
    public FindItemSetBySetId(DestSetId: number): ItemSet | null {
        //this is where somewhat complicated logic needs to happen. We need to replicate here the logic that decides if a new itemset is needed or not...
        let result: ItemSet | null = null;
        for (let itemSet of this._ItemCodingList) {
            if (itemSet.setId == DestSetId) {
                //we have an itemSet in the desired set: if complete, we'll use it. Otherwise, check that it belongs to current user.
                //if itemset to be used is locked, we should not even have tried, so tricky case...
                if (itemSet.isCompleted) {
                    if (itemSet.isLocked) {
                        alert('Coding is locked! We shouldn\'t be doing this...');
                        throw new Error('Coding is locked! We shouldn\'t be doing this...');
                    }
                    result = itemSet;
                    break;
                }
                else if (itemSet.contactId == this.ReviewerIdentityService.reviewerIdentity.userId) {
                    if (itemSet.isLocked) {
                        alert('Coding is locked! We shouldn\'t be doing this...');
                        throw new Error('Coding is locked! We shouldn\'t be doing this...');
                    }
                    result = itemSet;
                    break;
                }
            }
        }
        return result;
    }
}

export class ItemSet {
    itemSetId: number = 0;
    setId: number = 0;
    itemId: number = 0;
    contactId: number = 0;
    contactName: string = "";
    setName: string = "";
    isCompleted: boolean = false;
    isLocked: boolean = true;
    itemAttributesList: ReadOnlyItemAttribute[] = [];
    isSelected: boolean = false;
    outcomeItemList: OutcomeItemList = new OutcomeItemList();
}
export class ReadOnlyItemAttribute {
    attributeId: number = 0;
    itemAttributeId: number = 0;
    additionalText: string = "";
    armId: number = 0;
    armTitle: string = "";
    itemAttributeFullTextDetails: ItemAttributeFullTextDetails[] = [];
}
export class OutcomeItemList {
    outcomesList: Outcome[] = [];
}
export class Outcome {
    outcomeId: number = 0;
    itemSetId: number = 0;
    outcomeTypeName: string = "";
    outcomeTypeId: number = 0;
    outcomeCodes: OutcomeItemAttribute[] = [];
    itemAttributeIdIntervention: number = 0;
    itemAttributeIdControl: number = 0;
    itemAttributeIdOutcome: number = 0;
    title: string = "";
    shortTitle: string = "";
    outcomeDescription: string = "";
    data1: number = 0;
    data2: number = 0;
    data3: number = 0;
    data4: number = 0;
    data5: number = 0;
    data6: number = 0;
    data7: number = 0;
    data8: number = 0;
    data9: number = 0;
    data10: number = 0;
    data11: number = 0;
    data12: number = 0;
    data13: number = 0;
    data14: number = 0;
    interventionText: string = "";
    controlText: string = "";
    outcomeText: string = "";
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
    es: number = 0;
    sees: number = 0;
    nRows: number = 0;
    ciLower: number = 0;
    ciUpper: number = 0;
    esDesc: string = "";
    seDesc: string = "";
    data1Desc: string = "";
    data2Desc: string = "";
    data3Desc: string = "";
    data4Desc: string = "";
    data5Desc: string = "";
    data6Desc: string = "";
    data7Desc: string = "";
    data8Desc: string = "";
    data9Desc: string = "";
    data10Desc: string = "";
    data11Desc: string = "";
    data12Desc: string = "";
    data13Desc: string = "";
    data14Desc: string = "";
}
export interface OutcomeItemAttribute {
    outcomeItemAttributeId: number;
    outcomeId: number;
    attributeId: number;
    additionalText: string;
    attributeName: string;
}

export interface ItemAttributeFullTextDetails {
    itemDocumentId: number;
    isFromPDF: boolean;
    itemArm: string;
    docTitle: string;
    text: string;
    textTo: number;
    textFrom: number;
}

export class QuickQuestionReportOptions {
    IncludeFullTitle: boolean = false;
    ShowInfobox: boolean = true;
    ShowCodedText: boolean = true;
    ShowCodeIds: boolean = false;
}