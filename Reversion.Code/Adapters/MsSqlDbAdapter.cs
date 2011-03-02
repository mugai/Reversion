using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Reversion.Adapters {
    /// <summary>
    /// MS SQL Server (Express, 2005/2008) Adapter for the Transaction based tester
    /// </summary>
    public class MsSqlDbAdapter : IDatabaseAdapter {
        SqlConnection _connection;

        string _activeTable;
        string _postfix;
        string[] _columns;

        const string LIST_COLUMNS_QUERY = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '{table.name}'";
        const string LIST_TABLES_QUERY = "SELECT [name] FROM {db.name}.sys.tables";
        const string COPY_TABLE_QUERY = "SELECT * INTO {table.name}{post} FROM {table.name}";
        const string RESTORE_TABLE_QUERY = "TRUNCATE TABLE {table.name};"
                                            + "SET IDENTITY_INSERT {db.name}.dbo.{table.name} ON;"
                                            + "INSERT INTO {table.name} ({columns}) SELECT {columns} FROM {table.name}{post};"
                                            + "SET IDENTITY_INSERT {db.name}.dbo.{table.name} OFF;"
                                            + "DROP TABLE {table.name}{post};";


        /// <summary>
        /// Create and open a connection to the specified sql server
        /// </summary>
        /// <param name="connectionString">the connection string containing the credentails, server and database to connect to</param>
        /// <returns>An open connection to the database</returns>
        public DbConnection OpenConnection(string connectionString){
            _connection = new SqlConnection(connectionString);
            _connection.Open();
            return _connection;
        }

        /// <summary>
        /// Requests the table names from the sys.tables table
        /// </summary>
        /// <returns>A string array containing the name of all non-system tables</returns>
        public string[] GetDatabaseTables(){
            var tables = ExecuteCommand(LIST_TABLES_QUERY);
            return (from t in tables.AsEnumerable() select t[0].ToString()).ToArray();
        }

        /// <summary>
        /// Creates a new table based on the structure of an existing table. It will also fill the new table with the original data
        /// The new table will have the same name as the original, but with the postfix added
        /// </summary>
        /// <param name="tableName">The table that you want to make a backup of</param>
        /// <param name="postFix">A string variable that will be prepended onto the new table name to make sure it's distinct</param>
        /// <returns>true on success; else false on failure</returns>
        public bool CopyTable(string tableName, string postFix) {
            _activeTable = tableName;
            _postfix = postFix;
            ExecuteCommand(COPY_TABLE_QUERY);
            return true;
        }

        /// <summary>
        /// Restores a table back to it's original state (the table used by the copy table method will be used as the source for the original state)
        /// </summary>
        /// <param name="tableName">The table you wish to restore</param>
        /// <param name="postFix">The postfix that was included in the copytable call to ensure we are using the right table back up</param>
        /// <returns>true on success; else false on failure</returns>
        public bool RestoreTable(string tableName, string postFix) {
            _activeTable = tableName;
            _columns = GetTableColumns(tableName);
            _postfix = postFix;
            ExecuteCommand(RESTORE_TABLE_QUERY);
            return true;
        }

        /// <summary>
        /// Requests the column names for a specified table from the INFORMATION_SCHEMA.Columns
        /// </summary>
        /// <param name="tableName">The table name that you wish to receive the columns for</param>
        /// <returns>A string array containing the name of all the columns within the specified table</returns>
        private string[] GetTableColumns(string tableName) {
            _activeTable = tableName;
            var columns = ExecuteCommand(LIST_COLUMNS_QUERY);
            return (from c in columns.AsEnumerable() select c[0].ToString()).ToArray();
        }

        /// <summary>
        /// Executes a query on the database and returns a datatable filled with the result set
        /// </summary>
        /// <param name="query">The T-SQL to execute</param>
        /// <returns>A DataTable filled with the result set returned back from the executed query</returns>
        private DataTable ExecuteCommand(string query) {
            query = ReplaceTokens(query);
            if (_connection == null || _connection.State != ConnectionState.Open){
                throw new Exception("Invalid connection to perform operation.");
            }

            var dt = new DataTable();
            var command = new SqlCommand();
            command.Connection = _connection;
            command.CommandText = query;
            var adapter = new SqlDataAdapter(command);
            adapter.Fill(dt);

            return dt;
        }

        /// <summary>
        /// Replaces specified placeholders within a string with dynamic values that are changing during the programs life cycle
        /// </summary>
        /// <param name="input">The input to filter/replace the specified placholders with the dynamic data</param>
        /// <returns>A string representing the original input value with the dynamic data in place of the original placeholders</returns>
        private string ReplaceTokens(string input) {
            if (_columns != null){
                input = input.Replace("{columns}", string.Join(", ", _columns));       
            }

            input = input.Replace("{post}", _postfix)
                    .Replace("{db.name}", _connection.Database)
                    .Replace("{table.name}", _activeTable)
                    .Replace("{app.path}", Environment.CurrentDirectory);

            return input;
        }
    }
}
