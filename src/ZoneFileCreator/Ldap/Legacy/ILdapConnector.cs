namespace ZoneFileCreator.Ldap.Legacy
{
    using System.Collections.Generic;

    public interface ILdapConnector
    {
        /// <summary>
        /// The get DNS data.
        /// </summary>
        /// <returns>
        /// The <see cref="List{Zone}"/>.
        /// </returns>
        IEnumerable<Zone> GetDnsData();

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
        List<ResourceRecord> GetDomainDnsData(Zone domainBase, string domainDN, string path);
    }
}