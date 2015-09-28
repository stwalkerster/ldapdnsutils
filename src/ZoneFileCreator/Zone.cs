// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Zone.cs" company="Simon Walker">
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
//   The zone.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ZoneFileCreator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    using ZoneFileCreator.RecordTypes;

    /// <summary>
    /// The zone.
    /// </summary>
    public class Zone
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Zone"/> class.
        /// </summary>
        /// <param name="zoneOrigin">
        /// The zone origin.
        /// </param>
        /// <param name="zoneDn">
        /// The zone DN.
        /// </param>
        /// <param name="resourceRecords">
        /// The resource records.
        /// </param>
        public Zone(string zoneOrigin, string zoneDn, List<ResourceRecord> resourceRecords)
        {
            this.ZoneOrigin = zoneOrigin;
            this.ZoneDN = zoneDn;
            this.ResourceRecords = resourceRecords;
            this.DefaultTimeToLive = 3600;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the default time to live.
        /// </summary>
        public int DefaultTimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the last modification.
        /// </summary>
        public DateTime LastModification { get; set; }

        /// <summary>
        /// Gets or sets the resource records.
        /// </summary>
        public List<ResourceRecord> ResourceRecords { get; set; }

        /// <summary>
        /// Gets or sets the zone dn.
        /// </summary>
        public string ZoneDN { get; set; }

        /// <summary>
        /// Gets or sets the zone root.
        /// </summary>
        public string ZoneOrigin { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("ZoneOrigin: {0}", this.ZoneOrigin);
        }

        /// <summary>
        /// The write zone file.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public void CreateZoneFileData(Stream stream = null)
        {
            // set up the stream
            var ms = stream ?? new MemoryStream();
            var sw = new StreamWriter(ms) { NewLine = "\n" };

            // set up the data we need first
            var pathLength = 8;
            var recordTypeLength = 6;
            var ttlLength = 0;
            foreach (var record in this.ResourceRecords)
            {
                pathLength = Math.Max(pathLength, record.ZonePath.Length);
                recordTypeLength = Math.Max(recordTypeLength, record.RecordType.DnsRecordType.Length);
                ttlLength = Math.Max(ttlLength, record.TimeToLive.Length);
            }

            // create zone file
            sw.WriteLine(";");
            sw.WriteLine("; BIND data file for {0}", this.ZoneOrigin);
            sw.WriteLine(";");
            sw.WriteLine("; Managed in LDAP, last modified {0}", this.LastModification);
            sw.WriteLine("; DN: {0}", this.ZoneDN);
            sw.WriteLine(";");
            sw.WriteLine("$TTL  " + this.DefaultTimeToLive);

            // handle some special cases first
            sw.WriteLine(";");
            sw.WriteLine("; Start of Authority");
            foreach (var record in this.ResourceRecords.Where(x => x.RecordType == RecordType.SOA))
            {
                var soaRecord = new StartOfAuthority(record);

                var path = soaRecord.ZonePath == string.Empty ? "@" : soaRecord.ZonePath;

                sw.WriteLine(
                    "{0} {9} IN   {1} {2} {3} {4} {5} {6} {7} {8}", 
                    path.PadRight(pathLength), 
                    soaRecord.RecordType.DnsRecordType.PadRight(recordTypeLength), 
                    soaRecord.PrimaryNameserver, 
                    soaRecord.ResponsiblePerson, 
                    this.LastModification.ToSerialNumber(), 
                    soaRecord.Refresh, 
                    soaRecord.Retry, 
                    soaRecord.Expiry, 
                    this.DefaultTimeToLive, 
                    soaRecord.TimeToLive.PadRight(ttlLength));
            }

            sw.WriteLine(";");
            sw.WriteLine("; Remaining records");

            var remainingRecords =
                this.ResourceRecords.Where(x => x.RecordType != RecordType.SOA)
                    .OrderBy(x => string.Join(".", x.ZonePath.Split('.').Reverse()));

            foreach (var record in remainingRecords)
            {
                var path = record.ZonePath == string.Empty ? "@" : record.ZonePath;

                var recordValue = record.RecordValue;

                if (record.RecordType == RecordType.NS || record.RecordType == RecordType.CNAME
                    || record.RecordType == RecordType.MX || record.RecordType == RecordType.SRV)
                {
                    if (!recordValue.EndsWith("."))
                    {
                        recordValue += ".";
                    }
                }

                sw.WriteLine(
                    "{0} {3} IN   {1} {2}", 
                    path.PadRight(pathLength), 
                    record.RecordType.DnsRecordType.PadRight(recordTypeLength), 
                    recordValue,
                    record.TimeToLive.PadRight(ttlLength));
            }

            sw.Flush();
        }

        #endregion
    }
}