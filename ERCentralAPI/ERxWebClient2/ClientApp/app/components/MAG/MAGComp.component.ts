import { Component, OnInit, ViewChild} from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService } from '../services/ItemList.service';
import { searchService } from '../services/search.service';
import { MAGService } from '../services/mag.service';
import { singleNode } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';

@Component({
	selector: 'MAGComp',
	templateUrl: './MAGComp.component.html',
	providers: []
})

export class MAGComp implements OnInit {

	constructor(private router: Router,
		private ReviewerIdentityServ: ReviewerIdentityService,
		private _MAGService: MAGService,
		public _searchService: searchService,

	) {

	}

	ngOnInit() {

		
	}
	@ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
	public CurrentDropdownSelectedCode: singleNode | null = null;
	public ItemsWithCode: boolean = false;
	public MAGItems: any[] = [];
	public ShowPanel: boolean = false;
	public isCollapsed: boolean = false;
	CanOnlySelectRoots() {
		return true;
	}
	CloseCodeDropDown() {
		if (this.WithOrWithoutCodeSelector) {
			this.CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData;
		}
		this.isCollapsed = false;
	}
	public value: Date = new Date(2000, 2, 10);
	public searchAll: string = 'true';
	public magDate: string = 'true';
	public ToggleMAGPanel(): void {
		this.ShowPanel = !this.ShowPanel;
	}
	public HasWriteRights(): boolean{

		return true;
	}
	public IsServiceBusy(): boolean {

		return false;
	}
	public Selected(): void {

	}
	public ClearSelected(): void {

	}
	public ImportSelected(): void {

	}
	public AutoUpdateHome(): void {

	}
	public AdvancedFeatures(): void{

	}
	public ShowHistory(): void {

	}
	public Admin(): void{

	}
	public GoBack(): void {

	}
}
	
export interface iMAG {

}