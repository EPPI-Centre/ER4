import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { MAGBrowserService } from './MAGBrowser.service';
import { MagRelatedPapersRun, MagPaperList, MagPaper,  MagItemPaperInsertCommand } from './MAGClasses.service';
import { ConfirmationDialogService } from './confirmation-dialog.service';

@Injectable({
    providedIn: 'root',
})

export class BasicMAGService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        private _magBrowserService: MAGBrowserService,
        private notificationService: ConfirmationDialogService,
        //private _eventEmitterService: EventEmitterService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
    public MagPaperList: MagPaperList = new MagPaperList();
    public MagRelatedRunPapers: MagPaper[] = [];
    private _MagRelatedPapersRunList: MagRelatedPapersRun[] = [];
    private _MagItemPaperInsert: MagItemPaperInsertCommand = new MagItemPaperInsertCommand();

    public get MagRelatedPapersRunList(): MagRelatedPapersRun[] {

        return this._MagRelatedPapersRunList;

    }
    public set MagRelatedPapersRunList(magRun: MagRelatedPapersRun[]) {
        this._MagRelatedPapersRunList = magRun;

    }

    FetchMagRelatedPapersRunList() {
        
        this._BusyMethods.push("FetchMagRelatedPapersRunList");
        this._httpC.get<MagRelatedPapersRun[]>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRuns')
            .subscribe(result => {
                this.RemoveBusy("FetchMagRelatedPapersRunList");
                this.MagRelatedPapersRunList = result;
            },
                error => {
                    this.RemoveBusy("FetchMagRelatedPapersRunList");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("FetchMagRelatedPapersRunList");
                });
    }
    DeleteMAGRelatedRun(Id: number) {

        this._BusyMethods.push("DeleteMAGRelatedRun");
        let body = JSON.stringify({ Value: Id });
        this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/DeleteMagRelatedPapersRun',
            body)
            .subscribe(result => {

                this.RemoveBusy("DeleteMAGRelatedRun");
                if (result.magRelatedRunId > 0) {

                    this.notificationService.showMAGRunMessage('MAG search was deleted');

                } else {

                    this.notificationService.showMAGRunMessage(result.status);
                }
                let tmpIndex: number = this.MagRelatedPapersRunList.findIndex(x => x.magRelatedRunId == Number(result.magRelatedRunId));
                if (tmpIndex > -1) {
                    this.MagRelatedPapersRunList.splice(tmpIndex, 1);
                }

            }, error => {
                this.RemoveBusy("DeleteMAGRelatedRun");
                this.modalService.GenericError(error);
            },
                () => {
                    this.RemoveBusy("DeleteMAGRelatedRun");
                });
    }
    CreateMAGRelatedRun(magRun: MagRelatedPapersRun) {
        this._BusyMethods.push("MagRelatedPapersRunCreate");
        this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/CreateMagRelatedPapersRun',
            magRun)
            .subscribe(result => {

                this.RemoveBusy("MagRelatedPapersRunCreate");
                if (result.magRelatedRunId > 0) {

                    this.notificationService.showMAGRunMessage('MAG search was created');

                } else {

                    this.notificationService.showMAGRunMessage(result.status);
                }
                this.MagRelatedPapersRunList.push(result);
                this._magBrowserService.FetchMAGRelatedPaperRunsListId(result.magRelatedRunId);

            }, error => {
                this.RemoveBusy("MagRelatedPapersRunCreate");
                this.modalService.GenericError(error);
            },
                () => {
                    this.RemoveBusy("MagRelatedPapersRunCreate");
                });
    }
    ImportMagRelatedRunPapers(magRelatedRun: MagRelatedPapersRun) {

        let notificationMsg: string = '';
        this._BusyMethods.push("ImportMagRelatedRunPapers");
        this._httpC.post<MagItemPaperInsertCommand>(this._baseUrl + 'api/MagRelatedPapersRunList/ImportMagRelatedPapers',
            magRelatedRun)
            .subscribe(result => {

                this.RemoveBusy("ImportMagRelatedRunPapers");
                if (result.nImported != null) {
                    if (result.nImported == magRelatedRun.nPapers) {

                        notificationMsg += "Imported " + result.nImported + " out of " +
                            magRelatedRun.nPapers + " items";

                    } else if (result.nImported != 0) {

                        notificationMsg += "Some of these items were already in your review.\n\nImported " +
                            result.nImported + " out of " + magRelatedRun.nPapers +
                            " new items";
                    }
                    else {
                        notificationMsg += "All of these records were already in your review.";
                    }
                    this.notificationService.showMAGRunMessage(notificationMsg);
                }

            },
                error => {
                    this.RemoveBusy("ImportMagRelatedRunPapers");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("ImportMagRelatedRunPapers");
                });
    }
    UpdateMagRelatedRun(magRelatedRun: MagRelatedPapersRun) {

        this._BusyMethods.push("UpdateMagRelatedRun");
        return this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/UpdateMagRelatedRun',
            magRelatedRun).
            subscribe(

                (result: MagRelatedPapersRun) => {
                    this.RemoveBusy("UpdateMagRelatedRun");
                    if (result.magRelatedRunId > 0) {
                        let tmpIndex: any = this.MagRelatedPapersRunList.findIndex(x => x.magRelatedRunId == Number(result.magRelatedRunId));
                        if (tmpIndex > -1) {
                           
                            this.MagRelatedPapersRunList[tmpIndex] = result;
                        }
                        this.notificationService.showMAGRunMessage('MAG search was updated');

                    } else {
                        this.notificationService.showMAGRunMessage('User status is: ' + result.userStatus);
                    }

                }, error => {
                    this.RemoveBusy("UpdateMagRelatedRun");
                    this.modalService.GenericErrorMessage('An api error with calling UpdateMagRelatedRun: ' + error);
                }
            );
    }
}
