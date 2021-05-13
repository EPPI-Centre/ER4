import { Component, OnInit, OnDestroy, Inject, ViewChild } from '@angular/core';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { iConfigurableReport, ConfigurableReportService, ReportStandard, ReportOutcomes, ReportRiskOfBias, CommonReportFields } from '../services/configurablereport.service';
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
	public EditingReport: iConfigurableReport | null = null;
	public get Reports(): iConfigurableReport[] | null {
		return this.configurablereportServ.Reports;
    }
	public EditReport(rpt: iConfigurableReport) {
		this.EditingReport = ConfigurableReportService.CloneReport(rpt);
	}
	public CancelEditing() {
		this.EditingReport = null;
	}
	ngOnDestroy() {

		
	}
}
