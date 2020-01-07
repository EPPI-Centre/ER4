import { Component, OnInit, OnDestroy, Inject, ViewChild } from '@angular/core';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { Report, ConfigurableReportService, ReportStandard, ReportOutcomes, ReportRiskOfBias } from '../services/configurablereport.service';
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

	public ReportStandard: ReportStandard = new ReportStandard();
	public ReportOutcomes: ReportOutcomes = new ReportOutcomes();
	public ReportRiskOfBias: ReportRiskOfBias = new ReportRiskOfBias();

	ngOnInit() {

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
	public ReportChoiceStandard: Report = {} as Report;
	public ReportChoiceROB: Report = {} as Report;
	public ReportChoiceOutcomes: Report = {} as Report;

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

		this.Clear();
		this.ReportChoice = {} as Report;
		let index: number = event.index;
		this.outcomesHidden = false;
		this.OutcomesModel = false;
		this.ShowRiskOfBiasFigureModel = false;
		this.RiskOfBias = false;
		this.ReportChoiceOutcomes = {} as Report;
		this.ReportChoiceStandard = {} as Report;
		this.ReportChoiceROB = {} as Report;
		if (index == 1) {
			// ROB reports
			this.tabSelectedIndex = 1;
			this.RiskOfBias = true;
			this.ShowRiskOfBiasFigureModel = true;
		} else if (index == 2) {
			this.tabSelectedIndex = 2;
			this.outcomesHidden = true;
			this.OutcomesModel = true;
		} else {
			this.tabSelectedIndex = 0;
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

		this.configurablereportServ.FetchReports();
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


		if (this.ItemsChoice == 'All selected items') {
			if (!this.HasSelectedItems) {
				return false;
			} 
		}

		if (this.ItemsChoice == 'Items with this code') {
			if (this.DropdownSelectedCodingTool != null) {
				if (this.DropdownSelectedCodingTool.name != '') {
				
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
			
			if (this.ReportStandard.report.reportId > 0) {
				return true;
			} else {
				return false;
			}
		} else if (this.tabSelectedIndex == 1) {
			//ROB

			if (this.ReportRiskOfBias.report.reportId > 0) {
				return true;
			}

		} else if (this.tabSelectedIndex == 2) {
			
			if (this.ReportOutcomes.report.reportId > 0) {
				return true;
			}
		}
		return false;
	}
	
	public RunReports()  {

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

		if (this.tabSelectedIndex == 0) {

			this.RunStandardReports(attribute, reviewSet);

		} else if (this.tabSelectedIndex == 1) {

			this.RunROBReports(attribute, reviewSet);

		} else if (this.tabSelectedIndex == 2) {

			this.RunOutcomesReports(attribute, reviewSet);

		}
	}

	public RunStandardReports(attribute: SetAttribute, reviewSet: ReviewSet) {

		if (this.ReportStandard.report.reportId != 0) {

			this.ReportStandard.items = this.ItemListService.SelectedItems.map(x => x.itemId.toString()).join();
			this.ReportStandard.orderBy = this.OrderByChoice;
			this.ReportStandard.attributeId = this.DropdownSelectedCodingTool != null ? attribute.attribute_id : 0;
			this.ReportStandard.setId = this.DropdownSelectedCodingTool != null ? reviewSet.set_id : 0;
			this.ReportStandard.showRiskOfBias = false;
			this.ReportStandard.isQuestion = true;
			this.ReportStandard.reportId = this.ReportStandard.report.reportId;
			
			if (this.ReportStandard != null) {
				var stringReport = this.configurablereportServ.FetchStandardReport(this.ReportStandard);
				if (stringReport) {
					stringReport.then(
						(result) => {
							if (result != 'error') {
								this.reportHTML = result;
								this.GeneratedReport = true;
								if (this.GeneratedReport) {
									this.OpenInNewWindow();
									return;
								}
							}
						}
					);
				}
			}
		}
	}
	public RunROBReports(attribute: SetAttribute, reviewSet: ReviewSet) {

		if (this.ReportRiskOfBias.report.reportId) {

			this.ReportRiskOfBias.items = this.ItemListService.SelectedItems.map(x => x.itemId.toString()).join();
			this.ReportRiskOfBias.orderBy = this.OrderByChoice;
			this.ReportRiskOfBias.attributeId = this.DropdownSelectedCodingTool != null ? attribute.attribute_id : 0;
			this.ReportRiskOfBias.setId = this.DropdownSelectedCodingTool != null ? reviewSet.set_id : 0;
			this.ReportRiskOfBias.isQuestion = true;
			this.ReportRiskOfBias.reportId = this.ReportRiskOfBias.report.reportId;
			this.ReportRiskOfBias.showRiskOfBias = true;

			if (this.ReportRiskOfBias) {
				var stringReport = this.configurablereportServ.FetchROBReport(this.ReportRiskOfBias);
				if (stringReport) {
					stringReport.then(
						(result) => {
							this.reportHTML = result;
							this.GeneratedReport = true;
							if (this.GeneratedReport) {
								this.OpenInNewWindow();
								return;
							}
						}
					);
				}
			}
		}
	}
	public RunOutcomesReports(attribute: SetAttribute, reviewSet: ReviewSet) {

		if (this.ReportOutcomes.report.reportId != 0) {

			this.ReportOutcomes.reportId = this.ReportOutcomes.report.reportId;
			this.ReportOutcomes.codes = this.ItemListService.SelectedItems.map(x => x.itemId.toString()).join();
			this.ReportOutcomes.orderBy = this.OrderByChoice;
			this.ReportOutcomes.attributeId = this.DropdownSelectedCodingTool != null ? attribute.attribute_id : 0;
			this.ReportRiskOfBias.setId = this.DropdownSelectedCodingTool != null ? reviewSet.set_id : 0;
			this.ReportOutcomes.showOutcomes = true;

			if (this.ReportOutcomes) {
				var report = this.configurablereportServ.FetchOutcomesReport(this.ReportOutcomes);
				if (report) {
					report.then(
						(res) => {
							this.reportHTML = res.returnReport;
							this.GeneratedReport = true;
							if (this.GeneratedReport) {
								this.OpenInNewWindow();
								return;
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
	public CloseCodeDropDownAllocate() {
		if (this.CodeTreeAllocate) {
			this.DropdownSelectedCodeAllocate = this.CodeTreeAllocate.SelectedNodeData;
			this.DropDownAllocateAtt = this.DropdownSelectedCodeAllocate as SetAttribute;
		}
		this.isCollapsedCodeAllocate = false;
	}

}
