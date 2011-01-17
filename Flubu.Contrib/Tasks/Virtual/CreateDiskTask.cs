using System;
using System.Globalization;
using Flubu.Tasks.Virtual.HyperV;

namespace Flubu.Tasks.Virtual
{
    /// <summary>
    /// Creates new virtual disk on hyper-v server.
    /// <code>
    ///         CreateDiskTask
    ///             .New(host, @"h:\virtual\ssmTest.vhd", 10)
    ///            .Type(CreateDiskType.Differencing)
    ///            .BasePath(@"h:\virtual\ssmTestBase.vhd")
    ///            .Execute(ScriptExecutionEnvironment);
    /// </code>
    /// </summary>
    public class CreateDiskTask : TaskBase
    {
        private readonly string hostName;
        private string imagePath;
        private string baseImagePath;
        private int size;

        private CreateDiskType type;

        /// <summary>
        /// Initializes new instance of <see cref="CreateDiskTask"/>
        /// It also sets default disk type as <see cref="CreateDiskType.Fixed"/>
        /// </summary>
        /// <param name="host">Hyper-V server machine.</param>
        public CreateDiskTask(string host)
        {
            hostName = host;
            Type(CreateDiskType.Fixed);
        }

        /// <summary>
        /// Initializes new instance of <see cref="CreateDiskTask"/>
        /// </summary>
        /// <param name="host">Hyper-V server machine</param>
        /// <param name="path">Full path on remote machine where image will be created.</param>
        /// <param name="size">Maximum disk size in Gb.</param>
        /// <returns>New instance of <see cref="CreateDiskTask"/></returns>
        public static CreateDiskTask New(string host, string path, int size)
        {
            return new CreateDiskTask(host)
                .Path(path)
                .Size(size);
        }

        /// <summary>
        /// Sets virtual disk file path on remote machine.
        /// </summary>
        /// <param name="fullPath">Full VHD file path.</param>
        /// <returns>This instance of <see cref="CreateDiskTask"/></returns>
        public CreateDiskTask Path(string fullPath)
        {
            imagePath = fullPath;
            return this;
        }

        /// <summary>
        /// Sets base image path. Parameter is only used when creating diferencing disk.
        /// </summary>
        /// <param name="fullPath">Full path to base VHD image to use.</param>
        /// <returns>This instance of <see cref="CreateDiskTask"/></returns>
        public CreateDiskTask BasePath(string fullPath)
        {
            baseImagePath = fullPath;
            return this;
        }

        /// <summary>
        /// Sets maximum disk size. If <see cref="Type"/> is set
        /// to <see cref="CreateDiskType.Fixed"/> file is prealocated, otherwise initial
        /// file size will be 0 and will be increased as needed.
        /// </summary>
        /// <param name="diskSize">Maximum disk size in Gb.</param>
        /// <returns>This instance of <see cref="CreateDiskTask"/></returns>
        public CreateDiskTask Size(int diskSize)
        {
            size = diskSize;
            return this;
        }

        /// <summary>
        /// Sets disk type to create <see cref="CreateDiskType"/>
        /// </summary>
        /// <param name="diskType"></param>
        /// <returns>This instance of <see cref="CreateDiskTask"/></returns>
        public CreateDiskTask Type(CreateDiskType diskType)
        {
            type = diskType;
            return this;
        }


        public override string Description
        {
            get { return string.Format(CultureInfo.InvariantCulture, "Create disk {0}:{1}:Size:{2}", type, imagePath, size); }
        }

        protected override void DoExecute(ITaskContext context)
        {
            using (HyperVManager manager = new HyperVManager())
            {
                manager.Connect(hostName);
                IVirtualTask t;
                switch (type)
                {
                    case CreateDiskType.Fixed:
                        if(string.IsNullOrEmpty(imagePath) || size < 1)
                            throw new ArgumentException("ImagePath or size not set");
                        t = manager.CreateFixedDisk(imagePath, size);
                        break;
                    case CreateDiskType.Dynamic:
                        if(string.IsNullOrEmpty(imagePath) || size < 1)
                            throw new ArgumentException("ImagePath or size not set");
                        t = manager.CreateDynamicDisk(imagePath, size);
                        break;
                    case CreateDiskType.Differencing:
                        if(string.IsNullOrEmpty(imagePath) || string.IsNullOrEmpty(baseImagePath))
                            throw new ArgumentException("ImagePath or BaseImagePath not set");
                        t = manager.CreateDifferencingDisk(imagePath, baseImagePath);
                        break;
                    default:
                        throw new NotSupportedException("Not supported disk type.");
                }
                t.WaitForCompletion(new TimeSpan(0, 0, 1, 0));
            }
        }
    }
}