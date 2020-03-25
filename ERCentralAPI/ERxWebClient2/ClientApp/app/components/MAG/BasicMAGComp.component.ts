import { Component, OnInit, ViewChild} from '@angular/core';
import { searchService } from '../services/search.service';
import { BasicMAGService, MagRelatedPapersRun } from '../services/BasicMAG.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ItemListService } from '../services/ItemList.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { NotificationService } from '@progress/kendo-angular-notification';


@Component({
	selector: 'BasicMAGComp',
	templateUrl: './BasicMAGComp.component.html',
	providers: []
})

export class BasicMAGComp implements OnInit {

	constructor(private ConfirmationDialogService: ConfirmationDialogService,
        private _basicMAGService: BasicMAGService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _notificationService: NotificationService,
        private router: Router

	) {

    }
    @ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
    public CurrentDropdownSelectedCode: singleNode | null = null;
    public ItemsWithCode: boolean = false;
    public MAGItems: any[] = [];
    public ShowPanel: boolean = false;
    public isCollapsed: boolean = true;
    public description: string = '';
    public valueKendoDatepicker: Date = new Date(2000, 2, 10);
    public searchAll: string = 'true';
    public magSearchCheck: boolean = false;
    public magDateRadio: string = 'true';
    public magRCTRadio: string = 'NoFilter';
    public magMode: string = '';

	ngOnInit() {

        if (this._ReviewerIdentityServ.reviewerIdentity.userId == 0 ||
            this._ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else if (!this._ReviewerIdentityServ.HasWriteRights) {
            this.router.navigate(['Main']);
        }
        else {
            //this.ReviewSetsEditingService.FetchReviewTemplates();
        }

    }
    Clear() {

        this.CurrentDropdownSelectedCode = {} as SetAttribute;
        this.description = '';
        this.ItemsWithCode = false;
        this.magDateRadio = 'true';
        this.magMode = '';

    }
    public AdvancedFeatures() {
        
        this.router.navigate(['AdvancedMAGFeatures']);
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
	public ToggleMAGPanel(): void {
		this.ShowPanel = !this.ShowPanel;
	}
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get IsServiceBusy(): boolean {
        return this._basicMAGService.IsBusy;
    }
    public GetItems(item: MagRelatedPapersRun) {

        //console.log(item.magRelatedRunId );
        if (item.magRelatedRunId > 0) {
            this._basicMAGService.FetchMAGRelatedPaperRunsListId(item.magRelatedRunId).then(
                () => {
                    //console.log('List of papers: ', this._magService.MagPaperList);
                    this.router.navigate(['MAGBrowser']);
                }
            );
        }
    }
    public ImportMagSearchPapers(item: MagRelatedPapersRun) {
        //console.log(item.status + ' : ' + item.nPapers);
   
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
    public CanDeleteMAGRun() : boolean {
        return this.HasWriteRights;
    }
    public CanGetPapers(paperNumbers: number): boolean {
        if (paperNumbers > 0 && this.HasWriteRights) {
            return true;
        } else {
            return false;
        }

    }
    public CanAddNewMAGSearch(): boolean {

        if (this.description != '' && this.description != null && this.HasWriteRights
            ) {
            return true;
        } else {
            return false;
        }
    }
    public CanImportMagPapers(item: MagRelatedPapersRun): boolean {

        if (item != null && item.magRelatedRunId > 0 && item.nPapers > 0 && this.HasWriteRights) {
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
        magRun.dateFrom = this.valueKendoDatepicker;
		magRun.autoReRun = this.magSearchCheck.toString();
		magRun.filtered = this.magRCTRadio;
		magRun.mode = this.magMode;
		magRun.userDescription = this.description;

        this._basicMAGService.CreateMAGRelatedRun(magRun);

    }
    public ImportMagRelatedPapersRun(magRun: MagRelatedPapersRun, msg: string) {

        this.ConfirmationDialogService.confirm("Importing papers for the selected MAG run",
                msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._basicMAGService.ImportMagRelatedRunPapers(magRun);
                }
            });
    }
	public DoDeleteMagRelatedPapersRun(magRunId: number) {

        this.ConfirmationDialogService.confirm("Deleting the selected MAG run",
            "Are you sure you want to delete MAG run Id:" + magRunId + "?", false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._basicMAGService.DeleteMAGRelatedRun(magRunId);
                }
            });
        }
}
	