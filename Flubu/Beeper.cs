using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Flubu
{
    public static class Beeper
    {
        /// <summary>
        /// Sounds a beep.
        /// </summary>
        /// <param name="messageBeepType">Type of the message beep.</param>
        public static void Beep(MessageBeepType messageBeepType)
        {
            Console.Out.Flush();
            MessageBeep(messageBeepType);
        }

        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible")]
        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs")]
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MessageBeep(MessageBeepType type);
    }
}