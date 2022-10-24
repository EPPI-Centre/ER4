import { Injectable } from '@angular/core';


@Injectable({
    providedIn: 'root',
})

export class ZoteroClasses  {
    constructor(
    ) {
    }
}


export class GroupSelf {
    href: string = '';
    type: string = '';
}

export class GroupAlternate {
    href: string = '';
    type: string = '';
}

export class GroupLinks {
    self: GroupSelf = new GroupSelf();
    alternate: GroupAlternate = new GroupAlternate();
}

export class GroupMeta {
    created: string = '';
    lastModified: string = '';
    numItems: number = 0;
}

export class GroupData {
    id: number = 0;
    version: number = 0;
    name: string = '';
    owner: number = 0;
    type: string = '';
    description: string = '';
    url: string = '';
    libraryEditing: string = '';
    libraryReading: string = '';
    fileEditing: string = '';
    groupBeingSynced: boolean = false;
}

export class Group {
    id: number = 0;
    version: number = 0;
    links: GroupLinks = new GroupLinks();
    meta: GroupMeta = new GroupMeta();
    data: GroupData = new GroupData();
    groupBeingSynced: boolean = false;
}


export interface User {
    library: boolean;
    files: boolean;
    notes: boolean;
    write: boolean;
}

export interface AllPermissions {
    library: boolean;
    write: boolean;
}

export interface PerGroupPermissions {
    [key: string]: AllPermissions
}


export interface Groups {
    all: AllPermissions | any;
}




export interface ApiKeyInfo {
  zoteroConnectionId: number;
  libraryId: string;
  rEVIEW_ID: number;
  erUserId: number;
  zoteroUserId: number;
  status: string;
}

export interface UserSubscription {

}


export interface IERWebObjects {
    itemID: number;
    itemReviewID: number;
    itemDocumentID: number;
}


export interface IERWebZoteroObjects {
    itemID: number;
    itemReviewID: number;
    itemDocumentID: number;
    itemkey: string;
    version: string;
}

export interface IZoteroReviewItem {
    zotero_item_review_ID: number;
    itemKey: string;
    libraryID: string;
    iteM_REVIEW_ID: number;
    version: string;
    lAST_MODIFIED: string;
}

export interface IERWebANDZoteroReviewItem {
    zotero_item_review_ID: number;
    itemKey: string;
    libraryID: string;
    iteM_REVIEW_ID: number;
    version: string;
    lAST_MODIFIED: string;
    itemID: number;
    shortTitle: string;
    typeName: string;
    title: string;
}

export interface IUserGroup {
    items: GroupItem[];
}

export class GroupItem {
    groupID: string = '';
    version: string = '';
    sync: string = '';
}

//export class TypeCollection {
//    key: string = '';
//    version: number = 0;
//    library: any;
//    links: any;
//    meta: any;
//    data: CollectionData = new CollectionData();
//}

export class Book {
    key: string = '';
    version: number = 0;
    itemType: string = '';
    title: string = '';
    creators: CreatorsItem[] = [];
    abstractNote: string = '';
    series: string = '';
    seriesNumber: string = '';
    volume: string = '';
    numberOfVolumes: string = '';
    edition: string = '';
    place: string = '';
    publisher: string = '';
    date: string = '';
    numPages: string = '';
    language: string = '';
    ISBN: string = '';
    shortTitle: string = '';
    url: string = '';
    accessDate: string = '';
    archive: string = '';
    archiveLocation: string = '';
    libraryCatalog: string = '';
    callNumber: string = '';
    rights: string = '';
    extra: string = '';
    tags: string[] = [];
    collections: string[] = [];
    relations: any;
    dateAdded: string = '';
    dateModified: string = '';
}

export class BookRecieved {
    key: string = '';
    version: number = 0;
    itemType: string = '';
    title: string = '';
    creators: any[] = [];
    abstractNote: string = '';
    series: string = '';
    seriesNumber: string = '';
    volume: string = '';
    numberOfVolumes: string = '';
    edition: string = '';
    place: string = '';
    publisher: string = '';
    date: string = '';
    numPages: string = '';
    language: string = '';
    ISBN: string = '';
    shortTitle: string = '';
    url: string = '';
    accessDate: string = '';
    archive: string = '';
    archiveLocation: string = '';
    libraryCatalog: string = '';
    callNumber: string = '';
    rights: string = '';
    extra: string = '';
    tags: any[] = [];
    collections: any[] = [];
    relations: any;
    dateAdded: string = '';
    dateModified: string = ''; 
}

export class CreatorsItem {
    creatorType: string = '';
    firstName: string = '';
    lastName: string = '';
}

//export class Collection {
//    key: string = '';
//    version: number = 0;
//    library: any; 
//    links: Links = new Links();
//    meta: Meta = new Meta(); 
//    data: CollectionData = new CollectionData();
//}

//export class Meta {
//    created: string = '';
//    lastModified: string = '';
//    numItems: number = 0;
//    numChildren: number = 0;
//}

//export class CollectionData {

//    key: string = '';
//    version: number = 0;
//    itemType: string = '';
//    title: string = '';
//    creators: any[] = [];
//    abstractNote: string = '';
//    series: string = '';
//    seriesNumber: string = '';
//    volume: string = '';
//    date: string = '';
//    language: string = '';
//    shortTitle: string = '';
//    publicationTitle: string = '';
//    url: string = '';
//    accessDate: string = '';
//    archive: string = '';
//    archiveLocation: string = '';
//    libraryCatalog: string = '';
//    callNumber: string = '';
//    rights: string = '';
//    extra: string = '';
//    tags: any;
//    collections: any[] = [];
//    relations: any;
//    dateAdded: string = '';
//    dateModified: string = '';
//    parentItem: string = '';
//}

//export class Library {
//    type: string = '';
//    id: number = 0;
//    name: string = '';
//    links: Links = new Links();
//}

//export class Links {
//    alternate: Alternate = new Alternate();
//    self: Self = new Self();
//    attachment: Attachment = new Attachment();
//}

//export class Alternate {
//    href: string = '';
//    type: string = '';
//}

//export class Self {
//    href: string = '';
//    type: string = '';
//}



//export class Attachment {
//    href: string = '';
//    type: string = '';
//    attachmentSize: number = 0;
//    attachmentType: string = '';
//}

export class ZoteroReviewCollection {
    collectionKey: string = '';
    libraryID: string = '';
    apiKey: string = '';
    revieW_ID: string = '';
    userId: string = '';
    parentCollection: string = '';
    collectionName: string = '';
    version: string = '';
    dateCreated: string = '';
    groupBeingSynced: boolean = false;
}

export class ZoteroReviewCollectionList {

    ZoteroReviewCollectionList: ZoteroReviewCollection[] = [];
}



export interface IObjectSyncState {
    objectKey: string;
    syncState: SyncState;
}

export interface IObjectsInERWebNotInZotero {
    itemId: number;
    shortTitle: string;
    itemReviewId: number;
    typeName: string;
    documentAttached: boolean;
}

export interface IZoteroERWebReviewItem {
  itemId: number | null;
  itemKey: string | null;
  itemState: SyncState;
  hasDocsOutofSync: boolean;
  documents: IDocSyncState[];
}
export interface IDocSyncState {
  itemDocument_id: number | null;
  docZoterokey: string | null;
  documentSyncState: any;// DocSyncState;
}


export class ZoteroItem {
  constructor(izjo: iZoteroJobject) {
    const t = izjo.data;
    this.key = t.key;
    this.title = t.title;
    this.shortTitle = t.shortTitle;
    this.parentTitle = t.publicationTitle;
    this.dateModified = t.dateModified;
    this.itemType = t.itemType;
    this.version = t.version;
  }
  public ToZoteroERWebReviewItem(): ZoteroERWebReviewItem {
    let res = new ZoteroERWebReviewItem(null, this);
    return res;
  }
  public get HasAttachments(): boolean {
    if (this.attachments.length == 0) return false;
    else return true;
  }
  public get HasAttachmentsToPull(): boolean {
    const ind = this.attachments.findIndex(f => f.syncState == SyncState.canPull);
    if (ind == -1) return false;
    else return true;
  }
  public FindAttachmentByZoteroKey(key: string): ZoteroAttachment | null {
    const ind = this.attachments.findIndex(f => f.key == key);
    if (ind == -1) return null;
    else return this.attachments[ind];
  }
  key: string = "";
  title: string = "";
  shortTitle: string = "";
  parentTitle: string = "";
  dateModified: string = "";
  itemType: string = "";
  itemId: number = 0;
  attachments: ZoteroAttachment[] = [];
  version: number = 0;
  syncState: SyncState = SyncState.notSet;
}
export class ZoteroAttachment {
  constructor(izjo: iZoteroJobject) {
    this.key = izjo.data.key;
    this.filename = izjo.data.title;
    this.dateModified = izjo.data.dateModified;
  }
  key: string = "";
  filename: string = "";
  dateModified: string = "";
  syncState: SyncState = SyncState.notSet;
}

export interface iZoteroItemsResult {
  zoteroItems: iZoteroJobject[];
  pairedItems: iZoteroERWebReviewItem[];
}
export interface iZoteroJobject {
  key: string;
  version: number;
  library: iZoteroLibrary;
  links: iZoteroLinks;
  meta: iZoteroMeta;
  data: iCollectionData;
}

export interface iZoteroLibrary {
  type: string;
  id: number;
  name: string;
  links: iZoteroLinks;
}

export interface iZoteroMeta {
  created: string;
  lastModified: string;
  numItems: number;
  numChildren: number;
}

export interface iZoteroLinks {
  alternate: iZoteroTypeRefPair;
  self: iZoteroTypeRefPair;
  attachment: iZoteroAttachment;
}
export interface iZoteroTypeRefPair {
  href: string;
  type: string;
}
export interface iZoteroAttachment extends iZoteroTypeRefPair {
  attachmentSize: number;
  attachmentType: string;
}

export interface iCollectionData {

  key: string;
  version: number;
  itemType: string;
  title: string;
  creators: any[];
  abstractNote: string;
  series: string;
  seriesNumber: string;
  volume: string;
  date: string;
  language: string;
  shortTitle: string;
  publicationTitle: string;
  url: string;
  accessDate: string;
  archive: string;
  archiveLocation: string;
  libraryCatalog: string;
  callNumber: string;
  rights: string;
  extra: string;
  tags: any;
  collections: any[];
  relations: any;
  dateAdded: string;
  dateModified: string;
  parentItem: string;
}


export interface iZoteroERWebReviewItem {
  itemID: number;
  iteM_REVIEW_ID: number;
  itemKey: string;
  lasT_MODIFIED: string;
  pdfList: iZoteroERWebItemDoc[];
  shortTitle: string;
  syncState: SyncState;
  title: string;
  version: number;
}
export interface iZoteroERWebItemDoc {
  docZoteroKey: string;
  documenT_TITLE: string;
  itemDocumentId: number;
  syncState: SyncState;
}

export enum SyncState {
  notSet,
  upToDate,
  canPush,
  canPull
}

export class ZoteroERWebReviewItem {
  constructor(data: iZoteroERWebReviewItem | null , data2: ZoteroItem | null = null) {
    if (data != null) {
      this.itemID = data.itemID;
      this.iteM_REVIEW_ID = data.iteM_REVIEW_ID;
      this.itemKey = data.itemKey;
      this.lasT_MODIFIED = data.lasT_MODIFIED;
      this.shortTitle = data.shortTitle;
      this.syncState = data.syncState;
      this.title = data.title;
      this.version = data.version;
      this.pdfList = [];
      for (let ip of data.pdfList) {
        let pdf = new ZoteroERWebItemDoc(ip);
        this.pdfList.push(pdf);
      }
    } else if (data2 != null) {
      this.itemID = data2.itemId;
      this.iteM_REVIEW_ID = -1;
      this.itemKey = data2.key;
      this.lasT_MODIFIED = data2.dateModified;
      this.shortTitle = data2.shortTitle;
      this.syncState = data2.syncState;
      this.title = data2.title;
      this.version = -1;
      this.pdfList = [];
      for (let ip of data2.attachments) {
        let pdf = new ZoteroERWebItemDoc(null, ip);
        this.pdfList.push(pdf);
      }
    } else {
      this.itemID = -1;
      this.iteM_REVIEW_ID = -1;
      this.itemKey = "";
      this.lasT_MODIFIED = "";
      this.shortTitle = "";
      this.syncState = SyncState.notSet;
      this.title = "";
      this.version = -1;
      this.pdfList = [];
    }
  }
  public get HasPdf(): boolean {
    if (this.pdfList.length == 0) return false;
    else return true;
  }
  public get HasPdfToPush(): boolean {
    const ind = this.pdfList.findIndex(f => f.syncState == SyncState.canPush);
    if (ind == -1) return false;
    else return true;
  }
  itemID: number;
  iteM_REVIEW_ID: number;
  itemKey: string;
  lasT_MODIFIED: string;
  pdfList: ZoteroERWebItemDoc[];
  shortTitle: string;
  version: number;
  syncState: SyncState;
  title: string;
}

export class ZoteroERWebItemDoc {
  constructor(data: iZoteroERWebItemDoc | null, data2: ZoteroAttachment | null = null) {
    if (data != null) {
      this.documenT_TITLE = data.documenT_TITLE;
      this.docZoteroKey = data.docZoteroKey;
      this.item_Document_Id = data.itemDocumentId;
      this.syncState = data.syncState;
    } else if (data2 != null) {
      this.documenT_TITLE = data2.filename;
      this.docZoteroKey = data2.key;
      this.item_Document_Id = -1;
      this.syncState = data2.syncState;
    } else {
      this.documenT_TITLE = "";
      this.docZoteroKey = "";
      this.item_Document_Id = -1;
      this.syncState = SyncState.notSet;
    }
  }
  docZoteroKey: string;
  documenT_TITLE: string;
  item_Document_Id: number;
  syncState: SyncState;
}
