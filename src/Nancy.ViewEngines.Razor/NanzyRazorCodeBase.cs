namespace Nancy.ViewEngines.Razor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;

    using Nancy.Helpers;

    public class NanzyRazorCodeBase
    {
        public static void WriteAttributeTo(TextWriter writer, string name, Tuple<string, int> prefix, Tuple<string, int> suffix, params AttributeValue[] values)
        {
            var attributeValue = BuildAttribute(name, prefix, suffix, values);
            WriteLiteralTo(writer, attributeValue);
        }

        private static string BuildAttribute(string name, Tuple<string, int> prefix, Tuple<string, int> suffix,
                                             params AttributeValue[] values)
        {
            var writtenAttribute = false;
            var attributeBuilder = new StringBuilder(prefix.Item1);

            foreach (var value in values)
            {
                if (ShouldWriteValue(value.Value.Item1))
                {
                    var stringValue = GetStringValue(value);
                    var valuePrefix = value.Prefix.Item1;

                    if (!string.IsNullOrEmpty(valuePrefix))
                    {
                        attributeBuilder.Append(valuePrefix);
                    }

                    attributeBuilder.Append(stringValue);
                    writtenAttribute = true;
                }
            }

            attributeBuilder.Append(suffix.Item1);

            var renderAttribute = writtenAttribute || values.Length == 0;

            if (renderAttribute)
            {
                return attributeBuilder.ToString();
            }

            return string.Empty;
        }

        private static string GetStringValue(AttributeValue value)
        {
            if (value.IsLiteral)
            {
                return (string)value.Value.Item1;
            }

            if (value.Value.Item1 is IHtmlString)
            {
                return ((IHtmlString)value.Value.Item1).ToHtmlString();
            }

            if (value.Value.Item1 is DynamicDictionaryValue)
            {
                var dynamicValue = (DynamicDictionaryValue)value.Value.Item1;
                return dynamicValue.HasValue ? dynamicValue.Value.ToString() : string.Empty;
            }

            return value.Value.Item1.ToString();
        }

        private static bool ShouldWriteValue(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is bool)
            {
                var boolValue = (bool)value;

                return boolValue;
            }

            return true;
        }

        /// <summary>
        /// Writes the provided <paramref name="value"/> to the provided <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> that should be written to.</param>
        /// <param name="value">The value that should be written.</param>
        public static void WriteTo(TextWriter writer, object value)
        {
            writer.Write(HtmlEncode(value));
        }

        /// <summary>
        /// Writes the provided <paramref name="value"/>, as a literal, to the provided <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> that should be written to.</param>
        /// <param name="value">The value that should be written as a literal.</param>
        public static void WriteLiteralTo(TextWriter writer, object value)
        {
            writer.Write(value);
        }

        /// <summary>
        /// Writes the provided <paramref name="value"/> to the provided <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> that should be written to.</param>
        /// <param name="value">The <see cref="HelperResult"/> that should be written.</param>
        public static void WriteTo(TextWriter writer, HelperResult value)
        {
            if (value != null)
            {
                value.WriteTo(writer);
            }
        }

        /// <summary>
        /// Writes the provided <paramref name="value"/>, as a literal, to the provided <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> that should be written to.</param>
        /// <param name="value">The <see cref="HelperResult"/> that should be written as a literal.</param>
        public static void WriteLiteralTo(TextWriter writer, HelperResult value)
        {
            if (value != null)
            {
                value.WriteTo(writer);
            }
        }

        /// <summary>
        /// Html encodes an object if required
        /// </summary>
        /// <param name="value">Object to potentially encode</param>
        /// <returns>String representation, encoded if necessary</returns>
        private static string HtmlEncode(object value)
        {
            if (value == null)
            {
                return null;
            }

            var str = value as IHtmlString;

            return str != null ? str.ToHtmlString() : HttpUtility.HtmlEncode(Convert.ToString(value, CultureInfo.CurrentCulture));
        }
    }
}