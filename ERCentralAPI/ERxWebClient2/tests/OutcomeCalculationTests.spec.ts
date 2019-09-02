import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { OutcomesService, iOutcome, Outcome } from '../ClientApp/app/components/services/outcomes.service';
import { RouterTestingModule } from '@angular/router/testing';

describe('OutcomesService', () => {

	beforeEach(() => {
		TestBed.configureTestingModule({
			imports: [HttpClientTestingModule,
				RouterTestingModule.withRoutes([])],
			providers: [OutcomesService,
				{ provide: 'BASE_URL', useFactory: getBaseUrl }]
		});
	});

	//OutcomeTypeId = 0 MANUAL ONE MOST TRICKY==============================================
	const outcomeInfo: iOutcome =
	{
		"canSelect": false,
		"isSelected": false,
		"manuallyEnteredOutcomeTypeId": 0,
		"unifiedOutcomeTypeId": 1, "grp1ArmName": "sdfgsdfg",
		"grp2ArmName": "sdfgsdfg",
		"outcomeCodes": {
			"outcomeItemAttributesList": [{ "outcomeItemAttributeId": 5957, "outcomeId": 6511, "attributeId": 1095, "additionalText": "", "attributeName": "1" },
			{ "outcomeItemAttributeId": 5958, "outcomeId": 6511, "attributeId": 1096, "additionalText": "", "attributeName": "2" }, { "outcomeItemAttributeId": 5959, "outcomeId": 6511, "attributeId": 47780, "additionalText": "", "attributeName": "ee" }, { "outcomeItemAttributeId": 5960, "outcomeId": 6511, "attributeId": 84001, "additionalText": "", "attributeName": "somecod" }, { "outcomeItemAttributeId": 5961, "outcomeId": 6511, "attributeId": 305251, "additionalText": "", "attributeName": "qqqq" }, { "outcomeItemAttributeId": 5962, "outcomeId": 6511, "attributeId": 345258, "additionalText": "", "attributeName": "test2" }, { "outcomeItemAttributeId": 5963, "outcomeId": 6511, "attributeId": 345259, "additionalText": "", "attributeName": "ttttt" }, { "outcomeItemAttributeId": 5964, "outcomeId": 6511, "attributeId": 345261, "additionalText": "", "attributeName": "sadsadas" }]
		},
		"outcomeId": 6511, "itemSetId": 3514232,
		"outcomeTypeName": "Continuous",
		"outcomeTypeId": 0, "NRows": 6, "itemAttributeIdIntervention": 87589,
		"itemAttributeIdControl": 83708, "itemAttributeIdOutcome": 335256,
		"itemArmIdGrp1": 90029, "itemArmIdGrp2": 90029, "itemId": 0,
		"itemTimepointValue": "2", "itemTimepointMetric": "months", "itemTimepointId": 20003,
		"outcomeTimePoint": {
			itemId: 0,
			timepointValue: '',
			timepointMetric: '',
			itemTimepointId: 0
		},
		"title": "TESTER 12345", "shortTitle": "Brandon (1986)",
		"timepointDisplayValue": "2 months", "outcomeDescription": "testerooo again",
		"data1": 0.9,
		"data2": 8,
		"data3": 0,
		"data4": 0,
		"data5": 0,
		"data6": 0,
		"data7": 0,
		"data8": 0,
		"data9": 0,
		"data10": 0,
		"data11": 0,
		"data12": 0,
		"data13": 0,
		"data14": 0,
		"interventionText": "AAAA Compar", "controlText": "control (comparison TP)",
		"outcomeText": "child 2 outcome", "feWeight": 0, "reWeight": 0, "smd": 0.9,
		"sesmd": 8, "r": 0, "ser": 0, "oddsRatio": 0,
		"seOddsRatio": 0,
		"riskRatio": 0,
		"seRiskRatio": 0, "CIUpperSMD": 0, "CILowerSMD": 0,
		"CIUpperR": 0, "CILowerR": 0, "CIUpperOddsRatio": 0, "CILowerOddsRatio": 0,
		"CIUpperRiskRatio": 0, "CILowerRiskRatio": 0, "CIUpperRiskDifference": 0,
		"CILowerRiskDifference": 0, "CIUpperPetoOddsRatio": 0, "CILowerPetoOddsRatio": 0,
		"CIUpperMeanDifference": 0, "CILowerMeanDifference": 0,
		"riskDifference": 0,
		"seRiskDifference": 0,
		"meanDifference": 0,
		"seMeanDifference": 0,
		"petoOR": 0, "sePetoOR": 0, "es": 0.9, "sees": 8, "nRows": 0,
		"CILower": -14.78, "CIUpper": 16.58, "esDesc": "Effect size",
		"seDesc": "SE",
		"data1Desc": "SMD",
		"data2Desc": "standard error",
		"data3Desc": "r",
		"data4Desc": "SE (Z transformed)",
		"data5Desc": "odds ratio",
		"data6Desc": "SE (log OR)",
		"data7Desc": "risk ratio",
		"data8Desc": "SE (log RR)",
		"data9Desc": "",
		"data10Desc": "",
		"data11Desc": "risk difference",
		"data12Desc": "standard error",
		"data13Desc": "mean difference",
		"data14Desc": "standard error"
	}
	let newOutcome: Outcome = new Outcome(outcomeInfo);
	newOutcome.data1 = 0.9;
	newOutcome.data2 = 8;
	newOutcome.data9 = 0;
	newOutcome.data10 = 0;

	newOutcome.SetCalculatedValues();

	it('should have a correct SMD and SESMD values after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 0', (() => {

			let SMD = newOutcome.smd.toFixed(15);
			let SESMD = newOutcome.sesmd;
			expect(SMD).toEqual('0.900000000000000');
			expect(SESMD).toEqual(8);
	}));

	let newOutcomeCUA: Outcome = new Outcome(outcomeInfo);
	newOutcomeCUA.data1 = 0.9;
	newOutcomeCUA.data2 = 8;
	newOutcomeCUA.data9 = 4;
	newOutcomeCUA.data10 = 4;

	newOutcomeCUA.SetCalculatedValues();

	it('should have a correct SMD and SESMD values with CUA after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 0', (() => {

			let SMD = newOutcomeCUA.smd.toFixed(15);
			let SESMD = newOutcomeCUA.sesmd;
			expect(SMD).toEqual('0.900000000000000');
			expect(SESMD).toEqual(28.844410203711913);
		}));
	//=============================================================================

	//OutcomeTypeId = 1====================================================
	const outcomeInfo1: iOutcome =
	{
		"canSelect": false,
		"isSelected": false,
		"manuallyEnteredOutcomeTypeId": 0,
		"unifiedOutcomeTypeId": 1,
		"grp1ArmName": "wertwert54",
		"grp2ArmName": "sdfgsdfg",
		"outcomeCodes": { "outcomeItemAttributesList": [] },
		"outcomeId": 7502, "itemSetId": 3514232,
		"outcomeTypeName": "Continuous",
		"outcomeTypeId": 1,
		"NRows": 3, "itemAttributeIdIntervention": 87589,
		"itemAttributeIdControl": 305251,
		"itemAttributeIdOutcome": 84005,
		"itemArmIdGrp1": 80029, "itemArmIdGrp2": 90030,
		"itemId": 0, "itemTimepointValue": "1",
		"itemTimepointMetric": "days", "itemTimepointId": 40012,
		"outcomeTimePoint": {
			itemId: 0,
			timepointValue: '',
			timepointMetric: '',
			itemTimepointId: 0
		},
		"title": "test 33", "shortTitle": "Brandon (1986)",
		"timepointDisplayValue": "1 days",
		"outcomeDescription": "sdfgAAAAAAA",
		"data1": 10,
		"data2": 9,
		"data3": 8,
		"data4": 7,
		"data5": 6,
		"data6": 5,
		"data7": 14,
		"data8": 13,
		"data9": 0,
		"data10": 0,
		"data11": 12,
		"data12": 11,
		"data13": 10,
		"data14": 9,
		"interventionText": "AAAA Compar",
		"controlText": "qqqq", "outcomeText": "Code Of outcome Type1",
		"feWeight": 0, "reWeight": 0, "smd": 0.17205368889046432,
		"sesmd": 0.4605365663028017, "r": 0, "ser": 0,
		"oddsRatio": 0, "seOddsRatio": 0, "riskRatio": 0,
		"seRiskRatio": 0, "CIUpperSMD": 0, "CILowerSMD": 0,
		"CIUpperR": 0, "CILowerR": 0, "CIUpperOddsRatio": 0,
		"CILowerOddsRatio": 0, "CIUpperRiskRatio": 0, "CILowerRiskRatio": 0,
		"CIUpperRiskDifference": 0, "CILowerRiskDifference": 0,
		"CIUpperPetoOddsRatio": 0, "CILowerPetoOddsRatio": 0,
		"CIUpperMeanDifference": 0, "CILowerMeanDifference": 0,
		"riskDifference": 0, "seRiskDifference": 0, "meanDifference": 1,
		"seMeanDifference": 2.5254262566501082, "petoOR": 0,
		"sePetoOR": 0, "es": 0.17205368889046432, "sees": 0.4605365663028017,
		"nRows": 0, "CILower": -0.730597981063027, "CIUpper": 1.0747053588439557,
		"esDesc": "SMD", "seDesc": "SE",
		"data1Desc": "Group 1 N",
		"data2Desc": "Group 2 N",
		"data3Desc": "Group 1 mean",
		"data4Desc": "Group 2 mean",
		"data5Desc": "Group 1 SD",
		"data6Desc": "Group 2 SD",
		"data7Desc": "",
		"data8Desc": "",
		"data9Desc": "",
		"data10Desc": "",
		"data11Desc": "",
		"data12Desc": "",
		"data13Desc": "",
		"data14Desc": "" 
	}
	let newOutcome1: Outcome = new Outcome(outcomeInfo1);
	newOutcome1.data1 = 10;
	newOutcome1.data2 = 9;
	newOutcome1.data3 = 8;
	newOutcome1.data4 = 7;
	newOutcome1.data5 = 6;
	newOutcome1.data6 = 5;

	newOutcome1.SetCalculatedValues();

	it('should have a correct SMD and SEES values after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 1', (() => {
			let SMD = newOutcome1.es.toFixed(15);
			let SEES = newOutcome1.sees.toFixed(15);
			expect(SMD).toEqual('0.172053688890464');
			expect(SEES).toEqual('0.460536566302802');
			//console.log('The result is: ' + newOutcome1.smd)
		}));

	let newOutcome1CUA: Outcome = new Outcome(outcomeInfo1);
	newOutcome1CUA.data1 = 10;
	newOutcome1CUA.data2 = 9;
	newOutcome1CUA.data3 = 8;
	newOutcome1CUA.data4 = 7;
	newOutcome1CUA.data5 = 6;
	newOutcome1CUA.data6 = 5;
	newOutcome1CUA.data9 = 4;
	newOutcome1CUA.data10 = 4;

	newOutcome1CUA.SetCalculatedValues();

	it('should have a correct SMD and SEES values with CUA after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 0', (() => {

			let SMD = newOutcome1CUA.es.toFixed(15);
			let SEES = newOutcome1CUA.sees.toFixed(15);
			expect(SMD).toEqual('0.172053688890464');
			expect(SEES).toEqual('1.660488204030873');
		}));
	//=============================================================================

	//OutcomeTypeId = 2 ====================================================
	const outcomeInfo2: iOutcome =
	{
		"canSelect": false,
		"isSelected": false,
		"manuallyEnteredOutcomeTypeId": 0,
		"unifiedOutcomeTypeId": 2,
		"grp1ArmName": "wertwert54",
		"grp2ArmName": "sdfgsdfg",
		"outcomeCodes": { "outcomeItemAttributesList": [] },
		"outcomeId": 7502, "itemSetId": 3514232,
		"outcomeTypeName": "Binary",
		"outcomeTypeId": 2,
		"NRows": 2,
		"itemAttributeIdIntervention": 87589,
		"itemAttributeIdControl": 305251,
		"itemAttributeIdOutcome": 84005,
		"itemArmIdGrp1": 80029,
		"itemArmIdGrp2": 90030,
		"itemId": 0,
		"itemTimepointValue": "1",
		"itemTimepointMetric": "days",
		"itemTimepointId": 40012,
		"outcomeTimePoint": {
			itemId: 0,
			timepointValue: '',
			timepointMetric: '',
			itemTimepointId: 0
		},
		"title": "test 33",
		"shortTitle": "Brandon (1986)",
		"timepointDisplayValue": "1 days",
		"outcomeDescription": "sdfgAAAAAAA",
		"data1": 10,
		"data2": 9,
		"data3": 8,
		"data4": 7,
		"data5": 6,
		"data6": 5,
		"data7": 14,
		"data8": 13,
		"data9": 0,
		"data10": 0,
		"data11": 0,
		"data12": 0,
		"data13": 0,
		"data14": 0,
		"interventionText": "AAAA Compar",
		"controlText": "qqqq",
		"outcomeText": "Code Of outcome Type1",
		"feWeight": 0, "reWeight": 0,
		"smd": 0.17205368889046432,
		"sesmd": 0.4605365663028017,
		"r": 0, "ser": 0,
		"oddsRatio": 0.9722222222222222,
		"seOddsRatio": 0.6920753239122559,
		"riskRatio": 0.9876543209876544,
		"seRiskRatio": 0.3050500869620521,
		"CIUpperSMD": 0, "CILowerSMD": 0,
		"CIUpperR": 0, "CILowerR": 0,
		"CIUpperOddsRatio": 0,
		"CILowerOddsRatio": 0, "CIUpperRiskRatio": 0,
		"CILowerRiskRatio": 0, "CIUpperRiskDifference": 0, "CILowerRiskDifference": 0,
		"CIUpperPetoOddsRatio": 0, "CILowerPetoOddsRatio": 0, "CIUpperMeanDifference": 0,
		"CILowerMeanDifference": 0, "riskDifference": -0.00694444444444442,
		"seRiskDifference": 0.17058218107360607, "meanDifference": 1,
		"seMeanDifference": 2.5254262566501082, "petoOR": 0.973031151589488,
		"sePetoOR": 0.681737546179866, "es": 0.9722222222222222,
		"sees": 0.6920753239122559, "nRows": 0, "CILower": -1.1844139459775573,
		"CIUpper": 1.5285213237584858, "esDesc": "OR", "seDesc": "SE (log OR)",
		"data1Desc": "Group 1 events",
		"data2Desc": "Group 2 events",
		"data3Desc": "Group 1 no events",
		"data4Desc": "Group 2 no events",
		"data5Desc": "", "data6Desc": "",
		"data7Desc": "", "data8Desc": "",
		"data9Desc": "", "data10Desc": "",
		"data11Desc": "", "data12Desc": "",
		"data13Desc": "", "data14Desc": ""
	}
	let newOutcome2: Outcome = new Outcome(outcomeInfo2);
	newOutcome2.data1 = 10;
	newOutcome2.data2 = 9;
	newOutcome2.data3 = 8;
	newOutcome2.data4 = 7;
	newOutcome2.data5 = 6;
	newOutcome2.data6 = 5;

	newOutcome2.SetCalculatedValues();

	it('should have a correct OR and SEOR values after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 2', (() => {

			let OR = newOutcome2.oddsRatio.toFixed(15);
			let SEOR = newOutcome2.seOddsRatio.toFixed(15);
			
			expect(OR).toEqual('0.972222222222222');
			expect(SEOR).toEqual('0.692075323912256');
		}));


	let newOutcome2CUA: Outcome = new Outcome(outcomeInfo2);
	newOutcome2CUA.data1 = 10;
	newOutcome2CUA.data2 = 9;
	newOutcome2CUA.data3 = 8;
	newOutcome2CUA.data4 = 7;
	newOutcome2CUA.data5 = 6;
	newOutcome2CUA.data6 = 5;
	newOutcome2CUA.data9 = 3;
	newOutcome2CUA.data10 = 3;
	newOutcome2CUA.SetCalculatedValues();

	it('should have a correct OR and SEOR values with CUA after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 0', (() => {

			let OR = newOutcome2CUA.oddsRatio.toFixed(15);
			let SEOR = newOutcome2CUA.seOddsRatio.toFixed(15);

			expect(OR).toEqual('0.972222222222222');
			expect(SEOR).toEqual('1.831059195596302');
		}));
	//=============================================================================


	//OutcomeTypeId = 3====================================================
	const outcomeInfo3: iOutcome =
	{
		"canSelect": false,
		"isSelected": false,
		"manuallyEnteredOutcomeTypeId": 0,
		"unifiedOutcomeTypeId": 3,
		"grp1ArmName": "sdfgsdfg",
		"grp2ArmName": "sdfgsdfg",
		"outcomeCodes": {
			"outcomeItemAttributesList":
				[
					{ "outcomeItemAttributeId": 5933, "outcomeId": 6511, "attributeId": 1095, "additionalText": "", "attributeName": "1" },
					{ "outcomeItemAttributeId": 5934, "outcomeId": 6511, "attributeId": 1096, "additionalText": "", "attributeName": "2" },
					{ "outcomeItemAttributeId": 5935, "outcomeId": 6511, "attributeId": 47780, "additionalText": "", "attributeName": "ee" },
					{ "outcomeItemAttributeId": 5936, "outcomeId": 6511, "attributeId": 84001, "additionalText": "", "attributeName": "somecod" },
					{ "outcomeItemAttributeId": 5937, "outcomeId": 6511, "attributeId": 305251, "additionalText": "", "attributeName": "qqqq" },
					{ "outcomeItemAttributeId": 5938, "outcomeId": 6511, "attributeId": 345258, "additionalText": "", "attributeName": "test2" },
					{ "outcomeItemAttributeId": 5939, "outcomeId": 6511, "attributeId": 345259, "additionalText": "", "attributeName": "ttttt" },
					{ "outcomeItemAttributeId": 5940, "outcomeId": 6511, "attributeId": 345261, "additionalText": "", "attributeName": "sadsadas" }
				]
		},
		"outcomeId": 6511,
		"itemSetId": 3514232,
		"outcomeTypeName": "Continuous",
		"outcomeTypeId": 3, "NRows": 3,
		"itemAttributeIdIntervention": 87589,
		"itemAttributeIdControl": 83708,
		"itemAttributeIdOutcome": 335256,
		"itemArmIdGrp1": 90029,
		"itemArmIdGrp2": 90029,
		"itemId": 0,
		"itemTimepointValue": "2",
		"itemTimepointMetric": "months",
		"itemTimepointId": 20003,
		"outcomeTimePoint": {
			itemId: 0,
			timepointValue: '',
			timepointMetric: '',
			itemTimepointId: 0
		}
		,
		"title": "TESTER 12345",
		"shortTitle": "Brandon (1986)",
		"timepointDisplayValue": "2 months",
		"outcomeDescription": "testerooo again",
		"data1": 0.6,
		"data2": 0.5,
		"data3": 0.5,
		"data4": 0,
		"data5": 0.9,
		"data6": 0.3,
		"data7": 0.2,
		"data8": 0.1, "data9": 0, "data10": 0, "data11": 0.3,
		"data12": 0.1, "data13": 12, "data14": 0.5,
		"interventionText": "AAAA Compar",
		"controlText": "control (comparison TP)",
		"outcomeText": "child 2 outcome",
		"feWeight": 0, "reWeight": 0, "smd": 0,
		"sesmd": 1.9148542155126762, "r": 0, "ser": 0,
		"oddsRatio": 0, "seOddsRatio": 0,
		"riskRatio": 0, "seRiskRatio": 0, "CIUpperSMD": 0,
		"CILowerSMD": 0, "CIUpperR": 0, "CILowerR": 0,
		"CIUpperOddsRatio": 0, "CILowerOddsRatio": 0,
		"CIUpperRiskRatio": 0, "CILowerRiskRatio": 0,
		"CIUpperRiskDifference": 0, "CILowerRiskDifference": 0,
		"CIUpperPetoOddsRatio": 0, "CILowerPetoOddsRatio": 0,
		"CIUpperMeanDifference": 0, "CILowerMeanDifference": 0,
		"riskDifference": 0, "seRiskDifference": 0,
		"meanDifference": 0.5, "seMeanDifference": 1.2369316876852983,
		"petoOR": 0, "sePetoOR": 0, "es": 0, "sees": 1.9148542155126762,
		"nRows": 0, "CILower": -3.7531142624048455, "CIUpper": 3.7531142624048455,
		"esDesc": "SMD", "seDesc": "SE", "data1Desc": "Group 1 N",
		"data2Desc": "Group 2 N", "data3Desc": "Group 1 mean",
		"data4Desc": "Group 2 mean", "data5Desc": "Group 1 SE",
		"data6Desc": "Group 2 SE", "data7Desc": "", "data8Desc": "",
		"data9Desc": "", "data10Desc": "", "data11Desc": "",
		"data12Desc": "", "data13Desc": "", "data14Desc": ""
	}
	let newOutcome3: Outcome = new Outcome(outcomeInfo3);
	newOutcome3.data1 = 10;
	newOutcome3.data2 = 9;
	newOutcome3.data3 = 8;
	newOutcome3.data4 = 7;
	newOutcome3.data5 = 6;
	newOutcome3.data6 = 5;

	newOutcome3.SetCalculatedValues();

	it('should have a correct SMD and SEES values after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 3', ( ()=> {

			let SMD = newOutcome3.smd.toFixed(15);
			let SEES = newOutcome3.sees.toFixed(15);
			expect(SMD).toEqual('0.054408153672788');
			expect(SEES).toEqual('0.459575230936348');
			
		}));


	let newOutcome3CUA: Outcome = new Outcome(outcomeInfo3);
	newOutcome3CUA.data1 = 10;
	newOutcome3CUA.data2 = 9;
	newOutcome3CUA.data3 = 8;
	newOutcome3CUA.data4 = 7;
	newOutcome3CUA.data5 = 6;
	newOutcome3CUA.data6 = 5;
	newOutcome3CUA.data9 = 2;
	newOutcome3CUA.data10 = 2;

	newOutcome3CUA.SetCalculatedValues();

	it('should have a correct SMD and SEES values with CUA after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 3', (() => {

			let SMD = newOutcome3CUA.smd.toFixed(15);
			let SEES = newOutcome3CUA.sees.toFixed(15);
			expect(SMD).toEqual('0.054408153672788');
			expect(SEES).toEqual('0.796007649881955');
		}));
	//=============================================================================

	//OutcomeTypeId = 4====================================================
	const outcomeInfo4: iOutcome =
	{
		"canSelect": false,
		"isSelected": false,
		"manuallyEnteredOutcomeTypeId": 0,
		"unifiedOutcomeTypeId": 4,
		"grp1ArmName": "wertwert54",
		"grp2ArmName": "sdfgsdfg",
		"outcomeCodes": { "outcomeItemAttributesList": [] },
		"outcomeId": 7502, "itemSetId": 3514232,
		"outcomeTypeName": "Continuous",
		"outcomeTypeId": 4, "NRows": 4,
		"itemAttributeIdIntervention": 87589,
		"itemAttributeIdControl": 305251,
		"itemAttributeIdOutcome": 84005,
		"itemArmIdGrp1": 80029, "itemArmIdGrp2": 90030, "itemId": 0,
		"itemTimepointValue": "1", "itemTimepointMetric": "days", "itemTimepointId": 40012,
		"outcomeTimePoint": {
			itemId: 0,
			timepointValue: '',
			timepointMetric: '',
			itemTimepointId: 0
		},
		"title": "test 33", "shortTitle": "Brandon (1986)",
		"timepointDisplayValue": "1 days", "outcomeDescription": "sdfgAAAAAAA",
		"data1": 10,
		"data2": 9,
		"data3": 8,
		"data4": 7,
		"data5": 6,
		"data6": 5,
		"data7": 14,
		"data8": 13,
		"data9": 0,
		"data10": 0,
		"data11": 12,
		"data12": 11,
		"data13": 10,
		"data14": 9,
		"interventionText": "AAAA Compar",
		"controlText": "qqqq", "outcomeText": "Code Of outcome Type1", "feWeight": 0, "reWeight": 0,
		"smd": 0.1480134737654931, "sesmd": 0.4602591313208639, "r": 0, "ser": 0,
		"oddsRatio": 0.9722222222222222, "seOddsRatio": 0.6920753239122559, "riskRatio": 0.9876543209876544,
		"seRiskRatio": 0.3050500869620521, "CIUpperSMD": 0, "CILowerSMD": 0, "CIUpperR": 0, "CILowerR": 0,
		"CIUpperOddsRatio": 0, "CILowerOddsRatio": 0, "CIUpperRiskRatio": 0, "CILowerRiskRatio": 0,
		"CIUpperRiskDifference": 0, "CILowerRiskDifference": 0, "CIUpperPetoOddsRatio": 0,
		"CILowerPetoOddsRatio": 0, "CIUpperMeanDifference": 0, "CILowerMeanDifference": 0,
		"riskDifference": -0.00694444444444442, "seRiskDifference": 0.17058218107360607,
		"meanDifference": 1, "seMeanDifference": 2.5254262566501082, "petoOR": 0.973031151589488,
		"sePetoOR": 0.681737546179866, "es": 0.1480134737654931, "sees": 0.4602591313208639,
		"nRows": 0, "CILower": -0.7540944236234001, "CIUpper": 1.0501213711543862,
		"esDesc": "SMD", "seDesc": "SE",
		"data1Desc": "Group 1 N",
		"data2Desc": "Group 2 N",
		"data3Desc": "Group 1 mean",
		"data4Desc": "Group 2 mean",
		"data5Desc": "Group 1 CI lower",
		"data6Desc": "Group 2 CI lower",
		"data7Desc": "Group 1 CI upper",
		"data8Desc": "Group 2 CI upper",
		"data9Desc": "", "data10Desc": "",
		"data11Desc": "", "data12Desc": "",
		"data13Desc": "", "data14Desc": ""
	}
	let newOutcome4: Outcome = new Outcome(outcomeInfo4);
	newOutcome4.data1 = 10;
	newOutcome4.data2 = 9;
	newOutcome4.data3 = 8;
	newOutcome4.data4 = 7;
	newOutcome4.data5 = 6;
	newOutcome4.data6 = 5;
	newOutcome4.data7 = 4;
	newOutcome4.data8 = 3;

	newOutcome4.SetCalculatedValues();

	it('should have a correct SMD and SEES values after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 4', (() => {

			let SMD = newOutcome4.smd.toFixed(15);
			let SEES = newOutcome4.sees.toFixed(15);
			expect(SMD).toEqual('0.592053895061972');
			expect(SEES).toEqual('0.471962732385845');
		}));

	let newOutcome4CUA: Outcome = new Outcome(outcomeInfo4);
	newOutcome4CUA.data1 = 10;
	newOutcome4CUA.data2 = 9;
	newOutcome4CUA.data3 = 8;
	newOutcome4CUA.data4 = 7;
	newOutcome4CUA.data5 = 6;
	newOutcome4CUA.data6 = 5;
	newOutcome4CUA.data7 = 4;
	newOutcome4CUA.data8 = 3;
	newOutcome4CUA.data9 = 4;
	newOutcome4CUA.data10 = 4;

	newOutcome4CUA.SetCalculatedValues();

	it('should have a correct SMD and SEES values with CUA after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 4', (() => {

			let SMD = newOutcome4CUA.smd.toFixed(15);
			let SEES = newOutcome4CUA.sees.toFixed(15);
			expect(SMD).toEqual('0.592053895061972');
			expect(SEES).toEqual('1.701685831725253');
	}));
	//=============================================================================

	//OutcomeTypeId = 5====================================================
	const outcomeInfo5: iOutcome =
	{
		"canSelect": false,
		"isSelected": false,
		"manuallyEnteredOutcomeTypeId": 0,
		"unifiedOutcomeTypeId": 5,
		"grp1ArmName": "wertwert54", "grp2ArmName": "sdfgsdfg",
		"outcomeCodes": { "outcomeItemAttributesList": [] },
		"outcomeId": 7502, "itemSetId": 3514232,
		"outcomeTypeName": "Continuous",
		"outcomeTypeId": 5,
		"NRows": 2, "itemAttributeIdIntervention": 87589,
		"itemAttributeIdControl": 305251, "itemAttributeIdOutcome": 84005,
		"itemArmIdGrp1": 80029, "itemArmIdGrp2": 90030, "itemId": 0,
		"itemTimepointValue": "1", "itemTimepointMetric": "days", "itemTimepointId": 40012,
		"outcomeTimePoint": {
			itemId: 0,
			timepointValue: '',
			timepointMetric: '',
			itemTimepointId: 0
		},
		"title": "test 33", "shortTitle": "Brandon (1986)",
		"timepointDisplayValue": "1 days", "outcomeDescription": "sdfgAAAAAAA",
		"data1": 0.1,
		"data2": 0.2,
		"data3": 0.3,
		"data4": 0.4,
		"data5": 6,
		"data6": 5,
		"data7": 14,
		"data8": 13,
		"data9": 0,
		"data10": 0,
		"data11": 12,
		"data12": 11,
		"data13": 10,
		"data14": 9,
		"interventionText": "AAAA Compar", "controlText": "qqqq", "outcomeText": "Code Of outcome Type1",
		"feWeight": 0, "reWeight": 0, "smd": 0, "sesmd": 0, "r": 0,
		"ser": 0, "oddsRatio": 0, "seOddsRatio": 0, "riskRatio": 0, "seRiskRatio": 0, "CIUpperSMD": 0,
		"CILowerSMD": 0, "CIUpperR": 0, "CILowerR": 0, "CIUpperOddsRatio": 0, "CILowerOddsRatio": 0,
		"CIUpperRiskRatio": 0, "CILowerRiskRatio": 0, "CIUpperRiskDifference": 0, "CILowerRiskDifference": 0,
		"CIUpperPetoOddsRatio": 0, "CILowerPetoOddsRatio": 0, "CIUpperMeanDifference": 0,
		"CILowerMeanDifference": 0, "riskDifference": 0, "seRiskDifference": 0,
		"meanDifference": -0.10000000000000003, "seMeanDifference": 22.02271554554524, "petoOR": 0,
		"sePetoOR": 0, "es": 7.380975123700388, "sees": 2.74165081775551, "nRows": 0,
		"CILower": 2.0073395208995883, "CIUpper": 12.754610726501188, "esDesc": "SMD",
		"seDesc": "SE",
		"data1Desc": "Group 1 N",
		"data2Desc": "Group 2 N",
		"data3Desc": "t-value",
		"data4Desc": "p-value",
		"data5Desc": "",
		"data6Desc": "",
		"data7Desc": "",
		"data8Desc": "",
		"data9Desc": "",
		"data10Desc": "",
		"data11Desc": "",
		"data12Desc": "",
		"data13Desc": "",
		"data14Desc": ""
	}
	let newOutcome5: Outcome = new Outcome(outcomeInfo5);
	newOutcome5.data1 = 0.1;
	newOutcome5.data2 = 0.2;
	newOutcome5.data3 = 0.3;
	newOutcome5.data4 = 0.4;

	newOutcome5.SetCalculatedValues();

	it('should have a correct SMD and SEES values after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 5', (() => {

			let SMD = newOutcome5.smd.toFixed(15);
			let SEES = newOutcome5.sesmd.toFixed(15);
			expect(SMD).toEqual('7.380975123700388');
			expect(SEES).toEqual('2.741650817755510');

		}));

	let newOutcome5CUA: Outcome = new Outcome(outcomeInfo5);
	newOutcome5CUA.data1 = 0.1;
	newOutcome5CUA.data2 = 0.2;
	newOutcome5CUA.data3 = 0.3;
	newOutcome5CUA.data4 = 0.4;
	newOutcome5CUA.data9 = 4;
	newOutcome5CUA.data10 = 4;

	newOutcome5CUA.SetCalculatedValues();

	it('should have a correct SMD and SEES values with CUA after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 5', (() => {

			let SMD = newOutcome5CUA.smd.toFixed(15);
			let SEES = newOutcome5CUA.sesmd.toFixed(15);
			expect(SMD).toEqual('7.380975123700388');
			expect(SEES).toEqual('9.885162602835267');
		}));
	//=============================================================================

	//OutcomeTypeId = 6====================================================
	const outcomeInfo6: iOutcome =
	{
		"canSelect": false,
		"isSelected": false,
		"manuallyEnteredOutcomeTypeId": 0,
		"unifiedOutcomeTypeId": 6, "grp1ArmName": "wertwert54",
		"grp2ArmName": "wertwert54",
		"outcomeCodes": { "outcomeItemAttributesList": [] }, "outcomeId": 7497,
		"itemSetId": 3514232,
		"outcomeTypeName": "Binary",
		"outcomeTypeId": 6,
		"NRows": 2, "itemAttributeIdIntervention": 87589, "itemAttributeIdControl": 83708,
		"itemAttributeIdOutcome": 87596, "itemArmIdGrp1": 80029, "itemArmIdGrp2": 80029,
		"itemId": 0, "itemTimepointValue": "1", "itemTimepointMetric": "days",
		"itemTimepointId": 40012,
		"outcomeTimePoint": {
			itemId: 0,
			timepointValue: '',
			timepointMetric: '',
			itemTimepointId: 0
		},
		"title": "test1984",
		"shortTitle": "Brandon (1986)", "timepointDisplayValue": "1 days",
		"outcomeDescription": "1984",
		"data1": 9,
		"data2": 9,
		"data3": 9,
		"data4": 7,
		"data5": 6,
		"data6": 5,
		"data7": 4,
		"data8": 3,
		"data9": 0,
		"data10": 0,
		"data11": 2,
		"data12": 1,
		"data13": 1,
		"data14": 1,
		"interventionText": "AAAA Compar",
		"controlText": "control (comparison TP)",
		"outcomeText": "2by2",
		"feWeight": 0,
		"reWeight": 0,
		"smd": 0, "sesmd": 0,
		"r": 0, "ser": 0,
		"oddsRatio": 0.7777777777777778,
		"seOddsRatio": 0.6900655593423543,
		"riskRatio": 0, "seRiskRatio": 0,
		"CIUpperSMD": 0, "CILowerSMD": 0,
		"CIUpperR": 0, "CILowerR": 0,
		"CIUpperOddsRatio": 0,
		"CILowerOddsRatio": 0,
		"CIUpperRiskRatio": 0,
		"CILowerRiskRatio": 0,
		"CIUpperRiskDifference": 0,
		"CILowerRiskDifference": 0,
		"CIUpperPetoOddsRatio": 0,
		"CILowerPetoOddsRatio": 0,
		"CIUpperMeanDifference": 0,
		"CILowerMeanDifference": 0,
		"riskDifference": 0,
		"seRiskDifference": 0,
		"meanDifference": 0,
		"seMeanDifference": 0,
		"petoOR": 0, "sePetoOR": 0,
		"es": 0.7777777777777778,
		"sees": 0.6900655593423543,
		"nRows": 0, "CILower": -1.3525284963110142,
		"CIUpper": 1.3525284963110142,
		"esDesc": "Diagnostic OR", "seDesc": "SE",
		"data1Desc": "True positive",
		"data2Desc": "False positive",
		"data3Desc": "False negative",
		"data4Desc": "True negative",
		"data5Desc": "",
		"data6Desc": "",
		"data7Desc": "",
		"data8Desc": "",
		"data9Desc": "",
		"data10Desc": "",
		"data11Desc": "",
		"data12Desc": "",
		"data13Desc": "",
		"data14Desc": ""
	}
	let newOutcome6: Outcome = new Outcome(outcomeInfo6);
	newOutcome6.data1 = 9;
	newOutcome6.data2 = 9;
	newOutcome6.data3 = 9;
	newOutcome6.data4 = 7;

	newOutcome6.SetCalculatedValues();

	it('should have a correct DOR and SEDOR value after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 6', (() => {

			let DOR = newOutcome6.oddsRatio.toFixed(15);
			let SEDOR = newOutcome6.sees.toFixed(15);
			expect(DOR).toEqual('0.777777777777778');
			expect(SEDOR).toEqual('0.690065559342354');
		}));

	let newOutcome6CUA: Outcome = new Outcome(outcomeInfo6);
	newOutcome6CUA.data1 = 9;
	newOutcome6CUA.data2 = 9;
	newOutcome6CUA.data3 = 9;
	newOutcome6CUA.data9 = 4;
	newOutcome6CUA.data10 = 4;

	newOutcome6CUA.SetCalculatedValues();

	it('should have a correct DOR and SEDOR values with CUA after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 6', (() => {

			let DOR = newOutcome6CUA.oddsRatio.toFixed(15);
			let SEDOR = newOutcome6CUA.sees.toFixed(15);
			expect(DOR).toEqual('0.777777777777778');
			expect(SEDOR).toEqual('0.690065559342354');
		}));
	//=============================================================================

	//OutcomeTypeId = 7 ====================================================
	const outcomeInfo7: iOutcome =
	{
		"canSelect": false,
		"isSelected": false,
		"manuallyEnteredOutcomeTypeId": 0,
		"unifiedOutcomeTypeId": 7,
		"grp1ArmName": "wertwert54",
		"grp2ArmName": "sdfgsdfg",
		"outcomeCodes": { "outcomeItemAttributesList": [] },
		"outcomeId": 7502, "itemSetId": 3514232,
		"outcomeTypeName": "Correlation",
		"outcomeTypeId": 7, "NRows": 1,
		"itemAttributeIdIntervention": 87589,
		"itemAttributeIdControl": 305251,
		"itemAttributeIdOutcome": 84005, "itemArmIdGrp1": 80029,
		"itemArmIdGrp2": 90030, "itemId": 0, "itemTimepointValue": "1",
		"itemTimepointMetric": "days", "itemTimepointId": 40012,
		"outcomeTimePoint": {
			itemId: 0,
			timepointValue: '',
			timepointMetric: '',
			itemTimepointId: 0
		},
		"title": "test 33",
		"shortTitle": "Brandon (1986)", "timepointDisplayValue": "1 days",
		"outcomeDescription": "sdfgAAAAAAA",
		"data1": 5,
		"data2": 0.8,
		"data3": 0.3,
		"data4": 0.4,
		"data5": 6,
		"data6": 5,
		"data7": 14,
		"data8": 13,
		"data9": 0,
		"data10": 0,
		"data11": 12,
		"data12": 11,
		"data13": 10,
		"data14": 9,
		"interventionText": "AAAA Compar",
		"controlText": "qqqq", "outcomeText": "Code Of outcome Type1",
		"feWeight": 0, "reWeight": 0, "smd": 0, "sesmd": 0, "r": 0.8,
		"ser": 0.7071067811865476, "oddsRatio": 0, "seOddsRatio": 0,
		"riskRatio": 0, "seRiskRatio": 0, "CIUpperSMD": 0, "CILowerSMD": 0,
		"CIUpperR": 0, "CILowerR": 0, "CIUpperOddsRatio": 0, "CILowerOddsRatio": 0,
		"CIUpperRiskRatio": 0, "CILowerRiskRatio": 0, "CIUpperRiskDifference": 0,
		"CILowerRiskDifference": 0, "CIUpperPetoOddsRatio": 0,
		"CILowerPetoOddsRatio": 0, "CIUpperMeanDifference": 0,
		"CILowerMeanDifference": 0, "riskDifference": 0, "seRiskDifference": 0,
		"meanDifference": 0, "seMeanDifference": 0, "petoOR": 0, "sePetoOR": 0,
		"es": 0.8, "sees": 0.7071067811865476, "nRows": 0, "CILower": -1.3859292911256331,
		"CIUpper": 1.3859292911256331, "esDesc": "r", "seDesc": "SE (Z transformed)",
		"data1Desc": "group(s) size",
		"data2Desc": "correlation",
		"data3Desc": "",
		"data4Desc": "",
		"data5Desc": "",
		"data6Desc": "",
		"data7Desc": "",
		"data8Desc": "",
		"data9Desc": "",
		"data10Desc": "",
		"data11Desc": "",
		"data12Desc": "",
		"data13Desc": "",
		"data14Desc": ""
	}
	let newOutcome7: Outcome = new Outcome(outcomeInfo7);
	newOutcome7.data1 = 5;
	newOutcome7.data2 = 0.8;


	newOutcome7.SetCalculatedValues();

	it('should have a correct r And SER values after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 7', (() => {

			let r = newOutcome7.r.toFixed(1);
			let SE = newOutcome7.sees.toFixed(15);
			expect(r).toEqual('0.8');
			expect(SE).toEqual('0.707106781186548');
		}));

	let newOutcome7CUA: Outcome = new Outcome(outcomeInfo7);
	newOutcome7CUA.data1 = 5;
	newOutcome7CUA.data2 = 0.8;
	newOutcome7CUA.data9 = 4;
	newOutcome7CUA.data10 = 4;

	newOutcome7CUA.SetCalculatedValues();

	it('should have a correct r and SER values with CUA after calling SetCalculatedValues'
		+ 'for OutcomeTypeId 7', (() => {

			let r = newOutcome7CUA.r.toFixed(1);
			let SE = newOutcome7CUA.sees.toFixed(15);
			expect(r).toEqual('0.8');
			expect(SE).toEqual('0.707106781186548');
		}));
	//=============================================================================
	


});


export function getBaseUrl() {
	return "http://localhost:52335/";
}


