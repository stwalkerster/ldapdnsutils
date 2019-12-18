// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Simon Walker">
//   Copyright (C) 2014 Simon Walker
//   
//   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//   documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
//   the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
//   to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
//   the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
//   THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
//   SOFTWARE.
// </copyright>
// <summary>
//   The string extensions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ZoneFileCreator
{
    using System;
    using System.Collections.Generic;

    using Novell.Directory.Ldap;

    /// <summary>
    /// The string extensions.
    /// </summary>
    public static class ExtensionMethods
    {
        #region Public Methods and Operators

        /// <summary>
        /// Cast to a nullable integer.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="System.Nullable{Int32}"/>.
        /// </returns>
        public static int? ToIntOrNull(this string input)
        {
            int result;
            if (int.TryParse(input, out result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// The to serial number.
        /// </summary>
        /// <param name="lastModification">
        /// The last modification.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToSerialNumber(this DateTime lastModification)
        {
            var value = string.Format(
                "{0}{1,2:D2}",
                lastModification.ToString("yyyyMMdd"),
                (lastModification.Hour * 4) + (lastModification.Minute / 15));

            return value;
        }

        public static IList<LdapEntry> ToList(this LdapSearchResults results)
        {
            List<LdapEntry> entries = new List<LdapEntry>();

            // Argh Novell library, why you no implement IEnumerable!?!
            while (results.hasMore())
            {
                LdapEntry entry = results.next();
                entries.Add(entry);
            }

            return entries;
        }

        #endregion
    }
}