﻿using System;
using GTANetworkAPI;
using System.Threading.Tasks;
using server_side.Data;
using server_side.Ints;
using server_side.Systems;
using server_side.Utilities;
using server_side.Jobs;
using System.Linq;

namespace Main.events
{
    class Events : Script
    {
        [ServerEvent(Event.PlayerEnterVehicle)]
        public void Event_PlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatID)
        {
            // NAPI.ClientEvent.TriggerClientEvent(player, "createSpeedo");
        }
        [ServerEvent(Event.PlayerExitVehicle)]
        async public void Event_PlayerExitVehicle(Player player, Vehicle vehicle)
        {
            // NAPI.ClientEvent.TriggerClientEvent(player, "destroySpeedo");
        }
        [ServerEvent(Event.ResourceStart)]
        public void Event_OnResourceStart()
        {
            // ints
            Interiors.CreateInterior(new Vector3(1839.098f, 3673.332f, 34.2767f), new Vector3(275.9121, -1361.429, 24.5378), 211.3162f, 51.81643f, 1, NAPI.Blip.CreateBlip(61, new Vector3(343.0853f, -1399.852f, 32.5092f), 1f, 0, name: "Hospital", drawDistance: 15.0f, shortRange: true, dimension: 0));
            
            // houses
            House.InitHouses();

            // jobs
            Job.InitJobs();
            
            /*
            DateTime time = DateTime.Now;
            NAPI.World.SetTime(time.Hour, time.Minute, time.Second); // set current time
            */

            NAPI.Server.SetGlobalServerChat(true); // disable default global chat
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            RealWeather.SetCurrentWeatherInLA(); // set current weather how in real LA 
            NAPI.Server.SetCommandErrorMessage("~r~[Ошибка]: ~w~Данной команды не существует!");
            NAPI.Server.SetDefaultSpawnLocation(new Vector3(1789.294, -244.4794, 291.7196), 353.7821f);
            //
            NAPI.Server.SetLogCommandParamParserExceptions(true);
            NAPI.Server.SetLogRemoteEventParamParserExceptions(true);

            Console.ForegroundColor = ConsoleColor.Green;
            NAPI.Util.ConsoleOutput("\nServer is loaded!\n");
            Console.ResetColor();
        }

        [ServerEvent(Event.PlayerConnected)]
        async public void Event_PlayerConnected(Player client)
        {
            PlayerInfo playerInfo = new PlayerInfo(client);

            playerInfo.SetDataToDefault(); // reset player data
            playerInfo.SetSocialClub(client.SocialClubName);

            await Task.Run(() =>
            {
                client.Position = new Vector3(1789.294, -244.4794, 291.7196);
                client.Rotation = new Vector3(client.Rotation.X, client.Rotation.Y, 353.7821f);
            });
            

            NAPI.Entity.SetEntityTransparency(client, 0);

            client.Dimension = Convert.ToUInt16("1234" + Convert.ToString(client.Value));

            NAPI.ClientEvent.TriggerClientEvent(client, "CreateAuthWindow");

            NAPI.Util.ConsoleOutput($"Connected: {NAPI.Player.GetPlayerAddress(client)} | ID: {client.Value}");

            //
            NAPI.ClientEvent.TriggerClientEvent(client, "createDebugUI");
        }
        /*
        [ServerEvent(Event.ChatMessage)]
        public void Event_ChatMessage(Player client, string text)
        {
            
            NAPI.Pools.GetAllPlayers().ForEach(p =>
            {
                double dist = Math.Sqrt(Math.Pow(p.Position.X - client.Position.X, 2) + Math.Pow(p.Position.Y - client.Position.Y, 2) + Math.Pow(p.Position.Z - client.Position.Z, 2));
                if (dist <= 10) p.SendChatMessage("- " + text + $" ({client.Name})[{client.Value}]");
            });
            
            NAPI.Pools.GetAllPlayers().ForEach(p =>
            {
                p.SendChatMessage("- " + text + $" ({client.Name})[{client.Value}]");
            });
        }
        */

        [ServerEvent(Event.PlayerDeath)]
        async public void Event_PlayerDeath(Player client, Player killer, uint reason)
        {
            await Task.Run(() =>
            {
                Random rand = new Random();
                Vector3[] respawnPositions =
                {
                    new Vector3(258.9378f, -1340.669f, 24.53781f),
                    new Vector3(250.3403f, -1351.37f, 24.53782f),
                    new Vector3(251.3651f, -1347.497f, 24.53782f)
                };
                float[] rots = { 176.6944f, 250.9094f, 137.5093f };

                Task.Delay(5000);

                int randVal = rand.Next(0, respawnPositions.Length);
                NAPI.Player.SpawnPlayer(client, respawnPositions[randVal], rots[randVal]);
                client.Dimension = 1;
            });
        }
        [ServerEvent(Event.PlayerEnterColshape)]
        public void Event_PlayerEnterColshape(ColShape colshape, Player client)
        {
            if (client.GetData<int>(EntityData.PLAYER_PICKUPKD) != 0) return;
            client.SetData<int>(EntityData.PLAYER_PICKUPKD, client.GetData<int>(EntityData.PLAYER_PICKUPKD) + 5);

            // there another "EnterColShape" events:
            Interiors.OnPlayerEnterConshape(colshape, client);
            House.OnPlayerEnterColshape(colshape, client);
            Job.OnEnterJobPickUp(colshape, client);
        }

        [ServerEvent(Event.Update)]
        public void Event_Update()
        {
            NAPI.Pools.GetAllPlayers().ForEach((p) =>
            {
                if(p.GetData<bool>("temp_gm") == true)
                {
                    p.Health = 100;
                }
            });
        }
    }
}
