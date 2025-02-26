import { Component, OnInit, ViewChild, EventEmitter, Input, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { SelectEvent, TabStripComponent } from '@progress/kendo-angular-layout';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import { ReviewInfoService } from '../services/ReviewInfo.service';

@Component({
  selector: 'JobsContainer',
  templateUrl: './JobsContainer.component.html',
  providers: []
})
export class JobsContainer implements OnInit, OnDestroy {

  constructor(private router: Router,
    private route: ActivatedRoute,
    public reviewInfoService: ReviewInfoService
  ) {

  }

  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;

  ngOnInit() {
    if (this.reviewInfoService.ReviewInfo.reviewId < 1) this.reviewInfoService.Fetch();
  }
  public get CanUseRobots(): boolean {
    return this.reviewInfoService.ReviewInfo.canUseRobots;
  }
  onTabSelect(e: SelectEvent) {
  }


  BackToMain() {
    this.router.navigate(['Main']);
  }

  ngOnDestroy() {

  }
}




