﻿namespace Sheepishly.Tracker
{
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Interfaces;
    using System.Fabric;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;
    using Sheep.Interfaces;

    internal sealed class Tracker : StatefulService, ILocationReporter, ILocationViewer, ISheepRemover
    {
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] {
                new ServiceReplicaListener(this.CreateServiceRemotingListener)
            };
        }

        public async Task<bool> Delete(Guid sheepId)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var timestamps = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, DateTime>>("timestamps");
                var sheepIds = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, ActorId>>("sheepIds");

                //delete sheep
                await sheepIds.TryRemoveAsync(tx, sheepId);
                //await //SheepConnectionFactory.GetSheep(sheepActorId).SetLocation(timestamp, location.Latitude, location.Longitude);

                //remove timestamps for this sheep
                await timestamps.TryRemoveAsync(tx, sheepId);
                await tx.CommitAsync();
                return true;
            }
        }

        public async Task ReportLocation(Location location)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var timestamps = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, DateTime>>("timestamps");
                var sheepIds = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, ActorId>>("sheepIds");

                var timestamp = DateTime.UtcNow;

                // Update sheep
                var sheepActorId = await sheepIds.GetOrAddAsync(tx, location.SheepId, ActorId.CreateRandom());
                await SheepConnectionFactory.GetSheep(sheepActorId).SetLocation(timestamp, location.Latitude, location.Longitude);

                // Update service with new timestamp
                await timestamps.AddOrUpdateAsync(tx, location.SheepId, DateTime.UtcNow, (guid, time) => timestamp);
                await tx.CommitAsync();
            }
        }

        public async Task<KeyValuePair<float, float>?> GetLastSheepLocation(Guid sheepId)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var sheepIds = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, ActorId>>("sheepIds");

                var sheepActorId = await sheepIds.TryGetValueAsync(tx, sheepId);
                if (!sheepActorId.HasValue)
                    return null;

                var sheep = SheepConnectionFactory.GetSheep(sheepActorId.Value);
                return await sheep.GetLatestLocation();
            }
        }

        public async Task<DateTime?> GetLastReportTime(Guid sheepId)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var timestamps = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, DateTime>>("timestamps");

                var timestamp = await timestamps.TryGetValueAsync(tx, sheepId);
                await tx.CommitAsync();

                return timestamp.HasValue ? (DateTime?)timestamp.Value : null;
            }
        }

        public Tracker(StatefulServiceContext serviceContext) : base(serviceContext)
        {
        }

        public Tracker(StatefulServiceContext serviceContext, IReliableStateManagerReplica reliableStateManagerReplica) : base(serviceContext, reliableStateManagerReplica)
        {
        }

        
    }
}