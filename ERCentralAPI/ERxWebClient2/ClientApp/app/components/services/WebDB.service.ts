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
}
export class WebDbReviewSetJson {
    webDBId: number = 0;
    setId: number = 0;
    webDBSetId: number = 0;
    setName: string = "";
    setDescription: string = "";
}
