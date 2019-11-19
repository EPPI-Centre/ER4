import {  Inject, Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Helpers } from '../helpers/HelperMethods';
import { ReviewSet } from './ReviewSets.service';
import { Observable, Subscription } from 'rxjs';

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
	public reportHTML: string = '';
    private _ReportList: Report[] = [];
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

	FetchQuestionReport(args: ReportQuestionExecuteCommandParams): Observable<ReportResult>   {

		let res: ReportResult = {} as ReportResult;
		this._BusyMethods.push("FetchQuestionReport");
		return this._httpC.post<string>(this._baseUrl + 'api/ReportList/FetchQuestionReport',
			args
		).map(
				(result: string) => {
					//this.reportHTML = res;
					//Helpers.OpenInNewWindow(this.reportHTML, this._baseUrl);
					this.RemoveBusy("FetchQuestionReport");
					//console.log(result);
					res.returnReport = result;
					return res;
				}
			); 
	}
	FetchAnswerReport(args: ReportAnswerExecuteCommandParams): Observable<ReportResult>   {

		this._BusyMethods.push("FetchAnswerReport");
		let res: ReportResult = {} as ReportResult;

		return this._httpC.post<string>(this._baseUrl
			+ 'api/ReportList/FetchAnswerReport', args
			)
			.map(result => {
				//let reportHTML: string = result.returnReport;
				//Helpers.OpenInNewWindow(reportHTML, this._baseUrl);
				//console.log(result);
				this.RemoveBusy("FetchAnswerReport");
				res.returnReport = result;
				return res;

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
