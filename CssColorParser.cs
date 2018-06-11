using System;
using System.Text.RegularExpressions;

using UIKit;

namespace OuttuneMail
{
    static class CssColorParser
    {
        static readonly Regex functionRegex = new Regex (@"(?<func>rgb|rgba)\(\s*(?<r>[0-9]+)\s*,\s*(?<g>[0-9]+)\s*,\s*(?<b>[0-9]+)(\s*,\s*(?<a>[0-9.]+))?\s*\);?");

        public static UIColor Parse (string cssColor)
        {
            var functionMatch = functionRegex.Match (cssColor);
            if (functionMatch.Success) {
                nfloat alpha = 1;
                if (!string.IsNullOrEmpty (functionMatch.Groups ["a"].Value))
                    alpha = nfloat.Parse (functionMatch.Groups ["a"].Value);
                
                return UIColor.FromRGBA (
                    int.Parse (functionMatch.Groups ["r"].Value) / 255f,
                    int.Parse (functionMatch.Groups ["g"].Value) / 255f,
                    int.Parse (functionMatch.Groups ["b"].Value) / 255f,
                    alpha);
            }

            return UIColor.Black;
        }
    }
}