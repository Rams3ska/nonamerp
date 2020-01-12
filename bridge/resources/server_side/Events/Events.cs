﻿using System;
using GTANetworkAPI;
using System.Threading.Tasks;
using server_side.Data;
using server_side.Utilities;
using server_side.Ints;

namespace Main.events
{
    class Events : Script
    {
        [ServerEvent(Event.PlayerEnterVehicle)]
        async public void Event_PlayerEnterVehicle(Client player, Vehicle vehicle, sbyte seatID)
        {
            await Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "createSpeedo");
            });
        }
        [ServerEvent(Event.PlayerExitVehicle)]
        async public void Event_PlayerExitVehicle(Client player, Vehicle vehicle)
        {
            await Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "destroySpeedo");
            });
        }
        [ServerEvent(Event.ResourceStart)]
        async public void Event_OnResourceStart()
        {
            Interiors.CreateInterior(new Vector3(343.0853f, -1399.852f, 32.5092), new Vector3(275.9121, -1361.429, 24.5378), 47.1863f, 51.81643f, 1, NAPI.Blip.CreateBlip(61, new Vector3(343.0853f, -1399.852f, 32.5092f), 1f, 0, "Hospital", 255, 200f));

            /*
            await Task.Run(() =>
            {
                DateTime time = DateTime.Now;
                NAPI.World.SetTime(time.Hour, time.Minute, time.Second); // set current time
            });
            */

            NAPI.Server.SetGlobalServerChat(false); // disable default global chat
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            UtilityFuncs.SetCurrentWeatherInLA(); // set current weather how in real LA 

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Server is loaded!\n");
            Console.ResetColor();
        }

        [ServerEvent(Event.PlayerConnected)]
        async public void Event_PlayerConnected(Client client)
        {
            await Task.Run(() => 
            {
                PlayerInfo player = new PlayerInfo(client);
                player.SetDataToDefault(); // reset player data

                client.Position = new Vector3(1789.294, -244.4794, 291.7196);
                client.Rotation.Z = 353.7821f;

                NAPI.Entity.SetEntityTransparency(client, 0);

                client.Dimension = Convert.ToUInt16("1234" + Convert.ToString(client.Value));

                NAPI.ClientEvent.TriggerClientEvent(client, "CreateAuthWindow");
            });
        }

        [ServerEvent(Event.ChatMessage)]
        public void Event_ChatMessage(Client client, string text)
        {
            NAPI.Pools.GetAllPlayers().ForEach(p =>
            {
                double dist = Math.Sqrt(Math.Pow((p.Position.X - client.Position.X), 2) + Math.Pow((p.Position.Y - client.Position.Y), 2) + Math.Pow((p.Position.Z - client.Position.Z), 2));
                if (dist <= 10) p.SendChatMessage("- " + text + $" ({client.Name})[{client.Value}]");
            });
        }

        
        [ServerEvent(Event.PlayerDeath)]
        async public void Event_PlayerDeath(Client client, Client killer, uint reason)
        {
            Random rand = new Random();
            Vector3[] respawnPositions =
            {
                new Vector3(258.9378f, -1340.669f, 24.53781f),
                new Vector3(250.3403f, -1351.37f, 24.53782f),
                new Vector3(251.3651f, -1347.497f, 24.53782f)
            };
            float[] rots = { 176.6944f, 250.9094f, 137.5093f };

            await Task.Delay(5000);

            int randResult = rand.Next(0, respawnPositions.Length);

            client.Position = respawnPositions[randResult];
            client.Rotation.Z = rots[randResult];

            client.Dimension = 1;
        }
    }
}