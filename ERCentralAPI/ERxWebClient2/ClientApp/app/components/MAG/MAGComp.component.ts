import { Component, OnInit, ViewChild} from '@angular/core';
import { searchService } from '../services/search.service';
import { MAGService, MagRelatedPapersRun, MagPaperListSelectionCriteria } from '../services/mag.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import {  ItemListService } from '../services/ItemList.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { AdvancedMAGFeaturesComponent } from './AdvancedMAGFeatures.component';
import { NotificationService } from '@progress/kendo-angular-notification';


@Component({
	selector: 'MAGComp',
	templateUrl: './MAGComp.component.html',
	providers: []
})

export class MAGComp implements OnInit {

	constructor(private ConfirmationDialogService: ConfirmationDialogService,
		private _magService: MAGService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _notificationService: NotificationService,
        private _itemListService: ItemListService,
        private _eventEmitter: EventEmitterService,
        private router: Router

	) {

	}


	ngOnInit() {

        this.Clear();
        //this._magService.Fetch();

    }
    public AdvancedFeatures() {
        
        this.router.navigate(['AdvancedMAGFeatures']);
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
	public magSearchCheck: boolean = false;
    public magDateRadio: string = 'true';
    public magRCTRadio: string = 'NoFilter';
	public magMode: string = '';
	public ToggleMAGPanel(): void {
		this.ShowPanel = !this.ShowPanel;
	}
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public GetItems(item: MagRelatedPapersRun) {

        console.log(item.magRelatedRunId );
        if (item.magRelatedRunId > 0) {
            this._magService.Fetch(item.magRelatedRunId).then(
                () => {
                    console.log('List of papers: ', this._magService.MagPaperList);
                    this.router.navigate(['MAGBrowser']);
                }
            );
        }
    }
    public ImportMagSearchPapers(item: MagRelatedPapersRun) {
        console.log(item.status + ' : ' + item.nPapers);
   
        if (item.nPapers == 0) {
            this.ShowMAGRunMessage('There are no papers to import');

        } else if (item.status == 'Imported') {
            this.ShowMAGRunMessage('Papers have already been imported');

        } else if (item.status == 'Checked') {
           
            let msg: string = 'Are you sure you want to import these items?\n(This set is already marked as \'checked\'.)';
            this.ImportMagRelatedPapersRun(item, msg);

        } else if (item.status == 'Unchecked') {
          
            let msg: string = 'Are you sure you want to import these items?';
            this.ImportMagRelatedPapersRun(item, msg);
        }
        

    }
    private ShowMAGRunMessage(notifyMsg: string) {

        this._notificationService.show({
            content: notifyMsg,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
        });
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
        this.magDateRadio = 'true';
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
    public CanImportMagPapers(item: MagRelatedPapersRun): boolean {

        if (item != null && item.magRelatedRunId > 0 && this.HasWriteRights) {
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
    public ImportMagRelatedPapersRun(magRun: MagRelatedPapersRun, msg: string) {

        this.ConfirmationDialogService.confirm("Importing papers for the selected MAG run",
                msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magService.ImportMagPapers(magRun);
                }
            });
    }
	public DoDeleteMagRelatedPapersRun(magRun: MagRelatedPapersRun) {

        this.ConfirmationDialogService.confirm("Deleting the selected MAG run",
            "Are you sure you want to delete MAG RUN:" + magRun.magRelatedRunId + "?", false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magService.Delete(magRun);
                }
            });
        }
}
	