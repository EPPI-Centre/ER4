import { Injectable, NgZone } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Observable, from, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
    ModalDialogComponent
} from '../ModalDialog/ModalDialog.component';
import { Router } from '@angular/router';

@Injectable({
    providedIn: 'root'
})
export class ModalService {

    constructor(private ngbModal: NgbModal,
        private ngZone: NgZone,
        private router: Router) { }

    private confirm(
        prompt = 'Really?!', title = 'Error', fullHTMLdoc = ''
    ): Observable<boolean | undefined> {
        const modal = this.ngbModal.open(
            ModalDialogComponent, { backdrop: 'static' });

        modal.componentInstance.prompt = prompt;
        modal.componentInstance.title = title;
        modal.componentInstance.DetailsAsHTMLdoc = fullHTMLdoc;
        return from(modal.result).pipe(
            catchError(error => {
                console.warn(error);
                return of(undefined);
            })
        );
    }
    //see: https://github.com/angular/angular/issues/25837#issuecomment-434049467
    //the ngZone thing makes sure Angular is happy, even when the error started outside ngZone (that is: PDFTron, in some cases...)
    public SendBackHome(message: string) {
        this.confirm(
            message
        ).subscribe(result => {
            this.ngZone.run(() => this.router.navigate(['home']).then());
        }, error => {
            this.ngZone.run(() => this.router.navigate(['home']).then());
        });
    }
    public SendBackHomeWithError(error: any) {
        if (error.error) {
            const FullE = error.error as string;
            if (FullE.includes('<!DOCTYPE html')) {
                //full error is an HTML (created by IIS, usually)
                this.confirm(
                    'Sorry, could not complete the operation. Error code is: ' + error.status + " (" + error.statusText + ').'
                    , "Error"
                    , FullE
                ).subscribe(result => {
                    this.ngZone.run(() => this.router.navigate(['home']).then());
                }, error => {
                    this.ngZone.run(() => this.router.navigate(['home']).then());
                });
            } else {
                this.confirm(
                    'Sorry, could not complete the operation. Error code is: ' + error.status + " (" + error.statusText + '). Error Message is: "' + error.error + '".'
                ).subscribe(result => {
                    this.ngZone.run(() => this.router.navigate(['home']).then());
                }, error => {
                    this.ngZone.run(() => this.router.navigate(['home']).then());
                });
            }
        }
        else {
            this.confirm(
                'Sorry, could not complete the operation. Error code is: ' + error.status + " (" + error.statusText + ').'
            ).subscribe(result => {
                this.ngZone.run(() => this.router.navigate(['home']).then());
            }, error => {
                this.ngZone.run(() => this.router.navigate(['home']).then());
            });
        }
    }
    public GenericError(error: any) {
        if (error.error) {
            const FullE = error.error as string;
            if (FullE.includes('<!DOCTYPE html')) {
                //full error is an HTML (created by IIS, usually)
                this.confirm(
                    'Sorry, could not complete the operation. Error code is: ' + error.status + " (" + error.statusText + ').'
                    , "Error"
                    , FullE
                );
            } else {
                this.confirm(
                  'Sorry, could not complete the operation. Error code is: ' + error.status + " (" + error.statusText + '). Error Message is: <br>' + error.error + ''
                );
            }
        }
        else {
            this.confirm(
                'Sorry, could not complete the operation. Error code is: ' + error.status + " (" + error.statusText + ').'
            );
        }
    
    }
    public GenericErrorMessage(Message: string) {
        this.confirm(Message);
	}

	//private modals: any[] = [];

	//add(modal: any) {
	//	// add modal to array of active modals
	//	this.modals.push(modal);
	//}

	//remove(id: string) {
	//	// remove modal from array of active modals
	//	let modalToRemove = _.findWhere(this.modals, { id: id });
	//	this.modals = _.without(this.modals, modalToRemove);
	//}

	//open(id: string) {
	//	// open modal specified by id
	//	let modal = _.findWhere(this.modals, { id: id });
	//	modal.open();
	//}

	//close(id: string) {
	//	// close modal specified by id
	//	let modal = _.find(this.modals, { id: id });
	//	modal.close();
	//}
}
