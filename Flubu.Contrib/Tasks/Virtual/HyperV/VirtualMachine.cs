namespace Flubu.Tasks.Virtual.HyperV
{
    /// <summary>
    ///   Class describing single virtual machine on virtual server.
    /// </summary>
    public class VirtualMachine
    {
        /// <summary>
        ///   Gets or sets Id of the machine.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///   Gets or sets name of the machine.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   Gets or sets virtual machine <see cref = "VirtualMachineState" /> state.
        /// </summary>
        public VirtualMachineState Status { get; set; }
    }
}