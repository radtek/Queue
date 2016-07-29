using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Engine.Cloud.Core.Utils.Logging
{
    public static class LogUtils
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod(object obj)
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Format("namespace: {0}, method: {1}", obj.GetType().FullName, sf.GetMethod().Name);
        }
    }
}
