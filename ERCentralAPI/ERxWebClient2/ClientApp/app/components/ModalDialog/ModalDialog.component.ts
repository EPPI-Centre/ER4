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
  <div class="modal-body">
    <p>{{prompt}}</p>
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

    constructor(public activeModal: NgbActiveModal) {
    }
}