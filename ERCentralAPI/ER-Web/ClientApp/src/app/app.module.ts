import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule, RouteReuseStrategy } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

//import { AngularFontAwesomeModule } from 'angular-font-awesome';
//import { DataTablesModule } from 'angular-datatables';
//import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { NotificationModule } from '@progress/kendo-angular-notification';
import { ChartModule } from '@progress/kendo-angular-charts';
import { ToolBarModule } from '@progress/kendo-angular-toolbar';
import { DialogsModule } from '@progress/kendo-angular-dialog';
import { UploadModule } from '@progress/kendo-angular-upload';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { DatePickerModule } from '@progress/kendo-angular-dateinputs';
import { GridModule, GridComponent } from '@progress/kendo-angular-grid';

import { NgChartsModule } from 'ng2-charts';
import { TreeModule } from 'angular-tree-component';
import { CKEditorModule } from 'ckeditor4-angular';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { CodesetTreeCodingComponent, InfoBoxModalContent } from './CodesetTrees/codesetTreeCoding.component';
import { FetchReadOnlyReviewsComponent } from './readonlyreviews/readonlyreviews.component';
import { MainComponent } from './main/main.component';
import { MainFullReviewComponent } from './main/mainfull.component';
import { ItemListComp } from './ItemList/itemListComp.component';
import { ItemCodingComp } from './coding/coding.component';
import { paginatorComp } from './commonComponents/paginator.component';
import { StatusBarComponent } from './commonComponents/statusbar.component';
import { ItemDocListComp } from './ItemDocumentList/itemDocListComp.component';
import { intropageComponent } from './intropage/intropage.component';
import { HeaderComponent } from './commonComponents/header.component';
import { ModalDialogComponent } from './ModalDialog/ModalDialog.component';
import { armsComp } from './ArmsAndTimePoints/armsComp.component';
import { ItemCodingFullComp } from './coding/codingFull.component';
import { itemDetailsComp } from './itemDetails/itemDetails.component';
import { ReviewStatisticsComp } from './reviewStatistics/reviewstatistics.component';
import { itemDetailsPaginatorComp } from './ItemDetailsPaginator/itemDetailsPaginator.component';
import { frequenciesResultsComp } from './Frequencies/frequenciesResults.component';
import { EventEmitterService } from './services/EventEmitter.service';
import { CrossTabsComp } from './CrossTabs/crosstab.component';
import { SearchComp } from './Search/SearchComp.component';
import { frequenciesComp } from './Frequencies/frequencies.component';
import { CustomRouteReuseStrategy } from './helpers/CustomRouteReuseStrategy';
import { ImportReferencesFileComponent } from './Sources/importreferencesfile.component';
import { ROSourcesListComponent } from './Sources/ROSourcesList.component';
import { SourcesComponent } from './Sources/sources.component';
import { PubMedComponent } from './Sources/PubMed.component';
import { ReviewSetsEditorComponent } from './CodesetTrees/reviewSetsEditor.component';
import { CodesetTreeMainComponent } from './CodesetTrees/codesetTreeMain.component';
import { CodesetTreeEditComponent } from './CodesetTrees/codesetTreeEdit.component';
import { BuildModelComponent } from './BuildModel/buildmodel.component';
import { codesetSelectorComponent } from './CodesetTrees/codesetSelector.component';
import { ImportCodesetsWizardComponent } from './CodesetTrees/importCodesetsWizard.component';
import { codesetTree4CopyComponent } from './CodesetTrees/codesetTree4Copy.component';
import { ConfirmationDialogComponent } from './ConfirmationDialog/confirmation-dialog.component';
import { QuickCodingReportComponent } from './Reports/quickcodingreport.component';
import { NewReviewComponent } from './Review/newreview.component';
import { EditAccountComponent } from './Reviewer/editAccount.component';
import { EditReviewComponent } from './Review/editReview.component';
import { RunLingo3G } from './CodesetTrees/runlingo3g.component';
import { armDetailsComp } from './ArmsAndTimePoints/armDetailsComp.component';
import { CodesetTree4QuickQuestionReportComponent } from './CodesetTrees/codesetTree4QuickQuestionReport.component';
import { WorkAllocationComp } from './WorkAllocations/WorkAllocationComp.component';
import { SiteAdminComponent } from './SiteAdmin/siteadmin.component';
import { WorkAllocationContactListComp } from './WorkAllocations/WorkAllocationContactListComp.component';
import { SiteAdminEntryComponent } from './SiteAdmin/siteadminEntry.component';
import { editItemDetailsComp } from './itemDetails/editItemDetails.component';
import { CreateNewCodeComp } from './CodesetTrees/createnewcode.component';
import { ComparisonComp } from './Comparison/createnewcomparison.component';
import { ComparisonStatsComp } from './Comparison/comparisonstatistics.component';
import { ComparisonReconciliationComp } from './Comparison/comparisonreconciliation.component';
import { ArchieCallBackComponent } from './home/ArchieCallBack.component ';
import { WebViewerComponent } from './PDFTron/webviewer.component';
import { PdfTronContainer } from './PDFTron/pdftroncontainer.component';
import { codingRecordComp } from './CodingRecord/codingRecord.component';
import { LiveComparisonComp } from './CodingRecord/LiveComparison.component';
import { timePointsComp } from './ArmsAndTimePoints/timePointsComp.component';
import { OutcomesComponent } from './Outcomes/outcomes.component'
import { EditCodeComp } from './CodesetTrees/editcode.component';
import { SingleCodesetTreeCodingComponent } from './CodesetTrees/SingleCodesetTreeCoding.component';
import { TextSelectDirective } from './helpers/text-select.directive';
import { ReviewTermsListComp } from './ReviewTermsList/ReviewTermsListComp.component';
import { configurablereportComp } from './Reports/configurablereport.component';
import { DuplicatesComponent } from './Duplicates/duplicates.component';
import { codesetTree4Move } from './CodesetTrees/codesetTree4Move.component';
import { BasicMAGComp } from './MAG/BasicMAGComp.component';
import { AdvancedMAGFeaturesComponent } from './MAG/AdvancedMAGFeatures.component';
import { MAGpaginatorComp } from './commonComponents/MAGpaginator.component';
import { MAGBrowser } from './MAG/MAGBrowser.component';
import { MAGBrowserHistory } from './MAG/MAGBrowserHistory.component';
import { MAGAdminComp } from './MAG/MAGAdmin.component';
import { MatchingMAGItemsComponent } from './MAG/MatchingMAGItems.component';
import { WorkAllocationWizardComp } from './WorkAllocations/WorkAllocationWizardComp.component';
import { microsoftAcademicComp } from './MAG/microsoftAcademic.component';
import { MAGHeaderBar2Comp } from './commonComponents/MAGHeaderBar2.component';
import { ScreeningSetupComp } from './WorkAllocations/ScreeningSetup.component';
import { MAGSearchComponent } from './MAG/MAGSearch.component';
import { WebDBsComponent } from './Review/WebDBs.component';
import { TruncatePipe } from './MAG/TruncatePipe.component';
import { MAGSearchDetailsComponent } from './MAG/MAGSearchDetails.component';
import { WebDbCcodesetTreeComponent } from './CodesetTrees/WebDbCcodesetTree.component';
import { MAGKeepUpToDate } from './MAG/MAGKeepUpToDate.component';
import { MAGComp } from './MAG/MAG.component';
import { ItemLinksComp } from './ArmsAndTimePoints/ItemLinks.component';
import { SetupConfigurableReports } from './Reports/SetupConfigurableReports.component';
import { FreqXtabMapsComp } from './Frequencies/FreqXtabMaps.component';
import { ReconcilingCodesetTreeComponent } from './CodesetTrees/ReconcilingCodesetTree.component';
import { SourcesListSearchesComponent } from './Sources/SourcesListSearches.component';
import { VisLogComp } from './Review/VisLog.component';
import { DatePipe } from '@angular/common';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';


@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    SearchComp,
    BasicMAGComp,
    frequenciesComp,
    frequenciesResultsComp,
    ReviewSetsEditorComponent,
    CrossTabsComp,
    ReviewStatisticsComp,
    itemDetailsPaginatorComp,
    CodesetTreeMainComponent,
    CodesetTreeCodingComponent,
    CodesetTreeEditComponent,
    ImportCodesetsWizardComponent,
    codesetTree4CopyComponent,
    codesetSelectorComponent,
    armsComp,
    armDetailsComp,
    FetchReadOnlyReviewsComponent,
    ArchieCallBackComponent,
    ItemListComp,
    ItemCodingComp,
    ItemCodingFullComp,
    itemDetailsComp,
    editItemDetailsComp,
    paginatorComp,
    StatusBarComponent,
    InfoBoxModalContent,
    ItemDocListComp,
    LiveComparisonComp,
    SourcesComponent,
    BuildModelComponent,
    ImportReferencesFileComponent,
    PubMedComponent,
    intropageComponent,
    ModalDialogComponent,
    HeaderComponent,
    ROSourcesListComponent,
    SourcesListSearchesComponent,
    MainFullReviewComponent,
    MainComponent,
    ConfirmationDialogComponent,
    QuickCodingReportComponent,
    CodesetTree4QuickQuestionReportComponent,
    NewReviewComponent,
    EditAccountComponent,
    EditReviewComponent,
    RunLingo3G,
    WorkAllocationComp,
    WorkAllocationContactListComp,
    SiteAdminComponent,
    SiteAdminEntryComponent,
    CreateNewCodeComp,
    ComparisonComp,
    ComparisonStatsComp,
    ComparisonReconciliationComp,
    timePointsComp,
    PdfTronContainer,
    WebViewerComponent,
    codingRecordComp,
    OutcomesComponent,
    EditCodeComp,
    SingleCodesetTreeCodingComponent,
    TextSelectDirective,
    ReviewTermsListComp,
    configurablereportComp,
    DuplicatesComponent,
    codesetTree4Move,
    AdvancedMAGFeaturesComponent,
    MAGpaginatorComp,
    MAGBrowser,
    MAGBrowserHistory,
    MAGAdminComp,
    MatchingMAGItemsComponent,
    MAGAdminComp,
    MAGKeepUpToDate,
    WorkAllocationWizardComp,
    microsoftAcademicComp,
    MAGHeaderBar2Comp,
    MAGComp,
    ScreeningSetupComp,
    MAGSearchComponent,
    TruncatePipe,
    MAGSearchDetailsComponent,
    WebDBsComponent,
    WebDbCcodesetTreeComponent,
    ItemLinksComp,
    SetupConfigurableReports,
    FreqXtabMapsComp,
    ReconcilingCodesetTreeComponent,
    VisLogComp
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    //AngularFontAwesomeModule,
    //DataTablesModule,
    CommonModule,
    //NgbModule,
    HttpClientModule,
    FormsModule,
    BrowserModule,
    BrowserAnimationsModule,
    NgChartsModule,
    ReactiveFormsModule,
    TreeModule,
    GridModule,
    ChartModule,
    DialogsModule,
    ToolBarModule,
    InputsModule,
    UploadModule,
    ButtonsModule,
    NotificationModule,
    DatePickerModule,
    LayoutModule,
    CKEditorModule,
    RouterModule.forRoot([
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: HomeComponent },
      { path: 'ArchieCallBack', component: ArchieCallBackComponent },
      { path: 'readonlyreviews', component: FetchReadOnlyReviewsComponent },
      { path: 'Main', component: MainFullReviewComponent },
      { path: 'MainCodingOnly', component: MainComponent },
      { path: 'sources', component: SourcesComponent },
      { path: 'BuildModel', component: BuildModelComponent },
      { path: 'BasicMAGFeatures', component: BasicMAGComp },
      { path: 'ItemList', component: ItemListComp },
      { path: 'MAGAdmin', component: MAGAdminComp },
      { path: 'MAG', component: MAGComp },
      { path: 'MAG/:paperId', component: MAGComp },
      { path: 'MAGBrowser', component: MAGBrowser },
      { path: 'MagSearch', component: MAGSearchComponent },
      { path: 'MAGKeepUpToDate', component: MAGKeepUpToDate },
      { path: 'microsoftAcademic', component: microsoftAcademicComp },
      { path: 'MAGBrowserHistory', component: MAGBrowserHistory },
      { path: 'MatchingMAGItems', component: MatchingMAGItemsComponent },
      { path: 'AdvancedMAGFeatures', component: AdvancedMAGFeaturesComponent },
      { path: 'itemcodingOnly/:itemId', component: ItemCodingComp },
      { path: 'itemcoding/:itemId', component: ItemCodingFullComp },
      { path: 'EditItem/:itemId', component: editItemDetailsComp },
      { path: 'EditItem', component: editItemDetailsComp },
      { path: 'EditCodeSets', component: ReviewSetsEditorComponent },
      { path: 'Reconciliation', component: ComparisonReconciliationComp },
      { path: 'ImportCodesets', component: ImportCodesetsWizardComponent },
      { path: 'intropage', component: intropageComponent },
      { path: 'Duplicates', component: DuplicatesComponent },
      { path: 'SiteAdmin', component: SiteAdminComponent },
      { path: 'WebDBs', component: WebDBsComponent },
      { path: '**', redirectTo: 'home' }
    ]),
    ButtonsModule,
    BrowserAnimationsModule,
    NgbModule
  ],
  providers: [DatePipe,
    EventEmitterService,
    {
      provide: RouteReuseStrategy, useClass: CustomRouteReuseStrategy
    }],
  entryComponents: [InfoBoxModalContent, ModalDialogComponent, ConfirmationDialogComponent],
  bootstrap: [AppComponent]
})
export class AppModule { }
