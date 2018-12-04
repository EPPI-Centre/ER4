import { Component, ElementRef, Input, OnInit, OnDestroy } from '@angular/core';
import * as $ from 'jquery';
 
import { ModalSearchService } from '../services/modalSearch.service';
 
@Component({

    moduleId: module.id.toString(),
	selector: 'modal',
	template: '<ng-content></ng-content>',

	//template: `<modal id="custom-modal-1">
	//	<div class= "modal">
	//		<div class="modal-body">
	//			<h1>A Custom Modal! </h1>
	//				Home page text: <input type = "text"[(ngModel)] = "bodyText"/>
	//				<button (click)="closeModal('custom-modal-1');"> Close </button>
	//				</div>
	//				</div>
	//<div class="modal-background"> </div>
	//</modal>`,
})
 
export class ModalSearchComponent implements OnInit, OnDestroy {

    @Input() id!: string;
    private element: JQuery;
 
	constructor(private modalService: ModalSearchService,
		private el: ElementRef) {
        this.element = $(el.nativeElement);
    }
 
	ngOnInit(): void {


        let modal = this;
 
        // ensure id attribute exists
        if (!this.id) {
            console.error('modal must have an id');
            return;
        }
 
        // move element to bottom of page (just before </body>) so it can be displayed above everything else
        this.element.appendTo('body');
 
        // close modal on background click
        this.element.on('click', function (e: any) {
            var target = $(e.target);
            if (!target.closest('.modal-body').length) {
                modal.close();
            }
        });
 
        // add self (this modal instance) to the modal service so it's accessible from controllers
        this.modalService.add(this);
    }
 
    // remove self from modal service when directive is destroyed
    ngOnDestroy(): void {
        this.modalService.remove(this.id);
        this.element.remove();
    }
 
    // open modal
    open(): void {
        this.element.show();
        $('body').addClass('modal-open');
    }
 
    // close modal
    close(): void {
        this.element.hide();
        $('body').removeClass('modal-open');
    }
}