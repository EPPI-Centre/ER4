import { Component, OnInit, OnDestroy, Inject, ViewChild } from '@angular/core';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet, SetAttribute, singleNode, ReviewSetsService } from '../services/ReviewSets.service';
import { iConfigurableReport, ConfigurableReportService, ReportStandard, ReportOutcomes, ReportRiskOfBias, CommonReportFields, iReportColumnCode, iReportColumn } from '../services/configurablereport.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { encodeBase64, saveAs } from '@progress/kendo-file-saver';
import { Helpers } from '../helpers/HelperMethods';

@Component({
	selector: 'SetupConfigurableReports',
	templateUrl: './SetupConfigurableReports.component.html',
})
export class SetupConfigurableReports implements OnInit, OnDestroy {

	constructor(
		private ItemListService: ItemListService,
		@Inject('BASE_URL') private _baseUrl: string,
		private configurablereportServ: ConfigurableReportService,
		private ReviewerIdentityServ: ReviewerIdentityService,
		private EventEmitterServ: EventEmitterService,
		private reviewSetsService: ReviewSetsService,
		private confirmationDialogService: ConfirmationDialogService
	) { }

	ngOnInit() {
		if (this.configurablereportServ.Reports == null || this.configurablereportServ.Reports.length == 0)
			this.configurablereportServ.FetchReports();
	}
	public get HasWriteRights(): boolean {
		return this.ReviewerIdentityServ.HasWriteRights;
	}

	public get selectedNode(): singleNode | null {
		return this.reviewSetsService.selectedNode;
	}
	public EditingReport: iConfigurableReport | null = null;
	private EditingColumn: iReportColumn | null = null;
	private EditingColumnCode: iReportColumnCode | null = null;
	public EditingReportHasChanged: boolean = false;
	public ShowCreateNew: boolean = false;
	public UpdatingReportName: boolean = false;
	public get Reports(): iConfigurableReport[] {
		return this.configurablereportServ.Reports;
    }
	public EditReport(rpt: iConfigurableReport) {
		this.EditingReport = ConfigurableReportService.CloneReport(rpt);
	}
	//we use getters and setters so to allow keeping track of changes, these are in use for private members "EditingColumn" and "EditingColumnCode".
	public get EditingReportName(): string {
		if (this.EditingReport) return this.EditingReport.name;
		else return "N/A";
	}
	public set EditingReportName(val: string) {
		if (this.EditingReport && val !== this.EditingReport.name) {
			this.EditingReportHasChanged = true;
			this.EditingReport.name = val;
		}
	}
	public get EditingReportType(): string {
		if (this.EditingReport) {
			//console.log("get EditingReportType", this.EditingReport.reportType);
			return this.EditingReport.reportType;
		}
		else return "N/A";
    }
	public set EditingReportType(val: string) {
		if (this.EditingReport == null) return;
		else {

			//console.log("set EditingReportType", this.EditingReport.reportType, val);
			if (this.EditingReport.reportType != val
				&& (val == "Question" || val == "Answer")
				&& this.EditingReport.columns.findIndex(f => f.codes.length > 1) == -1) {
				//we can change type as all columns have 1 code or less 
				this.EditingReportHasChanged = true;
				this.EditingReport.reportType = val;
				if (val == "Question") this.EditingReport.isAnswer = false;
				else this.EditingReport.isAnswer = true;
            }
		}
	}
	public get EditingColumnId(): number | null {
		if (this.EditingColumn) return this.EditingColumn.reportColumnId;
		else return null;
	}
	public get EditingColumnName(): string {
		if (this.EditingColumn) return this.EditingColumn.name;
		else return "N/A";
	}
	public set EditingColumnName(val: string) {
		if (this.EditingColumn && val !== this.EditingColumn.name) {
			this.EditingReportHasChanged = true;
			this.EditingColumn.name = val;
		}
	}

	public get EditingColumnCodeId(): number | null {
		if (this.EditingColumnCode) return this.EditingColumnCode.reportColumnCodeId;
		else return null;
	}
	public get EditingColumnNamedisplayAdditionalText(): boolean {
		if (this.EditingColumnCode) return this.EditingColumnCode.displayAdditionalText;
		else return false;
	}
	public set EditingColumnNamedisplayAdditionalText(val: boolean) {
		if (this.EditingColumnCode && val !== this.EditingColumnCode.displayAdditionalText) {
			this.EditingReportHasChanged = true;
			this.EditingColumnCode.displayAdditionalText = val;
		}
	}
	public get EditingColumnCodedisplayCode(): boolean {
		if (this.EditingColumnCode) return this.EditingColumnCode.displayCode;
		else return false;
	}
	public set EditingColumnCodedisplayCode(val: boolean) {
		if (this.EditingColumnCode && val !== this.EditingColumnCode.displayCode) {
			this.EditingReportHasChanged = true;
			this.EditingColumnCode.displayCode = val;
		}
	}
	public get EditingColumnCodedisplayCodedText(): boolean {
		if (this.EditingColumnCode) return this.EditingColumnCode.displayCodedText;
		else return false;
	}
	public set EditingColumnCodedisplayCodedText(val: boolean) {
		if (this.EditingColumnCode && val !== this.EditingColumnCode.displayCodedText) {
			this.EditingReportHasChanged = true;
			this.EditingColumnCode.displayCodedText = val;
		}
	}
	public get EditingColumnCodeText(): string {
		if (this.EditingColumnCode) return this.EditingColumnCode.userDefText;
		else return "N/A";
	}
	public set EditingColumnCodeText(val: string) {
		if (this.EditingColumnCode && val !== this.EditingColumnCode.userDefText) {
			this.EditingReportHasChanged = true;
			this.EditingColumnCode.userDefText = val;
		}
	}

	//Order: {{ code.codeOrder }}<br />
	//Show add.txt: { { code.displayAdditionalText } } <br />
	//Show code name: { { code.displayCode } } <br />
	//Show PDF txt: { { code.displayCodedText } } <br />
	//ParentCodeId: { { code.parentAttributeId } } <br />
	//parentAttributeText ? {{ code.parentAttributeText }}<br />
	//reportColumnCodeId: { { code.reportColumnCodeId } } <br />
	//reportColumnId: { { code.reportColumnId } } <br />
	//setId: { { code.setId } } <br />
	//userDefText: { { code.userDefText } }
	public FetchReports() {
		this.CancelEditing();
		this.configurablereportServ.FetchReports();
    }
	public CancelEditing() {
		this.EditingReport = null;
		this.EditingReportHasChanged = false;
	}
	public NewReport() {
		let newR: iConfigurableReport = {
			name: "New Report (please edit)",
			reportId: 0,
			contactId: this.ReviewerIdentityServ.reviewerIdentity.userId,
			contactName: this.ReviewerIdentityServ.reviewerIdentity.name,
			isAnswer: false,
			reportType: "Question",
			//detail: string;
			columns: []
		};
		this.EditingReportHasChanged = false;
		this.EditingReport = newR;
		this.ShowCreateNew = true;
    }
	public MoveColumnCodeDown(col: iReportColumn, code: iReportColumnCode) {
		if (this.EditingReport == null) return;
		let toMoveInd = col.codes.findIndex(f => f.codeOrder == code.codeOrder + 1);
		if (toMoveInd == -1) {
			//bad luck, the oder values are malformed, let's fix them...
			this.FixOrderValuesInColumnCodes(col);
			toMoveInd = col.codes.findIndex(f => f.codeOrder == code.codeOrder + 1);
			if (toMoveInd == -1) {
				//??? this shouldn't happen!
				return;
            }
		}
		const toMove = col.codes[toMoveInd];
		toMove.codeOrder--;
		code.codeOrder++;
		col.codes.splice(toMoveInd - 1, 2, toMove, code);
		this.EditingReportHasChanged = true;
	}
	public MoveColumnCodeUp(col: iReportColumn, code: iReportColumnCode) {
		if (this.EditingReport == null) return;
		let toMoveInd = col.codes.findIndex(f => f.codeOrder == code.codeOrder - 1);
		if (toMoveInd == -1) {
			//bad luck, the oder values are malformed, let's fix them...
			this.FixOrderValuesInColumnCodes(col);
			toMoveInd = col.codes.findIndex(f => f.codeOrder == code.codeOrder - 1);
			if (toMoveInd == -1) {
				//??? this shouldn't happen!
				return;
			}
		}
		const toMove = col.codes[toMoveInd];
		toMove.codeOrder++;
		code.codeOrder--;
		col.codes.splice(toMoveInd, 2, code, toMove);
		this.EditingReportHasChanged = true;
	}
	private FixOrderValuesInColumnCodes(col: iReportColumn) {
		let i: number = 0;
		for (let cc of col.codes) {
			cc.codeOrder = i;
			i++;
		}
		this.EditingReportHasChanged = true;
	}
	private FixOrderValuesInColumns(EditingReport: iConfigurableReport) {
		let i: number = 0;
		for (let cc of EditingReport.columns) {
			cc.columnOrder = i;
			i++;
		}
		this.EditingReportHasChanged = true;
    }
	public AddColumn() {
		if (this.EditingReport == null) return;
		else {
			this.EditingReportHasChanged = true;
			let newCol: iReportColumn = {
				codes: [],
				columnOrder: this.EditingReport.columns.length,
				name: "New Column",
				reportColumnId: 0
            }
			this.EditingReport.columns.push(newCol);
		}
	}
	public CanAddNodeToColumn(col: iReportColumn, nodeToAdd: singleNode | null): boolean {
		if (this.EditingReport == null || nodeToAdd == null) return false;
		else {
			if (this.EditingReport.reportType == "Question" && nodeToAdd.attributes.length > 0) return true;
			else if (this.EditingReport.reportType == "Answer" && col.codes.length == 0 && nodeToAdd.nodeType == "SetAttribute") return true;
		}
		return false;
    } 
	public AddCodeToColumn(col: iReportColumn, nodeToAdd: singleNode) {
		if (!this.CanAddNodeToColumn(col, nodeToAdd)) return;
		let colCode: iReportColumnCode = {
			attributeId: 0,
			codeOrder: col.codes.length,
			displayAdditionalText: true,
			displayCode: true,
			displayCodedText: true,
			parentAttributeId: 0,
			parentAttributeText: nodeToAdd.name,
			reportColumnCodeId: 0,
			reportColumnId: col.reportColumnId,
			setId: nodeToAdd.set_id,
			userDefText: nodeToAdd.name,
		}
		if (nodeToAdd.nodeType == "SetAttribute") {
			let att = nodeToAdd as SetAttribute;
			colCode.attributeId = att.attribute_id;
			colCode.parentAttributeId = att.attribute_id;
		} else if (nodeToAdd.nodeType !== "ReviewSet") { return; }
		this.EditingReportHasChanged = true;
		col.codes.push(colCode);
	}
	public EditColumn(col: iReportColumn | null) {
		this.EditingColumn = col;
	}
	public MoveColumnLeft(EditingReport: iConfigurableReport, col: iReportColumn) {
		if (EditingReport == null) return;
		let toMoveInd = EditingReport.columns.findIndex(f => f.columnOrder == col.columnOrder - 1);
		if (toMoveInd == -1) {
			//bad luck, the oder values are malformed, let's fix them...
			this.FixOrderValuesInColumns(EditingReport);
			toMoveInd = EditingReport.columns.findIndex(f => f.columnOrder == col.columnOrder - 1);
			if (toMoveInd == -1) {
				//??? this shouldn't happen!
				return;
			}
		}
		const toMove = EditingReport.columns[toMoveInd];
		toMove.columnOrder++;
		col.columnOrder--;
		EditingReport.columns.splice(toMoveInd, 2, col, toMove);
		this.EditingReportHasChanged = true;
	}
	public MoveColumnRight(EditingReport: iConfigurableReport, col: iReportColumn) {
		if (EditingReport == null) return;
		let toMoveInd = EditingReport.columns.findIndex(f => f.columnOrder == col.columnOrder + 1);
		if (toMoveInd == -1) {
			//bad luck, the oder values are malformed, let's fix them...
			this.FixOrderValuesInColumns(EditingReport);
			toMoveInd = EditingReport.columns.findIndex(f => f.columnOrder == col.columnOrder + 1);
			if (toMoveInd == -1) {
				//??? this shouldn't happen!
				return;
			}
		}
		const toMove = EditingReport.columns[toMoveInd];
		toMove.columnOrder--;
		col.columnOrder++;
		EditingReport.columns.splice(toMoveInd -1, 2, toMove, col);
		this.EditingReportHasChanged = true;
	}
	public EditCode(code: iReportColumnCode | null) {
		this.EditingColumnCode = code;
	}
	public DeleteColumnCode(code: iReportColumnCode, col:iReportColumn) {
		const ind = col.codes.findIndex(f => f.reportColumnCodeId == code.reportColumnCodeId);
		if (ind != -1) {this.confirmationDialogService.confirm("Remove code from column?"
			, "Are you sure? This will remove the \"<strong>" + code.userDefText + "\"</strong> code from the <strong>\"" + col.name + "\"</strong> column."
			, false, '').then((res: any) => {
				if (res == true) {
					this.EditingReportHasChanged = true;
					col.codes.splice(ind, 1);
                }
			});
        }
	}
	public DeleteColumn(col: iReportColumn, EditingReport: iConfigurableReport) {
		const ind = EditingReport.columns.findIndex(f => f.reportColumnId == col.reportColumnId);
		if (ind != -1) {
			this.confirmationDialogService.confirm("Remove column from report?"
				, "Are you sure? This will remove the  entire \"<strong>" + col.name + "\"</strong> column from the report."
				, false, '').then((res: any) => {
					if (res == true) {
						this.EditingReportHasChanged = true;
						EditingReport.columns.splice(ind, 1);
					}
				});
		}
	}
	public Save() {
		if (!this.HasWriteRights || this.EditingReport == null) return;
		else {
			if (this.EditingReport.reportId == 0) {
				this.configurablereportServ.CreateReport(this.EditingReport);
			} else {
				this.configurablereportServ.UpdateReport(this.EditingReport);
			}
			this.CancelEditing();
		}
	}
	ngOnDestroy() {

		
	}
}
