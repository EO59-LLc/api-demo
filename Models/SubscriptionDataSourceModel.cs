using System;

namespace EO59.Api.Demo.Models
{
    /// <summary>
    /// Information about the data set, this can be used to monitor for
    /// changes, for example, you can compare LastUpdate field value with
    /// previous download, if the value has changed, data can be updated.
    /// Please also pay attention to field NumberOfPoints as this is needed
    /// to create paged download as maximum number of points down-loadable in one
    /// request is limited to 10 000.
    /// </summary>
    public class SubscriptionDataSourceModel
    {
        /// <summary>
        /// Key value that needs to be used when reading data context.
        /// </summary>
        public int DataSourceKey { get; set; }

        /// <summary>
        /// Latest update date when data was changed, this field can be used by automation
        /// for determining if the update needs to be downloaded.
        /// </summary>
        public DateTimeOffset LastUpdate { get; set; }

        /// <summary>
        /// Start date for data in the set.
        /// </summary>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// End date for data in the set.
        /// </summary>
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// Number of data points in the set. This value is paramount for creating
        /// paged gradual download automation.
        /// </summary>
        public int NumberOfPoints { get; set; }

        /// <summary>
        /// Name of the satellite data is coming from.
        /// </summary>
        public string SatellitesName { get; set; }

        /// <summary>
        /// Orbit definition
        /// </summary>
        public string Orbit { get; set; }

        /// <summary>
        /// Is data based on reflector devices
        /// </summary>
        public bool CornerReflector { get; set; }
    }
}