import { Component } from '@angular/core';
@Component({
    selector: 'app-root2',
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
export class AppComponent2 {
    constructor() { }
}
