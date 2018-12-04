import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SourcesService, ReadOnlySource } from '../services/sources.service';


@Component({
    selector: 'SourcesComp',
    templateUrl: './sources.component.html',
    providers: []
})

export class SourcesComponent implements OnInit {
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        private SourcesService: SourcesService
    ) {    }

    ngOnInit() {
        if (this.SourcesService.ReviewSources && this.SourcesService.ReviewSources.length == 0) this.SourcesService.FetchSources();
    }
    get ReviewSources(): ReadOnlySource[] {
        return this.SourcesService.ReviewSources;
    }
    BackToMain() {
        this.router.navigate(['mainFullReview']);
    }
    HideManuallyCreatedItems(ROS: ReadOnlySource): boolean {
        if (ROS.source_Name == 'NN_SOURCELESS_NN' && ROS.source_ID == -1) return true;
        else return false;
    }
    SelectSource(ROS: ReadOnlySource) {

    }
}
