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

export class CollectionData {

    key: string = '';
    version: number = 0;
    itemType: string = '';
    title: string = '';
    creators: any[] = [];
    abstractNote: string = '';
    series: string = '';
    seriesNumber: string = '';
    volume: string = '';
    date: string = '';
    language: string = '';
    shortTitle: string = '';
    publicationTitle: string = '';
    url: string = '';
    accessDate: string = '';
    archive: string = '';
    archiveLocation: string = '';
    libraryCatalog: string = '';
    callNumber: string = '';
    rights: string = '';
    extra: string = '';
    tags: any;
    collections: any[] = [];
    relations: any;
    dateAdded: string = '';
    dateModified: string = '';
    parentItem: string = '';
}

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
  }
  key: string = "";
  title: string = "";
  shortTitle: string = "";
  parentTitle: string = "";
  dateModified: string = "";
  itemType: string = "";
  attachments: ZoteroAttachment[] = [];
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
export interface iZoteroJobject {
  key: string;
  version: number;
  library: iZoteroLibrary;
  links: iZoteroLinks;
  meta: iZoteroMeta;
  data: CollectionData;
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

export interface iZoteroERWebReviewItem {
  itemID: number;
  itemKey: string;
  lasT_MODIFIED: string;
  pdfList: iZoteroERWebItemDoc[];
  shortTitle: number;
  syncState: SyncState;
  title: string;
}
export interface iZoteroERWebItemDoc {
  doc_Zotero_Key: string;
  documenT_TITLE: string;
  item_Document_Id: number;
  syncState: SyncState;
}

export enum SyncState {
  notSet,
  upToDate,
  canPush,
  canPull
}

export class ZoteroERWebReviewItem {
  constructor(data: iZoteroERWebReviewItem) {
    this.itemID = data.itemID;
    this.itemKey = data.itemKey;
    this.lasT_MODIFIED = data.lasT_MODIFIED;
    this.shortTitle = data.shortTitle;
    this.syncState = data.syncState;
    this.title = data.title;
    this.pdfList = [];
    for (let ip of data.pdfList) {
      let pdf = new ZoteroERWebItemDoc(ip);
      this.pdfList.push(pdf);
    }
  }
  public get HasPdf(): boolean {
    if (this.pdfList.length == 0) return false;
    else return true;
  }
  itemID: number;
  itemKey: string;
  lasT_MODIFIED: string;
  pdfList: iZoteroERWebItemDoc[];
  shortTitle: number;
  syncState: SyncState;
  title: string;
}
export class ZoteroERWebItemDoc {
  constructor(data: iZoteroERWebItemDoc) {
    this.documenT_TITLE = data.documenT_TITLE;
    this.doc_Zotero_Key = data.doc_Zotero_Key;
    this.item_Document_Id = data.item_Document_Id;
    this.syncState = data.syncState;
  }
  doc_Zotero_Key: string;
  documenT_TITLE: string;
  item_Document_Id: number;
  syncState: SyncState;
}
