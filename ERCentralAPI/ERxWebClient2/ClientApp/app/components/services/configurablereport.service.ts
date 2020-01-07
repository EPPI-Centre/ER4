import { Inject, Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';

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
	FetchStandardReport(args: ReportStandard): Promise<string> {


		this._BusyMethods.push("FetchStandardReport");
		return this._httpC.post<string>(this._baseUrl + 'api/ReportList/FetchStandardReport',
			args
		).toPromise().then(
			(result) => {
				this.RemoveBusy("FetchStandardReport");
				console.log("FetchStandardReport got:", result);
				return result;

			}, error => {
				this.RemoveBusy("FetchStandardReport");
				this.modalService.GenericError(error);
				return "error";
			}
		).catch(
			(error) => {
				console.log('error in FetchStandardReport catch', error);
				this.modalService.GenericError(error);
				this.RemoveBusy("FetchStandardReport");
				return "error";
			}
		);
	}
	FetchROBReport(args: ReportRiskOfBias): Promise<string>   {

		this._BusyMethods.push("FetchROBReport");
		return this._httpC.post<string>(this._baseUrl + 'api/ReportList/FetchROBReport',
			args
		).toPromise().then(
			(result) => {
				this.RemoveBusy("FetchROBReport");
				return result;
			}, error => {
				this.RemoveBusy("FetchROBReport");
				this.modalService.GenericError(error);
				// I do not understand below line
				return "error";
			}
		).catch(
			(error) => {
				console.log('error in FetchROBReport catch', error);
				this.modalService.GenericError(error);
				this.RemoveBusy("FetchROBReport");
				// I do not understand below line
				return "error";
			}
		);
	}
	FetchOutcomesReport(args: ReportOutcomes): Promise<ReportResult>   {

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
                    this.modalService.GenericError(error);
                    return {
                        returnReport : 'error'
                    };
					}
		).catch(
			(error) => {
				console.log('error in FetchOutcomesReport catch', error);
				this.modalService.GenericError(error);
				this.RemoveBusy("FetchOutcomesReport");
                return {
                    returnReport: 'error'
                };
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

export class ReportStandard {

	items: string = '';
	showFullTitle: boolean = false;
	showAbstract: boolean = false;
	showYear: boolean = false;
	showShortTitle: boolean = true;
	reportId: number = 0;
	report: Report = {} as Report;
	showItemId: boolean = false;
	showOldItemId: boolean = false;
	showOutcomes: boolean = false;
	isHorizontal: boolean = true;
	orderBy: string = '';
	isQuestion: boolean = false;
	attributeId: number = 0;
	setId: number = 0;
	showRiskOfBias: boolean = false;
	showUncodedItems: boolean = true;
	showBullets: boolean = false;
    txtInfoTag: string = '[info]';
}

export class ReportOutcomes {

	reportType: string = '';
	codes: string = '';
	reportId: number = 0;
	report: Report = {} as Report;
	showItemId: boolean = false; 
	showOldItemId: boolean = false; 
	showOutcomes: boolean = false; 
	isHorizontal: string = '';
	orderBy: string = '';
	title: string = '';
	attributeId: number = 0;
	setId: number = 0;

}

export class ReportRiskOfBias {

	items: string = '';
	showFullTitle: boolean = false;
	showAbstract: boolean = false;
	showYear: boolean = false;
	showShortTitle: boolean = false;
	reportId: number = 0;
	report: Report = {} as Report;
	showItemId: boolean = false;
	showOldID: boolean = true;
	showOutcomes: boolean = false;
	isHorizontal: boolean = false;
	orderBy: string = '';
	isQuestion: boolean = false;
	attributeId: number = 0;
	setId: number = 0;
	showRiskOfBias: boolean = false;
	showUncodedItems: boolean = false;
	showBullets: boolean = false;
	txtInfoTag: string = '';

}

export class CommonReportFields {

	orderByChoice: string = 'Short title';
	itemsChoice: string = 'All selected items';

}