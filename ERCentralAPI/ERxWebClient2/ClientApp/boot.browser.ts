import 'reflect-metadata';
import * as $ from 'jquery';
import 'zone.js';
import 'bootstrap';
import 'angular-tree-component';
import 'angular-datatables';
import 'datatables.net';
import 'datatables.net-dt'
import 'angular-tree-component/dist/angular-tree-component.css';
import { enableProdMode, NgModuleRef, ApplicationRef } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.browser.module';
import { createNewHosts } from '@angularclass/hmr';
import { asap } from 'rxjs/internal/scheduler/asap';
import { destroyView } from '@angular/core/src/view/view';
import { ReviewerIdentityService } from './app/components/services/revieweridentity.service';
//import 'font-awesome/css/font-awesome.css';


console.log('boot.browser');

// Note: @ng-tools/webpack looks for the following expression when performing production
// builds. Don't change how this line looks, otherwise you may break tree-shaking.
//const modulePromise1 = () => modulePromise;

if (module.hot) {
    const modulePromise = platformBrowserDynamic().bootstrapModule(AppModule);
    let ngModule: NgModuleRef<any>;
    module.hot.accept();
    modulePromise.then(mod => {
        ngModule = mod;
        console.log("got the module");
    });
    module.hot.dispose(() => {
        console.log('Hot dispose');

        const appRef: ApplicationRef = ngModule.injector.get(ApplicationRef);
        
        appRef.components[0].injector.get(ReviewerIdentityService).ngOnDestroy();
        const elements = appRef.components.map(c => c.location.nativeElement);
        const makeVisible = createNewHosts(elements);
        ngModule.destroy();
        makeVisible();



        // Before restarting the app, we create a new root element and dispose the old one
        //const oldRootElem = document.querySelector('app');
        //const newRootElem = document.createElement('app');
        //if (oldRootElem && oldRootElem.parentNode) {
        //    console.log("swapping hot elements...", oldRootElem.className, oldRootElem);
        //    oldRootElem!.parentNode!.insertBefore(newRootElem, oldRootElem);
        //    oldRootElem.parentNode.removeChild(oldRootElem);
        //    let handle: any = oldRootElem;
        //    let ref: any = oldRootElem as AppModule;// handle['componentRef'];
        //    if (ref) {
        //        console.log("I have the REF!", ref, ref.className);
        //        //ref.destroy();
        //    }
        //    //const appRef: ApplicationRef = ngModule.injector.get(ApplicationRef);
        //    //let ngMR: NgModuleRef<AppModule> = oldRootElem as NgModuleRef<AppModule>
        //}

        //modulePromise.then(appModule => {
        //    console.log("Self destruct!!!");
        //    //if (oldRootElem) oldRootElem..destroy();
        //    const appRef: ApplicationRef = appModule.injector.get(ApplicationRef);
        //    let i = 0;
        //    while (appRef.components[0].destroyView() > i) {
        //        console.log("destroying: ", appRef.components[i].componentType);
        //        appRef.components[i].destroy();
        //        i++;
        //    }
        //    appModule.destroy();
        //});
    });
} else {
    enableProdMode();
    const modulePromise = platformBrowserDynamic().bootstrapModule(AppModule);
}



//const modulePromise1 = () => platformBrowserDynamic().bootstrapModule(AppModule);;


//if (module.hot) {
//    let ngModule: NgModuleRef<any>;
//    module.hot.accept();
//    modulePromise1().then(mod => {
//        ngModule = mod;
//        ngModule.destroy();
//        console.log("Self destruct!!!");

//    });
    
//    module.hot.dispose(() => {
        
//        //modulePromise1().then(mod => {
//        //    ngModule = mod;
//        //    ngModule.destroy();
//        //    console.log("Self destruct!!!");
            
//        //});
//        console.log('Hot dispose');
//        const appRef: ApplicationRef = ngModule.injector.get(ApplicationRef);
//        const elements = appRef.components.map(c => c.location.nativeElement);
//        const makeVisible = createNewHosts(elements);
//        makeVisible();
//    });
//} else {
//    enableProdMode();
//}
