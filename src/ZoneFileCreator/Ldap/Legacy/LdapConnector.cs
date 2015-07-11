// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LdapConnector.cs" company="Simon Walker">
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
//   The ldap connector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ZoneFileCreator.Ldap.Legacy
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.DirectoryServices.Protocols;
    using System.Globalization;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// The LDAP connector.
    /// </summary>
    public class LdapConnector
    {
        #region Fields

        /// <summary>
        /// The DNS root DN.
        /// </summary>
        private readonly string dnsRootDn;

        /// <summary>
        /// The LDAP connection.
        /// </summary>
        private readonly LdapConnection ldapConnection;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="LdapConnector"/> class.
        /// </summary>
        /// <param name="hostname">
        /// The hostname.
        /// </param>
        /// <param name="networkCredential">
        /// The network credential.
        /// </param>
        /// <param name="dnsRootDn">
        /// The DNS root DN.
        /// </param>
        public LdapConnector(string hostname, NetworkCredential networkCredential, string dnsRootDn)
        {
            var ldapDirectoryIdentifier = new LdapDirectoryIdentifier(hostname);
            this.ldapConnection = new LdapConnection(ldapDirectoryIdentifier, networkCredential, AuthType.Basic);
            this.ldapConnection.SessionOptions.ProtocolVersion = 3;
            this.dnsRootDn = dnsRootDn;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get DNS data.
        /// </summary>
        /// <returns>
        /// The <see cref="List{Zone}"/>.
        /// </returns>
        public IEnumerable<Zone> GetDnsData()
        {
            var sr = new SearchRequest(
                this.dnsRootDn,
                "(&(objectClass=dnsdomain2)(soarecord=*))", 
                SearchScope.Subtree, 
                "associateddomain");
            var directoryResponse = (SearchResponse)this.ldapConnection.SendRequest(sr);

            var zones = directoryResponse.Entries.Cast<SearchResultEntry>().Select(
                d =>
                    {
                        var zoneOrigin = (string)d.Attributes["associateddomain"][0];

                        var resourceRecords = new List<ResourceRecord>();
                        var zone = new Zone(zoneOrigin, d.DistinguishedName, resourceRecords);

                        var domainDnsData = this.GetDomainDnsData(zone, d.DistinguishedName, string.Empty);
                        resourceRecords.AddRange(domainDnsData);

                        return zone;
                    }).ToList();
            
            return zones;
        }

        /// <summary>
        /// The get domain DNS data.
        /// </summary>
        /// <param name="domainBase">
        /// The domain base.
        /// </param>
        /// <param name="domainDN">
        /// The domain DN.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The <see cref="List{ResourceRecord}"/>.
        /// </returns>
        public List<ResourceRecord> GetDomainDnsData(Zone domainBase, string domainDN, string path)
        {
            var attributes = RecordType.SupportedRecordTypes().Union(new[] { "dc", "modifyTimestamp" }).ToArray();

            var sr = new SearchRequest(domainDN, "(objectClass=dnsdomain2)", SearchScope.Base, attributes);
            var directoryResponse = (SearchResponse)this.ldapConnection.SendRequest(sr);

            var records = new List<ResourceRecord>();

            foreach (var entry in directoryResponse.Entries.Cast<SearchResultEntry>())
            {
                foreach (var attr in entry.Attributes)
                {
                    var attribute = ((DictionaryEntry)attr).Value as DirectoryAttribute;

                    if (attribute == null)
                    {
                        continue;
                    }

                    if (attribute.Name == "dc")
                    {
                        continue;
                    }

                    // last modification timestamp for SOA
                    if (attribute.Name == "modifyTimestamp")
                    {
                        var timestamp = attribute.GetValues(typeof(string)).Cast<string>().FirstOrDefault();
                        var dateTime = DateTime.ParseExact(timestamp, "yyyyMMddHHmmssZ", CultureInfo.InvariantCulture);

                        if (DateTime.Compare(dateTime, domainBase.LastModification) > 0)
                        {
                            domainBase.LastModification = dateTime;
                        }

                        continue;
                    }

                    records.AddRange(
                        attribute.ToStringCollection()
                            .ToList()
                            .Select(
                                x =>
                                new ResourceRecord
                                    {
                                        ZonePath = path, 
                                        RecordType = RecordType.FromLdap(attribute.Name), 
                                        Zone = domainBase, 
                                        RecordValue = x
                                    }));
                }
            }

            var srsub = new SearchRequest(domainDN, "(objectClass=dnsdomain2)", SearchScope.OneLevel, "dc");
            var srsubresult = (SearchResponse)this.ldapConnection.SendRequest(srsub);

            foreach (var entry in srsubresult.Entries.Cast<SearchResultEntry>())
            {
                var dc = (string)entry.Attributes["dc"][0];
                var actualPath = (dc + "." + path).TrimEnd('.');

                records.AddRange(this.GetDomainDnsData(domainBase, entry.DistinguishedName, actualPath));
            }

            return records;
        }

        #endregion
    }
}