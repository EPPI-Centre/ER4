import { Component, OnInit, OnDestroy, Inject, ViewChild, EventEmitter, Output } from '@angular/core';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { iConfigurableReport, ConfigurableReportService, ReportStandard, ReportOutcomes, ReportRiskOfBias, CommonReportFields } from '../services/configurablereport.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
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
		private _confirmationDialogService: ConfirmationDialogService
	) { }
  @Output() PleaseCloseMe = new EventEmitter();
	public ReportStandard: ReportStandard = new ReportStandard();
	public ReportOutcomes: ReportOutcomes = new ReportOutcomes();
	public ReportRiskOfBias: ReportRiskOfBias = new ReportRiskOfBias();
	public ReportCommonParams: CommonReportFields = new CommonReportFields();

	ngOnInit() {
        //this.configurablereportServ.FetchReports();
		if (!this.HasSelectedItems) {
			this.ReportCommonParams.itemsChoice = "Items with this code";
			this.ItemsChoiceChange();
		}
	}

	ngOnDestroy() {

		
	}
	@ViewChild('CodingToolTreeReports') CodingToolTree!: codesetSelectorComponent;
	@ViewChild('CodeTreeAllocate') CodeTreeAllocate!: codesetSelectorComponent;

	public AllocateChoice: string = '';
	public AllIncOrExcShow: boolean = false;
	public RunReportsShow: boolean = false;
	public ReportChoice: iConfigurableReport = {} as iConfigurableReport;
	public AddBulletstoCodes: boolean = false;
	public AdditionalTextTag: string = '[Info]';
	public AssignDocs: string = 'true';
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
	public GeneratedReport: boolean = false;
	public QuestionReports: iConfigurableReport[] = [];
	public AnswerReports: iConfigurableReport[] = [];
	public tabSelectedIndex: number = 0;
	public showRiskOfBias() {
		this.showROB = !this.showROB;
	}
	public AlwaysShow: boolean = false;
	public RiskOfBias: boolean = false;
  public outcomesHidden: boolean = false;
  public get Reports(): iConfigurableReport[] | null {
    return this.configurablereportServ.Reports;
  }
  public get ReportCollectionROB(): iConfigurableReport[] | null {
    return this.configurablereportServ.ReportCollectionROB;
  }
  public get ReportCollectionOutcomes(): iConfigurableReport[] | null {
  return this.configurablereportServ.ReportCollectionOutcomes;
}

	public onTabSelect(event: any) {

		this.Clear();
		this.ReportChoice = {} as iConfigurableReport;
		let index: number = event.index;
		this.outcomesHidden = false;

		this.RiskOfBias = false;

		if (index == 1) {
			// ROB reports
			this.tabSelectedIndex = 1;
			this.RiskOfBias = true;

		} else if (index == 2) {
			this.tabSelectedIndex = 2;
			this.outcomesHidden = true;

		} else {
			this.tabSelectedIndex = 0;
		}

  }
  public OpenInNewWindow() {

		if (this.reportHTML.length < 1) return;
		else if (this.reportHTML.length < 1) {
			this.RunReports
		}
		else {//do the magic
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
  //public ChangedReport(item: any) {
  public ChangedReport(event: Event) {
    let selection = (event.target as HTMLOptionElement).value;
    if (selection != null || selection != '') {
			this.GeneratedReport = false;
		}
	}
  public CloseReportsSection() {
    this.PleaseCloseMe.emit();
    this.Clear();
  }
	public Clear() {

		
		this.configurablereportServ.reportHTML = '';
		this.reportHTML = '';
		this.ReportChoice = {} as iConfigurableReport;
		this.ReportCommonParams.itemsChoice == 'Items with this code'
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
    if (this.ReportCommonParams.itemsChoice == 'Items with this code') {
			this.ShowCodeTree = true;
    } else {
      this.DropdownSelectedCodingTool = null;
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

	public get CheckOptionsAreCorrectForReports(): boolean {

		if (this.ReportCommonParams.itemsChoice == 'All selected items') {
			if (!this.HasSelectedItems) {
				return false;
			} 
		}
		if (this.ReportCommonParams.itemsChoice == 'Items with this code') {
			if (this.DropdownSelectedCodingTool != null) {
                if (this.DropdownSelectedCodingTool.name == '' || this.DropdownSelectedCodingTool.nodeType == 'ReviewSet') {
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

		if (!this.HasSelectedItems && this.ReportCommonParams.itemsChoice == 'All selected items') {
			this._confirmationDialogService.confirm('Report Message', 'Sorry you have not selected any items', false,
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
      if (this.ReportCommonParams.itemsChoice == 'All selected items')
        this.ReportStandard.items = this.ItemListService.SelectedItems.map(x => x.itemId.toString()).join();
      else (this.ReportStandard.items = "");
			this.ReportStandard.orderBy = this.ReportCommonParams.orderByChoice;
			this.ReportStandard.attributeId = this.DropdownSelectedCodingTool != null ? attribute.attribute_id : 0;
			this.ReportStandard.setId = this.DropdownSelectedCodingTool != null ? reviewSet.set_id : 0;
			this.ReportStandard.showRiskOfBias = false;
			this.ReportStandard.isQuestion = this.ReportStandard.report.reportType == "Question";
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
      if (this.ReportCommonParams.itemsChoice == 'All selected items')
        this.ReportRiskOfBias.items = this.ItemListService.SelectedItems.map(x => x.itemId.toString()).join();
      else (this.ReportRiskOfBias.items = "");
			this.ReportRiskOfBias.orderBy = this.ReportCommonParams.orderByChoice;
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
	public RunOutcomesReports(attribute: SetAttribute, reviewSet: ReviewSet) {

		if (this.ReportOutcomes.report.reportId != 0) {
      this.ReportOutcomes.reportId = this.ReportOutcomes.report.reportId;

      if (this.ReportCommonParams.itemsChoice == 'All selected items')
        this.ReportOutcomes.codes = this.ItemListService.SelectedItems.map(x => x.itemId.toString()).join();
      else (this.ReportOutcomes.codes = "");

			this.ReportOutcomes.orderBy = this.ReportCommonParams.orderByChoice;
			this.ReportOutcomes.attributeId = this.DropdownSelectedCodingTool != null ? attribute.attribute_id : 0;
			this.ReportRiskOfBias.setId = this.DropdownSelectedCodingTool != null ? reviewSet.set_id : 0;
			this.ReportOutcomes.showOutcomes = true;

			if (this.ReportOutcomes) {
				var report = this.configurablereportServ.FetchOutcomesReport(this.ReportOutcomes);
				if (report) {
					report.then(
                        (res) => {
                            if (res.returnReport != 'error') {
                                this.reportHTML = res.returnReport;
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

	public OrderByChoiceChange() {

		if (this.ReportCommonParams.orderByChoice == 'Item Id') {
			this.ReportStandard.showItemId = true;
			this.ReportOutcomes.showItemId = true;
			this.ReportRiskOfBias.showItemId = true;

		} else if (this.ReportCommonParams.orderByChoice == 'Imported Id') {
			this.ReportStandard.showOldItemId = true;
			this.ReportOutcomes.showOldItemId = true;
			this.ReportRiskOfBias.showOldID = true;
	
		} else if (this.ReportCommonParams.orderByChoice == 'Short title') {
			this.ReportStandard.showShortTitle = true;
			this.ReportRiskOfBias.showShortTitle = true;
		
		} else if (this.ReportCommonParams.orderByChoice == 'Title') {
			this.ReportStandard.showFullTitle = true;
			this.ReportRiskOfBias.showFullTitle = true;

		} else if (this.ReportCommonParams.orderByChoice == 'Year') {
			this.ReportStandard.showYear = true;
			this.ReportRiskOfBias.showYear = true;

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
		const dataURI = "data:text/plain;base64," + encodeBase64(this.reportHTML);
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
