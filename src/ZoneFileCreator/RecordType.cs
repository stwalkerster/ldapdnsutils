// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecordType.cs" company="Simon Walker">
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
//   The record type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ZoneFileCreator
{
    using System.Collections.Generic;
    using System.Collections.Specialized;

    /// <summary>
    /// The record type.
    /// </summary>
    public class RecordType
    {
        #region Static Fields

        public static RecordType A = new RecordType("A", "arecord");
        public static RecordType AAAA = new RecordType("AAAA", "aaaarecord");
        public static RecordType CAA = new RecordType("CAA", "caarecord");
        public static RecordType CNAME = new RecordType("CNAME", "cnamerecord");
        public static RecordType MX = new RecordType("MX", "mxrecord");
        public static RecordType NS = new RecordType("NS", "nsrecord");
        public static RecordType SOA = new RecordType("SOA", "soarecord");
        public static RecordType SRV = new RecordType("SRV", "srvrecord");
        public static RecordType SPF = new RecordType("SPF", "spfrecord");
        public static RecordType SSHFP = new RecordType("SSHFP", "sshfprecord");
        public static RecordType TXT = new RecordType("TXT", "txtrecord");
        
        private static Dictionary<string, RecordType> DnsLookup = new Dictionary<string, RecordType>();
        private static Dictionary<string, RecordType> LdapLookup = new Dictionary<string, RecordType>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises static members of the <see cref="RecordType"/> class.
        /// </summary>
        static RecordType()
        {
            Add(A);
            Add(AAAA);
            Add(CAA);
            Add(CNAME);
            Add(MX);
            Add(NS);
            Add(SOA);
            Add(SPF);
            Add(SRV);
            Add(SSHFP);
            Add(TXT);
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="RecordType"/> class.
        /// </summary>
        /// <param name="dnsRecordType">
        /// The dns record type.
        /// </param>
        /// <param name="directoryAttribute">
        /// The directory attribute.
        /// </param>
        private RecordType(string dnsRecordType, string directoryAttribute)
        {
            this.DnsRecordType = dnsRecordType;
            this.DirectoryAttribute = directoryAttribute;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the directory attribute.
        /// </summary>
        public string DirectoryAttribute { get; private set; }

        /// <summary>
        /// Gets the DNS record type.
        /// </summary>
        public string DnsRecordType { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The from DNS.
        /// </summary>
        /// <param name="dnsRecordType">
        /// The DNS record type.
        /// </param>
        /// <returns>
        /// The <see cref="RecordType"/>.
        /// </returns>
        public static RecordType FromDNS(string dnsRecordType)
        {
            RecordType recordType;
            var result = DnsLookup.TryGetValue(dnsRecordType.ToLowerInvariant(), out recordType);
            return result ? recordType : null;
        }

        /// <summary>
        /// The from LDAP.
        /// </summary>
        /// <param name="attributeName">
        /// The attribute name.
        /// </param>
        /// <returns>
        /// The <see cref="RecordType"/>.
        /// </returns>
        public static RecordType FromLdap(string attributeName)
        {
            RecordType recordType;
            var result = LdapLookup.TryGetValue(attributeName.ToLowerInvariant(), out recordType);
            return result ? recordType : new RecordType(attributeName.ToLowerInvariant(), attributeName.ToLowerInvariant());
        }

        /// <summary>
        /// The supported record types.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{string}"/>.
        /// </returns>
        public static IEnumerable<string> SupportedRecordTypes()
        {
            return LdapLookup.Keys;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="recordType">
        /// The recordType.
        /// </param>
        private static void Add(RecordType recordType)
        {
            if (recordType.DirectoryAttribute != string.Empty)
            {
                LdapLookup.Add(recordType.DirectoryAttribute, recordType);
            }

            if (recordType.DnsRecordType != string.Empty)
            {
                DnsLookup.Add(recordType.DnsRecordType, recordType);
            }
        }

        #endregion
    }
}