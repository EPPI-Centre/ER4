import { Component } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    //styleUrls: ['./app.component.css'],
    styles: [`
                .body-content {
                    padding-top: 50px;
                }
               .MainBg {
                    background-color:#d8d8e2 !important; 
                }
        `],
})
export class AppComponent {
    constructor() { }
}
