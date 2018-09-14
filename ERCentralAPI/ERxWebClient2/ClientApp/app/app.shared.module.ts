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
import { WorkAllocationContactListComp } from './components/WorkAllocationContactList/workAllocationContactListComp.component';
import { ItemListComp } from './components/ItemList/itemListComp.component';
import { ItemCodingComp } from './components/coding/coding.component';
import { paginatorComp } from './components/commonComponents/paginator.component';
import { StatusBarComponent } from './components/commonComponents/statusbar.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ItemDocListComp } from './components/ItemDocumentList/itemDocListComp.component';
import { intropageComponent } from './components/intropage/intropage.component';
import { HeaderComponent } from './components/commonComponents/header.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';

@NgModule({
    declarations: [
        AppComponent,
        ReviewSetsComponent,
        FetchReadOnlyReviewsComponent,
        HomeComponent,
        WorkAllocationContactListComp,
        ItemListComp,
        ItemCodingComp,
        paginatorComp,
        StatusBarComponent,
        InfoBoxModalContent,
        ItemDocListComp,
        intropageComponent,
        ConfirmDialogComponent,
        HeaderComponent,
        MainComponent
    ],
    entryComponents: [InfoBoxModalContent, ConfirmDialogComponent],
    imports: [
        CommonModule,
        NgbModule,
        HttpModule,
        FormsModule,
        ReactiveFormsModule,
        TreeModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'reviewsets', component: ReviewSetsComponent },
            { path: 'readonlyreviews', component: FetchReadOnlyReviewsComponent },
            { path: 'main', component: MainComponent }, 
            { path: 'itemcoding/:itemId', component: ItemCodingComp },
            { path: 'WorkAllocationContactListComp', component: WorkAllocationContactListComp },//intropage
            { path: 'intropage', component: intropageComponent },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModuleShared {
}
