using System.Linq;
using System.Text;
using System.Xml;

namespace System
{
    public static class StringExtentions
    {
        public static string RemoveNonNumeric(this string self)
        {
            Throw.IfIsNullOrEmpty(self);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < self.Length; i++)
                if (Char.IsNumber(self[i]))
                    sb.Append(self[i]);
            return sb.ToString();
        }

        public static string RemoveNumeric(this string self)
        {
            Throw.IfIsNullOrEmpty(self);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < self.Length; i++)
                if (!Char.IsNumber(self[i]))
                    sb.Append(self[i]);
            return sb.ToString();
        }

        public static string SentenceCase(this string self)
        {
            Throw.IfIsNullOrEmpty(self);

            if (self.Length < 1)
                return self;

            string sentence = self.ToLower();
            return sentence[0].ToString().ToUpper() +
               sentence.Substring(1);
        }

        public static string TitleCase(this string self)
        {
            Throw.IfIsNullOrEmpty(self);

            if (self.Length == 0) return string.Empty;

            string[] tokens = self.Split(' ');
            StringBuilder sb = new StringBuilder(self.Length);
            foreach (string s in tokens)
            {
                sb.Append(s[0].ToString().ToUpper());
                sb.Append(s.Substring(1).ToLower());
                sb.Append(" ");
            }

            return sb.ToString().Trim();
        }

        public static string Truncate(this string self, int length, string suffix = "")
        {
            Throw.IfIsNullOrEmpty(self);
            Throw.IfLessThanOrEqZero(length);

            if (self.Length <= length) return self;
            var fragment = self.Substring(0, length);

            return string.Format("{0}{1}", fragment, suffix);
        }

        public static string Right(this string self, int length)
        {
            Throw.IfIsNullOrEmpty(self);
            Throw.IfLessThanZero(length);

            return self.Length > length ? self.Substring(self.Length - length) : self;
        }

        public static string Left(this string self, int length)
        {
            Throw.IfIsNullOrEmpty(self);
            Throw.IfLessThanZero(length);

            return self.Length > length ? self.Substring(0, length) : self;
        }

        public static bool In(this string self, params string[] items)
        {
            Throw.IfIsNullOrEmpty(self);
            Throw.IfIsNull(items);
            Throw.IfEqZero(items.Length);

            return items.Contains(self);
        }

        public static string RemoveLastChar(this string self)
        {
            Throw.IfIsNullOrEmpty(self);
            return self.Left(self.Length - 1);
        }

        public static string RandomString(int size)
        {
            var Random = new Random((int)DateTime.Now.Ticks);

            var builder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string StringNoEponential(string val) { 
            var pos = val.IndexOf("e", StringComparison.OrdinalIgnoreCase);
            if (pos >= 0)
                val = val.Substring(0, pos);

            return val;
        }

        public static bool IsValidGuid(this string value)
        {
            Guid guid;
            return Guid.TryParse(value, out guid);
        }

        public static bool IsValidXml(this string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value) == false)
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(value);
                    return true;
                }
                
                return false;
            }
            catch (XmlException)
            {
                return false;
            }
        }
    }
}
