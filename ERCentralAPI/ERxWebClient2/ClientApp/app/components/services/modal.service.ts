import { Injectable } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Observable, from, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
    ConfirmDialogComponent
} from '../confirm-dialog/confirm-dialog.component';
//import {
//    InputDialogComponent
//} from './input-dialog/input-dialog.component';
//import {
//    MessageDialogComponent
//} from './message-dialog/message-dialog.component';

@Injectable({
    providedIn: 'root'
})
export class ModalService {

    constructor(private ngbModal: NgbModal) { }

    public confirm(
        prompt = 'Really?', title = 'Error'
    ): Observable<boolean> {
        const modal = this.ngbModal.open(
            ConfirmDialogComponent, { backdrop: 'static' });

        modal.componentInstance.prompt = prompt;
        modal.componentInstance.title = title;

        return from(modal.result).pipe(
            catchError(error => {
                console.warn(error);
                return of(undefined);
            })
        );
    }

    // similar methods for other standard dialogs...
}