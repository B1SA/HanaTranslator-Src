using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data;

namespace Translator
{
    public class DBObjectsCache
    {
        //schema, objType, uppercase name, real db name
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> mCachedObjects = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        private Dictionary<string, string> mCachedSchemas = new Dictionary<string, string>();

        public DBObjectsCache()
        {

        }

        public void AddObject(string schemaName, string objType, string objectName)
        {
            Dictionary<string, Dictionary<string, string>> schema = mCachedObjects.ContainsKey(schemaName) ? schema = mCachedObjects[schemaName] : AddNewSchemaToCache(mCachedObjects, schemaName);
            Dictionary<string, string> objs = schema.ContainsKey(objType) ? schema[objType] : AddObjectTypeToSchema(schema, objType);

            objs[objectName.ToUpper()] = objectName;
        }

        public string[] GetObject(string schemaName, string objType, string objectName)
        {
            string[] ret = { null, null };

            if (Contains(schemaName, objType, objectName))
            {
                ret[1] = mCachedObjects[schemaName][objType][objectName];
                ret[0] = mCachedSchemas[schemaName];
            };

            return ret;
        }

        public bool Contains(string schemaName, string objType, string name)
        {
            if (mCachedObjects.ContainsKey(schemaName))
            {
                if (mCachedObjects[schemaName].ContainsKey(objType))
                {
                    if (mCachedObjects[schemaName][objType].ContainsKey(name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Dictionary<string, string> AddObjectTypeToSchema(Dictionary<string, Dictionary<string, string>> schema, string objType)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            schema[objType] = ret;

            return ret;
        }

        private Dictionary<string, Dictionary<string, string>> AddNewSchemaToCache(Dictionary<string, Dictionary<string, Dictionary<string, string>>> cache, string schemaName)
        {
            Dictionary<string, Dictionary<string, string>> objs = new Dictionary<string, Dictionary<string, string>>();
            cache[schemaName.ToUpper()] = objs;
            mCachedSchemas[schemaName.ToUpper()] = schemaName;

            return objs;
        }
    }

    public class DbUtil : IDisposable
    {
        private string          mSchema;
        private string          mConnectionInfo;
        private OdbcConnection  mConnection;
        private static DbUtil   mSingleton = null;

        private DBObjectsCache  mCachedObjects = new DBObjectsCache();

        #region Cached objects from DB
        class TableDefs 
        {
            Dictionary<string, List<string>> defs;

            public TableDefs()
            {
                defs = new Dictionary<string, List<string>>(); 
            }
        };


        private Dictionary<string, TableDefs>   mCachedColums = new Dictionary<string, TableDefs>();

        private List<string> GetCachedColumns(string schema, string table)
        {
            return null;
        }

        // Loads columns of given tables if not loaded
        private void LoadColumns(string schema, List<string> tables)
        {

        }

        #endregion //Cached objects from DB

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

        public static DbUtil GetSingleton(string server, string schema, string uid, string passwd)
        {
            if (mSingleton == null)
            {
                mSingleton = new DbUtil();
                mSingleton.Connect(server, schema, uid, passwd);
            }

            return mSingleton;
        }

        public static void ReleaseSingleton()
        {
            if (mSingleton != null)
            {
                mSingleton.Dispose();
                mSingleton = null;
            }
        }

        public DbUtil()
        {
        }

        public void Connect(string server, string schema, string uid, string passwd)
        {
            mSchema = schema;
            string connectionString = string.Format("DRIVER={{HDBODBC32}};UID={0};PWD={1};SERVERNODE={2};DATABASE={3}", uid, passwd, server, schema);
            
            if (IntPtr.Size == 8)//x64bit release
                connectionString = string.Format("DRIVER={{HDBODBC}};UID={0};PWD={1};SERVERNODE={2};DATABASE={3}", uid, passwd, server, schema);                

            mConnection = new OdbcConnection(connectionString);
            try
            {
                mConnection.Open();
                if ( schema.Length > 0 && !SchemaExist(schema))
                {
                    mConnection.Close();
                    mConnection.Dispose();
                    mConnection = null;
                    mConnectionInfo = string.Format(ResStr.MSG_MISSING_SCHEMA_ON_SERVER, schema, server); 
                }
            }
            catch (OdbcException ex)
            {
                mConnectionInfo = ex.Message;
            }
        }

        public bool SchemaExist(string schemaName)
        {
            string queryString = string.Format("select SCHEMA_NAME from SYS.OBJECTS where OBJECT_TYPE = 'SCHEMA' and SCHEMA_NAME = ?");
            OdbcCommand cmd = new OdbcCommand(queryString, mConnection);

            cmd.Parameters.Add("@schema", OdbcType.VarChar, 64).Value = schemaName;
            return cmd.ExecuteNonQuery() == 1;
        }

        public string[] GetCachedObjectName(string identifier, string objType)
        {
            string[] ret = null;
            string[] idens = identifier.Split('.');

            if (idens.Length == 1)
            {
                ret = mCachedObjects.GetObject(mSchema.ToUpper(), objType, idens[0].ToUpper());
            }
            else
            {
                ret = mCachedObjects.GetObject(idens[0].ToUpper(), objType, idens[1].ToUpper());
            }

            return ret;
        }

        public int LoadObjectNames(string schemaName, string objType, HashSet<string> names)
        {
            string objNames = "";
            int cnt = 0;

            foreach (string name in names)
            {
                if (mCachedObjects.Contains(schemaName, objType, name) == false)
                {
                    objNames += "'" + name + "',";
                    ++cnt;
                }
            }

            if (cnt == 0)
            {
                //all objects are already stored in cache
                return 0;
            }

            string objTypes = (objType.ToUpper() == "TABLE") ? "'TABLE','VIEW'" : "'" + objType + "'";
            objNames = objNames.TrimEnd(',');

            string queryString = string.Format("select OBJECT_NAME, SCHEMA_NAME from SYS.OBJECTS T0 where UPPER(T0.SCHEMA_NAME) = UPPER(?) AND UPPER(T0.OBJECT_NAME) IN (" + objNames + ") AND UPPER(T0.OBJECT_TYPE) IN (" + objTypes + ")");
            cnt = 0;

            using (OdbcCommand cmd = new OdbcCommand(queryString, mConnection))
            {
                cmd.Parameters.Add("@schema", OdbcType.VarChar, 64).Value = schemaName;

                using (OdbcDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        mCachedObjects.AddObject(dataReader.GetString(1), objType, dataReader.GetString(0));
                        ++cnt;
                    }
                }
            }

            return cnt;
        }

        public List<string> GetColumnsNames(ref SortedDictionary<string, List<string>> columntable, string table, List<string> arguments)
        {
            string ret = null;
            List<string> retlist = new List<string>();

            string[] args = table.Split('.');
            string schema = string.Empty;
            if (args.Count() > 1)
            {
                schema = args[0];
                schema = schema.ToUpper();
                ret = schema;
            }
            if (schema == "")
                schema = mSchema;
            table = args.Last();

            string queryString = string.Format("select T0.COLUMN_NAME, T1.OBJECT_NAME from SYS.COLUMNS T0 inner join SYS.OBJECTS T1 on T0.TABLE_OID = T1.OBJECT_OID where UPPER(T1.SCHEMA_NAME) = UPPER(?) and UPPER(T1.OBJECT_NAME) = UPPER(?) and UPPER(T0.COLUMN_NAME) in (");

            for (int i = 0; i < arguments.Count;i++ )
            {
                queryString += " UPPER('" + arguments[i] + "'),";
            }

            queryString = queryString.TrimEnd(',');
            queryString += ")";

            using (OdbcCommand cmd = new OdbcCommand(queryString, mConnection))
            {
                cmd.Parameters.Add("@schema", OdbcType.VarChar, 64).Value = schema;
                cmd.Parameters.Add("@objName", OdbcType.VarChar, 128).Value = table;

                using (OdbcDataReader dataReader = cmd.ExecuteReader())
                {
                    string m_column = null, m_table = null;
                    while (dataReader.Read())
                    {
                        m_column = dataReader.GetString(0);
                        m_table = dataReader.GetString(1);
                        try
                        {
                            columntable.Add(m_column, new List<string> { m_table });
                        }
                        catch (ArgumentException)
                        {
                            columntable[m_column].Add(m_table);
                            columntable[m_column] = columntable[m_column].Distinct().ToList();
                        }
                        ret += "." + m_table + "." + m_column;
                        ret = ret.TrimStart('.');
                        retlist.Add(ret);
                        ret = string.Empty;
                    }
                }
            }
            return retlist;
        }

        public List<string> GetIndexesNames(ref SortedDictionary<string, List<string>> indexTable, string table, List<string> arguments)
        {
            string ret = null;
            List<string> retlist = new List<string>();

            string[] args = table.Split('.');
            string schema = string.Empty;

            if (args.Count() > 1)
            {
                schema = args[0].ToUpper();
                ret = schema;
            }

            if (string.IsNullOrEmpty(schema))
            {
                schema = mSchema;
            }
            table = args.Last();

            string queryString = string.Format("SELECT T0.INDEX_NAME, T1.OBJECT_NAME FROM SYS.INDEXES T0 INNER JOIN SYS.OBJECTS T1 ON T0.TABLE_OID = T1.OBJECT_OID WHERE UPPER(T1.SCHEMA_NAME) = UPPER(?) AND UPPER(T0.INDEX_NAME) IN ({0})",
                arguments.Select(s => string.Format("UPPER('{0}')", s)).DefaultIfEmpty().Aggregate((aggr, item) => aggr + ", " + item));
            if (string.IsNullOrEmpty(table) == false)
            {
                queryString += string.Format(" AND UPPER(T1.OBJECT_NAME) = UPPER(?)"); 
            }

            using (OdbcCommand cmd = new OdbcCommand(queryString, mConnection))
            {
                cmd.Parameters.Add("@schema", OdbcType.VarChar, 64).Value = schema;
                if (string.IsNullOrEmpty(table) == false)
                {
                    cmd.Parameters.Add("@table", OdbcType.VarChar, 128).Value = table;
                }

                using (OdbcDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        string corrColumn = dataReader.GetString(0);
                        string corrTable = dataReader.GetString(1);

                        try
                        {
                            indexTable.Add(corrColumn, new List<string> { corrTable });
                        }
                        catch (ArgumentException)
                        {
                            indexTable[corrColumn].Add(corrTable);
                            indexTable[corrColumn] = indexTable[corrColumn].Distinct().ToList();
                        }

                        ret += string.Format(".{0}.{1}", corrTable, corrColumn);
                        ret = ret.TrimStart('.');

                        retlist.Add(ret);

                        ret = string.Empty;
                    }
                }
            }

            return retlist;
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
