import { Inject, Injectable, EventEmitter, Output} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { ReviewSet, SetAttribute, iAttributesList, iReviewSet, ReviewSetsService, singleNode } from './ReviewSets.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ThemeService } from '@progress/kendo-angular-charts/dist/es2015/common/theme.service';


@Injectable({

    providedIn: 'root',

})

export class WebDBService extends BusyAwareService  {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) { super(); }

    @Output() PleaseRedrawTheTree = new EventEmitter();
    private _WebDBs: iWebDB[] = [];
    public get WebDBs(): iWebDB[] {
        return this._WebDBs;
    }
    private _CurrentDB: iWebDB | null = null;
    public get CurrentDB(): iWebDB | null {
        return this._CurrentDB;
    }
    public set CurrentDB(db: iWebDB | null) {
        const ind = this._WebDBs.findIndex((f) => db != null && f.webDBId == db.webDBId);
        if (ind == -1) {
            this._CurrentDB = null;
            this._CurrentSets = [];
        }
        else {
            this._CurrentDB = this._WebDBs[ind];
            this.GetWebDbReviewSetsList();
        }
        //console.log("Setting current DB:", db, this._CurrentDB);
    }
    private _CurrentSets: WebDbReviewSet[] = [];
    public get CurrentSets(): WebDbReviewSet[] {
        if (this._CurrentSets.length > 0 && this._CurrentDB != null && this._CurrentSets[0].webDBId == this._CurrentDB.webDBId) {
            return this._CurrentSets;
        }
        else {
            this._CurrentSets = [];
            return this._CurrentSets;
        }
    }
    public SelectedNodeData: singleNode | null = null;

    public Fetch(): void {
        this._BusyMethods.push("Fetch");
        this._httpC.get<iWebDB[]>(this._baseUrl + 'api/WebDB/GetWebDBs').subscribe(
            res => {
                this._WebDBs = res;
                if (res.length > 0) {
                    if (this._CurrentDB == null) this.CurrentDB = res[0];
                    else {
                        const ind = this._WebDBs.findIndex((f) => this._CurrentDB != null  && f.webDBId == this._CurrentDB.webDBId);
                        if (ind == -1) this.CurrentDB = res[0];
                        else this._CurrentDB = res[ind];
                    }
                }
                this.RemoveBusy("Fetch");
            }, error => {
                this.RemoveBusy("Fetch");
                this.modalService.GenericError(error);
            }
        );
    }

    public Delete(toDel: iWebDB): void {
        if (this._WebDBs.indexOf(toDel) == -1) {
            this.Fetch();//let's refresh this list: why did user ask to delete something we don't have??
            return;//do nothing else
        }
        this._BusyMethods.push("Delete");
        let body = JSON.stringify({ Value: toDel.webDBId });
        this._httpC.post<iWebDB[]>(this._baseUrl + 'api/WebDB/DeleteWebDB', body).subscribe(
            res => {
                this._WebDBs = res;
                if (this._CurrentDB !== null) {
                    let ind = this._WebDBs.findIndex(f => toDel.webDBId == f.webDBId)
                    if (ind == -1) {
                        if (this._WebDBs.length > 0) this._CurrentDB = this._WebDBs[0];
                        else this._CurrentDB = null;
                    }
                }
                else if (this._WebDBs.length > 0)  {
                    this._CurrentDB = this._WebDBs[0];
                }
                this.RemoveBusy("Delete");
            }, error => {
                this.RemoveBusy("Delete");
                this.modalService.GenericError(error);
            }
        );
    }
    public CreateOrEdit(updating: iWebDB): void {
        if (updating.webDBId != 0 && this._WebDBs.findIndex(f => f.webDBId == updating.webDBId) == -1) {
            //check: if it's not new (Id != 0) but doesn't exist in current list (based on the id), refetch list and do nothing more
            this.Fetch();
            return;
        }
        this._BusyMethods.push("CreateOrEdit");
        this._httpC.post<iWebDB[]>(this._baseUrl + 'api/WebDB/CreateOrEditWebDB', updating).subscribe(
            res => {
                this._WebDBs = res;
                if (this._CurrentDB == null && this._WebDBs.length > 0) this._CurrentDB = this._WebDBs[0];
                this.RemoveBusy("CreateOrEdit");
            }, error => {
                this.RemoveBusy("CreateOrEdit");
                this.modalService.GenericError(error);
            }
        );
    }

    public CloneWebDBforEdit(toClone: iWebDB): iWebDB {
        let res = {
            webDBId: toClone.webDBId,
            webDBName: toClone.webDBName,
            webDBDescription: toClone.webDBDescription,
            attributeIdFilter: toClone.attributeIdFilter,
            isOpen: toClone.isOpen,
            userName: toClone.userName,
            password: toClone.password,
            createdBy: toClone.createdBy,
            editedBy: toClone.editedBy
        }
        return res;
    }

    public GetWebDbReviewSetsList() {
        if (this._CurrentDB == null) return;
        else {
            this._BusyMethods.push("GetWebDbReviewSetsList");
            let body = JSON.stringify({ Value: this._CurrentDB.webDBId });
            this._httpC.post<iWebDbReviewSet[]>(this._baseUrl + 'api/WebDB/GetWebDbReviewSetsList', body).subscribe(
                res => {
                    this._CurrentSets = [];
                    for (let iwSet of res) {
                        this._CurrentSets.push(new WebDbReviewSet(iwSet));
                    }
                    this.PleaseRedrawTheTree.emit();
                    this.RemoveBusy("GetWebDbReviewSetsList");
                }, error => {
                    this.RemoveBusy("GetWebDbReviewSetsList");
                    this.modalService.GenericError(error);
                }
            );
        }
    }
    public AddWebDbReviewSet(webDBId: number, setId: number) {
        if (this._CurrentDB == null
            || this._CurrentDB.webDBId !== webDBId) return;
        let body: WebDbReviewSetJson = new WebDbReviewSetJson();
        body.webDBId = webDBId;
        body.setId = setId;
        this._BusyMethods.push("AddWebDbReviewSet");
        this._httpC.post<iWebDbReviewSet>(this._baseUrl + 'api/WebDB/AddWebDbReviewSet', body).subscribe(
            res => {
                this._CurrentSets.push(new WebDbReviewSet(res));
                this._CurrentSets.sort((a, b) => { return a.set_order - b.set_order });
                this.PleaseRedrawTheTree.emit();
                this.RemoveBusy("AddWebDbReviewSet");
            }, error => {
                this.RemoveBusy("AddWebDbReviewSet");
                this.modalService.GenericError(error);
            }
        );
    }
    public RemoveWebDbReviewSet(webDBId: number, webDBsetId: number) {
        if (this._CurrentDB == null || this._CurrentDB.webDBId !== webDBId) return;
        let body: WebDbReviewSetJson = new WebDbReviewSetJson();
        body.webDBId = webDBId;
        body.webDBSetId = webDBsetId;
        this._BusyMethods.push("RemoveWebDbReviewSet");
        this._httpC.post<iWebDbReviewSet>(this._baseUrl + 'api/WebDB/RemoveWebDbReviewSet', body).subscribe(
            res => {
                let ind = this._CurrentSets.findIndex(f => f.webDBSetId == webDBsetId);
                if (ind != -1) {
                    this._CurrentSets.splice(ind, 1);
                }
                if (this.SelectedNodeData != null
                    && this.SelectedNodeData.nodeType == "ReviewSet"
                    && (this.SelectedNodeData as WebDbReviewSet).webDBSetId == webDBsetId
                ) this.SelectedNodeData = null;
                this.PleaseRedrawTheTree.emit();
                this.RemoveBusy("RemoveWebDbReviewSet");
            }, error => {
                this.RemoveBusy("RemoveWebDbReviewSet");
                this.modalService.GenericError(error);
            }
        );
    }
    public UpdateWebDbReviewSet(data: WebDbReviewSet) {
        if (this._CurrentDB == null || this._CurrentDB.webDBId !== data.webDBId) return;
        let body: WebDbReviewSetJson = new WebDbReviewSetJson();
        body.webDBId = data.webDBId;
        body.webDBSetId = data.webDBSetId;
        body.setDescription = data.description;
        body.setName = data.set_name;
        this._BusyMethods.push("UpdateWebDbReviewSet");
        this._httpC.post<iWebDbReviewSet>(this._baseUrl + 'api/WebDB/UpdateWebDbReviewSet', body).subscribe(
            res => {
                let ind = this._CurrentSets.findIndex(f => f.webDBSetId == data.webDBSetId);
                if (ind != -1) {
                    let rSet: WebDbReviewSet = new WebDbReviewSet(res);
                    this._CurrentSets.splice(ind, 1, rSet);
                }
                if (this.SelectedNodeData != null
                    && this.SelectedNodeData.nodeType == "ReviewSet"
                    && (this.SelectedNodeData as WebDbReviewSet).webDBSetId == data.webDBSetId
                ) this.SelectedNodeData = null;
                this.PleaseRedrawTheTree.emit();
                this.RemoveBusy("UpdateWebDbReviewSet");
            }, error => {
                this.RemoveBusy("UpdateWebDbReviewSet");
                this.modalService.GenericError(error);
            }
        );
    }
    private AttributeIsPresentInThisWebDbSet(SetA: SetAttribute): WebDbReviewSet | null {
        let chk: SetAttribute | null = null;
        let candidate: WebDbReviewSet | null = null;
        for (candidate of this.CurrentSets) {
            if (candidate.set_id == SetA.set_id) {
                chk = candidate.FindAttributeById(SetA.attribute_id);
                break;
            }
        }
        //console.log("AttributeIsPresentInThisWebDbSet", candidate, chk);
        if (chk !== null) return candidate;
        else return null;
    }
    public EditWebDbAttribute(SetA: SetAttribute, webDbId: number) {
        //console.log("EditWebDbAttribute", SetA, webDbId);
        let webDBSet = this.AttributeIsPresentInThisWebDbSet(SetA);
        //console.log("EditWebDbAttribute2", webDBSet);
        if (webDBSet == null) return;//we don't edit what isn't here.

        //console.log("EditWebDbAttribute3", webDBSet);
        let cmd = new WebDbAttributeSetEditAddRemoveCommandJson();
        cmd.attributeId = SetA.attribute_id;
        cmd.setId = SetA.set_id;
        cmd.webDbId = webDbId;
        cmd.webDBSetId = webDBSet.webDBSetId;
        cmd.publicDescription = SetA.attribute_set_desc;
        cmd.publicName = SetA.attribute_name;
        this.EditAddRemoveWebDbAttribute(cmd);
    }
    public AddWebDbAttribute(SetA: SetAttribute, webDbId: number) {
        if (this.AttributeIsPresentInThisWebDbSet(SetA) != null) return;//we don't add what's here already
        let webDBSetId: number = -1;
        for (let set of this.CurrentSets) {
            if (SetA.set_id == set.set_id) {
                webDBSetId = set.webDBSetId;
                break;
            }
        }
        if (webDBSetId == -1) return;//can't do this if we couldn't find the destination WebDbSet
        let cmd = new WebDbAttributeSetEditAddRemoveCommandJson();
        cmd.attributeId = SetA.attribute_id;
        cmd.setId = SetA.set_id;
        cmd.webDbId = webDbId;
        cmd.webDBSetId = webDBSetId;
        this.EditAddRemoveWebDbAttribute(cmd);
    }
    public RemoveWebDbAttribute(SetA: SetAttribute, webDbId: number) {
        let webDBSet = this.AttributeIsPresentInThisWebDbSet(SetA);
        if (webDBSet == null) return;//we don't remove what isn't here.
        let cmd = new WebDbAttributeSetEditAddRemoveCommandJson();
        cmd.attributeId = SetA.attribute_id;
        cmd.setId = SetA.set_id;
        cmd.webDbId = webDbId;
        cmd.webDBSetId = webDBSet.webDBSetId;
        cmd.deleting = true;
        this.EditAddRemoveWebDbAttribute(cmd);
    }
    private EditAddRemoveWebDbAttribute(body: WebDbAttributeSetEditAddRemoveCommandJson) {
        this._BusyMethods.push("EditAddRemoveWebDbAttribute");
        this._httpC.post<iWebDbReviewSet>(this._baseUrl + 'api/WebDB/EditAddRemoveWebDbAttribute', body).subscribe(
            res => {
                let ind = this._CurrentSets.findIndex(f => f.webDBSetId == body.webDBSetId);
                if (ind != -1) {
                    let rSet: WebDbReviewSet = new WebDbReviewSet(res);
                    this._CurrentSets.splice(ind, 1, rSet);
                }
                this.SelectedNodeData == null;//cheap: we just unselect the current node, we're not sure it still exists...
                this.PleaseRedrawTheTree.emit();
                this.RemoveBusy("EditAddRemoveWebDbAttribute");
            }, error => {
                this.RemoveBusy("EditAddRemoveWebDbAttribute");
                this.modalService.GenericError(error);
            }
        );
    }

    public Clear() {
        this._WebDBs = [];
        this._CurrentDB = null;
        this._CurrentSets = [];
    }
}


export interface iWebDB {
    webDBId: number;
    webDBName: string;
    webDBDescription: string;
    attributeIdFilter: number;
    isOpen: boolean;
    userName: string;
    password: string;
    createdBy: string;
    editedBy: string;
}
export interface iWebDbReviewSet extends iReviewSet {
    webDBId: number;
    webDBSetId: number;
}
export class WebDbReviewSet extends ReviewSet {
    constructor(data: iWebDbReviewSet) {
        super();
        this.set_id = data.setId;
        this.reviewSetId = data.reviewSetId;
        this.set_name = data.setName;
        this.order = data.setOrder;
        this.codingIsFinal = data.codingIsFinal;
        this.allowEditingCodeset = data.allowCodingEdits;
        this.description = data.setDescription;
        this.setType = data.setType;
        this.userCanEditURLs = data.userCanEditURLs;
        this.webDBId = data.webDBId;
        this.webDBSetId = data.webDBSetId;
        this.attributes = ReviewSetsService.childrenFromJSONarray(data.attributes.attributesList);
    }
    public webDBId: number;
    public webDBSetId: number;
    public FindAttributeById(Id: number): SetAttribute | null {
        let result: SetAttribute | null = null;
        result = this.InternalFindAttributeById(this.attributes, Id);
        return result;
    }
    private InternalFindAttributeById(list: SetAttribute[], Id: number): SetAttribute | null {
        let result: SetAttribute | null = null;
        for (let candidate of list) {
            if (result) break;
            if (Id == candidate.attribute_id) {
                result = candidate;
                break;
            }
            else if (candidate.attributes && candidate.attributes.length > 0) {
                result = this.InternalFindAttributeById(candidate.attributes, Id);
            }
        }
        return result;
    }
}
export class WebDbReviewSetJson {
    webDBId: number = 0;
    setId: number = 0;
    webDBSetId: number = 0;
    setName: string = "";
    setDescription: string = "";
}
export class WebDbAttributeSetEditAddRemoveCommandJson {
    public attributeId: number = 0;
    public setId: number = 0;
    public webDbId: number = 0;
    public webDBSetId: number = 0;
    public deleting: boolean = false;
    public publicName: string = "";
    public publicDescription: string = "";
}
