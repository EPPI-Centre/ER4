import { Inject, Injectable, EventEmitter, Output, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { SetAttribute } from './ReviewSets.service';
import { iTimePoint } from './timePoints.service';
import { Helpers } from '../helpers/HelperMethods';

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
		this._Outcomes = [];
        this._http.post<iOutcomeList>(this._baseUrl + 'api/OutcomeList/Fetch', body).subscribe(result => {
   
            for (let iO of result.outcomesList) {
   
				let RealOutcome: Outcome = new Outcome(iO);
				
                this._Outcomes.push(RealOutcome);
       
            }
        
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
					this.FetchOutcomes(currentOutcome.itemSetId);	
					this.RemoveBusy("UpdateOutcome");
					return result;
				}
				, (error) => {
					this.FetchOutcomes(currentOutcome.itemSetId);		
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("UpdateOutcome");
					return error;
				});
	}

	public DeleteOutcome(outcomeId: number, itemSetId: number, key: number) {

		this._BusyMethods.push("DeleteOutcome");
			let ErrMsg = "Something went wrong when deleting an outcome. \r\n If the problem persists, please contact EPPISupport.";

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

	isSelected: boolean;
	canSelect: boolean;
	grp1ArmName: string;
	grp2ArmName: string;
	outcomeId: number;
	manuallyEnteredOutcomeTypeId: number;
	unifiedOutcomeTypeId: number;
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
	timepointDisplayValue: string;
    title: string;
    shortTitle: string;
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
    CIUpperSMD: number;
    CILowerSMD: number;
    CIUpperR: number;
    CILowerR: number;
    CIUpperOddsRatio: number;
    CILowerOddsRatio: number;
    CIUpperRiskRatio: number;
    CILowerRiskRatio: number;
    CIUpperRiskDifference: number;
    CILowerRiskDifference: number;
    CIUpperPetoOddsRatio: number;
    CILowerPetoOddsRatio: number;
    CIUpperMeanDifference: number;
    CILowerMeanDifference: number;
    riskDifference: number;
    seRiskDifference: number;
    meanDifference: number;
    seMeanDifference: number;
    petoOR: number;
    sePetoOR: number;
    es: number;
    sees: number;
    nRows: number;
    CILower: number;
    CIUpper: number;
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

	unifiedOutcomeTypeId: number = 0;
	manuallyEnteredOutcomeTypeId: number = 0;
	grp1ArmName: string = '';
	grp2ArmName: string = '';
	isSelected: boolean = false;
	canSelect: boolean = false;

    public constructor(iO?: iOutcome) {
		if (iO) {
			console.log('going through here: ');
			console.log(iO);
			this.itemSetId = iO.itemSetId;
			this.OutcomeTypeId = iO.outcomeTypeId;
			this.manuallyEnteredOutcomeTypeId = iO.manuallyEnteredOutcomeTypeId;
			this.unifiedOutcomeTypeId = iO.unifiedOutcomeTypeId;
			this.OutcomeTypeName = iO.outcomeTypeName;
			this.itemAttributeIdIntervention =  iO.itemAttributeIdIntervention;
			this.itemAttributeIdControl = iO.itemAttributeIdControl;
			this.itemAttributeIdOutcome = iO.itemAttributeIdOutcome
			this.title = iO.title;
			this.shortTitle = iO.shortTitle;
			this.OutcomeDescription = iO.outcomeDescription
            this.outcomeId = iO.outcomeId;
            this.data1 = Number(iO.data1 == null ? 0: iO.data1);
			this.data2 = Number(iO.data2 == null ? 0 : iO.data2);
			this.data3 = Number(iO.data3 == null ? 0 : iO.data3);
			this.data4 = Number(iO.data4 == null ? 0 : iO.data4);
			this.data5 = Number(iO.data5 == null ? 0 : iO.data5);
			this.data6 = Number(iO.data6 == null ? 0 : iO.data6);
			this.data7 = Number(iO.data7 == null ? 0 : iO.data7);
			this.data8 = Number(iO.data8 == null ? 0 : iO.data8);
			this.data9 = Number(iO.data9 == null ? 0 : iO.data9);
			this.data10 = Number(iO.data10 == null ? 0 : iO.data10);
			this.data11 = Number(iO.data11 == null ? 0 : iO.data11);
			this.data12 = Number(iO.data12 == null ? 0 : iO.data12);
			this.data13 = Number(iO.data13 == null ? 0 : iO.data13);
			this.data14 = Number(iO.data14 == null ? 0 : iO.data14);
			this.interventionText = iO.interventionText;
			this.controlText = iO.controlText;
			this.OutcomeText = iO.outcomeText;
			this.itemTimepointId = iO.itemTimepointId;
			this.itemTimepointMetric = iO.itemTimepointMetric;
			this.itemTimepointValue = iO.itemTimepointValue;
			this.itemArmIdGrp1 = iO.itemArmIdGrp1;
			this.itemArmIdGrp2 = iO.itemArmIdGrp2;
			this.timepointDisplayValue = iO.timepointDisplayValue;
			this.grp1ArmName = iO.grp1ArmName;
			this.grp2ArmName = iO.grp2ArmName;
			this.isSelected = iO.isSelected;
			this.canSelect = iO.canSelect;
			this.outcomeCodes = iO.outcomeCodes;
			this.feWeight = iO.feWeight;
			this.reWeight = iO.reWeight;

            this.SetCalculatedValues();
        }
	}

    public SetCalculatedValues() {
		
		this.SetEffectSizes();
		switch (this.outcomeTypeId) {
			case 0: // manual entry
				this.esDesc= "Effect size";
				this.seDesc= "SE";
				this.NRows= 6;
				this.data1Desc = "SMD";
				this.data2Desc = "standard error";
				this.data3Desc = "r";
				this.data4Desc = "SE (Z transformed)";
				this.data5Desc = "odds ratio";
				this.data6Desc = "SE (log OR)";
				this.data7Desc = "risk ratio";
				this.data8Desc = "SE (log RR)";
				this.data9Desc = "";
				this.data10Desc = "";
				this.data11Desc = "risk difference";
				this.data12Desc = "standard error";
				this.data13Desc = "mean difference";
				this.data14Desc = "standard error";
				break;

			case 1: // n, mean, SD
				this.esDesc = "SMD";
				this.seDesc = "SE";
				this.NRows = 3;
				this.data1Desc = "Group 1 N";
				this.data2Desc = "Group 2 N";
				this.data3Desc= "Group 1 mean";
				this.data4Desc= "Group 2 mean";
				this.data5Desc= "Group 1 SD";
				this.data6Desc= "Group 2 SD";
				this.data7Desc= "";
				this.data8Desc= "";
				this.data9Desc= "";
				this.data10Desc= "";
				this.data11Desc= "";
				this.data12Desc= "";
				this.data13Desc= "";
				this.data14Desc= "";
				this.outcomeTypeName= "Continuous";
				break;

			case 2: // binary 2 x 2 table
				this.esDesc= "OR";
				this.seDesc= "SE (log OR)";
				this.NRows= 2;
				this.data1Desc= "Group 1 events";
				this.data2Desc= "Group 2 events";
				this.data3Desc= "Group 1 no events";
				this.data4Desc= "Group 2 no events";
				this.data5Desc= "";
				this.data6Desc= "";
				this.data7Desc= "";
				this.data8Desc= "";
				this.data9Desc= "";
				this.data10Desc= "";
				this.data11Desc= "";
				this.data12Desc= "";
				this.data13Desc= "";
				this.data14Desc= "";
				this.outcomeTypeName= "Binary";
				break;

			case 3: //n, mean SE
				this.esDesc= "SMD";
				this.seDesc= "SE";
				this.NRows= 3;
				this.data1Desc= "Group 1 N";
				this.data2Desc= "Group 2 N";
				this.data3Desc= "Group 1 mean";
				this.data4Desc= "Group 2 mean";
				this.data5Desc= "Group 1 SE";
				this.data6Desc= "Group 2 SE";
				this.data7Desc= "";
				this.data8Desc= "";
				this.data9Desc= "";
				this.data10Desc= "";
				this.data11Desc= "";
				this.data12Desc= "";
				this.data13Desc= "";
				this.data14Desc= "";
				this.outcomeTypeName= "Continuous";
				break;

			case 4: //n, mean CI
				this.esDesc= "SMD";
				this.seDesc= "SE";
				this.NRows= 4;
				this.data1Desc= "Group 1 N";
				this.data2Desc= "Group 2 N";
				this.data3Desc= "Group 1 mean";
				this.data4Desc= "Group 2 mean";
				this.data5Desc= "Group 1 CI lower";
				this.data6Desc= "Group 2 CI lower";
				this.data7Desc= "Group 1 CI upper";
				this.data8Desc= "Group 2 CI upper";
				this.data9Desc= "";
				this.data10Desc= "";
				this.data11Desc= "";
				this.data12Desc= "";
				this.data13Desc= "";
				this.data14Desc= "";
				this.outcomeTypeName= "Continuous";
				break;

			case 5: //n, t or p value
				this.esDesc= "SMD";
				this.seDesc= "SE";
				this.NRows= 2;
				this.data1Desc= "Group 1 N";
				this.data2Desc= "Group 2 N";
				this.data3Desc= "t-value";
				this.data4Desc= "p-value";
				this.data5Desc= "";
				this.data6Desc= "";
				this.data7Desc= "";
				this.data8Desc= "";
				this.data9Desc= "";
				this.data10Desc= "";
				this.data11Desc= "";
				this.data12Desc= "";
				this.data13Desc= "";
				this.data14Desc= "";
				this.outcomeTypeName= "Continuous";
				break;

			case 6: // diagnostic test 2 x 2 table
				this.esDesc= "Diagnostic OR";
				this.seDesc= "SE";
				this.NRows= 2;
				this.data1Desc= "True positive";
				this.data2Desc= "False positive";
				this.data3Desc= "False negative";
				this.data4Desc= "True negative";
				this.data5Desc= "";
				this.data6Desc= "";
				this.data7Desc= "";
				this.data8Desc= "";
				this.data9Desc= "";
				this.data10Desc= "";
				this.data11Desc= "";
				this.data12Desc= "";
				this.data13Desc= "";
				this.data14Desc= "";
				this.outcomeTypeName= "Binary";
				break;

			case 7: // correlation coeffiCIent r
				this.esDesc= "r";
				this.seDesc= "SE (Z transformed)";
				this.NRows= 1;
				this.data1Desc= "group(s) size";
				this.data2Desc= "correlation";
				this.data3Desc= "";
				this.data4Desc= "";
				this.data5Desc= "";
				this.data6Desc= "";
				this.data7Desc= "";
				this.data8Desc= "";
				this.data9Desc= "";
				this.data10Desc= "";
				this.data11Desc= "";
				this.data12Desc= "";
				this.data13Desc= "";
				this.data14Desc= "";
				this.outcomeTypeName= "Correlation";
				break;

			default:
				this.data1Desc= "";
				this.data2Desc= "";
				this.data3Desc= "";
				this.data4Desc= "";
				this.data5Desc= "";
				this.data6Desc= "";
				this.data7Desc= "";
				this.data8Desc= "";
				this.data9Desc= "";
				this.data10Desc= "";
				this.data11Desc= "";
				this.data12Desc= "";
				this.data13Desc= "";
				this.data14Desc= "";
				this.outcomeTypeName= "";
				break;
		}


	}
	private SetEffectSizes() {

		let es: number = 0;
		let se: number = 0;
		this.unifiedOutcomeTypeId = this.outcomeTypeId;

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
				this.sesmd = this.CorrectForClustering(this.GetSEforD(this.data1, this.data2, this.smd));
				this.meanDifference = this.MeanDiff();
				this.seMeanDifference = this.CorrectForClustering(this.GetSEforMeanDiff(this.data1,
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
				this.sees = this.CalcOddsRatioSE();
				this.oddsRatio = this.es;
				this.seOddsRatio = this.sees;
				break;

			case 7: // correlation coeffiCIent r

				this.r = this.data2;
				this.ser =  Math.sqrt(1 / (this.data1 - 3));
				this.es = this.r;
				this.sees = this.ser;

				break;

			default:

				break;
		}

		this.CILower = this.smd - (1.96 * this.sees);
		this.CIUpper = this.smd + (1.96 * this.sees);

	}

	SetESforManualEntry(): any {

		if (this.oddsRatio == 0 && this.riskRatio == 0 &&
			this.riskDifference == 0 && this.petoOR == 0
			&& this.r == 0 && this.meanDifference == 0) {

			this.es = this.smd;
			this.sees = this.sesmd;
			this.unifiedOutcomeTypeId = 1;
			this.outcomeTypeName = "Continuous";
		}
		if (this.smd == 0 && this.riskRatio == 0 && this.riskDifference == 0
			&& this.petoOR == 0 && this.r == 0 && this.meanDifference == 0) {

			this.es = this.oddsRatio;
			this.sees = this.seOddsRatio;
			this.unifiedOutcomeTypeId = 2;
			this.outcomeTypeName = "Binary";

		}
		if (this.smd == 0 && this.smd == 0 && this.riskDifference == 0
			&& this.petoOR == 0 && this.r == 0 && this.meanDifference == 0) {
			this.es = this.riskRatio;
			this.sees = this.seRiskRatio;
			this.unifiedOutcomeTypeId = 2;
			this.outcomeTypeName = "Binary";
		}
		if (this.smd == 0 && this.oddsRatio == 0 && this.riskRatio == 0
			&& this.petoOR == 0 && this.r == 0 && this.meanDifference == 0) {
			this.es = this.riskDifference;
			this.sees = this.seRiskDifference;
			this.unifiedOutcomeTypeId = 2;
			this.outcomeTypeName = "Binary";
		}
		if (this.smd == 0 && this.oddsRatio == 0 && this.riskRatio == 0
			&& this.riskDifference == 0 && this.r == 0 && this.meanDifference == 0) {
			this.es = this.petoOR;
			this.sees = this.sePetoOR;
			this.unifiedOutcomeTypeId = 2;
			this.outcomeTypeName = "Binary";
		}
		if (this.smd == 0 && this.oddsRatio == 0 && this.riskRatio == 0
			&& this.riskDifference == 0 && this.petoOR == 0 && this.meanDifference == 0) {
			this.es =  this.r;
			this.sees = this.ser;
			this.unifiedOutcomeTypeId = 7;
			this.outcomeTypeName = "Correlation";
		}
		if (this.smd == 0 && this.oddsRatio == 0 && this.riskRatio == 0
			&& this.riskDifference == 0 && this.petoOR == 0 && this.r == 0) {
			this.es = this.meanDifference;
			this.sees =  this.seMeanDifference;
			this.unifiedOutcomeTypeId = 1;
			this.outcomeTypeName =  "Continuous";
		}
    }
	private GetSEforD(n1: number, n2: number, d: number): number {
		let top, lower, left, right, se: number;

		left = (n1 + n2) / (n1 * n2);
		top = d * d;
		lower = 2 * (n1 + n2 - 3.94);
		right = top / lower;
		se = Math.sqrt(left + right);
		return se;
	}
	private GetSEforMeanDiff(n1: number, n2: number,
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
		
		let thirdArg: number = this.SdFromSe(this.data5, this.data1);
		let fourthArg: number = this.SdFromSe(this.data6, this.data1);
		let SD: number = this.PoolSDs(this.data1, this.data2,
			thirdArg, fourthArg);
		if (SD == 0) {
			return 0;
		}
		let cohensD: number = (this.data3 - this.data4) / SD;
		return cohensD * (1 - (3 / (4 * (this.data1 + this.data2) - 9)));
	}
	private SmdFromNMeanCI(): number {
		let SD: number = this.PoolSDs(this.data1, this.data2,
			this.SdFromSe(this.SeFromCi(this.data7, this.data5), this.data1),
			this.SdFromSe(this.SeFromCi(this.data8, this.data6), this.data1));
		if (SD == 0) {
			return 0;
		}
		let cohensD: number = (this.data3 - this.data4) / SD;
		return cohensD * (1 - (3 / (4 * (this.data1 + this.data2) - 9)));
	}
	private SmdFromP(): number {
		let t = StatFunctions.qt(this.data4 / 2, this.data1 + this.data2 - 2, false);
		return this.SmdFromT(t);
	}
	private SmdFromT( t: number): number  {
		let g, top, lower: number;

		if (this.data1 == this.data2) {
			top = 2 * t;
			lower = Math.sqrt(this.data1 + this.data2);
		}
		else {
			top = t * Math.sqrt(this.data1 + this.data2);
			lower = Math.sqrt(this.data1 * this.data2);
		}
		if (lower != 0) {
			g = top / lower;
		}
		else {
			g = 0;
		}
		return g;
	}
	private CorrectG(n1: number, n2: number, g: number ): number  // for single group studies n2=0
	{
		let gc, top, lower: number ;

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
	private CalcOddsRatio(): number  {
		let d1: number = 0, d2: number = 0, d3: number = 0, d4: number  = 0;
		if ((this.data1 == 0) || (this.data2 == 0) || (this.data3 == 0) || (this.data4 == 0)) {
			d1 = this.data1 + 0.5;
			d2 = this.data2 + 0.5;
			d3 = this.data3 + 0.5;
			d4 = this.data4 + 0.5;
		}
		else {
			d1 = this.data1;
			d2 = this.data2;
			d3 = this.data3;
			d4 = this.data4;
		}
		return (d1 * d4) / (d3 * d2);
	}
	private CalcOddsRatioSE(): number {
		let d1: number = 0, d2: number = 0, d3: number = 0, d4: number = 0;
		if ((this.data1 == 0) || (this.data2 == 0) || (this.data3 == 0) || (this.data4 == 0)) {
			d1 = this.data1 + 0.5;
			d2 = this.data2 + 0.5;
			d3 = this.data3 + 0.5;
			d4 = this.data4 + 0.5;
		}
		else {
			d1 = Number(this.data1);
			d2 = Number(this.data2);
			d3 = Number(this.data3);
			d4 = Number(this.data4);
		}
		return Math.sqrt(Number(1 / d1) + Number(1 / d2) + Number(1 / d3) + Number(1 / d4));
	}
	private CalcRiskRatio(): number {
		let d1: number = 0, d2: number = 0, d3: number = 0, d4: number  = 0;
		if ((this.data1 == 0) || (this.data2 == 0) || (this.data3 == 0) || (this.data4 == 0)) {
			d1 = this.data1 + 0.5;
			d2 = this.data2 + 0.5;
			d3 = this.data3 + 0.5;
			d4 = this.data4 + 0.5;
		}
		else {
			d1 = this.data1;
			d2 = this.data2;
			d3 = this.data3;
			d4 = this.data4;
		}
		return (d1 / (d1 + d3)) / (d2 / (d2 + d4));
	}
	private CalcRiskRatioSE(): number {
		let d1: number = 0, d2: number = 0, d3: number = 0, d4: number = 0;
		if ((this.data1 == 0) || (this.data2 == 0) || (this.data3 == 0) || (this.data4 == 0)) {
			d1 = this.data1 + 0.5;
			d2 = this.data2 + 0.5;
			d3 = this.data3 + 0.5;
			d4 = this.data4 + 0.5;
		}
		else {
			d1 = this.data1;
			d2 = this.data2;
			d3 = this.data3;
			d4 = this.data4;
		}
		return Math.sqrt((1 / d1) + (1 / d2) - (1 / (d1 + d3)) - (1 / (d2 + d4)));
	}
	private CalcRiskDifference(): number {
		let d1: number = 0, d2: number = 0, d3: number = 0, d4: number = 0;
		if ((this.data1 == 0) || (this.data2 == 0) || (this.data3 == 0) || (this.data4 == 0)) {
			d1 = this.data1 + 0.5;
			d2 = this.data2 + 0.5;
			d3 = this.data3 + 0.5;
			d4 = this.data4 + 0.5;
		}
		else {
			d1 = this.data1;
			d2 = this.data2;
			d3 = this.data3;
			d4 = this.data4;
		}
		return (d1 / (d1 + d3)) - (d2 / (d2 + d4));
	}
	private CalcRiskDifferenceSE(): number {
		let d1: number = 0, d2: number = 0, d3: number = 0, d4: number = 0;
		if ((this.data1 == 0) || (this.data2 == 0) || (this.data3 == 0) || (this.data4 == 0)) {
			d1 = this.data1 + 0.5;
			d2 = this.data2 + 0.5;
			d3 = this.data3 + 0.5;
			d4 = this.data4 + 0.5;
		}
		else {
			d1 = this.data1;
			d2 = this.data2;
			d3 = this.data3;
			d4 = this.data4;
		}
		return Math.sqrt((d1 * d3 / Math.pow((d1 + d3), 3)) + (d2 * d4 / Math.pow((d2 + d4), 3)));
	}
	private CalcPetoOR(): number {
		let d1: number = 0, d2: number = 0, d3: number = 0, d4: number = 0;
		if ((this.data1 == 0) || (this.data2 == 0) || (this.data3 == 0) || (this.data4 == 0)) {
			d1 = this.data1 + 0.5;
			d2 = this.data2 + 0.5;
			d3 = this.data3 + 0.5;
			d4 = this.data4 + 0.5;
		}
		else {
			d1 = this.data1;
			d2 = this.data2;
			d3 = this.data3;
			d4 = this.data4;
		}
		let n1: number = d1 + d3;
		let n2: number = d2 + d4;
		let n: number = n1 + n2;

		let v: number = (n1 * n2 * (d1 + d2) * (d3 + d4)) / (n * n * (n - 1));
		let e: number = n1 * (d1 + d2) / n;
		return Math.exp((d1 - e) / v);
	}
	private CalcPetoORSE(): number  {
		let d1: number = 0, d2: number = 0, d3: number = 0, d4: number  = 0;
		if ((this.data1 == 0) || (this.data2 == 0) || (this.data3 == 0) || (this.data4 == 0)) {
			d1 = this.data1 + 0.5;
			d2 = this.data2 + 0.5;
			d3 = this.data3 + 0.5;
			d4 = this.data4 + 0.5;
		}
		else {
			d1 = this.data1;
			d2 = this.data2;
			d3 = this.data3;
			d4 = this.data4;
		}
		let n1: number  = d1 + d3;
		let n2: number  = d2 + d4;
		let n: number = n1 + n2;

		let v: number  = (n1 * n2 * (d1 + d2) * (d3 + d4)) / (n * n * (n - 1));
		return Math.sqrt(1 / v);
	}
	private PoolSDs(n1: number, n2: number, sd1: number, sd2: number): number {
		n1 = Number(n1);
		n2 = Number(n2);
		sd1 = Number(sd1);
		sd2 = Number(sd2);
		if (n1 + n2 < 3) {
			return 0;
		}
		let part1Of1: number = ((n1 - 1) * sd1 * sd1);
		let part2Of1: number = ((n2 - 1) * sd2 * sd2);
		let part1OfS: number = (part1Of1 + part2Of1);
		let part2OfS: number = 0;
		part2OfS = n1 + n2 -2;
		let s: number = part1OfS / part2OfS;
		let ans: number = Math.sqrt(s);
		return ans;
	}
	private SdFromSe(se: number, n: number): number {
		return se * Math.sqrt(n);
	}
	private SeFromCi(CIUpper: number, CILower: number): number {
		let se: number = Math.abs((0.5 * (CIUpper - CILower)) / 1.96);
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
	private OutcomeTypeName: string = "Manual entry";
	public get outcomeTypeName(): string {

		return this.OutcomeTypeName;
	}
	public set outcomeTypeName(val: string) {

		this.OutcomeTypeName = val;
	}
	private OutcomeTypeId: number = 0;
	public get outcomeTypeId(): number {

		return this.OutcomeTypeId;
	}
	public set outcomeTypeId(val: number) {

		this.OutcomeTypeId = val;
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
	OutcomeDescription: string = "";
    private Data1: number = 0;
    public get data1(): number {
		return Number(this.Data1);
    }
    public set data1(val: number) {
        this.Data1 = val;
        this.SetCalculatedValues();
    }
	private Data2: number = 0;
	public get data2(): number {
		return Number(this.Data2);
	}
	public set data2(val: number) {
		this.Data2 = val;
		this.SetCalculatedValues();
	}
	private Data3: number = 0;
	public get data3(): number {
		return Number(this.Data3);
	}
	public set data3(val: number) {
		this.Data3 = val;
		this.SetCalculatedValues();
	}
	private Data4: number = 0;
	public get data4(): number {
		return Number(this.Data4);
	}
	public set data4(val: number) {
		this.Data4 = val;
		this.SetCalculatedValues();
	}
	private Data5: number = 0;
	public get data5(): number {
		return Number(this.Data5);
	}
	public set data5(val: number) {
		this.Data5 = val;
		this.SetCalculatedValues();
	}
	private Data6: number = 0;
	public get data6(): number {
		return this.Data6;
	}
	public set data6(val: number) {
		this.Data6 = val;
		this.SetCalculatedValues();
	}
	private Data7: number = 0;
	public get data7(): number {
		return this.Data7;
	}
	public set data7(val: number) {
		this.Data7 = val;
		this.SetCalculatedValues();
	}
	private Data8: number = 0;
	public get data8(): number {
		return this.Data8;
	}
	public set data8(val: number) {
		this.Data8 = val;
		this.SetCalculatedValues();
	}
	private Data9: number = 0;
	public get data9(): number {
		return this.Data9;
	}
	public set data9(val: number) {
		this.Data9 = val;
		this.SetCalculatedValues();
	}
	private Data10: number = 0;
	public get data10(): number {
		return this.Data10;
	}
	public set data10(val: number) {
		this.Data10 = val;
		this.SetCalculatedValues();
	}
	private Data11: number = 0;
	public get data11(): number {
		return this.Data11;
	}
	public set data11(val: number) {
		this.Data11 = val;
		this.SetCalculatedValues();
	}
	private Data12: number = 0;
	public get data12(): number {
		return this.Data12;
	}
	public set data12(val: number) {
		this.Data12 = val;
		this.SetCalculatedValues();
	}
	private Data13: number = 0;
	public get data13(): number {
		return this.Data13;
	}
	public set data13(val: number) {
		this.Data13 = val;
		this.SetCalculatedValues();
	}
	private Data14: number = 0;
	public get data14(): number {
		return this.Data14;
	}
	public set data14(val: number) {
		this.Data14 = val;
		this.SetCalculatedValues();
	}
	interventionText: string = "";
	controlText: string = "";
	OutcomeText: string = "";
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
	CIUpperSMD: number = 0;
	CILowerSMD: number = 0;
	CIUpperR: number = 0;
	CILowerR: number = 0;
	CIUpperOddsRatio: number = 0;
	CILowerOddsRatio: number = 0;
	CIUpperRiskRatio: number = 0;
	CILowerRiskRatio: number = 0;
	CIUpperRiskDifference: number = 0;
	CILowerRiskDifference: number = 0;
	CIUpperPetoOddsRatio: number = 0;
	CILowerPetoOddsRatio: number = 0;
	CIUpperMeanDifference: number = 0;
	CILowerMeanDifference: number = 0;
	riskDifference: number = 0;
	seRiskDifference: number = 0;
	meanDifference: number = 0;
	seMeanDifference: number = 0;
	petoOR: number = 0;
	sePetoOR: number = 0;
	es: number = 0;
	sees: number = 0;
	nRows: number = 0;
	CILower: number = 0;
	CIUpper: number = 0;
	esDesc: string = "";
	seDesc: string = "";
	private Data1Desc: string = "";
	public get data1Desc(): string {
		return this.Data1Desc;
	}
	public set data1Desc(val: string) {
		this.Data1Desc = val;
		//this.SetCalculatedValues();
	}
	private Data2Desc: string = "";
	public get data2Desc(): string {
		return this.Data2Desc;
	}
	public set data2Desc(val: string) {
		this.Data2Desc = val;
		//this.SetCalculatedValues();
	}
	private Data3Desc: string = "";
	public get data3Desc(): string {
		return this.Data3Desc;
	}
	public set data3Desc(val: string) {
		this.Data3Desc = val;
		//this.SetCalculatedValues();
	}
	private Data4Desc: string ="";
	public get data4Desc(): string {
		return this.Data4Desc;
	}
	public set data4Desc(val: string) {
		this.Data4Desc = val;
		//this.SetCalculatedValues();
	}
	private Data5Desc: string = "";
	public get data5Desc(): string {
		return this.Data5Desc;
	}
	public set data5Desc(val: string) {
		this.Data5Desc = val;
		//this.SetCalculatedValues();
	}
	private Data6Desc: string = "";
	public get data6Desc(): string {
		return this.Data6Desc;
	}
	public set data6Desc(val: string) {
		this.Data6Desc = val;
		//this.SetCalculatedValues();
	}
	private Data7Desc: string = "";
	public get data7Desc(): string {
		return this.Data7Desc;
	}
	public set data7Desc(val: string) {
		this.Data7Desc = val;
		//this.SetCalculatedValues();
	}
	private Data8Desc: string = "";
	public get data8Desc(): string {
		return this.Data8Desc;
	}
	public set data8Desc(val: string) {
		this.Data8Desc = val;
		//this.SetCalculatedValues();
	}
	private Data9Desc: string = "";
	public get data9Desc(): string {
		return this.Data9Desc;
	}
	public set data9Desc(val: string) {
		this.Data9Desc = val;
		//this.SetCalculatedValues();
	}
	private Data10Desc: string = "";
	public get data10Desc(): string {
		return this.Data10Desc;
	}
	public set data10Desc(val: string) {
		this.Data10Desc = val;
		//this.SetCalculatedValues();
	}
	private Data11Desc: string = "";
	public get data11Desc(): string {
		return this.Data11Desc;
	}
	public set data11Desc(val: string) {
		this.Data11Desc = val;
		//this.SetCalculatedValues();
	}
	private Data12Desc: string ="";
	public get data12Desc(): string {
		return this.Data12Desc;
	}
	public set data12Desc(val: string) {
		this.Data12Desc = val;
		//this.SetCalculatedValues();
	}
	private Data13Desc: string = "";
	public get data13Desc(): string {
		return this.Data13Desc;
	}
	public set data13Desc(val: string) {
		this.Data13Desc = val;
		//this.SetCalculatedValues();
	}
	private Data14Desc: string = "";
	public get data14Desc(): string {
		return this.Data14Desc;
	}
	public set data14Desc(val: string) {
		this.Data14Desc = val;
		//this.SetCalculatedValues();
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

export class StatFunctions {

	public static qnorm(p: number, upper: boolean): number {
		/* Reference:
		   J. D. Beasley and S. G. Springer 
		   Algorithm AS 111: "The Percentage Points of the Normal Distribution"
		   Applied Statistics
		*/
		//if(p<0 || p>1)
		//			throw new IllegalArgumentException("Illegal argument "+p+" for qnorm(p).");
		let split: number = 0.42;
		let a0: number = 2.50662823884;
		let a1: number = -18.61500062529;
		let a2: number = 41.39119773534;
		let a3: number = -25.44106049637;
		let b1: number = -8.47351093090;
		let b2: number = 23.08336743743;
		let b3: number = -21.06224101826;
		let b4: number = 3.13082909833;
		let c0: number = -2.78718931138;
		let c1: number = -2.29796479134;
		let c2: number = 4.85014127135;
		let c3: number = 2.32121276858;
		let d1: number = 3.54388924762;
		let d2: number = 1.63706781897;
		let q: number = p - 0.5;
		let r: number, ppnd: number;
		if (Math.abs(q) <= split) {
			r = q * q;
			ppnd = q * (((a3 * r + a2) * r + a1) * r + a0) / ((((b4 * r + b3) * r + b2) * r + b1) * r + 1);
		}
		else {
			r = p;
			if (q > 0) r = 1 - p;
			if (r > 0) {
				r = Math.sqrt(-Math.log(r));
				ppnd = (((c3 * r + c2) * r + c1) * r + c0) / ((d2 * r + d1) * r + 1);
				if (q < 0) ppnd = -ppnd;
			}
			else {
				ppnd = 0;
			}
		}
		if (upper) ppnd = 1 - ppnd;
		return (ppnd);
	}


	public static pnorm(z: number, upper: boolean): number{
		// Reference:
		//  I. D. Hill 
		//  Algorithm AS 66: "The Normal Integral"
		//  Applied Statistics
		let ltone: number = 7.0,
			utzero = 18.66,
			con = 1.28,
			a1 = 0.398942280444,
			a2 = 0.399903438504,
			a3 = 5.75885480458,
			a4 = 29.8213557808,
			a5 = 2.62433121679,
			a6 = 48.6959930692,
			a7 = 5.92885724438,
			b1 = 0.398942280385,
			b2 = 3.8052e-8,
			b3 = 1.00000615302,
			b4 = 3.98064794e-4,
			b5 = 1.986153813664,
			b6 = 0.151679116635,
			b7 = 5.29330324926,
			b8 = 4.8385912808,
			b9 = 15.1508972451,
			b10 = 0.742380924027,
			b11 = 30.789933034,
			b12 = 3.99019417011;
		let y: number, alnorm: number;

		if (z < 0) {
			upper = !upper;
			z = -z;
		}
		if (z <= ltone || upper && z <= utzero) {
			y = 0.5 * z * z;
			if (z > con) {
				alnorm = b1 * Math.exp(-y) / (z - b2 + b3 / (z + b4 + b5 / (z - b6 + b7 / (z + b8 - b9 / (z + b10 + b11 / (z + b12))))));
			}
			else {
				alnorm = 0.5 - z * (a1 - a2 * y / (y + a3 - a4 / (y + a5 + a6 / (y + a7))));
			}
		}
		else {
			alnorm = 0;
		}
		if (!upper) alnorm = 1 - alnorm;
		return (alnorm);
	}


	public static qt(p: number, ndf: number, lower_tail: boolean) {
		// Algorithm 396: Student's t-quantiles by
		// G.W. Hill CACM 13(10), 619-620, October 1970
		//	if(p<=0 || p>=1 || ndf<1) 
		//		throw new IllegalArgumentException("Invalid p or df in call to qt(double,double,boolean).");
		let eps: number = 1e-12;
		let M_PI_2: number = 1.570796326794896619231321691640; // pi/2
		let neg: boolean;
		let P: number, q: number, prob: number, a: number, b: number, c: number, d: number, y: number, x: number;
		if ((lower_tail && p > 0.5) || (!lower_tail && p < 0.5)) {
			neg = false;
			P = 2 * (lower_tail ? (1 - p) : p);
		}
		else {
			neg = true;
			P = 2 * (lower_tail ? p : (1 - p));
		}

		if (Math.abs(ndf - 2) < eps) {   /* df ~= 2 */
			q = Math.sqrt(2 / (P * (2 - P)) - 2);
		}
		else if (ndf < 1 + eps) {   /* df ~= 1 */
			prob = P * M_PI_2;
			q = Math.cos(prob) / Math.sin(prob);
		}
		else {      /*-- usual case;  including, e.g.,  df = 1.1 */
			a = 1 / (ndf - 0.5);
			b = 48 / (a * a);
			c = ((20700 * a / b - 98) * a - 16) * a + 96.36;
			d = ((94.5 / (b + c) - 3) / b + 1) * Math.sqrt(a * M_PI_2) * ndf;
			y = Math.pow(d * P, 2 / ndf);
			if (y > 0.05 + a) {
				/* Asymptotic inverse expansion about normal */
				x = StatFunctions.qnorm(0.5 * P, false);
				y = x * x;
				if (ndf < 5)
					c += 0.3 * (ndf - 4.5) * (x + 0.6);
				c = (((0.05 * d * x - 5) * x - 7) * x - 2) * x + b + c;
				y = (((((0.4 * y + 6.3) * y + 36) * y + 94.5) / c - y - 3) / b + 1) * x;
				y = a * y * y;
				if (y > 0.002)/* FIXME: This cutoff is machine-preCIsion dependent*/
					y = Math.exp(y) - 1;
				else { /* Taylor of    e^y -1 : */
					y = (0.5 * y + 1) * y;
				}
			}
			else {
				y = ((1 / (((ndf + 6) / (ndf * y) - 0.089 * d - 0.822)
					* (ndf + 2) * 3) + 0.5 / (ndf + 4))
					* y - 1) * (ndf + 1) / (ndf + 2) + 1 / y;
			}
			q = Math.sqrt(ndf * y);
		}
		if (neg) q = -q;
		return q;
	}

	public static pt(t: number, df: number): number {
		// ALGORITHM AS 3  APPL. STATIST. (1968) VOL.17, P.189
		// Computes P(T<t)
		let a: number, b: number, idf: number, im2: number,
			ioe: number, s: number, c: number, ks: number, fk: number, k: number;

		let g1: number = 0.3183098862;// =1/pi;

		idf = df;
		a = t / Math.sqrt(idf);
		b = idf / (idf + t * t);
		im2 = df - 2;
		ioe = idf % 2;
		s = 1;
		c = 1;
		idf = 1;
		ks = 2 + ioe;
		fk = ks;
		if (im2 >= 2) {
			for (k = ks; k <= im2; k += 2) {
				c = c * b * (fk - 1) / fk;
				s += c;
				if (s != idf) {
					idf = s;
					fk += 2;
				}
			}
		}
		if (ioe != 1)
			return 0.5 + 0.5 * a * Math.sqrt(b) * s;
		if (df == 1)
			s = 0;
		return 0.5 + (a * b * s + Math.atan(a)) * g1;
	}

	public static pchisq(q: number, df: number): number {
		// Posten, H. (1989) American StatistiCIan 43 p. 261-265
		let df2: number = df * .5;
		let q2: number = q * .5;
		let n: number = 5, k: number;
		let tk: number, CFL: number, CFU: number, prob: number;
		//if(q<=0 || df<=0)
		//	throw new IllegalArgumentException("Illegal argument "+q+" or "+df+" for qnorm(p).");
		if (q < df) {
			tk = q2 * (1 - n - df2) / (df2 + 2 * n - 1 + n * q2 / (df2 + 2 * n));
			for (k = n - 1; k > 1; k--)
				tk = q2 * (1 - k - df2) / (df2 + 2 * k - 1 + k * q2 / (df2 + 2 * k + tk));
			CFL = 1 - q2 / (df2 + 1 + q2 / (df2 + 2 + tk));
			prob = Math.exp(df2 * Math.log(q2) - q2 - this.lnfgamma(df2 + 1) - Math.log(CFL));
		}
		else {
			tk = (n - df2) / (q2 + n);
			for (k = n - 1; k > 1; k--)
				tk = (k - df2) / (q2 + k / (1 + tk));
			CFU = 1 + (1 - df2) / (q2 + 1 / (1 + tk));
			prob = 1 - Math.exp((df2 - 1) * Math.log(q2) - q2 - this.lnfgamma(df2) - Math.log(CFU));
		}
		return prob;
	}



	/*  POCHISQ  --  probability of chi-square value

		  Adapted from:
				  Hill, I. D. and Pike, M. C.  Algorithm 299
				  Collected Algorithms for the CACM 1967 p. 243
		  Updated for rounding errors based on remark in
				  ACM TOMS June 1985, page 185
	 * 
	 * downloaded from: http://www.fourmilab.ch/rpkp/experiments/analysis/chiCalc.html
*/

	public static pochisq(x: number, df: number) {
		let a: number, s: number;
		let y: number = 0;
		let e: number, c: number, z: number;
		let even: boolean;                     /* True if df is an even number */
		let BIGX: number = 20.0;                  /* max value to represent exp(x) */

		const LOG_SQRT_PI: number = 0.5723649429247000870717135; /* log(sqrt(pi)) */
		const I_SQRT_PI: number = 0.5641895835477562869480795;   /* 1 / sqrt(pi) */

		if (x <= 0.0 || df < 1) {
			return 1.0;
		}

		a = 0.5 * x;
		even = (df % 2 == 0) ? true : false;
		if (df > 1) {
			y = this.ex(-a);
		}
		s = (even ? y : (2.0 * this.poz(-Math.sqrt(x))));
		if (df > 2) {
			x = 0.5 * (df - 1.0);
			z = (even ? 1.0 : 0.5);
			if (a > BIGX) {
				e = (even ? 0.0 : LOG_SQRT_PI);
				c = Math.log(a);
				while (z <= x) {
					e = Math.log(z) + e;
					s += this.ex(c * z - a - e);
					z += 1.0;
				}
				return s;
			} else {
				e = (even ? 1.0 : (I_SQRT_PI / Math.sqrt(a)));
				c = 0.0;
				while (z <= x) {
					e = e * (a / z);
					c = c + e;
					z += 1.0;
				}
				return c * y + s;
			}
		} else {
			return s;
		}
	}

	public static ex(x: number): number {
		let BIGX: number = 20.0;                  /* max value to represent exp(x) */

		return (x < -BIGX) ? 0.0 : Math.exp(x);
	}

	public static poz(z: number): number {
		let y: number, x: number, w: number;
		let Z_MAX: number = 6.0;              /* Maximum meaningful z value */

		if (z == 0.0) {
			x = 0.0;
		} else {
			y = 0.5 * Math.abs(z);
			if (y >= (Z_MAX * 0.5)) {
				x = 1.0;
			} else if (y < 1.0) {
				w = y * y;
				x = ((((((((0.000124818987 * w
					- 0.001075204047) * w + 0.005198775019) * w
					- 0.019198292004) * w + 0.059054035642) * w
					- 0.151968751364) * w + 0.319152932694) * w
					- 0.531923007300) * w + 0.797884560593) * y * 2.0;
			} else {
				y -= 2.0;
				x = (((((((((((((-0.000045255659 * y
					+ 0.000152529290) * y - 0.000019538132) * y
					- 0.000676904986) * y + 0.001390604284) * y
					- 0.000794620820) * y - 0.002034254874) * y
					+ 0.006549791214) * y - 0.010557625006) * y
					+ 0.011630447319) * y - 0.009279453341) * y
					+ 0.005353579108) * y - 0.002141268741) * y
					+ 0.000535310849) * y + 0.999936657524;
			}
		}
		return z > 0.0 ? ((x + 1.0) * 0.5) : ((1.0 - x) * 0.5);
	}

	public static lnfgamma(c: number) {
		let j: number;
		let x: number, y: number, tmp: number, ser: number;
		let cof: number[] = [76.18009172947146 ,-86.50532032941677,
				24.01409824083091, -1.231739572450155,
				0.1208650973866179e-2, -0.5395239384953e-5
				];
				y = x = c;
				tmp = x + 5.5 - (x + 0.5) * Math.log(x + 5.5);
				ser = 1.000000000190015;
				for(j = 0; j<= 5; j++)
			ser += (cof[j] / ++y);
			return (Math.log(2.5066282746310005 * ser / x) - tmp);
		}

	private constructor() { /* This works; without this line you can instanitate */ }

}