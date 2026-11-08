import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styles: [`
                .body-content {
                    padding-top: 50px;
                }
               .MainBg {
                    background-color:#d8d8e2 !important; 
                }
        `],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class AppComponent {
  title = 'app';
}
