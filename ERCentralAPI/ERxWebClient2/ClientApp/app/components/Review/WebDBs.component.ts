import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { WebDBService, iWebDB, iWebDbReviewSet } from '../services/WebDB.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { ReviewSetsService, SetAttribute, ReviewSet } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

@Component({
	selector: 'WebDBsComp',
	templateUrl: './WebDBs.component.html',
	providers: []
})

export class WebDBsComponent implements OnInit, OnDestroy {
	constructor(private router: Router,
		@Inject('BASE_URL') private _baseUrl: string,
		private WebDBService: WebDBService,
		private ReviewerIdentityService: ReviewerIdentityService,
		private ReviewSetsService: ReviewSetsService,
		private ModalService: ModalService,
		private ConfirmationDialogService: ConfirmationDialogService
	)
	{ }

	
	ngOnInit() {
		if (this.WebDBService.WebDBs.length == 0) this.WebDBService.Fetch();
		if (this.ReviewSetsService.ReviewSets.length == 0) this.ReviewSetsService.GetReviewSets();
	}
	//@Output() onCloseClick = new EventEmitter();
	//public isExpanded: boolean = false;
	public EditingDB: iWebDB | null = null;
	public EditingFilter: boolean = false;
	public ConfirmPassword: string = "";
	public isCollapsedFilterCode: boolean = false;
	@ViewChild('FilterCodeSelector') FilterCodeSelector!: codesetSelectorComponent;
	public selectedCodeSetDropDown: ReviewSet | null = null;
	public isCollapsedAddTool: boolean = false;
	public get WebDbs(): iWebDB[] {
		return this.WebDBService.WebDBs;
	}
	public get CurrentDB(): iWebDB | null {
		return this.WebDBService.CurrentDB;
	}
	public get IsServiceBusy(): boolean {
		return (this.WebDBService.IsBusy
			|| this.ReviewSetsService.IsBusy);
	}
	public FilterName(AttId: number): string {
		if (AttId == 0) return "No filter";
		else {
			let att = this.ReviewSetsService.FindAttributeById(AttId);
			if (att) return att.attribute_name;
			else return "Unknown Code (ID: " + AttId.toString() + ")";
        }
    }
    public get CanWrite(): boolean {
        //console.log("create rev check:", this._reviewerIdentityServ.reviewerIdentity);
		return this.ReviewerIdentityService.HasWriteRights && this.ReviewerIdentityService.HasAdminRights;
	}
	public get AddableCodingTools(): ReviewSet[] {
		//we allow to "add" only coding tools that aren't in the WebDB already...
		console.log("AddableCodingTools", this.ReviewSetsService.ReviewSets.length, this.WebDBService.CurrentSets.length);
		return this.ReviewSetsService.ReviewSets.filter(
			f => {
				//console.log("f:", f.set_id);
				return this.WebDBService.CurrentSets.findIndex(ff => {
					console.log("ff:", ff.setId);
					return ff.setId == f.set_id;
				}) == -1 || this.WebDBService.CurrentSets.length == 0;
			});
	}
	public get CurrentWebDbTools(): iWebDbReviewSet[] {
		return this.WebDBService.CurrentSets;
    }
	Edit(item: iWebDB | null) {
		if (!item) {
			item = {
				webDBId: 0,
				webDBName: '',
				webDBDescription: '',
				attributeIdFilter: 0,
				isOpen: true,
				userName: '',
				password: '',
				createdBy: '',
				editedBy: ''
			};
        }
		this.EditingDB = this.WebDBService.CloneWebDBforEdit(item);
        //this.isExpanded = true;
    }
	CancelEdit() {
		this.EditingDB = null;
		this.EditingFilter = false;
		this.isCollapsedFilterCode = false;
        //this.isExpanded = false;
	}
	ShowDBSetting(db: iWebDB) {
		//console.log("Changing DB: ", db);
		this.WebDBService.CurrentDB = db;
		this.ReloadDBDependentData();
	}
	DeleteDB(db: iWebDB) {
		//console.log("Changing DB: ", db);
		if (!this.CanWrite) return;
		this.ConfirmationDialogService.confirm("Delete Web Database?"
			, "Are you sure you want to delete this Web Database (Id: " + db.webDBId.toString() + ")? <BR /> This action is <strong>irreversible</strong> and the URL for this Web Database will stop working."
			, false, "", "Yes Delete", "Cancel").then((confirm: any) => {
				if (confirm) {
					this.WebDBService.Delete(db);
				}
			});
	}
	EditFilter() {
		this.EditingFilter = true;
		this.isCollapsedFilterCode = false;
	}
	CancelEditFilter() {
		this.EditingFilter = false;
		this.isCollapsedFilterCode = false;
	}
	RemoveFilter() {
		if (this.EditingDB) this.EditingDB.attributeIdFilter = 0;
		this.CancelEditFilter();
    }
	CloseCodeDropDownFilterCode() {
		if (this.FilterCodeSelector) {
			if (this.EditingDB) this.EditingDB.attributeIdFilter = (this.FilterCodeSelector.SelectedNodeData as SetAttribute).attribute_id;
		}
		this.EditingFilter = false;
		this.isCollapsedFilterCode = false;
    }
	public get CanSave(): boolean {
		return this.CantSaveMessage == "";
	}
	public get CantSaveMessage(): string {
		if (!this.CanWrite) return "Forbidden";
		if (this.EditingDB) {
			if (this.EditingDB.isOpen == false) {
				if (this.EditingDB.userName.length < 4) return "Username too short";
				else if (this.EditingDB.password.length > 0) {
					if (this.EditingDB.password.length < 6) return "Password is too short";
					if (this.ConfirmPassword != this.EditingDB.password) return "Password fields don't match";
                }
			}
		}
		return "";
    }
	Save() {
		if (this.EditingDB && this.CanSave) this.WebDBService.CreateOrEdit(this.EditingDB);
		this.CancelEdit();
	}

	GetCodingToolName(): string {
		if (this.selectedCodeSetDropDown == null) return "Please select...";
		else return this.selectedCodeSetDropDown.set_name;
	}
	SetCodeSetDropDown(item: ReviewSet) {
		this.selectedCodeSetDropDown = item;
    }
	AddCodingTool(item: ReviewSet) {
		if (this.WebDBService.CurrentDB != null)
			this.WebDBService.AddWebDbReviewSet(this.WebDBService.CurrentDB.webDBId, item.set_id);
		this.selectedCodeSetDropDown = null;
    }
	ReloadDBDependentData() {
		this.selectedCodeSetDropDown = null;
		//this.WebDBService.GetWebDbReviewSetsList();
	}
	RemoveTool(rs: iWebDbReviewSet) {
		if (this.WebDBService.CurrentDB != null)
			this.WebDBService.RemoveWebDbReviewSet(this.WebDBService.CurrentDB.webDBId, rs.webDBSetId);
    }



	BackToMain() {
		this.router.navigate(['Main']);
	}
	ngOnDestroy() {
		this.WebDBService.Clear();
	}
	ngAfterViewInit() {

	}
}
