// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartOfAuthority.cs" company="Simon Walker">
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
//   The start of authority.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ZoneFileCreator.RecordTypes
{
    /// <summary>
    /// The start of authority.
    /// </summary>
    public class StartOfAuthority : ResourceRecord
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="StartOfAuthority"/> class.
        /// </summary>
        public StartOfAuthority()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="StartOfAuthority"/> class.
        /// </summary>
        /// <param name="otherRecord">
        /// The other record.
        /// </param>
        public StartOfAuthority(ResourceRecord otherRecord)
        {
            this.RecordType = otherRecord.RecordType;
            this.Zone = otherRecord.Zone;
            this.ZonePath = otherRecord.ZonePath;
            this.RecordValue = otherRecord.RecordValue;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the expiry.
        /// </summary>
        public int Expiry { get; set; }

        /// <summary>
        /// Gets or sets the minimum ttl.
        /// </summary>
        public int MinimumTTL { get; set; }

        /// <summary>
        /// Gets or sets the primary nameserver.
        /// </summary>
        public string PrimaryNameserver { get; set; }

        /// <summary>
        /// Gets or sets the record value.
        /// </summary>
        public sealed override string RecordValue
        {
            get
            {
                return string.Format(
                    "{0} {1} {2} {3} {4} {5} {6}", 
                    this.PrimaryNameserver, 
                    this.ResponsiblePerson, 
                    this.SerialNumber, 
                    this.Refresh, 
                    this.Retry, 
                    this.Expiry, 
                    this.MinimumTTL);
            }

            set
            {
                var strings = value.Split(' ');
                this.PrimaryNameserver = strings[0];
                this.ResponsiblePerson = strings[1];
                this.SerialNumber = int.Parse(strings[2]);
                this.Refresh = int.Parse(strings[3]);
                this.Retry = int.Parse(strings[4]);
                this.Expiry = int.Parse(strings[5]);
                this.MinimumTTL = int.Parse(strings[6]);
            }
        }

        /// <summary>
        /// Gets or sets the refresh.
        /// </summary>
        public int Refresh { get; set; }

        /// <summary>
        /// Gets or sets the responsible person.
        /// </summary>
        public string ResponsiblePerson { get; set; }

        /// <summary>
        /// Gets or sets the retry.
        /// </summary>
        public int Retry { get; set; }

        /// <summary>
        /// Gets or sets the serial number.
        /// </summary>
        public int SerialNumber { get; set; }

        #endregion
    }
}