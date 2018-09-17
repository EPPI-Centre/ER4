import { Component, Inject, } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';

@Component({
    selector: 'intropage',
    templateUrl: './intropage.component.html',
    providers: []
})
export class intropageComponent  {

    constructor(private router: Router,
                @Inject('BASE_URL') private _baseUrl: string,
               ) {  }
}
