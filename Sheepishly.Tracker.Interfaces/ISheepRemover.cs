namespace Sheepishly.Tracker.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface ISheepRemover: IService
    {
        Task<bool> Delete(Guid sheepId);
    }
}