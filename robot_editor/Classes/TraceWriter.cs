using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace robot_editor.Classes
{
    /// <summary>
    ///     TraceWriter
    /// </summary>
    public static class TraceWriter
    {
        /// <summary>
        ///     Write Trace Message
        /// </summary>
        /// <param name="message"></param>
        [Localizable(false), DebuggerStepThrough]
        public static void Trace(string message)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("{0} : {1}",
                DateTime.Now.ToString(CultureInfo.InvariantCulture), message));
        }
    }
}
