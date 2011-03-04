Reversion is a database reverting tool that will help to facilitate unit testing against your repository interfaces
===================================================================================================================

Databases currently supported:
------------------------------
* MS SQL Server:	Yes
* All others:		Pending

How do you install it?
------------------
Pull down the latest source and compile it. You can then add a reference to the Reversion.dll
or
Include the source in your project and use it inline

How do you use it in your unit tests?
------------------
* Add a reference (or include the source) to your test project
* Create a global variable of type DbReverter (needs to be global for the scope), let us name it _reverter
	* When you call the constructor you can pass in a connection string or if you leave it blank, the DbReverter will use a connection string found in an app.config (if included in your test project)
* In your ClassInitialize method call _reverter.TakeSnapShot(); to record the current state of all non-system tables (if you pass in the table names it will only record those tables instead of using the discovery method)
* In your TestCleanup method call _reverter.RevertChanges(); to roll any changes back that might have occurred during any of your tests
	* Alternatively if you want to be selective, just call _reverter.RevertChanges() in just the test methods that you need it to run

Sample Code
-----------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reversion;                //for DbReverter
using Reversion.Adapters;       //for MsSqlDbAdapter

namespace TestProject
{
    [TestClass]
    public class SampleTest
    {
        DbReverter _reverter;

        [ClassInitialize]
        public void Initialize() 
        {
            //instantiate the dbReverter using a MS SQL Server database (using the provided adapter)
            //then we take a snap shot using the default discovery mode (all non-system tables will be scanned)
            // * note that we didn't need to pass in a connection string as it used the one found in my app.config automatically
            _reverter = new DbReverter(new MsSqlDbAdapter());
            _reverter.TakeSnapShot();

            //alternatively you can pass in a specific set of tables that you wish to record instead
            _reverter.TakeSnapShot(tableNames: new[] { "table1", "anothertable" });
        }

        [TestCleanup]
        public void CleanUp()
        {
            _reverter.RevertChanges();
        }

        [TestMethod]
        public void SampleTestMethod()
        {
            //perform some actions against your database using your repository class
            //perform a set of assertions against some criteria to ensure the database matches your expectations
            
			//these changes will revert back to their original state once the clean up method runs
        }
    }
}

