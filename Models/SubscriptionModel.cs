using System.Collections.Generic;

namespace EO59.Api.Downloader.Models
{
    /// <summary>
    /// Information block about subscription.
    /// This data can be used to automatically monitor subscription for updates,
    /// please check information on SubscriptionDataSource for more details on how to
    /// detect changes and prepare to download data.
    /// </summary>
    public class SubscriptionModel
    {
        /// <summary>
        /// Name of the object (site) in subscription.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Central Latitude of object in subscription.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Central Longitude of object in subscription.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// List of data sets in the subscription.
        /// </summary>
        public List<SubscriptionDataSource> DataSets { get; set; } = new List<SubscriptionDataSource>();
    }
}