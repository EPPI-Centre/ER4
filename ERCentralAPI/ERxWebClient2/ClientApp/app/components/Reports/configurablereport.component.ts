import { Component, OnInit, OnDestroy, Inject, ViewChild } from '@angular/core';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { Report, ConfigurableReportService, ReportAnswerExecuteCommandParams, ReportQuestionExecuteCommandParams } from '../services/configurablereport.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { encodeBase64, saveAs } from '@progress/kendo-file-saver';
import { Helpers } from '../helpers/HelperMethods';

@Component({
	selector: 'configurablereport',
	templateUrl: './configurablereport.component.html',
})
export class configurablereportComp implements OnInit, OnDestroy {

	constructor(
		private ItemListService: ItemListService,
		@Inject('BASE_URL') private _baseUrl: string,
		private configurablereportServ: ConfigurableReportService,
		private ReviewerIdentityServ: ReviewerIdentityService,
		private EventEmitterServ: EventEmitterService,
		private _confirmationDialogService: ConfirmationDialogService
	) { }

	ngOnInit() {


		this.configurablereportServ.FetchReports(0);
		//if (this.configurablereportServ.Reports != null) {
		//	this.ReportChoice = this.configurablereportServ.Reports[0];
		//	console.log('report should nto be null initiliase: ', this.ReportChoice.name);
		//}


	}
	ngOnDestroy() {

	}
	@ViewChild('CodingToolTreeReports') CodingToolTree!: codesetSelectorComponent;
	@ViewChild('CodeTreeAllocate') CodeTreeAllocate!: codesetSelectorComponent;

	public AllocateChoice: string = '';
	public AllIncOrExcShow: boolean = false;
	public RunReportsShow: boolean = false;
	public OrderByChoice: string = 'Short title';
	public ItemsChoice: string = 'All selected items';
	public ReportChoice: Report = {} as Report;
	public AddBulletstoCodes: boolean = false;
	public AdditionalTextTag: string = '[Info]';
	public AssignDocs: string = 'true';
	public ItemIdModel: boolean = true;
	public ImportedIdModel: boolean = false;
	public ShortTitleModel: boolean = true;
	public TitleModel: boolean = false;
	public YearModel: boolean = false;
	public AbstractModel: boolean = false;
	public UncodedItemsModel: boolean = true;
	public AdditionalTextTagModel: string = '[Info]';
	public AddBulletsToCodesModel: boolean = true;
	public ShowRiskOfBiasFigureModel: boolean = false;
	public AlignmentHorizontalModel: boolean = true;
	public AlignmentVerticalModel: boolean = false;
	public OutcomesModel: boolean = false;
	public DropdownSelectedCodeAllocate: singleNode | null = null;
	public DropdownSelectedCodingTool: singleNode | null = null;
	public isCollapsedCodingTool: boolean = false;
	public DropDownBasicCodingTool: ReviewSet = new ReviewSet();
	public isCollapsedAllocateOptions: boolean = false;
	public ShowCodeTree: boolean = false;
	public isCollapsedCodeAllocate: boolean = false;
	public DropDownAllocateAtt: SetAttribute = new SetAttribute();
	public showROB: boolean = false;
	public reportHTML: string = '';
	public sectionShow: string = 'Standard';
	public GeneratedReport: boolean = false;
	public QuestionReports: Report[] = [];
	public AnswerReports: Report[] = [];
	public tabSelectedIndex: number = 0;
	public showRiskOfBias() {
		this.showROB = !this.showROB;
	}
	public AlwaysShow: boolean = false;
	public RiskOfBias: boolean = false;
	public outcomesHidden: boolean = false;
	public ShowStandard() {

		this.OutcomesModel = false;
		this.RiskOfBias = false;

		if (this.sectionShow == 'Standard') {
			this.sectionShow = ''

		} else if (this.sectionShow == '') {
			this.sectionShow = 'Standard';
		}
		console.log(this.sectionShow);
	}
	public ShowRiskOfBias() {

		this.OutcomesModel = false;
		if (this.RiskOfBias) {
			this.sectionShow = ''
			this.RiskOfBias = false;
		} else {
			this.sectionShow = 'Standard';
			this.RiskOfBias = true;
		}
	}
	public onTabSelect(event: any) {
		
		let index: number = event.index;
		this.outcomesHidden = false;
		this.OutcomesModel = false;
		this.ShowRiskOfBiasFigureModel = false;
		this.RiskOfBias = false;
		if (index == 1) {
			// ROB reports
			this.tabSelectedIndex = 1;
			this.RiskOfBias = true;
			this.ShowRiskOfBiasFigureModel = true;
			this.configurablereportServ.FetchReports(1);
		} else if (index == 2) {
			this.tabSelectedIndex = 2;
			this.outcomesHidden = true;
			this.OutcomesModel = true;
			this.configurablereportServ.FetchReports(2);
		} else {
			this.tabSelectedIndex = 0;
			this.configurablereportServ.FetchReports(0);
		}
	}
	public ShowOutcomes() {

		this.RiskOfBias = false;
		if (!this.outcomesHidden) {
			this.OutcomesModel = true;
			this.outcomesHidden = true;
			this.sectionShow = 'Standard'
		} else {
			this.outcomesHidden = false;
			this.sectionShow = ''
		}
	}
	public OpenInNewWindow() {

		if (this.reportHTML.length < 1) return;
		else if (this.reportHTML.length < 1) {
			this.RunReports
		}
		else {//do the magic

			console.log('got in here');
			let Pagelink = "about:blank";
			let pwa = window.open(Pagelink, "_new");
			//let pwa = window.open("data:text/plain;base64," + btoa(this.AddHTMLFrame(this.ReportHTML)), "_new");
			if (pwa) {
				pwa.document.open();

				pwa.document.write(Helpers.AddHTMLFrame(this.reportHTML, this._baseUrl));
				pwa.document.close();
			}
		}
	}
	public ChangedReport(item: any) {
		if (item != null || item != '') {
			this.GeneratedReport = false;
		}
	}
	public CloseReportsSection() {

		this.EventEmitterServ.CloseReportsSectionEmitter.emit();
		this.Clear();

	}
	public Clear() {

		this.configurablereportServ.FetchReports(0);
		this.configurablereportServ.reportHTML = '';
		this.reportHTML = '';
		this.ReportChoice = {} as Report;
		this.ItemsChoice == 'Items with this code'
		this.DropdownSelectedCodingTool = {} as singleNode;
		this.GeneratedReport = false;

	}
	public CanRunReports(): boolean {

		if (this.ReportChoice == null || this.ReportChoice.name == '') {

			return false;
		} else {
			return true;
		}
	}
	public ItemsChoiceChange() {
		if (this.ItemsChoice == 'Items with this code') {
			this.ShowCodeTree = true;
		} else {
			this.ShowCodeTree = false;
		}
	}
	CloseCodeDropDownCodingTool() {
		if (this.CodingToolTree) {
			this.DropdownSelectedCodingTool = this.CodingToolTree.SelectedNodeData;
			//console.log(JSON.stringify(this.DropdownSelectedCodingTool));
		}
		this.isCollapsedCodingTool = false;
	}
	public get HasSelectedItems(): boolean {
		return this.ItemListService.HasSelectedItems;
	}
	public get HasReport(): boolean {
		if (this.ReportChoice && this.ReportChoice.name) {
			return this.ReportChoice.name.length > 0 ? true : false;
		} else {
			return false;
		}
	}
	public get HasWriteRights(): boolean {
		return this.ReviewerIdentityServ.HasWriteRights;
	}
	public get CheckOptionsAreCorrectForReports(): boolean {

		if (this.ReportChoice == null) {
			return false;
		}
		if (this.ItemsChoice == 'All selected items') {
			if (!this.HasSelectedItems) {
				return false;
			} else {
				return true;
			}
		}
		if (this.ItemsChoice == 'Items with this code') {
			if (this.DropdownSelectedCodingTool != null) {
				if (this.DropdownSelectedCodingTool.name != '') {
					return true;
				} else {
					return false;
				}
			}
			else {
				return false;
			}
		}
		if (this.tabSelectedIndex == 0) {
			//STANDARD
			if (this.ItemsChoice == 'All included items') {
				//not sure yet...

			}
		} else if (this.tabSelectedIndex == 1) {
			//ROB
			if (!this.RiskOfBias) {
				return false;
			}

		} else if (this.tabSelectedIndex == 2) {
			//OUTCOME
			if (this.ReportChoice == null) {
				return false;
			}
			if (this.OutcomesModel == null) {
				return false;
			}

		}
		return false;
	}
	public RunReports()  {


		//console.log('report should nto be null: ', this.ReportChoice.name);

		if (this.ReportChoice == null || this.ReportChoice == undefined
			|| this.ReportChoice.name == 'Please selected a generated report') {
			return;
		}

		if (!this.HasSelectedItems && this.ItemsChoice == 'All selected items') {
			this._confirmationDialogService.confirm('Report Message', 'Sorry you have not selected any items', false,
				'', 'Ok')
				.then({}
				);
			return;
		}
		if (!this.HasWriteRights) {
			this._confirmationDialogService.confirm('Report Message', 'Sorry you do not have permission', false,
				'', 'Ok')
				.then({}
				);
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

		if (this.ReportChoice.reportType == "Answer" && this.OutcomesModel) {

			//('this is an answer');
			let args: ReportAnswerExecuteCommandParams = {} as ReportAnswerExecuteCommandParams;
			args.reportType = this.ReportChoice.reportType;
			args.codes = this.ItemListService.SelectedItems.map(x => x.itemId.toString()).join();
			args.reportId = this.ReportChoice.reportId;
			args.showItemId = this.ItemIdModel;
			args.showOldItemId = this.ImportedIdModel;
			args.showOutcomes = this.OutcomesModel;
			args.isHorizontal = this.AlignmentHorizontalModel;
			args.orderBy = this.OrderByChoice;
			args.title = this.ReportChoice.name;
			args.attributeId = this.DropdownSelectedCodingTool != null ? attribute.attribute_id : 0;
			args.setId = this.DropdownSelectedCodingTool != null ? reviewSet.set_id : 0;
			args.showRiskOfBias = this.ShowRiskOfBiasFigureModel;
			args.showBullets = this.AddBulletsToCodesModel;
			args.showUncodedItems = !this.UncodedItemsModel;
			args.txtInfoTag = this.AdditionalTextTagModel;
			args.isQuestion = false;

			if (args) {
				var report = this.configurablereportServ.FetchOutcomesReport(args);
				if (report) {
					report.then(
						(res) => {
							this.reportHTML = res.returnReport;
							if (this.GeneratedReport) {
								this.OpenInNewWindow();
							}
						}
					);
				}
			}

		} else {// report type is a question as a test

			//alert('this is a question');
			let args: ReportQuestionExecuteCommandParams = {} as ReportQuestionExecuteCommandParams;
			args.items = this.ItemListService.SelectedItems.map(x => x.itemId.toString()).join();
			args.reportId = this.ReportChoice.reportId;
			args.orderBy = this.OrderByChoice;
			args.attributeId = this.DropdownSelectedCodingTool != null ? attribute.attribute_id : 0;
			args.setId = this.DropdownSelectedCodingTool != null ? reviewSet.set_id : 0;
			args.isHorizontal = this.AlignmentHorizontalModel;
			args.showItemId = this.ItemIdModel;
			args.showOldID = this.ImportedIdModel;
			args.showOutcomes = this.OutcomesModel;
			args.showFullTitle = this.TitleModel;
			args.showAbstract = this.AbstractModel;
			args.showYear = this.YearModel;
			args.showShortTitle = this.ShortTitleModel;
			args.showRiskOfBias = this.ShowRiskOfBiasFigureModel;
			args.showBullets = this.AddBulletsToCodesModel;
			args.showUncodedItems = !this.UncodedItemsModel;
			args.txtInfoTag = this.AdditionalTextTagModel;
			args.isQuestion = this.ReportChoice.reportType == "Question" ? true : false;

			if (args) {
				var stringReport = this.configurablereportServ.FetchQuestionReport(args);
				if (stringReport) {
					stringReport.then(
						(result) => {
							this.reportHTML = result;
							this.GeneratedReport = true;
							if (this.GeneratedReport) {
								this.OpenInNewWindow();
							}
						}
					);
				}
			}
		}


	}
	public OrderByChoiceChange() {
		if (this.OrderByChoice == 'Item Id') {
			this.ItemIdModel = true;
		} else if (this.OrderByChoice == 'Imported Id') {
			this.ImportedIdModel = true;
		} else if (this.OrderByChoice == 'Short title') {
			this.ShortTitleModel = true;
		} else if (this.OrderByChoice == 'Title') {
			this.TitleModel = true;
		} else if (this.OrderByChoice == 'Year') {
			this.YearModel = true;
		}
	}
	public get ReportCollection(): Report[] | null {
		if (this.configurablereportServ.Reports) {
			let viewableReports: Report[] = [];
			let dummyReport = {} as Report;
			viewableReports.push(dummyReport);
			for (var i = 0; i < this.configurablereportServ.Reports.length ; i++) {
				viewableReports.push(this.configurablereportServ.Reports[i]);
			}
			this.configurablereportServ.Reports;
			return viewableReports;
		} else {
			return null;
		}
	}
	public SaveReport() {
		this.SaveAsHtml();
	}
	public get IsServiceBusy(): boolean {
		return (this.configurablereportServ.IsBusy);
	}
	public CanSaveReport(): boolean {
		if (this.configurablereportServ.reportHTML.length < 1) return false;
		return true;
	}
	public SaveAsHtml() {

		if (this.reportHTML.length < 1) return;
		const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(this.reportHTML, this._baseUrl));
		saveAs(dataURI, "ConfigurableReport.html");
	}
	public GetReports() {
		this.configurablereportServ.FetchReports(0);
	}
	public RunConfigurableReports() {
		if (!this.RunReportsShow) {
			this.RunReportsShow = true;
			this.GetReports();
		} else {
			this.RunReportsShow = false;
		}
	}
	public CloseCodeDropDownAllocate() {
		if (this.CodeTreeAllocate) {
			this.DropdownSelectedCodeAllocate = this.CodeTreeAllocate.SelectedNodeData;
			this.DropDownAllocateAtt = this.DropdownSelectedCodeAllocate as SetAttribute;
		}
		this.isCollapsedCodeAllocate = false;
	}
}
