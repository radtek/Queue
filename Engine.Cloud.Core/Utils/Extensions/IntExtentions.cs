using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utils
{
    public static class IntExtentions
    {
        #region [ Times ]

        public static void Times(this int self, Action action)
        {
            Throw.IfIsNull(action);
            Throw.IfLessThanOrEqZero(self);

            for (int i = 0; i < self; i++)
                action();
        }

        public static void Times(this int self, Action<int> action)
        {
            Throw.IfIsNull(action);
            Throw.IfLessThanOrEqZero(self);

            for (int i = 0; i < self; i++)
                action(i);
        }

        #endregion [ Times ]

        #region [ PercentOf ]

        public static int PercentOf(this int self, int value)
        {
            decimal x = ((decimal) self/(decimal) value)*100;
            //var x = ((decimal)value * (decimal)self) / 100;
            return (int) x;
        }

        public static long PercentOf(this int self, long value)
        {
            return (value*self)/100;
        }

        public static float PercentOf(this int self, float value)
        {
            return (value*self)/100;
        }

        public static decimal PercentOf(this int self, decimal value)
        {
            return (value*self)/100;
        }

        public static double PercentOf(this int self, double value)
        {
            return (value*self)/100;
        }

        #endregion [ PercentOf ]

        public static double Pow(this int self, int y)
        {
            return Math.Pow(self, y);
        }

        public static double Factorial(this int self)
        {
            if (self <= 1)
                return 1;
            else
                return self*Factorial(self - 1);
        }

        //Test: phrase = "{{|One|Two}} {{|car|cars}}"
        //Test: phrase = "{{0|1|[value]}} {{|car|cars}}"        
        public static string Pluralization(this int self, string phrase)
        {
            var valuePhrase = Regex.Replace(phrase, @"\[value]", self.ToString());
            var result = Regex.Match(valuePhrase, @"\{{.*?\}}");

            foreach (Group group in result.Groups)
            {
                if (string.IsNullOrWhiteSpace(group.Value))
                    break;

                var content = group.Value.Replace("{{", "").Replace("}}", "");
                var parts = content.Split('|');

                Throw.IfIsFalse(parts.Length == 3,
                    new IndexOutOfRangeException(string.Format("Invalid content template: {0}", group.Value)));

                var newPhrase = string.Empty;
                if (self == 0)
                    newPhrase = valuePhrase.Replace(group.Value, parts[0]);
                if (self == 1)
                    newPhrase = valuePhrase.Replace(group.Value, parts[1]);
                if (self > 1)
                    newPhrase = valuePhrase.Replace(group.Value, parts[2]);

                return self.Pluralization(newPhrase);
            }

            return phrase;
        }

        public static string GetRandomNumber(int length)
        {
            var random = new Random(Environment.TickCount);
            return string.Concat(Enumerable.Range(0, length).Select((index) => random.Next(10).ToString()));
        }
    }
}
