import { Injectable, Inject } from "@angular/core";
import { ModalService } from "./modal.service";
import { HttpClient } from "@angular/common/http";
import { BusyAwareService } from "../helpers/BusyAwareService";

@Injectable({

    providedIn: 'root',

})
export class MAGAdminService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
    public DoCheckChangedPaperIds(magLatest: string ) {

        this._BusyMethods.push("DoCheckChangedPaperIds");
        let body = JSON.stringify({ Value: magLatest });
        this._httpC.post<boolean>(this._baseUrl + 'api/MagCurrentInfo/DoCheckChangedPaperIds', body)
            .subscribe(result => {
                this.RemoveBusy("DoCheckChangedPaperIds");
                if (result != null ) {
                    this.SwitchOnAutoRefreshLogList();
                }
            },
                error => {
                    this.RemoveBusy("DoCheckChangedPaperIds");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("DoCheckChangedPaperIds");
                });
    }
    SwitchOnAutoRefreshLogList() {
        throw new Error("Method not implemented.");
    }
   
}