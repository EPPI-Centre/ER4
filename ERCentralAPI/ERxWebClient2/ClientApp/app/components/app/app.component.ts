import { Component } from '@angular/core';
import { ReviewerIdentityService } from '../app/revieweridentity.service';

@Component({
    selector: 'app',
    providers: [ReviewerIdentityService],
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {
    constructor(private ReviewerIdentity: ReviewerIdentityService) { }
}
