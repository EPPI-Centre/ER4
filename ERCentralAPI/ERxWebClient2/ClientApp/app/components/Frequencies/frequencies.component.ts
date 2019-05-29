import { Component, OnInit, OnDestroy, Inject } from "@angular/core";
import { ReviewerIdentityService } from "../services/revieweridentity.service";
import { ItemListService } from "../services/ItemList.service";
import { ActivatedRoute } from "@angular/router";
import { ReviewSetsService, singleNode, SetAttribute } from "../services/ReviewSets.service";
import { EventEmitterService } from "../services/EventEmitter.service";
import { frequenciesService } from "../services/frequencies.service";

@Component({

    selector: 'frequencies',
    templateUrl: './frequencies.component.html',
    providers: [],

})
export class frequenciesComp implements OnInit {

    constructor(

		public reviewSetsService: ReviewSetsService,
        private frequenciesService: frequenciesService,
        @Inject('BASE_URL') private _baseUrl: string

    ) { }

    ngOnInit() { }

    public freqIncEx: string = 'true';
    public FreqShowWhat: string = 'table';
    public NoneOfTheAboveCB: boolean = true;
    public get selectedNode(): singleNode | null {
        return this.reviewSetsService.selectedNode;
    }
    public chosenNode: singleNode | null = null;
    public chosenFilter: SetAttribute | null = null;

    fetchFrequencies(selectedNodeDataF: any, selectedFilter: any) {
        //console.log('NoneOfTheAboveCB:' + this.NoneOfTheAboveCB);
        if (!selectedNodeDataF || selectedNodeDataF == undefined) {

            alert('Please select a code from the tree');

        } else {

            console.log(selectedNodeDataF.name);
            // need to filter data before calling the below Fetch	
            this.frequenciesService.crit.Included = this.freqIncEx == 'true';
            this.frequenciesService.Fetch(selectedNodeDataF, selectedFilter);

        }
    }
    canSetCode(): boolean {
        if (this.reviewSetsService.selectedNode
            && this.reviewSetsService.selectedNode.attributes
            && this.reviewSetsService.selectedNode.attributes.length > 0) return true;
        return false;
    }
    canSetFilter(): boolean {
        if (this.reviewSetsService.selectedNode
            && this.reviewSetsService.selectedNode.nodeType == "SetAttribute") return true;
        return false;
    }
    //public Code1: boolean = false;

    SetCode1() {
        //this.Code1 = true;
        this.chosenNode = this.reviewSetsService.selectedNode;
    }
    
    SetFilter() {
        if (this.reviewSetsService.selectedNode && this.reviewSetsService.selectedNode.nodeType == "SetAttribute")
            this.chosenFilter = this.reviewSetsService.selectedNode as SetAttribute;
    }

    clearChosenNode() {
        //this.Code1 = false;
        this.chosenNode = null;
    }
    clearChosenFilter() {
        this.chosenFilter = null;
    }
    public Clear() {
        this.chosenNode = null;
        this.chosenFilter = null;
        this.FreqShowWhat = 'table';
        this.freqIncEx = 'true';
        this.frequenciesService.Clear();
    }
}