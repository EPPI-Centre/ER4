import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppModuleShared } from './app.shared.module';
import { AppComponent } from './components/app/app.component';
import { ReviewerIdentityService } from './components/app/revieweridentity.service';
import { HomeComponent } from './components/home/home.component';
import { HttpClientModule } from '@angular/common/http';

//import { FetchReadOnlyReviewsComponent } from './components/readonlyreviews/readonlyreviews.component';

@NgModule({
    bootstrap: [AppComponent
        
        
    ],
    imports: [
        BrowserModule,
        HttpClientModule,
        AppModuleShared
    ],
    providers: [
        //ReviewerIdentityService ,
        { provide: 'BASE_URL', useFactory: getBaseUrl }
    ]
})
export class AppModule {
    constructor(private ReviewerIdentity: ReviewerIdentityService) { }
}

export function getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
}
