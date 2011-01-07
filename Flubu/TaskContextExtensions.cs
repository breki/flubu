﻿using System.Globalization;

namespace Flubu
{
    public static class TaskContextExtensions
    {
        public static void Fail(
            this ITaskContext context,
            string messageFormat,
            params object[] args)
        {
            if (messageFormat == null)
                return;

            string message = string.Format(
                CultureInfo.InvariantCulture,
                messageFormat,
                args);
            context.Fail(message);
        }

        public static void WriteMessage(
            this ITaskContext context, 
            TaskMessageLevel level,
            string messageFormat, 
            params object[] args)
        {
            if (messageFormat == null)
                return;

            string message = string.Format(
                CultureInfo.InvariantCulture,
                messageFormat,
                args);
            context.WriteMessage(level, message);
        }

        public static void WriteError(this ITaskContext context, string messageFormat, params object[] args)
        {
            context.WriteMessage(TaskMessageLevel.Error, messageFormat, args);
        }

        public static void WriteInfo(this ITaskContext context, string messageFormat, params object[] args)
        {
            context.WriteMessage(TaskMessageLevel.Info, messageFormat, args);
        }
    }
}