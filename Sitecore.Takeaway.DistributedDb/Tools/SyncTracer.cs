using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace Sitecore.Takeaway.DistributedDb.Tools
{
    public class SyncTracerExtended
    {
        public static void TraceCommandAndParameters(IDbCommand command)
        {
            TraceCommandAndParameters(1, command);
        }

        public static void TraceCommandAndParameters(int indentLevel, IDbCommand command)
        {
            if (!Microsoft.Synchronization.Data.SyncTracer.IsVerboseEnabled())
                return;
            Microsoft.Synchronization.Data.SyncTracer.Verbose(indentLevel, "Executing Command: {0}", (object)command.CommandText);
            foreach (DbParameter parameter in (IEnumerable)command.Parameters)
                TraceCommandParameter(indentLevel + 1, parameter);
        }

        public static void TraceCommandParameter(int indentLevel, DbParameter parameter)
        {
            if (!Microsoft.Synchronization.Data.SyncTracer.IsVerboseEnabled())
                return;
            if (parameter.Direction != ParameterDirection.Input && parameter.Direction != ParameterDirection.InputOutput)
            {
                Microsoft.Synchronization.Data.SyncTracer.Verbose(indentLevel, "Parameter: {0} Value: Skipped since Not Input/InputOutput", (object)parameter.ParameterName);
            }
            else
            {
                string str1 = "";
                int num = parameter.Size;
                string str2;
                if (parameter == null || parameter.Value is DBNull)
                    str2 = "NULL";
                else if (parameter.Value is byte[])
                {
                    byte[] numArray = (byte[])parameter.Value;
                    num = numArray.Length;
                    str2 = numArray.Length != 0 ? BitConverter.ToString(numArray) : "<zero length value>";
                }
                else
                {
                    str2 = parameter.Value.ToString();
                    PropertyInfo property = parameter.Value.GetType().GetProperty("Length");
                    if (property != null && property.PropertyType.ToString().Equals("System.Int32"))
                        num = (int)property.GetValue(parameter.Value, (object[])null);
                }
                string str3;
                if (str2.Length > 50)
                {
                    str3 = str2.Substring(0, 50);
                    str1 = "...";
                }
                else
                    str3 = str2;
                string str4;
                if (num <= 0)
                    str4 = "";
                else
                    str4 = string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Len: {0} ", new object[1]
          {
            (object) num
          });
                string str5 = str4;
                Microsoft.Synchronization.Data.SyncTracer.Verbose(indentLevel, "Parameter: {0} {1}Value: {2}{3}", (object)parameter.ParameterName, (object)str5, (object)str3, (object)str1);
            }
        }
    }
}