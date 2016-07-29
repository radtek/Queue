using System.Collections.Generic;
using System.Linq;

namespace System
{
    public class Throw
    {
        public static void IfIsNull(object value, Exception ex = null)
        {
            if (value == null)
                if (ex == null)
                    throw new ArgumentNullException();
                else
                    throw ex;
        }

        public static void IfEqZero(long value, Exception ex = null)
        {
            if (value == 0)
                if (ex == null)
                    throw new ArgumentOutOfRangeException();
                else
                    throw ex;
        }

        public static void IfLessThanZero(long value, Exception ex = null)
        {
            if (value < 0)
                if (ex == null)
                    throw new ArgumentOutOfRangeException();
                else
                    throw ex;
        }

        public static void IfLessThanOrEqZero(long value, Exception ex = null)
        {
            if (value <= 0)
                if (ex == null)
                    throw new ArgumentOutOfRangeException();
                else
                    throw ex;
        }

        public static void IfIsNullOrEmpty(string value, Exception ex = null)
        {
            if (string.IsNullOrEmpty(value))
                if (ex == null)
                    throw new ArgumentNullException();
                else
                    throw ex;
        }

        public static void IfIsEmpty<T>(IEnumerable<T> value, Exception ex = null)
        {
            if (value == null || (value != null) && value.Count() == 0)
                if (ex == null)
                    throw new ArgumentOutOfRangeException();
                else
                    throw ex;
        }

        public static void IfIsFalse(bool value, Exception ex = null)
        {
            if (!value)
                if (ex == null)
                    throw new InvalidOperationException();
                else
                    throw ex;
        }

        public static void IfIsTrue(bool value, Exception ex = null)
        {
            if (value)
                if (ex == null)
                    throw new InvalidOperationException();
                else
                    throw ex;
        }
    }
}
