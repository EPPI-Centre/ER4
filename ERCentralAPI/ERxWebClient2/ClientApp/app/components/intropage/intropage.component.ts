import { Component, Inject, OnInit, ViewChild, AfterViewInit, } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { readonlyreviewsService } from '../services/readonlyreviews.service';
import { FetchReadOnlyReviewsComponent } from '../readonlyreviews/readonlyreviews.component';

@Component({
    selector: 'intropage',
    templateUrl: './intropage.component.html',
    providers: []
})
export class intropageComponent implements OnInit, AfterViewInit  {

    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        private CodesetStatisticsService: CodesetStatisticsService        
    ) { }
    ngOnInit() {
        this.CodesetStatisticsService.Clear();
    }
    @ViewChild(FetchReadOnlyReviewsComponent) private ReadOnlyReviewsComponent!: FetchReadOnlyReviewsComponent;
    ngAfterViewInit() {
        if (this.ReadOnlyReviewsComponent) this.ReadOnlyReviewsComponent.getReviews();
    }
}
