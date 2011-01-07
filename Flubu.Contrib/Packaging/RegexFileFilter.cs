using System.Text.RegularExpressions;

namespace Flubu.Packaging
{
    public class RegexFileFilter : IFileFilter
    {
        public RegexFileFilter(string filterRegexValue)
        {
            filterRegex = new Regex(filterRegexValue, RegexOptions.IgnoreCase);
        }

        public bool IsPassedThrough(string fileName)
        {
            return false == filterRegex.IsMatch(fileName);
        }

        private readonly Regex filterRegex;
    }
}