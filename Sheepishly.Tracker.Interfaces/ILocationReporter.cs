namespace Sheepishly.Tracker.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface ILocationReporter : IService
    {
        Task ReportLocation(Location location);
    }
}