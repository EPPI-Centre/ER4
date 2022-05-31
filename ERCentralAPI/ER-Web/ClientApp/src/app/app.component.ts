import { Component } from '@angular/core';

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
        `]
})
export class AppComponent {
  title = 'app';
}
