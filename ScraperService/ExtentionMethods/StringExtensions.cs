using System;

namespace ScraperService.ExtentionMethods
{
    public static class StringExtensions
    {
        //Simple extention method for Strings, this method cleans the string
        //This method is very specific to our use case since we expect the string to be something like this: "Wanted String Part ( Unwanted String Part )"
        public static string CleanString(this String str)
        {
            int index = str.IndexOf("(");
            return index == -1?  str : str.Remove(index - 1);
        }
    }
}
