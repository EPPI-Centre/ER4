import { Injectable } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import * as _ from 'underscore';

@Injectable({
    providedIn: 'root'
})
export class ModalSearchService {

    constructor(private ngbModal: NgbModal, private router: Router) { }
	
	private modals: any[] = [];

	add(modal: any) {
		// add modal to array of active modals

		this.modals.push(modal);
	}

	remove(id: string) {
		// remove modal from array of active modals
		let modalToRemove = _.findWhere(this.modals, { id: id });
		this.modals = _.without(this.modals, modalToRemove);
	}

	open(id: string) {
		// open modal specified by id
		alert('number of modals: ' + this.modals.length);

		let modal = _.findWhere(this.modals, { id: id });

		modal.open();
	}

	close(id: string) {
		// close modal specified by id
		let modal = _.find(this.modals, { id: id });
		modal.close();
	}
}