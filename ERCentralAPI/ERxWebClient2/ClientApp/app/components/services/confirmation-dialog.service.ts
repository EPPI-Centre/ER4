import { Injectable, OnDestroy, EventEmitter, Input } from '@angular/core';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationDialogComponent } from '../ConfirmationDialog/confirmation-dialog.component';
import { EventEmitterService } from './EventEmitter.service';


@Injectable({

	providedIn: 'root',

})

export class ConfirmationDialogService implements OnDestroy {

	constructor(private modalService: NgbModal,
		private eventsService: EventEmitterService) { }

	//@Input() UserInputTextArms: string = '';

	public confirm(

		title: string,
		message: string,
		ShowInputTextWarning: boolean,
        RequiredConfirmationTxt: string = "I confirm",
		btnOkText: string = 'OK',
		btnCancelText: string = 'Cancel',

		dialogSize: 'sm' | 'lg' = 'sm')
		: any {
		
		let modalRef = this.modalService.open(ConfirmationDialogComponent,
			{size: dialogSize  }
		);
		modalRef.componentInstance.UserOuputText
		modalRef.componentInstance.title = title;
		modalRef.componentInstance.message = message;
		modalRef.componentInstance.btnOkText = btnOkText;
        modalRef.componentInstance.btnCancelText = btnCancelText;
        modalRef.componentInstance.ShowInputTextWarning = ShowInputTextWarning;
        modalRef.componentInstance.RequiredConfirmationTxt = RequiredConfirmationTxt;
		//this.UserInputTextArms = UserInputTextArms;
		
		return modalRef.result;
	}

	ngOnDestroy() {

	}

}