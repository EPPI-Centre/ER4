import { Injectable, OnDestroy } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationDialogComponent } from '../ConfirmationDialog/confirmation-dialog.component';


@Injectable({

	providedIn: 'root',

})

export class ConfirmationDialogService implements OnDestroy {

	constructor(private modalService: NgbModal) { }

	public confirm(
		title: string,
		message: string,
		btnOkText: string = 'OK',
		btnCancelText: string = 'Cancel',
		dialogSize: 'sm' | 'lg' = 'sm'): Promise<boolean> {
		const modalRef = this.modalService.open(ConfirmationDialogComponent, { size: dialogSize });
		modalRef.componentInstance.title = title;
		modalRef.componentInstance.message = message;
		modalRef.componentInstance.btnOkText = btnOkText;
		modalRef.componentInstance.btnCancelText = btnCancelText;

		return modalRef.result;
	}

	ngOnDestroy() {


	}

}