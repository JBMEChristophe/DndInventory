using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InventoryControlLib
{
    public static class EnumHelper
    {
        public static string GetDescriptionFromEnumValue<T>(T value)
        {
            if (typeof(T).IsEnum)
            {
                DescriptionAttribute attribute = value.GetType()
                    .GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault() as DescriptionAttribute;
                return attribute == null ? value.ToString() : attribute.Description;
            }
            throw new NotSupportedException("Only Enum type is supported");
        }

        public static T GetEnumValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();
            FieldInfo[] fields = type.GetFields();
            var field = fields
                            .SelectMany(f => f.GetCustomAttributes(
                                typeof(DescriptionAttribute), false), (
                                    f, a) => new { Field = f, Att = a })
                            .Where(a => ((DescriptionAttribute)a.Att)
                                .Description == description).SingleOrDefault();
            return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
        }

        public static List<string> GetDescriptionListFromEnumList<T>(IList<T> values)
        {
            if (typeof(T).IsEnum)
            {
                var result = new List<string>();
                foreach (var value in values)
                {
                    result.Add(GetDescriptionFromEnumValue(value));
                }

                return result;
            }
            throw new NotSupportedException("Only List<Enum> type is supported");
        }
    }
}
