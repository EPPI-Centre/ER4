using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;

// added for using Lingo3G
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Net;
using System.Collections.Specialized;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class PerformClusterCommand : CommandBase<PerformClusterCommand>
    {
        public PerformClusterCommand(){}

        private string _itemList;
        private string _attributeSetList;
        private int _maxHierarchyDepth;
        private double _minClusterSize;
        private double _maxClusterSize;
        private double _singleWordLabelWeight;
        private int _minLabelLength;
        private bool _useUploadedDocs;
        private int _reviewSetIndex;

        public PerformClusterCommand(string itemList, int maxHierarchyDepth, double minClusterSize, double maxClusterSize,
            double singleWordLabelWeight, int minLabelLength, string attributeSetList, bool useUploadedDocs, int reviewSetIndex)
        {
            _itemList = itemList;
            _maxHierarchyDepth = maxHierarchyDepth;
            _minClusterSize = minClusterSize;
            _maxClusterSize = maxClusterSize;
            _singleWordLabelWeight = singleWordLabelWeight;
            _minLabelLength = minLabelLength;
            _attributeSetList = attributeSetList;
            _useUploadedDocs = useUploadedDocs;
            _reviewSetIndex = reviewSetIndex;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_itemList", _itemList);
            info.AddValue("_maxHierarchyDepth", _maxHierarchyDepth);
            info.AddValue("_minClusterSize", _minClusterSize);
            info.AddValue("_maxClusterSize", _maxClusterSize);
            info.AddValue("_singleWordLabelWeight", _singleWordLabelWeight);
            info.AddValue("_minLabelLength", _minLabelLength);
            info.AddValue("_attributeSetList", _attributeSetList);
            info.AddValue("_useUploadedDocs", _useUploadedDocs);
            info.AddValue("_reviewSetIndex", _reviewSetIndex);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _itemList = info.GetValue<string>("_itemList");
            _maxHierarchyDepth = info.GetValue<int>("_maxHierarchyDepth");
            _minClusterSize = info.GetValue<double>("_minClusterSize");
            _maxClusterSize = info.GetValue<double>("_maxClusterSize");
            _singleWordLabelWeight = info.GetValue<double>("_singleWordLabelWeight");
            _minLabelLength = info.GetValue<int>("_minLabelLength");
            _attributeSetList = info.GetValue<string>("_attributeSetList");
            _useUploadedDocs = info.GetValue<bool>("_useUploadedDocs");
            _reviewSetIndex = info.GetValue<int>("_reviewSetIndex");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            string xml = "";
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                // Filtered or all items?

                using (SqlCommand command = new SqlCommand("st_ClusterGetXmlAll", connection))
                {

                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));

                    if (_useUploadedDocs == false)
                    {
                        if (_itemList != "")
                        {
                            command.CommandText = "st_ClusterGetXmlFiltered";
                            command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", _itemList));
                        }
                        if (_attributeSetList != "")
                        {
                            command.CommandText = "st_ClusterGetXmlFilteredCode";
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID_LIST", _attributeSetList));
                        }
                    }
                    else
                    {
                        command.CommandText = "st_ClusterGetXmlAllDocs";
                        if (_itemList != "")
                        {
                            command.CommandText = "st_ClusterGetXmlFilteredDocs";
                            command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", _itemList));
                        }
                        if (_attributeSetList != "")
                        {
                            command.CommandText = "st_ClusterGetXmlFilteredCodeDocs";
                            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID_LIST", _attributeSetList));
                        }
                    }


                    using (System.Xml.XmlReader reader = command.ExecuteXmlReader())
                    {

                        if (reader.Read())
                        {

                            while (reader.ReadState != System.Xml.ReadState.EndOfFile)
                            {
                                xml += reader.ReadOuterXml();

                            }
                        }
                        reader.Close();
                    }

                }

                XmlDocument document = GetClusters(xml, _maxHierarchyDepth, _minClusterSize, _maxClusterSize, _singleWordLabelWeight,
                    _minLabelLength);

                SqlCommand command2 = new SqlCommand("st_ReviewSetInsert", connection);
                command2.CommandType = System.Data.CommandType.StoredProcedure;
                command2.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                command2.Parameters.Add(new SqlParameter("@SET_NAME", "Lingo3G clusters"));
                command2.Parameters.Add(new SqlParameter("@NEW_SET_ID", 0));
                command2.Parameters.Add(new SqlParameter("@NEW_REVIEW_SET_ID", 0));
                command2.Parameters.Add(new SqlParameter("@SET_ORDER", _reviewSetIndex));
                command2.Parameters["@NEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                command2.Parameters["@NEW_REVIEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                command2.ExecuteNonQuery();
                Int64 SetId = Convert.ToInt64(command2.Parameters["@NEW_SET_ID"].Value.ToString());

                foreach (XmlNode group in
                    document.SelectNodes("/searchresult/group"))
                {
                    SaveAttribute(connection, 0, SetId, group);
                    //string doccount = "Documents: " + group.SelectNodes("document").Count.ToString();
                }
                connection.Close();
            }
        }

        public static XmlDocument GetClusters(string data, int maxHierarchyDepth, double minClusterSize, double maxClusterSize, double singleWordLabelWeight,
            int minLabelLength)
        {
            MultipartFileUpload upload = new MultipartFileUpload();
            //use the following URI when running outside ioe and you have Lingo3G active @Localhost:8090
            //upload.Uri = new Uri("http://localhost:8090/rest/processor");

            //use the following URI if running within the IOE firewall
            upload.Uri = new Uri("http://localhost:8080/dcs/rest");

            string tmp = Dns.GetHostName().ToLower();
            if (tmp == "epi3" || tmp == "eppi.ioe.ac.uk" || tmp == "eppi" || tmp == "eppi-management")
            {
                upload.Uri = new Uri("***REMOVED***");
            }



            // 'xml' or 'json'
            upload.AddFormValue("dcs.default.output", "xml");

            // If not present, documents are also returned. Usually
            // not required.
            upload.AddFormValue("dcs.clusters.only", "true");

            // The algorithm to use for clustering. Leave empty
            // for default.
            upload.AddFormValue("dcs.algorithm", "");

            upload.AddFormValue("max-hierarchy-depth", maxHierarchyDepth.ToString());
            upload.AddFormValue("min-cluster-size", minClusterSize.ToString());
            upload.AddFormValue("max-cluster-size", maxClusterSize.ToString());
            upload.AddFormValue("single-word-label-weight", singleWordLabelWeight.ToString());
            upload.AddFormValue("min-label-words", minLabelLength.ToString());

            // testing using clusters for duplicate identification
            //upload.AddFormValue("precise-document-assignment", "true");
            //upload.AddFormValue("max-clustering-passes-top", "0");
            //upload.AddFormValue("document-coverage-target", "1.0");
            //upload.AddFormValue("max-improvement-iterations", "0");

            // Add the XML stream here. It can be read directly
            // from a file or just placed as a string.

            // upload.AddFormValue("c2stream", "xml-string-here");

            //use the following when using the db-epi service (when running within IOE)
            upload.AddFormValue("dcs.c2stream", data);

            //use this version instead if running a local Lingo3G version that is not 1.3.1 or later
            //upload.AddFormValue("c2stream", data);

            //upload.AttachFile("input.xml", "c2stream", new FileInfo(@"c:\james\data-mining.xml"));
            //upload.AttachFile("input.xml", "dcs.c2stream", new FileInfo(@"d:\temp\test.xml"));
            // Perform the actual query.
            byte[] response = upload.UploadFileEx();

            // Parse the output and dump group headers.
            MemoryStream input = new MemoryStream(response);

            //XmlDocument document = new XmlDocument();
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(input);
            return doc;
        }

        private void SaveAttribute(SqlConnection connection, Int64 ParentAttributeId, Int64 SetId, XmlNode node)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            SqlCommand command = new SqlCommand("st_AttributeSetInsert", connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@SET_ID", SetId));
            command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", ParentAttributeId));
            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_TYPE_ID", 2)); // 2 = standard selectable attribute
            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_NAME", node.SelectSingleNode("title/phrase").InnerText));
            command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_ID", 0));
            command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_SET_ID", 0));
            command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
            command.Parameters["@NEW_ATTRIBUTE_SET_ID"].Direction = System.Data.ParameterDirection.Output;
            command.Parameters["@NEW_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
            command.ExecuteNonQuery();
            Int64 AttributeId = Int64.Parse(command.Parameters["@NEW_ATTRIBUTE_ID"].Value.ToString());
            string itemIds = "";
            foreach (XmlNode doc in node.SelectNodes("document"))
            {
                if (itemIds == "")
                {
                    itemIds = doc.Attributes["refid"].Value;
                }
                else
                {
                    itemIds += "," + doc.Attributes["refid"].Value;
                }
            }
            if (itemIds != "")
            {
                SqlCommand command2 = new SqlCommand("st_ItemAttributeBulkInsert", connection);
                command2.CommandType = System.Data.CommandType.StoredProcedure;
                command2.Parameters.Add(new SqlParameter("@SET_ID", SetId));
                command2.Parameters.Add(new SqlParameter("@IS_COMPLETED", true));
                command2.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                command2.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", AttributeId));
                command2.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                command2.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", itemIds));
                command2.CommandTimeout = 1200;
                command2.ExecuteNonQuery();
            }
            foreach (XmlNode group in node.SelectNodes("group"))
            {
                SaveAttribute(connection, AttributeId, SetId, group);
            }

        }

        //
        // This class based on an example from:
        // http://blog.mindbridge.com/?p=59
        //
        // With modifications.
        //
        public class MultipartFileUpload
        {
            private Uri uri = null;

            private NameValueCollection parameters = null;
            private NameValueCollection formData = null;

            private String fileDisplayName = null;
            private String fileParameterName = null;

            private String boundary;

            private FileInfo file = new FileInfo(@"d:\temp\test.xml");
            private NetworkCredential networkCredential = null;
            private readonly int BUFFER_SIZE = 500;

            public MultipartFileUpload()
            {
                parameters = new NameValueCollection();
                formData = new NameValueCollection();
            }

            public MultipartFileUpload(Uri uri)
                : this()
            {
                this.uri = uri;
            }

            #region getters and setters
            /// <summary>
            /// the destination of the multipart file upload
            /// </summary>
            public Uri Uri
            {
                get { return this.uri; }
                set { this.uri = value; }
            }

            /// <summary>
            /// the parameters to be passed along with the post method — optional
            /// </summary>
            public NameValueCollection Parameters
            {
                get { return this.parameters; }
                set { this.parameters = value; }
            }

            /// <summary>
            /// any form data associated with the post method
            /// </summary>
            public NameValueCollection FormData
            {
                get { return this.formData; }
                set { this.formData = value; }
            }

            /// <summary>
            /// if your web server requires credentials to upload a file, set them here — optional
            /// </summary>
            public NetworkCredential NetworkCredential
            {
                get { return this.networkCredential; }
                set { this.networkCredential = value; }
            }

            #endregion

            #region public methods

            /// <summary>
            /// adds a parameter to the request, if the parameter already has a value,
            /// it will be overwritten
            /// </summary>
            /// <param name="name">name of the parameter</param>
            /// <param name="value">value of the parameter</param>
            public void AddParameter(String name, String value)
            {
                if (this.parameters == null)
                {
                    this.parameters = new NameValueCollection();
                }
                this.parameters.Set(name, value);
            }

            /// <summary>
            /// adds a form value to the request, if the form field already exists, overwrite it
            /// </summary>
            /// <param name="name">the name of the form field</param>
            /// <param name="value">value of the form field</param>
            public void AddFormValue(String name, String value)
            {
                if (this.formData == null)
                {
                    this.formData = new NameValueCollection();
                }
                this.formData.Set(name, value);
            }

            /// <summary>
            /// attach a file to the post method, the parameter name and file can not be null
            /// </summary>
            /// <param name="fileDisplayName">the name of the file as it should appear to the web server</param>
            /// <param name="parameterName">the parameter that your web server expects to be associated with a file</param>
            /// <param name="file">the actual file you want to upload</param>
            /// <exception cref="ArgumentNullException">file can not be null, name of the parameter can’t be null</exception>
            public void AttachFile(String fileDisplayName, String parameterName, FileInfo file)
            {
                if (file == null)
                {
                    throw new ArgumentNullException("file", "You must pass a reference to a file");
                }
                if (parameterName == null)
                {
                    throw new ArgumentNullException("parameterName", "You must provide the name of the file parameter.");
                }

                this.file = file;
                this.fileParameterName = parameterName;
                this.fileDisplayName = fileDisplayName;
            }

            /// <summary>
            /// performs the actual upload
            /// </summary>
            /// <returns>the response as a byte array</returns>
            public byte[] UploadFileEx()
            {
                // generate boundary
                boundary = "-----------boundary" + DateTime.Now.Ticks.ToString("x");

                // Tack on any parameters or just give us back the uri if there are no parameters
                Uri targetUri = CreateUriWithParameters();

                HttpWebRequest webrequest = CreateHwr(targetUri);
                webrequest.Credentials = networkCredential; //fine if it’s null
                webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
                webrequest.Method = "POST";

                // Encode form parameters
                String postHeader = CreatePostDataString1();
                byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);
                byte[] endBoundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--");

                // Read in the file as a stream, if it exists.
                long length = postHeaderBytes.Length + endBoundaryBytes.Length;
                //FileStream fileStream = null;
                //if (file != null)
                //{
                //    fileStream = file.Open(FileMode.Open, FileAccess.Read);

                //    length += fileStream.Length + endBoundaryBytes.Length;
                //}

                webrequest.ContentLength = length;
                Stream requestStream = Stream.Null;
                try
                {
                    requestStream = webrequest.GetRequestStream();
                    requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                }
                catch (Exception e)
                { throw e; }
                // Write out the file contents
                byte[] buffer = new byte[BUFFER_SIZE];
                int bytesRead = 0;
                //if (fileStream != null)
                //{
                //    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                //    {
                //        requestStream.Write(buffer, 0, bytesRead);
                //    }
                //    requestStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                //    fileStream.Close();
                //}

                webrequest.Timeout = 1000 * 6000;
                requestStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                MemoryStream memStream = new MemoryStream();
                try
                {
                    HttpWebResponse response = (HttpWebResponse)webrequest.GetResponse();
                    Stream responseStream = response.GetResponseStream();

                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }

                    responseStream.Close();
                    response.Close();
                }
                catch (WebException we)
                {
                    Console.Write(we.Message);
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }


                return memStream.ToArray();
            }
#if (!CSLA_NETCORE)
            public HttpWebRequest CreateHwr(Uri uri)
            {
                HttpWebRequest r;
                Type t = typeof(HttpWebRequest);
                ConstructorInfo ctor = t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Uri), typeof(ServicePoint) }, null);
                r = (HttpWebRequest)ctor.Invoke(new object[] { uri, null });
                r.Timeout = 1000 * 400;
                return r;
            }
#else
            public HttpWebRequest CreateHwr(Uri uri)
            {
                HttpWebRequest r = (HttpWebRequest)WebRequest.Create(uri.AbsoluteUri);
                r.Timeout = 1000 * 400;
                return r;
            }
#endif
#endregion

            #region private methods

            /// <summary>
            /// helper method to tack on parameters to the request
            /// </summary>
            /// <returns>Uri with parameters, or the original uri if it’s null</returns>
            private Uri CreateUriWithParameters()
            {
                if (uri == null) return null;
                if (parameters == null || parameters.Count <= 0)
                {
                    return this.uri;
                }
                String paramString = "?";
                foreach (String key in parameters.Keys)
                {
                    paramString += key + "=" + parameters.Get(key) + "&";
                }
                paramString = paramString.Substring(0, paramString.Length - 1); //strip off last &
                return new Uri(uri.ToString() + paramString);
            }

            /// <summary>
            /// post data as a string with the boundaries
            /// </summary>
            /// <returns>a string representing the form data</returns>
            private String CreatePostDataString()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < formData.Count; i++)
                {
                    sb.Append("--" + boundary + "\r\n");
                    sb.Append("Content-Disposition: form-data; name=\"");
                    sb.Append(formData.GetKey(i) + "\"\r\n\r\n" + formData.Get(i) + "\r\n");
                }

                if (file != null)
                {
                    sb.Append("--" + boundary + "\r\n");
                    sb.Append("Content-Disposition: form-data; name=\"" + fileParameterName + "\"; ");
                    sb.Append("filename=\"" + fileDisplayName + "\"\r\n");
                    sb.Append("Content-Type: application/octet-stream\r\n\r\n");
                }

                return sb.ToString();
            }
            private String CreatePostDataString1()
            {
                string res = "";
                //StringBuilder sb = new StringBuilder();
                for (int i = 0; i < formData.Count; i++)
                {
                    res += "--" + boundary + "\r\n";
                    res += "Content-Disposition: form-data; name=\"";
                    res += formData.GetKey(i) + "\"\r\n\r\n" + formData.Get(i) + "\r\n";
                }

                res += "--" + boundary + "\r\nContent-Type: application/octet-stream\r\n\r\n";
                //res += "Content-Disposition: form-data; name=\"" + fileParameterName + "\"; ";
                //res += "Content-Disposition: form-data; name=\"f.xml\"; ";
                //res += "filename=\"" + fileDisplayName + "\"\r\n";
                //res += "";
                return res;
                //sb.Append("--" + boundary + "\r\n");
                //sb.Append("Content-Disposition: form-data; name=\"" + fileParameterName + "\"; ");
                //sb.Append("Content-Disposition: form-data; name=\"f.xml\"; ");
                //sb.Append("filename=\"" + fileDisplayName + "\"\r\n");
                //sb.Append("Content-Type: application/octet-stream\r\n\r\n");
                //return sb.ToString();
            }
            #endregion
        }
#endif
        }
}
