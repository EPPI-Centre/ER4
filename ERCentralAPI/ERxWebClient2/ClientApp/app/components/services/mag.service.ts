import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { NotificationService } from '@progress/kendo-angular-notification';
import { MagSimulation } from './magAdvanced.service';

@Injectable({
    providedIn: 'root',
})

export class MAGService extends BusyAwareService {
    

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        private notificationService: NotificationService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
	}
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

	Fetch() {

        this._BusyMethods.push("MagRelatedPapersRunFetch");
		this._httpC.get<MagRelatedPapersRun[]>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRuns')
			.subscribe(result => {
                this.RemoveBusy("MagRelatedPapersRunFetch");
				this.MagRelatedPapersRunList = result;
			},
				error => {
                    this.RemoveBusy("MagRelatedPapersRunFetch");
					this.modalService.GenericError(error);
				}
			);
	}

	Delete(magRun: MagRelatedPapersRun) {

		console.log(magRun);
        this._BusyMethods.push("MagRelatedPapersRunDelete");
		this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/DeleteMagRelatedPapersRun',
			magRun)
			.subscribe(result => {

                if (result.magRelatedRunId > 0) {

                    this.showMAGRunMessage('MAGRun was deleted');

                } else {

                    this.showMAGRunMessage(result.status);
                }

                this.RemoveBusy("MagRelatedPapersRunDelete");
                let tmpIndex: any = this.MagRelatedPapersRunList.findIndex(x => x.magRelatedRunId == Number(result.magRelatedRunId));
				this.MagRelatedPapersRunList.splice(tmpIndex, 1);
				this.Fetch();

			}, error => {
                this.RemoveBusy("MagRelatedPapersRunDelete");
				this.modalService.GenericError(error);
			}
			);
	}

	Create(magRun: MagRelatedPapersRun) {


        this._BusyMethods.push("MagRelatedPapersRunCreate");
		this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/CreateMagRelatedPapersRun',
			magRun)
			.subscribe(result => {

                if (result.magRelatedRunId > 0) {

                    this.showMAGRunMessage('MAGRun was created');

                } else {

                    this.showMAGRunMessage(result.status);
                }
                this.RemoveBusy("MagRelatedPapersRunCreate");
				this.MagRelatedPapersRunList.push(result);
				this.Fetch();

			}, error => {
                this.RemoveBusy("MagRelatedPapersRunCreate");
				this.modalService.GenericError(error);
			}
			);
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

    ImportMagPapers(magRun: MagRelatedPapersRun) {

        let notificationMsg: string = '';
        this._BusyMethods.push("ImportMagRelatedPapers");
        this._httpC.post<MagItemPaperInsertCommand>(this._baseUrl + 'api/MagRelatedPapersRunList/ImportMagRelatedPapers',
            magRun)
            .subscribe(result => {
                this.RemoveBusy("ImportMagRelatedPapers");
                this.MagItemPaperInsert = result;

                    if (result.nImported == magRun.nPapers) {

                         notificationMsg += "Imported " + result.nImported + " out of " +
                             magRun.nPapers + " items";

                    }else if(result.nImported != 0) {

                        notificationMsg += "Some of these items were already in your review.\n\nImported " +
                            result.nImported + " out of " + magRun.nPapers +
                            " new items";
                    }
                    else {

                        notificationMsg += "All of these records were already in your review.";

                    }
                    this.showMAGRunMessage(notificationMsg);

            },
                error => {
                    this.RemoveBusy("ImportMagRelatedPapers");
                    this.modalService.GenericError(error);
                }
            );

    }

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
	allIncluded: string = '';
    dateFrom: Date = new Date(2000, 2, 10);
	autoReRun: string = '';
	mode: string = '';
	filtered: string = '';
	status: string = '';
	userStatus: string = '';
	nPapers: number = 0;
	reviweIdId = 0;
}


export class MagPaperListSelectionCriteria {

    pageSize: number = 20;
    pageNumber: number = 0;
    listType: string = "MagRelatedPapersRunList";
    magRelatedRunId: number = 0;

}
