import { Component, OnInit, ViewChild} from '@angular/core';
import { searchService } from '../services/search.service';
import { BasicMAGService } from '../services/BasicMAG.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { NotificationService } from '@progress/kendo-angular-notification';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MagRelatedPapersRun } from '../services/MAGClasses.service';
import { Location } from '@angular/common';

@Component({
	selector: 'BasicMAGComp',
	templateUrl: './BasicMAGComp.component.html',
	providers: []
})

export class BasicMAGComp implements OnInit {

	constructor(private ConfirmationDialogService: ConfirmationDialogService,
        public _basicMAGService: BasicMAGService,
        private _magBrowserService: MAGBrowserService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _notificationService: NotificationService,
        private _location: Location,
        private router: Router

    ) {

    }
    @ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
    public CurrentDropdownSelectedCode: singleNode | null = null;
    public ShowPanel: boolean = false;
    public isCollapsed: boolean = false;
    public dropdownBasic1: boolean = false;
    public description: string = '';
    public valueKendoDatepicker: Date = new Date();
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
          
             this._basicMAGService.FetchMagRelatedPapersRunList();

        }

    }
    public Back() {
        this.router.navigate(['Main']);
    }
    Clear() {

        this.CurrentDropdownSelectedCode = {} as SetAttribute;
        this.description = '';
        this.magDateRadio = 'true';
        this.magMode = '';

    }
	CloseCodeDropDown() {
        
        console.log(this.WithOrWithoutCodeSelector);
        let node: SetAttribute = this.WithOrWithoutCodeSelector.SelectedNodeData as SetAttribute;
        this.CurrentDropdownSelectedCode = node;

        this.isCollapsed = false;
       
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get IsServiceBusy(): boolean {

        return this._basicMAGService.IsBusy || this._magBrowserService.IsBusy;
    }
    public GetItems(item: MagRelatedPapersRun) {

        if (item.magRelatedRunId > 0) {

            this._magBrowserService.FetchMAGRelatedPaperRunsListById(item.magRelatedRunId)
                .then(
                    () => {
                        this.router.navigate(['MAGBrowser']);
                    }
            );
        }
    }
    public ImportMagSearchPapers(item: MagRelatedPapersRun) {

        console.log('testing mag import: ', item);
   
        if (item.nPapers == 0) {
            this.ShowMAGRunMessage('There are no papers to import');

        } else if (item.userStatus == 'Imported') {
            this.ShowMAGRunMessage('Papers have already been imported');

        } else if (item.userStatus == 'Checked') {
           
            let msg: string = 'Are you sure you want to import these items?\n(This set is already marked as \'checked\'.)';
            this.ImportMagRelatedPapersRun(item, msg);

        } else if (item.userStatus == 'Unchecked') {
          
            let msg: string = 'Are you sure you want to import these items?';
            this.ImportMagRelatedPapersRun(item, msg);
        }
    }
    public UpdateAutoReRun(magRelatedRun: MagRelatedPapersRun) {

        magRelatedRun.autoReRun = !magRelatedRun.autoReRun;
        if (magRelatedRun != null && magRelatedRun.magRelatedRunId > 0) {
            this._basicMAGService.UpdateMagRelatedRun(magRelatedRun);
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
            case '8':
                this.magMode = 'New items published on next deployment of MAG';
                break;

            default:
                break;
		}
	}
	public AddNewMAGSearch() {

		let magRun: MagRelatedPapersRun = new MagRelatedPapersRun();

        if (this.searchAll =='true') {
            magRun.allIncluded = true;
        } else {
            magRun.allIncluded = false;
        }
		let att: SetAttribute = new SetAttribute();
		if (this.CurrentDropdownSelectedCode != null) {
			att = this.CurrentDropdownSelectedCode as SetAttribute;
            magRun.attributeId = att.attribute_id;
            magRun.attributeName = att.name;
        }
        magRun.dateFrom = this.valueKendoDatepicker.toDateString();
		magRun.autoReRun = this.magSearchCheck;
		magRun.filtered = this.magRCTRadio;
		magRun.mode = this.magMode;
		magRun.userDescription = this.description;

        this._basicMAGService.CreateMAGRelatedRun(magRun);

    }
    public ImportMagRelatedPapersRun(magRun: MagRelatedPapersRun, msg: string) {

       this.ConfirmationDialogService.confirm("Importing papers for the selected MAG search",
                msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._basicMAGService.ImportMagRelatedRunPapers(magRun);
                }
            });
    }
	public DoDeleteMagRelatedPapersRun(magRunId: number) {

        this.ConfirmationDialogService.confirm("Deleting the selected MAG search",
            "Are you sure you want to delete MAG search Id:" + magRunId + "?", false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._basicMAGService.DeleteMAGRelatedRun(magRunId);
                }
            });
        }
}
	