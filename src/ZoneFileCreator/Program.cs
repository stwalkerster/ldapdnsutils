// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Simon Walker">
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
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ZoneFileCreator
{
    using System.IO;
    using System.Net;

    using ZoneFileCreator.Ldap.Legacy;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        #region Methods

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            var directoryInfo = new DirectoryInfo("output");
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }

            var ldapConnector = new LdapConnectorNovell(args[0], new NetworkCredential(args[1], args[2]), args[3]);

            var zones = ldapConnector.GetDnsData();

            directoryInfo.Create();

            var configFileStream = File.Create("output/named.conf.localzones");
            var configWriter = new StreamWriter(configFileStream) { NewLine = "\n" };

            var checkZonesStream = File.Create("output/checkzones.sh");
            var checkZonesWriter = new StreamWriter(checkZonesStream) { NewLine = "\n" };

            checkZonesWriter.WriteLine("#!/bin/bash");

            foreach (var zone in zones)
            {
                var fileStream = File.Create(string.Format("output/db.{0}", zone.ZoneOrigin));
                zone.CreateZoneFileData(fileStream);
                fileStream.Close();

                configWriter.WriteLine("zone \"{0}\" {{\n\ttype master;\n\tfile \"/etc/bind/db.{0}\";\n}};\n", zone.ZoneOrigin);
                checkZonesWriter.WriteLine("echo \">>> Checking zone: {0}\"", zone.ZoneOrigin);
                checkZonesWriter.WriteLine("named-checkzone {0} db.{0}", zone.ZoneOrigin);
            }

            configWriter.Flush();
            configFileStream.Close();

            checkZonesWriter.Flush();
            checkZonesStream.Close();
        }

        #endregion
    }
}