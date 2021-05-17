import { Component, OnInit, OnDestroy, Inject, ViewChild } from '@angular/core';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet, SetAttribute, singleNode } from '../services/ReviewSets.service';
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
		private _confirmationDialogService: ConfirmationDialogService
	) { }

	ngOnInit() {
		if (this.configurablereportServ.Reports == null || this.configurablereportServ.Reports.length == 0)
			this.configurablereportServ.FetchReports();
	}
	public get HasWriteRights(): boolean {
		return this.ReviewerIdentityServ.HasWriteRights;
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
