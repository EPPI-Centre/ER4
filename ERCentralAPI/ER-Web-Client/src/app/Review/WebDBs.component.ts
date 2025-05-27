import { Component, Inject, OnInit, OnDestroy, ViewChild, AfterViewInit, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { WebDBService, iWebDB, iWebDbReviewSet, WebDbReviewSet, MissingAttribute, iWebDBMap, iWebDBLog, FieldList } from '../services/WebDB.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { ReviewSetsService, SetAttribute, ReviewSet } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { FileRestrictions, UploadEvent, SelectEvent } from '@progress/kendo-angular-upload';

@Component({
	selector: 'WebDBsComp',
	templateUrl: './WebDBs.component.html',
	providers: []
})

export class WebDBsComponent implements OnInit, OnDestroy {
	constructor(private router: Router,
		private WebDBService: WebDBService,
		private ReviewerIdentityService: ReviewerIdentityService,
		private ReviewSetsService: ReviewSetsService,
		private ModalService: ModalService,
		private ConfirmationDialogService: ConfirmationDialogService
	)
	{ }

	//possible editor, see: https://github.com/chymz/ng2-ckeditor

	ngOnInit() {
		if (!this.ReviewerIdentityService.HasAdminRights) this.BackToMain();
		if (this.WebDBService.WebDBs.length == 0) this.WebDBService.Fetch();
    if (this.ReviewSetsService.ReviewSets.length == 0) this.ReviewSetsService.GetReviewSets(false); 
	}



	//@Output() onCloseClick = new EventEmitter();
	//public isExpanded: boolean = false;
	public EditingDB: iWebDB | null = null;
	public EditingWebDbReviewSet: WebDbReviewSet | null = null;
	public EditingSetAttribute: SetAttribute | null = null;
  public EditingFilter: boolean = false;
  public DropdownSelectedCodeName: string = "";
	public EditingMap: iWebDBMap | null = null;
	public ShowMapsMiniHelp: boolean = false;
	public ConfirmPassword: string = "";
	public ShowPassword: boolean = false;
	public ShowLogs: boolean = false;
	public isCollapsedFilterCode: boolean = false;
  public get uploadSaveUrl(): string {
    return this.WebDBService.BaseUrl + 'api/WebDB/UploadImage';
  }

  public FieldList: FieldList[] = [
    { "fieldNumber": 0, enabled: false, selected: true, "fieldName": "Title", "friendlyFieldName": "Title" },
    { "fieldNumber": 1, enabled: true, selected: true, "fieldName": "Abstract", "friendlyFieldName": "Abstract" },
    { "fieldNumber": 2, enabled: false, selected: true, "fieldName": "Authors", "friendlyFieldName": "Author(s)" },
    { "fieldNumber": 3, enabled: false, selected: true, "fieldName": "ParentTitle", "friendlyFieldName": "Journal / Book" },
    { "fieldNumber": 4, enabled: true, selected: true, "fieldName": "ItemStatus", "friendlyFieldName": "Item status" },
    { "fieldNumber": 5, enabled: false, selected: true, "fieldName": "ItemID", "friendlyFieldName": "Item ID" },
    { "fieldNumber": 6, enabled: true, selected: true, "fieldName": "OldItemID", "friendlyFieldName": "Imported ID" },
    { "fieldNumber": 7, enabled: true, selected: true, "fieldName": "ParentAuthors", "friendlyFieldName": "Book editor(s)" },
    { "fieldNumber": 8, enabled: false, selected: true, "fieldName": "Year", "friendlyFieldName": "Year" },
    { "fieldNumber": 9, enabled: true, selected: true, "fieldName": "StandardNumber", "friendlyFieldName": "ISSN / ISBN" },
    { "fieldNumber": 10, enabled: false, selected: true, "fieldName": "ShortTitle", "friendlyFieldName": "Short title" },
    { "fieldNumber": 11, enabled: true, selected: true, "fieldName": "Pages", "friendlyFieldName": "Pages" },
    { "fieldNumber": 12, enabled: true, selected: true, "fieldName": "Volume", "friendlyFieldName": "Volume" },
    { "fieldNumber": 13, enabled: true, selected: true, "fieldName": "Issue", "friendlyFieldName": "Issue" },
    { "fieldNumber": 14, enabled: true, selected: true, "fieldName": "URL", "friendlyFieldName": "Url" },
    { "fieldNumber": 15, enabled: true, selected: true, "fieldName": "DOI", "friendlyFieldName": "DOI" },
    { "fieldNumber": 16, enabled: true, selected: true, "fieldName": "Availability", "friendlyFieldName": "Availability" },
    { "fieldNumber": 17, enabled: true, selected: true, "fieldName": "Edition", "friendlyFieldName": "Edition" },
    { "fieldNumber": 18, enabled: true, selected: true, "fieldName": "Publisher", "friendlyFieldName": "Publisher" },
    { "fieldNumber": 19, enabled: true, selected: true, "fieldName": "Month", "friendlyFieldName": "Month" },
    { "fieldNumber": 20, enabled: true, selected: true, "fieldName": "City", "friendlyFieldName": "City" },
    { "fieldNumber": 21, enabled: true, selected: true, "fieldName": "Country", "friendlyFieldName": "Country" },
    { "fieldNumber": 22, enabled: true, selected: true, "fieldName": "Institution", "friendlyFieldName": "Institution" },
    { "fieldNumber": 23, enabled: true, selected: true, "fieldName": "Comments", "friendlyFieldName": "Comments" },
    { "fieldNumber": 24, enabled: true, selected: true, "fieldName": "Keywords", "friendlyFieldName": "Keywords" },
    { "fieldNumber": 25, enabled: true, selected: true, "fieldName": "DateCreated", "friendlyFieldName": "Created on" },
    { "fieldNumber": 26, enabled: true, selected: true, "fieldName": "DateEdited", "friendlyFieldName": "Edited on" },
    { "fieldNumber": 27, enabled: true, selected: true, "fieldName": "Source", "friendlyFieldName": "Source name" },
    { "fieldNumber": 28, enabled: true, selected: true, "fieldName": "Duplicates", "friendlyFieldName": "Duplicates" }
  ];

	public uploadRestrictions: FileRestrictions = {
		allowedExtensions: [
			'.jpg'
			, '.jpeg'
			, '.png'
		]
		, maxFileSize: 1024000
  };

  public pasteCleanupSettings = {
    convertMsLists: true,
    removeHtmlComments: true,
    stripTags: ['script', 'img'],
    // removeAttributes: ['lang'],
    removeMsClasses: true,
    removeMsStyles: true,
    removeInvalidHTML: true,
  };

	public ImageSizeError: string = "";
	public UploadImageNumber: number = 1;
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
	public get CurrentMaps(): iWebDBMap[] {
		return this.WebDBService.CurrentMaps;
  }
	public get CurrentLogs(): iWebDBLog[] {
		return this.WebDBService.CurrentLogs;
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
		return this.EditingDB != null || this.EditingSetAttribute != null || this.EditingWebDbReviewSet != null || this.EditingMap != null;
	}
	public get VisitURL(): string {
		if (this.CurrentDB) return this.WebDBService.URLfromWebDB(this.CurrentDB);
		else return "";
    }


  public get HasSelections(): number {
    const OptionalFields = this.FieldList.filter(f => f.enabled == true);
    const selectedCount = OptionalFields.filter(f => f.selected == true).length;
    if (selectedCount == 0) return 0; // nothing selected
    else if (selectedCount == OptionalFields.length) return 2; // all selected
    else return 1; // partial selection
  }

  public allFieldsSelectedUnselected(event: Event) {
    
    for (let i = 0; i < this.FieldList.length; i++) {
      let checked = (event.target as HTMLInputElement).checked;
      
      if (checked == true) {
        this.FieldList[i].selected = true;
      }
      else {
        if (this.FieldList[i].enabled == true) {
          this.FieldList[i].selected = false;
        }
      }
    }
    this.HiddenFieldsSetDataFromUI();
  }
  

  public FieldChangeSelected(fieldNumber: number, event: Event) {
    // update FieldList
    let i = fieldNumber;
    let checked = (event.target as HTMLInputElement).checked;
    if (checked == true) {
      this.FieldList[i].selected = true;
    }
    else {
      this.FieldList[i].selected = false;
    }

    // update hiddenFields
    // this is recreating the list each time. Not very efficient but..
    // the list needs to be in the correct order to work correctly in EPPI Vis.
    // I may need to fix that somehow.
    this.HiddenFieldsSetDataFromUI();
  }




	Edit(item: iWebDB | null = null) {
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
        headerImage3Url: '',
        hiddenFields: '',
        SplittedHiddenFields: undefined
			};
    }
		this.EditingDB = this.WebDBService.CloneWebDBforEdit(item);
    //this.isExpanded = true;

    this.HiddenFieldsSetUIfromData();
  }

  private HiddenFieldsSetUIfromData() {
    if (this.EditingDB != null) {
      let hiddenFields = this.EditingDB.SplittedHiddenFields;
      // set the values in FieldList
      for (let i = 0; i < this.FieldList.length; i++) {
        // if the field is disabled, or if no fields are hidden, check the box
        if (this.FieldList[i].enabled == false || hiddenFields === undefined || hiddenFields.length === 0) {
          this.FieldList[i].selected = true;
        }
        else { // see if it is in the hidden list
          if (hiddenFields.includes(this.FieldList[i].fieldName)) {
            this.FieldList[i].selected = false;
          } else {
            this.FieldList[i].selected = true;
          }
        }
      }
    }
  }
  private HiddenFieldsSetDataFromUI() {
    if (this.EditingDB != null) {
      this.EditingDB.SplittedHiddenFields = [];
      let hiddenFields = "";
      for (let i = 0; i < this.FieldList.length; i++) {
        if (this.FieldList[i].enabled == true) {
          if (this.FieldList[i].selected == false) {
            this.EditingDB.SplittedHiddenFields.push(this.FieldList[i].fieldName);
            hiddenFields += this.FieldList[i].fieldName;
            hiddenFields += ",";
          }
        }
      }
      this.EditingDB.hiddenFields = hiddenFields.substring(0, hiddenFields.length - 1);
    }
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
  ShowDBSettingById(event: Event) {
    let dbId = parseInt((event.target as HTMLOptionElement).value);
		//console.log("Changing DB (id): ", dbId);
		let db = this.WebDbs.find(f => f.webDBId == dbId);
		if (db) {
			this.WebDBService.CurrentDB = db;
			this.ReloadDBDependentData();
			this.ShowLogs = false;
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
	//GetLogs(CurrentDB: iWebDB) {
	//	this.WebDBService.GetWebDBLogs(CurrentDB.webDBId, "1980/01/01 00:00:00", "1980/01/01 00:00:00", "All");
    //}
	EditFilter() {
		this.EditingFilter = true;
		this.isCollapsedFilterCode = false;
	}
	CancelEditFilter() {
		this.EditingFilter = false;
    this.isCollapsedFilterCode = false;
    this.DropdownSelectedCodeName = "";
	}
	RemoveFilter() {
		if (this.EditingDB) this.EditingDB.attributeIdFilter = 0;
		this.CancelEditFilter();
    }
	CloseCodeDropDownFilterCode() {
		if (this.FilterCodeSelector) {
      if (this.EditingDB) {
        this.EditingDB.attributeIdFilter = (this.FilterCodeSelector.SelectedNodeData as SetAttribute).attribute_id;
        this.DropdownSelectedCodeName = (this.FilterCodeSelector.SelectedNodeData as SetAttribute).attribute_name;
      }
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
		this.ImageSizeError = "";
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
			else this.ImageSizeError = "File size is too big (Max: 1MB)";
		});
	}
	clearUploadImageEventHandler() {
		this.imagePreview = null;
		this.ImageSizeError = "";
    }

	uploadEventHandler(e: UploadEvent) {
		if (this.EditingDB && this.EditingDB.webDBId > 0 && this.ImageToUploadisGood) {
			e.data = {
				webDbId: this.EditingDB.webDBId,
				imageNumber: this.UploadImageNumber
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
		this.ImageSizeError = "";
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
  RemoveTool(rs: WebDbReviewSet | null) {
    if (!rs) return;
		if (this.WebDBService.CurrentDB != null)
			this.WebDBService.RemoveWebDbReviewSet(this.WebDBService.CurrentDB.webDBId, rs.webDBSetId);
    }
  EditTool(rs: WebDbReviewSet | null) {
    if (!rs) return;
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
  RemoveAttribute(att: SetAttribute | null) {
    if (!att) return;
		if (this.WebDBService.CurrentDB != null)
			this.WebDBService.RemoveWebDbAttribute(att, this.WebDBService.CurrentDB.webDBId);
	}
  EditAttribute(att: SetAttribute | null) {
    if (!att) return;
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

	EditMap(map: iWebDBMap) {
		this.EditingMap = {
			columnsAttributeID: map.columnsAttributeID,
			columnsPublicAttributeID: map.columnsPublicAttributeID,
			columnsPublicAttributeName: map.columnsPublicAttributeName,
			columnsPublicSetID: map.columnsPublicSetID,
			columnsPublicSetName: map.columnsPublicSetName,
			columnsSetID: map.columnsSetID,
			rowsAttributeID: map.rowsAttributeID,
			rowsPublicAttributeID: map.rowsPublicAttributeID,
			rowsPublicAttributeName: map.rowsPublicAttributeName,
			rowsPublicSetID: map.rowsPublicSetID,
			rowsPublicSetName: map.rowsPublicSetName,
			rowsSetID: map.rowsSetID,
			segmentsAttributeID: map.segmentsAttributeID,
			segmentsPublicAttributeID: map.segmentsPublicAttributeID,
			segmentsPublicAttributeName: map.segmentsPublicAttributeName,
			segmentsPublicSetID: map.segmentsPublicSetID,
			segmentsPublicSetName: map.segmentsPublicSetName,
			segmentsSetID: map.segmentsSetID,
			webDBId: map.webDBId,
			webDBMapDescription: map.webDBMapDescription,
			webDBMapId: map.webDBMapId,
			webDBMapName: map.webDBMapName
		}
	}
	CreateMap() {
		if (this.CurrentDB) {
			this.EditingMap = {
				columnsAttributeID: 0,
				columnsPublicAttributeID: 0,
				columnsPublicAttributeName: '',
				columnsPublicSetID: 0,
				columnsPublicSetName: '',
				columnsSetID: 0,
				rowsAttributeID: 0,
				rowsPublicAttributeID: 0,
				rowsPublicAttributeName: '',
				rowsPublicSetID: 0,
				rowsPublicSetName: '',
				rowsSetID: 0,
				segmentsAttributeID: 0,
				segmentsPublicAttributeID: 0,
				segmentsPublicAttributeName: '',
				segmentsPublicSetID: 0,
				segmentsPublicSetName: '',
				segmentsSetID: 0,
				webDBId: this.CurrentDB.webDBId,
				webDBMapDescription: '',
				webDBMapId: 0,
				webDBMapName: 'New Map (please edit!)'
			}
		}
	}
	public get CanAssignToMapRowAndCols(): boolean {
		if (this.WebDBService.SelectedNodeData == null) return false;
		else if (this.WebDBService.SelectedNodeData.attributes.length > 0) return true;
		return false;
	}
	public get CanAssignToMapSegments(): boolean {
		if (this.WebDBService.SelectedNodeData == null) return false;
		else if (this.WebDBService.SelectedNodeData.attributes.length > 0 && this.WebDBService.SelectedNodeData.attributes.length < 7) return true;
		return false;
	}
	SetColumnsNode() {
		if (this.WebDBService.SelectedNodeData == null || this.EditingMap == null) return;
		else {
			if (this.SelectedNodeIsRoot == true) {
				//adding the root of a coding tool
				this.EditingMap.columnsAttributeID = 0;
				this.EditingMap.columnsPublicAttributeName = "";
				this.EditingMap.columnsSetID = this.WebDBService.SelectedNodeData.set_id;
				this.EditingMap.columnsPublicSetName = this.WebDBService.SelectedNodeData.name;
				this.EditingMap.columnsPublicSetID = 0;
				this.EditingMap.columnsPublicAttributeID = 0;
			}
			else {
				let setid = this.WebDBService.SelectedNodeData.set_id;
				let ind = this.WebDBService.CurrentSets.findIndex(f => f.set_id == setid);
				this.EditingMap.columnsPublicSetID = 0;
				this.EditingMap.columnsPublicAttributeID = 0;
				if (ind == -1) {
					//oh, can't do much here, then...
					this.EditingMap.columnsAttributeID = 0;
					this.EditingMap.columnsPublicAttributeName = "";
					this.EditingMap.columnsSetID = 0;
					this.EditingMap.columnsPublicSetName = "";
				} else {
					this.EditingMap.columnsSetID = this.WebDBService.SelectedNodeData.set_id;
					this.EditingMap.columnsAttributeID = (this.WebDBService.SelectedNodeData as SetAttribute).attribute_id;
					this.EditingMap.columnsPublicAttributeName = (this.WebDBService.SelectedNodeData as SetAttribute).name;
					this.EditingMap.columnsPublicSetName = this.WebDBService.CurrentSets[ind].name;
                }
				
			}
        }
	}
	SetRowsNode() {
		if (this.WebDBService.SelectedNodeData == null || this.EditingMap == null) return;
		else {
			if (this.SelectedNodeIsRoot == true) {
				//adding the root of a coding tool
				this.EditingMap.rowsAttributeID = 0;
				this.EditingMap.rowsPublicAttributeName = "";
				this.EditingMap.rowsSetID = this.WebDBService.SelectedNodeData.set_id;
				this.EditingMap.rowsPublicSetName = this.WebDBService.SelectedNodeData.name;
				this.EditingMap.rowsPublicSetID = 0;
				this.EditingMap.rowsPublicAttributeID = 0;
			}
			else {
				let setid = this.WebDBService.SelectedNodeData.set_id;
				let ind = this.WebDBService.CurrentSets.findIndex(f => f.set_id == setid);
				this.EditingMap.rowsPublicSetID = 0;
				this.EditingMap.rowsPublicAttributeID = 0;
				if (ind == -1) {
					//oh, can't do much here, then...
					this.EditingMap.rowsAttributeID = 0;
					this.EditingMap.rowsPublicAttributeName = "";
					this.EditingMap.rowsSetID = 0;
					this.EditingMap.rowsPublicSetName = "";
				} else {
					this.EditingMap.rowsSetID = this.WebDBService.SelectedNodeData.set_id;
					this.EditingMap.rowsAttributeID = (this.WebDBService.SelectedNodeData as SetAttribute).attribute_id;
					this.EditingMap.rowsPublicAttributeName = (this.WebDBService.SelectedNodeData as SetAttribute).name;
					this.EditingMap.rowsPublicSetName = this.WebDBService.CurrentSets[ind].name;
				}

			}
		}
	}
	SetSegmentsNode() {
		if (this.WebDBService.SelectedNodeData == null || this.EditingMap == null) return;
		else {
			if (this.SelectedNodeIsRoot == true) {
				//adding the root of a coding tool
				this.EditingMap.segmentsAttributeID = 0;
				this.EditingMap.segmentsPublicAttributeName = "";
				this.EditingMap.segmentsSetID = this.WebDBService.SelectedNodeData.set_id;
				this.EditingMap.segmentsPublicSetName = this.WebDBService.SelectedNodeData.name;
				this.EditingMap.segmentsPublicSetID = 0;
				this.EditingMap.segmentsPublicAttributeID = 0;
			}
			else {
				let setid = this.WebDBService.SelectedNodeData.set_id;
				let ind = this.WebDBService.CurrentSets.findIndex(f => f.set_id == setid);
				this.EditingMap.segmentsPublicSetID = 0;
				this.EditingMap.segmentsPublicAttributeID = 0;
				if (ind == -1) {
					//oh, can't do much here, then...
					this.EditingMap.segmentsAttributeID = 0;
					this.EditingMap.segmentsPublicAttributeName = "";
					this.EditingMap.segmentsSetID = 0;
					this.EditingMap.segmentsPublicSetName = "";
				} else {
					this.EditingMap.segmentsSetID = this.WebDBService.SelectedNodeData.set_id;
					this.EditingMap.segmentsAttributeID = (this.WebDBService.SelectedNodeData as SetAttribute).attribute_id;
					this.EditingMap.segmentsPublicAttributeName = (this.WebDBService.SelectedNodeData as SetAttribute).name;
					this.EditingMap.segmentsPublicSetName = this.WebDBService.CurrentSets[ind].name;
				}

			}
		}
	}
	public get CanSaveMap(): string {
		if (this.EditingMap == null) return "no map";
		else {
			let em = this.EditingMap;
			let ind = this.WebDBService.CurrentMaps.findIndex( //comparing all significant fields...
				f => f.webDBMapId == em.webDBMapId
					&& f.columnsAttributeID == em.columnsAttributeID
					&& f.columnsSetID == em.columnsSetID
					&& f.rowsAttributeID == em.rowsAttributeID
					&& f.rowsSetID == em.rowsSetID
					&& f.segmentsAttributeID == em.segmentsAttributeID
					&& f.segmentsSetID == em.segmentsSetID
					&& f.webDBMapName == em.webDBMapName
					&& f.webDBMapDescription == em.webDBMapDescription
			);
			if (ind == -1) {
				const m = this.EditingMap;
				if (m.webDBMapName == "" || m.webDBMapName == "New Map (please edit!)") return "Map name is required";
				else if (m.columnsSetID < 1) return "Please set the columns dimension";
				else if (m.rowsSetID < 1) return "Please set the rows dimension";
				else if (m.segmentsSetID < 1) return "Please set the segments (within each cell) dimension";
				else return "";
			}
			else return "no change";
        }
	}
	async SaveMap(map: iWebDBMap | null) {
		if (map) await this.WebDBService.SaveMap(map).then(res => {
			this.EditingMap = null;//not sure if it's best to close the editing panel or keep it open!!
		})
	}
	DeleteMap(map: iWebDBMap) {
		if (!this.CanWrite) return;
		this.ConfirmationDialogService.confirm("Delete Map?"
			, "Are you sure you want to delete this Map (\"<em>" + map.webDBMapName + "</em>\")?"
			, false, "", "Yes, Delete", "Cancel").then((confirm: any) => {
				if (confirm) {
					this.WebDBService.DeleteMap(map);
				}
			});
	}

	BackToMain() {
		this.router.navigate(['Main']);
	}
	ngOnDestroy() {
		this.WebDBService.Clear();
	}

  



}
