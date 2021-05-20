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
		private _confirmationDialogService: ConfirmationDialogService
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
	public EditingReportHasChanged: boolean = false;
	public get Reports(): iConfigurableReport[] {
		return this.configurablereportServ.Reports;
    }
	public EditReport(rpt: iConfigurableReport) {
		this.EditingReport = ConfigurableReportService.CloneReport(rpt);
	}
	public CancelEditing() {
		this.EditingReport = null;
		this.EditingReportHasChanged = false;
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
	public Save() {
		if (!this.HasWriteRights || this.EditingReport == null) return;
		else {
			this.configurablereportServ.UpdateReport(this.EditingReport);
			this.CancelEditing();
		}
	}
	ngOnDestroy() {

		
	}
}
