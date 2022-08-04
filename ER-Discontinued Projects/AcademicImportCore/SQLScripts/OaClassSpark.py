# Databricks notebook source
#
# AzureStorageAccess class to access Azure Storage streams
#
#   Parameters:
#     container: container name in Azure Storage (AS) account
#     account: Azure Storage (AS) account name
#     sas: complete 'Blob service SAS URL' of the shared access signature (sas) for the container
#     key: access key for the container, if sas is specified, key is ignored
#
#   Note:
#     you need to provide value for either sas or key
#
class AzureStorageAccess:
  # constructor
  def __init__(self, container, account, sas='', key=''):

    if container == '':
      raise ValueError('container should not be empty')
    
    if account == '':
      raise ValueError('account should not be empty')
    
    if sas == '' and key == '' :
      raise ValueError('provide value for either sas or key')
    
    self.container = container
    self.account = account

    # Set up an account access key or a SAS for the container
    # Once an account access key or a SAS is set up in your notebook, you can use standard Spark and Databricks APIs to read from the storage account
    # Use SAS first then account access key
    if sas != '':
      spark.conf.set('fs.azure.sas.%s.%s.blob.core.windows.net' % (container, account), sas)
    else :
      spark.conf.set('fs.azure.account.key.%s.blob.core.windows.net' % account, key)

  def getFullpath(self, path):
    return 'wasbs://%s@%s.blob.core.windows.net/%s' % (self.container, self.account, path)

# COMMAND ----------

# MAGIC %md **MicrosoftAcademicGraph** class to access MAG streams

# COMMAND ----------

#
# MicrosoftAcademicGraph class to access MAG streams
#
#   Parameters:
#     container: container name in Azure Storage (AS) account for the MAG dataset. Usually in forms of mag-yyyy-mm-dd
#     account: Azure Storage (AS) account containing MAG dataset
#     sas: complete 'Blob service SAS URL' of the shared access signature (sas) for the container
#     key: access key for the container, if sas is specified, key is ignored
#
#   Note:
#     you need to provide value for either sas or key
#     MAG streams do not have header
#
from pyspark.sql.types import *

class MicrosoftAcademicGraph(AzureStorageAccess):
  # constructor
  def __init__(self, container, account, version='', sas='', key=''):
    AzureStorageAccess.__init__(self, container, account, sas, key) 
    self.version = version

  # return stream path
  def getBasepath(self):
    basepath = ''
    if (self.version != ''):
      basepath = self.version + '/'
    return basepath

  # return stream path
  def getFullpath(self, streamName):
    path = self.getBasepath() + self.streams[streamName][0]
    return AzureStorageAccess.getFullpath(self, path)

  # return stream header
  def getHeader(self, streamName):
    return self.streams[streamName][1]

  datatypedict = {
    'bool' : BooleanType(),
    'int' : IntegerType(),
    'uint' : IntegerType(),
    'long' : LongType(),
    'ulong' : LongType(),
    'float' : FloatType(),
    'string' : StringType(),
    'DateTime' : DateType(),
  }

  # return stream schema
  def getSchema(self, streamName):
    schema = StructType()
    for field in self.streams[streamName][1]:
      fieldname, fieldtype = field.split(':')
      nullable = fieldtype.endswith('?')
      if nullable:
        fieldtype = fieldtype[:-1]
      schema.add(StructField(fieldname, self.datatypedict[fieldtype], nullable))
    return schema

  # return stream dataframe
  def getDataframe(self, streamName):
    return spark.read.format('csv').options(header='true', delimiter='\t').schema(self.getSchema(streamName)).load(self.getFullpath(streamName))

  # define stream dictionary
  # entities removed: ConferenceInstances, ConferenceSeries, PaperCitationContexts, PaperUrls
  streams = {
    'Affiliations' : ('mag/Affiliations.txt', ['AffiliationId:long', 'Rank:uint', 'NormalizedName:string', 'DisplayName:string', 'GridId:string', 'RorId:string', 'OfficialPage:string', 'WikiPage:string', 'PaperCount:long', 'PaperFamilyCount:long', 'CitationCount:long', 'Iso3166Code:string', 'Latitude:float?', 'Longitude:float?', 'CreatedDate:DateTime', 'UpdatedDate:string']),
    'AuthorExtendedAttributes' : ('mag/AuthorExtendedAttributes.txt', ['AuthorId:long', 'AttributeType:int', 'AttributeValue:string']),
    'Authors' : ('mag/Authors.txt', ['AuthorId:long', 'Rank:uint', 'NormalizedName:string', 'DisplayName:string', 'Orcid:string', 'LastKnownAffiliationId:long?', 'PaperCount:long', 'PaperFamilyCount:long', 'CitationCount:long', 'CreatedDate:DateTime', 'UpdatedDate:string']),
    'EntityRelatedEntities' : ('advanced/EntityRelatedEntities.txt', ['EntityId:long', 'EntityType:string', 'RelatedEntityId:long', 'RelatedEntityType:string', 'RelatedType:int', 'Score:float']),
    'FieldOfStudyChildren' : ('advanced/FieldOfStudyChildren.txt', ['FieldOfStudyId:long', 'ChildFieldOfStudyId:long']),
    'FieldOfStudyExtendedAttributes' : ('advanced/FieldOfStudyExtendedAttributes.txt', ['FieldOfStudyId:long', 'AttributeType:int', 'AttributeValue:string']),
    'FieldsOfStudy' : ('advanced/FieldsOfStudy.txt', ['FieldOfStudyId:long', 'Rank:uint', 'NormalizedName:string', 'DisplayName:string', 'MainType:string', 'Level:int', 'PaperCount:long', 'PaperFamilyCount:long', 'CitationCount:long', 'CreatedDate:DateTime']),
    'Journals' : ('mag/Journals.txt', ['JournalId:long', 'Rank:uint', 'NormalizedName:string', 'DisplayName:string', 'Issn:string', 'Issns:string', 'IsOa:bool', 'IsInDoaj:bool','Publisher:string', 'Webpage:string', 'PaperCount:long', 'PaperFamilyCount:long', 'CitationCount:long', 'CreatedDate:DateTime', 'UpdatedDate:string']),
    'PaperAbstractsInvertedIndex' : ('nlp/PaperAbstractsInvertedIndex.txt.{*}', ['PaperId:long', 'IndexedAbstract:string']),
    'PaperAuthorAffiliations' : ('mag/PaperAuthorAffiliations.txt', ['PaperId:long', 'AuthorId:long', 'AffiliationId:long?', 'AuthorSequenceNumber:uint', 'OriginalAuthor:string', 'OriginalAffiliation:string']),
    'PaperExtendedAttributes' : ('mag/PaperExtendedAttributes.txt', ['PaperId:long', 'AttributeType:int', 'AttributeValue:string']),
    'PaperFieldsOfStudy' : ('advanced/PaperFieldsOfStudy.txt', ['PaperId:long', 'FieldOfStudyId:long', 'Score:float', 'AlgorithmVersion:int']),
    'PaperMeSH' : ('advanced/PaperMeSH.txt', ['PaperId:long', 'DescriptorUI:string', 'DescriptorName:string', 'QualifierUI:string', 'QualifierName:string', 'IsMajorTopic:bool']),
    'PaperRecommendations' : ('advanced/PaperRecommendations.txt', ['PaperId:long', 'RecommendedPaperId:long', 'Score:float']),
    'PaperReferences' : ('mag/PaperReferences.txt', ['PaperId:long', 'PaperReferenceId:long']),
    'PaperResources' : ('mag/PaperResources.txt', ['PaperId:long', 'ResourceType:int', 'ResourceUrl:string', 'SourceUrl:string', 'RelationshipType:int']),
    'Papers' : ('mag/Papers.txt', ['PaperId:long', 'Rank:uint', 'Doi:string', 'DocType:string', 'Genre:string', 'IsParatext:string', 'PaperTitle:string', 'OriginalTitle:string', 'BookTitle:string', 'Year:int?','Date:DateTime?', 'OnlineDate:DateTime?', 'Publisher:string', 'JournalId:long?', 'ConferenceSeriesId:long?', 'ConferenceInstanceId:long?', 'Volume:string', 'Issue:string', 'FirstPage:string', 'LastPage:string', 'ReferenceCount:long', 'CitationCount:long', 'EstimatedCitation:long', 'OriginalVenue:string', 'FamilyId:long?', 'FamilyRank:uint?', 'DocSubTypes:string', 'OaStatus:string', 'BestUrl:string', 'BestFreeUrl:string', 'BestFreeVersion:string', 'DoiLower:string', 'CreatedDate:DateTime', 'UpdatedDate:string']),
    'RelatedFieldOfStudy' : ('advanced/RelatedFieldOfStudy.txt', ['FieldOfStudyId1:long', 'Type1:string', 'FieldOfStudyId2:long', 'Type2:string', 'Rank:float']),
  }

# COMMAND ----------

# MAGIC %md **AzureStorageUtil** class to access Azure Storage streams

# COMMAND ----------

#
# AzureStorageUtil class to access Azure Storage streams
#
#   Parameters:
#     container: container name in Azure Storage (AS) account for input/output streams in PySpark notebook
#     account: Azure Storage (AS) account
#     sas: complete 'Blob service SAS URL' of the shared access signature (sas) for the container
#     key: access key for the container, if sas is specified, key is ignored
#
#   Note:
#     you need to provide value for either sas or key
#     streams contain headers
#
import uuid
class AzureStorageUtil(AzureStorageAccess):
  # constructor
  def __init__(self, container, account, sas='', key=''):
    AzureStorageAccess.__init__(self, container, account, sas, key) 

  def load(self, path, header=True):
    _path = self.getFullpath(path)
    # print('laoding from ' + _path)
    return spark.read.format('csv').options(header=header, sep='\t', inferSchema='true').load(_path)

  def save(self, df, path, coalesce=False, header=True):
    _path = self.getFullpath(path)
    # print('saving to ' + _path)
    if coalesce:
      _tmppath = _path + '.' + str(uuid.uuid1())
      df.coalesce(1).write.mode('overwrite').csv(_tmppath, sep='\t', header=header)
      source_blob = [f.path for f in dbutils.fs.ls(_tmppath) if f.name.startswith('part-00000-')][0]
      dbutils.fs.mv(source_blob, _path)
      dbutils.fs.rm(_tmppath, recurse=True)
    else :
      df.write.mode('overwrite').csv(_path, sep='\t', header=header)

