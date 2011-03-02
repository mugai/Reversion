using Reversion;
using Reversion.Adapters;

namespace Reversion.ConsoleExecutable {
    class Program {
        static void Main(string[] args) {
            var r = new DbReverter(new MsSqlDbAdapter());
            r.TakeSnapShot();
            //.. make changes to a database table
            r.RevertChanges();
            //.. verify that changes were reverted
        }
    }
}
