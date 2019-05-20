import { Component,  OnInit, Input} from '@angular/core';
import { Router } from '@angular/router';
import { ReconciliationService } from '../services/reconciliation.service';
import { PagerService } from '../services/pagination.service';
import { EventEmitterService } from '../services/EventEmitter.service';


@Component({
	selector: 'comparisonPaginator',
	templateUrl: './comparisonPaginator.component.html',
	providers: [],
	styles: ["button.disabled {color:black; }"]
})

export class ComparisonPaginatorComp implements OnInit {
	constructor(private router: Router,
		private _reconciliationService: ReconciliationService,
		private _eventEmitterService: EventEmitterService,
		private pagerService: PagerService) { }

	@Input() localItems: any[] =[];

	// array of all items to be paged
	private allItems: any[] = [];

	// pager object
	pager: any = {};

	// paged items
	pagedItems!: any[];

	ngOnInit() {

		// get data
		this._eventEmitterService.reconDataChanged.subscribe(

			(res: any) => {
				
				this.allItems = res;
				console.log('the data changed' + this.allItems);
			}
		);
		
		// initialize to page 1
		this.setPage(1);
		
	}

	setPage(page: number) {
		// get pager object from service
		this.pager = this.pagerService.getPager(this.allItems.length, page);

		// get current page of items
		this.pagedItems = this.allItems.slice(this.pager.startIndex, this.pager.endIndex + 1);
	}
}




