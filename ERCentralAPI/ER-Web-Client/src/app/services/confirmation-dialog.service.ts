import { Injectable, OnDestroy } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationDialogComponent } from '../ConfirmationDialog/confirmation-dialog.component';
import { NotificationService } from '@progress/kendo-angular-notification';


@Injectable({

	providedIn: 'root',

})

export class ConfirmationDialogService implements OnDestroy {

	constructor(private modalService: NgbModal,
		private notificationService: NotificationService) { }

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
		
		return modalRef.result;
  }
  public ShowInformationalModal(message: string, title: string = "Information", dialogSize: 'sm' | 'lg' = 'sm'): any {
    let modalRef = this.modalService.open(ConfirmationDialogComponent,
      { size: dialogSize }
    ); 
    modalRef.componentInstance.title = title;
    modalRef.componentInstance.message = message;
    modalRef.componentInstance.btnCancelText = "OK";
    modalRef.componentInstance.ShowInputTextWarning = false;
    modalRef.componentInstance.IsInformational = true;
  }

	public showMAGRunMessage(notifyMsg: string) {

		this.notificationService.show({
			content: notifyMsg,
			animation: { type: 'slide', duration: 400 },
			position: { horizontal: 'center', vertical: 'top' },
			type: { style: "info", icon: true },
			closable: true
		});
	}
	public showMAGDelayMessage(notifyMsg: string) {

		this.notificationService.show({
			content: notifyMsg,
			animation: { type: 'slide', duration: 400 },
			position: { horizontal: 'center', vertical: 'top' },
			type: { style: "info", icon: true },
			hideAfter: 4000
		});
	}
	public showErrorMessageInStrip(notifyMsg: string) {

		this.notificationService.show({
			content: notifyMsg,
			animation: { type: 'slide', duration: 400 },
			position: { horizontal: 'center', vertical: 'top' },
			type: { style: "error", icon: true },
			closable: true
		});
  }
  public showWarningMessageInStrip(notifyMsg: string) {

    this.notificationService.show({
      content: notifyMsg,
      animation: { type: 'slide', duration: 400 },
      position: { horizontal: 'center', vertical: 'top' },
      type: { style: "warning", icon: true },
      closable: true
    });
  }
	ngOnDestroy() {

	}

}
