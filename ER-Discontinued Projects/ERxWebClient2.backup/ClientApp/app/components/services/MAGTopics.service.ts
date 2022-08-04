import { Injectable, Inject } from "@angular/core";
import { ModalService } from "./modal.service";
import { HttpClient } from "@angular/common/http";
import { BusyAwareService } from "../helpers/BusyAwareService";
import { MVCMagFieldOfStudyListSelectionCriteria, MagFieldOfStudy, TopicLink } from "./MAGClasses.service";

@Injectable({

    providedIn: 'root',

})
export class MAGTopicsService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }        
    public ShowingParentAndChildTopics: boolean = true;
    public ShowingChildTopicsOnly: boolean = false;
    public WPParentTopics: TopicLink[] = [];
    public WPChildTopics: TopicLink[] = [];
    public FetchMagFieldOfStudyList(criteria: MVCMagFieldOfStudyListSelectionCriteria, goBackListType: string): Promise<MagFieldOfStudy[] | boolean> {
        this._BusyMethods.push("FetchMagFieldOfStudyList");

        return this._httpC.post<MagFieldOfStudy[]>(this._baseUrl + 'api/MagFieldOfStudyList/GetMagFieldOfStudyList', criteria)
            .toPromise().then(
                (result: MagFieldOfStudy[]) => {
                    if (criteria.listType == "FieldOfStudyChildrenList") {
                        this.WPChildTopics = [];
                    }
                    else if (criteria.listType == "FieldOfStudyParentsList") {
                        this.WPParentTopics = [];
                    }
                    else {
                        this.WPParentTopics = [];
                        this.WPChildTopics = [];
                    }
                    if (result != null) {

                        let FosList: MagFieldOfStudy[] = result;
                        let i: number = 1.1;
                        let j: number = 1.1;
                        for (var fos of FosList) {

                            let item: TopicLink = new TopicLink();
                            item.displayName = fos.displayName;
                            item.fieldOfStudyId = fos.fieldOfStudyId;

                            if (criteria.listType == 'FieldOfStudyParentsList') {
                                if (i > 0.1) {
                                    i -= 0.004;
                                }
                                item.fontSize = i;
                                this.WPParentTopics.push(item);


                            } else {
                                if (j > 0.1) {
                                    j -= 0.004;
                                }
                                item.fontSize = j;
                                this.WPChildTopics.push(item);

                            }
                        }
                    }
                    this.RemoveBusy("FetchMagFieldOfStudyList");
                    //this.ListCriteria.listType = goBackListType;
                    return result;
                },
                error => {
                    this.RemoveBusy("FetchMagFieldOfStudyList");
                    this.modalService.GenericError(error);
                    return false;// error;
                }
            ).catch(
                (error) => {

                    this.modalService.GenericErrorMessage("error with FetchMagFieldOfStudyList: " + error);
                    this.RemoveBusy("FetchMagFieldOfStudyList");
                    return false;// error;
                });
    }


   
}