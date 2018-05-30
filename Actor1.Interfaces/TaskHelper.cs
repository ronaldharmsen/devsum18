using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VehicleActor.Interfaces
{
    public static class TaskHelper
    {
        public static void FireAndForget(this Task task)
        {
            Task.Run(async () => await task).ConfigureAwait(false);
        }
    }
}
