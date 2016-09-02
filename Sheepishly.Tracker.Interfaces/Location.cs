namespace Sheepishly.Tracker.Interfaces
{
    using System;
    public class Location
    {
        public Guid SheepId { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}