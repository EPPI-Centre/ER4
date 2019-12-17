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
	public get ReportCollectionROB(): Report[] | null {

		return this._ReportList.filter(x => x.reportType == 'Question');
			
	}
	public get ReportCollectionOutcomes(): Report[] | null {

		return this._ReportList.filter(x => x.reportType == 'Answer');

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
		).catch(
			(error) => {
				console.log('error in FetchQuestionReport catch', error);
				this.modalService.GenericErrorMessage(error);
				this.RemoveBusy("FetchQuestionReport");
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
					
						return result;
				}, error => {
                    this.RemoveBusy("FetchOutcomesReport");
						this.modalService.GenericErrorMessage(error);
						return error;
					}
		).catch(
			(error) => {
				console.log('error in FetchOutcomesReport catch', error);
				this.modalService.GenericErrorMessage(error);
				this.RemoveBusy("FetchOutcomesReport");
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
