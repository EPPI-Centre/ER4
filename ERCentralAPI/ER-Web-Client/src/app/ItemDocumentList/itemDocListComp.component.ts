import { Component, Inject, OnInit, EventEmitter, Output, Input, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subscribable, Subscription } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { FileRestrictions, SelectEvent, ClearEvent, UploadEvent, RemoveEvent, FileInfo } from '@progress/kendo-angular-upload';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ConfigService } from '../services/config.service';


@Component({
    selector: 'ItemDocListComp',
    templateUrl: './ItemDocListComp.component.html',
    providers: []
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
    public sub: Subscription | null = null;
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
    public DeleteDoc(DocId: number) {
        this.ItemDocsService.DeleteDocWarning(DocId).then(
            //errors are handled within the service (will return -1 if anything went wrong...)
            (result) => {
                if (result >= 0) this.DoDeleteDoc(DocId, result);
            }
        );
    }
    private DoDeleteDoc(DocId: number, Numcodings: number) {
        if (Numcodings > 0){
            this.confirmationDialogService.confirm('Please confirm', 'Deleting a Document is a permanent operation.' +
                '<br/><b>This document has been coded ' + Numcodings + ' time(s)</b>.<br/> All codings associated with this document will be premanently deleted!', false, '')
                .then(
                    (confirmed: any) => {
                        if (confirmed) {
                            this.ItemDocsService.DeleteItemDoc(DocId);
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
                            this.ItemDocsService.DeleteItemDoc(DocId);
                        } else {
                            //alert('did not confirm');
                        }
                    }
                )
                .catch(() => console.log('User dismissed the confirm delete doc dialog'));
        }
    }

    ngOnDestroy() {

        if (this.sub) this.sub.unsubscribe();
    }

}



export class ItemDocumentList {

    ItemDocuments: ItemDocument[] = [];
}


export class ItemDocument {

    public itemDocumentId: number = 0;
    public itemId: number = 0;
    public shortTitle: string = '';
    public extension: string = '';
    public title: string = '';
    public text: string = "";
    public binaryExists: boolean = false;
    public textFrom: number = 0;
    public textTo: number = 0;
    public freeNotesStream: string = "";
    public freeNotesXML: string = '';
    public isBusy: boolean = false;
    public isChild: boolean = false;
    public isDeleted: boolean = false;
    public isDirty: boolean = false;
    public isNew: boolean = false;
    public isSavable: boolean = false;
    public isSelfBusy: boolean = false;
    public isSelfDirty: boolean = false;
    public isSelfValid: boolean = false;
    public isValid: boolean = false;

}



