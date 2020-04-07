import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { NotificationService } from '@progress/kendo-angular-notification';
import { MagPaperList, MagPaper } from './magAdvanced.service';
import { MAGListService } from './MagList.service';

@Injectable({
    providedIn: 'root',
})

export class BasicMAGService extends BusyAwareService {
    
    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        private _magListService: MAGListService,
        private notificationService: NotificationService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
    public MagPaperList: MagPaperList = new MagPaperList();
    public MagRelatedRunPapers: MagPaper[] = [];

	private _MagRelatedPapersRunList: MagRelatedPapersRun[] = [];
		
	public get MagRelatedPapersRunList(): MagRelatedPapersRun[] {

		return this._MagRelatedPapersRunList;

	}

	public set MagRelatedPapersRunList(magRun: MagRelatedPapersRun[]) {
		this._MagRelatedPapersRunList = magRun;

    }

    private _MagItemPaperInsert: MagItemPaperInsertCommand = new MagItemPaperInsertCommand();

    public get MagItemPaperInsert(): MagItemPaperInsertCommand{

        return this._MagItemPaperInsert;

    }

    public set MagItemPaperInsert(magRunCmd: MagItemPaperInsertCommand) {
        this._MagItemPaperInsert = magRunCmd;

    }

    FetchMagRelatedPapersRunList() {

        console.log('basic mag service 1');
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
    FetchMAGRelatedPaperRunsListId(Id : number) : Promise<void> {

        console.log('basic mag service 2');

        this._BusyMethods.push("FetchMAGRelatedPaperRunsListId");
        let body = JSON.stringify({Value: Id});
        return this._httpC.post<MagList>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRunsId',
        body)
            .toPromise().then(
            (result) => {

                console.log(result);
                    this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
                   // this.MagRelatedRunPapers = result;
                    this._magListService.MAGList = result;
                    this._magListService.ListCriteria.listType = "MagRelatedPapersRunList";
                    this._magListService.ListCriteria.pageSize = 20;
                    this._magListService.ListCriteria.magRelatedRunId = Id;
			    },
				    error => {
                        this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
					    this.modalService.GenericError(error);
				    }
                ).catch (
            (error) => {

                this.modalService.GenericErrorMessage("error with FetchMAGRelatedPaperRunsListId");
                this.RemoveBusy("FetchMAGRelatedPaperRunsListId");
            }      
            );
	}
	DeleteMAGRelatedRun(Id: number) {

        console.log('basic mag service 3');

		//console.log(magRun);
        this._BusyMethods.push("DeleteMAGRelatedRun");
        let body = JSON.stringify({Value: Id});
		this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/DeleteMagRelatedPapersRun',
			body)
			    .subscribe(result => {

                    this.RemoveBusy("DeleteMAGRelatedRun");
                    if (result.magRelatedRunId > 0) {

                        this.showMAGRunMessage('MAGRun was deleted');

                    } else {

                        this.showMAGRunMessage(result.status);
                    }
                    let tmpIndex: number = this.MagRelatedPapersRunList.findIndex(x => x.magRelatedRunId == Number(result.magRelatedRunId));
                    if (tmpIndex > -1) {
                        this.MagRelatedPapersRunList.splice(tmpIndex, 1);
                        //this.FetchMagRelatedPapersRunList();
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

        console.log('basic mag service 4');

        this._BusyMethods.push("MagRelatedPapersRunCreate");
		this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/CreateMagRelatedPapersRun',
			magRun)
			.subscribe(result => {

                this.RemoveBusy("MagRelatedPapersRunCreate");
                if (result.magRelatedRunId > 0) {

                    this.showMAGRunMessage('MAGRun was created');

                } else {

                    this.showMAGRunMessage(result.status);
                }
				this.MagRelatedPapersRunList.push(result);
                this.FetchMAGRelatedPaperRunsListId(result.magRelatedRunId);

			}, error => {
                this.RemoveBusy("MagRelatedPapersRunCreate");
				this.modalService.GenericError(error);
            },
            () => {
                this.RemoveBusy("MagRelatedPapersRunCreate");
            });
    }
    showMAGRunMessage(notifyMsg: string) {

        this.notificationService.show({
            content: notifyMsg,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
        });
    }
    ImportMagRelatedRunPapers(magRelatedRun: MagRelatedPapersRun) {

        console.log('basic mag service 5', magRelatedRun);

        let notificationMsg: string = '';
        this._BusyMethods.push("ImportMagRelatedRunPapers");
        this._httpC.post<MagItemPaperInsertCommand>(this._baseUrl + 'api/MagRelatedPapersRunList/ImportMagRelatedPapers',
            magRelatedRun)
            .subscribe(result => {

                this.RemoveBusy("ImportMagRelatedRunPapers");
                this.MagItemPaperInsert = result;
                if (result.nImported != null) {

                
                    if (result.nImported == magRelatedRun.nPapers) {

                         notificationMsg += "Imported " + result.nImported + " out of " +
                             magRelatedRun.nPapers + " items";

                    }else if(result.nImported != 0) {

                        notificationMsg += "Some of these items were already in your review.\n\nImported " +
                            result.nImported + " out of " + magRelatedRun.nPapers +
                            " new items";
                    }
                    else {

                        notificationMsg += "All of these records were already in your review.";

                    }
                    this.showMAGRunMessage(notificationMsg);
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
    UpdateMagRelatedRun(magRelatedRun: MagRelatedPapersRun): Promise<void> {

        console.log('basic mag service 6');

        console.log(magRelatedRun);
        this._BusyMethods.push("UpdateMagRelatedRun");
        return this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/UpdateMagRelatedRun',
            magRelatedRun).
        toPromise().then(

            (result: MagRelatedPapersRun) => {
                this.RemoveBusy("UpdateMagRelatedRun");
                if (result.magRelatedRunId > 0) {
                    let tmpIndex: any = this.MagRelatedPapersRunList.findIndex(x => x.magRelatedRunId == Number(result.magRelatedRunId));
                    if (tmpIndex > -1) {
                        console.log(tmpIndex);
                        this.MagRelatedPapersRunList[tmpIndex] = result;
                        //this.FetchMagRelatedPapersRunList();
                    }
                    this.showMAGRunMessage('MAG Run was updated');

                } else {
                    this.showMAGRunMessage('User status is: ' + result.userStatus);
                }
                

            }, error => {
                this.RemoveBusy("UpdateMagRelatedRun");
                this.modalService.GenericError(error);
            }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with UpdateMagRelatedRun");
                    this.RemoveBusy("UpdateMagRelatedRun");
                });
        }
}
export class MagList {

    pagesize: number = 0;
    paperIds: string = '';
    pagecount: number = 0;
    pageindex: number = 0;
    totalItemCount: number = 0;
    papers: MagPaper[] = [];

}
export class MagItemPaperInsertCommand {

    paperIds: string = '';
    nImported: number = 0;
    sourceOfIds: string = '';
    magRelatedRunId: number = 0;

}
export class MagRelatedPapersRun {

	magRelatedRunId: number = 0;
	userDescription: string = '';
    attributeId: number = 0;
    attributeName: string = '';
    allIncluded: boolean = false;
    dateRun: string = "";
    dateFrom: string = "";
    autoReRun: boolean = false;
	mode: string = '';
	filtered: string = '';
	status: string = '';
	userStatus: string = '';
	nPapers: number = 0;
	reviweIdId = 0;
}
export class MagRelatedPaperListSelectionCriteria {

    pageSize: number = 20;
    pageNumber: number = 0;
    listType: string = "MagRelatedPapersRunList";
    magRelatedRunId: number = 0;

}
