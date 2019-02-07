import { Component, Input, OnInit, EventEmitter, Output } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { EventEmitterService } from '../services/EventEmitter.service';

@Component({
	selector: 'app-confirmation-dialog',
	templateUrl: './confirmation-dialog.component.html',
})
export class ConfirmationDialogComponent implements OnInit {

	@Input() title: string='';
	@Input() message: string='';
	@Input() btnOkText: string='';
	@Input() btnCancelText: string = '';
	//@Output() action = new EventEmitter<string>();

	public UserInputTextArms: string = '';

	public ShowInputTextWarning: boolean = false;
	
	constructor(private activeModal: NgbActiveModal,
		private eventsService: EventEmitterService) {

	}

	ngOnInit() {
	}

	public decline() {
		this.activeModal.close(false);
	}
	   
	public accept() {

		this.eventsService.UserInput = this.UserInputTextArms;
		
		this.activeModal.close(true);

	}

	public dismiss() {

		this.activeModal.dismiss();
	}
}