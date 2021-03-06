﻿using System;
using System.Linq;
using GTANetworkAPI;
using server_side.Data;
using server_side.Items;
using System.IO;
using Newtonsoft.Json;

namespace server_side.InventorySystem
{

    class Inventory : ItemsStorage
    {
        private Player _player = null;
        private PlayerInfo player = null;

        private Inventory() { }
        public Inventory(Player player)
        {
            this._player = player;
            this.player = new PlayerInfo(player);
        }


        public void Init()
        { 
            NAPI.ClientEvent.TriggerClientEvent(_player, "InventoryLoad", player.GetName(), player.GetMoney(), player.GetBankMoney(), _player.Health, player.GetSatiety(), player.GetThirst(), JsonConvert.SerializeObject(ItemController.ItemsList.Where(x => x.OwnerID == player.GetDbID())), File.ReadAllText(@"dotnet/itemData.json"));

            NAPI.Util.ConsoleOutput("[Inventory]: " + player.GetName() + " - инвентарь инициализирован");
        }

        public void Update()
        {

        }

        public void UpdateItem(object item)
        {

        }

        [RemoteEvent("sUseItem")]
        public void UseItem(Player player, int id)
        {
            ItemEntity item = ItemController.ItemsList.Where(x => x.ItemID == id).FirstOrDefault();

            if (item == null)
            {
                Utils.UtilityFuncs.SendPlayerNotify(player, 0, "Ошибка! Данного предмета не существует на сервере!");
                return;
            }

            item = new ItemController().UseItem(player, item);

            NAPI.Util.ConsoleOutput($"in sUseItem > id: {item.ItemID}, amount: {item.ItemAmount}");

            NAPI.ClientEvent.TriggerClientEvent(player, "InventoryUpdateItem", item.ItemID, item.ItemAmount);
        }

        public void GiveItem(ItemEntity item)
        {
            NAPI.ClientEvent.TriggerClientEvent(_player, "InventoryAddItem", JsonConvert.SerializeObject(item));
        }

        public void UpdateBar()
        {
            NAPI.ClientEvent.TriggerClientEvent(_player, "InventoryUpdateBar", player.GetName(), player.GetMoney(), player.GetBankMoney(), _player.Health, player.GetSatiety(), player.GetThirst());
        }
    }
}
