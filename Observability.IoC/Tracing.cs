using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Observability.IoC
{
    public static class Tracing
    {
        public static ActivitySource ActivitySource = new ActivitySource("RabbitMQ.Producer");
    }
}
