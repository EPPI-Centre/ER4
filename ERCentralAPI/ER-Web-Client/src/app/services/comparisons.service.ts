import {  Inject, Injectable, EventEmitter, Output, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewSet, SetAttribute } from './ReviewSets.service';
import { EventEmitterService } from './EventEmitter.service';
import { Helpers } from '../helpers/HelperMethods';
import { Subscription } from 'rxjs';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { ConfigService } from './config.service';


@Injectable({
    providedIn: 'root',
})

export class ComparisonsService extends BusyAwareService implements OnDestroy {

    @Output() ListLoaded = new EventEmitter();
    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
		private EventEmitterService: EventEmitterService,
      configService: ConfigService
    ) {
      super(configService);
		//console.log("On create ComparisonsService");
		this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
	}
	ngOnDestroy() {
		console.log("Destroy DuplicatesService");
		if (this.clearSub != null) this.clearSub.unsubscribe();
	}
	private clearSub: Subscription | null = null;
	
	private _Comparisons: Comparison[] = [];
	public currentComparison: Comparison = new Comparison();

	public get Comparisons(): Comparison[] {
		if (this._Comparisons) {
			return this._Comparisons;
		} else {
			return [];
		}
	}

	public set Comparisons(comparisons: Comparison[]) {
		if (comparisons) {
			this._Comparisons = comparisons;
		}
    }

	private _Statistics: ComparisonStatistics | null = null;

    public get Statistics(): ComparisonStatistics | null {
		return this._Statistics;		
	}

	public sort: SortDescriptor[] = [{
		field: 'shortTitle',
		dir: 'asc'
	}];
	FormatDateWithInputSlashes(DateSt: string): string {
		return Helpers.FormatDateWithInputSlashes(DateSt);
	}
	public sortChange(sort: SortDescriptor[]): void {
		this.sort = sort;
		this._Comparisons = orderBy(this._Comparisons, this.sort);
	}

	public FetchComparisonReport(comparisonId: number, ParentAttributeId: number, SetId: number, chosenParent: any)  : Promise<any>
	{
		
		this._BusyMethods.push("FetchComparisonReport");
		let comparison: Comparison = this._Comparisons.filter(x => x.comparisonId == comparisonId)[0];
		let compReport: ComparisonAttributeSelectionCriteria = new ComparisonAttributeSelectionCriteria();
		compReport.comparisonId = comparisonId;
		compReport.parentAttributeId = ParentAttributeId;
		compReport.setId = SetId;
		//compReport.comparison = comparison;
		let chosenSetFilter: ReviewSet = new ReviewSet();
		let chosenAttFilter: SetAttribute = new SetAttribute();
		if (ParentAttributeId == 0) {
            chosenSetFilter = chosenParent as ReviewSet;
		} else {
            chosenAttFilter = chosenParent as SetAttribute;
		}
		return this._httpC.post<any>(
			this._baseUrl + 'api/Comparisons/ComparisonReport',
			compReport
			)
			.toPromise().then(
				(res) => {
					
					//console.log(JSON.stringify(res));
					let reportHTML: string = this.GenerateReportHTMLHere(res,
						chosenAttFilter, chosenSetFilter, comparison);
					Helpers.OpenInNewWindow(reportHTML, this._baseUrl);
					this.RemoveBusy("FetchComparisonReport");
			},

			error => {
				this.RemoveBusy("FetchComparisonReport");
				this.modalService.SendBackHomeWithError(error);
			}
			);
	}

	private GenerateReportHTMLHere(comparisonAttributeList: ComparisonAttribute[] ,
		chosenFilter: SetAttribute  ,
		chosenSetFilter: ReviewSet ,
		comparison: Comparison 
	): string {

		let thirdReviewerIncluded: boolean = false;
		let report: string  = "<html><body><p><h3>Comparison report between: <i>" + comparison.contactName1 + "</i> and <i>" +
			comparison.contactName2 + "</i>";
		if (comparison.contactName3 != "") {
			report += " and <i>" + comparison.contactName3 + "</i>";
		}
		report += "</h3>";
		report += "<P>This report is based on the status of the database at the time the comparison was created. Any coding <b>completed</b> after the comparison was created will be displayed also in the Agreed column.</P>";
		if (chosenFilter.attribute_id != -1) {
			report += "<h4>" + chosenFilter.name + "</h4>";
		}
		if (chosenSetFilter.set_id != -1) {
			report += "<h4>" + chosenSetFilter.set_name + "</h4>";
		}
		report += "<table border='1'><tr><td>Id</td><td>Item</td><td>" + comparison.contactName1 + "</td><td>" + comparison.contactName2 + "</td>";
		if (comparison.contactName3 != "") {
			report += "<td>" + comparison.contactName3 + "</td>";
			thirdReviewerIncluded = true;
		}
		report += "<td><b>Agreed version</b></td></tr>";
		let list: ComparisonAttribute[] = comparisonAttributeList;
		let CurrentItemId: number = -1;
		let CurrentItem: string = "";
		let Reviewer1: string =  "";
		let Reviewer2: string =  "";
		let Reviewer3: string =  "";
		let Agreed: string = "";
		console.log('list length in report: ' + list.length);
		for(let item of list)
		{
			if (item.itemId != CurrentItemId) {
				if (CurrentItemId != -1) {
					report += "<tr><td valign='top'>" + CurrentItemId + "</td><td valign='top'>" + CurrentItem + "</td>";
					report += "<td valign='top'>" + Reviewer1 + "</td>";
					report += "<td valign='top'>" + Reviewer2 + "</td>";
					if (thirdReviewerIncluded == true) {
						report += "<td valign='top'>" + Reviewer3 + "</td>";
					}
					report += "<td valign='top'>" + Agreed + "</td>";
					report += "</tr>";
				}
				Reviewer1 = "";
				Reviewer2 = "";
				Reviewer3 = "";
				Agreed = "";
				CurrentItemId = item.itemId;
				CurrentItem = item.itemTitle;
			}
			if ((item.contactId == comparison.contactId1) && (item.isCompleted == false)) {
				if (Reviewer1 == "")
					Reviewer1 = item.attributeNameWithArm4HTML;
				else
					Reviewer1 += "<br><br>" + item.attributeNameWithArm4HTML;
				if (item.additionalText != "")
					Reviewer1 += "<br><i> " + item.additionalText + "</i>";
			}
			else {
				if ((item.contactId == comparison.contactId2) && (item.isCompleted == false)) {
					if (Reviewer2 == "")
						Reviewer2 = item.attributeNameWithArm4HTML;
					else
						Reviewer2 += "<br><br>" + item.attributeNameWithArm4HTML;
					if (item.additionalText != "")
						Reviewer2 += "<br><i> " + item.additionalText + "</i>";
				}
				else {
					if ((item.contactId == comparison.contactId3) && (item.isCompleted == false)) {
						if (Reviewer3 == "")
							Reviewer3 = item.attributeNameWithArm4HTML;
						else
							Reviewer3 += "<br><br>" + item.attributeNameWithArm4HTML;
						if (item.additionalText != "")
							Reviewer3 += "<br><i> " + item.additionalText + "</i>";
					}
					else {
						if (item.isCompleted == true) {
							if (Agreed == "")
								Agreed = "<b>" + item.attributeNameWithArm4HTML + "</b>";
							else
                                Agreed += "<br><br><b>" + item.attributeNameWithArm4HTML + "</b>";
							if (item.additionalText != "")
								Agreed += "<br><i> " + item.additionalText + "</i>";
						}
					}
				}
			}
		}
		report += "<tr><td valign='top'>" + CurrentItemId + "</td><td valign='top'>" + CurrentItem + "</td>";
		report += "<td valign='top'>" + Reviewer1 + "</td>";
		report += "<td valign='top'>" + Reviewer2 + "</td>";
		if (thirdReviewerIncluded == true) {
			report += "<td valign='top'>" + Reviewer3 + "</td>";
		}
		report += "<td valign='top'>" + Agreed + "</td>";
		report += "</tr>";
		report += "</table></p>";
		return report;
	}

	private AddHTMLFrame(report: string): string {
		let res = "<HTML id='content'><HEAD><title>EPPI-Reviewer Comparison Report</title><link rel='stylesheet' href='" + this._baseUrl + "styles.css' /></HEAD><BODY class='m-2' id='body'>" + report;
		//res += "<br /><a download='report.html' href='data:text/html;charset=utf-8," + report + "'>Save...</a></BODY></HTML>";
		//res += "<br />" + this.AddSaveMe() + "</BODY></HTML>";
		res += "</BODY></HTML>";
		return res;
	}
	

    public FetchAll() {
        this._BusyMethods.push("FetchAll");
		this._httpC.get<Comparison[]>(this._baseUrl + 'api/Comparisons/ComparisonList')
			.subscribe(result => {
				this._Comparisons = result;
                this.RemoveBusy("FetchAll");
            }, error => {
                this.RemoveBusy("FetchAll");
                this.modalService.SendBackHomeWithError(error);
            }
        );
	}

	public  FetchStats(ComparisonId: number ) {
        this._BusyMethods.push("FetchStats");
		let body = JSON.stringify({ Value: ComparisonId });
		 this._httpC.post<iComparisonStatistics>(this._baseUrl + 'api/Comparisons/ComparisonStats', body)
			.subscribe(result => {
                this._Statistics = new ComparisonStatistics(result, ComparisonId);
				this.currentComparison = this.Comparisons.filter(x => x.comparisonId == ComparisonId)[0];//consider a get
				
                this.RemoveBusy("FetchStats");
			 },
				 error => {
					 this.RemoveBusy("FetchStats");
					 this.modalService.GenericError(error);
				 }
             );
	}

	public CreateComparison(comparison: Comparison) {

        this._BusyMethods.push("CreateComparison");
		//console.log('inside the service now' + JSON.stringify(comparison));
		this._httpC.post<Comparison>(this._baseUrl +
			'api/Comparisons/CreateComparison', comparison)
			.subscribe(() => {

				this.FetchAll();
				this.RemoveBusy("CreateComparison");

			},
				error => {
					this.modalService.GenericError(error);
					this.RemoveBusy("CreateComparison");
				}
				, () => {
					this.RemoveBusy("CreateComparison");
				}
			);
	}

	public CompleteComparison(completeComparison: iCompleteComparison) {

		this._BusyMethods.push("CompleteComparison");

		//let body = JSON.stringify({ Value: ComparisonId });
		return this._httpC.post<string>(this._baseUrl +
			'api/Comparisons/CompleteComparison', completeComparison)
			.toPromise().then(
			(result: string) => {
                this.RemoveBusy("CompleteComparison");
                this.FetchStats(completeComparison.comparisonId);
				return result;
			},
			error => {
                this.modalService.GenericError(error);
                this.FetchStats(completeComparison.comparisonId);
				this.RemoveBusy("CompleteComparison");
			}
		);
	}
		
	public DeleteComparison(ComparisonId: number) {
        this._BusyMethods.push("DeleteComparison");
		let body = JSON.stringify({ Value: ComparisonId });

		this._httpC.post<Comparison>(this._baseUrl + 'api/Comparisons/DeleteComparison', body)
			.subscribe(() => {
                this.RemoveBusy("DeleteComparison");
				this.FetchAll();
			},
            error => {
                this.RemoveBusy("DeleteComparison");
                this.modalService.SendBackHomeWithError(error);
            }
		 );
	}

    public Clear() {
        //clear current stats details AS Well!
		this._Comparisons = [];
		this.currentComparison = new Comparison();
		this._Statistics = null;
	}

}

export class Comparison {

	public comparisonId: number = 0;
	public isScreening: boolean = false;
	public reviewId: number = 0;
	public inGroupAttributeId: number = -1;
	public setId: number = 0;
	public comparisonDate: string = "";
	public contactId1: number = 0;
	public contactId2: number = 0;
	public contactId3: number = -1;
	public contactName1: string = '';
	public contactName2: string = '';
	public contactName3: string = '';
	public attributeName: string = '';
	public setName: string = '';

}

export class ComparisonCopy {

	public comparisonId: number = 0;
	public isScreening: boolean = false;
	public reviewId: number = 0;
	public inGroupAttributeId: number = -1;
	public setId: number = 0;
	public comparisonDate: Date = new Date();
	public contactId1: number = 0;
	public contactId2: number = 0;
	public contactId3: number = -1;
	public contactName1: string = '';
	public contactName2: string = '';
	public contactName3: string = '';
	public attributeName: string = '';
	public setName: string = '';

}

export class ComparisonStatistics {
	public constructor(data: iComparisonStatistics, comparisonID: number) {
        this.RawStats = data;
        this.comparisonID = comparisonID;
    }
    public RawStats: iComparisonStatistics;
    public comparisonID: number;
    public get Agreements1vs2(): number {
		
        return this.RawStats.n1vs2 - this.RawStats.disagreements1vs2;
    };
    public get Agreements1vs3(): number {
        return this.RawStats.n1vs3 - this.RawStats.disagreements1vs3;
    };
    public get Agreements2vs3(): number {
        return this.RawStats.n2vs3 - this.RawStats.disagreements2vs3;
	};
    public get SCAgreements1vs2(): number {
		
		return this.RawStats.n1vs2 - this.RawStats.scDisagreements1vs2;
	};
    public get SCAgreements1vs3(): number {
		return this.RawStats.n1vs3 - this.RawStats.scDisagreements1vs3;
	};
    public get SCAgreements2vs3(): number {
		return this.RawStats.n2vs3 - this.RawStats.scDisagreements2vs3;
	};
}

export class ComparisonAttributeSelectionCriteria {
	comparisonId: number = 0;
	parentAttributeId: number = 0;
	setId: number = 0;
	//comparison: any = null;
}

export interface iComparisonStatistics {
    comparisonId: number;
    n1vs2: number;
    n2vs3: number;
    n1vs3: number;
    disagreements1vs2: number;
    disagreements2vs3: number;
    disagreements1vs3: number;
    ncoded1: number;
    ncoded2: number;
    ncoded3: number;
    canComplete1vs2: boolean;
    canComplete1vs3: boolean;
    canComplete2vs3: boolean;
    scDisagreements1vs2: number;
    scDisagreements2vs3: number;
    scDisagreements1vs3: number;
    isScreening: boolean;
}

export interface iCompleteComparison {

	comparisonId: number;
	whichReviewers: string;
	contactId: number;
    lockCoding: string;
}

export class ComparisonAttribute {

	comparisonAttributeId: number = 0;
	comparisonId: number = 0;
	itemId: number = 0;
	attributeId: number = 0;
	additionalText: string = '';
	contactId: number = 0;
	setId: number = 0;
	attributeName: string = '';
	attributeNameWithArm4HTML: string = '';
	itemTitle: string = '';
	itemArm: string = '';
	isCompleted: boolean = false;

}
