import { Component, OnInit, ViewChild} from '@angular/core';
import { searchService } from '../services/search.service';
import { MAGService, MagRelatedPapersRun } from '../services/mag.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGAdvancedService, ClassifierContactModel } from '../services/magAdvanced.service';
import { Criteria, ItemListService } from '../services/ItemList.service';

@Component({
    selector: 'AdvancedMAGFeatures',
    templateUrl: './AdvancedMAGFeatures.component.html',
	providers: []
})

export class AdvancedMAGFeaturesComponent implements OnInit {

	constructor(private ConfirmationDialogService: ConfirmationDialogService,
		private _magAdvancedService: MAGAdvancedService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _itemListService: ItemListService,
        private router: Router

	) {

	}

	ngOnInit() {

        this.GetMAGCurrentInfo();
        this.GetContactModelList();
        this.Clear();
    }

    public GetContactModelList(): void {

       this._magAdvancedService.FetchClassifierContactModelList();
    }
    public RunMatchingAlgo() {

       let msg: string = 'Are you sure you want to match all the items in your review\n to Microsoft Academic records?';
        this.ConfirmationDialogService.confirm('MAG RUN ALERT', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magAdvancedService.RunMatchingAlgorithm();
                }
            });
    }

    public AdvancedFeatures() {

        this.router.navigate(['AdvancedMAGFeatures']);

    }

    public AutoUpdateHome() {

        this.router.navigate(['MAGFeatures']);
    }

    public Back() {
        this.router.navigate(['Main']);
    }

	@ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
	public CurrentDropdownSelectedCode: singleNode | null = null;
	public ItemsWithCode: boolean = false;
	public MAGItems: any[] = [];
	public ShowPanel: boolean = false;
    public isCollapsed: boolean = true;
    public magMatchedIncluded: number = 0;
    public magMatchedExcluded: number = 0;
    public magMatchedAll: number = 0;
    public magMatchedWithThisCode: number = 0;
    public magPaperId: number = 0;
    public currentClassifierContactModel: ClassifierContactModel = new ClassifierContactModel();

    public GetMatchedMagIncludedList() {

        //"Showing: included items that are matched to at least one Microsoft Academic record";
        let SelectionCritieraItemList: Criteria = new Criteria();
        SelectionCritieraItemList.listType = "MagMatchesMatched";
        SelectionCritieraItemList.onlyIncluded = true;
        SelectionCritieraItemList.showDeleted = false;
        SelectionCritieraItemList.attributeSetIdList = "";
        SelectionCritieraItemList.pageNumber = 0;

        this._itemListService.FetchWithCrit(SelectionCritieraItemList, "MagMatchesMatched");
     
    }
    public GetMatchedMagExcludedList() {

        let SelectionCritieraItemList: Criteria = new Criteria();
        SelectionCritieraItemList.listType = "MagMatchesMatched";
        SelectionCritieraItemList.onlyIncluded = false;
        SelectionCritieraItemList.showDeleted = false;
        SelectionCritieraItemList.attributeSetIdList = "";
        SelectionCritieraItemList.pageNumber = 0;

        this._itemListService.FetchWithCrit(SelectionCritieraItemList, "MagMatchesMatched");

    }
    public GetMatchedMagAllList() {

        let SelectionCritieraItemList: Criteria = new Criteria();
        SelectionCritieraItemList.listType = "MagMatchesMatched";
        SelectionCritieraItemList.onlyIncluded = true;
        SelectionCritieraItemList.showDeleted = false;
        SelectionCritieraItemList.attributeSetIdList = "";
        SelectionCritieraItemList.pageNumber = 0;

        this._itemListService.FetchWithCrit(SelectionCritieraItemList, "MagMatchesMatched");

    }
    public GetMatchedMagWithCodeList() {

        let SelectionCritieraItemList: Criteria = new Criteria();
        SelectionCritieraItemList.listType = "MagMatchesMatched";
        SelectionCritieraItemList.onlyIncluded = true;
        SelectionCritieraItemList.showDeleted = false;
        SelectionCritieraItemList.attributeSetIdList = "";
        SelectionCritieraItemList.pageNumber = 0;

        this._itemListService.FetchWithCrit(SelectionCritieraItemList, "MagMatchesMatched");

    }
    public GetMagPaper() {

        this._magAdvancedService.FetchMagPaper(this.magPaperId);

    }
	CanOnlySelectRoots() {
		return true;
	}
	CloseCodeDropDown() {
		if (this.WithOrWithoutCodeSelector) {
			this.CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData;
		}
		this.isCollapsed = false;
    }
	public desc: string = '';
	public value: Date = new Date(2000, 2, 10);
	public searchAll: string = 'true';
	public magDate: string = 'true';
	public magSearchCheck: boolean = false;
	public magDateRadio: boolean = false;
    public magRCTRadio: string = 'NoFilter';
	public magMode: string = '';
	public ToggleMAGPanel(): void {
		this.ShowPanel = !this.ShowPanel;
	}
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }

	public IsServiceBusy(): boolean {

		return false;
	}
	public Selected(): void {

	}
    Clear() {

        this.CurrentDropdownSelectedCode = {} as SetAttribute;
        this.desc = '';
        this.ItemsWithCode = false;
        this.magDate = '';
        this.magMode = '';

    }
    public CanDeleteMAGRun() : boolean {

        return this.HasWriteRights;
    }

    public CanAddNewMAGSearch(): boolean {

        if (this.desc != '' && this.desc != null && this.HasWriteRights
            ) {
            return true;
        } else {
            return false;
        }
    }

	public ClickSearchMode(searchModeChoice: string) {

		switch (searchModeChoice) {

            case '1':
                this.magMode = 'Recommended by';
                break;
            case '2':
                this.magMode = 'That recommend';
                break;
            case '3':
                this.magMode = 'Recommendations';
                break;
            case '4':
                this.magMode = 'Bibliography';
                break;
            case '5':
                this.magMode = 'Cited by';
                break;
            case '6':
                this.magMode = 'Bi-Citation';
                break;
            case '7':
                this.magMode = 'Bi-Citation AND Recommendations';
                break;

            default:
                break;
		}
	}
    
	public GetMAGCurrentInfo() {

        this._magAdvancedService.FetchCurrentInfo();
	}

    public GetMagSimulationList() {

        this._magAdvancedService.FetchMagSimulationList();

    }
}
	