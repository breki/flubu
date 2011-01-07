using System;
using System.Collections.Generic;
using System.Text;

namespace Flubu.Tasks.UserInterface
{
    /// <summary>
    /// Prompts the user for a response. Optionally stores the response into a specified configuration setting.
    /// </summary>
    public class AskUserTask : TaskBase
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string TaskDescription
        {
            get { return "Ask user"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AskUserTask"/> class using the specified
        /// user prompt string and configuration setting name.
        /// </summary>
        /// <param name="prompt">The user prompt string.</param>
        /// <param name="configurationSettingName">Name of the configuration setting where the user input should be stored.</param>
        public AskUserTask (string prompt, string configurationSettingName)
        {
            this.prompt = prompt;
            this.configurationSettingName = configurationSettingName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AskUserTask"/> class using the specified
        /// user prompt string.
        /// </summary>
        /// <param name="prompt">The user prompt string.</param>
        public AskUserTask (string prompt)
            : this (prompt, null)
        {
        }

        /// <summary>
        /// Executes the <see cref="AskUserTask"/> using the specified
        /// user prompt string and configuration setting name.
        /// </summary>
        /// <param name="environment">The script execution environment.</param>
        /// <param name="prompt">The user prompt string.</param>
        /// <param name="configurationSettingName">Name of the configuration setting where the user input should be stored.</param>
        public static void Execute (
            ITaskContext environment, 
            string prompt, 
            string configurationSettingName)
        {
            AskUserTask task = new AskUserTask (prompt, configurationSettingName);
            task.Execute (environment);
        }

        /// <summary>
        /// Executes the <see cref="AskUserTask"/> using the specified
        /// user prompt string.
        /// </summary>
        /// <param name="environment">The script execution environment.</param>
        /// <param name="prompt">The user prompt string.</param>
        public static void Execute (
            ITaskContext environment,
            string prompt)
        {
            AskUserTask task = new AskUserTask (prompt);
            task.Execute (environment);
        }

        /// <summary>
        /// Method defining the actual work for a task.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            string input = context.ReceiveInput (prompt);

            if (configurationSettingName != null)
                context.SetConfigSetting (configurationSettingName, input);
        }

        private string prompt;
        private string configurationSettingName;
    }
}
