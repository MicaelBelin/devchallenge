using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test_DevChallengeServer
{
    public class FakeDataContext : DevChallenge.Server.db.DevChallengeDataContext
    {
        public FakeDataContext()
            : base(String.Format("Data Source={0};Persist Security Info=False;", Path.ChangeExtension(System.IO.Path.GetTempFileName(),"sdf")))
        {
            CreateDatabase();
        }

        public void Clear()
        {
            this.AgentRecords.DeleteAllOnSubmit(this.AgentRecords);
            this.Instances.DeleteAllOnSubmit(this.Instances);
            this.Scenarios.DeleteAllOnSubmit(this.Scenarios);
            this.Users.DeleteAllOnSubmit(this.Users);
            SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
            Assert.AreEqual(0, AgentRecords.Count());
            Assert.AreEqual(0, Instances.Count());
            Assert.AreEqual(0, Scenarios.Count());
            Assert.AreEqual(0, Users.Count());
        }
    }
}
