import {  Inject, Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
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
	public reportHTML: string = '';
    private _ReportList: Report[] = [];
	public get Reports(): Report[] | null {
		return this._ReportList;
    }
  
	public FetchReports(tabIndex: any | null) {

		this._BusyMethods.push("FetchReports");
		this._httpC.get<Report[]>(this._baseUrl + 'api/ReportList/FetchReports')
			.subscribe(result => {



				if (tabIndex == 1) {
					this._ReportList = result.filter(x => x.reportType == 'Question');
				} else if (tabIndex == 2) {
					this._ReportList = result.filter(x => x.reportType == 'Answer');
				}
				else {
					this._ReportList = result;
				}

				//let emptyReport: Report = {} as Report;
				//this._ReportList.push(emptyReport);

				this.RemoveBusy("FetchReports");
			}, error => {
				this.RemoveBusy("FetchReports");
				this.modalService.GenericErrorMessage(error);
			}
		);
	}
	FetchQuestionReport(args: ReportQuestionExecuteCommandParams): Promise<string>   {
		this._BusyMethods.push("FetchQuestionReport");
		return this._httpC.post<string>(this._baseUrl + 'api/ReportList/FetchQuestionReport',
			args
		).toPromise().then(
			(result) => {
				this.RemoveBusy("FetchQuestionReport");
				return result;
			}, error => {
				this.RemoveBusy("FetchQuestionReport");
				this.modalService.GenericErrorMessage(error);
				return error;
			}
		);
	}
    FetchOutcomesReport(args: ReportAnswerExecuteCommandParams): Promise<ReportResult>   {
        this._BusyMethods.push("FetchOutcomesReport");
		return this._httpC.post<ReportResult>(this._baseUrl
            + 'api/ReportList/FetchOutcomesReport', args
			)
			.toPromise().then(
				(result) => {
					this.RemoveBusy("FetchOutcomesReport");
					console.log('service report', result); 
						return result;
				}, error => {
                    this.RemoveBusy("FetchOutcomesReport");
						this.modalService.GenericErrorMessage(error);
						return error;
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
	isHorizontal: boolean;
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
