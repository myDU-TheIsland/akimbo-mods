using Orleans;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Backend;
using Backend.Business;
using Backend.Database;
using NQutils.Config;
using Backend.Storage;
using Backend.Scenegraph;
using NQ;
using NQ.RDMS;
using NQ.Interfaces;
using NQutils;
using NQutils.Net;
using NQutils.Serialization;
using NQutils.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.Runtime;

//Mod class, must be called MyDuMod and implements IMod
public class MyDuMod : IMod, ISubObserver
{

    private IServiceProvider isp;
    private IClusterClient orleans;
    private ILogger logger;

    public string GetName()
    {
        return "AkimboAdmin";
    }

    public Task Initialize(IServiceProvider isp)
    {
        this.isp = isp;
        this.orleans = isp.GetRequiredService<IClusterClient>();
        this.logger = isp.GetRequiredService<ILogger<MyDuMod>>();
        return Task.CompletedTask;
    }

    /// Return a ModInfo to a connecting client
    public Task<ModInfo> GetModInfoFor(ulong playerId, bool admin)
    {
        var res = new ModInfo
        {
            name = "AkimboAdmin",
            actions = new List<ModActionDefinition>
            { },
        };
        // Only send this stuff to players flagged as admin in the BO
        if (admin)
            res.actions.AddRange(new List<ModActionDefinition>
                {
                    new ModActionDefinition
                    {
                        id = 201,
                        label = "Admin\\Constructs\\Get construct id",
                        context = ModActionContext.Construct,
                    },
                    new ModActionDefinition
                    {
                        id = 202,
                        label = "Admin\\Elements\\Get element id",
                        context = ModActionContext.Element,
                    },
                    new ModActionDefinition
                    {
                        id = 203,
                        label = "Admin\\Players\\Get player id",
                        context = ModActionContext.Avatar,
                    },
                    new ModActionDefinition
                    {
                        id = 1,
                        label = "Admin\\Constructs\\disown construct",
                        context = ModActionContext.Construct,
                    },

                    new ModActionDefinition
                    {
                        id = 2,
                        label = "Admin\\Elements\\repair element",
                        context = ModActionContext.Element,
                    },

                    new ModActionDefinition
                    {
                        id = 1338,
                        label = "Admin\\Dispensers\\Bypass Admin Dispenser",
                        context = ModActionContext.Element,
                    },
                    new ModActionDefinition
                    {
                        id = 1339,
                        label = "Admin\\Dispensers\\Bypass Reset",
                        context = ModActionContext.Element,
                    },
                    new ModActionDefinition
                    {
                        id = 1340,
                        label = "Admin\\Constructs\\take over construct",
                        context = ModActionContext.Construct,
                    },
                    new ModActionDefinition
                    {
                        id = 1341,
                        label = "Admin\\Constructs\\repair construct",
                        context = ModActionContext.Construct,
                    },
                    new ModActionDefinition
                    {
                        id = 1342,
                        label = "Admin\\Territory\\Claim Owned territory",
                        context = ModActionContext.Global,
                    },
                    new ModActionDefinition
                    {
                        id = 1343,
                        label = "Admin\\Territory\\Claim unOwned territory",
                        context = ModActionContext.Global,
                    },
                });
        return Task<ModInfo>.FromResult(res);
    }

    public async Task TriggerAction(ulong playerId, ModAction action)
    { // Called when a player clicks on one of you Mod's popup entries
        if (action.actionId == 201)
        { // Send a popup to player with the construct id
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.PopupReceived(new PopupMessage
            {
                message = $"Constructid: {action.constructId}",
                target = playerId,
            }));
        }
        if (action.actionId == 202)
        {
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.PopupReceived(new PopupMessage
            {
                message = $"ElementId: {action.elementId}",
                target = playerId,
            }));
        }
        if (action.actionId == 203)
        {
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.PopupReceived(new PopupMessage
            {
                message = $"PlayerId: {action.playerId}",
                target = playerId,
            }));
        }
        // Always recheck admin status as a bot could force-invoke an action
        // even if not received in your ModInfo
        if (!await orleans.GetPlayerGrain(playerId).IsAdmin())
            return;
        if (action.actionId == 1)
        { // disown construct
            await orleans.GetConstructGrain(action.constructId).ConstructSetOwner(0, new ConstructOwnerSet { ownerId = new EntityId() }, false);
        }
        else if (action.actionId == 2)
        { //repair element. Don't do that on destroyed core units!
            await orleans.GetConstructElementsGrain(action.constructId)
                .UpdateElementProperty(new ElementPropertyUpdate
                {
                    timePoint = TimePoint.Now(),
                    elementId = action.elementId,
                    constructId = action.constructId,
                    name = "hitpointsRatio",
                    value = new PropertyValue(1.0),
                });
        }
        else if (action.actionId == 1338)
        { //Bypass admin dispenser --> set tot true
            var cid = action.constructId;
            var eid = action.elementId;
            var right = await orleans.GetRDMSRightGrain(playerId).GetRightsForPlayerOnAsset(
                playerId,
                new AssetId
                {
                    type = AssetType.Element,
                    construct = cid,
                    element = eid,
                },
                true);
            if (!right.rights.Contains(Right.ElementEdit))
            {
                await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                    new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                    {
                        eventName = "modinjectjs",
                        eventPayload = "CPPHud.addFailureNotification(\"You do not have permissions on this element to configure teleporter\");",
                    }));
                return;
            }
            var key = "bypassPrimaryContainer";
            var value = true;
            await orleans.GetConstructElementsGrain(cid).UpdateElementProperty(
                new ElementPropertyUpdate
                {
                    constructId = cid,
                    elementId = eid,
                    name = key,
                    value = new PropertyValue(value),
                    timePoint = TimePoint.Now(),
                });
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                    new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                    {
                        eventName = "modinjectjs",
                        eventPayload = "CPPHud.addFailureNotification(\"Bypass added successful\");",
                    }));
        }
        else if (action.actionId == 1339)
        { //Bypass admin dispenser reset --> set to false
            var cid = action.constructId;
            var eid = action.elementId;
            var right = await orleans.GetRDMSRightGrain(playerId).GetRightsForPlayerOnAsset(
                playerId,
                new AssetId
                {
                    type = AssetType.Element,
                    construct = cid,
                    element = eid,
                },
                true);
            if (!right.rights.Contains(Right.ElementEdit))
            {
                await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                    new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                    {
                        eventName = "modinjectjs",
                        eventPayload = "CPPHud.addFailureNotification(\"You do not have permissions on this element to configure teleporter\");",
                    }));
                return;
            }
            var key = "bypassPrimaryContainer";
            var value = false;
            await orleans.GetConstructElementsGrain(cid).UpdateElementProperty(
                new ElementPropertyUpdate
                {
                    constructId = cid,
                    elementId = eid,
                    name = key,
                    value = new PropertyValue(value),
                    timePoint = TimePoint.Now(),
                });
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                    new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                    {
                        eventName = "modinjectjs",
                        eventPayload = "CPPHud.addFailureNotification(\"Bypass removed successful\");",
                    }));
        }

        else if (action.actionId == 1340)
        { //take over construct
            await orleans.GetConstructGrain(action.constructId).ConstructSetOwner(playerId, new ConstructOwnerSet { ownerId = new EntityId { playerId = playerId } }, false);
        }

        else if (action.actionId == 1341)
        { //repair construct. // DO NOT USE ON ABANDONED CORES
            await orleans.GetConstructElementsGrain(action.constructId)
                .RepairAllAdmin();
        }

        else if (action.actionId == 1342)
        { // claim an owned territory without searching for territory unit

            // Get The current Position from the player
            var playerPosition = await orleans.GetPlayerGrain(playerId).GetPositionUpdate();

            // Extract the local position from the player
            Vec3 playerLocalPos = playerPosition.localPosition.position;

            // get the current planet
            var planetInfo = await orleans.GetPlayerGrain(playerId).GetPlanet();

            // get the current planet information
            var constructInfo = await orleans.GetConstructInfoGrain(planetInfo.constructId).Get();


            // calculate the tile index using planetPos , radius and tilesize
            var tileIndex1 = await isp.GetRequiredService<IPlanetList>().GetTileIndex(planetInfo.constructId, playerLocalPos);

            // DEBUG: get total tiles calculated from radius and tilesize
            var numberOfTiles = NQutils.Core.Shared.Tile.NQGetNbTiles(constructInfo.planetProperties.altitudeReferenceRadius, constructInfo.planetProperties.territoryTileSize);

            // DEBUG: log all values to grain_dev.log
            logger.Info(tileIndex1.ToString() + ' ' + numberOfTiles + ' ' + playerLocalPos + ' ' + planetInfo.constructId + ' ' + constructInfo.planetProperties.altitudeReferenceRadius + ' ' + constructInfo.planetProperties.territoryTileSize);

            // Update the territory information and take ownership
            await orleans.GetPlanetTerritoryGrain(planetInfo.constructId)
                .UpdateTerritory(playerId, new TerritoryUpdate { tileIndex = ((uint)tileIndex1), ownerId = new EntityId { playerId = playerId } });
        }
        else if (action.actionId == 1343)
        { // claim an unowned territory without placing a territory unit 

            // Get The current Position from the player
            var playerPosition = await orleans.GetPlayerGrain(playerId).GetPositionUpdate();

            // Extract the local position from the player
            Vec3 playerLocalPos = playerPosition.localPosition.position;

            // get the current planet
            var planetInfo = await orleans.GetPlayerGrain(playerId).GetPlanet();

            // get the current planet information
            var constructInfo = await orleans.GetConstructInfoGrain(planetInfo.constructId).Get();

            // calculate the tile index using planetPos , radius and tilesize
            var tileIndex1 = await isp.GetRequiredService<IPlanetList>().GetTileIndex(planetInfo.constructId, playerLocalPos);

            // DEBUG: get total tiles calculated from radius and tilesize
            var numberOfTiles = NQutils.Core.Shared.Tile.NQGetNbTiles(constructInfo.planetProperties.altitudeReferenceRadius, constructInfo.planetProperties.territoryTileSize);

            // DEBUG: log all values to grain_dev.log
            logger.Info(tileIndex1.ToString() + ' ' + numberOfTiles + ' ' + playerLocalPos + ' ' + planetInfo.constructId + ' ' + constructInfo.planetProperties.altitudeReferenceRadius + ' ' + constructInfo.planetProperties.territoryTileSize);
            var itemId = planetInfo.constructId == 27 ? 3954055294 : planetInfo.constructId == 26 ? 3954055294 : 1358842892;
            var Entity = new EntityId { playerId = playerId };
            //  take ownership off an unclaimed tile in god mode 
            await orleans.GetPlanetTerritoryGrain(planetInfo.constructId)
                .ClaimTerritoryBySpecialOwner(playerId, new TerritoryClaim {item = new ItemId{ownerId = Entity , typeId = itemId }, planetId = planetInfo.constructId,position=playerLocalPos, ownerId = Entity , name = "My Unclaimed Terrotiry" });
        }
    }

    public ConcurrentDictionary<ulong, DateTime> notifs = new();
    public string GetObserverKey() => "ModAdmin"; // From ISubObserver, UID
    public async Task OnSubscriptionMessageReceived(PubSubTopic topic, AbstractPacket message)
    {
        var parser = new NQutils.Messages.Parser(message);
        string extra = "";
        if (parser.ElementsChanged(out var ec)) //of type ElementOperation
        {
            extra = $"element change";
        }
        logger.LogInformation($"PUBSUB {topic.Exchange} {message.MessageType}");
        var cid = ulong.Parse(topic.RoutingKey);
        var cn = (await orleans.GetConstructInfoGrain(cid).Get()).rData.name;
        // rate limit
        if (notifs.TryGetValue(cid, out var dt) && dt >= DateTime.Now.Subtract(TimeSpan.FromMinutes(1)))
            return;
        notifs[cid] = DateTime.Now;
        // spawn external process, whose implementation is left as an exercise to the reader
        var opts = new ProcessStartInfo
        {
            FileName = "/OrleansGrains/Mods/external-notification",
            Arguments = $"'Something is happening on {cn}: {extra} {message.MessageType}'",
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };
        try
        {
            using var proc = Process.Start(opts);

            await proc.WaitForExitAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Notification error");
        }
    }
}