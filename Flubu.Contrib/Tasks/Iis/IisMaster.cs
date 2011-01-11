using System;
using System.Globalization;
using Flubu.Tasks.Iis.Iis6;
using Flubu.Tasks.Iis.Iis7;

namespace Flubu.Tasks.Iis
{
    public class IisMaster : IIisMaster
    {
        public IisMaster(ITaskContext context)
        {
            this.context = context;
        }

        public IIisTasksFactory Iis6TasksFactory
        {
            get
            {
                return new Iis6TasksFactory();
            }
        }

        public IIisTasksFactory Iis7TasksFactory
        {
            get
            {
                return new Iis7TasksFactory();
            }
        }

        public IIisTasksFactory LocalIisTasksFactory
        {
            get
            {
                Version version = new Version(GetLocalIisVersionTask.GetIisVersion(context, true));
                if (version.Major >= 7)
                    return Iis7TasksFactory;
                if (version.Major >= 6)
                    return Iis6TasksFactory;

                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "IIS version {0} is not supported.",
                    version);
                throw new NotSupportedException(message);
            }
        }

        private readonly ITaskContext context;
    }
}