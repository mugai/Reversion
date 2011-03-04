Reversion is a database reverting tool that will help to facilitate unit testing against your repository interfaces
===================================================================================================================

Databases currently supported: (adapters are used to handle all database communication, so hopefully others can contribute new ones to support new databases)
* MS SQL Server: Yes
* All others: Pending

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
* In your TestInitialize method call _reverter.TakeSnapShot(); to record the current state of all non-system tables (if you pass in the table names it will only record those tables instead of using the discovery method)

* Now in anyone of your TestMethods simply call _reverter.RevertChanges(); to roll any changes back that might have occurred during your test

Sample Code
-----------


