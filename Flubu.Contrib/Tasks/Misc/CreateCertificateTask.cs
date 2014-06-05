using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Flubu.Tasks.Processes;

namespace Flubu.Tasks.Misc
{
    /// <summary>
    /// Task for creating development self signed certificates. Task uses makecert.exe utility.
    /// Authority certificate and .pvk should be created only once and should be added to source code.
    /// When creating authority certificate makecert will ask for a private key password, you should select none.
    /// Example code for creating all certificates.
    /// <code>
    /// CreateCertificateTask authority = new CreateCertificateTask("SsoAuthority");
    /// authority
    ///    .AuthorityKeyFile(".\\lib\\certificates\\SsoAuthority.pvk")
    ///    .Output(".\\lib\\certificates\\SsoAuthority.cer")
    ///    .Execute(context);
    /// CreateCertificateTask server = new CreateCertificateTask("SsoWebService");
    /// server
    ///    .AuthorityCertFile(".\\lib\\certificates\\SsoAuthority.cer")
    ///    .AuthorityKeyFile(".\\lib\\certificates\\SsoAuthority.pvk")
    ///    .Output(".\\lib\\certificates\\SsoWebService.cer")
    ///    .Execute(context);
    /// CreateCertificateTask client = new CreateCertificateTask("TestClient");
    /// client
    ///    .AuthorityCertFile(".\\lib\\certificates\\SsoAuthority.cer")
    ///    .AuthorityKeyFile(".\\lib\\certificates\\SsoAuthority.pvk")
    ///    .Output(".\\lib\\certificates\\SsoTestClient.cer")
    ///    .Execute(context);
    /// </code>
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class CreateCertificateTask : TaskBase
    {
        public CreateCertificateTask(string commonName)
        {
            ExecutablePath = ".\\lib\\certificates\\makecert.exe";
            CertCommonName = commonName;
            AuthorityCertificateFile = ".\\lib\\certificates\\LocalAuthority.cer";
            AuthorityPrivateKeyFile = ".\\lib\\certificates\\LocalAuthority.pvk";
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Create certificate CN={0} at {1}",
                    CertCommonName,
                    OutputFile);
            }
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

        public CreateCertificateTask Output(string fullPath)
        {
            OutputFile = fullPath;
            return this;
        }

        public CreateCertificateTask AuthorityCertFile(string fullPath)
        {
            AuthorityCertificateFile = fullPath;
            return this;
        }

        public CreateCertificateTask AuthorityKeyFile(string fullPath)
        {
            AuthorityPrivateKeyFile = fullPath;
            return this;
        }

        protected string AuthorityPrivateKeyFile { get; set; }

        protected string AuthorityCertificateFile { get; set; }

        protected string OutputFile { get; set; }

        protected string CertCommonName { get; set; }

        protected CertificateType CertType { get; set; }

        protected string ExecutablePath { get; set; }

        protected override void DoExecute(ITaskContext context)
        {
            var task = new RunProgramTask(ExecutablePath);

            if (string.IsNullOrEmpty(CertCommonName))
                throw new TaskExecutionException("Certificate common name must be set.");

            task
                .EncloseParametersInQuotes(false)
                .AddArgument("-n \"CN={0}\"", CertCommonName)
                .AddArgument("-cy {0}", CertType == Misc.CertificateType.Authority ? "authority" : "end");

            if (CertType == Misc.CertificateType.Authority)
            {
                task
                    .AddArgument("-sv \"{0}\"", AuthorityPrivateKeyFile)
                    .AddArgument("-r");
            }
            else
            {
                if (string.IsNullOrEmpty(OutputFile))
                    throw new TaskExecutionException("Output filename must be set.");

                task
                    .AddArgument("-a sha1")
                    .AddArgument("-sky exchange")
                    .AddArgument("-sy 12")
                    .AddArgument("-sp \"Microsoft RSA SChannel Cryptographic Provider\"")
                    .AddArgument("-ss My")
                    .AddArgument("-ic \"{0}\"", AuthorityCertificateFile)
                    .AddArgument("-iv \"{0}\"", AuthorityPrivateKeyFile)
                    .AddArgument("-pe");

                if (CertType == Misc.CertificateType.Server)
                {
                    task.AddArgument("-eku 1.3.6.1.5.5.7.3.1")
                        .AddArgument("-sr localmachine");
                }
                else
                {
                    task.AddArgument("-eku 1.3.6.1.5.5.7.3.2")
                        .AddArgument("-sr currentuser")
                        .AddArgument("-sk \"{0}\"", CertCommonName)
                        .AddArgument("-ir localmachine");
                }
            }

            task.AddArgument("\"{0}\"", OutputFile)
                .Execute(context);
        }
    }

    public enum CertificateType
    {
        Authority,
        Server,
        Client
    }
}