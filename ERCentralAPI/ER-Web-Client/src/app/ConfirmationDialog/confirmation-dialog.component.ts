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
  @Input() btnOkText: string = '';
  @Input() btnCancelText: string = '';
  @Input() RequiredConfirmationTxt: string = '';
  @Input() IsInformational: boolean = false;
  @Input() IsLarge: boolean = false; 
	//@Output() action = new EventEmitter<string>();

	public UserInputConfirmationText: string = '';

	public ShowInputTextWarning: boolean = false;
	constructor(private activeModal: NgbActiveModal,
		private eventsService: EventEmitterService) {

	}

	ngOnInit() {
	}

	public decline() {
		this.activeModal.close(false);
	}

    CheckInputBoxEntry(): boolean {
        //console.log("CheckInputBoxEntry", this.UserInputConfirmationText, "<->", this.RequiredConfirmationTxt);
        return this.UserInputConfirmationText.toLowerCase().trim() == this.RequiredConfirmationTxt.toLowerCase().trim();
  }
  public get PlaceButtonsOnTwoLines(): boolean {
    const length = this.btnOkText.length + this.btnCancelText.length;
    if (this.IsLarge == false && length > 21) return true;
    return false;
  }
  public get ReduceCancelPadding(): boolean {
    const length = this.btnOkText.length + this.btnCancelText.length;
    if (this.IsLarge == false && length > 15) return true;
    return false;
  }
	   
	public accept() {
        this.eventsService.UserInput = this.UserInputConfirmationText;
		this.activeModal.close(true);
	}

	public dismiss() {

		this.activeModal.dismiss();
	}
}
