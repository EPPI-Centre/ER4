import { Component, OnInit, ViewChild} from '@angular/core';
import { searchService } from '../services/search.service';
import { MAGService, MagRelatedPapersRun } from '../services/mag.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGAdvancedService } from '../services/magAdvanced.service';

@Component({
    selector: 'AdvancedMAGFeatures',
    templateUrl: './AdvancedMAGFeatures.component.html',
	providers: []
})

export class AdvancedMAGFeaturesComponent implements OnInit {

	constructor(private ConfirmationDialogService: ConfirmationDialogService,
		private _magAdvancedService: MAGAdvancedService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private router: Router

	) {

	}

	ngOnInit() {

        this.Clear();
    }
    public AdvancedFeatures() {

        //navigate to the relevant page now and page should call the following:
        
        //private void ShowAdvancedPage() {

        //StatusGrid.Visibility = Visibility.Visible;

        //PaperGrid.Visibility = Visibility.Collapsed;

        //TopicsGrid.Visibility = Visibility.Collapsed;

        //PaperListGrid.Visibility = Visibility.Collapsed;

        //HistoryGrid.Visibility = Visibility.Collapsed;

        //RelatedPapersGrid.Visibility = Visibility.Collapsed;

        //AdminGrid.Visibility = Visibility.Collapsed;



        //DataPortal < MagCurrentInfo > dp = new DataPortal<MagCurrentInfo>();

        //MagCurrentInfo mci = new MagCurrentInfo();

        //dp.FetchCompleted += (o, e2) => {

        //    if (e2.Error != null) {

        //        RadWindow.Alert(e2.Error.Message);

        //    }

        //    else {

        //        MagCurrentInfo mci2 = e2.Object as MagCurrentInfo;

        //        if (mci2.CurrentAvailability == "available") {

        //            tbAcademicTitle.Text = "Microsoft Academic dataset last updated: " + mci2.LastUpdated.ToString();

        //        }

        //        else {

        //            tbAcademicTitle.Text = "Microsoft Academic dataset currently unavailable";

        //        }

        //    }

        //};

        //dp.BeginFetch(mci);



        //DataPortal < MAgReviewMagInfoCommand > dp2 = new DataPortal<MAgReviewMagInfoCommand>();

        //MAgReviewMagInfoCommand mrmic = new MAgReviewMagInfoCommand();

        //dp2.ExecuteCompleted += (o, e2) => {

        //    if (e2.Error != null) {

        //        RadWindow.Alert(e2.Error.Message);

        //    }

        //    else {

        //        MAgReviewMagInfoCommand mrmic2 = e2.Object as MAgReviewMagInfoCommand;

        //        //TBNumInReview.Text = mrmic2.NInReviewIncluded.ToString() + " / " + mrmic2.NInReviewExcluded.ToString();

        //        LBListMatchesIncluded.Content = mrmic2.NMatchedAccuratelyIncluded.ToString();

        //        LBListMatchesExcluded.Content = mrmic2.NMatchedAccuratelyExcluded.ToString();

        //        LBListAllInReview.Content = (mrmic2.NMatchedAccuratelyIncluded + mrmic2.NMatchedAccuratelyExcluded).ToString();

        //        LBManualCheckIncluded.Content = mrmic2.NRequiringManualCheckIncluded.ToString();

        //        LBManualCheckExcluded.Content = mrmic2.NRequiringManualCheckExcluded.ToString();

        //        LBMNotMatchedIncluded.Content = mrmic2.NNotMatchedIncluded.ToString();

        //        LBMNotMatchedExcluded.Content = mrmic2.NNotMatchedExcluded.ToString();

        //    }

        //};

        //dp2.BeginExecute(mrmic);



        //CslaDataProvider provider = this.Resources["ClassifierContactModelListData"] as CslaDataProvider;

        //provider.Refresh();

        //CslaDataProvider provider1 = this.Resources["MagSimulationListData"] as CslaDataProvider;

        //provider1.Refresh();

        //}



    }

    public Back() {
        this.router.navigate(['AdvancedMAGFeatures']);
    }

	@ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
	public CurrentDropdownSelectedCode: singleNode | null = null;
	public ItemsWithCode: boolean = false;
	public MAGItems: any[] = [];
	public ShowPanel: boolean = false;
    public isCollapsed: boolean = true;

	CanOnlySelectRoots() {
		return true;
	}
	CloseCodeDropDown() {
		if (this.WithOrWithoutCodeSelector) {
			this.CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData;
		}
		this.isCollapsed = false;
    }
	public desc: string = '';
	public value: Date = new Date(2000, 2, 10);
	public searchAll: string = 'true';
	public magDate: string = 'true';
	public magSearchCheck: boolean = false;
	public magDateRadio: boolean = false;
    public magRCTRadio: string = 'NoFilter';
	public magMode: string = '';
	public ToggleMAGPanel(): void {
		this.ShowPanel = !this.ShowPanel;
	}
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }

	public IsServiceBusy(): boolean {

		return false;
	}
	public Selected(): void {

	}
    Clear() {

        this.CurrentDropdownSelectedCode = {} as SetAttribute;
        this.desc = '';
        this.ItemsWithCode = false;
        this.magDate = '';
        this.magMode = '';

    }
    public CanDeleteMAGRun() : boolean {

        return this.HasWriteRights;
    }

    public CanAddNewMAGSearch(): boolean {

        if (this.desc != '' && this.desc != null && this.HasWriteRights
            ) {
            return true;
        } else {
            return false;
        }
    }

	public ClickSearchMode(searchModeChoice: string) {

		switch (searchModeChoice) {

            case '1':
                this.magMode = 'Recommended by';
                break;
            case '2':
                this.magMode = 'That recommend';
                break;
            case '3':
                this.magMode = 'Recommendations';
                break;
            case '4':
                this.magMode = 'Bibliography';
                break;
            case '5':
                this.magMode = 'Cited by';
                break;
            case '6':
                this.magMode = 'Bi-Citation';
                break;
            case '7':
                this.magMode = 'Bi-Citation AND Recommendations';
                break;

            default:
                break;
		}
	}
    
	public GetMAGCurrentInfo() {

        this._magAdvancedService.FetchCurrentInfo();
	}

    public GetMagSimulationList() {

        this._magAdvancedService.FetchMagSimulationList();

    }
}
	