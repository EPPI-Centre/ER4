import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import { ClassifierService, PriorityScreeningSimulation } from '../services/classifier.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { SetAttribute, singleNode } from '../services/ReviewSets.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ChartComponent } from '@progress/kendo-angular-charts';
import { saveAs } from '@progress/kendo-file-saver';

function nextMultipleOfTen(num: number): number {
  return Math.ceil(num / 10) * 10;
}
function mean(arr: number[]): number {
  const sum = arr.reduce((a, b) => a + b, 0);
  return sum / arr.length;
}
function standardDeviation(arr: number[]): number {
  const avg = mean(arr);
  const squareDiffs = arr.map(value => {
    const diff = value - avg;
    return diff * diff;
  });
  const avgSquareDiff = mean(squareDiffs);
  return Math.sqrt(avgSquareDiff);
}
function calculateConfidence(stdev: number, n: number): number {
  // Z-score for 95% confidence level
  const z = 1.96; 

  // Standard error
  const standardError = stdev / Math.sqrt(n);

  // Confidence interval
  const confidenceInterval = z * standardError;

  return confidenceInterval;
}
interface simulationStats {
  simulation: number;
  nIncludes: number;
  nScreened: number;
  nScreenedAt100: number | null;
  workloadReduction: number;
  workloadReductionPercent: number;
}

@Component({
  selector: 'PriorityScreeningSim',
  templateUrl: './PriorityScreeningSim.component.html',
  providers: []
})

export class PriorityScreeningSim implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private classifierService: ClassifierService,
    private _reviewerIdentityServ: ReviewerIdentityService,
    private confirmationDialogService: ConfirmationDialogService,
    private _eventEmitterService: EventEmitterService,
    private notificationService: NotificationService,
    private modalService: ModalService,
  ) { }

  @Output() PleaseCloseMe = new EventEmitter();
  ngOnInit() {
    this.refreshPriorityScreeningSimulationList();
  }
  @ViewChild('VisualiseChart')
  private VisualiseChart!: ChartComponent;
  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;

  CanWrite(): boolean {
    return this._reviewerIdentityServ.HasWriteRights;
  }
  public get ClassifierServiceIsBusy(): boolean {
    return this.classifierService.IsBusy;
  }

  public get PriorityScreeningSimulationList(): PriorityScreeningSimulation[] {
    return this.classifierService.PriorityScreeningSimulationList;
  }

  public get PriorityScreeningSimulationText(): string | null
  {
    return this.classifierService.PriorityScreeningSimulationResults;
  }

  refreshPriorityScreeningSimulationList() {
    this.classifierService.FetchPriorityScreeningSimulationList();
  }
  
  simulationDataContainer: [number, number][][] = [];
  simulationDataItemCount: number = 0;
  simulationDataIncludedItemsCountRoundedUp: number = 0;
  simulationDataItemCountRoundedUp: number = 0;
  xAxisText: string = "";
  yAxisText: string = "";
  statistics: simulationStats[] = [];
  recallLevel: number = 100;
  simulationDataIncludedItemsCount: number = 0;

  summaryStatisticsAgg: string = "";
  workloadReductionStats: string = "";
  workloadReductionPercentStats: string = "";

  async showSimulation(simulation: PriorityScreeningSimulation) {
    const res = await this.classifierService.FetchPriorityScreeningSimulation(simulation.simulationName);
    if (res == true)
    {
      this.processSimulation(true);
    }
  }

  processSimulation(redrawGraph: boolean)
  {
    const dataframe = this.PriorityScreeningSimulationText;

    if (this.PriorityScreeningSimulationText != null) {

      // clear out previous runs
      if (redrawGraph)
      { 
        this.simulationDataContainer = [];
      }
      this.statistics = [];
      this.summaryStatisticsAgg = "";
      this.workloadReductionStats = "";
      this.workloadReductionPercentStats = "";

      // Split the TSV string into rows
      const rows = this.PriorityScreeningSimulationText.split('\n');

      // Extract the headers
      const headers = rows[0].split('\t');
      headers[0] = "index";

      // Transform the rows
      const transformedArray = rows.slice(1).map(row => {
        const values = row.split('\t');
        const obj: any = {};
        headers.forEach((header, index) => {
          const num = Number(values[index]);
          if (!isNaN(num)) {
            obj[header] = num;
          }
        });
        if (Object.keys(obj).length === headers.length) {
          return obj;
        }
      }).filter(row => row !== undefined);

      // Extract the index and first simulation columns
      const indices = transformedArray.map(row => row.index);
      const values = transformedArray.map(row => row.CumulativeIncl1);

      // Get the maximum index and included item count
      this.simulationDataItemCount = Math.max(...indices);
      this.simulationDataIncludedItemsCount = Math.max(...values);
      this.simulationDataItemCountRoundedUp = nextMultipleOfTen(Math.max(...indices));
      this.simulationDataIncludedItemsCountRoundedUp = nextMultipleOfTen(Math.max(...values));
      this.xAxisText = "Number of items screened (N=" + this.simulationDataItemCount + ")";
      this.yAxisText = "Cumulative number of includes found (N=" + this.simulationDataIncludedItemsCount + ")";
      

      // lastly, set the data
      let simCount = (headers.length - 1) / 2;
      if (redrawGraph) {
        for (let c = 0; c < simCount; c++) {
          this.simulationDataContainer.push(transformedArray.map(row => [Number(row.index), Number(row["CumulativeIncl" + String(c + 1)])]));
        }
      }

      // now calculate the statistics
      let nScreenedArray = [];
      let workloadReductionArray = [];
      let workloadReductionPercentArray = [];

      for (let c = 0; c < simCount; c++) {
        const simulationData = transformedArray.map(row => [Number(row.index), Number(row["CumulativeIncl" + String(c + 1)])]);

        // Find max index and value
        const indices = simulationData.map(row => row[0]);
        const values = simulationData.map(row => row[1]);

        const maxIndex = Math.max(...indices);
        const maxValue = Math.max(...values);

        // Find the index when the value first reaches maximum
        const firstMaxIndex = simulationData.find(row => row[1] === Math.round((maxValue * this.recallLevel) / 100))?.[0] ?? 0;
        let workloadReduction = maxIndex - firstMaxIndex;
        let workloadReductionPercent = Math.round(100 - (firstMaxIndex / maxIndex * 100))

        nScreenedArray.push(firstMaxIndex);
        workloadReductionArray.push(workloadReduction);
        workloadReductionPercentArray.push(workloadReductionPercent);

        this.statistics.push({
          simulation: c,
          nIncludes: maxValue,
          nScreened: maxIndex,
          nScreenedAt100: firstMaxIndex,
          workloadReduction: workloadReduction,
          workloadReductionPercent: workloadReductionPercent
        });
      }
      let meanNScreened = Math.round(mean(nScreenedArray));
      let stdev = Math.round(standardDeviation(nScreenedArray));
      let conf = Math.round(calculateConfidence(stdev, simCount));
      let ciLower = meanNScreened - conf;
      let ciUpper = meanNScreened + conf;
      this.summaryStatisticsAgg = "Mean number to screen to achieve " + this.recallLevel + "% recall (" + Math.round(this.simulationDataIncludedItemsCount * this.recallLevel / 100) +
        " out of " + this.simulationDataIncludedItemsCount + " retreived): " + meanNScreened + " (" + ciLower + ", " + ciUpper + ").";
      this.workloadReductionStats = "Mean simulated workload reduction: " + String(Math.round(this.simulationDataItemCount - meanNScreened)) +
        " (" + String(Math.round(this.simulationDataItemCount - ciUpper)) + ", " + String(Math.round(this.simulationDataItemCount - ciLower)) + ")";
      this.workloadReductionPercentStats = "Mean simulated workload reduction percent: " + String(Math.round(100 - (meanNScreened / this.simulationDataItemCount * 100))) +
        "% (" + String(Math.round(100 - (ciUpper / this.simulationDataItemCount * 100))) + ", " + String(Math.round(100 -
          (ciLower / this.simulationDataItemCount * 100))) + ")";
    }
  }


  public exportChart(): void {

  this.VisualiseChart.exportImage().then((dataURI) => {
    saveAs(dataURI, 'chart.png');
  });

}


  downloadData() {
    if (this.PriorityScreeningSimulationText != null) {
      const blob = new Blob([this.PriorityScreeningSimulationText], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', 'simulationStudyResults.tsv');
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  }

  public confirmDeleteSimulation(simulation: PriorityScreeningSimulation) {
    this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to delete this priority screening simulation ?', false, '')
      .then(
        (confirmed: any) => {
          console.log('User confirmed:', confirmed);
          if (confirmed) {
            this.deletePriorityScreeningSimulation(simulation);
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
  }

  deletePriorityScreeningSimulation(simulation: PriorityScreeningSimulation) {
    this.classifierService.DeletePriorityScreeningSimulation(simulation.simulationName);
  }


  public get nodeSelected(): singleNode | null | undefined {
    return this._eventEmitterService.nodeSelected;
  }
  SetAttrOn(node: singleNode | null | undefined) {
    //alert(JSON.stringify(node));
    if (node != null && node.nodeType == "SetAttribute") {
      let a = node as SetAttribute;
      this.selectedModelDropDown1 = node.name;
      this.DD1 = a.attribute_id;
      this._eventEmitterService.nodeSelected = undefined;
    }
    else {
      this.selectedModelDropDown1 = "";
      this.DD1 = 0;
    }
  }
  SetAttrNotOn(node: singleNode | null | undefined) {
    //alert(JSON.stringify(node));
    if (node != null && node != undefined && node.nodeType == "SetAttribute") {
      let a = node as SetAttribute;
      this.selectedModelDropDown2 = node.name;
      this.DD2 = a.attribute_id;
      this._eventEmitterService.nodeSelected = undefined;
    }
    else {
      this.selectedModelDropDown2 = "";
      this.DD2 = 0;
    }
  }
  public isCollapsedPriorityScreening: boolean = false;
  public isCollapsed2PriorityScreening: boolean = false;
  public selectedModelDropDown1: string = '';
  public selectedModelDropDown2: string = '';
  public DD1: number = 0;
  public DD2: number = 0;
  public simulationNameText: string = '';

  CloseBMDropDown1() {

    this.isCollapsedPriorityScreening = false;
  }
  CloseBMDropDown2() {

    this.isCollapsed2PriorityScreening = false;
  }

  CanPriorityScreening() {
    if (this.CanWrite() == false) return false;
    if (this.selectedModelDropDown1 && this.selectedModelDropDown2 && this.simulationNameText != '' && !this.SimulationNameAlreadyExists
      && this.DD1 > 0 && this.DD2 > 0 && this.DD1 != this.DD2) {
      return true;
    }
    return false;
  }
  public get SameCodesAreSelected(): boolean {
    if (this.selectedModelDropDown1 && this.selectedModelDropDown2
      && this.DD1 > 0 && this.DD1 == this.DD2) {
      return true;
    }

    return false;
  }

  SimulationNameAlreadyExists: boolean = false;

  CheckIfSimulationNameAlreadyExists() {
    for (let c = 0; c < this.PriorityScreeningSimulationList.length; c++) {
      if (this.PriorityScreeningSimulationList[c].simulationName.replace(".tsv", "").toLowerCase() == this.simulationNameText.toLowerCase()) {
        this.SimulationNameAlreadyExists = true;
        return;
      }
    }
    this.SimulationNameAlreadyExists = false;
  }


  public openConfirmationDialogPriorityScreening() {
    this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to run the priority screening simulation with these codes ?', false, '')
      .then(
        (confirmed: any) => {
          console.log('User confirmed:', confirmed);
          if (confirmed) {
            this.PriorityScreening();
          }
          else {
            //alert('pressed cancel close dialog');
          };
        }
      )
      .catch(() => { });
  }

  async PriorityScreening() {

    if (this.DD1 != null && this.DD2 != null && this.simulationNameText != "") {

      let res = await this.classifierService.RunPriorityScreeningSimulation("¬¬PriorityScreening¬¬" + this.simulationNameText, this.DD1, this.DD2);

      if (res != false) { //we get "false" if an error happened...
        if (res == "Starting...") {
          this.notificationService.show({
            content: 'Priority screening simulation started. Results will appear below when finished (click refresh list periodically)',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            hideAfter: 5000
          });
          //this.Close();-- I think for this one we'll keep the window open, as you might want to look at other results
        }
        else if (res == "Already running") {
          this.notificationService.show({
            content: 'Priority screening simulation could not be run. A priority screening simulation is already running for this review',
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "error", icon: true },
            closable: true
          });
        }
        else if ((res as string).startsWith("Insufficient Data")) {
          this.modalService.GenericErrorMessage((res as string));
        }
        else {
          this.modalService.GenericErrorMessage("Unexpected result:\r\n" + res.toString());
        }
      }
    }

  }
  Close() {
    this.PleaseCloseMe.emit();
  }


  ngOnDestroy() {

  }

  BackToMain() {
    this.router.navigate(['Main']);
  }
}
