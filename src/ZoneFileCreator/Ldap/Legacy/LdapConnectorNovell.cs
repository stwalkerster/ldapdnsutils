// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LdapConnectorNovell.cs" company="Simon Walker">
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
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;

    using Novell.Directory.Ldap;

    /// <summary>
    /// The LDAP connector.
    /// </summary>
    public class LdapConnectorNovell : ILdapConnector
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
        /// Initialises a new instance of the <see cref="LdapConnectorNovell"/> class.
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
        public LdapConnectorNovell(string hostname, NetworkCredential networkCredential, string dnsRootDn)
        {
            this.ldapConnection = new LdapConnection();
            this.ldapConnection.Connect(hostname, 389);
            this.ldapConnection.Bind(networkCredential.UserName, networkCredential.Password);

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
            var ldapSearchResults = this.ldapConnection.Search(
                this.dnsRootDn,
                LdapConnection.SCOPE_SUB,
                "(&(objectClass=dnsdomain2)(soarecord=*))",
                new[] { "associateddomain" },
                false);
            
            var zones = ldapSearchResults.ToList().Select(
                d =>
                    {
                        var zoneOrigin = d.getAttribute("associateddomain").StringValue;

                        var resourceRecords = new List<ResourceRecord>();
                        var zone = new Zone(zoneOrigin, d.DN, resourceRecords);

                        var domainDnsData = this.GetDomainDnsData(zone, d.DN, string.Empty);
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

            var ldapSearchResults = this.ldapConnection.Search(
                domainDN,
                LdapConnection.SCOPE_BASE,
                "(objectClass=dnsdomain2)",
                attributes,
                false).ToList();
            
            var records = new List<ResourceRecord>();

            foreach (var entry in ldapSearchResults)
            {
                foreach (LdapAttribute attribute in entry.getAttributeSet().Cast<LdapAttribute>())
                {
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
                        var dateTime = DateTime.ParseExact(attribute.StringValue, "yyyyMMddHHmmssZ", CultureInfo.InvariantCulture);

                        if (DateTime.Compare(dateTime, domainBase.LastModification) > 0)
                        {
                            domainBase.LastModification = dateTime;
                        }

                        continue;
                    }

                    records.AddRange(
                        attribute.StringValueArray
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

            var ldapSubSearchResults = this.ldapConnection.Search(
                domainDN,
                LdapConnection.SCOPE_ONE,
                "(objectClass=dnsdomain2)",
                new[] { "dc" },
                false).ToList();
            
            foreach (var entry in ldapSubSearchResults)
            {
                var dc = entry.getAttribute("dc").StringValue;
                var actualPath = (dc + "." + path).TrimEnd('.');

                records.AddRange(this.GetDomainDnsData(domainBase, entry.DN, actualPath));
            }

            return records;
        }

        #endregion
    }
}