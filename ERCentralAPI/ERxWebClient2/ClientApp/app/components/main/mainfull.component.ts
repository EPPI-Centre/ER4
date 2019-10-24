import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy, EventEmitter, Output, Input } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationListService } from '../services/WorkAllocationList.service'
import { Criteria, ItemList } from '../services/ItemList.service'
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
import { ConfigurableReportService, Report, ReportAnswerExecuteCommandParams, ReportQuestionExecuteCommandParams } from '../services/configurablereport.service';


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
		, private ComparisonsService: ComparisonsService,
		private searchService: searchService,
		private configurablereportServ: ConfigurableReportService,
		@Inject('BASE_URL') private _baseUrl: string
    ) {}
	@ViewChild('WorkAllocationContactList') workAllocationsContactComp!: WorkAllocationContactListComp;
	@ViewChild('WorkAllocationCollaborateList') workAllocationCollaborateComp!: WorkAllocationComp;
    @ViewChild('tabstrip') public tabstrip!: TabStripComponent;
    //@ViewChild('tabset') tabset!: NgbTabset;
    @ViewChild('ItemList') ItemListComponent!: ItemListComp;
    @ViewChild('FreqComp') FreqComponent!: frequenciesComp;
    @ViewChild('CrosstabsComp') CrosstabsComponent!: CrossTabsComp;
	@ViewChild('SearchComp') SearchComp!: SearchComp;
	@ViewChild('ComparisonComp') ComparisonComp!: ComparisonComp;
	@ViewChild('CodeTreeAllocate') CodeTreeAllocate!: codesetSelectorComponent;
	@ViewChild('CodingToolTreeReports') CodingToolTree!: codesetSelectorComponent;

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
	tabsInitialized: boolean = false;

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
    public ItemsWithThisCodeDDData: Array<any> = [{
        text: 'With this Code (Excluded)',
        click: () => {
            this.ListItemsWithThisCode(false);
        }
    }];
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
    public ImportOrNewDDData: Array<any> = [{
        text: 'New Reference',
        click: () => {
            this.NewReference();
        }
    }];
    private _ShowQuickReport: boolean = false;
    public get ShowQuickReport(): boolean {
        if (this._ShowQuickReport && !this.ItemListService.HasSelectedItems) {
            this._ShowQuickReport = false;
            this.ItemCodingService.Clear();
            this.reviewSetsService.clearItemData();
        }
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
    public ShowClusterCommand: boolean = false;
    public HelpAndFeebackContext: string = "main\\reviewhome";

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
	public AllocateChoice: string = '';
	public AllIncOrExcShow: boolean = false;
	public RunReportsShow: boolean = false;
	public OrderByChoice: string = 'Short title';
	public ItemsChoice: string = 'All included items';
	public ReportChoice: Report = {} as Report;
	public AddBulletstoCodes: boolean = false;
	public AdditionalTextTag: string = '';
	public AssignDocs: string = 'true';
	public ItemIdModel: boolean = false;
	public ImportedIdModel: boolean = false;
	public ShortTitleModel: boolean = false;
	public TitleModel: boolean = false;
	public YearModel: boolean = false;
	public AbstractModel: boolean = false;
	public UncodedItemsModel: boolean = false;
	public AdditionalTextTagModel: string = '';
	public AddBulletsToCodesModel: boolean = false;
	public ShowRiskOfBiasFigureModel: boolean = false;
	public AlignmentModel: boolean = false;
	public OutcomesModel: boolean = false;
	public AllocateRelevantItems() {

		if (!this.AllIncOrExcShow) {

			this.AllIncOrExcShow = true;
		} else {

			this.AllIncOrExcShow = false;
		}
	}
	public CloseReportsSection() {

		this.RunReportsShow = false;
	}
	public CanRunReports() : boolean {

		return true;
	}
	public ShowCodeTree: boolean = false;
	public ItemsChoiceChange() {
		if (this.ItemsChoice == 'Items with this code') {
			this.ShowCodeTree = true;
		} else {
			this.ShowCodeTree = false;
		}
	}
	public ReportChoiceChange(item: Report) {
		if (item) {
			this.ReportChoice = item;
		}
	}
	public DropdownSelectedCodingTool: singleNode | null = null;
	public isCollapsedCodingTool: boolean = false;
	public DropDownBasicCodingTool: ReviewSet = new ReviewSet();
	public isCollapsedAllocateOptions: boolean = false;
	CloseCodeDropDownCodingTool() {

		if (this.CodingToolTree) {
			this.DropdownSelectedCodingTool = this.CodingToolTree.SelectedNodeData;
			console.log(JSON.stringify(this.DropdownSelectedCodingTool));
		}
		this.isCollapsedCodingTool = false;
	}
	public RunReports() {
		//console.log('report chocie is: ', this.ReportChoice);
		if (!this.HasSelectedItems || !this.HasWriteRights) {
			alert("Sorry: you don't have any items selected or you do not have permissions");
			return;
		}
		let attribute: SetAttribute = new SetAttribute();
		let reviewSet: ReviewSet = new ReviewSet();
		if (this.DropdownSelectedCodingTool) {
			if (this.DropdownSelectedCodingTool.nodeType =='ReviewSet') {
				reviewSet = this.DropdownSelectedCodingTool as ReviewSet;
			} else {
				attribute = this.DropdownSelectedCodingTool as SetAttribute;
			}
		}


		if (this.ReportChoice.reportType == "Answer") {

	
			alert('is an answer type');
			let args: ReportAnswerExecuteCommandParams = {} as ReportAnswerExecuteCommandParams;
			args.reportType = this.ReportChoice.reportType;
			args.codes = this.ItemListService.SelectedItems.map(x => x.itemId.toString()).join();
			args.reportId = this.ReportChoice.reportId;
			args.showItemId = this.ItemIdModel;
			args.showOldItemId = this.ImportedIdModel;
			args.showOutcomes = this.OutcomesModel;
			args.isHorizontal = this.AlignmentModel;
			args.orderBy = this.OrderByChoice;
			args.title = this.ReportChoice.name;
			args.attributeId = this.DropdownSelectedCodingTool != null ? attribute.attribute_id : 0;
			args.setId = this.DropdownSelectedCodingTool != null ? reviewSet.set_id : 0;
			args.showRiskOfBias = this.ShowRiskOfBiasFigureModel;
			args.showBullets = this.AddBulletsToCodesModel;
			args.showUncodedItems = this.UncodedItemsModel;
			args.txtInfoTag = this.AdditionalTextTagModel;

			if (args) {
				this.configurablereportServ.FetchAnswerReport(args);
			}

		}else {// report type is a question as a test

			let args: ReportQuestionExecuteCommandParams = {} as ReportQuestionExecuteCommandParams;
			args.items = this.ItemListService.SelectedItems.map(x => x.itemId.toString()).join();
			args.reportId = this.ReportChoice.reportId;
			args.orderBy = this.OrderByChoice;
			args.attributeId = this.DropdownSelectedCodingTool != null ? attribute.attribute_id : 0;
			args.setId = this.DropdownSelectedCodingTool != null ? reviewSet.set_id : 0;
			args.isHorizantal = this.AlignmentModel;
			args.showItemId = this.ItemIdModel;
			args.showOldID = this.ImportedIdModel;
			args.showOutcomes = this.OutcomesModel;
			args.showFullTitle = this.TitleModel;
			args.showAbstract = this.AbstractModel;
			args.showYear = this.YearModel;
			args.showShortTitle = this.ShortTitleModel;
			args.showRiskOfBias = this.ShowRiskOfBiasFigureModel;
			args.showBullets = this.AddBulletsToCodesModel;
			args.showUncodedItems = this.UncodedItemsModel;
			args.txtInfoTag = this.AdditionalTextTagModel;

			if (args) {

				//console.log('question report args: ', args);
				this.configurablereportServ.FetchQuestionReport(args);
			}
		}
		// TODO ASK SERGIO about the logic here not totally clear from the ER4 code.
		//else if (cmdGo.DataContext != null) {
		//}
	}

	public get ReportCollection(): Report[] | null {
		return this.configurablereportServ.Reports;
	}
	public GetReports() {

		this.configurablereportServ.FetchReports();
	}
	public RunConfigurableReports() {

		if (!this.RunReportsShow) {
			this.RunReportsShow = true;
			this.GetReports();

		} else {

			this.RunReportsShow = false;
		}
	}
	public isCollapsedCodeAllocate: boolean = false;
	public DropDownAllocateAtt: SetAttribute = new SetAttribute();
	public CloseCodeDropDownAllocate() {

		if (this.CodeTreeAllocate) {

			this.DropdownSelectedCodeAllocate = this.CodeTreeAllocate.SelectedNodeData;
			this.DropDownAllocateAtt = this.DropdownSelectedCodeAllocate as SetAttribute;
			
		}
		this.isCollapsedCodeAllocate = false;

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
	public CloseSection() {

		this.AllIncOrExcShow = false;
	}
	dtTrigger: Subject<any> = new Subject();
	private ListSubType: string = '';
	ngOnInit() {


        console.log("MainComp init: ", this.InstanceId);
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
		//this.reviewSetsService.GetReviewSets();
        this.subOpeningReview = this.ReviewerIdentityServ.OpeningNewReview.subscribe(() => this.Reload());
        this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
            () => this.GetStats()
        );
        if (this.codesetStatsServ.ReviewStats.itemsIncluded == -1
            || (this.reviewSetsService.ReviewSets == undefined && this.codesetStatsServ.tmpCodesets == undefined)
            || (this.reviewSetsService.ReviewSets.length > 0 && this.codesetStatsServ.tmpCodesets.length == 0)
        ) this.Reload();
        //this.searchService.Fetch();
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
        if (!this.ItemListService.HasSelectedItems) this._ShowQuickReport = false;
        else this._ShowQuickReport = !this._ShowQuickReport;
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
        this.ShowClusterCommand =  !this.ShowClusterCommand;
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
    ListItemsWithThisCode(Included: boolean) {
        if (!this.selectedNode || this.selectedNode.nodeType != "SetAttribute") return;
        let CurrentAtt = this.selectedNode as SetAttribute;
        if (!CurrentAtt) return;
        let cr: Criteria = new Criteria();
        cr.onlyIncluded = Included;
        cr.showDeleted = false;
        cr.pageNumber = 0;
        let ListDescription: string = "";
        if (Included) ListDescription = CurrentAtt.attribute_name + ".";
        else ListDescription = CurrentAtt.attribute_name + " (excluded).";
        cr.attributeid = CurrentAtt.attribute_id;
        cr.sourceId = 0;
        cr.listType = "StandardItemList";
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
            else {
                if (IsBulkAssign
					&& this.reviewSetsService.selectedNode) {
                    this.ConfirmationDialogService.confirm("Assign selected ("
                        + this.ItemListService.SelectedItems.length + ") items ? ", "Are you sure you want to assign all selected items to this ("
                        + this.reviewSetsService.selectedNode.name + ") code?", false, '')
                        .then((confirm: any) => {
                            if (confirm) {
                                this.BulkAssingCodes(SetA.attribute_id, SetA.set_id);
                            }
                        });
                }
                else if (!IsBulkAssign
                    && this.reviewSetsService.selectedNode) {
                    this.ConfirmationDialogService.confirm("Remove selected ("
                        + this.ItemListService.SelectedItems.length + ") items?", "Are you sure you want to remove all selected items to this ("
						+ this.reviewSetsService.selectedNode.name + ") code?", false, '')
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
	ngAfterViewInit() {
		//this.tabsInitialized = true;
		//console.log('tabs initialised');
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

	//ngOnChanges() {
		//if (this.tabsInitialized) {
		//	console.log('tabs experiment');

		//	this.tabstrip.selectTab(1);
		//}
	//}
    toggleReviewPanel() {
        this.isReviewPanelCollapsed = !this.isReviewPanelCollapsed;
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
        else if (e.title == 'Frequencies') {
            this.HelpAndFeebackContext = "main\\frequencies";
			this.ShowItemsTable = false;
			this.ShowSearchesAssign = false;
        }
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
        this.Clear();
        console.log('Reload mainfull');
        this.reviewSetsService.GetReviewSets();
        this.isSourcesPanelVisible = false;
		if (this.workAllocationsContactComp) this.workAllocationsContactComp.getWorkAllocationContactList();
        //else console.log("work allocs comp is undef :-(");
        if (this.ItemListService.ListCriteria && this.ItemListService.ListCriteria.listType == "") 
            this.IncludedItemsListNoTabChange();
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
    GetStats() {
        console.log('getting stats (mainfull):', this.InstanceId);
        this.codesetStatsServ.GetReviewStatisticsCountsCommand();
        this.codesetStatsServ.GetReviewSetsCodingCounts(true, this.dtTrigger);
    }
    Clear() {
        console.log('Clear in mainfull');
        this.ItemListService.SaveItems(new ItemList(), new Criteria());
        //this.codesetStatsServ.
        this.reviewSetsService.Clear();
        this.codesetStatsServ.Clear();
		this.SourcesService.Clear();
		this.workAllocationListService.Clear();

        if (this.FreqComponent) this.FreqComponent.Clear();
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

        //this.dtTrigger.unsubscribe();
        //if (this.statsSub) this.statsSub.unsubscribe();
        //this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
        //    () => this.GetStats()
        //);
    }
  
    public get MyAccountMessage(): string {
        let msg: string = "Your account expires on: ";
        let AccExp: string = new Date(this.ReviewerIdentityServ.reviewerIdentity.accountExpiration).toLocaleDateString();
        msg += AccExp;
        return msg;
    }
    public get MyReviewMessage(): string {
        let revPart: string = "";
        if (this.ReviewerIdentityServ.getDaysLeftReview() == -999999) {//review is private
            revPart = "Current review is private (does not expire).";
        }
        else {
            let RevExp: string = new Date(this.ReviewerIdentityServ.reviewerIdentity.reviewExpiration).toLocaleDateString();
            revPart = "Current(shared) review expires on " + RevExp + ".";
        }
        return revPart;
    }
    EditCodeSets() {
        this.router.navigate(['EditCodeSets']);
    }
    GoToSources() {
        this.router.navigate(['sources']);
    }
    //ImportCodesetClick() {
    //    this.router.navigate(['ImportCodesets']);
    //}
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
    }
}

export class RadioButtonComp {
	IncEnc = true;
}


