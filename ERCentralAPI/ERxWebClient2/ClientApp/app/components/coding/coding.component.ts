import { Component, Inject, OnInit, EventEmitter, Output, OnDestroy, Input, ViewChild, ElementRef, AfterViewInit, HostListener } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription, Subject, Subscribable, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationList.service';
import { ItemListService, Criteria, Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet, ReadOnlyItemAttribute } from '../services/ItemCoding.service';
import { ReviewSetsService, ItemAttributeSaveCommand, SetAttribute } from '../services/ReviewSets.service';
import { CodesetTreeCodingComponent, CheckBoxClickedEventData } from '../CodesetTrees/codesetTreeCoding.component';
import { ReviewInfo, ReviewInfoService } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ReviewerTermsService } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ArmsService } from '../services/arms.service';
import { armsComp } from '../ArmsAndTimePoints/armsComp.component';
import { NotificationService } from '@progress/kendo-angular-notification';
import { SelectEvent, TabStripComponent } from '@progress/kendo-angular-layout';
import { PdfTronContainer } from '../PDFTron/pdftroncontainer.component';
import { Helpers } from '../helpers/HelperMethods';
import { OutcomesComponent } from '../Outcomes/outcomes.component';
import { OutcomesService } from '../services/outcomes.service';


@Component({
   
    selector: 'itemcoding',
    templateUrl: './coding.component.html',
    //providers: [ReviewerIdentityService]
    providers: [],
    styles: [`
                button.disabled {
                    color:black; 
                    }

                .vertical-text {
                    position: fixed;
                    top: 50%;
                    z-index:2002;
                    transform: rotate(90deg);
                    left: -23px;
                    float: left;
                }
                .vertical-text-R {
                    position: fixed;
                    top: 50%;
                    z-index:2002;
                    transform: rotate(90deg);
                    right: -18px;
                    float: left;
                }
                .codesInSmallScreen {
                 position:absolute; 
                 left: 0; z-index:2000;
                  top: 106px;
                transition: transform 0.31s;
                transform-origin:left;
                }
                .codesInSmallScreen.hide {
                  transform:scaleX(0);
                }
                .codesInSmallScreen.show {
                  width:99.5%;
                  transform:scaleX(1);
                }  
               
            `]

})
export class ItemCodingComp implements OnInit, OnDestroy, AfterViewInit {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService
        , private route: ActivatedRoute, private ItemCodingService: ItemCodingService,
        private ReviewSetsService: ReviewSetsService,
        private reviewInfoService: ReviewInfoService,
        public PriorityScreeningService: PriorityScreeningService
        , private ReviewerTermsService: ReviewerTermsService,
        public ItemDocsService: ItemDocsService,
        private armservice: ArmsService,
		private notificationService: NotificationService,
		private _outcomeService: OutcomesService,
		private _ItemCodingService: ItemCodingService
    ) { }
//     .codesInSmallScreen.collapse{
//    display: block!important;
//    transition: all .25s ease -in -out;
//}

//                .codesInSmallScreen.collapse {
//    opacity: 0;
//    height: 0;
//}

//                .codesInSmallScreen.collapse.show {
//    opacity: 1;
//    height: 100 %;
//}
	@ViewChild('OutcomesCmp')
	private OutcomesCmpRef!: OutcomesComponent;
    @ViewChild('cmp') private ArmsCompRef!: any;
    @ViewChild('pdftroncontainer') private pdftroncontainer!: PdfTronContainer;

    private subItemIDinPath: Subscription | null = null;
    private subCodingCheckBoxClickedEvent: Subscription | null = null;
    private ItemCodingServiceDataChanged: Subscription | null = null;
    private ItemArmsDataChanged: Subscription | null = null;
    private subGotPDFforViewing: Subscription | null = null;
    public get itemID(): number {
        if (this.item) return this.item.itemId;
        else return -1;
    }
    private itemString: string = '0';
    public item?: Item;
	public ShowOutComes: boolean = false;
    private subGotScreeningItem: Subscription | null = null;
    public IsScreening: boolean = false;
    public ShowHighlights: boolean = false;
    public HAbstract: string = "";
    public HTitle: string = "";
    public HelpAndFeebackContext: string = "(codingui)itemdetails";
    @ViewChild('ItemDetailsCmp') private ItemDetailsCompRef!: any;
    @ViewChild('codesetTreeCoding') public codesetTreeCoding!: CodesetTreeCodingComponent;
    @ViewChild('tabstripCoding') public tabstrip!: TabStripComponent;
    

    public innerWidth: any = 900;
    @HostListener('window:resize', ['$event'])
    onResize(event: any) {
        this.innerWidth = window.innerWidth;
    }
    IsSmallScreen(): boolean {
        if (this.innerWidth && this.innerWidth < 768) {
            return true;
        }
        else return false;
    }
    public ShowCodesInSmallScreen: boolean = false;
    public ShowHideCodes() {
        this.ShowCodesInSmallScreen = !this.ShowCodesInSmallScreen;
    }
    public get HasTermList(): boolean {
        if (!this.ReviewerTermsService || !this.ReviewerTermsService.TermsList || !(this.ReviewerTermsService.TermsList.length > 0)) return false;
        else return true;
    }
    ngAfterViewInit() {
        // child is set
    }

    onTabSelect(e: SelectEvent) {

        if (e.title == 'Item Details') {
            this.HelpAndFeebackContext = "(codingui)itemdetails";
        }
        else if (e.title == 'Study Arms') {
            this.HelpAndFeebackContext = "(codingui)itemdetails\\arms";
        }
        else if (e.title == 'PDF') {
            //if (this.HasDocForView && this.pdftroncontainer.currentDocId !== this.ItemDocsService.CurrentDocId) {
                //this.pdftroncontainer.loadDoc();//only load it if it's not there already
            //}
            this.HelpAndFeebackContext = "(codingui)itemdetails\\pdf";//no record in DB for the help!!
        }
        else {
            this.HelpAndFeebackContext = "(codingui)itemdetails";
        }
    }
    public IsServiceBusy4PDF(): boolean {
        if (this.ItemCodingService.IsBusy
            || this.ReviewSetsService.IsBusy
            //|| this.armservice.IsBusy
            //|| this.ItemDocsService.IsBusy
        ) return true;
        else return false;
    }
    public get HasDocForView(): boolean {
        //console.log("hasDocForView", this.ItemDocsService.CurrentDoc);
        if (this.ItemDocsService.CurrentDoc) return true;
        else return false;
    }
    async CheckAndMoveToPDFTab() {
        console.log("CheckAndMoveToPDFTab", this.ItemDocsService.CurrentDoc, this.tabstrip);
        if (this.HasDocForView) {
            console.log("CheckAndMoveToPDFTab2");
            if (this.pdftroncontainer) this.pdftroncontainer.loadDoc();
            if (this.tabstrip) {
                console.log("CheckAndMoveToPDFTab3");
                await Helpers.Sleep(50);//we need to give the UI thread the time to catch up and "un-disable" the tab.
                this.tabstrip.selectTab(1);
            }
        }
    }
    public get ShouldFetchPDFCoding(): boolean {//tells the tree component whether we should go fetch the PDF coding details...
        if (this.HelpAndFeebackContext == "itemdetails\\pdf") return true;
        else if (this.ItemDocsService._itemDocs.filter(found => found.itemDocumentId == this.ItemDocsService.CurrentDocId).length > 0) return true;
        else return false;
    }
    public CheckBoxAutoAdvanceVal: boolean = false;
    onSubmit(f: string) {
    }
    //@Output() criteriaChange = new EventEmitter();
    //public ListSubType: string = "";
	public ShowingOutComes() {
		this.ShowOutComes = !this.ShowOutComes;
	}
	private outcomeSubscription: Subscription | null = null;
	ngOnInit() {

        this.innerWidth = window.innerWidth;
		//this.route.params.subscribe(params => {

		//	alert(params);

		//	if (params['itemId']) {

		//		alert(params['itemId']);
		//	}
		//});

        
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
		else {
			this.outcomeSubscription = this._outcomeService.outcomesChangedEE.subscribe(

				(res: any) => {

					var selectedNode = res as SetAttribute;

					if (selectedNode && selectedNode.nodeType == 'SetAttribute') {

						console.log('a node has been selected');
						var itemSet = this._ItemCodingService.FindItemSetBySetId(selectedNode.set_id);
						if (itemSet != null) {
							this._outcomeService.ItemSetId = itemSet.itemSetId;
							this._outcomeService.FetchOutcomes(itemSet.itemSetId);
							//this._outcomeService.outcomesList = itemSet.OutcomeList;
						}
						this.ShowingOutComes();

					} else {

						console.log('a code is not selected');
						if (this.OutcomesCmpRef) {
							console.log('inside OutcomesCmpRef');
							this._outcomeService.outcomesList = [];
							this.OutcomesCmpRef.ShowOutcomesList = false;
							this.ShowingOutComes();
						}
					}
				}
				// ERROR HANDLING IN HERE NEXT....
			);


            //if (this.ArmsCompRef) {
                this.armservice.armChangedEE.subscribe(() => {
                    if (this.armservice.SelectedArm) this.SetArmCoding(this.armservice.SelectedArm.itemArmId);
                    else this.SetArmCoding(0);
                });
            //}
            this.subItemIDinPath = this.route.params.subscribe(params => {
                this.itemString = params['itemId'];
				this.GetItem();
				console.log('coding kjkhjkhk: ' + this.itemID);
            });
            this.ItemCodingServiceDataChanged = this.ItemCodingService.DataChanged.subscribe(

                () => {
                    if (this.ItemCodingService && this.ItemCodingService.ItemCodingList) {
                        //console.log('data changed event caught');
                        this.SetCoding();
                    }
                }
            );
            this.subCodingCheckBoxClickedEvent = this.ReviewSetsService.ItemCodingCheckBoxClickedEvent.subscribe((data: CheckBoxClickedEventData) => this.ItemAttributeSave(data));
            this.subGotPDFforViewing = this.ItemDocsService.GotDocument.subscribe(() => this.CheckAndMoveToPDFTab());
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.subscribe((cmdErr: any) => this.HandleItemAttributeSaveCommandError(cmdErr));
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.subscribe((cmd: ItemAttributeSaveCommand) => this.HandleItemAttributeSaveCommandDone(cmd));
        }


    }
    
    
    public GetItem() {

        this.WipeHighlights();
        if (this.itemString == 'PriorityScreening') {
            if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
            this.IsScreening = true;
            this.PriorityScreeningService.NextItem();
        }
        else {
            //this.itemID = +this.itemString;
            this.item = this.ItemListService.getItem(+this.itemString);

            this.IsScreening = false;
            this.GetItemCoding();
            //this.ItemListService.eventChange(this.itemID);
            console.log('fill in arms here teseroo1');

        }
    }

    public HasPreviousScreening(): boolean{
        //console.log('CanMoveToPInScreening' + this.PriorityScreeningService.CurrentItemIndex);
        if (this.PriorityScreeningService.CurrentItemIndex > 0) return true;
        return false;
    }
    public CanMoveToNextInScreening(): boolean {
        
        let ItemSetIndex = this.ItemCodingService.ItemCodingList.findIndex(cset =>
                cset.setId == this.reviewInfoService.ReviewInfo.screeningCodeSetId
            && (cset.contactId == this.ReviewerIdentityServ.reviewerIdentity.userId || cset.isCompleted));
        if (ItemSetIndex == -1) return false;
        else return true;
    }
    public prevScreeningItem() {
        this.WipeHighlights();
        if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
        this.IsScreening = true;
        this.PriorityScreeningService.PreviousItem();
    }
    public GotScreeningItem() {
        //console.log('got Screening Item');
        this.item = this.PriorityScreeningService.CurrentItem;
        //this.itemID = this.item.itemId;
        this.GetItemCoding();
    }
    private GetItemCoding() {
        //console.log('sdjghklsdjghfjklh ' + this.itemID);
        this.ItemDocsService.FetchDocList(this.itemID);
        if (this.item) {
            //if (this.ArmsCompRef) {
                this.ArmsCompRef.CurrentItem = this.item;
            //}
            this.armservice.FetchArms(this.item);
        }
        this.ItemCodingService.Fetch(this.itemID);    

    }
    SetCoding() {
        //console.log('set coding');
        this.SetHighlights();
        this.ReviewSetsService.clearItemData();
        if (this.ItemCodingService.ItemCodingList.length == 0) return;//no need to add codes that don't exist.
        if (this.armservice.SelectedArm) this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, this.armservice.SelectedArm.itemArmId);
        else this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, 0);
    }
    SetArmCoding(armId: number) {
        //console.log('change Arm');
        this.ReviewSetsService.clearItemData();
        this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, armId);
    }
    
    private _hasPrevious: boolean | null = null;
    hasPrevious(): boolean {
        
        if (!this.item || !this.ItemListService || !this.ItemListService.ItemList || !this.ItemListService.ItemList.items || this.ItemListService.ItemList.items.length < 1) {
            //console.log('NO!');
            return false;
        }
        else if (this._hasPrevious === null) {
            this._hasPrevious = this.ItemListService.hasPrevious(this.item.itemId);
        }
        //console.log('has prev? ' + this._hasPrevious);
        return this._hasPrevious;
    }
    MyHumanIndex():number {
        return this.ItemListService.ItemList.items.findIndex(found => found.itemId == this.itemID) + 1;
    }
    private _hasNext: boolean | null = null;
    hasNext(): boolean {
        if (!this.item || !this.ItemListService || !this.ItemListService.ItemList || !this.ItemListService.ItemList.items || this.ItemListService.ItemList.items.length < 1) {
            return false;
        }
        else if (this._hasNext === null)
        {
            this._hasNext = this.ItemListService.hasNext(this.item.itemId);
        }
        return this._hasNext;
    }
    firstItem() {
        this.WipeHighlights();
        
        if (this.item) this.goToItem(this.ItemListService.getFirst());

    }
    prevItem() {
        
        this.WipeHighlights();
        if (this.item) {
            console.log('inside previous coding item component part' + this.item.itemId);
   
            this.goToItem(this.ItemListService.getPrevious(this.item.itemId));
        }
    }
    nextItem() {
        
        this.WipeHighlights();
        if (this.item) {
            this.goToItem(this.ItemListService.getNext(this.item.itemId));
            console.log('inside next coding item component part' + this.item.itemId);
            //this.valueChange.next(this.item.itemId);
        }
    }
    lastItem() {
        this.WipeHighlights();
        if (this.item) this.goToItem(this.ItemListService.getLast());
    }
    clearItemData() {
        this._hasNext = null;
        this._hasPrevious = null;
        this.item = undefined;
        //this.itemID = -1;
        this.ItemCodingService.ItemCodingList = [];
        if (this.ReviewSetsService) {
            this.ReviewSetsService.clearItemData();
        }
    }
    goToItem(item: Item) {
        this.WipeHighlights();
        this.clearItemData();
        console.log('what do you need me to do?' + item.itemId);
        this.router.navigate(['itemcodingOnly', item.itemId]);
        this.item = item;
        //if (this.item.itemId != this.itemID) {

        //    this.itemID = this.item.itemId;
        //}
        //this.GetItemCoding();
    }
    BackToMain() {
        this.clearItemData();
        this.router.navigate(['MainCodingOnly']);
    }
    ItemAttributeSave(data: CheckBoxClickedEventData) {
        
        let SubSuccess: Subscription;
        let SubError: Subscription;//see https://medium.com/thecodecampus-knowledge/the-easiest-way-to-unsubscribe-from-observables-in-angular-5abde80a5ae3
        //console.log(data.AttId);
        let attribute: SetAttribute | null = this.ReviewSetsService.FindAttributeById(data.AttId);
        
        if (!attribute) {
            //problem: we don't know what to code!
            alert('Could not find the code we want to add to this item, please reload: something is not right with the data.');
            return;
        }

        let itemSet: ItemSet | null = this.ItemCodingService.FindItemSetBySetId(attribute.set_id);

        let cmd: ItemAttributeSaveCommand = new ItemAttributeSaveCommand();
        if (itemSet) {
            //we have an item set to use, so put the relevant data in cmd. Otherwise, default value is fine.
            cmd.itemSetId = itemSet.itemSetId;
        }
        cmd.itemId = this.itemID;
        cmd.additionalText = data.additionalText;
        cmd.itemArmId = data.armId;
        cmd.setId = attribute.set_id;
        //console.log(attribute.set_id);
        cmd.attributeId = data.AttId;
        cmd.revInfo = this.reviewInfoService.ReviewInfo;
        let itemAtt: ReadOnlyItemAttribute | null = null;
        if ((data.event.target && data.event.target.checked) || data.event == 'InfoboxTextAdded') {
            //add new code to item
            //console.log('cmd.saveType = "Insert"');
            cmd.saveType = "Insert";
        }
        else if (data.event == 'InfoboxTextUpdate' && itemSet) {
            //console.log('cmd.saveType = "Update"');
            cmd.saveType = "Update";
            for (let Candidate of itemSet.itemAttributesList) {
                if (Candidate.attributeId == cmd.attributeId
                    && Candidate.armId == cmd.itemArmId) {
                    itemAtt = Candidate;
                    break;
                }
            }
            if (!itemAtt) {
                //problem: we can't remove an item att, if we can't find it!
                alert('Sorry: can\'t find the Attribute to update...');
                return;
            }
            cmd.itemAttributeId = itemAtt.itemAttributeId;
        }
        else if (!data.event.target.checked && itemSet) {
            //we delete the coding
            cmd.saveType = "Delete"; 
            
            for (let Candidate of itemSet.itemAttributesList) {
                if (Candidate.attributeId == cmd.attributeId
                    && Candidate.armId == cmd.itemArmId) {
                    itemAtt = Candidate;
                    break;
                }
            }
            if (!itemAtt) {
                //problem: we can't remove an item att, if we can't find it!
                alert('Sorry: can\'t find the Attribute to remove...');
                return;
            }
            cmd.itemAttributeId = itemAtt.itemAttributeId;
        }
        SubError = this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.subscribe((cmdErr: any) => {
            this.ReviewSetsService.ItemCodingItemAttributeSaveCommandHandled();
            //do something if command ended with an error
            //console.log('Error handling');
            alert("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.unsubscribe();
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.unsubscribe();
        });
        SubSuccess = this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.subscribe((cmdResult: ItemAttributeSaveCommand) => {
            //update local version of the coding...
            
            if (cmd.saveType == "Insert" || cmd.saveType == "Update") {
                this.ItemCodingService.ApplyInsertOrUpdateItemAttribute(cmdResult, itemSet);
                //if (cmd.saveType == "Insert") this.ItemCodingService.FetchItemAttPDFCoding;
            }
            else if (cmd.saveType == "Delete") {
                
                //if (itemSet) console.log(itemSet.itemAttributesList.length);
                //if (itemAtt) console.log(itemAtt.attributeId);
                if (itemSet && itemAtt) {
                    //remove the itemAttribute from itemSet
                    itemSet.itemAttributesList = itemSet.itemAttributesList.filter(obj => obj !== itemAtt);
                    if (itemSet.itemAttributesList.length == 0) {
                        //if itemset does not have item attributes, remove the itemset
                        this.ItemCodingService.ItemCodingList = this.ItemCodingService.ItemCodingList.filter(obj => itemSet && obj.itemSetId !== itemSet.itemSetId);
                    }
                    //if (itemSet) console.log(itemSet.itemAttributesList.length);
                }
            }
            
            this.SetCoding();
            this.ReviewSetsService.ItemCodingItemAttributeSaveCommandHandled();
            console.log('set dest');
            SubSuccess.unsubscribe();
            SubError.unsubscribe();
            if (cmd.saveType == "Insert" && this.CheckBoxAutoAdvanceVal) {
                //auto advance is on, we want to go to the next item
                if (this.IsSmallScreen() && this.ShowCodesInSmallScreen) this.ShowCodesInSmallScreen = false;
                if (attribute) {
                    this.notificationService.show({
                        content: "Code just added: " + attribute.name,
                        position: { horizontal: 'left', vertical: 'bottom' },
                        animation: { type: 'fade', duration: 500 },
                        type: { style: 'none', icon: false },
                        hideAfter: 3000
                    });
                }
                if (!this.IsScreening && this.hasNext()) this.nextItem();
                else if (this.IsScreening) this.GetItem();//in screening mode, this uses the screening service to receive the next item
            }
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.unsubscribe();
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.unsubscribe();
        });
        //console.log("canwrite:" + this.ReviewSetsService.CanWrite);
        this.ReviewSetsService.ExecuteItemAttributeSaveCommand(cmd, this.ItemCodingService.ItemCodingList);
    }
    toHTML(text: string): string {
        return text.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
    }

    ItemChanged() {
        if (this.tabstrip) this.tabstrip.selectTab(0);
        this.WipeHighlights();
        this.SetHighlights();
    }

    ngOnDestroy() {
        //console.log('killing coding comp');
        if (this.subItemIDinPath) this.subItemIDinPath.unsubscribe();
        if (this.ItemCodingServiceDataChanged) this.ItemCodingServiceDataChanged.unsubscribe();
        if (this.subCodingCheckBoxClickedEvent) this.subCodingCheckBoxClickedEvent.unsubscribe();
        if (this.subGotScreeningItem) this.subGotScreeningItem.unsubscribe();
		if (this.subGotPDFforViewing) this.subGotPDFforViewing.unsubscribe();
		if (this.outcomeSubscription) this.outcomeSubscription.unsubscribe();
    }
    WipeHighlights() {
        if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.WipeHighlights();
    }
    SetHighlights() {
        if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.SetHighlights();
    }
    ShowHighlightsClicked() {
        if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.ShowHighlightsClicked();
    }


}






