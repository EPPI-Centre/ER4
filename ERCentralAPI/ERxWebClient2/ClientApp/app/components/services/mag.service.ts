import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';

@Injectable({
    providedIn: 'root',
})

export class MAGService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
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


        this._BusyMethods.push("MagRelatedPapersRunDelete");
		this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/DeleteMagRelatedPapersRun',
			magRun)
			.subscribe(result => {

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

                this.RemoveBusy("MagRelatedPapersRunCreate");
				this.MagRelatedPapersRunList.push(result);
				this.Fetch();

			}, error => {
                this.RemoveBusy("MagRelatedPapersRunCreate");
				this.modalService.GenericError(error);
			}
			);
	}

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
	
}
