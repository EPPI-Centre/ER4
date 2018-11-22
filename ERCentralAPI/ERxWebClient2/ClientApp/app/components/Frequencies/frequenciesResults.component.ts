import { Component,  OnInit,  OnDestroy, AfterViewInit, ViewChild,  Input } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { frequenciesService, Frequency } from '../services/frequencies.service';
import { ReviewSetsService, SetAttribute } from '../services/ReviewSets.service';
import { ItemListComp } from '../ItemList/itemListComp.component';
import { EventEmitterService } from '../services/EventEmitter.service';
import { BaseChartDirective } from 'ng2-charts';

@Component({
   
    selector: 'frequenciesResults',
    templateUrl: './frequenciesResults.component.html',
    providers: [],

})
export class frequenciesResultsComp implements OnInit, OnDestroy, AfterViewInit {

	constructor(private router: Router,
		private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService,
        public PriorityScreeningService: PriorityScreeningService,
        private ReviewSetsService: ReviewSetsService,
		public frequenciesService: frequenciesService,
		private _eventEmitter: EventEmitterService,
    ) {}

    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            this.frequenciesService.frequenciesChanged.subscribe(
                () => {
                    this.RedrawGraph();
                });
        }
    }


    ngAfterViewInit() {
        // child is set
	}
	@ViewChild('testChart') testChart!: BaseChartDirective;
    @ViewChild('ItemList') ItemListComponent!: ItemListComp;
    

    @Input() ChartType: string | undefined;
    
    private _ShowNoneOfTheAbove: boolean | undefined;
    @Input() set ShowNoneOfTheAbove(value: boolean | undefined) {
        console.log('ouch source bis');
        this.RedrawGraph();
        this._ShowNoneOfTheAbove = value;
        //this.doSomething(this._categoryId);
    }
    get ShowNoneOfTheAbove(): boolean | undefined {
        return this._ShowNoneOfTheAbove;
    }

	
    public get FrequenciesData(): Frequency[] {
        if (this._ShowNoneOfTheAbove) return this.frequenciesService.Frequencies;
        else return this.frequenciesService.Frequencies.slice(0, this.frequenciesService.Frequencies.length - 1);
    }
	
   
    

    RedrawGraph() {
        console.log("I'm doing RedrawGraph...");
        if (this.ChartType == 'table') return;
        else {
            const tmp = this.ChartType
            this.ChartType = 'table';
            setTimeout(() => {
                this.ChartType = tmp;
            }, 50);
        }
    }

    public DataForPlots: Frequency[] = [];
    
	onSubmit(f: string) {

    }

    FrequencyGetItemList(freq: Frequency) {
        let cr: Criteria = new Criteria();
        cr.onlyIncluded = freq.isIncluded;
        cr.showDeleted = false;
        cr.pageNumber = 0;
        let ListDescription: string = "";
        let attributeFilter: SetAttribute | null = this.ReviewSetsService.FindAttributeById(freq.filterAttributeId);
        if (freq.attribute == "None of the codes above" && freq.attributeSetId < 0) {
            //CASE: the special one for getting everything else (not listed in freq results)
            console.log('FrequencyNoneOfTheAbove, freq.attributeSetId = ' + freq.attributeSetId);
            ListDescription = "Frequencies '" + freq.attribute + "'" + (freq.filterAttributeId > 0 && attributeFilter ? " (filtered by: " + attributeFilter.attribute_name + ")." : ".");
            cr.listType = "FrequencyNoneOfTheAbove";
            cr.xAxisAttributeId = - freq.attributeId;
            cr.sourceId = 0;
            cr.setId = freq.setId;
            cr.filterAttributeId = freq.filterAttributeId;
        }
        else {
            if (freq.filterAttributeId < 1 || !attributeFilter) {
                //CASE2: we don't have a filter by "items with this code"
                ListDescription = freq.attribute + ".";
                cr.attributeid = freq.attributeId;
                cr.sourceId = 0;
                cr.listType = "StandardItemList";
                cr.attributeSetIdList = freq.attributeSetId.toString();
            }
            else {
                //CASE3: we do have the filter
                ListDescription = freq.attribute + "; filtered by: " + attributeFilter.attribute_name + ".";
                cr.listType = "FrequencyWithFilter";
                cr.xAxisAttributeId = freq.attributeId;
                cr.filterAttributeId = freq.filterAttributeId;
                cr.setId = freq.setId;

            }
        }
        this.ItemListService.FetchWithCrit(cr, ListDescription);
        this._eventEmitter.PleaseSelectItemsListTab.emit();
	}
    
	ngOnDestroy() {
	}

}






