namespace Sheepishly.IngestPlus
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Tracker.Interfaces;

    public class TrackerController : ApiController
    {
        [HttpGet]
        [Route("")]
        public string Index()
        {
            return "Welcome to Sheepishly 1.0.0 - The Combleat Sheep Tracking Suite";
        }

        [HttpPost]
        [Route("locations")]
        public async Task<bool> Log(Location location)
        {
            var reporter = TrackerConnectionFactory.CreateLocationReporter();
            await reporter.ReportLocation(location);
            return true;
        }

        [HttpPost]
        [Route("killasheep")]
        public async Task<bool> KillASheep(Guid sheepId)
        {
            var reporter = TrackerConnectionFactory.CreateSheepRemover();
            var result = await reporter.Delete(sheepId);
            return result;
        }

        [HttpGet]
        [Route("sheep/{sheepId}/lastseen")]
        public async Task<DateTime?> LastSeen(Guid sheepId)
        {
            var viewer = TrackerConnectionFactory.CreateLocationViewer();
            return await viewer.GetLastReportTime(sheepId);
        }

        [HttpGet]
        [Route("sheep/{sheepId}/lastlocation")]
        public async Task<object> LastLocation(Guid sheepId)
        {
            var viewer = TrackerConnectionFactory.CreateLocationViewer();
            var location = await (viewer.GetLastSheepLocation(sheepId));
            return location == null ? null : new {Latitude = location.Value.Key, Longitude = location.Value.Value};
        }
    }

}