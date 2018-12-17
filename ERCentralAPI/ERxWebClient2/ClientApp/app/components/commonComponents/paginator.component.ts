import { Component, Inject, OnInit, EventEmitter, Output } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';
import { FormGroup, FormControl, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject } from 'rxjs/index';
import { Services } from '@angular/core/src/view';


@Component({
    selector: 'paginatorComp',
    templateUrl: './paginator.component.html',
    providers: [],
    styles: ["button.disabled {color:black; }"]
})
export class paginatorComp implements OnInit {

    constructor(private router: Router,
        public ItemListService: ItemListService    // I would like to make this generic

    ) {

    }

    onSubmit(f: string) {

    }
    private sub: any;

    value = 1;
    onEnter(value: number) {
        this.value = value; 
        this.ItemListService.FetchParticularPage(value - 1);
    }
    
    ngOnInit() {
	
    }

    nextPage() {
       
        this.ItemListService.FetchNextPage();
    }
    prevPage() {
      
        this.ItemListService.FetchPrevPage();
    }
    firstPage() {
     
        this.ItemListService.FetchFirstPage();
    }
    lastPage() {
       
        this.ItemListService.FetchLastPage();
    }
    
}






