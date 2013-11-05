using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FILO.Core.Log
{
    public interface ILogger
    {
        void OnLog(Logger.MessageData message);
    }
}
