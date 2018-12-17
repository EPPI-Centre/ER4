using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
#if !SILVERLIGHT
using System.Data;
using System.Data.SqlClient;
#endif
namespace BusinessLibrary.BusinessClasses
{
    public static class Cryptography
    {//adapted from http://weblogs.asp.net/stevesheldon/archive/2008/10/25/simple-cryptography-block.aspx
        public static string[] whitelist = {"txt" 
                                 ,".txt"
                                 , ".doc"
                                 , ".docx"
                                 , ".pdf"
                                 , ".ppt"
                                 , ".pps"
                                 ,".pptx"
                                 , ".ppsx"
                                 ,".xls"
                                 ,".xlsx"
                                 , ".htm"
                                 , ".html"
                                 ,".odt"
                                 ,".ods"
                                 ,".odp"
                                 ,".ps"
                                 ,".eps"
                                 ,".csv"};
        public static string Encrypt(string instring, string passwd)
        {
            //Get the encryption key from passwd, adding some text just in case passwd is too short
            byte[] keyBytes = UTF8Encoding.UTF8.GetBytes(passwd + "pippobaudo");
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(passwd + "pippobaudo", keyBytes, 1000);

            // Use the AES managed encryption provider
            AesManaged encryptor = new AesManaged();
            encryptor.Key = rfc.GetBytes(16);
            encryptor.IV = rfc.GetBytes(16);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream encrypt = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] dataBytes = new UTF8Encoding(false).GetBytes(instring);

                    encrypt.Write(dataBytes, 0, dataBytes.Length);
                    encrypt.FlushFinalBlock();
                    encrypt.Close();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        public static bool IsAllowedExtension(string ext)
        {
            bool res = false;
            foreach (string whit in whitelist)
            {
                if (ext.ToLower().IndexOf(whit) > -1)
                {
                    res = true;
                    break;
                }
            }
            return res;
        }
#if !SILVERLIGHT
        public static string Decrypt(string instring, string passwd)
        {
            //Get the decryption key from from passwd, adding some text just in case passwd is too short
            byte[] keyBytes = new UTF8Encoding(false).GetBytes(passwd + "pippobaudo");
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(passwd + "pippobaudo", keyBytes, 1000);
            int ln = instring.Length;
            AesManaged decryptor = new AesManaged();
            decryptor.Key = rfc.GetBytes(16);
            decryptor.IV = rfc.GetBytes(16);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream decrypt = new CryptoStream(ms, decryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    byte[] dataBytes = Convert.FromBase64String(instring);
                    decrypt.Write(dataBytes, 0, dataBytes.Length);
                    decrypt.FlushFinalBlock();
                    decrypt.Close();
                    string res = new UTF8Encoding(false).GetString(ms.ToArray());
                    return res;
                }
            }
        }
        private static string getPass(int UserID, string GUID)
        {
            string res = "";
            SqlConnection conn = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString);
            try
            {
                conn.Open();
                SqlCommand Cmd = new SqlCommand("st_ContactPasswordFromID", conn);
                SqlParameter ID = Cmd.Parameters.Add("@ID", SqlDbType.Int);
                Cmd.CommandType = CommandType.StoredProcedure;
                ID.Value = UserID;
                SqlParameter sqlGUID = Cmd.Parameters.Add("@GUI", SqlDbType.UniqueIdentifier);
                Cmd.CommandType = CommandType.StoredProcedure;
                sqlGUID.Value = new Guid( GUID);
                SqlDataReader dr = Cmd.ExecuteReader();
                dr.Read();
                res = (string)dr["PASSWORD"];
            }
            finally
            {
                if (conn.State != System.Data.ConnectionState.Closed)
                {
                    conn.Close();
                }
            }
            return res;
        }
        public static bool IsAllowed(string GUID,  string E_RevID, string E_ItemDID, out int RevID, out int ItemDID)//this is used when asking to download a bin doc
        {
            BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            ItemDID = -1;
            RevID = 0;
            int userID = 0, suppRevID = 0;
            //step1 get userID & RevID from GUID
            try
            {
                using (SqlConnection aconn = new SqlConnection(BusinessLibrary.Data.DataConnection.AdmConnectionString))
                {
                    aconn.Open();
                    SqlCommand Cmd = new SqlCommand("st_LogonTicket_Get_UserID_FROM_GUID", aconn);
                    Cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter pGUID = Cmd.Parameters.Add("@guid", SqlDbType.UniqueIdentifier);
                    pGUID.Value = System.Guid.Parse(GUID);
                    Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(Cmd.ExecuteReader());

                    if (!reader.Read())
                    {
                        aconn.Close();
                        return false;
                    }
                    RevID = reader.GetInt32("Review_id");
                    userID = reader.GetInt32("CONTACT_ID");
                }
                if (RevID == 0 || userID == 0) return false;
                //step2 get password from userID
                string pass = getPass(userID, GUID);
                if (pass == null || pass == "") return false;

                //step3 decrypt E_RevID & E_ItemDID, check if values supplied match the ones from step1
                if (!Int32.TryParse(Decrypt(E_RevID, pass), out suppRevID)) return false;
                if (suppRevID != RevID) return false;
                if (!Int32.TryParse(Decrypt(E_ItemDID, pass), out ItemDID)) return false;
                
            }
            catch
            {//soemthing went wrong, report failure
                return false;
            }
            //return success
            return true;
        }
        public static bool IsAllowed(string GUID, string E_ItemID, string E_RevID, out int RevID, out Int64 ItemID)//this is used when uploading a bin doc
        {
            RevID = 0;
            int userID = 0, suppRevID = 0;
            ItemID = 0;
            //step1 get userID & RevID from GUID
            try
            {
                using (SqlConnection aconn = new SqlConnection(BusinessLibrary.Data.DataConnection.AdmConnectionString))
                {
                    aconn.Open();
                    SqlCommand Cmd = new SqlCommand("st_LogonTicket_Get_UserID_FROM_GUID", aconn);
                    Cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter pGUID = Cmd.Parameters.Add("@guid", SqlDbType.UniqueIdentifier);
                    pGUID.Value = System.Guid.Parse(GUID);
                    Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(Cmd.ExecuteReader());

                    if (!reader.Read())
                    {
                        aconn.Close();
                        return false;
                    }
                    RevID = reader.GetInt32("Review_id");
                    userID = reader.GetInt32("CONTACT_ID");
                }
                if (RevID == 0 || userID == 0) return false;
                //step2 get password from userID
                string pass = getPass(userID, GUID);
                if (pass == null || pass == "") return false;

                //step3 decrypt E_RevID & E_ItemID, check if values supplied match the ones from step1
                if (!Int64.TryParse(Decrypt(E_ItemID, pass), out ItemID)) return false;
                if (!Int32.TryParse(Decrypt(E_RevID, pass), out suppRevID)) return false;
                if (suppRevID != RevID) return false;
                using (SqlConnection conn = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString))
                {
                    conn.Open();
                    SqlCommand Cmd = new SqlCommand("st_Item", conn);
                    Cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter pRevID = Cmd.Parameters.Add("@REVIEW_ID", SqlDbType.Int);
                    pRevID.Value = RevID;
                    SqlParameter pItemID = Cmd.Parameters.Add("@ITEM_ID", SqlDbType.BigInt);
                    pItemID.Value = ItemID;
                    Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(Cmd.ExecuteReader());
                    if (!reader.Read() || ItemID != reader.GetInt64("ITEM_ID"))//here we check if the supplied item ID exists in the supplied review
                    {
                        conn.Close();
                        return false;
                    }
                }
            }
            catch
            {//soemthing went wrong, report failure
                return false;
            }
            //return success
            return true;
        }

        

#endif
    }
}
