import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { iRobotSettings, RobotsService } from '../services/Robots.service';

@Component({
  selector: 'RobotSettings',
  templateUrl: './robotSettings.component.html',
  providers: []
})

export class RobotSettings implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private _reviewService: ReviewService,
    private _reviewerIdentityServ: ReviewerIdentityService,
    private robotsService: RobotsService,
    private modalService: ModalService
  ) { }

  //@Output() onCloseClick = new EventEmitter();
  @Input() Context: string = "";
  public isExpanded: boolean = false;
  ngOnInit() {

  }
  CanWrite(): boolean {
    return this._reviewerIdentityServ.HasWriteRights;
  }

  public get RobotSettings(): iRobotSettings {
    return this.robotsService.RobotSetting;
  }

  ngOnDestroy() {

  }
  ngAfterViewInit() {

  }
}
