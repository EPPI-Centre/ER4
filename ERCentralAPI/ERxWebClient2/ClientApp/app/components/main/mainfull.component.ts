import { Component, Inject, OnInit, ViewChild, OnDestroy, ElementRef, Renderer, AfterViewInit, Renderer2} from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationListService } from '../services/WorkAllocationList.service'
import { Criteria, ItemList, Item } from '../services/ItemList.service'
import { WorkAllocationContactListComp } from '../WorkAllocations/WorkAllocationContactListComp.component';
import { ItemListService } from '../services/ItemList.service'
import { ItemListComp } from '../ItemList/itemListComp.component';
import { timer, Subject, Subscription } from 'rxjs'; 
import { ReviewSetsService, singleNode, SetAttribute, ItemAttributeBulkSaveCommand, ReviewSet } from '../services/ReviewSets.service';
import { CodesetStatisticsService, ReviewStatisticsCountsCommand } from '../services/codesetstatistics.service';
import { frequenciesService } from '../services/frequencies.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { crosstabService } from '../services/crosstab.service';
import { searchService } from '../services/search.service';
import { SourcesService } from '../services/sources.service';
import { SelectEvent, TabStripComponent } from '@progress/kendo-angular-layout';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { WorkAllocationComp } from '../WorkAllocations/WorkAllocationComp.component';
import { frequenciesComp } from '../Frequencies/frequencies.component';
import { CrossTabsComp } from '../CrossTabs/crosstab.component';
import { SearchComp } from '../Search/SearchComp.component';
import { ComparisonComp } from '../Comparison/createnewcomparison.component';
import { Comparison, ComparisonsService } from '../services/comparisons.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfigurableReportService } from '../services/configurablereport.service';
import { Helpers } from '../helpers/HelperMethods';
import { ExcelService } from '../services/excel.service';
import { DuplicatesService } from '../services/duplicates.service';
import { FetchReadOnlyReviewsComponent } from '../readonlyreviews/readonlyreviews.component';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { SetupConfigurableReports } from '../Reports/SetupConfigurableReports.component';
import { FreqXtabMapsComp } from '../Frequencies/FreqXtabMaps.component';
import { ClassifierService } from '../services/classifier.service';
import { ArmTimepointLinkListService } from '../services/ArmTimepointLinkList.service';
//import { AdvancedMAGFeaturesComponent } from '../MAG/AdvancedMAGFeatures.component';


@Component({
    selector: 'mainComp',
    templateUrl: './mainfull.component.html'
    ,styles: [`
                .pane-content { padding: 0em 1em; margin: 1;}
                .ReviewsBg {
                    background-color:#f1f1f8 !important; 
                }
                .vertical-text {
                    position: fixed;
                    top: 50%;
                    z-index:1002;
                    transform: rotate(90deg);
                    right: -23px;
                    float: right;
                }

        `]
     ,providers: []

})
export class MainFullReviewComponent implements OnInit, OnDestroy {

	
    constructor(private router: Router,
        public ReviewerIdentityServ: ReviewerIdentityService,
        public reviewSetsService: ReviewSetsService,
        private ItemListService: ItemListService,
		private codesetStatsServ: CodesetStatisticsService,
        private _eventEmitter: EventEmitterService,
		private frequenciesService: frequenciesService
		, private crosstabService: crosstabService
		, private _searchService: searchService
        , private SourcesService: SourcesService
        , private ConfirmationDialogService: ConfirmationDialogService
        , private ItemCodingService: ItemCodingService
		, private ReviewSetsEditingService: ReviewSetsEditingService
        , private workAllocationListService: WorkAllocationListService
        , private DuplicatesService: DuplicatesService
		, private ComparisonsService: ComparisonsService,
		private searchService: searchService,
		private configurablereportServ: ConfigurableReportService,
		@Inject('BASE_URL') private _baseUrl: string,
        private excelService: ExcelService,
        private reviewInfoService: ReviewInfoService,
        private classifierService: ClassifierService,
        private ArmTimepointLinkListService: ArmTimepointLinkListService
    ) {}
	@ViewChild('WorkAllocationContactList') workAllocationsContactComp!: WorkAllocationContactListComp;
	@ViewChild('WorkAllocationCollaborateList') workAllocationCollaborateComp!: WorkAllocationComp;
    @ViewChild('tabstrip') public tabstrip!: TabStripComponent;
    @ViewChild('ItemList') ItemListComponent!: ItemListComp;
    @ViewChild('FreqComp') FreqComponent!: frequenciesComp;
    @ViewChild('FreqXtabMapsComp') FreqXtabMapsComp!: FreqXtabMapsComp;
    @ViewChild('CrosstabsComp') CrosstabsComponent!: CrossTabsComp;
	@ViewChild('SearchComp') SearchComp!: SearchComp;
	@ViewChild('ComparisonComp') ComparisonComp!: ComparisonComp;
	@ViewChild('CodeTreeAllocate') CodeTreeAllocate!: codesetSelectorComponent;
    @ViewChild('CodingToolTreeReports') CodingToolTree!: codesetSelectorComponent;
    @ViewChild(FetchReadOnlyReviewsComponent) private ReadOnlyReviewsComponent!: FetchReadOnlyReviewsComponent;
    @ViewChild(SetupConfigurableReports) private SetupConfigurableReports!: SetupConfigurableReports; 
    //@ViewChild('AdvancedMAG') AdvancedMAG!: AdvancedMAGFeaturesComponent;

	public DropdownSelectedCodeAllocate: singleNode | null = null;
	public stats: ReviewStatisticsCountsCommand | null = null;
	public countDown: any | undefined;
	public count: number = 60;
	public isSourcesPanelVisible: boolean = false;
	public isReviewPanelCollapsed: boolean = false;
	public isWorkAllocationsPanelCollapsed: boolean = false;
	private statsSub: Subscription = new Subscription();
	private InstanceId: number = Math.random();
	public crossTabResult: any | 'none';
    public CodesAreCollapsed: boolean = true;
    public DropDownAllocateAtt: SetAttribute = new SetAttribute();
    public isCollapsedCodeAllocate: boolean = false;
    private _ShowQuickReport: boolean = false;
    public ShowClusterCommand: boolean = false;
    public HelpAndFeebackContext: string = "main\\reviewhome";
    private ListSubType: string = '';

    public ShowPrintCodeset: boolean = false;
    public printCsetShowIds: boolean = true;
    public printCsetShowDescriptions: boolean = false;
    public printCsetShowTypes: boolean = false;
    public reportsShowWhat: string = "AllFreq";

    ngOnInit() {

        this._eventEmitter.CloseReportsSectionEmitter.subscribe(
            () => {
                this.CloseReportsSection();
            }
        )
        this._eventEmitter.PleaseSelectItemsListTab.subscribe(
            () => {
                this.tabstrip.selectTab(1);
            }
        )
        this._eventEmitter.criteriaComparisonChange.subscribe(
            (item: Comparison) => {
                this.LoadComparisonList(item, this.ListSubType);
            }
        )
        this._eventEmitter.criteriaMAGChange.subscribe(
            (item: any) => {
                this.router.navigate(['Main']);
                this.GoToItemList();
                this.LoadMAGAllocList(item);
            }

        )
        this.subOpeningReview = this._eventEmitter.OpeningNewReview.subscribe(() => this.Reload());
        this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
            () => this.GetStats()
        );
        if (this.codesetStatsServ.ReviewStats.itemsIncluded == -1
            || (this.reviewSetsService.ReviewSets == undefined && this.codesetStatsServ.tmpCodesets == undefined)
            || (this.reviewSetsService.ReviewSets.length > 0 && this.codesetStatsServ.tmpCodesets.length == 0)
        ) this.Reload();

    }

    public get IsServiceBusy(): boolean {
        //console.log("mainfull IsServiceBusy", this.ItemListService, this.codesetStatsServ, this.SourcesService )
        return (this.reviewSetsService.IsBusy ||
            this.ItemListService.IsBusy ||  
            //this.codesetStatsServ.IsBusy ||
            this.frequenciesService.IsBusy ||
            this.crosstabService.IsBusy ||
            this.ReviewSetsEditingService.IsBusy ||
            this.SourcesService.IsBusy ||
            this.ComparisonsService.IsBusy);
    }
    public get ReviewSets(): ReviewSet[] {
        return this.reviewSetsService.ReviewSets;
    }
    public get HasWriteRights(): boolean {
        return this.ReviewerIdentityServ.HasWriteRights;
    }
    public OpenBasicMAG() {
        this.router.navigate(['BasicMAGFeatures']);
    }
    public OpenMAG() {
        this.router.navigate(['MAG']);
    }

    StartScreening() {
        if (this.workAllocationsContactComp) this.workAllocationsContactComp.StartScreening();
    }

    public get HasSreeningList(): boolean {
        if (this.reviewInfoService.ReviewInfo.reviewId != this.ReviewerIdentityServ.reviewerIdentity.reviewId) return false;
        else {
            if (this.reviewInfoService.ReviewInfo.showScreening
                && this.reviewInfoService.ReviewInfo.screeningCodeSetId > 0
                && this.reviewInfoService.ReviewInfo.screeningListIsGood) {
                return true;
            }
        }
        return false;
    }

	public RunExportReferences() {
		alert('not implemented yet');
	}
    public async ShowHideExportReferences(style: string): Promise<string | any[]> {

        let report: string = '';
        let jsonReport: any[] = [];
        let items: Item[] = this.ItemListService.ItemList.items.filter(found => found.isSelected == true);

        if (style == "LINKS") {
            for (var k = 0; k < items.length; k++) {
                let currentItem: Item = items[k];
                let lastItemID: number = items[items.length - 1].itemId;

                let res = await this.ArmTimepointLinkListService.GetLinksForThisItem(currentItem.itemId);
                if (k == 0) {
                    report += "<h3>Linked reference report</h3>";
                    report += "<table border='1' cellspacing='0' cellpadding='2'>";
                    report += "<tr>"
                    report += "<td><b>Master<br>EPPI ID</b></td>";
                    report += "<td><b>Master<br>Short title</b></td>";
                    report += "<td><b>Master title</b></td>";
                    report += "<td><b>Linked EPPI ID <br>& Short title</b></td>";
                    report += "<td><b>Linked Item title</b></td>";
                    report += "<td><b>Link description</b></td>";
                    report += "</tr>"
                }

                if (res == false) {
                    //console.log('res', res);
                    res = true; // so we go through the process but have an empty row
                }
                  
                report += ItemListService.GetLinks(currentItem, res, lastItemID);
                if (report.endsWith('</table>')) {
                    return report;
                }         
            }

            if (report == "") {
                report = "No linked records to show";
                return report;
            }
            else { // we should only reach this one if the last item doesn't have any links
                report += "</table><p>&nbsp;</p>";
                return report; 
            }
        }
        else { 
            for (var i = 0; i < items.length; i++) {
                let currentItem: Item = items[i];
                let lastItemID: number = items[items.length - 1].itemId;

                switch (style) {
                    case "Chicago":
                        report += "<p>" + ItemListService.GetCitation(currentItem) + "</p>";
                        break;
                    case "Harvard":
                        report += "<p>" + ItemListService.GetHarvardCitation(currentItem) + "</p>";
                        break;
                    case "NICE":
                        report += "<p>" + ItemListService.GetNICECitation(currentItem) + "</p>";
                        break;
                    case "ExportTable":
                        jsonReport.push(this.ItemListService.GetCitationForExport(currentItem));
                        break;
                    case "HIS":
                        jsonReport.push(ItemListService.GetHISCitationForExport(currentItem));
                        break;
                }
            }

            if (report == '') {
                return jsonReport;
            } else {
                return report;
            }
        }

    }


	public ExportReferences(report: string) {
		
		const dataURI = "data:text/plain;base64," +
			encodeBase64(report);

		console.log('EXPORT REFERENCES FUNCTION, report: ', dataURI);
	}
	exportAsXLSX(report: string[]): void {
		this.excelService.exportAsExcelFile(report, 'ItemsList');
    }
    exportAsHisXLSX(report: string[]): void {
        this.excelService.exportHISscreeningFile(report, 'ItemsList (RevId ' + this.ReviewerIdentityServ.reviewerIdentity.reviewId.toString()+ ')') ;
    }
    public ItemsWithThisCodeDDData: Array<any> = [
        {
            text: 'With this Code (Excluded)',
            click: () => {
                this.ListItemsWithWithoutThisCode(false, true);
            }
        },
        {
            text: 'Without this Code ',
            click: () => {
                this.ListItemsWithWithoutThisCode(true, false);
            }
        },
        {
            text: 'Without this Code (Excluded)',
            click: () => {
                this.ListItemsWithWithoutThisCode(false, false);
            }
        }
    ];
    public AddRemoveItemsDDData: Array<any> = [{
        text: 'Remove code from selection',
        click: () => {
            this.BulkAssignRemoveCodes(false);
        }
	}];
	public AddRemoveSearchesDDData: Array<any> = [{
		text: 'Remove search(es) from code',
		click: () => {
			this.BulkAssignRemoveCodesToSearches(false);
		}
	}];
    public QuickReportsDDData: Array<any> = [{
        text: 'Quick Question Report',
        click: () => {
			this.ShowHideQuickQuestionReport();
        }
	}];
	public ExportReferencesDDData: Array<any> = [
		{
			text: 'Harvard',
			click: async () => {
				Helpers.OpenInNewWindow(await this.ShowHideExportReferences('Harvard'), this._baseUrl);
			}
		},
		{
			text: 'Chicago',
			click: async () => {
				Helpers.OpenInNewWindow(await this.ShowHideExportReferences('Chicago'), this._baseUrl);
			}
		},
		{
			text: 'NICE Format',
            click: async () => {
				Helpers.OpenInNewWindow(await this.ShowHideExportReferences('NICE'), this._baseUrl);
			}
		},
		{
			text: 'Excel',
			click: () => {
				//this.ExportReferencesAsHTML(this.ShowHideExportReferences('ExportTable'));
				let testRefs: any = this.ShowHideExportReferences('ExportTable');
				//console.log(testRefs);
				this.exportAsXLSX(testRefs);
			}
        },
        {
            text: 'HTML',
            click: () => {
                let tmp = document.getElementById('ItemsTable');
                if (tmp) {
                    //let report = Helpers.AddHTMLFrame(tmp.innerHTML, this._baseUrl);
                    let removals = 
                        [
                            {
                                searchFor: '<th></th><th class="pl-0 pr-0"><input class="m-1 ng-valid ng-dirty ng-touched" name="selectAll" style="zoom: 1.2;" type="checkbox" ng-reflect-name="selectAll" ng-reflect-model="true"></th>',
                                changeTo: ""
                            },
                            {
                                searchFor: '<td class="p-1 pt-2"><button class="btn btn-outline-primary btn-sm m-0">GO</button></td><td class="pl-0 pr-0 "><input class="m-1 ng-untouched ng-pristine ng-valid" style="zoom: 1.2;" type="checkbox" ng-reflect-model="true"></td>',
                                changeTo: ""
                            }
                        ]
                    let report = Helpers.CleanHTMLforExport(tmp.outerHTML, removals);
                    const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(report, this._baseUrl, "Items Table"));
                    saveAs(dataURI, "Items Table.html");
                }
                //let t2 = this.ItemListComponent.exportItemsTable;
                //if (t2) {
                //    console.log(t2.nativeElement.outerHTML);
                //    const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(t2.nativeElement.outerHTML, this._baseUrl, "Items Table"));
                //    saveAs(dataURI, "ItemsTable.html");
                //}
            }
        },
        {
            text: 'HIS (ext. scr.)',
            click: async () => {
                //this.ExportReferencesAsHTML(this.ShowHideExportReferences('ExportTable'));
                let testRefs: any = await this.ShowHideExportReferences('HIS');
                //console.log(testRefs);
                this.exportAsHisXLSX(testRefs);
            }
        },
        {
            text: 'Linked report',
            click: async () => {
                let linkReport: any = await this.ShowHideExportReferences('LINKS');
                const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(linkReport, this._baseUrl, "Links Table"));
                saveAs(dataURI, "Links table.html");

                // for displaying in a new tab rather than a file
                //Helpers.OpenInNewWindow(await this.ShowHideExportReferences('LINKS'), this._baseUrl);
            }
        }
    ];
    public ImportOrNewDDData: Array<any> = [{
        text: 'New Reference',
        click: () => {
            this.NewReference();
        }
    }];

    public get ShowQuickReport(): boolean {
        
        return this._ShowQuickReport;
    }
    private _ShowQuickQuestionReport: boolean = false;
    public get ShowQuickQuestionReport(): boolean {
        if (this._ShowQuickQuestionReport && !this.ItemListService.HasSelectedItems) {
            this._ShowQuickQuestionReport = false;
            this.ItemCodingService.Clear();
            this.reviewSetsService.clearItemData();
        }
        return this._ShowQuickQuestionReport;
    }
    public get HasSelectedItems(): boolean {
        return this.ItemListService.HasSelectedItems;
    }
    public get selectedNode(): singleNode | null {
        return this.reviewSetsService.selectedNode;
    }
    public get CanGetItemsWithThisCode(): boolean {
        if (this.selectedNode && this.selectedNode.nodeType == "SetAttribute") return true;
        else return false;
    }
    public get CanBulkAssignRemoveCodes(): boolean {
        if (
            this.selectedNode
            && this.selectedNode.nodeType == "SetAttribute"
            && this.ReviewerIdentityServ.HasWriteRights
            && this.ItemListService.ItemList
            && this.ItemListService.ItemList.items
            && this.ItemListService.ItemList.items.length > 0
            && this.ItemListService.HasSelectedItems
        ) return true;
        else return false;
	}
	public get CanBulkAssignRemoveCodesToSearches(): boolean {
		if (
			this.selectedNode
			&& this.selectedNode.nodeType == "SetAttribute" &&
			this.ReviewerIdentityServ.HasWriteRights
			&& this.searchService.SearchList
			&& this.searchService.SearchList.findIndex(x => x.add == true) != -1
			//&& this.searchService.SearchList.length > 0
		) return true;
		else return false;
    }
    public get ReviewIsMagEnabled(): boolean {
        if (this.reviewInfoService.ReviewInfo.magEnabled
            //&& this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin
        ) return true;
        return false;
    }

    public get IsSiteAdmin(): boolean {
        //console.log("Is it?", this.ReviewerIdentityServ.reviewerIdentity
        //    , this.ReviewerIdentityServ.reviewerIdentity.userId > 0
        //    , this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated
        //    , this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin);
        if (this.ReviewerIdentityServ
            && this.ReviewerIdentityServ.reviewerIdentity
            && this.ReviewerIdentityServ.reviewerIdentity.userId > 0
            && this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated
            && this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) return true;
        else return false;
    }
    public get HasAdminRights(): boolean {
        return this.ReviewerIdentityServ.HasAdminRights;
    }
	public CanAssignDocs(): boolean {
		if (this.AssignDocs != null && this.DropdownSelectedCodeAllocate != null
				&& this.AllocateChoice == 'Documents with this code') {
			return true;
		}
		if (this.AssignDocs != null && this.ItemListService.SelectedItems.length > 0
				&& this.AllocateChoice == 'Selected documents') {
			return true;
		}
		return false;
    }
    public RunAssignment() {

        let itemIdsStr: string = "";
        if (this.AllocateChoice !== 'Documents with this code') {
            var itemids = this.ItemListService.SelectedItems.map(
                x => x.itemId
            );
            itemIdsStr = itemids.toString();
            console.log("itemIdsStr", itemIdsStr);
        }

        this.ItemListService.AssignDocumentsToIncOrExc(
            this.AssignDocs,
            itemIdsStr,
            this.AllocateChoice == 'Documents with this code' ? this.DropDownAllocateAtt.attribute_id : 0,
            this.AllocateChoice == 'Documents with this code' ? this.DropDownAllocateAtt.set_id : 0
        ).then(

            () => {

                this.ItemListService.Refresh();
                this.AllIncOrExcShow = false;
                this.DropdownSelectedCodeAllocate = null;
                this.AssignDocs = 'true';
                this.AllocateChoice = 'Selected documents';
            }
        );
    }
    public CloseCodeDropDownAllocate() {

        if (this.CodeTreeAllocate) {

            this.DropdownSelectedCodeAllocate = this.CodeTreeAllocate.SelectedNodeData;
            this.DropDownAllocateAtt = this.DropdownSelectedCodeAllocate as SetAttribute;

        }
        this.isCollapsedCodeAllocate = false;

    }
	public DeleteRelevantItems() {
		if (this.ItemListService.SelectedItems != null &&
			this.ItemListService.SelectedItems.length > 0) {
		this.AllIncOrExcShow = false;
		this.ConfirmationDialogService.confirm("Delete the selected items?",
			"Are you sure you want to delete these " + this.ItemListService.SelectedItems.length  + " item/s?", false, '')
			.then((confirm: any) => {
					if (confirm) {
						this.ItemListService.DeleteSelectedItems(this.ItemListService.SelectedItems);
				}
				});
		}
	}
	public AllocateRelevantItems() {

		if (!this.AllIncOrExcShow) {

			this.AllIncOrExcShow = true;
			this.RunReportsShow = false;
			this.ShowClusterCommand = false;

		} else {

			this.AllIncOrExcShow = false;
		}
	}
	public AllocateChoice: string = '';
    public AllIncOrExcShow: boolean = false;
    public RunReportsShow: boolean = false;
    public RunReportsShow2: boolean = false;
	public AssignDocs: string = 'true';

	public CloseReportsSection() {
        this.RunReportsShow2 = false;
		this.RunReportsShow = false;
	}

    public SetupWebDBs() {
        this.router.navigate(['WebDBs']);
    }

	public GetReports(force: boolean = false) {
        if (force || this.configurablereportServ.Reports.length == 0)
            this.configurablereportServ.FetchReports();
	}
	public RunConfigurableReports() {

		if (!this.RunReportsShow) {
			this.RunReportsShow = true;
			this.AllIncOrExcShow = false;
			this.ShowClusterCommand = false;
			this.GetReports();

		} else {

			this.RunReportsShow = false;
		}
    }
    public RunConfigurableReports2() {
        this.RunReportsShow2 = !this.RunReportsShow2;
    }
    public get CanGetFrequencies(): boolean {
        if (!this.FreqXtabMapsComp || this.selectedNode == null) return false;
        else {
            return this.FreqXtabMapsComp.canSetCode();
        }
    }
    public GetFrequencies() {
        if (!this.FreqXtabMapsComp || this.selectedNode == null) return;
        else {
            this.FreqXtabMapsComp.Clear();
            this.FreqXtabMapsComp.selectedNodeDataY = this.selectedNode;
            this.reportsShowWhat = 'AllFreq';
            this.FreqXtabMapsComp.fetchFrequencies(this.selectedNode, null);
            this.tabstrip.selectTab(2);
        }
    }

	public CloseSection() {
		this.AllIncOrExcShow = false;
	}
	public LoadComparisonList(comparison: Comparison, ListSubType: string) {

		let crit = new Criteria();
		crit.listType = ListSubType;
		let typeMsg: string = '';
		if (ListSubType.indexOf('Disagree') != -1) {
			typeMsg = 'disagreements between';
		} else {
			typeMsg = 'agreements between';
		}
		let middleDescr: string = ' ' + comparison.contactName3 != '' ? ' and ' + comparison.contactName3 : '';
		let listDescription: string = typeMsg + '  ' + comparison.contactName1 + ' and ' + comparison.contactName2 + middleDescr + ' using ' + comparison.setName;
		crit.description = listDescription;
		crit.listType = ListSubType;
		crit.comparisonId = comparison.comparisonId;
		console.log('checking: ' + JSON.stringify(crit) + '\n' + ListSubType);
		this.ItemListService.FetchWithCrit(crit, listDescription);

	}
    ShowHideCodes() {
        this.CodesAreCollapsed = !this.CodesAreCollapsed;
    }
    OpenCodesPanel() {
        this.CodesAreCollapsed = false;
    }
    ShowHideQuickReport() {
        this._ShowQuickQuestionReport = false;
        this._ShowQuickReport = !this._ShowQuickReport;
        //console.log("ShowHideQuick Rep:", this._ShowQuickReport, this.ItemListService.HasSelectedItems);
    }
    ShowHideQuickQuestionReport() {
        this._ShowQuickReport = false;//_ShowQuickQuestionReport
        if (!this.ItemListService.HasSelectedItems) this._ShowQuickQuestionReport = false;
        else this._ShowQuickQuestionReport = !this._ShowQuickQuestionReport;
        //console.log("ShowHideQuickQ Rep:", this._ShowQuickQuestionReport, this.ItemListService.HasSelectedItems);
    }
    CloseQuickReport() {
        this._ShowQuickReport = false;
        this._ShowQuickQuestionReport = false;
    }
	ShowHideClusterCommand() {

		if (!this.ShowClusterCommand) {
			this.ShowClusterCommand = true;
			this.AllIncOrExcShow = false;
			this.RunReportsShow = false;

		} else {

			this.ShowClusterCommand = false;
		}
    }
    CloseClusterCommand() {
        this.ShowClusterCommand = false;
    }
    setTabSelected(tabSelect: SelectEvent) {
		//nothing for now, selectEvent is like this:
        //index: number
        //title: string
	}
	BuildModel() {
		this.router.navigate(['BuildModel']);
    }
    NewReference() {
        this.router.navigate(['EditItem'], { queryParams: { return: 'Main' } });
    }
    ListItemsWithWithoutThisCode(Included: boolean, withThisCode: boolean) {
        if (!this.selectedNode || this.selectedNode.nodeType != "SetAttribute") return;
        let CurrentAtt = this.selectedNode as SetAttribute;
        if (!CurrentAtt) return;
        let cr: Criteria = new Criteria();
        cr.onlyIncluded = Included;
        cr.showDeleted = false;
        cr.pageNumber = 0;
        cr.showInfoColumn = true;
        let ListDescription: string = "";
        if (withThisCode) {
            //with this code
            if (Included) ListDescription = CurrentAtt.attribute_name + ".";
            else ListDescription = CurrentAtt.attribute_name + " (excluded).";
            cr.listType = "StandardItemList";
            cr.attributeid = CurrentAtt.attribute_id;
            cr.showInfoColumn = true;
        } else {
            //without this code
            if (Included) ListDescription = "Without code: " + CurrentAtt.attribute_name + ".";
            else ListDescription = "Without code: " + CurrentAtt.attribute_name + " (excluded).";
            cr.listType = "ItemListWithoutAttributes";
            cr.attributeid = 0;
            cr.attributeSetIdList = CurrentAtt.attributeSetId.toString();
            cr.showInfoColumn = false;
        }
        cr.sourceId = 0;
        cr.attributeSetIdList = CurrentAtt.attributeSetId.toString();
        this.ItemListService.FetchWithCrit(cr, ListDescription);
        this.tabstrip.selectTab(1);
        //this._eventEmitter.PleaseSelectItemsListTab.emit();
    }
    
	BulkAssignRemoveCodesToSearches(IsBulkAssign: boolean) {

		//alert(this.searchService.SearchList.filter(x => x.add == true).length);
		//alert(JSON.stringify(this.searchService.SearchList));

		if (!this.reviewSetsService.selectedNode || this.reviewSetsService.selectedNode.nodeType != "SetAttribute") return;
		else {
			const SetA = this.reviewSetsService.selectedNode as SetAttribute;
			if (!SetA) return;
			else {
				if (IsBulkAssign
					&& this.reviewSetsService.selectedNode) {
					this.ConfirmationDialogService.confirm("Assign the selected ("
						+ this.searchService.SearchList.filter(x => x.add == true).length + ") searches ? ", "Are you sure you want to assign all selected searches to this ("
						+ this.reviewSetsService.selectedNode.name + ") code?", false, '')
						.then((confirm: any) => {
							if (confirm) {
								this.BulkAssignCodesToSearches(SetA.attribute_id, SetA.set_id);
							}
						});
				}
				else if (!IsBulkAssign
					&& this.reviewSetsService.selectedNode) {
					this.ConfirmationDialogService.confirm("Remove the selected ("
						+ this.searchService.SearchList.filter(x => x.add == true).length + ") searches?", "Are you sure you want to remove all selected searches from this ("
						+ this.reviewSetsService.selectedNode.name + ") code?", false, '')
						.then((confirm: any) => {
							if (confirm) {
								this.BulkDeleteCodesToSearches(SetA.attribute_id, SetA.set_id);
							}
						});
				}
			}
		}

	}
    BulkAssignCodesToSearches(attribute_id: number, set_id: number): any {
		let ItemIds: string = "";
		let cmd: ItemAttributeBulkSaveCommand = new ItemAttributeBulkSaveCommand();
		cmd.itemIds = ItemIds;
		cmd.attributeId = attribute_id;
		cmd.setId = set_id;
		cmd.saveType = "Insert";
		const searches = this.searchService.SearchList.filter(x => x.add == true);
		let SearchIds: string = "";
		for (let item of searches) {
			SearchIds += item.searchId.toString() + ",";
		}
		SearchIds = SearchIds.substring(0, SearchIds.length - 1);
		cmd.searchIds = SearchIds;
		this.reviewSetsService.ExecuteItemAttributeBulkInsertCommand(cmd);
    }
    BulkDeleteCodesToSearches(attribute_id: number, set_id: number): any {
		let ItemIds: string = "";
		let cmd: ItemAttributeBulkSaveCommand = new ItemAttributeBulkSaveCommand();
		cmd.itemIds = ItemIds;
		cmd.attributeId = attribute_id;
		cmd.setId = set_id;
		const searches = this.searchService.SearchList.filter(x => x.add == true);
		let SearchIds: string = "";
		for (let item of searches) {
			SearchIds += item.searchId.toString() + ",";
		}
		SearchIds = SearchIds.substring(0, SearchIds.length - 1);
		cmd.searchIds = SearchIds;
		this.reviewSetsService.ExecuteItemAttributeBulkDeleteCommand(cmd);
    }
    BulkAssignRemoveCodes(IsBulkAssign: boolean) {
        if (!this.reviewSetsService.selectedNode || this.reviewSetsService.selectedNode.nodeType != "SetAttribute") return;
        else {
            const SetA = this.reviewSetsService.selectedNode as SetAttribute;
            if (!SetA) return;
            else if (this.reviewSetsService.selectedNode) {
                let encoded = Helpers.htmlEncode(this.reviewSetsService.selectedNode.name);
                //return doc.documentElement.textContent;
                if (IsBulkAssign) {
                    this.ConfirmationDialogService.confirm("Assign selected ("
                        + this.ItemListService.SelectedItems.length + ") items ? "
                        , "Are you sure you want to assign all selected items (<strong>"
                        + this.ItemListService.SelectedItems.length + "</strong>) to this code?<br>"
                        + "<div class='w-100 p-0 mx-0 my-2 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
                        + encoded + "</strong></div>"
                        , false, '')
                        .then((confirm: any) => {
                            if (confirm) {
                                this.BulkAssingCodes(SetA.attribute_id, SetA.set_id);
                            }
                        });
                }
                else if (!IsBulkAssign) {
                    this.ConfirmationDialogService.confirm("Remove selected ("
                        + this.ItemListService.SelectedItems.length + ") items?"
                        , "Are you sure you want to remove all selected items (<strong>"
                        + this.ItemListService.SelectedItems.length + "</strong>) from this code?<br>"
                        + "<div class='w-100 p-0 mx-0 my-2 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
                        + encoded + "</strong></div>"
                        , false, '')
                        .then((confirm: any) => {
                            if (confirm) {
                                this.BulkDeleteCodes(SetA.attribute_id, SetA.set_id);
                            }
                        });
                }
            }
        }

    }
    private BulkAssingCodes(attributeId:number, setId:number) {
        const items = this.ItemListService.SelectedItems;
        let ItemIds: string = "";
        for (let item of items) {
            ItemIds += item.itemId.toString() + ",";
        }
        ItemIds = ItemIds.substring(0, ItemIds.length - 1);
        let cmd: ItemAttributeBulkSaveCommand = new ItemAttributeBulkSaveCommand();
        cmd.itemIds = ItemIds;
        cmd.attributeId = attributeId;
        cmd.setId = setId;
        this.reviewSetsService.ExecuteItemAttributeBulkInsertCommand(cmd);
    }
    private BulkDeleteCodes(attributeId: number, setId: number) {
        const items = this.ItemListService.SelectedItems;
        let ItemIds: string = "";
        for (let item of items) {
            ItemIds += item.itemId.toString() + ",";
        }
        ItemIds = ItemIds.substring(0, ItemIds.length - 1);
        let cmd: ItemAttributeBulkSaveCommand = new ItemAttributeBulkSaveCommand();
        cmd.itemIds = ItemIds;
        cmd.attributeId = attributeId;
        cmd.setId = setId;
        this.reviewSetsService.ExecuteItemAttributeBulkDeleteCommand(cmd);
    }
    public RefreshCodingTools() {
        this.reviewSetsService.GetReviewSets(false);
    }
    public PrintCodingTool() {
        if (this.selectedNode == null || this.selectedNode.nodeType != 'ReviewSet') return;
        let report: string = "";
        let reviewSet = this.selectedNode as ReviewSet;
        report += "<h2>" + reviewSet.set_name;
        if (this.printCsetShowIds) report += " (ID: " + reviewSet.set_id + ")";
        if (this.printCsetShowTypes) report += " [" + reviewSet.setType.setTypeName + "]";
        report += "</h2>";

        if (this.printCsetShowDescriptions && reviewSet.description.trim().length > 0) {
            let desc: string = reviewSet.description;
            desc = desc.replace("\r\n", "<br>");
            desc = desc.replace("\n", "<br>");
            desc = desc.replace("\r", "<br>");
            report += "<i>" + desc + " </i>";
        }
        report += "<p><ul>";
        for(let attributeSet of reviewSet.attributes)
        {
            report = this.PrintSelectedReviewSetAddAttributes(report, attributeSet, this.printCsetShowIds, this.printCsetShowDescriptions, this.printCsetShowTypes);
        }
        report += "</ul></p>";
        const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(report, this._baseUrl, "Coding Tool Printout"));
        //console.log("Savign report:", dataURI)
        saveAs(dataURI, "Coding Tool printout.html");
    }
    PrintSelectedReviewSetAddAttributes(report: string, attributeSet: SetAttribute, showIDs: boolean, showDescriptions: boolean, ShowTypes: boolean): string {
        let desc:string = attributeSet.description;
        desc = desc.replace("\r\n", "<br>");
        desc = desc.replace("\n", "<br>");
        desc = desc.replace("\r", "<br>");
        report += "<li>" + attributeSet.attribute_name;
        if (showIDs) report += " (ID = " + attributeSet.attribute_id + ")";
        if (ShowTypes) report += " [" + attributeSet.attribute_type + "]";
        if (showDescriptions && desc.trim().length > 0) report += "<br><i>" + desc + " </i>";

        if (attributeSet.attributes != null && attributeSet.attributes.length > 0) {
            report += "<ul>";
            for(let child of attributeSet.attributes)
            {
                report = this.PrintSelectedReviewSetAddAttributes(report, child, showIDs, showDescriptions, ShowTypes);
            }
            report += "</ul>";
        }
        report += "</li>";
        return report;
    }

    public get ReviewPanelTogglingSymbol(): string {
        if (this.isReviewPanelCollapsed) return '&uarr;';
        else return '&darr;';
	}

    public get WorkAllocationsPanelTogglingSymbol(): string {
        if (this.isWorkAllocationsPanelCollapsed) return '&uarr;';
        else return '&darr;';
	}
    public get SourcesPanelTogglingSymbol(): string {
        if (this.isSourcesPanelVisible) return '&uarr;';
        else return '&darr;';
    }
	IncludedItemsList() {
        this.IncludedItemsListNoTabChange();
		this.tabstrip.selectTab(1);
    }
    IncludedItemsListNoTabChange() {
        this.ItemListService.GetIncludedItems();
    }
    ExcludedItemsList() {
        this.ItemListService.GetExcludedItems();
        console.log('selecting tab 2...');
        this.tabstrip.selectTab(1);
    }
    DeletedItemList() {
        this.ItemListService.GetDeletedItems();
        this.tabstrip.selectTab(1);
    }
	GoToItemList() {
		this.tabstrip.selectTab(1);
	}
    LoadContactWorkAllocList(workAlloc: WorkAllocation) {
		if (this.ItemListComponent) this.ItemListComponent.LoadWorkAllocList(workAlloc, this.workAllocationsContactComp.ListSubType);
        else console.log('attempt failed');
    }
    LoadWorkAllocList(workAlloc: WorkAllocation){
		if (this.ItemListComponent) this.ItemListComponent.LoadWorkAllocList(workAlloc, this.workAllocationCollaborateComp.ListSubType);
		else console.log('attempt failed');
    }
    LoadMAGAllocList(ListSubType: string) {
        if (this.ItemListComponent) this.ItemListComponent.LoadMAGAllocList(ListSubType);
        else console.log('attempt failed');
    }

	//ngOnChanges() {
		//if (this.tabsInitialized) {
		//	console.log('tabs experiment');

		//	this.tabstrip.selectTab(1);
		//}
	//}
    toggleReviewPanel() {
        this.isReviewPanelCollapsed = !this.isReviewPanelCollapsed;
        if (this.isReviewPanelCollapsed && this.ReadOnlyReviewsComponent) this.ReadOnlyReviewsComponent.getReviews();
    }
    toggleWorkAllocationsPanel() {
		this.isWorkAllocationsPanelCollapsed = !this.isWorkAllocationsPanelCollapsed;
		//this.workAllocationListService.FetchAll();
        if (this.workAllocationsContactComp && this.isWorkAllocationsPanelCollapsed) this.workAllocationsContactComp.getWorkAllocationContactList();

	}
    toggleSourcesPanel() {
        if (!this.isSourcesPanelVisible) {
            this.SourcesService.FetchSources();
        }
        this.isSourcesPanelVisible = !this.isSourcesPanelVisible;
    }
    getDaysLeftAccount() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftAccount;
    }
    getDaysLeftReview() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftReview;
    }
    onLogin(u: string, p:string) {

        this.ReviewerIdentityServ.LoginReq(u, p);
        
    };
    subOpeningReview: Subscription | null = null;
	ShowItemsTable: boolean = false;
	ShowSearchesAssign: boolean = false;
    onTabSelect(e: SelectEvent) {

        if (e.title == 'Review home') {
            this.HelpAndFeebackContext = "main\\reviewhome";
			this.ShowItemsTable = false;
			this.ShowSearchesAssign = false;
        }
        else if (e.title == 'References') {
            this.HelpAndFeebackContext = "main\\references";
			this.ShowItemsTable = true;
			this.ShowSearchesAssign = false;
        }
        else if (e.title == 'Reports') {
            this.HelpAndFeebackContext = "main\\reports";
            this.ShowItemsTable = false;
            this.ShowSearchesAssign = false;
        }
        //else if (e.title == 'Frequencies') {
        //    this.HelpAndFeebackContext = "main\\frequencies";
        //    this.ShowItemsTable = false;
        //    this.ShowSearchesAssign = false;
        //}
        else if (e.title == 'Crosstabs') {
            this.HelpAndFeebackContext = "main\\crosstabs";
			this.ShowItemsTable = false;
			this.ShowSearchesAssign = false;
        }
        else if (e.title == 'Search & Classify') {
            this.HelpAndFeebackContext = "main\\search";
			this.ShowItemsTable = false;
			this.ShowSearchesAssign = true;
            this._searchService.Fetch();
		}
  //      else if (e.title == 'Microsoft Academic Graph') {
		//	this.HelpAndFeebackContext = "main\\microsoft";
		//	this.ShowItemsTable = false;
		//	//this.ShowSearchesAssign = true;
  //          this._basicMAGService.FetchMagRelatedPapersRunList();
		//}
		else if (e.title == 'Collaborate') {
			this.HelpAndFeebackContext = "main\\collaborate";
			if (this.workAllocationCollaborateComp) this.workAllocationCollaborateComp.RefreshData();
			this.ShowItemsTable = false;
			this.ShowSearchesAssign = false;
		}
        else {
            this.HelpAndFeebackContext = "main\\reviewhome";
			this.ShowItemsTable = false;
			this.ShowSearchesAssign = false;
        }
    }
    //NewReference() {
    //    this.router.navigate(['EditItem'], { queryParams: { return: 'Main' } } );
    //}

    Reload() {
        console.log('Reload mainfull');
        this.Clear();
        if (this.ReviewerIdentityServ.IsCodingOnly) return;
        this.reviewSetsService.GetReviewSets();
        this.isSourcesPanelVisible = false;
		if (this.workAllocationsContactComp) this.workAllocationsContactComp.getWorkAllocationContactList();
        //else console.log("work allocs comp is undef :-(");
        if (this.ItemListService.ListCriteria && this.ItemListService.ListCriteria.listType == "") 
            this.IncludedItemsListNoTabChange();
        setTimeout(() => {this.GetReports(true);}, 1000);//always get reports list, but we can wait 1s before doing so...
        
    }
    //GetStatsFromSubscription() {
    //    //we unsubscribe here as we won't use this again.
    //    if (this.statsSub) {
    //        console.log("(mainfull GetStatsFromSubscription) I should not happen more than once");
    //        this.statsSub.unsubscribe();
    //        this.statsSub = new Subscription();
    //    }
    //    this.GetStats();
    //}
    GetStats(forceAllDetails: boolean = false) {
        console.log('getting stats (mainfull):', this.InstanceId);
        this.codesetStatsServ.GetReviewStatisticsCountsCommand(true, forceAllDetails);
        //this.codesetStatsServ.GetReviewSetsCodingCounts(true, this.dtTrigger);
    }
    Clear() {
        console.log('Clear in mainfull');
        this.ItemListService.Clear();
        //this.codesetStatsServ.
        this.reviewSetsService.Clear();
        this.classifierService.Clear();
        this.codesetStatsServ.Clear();
		this.SourcesService.Clear();
		this.workAllocationListService.Clear();
        this.DuplicatesService.Clear();
        this.configurablereportServ.Clear(); FreqXtabMapsComp
        if (this.FreqComponent) this.FreqComponent.Clear();
        if (this.FreqXtabMapsComp) this.FreqXtabMapsComp.Clear();
		if (this.CrosstabsComponent) this.CrosstabsComponent.Clear();
		if (this.workAllocationCollaborateComp) {
			//console.log('this comp exists');
			this.workAllocationCollaborateComp.Clear();
		}
		if (this.SearchComp) {
			this.SearchComp.Clear();
		}
		if (this.ComparisonComp) {
			this.ComparisonComp.Clear();
        }
        if (this.ReadOnlyReviewsComponent) this.ReadOnlyReviewsComponent.Clear();
        if (this.SetupConfigurableReports) this.SetupConfigurableReports.Clear();
        this.isReviewPanelCollapsed = false;
        this.isWorkAllocationsPanelCollapsed = false;
        this.isSourcesPanelVisible = false;
        this.AllIncOrExcShow = false;
        this.RunReportsShow = false;
        this._ShowQuickReport = false;
        this._ShowQuickQuestionReport = false;
        this.ShowClusterCommand = false;
        this._ShowQuickQuestionReport = false;
        this._ShowQuickQuestionReport = false;
        this._ShowQuickQuestionReport = false;
        this.reportsShowWhat = "AllFreq";
        this.RunReportsShow2 = false;
        //this.dtTrigger.unsubscribe();
        //if (this.statsSub) this.statsSub.unsubscribe();
        //this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
        //    () => this.GetStats()
        //);
    }

    FormatDate(DateSt: string): string {
        return Helpers.FormatDate2(DateSt);
    }
  
    public get MyAccountMessage(): string {
        let msg: string = "Your account expires on: ";
        let AccExp: string = this.FormatDate(this.ReviewerIdentityServ.reviewerIdentity.accountExpiration);
        msg += AccExp;
        return msg;
    }
    public get MyReviewMessage(): string {
        let revPart: string = "";
        if (this.ReviewerIdentityServ.getDaysLeftReview() == -999999) {//review is private
            revPart = "Current review is private (does not expire).";
        }
        else {
            let RevExp: string = this.FormatDate(this.ReviewerIdentityServ.reviewerIdentity.reviewExpiration);
            revPart = "Current(shared) review expires on: " + RevExp + ".";
        }
        return revPart;
    }
    EditCodeSets() {
        this.router.navigate(['EditCodeSets']);
    }
    GoToSources() {
        this.router.navigate(['sources']);
    }
    ImportCodesetClick() {
        this.router.navigate(['ImportCodesets']);
    }
    ToRis() {
        if (!this.HasSelectedItems) return;
        const dataURI = "data:text/plain;base64," + encodeBase64(this.ItemListService.SelectedItemsToRIStext());
        //console.log("ToRis", dataURI)
        saveAs(dataURI, "ExportedRis.txt");
	}
    
    ngOnDestroy() {
        this.Clear();
        console.log("destroy MainFull..");
        if (this.subOpeningReview) {
            this.subOpeningReview.unsubscribe();			
        }
        if (this.statsSub) this.statsSub.unsubscribe();
        //if (this._routingStateService.MAGSubscription) this._routingStateService.UnsubscribeMAGHistory();
    }
}

export class RadioButtonComp {
	IncEnc = true;
}
export interface iItemLink {
    itemLinkId: number;
    itemIdPrimary: number;
    itemIdSecondary: number;
    title: string;
    shortTitle: string;
    description: string;
}


