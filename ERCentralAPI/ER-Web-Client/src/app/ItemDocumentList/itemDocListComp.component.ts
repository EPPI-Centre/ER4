import { Component, OnInit, Input, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemDocsService, ItemDocument } from '../services/itemdocs.service';
import { FileRestrictions, SelectEvent, ClearEvent, UploadEvent, RemoveEvent } from '@progress/kendo-angular-upload';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ConfigService } from '../services/config.service';


@Component({
    selector: 'ItemDocListComp',
    templateUrl: './ItemDocListComp.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class ItemDocListComp implements OnInit, OnDestroy {

  constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
    private ItemCodingService: ItemCodingService,
    public ItemDocsService: ItemDocsService,
    private confirmationDialogService: ConfirmationDialogService,
    configService: ConfigService

  ) {
    this._baseUrl = configService.baseUrl;
    this.uploadSaveUrl = this._baseUrl + 'api/ItemDocumentList/Upload';
  }
  private _baseUrl: string = "";
  public me: string = "I don't know";
  public ShowUpload: boolean = false;
  @Input() itemID: number = 0;
  @Input() showUpload: boolean = true;
  @Input() ShowViewButton: boolean = true;
  public get HasWriteRights(): boolean {
    return this.itemID != 0 && this.ReviewerIdentityServ.HasWriteRights;
  }
  public uploadRestrictions: FileRestrictions = {
    allowedExtensions: ['.txt'
      , '.doc'
      , '.docx'
      , '.pdf'
      , '.ppt'
      , '.pps'
      , '.pptx'
      , '.ppsx'
      , '.xls'
      , '.xlsx'
      , '.htm'
      , '.html'
      , '.odt'
      , '.ods'
      , '.odp'
      , '.ps'
      , '.eps'
      , '.csv']
  };
  Restrictions: FileRestrictions = {
    maxFileSize: 15000000
  };
  ngOnInit() {
    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {

      this.router.navigate(['home']);

    }
    else {

    }
  }
  OpenFirstPDF() {
    let ind = this.ItemDocsService._itemDocs.findIndex((found) => found.extension.toLowerCase() == ".pdf");
    if (ind > -1) {
      this.DownloadDoc(this.ItemDocsService._itemDocs[ind].itemDocumentId, true);
    }
  }
  DownloadDoc(itemDocumentId: number, ForView: boolean) {
    this.ItemDocsService.GetItemDocument(itemDocumentId, ForView);
  }
  public uploadSaveUrl = this._baseUrl + 'api/ItemDocumentList/Upload'; // 


  public completeEventHandler() {
    this.ShowUpload = false;
    this.ItemDocsService.Refresh();
    //this.log(`All files processed`);
  }

  public removeEventHandler(e: RemoveEvent): void {
    //this.log(`Removing ${e.files[0].name}`);
  }

  public selectEventHandler(e: SelectEvent): void {
    const that = this;

    e.files.forEach((file) => {
      //that.log(`File selected: ${file.name}`);

      if (!file.validationErrors) {
        const reader = new FileReader();

        reader.onload = function (ev) {
          //const image = {
          //    src: ev.target.result,
          //    uid: file.uid
          //};

          //that.imagePreviews.unshift(image);
        };

        reader.readAsDataURL(file.rawFile as Blob);
      }
    });
  }
  uploadEventHandler(e: UploadEvent) {
    e.data = {
      itemID: this.itemID
    };
  }
  public DeleteDoc(Doc: ItemDocument) {
    this.ItemDocsService.DeleteDocWarning(Doc.itemDocumentId, this.itemID).then(
      //to check for coding that will disappear, we pass the current item ID
      //errors are handled within the service (will return -1 if anything went wrong...)
      (result) => {
        if (result >= 0) this.DoDeleteDoc(Doc, result);
      }
    );
  }
  private DoDeleteDoc(Doc: ItemDocument, Numcodings: number) {
    if (Doc.itemId > 0) {
      //to actually delete a doc, it's easier on the SQL side to provide the itemId on which this doc has been uploaded.
      //The itemId can be different from the current item, when we're dealing with linked items.
      if (Numcodings > 0) {
        this.confirmationDialogService.confirm('Please confirm', 'Deleting a Document is a permanent operation.' +
          '<br/><b>This document has been coded ' + Numcodings + ' time(s)</b>.<br/> All codings associated with this document will be premanently deleted!', false, '')
          .then(
            (confirmed: any) => {
              if (confirmed) {
                this.ItemDocsService.DeleteItemDoc(Doc.itemDocumentId, Doc.itemId);
              } else {
                console.log('User cancelled the confirm delete doc dialog');
              }
            }
          )
          .catch(() => console.log('User dismissed the confirm delete doc dialog'));
      }
      else {
        this.confirmationDialogService.confirm('Please confirm', 'Deleting a Document is a permanent operation.' +
          ' This document does not appear to have been coded.', false, '')
          .then(
            (confirmed: any) => {
              console.log('User confirmed delete doc dialog');
              if (confirmed) {
                this.ItemDocsService.DeleteItemDoc(Doc.itemDocumentId, Doc.itemId);
              } else {
                //alert('did not confirm');
              }
            }
          )
          .catch(() => console.log('User dismissed the confirm delete doc dialog'));
      }
    }
  }

  ngOnDestroy() {
  }

}







