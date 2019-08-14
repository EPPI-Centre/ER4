import { Inject, Injectable, EventEmitter, Output, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { SetAttribute } from './ReviewSets.service';
import { iTimePoint } from './timePoints.service';

@Injectable({
    providedIn: 'root',
})

export class OutcomesService extends BusyAwareService  {

    constructor(
		private _http: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }

	private _currentItemSetId: number = 0;
	private _Outcomes: Outcome[] = [];
	//public Outcomes: Outcome[] = [];
	public ItemSetId: number = 0;
	public ShowOutComeList: EventEmitter<SetAttribute> = new EventEmitter();

	public get outcomesList(): Outcome[] {

        if (this._Outcomes) return this._Outcomes;
        else {
            this._Outcomes = [];
            return this._Outcomes;
		}

	}

	public set outcomesList(Outcomes: Outcome[]) {
        this._Outcomes = Outcomes;
    }

	@Output() outcomesChangedEE = new EventEmitter();

	public ReviewSetOutcomeList: ReviewSetDropDownResult[] = [];
	public ReviewSetControlList: ReviewSetDropDownResult[] = [];
	public ReviewSetInterventionList: ReviewSetDropDownResult[] = [];
	public ReviewSetItemArmList: ItemArm[] = [];

	public IsServiceBusy(): boolean {

		if (this._BusyMethods.length > 0) {
			return true;
		} else {
			return false;
		}
	}

	// this is in draft stage still!!!!!!!!!!!!!!!!!!
    public FetchOutcomes(ItemSetId: number): Outcome[] {

		this._BusyMethods.push("FetchOutcomes");
		let body = JSON.stringify({ Value: ItemSetId });

        this._http.post<iOutcomeList>(this._baseUrl + 'api/OutcomeList/Fetch', body).subscribe(result => {
            //console.log("count of outcomes is:", this._Outcomes.length);
            //console.log('can see the new outcome in here: ' + JSON.stringify(result));
            for (let iO of result.outcomesList) {
                //console.log("iO is:", iO);
                let RealOutcome: Outcome = new Outcome(iO);
                this._Outcomes.push(RealOutcome);
                //console.log("count of outcomes is:", this._Outcomes.length);
            }
            //console.log("count of outcomes is:", this._Outcomes.length);
            this.RemoveBusy("FetchOutcomes");

        }, error => {

            this.modalService.SendBackHomeWithError(error);
            this.RemoveBusy("FetchOutcomes");
            return error;
        }
		);
		return this._Outcomes;
	}

	public FetchReviewSetOutcomeList(itemSetId: number, setId: number) {

		this._BusyMethods.push("FetchReviewSetOutcomeList");
		let body = JSON.stringify({ itemSetId: itemSetId, setId: setId });

		this._http.post<ReviewSetDropDownResult[]>(this._baseUrl + 'api/OutcomeList/FetchReviewSetOutcomeList',
			body)
			.subscribe(result => {

				this.ReviewSetOutcomeList = result;
				
				//this.outcomesChangedEE.emit(this.outcomesList);
				this.RemoveBusy("FetchReviewSetOutcomeList");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchReviewSetOutcomeList");
		}
		);

	}

	public FetchReviewSetInterventionList(itemSetId: number, setId: number) {

		this._BusyMethods.push("FetchReviewSetInterventionList");
		let body = JSON.stringify({ itemSetId: itemSetId, setId: setId });

		this._http.post<ReviewSetDropDownResult[]>(this._baseUrl + 'api/OutcomeList/FetchReviewSetInterventionList',
			body)
			.subscribe(result => {

				this.ReviewSetInterventionList = result;
			
				this.RemoveBusy("FetchReviewSetInterventionList");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchReviewSetInterventionList");
			}
			);

	}

	public FetchReviewSetControlList(itemSetId: number, setId: number) {

		this._BusyMethods.push("FetchReviewSetControlList");
		let body = JSON.stringify({ itemSetId: itemSetId, setId: setId });

		this._http.post<ReviewSetDropDownResult[]>(this._baseUrl + 'api/OutcomeList/FetchReviewSetControlList',
			body)
			.subscribe(result => {

				this.ReviewSetControlList = result;
				
				this.RemoveBusy("FetchReviewSetControlList");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchReviewSetControlList");
			}
			);


	}

	public FetchItemArmList(itemId: number) {

		this._BusyMethods.push("FetchItemArmList");
		let body = JSON.stringify({ Value: itemId});

		this._http.post<ItemArm[]>(this._baseUrl + 'api/OutcomeList/FetchItemArmList',
			body)
			.subscribe(result => {

				this.ReviewSetItemArmList = result;
				
				this.RemoveBusy("FetchItemArmList");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchItemArmList");
			}
			);
	}

	public listOutcomes: Outcome[] = [];
	public Createoutcome(currentoutcome: Outcome): Promise<Outcome> {

		this._BusyMethods.push("CreateOutcome");
		let ErrMsg = "Something went wrong when creating an outcome. \r\n If the problem persists, please contact EPPISupport.";

		return this._http.post<Outcome>(this._baseUrl + 'api/OutcomeList/Createoutcome',

			currentoutcome).toPromise()
						.then(
						(result) => {

							this.outcomesList.push(result);
							
							if (!result) this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("CreateOutcome");
							
						}
						, (error) => {

							this.FetchOutcomes(this._currentItemSetId);		
							this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("CreateOutcome");
							return error;
						}
					)
					.catch(
						(error) => {
							this.FetchOutcomes(this._currentItemSetId);		
							this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("CreateOutcome");
							return error;
					}
		);
	}
	
	public Updateoutcome(currentOutcome: Outcome) {

		this._BusyMethods.push("UpdateOutcome");
		let ErrMsg = "Something went wrong when updating an outcome. \r\n If the problem persists, please contact EPPISupport.";

		this._http.post<Outcome[]>(this._baseUrl + 'api/OutcomeList/UpdateOutcome',

			currentOutcome).subscribe(

				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.FetchOutcomes(this._currentItemSetId);	
					this.RemoveBusy("UpdateOutcome");
					return result;
				}
				, (error) => {
					this.FetchOutcomes(this._currentItemSetId);		
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("UpdateOutcome");
					return error;
				});
	}

	public DeleteOutcome(outcomeId: number, itemSetId: number, key: number) {

		this._BusyMethods.push("DeleteOutcome");
			let ErrMsg = "Something went wrong when deleting an outcome. \r\n If the problem persists, please contact EPPISupport.";

		//console.log('outcome deleting: ' + JSON.stringify(outcomeId) + 'asdfsadf: ==== ' + JSON.stringify(itemSetId) );
		let body = JSON.stringify({ outcomeId: outcomeId, itemSetId: itemSetId  });		
		this._http.post<any>(this._baseUrl + 'api/OutcomeList/DeleteOutcome',

			body).subscribe(
				(result) => {

                    this.outcomesList.splice(key, 1);
					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteOutcome");
					return result;
				}
				, (error) => {


					this.FetchOutcomes(this._currentItemSetId);					
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteOutcome");
					return error;
					}
				);
	}


}


export class ItemArm {

	itemArmId: number = 0;
	title: string = '';

}
export interface iOutcomeList {
	outcomesList: iOutcome[];
}
export class OutcomeItemList {
    outcomesList: iOutcome[] = [];
}
export class OutcomeType {

	outcomeTypeId: number = 0;
	outcomeTypeName: string = '';

}
export class ReviewSetDropDownResult {

	public attributeId: number = 0;
	public attributeName: string = '';

}

export interface iOutcome {
    outcomeId: number;
    itemSetId: number;
    outcomeTypeName: string;
    outcomeTypeId: number;
    NRows: number;
    outcomeCodes: OutcomeItemAttributesList;
    itemAttributeIdIntervention: number;
    itemAttributeIdControl: number;
    itemAttributeIdOutcome: number;
    itemArmIdGrp1: number;
    itemArmIdGrp2: number;
    itemId: number;
    itemTimepointValue: string;
    itemTimepointMetric: string;
    itemTimepointId: number;
    outcomeTimePoint: iTimePoint;
    title: string;
    shortTitle: string;
    timepointDisplayValue: string;
    outcomeDescription: string;
    data1: number;
    data2: number;
    data3: number;
    data4: number;
    data5: number;
    data6: number;
    data7: number;
    data8: number;
    data9: number;
    data10: number;
    data11: number;
    data12: number;
    data13: number;
    data14: number;
    interventionText: string;
    controlText: string;
    outcomeText: string;
    feWeight: number;
    reWeight: number;
    smd: number;
    sesmd: number;
    r: number;
    ser: number;
    oddsRatio: number;
    seOddsRatio: number;
    riskRatio: number;
    seRiskRatio: number;
    ciUpperSMD: number;
    ciLowerSMD: number;
    ciUpperR: number;
    ciLowerR: number;
    ciUpperOddsRatio: number;
    ciLowerOddsRatio: number;
    ciUpperRiskRatio: number;
    ciLowerRiskRatio: number;
    ciUpperRiskDifference: number;
    ciLowerRiskDifference: number;
    ciUpperPetoOddsRatio: number;
    ciLowerPetoOddsRatio: number;
    ciUpperMeanDifference: number;
    ciLowerMeanDifference: number;
    riskDifference: number;
    seRiskDifference: number;
    meanDifference: number;
    seMeanDifference: number;
    petoOR: number;
    sePetoOR: number;
    es: number;
    sees: number;
    nRows: number;
    ciLower: number;
    ciUpper: number;
    esDesc: string;
    seDesc: string;
    data1Desc: string;
    data2Desc: string;
    data3Desc: string;
    data4Desc: string;
    data5Desc: string;
    data6Desc: string;
    data7Desc: string;
    data8Desc: string;
    data9Desc: string;
    data10Desc: string;
    data11Desc: string;
    data12Desc: string;
    data13Desc: string;
    data14Desc: string;
}
export class Outcome {

    unifiedOutcomeTypeIdProperty: number = 0;
    public constructor(iO?: iOutcome) {
        if (iO) {
            this.title = iO.title;
            this.outcomeId = iO.outcomeId;
            this._data1 = iO.data1;
            this.data2 = iO.data2;
            this.data3 = iO.data3;
            this.data4 = iO.data4;
            this.data5 = iO.data5;
            this.data6 = iO.data6;
            this.SetCalculatedValues();
        }
    }
    private SetCalculatedValues() {
        console.log("SetCalculatedValues");
        this.SetEffectSizes();
    }
	private SetEffectSizes() {

		let es: number = 0;
		let se: number = 0;
		this.unifiedOutcomeTypeIdProperty = this.outcomeTypeId;

		switch (this.outcomeTypeId) {

			case 0: // manual entry

				this.smd = this.data1;
				this.sesmd = this.CorrectForClustering(this.data2);
				this.r = this.data3;
				this.ser = this.data4;
				this.oddsRatio = this.data5;
				this.seOddsRatio = this.CorrectForClustering(this.data6);
				this.riskRatio = this.data7;
				this.seRiskRatio = this.CorrectForClustering(this.data8);
				this.riskDifference = this.data11;
				this.seRiskDifference = this.CorrectForClustering(this.data12);
				this.meanDifference = this.data13;
				this.seMeanDifference = this.CorrectForClustering(this.data14);
				this.SetESforManualEntry();

				break;

			case 1: // n, mean, SD

				this.smd = this.SmdFromNMeanSD();
				this.sesmd = this.CorrectForClustering(this.getSEforD(this.data1, this.data2, this.smd));
				this.meanDifference = this.MeanDiff();
				this.seMeanDifference = this.CorrectForClustering(this.getSEforMeanDiff(this.data1,
					this.data2, this.data5, this.data6));
				this.es = this.smd;
				this.sees = this.sesmd;
						
				break;

			case 2: // binary 2 x 2 table

				this.oddsRatio = this.CalcOddsRatio();
				this.seOddsRatio = this.CorrectForClustering(this.CalcOddsRatioSE());
				this.riskRatio = this.CalcRiskRatio();
				this.seRiskRatio = this.CorrectForClustering(this.CalcRiskRatioSE());
				this.riskDifference = this.CalcRiskDifference();
				this.seRiskDifference = this.CorrectForClustering(this.CalcRiskDifferenceSE());
				this.petoOR = this.CalcPetoOR();
				this.sePetoOR = this.CorrectForClustering(this.CalcPetoORSE());
				this.es = this.oddsRatio;
				this.sees = this.seOddsRatio;

				break;

			case 3: //n, mean SE

				this.smd = this.SmdFromNMeanSe();

				this.sesmd = this.CorrectForClustering(this.GetSEforD(this.data1, this.data2, this.smd));

				this.meanDifference =  this.MeanDiff();

				this.seMeanDifference = this.CorrectForClustering(this.GetSEforMeanDiff(
					this.data1, this.data2, this.data5, this.data6));

				this.es = this.smd;
				this.sees = this.sesmd;
				break;

			case 4: //n, mean CI

				this.smd =  this.SmdFromNMeanCI();
				this.sesmd = this.CorrectForClustering(this.GetSEforD(
					this.data1, this.data2, this.smd));

				this.meanDifference = this.MeanDiff();

				this.seMeanDifference = this.CorrectForClustering(
					this.GetSEforMeanDiff(this.data1, this.data2,
						this.data5, this.data6));
				
				this.es = this.smd;

				this.sees = this.sesmd;

				break;

			case 5: // N, t- or p-value

				if (this.data4 != 0) {
					this.smd = this.CorrectG(this.data1, this.data2, this.SmdFromP());
				}
				else {
					this.smd = this.SmdFromT(this.data3);
				}
				this.sesmd = this.CorrectForClustering(this.GetSEforD(
					this.data1, this.data2, this.smd));
				this.meanDifference = this.MeanDiff();
				this.seMeanDifference = this.CorrectForClustering(
					this.GetSEforMeanDiff(this.data1, this.data2, this.data5, this.data6));

				this.es = this.smd;
				this.sees = this.sesmd;

				break;

			case 6: // diagnostic binary 2 x 2 table

				this.es = this.CalcOddsRatio();
				this.seOddsRatio = this.CalcOddsRatioSE();
				this.es = this.oddsRatio;
				this.sees = this.seOddsRatio;
				break;

			case 7: // correlation coefficient r

				this.r = this.data2;
				this.ser =  Math.sqrt(1 / (this.data1 - 3));
				this.es = this.r;
				this.sees = this.ser;
				break;

			default:
				break;
		}

		this.ciLower = this.smd - (1.96 * this.sees);
		this.ciUpper = this.smd + (1.96 * this.sees);

	}

	private getSEforD(n1: number, n2: number, d: number): number {
		let top, lower, left, right, se: number;

		left = (n1 + n2) / (n1 * n2);
		top = d * d;
		lower = 2 * (n1 + n2 - 3.94);
		right = top / lower;
		se = Math.sqrt(left + right);
		return se;
	}
	private getSEforMeanDiff(n1: number, n2: number,
		SD1: number, SD2: number): number {
		return Math.sqrt(SD1 * SD1 / n1 + SD2 * SD2 / n2);
	}
	private SmdFromNMeanSD(): number {
		let SD: number = this.PoolSDs(this.data1, this.data2, this.data5, this.data6);
		if (SD == 0) {
			return 0;
		}
		let cohensD: number = (this.data3 - this.data4) / SD;
		return cohensD * (1 - (3 / (4 * (this.data1 + this.data2) - 9)));
	}

	private MeanDiff(): number {
		return this.data3 - this.data4;
	}
	private SmdFromNMeanSe(): number {
		let SD: number = this.PoolSDs(this.data1, this.data2,
			this.SdFromSe(this.data5, this.data1), this.SdFromSe(this.data6, this.data1));
		if (SD == 0) {
			return 0;
		}
		let cohensD: number = (this.data3 - this.data4) / SD;
		return cohensD * (1 - (3 / (4 * (this.data1 + this.data2) - 9)));
	}
	private SmdFromNMeanCI(): number {
		let SD: number = PoolSDs(this.data1, this.data2,
			this.SdFromSe(SeFromCi(this.data7, this.data5), this.data1),
			this.SdFromSe(SeFromCi(this.data8, this.data6), this.data1));
		if (SD == 0) {
			return 0;
		}
		let cohensD: number = (this.data3 - this.data4) / SD;
		return cohensD * (1 - (3 / (4 * (this.data1 + this.data2) - 9)));
	}
	private SmdFromP(): number {
		let t = this.StatFunctions.qt(this.data4 / 2, this.data1 + this.data2 - 2, false);
		return this.SmdFromT(t);
	}
	private SmdFromT(double t): number  {
		double g, top, lower;

		if (Data1 == Data2) {
			top = 2 * t;
			lower = System.Math.Sqrt(Data1 + Data2);
		}
		else {
			top = t * System.Math.Sqrt(Data1 + Data2);
			lower = System.Math.Sqrt(Data1 * Data2);
		}
		if (lower != 0) {
			g = top / lower;
		}
		else {
			g = 0;
		}
		return g;
	}

	private double CorrectG(double n1, double n2, double g) // for single group studies n2=0
	{
		double gc, top, lower;

		top = 3;
		lower = (4 * (n1 + n2)) - 9;
		if (lower != 0) {
			gc = g * (1 - (top / lower));
		}
		else {
			gc = 0;
		}
		return gc;
	}

	private double calcOddsRatio() {
		double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
		if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0)) {
			d1 = Data1 + 0.5;
			d2 = Data2 + 0.5;
			d3 = Data3 + 0.5;
			d4 = Data4 + 0.5;
		}
		else {
			d1 = Data1;
			d2 = Data2;
			d3 = Data3;
			d4 = Data4;
		}
		return (d1 * d4) / (d3 * d2);
	}

	private double calcOddsRatioSE() {
		double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
		if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0)) {
			d1 = Data1 + 0.5;
			d2 = Data2 + 0.5;
			d3 = Data3 + 0.5;
			d4 = Data4 + 0.5;
		}
		else {
			d1 = Data1;
			d2 = Data2;
			d3 = Data3;
			d4 = Data4;
		}
		return Math.Sqrt((1 / d1) + (1 / d2) + (1 / d3) + (1 / d4));
	}

	private double calcRiskRatio() {
		double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
		if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0)) {
			d1 = Data1 + 0.5;
			d2 = Data2 + 0.5;
			d3 = Data3 + 0.5;
			d4 = Data4 + 0.5;
		}
		else {
			d1 = Data1;
			d2 = Data2;
			d3 = Data3;
			d4 = Data4;
		}
		return (d1 / (d1 + d3)) / (d2 / (d2 + d4));
	}

	private double calcRiskRatioSE() {
		double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
		if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0)) {
			d1 = Data1 + 0.5;
			d2 = Data2 + 0.5;
			d3 = Data3 + 0.5;
			d4 = Data4 + 0.5;
		}
		else {
			d1 = Data1;
			d2 = Data2;
			d3 = Data3;
			d4 = Data4;
		}
		return Math.Sqrt((1 / d1) + (1 / d2) - (1 / (d1 + d3)) - (1 / (d2 + d4)));
	}

	private double calcRiskDifference() {
		double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
		if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0)) {
			d1 = Data1 + 0.5;
			d2 = Data2 + 0.5;
			d3 = Data3 + 0.5;
			d4 = Data4 + 0.5;
		}
		else {
			d1 = Data1;
			d2 = Data2;
			d3 = Data3;
			d4 = Data4;
		}
		return (d1 / (d1 + d3)) - (d2 / (d2 + d4));
	}

	private double calcRiskDifferenceSE() {
		double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
		if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0)) {
			d1 = Data1 + 0.5;
			d2 = Data2 + 0.5;
			d3 = Data3 + 0.5;
			d4 = Data4 + 0.5;
		}
		else {
			d1 = Data1;
			d2 = Data2;
			d3 = Data3;
			d4 = Data4;
		}
		return Math.Sqrt((d1 * d3 / Math.Pow((d1 + d3), 3)) + (d2 * d4 / Math.Pow((d2 + d4), 3)));
	}

	private double calcPetoOR() {
		double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
		if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0)) {
			d1 = Data1 + 0.5;
			d2 = Data2 + 0.5;
			d3 = Data3 + 0.5;
			d4 = Data4 + 0.5;
		}
		else {
			d1 = Data1;
			d2 = Data2;
			d3 = Data3;
			d4 = Data4;
		}
		double n1 = d1 + d3;
		double n2 = d2 + d4;
		double n = n1 + n2;

		double v = (n1 * n2 * (d1 + d2) * (d3 + d4)) / (n * n * (n - 1));
		double e = n1 * (d1 + d2) / n;
		return Math.Exp((d1 - e) / v);
	}

	private double calcPetoORSE() {
		double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
		if ((Data1 == 0) || (Data2 == 0) || (Data3 == 0) || (Data4 == 0)) {
			d1 = Data1 + 0.5;
			d2 = Data2 + 0.5;
			d3 = Data3 + 0.5;
			d4 = Data4 + 0.5;
		}
		else {
			d1 = Data1;
			d2 = Data2;
			d3 = Data3;
			d4 = Data4;
		}
		double n1 = d1 + d3;
		double n2 = d2 + d4;
		double n = n1 + n2;

		double v = (n1 * n2 * (d1 + d2) * (d3 + d4)) / (n * n * (n - 1));
		return Math.Sqrt(1 / v);
	}

	private double PoolSDs(double n1, double n2, double sd1, double sd2) {
		if (n1 + n2 < 3) {
			return 0;
		}
		double s = (((n1 - 1) * sd1 * sd1) + ((n2 - 1) * sd2 * sd2)) / (n1 + n2 - 2);
		s = Math.Sqrt(s);
		return s;
	}

	private double SdFromSe(double se, double n) {
		return se * Math.Sqrt(n);
	}

	private double SeFromCi(double ciUpper, double ciLower) {
		double se = Math.Abs((0.5 * (ciUpper - ciLower)) / 1.96);
		return se;
	}
	CorrectForClustering(se: number): number {

		if (this.data9 != 0) {
			let deff: number = 1 + (this.data9 - 1) * this.data10;
			return se * Math.sqrt(deff);
		}
		else {
			return se;
		}
    }
	outcomeId: number = 0;
	itemSetId: number = 0;
	outcomeTypeName: string = "";
	private _outcomeTypeId: number = 0;
	public get outcomeTypeId(): number {
		return this._outcomeTypeId;
	}
	public set outcomeTypeId(val: number) {
		this._outcomeTypeId = val;
		this.SetCalculatedValues();
	}
	NRows: number = 0;
	outcomeCodes: OutcomeItemAttributesList = new OutcomeItemAttributesList();//OutcomeItemAttribute[] = [];
	itemAttributeIdIntervention: number = 0;
	itemAttributeIdControl: number = 0;
	itemAttributeIdOutcome: number = 0;
	itemArmIdGrp1: number = 0;
	itemArmIdGrp2: number = 0;
	itemId: number = 0;
	itemTimepointValue: string = '';
	itemTimepointMetric: string = '';
	itemTimepointId: number = 0;
	outcomeTimePoint = {} as iTimePoint;
	title: string = "";
    shortTitle: string = "";
    timepointDisplayValue: string = "";
	outcomeDescription: string = "";
    private _data1: number = 0;
    public get data1(): number {
        return this._data1;
    }
    public set data1(val: number) {
        this._data1 = val;
        this.SetCalculatedValues();
    }
	private _data2: number = 0;
	public get data2(): number {
		return this._data2;
	}
	public set data2(val: number) {
		this._data2 = val;
		this.SetCalculatedValues();
	}
	private _data3: number = 0;
	public get data3(): number {
		return this._data3;
	}
	public set data3(val: number) {
		this._data3 = val;
		this.SetCalculatedValues();
	}
	private _data4: number = 0;
	public get data4(): number {
		return this._data4;
	}
	public set data4(val: number) {
		this._data4 = val;
		this.SetCalculatedValues();
	}
	private _data5: number = 0;
	public get data5(): number {
		return this._data5;
	}
	public set data5(val: number) {
		this._data5 = val;
		this.SetCalculatedValues();
	}
	private _data6: number = 0;
	public get data6(): number {
		return this._data6;
	}
	public set data6(val: number) {
		this._data6 = val;
		this.SetCalculatedValues();
	}
	private _data7: number = 0;
	public get data7(): number {
		return this._data7;
	}
	public set data7(val: number) {
		this._data7 = val;
		this.SetCalculatedValues();
	}
	private _data8: number = 0;
	public get data8(): number {
		return this._data8;
	}
	public set data8(val: number) {
		this._data8 = val;
		this.SetCalculatedValues();
	}
	private _data9: number = 0;
	public get data9(): number {
		return this._data9;
	}
	public set data9(val: number) {
		this._data9 = val;
		this.SetCalculatedValues();
	}
	private _data10: number = 0;
	public get data10(): number {
		return this._data10;
	}
	public set data10(val: number) {
		this._data10 = val;
		this.SetCalculatedValues();
	}
	private _data11: number = 0;
	public get data11(): number {
		return this._data11;
	}
	public set data11(val: number) {
		this._data11 = val;
		this.SetCalculatedValues();
	}
	private _data12: number = 0;
	public get data12(): number {
		return this._data12;
	}
	public set data12(val: number) {
		this._data12 = val;
		this.SetCalculatedValues();
	}
	private _data13: number = 0;
	public get data13(): number {
		return this._data13;
	}
	public set data13(val: number) {
		this._data13 = val;
		this.SetCalculatedValues();
	}
	private _data14: number = 0;
	public get data14(): number {
		return this._data14;
	}
	public set data14(val: number) {
		this._data14 = val;
		this.SetCalculatedValues();
	}
	interventionText: string = "";
	controlText: string = "";
	outcomeText: string = "";
	feWeight: number = 0;
	reWeight: number = 0;
	smd: number = 0;
	sesmd: number = 0;
	r: number = 0;
	ser: number = 0;
	oddsRatio: number = 0;
	seOddsRatio: number = 0;
	riskRatio: number = 0;
	seRiskRatio: number = 0;
	ciUpperSMD: number = 0;
	ciLowerSMD: number = 0;
	ciUpperR: number = 0;
	ciLowerR: number = 0;
	ciUpperOddsRatio: number = 0;
	ciLowerOddsRatio: number = 0;
	ciUpperRiskRatio: number = 0;
	ciLowerRiskRatio: number = 0;
	ciUpperRiskDifference: number = 0;
	ciLowerRiskDifference: number = 0;
	ciUpperPetoOddsRatio: number = 0;
	ciLowerPetoOddsRatio: number = 0;
	ciUpperMeanDifference: number = 0;
	ciLowerMeanDifference: number = 0;
	riskDifference: number = 0;
	seRiskDifference: number = 0;
	meanDifference: number = 0;
	seMeanDifference: number = 0;
	petoOR: number = 0;
	sePetoOR: number = 0;
	es: number = 0;
	sees: number = 0;
	nRows: number = 0;
	ciLower: number = 0;
	ciUpper: number = 0;
	esDesc: string = "";
	seDesc: string = "";
	private _data1Desc: number = 0;
	public get data1Desc(): number {
		return this._data1Desc;
	}
	private _data2Desc: number = 0;
	public get data2Desc(): number {
		return this._data2Desc;
	}
	private _data3Desc: number = 0;
	public get data3Desc(): number {
		return this._data3Desc;
	}
	private _data4Desc: number = 0;
	public get data4Desc(): number {
		return this._data4Desc;
	}
	private _data5Desc: number = 0;
	public get data5Desc(): number {
		return this._data5Desc;
	}
	private _data6Desc: number = 0;
	public get data6Desc(): number {
		return this._data6Desc;
	}
	private _data7Desc: number = 0;
	public get data7Desc(): number {
		return this._data7Desc;
	}
	private _data8Desc: number = 0;
	public get data8Desc(): number {
		return this._data8Desc;
	}
	private _data9Desc: number = 0;
	public get data9Desc(): number {
		return this._data9Desc;
	}
	private _data10Desc: number = 0;
	public get data10Desc(): number {
		return this._data10Desc;
	}
	private _data11Desc: number = 0;
	public get data11Desc(): number {
		return this._data11Desc;
	}
	private _data12Desc: number = 0;
	public get data12Desc(): number {
		return this._data12Desc;
	}
	private _data13Desc: number = 0;
	public get data13Desc(): number {
		return this._data13Desc;
	}
	private _data14Desc: number = 0;
	public get data14Desc(): number {
		return this._data14Desc;
	}
}
export class OutcomeItemAttributesList {
	outcomeItemAttributesList: OutcomeItemAttribute[] = [];
}
export interface OutcomeItemAttribute {
	outcomeItemAttributeId: number;
	outcomeId: number;
	attributeId: number;
	additionalText: string;
	attributeName: string;
}
