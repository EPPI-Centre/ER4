import {  Inject, Injectable } from '@angular/core';
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
				//console.log(JSON.stringify(this.MagRelatedPapersRunList));
				//console.log('la la: ' + this.MagRelatedPapersRunList.length);
			},
				error => {
                    this.RemoveBusy("MagRelatedPapersRunFetch");
					this.modalService.GenericError(error);
				}
			);
	}

	Delete(value: MagRelatedPapersRun) {

        this._BusyMethods.push("MagRelatedPapersRunDelete");
		let body = JSON.stringify({ Value: value });
		this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/DeleteMagRelatedPapersRun',
			body)
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

	
}

export class MagRelatedPapersRun {

	magRelatedRunId: number = 0;
	userDescription: string = '';
	paperIdList: string = '';
	attributeId: number = 0;
	allIncluded: string = '';
	dateFrom: string = '';
	autoReRun: string = '';
	mode: string = '';
	filtered: string = '';
	status: string = '';
	userStatus: string = '';
	nPapers: number = 0;
	
}
