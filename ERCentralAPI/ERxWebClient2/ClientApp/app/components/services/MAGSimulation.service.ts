import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import {  MagPaperList, MagPaper,   MagSimulation } from './MAGClasses.service';

@Injectable({
    providedIn: 'root',
})

export class MAGSimulationService extends BusyAwareService {
    
    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }

    private _MagSimulationList: MagSimulation[] = [];
    public get MagSimulationList(): MagSimulation[] {
        return this._MagSimulationList;
    }
    public set MagSimulationList(magSimulationList: MagSimulation[]) {
        this._MagSimulationList = magSimulationList;
    }

    public DeleteSimulation(item: MagSimulation) {

        this._BusyMethods.push("DeleteSimulation");
        let body = JSON.stringify({ Value: item.magSimulationId });
        return this._httpC.post<MagSimulation>(this._baseUrl + 'api/MAGSimulationList/DeleteMagSimulation', body)
            .toPromise().then(
                (result: MagSimulation) => {

                    this.RemoveBusy("DeleteSimulation");
                    if (result != null && result.magSimulationId > 0) {

                        let ind: number = this.MagSimulationList.findIndex((x) => x.magSimulationId == item.magSimulationId);
                        if (ind > -1) {
                            this.MagSimulationList.splice(ind, 1);
                        }
                    }
                },
                error => {
                    this.RemoveBusy("DeleteSimulation");
                    this.modalService.GenericError(error);
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with DeleteSimulation: " + error);
                    this.RemoveBusy("DeleteSimulation");
                });
    }
    public AddMagSimulation(newMagSimulation: MagSimulation) {
        this._BusyMethods.push("AddMagSimulation");
        return this._httpC.post<MagSimulation>(this._baseUrl + 'api/MAGSimulationList/CreateMagSimulation', newMagSimulation)
            .toPromise().then(
                (result: MagSimulation) => {

                    this.RemoveBusy("AddMagSimulation");
                    if (this.MagSimulationList != null) {
                        this.MagSimulationList.push(result);
                    }
                },
                error => {
                    this.RemoveBusy("AddMagSimulation");
                    this.modalService.GenericError(error);
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with AddMagSimulation" + error);
                    this.RemoveBusy("AddMagSimulation");
                });
    }
    public FetchMagSimulationList() {
        this._BusyMethods.push("FetchMagSimulationList");
        this._httpC.get<MagSimulation[]>(this._baseUrl + 'api/MagSimulationList/GetMagSimulationList')
            .subscribe(result => {
                this.RemoveBusy("FetchMagSimulationList");
                this.MagSimulationList = result;
            },
                error => {
                    this.RemoveBusy("FetchMagSimulationList");
                    this.modalService.GenericError(error);
                },
                () => {
                    this.RemoveBusy("FetchMagSimulationList");
                });
    }

    public Clear() {
        this._MagSimulationList = [];
    }
}
