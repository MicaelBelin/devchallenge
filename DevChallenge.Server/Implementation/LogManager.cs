using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using DevChallenge.Server.Model;

namespace DevChallenge.Server.Implementation
{
    public partial class LogManager : IScenarioLogFactory
    {

        public Dispatcher WorkerDispatcher { get; set; }
        public db.DevChallengeDataContext DataContext { get; set; }
        public LogManager(db.DevChallengeDataContext DataContext)
        {
            this.DataContext = DataContext;
            AutoResetEvent starter = new AutoResetEvent(false);
            Task.Run(() =>
                {
                    WorkerDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                    starter.Set();
                    Dispatcher.Run();
                });
            starter.WaitOne();
        }


        public IScenarioLog Create(IScenario scenario)
        {
            return new ScenarioLog(this, scenario);
        }
    }
}
