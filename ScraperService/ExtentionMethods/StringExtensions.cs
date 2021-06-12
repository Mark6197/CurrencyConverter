using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.ExtentionMethods
{
    public static class StringExtensions
    {
        public static string RemoveDateFromString(this String str)
        {
            int index = str.IndexOf("(");
            return index == -1?  str : str.Remove(index - 1);
        }
    }
}
