import {  Inject, Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Helpers } from '../helpers/HelperMethods';

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
    private _ReportList: Report[] | null = [];
	public get Reports(): Report[] | null {
		return this._ReportList;
    }
  
	public FetchReports() {

		this._BusyMethods.push("FetchReports");
		this._httpC.get<Report[]>(this._baseUrl + 'api/ReportList/FetchReports')

			.subscribe(result => {
				console.log(result);
				this._ReportList = result;
				this.RemoveBusy("FetchReports");
				
			}, error => {
				this.RemoveBusy("FetchReports");
				this.modalService.GenericErrorMessage(error);
				
			}
		);
	}

	FetchQuestionReport(Args: ReportExecuteCommandParams): any {


		this._BusyMethods.push("FetchQuestionReport");
		let body = JSON.stringify({ Args });
		this._httpC.post<string>(this._baseUrl + 'api/ReportList/FetchQuestionReport',
		body)

			.subscribe(result => {

				console.log(result);

				this.RemoveBusy("FetchQuestionReport");

			}, error => {
				this.RemoveBusy("FetchQuestionReport");
				this.modalService.GenericErrorMessage(error);

			}
		);
	}

	FetchAnswerReport(Args: ReportExecuteCommandParams): any {

		this._BusyMethods.push("FetchAnswerReport");
		let body = JSON.stringify({ Args});
		this._httpC.post<string>(this._baseUrl + 'api/ReportList/FetchAnswerReport',
		body)

			.subscribe(result => {

				let reportHTML: string = result;

				//this.GenerateReportHTMLHere(res,
				//	chosenAttFilter, chosenSetFilter, comparison);

				Helpers.OpenInNewWindow(reportHTML, this._baseUrl);
				this.RemoveBusy("FetchAnswerReport");

			}, error => {
				this.RemoveBusy("FetchAnswerReport");
				this.modalService.GenericErrorMessage(error);

			}
			);
	}
}
export interface Report {

	name: string;
	reportId: number;
	contactId: number;
	reportType: string;

}
export interface ReportExecuteCommandParams {
	
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

}
