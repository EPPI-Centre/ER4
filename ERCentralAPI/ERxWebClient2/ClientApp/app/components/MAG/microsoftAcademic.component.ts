import { Component, OnInit, OnDestroy, Inject, Input} from '@angular/core';
import { Router } from '@angular/router';

@Component({
   
	selector: 'microsoftAcademicComp',
	templateUrl: './microsoftAcademic.component.html'  

})

export class microsoftAcademicComp implements OnInit, OnDestroy {

    constructor(private router: Router, 
        @Inject('BASE_URL') private _baseUrl: string
    ) { }

    @Input() ItemID: number = 0;

    ngOnInit() {

        //console.log('itemId here is: ' + this.ItemID);
    }
 
    BackToMain() {

        this.router.navigate(['Main']);
    }
	ngOnDestroy() {

	}
}


