import { Component, OnInit, ViewChild, Input, Output, EventEmitter} from '@angular/core';
import { searchService } from '../services/search.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MagRelatedPapersRun, MagBrowseHistoryItem} from '../services/MAGClasses.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { EventEmitterService } from '../services/EventEmitter.service';

@Component({
	selector: 'BasicMAGComp',
	templateUrl: './BasicMAGComp.component.html',
	providers: []
})

export class BasicMAGComp implements OnInit {

	constructor(private _confirmationDialogService: ConfirmationDialogService,
        public _basicMAGService: MAGRelatedRunsService,
        private _magBrowserService: MAGBrowserService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _eventEmitterService: EventEmitterService,
        private router: Router,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService

    ) {

    }
    @ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
    @Input() OuterContext: string | null = null;
    @Output() PleaseGoTo = new EventEmitter<string>();
    public CurrentDropdownSelectedCode: singleNode | null = null;
    public ShowPanel: boolean = false;
    public isCollapsed: boolean = false;
    public dropdownBasic1: boolean = false;
    public description: string = '';
    public valueKendoDatepicker: Date = new Date();
    public searchAll: string = 'all';
    public magSearchCheck: boolean = false;
    public magDateRadio: string = 'true';
    public magRCTRadio: string = 'NoFilter';
    public magMode: string = '';
    public basicSearchPanel: boolean = false;

	ngOnInit() {
        

        if (this._ReviewerIdentityServ.reviewerIdentity.userId == 0 ||
            this._ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else if (!this._ReviewerIdentityServ.HasWriteRights) {
            this.router.navigate(['Main']);
        }
        else {
             //this._eventEmitterService.firstVisitMAGBrowserPage = true;
             //this._basicMAGService.FetchMagRelatedPapersRunList();
        }

    }
    public Back() {
        this.router.navigate(['Main']);
    }
    public Refresh() {
        this._basicMAGService.FetchMagRelatedPapersRunList();
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

            this._magBrowserService.GetMagRelatedRunsListById(item.magRelatedRunId).then(
                    (res) => {
                    //this.router.navigate(['MAGBrowser']);
                    if (res) {
                        console.log("want to go to MagRelatedPapersRunList");
                        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Papers identified from auto-identification run", "MagRelatedPapersRunList", 0,
                            "", "", 0, "", "", 0, "", "", item.magRelatedRunId));
                        this.PleaseGoTo.emit("MagRelatedPapersRunList");
                    }
                }
            );
       
        }
    }

    public ShowSearchPanel() {
        this.basicSearchPanel = !this.basicSearchPanel;
    }
    public ImportMagSearchPapers(item: MagRelatedPapersRun) {

        if (item.nPapers == 0) {
            this._confirmationDialogService.showMAGRunMessage('There are no papers to import');

        } else if (item.userStatus == 'Imported') {
            this._confirmationDialogService.showMAGRunMessage('Papers have already been imported');

        } else if (item.userStatus == 'Checked') {
           
            let msg: string = 'Are you sure you want to import these items?\n(This set is already marked as \'checked\'.)';
            this.ImportMagRelatedPapersRun(item, msg);

        } else if (item.userStatus == 'Unchecked' || item.userStatus == 'Not imported') {
          
            let msg: string = 'Are you sure you want to import these items?';
            this.ImportMagRelatedPapersRun(item, msg);
        }
    }
    public UpdateUserStatus(magRelatedRun: MagRelatedPapersRun) {

        if (magRelatedRun != null && magRelatedRun.magRelatedRunId > 0
            && magRelatedRun.userStatus == 'Unchecked') {
            magRelatedRun.userStatus = 'Checked';
            this._basicMAGService.UpdateMagRelatedRun(magRelatedRun);
        }

    }
    public UpdateAutoReRun(magRelatedRun: MagRelatedPapersRun) {

        magRelatedRun.autoReRun = !magRelatedRun.autoReRun;
        if (magRelatedRun != null && magRelatedRun.magRelatedRunId > 0) {
            this._basicMAGService.UpdateMagRelatedRun(magRelatedRun);
        }

    }

    public CanDeleteMAGRun() : boolean {
        return this.HasWriteRights;
    }
    public CanAddNewMAGSearch(): boolean {

        if (this.description != '' && this.description != null && this.HasWriteRights
            && this.magMode != '') {
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
                this.magMode = 'BiCitation';
                break;
            case '7':
                this.magMode = 'Bi-Citation AND Recommendations';
                break;
            default:
                break;
		}
    }

    public CheckedStatus(magRelatedRun: MagRelatedPapersRun) {

        let msg: string = "";

        let status: string = magRelatedRun.userStatus;
        if (status == 'Checked') {

            msg = 'you have marked this search as checked';
        } else if (status == 'Unchecked') {
            this.UpdateUserStatus(magRelatedRun);
            msg = "updated run to be marked as checked";
        } else if (status == 'Waiting') {
            msg = 'this search is in a waiting state';
        } else if (status == 'Imported') {
            msg = 'you have imported the papers in this search already';
        } else {
            msg = 'there is an error in the status';
        }
        // JT commented out. We don't need to do anything with this now
        //this._confirmationDialogService.showMAGRunMessage(msg);

    }

	public AddNewMAGSearch() {

		let magRun: MagRelatedPapersRun = new MagRelatedPapersRun();

        if (this.searchAll =='all') {
            magRun.allIncluded = true;
        } else if (this.searchAll == 'specified') {
            magRun.allIncluded = false;
        }else{
           //children of this code...
        }
		let att: SetAttribute = new SetAttribute();
		if (this.CurrentDropdownSelectedCode != null) {
			att = this.CurrentDropdownSelectedCode as SetAttribute;
            magRun.attributeId = att.attribute_id;
            magRun.attributeName = att.name;
        }
        if (this.magDateRadio == 'false') {
            magRun.dateFrom = this.valueKendoDatepicker.toDateString();
        }   
		magRun.autoReRun = this.magSearchCheck;
		magRun.filtered = this.magRCTRadio;
		magRun.mode = this.magMode;
		magRun.userDescription = this.description;

        this._basicMAGService.CreateMAGRelatedRun(magRun);

    }
    public ImportMagRelatedPapersRun(magRun: MagRelatedPapersRun, msg: string) {

        this._confirmationDialogService.confirm("Importing papers for the selected search",
                msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._basicMAGService.ImportMagRelatedRunPapers(magRun);
                }
            });
    }
    public DoDeleteMagRelatedPapersRun(magRun: MagRelatedPapersRun) {

        this._confirmationDialogService.confirm("Deleting the selected search",
            "Are you sure you want to delete this search:" + magRun.userDescription + "?", false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._basicMAGService.DeleteMAGRelatedRun(magRun.magRelatedRunId);
                }
            });
        }
}
	