import { Component, OnInit, Output, EventEmitter, OnDestroy } from '@angular/core';
import { ReviewSetsService, ReviewSet } from '../services/ReviewSets.service';
import { ComparisonsService, Comparison, ComparisonStatistics } from '../services/comparisons.service';
import { Router } from '@angular/router';
import { ItemListService, Item, Criteria } from '../services/ItemList.service';
import { ItemCodingService, ItemSet } from '../services/ItemCoding.service';
import { ReconciliationService, ReconcilingItemList, ReconcilingItem, ReconcilingReviewSet, ReconcilingCode } from '../services/reconciliation.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Subscription } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { faArrowRight } from '@fortawesome/free-solid-svg-icons';
import { ConfigService } from '../services/config.service';
import { Outcome } from '../services/outcomes.service';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';
import { Helpers } from '../helpers/HelperMethods';



@Component({
    selector: 'ComparisonReconciliationComp',
    templateUrl: './comparisonReconciliation.component.html',
    styles: [`
               .bg-comp {    
                    background-color: #C2F3C0;
                }
               .bg-comp-sel {    
                    background-color: #95E292;
                }
               .bg-incomp {    
                    background-color: #EEEEEE;
                }
               .bg-incomp-sel {    
                    background-color: #DCDCDC;
                }
                .table-bordered th, .table-bordered td {
                    border: 1px solid #888888;
                } 
				button.disabled {color:black; }
            `],
    providers: []
})

export class ComparisonReconciliationComp extends BusyAwareService implements OnInit, OnDestroy {

    constructor(
        private router: Router,
        private _reviewSetsService: ReviewSetsService,
        private _ItemListService: ItemListService,
        private _comparisonsService: ComparisonsService,
        private _reconciliationService: ReconciliationService,
        private _ItemDocsService: ItemDocsService,
        private ReviewerIdentityServ: ReviewerIdentityService,
        private ItemCodingService: ItemCodingService,
        configService: ConfigService
    ) {
        super(configService);
    }

    @Output() criteriaChange = new EventEmitter();
    private localList: ReconcilingItemList = new ReconcilingItemList(new ReviewSet(),
        new Comparison(), ""
    );
    private ReviewSet: ReviewSet = new ReviewSet();
    private item: Item = new Item();
    public CurrentComparison: Comparison = new Comparison();
    public DetailsView: boolean = false;
    public get CurrentContext(): string {
        if (this.DetailsView) return "reconciliation\\treesview";
        else return "reconciliation";
    }
    public panelItem: Item | undefined = new Item();
    private _MatchedOutcomesHTML: string = "";
    public get MatchedOutcomesHTML(): string { return this._MatchedOutcomesHTML; }

    private _UnmatchedOutcomesHTML: string = "";
    public get UnmatchedOutcomesHTML(): string { return this._UnmatchedOutcomesHTML; }
    public get HasWriteRights(): boolean {
        return this.ReviewerIdentityServ.HasWriteRights;
    }
    public get HasAdminRights(): boolean {
        return this.ReviewerIdentityServ.HasAdminRights;
    }
    public hideme: boolean[] = [];
    public hidemeOne: boolean[] = [];
    public hidemeTwo: boolean[] = [];
    public hidemeThree: boolean[] = [];
    public hidemearms: boolean[] = [];
    public hidemearmsTwo: boolean[] = [];
    public hidemearmsThree: boolean[] = [];
    public faArrowRight = faArrowRight;
    public ShowSaveReportOptions: boolean = false;
    public showOutcomes: string = "none";
    public showFullPath: boolean = true;
    public showInfoBox: boolean = true;
    public showShortTitle: boolean = true;
    public showFullTitle: boolean = false;
    public showAbstract: boolean = false;
    public ReconcileReportHTML: string = "";
    public showReconcileReportHTML: boolean = false;
    public LoadingMsg: string = "";

    public get CodeSets(): ReviewSet[] {
        return this._reviewSetsService.ReviewSets.filter(x => x.setType.allowComparison != false);
    }
    public get FlatAttributes(): ReconcilingCode[] {
        return this.localList.Attributes;
    }
    public get ItemList() {
        return this._ItemListService.ItemList;
    }
    public allItems: ReconcilingItem[] = [];
    public testBool: boolean = false;
    public selectedRow: number = 0;
    private subscription: Subscription | null = null;

    ngOnInit() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
        this.subscription = this._ItemListService.ListChanged.subscribe(

            () => {
                this.item = this._ItemListService.ItemList.items[0];
                this.panelItem = this._ItemListService.ItemList.items[0];
                this.selectedRow = 0;
                if (this.panelItem) {
                    this.RefreshDataItems(this.panelItem);
                    this.testBool = true;
                    //if (this.DetailsView == true) this.PrepareDetailsViewData();
                }
            }
        );
        if (this.testBool) {
            this.item = this._ItemListService.ItemList.items[0];
            this.panelItem = this._ItemListService.ItemList.items[0];
            if (this.panelItem) {
                this.RefreshDataItems(this.panelItem);
                this.testBool = false;
            }
        }
    }
    public IsServiceBusy(): boolean {
        if (this._BusyMethods.length > 0
            || this._reviewSetsService.IsBusy
            || this._ItemListService.IsBusy
            || this._comparisonsService.IsBusy
            || this._reconciliationService.IsBusy
            || this._ItemDocsService.IsBusy) return true;
        else {
            return false;
        }
    }
    getReconciliations() {
        this.ReconcileReportHTML = "";
        this.showReconcileReportHTML = false;
        this.LoadingMsg = ""
        if (this.item != null && this.item != undefined) {
            this.CurrentComparison = this._comparisonsService.currentComparison;
            if (this.CurrentComparison) {
                this.ReviewSet = this._reviewSetsService.ReviewSets.filter(
                    x => x.set_id == this.CurrentComparison.setId)[0];
                this.localList = new ReconcilingItemList(this.ReviewSet,
                    this.CurrentComparison, "");
                this._BusyMethods.push("recursiveItemList");
                let i: number = 0;
                this.LoadingMsg = "Loading Item 1 of " + this._ItemListService.ItemList.items.length.toString();
                this.recursiveItemList(i);
            }
        }
    }
    ToDetailsView() {
        this.PrepareDetailsViewData();
        this.DetailsView = true;
    }
    OpenItem(itemId: number) {
        if (itemId > 0) {
            this.router.navigate(['itemcoding', itemId]);
        }
    }
    FirstItem() {
        this.ChangePanelItem(this.allItems[0].Item.itemId, 0);
    }
    NextItem() {
        this.ChangeItem(this.selectedRow + 2);
    }
    ChangeItemSt(notOffsetIndexSt: string) {
        let notOffsetIndex = parseInt(notOffsetIndexSt);
        if (isNaN(notOffsetIndex)) return;
        this.ChangeItem(notOffsetIndex)
    }
    ChangeItem(notOffsetIndex: number) {
        let index = notOffsetIndex - 1;
        if (this.allItems.length < index) index = this.allItems.length - 1;
        if (index < 1) this.ChangePanelItem(this.allItems[0].Item.itemId, 0);
        else this.ChangePanelItem(this.allItems[index].Item.itemId, index);
    }

    PreviousItem() {
        this.ChangeItem(this.selectedRow);
    }
    LastItem() {
        this.ChangePanelItem(this.allItems[this.allItems.length - 1].Item.itemId, this.allItems.length - 1);
    }
    recursiveItemList(i: number) {

        let ItemSetlst: ItemSet[] = [];
        this._reconciliationService.FetchItemSetList(this._ItemListService.ItemList.items[i].itemId)

            .then(
                (res: ItemSet[]) => {

                    ItemSetlst = res;
                    this.localList.AddItem(this._ItemListService.ItemList.items[i], ItemSetlst);

                    if (i < this._ItemListService.ItemList.items.length - 1) {
                        i = i + 1;
                        this.LoadingMsg = "Loading Item " + (i+1).toString() + " of " + this._ItemListService.ItemList.items.length.toString();
                        this.recursiveItemList(i);
                    } else {
                        this.allItems = this.localList.Items;
                        if (this.DetailsView) this.PrepareDetailsViewData();
                        this.LoadingMsg = "";
                        this.RemoveBusy("recursiveItemList");
                        return;
                    }
                }
            );
    }
    UpdateCurrentItem() {
        console.log("updating current item...");
        if (this.panelItem) this.UpdateItem(this.panelItem);
    }
    UpdateItem(item: Item) {
        let ItemSetlst: ItemSet[] = [];
        this._reconciliationService.FetchItemSetList(item.itemId)
            .then(
                (res: ItemSet[]) => {
                    ItemSetlst = res;
                    let tmp = this.localList.GenerateItem(item, ItemSetlst);
                    if (tmp) {
                        //we want to substitute the new reconciling item in the current list
                        let index = this.localList.Items.findIndex(found => found.Item.itemId == item.itemId);
                        if (index > -1) {
                            this.localList.Items[index] = tmp;
                            if (this.DetailsView) {
                                this.PrepareDetailsViewData();
                            }
                        }
                    }

                }
            );
    }
    RefreshDataItems(item: Item) {
        this.panelItem = this._ItemListService.ItemList.items[0];
        this.getReconciliations();
        if (this.panelItem && this.panelItem != undefined) {
            if (this.panelItem) {
                this.getItemDocuments(this.panelItem.itemId);
                this._reconciliationService.FetchArmsForReconItems(
                    this._ItemListService.ItemList.items);
            }
        }
    }

    RefreshDataItem(item: Item) {

        if (item == null) {
            this.panelItem = this._ItemListService.ItemList.items[0];
        } else {
            this.panelItem = item;
        }
        //this.getReconciliations();
        this.UpdateItem(item);
        let tempItems: Item[] = [];
        tempItems[0] = this.panelItem;
        if (this.panelItem && this.panelItem != undefined) {
            if (item.itemId) {
                //this.getItemDocuments(this.panelItem.itemId);
                this._reconciliationService.FetchArmsForReconItems(
                    tempItems);
            }
        }
    }
    getItemDocuments(itemid: number) {
        if (this._ItemDocsService.CurrentItemId != itemid) this._ItemDocsService.FetchDocList(itemid);
    }
    public getReconSplitArray(fullPath: string): string[] {
        if (fullPath != '') {
            return fullPath.split("<¬sep¬>");
        } else {
            return [];
        }
    }
    public UnComplete(recon: ReconcilingItem) {

        if (recon && this.CurrentComparison) {
            //alert(recon.Item.shortTitle);
            this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, 0, false, false)
                .then(
                    () => {
                        this.RefreshDataItem(recon.Item);
                    }
                );
        }
    }
    public Complete(recon: ReconcilingItem, contactID: number) {
        if (recon && this.CurrentComparison) {
            //alert(contactID);
            this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, contactID, true, false)
                .then(
                    () => {
                        this.RefreshDataItem(recon.Item);
                    }
                );
        }
    }
    public CompleteAndLock(recon: ReconcilingItem, contactID: number) {
        if (recon && this.CurrentComparison) {
            this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, contactID, true, true)
                .then(
                    () => {
                        this.RefreshDataItem(recon.Item);
                    }
                );
        }
    }
    public reconcilingCodeArrayLength(len: number): any {

        return Array.from({ length: len }, (v, k) => k + 1);
    }
    public ChangePanelItem(itemid: number, index: number) {

        this.selectedRow = index;
        let tempItemList = this._ItemListService.ItemList.items;
        this.panelItem = tempItemList.find(x => x.itemId == itemid);
        this.getItemDocuments(itemid);
        if (this.DetailsView == true) this.PrepareDetailsViewData();
    }
    public ReconcilingReviewSet: ReconcilingReviewSet | null = null;

    private PrepareDetailsViewData() {
        if (this.panelItem) {
            let rrs: ReconcilingReviewSet = new ReconcilingReviewSet(this.ReviewSet
                , this.allItems[this.selectedRow].CodesReviewer1
                , this.allItems[this.selectedRow].CodesReviewer2
                , this.allItems[this.selectedRow].CodesReviewer3);
            //console.log("justaTest", rrs);
            this.ReconcilingReviewSet = rrs;
            this.SetMatchOutcomes(this.selectedRow);
        }
    }

    private SetMatchOutcomes(index: number) {
        if (this.allItems[index] && (
            this.allItems[index].OutcomesReviewer1.length > 0
            || this.allItems[index].OutcomesReviewer2.length > 0 || this.allItems[index].OutcomesReviewer3.length > 0
        )) {
            let MatchedOutcomeIds = this.ItemCodingService.MatchOutcomes(this.allItems[index].OutcomesReviewer1
                , this.allItems[index].OutcomesReviewer2
                , this.allItems[index].OutcomesReviewer3);

            this.allItems[index].SetMatchedOutcomes(MatchedOutcomeIds);

            this.allItems[index].UnMatchedOutcomesReviewer1 = this.ItemCodingService.GetUnmatchedOutcomes(
                this.allItems[index].OutcomesReviewer1, MatchedOutcomeIds, 0);

            this.allItems[index].UnMatchedOutcomesReviewer2 = this.ItemCodingService.GetUnmatchedOutcomes(
                this.allItems[index].OutcomesReviewer2, MatchedOutcomeIds, 1);

            if (this.CurrentComparison.contactName3 != '') {
                this.allItems[index].UnMatchedOutcomesReviewer3 = this.ItemCodingService.GetUnmatchedOutcomes(
                    this.allItems[index].OutcomesReviewer3, MatchedOutcomeIds, 2);
            }
            this.SetMatchedOutcomesHTML(index);
        }
    }
    private SetMatchedOutcomesHTML(index: number) {
        let res = "";
        this._MatchedOutcomesHTML = "";
        this._UnmatchedOutcomesHTML = "";
        if (!this.allItems[index]) {
            return;
        }
        let MatchedToDo: Outcome[] = [];
        for (const matched of this.allItems[index].MatchedOutcomes) {
            let ToAdd = this.allItems[index].OutcomesReviewer1.find(f => matched[0] && f.outcomeId === (matched[0] as Outcome).outcomeId);
            if (ToAdd) MatchedToDo.push(ToAdd);
            ToAdd = this.allItems[index].OutcomesReviewer2.find(f => matched[1] && f.outcomeId === (matched[1] as Outcome).outcomeId);
            if (ToAdd) MatchedToDo.push(ToAdd);
            if (this.allItems[index].OutcomesReviewer3.length > 0 && matched[2]) {
                ToAdd = this.allItems[index].OutcomesReviewer3.find(f => matched[2] && f.outcomeId === (matched[2] as Outcome).outcomeId);
                if (ToAdd) MatchedToDo.push(ToAdd);
            }
            //console.log("SetMatchedOutcomesHTML", MatchedToDo);
            res += this.ItemCodingService.GetOutcomeTableForComparison(MatchedToDo, this.CurrentComparison.contactName1, this.CurrentComparison.contactName2, this.CurrentComparison.contactName3) + "<br />";
            MatchedToDo = [];
        }
        this._MatchedOutcomesHTML = res;
        //now we do the Remaining Unmatched items...
        res = "";
        if (this.allItems[index].UnMatchedOutcomesReviewer1.length > 0) {
            res += '<div class="border-bottom border-info col-12">Unmatched Outcomes for <strong>'
                + this.CurrentComparison.contactName1 + "</strong>"
                + this.OutcomeHTMLtable(this.allItems[index].UnMatchedOutcomesReviewer1) + "</div>";
        }
        if (this.allItems[index].UnMatchedOutcomesReviewer2.length > 0) {
            res += '<div class="border-bottom border-info col-12">Unmatched Outcomes for <strong>'
                + this.CurrentComparison.contactName2 + "</strong>"
                + this.OutcomeHTMLtable(this.allItems[index].UnMatchedOutcomesReviewer2) + "</div>";
        }
        if (this.allItems[index].UnMatchedOutcomesReviewer3.length > 0) {
            res += '<div class="border-bottom border-info col-12">Unmatched Outcomes for <strong>'
                + this.CurrentComparison.contactName3 + "</strong>"
                + this.OutcomeHTMLtable(this.allItems[index].UnMatchedOutcomesReviewer3) + "</div>";
        }
        this._UnmatchedOutcomesHTML = res;
    }
    OutcomeHTMLtable(data: Outcome[]): string {
        return this.ItemCodingService.OutcomesTable(data, false);
    }

    public BuildReconcileReport() {//bool showPath, bool showInfo, bool showST, bool this.showFullTitle, bool this.showAbstract
        if (this.localList.Description == '') this.SetComparisonDescription();
        let res = this.localList.Description;
        const rx: RegExp = /<¬sep¬>/g;
        res += "<table border=1><TR><TH width='7%'><p style='margin-left:3px; margin-right:3px;'>ID</p></TH>";
        if (this.showShortTitle) {
            res += "<TH width='8%'><p style='margin-left:3px; margin-right:3px;'>Short Title</p></TH>";
        }
        if (this.showFullTitle) {
            res += "<TH><p style='margin-left:3px; margin-right:3px;'>Title</p></TH>";
        }
        if (this.showAbstract) {
            res += "<TH><p style='margin-left:3px; margin-right:3px;'>Abstract</p></TH>";
        }

        res += "<TH><p style='margin-left:3px; margin-right:3px;'>" + this.CurrentComparison.contactName1 + "</p></TH>";
        res += "<TH><p style='margin-left:3px; margin-right:3px;'>" + this.CurrentComparison.contactName2 + "</p></TH>";
        if (this.CurrentComparison.contactName3 != '') {
            res += "<TH><p style='margin-left:3px; margin-right:3px;'>" + this.CurrentComparison.contactName3 + "</p></TH>";
        }
        res += "</TR>";
        let index: number = 0;
        for (const rli of this.localList.Items) {
            res += "<TR><td><p style='margin-left:3px; margin-right:3px;'>" + rli.Item.itemId.toString() + "</p></td>";
            if (this.showShortTitle) {
                res += "<td><p style='margin-left:3px; margin-right:3px;'>" + rli.Item.shortTitle + "</p></td>";
            }
            if (this.showFullTitle) {
                res += "<td><p style='margin-left:3px; margin-right:3px;'>" + rli.Item.title + "</p></td>";
            }
            if (this.showAbstract) {
                res += "<td><p style='margin-left:3px; margin-right:3px;'>" + rli.Item.abstract + "</p></td>";
            }
            res += "<td><p style='margin-left:3px; margin-right:3px;'>";//codes for R1
            if (rli.CompletedByID == this.CurrentComparison.contactId1) {
                res += "<strong class='text-success'>*Completed*</strong><br />";
            }
            for (const rcc of rli.CodesReviewer1) {
                res += "&#8226; ";
                if (this.showFullPath && rcc.Fullpath.length > 0) {
                    res += "<span class='small'>[" + rcc.Fullpath.replace(rx, "&#92;") + "]</span>";
                }
                if (rcc.ArmID != 0) {
                    let arms = rli.Item.arms.filter(x => x.itemArmId == rcc.ArmID);
                    if (arms != null && arms.length > 0) res += rcc.Name + " <span class='small'>[Arm: " + arms[0].title + "]</span><BR />";
                    else res += rcc.Name + " <span class='small'>[Arm ID: " + rcc.ArmID + "]</span><BR />";
                }
                else res += rcc.Name + "<BR />";
                if (this.showInfoBox && rcc.InfoBox.length > 0) {
                    res += "<span class='small text-info'>[Info:] " + rcc.InfoBox + "</span><br />";
                }
            }
            res += "</p></td>";
            res += "<td><p style='margin-left:3px; margin-right:3px;'>";//codes for R2
            if (rli.CompletedByID == this.CurrentComparison.contactId2) {
                res += "<strong class='text-success'>*Completed*</strong><br />";
            }
            for (const rcc of rli.CodesReviewer2) {
                res += "&#8226; ";
                if (this.showFullPath && rcc.Fullpath.length > 0) {
                    res += "<span class='small'>[" + rcc.Fullpath.replace(rx, "&#92;") + "]</span>";
                }
                if (rcc.ArmID != 0) {
                    let arms = rli.Item.arms.filter(x => x.itemArmId == rcc.ArmID);
                    if (arms != null && arms.length > 0) res += rcc.Name + " <span style='font-family:Times, serif; font-size: 76%;'>[Arm: " + arms[0].title + "]</span><BR />";
                    else res += rcc.Name + " <span class='small'>[Arm ID: " + rcc.ArmID + "]</span><BR />";
                }
                else res += rcc.Name + "<BR />";
                if (this.showInfoBox && rcc.InfoBox.length > 0) {
                    res += "<span class='small text-info'>[Info:] " + rcc.InfoBox + "</span><br />";
                }
            }
            res += "</p></td>";
            if (this.CurrentComparison.contactName3 != '') {
                res += "<td><p style='margin-left:3px; margin-right:3px;'>";//codes for R3
                if (rli.CompletedByID == this.CurrentComparison.contactId3) {
                    res += "<strong class='text-success'>*Completed*</strong><br />";
                }
                for (const rcc of rli.CodesReviewer3) {
                    res += "&#8226; ";
                    if (this.showFullPath && rcc.Fullpath.length > 0) {
                        res += "<span class='small'>[" + rcc.Fullpath.replace(rx, "&#92;") + "]</span>";
                    }
                    if (rcc.ArmID != 0) {
                        let arms = rli.Item.arms.filter(x => x.itemArmId == rcc.ArmID);
                        if (arms != null && arms.length > 0) res += rcc.Name + " <span class='small'>[Arm: " + arms[0].title + "]</span><BR />";
                        else res += rcc.Name + " <span class='small'>[Arm ID: " + rcc.ArmID + "]</span><BR />";
                    }
                    else res += rcc.Name + "<BR />";
                    if (this.showInfoBox && rcc.InfoBox.length > 0) {
                        res += "<span  class='small text-info'>[Info:] " + rcc.InfoBox + "</span><br />";
                    }
                }
                res += "</p></td>";
            }
            res += "</TR>";
            if (this.showOutcomes != "none" && (
                this.allItems[index].OutcomesReviewer1.length > 0
                || this.allItems[index].OutcomesReviewer2.length > 0
                || this.allItems[index].OutcomesReviewer2.length > 0)) {
                let colsN: number = 3;
                if (this.showAbstract) colsN++;
                if (this.showFullTitle) colsN++;
                if (this.showShortTitle) colsN++;
                if (this.CurrentComparison.contactName3 !== '') colsN++;
                res += "<TR><td class='m-2' colspan='" + colsN.toString() + "'>";
                if (this.showOutcomes == 'matchedOutcomes') {
                    this.SetMatchOutcomes(index);
                    res += this.MatchedOutcomesHTML;
                    res += "<br />" + this._UnmatchedOutcomesHTML;
                }
                else if (this.showOutcomes == 'allOutcomes') {
                    if (this.allItems[index].OutcomesReviewer1.length > 0) {
                        res += "Outcomes Outcomes for <strong>" + this.CurrentComparison.contactName1 + "</strong><br />"
                        res += this.ItemCodingService.OutcomesTable(this.allItems[index].OutcomesReviewer1, false);
                    }
                    if (this.allItems[index].OutcomesReviewer2.length > 0) {
                        res += "Outcomes Outcomes for <strong>" + this.CurrentComparison.contactName2 + "</strong><br />"
                        res += this.ItemCodingService.OutcomesTable(this.allItems[index].OutcomesReviewer2, false);
                    }
                    if (this.allItems[index].OutcomesReviewer3.length > 0) {
                        res += "Outcomes Outcomes for <strong>" + this.CurrentComparison.contactName3 + "</strong><br />"
                        res += this.ItemCodingService.OutcomesTable(this.allItems[index].OutcomesReviewer3, false);
                    }
                }
                this._MatchedOutcomesHTML = "";
                this._UnmatchedOutcomesHTML = "";
                res += "</td></tr>";
            }
            index++;
        }
        res += "</TABLE>";
        this.ReconcileReportHTML = res;
        this._MatchedOutcomesHTML = "";
        this._UnmatchedOutcomesHTML = "";
        this.showReconcileReportHTML = true;
        this.ShowSaveReportOptions = false;
    }
    private SetComparisonDescription() {
        this.localList.Description = "";
        if (this._comparisonsService.Statistics) {
            let ComparisonDescription: string = "<H1>Reconciliation Report</H1>";
            ComparisonDescription += "<B>Code Set: " + this.ReviewSet.set_name + ".</B><br />";
            const comp = this.CurrentComparison;
            ComparisonDescription += "Comparison Created on: " + Helpers.FormatDate(comp.comparisonDate) + "; report created on: "
                + Helpers.FormatDate2(new Date().toJSON().slice(0, 10)) + ".<br />";

            ComparisonDescription += "Reviewers in this comparison: " + comp.contactName1;
            if (comp.contactId3 > 0) {
                ComparisonDescription += ", " + comp.contactName2 + " and " + comp.contactName3 + ".<br />";
            }
            else {
                ComparisonDescription += " and " + comp.contactName2 + ".<br />";
            }
            let stats: ComparisonStatistics = this._comparisonsService.Statistics;
            if (stats == null) return;//stats should now contain the results of the stats command!
            const ListType = this._ItemListService.ListCriteria.listType;
            ComparisonDescription += "<br />The Following numbers summarise the Comparison stats as they where when the comparison was created (Snapshot).<br />";


            ComparisonDescription += "Number of documents coded by <em>" + comp.contactName1 + "</em>: <strong>" + stats.RawStats.nCoded1 + "</strong>.<br / > ";
            ComparisonDescription += "Number of documents coded by <em>" + comp.contactName2 + "</em>: <strong>" + stats.RawStats.nCoded2 + "</strong>.<br / > ";

            if (comp.contactId3 > 0) {
                ComparisonDescription += "Number of documents coded by <em>" + comp.contactName3 + "</em>: <strong>" + stats.RawStats.nCoded3 + "</strong>.<br / > ";
            }

            ComparisonDescription += "Number of documents coded by both <em>" + comp.contactName1 + "</em> and <em>" + comp.contactName2
                + "</em>: <strong>" + stats.RawStats.n1vs2 + "</strong>.<br />";
            if (comp.contactId3 > 0) {
                ComparisonDescription += "Number of documents coded by both <em>" + comp.contactName2 + "</em> and <em>" + comp.contactName3
                    + "</em>: <strong>" + stats.RawStats.n2vs3 + "</strong>.<br />";
                ComparisonDescription += "Number of documents coded by both <em>" + comp.contactName1 + "</em> and <em>" + comp.contactName3
                    + "</em>: <strong>" + stats.RawStats.n1vs3 + "</strong>.<br />";
            }
            ComparisonDescription += "<br />The table below shows the coding status of items as they are now (not a snapshot), the list of items is based on the comparison snapshot.<br />";
            switch (ListType) {
                case "ComparisonDisagree1vs2":
                    ComparisonDescription += "Showing all disagreements (" + (stats.RawStats.disagreements1vs2).toString() + " of " + stats.RawStats.n1vs2.toString() + " - based on the snapshot) for: " + comp.contactName1 + " and " + comp.contactName2 + ".<br />";
                    break;
                case "ComparisonDisagree2vs3":
                    ComparisonDescription += "Showing all disagreements (" + (stats.RawStats.disagreements2vs3).toString() + " of " + stats.RawStats.n2vs3.toString() + " - based on the snapshot) for: " + comp.contactName2 + " and " + comp.contactName3 + ".<br />";
                    break;
                case "ComparisonDisagree1vs3":
                    ComparisonDescription += "Showing all disagreements (" + (stats.RawStats.disagreements1vs3).toString() + " of " + stats.RawStats.n1vs3.toString() + " - based on the snapshot) for: " + comp.contactName1 + " and " + comp.contactName3 + ".<br />";
                    break;
                case "ComparisonDisagree1vs2Sc":
                    ComparisonDescription += "Showing Include/Exclude disagreements (" + (stats.RawStats.scDisagreements1vs2).toString() + " of " + stats.RawStats.n1vs2.toString() + " - based on the snapshot) for: " + comp.contactName1 + " and " + comp.contactName2 + ".<br />";
                    break;
                case "ComparisonDisagree2vs3Sc":
                    ComparisonDescription += "Showing Include/Exclude disagreements (" + (stats.RawStats.scDisagreements2vs3).toString() + " of " + stats.RawStats.n2vs3.toString() + " - based on the snapshot) for: " + comp.contactName2 + " and " + comp.contactName3 + ".<br />";
                    break;
                case "ComparisonDisagree1vs3Sc":
                    ComparisonDescription += "Showing Include/Exclude disagreements (" + (stats.RawStats.scDisagreements1vs3).toString() + " of " + stats.RawStats.n1vs3.toString() + " - based on the snapshot) for: " + comp.contactName1 + " and " + comp.contactName3 + ".<br />";
                    break;
                default: return;

            }
            this.localList.Description = ComparisonDescription;
        }
    }
    SaveReconciliationReport() {
        if (this.ReconcileReportHTML != "" && this.showReconcileReportHTML) {
            let title = "Reconciliation Report";
            let HTML = this.ReconcileReportHTML;
            const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(HTML, this._baseUrl, title));
            saveAs(dataURI, title + ".html");
        }
    }

    BackToMain() {
        this.router.navigate(['Main']);
    }
    Clear() {
        this.CurrentComparison = new Comparison();
        this.panelItem = new Item();
        this.hideme = [];
        this._MatchedOutcomesHTML = "";
        this._UnmatchedOutcomesHTML = "";
        this.ShowSaveReportOptions = false;
    }
    ngOnDestroy() {
        console.log("Destroy reconcile Page");
        if (this.subscription) {
            this.subscription.unsubscribe();
            this.subscription = null;
        }
    }
}

