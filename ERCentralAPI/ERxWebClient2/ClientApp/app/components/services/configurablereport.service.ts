import { Inject, Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { EventEmitterService } from './EventEmitter.service';
import { Subscription } from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class ConfigurableReportService extends BusyAwareService {

	constructor(
		private _httpC: HttpClient,
		private modalService: ModalService,
		private EventEmitterService: EventEmitterService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
		super();
		//console.log("On create DuplicatesService");
		this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
	}

	ngOnDestroy() {
		console.log("Destroy DuplicatesService");
		if (this.clearSub != null) this.clearSub.unsubscribe();
	}
	private clearSub: Subscription | null = null;
	public reportHTML: string = '';
	private _ReportList: iConfigurableReport[] = [];
	public get Reports(): iConfigurableReport[]  {
		return this._ReportList;
	}
	public get ReportCollectionROB(): iConfigurableReport[] | null {
		return this._ReportList.filter(x => x.reportType == 'Question');
	}
	public get ReportCollectionOutcomes(): iConfigurableReport[] | null {
		return this._ReportList.filter(x => x.reportType == 'Answer');
	}  
	public FetchReports() {

		this._BusyMethods.push("FetchReports");
		this._httpC.get<iConfigurableReport[]>(this._baseUrl + 'api/ReportList/FetchReports')
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
				return "error";
			}
		).catch(
			(error) => {
				console.log('error in FetchROBReport catch', error);
				this.modalService.GenericError(error);
				this.RemoveBusy("FetchROBReport");
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

	UpdateReport(rep: iConfigurableReport) {
		this._BusyMethods.push("UpdateReport");
		
		//console.log("saving reviewSet via command", rs, rsC);
		return this._httpC.post<iConfigurableReport>(this._baseUrl + 'api/ReportList/UpdateReport', rep).subscribe((res) => {
			let ind = this.Reports.findIndex(f => f.reportId == res.reportId)
			if (ind == -1) {
				this.Reports.push(res);
			}
			else {
				this.Reports.splice(ind, 1, res);
            }

			this.RemoveBusy("UpdateReport");
		},
			(err) => {
				console.log("Error Updating Report:", err);
				this.RemoveBusy("UpdateReport");
				this.modalService.GenericError(err);
			});
    }

	public Clear() {
		this.reportHTML = '';
		this._ReportList = [];
	}
	public static CloneReport(rep: iConfigurableReport): iConfigurableReport {
		let cols: iReportColumn[] = []
		for (let inCol of rep.columns) {
			let codes: iReportColumnCode[] = [];
			for (let cCode of inCol.codes) {
				let code: iReportColumnCode = {
					attributeId: cCode.attributeId,
					codeOrder: cCode.codeOrder,
					displayAdditionalText: cCode.displayAdditionalText,
					displayCode: cCode.displayCode,
					displayCodedText: cCode.displayCodedText,
					parentAttributeId: cCode.parentAttributeId,
					parentAttributeText: cCode.parentAttributeText,
					reportColumnCodeId: cCode.reportColumnCodeId,
					reportColumnId: cCode.reportColumnId,
					setId: cCode.setId,
					userDefText: cCode.userDefText,
				}
				codes.push(code);
			}
			let column: iReportColumn = {
				codes: codes,
				columnOrder: inCol.columnOrder,
				name: inCol.name,
				reportColumnId: inCol.reportColumnId
			}
			cols.push(column)
        }
		let res: iConfigurableReport = {
			name: rep.name,
			reportId: rep.reportId,
			contactId: rep.contactId,
			contactName: rep.contactName,
			isAnswer: rep.isAnswer,
			reportType: rep.reportType,
			//detail: string;
			columns: cols
		}
		return rep;
    }
}

export interface ReportResult
{
	returnReport: string;

}

export interface iConfigurableReport {
	name: string;
	reportId: number;
	contactId: number;
	contactName: string;
	isAnswer: boolean;
	reportType: string;
	//detail: string;
	columns: iReportColumn[];
}

export interface iReportColumn {
	codes: iReportColumnCode[];
	columnOrder: number;
	name: string;
	reportColumnId: number;
}
export interface iReportColumnCode {
	attributeId: number;
	codeOrder: number;
	displayAdditionalText: boolean;
	displayCode: boolean;
	displayCodedText: boolean;
	parentAttributeId: number;
	parentAttributeText: string;
	reportColumnCodeId: number;
	reportColumnId: number;
	setId: number;
	userDefText: string;
}

export class ReportStandard {

	items: string = '';
	showFullTitle: boolean = false;
	showAbstract: boolean = false;
	showYear: boolean = false;
	showShortTitle: boolean = true;
	reportId: number = 0;
	report: iConfigurableReport = {} as iConfigurableReport;
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
	report: iConfigurableReport = {} as iConfigurableReport;
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
	showShortTitle: boolean = true;
	reportId: number = 0;
	report: iConfigurableReport = {} as iConfigurableReport;
	showItemId: boolean = false;
	showOldID: boolean = false;
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