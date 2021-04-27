import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter, AfterViewInit, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { WebDBService, iWebDB, iWebDbReviewSet, WebDbReviewSet, MissingAttribute } from '../services/WebDB.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { ReviewSetsService, SetAttribute, ReviewSet } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { forEach } from '@angular/router/src/utils/collection';
import { FileRestrictions, UploadEvent, SelectEvent } from '@progress/kendo-angular-upload';
import { Observable } from 'rxjs';
import { Image } from '@progress/kendo-drawing';

@Component({
	selector: 'WebDBsComp',
	templateUrl: './WebDBs.component.html',
	providers: []
})

export class WebDBsComponent implements OnInit, OnDestroy, AfterViewInit {
	constructor(private router: Router,
		@Inject('BASE_URL') private _baseUrl: string,
		private WebDBService: WebDBService,
		private ReviewerIdentityService: ReviewerIdentityService,
		private ReviewSetsService: ReviewSetsService,
		private ModalService: ModalService,
		private ConfirmationDialogService: ConfirmationDialogService
	)
	{ }

	//possible editor, see: https://github.com/chymz/ng2-ckeditor

	ngOnInit() {
		if (this.WebDBService.WebDBs.length == 0) this.WebDBService.Fetch();
		if (this.ReviewSetsService.ReviewSets.length == 0) this.ReviewSetsService.GetReviewSets();
	}

	async ngAfterViewInit() {
		//await this.loadScript("https://cdn.ckeditor.com/4.15.1/standard/ckeditor.js");
	}

	private loadScript(scriptUrl: string) {
		return new Promise((resolve, reject) => {
			const scriptElement = document.createElement('script')
			scriptElement.src = scriptUrl
			scriptElement.onload = resolve
			document.body.appendChild(scriptElement)
		})
	}

	//@Output() onCloseClick = new EventEmitter();
	//public isExpanded: boolean = false;
	public EditingDB: iWebDB | null = null;
	public EditingWebDbReviewSet: WebDbReviewSet | null = null;
	public EditingSetAttribute: SetAttribute | null = null;
	public EditingFilter: boolean = false;
	public ConfirmPassword: string = "";
	public isCollapsedFilterCode: boolean = false;
	public uploadSaveUrl = this._baseUrl + 'api/WebDB/UploadImage'; 
	public uploadRestrictions: FileRestrictions = {
		allowedExtensions: [
			'.jpg'
			, '.jpeg'
			, '.png'
		]
		, maxFileSize: 1024000
	};
	public isUploadImage1: boolean = true;
	public ShowUpload: boolean = false;
	public get MissingAttributes(): MissingAttribute[] {
		return this.WebDBService.MissingAttributes;
	}
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
	public get SelectedNodeIsRoot(): boolean | null {
		if (this.WebDBService.SelectedNodeData == null) return null;
		else if (this.WebDBService.SelectedNodeData.nodeType == "ReviewSet") return true;
		else return false;
	}
	private _lastSetID: number = 0;
	public get SelectedCodingTool(): WebDbReviewSet | null {
		if (this.WebDBService.SelectedNodeData == null || this.WebDBService.SelectedNodeData.nodeType !== "ReviewSet") {
			
			return null;
		}
		else {
			let tmp = this.WebDBService.SelectedNodeData as WebDbReviewSet;
			if (this._lastSetID != tmp.set_id) {
				this._lastSetID = tmp.set_id;
				this.FindMissingAttributes();
			}
			return tmp;
		}
	}
	
	private FindMissingAttributes() {
		if (this.WebDBService.SelectedNodeData && this.WebDBService.SelectedNodeData.nodeType == "ReviewSet") this.WebDBService.FindMissingAttributes();
	}
	public RestoreCode(item: MissingAttribute) {
		let needed = this.RestorePreview(item);
		if (needed == item) {
			this.ConfirmationDialogService.confirm("Make this code public?"
				, "This would make the code '<strong>" + item.name + "</strong>' visible in this web database, including all its child codes (if any).", false, ''
			).then((res: any) => { if (res == true) this.DoRestoreCode(needed);})
		} else {
			this.ConfirmationDialogService.confirm("Make this code public?"
				, "To make the code '<strong>" + item.name + "</strong>' visible in this web database"
				+ ", we need to restore his missing parent:<br />"
				+ "Parent name: <strong>" + needed.name + "</strong><br />"
				+ (needed.path.length == 0 ? "" : "Parent path: <strong>" + needed.path + "</strong>.<br />")
				+ "All the children of this code will be reinstated as well.<br />"
				, false, ''
			).then((res: any) => { if (res == true) this.DoRestoreCode(needed); })
        }
    }
	public RestorePreview(item: MissingAttribute): MissingAttribute {
		let MissingPartent: MissingAttribute | undefined = item;
		//console.log("RestorePreview: ", item);
		let res: MissingAttribute = item;
		let i = 0;
		while (MissingPartent != undefined && i < 300) {
			i++;
			res = MissingPartent;
			//console.log("RestorePreview t: " + res.path + "::" + res.name);
			MissingPartent = this.MissingAttributes.find(f => res.parentId == f.attributeId);
		}
		//console.log("RestorePreview FFF: " + res.path + "::" + res.name);
		return res;
	}
	DoRestoreCode(item: MissingAttribute) {
		if (this.WebDBService.CurrentDB != null) {
			let todo = this.ReviewSetsService.FindAttributeById(item.attributeId);
			if (todo != null) this.WebDBService.AddWebDbAttribute(todo, this.WebDBService.CurrentDB.webDBId);
		}
		this.EditingWebDbReviewSet = null;
		
    }
	
	public get SelectedAttribute(): SetAttribute | null {
		if (this.WebDBService.SelectedNodeData == null || this.WebDBService.SelectedNodeData.nodeType !== "SetAttribute") return null;
		else return this.WebDBService.SelectedNodeData as SetAttribute;
	}
    public get CanWrite(): boolean {
        //console.log("create rev check:", this._reviewerIdentityServ.reviewerIdentity);
		return this.ReviewerIdentityService.HasWriteRights && this.ReviewerIdentityService.HasAdminRights;
	}
	public get AddableCodingTools(): ReviewSet[] {
		//we allow to "add" only coding tools that aren't in the WebDB already...
		//console.log("AddableCodingTools", this.ReviewSetsService.ReviewSets.length, this.WebDBService.CurrentSets.length);
		return this.ReviewSetsService.ReviewSets.filter(
			f => {
				//console.log("f:", f.set_id);
				return this.WebDBService.CurrentSets.findIndex(ff => {
					//console.log("ff:", ff.set_id);
					return ff.set_id == f.set_id;
				}) == -1 || this.WebDBService.CurrentSets.length == 0;
			});
	}
	public get CurrentWebDbTools(): WebDbReviewSet[] {
		return this.WebDBService.CurrentSets;
	}
	public get EditingSomething(): boolean {
		return this.EditingDB != null || this.EditingSetAttribute != null || this.EditingWebDbReviewSet != null;
	}
	public get VisitURL(): string {
		if (this.CurrentDB) return this.WebDBService.URLfromWebDB(this.CurrentDB);
		else return "";
    }

	Edit(item: iWebDB | null) {
		if (!item) {
			item = {
				webDBId: 0,
				webDBName: '',
				subtitle: '',
				webDBDescription: '',
				attributeIdFilter: 0,
				isOpen: true,
				userName: '',
				password: '',
				createdBy: '',
				editedBy: '',
				encodedImage1: '',
				encodedImage2: '',
				encodedImage3: '',
				headerImage1Url: '',
				headerImage2Url: '',
				headerImage3Url: ''
			};
        }
		this.EditingDB = this.WebDBService.CloneWebDBforEdit(item);
        //this.isExpanded = true;
    }
	CancelEdit() {
		this.EditingDB = null;
		this.EditingFilter = false;
		this.isCollapsedFilterCode = false;
		this.imagePreview = null;
		this.ShowUpload = false;
        //this.isExpanded = false;
	}
	ShowDBSetting(db: any) {
		console.log("Changing DB: ", db);
		this.WebDBService.CurrentDB = db;
		this.ReloadDBDependentData();
	}
	ShowDBSettingById(dbId: number) {
		console.log("Changing DB (id): ", dbId);
		let db = this.WebDbs.find(f => f.webDBId == dbId);
		if (db) {
			this.WebDBService.CurrentDB = db;
			this.ReloadDBDependentData();
		}
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

	
	public imagePreview: any | null = null;
	@ViewChild('ImagePreview') ImagePreviewEl!: ElementRef;
	public readonly MaxImgW: number = 500;
	public readonly MaxImgH: number = 150;
	public ImageToUploadisGood: boolean = false;
	public get ImagePreviewH(): number {
		if (this.ImagePreviewEl) {
			return this.ImagePreviewEl.nativeElement.offsetHeight;
		}
		return 0;
	}

	public get ImagePreviewW(): number {
		if (this.ImagePreviewEl) {
			return this.ImagePreviewEl.nativeElement.offsetWidth;
		}
		return 0;
	}

	public selectEventHandler(e: SelectEvent): void {
		const that = this;

		e.files.forEach((file) => {
			console.log(`File selected: ${file.name}`);

			if (!file.validationErrors) {
				const reader = new FileReader();

				reader.onload = function (ev) {
					if (ev.target) {
						const image = {
							src: ev.target['result'],
							uid: file.uid
						};

						that.imagePreview = image;
						setTimeout(() => {
							const w = that.ImagePreviewW;
							const h = that.ImagePreviewH;
							that.ImageToUploadisGood = true;
							if (w == 0 || w > that.MaxImgW) that.ImageToUploadisGood = false;
							else if (h == 0 || h > that.MaxImgH) that.ImageToUploadisGood = false;
						}, 100);
					}
				};
				if (file && file.rawFile) reader.readAsDataURL(file.rawFile);
			}
		});
	}
	clearUploadImageEventHandler() {
		this.imagePreview = null;
    }

	uploadEventHandler(e: UploadEvent) {
		if (this.EditingDB && this.EditingDB.webDBId > 0 && this.ImageToUploadisGood) {
			e.data = {
				webDbId: this.EditingDB.webDBId,
				imageNumber: this.isUploadImage1 ? 1 : 2
			};
			//console.log("uploading", e.data);
		} else {
			this.imagePreview = null;
			e.preventDefault();
		}
	}

	public completeEventHandler() {
		this.ShowUpload = false;
		this.WebDBService.Fetch();
		this.imagePreview = null;
		//this.ItemDocsService.Refresh();
		//this.log(`All files processed`);
	}

	DeleteImage(imageN: number) {
		if (this.CurrentDB) this.WebDBService.DeleteHeaderImage(imageN);
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
	RemoveTool(rs: WebDbReviewSet) {
		if (this.WebDBService.CurrentDB != null)
			this.WebDBService.RemoveWebDbReviewSet(this.WebDBService.CurrentDB.webDBId, rs.webDBSetId);
    }
	EditTool(rs: WebDbReviewSet) {
		let iToE: iWebDbReviewSet = {
			webDBId: rs.webDBId,
			webDBSetId: rs.webDBSetId,
			reviewSetId: rs.reviewSetId,
			setId: rs.set_id,
			setType: rs.setType,
			setName: rs.set_name,
			setDescription: rs.set_name,
			setOrder: rs.set_order,
			codingIsFinal: rs.codingIsFinal,
			allowCodingEdits: rs.allowEditingCodeset,
			userCanEditURLs: rs.userCanEditURLs,
			attributes: {
				attributesList: []
			}
        }
		let toedit = new WebDbReviewSet(iToE);
		toedit.webDBId = rs.webDBId;
		toedit.webDBSetId= rs.webDBSetId;
		toedit.description = rs.description;
		toedit.set_name = rs.set_name;
		this.EditingWebDbReviewSet = toedit;
	}
	
	CancelEditTool() {
		this.EditingWebDbReviewSet = null;
	}

	SaveEditedTool() {
		if (this.EditingWebDbReviewSet != null) {
			this.WebDBService.UpdateWebDbReviewSet(this.EditingWebDbReviewSet);
			this.EditingWebDbReviewSet = null;
		}
	}
	RemoveAttribute(att: SetAttribute) {
		if (this.WebDBService.CurrentDB != null)
			this.WebDBService.RemoveWebDbAttribute(att, this.WebDBService.CurrentDB.webDBId);
	}
	EditAttribute(att: SetAttribute) {
		//console.log("EditAttribute", att)
		let ToEdit: SetAttribute = new SetAttribute();
		ToEdit.attribute_id = att.attribute_id;
		ToEdit.set_id = att.set_id;
		ToEdit.attribute_name = att.attribute_name;
		ToEdit.attribute_set_desc = att.attribute_set_desc;
		this.EditingSetAttribute = ToEdit;
	}
	CancelEditAttribute() {
		this.EditingSetAttribute = null;
    }

	SaveEditedAttribute() {
		//console.log("SaveEditedAttribute", this.EditingSetAttribute);
		if (this.WebDBService.CurrentDB != null && this.EditingSetAttribute != null) {
			this.WebDBService.EditWebDbAttribute(this.EditingSetAttribute, this.WebDBService.CurrentDB.webDBId);
			this.EditingSetAttribute = null;
		}
    }

	BackToMain() {
		this.router.navigate(['Main']);
	}
	ngOnDestroy() {
		this.WebDBService.Clear();
	}
	
}