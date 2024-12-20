import { Component, OnInit, OnDestroy, Input, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { ItemListService, Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet, ReadOnlyItemAttribute, ItemAttributeFullTextDetails } from '../services/ItemCoding.service';
import { ReviewSetsService, ReviewSet, SetAttribute } from '../services/ReviewSets.service';
import { encodeBase64, saveAs } from '@progress/kendo-file-saver';
import { Subscription } from 'rxjs';
import { ComparisonsService } from '../services/comparisons.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Helpers } from '../helpers/HelperMethods';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import { Outcome } from '../services/outcomes.service';

@Component({

    selector: 'codingRecordComp',
    templateUrl: './codingRecord.component.html'

})

export class codingRecordComp implements OnInit, OnDestroy {

    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        public ItemListService: ItemListService,
        private _comparisonService: ComparisonsService,
        private _ItemCodingService: ItemCodingService,
        private _ReviewSetsService: ReviewSetsService,
        private notificationService: NotificationService
    ) { }
    ngOnInit() {
        this.ItemCodingServiceDataChanged = this._ItemCodingService.DataChanged.subscribe(
            () => {
                //this.CodingRecords = [];
                //this._ItemCodingService.ItemCodingList.forEach(

                //	(x) => {

                //		let tmp = new ItemSet();
                //		tmp = x;
                //		this.CodingRecords.push(tmp);
                console.log("this.ItemCodingServiceDataChanged...");
                this.ComparisonReportHTML = "";
                //});
            }
        );
    }

    faArrowsRotate = faArrowsRotate;
    faSpinner = faSpinner;

    @Input() item: Item | undefined = new Item();
    public get CodingRecords(): ItemSet[] {
        return this._ItemCodingService.ItemCodingList;
    }
    allowUnsort: boolean = true;
    public itemsSelected: ItemSet[] = [];
    private comparison1: ItemSet | null = null;
    private comparison2: ItemSet | null = null;
    private comparison3: ItemSet | null = null;
    private ItemCodingServiceDataChanged: Subscription | null = null;
    public ComparisonReportHTML: string = "";
    public get DataSourceModel(): GridDataResult {
        return {
            data: orderBy(this._ItemCodingService.ItemCodingList, this.sort),
            total: this._ItemCodingService.ItemCodingList.length,
        };
    }
    public sort: SortDescriptor[] = [{
        field: 'setName'
        , dir: 'asc'
    }];
    public sortChangeModel(sort: SortDescriptor[]): void {
        this.sort = sort;
        console.log('sorting?', this.sort);
    }
    public get IsServiceBusy(): boolean {
        return this._ItemCodingService.IsBusy;
    }
     

    checkboxChange(item: CodingRecord) {
        //console.log("checkboxChange", item);
        this.itemsSelected = this.CodingRecords.filter((x) => x.isSelected == true);
        let count: number = this.itemsSelected.length;
    }

    public get CanRunComparison(): boolean {
        let isla: ItemSet[] = this.itemsSelected;
        if (isla.length !== 2 && isla.length !== 3) return false;
        let rs: ReviewSet | null = this._ReviewSetsService.FindSetById(isla[0].setId);
        if (!rs || (rs.setType.setTypeId != 3 && rs.setType.setTypeId != 5)) return false;
        isla = isla.filter(f => f.setId == isla[0].setId);
        if (isla.length != this.itemsSelected.length) return false;
        return true;
    }
    public get CanRunOutcomesComparison(): boolean {
        if (!this.CanRunComparison) return false;
        const isla = this.itemsSelected;
        let c = 0;
        for (let iset of isla) {
            if (iset.OutcomeList.length > 0) c++;
        }
        if (c == 2 || c == 3) return true;
        else return false;
    }
    SetComparisons(): boolean {

        this.comparison1 = null;
        this.comparison2 = null;
        this.comparison3 = null;

        let isla: ItemSet[] = this.itemsSelected;
        let itemSet: ItemSet = new ItemSet();

        if (isla != null) {

            for (var i = 0; i < isla.length; i++) {

                itemSet = isla[i];
                if (itemSet.isSelected == true) {
                    if (this.comparison1 == null) {
                        this.comparison1 = itemSet;
                    }
                    else {
                        if (this.comparison2 == null)
                            this.comparison2 = itemSet;
                        else {
                            if (this.comparison3 == null)
                                this.comparison3 = itemSet;
                            else {
                                this.ShowErrorMsg("Comparisons can handle no more than three codings.");
                                return false;
                            }
                        }
                    }
                }
            }
            if (this.comparison1 == null) {
                this.ShowErrorMsg("Nothing selected to compare");
                return false;
            }
            if (this.comparison2 == null) {
                this.ShowErrorMsg("You need to select at least two elements to compare");
                return false;
            }
            if (this.comparison2.setId != this.comparison1.setId) {
                this.ShowErrorMsg("Selected items must refer to the same Coding Tool");
                return false;
            }
            if ((this.comparison3 != null) && (this.comparison3.setId != this.comparison2.setId)) {
                this.ShowErrorMsg("Selected items must be the same Coding Tool");
                return false;
            }
        }
        return true;
    }
    private ShowErrorMsg(message: string) {
        this.notificationService.show({
            content: message,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: 'error', icon: true },
            closable: true
        });
    }


    private writeComparisonReportAttributes(comparison1: ItemSet, comparison2: ItemSet, comparison3: ItemSet | null, attributeSet: SetAttribute): string {

        let report: string = "";

        if (attributeSet.attribute_type_id > 1) {

            let oneReviewerHasSelected: boolean = false;
            let roias: ReadOnlyItemAttribute[];
            var listAttributes = comparison1.itemAttributesList.filter((x) => x.attributeId == attributeSet.attribute_id).sort(o => o.armId);
            if (listAttributes && listAttributes.length > 0) {
                roias = listAttributes;
                for (var i = 0; i < roias.length; i++) {

                    let roia: ReadOnlyItemAttribute = roias[i];

                    report += "<li><FONT COLOR='BLUE'>[" + comparison1.contactName + "] " +
                        attributeSet.attribute_name +

                        (roia.armTitle == "" ? "" : " (" + roia.armTitle + ")") +
                        "<br />" + (roia.additionalText != '' ? "[Info] <i>" + roia.additionalText + "</i>" : "") + "</font></li>";

                    oneReviewerHasSelected = true;
                    if (roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {
                        report += "<FONT COLOR='BLUE'>" + this._ItemCodingService.addFullTextToComparisonReport(roia.itemAttributeFullTextDetails) + "</FONT>";
                    }
                }

            }
            roias = comparison2.itemAttributesList.filter((x) => x.attributeId == attributeSet.attribute_id).sort(o => o.armId);

            for (var i = 0; i < roias.length; i++) {

                let roia: ReadOnlyItemAttribute = roias[i];

                report += "<li><FONT COLOR='RED'>[" + comparison2.contactName + "] " +
                    attributeSet.attribute_name +
                    (roia.armTitle == "" ? "" : " (" + roia.armTitle + ")") +
                    "<br />" + (roia.additionalText != '' ? "[Info] <i>" + roia.additionalText + "</i>" : "") + "</font></li>";
                oneReviewerHasSelected = true;
                if (roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {

                    report += "<FONT COLOR='RED'>" + this._ItemCodingService.addFullTextToComparisonReport(roia.itemAttributeFullTextDetails) + "</FONT>";
                }
            }

            if (comparison3 != null) {
                roias = comparison3.itemAttributesList.filter((x) => x.attributeId == attributeSet.attribute_id).sort(o => o.armId);

                for (var i = 0; i < roias.length; i++) {

                    let roia: ReadOnlyItemAttribute = roias[i];

                    report += "<li><FONT COLOR='GREEN'>[" + comparison3.contactName + "] " +
                        attributeSet.attribute_name +
                        (roia.armTitle == "" ? "" : " (" + roia.armTitle + ")") +
                        "<br />" + (roia.additionalText != '' ? "[Info] <i>" + roia.additionalText + "</i>" : "") + "</font></li>";
                    oneReviewerHasSelected = true;
                    if (roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {
                        report += "<FONT COLOR='GREEN'>" + this._ItemCodingService.addFullTextToComparisonReport(roia.itemAttributeFullTextDetails) + "</FONT>";
                    }
                }

            }

            if (oneReviewerHasSelected == false) {

                if ((this._ItemCodingService.CodingReportCheckChildSelected(comparison1, attributeSet) == true) ||
                    (this._ItemCodingService.CodingReportCheckChildSelected(comparison2, attributeSet) == true) ||
                    (this._ItemCodingService.CodingReportCheckChildSelected(comparison3 != null ? comparison3 : new ItemSet(), attributeSet) == true)) // ie an attribute below this is selected, even though this one isn't
                {
                    report += "<li>" + attributeSet.attribute_name + "</li>";
                    report += "<ul>";

                    for (var i = 0; i < attributeSet.attributes.length; i++) {

                        let child: SetAttribute = attributeSet.attributes[i];
                        report += this.writeComparisonReportAttributes(comparison1, comparison2, comparison3, child);
                    }
                    report += "</ul>";
                }
            }
            else {

                report += "<ul>";

                for (var i = 0; i < attributeSet.attributes.length; i++) {

                    let child: SetAttribute = attributeSet.attributes[i];
                    report += this.writeComparisonReportAttributes(comparison1, comparison2, comparison3, child);
                }
                report += "</ul>";
            }
        }
        else {

            report += "<li>" + attributeSet.attribute_name + "</li>";
            report += "<ul>";

            for (var i = 0; i < attributeSet.attributes.length; i++) {

                let child: SetAttribute = attributeSet.attributes[i];
                report += this.writeComparisonReportAttributes(comparison1, comparison2, comparison3, child);
            }
            report += "</ul>";
        }
        return report;
    }

    public ViewSingleCodingReport(itemset: ItemSet, show: boolean) {
        if (!this.item) return;
        this._ItemCodingService.FetchAllFullTextData(this.item.itemId).then(
            (GetFTWorked: boolean) => {
                if (!GetFTWorked || !this.item) {
                    return;
                }
                else {
                    //console.log("ViewSingleCodingReport", itemset.OutcomeList.length);
                    let result = this._ItemCodingService.FetchSingleCodingReport(itemset, this.item);
                    if (show) Helpers.OpenInNewWindow(result, this._baseUrl);
                    else this.SaveReportAsHtml(result);
                }
            });

    }
    SaveReportAsHtml(repHTML: string) {
        if (repHTML.length < 1) return;// && !this.CanStartReport) return;
        const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(repHTML, this._baseUrl));
        //console.log("Savign report:", dataURI)
        saveAs(dataURI, "Report.html");
    }
    public RefreshCodingRecord() {
        if (this.item) this._ItemCodingService.Fetch(this.item.itemId);
    }

    RunComparison() {
        this.ComparisonReportHTML = "";
        //console.log('RunComparison');
        let count: number = this.itemsSelected.length;
        if (!this.SetComparisons() || !this.item) {
            return;
        }

        this._ItemCodingService.FetchAllFullTextData(this.item.itemId).then(
            (GetFTWorked: boolean) => {
                if (!GetFTWorked) {
                    return;
                }
                else {
                    let firstItemSetSelected = this._ItemCodingService.ItemCodingList.find((x) => x.setId == this.itemsSelected[0].setId);
                    if (firstItemSetSelected != undefined && this.comparison1 && this.comparison2) {
                        let reviewSet = this._ReviewSetsService.ReviewSets.find((x) => firstItemSetSelected != undefined && x.set_id == firstItemSetSelected.setId);
                        if (reviewSet == undefined) return;
                        else {
                            this.ComparisonReportHTML = "<p><h1>" + reviewSet.set_name + "</h1></p><p><ul>";

                            for (var i = 0; i < reviewSet.attributes.length; i++) {

                                let attributeSet: SetAttribute = reviewSet.attributes[i];

                                this.ComparisonReportHTML += this.writeComparisonReportAttributes(this.comparison1, this.comparison2, this.comparison3, attributeSet);
                            }
                            this.ComparisonReportHTML += "</ul></p>";
                            this.ComparisonReportHTML += this.AddOutcomesToComparisonReport(this.comparison1, this.comparison2, this.comparison3, reviewSet);
                            Helpers.OpenInNewWindow(this.ComparisonReportHTML, this._baseUrl);
                        }
                    }

                }

            });
    }
    AddOutcomesToComparisonReport(comparison1: ItemSet, comparison2: ItemSet, comparison3: ItemSet | null, rSet: ReviewSet): string {
        let res: string = "";
        //console.log("AddOutcomesToComparisonReport", comparison1.OutcomeList.length, comparison2.OutcomeList.length);
        if (comparison1.OutcomeList.length > 0) {
            //console.log("AddOutcomesToComparisonReport in rev1");
            res += "<FONT COLOR='BLUE'>" + comparison1.contactName + " Outcomes:</font><br />";
            res += "<div style='background-color:#cce5ffaa'>"
                + this._ItemCodingService.OutcomesTable(comparison1.OutcomeList, false)
                + "</div>";
        }
        if (comparison2.OutcomeList.length > 0) {
            //console.log("AddOutcomesToComparisonReport in rev2");
            res += "<FONT COLOR='RED'>" + comparison2.contactName + " Outcomes:</font><br />";
            res += "<div style='background-color:#f8d7daaa'>"
                + this._ItemCodingService.OutcomesTable(comparison2.OutcomeList, false)
                + "</div>";
        }
        if (comparison3 != null && comparison3.OutcomeList.length > 0) {
            //console.log("AddOutcomesToComparisonReport in rev3");
            res += "<FONT COLOR='GREEN'>" + comparison3.contactName + " Outcomes:</font><br />";
            res += "<div style='background-color:#d4eddaaa'>"
                + this._ItemCodingService.OutcomesTable(comparison3.OutcomeList, false)
                + "</div>";
        }
        //if (res == "") res += "<p>No outcomes</p>";
        return res;
    }

    RunOutcomesComparison() {
        if (!this.CanRunOutcomesComparison) {
            this.ComparisonReportHTML = "";
            return;
        }
        let title = "Matched Outcomes for Item: " + this.item?.shortTitle + " - (Id: " + this.item?.itemId + ")";
        this.ComparisonReportHTML = "<p><strong>" + title + "</strong></p>";
        let OutcomesRev1 = this.itemsSelected[0].OutcomeList;
        let OutcomesRev2 = this.itemsSelected[1].OutcomeList;
        let Name1: string = this.itemsSelected[0].contactName;
        let Name2: string = this.itemsSelected[1].contactName;
        let Name3: string = "";
        let OutcomesRev3: Outcome[] = [];
        if (this.itemsSelected[2]) {
            OutcomesRev3 = this.itemsSelected[2].OutcomeList;
            Name3 = this.itemsSelected[2].contactName;
        }            
        let matchedIds = this._ItemCodingService.MatchOutcomes(OutcomesRev1, OutcomesRev2, OutcomesRev3);
        let unmatched1 = this._ItemCodingService.GetUnmatchedOutcomes(OutcomesRev1, matchedIds, 0);
        let unmatched2 = this._ItemCodingService.GetUnmatchedOutcomes(OutcomesRev2, matchedIds, 1);
        let unmatched3 = this._ItemCodingService.GetUnmatchedOutcomes(OutcomesRev3, matchedIds, 2);

        for (let matchedGroup of matchedIds) {
            let group: Outcome[] = [];
            let temp = OutcomesRev1.find(f => f.outcomeId == matchedGroup[0]);
            if (temp) group.push(temp);
            temp = OutcomesRev2.find(f => f.outcomeId == matchedGroup[1]);
            if (temp) group.push(temp);
            if (matchedGroup[2] != undefined && matchedGroup[2] > 0) {
                temp = OutcomesRev3.find(f => f.outcomeId == matchedGroup[2]);
                if (temp) group.push(temp);
            }
            if (group.length > 1) {
                this.ComparisonReportHTML += this._ItemCodingService.GetOutcomeTableForComparison(group, Name1, Name2, Name3) +"<br />";
            }
        }
        if (unmatched1.length > 0) {
            this.ComparisonReportHTML += "Unmatched Outcomes for <strong>" + Name1 + "</strong>:<br />";
            this.ComparisonReportHTML += this._ItemCodingService.OutcomesTable(unmatched1, false);
        }
        if (unmatched2.length > 0) {
            this.ComparisonReportHTML += "<br/>Unmatched Outcomes for <strong>" + Name2 + "</strong>:<br />";
            this.ComparisonReportHTML += this._ItemCodingService.OutcomesTable(unmatched2, false);
        }
        if (unmatched3.length > 0) {
            this.ComparisonReportHTML += "<br/>Unmatched Outcomes for <strong>" + Name3 + "</strong>:<br />";
            this.ComparisonReportHTML += this._ItemCodingService.OutcomesTable(unmatched3, false);
        }
        if (this.ComparisonReportHTML != "") {
            Helpers.OpenInNewWindow(this.ComparisonReportHTML, this._baseUrl);
        }
    }

    LiveComparison() {
        this._ItemCodingService.ToggleLiveComparison.emit();
    }
    BackToMain() {

        this.router.navigate(['Main']);
    }
    ngOnDestroy() {

        if (this.ItemCodingServiceDataChanged) {

            this.ItemCodingServiceDataChanged.unsubscribe();
        }

    }
}

export class CodingRecord {

    codeSet: string = '';
    reviewer: string = '';
    completed: boolean = false;
    locked: boolean = false;
    isSelected: boolean = false;
}








