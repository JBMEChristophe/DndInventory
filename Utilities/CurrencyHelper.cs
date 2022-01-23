using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utilities
{
    public class Currency
    {
        public double Value { get; set; }
        public CurrencyType Type { get; set; }

        public Currency(double value, CurrencyType type)
        {
            Value = value;
            Type = type;
        }

        public static Currency operator +(Currency a, Currency b)
        {
            var bInAType = b.ValueIn(a.Type);
            return new Currency(a.Value + bInAType, a.Type);
        }

        public static Currency operator *(Currency a, double times)
        {
            return new Currency(a.Value * times, a.Type);
        }

        public void ConvertTo(CurrencyType type)
        {
            Value *= CurrencyHelper.GetConversionRate(Type, type);
            Type = type;
        }
        
        public double ValueIn(CurrencyType type)
        {
            return Value * CurrencyHelper.GetConversionRate(Type, type);
        }

        public void ConvertToMaxType()
        {
            double resultValue = Value;
            CurrencyType resultType = Type;
            foreach (var tmp in Enum.GetValues(typeof(CurrencyType)))
            {
                double tmpValue = Value;
                var type = (CurrencyType)tmp;
                tmpValue *= CurrencyHelper.GetConversionRate(Type, type);
                bool wholeNumber = Math.Abs(tmpValue % 1) <= (Double.Epsilon * 100);
                if (wholeNumber)
                {
                    resultValue = tmpValue;
                    resultType = type;
                }
                else
                {
                    break;
                }
            }
            Value = resultValue;
            Type = resultType;
        }

        public override string ToString()
        {
            return $"{Value} {EnumHelper.GetDescriptionFromEnumValue(Type)}";
        }
    }

    public enum CurrencyType
    {
        [Description("cp")]
        CP = 0,
        [Description("sp")]
        SP = 1,
        [Description("ep")]
        EP = 2,
        [Description("gp")]
        GP = 3,
        [Description("pp")]
        PP = 4
    }

    public static class CurrencyHelper
    {
        public static Currency ConvertStringToCurrency(string text)
        {
            var numAlpha = new Regex("(?<Numeric>[0-9]*)(?<Alpha>[a-zA-Z\\s]*)");
            var match = numAlpha.Match(text);

            string numberPart = match.Groups["Numeric"].Value;
            string alphaPart = match.Groups["Alpha"].Value;

            double value = 0.0;
            double.TryParse(numberPart, out value);

            return new Currency(value, EnumHelper.GetEnumValueFromDescription<CurrencyType>(alphaPart.Trim()));
        }

        public static double GetConversionRate(CurrencyType from, CurrencyType to)
        {
            switch (to)
            {
                case CurrencyType.CP:
                    return new double[]{ 1.0, 10.0, 50.0, 100.0, 1000.0 }[(int)from];
                case CurrencyType.SP:
                    return new double[] { 1.0/10.0, 1.0, 5.0, 10.0, 100.0 }[(int)from];
                case CurrencyType.EP:
                    return new double[] { 1.0/50.0, 1.0/5.0, 1.0, 2.0, 20.0 }[(int)from];
                case CurrencyType.GP:
                    return new double[] { 1.0/100.0, 1.0/10.0, 1.0/2.0, 1.0, 10.0 }[(int)from];
                case CurrencyType.PP:
                    return new double[] { 1.0/1000.0, 1.0/100.0, 1.0/20.0, 1.0/10.0, 1.0 }[(int)from];
                default:
                    return 1.0;
            }
        }
    }
}
