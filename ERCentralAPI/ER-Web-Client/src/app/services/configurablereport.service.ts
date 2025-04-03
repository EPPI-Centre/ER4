import { Inject, Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { EventEmitterService } from './EventEmitter.service';
import { lastValueFrom, Subscription } from 'rxjs';
import { ConfigService } from './config.service';
import { iOutcome } from './outcomes.service';

@Injectable({
    providedIn: 'root',
})
export class ConfigurableReportService extends BusyAwareService {

	constructor(
		private _httpC: HttpClient,
		private modalService: ModalService,
		private EventEmitterService: EventEmitterService,
    configService: ConfigService
    ) {
    super(configService);
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

  public reportAllCodingCommandOptions: ReportAllCodingCommandOptions = new ReportAllCodingCommandOptions();

	public FetchReports() {
		this._BusyMethods.push("FetchReports");
    lastValueFrom(this._httpC.get<iConfigurableReport[]>(this._baseUrl + 'api/ReportList/FetchReports')).then
			(result => {

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
    return lastValueFrom(this._httpC.post<string>(this._baseUrl + 'api/ReportList/FetchStandardReport',
			args
		)).then(
			(result) => {
				this.RemoveBusy("FetchStandardReport");
				//console.log("FetchStandardReport got:", result);
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
    return lastValueFrom(this._httpC.post<string>(this._baseUrl + 'api/ReportList/FetchROBReport',
			args
		)).then(
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
    return lastValueFrom(this._httpC.post<ReportResult>(this._baseUrl
            + 'api/ReportList/FetchOutcomesReport', args
			)
			).then(
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

	FetchAllCodingReportBySet(setId: number): Promise<string> {

		let body = JSON.stringify({ Value: setId });
		this._BusyMethods.push("FetchAllCodingReportBySet");
    return lastValueFrom(this._httpC.post<string>(this._baseUrl + 'api/ReportList/FetchReportAllCoding',
			body
		)).then(
			(result) => {
				this.RemoveBusy("FetchAllCodingReportBySet");
				//console.log("FetchStandardReport got:", result);
				return result;

			}, error => {
				console.log('error in FetchAllCodingReportBySet error:', error);
				this.RemoveBusy("FetchAllCodingReportBySet");
				this.modalService.GenericError(error);
				return "error";
			}
		).catch(
			(error) => {
				console.log('error in FetchAllCodingReportBySet catch:', error);
				this.modalService.GenericError(error);
				this.RemoveBusy("FetchAllCodingReportBySet");
				return "error";
			}
		);
    }
  FetchAllCodingReportDataBySet(setId: number): Promise<iReportAllCodingCommand | boolean> {

    let body = JSON.stringify({ Value: setId });
    this._BusyMethods.push("FetchReportAllCodingData");
    return lastValueFrom(this._httpC.post<iReportAllCodingCommand>(this._baseUrl + 'api/ReportList/FetchReportAllCodingData',
      body
    )).then(
      (result) => {
        this.RemoveBusy("FetchReportAllCodingData");
        //console.log("FetchStandardReport got:", result);
        return result;

      }, error => {
        console.log('error in FetchAllCodingReportBySet error:', error);
        this.RemoveBusy("FetchReportAllCodingData");
        this.modalService.GenericError(error);
        return false;
      }
    ).catch(
      (error) => {
        console.log('error in FetchAllCodingReportBySet catch:', error);
        this.modalService.GenericError(error);
        this.RemoveBusy("FetchReportAllCodingData");
        return false;
      }
    );
  }
	CreateReport(rep: iConfigurableReport): Promise<iConfigurableReport | null> {
		this._BusyMethods.push("CreateReport");

		//console.log("saving reviewSet via command", rs, rsC);
    return lastValueFrom(this._httpC.post<iConfigurableReport>(this._baseUrl + 'api/ReportList/CreateReport', rep)).then((res: iConfigurableReport) => {
			this.Reports.push(res);			
			this.RemoveBusy("CreateReport");
			return res;
		},
			(err) => {
				console.log("Error Creating Report:", err);
				this.RemoveBusy("CreateReport");
				this.modalService.GenericError(err);
				return null;
			});
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
	DeleteReport(rep: iConfigurableReport) {
		if (rep.reportId < 1) return;
		let body = JSON.stringify({ Value: rep.reportId });
		this._BusyMethods.push("DeleteReport");
		//console.log("saving reviewSet via command", rs, rsC);
		return this._httpC.post<void>(this._baseUrl + 'api/ReportList/DeleteReport', body).subscribe(() => {
			let ind = this.Reports.findIndex(f => f.reportId == rep.reportId)
			if (ind !== -1) {
				this.Reports.splice(ind, 1);
			}
			this.RemoveBusy("DeleteReport");
		},
			(err) => {
				console.log("Error deleting Report:", err);
				this.RemoveBusy("DeleteReport");
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
		return res;
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

export interface iReportAllCodingCommand {
  attributes: iMiniAtt[];
  items: iMiniItem[];
}
export interface iMiniAtt {
  attId: number;
  attName: string;
  fullPath: string;
  isInReport: boolean;
}
export interface iMiniCoding {
  itemAttId: number;
  contactName: string;
  contactId: number;
  armName: string;
  isComplete: boolean;
  infoBox: string;
  pdf: iMiniPDFatt[];
}
export interface iMiniPDFatt {
  docName: string;
  page: number;
  text: string;
}
export interface iMiniItem {
  itemId: number;
  title: string;
  shortTitle: string;
  state: string;
  codingsList: iCodingByAttribute[];
  outcomesLists: iMaxiOutcomeKvp[];
}
export interface iCodingByAttribute {
  key: iMiniAtt;
  value: iMiniCoding[];
}
export interface iMaxiOutcomeKvp {
  key: number;
  value: iMaxiOutcome[];
}
export interface iMaxiOutcome {
  isComplete: boolean;
  contactName: string;
  outcome: iOutcome;
}
export class ReportAllCodingCommandOptions {
  private _labelForCompletedCoding: string = "1";
  private _labelForIncompleteCoding: string = "0";
  private _labelForNoCoding: string = "";
  private _saveLabelsAsNumbers: boolean = true;
  public get labelForCompletedCoding(): string {
    if (this.UseOnlyColourCodingForCompletion) {
      return "1";
    }
    else return this._labelForCompletedCoding;
  }
  public set labelForCompletedCoding(val: string) {
    this._labelForCompletedCoding = val;
  }
  public get labelForIncompleteCoding(): string {
    if (this.UseOnlyColourCodingForCompletion) {
      return "1";
    }
    else return this._labelForIncompleteCoding;
  }
  public set labelForIncompleteCoding(val: string) {
    this._labelForIncompleteCoding = val;
  }
  public get labelForNoCoding(): string {
    if (this.UseOnlyColourCodingForCompletion) {
      return "0";
    }
    else return this._labelForNoCoding;
  }
  public set labelForNoCoding(val: string) {
    this._labelForNoCoding = val;
  }
  public get DisableLabels(): boolean {
    if (this.UseOnlyColourCodingForCompletion) return true;
    else return false;
  }
  public get saveLabelsAsNumbers(): boolean {
    if (this.LabelsAreNumbers) {
      return this._saveLabelsAsNumbers;
    }
    else return false;
  }
  public set saveLabelsAsNumbers(val: boolean) {
    this._saveLabelsAsNumbers = val;
  }
  public get LabelsAreNumbers(): boolean {
    //console.log(this.isNumeric(this.labelForCompletedCoding), this.isNumeric(this.labelForIncompleteCoding), this.isNumeric(this.labelForNoCoding));
    if (!this.isNumeric(this.labelForCompletedCoding) && this.labelForCompletedCoding != "") return false;
    if (!this.isNumeric(this.labelForIncompleteCoding) && this.labelForIncompleteCoding != "") return false;
    if (!this.isNumeric(this.labelForNoCoding) && this.labelForNoCoding != "") return false;
    return true;
  }
  private isNumeric(str: string) {
    //adapted from https://stackoverflow.com/a/175787
    if (typeof str != "string") return false // we only process strings!  
    return !isNaN(+str) && // use type coercion to parse the _entirety_ of the string (`parseFloat` alone does not do this)...
      !isNaN(parseFloat(str)) // ...and ensure strings of whitespace fail
  }
  public linesSeparator: string = "------";
  UseOnlyColourCodingForCompletion: boolean = true;
  showFullTitle: boolean = false;
  includeArms: boolean = false;
  includeOutcomes: boolean = false;
}
