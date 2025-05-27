import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { iRobotCoderReadOnly, iRobotSettings, RobotsService } from '../services/Robots.service';
import { Helpers } from '../helpers/HelperMethods';

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
  public showRobotDetails = true;
  ngOnInit() {

  }
  CanWrite(): boolean {
    return this._reviewerIdentityServ.HasWriteRights;
  }

  public get RobotSettings(): iRobotSettings {
    return this.robotsService.RobotSetting;
  }
  public get RobotsList() {
    return this.robotsService.RobotsList;
  }
  FormatDate(DateSt: string): string {
    if (DateSt == "0001-01-01T00:00:00") return "None";
    return Helpers.FormatDate2(DateSt);
  }

  RobotChanged(event: Event) {
    let name =(event.target as HTMLOptionElement).value;
    this.RobotSettings.robotName = name;
  }
  public get SelectedRobot(): iRobotCoderReadOnly | null {
    if (this.RobotSettings.robotName == "") return null;
    const selected = this.RobotsList.find(f => f.robotName == this.RobotSettings.robotName);
    if (!selected) return null;
    return selected;
  }
  ngOnDestroy() {

  }
  ngAfterViewInit() {

  }
}
