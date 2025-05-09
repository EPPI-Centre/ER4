import { Inject, Injectable, OnDestroy} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { MAGBrowserService } from './MAGBrowser.service';
import { MagRelatedPapersRun, MagPaperList, MagPaper,  MagItemPaperInsertCommand, MagAutoUpdateRun, MagAutoUpdate, MagAutoUpdateVisualise, MagAutoUpdateVisualiseSelectionCriteria, MagAddClassifierScoresCommand } from './MAGClasses.service';
import { ConfirmationDialogService } from './confirmation-dialog.service';
import { Helpers } from '../helpers/HelperMethods';
import { EventEmitterService } from './EventEmitter.service';
import { lastValueFrom, Subscription } from 'rxjs';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ConfigService } from './config.service';

@Injectable({
    providedIn: 'root',
})

export class MAGRelatedRunsService extends BusyAwareService implements OnDestroy {

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        private notificationService: ConfirmationDialogService,
        private EventEmitterService: EventEmitterService,
        private ReviewerIdentityService: ReviewerIdentityService,
      configService: ConfigService
    ) {
      super(configService);
        console.log("On create MAGRelatedRunsService");
        this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(true); });
    }
    ngOnDestroy() {
        console.log("Destroy MAGRelatedRunsService");
        if (this.clearSub != null) this.clearSub.unsubscribe();
    }
    private clearSub: Subscription | null = null;

    private _MagRelatedPapersRunList: MagRelatedPapersRun[] = [];
    private _MagUpdatesList: MagAutoUpdate[] = [];
    private _MagAutoUpdateRunList: MagAutoUpdateRun[] = [];
    private _MagAutoUpdateVisualise: MagAutoUpdateVisualise[] = [];

    //these two "currently..." members are used to prevent re-applying a classifier to "autoUpdate runs" while already applying one!
    public currentlyApplyingModelToThisRunId: number = 0;
    public currentlyApplyedModelToRunId: string = "";
    //refresh until is used to avoid resetting the number of refreshes we'll do...
    public RefreshUntil: Date = new Date();
    //refreshForStudyClassifier tells us if the model to be applied is "study" or "user", hence it controls what we check for.
    public refreshForStudyClassifier: boolean = true;
    //we also need to stop the timer/looping/refreshing if the user has changed review...
    public refreshForStudyClassifierReviewId: number = 0;

    public get AutoRefreshIsOn(): boolean {
        return new Date() < this.RefreshUntil;
    }

    public get MagRelatedPapersRunList(): MagRelatedPapersRun[] {

        return this._MagRelatedPapersRunList;

    }
    public set MagRelatedPapersRunList(magRun: MagRelatedPapersRun[]) {
        this._MagRelatedPapersRunList = magRun;

    }
    public get MagAutoUpdatesList(): MagAutoUpdate[] {
        return this._MagUpdatesList;
    }
    //public set MagUpdatesList(magRun: MagRelatedPapersRun[]) {
    //    this._MagUpdatesList = magRun;
    //}
    public get MagAutoUpdateRunList(): MagAutoUpdateRun[] {
        return this._MagAutoUpdateRunList;
    }
    public get MagAutoUpdateVisualise(): MagAutoUpdateVisualise[] {
        return this._MagAutoUpdateVisualise;
    }
    FetchMagRelatedPapersRunList() {
        
        this._BusyMethods.push("FetchMagRelatedPapersRunList");
        lastValueFrom(this._httpC.get<MagRelatedPapersRun[]>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagRelatedPapersRuns')
        ).then(result => {
                this.RemoveBusy("FetchMagRelatedPapersRunList");
                this.MagRelatedPapersRunList = result;
            },
                error => {
                    this.RemoveBusy("FetchMagRelatedPapersRunList");
                    this.modalService.GenericError(error);
          }).catch((caught) => {
            this.RemoveBusy("FetchMagRelatedPapersRunList");
            this.modalService.GenericError(caught);
          });
    }

    public GetMagAutoUpdateList(alsoRuns: boolean = false) {

        this._BusyMethods.push("FetchMagUpdateLists");
        lastValueFrom(this._httpC.get<MagAutoUpdate[]>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagAutoUpdateList')
        ).then(result => {
                this._MagUpdatesList = result;
                if (alsoRuns) {
                    this.GetMagAutoUpdateRunList();
                }
                this.RemoveBusy("FetchMagUpdateLists");
            },
                error => {
                    this.RemoveBusy("FetchMagUpdateLists");
                    this.modalService.GenericError(error);
          }).catch((caught) => {
            this.RemoveBusy("FetchMagUpdateLists");
            this.modalService.GenericError(caught);
          });
    }
    public GetMagAutoUpdateRunList() {
        this._BusyMethods.push("GetMagAutoUpdateRunList");
        lastValueFrom(this._httpC.get<MagAutoUpdateRun[]>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagAutoUpdateRuns')
        ).then(result => {
                this._MagAutoUpdateRunList = result; 
                this.RemoveBusy("GetMagAutoUpdateRunList");
            },
                error => {
                    this.RemoveBusy("GetMagAutoUpdateRunList");
                    this.modalService.GenericError(error);
          }).catch((caught) => {
            this.RemoveBusy("GetMagAutoUpdateRunList");
            this.modalService.GenericError(caught);
          });
    }

    public async RefreshAutoUpdateRunsOnTimer() {
        this.refreshForStudyClassifierReviewId = this.ReviewerIdentityService.reviewerIdentity.reviewId;
        while (new Date() < this.RefreshUntil && this.ReviewerIdentityService.reviewerIdentity.reviewId == this.refreshForStudyClassifierReviewId) {
            await Helpers.Sleep(1000 * 25);
            if (this.ReviewerIdentityService.reviewerIdentity.reviewId !== this.refreshForStudyClassifierReviewId) {
                //user changed review in the meantime...
                this.RefreshUntil = new Date();
                break;
            }
            this.GetMagAutoUpdateRunList();
            await Helpers.Sleep(1000 * 5);//we give GetMagAutoUpdateRunList 5s to complete, not a big deal if it's not enough
            if (this.ReviewerIdentityService.reviewerIdentity.reviewId !== this.refreshForStudyClassifierReviewId) {
                //user changed review in the meantime...
                this.RefreshUntil = new Date();
                break;
            }

            let newRun = this.MagAutoUpdateRunList.find(f => f.magAutoUpdateRunId == this.currentlyApplyingModelToThisRunId);
            if (newRun != undefined) {
                //we'll check if the model has been applied
                if ((this.refreshForStudyClassifier && newRun.studyTypeClassifier !== this.currentlyApplyedModelToRunId)
                    || (!this.refreshForStudyClassifier && newRun.userClassifierModelId.toString() !== this.currentlyApplyedModelToRunId)) {
                    //yay! it's in, so we can stop looping
                    this.currentlyApplyingModelToThisRunId = 0;
                    this.currentlyApplyedModelToRunId = "";
                    this.RefreshUntil = new Date();
                    //this.CurrentMagAutoUpdateRun = newRun;//update what we have in here!!
                    //this.LoadGraph();//get update histogram, while we're there...
                    return;
                }
            }
        }
        //bad luck, either we were re-applying the same model or it's taking more than 15m, we give up
        this.currentlyApplyingModelToThisRunId = 0;
        this.currentlyApplyedModelToRunId = "";
        this.RefreshUntil = new Date();
    }

    public CreateAutoUpdate(magRun: MagRelatedPapersRun) {
        this._BusyMethods.push("CreateAutoUpdate");
        lastValueFrom(this._httpC.post<MagAutoUpdate>(this._baseUrl + 'api/MagRelatedPapersRunList/CreateAutoUpdate',
            magRun)).then(result => {
                if (result.magAutoUpdateId > 0) {
                    this.notificationService.showMAGRunMessage('Search was created');
                } 
                this._MagUpdatesList.push(result);
                this.RemoveBusy("CreateAutoUpdate");

            }, error => {
                this.RemoveBusy("CreateAutoUpdate");
                this.modalService.GenericError(error);
            }).catch((caught) => {
              this.RemoveBusy("CreateAutoUpdate");
              this.modalService.GenericError(caught);
            });
    }
    DeleteMAGAutoUpdate(Id: number) {

        this._BusyMethods.push("DeleteMAGAutoUpdate");
        let body = JSON.stringify({ Value: Id });
        lastValueFrom(this._httpC.post<MagAutoUpdate[]>(this._baseUrl + 'api/MagRelatedPapersRunList/DeleteAutoUpdate',
            body)).then(result => {
                this._MagUpdatesList = result;
                this.notificationService.showMAGRunMessage('MAG Auto update task was deleted');
                //let tmpIndex: number = this.MagAutoUpdatesList.findIndex(x => x.magAutoUpdateId == Number(result.magAutoUpdateId));
                //if (tmpIndex > -1) {
                //    this.MagAutoUpdatesList.splice(tmpIndex, 1);
                //}
                this.RemoveBusy("DeleteMAGAutoUpdate");

            }, error => {
                this.RemoveBusy("DeleteMAGAutoUpdate");
                this.modalService.GenericError(error);
            }).catch((caught) => {
              this.RemoveBusy("DeleteMAGAutoUpdate");
              this.modalService.GenericError(caught);
            });
    }
    DeleteMAGAutoUpdateRun(Id: number) {

        this._BusyMethods.push("DeleteMAGAutoUpdateRun");
        let body = JSON.stringify({ Value: Id });
        lastValueFrom(this._httpC.post<MagAutoUpdateRun[]>(this._baseUrl + 'api/MagRelatedPapersRunList/DeleteAutoUpdateRun',
            body)).then(result => {
                this._MagAutoUpdateRunList = result;
                this.notificationService.showMAGRunMessage('MAG Auto update task results were deleted');
                
                this.RemoveBusy("DeleteMAGAutoUpdateRun");

            }, error => {
                    this.RemoveBusy("DeleteMAGAutoUpdateRun");
                this.modalService.GenericError(error);
            }).catch((caught) => {
              this.RemoveBusy("DeleteMAGAutoUpdateRun");
              this.modalService.GenericError(caught);
            });
    }
    DeleteMAGRelatedRun(Id: number) {

        this._BusyMethods.push("DeleteMAGRelatedRun");
        let body = JSON.stringify({ Value: Id });
        lastValueFrom(this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/DeleteMagRelatedPapersRun',
            body)).then(result => {

                this.RemoveBusy("DeleteMAGRelatedRun");
                if (result.magRelatedRunId > 0) {

                    this.notificationService.showMAGRunMessage('Search was deleted');

                } else {

                    this.notificationService.showMAGRunMessage(result.status);
                }
                let tmpIndex: number = this.MagRelatedPapersRunList.findIndex(x => x.magRelatedRunId == Number(result.magRelatedRunId));
                if (tmpIndex > -1) {
                    this.MagRelatedPapersRunList.splice(tmpIndex, 1);
                }

            }, error => {
                this.RemoveBusy("DeleteMAGRelatedRun");
                this.modalService.GenericError(error);
            }).catch((caught) => {
              this.RemoveBusy("DeleteMAGRelatedRun");
              this.modalService.GenericError(caught);
            });
    }
    CreateMAGRelatedRun(magRun: MagRelatedPapersRun) {
        this._BusyMethods.push("MagRelatedPapersRunCreate");
        lastValueFrom(this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/CreateMagRelatedPapersRun',
            magRun)).then(result => {

                this.RemoveBusy("MagRelatedPapersRunCreate");
                if (result.magRelatedRunId > 0) {

                    this.notificationService.showMAGRunMessage('Search was created');

                } else {

                    this.notificationService.showMAGRunMessage(result.status);
                }
                this.MagRelatedPapersRunList.push(result);
                //this._magBrowserService.FetchMAGRelatedPaperRunsListId(result.magRelatedRunId);

            }, error => {
                this.RemoveBusy("MagRelatedPapersRunCreate");
                this.modalService.GenericError(error);
            }).catch((caught) => {
              this.RemoveBusy("MagRelatedPapersRunCreate");
              this.modalService.GenericError(caught);
            });
    }
  ImportMagRelatedRunPapers(cmd: MagItemPaperInsertCommand, magRelatedRun: MagRelatedPapersRun): Promise<void> {

        let notificationMsg: string = '';
        this._BusyMethods.push("ImportMagRelatedRunPapers");
    return lastValueFrom(this._httpC.post<MagItemPaperInsertCommand>(this._baseUrl + 'api/MagRelatedPapersRunList/ImportMagRelatedPapers',
      cmd)).then(
        result => {
          this.RemoveBusy("ImportMagRelatedRunPapers");
          if (result.nImported != null) {
            if (result.nImported == magRelatedRun.nPapers) {

              notificationMsg += "Imported " + result.nImported + " out of " +
                magRelatedRun.nPapers + " items.";

            } else if (result.nImported != 0) {
              if (cmd.filterDOI == "" && cmd.filterJournal == "" && cmd.filterTitle == "" && cmd.filterURL == "") {
                notificationMsg += "Some of these items were already in your review.\n\nImported " +
                  result.nImported + " out of " + magRelatedRun.nPapers +
                  " new items.";
              }
              else {
                notificationMsg += "Some of these items were already in your review, or were filtered out.\n\nImported " +
                  result.nImported + " out of " + magRelatedRun.nPapers +
                  " new items.";
              }
            }
            else {
              notificationMsg += "All of these records were already in your review.";
            }
            this.notificationService.showMAGRunMessage(notificationMsg);
          }
        },
        error => {
          this.RemoveBusy("ImportMagRelatedRunPapers");
          this.modalService.GenericError(error);
        }).catch(caught => {
          this.RemoveBusy("ImportMagRelatedRunPapers");
          this.modalService.GenericError(caught);
        });
    }
    UpdateMagRelatedRun(magRelatedRun: MagRelatedPapersRun) {

        this._BusyMethods.push("UpdateMagRelatedRun");
        return lastValueFrom(this._httpC.post<MagRelatedPapersRun>(this._baseUrl + 'api/MagRelatedPapersRunList/UpdateMagRelatedRun',
            magRelatedRun)).then(

                (result: MagRelatedPapersRun) => {
                    this.RemoveBusy("UpdateMagRelatedRun");
                    if (result.magRelatedRunId > 0) {
                        let tmpIndex: any = this.MagRelatedPapersRunList.findIndex(x => x.magRelatedRunId == Number(result.magRelatedRunId));
                        if (tmpIndex > -1) {
                           
                            this.MagRelatedPapersRunList[tmpIndex] = result;
                        }
                        this.notificationService.showMAGRunMessage('Search was updated');

                    } else {
                        this.notificationService.showMAGRunMessage('User status is: ' + result.userStatus);
                    }

                }, error => {
                    this.RemoveBusy("UpdateMagRelatedRun");
                    this.modalService.GenericErrorMessage('An api error with calling UpdateMagRelatedRun: ' + error);
                }
        ).catch(caught => {
          this.RemoveBusy("UpdateMagRelatedRun");
          this.modalService.GenericError(caught);
        });
    }
    public GetMagAutoVisualiseList(crit: MagAutoUpdateVisualiseSelectionCriteria) {

        this._BusyMethods.push("GetMagAutoVisualiseList");
        //this._MagAutoUpdateVisualise = [];
        lastValueFrom(this._httpC.post<MagAutoUpdateVisualise[]>(this._baseUrl + 'api/MagRelatedPapersRunList/GetMagMagAutoUpdateVisualise', crit)
        ).then(result => {
                this._MagAutoUpdateVisualise = result;
                this.RemoveBusy("GetMagAutoVisualiseList");
            },
                error => {
                    this._MagAutoUpdateVisualise = [];
                    this.RemoveBusy("GetMagAutoVisualiseList");
                    this.modalService.GenericError(error);
          }).catch(caught => {
            this.RemoveBusy("GetMagAutoVisualiseList");
            this.modalService.GenericError(caught);
          });
    }
    public RunMagAddClassifierScoresCommand(cmd: MagAddClassifierScoresCommand): Promise<boolean> {
        this._BusyMethods.push("RunMagAddClassifierScoresCommand");
      return lastValueFrom(this._httpC.post<MagAddClassifierScoresCommand>(this._baseUrl + 'api/MagRelatedPapersRunList/MagAddClassifierScoresCommand', cmd)
            ).then(
                (result: MagAddClassifierScoresCommand) => {
                    this.RemoveBusy("RunMagAddClassifierScoresCommand");
                    return true;
                },
                error => {
                    this.RemoveBusy("RunMagAddClassifierScoresCommand");
                    this.modalService.GenericError(error);
                    return false;
                }
        ).catch(caught => {
            this.RemoveBusy("RunMagAddClassifierScoresCommand");
            this.modalService.GenericErrorMessage("Sorry, an error occurred: " + caught.toString());
            return false;
        })
    }
    public AutoUpdateCountResultsCommand(cmd: MagItemPaperInsertCommand): Promise<number> {
        this._BusyMethods.push("CountResultsCommand");
      return lastValueFrom(this._httpC.post<MagItemPaperInsertCommand>(this._baseUrl + 'api/MagRelatedPapersRunList/CountResultsCommand', cmd)
            ).then(
                (result: MagItemPaperInsertCommand) => {
                    this.RemoveBusy("CountResultsCommand");
                    return result.topN;
                },
                error => {
                    this.RemoveBusy("CountResultsCommand");
                    this.modalService.GenericError(error);
                    return -1;
                }
            ).catch(caught => {
                this.RemoveBusy("CountResultsCommand");
                this.modalService.GenericErrorMessage("Sorry, an error occurred: " + caught.toString());
                return -1;
            })
    }
    ImportAutoUpdateRun(cmd: MagItemPaperInsertCommand) {

        let notificationMsg: string = '';
        this._BusyMethods.push("ImportAutoUpdateRun");
        lastValueFrom(this._httpC.post<MagItemPaperInsertCommand>(this._baseUrl + 'api/MagRelatedPapersRunList/ImportAutoUpdateRun',
            cmd)).then(result => {
                if (result.nImported != null) {
                    if (result.nImported == cmd.topN) {

                        notificationMsg += "Imported " + result.nImported + " out of " +
                            cmd.topN + " items";

                    } else if (result.nImported != 0) {
                        
                        if (cmd.filterDOI.trim() != "" || cmd.filterJournal.trim() != "" || cmd.filterURL.trim() != "") {
                            notificationMsg += "Some of these items were either already in your review, or were filtered out. Imported " +
                                result.nImported + " out of " + cmd.topN +
                                " new items";
                        } else {
                            notificationMsg += "Some of these items were already in your review. Imported " +
                                result.nImported + " out of " + cmd.topN +
                                " new items";
                        }
                        
                    }
                    else {
                        if (cmd.filterDOI.trim() != "" || cmd.filterJournal.trim() != "" || cmd.filterURL.trim() != "") {
                            notificationMsg += "All of these records were either already in your review, or were filtered out";
                        } else {
                            notificationMsg += "Nothing was imported: all records were already in your review";
                        }
                    }
                    this.notificationService.showMAGRunMessage(notificationMsg);
                }
                this.RemoveBusy("ImportAutoUpdateRun");
            },
                error => {
                    this.RemoveBusy("ImportAutoUpdateRun");
                    this.modalService.GenericError(error);
              }).catch(caught => {
                this.RemoveBusy("ImportAutoUpdateRun");
                this.modalService.GenericError(caught);
              });;
    }
    public Clear(AlsoStopTheTimer: boolean = false) {
        console.log("Clear in MAGRelatedRunsService", AlsoStopTheTimer);
        this._MagRelatedPapersRunList = [];
        this._MagAutoUpdateRunList = [];
        this._MagAutoUpdateVisualise = [];
        this._MagUpdatesList = [];
        if (AlsoStopTheTimer) this.RefreshUntil = new Date();
    }
}
