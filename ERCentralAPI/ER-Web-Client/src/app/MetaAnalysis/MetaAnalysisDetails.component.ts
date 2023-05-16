import { Component, OnInit,Input, ViewChild, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Item, ItemListService, iAdditionalItemDetails } from '../services/ItemList.service';
import { ReviewerTermsService, ReviewerTerm } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ModalService } from '../services/modal.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { TextSelectEvent } from "../helpers/text-select.directive";
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReviewerIdentityService, PersistingOptions } from '../services/revieweridentity.service';
import { Subscription } from 'rxjs';
import { ItemDocListComp } from '../ItemDocumentList/itemDocListComp.component';
import { faBook } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'MetaAnalysisDetailsComp',
  templateUrl: './MetaAnalysisDetails.component.html',
    providers: [],
    styles: []
})
export class MetaAnalysisDetailsComp implements OnInit, OnDestroy {

  constructor(
    //list of services, etc.
  ) { }


  ngOnInit() { }


  ngOnDestroy() { }
}






