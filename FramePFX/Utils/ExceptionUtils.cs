// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Text;

namespace FramePFX.Utils {
    public static class ExceptionUtils {
        private const String CAUSE_CAPTION = "Caused By: ";
        private const String AGGREGATED_CAPTION = "Additionally Caused By: ";
        private const String SUPPRESSED_CAPTION = "Suppressed: ";

        public static List<Exception> GetSuppressed(this Exception e, bool create = true) {
            if (e.Data.Contains("SuppressedList")) {
                return (List<Exception>) e.Data["SuppressedList"];
            }
            else if (create) {
                List<Exception> list = new List<Exception>();
                e.Data["SuppressedList"] = list;
                return list;
            }
            else {
                return null;
            }
        }

        public static void AddSuppressed(this Exception @this, Exception suppressed) {
            GetSuppressed(@this, true).Add(suppressed);
        }

        public static string GetToString(this Exception e, bool message = true, bool fileInfo = true) {
            List<string> list = new List<string>();
            HashSet<Exception> dejaVu = new HashSet<Exception>();
            dejaVu.Add(e);
            list.Add(GetExceptionHeader(e, message));

            StackFrame[] trace = new StackTrace(e, fileInfo).GetFrames() ?? new StackFrame[0];
            // Print our stack trace
            foreach (StackFrame frame in trace) {
                list.Add($"    at {FormatFrame(frame, fileInfo)}");
            }

            // Print aggregated exceptions, if any
            ReadOnlyCollection<Exception> aggregations;
            if (e is AggregateException aggregateException && (aggregations = aggregateException.InnerExceptions).Count > 0) {
                foreach (Exception ex in aggregations) {
                    GetEnclosedStackTrace(ex, list, message, fileInfo, trace, AGGREGATED_CAPTION, "    ", dejaVu);
                }
            }

            // Print suppressed exceptions, if any
            IReadOnlyCollection<Exception> suppressed = GetSuppressed(e, false);
            if (suppressed != null && suppressed.Count > 0) {
                foreach (Exception ex in suppressed) {
                    GetEnclosedStackTrace(ex, list, message, fileInfo, trace, SUPPRESSED_CAPTION, "    ", dejaVu);
                }
            }

            // Print cause, if any
            Exception cause = e.InnerException;
            if (cause != null) {
                GetEnclosedStackTrace(cause, list, message, fileInfo, trace, CAUSE_CAPTION, "", dejaVu);
            }

            StringBuilder sb = new StringBuilder(2048);
            foreach (string line in list) {
                if (!string.IsNullOrEmpty(line)) {
                    if (line.EndsWith(Environment.NewLine) || line[line.Length - 1] == '\n') {
                        sb.Append(line);
                    }
                    else {
                        sb.Append(line).Append(Environment.NewLine);
                    }
                }
            }

            return sb.ToString().Trim();
        }

        public static string GetExceptionHeader(Exception e, bool message) {
            string msg = message ? e.Message : null;
            if (string.IsNullOrEmpty(msg)) {
                return e.GetType().ToString();
            }
            else {
                return e.GetType() + ": " + msg;
            }
        }

        public static void GetEnclosedStackTrace(Exception e, List<string> list, bool message, bool fileInfo, StackFrame[] enclosing, String caption, String prefix, HashSet<Exception> dejaVu) {
            if (dejaVu.Contains(e)) {
                list.Add($"[CIRCULAR REFERENCE: {GetExceptionHeader(e, message)}]");
                return;
            }

            dejaVu.Add(e);
            // Compute number of frames in common between throwable and enclosing trace
            StackFrame[] trace = new StackTrace(e, fileInfo).GetFrames() ?? new StackFrame[0];
            int m = trace.Length - 1;
            int n = enclosing.Length - 1;
            while (m >= 0 && n >= 0 && trace[m].Equals(enclosing[n])) {
                m--;
                n--;
            }

            int framesInCommon = trace.Length - 1 - m;

            // Print our stack trace
            list.Add($"{prefix}{caption}{GetExceptionHeader(e, message)}");
            for (int i = 0; i <= m; i++)
                list.Add($"{prefix}    at {FormatFrame(trace[i], fileInfo)}");
            if (framesInCommon != 0)
                list.Add($"{prefix}    ... {framesInCommon} more");

            // Print suppressed exceptions, if any
            List<Exception> suppressed = GetSuppressed(e, false);
            if (suppressed != null && suppressed.Count > 0) {
                foreach (Exception ex in suppressed) {
                    GetEnclosedStackTrace(ex, list, message, fileInfo, trace, SUPPRESSED_CAPTION, prefix + "    ", dejaVu);
                }
            }

            // Print cause, if any
            Exception cause = e.InnerException;
            if (cause != null) {
                GetEnclosedStackTrace(cause, list, message, fileInfo, trace, CAUSE_CAPTION, prefix, dejaVu);
            }
        }

        public static string FormatFrame(StackFrame frame, bool fileInfo) {
            // return frame.ToString(); // default formatting
            MethodBase method = frame.GetMethod();
            if (method == null) {
                return frame.ToString();
            }

            StringBuilder sb = new StringBuilder(byte.MaxValue);
            Type declaringType = method.DeclaringType;
            if (declaringType != null) {
                sb.Append(declaringType.FullName?.Replace('+', '$') ?? "??");
                sb.Append(".");
            }

            bool isFirst;
            sb.Append(method.Name);
            if (method is MethodInfo && method.IsGenericMethod) {
                Type[] genericArguments = method.GetGenericArguments();
                sb.Append("[");
                int i = 0;
                isFirst = true;
                for (; i < genericArguments.Length; ++i) {
                    if (isFirst) {
                        isFirst = false;
                    }
                    else {
                        sb.Append(",");
                    }

                    sb.Append(genericArguments[i].Name);
                }

                sb.Append("]");
            }

            sb.Append("(");
            ParameterInfo[] parameters = method.GetParameters();
            isFirst = true;
            foreach (ParameterInfo param in parameters) {
                if (!isFirst)
                    sb.Append(", ");
                else
                    isFirst = false;
                string a = "<UnknownType>";
                if (param.ParameterType != null)
                    a = param.ParameterType.Name;
                sb.Append(a + " " + param.Name);
            }

            sb.Append(")");
            if (fileInfo && frame.GetILOffset() != -1) {
                string fileName = null;
                try {
                    fileName = frame.GetFileName();
                }
                catch (NotSupportedException ex) {
                }
                catch (SecurityException ex) {
                }

                if (fileName != null) {
                    sb.Append(' ');
                    sb.Append($"in '{fileName}' on line {frame.GetFileLineNumber()}");
                }
            }

            return sb.ToString();
        }

        public static void Assert(bool condition, string msg) {
            if (!condition) {
                throw new Exception(msg);
            }
        }
    }
}