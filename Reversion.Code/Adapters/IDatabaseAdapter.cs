using System.Data.Common;

namespace Reversion.Adapters{
    /// <summary>
    /// Acts as the database adapter, handling the connection and execution of queries
    /// </summary>
    public interface IDatabaseAdapter{
        /// <summary>
        /// Should be implemented to take the following action: Creates and opens a new connection to the db provider specified by the interface implementation
        /// </summary>
        /// <param name="connectionString">The connection string to be used, not the connection string name located within a configuration file</param>
        /// <returns>An active/open connection to the database specified in the connection string</returns>
        DbConnection OpenConnection(string connectionString);
        /// <summary>
        /// Should be implemented to take the following action: Used to return a list of non-system tables from the database
        /// </summary>
        /// <returns>a string array containing the names of all non-system tables in the database</returns>
        string[] GetDatabaseTables();
        /// <summary>
        /// Should be implemented to take the following action: Make a copy of an existing table, structure + data included. 
        /// </summary>
        /// <param name="tableName">The table you wish to make a copy of</param>
        /// <param name="postFix">Allows you to specify a post fix to prepend onto the copied table name</param>
        /// <returns>true on success; else false on failure</returns>
        bool CopyTable(string tableName, string postFix);
        /// <summary>
        /// Should be implemented to take the following: Restore the table back to it's original state (as provided by the CopyTable method)
        /// </summary>
        /// <param name="tableName">The original table that you are wishing to revert back</param>
        /// <param name="postFix">The prepended post fix that was given to the original copied table</param>
        /// <returns>true on success; else false on failure</returns>
        bool RestoreTable(string tableName, string postFix);
    }
}
