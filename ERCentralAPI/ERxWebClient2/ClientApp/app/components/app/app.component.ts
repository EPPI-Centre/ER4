import { Component } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

@Component({
    selector: 'app',
    providers: [ReviewerIdentityService],
    templateUrl: './app.component.html',
    //styleUrls: ['./app.component.css'],
    styles: [`
                .body-content {
                    padding-top: 50px;
                }
               .MainBg {
                    background-color:#f8f8fd !important; 
                }
        `],
})
export class AppComponent {
    constructor(private ReviewerIdentity: ReviewerIdentityService) { }
}
