using Flubu;
using Flubu.Tasks.Processes;

namespace Build
{
    public class CreateCertificateTask : TaskBase
    {
        public CreateCertificateTask()
        {
            ExecutablePath = ".\\lib\\certificates\\makecert.exe";
        }

        public CreateCertificateTask Executable(string fullPath)
        {
            ExecutablePath = fullPath;
            return this;
        }

        public CreateCertificateTask CertificateType(CertificateType type)
        {
            CertType = type;
            return this;
        }

        public CreateCertificateTask CommonName(string name)
        {
            CertCommonName = name;
            return this;
        }

        protected string CertCommonName { get; set; }

        protected CertificateType CertType { get; set; }

        protected string ExecutablePath { get; set; }

        protected override void DoExecute(ITaskContext context)
        {
            RunProgramTask task = new RunProgramTask(ExecutablePath);

            if (CertType == Build.CertificateType.Authority)
            {
                if(string.IsNullOrEmpty(CertCommonName))
                    throw new TaskExecutionException("Certificate common name must be set.");
                task
                    .AddArgument("-n \"CN={0}\"", CertCommonName)
                    .AddArgument("-cy authority")
                    .AddArgument("-sv \"{0}.pvk\"", CertCommonName)
                    .AddArgument("-r")
                    .AddArgument("\"{0}.cer\"", CertCommonName);
            }
            else if(CertType == Build.CertificateType.Server)
            {}

            task.Execute(context);
        }

        public override string Description
        {
            get { return "Create certificate"; }
        }
    }

    public enum CertificateType
    {
        Authority,
        Server,
        Client
    }
}
