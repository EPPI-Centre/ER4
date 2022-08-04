import {
    ChangeDetectionStrategy,
    Component
} from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
    selector: 'app-confirm-dialog',
    template: `
<div>
  <div class="modal-header  bg-danger">
    <h4 class="modal-title">{{title}}</h4>
  </div>
  <div  class="modal-body" >
    <p [innerHTML]="prompt"></p>
    <button  *ngIf="DetailsAsHTMLdoc !== ''" class="btn btn-sm py-1 px-1 my-0 ml-0 mr-1 btn-outline-info" (click)="SeeMore()">See more...</button>
  </div>
  <div class="modal-footer">
      <button type="button"
      class="btn btn-danger"
      (click)="activeModal.close(true)">Close</button>
  </div>
</div>
`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ModalDialogComponent {

    title: string = '';
    prompt: string= '';
    DetailsAsHTMLdoc: string = '';
    constructor(public activeModal: NgbActiveModal) {
    }
    SeeMore() {
        let Pagelink = "about:blank";
        let pwa = window.open(Pagelink, "_new");
        //let pwa = window.open("data:text/plain;base64," + btoa(this.AddHTMLFrame(this.ReportHTML)), "_new");
        if (pwa) {
            pwa.document.open();
            pwa.document.write(this.DetailsAsHTMLdoc);
            pwa.document.close();
        }
    }
}