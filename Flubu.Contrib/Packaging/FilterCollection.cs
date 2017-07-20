using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Flubu.Packaging
{
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class FilterCollection : IFileFilter
    {
        public FilterCollection Add (IFileFilter filter)
        {
            filters.Add(filter);
            return this;
        }

        public bool IsPassedThrough(string fileName)
        {
            foreach (IFileFilter filter in filters)
            {
                if (false == filter.IsPassedThrough(fileName))
                    return false;
            }

            return true;
        }

        private readonly List<IFileFilter> filters = new List<IFileFilter>();
    }
}