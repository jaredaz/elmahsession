#region License, Terms and Author(s)
//
// ELMAH - Error Logging Modules and Handlers for ASP.NET
// Copyright (c) 2004-9 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      James Driscoll, mailto:jamesdriscoll@btinternet.com
//      with contributions from Hath1
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

[assembly: Elmah.Scc("$Id: OracleErrorLog.cs 926 2011-12-23 22:50:57Z azizatif $")]

namespace Elmah
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using IDictionary = System.Collections.IDictionary;

    #endregion

    /// <summary>
    /// An <see cref="ErrorLog"/> implementation that uses Oracle as its backing store.
    /// </summary>

    public class OracleErrorLog : ErrorLog
    {
        private readonly string _connectionString;
        private string _schemaOwner;
        private bool _schemaOwnerInitialized;

        private const int _maxAppNameLength = 60;
        private const int _maxSchemaNameLength = 30;

        private static DbProviderFactory _dbProviderFactory;
        private static PropertyInfo _providerSpecificTypeProperty;
        private static object _clobDbType;
        private static object _refCursorDbType;

        private static DbParameter AddParameter(DbCommand command, string parameterName)
        {
            Debug.Assert(_providerSpecificTypeProperty != null);
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            command.Parameters.Add(parameter);
            return parameter;
        }

        private static DbParameter AddGenericTypeParameter(DbCommand command, string parameterName, DbType dbType)
        {
            var parameter = AddParameter(command, parameterName);
            parameter.DbType = dbType;
            return parameter;
        }

        private static DbParameter AddProviderSpecificTypeParameter(DbCommand command, string parameterName, object dbType)
        {
            var parameter = AddParameter(command, parameterName);
            _providerSpecificTypeProperty.SetValue(parameter, dbType, null);
            return parameter;
        }

        private static bool TryAndCreateDbProviderFactory(string providerInvariantName)
        {
            try
            {
                _dbProviderFactory = DbProviderFactories.GetFactory(providerInvariantName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        static void GetProviderSpecificType()
        {
            if (_dbProviderFactory == null)
                return;

            var parameter = _dbProviderFactory.CreateParameter();
            Debug.Assert(parameter != null);
            var parameterType = parameter.GetType();
            _providerSpecificTypeProperty = (from property in parameterType.GetProperties() 
                                            let attributes = property.GetCustomAttributes(typeof (DbProviderSpecificTypePropertyAttribute), false) 
                                            from attribute in attributes
                                            where ((DbProviderSpecificTypePropertyAttribute) attribute).IsProviderSpecificTypeProperty 
                                            select property).FirstOrDefault();
        }

        static object GetProviderSpecificDbType(params string[] dbTypes)
        {
            Debug.Assert(_providerSpecificTypeProperty != null);
            var enumValues = Enum.GetValues(_providerSpecificTypeProperty.PropertyType);

            foreach (var dbType in dbTypes)
                foreach (var enumValue in enumValues)
                    if (enumValue.ToString().Equals(dbType, StringComparison.OrdinalIgnoreCase))
                        return enumValue;

            return null;
        }
        
        static OracleErrorLog()
        {
            if (!TryAndCreateDbProviderFactory("Oracle.DataAccess.Client") &&
                !TryAndCreateDbProviderFactory("System.Data.OracleClient"))
                return;
            
            GetProviderSpecificType();
            _clobDbType = GetProviderSpecificDbType("NClob");
            _refCursorDbType = GetProviderSpecificDbType("Cursor", "RefCursor");
        }

        private DbConnection CreateOpenConnection()
        {
            Debug.Assert(_dbProviderFactory != null);
            var connection = _dbProviderFactory.CreateConnection();
            Debug.Assert(connection != null);
            connection.ConnectionString = ConnectionString;
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleErrorLog"/> class
        /// using a dictionary of configured settings.
        /// </summary>

        public OracleErrorLog(IDictionary config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            var connectionString = ConnectionStringHelper.GetConnectionString(config);

            //
            // If there is no connection string to use then throw an 
            // exception to abort construction.
            //

            if (connectionString.Length == 0)
                throw new ApplicationException("Connection string is missing for the Oracle error log.");

            _connectionString = connectionString;

            //
            // Set the application name as this implementation provides
            // per-application isolation over a single store.
            //

            var appName = (string) config["applicationName"] ?? string.Empty;

            if (appName.Length > _maxAppNameLength)
            {
                throw new ApplicationException(string.Format(
                    "Application name is too long. Maximum length allowed is {0} characters.",
                    _maxAppNameLength.ToString("N0")));
            }

            ApplicationName = appName;

            SchemaOwner = (string)config["schemaOwner"];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleErrorLog"/> class
        /// to use a specific connection string for connecting to the database.
        /// </summary>

        public OracleErrorLog(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");

            if (connectionString.Length == 0)
                throw new ArgumentException(null, "connectionString");

            _connectionString = connectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleErrorLog"/> class
        /// to use a specific connection string for connecting to the database and
        /// a specific schema owner.
        /// </summary>

        public OracleErrorLog(string connectionString, string schemaOwner)
            : this(connectionString)
        {
            SchemaOwner = schemaOwner;
        }

        /// <summary>
        /// Gets the name of the schema owner where the errors are being stored.
        /// </summary>

        public string SchemaOwner
        {
            get { return _schemaOwner ?? string.Empty; }

            set
            {
                if (_schemaOwnerInitialized)
                    throw new InvalidOperationException("The schema owner cannot be reset once initialized.");

                if (string.IsNullOrEmpty(value))
                    return;

                if (value.Length > 0)
                {
                    if (value.Length > _maxSchemaNameLength)
                        throw new ApplicationException(string.Format(
                            "Oracle schema owner is too long. Maximum length allowed is {0} characters.",
                            _maxSchemaNameLength.ToString("N0")));
                }

                _schemaOwner = value + ".";
                _schemaOwnerInitialized = true;
            }
        }

        /// <summary>
        /// Gets the name of this error log implementation.
        /// </summary>

        public override string Name
        {
            get { return "Oracle Error Log"; }
        }

        /// <summary>
        /// Gets the connection string used by the log to connect to the database.
        /// </summary>

        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        /// Logs an error to the database.
        /// </summary>
        /// <remarks>
        /// Use the stored procedure called by this implementation to set a
        /// policy on how long errors are kept in the log. The default
        /// implementation stores all errors for an indefinite time.
        /// </remarks>

        public override string Log(Error error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            var errorXml = ErrorXml.EncodeString(error);
            var id = Guid.NewGuid();

            using (var connection = CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    // because we are storing the XML data in a NClob, we need to jump through a few hoops!!
                    // so first we've got to operate within a transaction
                    command.Transaction = transaction;

                    // then we need to create a temporary lob on the database server
                    command.CommandText = "declare xx nclob; begin dbms_lob.createtemporary(xx, false, 0); :tempblob := xx; end;";
                    command.CommandType = CommandType.Text;

                    var parameters = command.Parameters;
                    AddProviderSpecificTypeParameter(command, "tempblob", _clobDbType).Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();

                    object xmlValue;

                    if (parameters[0].Value is string)
                    {
                        xmlValue = errorXml + "<!-- " + new string('a', 70000) + " -->";
                    }
                    else
                    {
                        // now we can get a handle to the NClob
                        var xmlLob = (Stream)parameters[0].Value;
                        // create a temporary buffer in which to store the XML
                        var tempbuff = Encoding.Unicode.GetBytes(errorXml);
                        // and finally we can write to it!
                        xmlLob.Write(tempbuff, 0, tempbuff.Length);
                        xmlValue = xmlLob;
                    }

                    command.CommandText = SchemaOwner + "pkg_elmah$log_error.LogError";
                    command.CommandType = CommandType.StoredProcedure;

                    parameters.Clear();
                    AddGenericTypeParameter(command, "v_ErrorId", DbType.String).Value = id.ToString("N");
                    AddGenericTypeParameter(command, "v_Application", DbType.String).Value = ApplicationName;
                    AddGenericTypeParameter(command, "v_Host", DbType.String).Value = error.HostName;
                    AddGenericTypeParameter(command, "v_Type", DbType.String).Value = error.Type;
                    AddGenericTypeParameter(command, "v_Source", DbType.String).Value = error.Source;
                    AddGenericTypeParameter(command, "v_Message", DbType.String).Value = error.Message;
                    AddGenericTypeParameter(command, "v_User", DbType.String).Value = error.User;
                    AddProviderSpecificTypeParameter(command, "v_AllXml", _clobDbType).Value = xmlValue;
                    AddGenericTypeParameter(command, "v_StatusCode", DbType.Int32).Value = error.StatusCode;
                    AddGenericTypeParameter(command, "v_TimeUtc", DbType.DateTime).Value = error.Time.ToUniversalTime();

                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
                return id.ToString();
            }
        }

        /// <summary>
        /// Returns a page of errors from the databse in descending order 
        /// of logged time.
        /// </summary>

        public override int GetErrors(int pageIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);

            if (pageSize < 0)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

            using (var connection = CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = SchemaOwner + "pkg_elmah$get_error.GetErrorsXml";
                command.CommandType = CommandType.StoredProcedure;

                var parameters = command.Parameters;

                AddGenericTypeParameter(command, "v_Application", DbType.String).Value = ApplicationName;
                AddGenericTypeParameter(command, "v_PageIndex", DbType.Int32).Value = pageIndex;
                AddGenericTypeParameter(command, "v_PageSize", DbType.Int32).Value = pageSize;
                AddGenericTypeParameter(command, "v_TotalCount", DbType.Int32).Direction = ParameterDirection.Output;
                AddProviderSpecificTypeParameter(command, "v_Results", _refCursorDbType).Direction = ParameterDirection.Output;

                using (var reader = command.ExecuteReader())
                {
                    Debug.Assert(reader != null);

                    if (errorEntryList != null)
                    {
                        while (reader.Read())
                        {
                            var id = reader["ErrorId"].ToString();
                            var guid = new Guid(id);

                            var error = new Error();

                            error.ApplicationName = reader["Application"].ToString();
                            error.HostName = reader["Host"].ToString();
                            error.Type = reader["Type"].ToString();
                            error.Source = reader["Source"].ToString();
                            error.Message = reader["Message"].ToString();
                            error.User = reader["UserName"].ToString();
                            error.StatusCode = Convert.ToInt32(reader["StatusCode"]);
                            error.Time = Convert.ToDateTime(reader["TimeUtc"]).ToLocalTime();

                            errorEntryList.Add(new ErrorLogEntry(this, guid.ToString(), error));
                        }
                    }
                    reader.Close();
                }

                return (int)command.Parameters["v_TotalCount"].Value;
            }
        }

        /// <summary>
        /// Returns the specified error from the database, or null 
        /// if it does not exist.
        /// </summary>

        public override ErrorLogEntry GetError(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            if (id.Length == 0)
                throw new ArgumentException(null, "id");

            Guid errorGuid;

            try
            {
                errorGuid = new Guid(id);
            }
            catch (FormatException e)
            {
                throw new ArgumentException(e.Message, "id", e);
            }

            string errorXml;

            using (var connection = CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = SchemaOwner + "pkg_elmah$get_error.GetErrorXml";
                command.CommandType = CommandType.StoredProcedure;

                var parameters = command.Parameters;
                AddGenericTypeParameter(command, "v_Application", DbType.String).Value = ApplicationName;
                AddGenericTypeParameter(command, "v_ErrorId", DbType.String).Value = errorGuid.ToString("N");
                AddProviderSpecificTypeParameter(command, "v_AllXml", _clobDbType).Direction = ParameterDirection.Output;

                command.ExecuteNonQuery();
                errorXml = command.Parameters["v_AllXml"].Value as string;
                if (errorXml == null)
                {
                    var xmlLob = (Stream) command.Parameters["v_AllXml"].Value;

                    var streamreader = new StreamReader(xmlLob, Encoding.Unicode);
                    var cbuffer = new char[1000];
                    int actual;
                    var sb = new StringBuilder();
                    while ((actual = streamreader.Read(cbuffer, 0, cbuffer.Length)) > 0)
                        sb.Append(cbuffer, 0, actual);
                    errorXml = sb.ToString();
                }
            }

            var error = ErrorXml.DecodeString(errorXml);
            return new ErrorLogEntry(this, id, error);
        }
    }
}
