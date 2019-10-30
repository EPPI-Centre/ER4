import {  Inject, Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Helpers } from '../helpers/HelperMethods';
import { ReviewSet } from './ReviewSets.service';

@Injectable({
    providedIn: 'root',
})
export class ConfigurableReportService extends BusyAwareService {

	constructor(
		private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
	}
	private reportHTML: string = '';
    private _ReportList: Report[] | null = [];
	public get Reports(): Report[] | null {
		return this._ReportList;
    }
  
	public FetchReports() {

		this._BusyMethods.push("FetchReports");
		this._httpC.get<Report[]>(this._baseUrl + 'api/ReportList/FetchReports')

			.subscribe(result => {

				this._ReportList = result;
				this.RemoveBusy("FetchReports");
				
			}, error => {
				this.RemoveBusy("FetchReports");
				this.modalService.GenericErrorMessage(error);
				
			}
		);
	}

	FetchQuestionReport(args: ReportQuestionExecuteCommandParams) {

		this._BusyMethods.push("FetchQuestionReport");
		this._httpC.post<string>(this._baseUrl + 'api/ReportList/FetchQuestionReport',
			args
		).toPromise()
			.then(
			(res: string) => {

				this.reportHTML = res;
				Helpers.OpenInNewWindow(this.reportHTML, this._baseUrl);
				this.RemoveBusy("FetchQuestionReport");
				}	
			); 
		this.RemoveBusy("FetchQuestionReport");
		
	}
	FetchAnswerReport(args: ReportAnswerExecuteCommandParams) {

		this._BusyMethods.push("FetchAnswerReport");
		this._httpC.post<ReportResult>(this._baseUrl
			+ 'api/ReportList/FetchAnswerReport', args
			)

			.subscribe(result => {
				let reportHTML: string = result.returnReport;
				Helpers.OpenInNewWindow(reportHTML, this._baseUrl);
				this.RemoveBusy("FetchAnswerReport");

			}, error => {
				this.RemoveBusy("FetchAnswerReport");
				this.modalService.GenericErrorMessage(error);
			}
		);
	}
}

export interface ReportResult
{
	returnReport: string;

}
export interface Report {

	name: string;
	reportId: number;
	contactId: number;
	reportType: string;

}
export interface ReportAnswerExecuteCommandParams {

	isQuestion: boolean;
	reportType: string;
	codes: string;
	reportId: number;
	showItemId: boolean;
	showOldItemId: boolean;
	showOutcomes: boolean;
	isHorizontal: boolean;
	orderBy: string;
	title: string;
	attributeId: number;
	setId: number;
	showRiskOfBias: boolean;
	showUncodedItems: boolean;
	showBullets: boolean;
	txtInfoTag: string;

}
export interface ReportQuestionExecuteCommandParams {

	isQuestion: boolean;
	items: string;
	reportId: number;
	orderBy: string;
	attributeId: number;
	setId: number;
	isHorizantal: boolean;
	showItemId: boolean;
	showOldID: boolean;
	showOutcomes: boolean;
	showFullTitle: boolean;
	showAbstract: boolean;
	showYear: boolean;
	showShortTitle: boolean;
	showRiskOfBias: boolean;
	showUncodedItems: boolean;
	showBullets: boolean;
	txtInfoTag: string;
	reviewSets: ReviewSet[]
}
