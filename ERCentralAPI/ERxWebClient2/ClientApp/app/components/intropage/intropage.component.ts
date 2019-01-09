import { Component, Inject, OnInit, } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';

@Component({
    selector: 'intropage',
    templateUrl: './intropage.component.html',
    providers: []
})
export class intropageComponent implements OnInit  {

    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        private CodesetStatisticsService: CodesetStatisticsService
    ) { }
    ngOnInit() {
        this.CodesetStatisticsService.Clear();
    }
}
