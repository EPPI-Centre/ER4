import { Inject, Injectable, EventEmitter, Output, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { SetAttribute } from './ReviewSets.service';
import { iTimePoint } from './timePoints.service';
import { StatFunctions } from '../helpers/StatisticsMethods';
import { Subscription } from 'rxjs';

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
	public ItemSetId: number = 0;
	public currentOutcome: Outcome = new Outcome();
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

	public FetchOutcomes(ItemSetId: number): Subscription {
		this._BusyMethods.push("FetchOutcomes");
		let body = JSON.stringify({ Value: ItemSetId });
		this._Outcomes = [];
		return this._http.post<iOutcomeList>(this._baseUrl + 'api/OutcomeList/Fetch', body)
			.subscribe(result => {

            for (let iO of result.outcomesList) {
   
				let RealOutcome: Outcome = new Outcome(iO);
                this._Outcomes.push(RealOutcome);
       		}
			
            this.RemoveBusy("FetchOutcomes");
        }, error => {

            this.modalService.SendBackHomeWithError(error);
            this.RemoveBusy("FetchOutcomes");

			},
			() => {
				this.RemoveBusy("FetchOutcomes");
			});

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
			},
			() => {
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
			},
			() => {
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
			},
			() => {
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
			},
			() => {
				this.RemoveBusy("FetchItemArmList");
			}
			);
	}

	public listOutcomes: Outcome[] = [];

	public Createoutcome(currentoutcome: Outcome): Promise<Outcome> {

		this._BusyMethods.push("CreateOutcome");
		let ErrMsg = "Something went wrong when creating an outcome. \r\n If the problem persists, please contact EPPISupport.";

		//console.log('did call this...');
		return this._http.post<iOutcome>(this._baseUrl + 'api/OutcomeList/Createoutcome',

			currentoutcome).toPromise()
						.then(
						(result) => {

							//console.log('did get results....');
							var newOutcome: Outcome = new Outcome(result);
							this.outcomesList.push(newOutcome);
							
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

		//console.log('outcome codes are: ' + JSON.stringify(currentOutcome.outcomeCodes));
		this._BusyMethods.push("UpdateOutcome");
		let ErrMsg = "Something went wrong when updating an outcome. \r\n If the problem persists, please contact EPPISupport.";

		this._http.post<iOutcome>(this._baseUrl + 'api/OutcomeList/UpdateOutcome',

			currentOutcome).subscribe(

			(result) => {

					this.RemoveBusy("UpdateOutcome");
					if (!result) {
						this.modalService.GenericErrorMessage(ErrMsg);
						this.FetchOutcomes(currentOutcome.itemSetId);
						return;
					}
					var updateOutcome: Outcome = new Outcome(result);
					return updateOutcome;
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
		this._http.post<iOutcome>(this._baseUrl + 'api/OutcomeList/DeleteOutcome',

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
export class Outcome implements iOutcome {

	unifiedOutcomeTypeId: number = 0;
	manuallyEnteredOutcomeTypeId: number = 0;
	grp1ArmName: string = '';
	grp2ArmName: string = '';
	isSelected: boolean = false;
	canSelect: boolean = false;
	outcomeCodes: OutcomeItemAttributesList = new OutcomeItemAttributesList();
    public constructor(iO?: iOutcome) {
		if (iO) {

			this.itemSetId = iO.itemSetId;
			this.OutcomeTypeId = iO.outcomeTypeId;
			this.manuallyEnteredOutcomeTypeId = iO.manuallyEnteredOutcomeTypeId;
			this.unifiedOutcomeTypeId = iO.unifiedOutcomeTypeId;
			this.OutcomeTypeName = iO.outcomeTypeName;
			this.itemAttributeIdIntervention =  iO.itemAttributeIdIntervention;
			this.itemAttributeIdControl = iO.itemAttributeIdControl;
			this.itemAttributeIdOutcome = iO.itemAttributeIdOutcome
			this.title = iO.title;
			//console.log('adding an outcome with codes: ', iO);
			if (iO.outcomeCodes != undefined) {
				for (var i = 0; i < iO.outcomeCodes.outcomeItemAttributesList.length; i++) {
					let tmpCode: OutcomeItemAttribute = iO.outcomeCodes.outcomeItemAttributesList[i];
					if (this.outcomeCodes.outcomeItemAttributesList != undefined) {
						//console.log('got inside outcome codes adding: ', tmpCode);
						this.outcomeCodes.outcomeItemAttributesList.push(tmpCode);
						
					}
				}
			}
			this.shortTitle = iO.shortTitle;
			this.outcomeDescription = iO.outcomeDescription
            this.outcomeId = iO.outcomeId;
			this.data1 = Number(iO.data1 == null ? 0 : iO.data1);
			this.data2 = Number(iO.data2 == null ? 0 : iO.data2);
			this.data3 = Number(iO.data3 == null ? 0 : iO.data3);
			this.data4 = Number(iO.data4 == null ? 0 : iO.data4);
			this.data5 = Number(iO.data5 == null ? 0 : iO.data5);
			this.data6 = Number(iO.data6 == null ? 0 : iO.data6);
			this.data7 = Number(iO.data7 == null ? 0 : iO.data7);
			this.data8 = Number(iO.data8 == null ? 0 : iO.data8);
			this.data9 = Number(iO.data9 == null ? 0 : iO.data9);
			if (this.data9 != null && this.data9 > 0) {
				this.isSelected = true;
			} else {
				this.isSelected = false;
			}
			this.data10 = Number(iO.data10 == null ? 0 : iO.data10);
			this.data11 = Number(iO.data11 == null ? 0 : iO.data11);
			this.data12 = Number(iO.data12 == null ? 0 : iO.data12);
			this.data13 = Number(iO.data13 == null ? 0 : iO.data13);
			this.data14 = Number(iO.data14 == null ? 0 : iO.data14);
			this.interventionText = iO.interventionText;
			this.controlText = iO.controlText;
			this.outcomeText = iO.outcomeText;
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
				this.seOddsRatio = this.CalcOddsRatioSE();
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
		var tester = Math.sqrt(Number(1 / d1) + Number(1 / d2) + Number(1 / d3) + Number(1 / d4));
		console.log("Odds ratio SE is: ", tester);
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

		//this.SetCalculatedValues();
		return this.OutcomeTypeId;
	}
	public set outcomeTypeId(val: number) {
		this.OutcomeTypeId = val;
		this.SetCalculatedValues();
	}
	NRows: number = 0;
	
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
		if (val != null && val > 0) {
			this.isSelected = true;
		} else {
			this.isSelected = false;
		}
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
	}
	private Data2Desc: string = "";
	public get data2Desc(): string {
		return this.Data2Desc;
	}
	public set data2Desc(val: string) {
		this.Data2Desc = val;
	}
	private Data3Desc: string = "";
	public get data3Desc(): string {
		return this.Data3Desc;
	}
	public set data3Desc(val: string) {
		this.Data3Desc = val;
	}
	private Data4Desc: string ="";
	public get data4Desc(): string {
		return this.Data4Desc;
	}
	public set data4Desc(val: string) {
		this.Data4Desc = val;
	}
	private Data5Desc: string = "";
	public get data5Desc(): string {
		return this.Data5Desc;
	}
	public set data5Desc(val: string) {
		this.Data5Desc = val;
	}
	private Data6Desc: string = "";
	public get data6Desc(): string {
		return this.Data6Desc;
	}
	public set data6Desc(val: string) {
		this.Data6Desc = val;
	}
	private Data7Desc: string = "";
	public get data7Desc(): string {
		return this.Data7Desc;
	}
	public set data7Desc(val: string) {
		this.Data7Desc = val;
	}
	private Data8Desc: string = "";
	public get data8Desc(): string {
		return this.Data8Desc;
	}
	public set data8Desc(val: string) {
		this.Data8Desc = val;
	}
	private Data9Desc: string = "";
	public get data9Desc(): string {
		return this.Data9Desc;
	}
	public set data9Desc(val: string) {
		this.Data9Desc = val;
	}
	private Data10Desc: string = "";
	public get data10Desc(): string {
		return this.Data10Desc;
	}
	public set data10Desc(val: string) {
		this.Data10Desc = val;
	}
	private Data11Desc: string = "";
	public get data11Desc(): string {
		return this.Data11Desc;
	}
	public set data11Desc(val: string) {
		this.Data11Desc = val;
	}
	private Data12Desc: string ="";
	public get data12Desc(): string {
		return this.Data12Desc;
	}
	public set data12Desc(val: string) {
		this.Data12Desc = val;
	}
	private Data13Desc: string = "";
	public get data13Desc(): string {
		return this.Data13Desc;
	}
	public set data13Desc(val: string) {
		this.Data13Desc = val;
	}
	private Data14Desc: string = "";
	public get data14Desc(): string {
		return this.Data14Desc;
	}
	public set data14Desc(val: string) {
		this.Data14Desc = val;
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

