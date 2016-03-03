using System.Globalization;
using System.Net;

namespace Flubu.Tasks.Misc
{
    public class HttpGetTask : TaskBase
    {
        public HttpGetTask(string url)
        {
            this.url = url;
        }

        public override string Description
        {
            get { return string.Format(CultureInfo.InvariantCulture, "HTTP GET {0}", url); }
        }

        protected override void DoExecute(ITaskContext context)
        {
            using (WebClient client = new WebClient())
                client.DownloadString(url);
        }

        private readonly string url;
    }
}
