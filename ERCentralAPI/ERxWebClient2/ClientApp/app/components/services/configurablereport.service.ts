import { Component, Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Helpers } from '../helpers/HelperMethods';

@Injectable({
    providedIn: 'root',
})

// need to change all this for the relevant business objects in ER4 for reports...
// Tuesday 22/10/2019
export class ConfigurableReportService extends BusyAwareService {


	FetchQuestionReport(Args: ReportExecuteCommandParams): any {


        throw new Error("Method not implemented.");
    }


	FetchAnswerReport(Args: ReportExecuteCommandParams): any {




		let reportHTML: string = '';
		
		//this.GenerateReportHTMLHere(res,
		//	chosenAttFilter, chosenSetFilter, comparison);

		Helpers.OpenInNewWindow(reportHTML, this._baseUrl);


    }

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
