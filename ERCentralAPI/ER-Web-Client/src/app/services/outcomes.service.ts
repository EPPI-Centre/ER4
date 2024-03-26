import { Inject, Injectable, EventEmitter, Output, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { SetAttribute } from './ReviewSets.service';
import { ArmTimepointLinkListService, iTimePoint } from './ArmTimepointLinkList.service';
import { StatFunctions } from '../helpers/StatisticsMethods';
import { lastValueFrom, Subscription } from 'rxjs';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root',
})

export class OutcomesService extends BusyAwareService {

  constructor(
    private _http: HttpClient,
    private modalService: ModalService,
    configService: ConfigService
  ) {
    super(configService);
  }

  public ItemSetId: number = 0;
  public currentOutcome: Outcome = new Outcome();
  public listOutcomes: Outcome[] = [];
  public UnchangedOutcome: Outcome = new Outcome();
  public ShowOutComeList: EventEmitter<SetAttribute> = new EventEmitter();

  public ReviewSetOutcomeList: ReviewSetDropDownResult[] = [];
  public ReviewSetControlList: ReviewSetDropDownResult[] = [];
  public ReviewSetInterventionList: ReviewSetDropDownResult[] = [];
  
  public get currentOutcomeHasChanges(): boolean {
    const curr = this.currentOutcome;
    const un = this.UnchangedOutcome;
    if (curr.outcomeId != un.outcomeId) return true;
    else if (curr.data1 != un.data1
      || curr.data2 != un.data2
      || curr.data3 != un.data3
      || curr.data4 != un.data4
      || curr.data5 != un.data5
      || curr.data6 != un.data6
      || curr.data7 != un.data7
      || curr.data8 != un.data8
      || curr.data9 != un.data9
      || curr.data10 != un.data10
      || curr.data11 != un.data11
      || curr.data12 != un.data12
      || curr.data13 != un.data13
      || curr.data14 != un.data14) return true;
    else if (curr.title != un.title
      || curr.itemTimepointId != un.itemTimepointId
      || curr.outcomeDescription != un.outcomeDescription
      || curr.outcomeTypeId != un.outcomeTypeId
      || curr.itemAttributeIdOutcome != un.itemAttributeIdOutcome
      || curr.itemAttributeIdIntervention != un.itemAttributeIdIntervention
      || curr.itemAttributeIdControl != un.itemAttributeIdControl
      || curr.itemArmIdGrp1 != un.itemArmIdGrp1
      || curr.itemArmIdGrp2 != un.itemArmIdGrp2) return true;
    //es oddsRatio smd r sees seOddsRatio sesmd ser - these depend on DATA_N and outcomeType, so we can skip them, I think
    else if (curr.outcomeCodes.outcomeItemAttributesList.length != un.outcomeCodes.outcomeItemAttributesList.length) return true;
    else {
      for (let i = 1; i < curr.outcomeCodes.outcomeItemAttributesList.length; i++) {
        if (curr.outcomeCodes.outcomeItemAttributesList[i].outcomeItemAttributeId != un.outcomeCodes.outcomeItemAttributesList[i].outcomeItemAttributeId) return true;
      }
    }
    return false;
  }

  public IsServiceBusy(): boolean {

    if (this._BusyMethods.length > 0) {
      return true;
    } else {
      return false;
    }
  }

  //public FetchOutcomes(ItemSetId: number): Subscription {
  //  this._BusyMethods.push("FetchOutcomes");
  //  let body = JSON.stringify({ Value: ItemSetId });
  //  this._Outcomes = [];
  //  return this._http.post<iOutcomeList>(this._baseUrl + 'api/OutcomeList/Fetch', body)
  //    .subscribe(result => {

  //      for (let iO of result.outcomesList) {

  //        let RealOutcome: Outcome = new Outcome(iO);
  //        this._Outcomes.push(RealOutcome);
  //      }

  //      this.RemoveBusy("FetchOutcomes");
  //    }, error => {

  //      this.modalService.SendBackHomeWithError(error);
  //      this.RemoveBusy("FetchOutcomes");

  //    },
  //      () => {
  //        this.RemoveBusy("FetchOutcomes");
  //      });

  //}

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

  

  public Createoutcome(currentoutcome: Outcome): Promise<Outcome | boolean> {

    this._BusyMethods.push("CreateOutcome");
    
    //console.log('did call this...');
    return lastValueFrom(this._http.post<iOutcome>(this._baseUrl + 'api/OutcomeList/Createoutcome', currentoutcome)
    ).then(
        (result) => {
          //console.log('did get results....');
          var newOutcome: Outcome = new Outcome(result);
          //this.outcomesList.push(newOutcome);
          this.RemoveBusy("CreateOutcome");
          return newOutcome;
        }
        , (error) => {
          //this.FetchOutcomes(this._currentItemSetId);
          this.modalService.GenericError(error);
          this.RemoveBusy("CreateOutcome");
          return false;
        }
      )
      .catch(
        (error) => {
          //this.FetchOutcomes(this._currentItemSetId);
          this.modalService.GenericError(error);
          this.RemoveBusy("CreateOutcome");
          return false;
        }
      );
  }

  public Updateoutcome(currentOutcome: Outcome): Promise<Outcome | boolean> {

    //console.log('outcome codes are: ' + JSON.stringify(currentOutcome.outcomeCodes));
    this._BusyMethods.push("UpdateOutcome");
    
    return lastValueFrom(this._http.post<iOutcome>(this._baseUrl + 'api/OutcomeList/UpdateOutcome', currentOutcome)
      ).then(
        (result) => {
          var updateOutcome: Outcome = new Outcome(result);
          this.RemoveBusy("UpdateOutcome");
          return updateOutcome;
        }
        , (error) => {
          //this.FetchOutcomes(currentOutcome.itemSetId);
          this.modalService.GenericError(error);
          this.RemoveBusy("UpdateOutcome");
          return false;
        });
  }

  public DeleteOutcome(outcomeId: number, itemSetId: number, key: number): Promise<boolean> {

    this._BusyMethods.push("DeleteOutcome");

    let body = JSON.stringify({ outcomeId: outcomeId, itemSetId: itemSetId });
    return lastValueFrom(this._http.post<iOutcome>(this._baseUrl + 'api/OutcomeList/DeleteOutcome', body)
      ).then(
        (result) => {

          //this.outcomesList.splice(key, 1);
          this.RemoveBusy("DeleteOutcome");
          return true;
        }
        , (error) => {

          //this.FetchOutcomes(this._currentItemSetId);
          this.modalService.GenericError(error);
          this.RemoveBusy("DeleteOutcome");
          return false;
        }
      );
  }
  public Clear() {
    this.currentOutcome = new Outcome();
    this.listOutcomes = [];
    this.UnchangedOutcome = new Outcome();
    this.ReviewSetOutcomeList = [];
    this.ReviewSetControlList = [];
    this.ReviewSetInterventionList = [];
  }
}


//export class ItemArm {

//  itemArmId: number = 0;
//  title: string = '';

//}
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
      this.outcomeTypeId = iO.outcomeTypeId;
      this.manuallyEnteredOutcomeTypeId = iO.manuallyEnteredOutcomeTypeId;
      this.unifiedOutcomeTypeId = iO.unifiedOutcomeTypeId;
      this.outcomeTypeName = iO.outcomeTypeName;
      this.itemAttributeIdIntervention = iO.itemAttributeIdIntervention;
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
      this.Data1 = Number(iO.data1 == null ? 0 : iO.data1);
      this.Data2 = Number(iO.data2 == null ? 0 : iO.data2);
      this.Data3 = Number(iO.data3 == null ? 0 : iO.data3);
      this.Data4 = Number(iO.data4 == null ? 0 : iO.data4);
      this.Data5 = Number(iO.data5 == null ? 0 : iO.data5);
      this.Data6 = Number(iO.data6 == null ? 0 : iO.data6);
      this.Data7 = Number(iO.data7 == null ? 0 : iO.data7);
      this.Data8 = Number(iO.data8 == null ? 0 : iO.data8);
      this.Data9 = Number(iO.data9 == null ? 0 : iO.data9);
      if (this.data9 != null && this.data9 > 0) {
        this.isSelected = true;
      } else {
        this.isSelected = false;
      }
      this.Data10 = Number(iO.data10 == null ? 0 : iO.data10);
      this.Data11 = Number(iO.data11 == null ? 0 : iO.data11);
      this.Data12 = Number(iO.data12 == null ? 0 : iO.data12);
      this.Data13 = Number(iO.data13 == null ? 0 : iO.data13);
      this.Data14 = Number(iO.data14 == null ? 0 : iO.data14);
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

      this.ciUpperSMD = iO.ciUpperSMD;
      this.ciLowerSMD = iO.ciLowerSMD;
      this.ciUpperR = iO.ciUpperR;
      this.ciLowerR = iO.ciLowerR;
      this.ciUpperOddsRatio = iO.ciUpperOddsRatio;
      this.ciLowerOddsRatio = iO.ciLowerOddsRatio;
      this.ciUpperRiskRatio = iO.ciUpperRiskRatio;
      this.ciLowerRiskRatio = iO.ciLowerRiskRatio;
      this.ciUpperRiskDifference = iO.ciUpperRiskDifference;
      this.ciLowerRiskDifference = iO.ciUpperRiskDifference;
      this.ciUpperPetoOddsRatio = iO.ciUpperPetoOddsRatio;
      this.ciLowerPetoOddsRatio = iO.ciLowerPetoOddsRatio;
      this.ciUpperMeanDifference = iO.ciUpperMeanDifference;
      this.ciLowerMeanDifference = iO.ciLowerMeanDifference;
      this.ciUpper = iO.ciUpper;
      this.ciLower = iO.ciLower;
      this.nRows = iO.nRows;
    }
  }

  public SetCalculatedValues() {

    this.SetEffectSizes();
    switch (this.outcomeTypeId) {
      case 0: // manual entry
        this.esDesc = "Effect size";
        this.seDesc = "SE";
        this.nRows = 6;
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
        this.nRows = 3;
        this.data1Desc = "Group 1 N";
        this.data2Desc = "Group 2 N";
        this.data3Desc = "Group 1 mean";
        this.data4Desc = "Group 2 mean";
        this.data5Desc = "Group 1 SD";
        this.data6Desc = "Group 2 SD";
        this.data7Desc = "";
        this.data8Desc = "";
        this.data9Desc = "";
        this.data10Desc = "";
        this.data11Desc = "";
        this.data12Desc = "";
        this.data13Desc = "";
        this.data14Desc = "";
        this.outcomeTypeName = "Continuous";
        break;

      case 2: // binary 2 x 2 table
        this.esDesc = "OR";
        this.seDesc = "SE (log OR)";
        this.nRows = 2;
        this.data1Desc = "Group 1 events";
        this.data2Desc = "Group 2 events";
        this.data3Desc = "Group 1 no events";
        this.data4Desc = "Group 2 no events";
        this.data5Desc = "";
        this.data6Desc = "";
        this.data7Desc = "";
        this.data8Desc = "";
        this.data9Desc = "";
        this.data10Desc = "";
        this.data11Desc = "";
        this.data12Desc = "";
        this.data13Desc = "";
        this.data14Desc = "";
        this.outcomeTypeName = "Binary";
        break;

      case 3: //n, mean SE
        this.esDesc = "SMD";
        this.seDesc = "SE";
        this.nRows = 3;
        this.data1Desc = "Group 1 N";
        this.data2Desc = "Group 2 N";
        this.data3Desc = "Group 1 mean";
        this.data4Desc = "Group 2 mean";
        this.data5Desc = "Group 1 SE";
        this.data6Desc = "Group 2 SE";
        this.data7Desc = "";
        this.data8Desc = "";
        this.data9Desc = "";
        this.data10Desc = "";
        this.data11Desc = "";
        this.data12Desc = "";
        this.data13Desc = "";
        this.data14Desc = "";
        this.outcomeTypeName = "Continuous";
        break;

      case 4: //n, mean CI
        this.esDesc = "SMD";
        this.seDesc = "SE";
        this.nRows = 4;
        this.data1Desc = "Group 1 N";
        this.data2Desc = "Group 2 N";
        this.data3Desc = "Group 1 mean";
        this.data4Desc = "Group 2 mean";
        this.data5Desc = "Group 1 CI lower";
        this.data6Desc = "Group 2 CI lower";
        this.data7Desc = "Group 1 CI upper";
        this.data8Desc = "Group 2 CI upper";
        this.data9Desc = "";
        this.data10Desc = "";
        this.data11Desc = "";
        this.data12Desc = "";
        this.data13Desc = "";
        this.data14Desc = "";
        this.outcomeTypeName = "Continuous";
        break;

      case 5: //n, t or p value
        this.esDesc = "SMD";
        this.seDesc = "SE";
        this.nRows = 2;
        this.data1Desc = "Group 1 N";
        this.data2Desc = "Group 2 N";
        this.data3Desc = "t-value";
        this.data4Desc = "p-value";
        this.data5Desc = "";
        this.data6Desc = "";
        this.data7Desc = "";
        this.data8Desc = "";
        this.data9Desc = "";
        this.data10Desc = "";
        this.data11Desc = "";
        this.data12Desc = "";
        this.data13Desc = "";
        this.data14Desc = "";
        this.outcomeTypeName = "Continuous";
        break;

      case 6: // diagnostic test 2 x 2 table
        this.esDesc = "Diagnostic OR";
        this.seDesc = "SE";
        this.nRows = 2;
        this.data1Desc = "True positive";
        this.data2Desc = "False positive";
        this.data3Desc = "False negative";
        this.data4Desc = "True negative";
        this.data5Desc = "";
        this.data6Desc = "";
        this.data7Desc = "";
        this.data8Desc = "";
        this.data9Desc = "";
        this.data10Desc = "";
        this.data11Desc = "";
        this.data12Desc = "";
        this.data13Desc = "";
        this.data14Desc = "";
        this.outcomeTypeName = "Binary";
        break;

      case 7: // correlation coeffiCIent r
        this.esDesc = "r";
        this.seDesc = "SE (Z transformed)";
        this.nRows = 1;
        this.data1Desc = "group(s) size";
        this.data2Desc = "correlation";
        this.data3Desc = "";
        this.data4Desc = "";
        this.data5Desc = "";
        this.data6Desc = "";
        this.data7Desc = "";
        this.data8Desc = "";
        this.data9Desc = "";
        this.data10Desc = "";
        this.data11Desc = "";
        this.data12Desc = "";
        this.data13Desc = "";
        this.data14Desc = "";
        this.outcomeTypeName = "Correlation";
        break;

      default:
        this.data1Desc = "";
        this.data2Desc = "";
        this.data3Desc = "";
        this.data4Desc = "";
        this.data5Desc = "";
        this.data6Desc = "";
        this.data7Desc = "";
        this.data8Desc = "";
        this.data9Desc = "";
        this.data10Desc = "";
        this.data11Desc = "";
        this.data12Desc = "";
        this.data13Desc = "";
        this.data14Desc = "";
        this.outcomeTypeName = "";
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
        this.meanDifference = this.MeanDiff();
        this.seMeanDifference = this.CorrectForClustering(this.GetSEforMeanDiff(
          this.data1, this.data2, this.data5, this.data6));
        this.es = this.smd;
        this.sees = this.sesmd;

        break;

      case 4: //n, mean CI

        this.smd = this.SmdFromNMeanCI();
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

      case 6: // diagnostic binary 2 x 2 table- not implemented (26/06/2023), shows Zero as ES and SE for all implemented MA types and reports.

        this.es = 0;// this.CalcOddsRatio();
        this.sees = 0;//this.CalcOddsRatioSE();
        this.oddsRatio = 0;//this.es;
        this.seOddsRatio = 0;//this.CalcOddsRatioSE();
        break;

      case 7: // correlation coeffiCIent r

        this.r = this.data2;
        this.ser = Math.sqrt(1 / (this.data1 - 3));
        this.es = this.r;
        this.sees = this.ser;

        break;

      default:

        break;
    }

    this.ciLower = this.smd - (1.96 * this.sees);
    this.ciUpper = this.smd + (1.96 * this.sees);

  }

  public SetESForThisOutcomeType(MAType: number) {
    /* Meta-analysis types:
 *     0: Continuous: d (Hedges g)
 *     1: Continuous: r
 *     2: Binary: odds ratio
 *     3: Binary: risk ratio
 *     4: Binary: risk difference
 *     5: Binary: diagnostic test OR
 *     6: Binary: Peto OR
 *     7: Continuous: mean difference
 */
    if (this.outcomeTypeId != 0) {
      this.es = 0.0;
      this.sees = 0.0;
      switch (MAType) {
        case 0: this.es = this.smd;
          this.sees = this.sesmd; break;
        case 1: this.es = this.r;
          this.sees = this.ser; break;
        case 2: this.es = this.oddsRatio;
          this.sees = this.seOddsRatio; break;
        case 3: this.es = this.riskRatio;
          this.sees = this.seRiskRatio; break;
        case 4: this.es = this.riskDifference;
          this.sees = this.seRiskDifference; break;
        case 5: this.es = this.oddsRatio;
          this.sees = this.seOddsRatio; break;
        case 6: this.es = this.petoOR;
          this.sees = this.sePetoOR; break;
        case 7: this.es = this.meanDifference;
          this.sees = this.seMeanDifference; break;
        default: break;
      }
    }
  }
  public updateCanSelect(MAType: number) {
    this.canSelect = false;
    if (this.sees == 0 || isNaN(this.sees)) {
      return;
    }
    switch (MAType) {
      // Hedges g
      case 0: if (this.unifiedOutcomeTypeId == 1 || this.unifiedOutcomeTypeId == 3 || this.unifiedOutcomeTypeId == 4 || this.unifiedOutcomeTypeId == 5) this.canSelect = true;
        break;
      // correlation coefficient
      case 1: if (this.unifiedOutcomeTypeId == 7) this.canSelect = true;
        break;
      // mean difference
      case 7: if (this.unifiedOutcomeTypeId == 1 || this.unifiedOutcomeTypeId == 3 || this.unifiedOutcomeTypeId == 4 || this.unifiedOutcomeTypeId == 5) this.canSelect = true;
        break;
      // 2, 3, 4, 5, 6 - binary outcomes
      default: if (this.unifiedOutcomeTypeId == 2) this.canSelect = true;
        break;
    }
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
    if (this.smd == 0 && this.oddsRatio == 0 && this.riskDifference == 0
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
      this.es = this.r;
      this.sees = this.ser;
      this.unifiedOutcomeTypeId = 7;
      this.outcomeTypeName = "Correlation";
    }
    if (this.smd == 0 && this.oddsRatio == 0 && this.riskRatio == 0
      && this.riskDifference == 0 && this.petoOR == 0 && this.r == 0) {
      this.es = this.meanDifference;
      this.sees = this.seMeanDifference;
      this.unifiedOutcomeTypeId = 1;
      this.outcomeTypeName = "Continuous";
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
  private SmdFromT(t: number): number {
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
  private CorrectG(n1: number, n2: number, g: number): number  // for single group studies n2=0
  {
    let gc, top, lower: number;

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
  private CalcOddsRatio(): number {
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
    //console.log("Odds ratio SE is: ", tester);
    return Math.sqrt(Number(1 / d1) + Number(1 / d2) + Number(1 / d3) + Number(1 / d4));
  }
  private CalcRiskRatio(): number {
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
  private CalcPetoORSE(): number {
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
    part2OfS = n1 + n2 - 2;
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
  private _outcomeId: number = 0;
  public get outcomeId(): number {
    return this._outcomeId;
  }
  public set outcomeId(val: number) {
    this._outcomeId = val;
    for (let Occ of this.outcomeCodes.outcomeItemAttributesList) {
      Occ.outcomeId = val;
    }
  }
  itemSetId: number = 0;
  public outcomeTypeName: string = "Continuous: Ns, means and SD";
  //public get outcomeTypeName(): string {

  //	return this.OutcomeTypeName;
  //}
  //public set outcomeTypeName(val: string) {

  //	this.OutcomeTypeName = val;
  //}
  public outcomeTypeId: number = 1;
  //public get outcomeTypeId(): number {

  //	//this.SetCalculatedValues();
  //	return this.OutcomeTypeId;
  //}
  //public set outcomeTypeId(val: number) {
  //	this.OutcomeTypeId = val;
  //	this.SetCalculatedValues();
  //}

  itemAttributeIdIntervention: number = 0;
  itemAttributeIdControl: number = 0;
  itemAttributeIdOutcome: number = 0;
  itemArmIdGrp1: number = 0;
  itemArmIdGrp2: number = 0;
  itemId: number = 0;
  itemTimepointId: number = 0;
  itemTimepointValue: string = '';
  itemTimepointMetric: string = '';
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
  private Data4Desc: string = "";
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
  private Data12Desc: string = "";
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
export interface iExtendedOutcome extends iOutcome {

  aq1: string;
  aq2: string;
  aq3: string;
  aq4: string;
  aq5: string;
  aq6: string;
  aq7: string;
  aq8: string;
  aq9: string;
  aq10: string;
  aq11: string;
  aq12: string;
  aq13: string;
  aq14: string;
  aq15: string;
  aq16: string;
  aq17: string;
  aq18: string;
  aq19: string;
  aq20: string;

  aa1: number;
  aa2: number;
  aa3: number;
  aa4: number;
  aa5: number;
  aa6: number;
  aa7: number;
  aa8: number;
  aa9: number;
  aa10: number;
  aa11: number;
  aa12: number;
  aa13: number;
  aa14: number;
  aa15: number;
  aa16: number;
  aa17: number;
  aa18: number;
  aa19: number;
  aa20: number;

  occ1: number;
  occ2: number;
  occ3: number;
  occ4: number;
  occ5: number;
  occ6: number;
  occ7: number;
  occ8: number;
  occ9: number;
  occ10: number;
  occ11: number;
  occ12: number;
  occ13: number;
  occ14: number;
  occ15: number;
  occ16: number;
  occ17: number;
  occ18: number;
  occ19: number;
  occ20: number;
  occ21: number;
  occ22: number;
  occ23: number;
  occ24: number;
  occ25: number;
  occ26: number;
  occ27: number;
  occ28: number;
  occ29: number;
  occ30: number;
}

export class ExtendedOutcome extends Outcome implements iExtendedOutcome {
  constructor(iO: iExtendedOutcome) {
    super(iO);
    this.aq1 = iO.aq1;
    this.aq2 = iO.aq2;
    this.aq3 = iO.aq3;
    this.aq4 = iO.aq4;
    this.aq5 = iO.aq5;
    this.aq6 = iO.aq6;
    this.aq7 = iO.aq7;
    this.aq8 = iO.aq8;
    this.aq9 = iO.aq9;
    this.aq10 = iO.aq10;
    this.aq11 = iO.aq11;
    this.aq12 = iO.aq12;
    this.aq13 = iO.aq13;
    this.aq14 = iO.aq14;
    this.aq15 = iO.aq15;
    this.aq16 = iO.aq16;
    this.aq17 = iO.aq17;
    this.aq18 = iO.aq18;
    this.aq19 = iO.aq19;
    this.aq20 = iO.aq20;

    this.aa1 = iO.aa1;
    this.aa2 = iO.aa2;
    this.aa3 = iO.aa3;
    this.aa4 = iO.aa4;
    this.aa5 = iO.aa5;
    this.aa6 = iO.aa6;
    this.aa7 = iO.aa7;
    this.aa8 = iO.aa8;
    this.aa9 = iO.aa9;
    this.aa10 = iO.aa10;
    this.aa11 = iO.aa11;
    this.aa12 = iO.aa12;
    this.aa13 = iO.aa13;
    this.aa14 = iO.aa14;
    this.aa15 = iO.aa15;
    this.aa16 = iO.aa16;
    this.aa17 = iO.aa17;
    this.aa18 = iO.aa18;
    this.aa19 = iO.aa19;
    this.aa20 = iO.aa20;

    this.occ1 = iO.occ1;
    this.occ2 = iO.occ2;
    this.occ3 = iO.occ3;
    this.occ4 = iO.occ4;
    this.occ5 = iO.occ5;
    this.occ6 = iO.occ6;
    this.occ7 = iO.occ7;
    this.occ8 = iO.occ8;
    this.occ9 = iO.occ9;
    this.occ10 = iO.occ10;
    this.occ11 = iO.occ11;
    this.occ12 = iO.occ12;
    this.occ13 = iO.occ13;
    this.occ14 = iO.occ14;
    this.occ15 = iO.occ15;
    this.occ16 = iO.occ16;
    this.occ17 = iO.occ17;
    this.occ18 = iO.occ18;
    this.occ19 = iO.occ19;
    this.occ20 = iO.occ20;
    this.occ21 = iO.occ21;
    this.occ22 = iO.occ22;
    this.occ23 = iO.occ23;
    this.occ24 = iO.occ24;
    this.occ25 = iO.occ25;
    this.occ26 = iO.occ26;
    this.occ27 = iO.occ27;
    this.occ28 = iO.occ28;
    this.occ29 = iO.occ29;
    this.occ30 = iO.occ30;
  }
  ShowSignificantDigits: number = 3;
  public get esRounded(): number {
    const multiplier = 10 ** this.ShowSignificantDigits;
    return Math.round((this.es + Number.EPSILON) * multiplier) / multiplier;
  }
  public get seesRounded(): number {
    const multiplier = 10 ** this.ShowSignificantDigits;
    return Math.round((this.sees + Number.EPSILON) * multiplier) / multiplier;
  }

  aq1: string;
  aq2: string;
  aq3: string;
  aq4: string;
  aq5: string;
  aq6: string;
  aq7: string;
  aq8: string;
  aq9: string;
  aq10: string;
  aq11: string;
  aq12: string;
  aq13: string;
  aq14: string;
  aq15: string;
  aq16: string;
  aq17: string;
  aq18: string;
  aq19: string;
  aq20: string;

  aa1: number;
  aa2: number;
  aa3: number;
  aa4: number;
  aa5: number;
  aa6: number;
  aa7: number;
  aa8: number;
  aa9: number;
  aa10: number;
  aa11: number;
  aa12: number;
  aa13: number;
  aa14: number;
  aa15: number;
  aa16: number;
  aa17: number;
  aa18: number;
  aa19: number;
  aa20: number;

  occ1: number;
  occ2: number;
  occ3: number;
  occ4: number;
  occ5: number;
  occ6: number;
  occ7: number;
  occ8: number;
  occ9: number;
  occ10: number;
  occ11: number;
  occ12: number;
  occ13: number;
  occ14: number;
  occ15: number;
  occ16: number;
  occ17: number;
  occ18: number;
  occ19: number;
  occ20: number;
  occ21: number;
  occ22: number;
  occ23: number;
  occ24: number;
  occ25: number;
  occ26: number;
  occ27: number;
  occ28: number;
  occ29: number;
  occ30: number;
}
