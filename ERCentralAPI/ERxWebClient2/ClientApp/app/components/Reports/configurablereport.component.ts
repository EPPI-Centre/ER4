import { Component, OnInit, OnDestroy, Inject, ViewChild } from '@angular/core';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet,  SetAttribute, singleNode } from '../services/ReviewSets.service';
import { Report, ConfigurableReportService, ReportAnswerExecuteCommandParams, ReportQuestionExecuteCommandParams } from '../services/configurablereport.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

@Component({
    selector: 'configurablereport',
    templateUrl: './configurablereport.component.html',
})

export class configurablereportComp implements OnInit, OnDestroy {

    constructor(
        private ItemListService: ItemListService,
		@Inject('BASE_URL') private _baseUrl: string,
		private configurablereportServ: ConfigurableReportService,
		private ReviewerIdentityServ: ReviewerIdentityService
    ) { }

	ngOnInit() {
	}

	ngOnDestroy() {

	}	
	public DropdownSelectedCodeAllocate: singleNode | null = null;
	@ViewChild('CodingToolTreeReports') CodingToolTree!: codesetSelectorComponent;
	@ViewChild('CodeTreeAllocate') CodeTreeAllocate!: codesetSelectorComponent;

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
	public CanRunReports(): boolean {

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
	public get HasSelectedItems(): boolean {
		return this.ItemListService.HasSelectedItems;
	}
	public get HasWriteRights(): boolean {
		return this.ReviewerIdentityServ.HasWriteRights;
	}
	public RunReports() {

		if (!this.HasSelectedItems || !this.HasWriteRights) {
			alert("Sorry: you don't have any items selected or you do not have permissions");
			return;
		}
		let attribute: SetAttribute = new SetAttribute();
		let reviewSet: ReviewSet = new ReviewSet();
		if (this.DropdownSelectedCodingTool) {
			if (this.DropdownSelectedCodingTool.nodeType == 'ReviewSet') {
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

		} else {// report type is a question as a test

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

}
