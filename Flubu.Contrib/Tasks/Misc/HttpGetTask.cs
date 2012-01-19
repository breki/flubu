using System.Globalization;
using System.Net;

namespace Flubu.Tasks.Misc
{
    public class HttpGetTask : TaskBase
    {
        private readonly string _url;

        public HttpGetTask(string url)
        {
            _url = url;
        }

        public override string Description
        {
            get { return string.Format(CultureInfo.InvariantCulture, "HTTP GET {0}", _url); }
        }

        protected override void DoExecute(ITaskContext context)
        {
            WebClient client = new WebClient();
            client.DownloadString(_url);
        }
    }
}
