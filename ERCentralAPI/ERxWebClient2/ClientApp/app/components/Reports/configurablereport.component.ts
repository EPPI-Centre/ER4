import { Component, OnInit, OnDestroy, Inject, ViewChild } from '@angular/core';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet,  SetAttribute, singleNode } from '../services/ReviewSets.service';
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

	}

	// TODO NOT COMPLETE NEED TO CLEAR RELEVANT VARIABLES
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

	public showRiskOfBias() {

		this.showROB = !this.showROB;
	}
	public OpenInNewWindow() {

		if (this.reportHTML.length < 1 ) return;
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
	public CloseReportsSection() {

		this.Clear();
	
	}
	public Clear() {

		//this.RunReportsShow = false;
		this.configurablereportServ.FetchReports();
		this.configurablereportServ.reportHTML = '';
		this.reportHTML = '';
		this.ReportChoice = {} as Report;
		this.ItemsChoice == 'Items with this code'
		this.DropdownSelectedCodingTool = {} as singleNode;

	}
	//TODO NOT COMPLETE SERGIO TO CHECK THE SPEC OF WHAT ENABLES THE REPORT TO BE SHOWN
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
		console.log('ItemsChoiceChange: ', this.ItemsChoice );
	}
	public ReportChoiceChange() {

		console.log('ReportChoiceChange: ', this.ReportChoice.name);
	}
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
	public RunReports() {

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

		if (this.ReportChoice.reportType == "Answer") {

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
			args.showUncodedItems = this.UncodedItemsModel;
			args.txtInfoTag = this.AdditionalTextTagModel;
			args.isQuestion = false;

			if (args) {
				var report = this.configurablereportServ.FetchAnswerReport(args);
				if (report) {

					report.toPromise().then(
						
						(res) => {
	
							this.reportHTML = res.returnReport
							console.log('reporthtml: ', this.reportHTML);
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
			args.isHorizantal = this.AlignmentHorizontalModel;
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
			args.isQuestion = true;

			if (args) {

				var report = this.configurablereportServ.FetchQuestionReport(args);
				if (report) {

					report.toPromise().then(
					
						(res) => {
							
								this.reportHTML = res.returnReport
								console.log('reporthtml: ', this.reportHTML);
							}
						);
					
				}
			}
		}
		// TODO ASK SERGIO about the logic here not totally clear from the ER4 code.
		//else if (cmdGo.DataContext != null) {
		//}
	}

	public get ReportCollection(): Report[] | null {
		return this.configurablereportServ.Reports;
	}
	public SaveReport() {
		//if (this.JsonReport) this.SaveAsJson();
		//else
		this.SaveAsHtml();
		//console.log(this.configurablereportServ.reportHTML.length);
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
		//this.reportHTML = this.configurablereportServ.reportHTML;
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
