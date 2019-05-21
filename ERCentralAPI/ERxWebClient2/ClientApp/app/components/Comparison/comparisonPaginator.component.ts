import { Component,  OnInit, Input, Output} from '@angular/core';
import { PagerService } from '../services/pagination.service';
import { EventEmitterService } from '../services/EventEmitter.service';


@Component({
	selector: 'comparisonPaginator',
	templateUrl: './comparisonPaginator.component.html',
	providers: [],
	styles: ["button.disabled {color:black; }"]
})

export class ComparisonPaginatorComp implements OnInit {
	constructor(
		private _eventEmitterService: EventEmitterService,
		private pagerService: PagerService) { }


	@Input() allItems: any[] = [];

	//private allItems: any[] = [];

	// pager object
	pager: any = {};

	// paged items
	pagedItems!: any[];

	setPage(page: number) {

		// get pager object from service
		console.log('==========length is: ' + this.allItems.length);
		this.pager = this.pagerService.getPager(this.allItems.length, page);
		console.log('setting pager: ' + JSON.stringify(this.pager) + 'current page: ' + this.pager.currentPage);
		// get current page of items
		this.pagedItems = this.allItems.slice(this.pager.startIndex, this.pager.endIndex + 1);
		this.pagerService.pagedItems = this.pagedItems;
		//this.localList.Items = this.pagedItems;

	}

	ngOnInit() {

		this._eventEmitterService.reconDataChanged.subscribe(
			() => {

				this.allItems = this._eventEmitterService.reconcilingArr;
				this.setPage(1);
			}
		);
	}

}




