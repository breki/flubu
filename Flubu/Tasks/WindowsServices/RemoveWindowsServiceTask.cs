using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Flubu.Tasks.WindowsServices
{
    public class RemoveWindowsServiceTask : TaskBase
    {
        public override string Description
        {
            get
            {
                return String.Format(
                    CultureInfo.InvariantCulture,
                    "Uninstall Windows service '{0}'",
                    ServiceName);
            }
        }

        public RemoveWindowsServiceTask(string serviceName, bool failIfNotExist)
        {
            ServiceName = serviceName;
            FailIfNotExist = failIfNotExist;
        }

        public static void Execute(
            ITaskContext environment,
            string serviceName,
            bool failIfNotExist)
        {
            RemoveWindowsServiceTask task = new RemoveWindowsServiceTask(serviceName, failIfNotExist);
            task.DoExecute(environment);
        }

        protected override void DoExecute(ITaskContext context)
        {
// ReSharper disable InconsistentNaming
            const int GenericWrite = 0x40000000;
// ReSharper restore InconsistentNaming
            IntPtr scHndl = IntPtr.Zero;
            IntPtr svcHndl = IntPtr.Zero;
            try
            {
                scHndl = OpenSCManager(null, null, GenericWrite);
                if (scHndl == IntPtr.Zero)
                {
                    throw new RunnerFailedException("Service manager could not be opened!");
                }

// ReSharper disable InconsistentNaming
                const int Delete = 0x10000;
// ReSharper restore InconsistentNaming
                svcHndl = OpenService(scHndl, ServiceName, Delete);
                if (svcHndl == IntPtr.Zero)
                {
                    if (FailIfNotExist)
                        throw new RunnerFailedException("Service {0} does not exist.", ServiceName);

                    context.WriteInfo("Service '{0}' does not exist, doing nothing.", ServiceName);
                    return;
                }

                int result = DeleteService(svcHndl);
                if (result != 0)
                {
                    throw new RunnerFailedException("Uninstall windows service failed with error code:{0}", result);
                }
            }
            finally
            {
                if (svcHndl != IntPtr.Zero)
                    CloseServiceHandle(svcHndl);
                if (scHndl != IntPtr.Zero)
                    CloseServiceHandle(scHndl);
            }
        }

        #region DLLImport

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        // ReSharper disable InconsistentNaming
        private static extern IntPtr OpenSCManager(string lpMachineName, string lpScdb, int scParameter);
        // ReSharper restore InconsistentNaming

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return")]
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        private static extern void CloseServiceHandle(IntPtr schandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        private static extern IntPtr OpenService(IntPtr schandle, string lpSvcName, int dwNumServiceArgs);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        private static extern int DeleteService(IntPtr svhandle);

        #endregion DLLImport

        private string ServiceName { get; set; }

        private bool FailIfNotExist { get; set; }
    }
}