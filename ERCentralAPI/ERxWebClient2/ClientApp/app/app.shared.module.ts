import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { TreeModule } from 'angular-tree-component';
import { AppComponent } from './components/app/app.component';
import { HomeComponent } from './components/home/home.component';
import { ReviewSetsComponent, InfoBoxModalContent } from './components/reviewsets/reviewsets.component';
import { FetchReadOnlyReviewsComponent } from './components/readonlyreviews/readonlyreviews.component';
import { MainComponent } from './components/main/main.component';
import { MainFullReviewComponent, SearchesModalContent } from './components/mainfull/mainfull.component';
import { WorkAllocationContactListComp } from './components/WorkAllocationContactList/workAllocationContactListComp.component';
import { ItemListComp } from './components/ItemList/itemListComp.component';
import { ItemCodingComp } from './components/coding/coding.component';
import { paginatorComp } from './components/commonComponents/paginator.component';
import { StatusBarComponent } from './components/commonComponents/statusbar.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ItemDocListComp } from './components/ItemDocumentList/itemDocListComp.component';
import { intropageComponent } from './components/intropage/intropage.component';
import { HeaderComponent } from './components/commonComponents/header.component';
import { ModalDialogComponent } from './components/ModalDialog/ModalDialog.component';
import { armsComp } from './components/arms/armsComp.component';
import { AngularFontAwesomeModule } from 'angular-font-awesome';
import { DataTablesModule } from 'angular-datatables';
import { ItemCodingFullComp } from './components/coding/codingFull.component';
import { itemDetailsComp } from './components/itemDetails/itemDetails.component';
import { ReviewStatisticsComp } from './components/reviewStatistics/reviewstatistics.component';
import { itemDetailsPaginatorComp } from './components/ItemDetailsPaginator/itemDetailsPaginator.component';
import { CodesetTreeComponent } from './components/CodesetTree/codesets.component';
import { frequenciesComp } from './components/Frequencies/frequencies.component';
import { EventEmitterService } from './components/services/EventEmitter.service';
import { WebApiObservableService } from './components/services/httpQuery.service';
import { CrossTabsComp } from './components/CrossTabs/crosstab.component';
import { ChartsModule } from 'ng2-charts'
import { SearchComp } from './components/Search/SearchComp.component';
import { MatTableModule, MatSortModule } from '@angular/material';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';


@NgModule({
    declarations: [
		AppComponent,
		CodesetTreeComponent,
		SearchComp,
		frequenciesComp,
		CrossTabsComp,
		ReviewStatisticsComp,
		itemDetailsPaginatorComp,
        ReviewSetsComponent,
        armsComp,
        FetchReadOnlyReviewsComponent,
        HomeComponent,
        WorkAllocationContactListComp,
        ItemListComp,
        ItemCodingComp,
        ItemCodingFullComp,
        itemDetailsComp,
        paginatorComp,
        StatusBarComponent,
		InfoBoxModalContent,
		SearchesModalContent,
        ItemDocListComp,
        intropageComponent,
        ModalDialogComponent,
        HeaderComponent,
        MainFullReviewComponent,
        MainComponent
	],
	providers: [EventEmitterService, WebApiObservableService ],
    entryComponents: [InfoBoxModalContent, ModalDialogComponent, SearchesModalContent],
    imports: [
        AngularFontAwesomeModule,
		DataTablesModule,
        CommonModule,
        NgbModule,
        HttpModule,
		FormsModule,
		BrowserModule,
		BrowserAnimationsModule,
		MatTableModule,
		MatSortModule,
		ChartsModule,
        ReactiveFormsModule,
        TreeModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'reviewsets', component: ReviewSetsComponent },
            { path: 'readonlyreviews', component: FetchReadOnlyReviewsComponent },
            { path: 'mainFullReview', component: MainFullReviewComponent }, 
            { path: 'main', component: MainComponent }, 
            { path: 'itemcodingOnly/:itemId', component: ItemCodingComp },
            { path: 'itemcoding/:itemId', component: ItemCodingFullComp },
            { path: 'WorkAllocationContactListComp', component: WorkAllocationContactListComp },//intropage
            { path: 'intropage', component: intropageComponent },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModuleShared {
}
