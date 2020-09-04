import { Component, Inject, OnInit, EventEmitter, Output, OnDestroy, Input, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription, Subject, Subscribable, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria, Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet, ReadOnlyItemAttribute } from '../services/ItemCoding.service';
import { ReviewSetsService, ItemAttributeSaveCommand, SetAttribute, singleNode, ReviewSet } from '../services/ReviewSets.service';
import { CheckBoxClickedEventData, CodesetTreeCodingComponent } from '../CodesetTrees/codesetTreeCoding.component';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ReviewerTermsService } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ArmsService } from '../services/arms.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { SelectEvent, TabStripComponent } from '@progress/kendo-angular-layout';
import { WebViewerComponent } from '../PDFTron/webviewer.component';
import { Helpers } from '../helpers/HelperMethods';
import { PdfTronContainer } from '../PDFTron/pdftroncontainer.component';
import { timePointsService } from '../services/timePoints.service';
import { CreateNewCodeComp } from '../CodesetTrees/createnewcode.component';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { OutcomesService } from '../services/outcomes.service';
import { OutcomesComponent } from '../Outcomes/outcomes.component';
import { read } from 'fs';
import { concat } from '@progress/kendo-data-query/dist/npm/transducers';


@Component({
   
    selector: 'itemcodingFull',
    templateUrl: './codingFull.component.html',
    //providers: [ReviewerIdentityService]
    providers: [],
    styles: [`
                button.disabled {
                    color:black; 
                    }
            `]

})
export class ItemCodingFullComp implements OnInit, OnDestroy {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService
        , private route: ActivatedRoute, private ItemCodingService: ItemCodingService,
        private ReviewSetsService: ReviewSetsService,
        private reviewInfoService: ReviewInfoService,
        public PriorityScreeningService: PriorityScreeningService
        , private ReviewerTermsService: ReviewerTermsService,
        public ItemDocsService: ItemDocsService,
		private armservice: ArmsService,
		private timePointsService: timePointsService,
		private notificationService: NotificationService,
		private _reviewSetsEditingService: ReviewSetsEditingService,
		private _outcomeService: OutcomesService
    ) { }
   
    @ViewChild('ArmsCmp')
	private ArmsCompRef!: any;
	@ViewChild('TimePointsComp')
	private TimePointsComp!: any;
	//ArmDetailsComp
	@ViewChild('ArmDetailsComp')
	private ArmDetailsComp!: any;
	@ViewChild('OutcomesCmp')
	private OutcomesCmpRef!: OutcomesComponent;
    @ViewChild('ItemDetailsCmp')
	private ItemDetailsCompRef!: any; 

	@ViewChild('CreateNewCode') createNewCodeRef!: CreateNewCodeComp;
    @ViewChild('pdftroncontainer') private pdftroncontainer!: PdfTronContainer;
    @ViewChild('tabstripCoding') public tabstrip!: TabStripComponent;
    @ViewChild('codesetTreeCoding') public codesetTreeCoding!: CodesetTreeCodingComponent;
    private subItemIDinPath: Subscription | null = null;
    public ShowLiveComparison: boolean = false;
    private subCodingCheckBoxClickedEvent: Subscription | null = null;
    private ItemCodingServiceDataChanged: Subscription | null = null; 
    private ReloadItemCoding: Subscription | null = null; 
    private subGotPDFforViewing: Subscription | null = null;
    
    public get itemID(): number {
        if (this.item) return this.item.itemId;
        else return -1;
	}
	
    private itemString: string = '0';
	public item?: Item;
	public itemSet?: ItemSet;
	public itemId = new Subject<number>();
	public ShowOutComes: boolean = false;
	public get leftPanel(): string {
		//console.log("leftPanel", this.ReviewerTermsService._ShowHideTermsList, this.ShowingOutComes);
		if (this.ReviewerTermsService._ShowHideTermsList) {
			return "Highlights";
		}	
		else if 
		(this.ShowOutComes) {
			this.EditCodesPanel = "";
			return "OutComes";
		}
		else
		{ return ""; }
	}
    private subGotScreeningItem: Subscription | null = null;
    public IsScreening: boolean = false;
    public get ShowHighlights(): boolean {
        return this.ReviewerIdentityServ.userOptions.ShowHighlight;
    }
    public set ShowHighlights(val: boolean) {
        this.ReviewerIdentityServ.userOptions.ShowHighlight = val;
        this.ReviewerIdentityServ.SaveOptions();//otherwise they won't persist...
    }
	public EditCodesPanel: string = "";
    ngOnInit() {
        //console.log('init!');

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
		else {



			this.RefreshTerms();
            this.outcomeSubscription = this._outcomeService.outcomesChangedEE.subscribe(

                (res: any) => {

                    var selectedNode = res as SetAttribute;

                    if (selectedNode && selectedNode.nodeType == 'SetAttribute') {

                        console.log('a node has been selected');
                        var itemSet = this.ItemCodingService.FindItemSetBySetId(selectedNode.set_id);
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

            this.armservice.armChangedEE.subscribe(() => {
                if (this.armservice.SelectedArm) this.SetArmCoding(this.armservice.SelectedArm.itemArmId);
                else this.SetArmCoding(0);
            });
            //this.timePointsService.gotNewTimepoints.subscribe(() => {

            //    console.log('need to do something here of course....');
            //});
            this.ItemCodingService.ToggleLiveComparison.subscribe(() => {
                this.ShowLiveComparison = !this.ShowLiveComparison;
            })
            this.subItemIDinPath = this.route.params.subscribe(params => {
                this.itemString = params['itemId'];
                this.GetItem();
                //console.log('coding full sajdhfkjasfdh: ' + this.itemID);
            });
            this.ItemCodingServiceDataChanged = this.ItemCodingService.DataChanged.subscribe(

                () => {
                    console.log('ItemCodingService data changed event caught');
                    if (this.ItemCodingService && this.ItemCodingService.ItemCodingList) {
                        this.SetCoding();
                    }
                }
			);
            this.ReloadItemCoding = this.ReviewSetsService.GetReviewStatsEmit.subscribe(
                () => { this.SetCoding(); }
            );
            this.subCodingCheckBoxClickedEvent = this.ReviewSetsService.ItemCodingCheckBoxClickedEvent.subscribe((data: CheckBoxClickedEventData) => this.ItemAttributeSave(data));
            this.subGotPDFforViewing = this.ItemDocsService.GotDocument.subscribe(() => this.CheckAndMoveToPDFTab());

            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.subscribe((cmdErr: any) => this.HandleItemAttributeSaveCommandError(cmdErr));
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.subscribe((cmd: ItemAttributeSaveCommand) => this.HandleItemAttributeSaveCommandDone(cmd));
        }
	}
	public RefreshTerms() {
		
		// need to reload the page 
		// but the addition of the term is working
		this.SetHighlights();
		this.ReviewerTermsService.Fetch();		
	}
	
	public ShowingOutComes() {
        this.ShowOutComes = !this.ShowOutComes;
	}
	public SetCreateNewCode() {
		if (this.EditCodesPanel == "CreateNewCode") {
			this.EditCodesPanel = "";
		}
		else {
			this.EditCodesPanel = "CreateNewCode";
            this.ShowOutComes = false;
            if (this.OutcomesCmpRef) {
                this.OutcomesCmpRef.ShowOutcomesStatistics = false;
                this.OutcomesCmpRef.ShowOutcomesList = false;
            }
		}
	}
    public get HasTermList(): boolean {
        if (!this.ReviewerTermsService || !this.ReviewerTermsService.TermsList || !(this.ReviewerTermsService.TermsList.length > 0)) return false;
        else return true;
	}
    public CanCreateNewCode(): boolean {
        //console.log("Can create:", this.ReviewSetsService.selectedNode != null, this.CurrentCodeCanHaveChildren);
		if (this.ReviewSetsService.selectedNode && this.CurrentCodeCanHaveChildren) return true;
		else return false;
    }
    public CanMoveNodeDown(): boolean {
        if (this.ReviewSetsService.selectedNode == null) return false;
        else return this._reviewSetsEditingService.CanMoveDown(this.ReviewSetsService.selectedNode);
    }
    public CanMoveNodeUp(): boolean {
        if (this.ReviewSetsService.selectedNode == null) return false;
        else return this._reviewSetsEditingService.CanMoveUp(this.ReviewSetsService.selectedNode);
    }
    public async MoveUpNode() {
        if (this.ReviewSetsService.selectedNode == null) return false;
        else {
            await this._reviewSetsEditingService.MoveUpNode(this.ReviewSetsService.selectedNode);
            //and notify the tree:
            this.codesetTreeCoding.UpdateTree();
        }
        
    }
    public async MoveDownNode() {
        if (this.ReviewSetsService.selectedNode == null) return false;
        else {
            await this._reviewSetsEditingService.MoveDownNode(this.ReviewSetsService.selectedNode);
            //and notify the tree:
            this.codesetTreeCoding.UpdateTree();
        }
        
    }
    CanEditCode(): boolean {
        if (!this.CanWrite) return false;
        else if (!this.ReviewSetsService.selectedNode) return false;
        else if (this.ReviewSetsService.selectedNode.nodeType == "ReviewSet") return false;//in here only editing of codes is allowed...
        else return this.ReviewSetsService.CanEditSelectedNode;
    }
    public get CanChangeSelectedCode(): boolean {
        if (this.EditCodesPanel == 'EditCode') return false;
        else return true;
    }
    EditCode() {
        if (this.EditCodesPanel == "EditCode") this.EditCodesPanel = "";
        else this.EditCodesPanel = "EditCode";
    }
    CancelActivity(refreshTree?: boolean) {
        if (refreshTree) {
            if (this.ReviewSetsService.selectedNode) {
                //this.codesetTreeCoding.UpdateTree();
                let IsSet: boolean = this.ReviewSetsService.selectedNode.nodeType == "ReviewSet";
                let Id: number = -1;
                if (IsSet) Id = (this.ReviewSetsService.selectedNode as ReviewSet).set_id;
                else Id = (this.ReviewSetsService.selectedNode as SetAttribute).attribute_id;
                let sub: Subscription = this.ReviewSetsService.GetReviewStatsEmit.subscribe(() => {
                    console.log("trying to reselect: ", Id);
                    if (IsSet) this.ReviewSetsService.selectedNode = this.ReviewSetsService.FindSetById(Id);
                    else this.ReviewSetsService.selectedNode = this.ReviewSetsService.FindAttributeById(Id);
                    if (sub) sub.unsubscribe();
                }
                    , () => { if (sub) sub.unsubscribe(); }
                );
                //this.SetCoding();
                this.ReviewSetsService.selectedNode = null;
                this.ReviewSetsService.GetReviewSets();
            }
        }
        this.EditCodesPanel = "";
    }
	IsServiceBusy(): boolean {
		if (this._reviewSetsEditingService.IsBusy ||
			this.reviewInfoService.IsBusy || this._outcomeService.IsBusy
			|| this.ReviewerTermsService.IsBusy) {
			return true;
		}
		else return false;
	}
	CanWrite(): boolean {
		//console.log('CanWrite', this.ReviewerIdentityServ.HasWriteRights, this.IsServiceBusy());
		if (this.ReviewerIdentityServ.HasWriteRights && !this.IsServiceBusy()) {
			//console.log('CanWrite', true);
			return true;
		}
		else {
			//console.log('CanWrite', false);
			return false;
		}
    }

    public get ShowMagTab(): boolean {
        if (
            this.reviewInfoService.ReviewInfo.magEnabled
            && this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin //remove this 2nd condition when MAG features are ready for publish
        ) {
            return true;
        }
        else return false
    }
    public get CurrentNode(): singleNode | null {
        return this.ReviewSetsService.selectedNode;
    }
	public get CurrentCodeCanHaveChildren(): boolean {
		//safety first, if anything didn't work as expexcted return false;
		if (!this.CanWrite()) return false;
		else {
			return this.ReviewSetsService.CurrentCodeCanHaveChildren;
			//end of bit that goes into "ReviewSetsService.CanNodeHaveChildren(node: singleNode): boolean"
		}
	}
    public HelpAndFeebackContext: string = "itemdetails";
    public get HasWriteRights(): boolean {
        return this.ReviewerIdentityServ.HasWriteRights
    }
    public get CheckBoxAutoAdvanceVal(): boolean {
        return this.ReviewerIdentityServ.userOptions.AutoAdvance;
    }
    public set CheckBoxAutoAdvanceVal(val: boolean) {
        this.ReviewerIdentityServ.userOptions.AutoAdvance = val;
    }
    onSubmit(f: string) {
    }
    //@Output() criteriaChange = new EventEmitter();
    //public ListSubType: string = "";
    public get HasDocForView(): boolean{
        //console.log("hasDocForView", this.ItemDocsService.CurrentDoc);
        if (this.ItemDocsService.CurrentDoc) return true;
        else return false;
    }

    public get ShouldFetchPDFCoding(): boolean {//tells the tree component whether we should go fetch the PDF coding details...
        if (this.HelpAndFeebackContext == "itemdetails\\pdf") return true;
        else if (this.ItemDocsService._itemDocs.filter(found => found.itemDocumentId == this.ItemDocsService.CurrentDocId).length > 0) return true;
        else return false;
    }
    public IsServiceBusy4PDF(): boolean {
        if (this.ItemCodingService.IsBusy
            || this.ReviewSetsService.IsBusy
            || this.ItemCodingService.IsBusy
            //|| this.armservice.IsBusy
            //|| this.ItemDocsService.IsBusy
        ) return true;
        else return false;
    }

	private outcomeSubscription: Subscription | null = null;


    
    async CheckAndMoveToPDFTab() {
        //console.log("CheckAndMoveToPDFTab", this.ItemDocsService.CurrentDoc, this.tabstrip);
        if (this.HasDocForView) {
            //console.log("CheckAndMoveToPDFTab2");
            if (this.pdftroncontainer) this.pdftroncontainer.loadDoc();
            if (this.tabstrip) {
                //console.log("CheckAndMoveToPDFTab3");
                await Helpers.Sleep(50);//we need to give the UI thread the time to catch up and "un-disable" the tab.
                this.tabstrip.selectTab(2);
            }
        }
    }
    public EditItem() {
        this.router.navigate(['EditItem', this.itemID.toString() + "?return=itemcoding/" + this.itemID.toString()]);
        //this.router.navigate(['EditItem?return=Main']);
    }
    
    public GetItem() {
        this.WipeHighlights();
        if (this.itemString == 'PriorityScreening') {
            if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
            this.IsScreening = true;
            console.log("asking for next screening item");
            this.PriorityScreeningService.NextItem();
        }
        else {
            //this.itemID = +this.itemString;
            this.item = this.ItemListService.getItem(+this.itemString);
            this.IsScreening = false;
            this.GetItemCoding();
            //this.ItemListService.eventChange(this.itemID);
            //console.log('fill in arms here teseroo1');
		}
		//console.log('this is item: ' + this.item);
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
        if (this.tabstrip) this.tabstrip.selectTab(0);
    }
    private GetItemCoding() {
        console.log('Getting item coding for itemID: ' + this.itemID);
        this.ItemDocsService.FetchDocList(this.itemID);
		if (this.item) {

			this._outcomeService.outcomesChangedEE.emit();
            this.ArmsCompRef.CurrentItem = this.item;
			this.armservice.FetchArms(this.item);
			this.timePointsService.Fetchtimepoints(this.item);

        }
        this.ItemCodingService.Fetch(this.itemID);    

    }
    SetCoding() {
        //console.log('set coding');
        this.SetHighlights();
        //this.ReviewSetsService.clearItemData();
        if (this.ItemCodingService.ItemCodingList.length == 0) {
            this.ReviewSetsService.clearItemData();
            return;//no need to add codes that don't exist.
        }
        if (this.armservice.SelectedArm) this.SetArmCoding(this.armservice.SelectedArm.itemArmId);
        else this.SetArmCoding(0);
    }
    SetArmCoding(armId: number) {
        //console.log('change Arm');
        this.ReviewSetsService.clearItemData();
        this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, armId);
        if (this.ItemDocsService.CurrentDoc && this.item && this.ItemDocsService.CurrentItemId == this.item.itemId && this.item.itemId > 0) {
            if (this.codesetTreeCoding) this.codesetTreeCoding.FetchPDFHighlights();//if there is a doc and it's for this item, we might need to fetch the highlights...
        }
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
        this.ItemCodingService.ItemCodingList = [];
        if (this.ReviewSetsService) {
            this.ReviewSetsService.clearItemData();
        }
		this.ItemCodingService.Clear();
    }
    goToItem(item: Item) {
        this.WipeHighlights();
		this.clearItemData();
		//this.ArmsCompRef.Clear();
		this.TimePointsComp.Clear();
		this.ArmDetailsComp.Clear();
        console.log('what do you need me to do?' + item.itemId);
        this.router.navigate(['itemcoding', item.itemId]);
        this.item = item;
        //if (this.item.itemId != this.itemID) {
        //    //console.log("set new ItemID:" , this.item.itemId);
        //    this.itemID = this.item.itemId;
        //}
        //this.GetItemCoding();
    }
	BackToMain() {
        this.clearItemData();
        this.router.navigate(['Main']);
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
                    && Candidate.armId == cmd.itemArmId
                ) {
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
                this.ItemCodingService.ApplyDeleteItemAttribute(itemSet, itemAtt);
                //if (itemSet) console.log(itemSet.itemAttributesList.length);
                //if (itemAtt) console.log(itemAtt.attributeId);
            }
            
            this.SetCoding();
            console.log('set dest');
            SubSuccess.unsubscribe();
            SubError.unsubscribe();
            this.ReviewSetsService.ItemCodingItemAttributeSaveCommandHandled();
            if (cmd.saveType == "Insert" && this.CheckBoxAutoAdvanceVal) {
                //auto advance is on, we want to go to the next item
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
    ItemChanged() {
        if (this.tabstrip) this.tabstrip.selectTab(0);
        this.WipeHighlights();
		this.SetHighlights();
		this.TimePointsComp.Clear();
		this.ArmDetailsComp.Clear();
		
    }
    WipeHighlights() {
        if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.WipeHighlights();
    }
    SetHighlights() {
        if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.SetHighlights();
    }
    ShowHighlightsClicked() {
		if (this.ItemDetailsCompRef) {
			this.ItemDetailsCompRef.ShowHighlightsClicked();

		}
		else { console.log('Ouch'); }
    }
    //public tabSelected: string = 'itemdetails';
    onTabSelect(e: SelectEvent) {

        if (e.title == 'Item Details') {
            
            this.HelpAndFeebackContext = "itemdetails";
        }
        else if (e.title == 'Arms and Timepoints') {
            
            this.HelpAndFeebackContext = "itemdetails\\arms";
        }
        else if (e.title == 'Coding Record') {
           
            this.HelpAndFeebackContext = "itemdetails\\codingrecord";
        }
        else if (e.title == 'PDF') {
            //if (this.HasDocForView && this.pdftroncontainer.currentDocId !== this.ItemDocsService.CurrentDocId) {
                //this.pdftroncontainer.loadDoc();//only load it if it's not there already
            //}
          
            this.HelpAndFeebackContext = "itemdetails\\pdf";//no record in DB for the help!!
        }
        else if (e.title == 'Microsoft Academic') {
           
            console.log('test tabs');
            this.HelpAndFeebackContext = "itemdetails\\Microsoft Academic";
        }
        else {
          
            this.HelpAndFeebackContext = "itemdetails";
        }
    }
    ngOnDestroy() {
        //console.log('killing coding comp');
        if (this.subItemIDinPath) this.subItemIDinPath.unsubscribe();
        if (this.ItemCodingServiceDataChanged) this.ItemCodingServiceDataChanged.unsubscribe();
        if (this.subCodingCheckBoxClickedEvent) this.subCodingCheckBoxClickedEvent.unsubscribe();
        if (this.subGotScreeningItem) this.subGotScreeningItem.unsubscribe();
		if (this.subGotPDFforViewing) this.subGotPDFforViewing.unsubscribe();
		if (this.outcomeSubscription) this.outcomeSubscription.unsubscribe();
        if (this.ReloadItemCoding) this.ReloadItemCoding.unsubscribe();
    }
}






