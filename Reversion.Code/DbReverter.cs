using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Reversion.Adapters;

namespace Reversion {
    /// <summary>
    /// Allows you to quickly take snap shots of database tables and restore them back to that original state
    /// Useful for running unit tests instead of mocking out your entire repository interface
    /// </summary>
    public class DbReverter {

        string _connectionString = "";
        string[] _tables;
        string _postFix;

        IDatabaseAdapter _adapter;

        /// <summary>
        /// Initializes default objects required by the snapshot/reset routines
        /// </summary>
        /// <param name="adapter">The database adapter to use. It is responsible for handling all communications with the database</param>
        /// <param name="connectionString">Optional: Allows you to specify the connection string. Defaults to the last connection string found within the app/web.config file</param>
        /// <param name="tables">Optional: Allows you to specify which tables to take a snapshot of. Defaults to null which will force the library to use discovery mode. This will enumerate all non-system tables found within the specified database </param>
        /// <param name="tablePostFix">Optional: Allows you to specify a post fix for the cloned tables. An example: Users could be given the new Users_Snapshot when cloned if the tablePostFix variable was "_Snapshot"</param>
        public DbReverter(IDatabaseAdapter adapter, string connectionString = "", string[] tables = null, string tablePostFix = "") {
            if (adapter == null) throw new ArgumentNullException("adapter", "adapter can't be null");

            _adapter = adapter;

            _tables = tables;
            _postFix = (tablePostFix == "" ? DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") : tablePostFix);
            if(connectionString == "" && ConfigurationManager.ConnectionStrings.Count > 0){
                _connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.ConnectionStrings.Count-1].ConnectionString;
            }
        }

        /// <summary>
        /// Takes a snapshot of the database tables found via discovery or by the params passed into the constructor
        /// </summary>
        /// <param name="tableNames">Allows you to specify which tables to track</param>
        /// <param name="forceDiscovery">Forces discovery mode. Which will attempt to discover all possible non-system tables again, despite already having been cached in the previous run</param>
        /// <param name="revertPreviousSnapShot">Forces the method to revert any tables back to their original state and to delete the previous snap shot tables</param>
        public void TakeSnapShot(string[] tableNames = null, bool forceDiscovery = false, bool revertPreviousSnapShot = true) {
            if(revertPreviousSnapShot && _tables != null)RevertChanges();
            if (tableNames != null) _tables = tableNames;

            ExecuteWithinConnection(() =>
            {
                if (_tables == null || forceDiscovery){
                    _tables = _adapter.GetDatabaseTables();
                }

                foreach (var t in _tables){
                    _adapter.CopyTable(t, _postFix);
                }
            });
        }

        /// <summary>
        /// Rerverts any tables back to their original state as found during the TakeSnapeShot call
        /// </summary>
        public void RevertChanges() {
            if (_tables == null) throw new Exception("No tables are currently being tracked.");
            ExecuteWithinConnection(() => { foreach (var t in _tables)_adapter.RestoreTable(t, _postFix); });
        }

        /// <summary>
        /// Allows you to execute some arbitrary code wrapped in an open connection (provided by the adapter)
        /// Automatically closes and disposes of the connection once the code have finished executing
        /// </summary>
        /// <param name="action">The arbitrary code that you wish to execute. Example: ExecuteWithinConnection(() => { MessageBox.Show("hello world"); });</param>
        private void ExecuteWithinConnection(Action action) {
            CheckDependencies();
            using (var conn = _adapter.OpenConnection(_connectionString)){
                action.Invoke();
                conn.Close();
            }
        }

        /// <summary>
        /// Asserts that any required objects have been instantiated and properly set prior to running any code that might depend on them
        /// </summary>
        private void CheckDependencies() {
            if (_adapter == null) throw new Exception("No valid adapter has been passed in.");
            if (string.IsNullOrEmpty(_connectionString)) throw new Exception("No valid connection string could be found or has been set.");
        }
    }
}
