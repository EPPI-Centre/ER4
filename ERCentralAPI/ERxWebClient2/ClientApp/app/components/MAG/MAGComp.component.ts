import { Component, OnInit, ViewChild} from '@angular/core';
import { searchService } from '../services/search.service';
import { MAGService, MagRelatedPapersRun } from '../services/mag.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

@Component({
	selector: 'MAGComp',
	templateUrl: './MAGComp.component.html',
	providers: []
})

export class MAGComp implements OnInit {

	constructor(private ConfirmationDialogService: ConfirmationDialogService,
		private _magService: MAGService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService

	) {

	}

	ngOnInit() {

        this.Clear();
    }

	@ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
	public CurrentDropdownSelectedCode: singleNode | null = null;
	public ItemsWithCode: boolean = false;
	public MAGItems: any[] = [];
	public ShowPanel: boolean = false;
    public isCollapsed: boolean = true;

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


	public AddNewMAGSearch() {

		let magRun: MagRelatedPapersRun = new MagRelatedPapersRun();

		magRun.allIncluded = this.searchAll;
		let att: SetAttribute = new SetAttribute();
		if (this.CurrentDropdownSelectedCode != null) {
			att = this.CurrentDropdownSelectedCode as SetAttribute;
            magRun.attributeId = att.attribute_id;
            magRun.attributeName = att.name;
        }
        magRun.dateFrom = this.value;
		magRun.autoReRun = this.magSearchCheck.toString();
		magRun.filtered = this.magRCTRadio;
		magRun.mode = this.magMode;
		magRun.userDescription = this.desc;

		this._magService.Create(magRun);

	}
	public doDeleteMagRelatedPapersRun(magRun: MagRelatedPapersRun) {

        this.ConfirmationDialogService.confirm("Deleting the selected MAG run",
            "Are you sure you want to delete MAG RUN:" + magRun.magRelatedRunId + "?", false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magService.Delete(magRun);
                }
            });
        }
}
	