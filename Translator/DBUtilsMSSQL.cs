using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data;
using System.Data.SqlClient;

namespace Translator
{
    public class DbUtilMSSQL : IDisposable
    {
        private string              mDatabase;
        private string              mConnectionInfo;
        private SqlConnection       mConnection;
        private static DbUtilMSSQL  mSingleton = null;

        public bool IsConnected
        {
            get
            {
                if (mConnection == null)
                {
                    return false;
                }

                return mConnection.State == ConnectionState.Open;
            }
        }

        public string ConnectionInfo
        {
            get
            {
                return mConnectionInfo;
            }
        }

        public static DbUtilMSSQL GetSingleton(string server, string database, string uid, string passwd)
        {
            if (mSingleton == null)
            {
                mSingleton = new DbUtilMSSQL();
                mSingleton.Connect(server, database, uid, passwd);
            }

            return mSingleton;
        }

        public static void ReleaseSingleton()
        {
            if (mSingleton != null)
            {
                mSingleton.Dispose();
            }
        }

        public DbUtilMSSQL()
        {
        }

        public void Connect(string server, string database, string uid, string passwd)
        {
            mDatabase = database;
            string connectionString = "data source=" + server + ";" +
                "Database=" + database + ";User Id=" + uid + ";Password=" + passwd + ";";
            mConnection = new System.Data.SqlClient.SqlConnection(connectionString);
            
            try
            {
                mConnection.Open();
                /*if ( database.Length > 0 && !DatabaseExist(database))
                {
                    mConnection.Close();
                    mConnection.Dispose();
                    mConnection = null;
                    mConnectionInfo = string.Format(ResStr.MSG_MISSING_SCHEMA_ON_SERVER, database, server); 
                }*/
            }
            catch (SqlException ex)
            {
                mConnectionInfo = ex.Message;
            }
        }

        public bool DatabaseExist(string databaseName)
        {
            string queryString = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", databaseName);
            //using (mConnection)
            //{
                using (SqlCommand sqlCmd = new SqlCommand(queryString, mConnection))
                {
                    int databaseID = (int)sqlCmd.ExecuteScalar();
                    if (databaseID > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            //}
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (mConnection != null)
                {
                    mConnection.Dispose();
                }
            }
        }
    }
}
