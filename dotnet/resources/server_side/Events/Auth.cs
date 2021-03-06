﻿using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using server_side.DBConnection;
using System.Threading.Tasks;
using server_side.Data;
using System.Text.RegularExpressions;
using server_side.Items;
using MySql.Data.MySqlClient.Memcached;
using server_side.DataBase;
using System.Data.OleDb;
using server_side.DataBase.Models;
using System.Linq;
using System.Security.Permissions;
using Newtonsoft.Json.Linq;

namespace server_side.Events
{
    class Auth : Script
    {
        [RemoteEvent("Event_CancelAuth")] // событие при нажатии "отмена" во время авторизации\регистрации
        public void Event_CancelAuth(Player client, object[] args)
        {
            Utils.UtilityFuncs.SendPlayerNotify(client, 0, "You been kicked for cancel auth/reg.");
            client.Kick();
        }

        [RemoteEvent("Event_CheckDataOnValid")]
        public void Event_CheckDataOnValid(Player player, int type, string login, string password, string email)
        {
            if (CheckLoginOnValid(login) != string.Empty)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "authSendError", CheckLoginOnValid(login));
                return;
            }
            else if (CheckPasswordOnValid(password) != string.Empty)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "authSendError", CheckPasswordOnValid(password));
                return;
            }
            else if (CheckMailOnValid(email) != string.Empty && type == 1)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "authSendError", CheckMailOnValid(email));
                return;
            }

            GetAccountFromBD(player, type, login, password, email);
        }

        private string CheckLoginOnValid(string login)
        {
            if (login.Length < 2 || login.Length > 24)
                return "Ошибка: Логин не может быть короче 2-х и длиннее 24-х символов!";
            else if (new Regex(@"[^A-Za-z0-9_]").IsMatch(login))
                return "Ошибка: Логин содержит запрещенные символы!";
            return string.Empty;
        }
        private string CheckPasswordOnValid(string password)
        {
            if (password.Length < 6)
                return "Ошибка: Пароль не может быть короче 6 - ти символов!";
            return string.Empty;
        }
        private string CheckMailOnValid(string email)
        {
            if (!new Regex(@"^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$").IsMatch(email))
                return "Ошибка: Некорректный email!";
            return string.Empty;
        }

        public void GetAccountFromBD(Player player, int type, string login, string password, string email)
        {
            using (DataBase.AppContext db = new DataBase.AppContext())
            {
                var authData = db.Accounts.Where(x => x.Login == login).FirstOrDefault();

                switch (type)
                {
                    case 0: // auth
                        {
                            if (authData == null)
                            {
                                NAPI.ClientEvent.TriggerClientEvent(player, "authSendError", "Ошибка: Данный аккаунт не зарегистрирован!");
                                break;
                            }
                            else if (authData.Password != password)
                            {
                                NAPI.ClientEvent.TriggerClientEvent(player, "authSendError", "Ошибка: Неверный пароль!");
                                break;
                            }

                            LogInPlayerAccount(player, authData);
                            break;
                        }
                    case 1: // reg
                        {
                            if (authData != null)
                            {
                                NAPI.ClientEvent.TriggerClientEvent(player, "authSendError", "Ошибка: Данный аккаунт уже зарегистрирован!");
                                break;
                            }

                            player.SetData<string>("R_TempLogin", login);
                            player.SetData<string>("R_TempPassword", password);
                            player.SetData<string>("R_TempMail", email);

                            NAPI.ClientEvent.TriggerClientEvent(player, "destroyAuthBrowser");

                            JumpToCustomizeStep(player);
                            break;
                        }
                }

            }
            /*
            try
            {
                MySqlConnection con = new MySqlConnector().GetDBConnection();
                con.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `accounts` WHERE `p_login` = '" + args[1] + "'", con);
                MySqlDataReader read = cmd.ExecuteReader();


                switch (args[0])
                {
                    case 0: // auth
                        {
                            AuthData auth = new AuthData();

                            if (!read.HasRows)
                            {
                                NAPI.ClientEvent.TriggerClientEvent(client, "authSendError", "Ошибка: Данный аккаунт не зарегистрирован!");
                                read.Close();
                                break;
                            }

                            while (read.Read())
                            {
                                auth.dbID = (int)read["p_id"];
                                auth.Login = (string)read["p_login"];
                                auth.Password = (string)read["p_password"];
                                auth.IP = (string)read["p_ip"];
                                auth.Mail = (string)read["p_mail"];
                                auth.LVL = (int)read["p_lvl"];
                                auth.Cash = (double)read["p_money"];
                                auth.BankMoney = (double)read["p_bank"];
                                auth.Age = (int)read["p_age"];
                                auth.Name = (string)read["p_name"];
                                auth.SocialName = (string)read["p_socialclub"];
                                auth.DateReg = (string)read["p_datereg"];
                                auth.PayCheck = (double)read["p_paycheck"];
                                auth.Satiety = (int)read["p_satiety"];
                                auth.Thirst = (int)read["p_thirst"];

                                client.SetData<object>("pCustomize", read["p_customize"]);
                                client.SetData<object>("pClothes", read["p_clothes"]);
                            }
                            read.Close();

                            if (auth.Password != args[2].ToString())
                            {
                                NAPI.ClientEvent.TriggerClientEvent(client, "authSendError", "Ошибка: Неверный пароль!");
                                break;
                            }

                            LogInPlayerAccount(client, auth);

                            break;
                        }
                    case 1: // reg
                        {
                            if (read.HasRows)
                            {
                                NAPI.ClientEvent.TriggerClientEvent(client, "authSendError", "Ошибка: Данный аккаунт уже зарегистрирован!");
                                read.Close();
                                break;
                            }
                            read.Close();

                            client.SetData<string>("R_TempLogin", (string)args[1]);
                            client.SetData<string>("R_TempPassword", (string)args[2]);
                            client.SetData<string>("R_TempMail", (string)args[3]);

                            NAPI.ClientEvent.TriggerClientEvent(client, "destroyAuthBrowser");

                            JumpToCustomizeStep(client);

                            break;
                        }
                }
                con.Close();
                
            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput($"[MySQL Error (#{++exceptionCount})]: " + e.Message);
                NAPI.ClientEvent.TriggerClientEvent(client, "authSendError", $"Ошибка: MySQL Exception! Обратитесь к администрации сервера. (#{exceptionCount})");
            }
            */
        }

        public void JumpToCustomizeStep(Player client)
        {
            NAPI.ClientEvent.TriggerClientEvent(client, "enableCustomize");

            NAPI.Task.Run(() =>
            {
                client.Position = new Vector3(402.8664, -996.4108, -99.00027);
                client.Rotation = new Vector3(0, 0, -185.0000);

                NAPI.Entity.SetEntityTransparency(client, 255);

                client.Dimension = (uint)client.Value;
            }, 2000);
        }

        [RemoteEvent("EndPlayerCustomize")]
        public void EndPlayerCustomize(Player player, string basedata, string customize, string clothes)
        {
            using (DataBase.AppContext db = new DataBase.AppContext())
            {
                dynamic based = NAPI.Util.FromJson(basedata);
                dynamic cust = NAPI.Util.FromJson(customize);

                PlayerModel md = new PlayerModel();

                md.Name = $"{based["name"]} {based["subname"]}";
                md.SClubName = player.SocialClubName;
                md.SClubId = player.SocialClubId;
                md.Serial = player.Serial;
                md.Login = player.GetData<string>("R_TempLogin");
                md.Password = player.GetData<string>("R_TempPassword");
                md.Mail = player.GetData<string>("R_TempMail");
                md.RegIP = player.Address;
                md.DateReg = DateTime.Now.ToString();
                md.Age = (int)based["old"];
                md.Sex = (bool)cust["sex"];
                md.Customize = customize;
                md.Clothes = clothes;

                db.Accounts.Add(md);
                db.SaveChanges();

                NAPI.ClientEvent.TriggerClientEvent(player, "disableCustomize");
                LogInPlayerAccount(player, md);
            }
            /*
            try
            {
                using(MySqlConnection con = new MySqlConnector().GetDBConnection())
                {
                    con.Open();

                    dynamic based = NAPI.Util.FromJson(basedata);
                    dynamic cust = NAPI.Util.FromJson(customize);

                    string name = $"{based["name"]} {based["subname"]}";
                    string sc = client.SocialClubName;

                    string currentTime = DateTime.Now.ToString();

                    if (sc == null)
                        sc = "-";

                    string query = "INSERT INTO `accounts` (`p_login`, `p_socialclub`, `p_password`, `p_ip`, `p_mail`, `p_name`, `p_age`, `p_sex`, `p_customize`, `p_clothes`, `p_datereg`) 
                        VALUES ('" + client.GetData<string>("R_TempLogin") + "', '" + sc + "', '" + client.GetData<string>("R_TempPassword") + "', '" + client.Address + "', '" + client.GetData<string>("R_TempMail") + "', '" + name + "', '" + based["old"] + "', '" + (int)cust["sex"] + "', '" + customize + "', '" + clothes + "', '" + currentTime + "')";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.ExecuteNonQuery();

                    cmd = new MySqlCommand("SELECT * FROM `accounts` WHERE `p_id` = LAST_INSERT_ID()", con);
                    MySqlDataReader read = cmd.ExecuteReader();

                    AuthData auth = new AuthData();

                    while (read.Read())
                    {
                        auth.dbID = (int)read["p_id"];
                        auth.Login = (string)read["p_login"];
                        auth.Password = (string)read["p_password"];
                        auth.IP = (string)read["p_ip"];
                        auth.Mail = (string)read["p_mail"];
                        auth.LVL = (int)read["p_lvl"];
                        auth.Cash = (double)read["p_money"];
                        auth.BankMoney = (double)read["p_bank"];
                        auth.Age = (int)read["p_age"];
                        auth.Name = (string)read["p_name"];
                        auth.SocialName = (string)read["p_socialclub"];
                        auth.DateReg = (string)read["p_datereg"];
                        auth.PayCheck = (double)read["p_paycheck"];
                        auth.Satiety = (int)read["p_satiety"];
                        auth.Thirst = (int)read["p_thirst"];

                        client.SetData<object>("pCustomize", read["p_customize"]);
                        client.SetData<object>("pClothes", read["p_clothes"]);
                    }

                    NAPI.ClientEvent.TriggerClientEvent(client, "disableCustomize");
                    LogInPlayerAccount(client, auth);

                    read.Close();
                    con.Close();
                }
            }
            catch (Exception e) { NAPI.Util.ConsoleOutput(e.ToString()); }
            */
        }

        public void LogInPlayerAccount(Player client, PlayerModel data)
        {
            try
            {
                PlayerInfo playerInfo = new PlayerInfo(client);

                playerInfo.SetAuthorized(true);
                playerInfo.SetDbID(data.Id);
                playerInfo.SetLVL(data.LVL);
                playerInfo.SetName(data.Name);
                playerInfo.SetLogin(data.Login);
                playerInfo.SetMail(data.Mail);
                playerInfo.SetPassword(data.Password);
                playerInfo.SetRegIP(data.RegIP);
                playerInfo.SetCustomize(Convert.ToString(data.Customize));
                playerInfo.SetClothes(Convert.ToString(data.Clothes));
                playerInfo.GiveMoney(data.Cash, updateindb: false);
                playerInfo.GiveBankMoney(data.Bank, updateindb: false);
                playerInfo.SetAge(data.Age);
                playerInfo.SetSocialClub(data.SClubName);
                playerInfo.SetDateReg(data.DateReg);
                playerInfo.SetLastJoin(DateTime.Now.ToString());
                playerInfo.SetCurrentIP(client.Address);
                playerInfo.InitPayCheck(data.PayCheck);
                playerInfo.SetSatiety(data.Satiety);
                playerInfo.SetThirst(data.Thirst);

                client.ResetData("R_TempLogin");
                client.ResetData("R_TempPassword");
                client.ResetData("R_TempMail");

                client.Name = data.Name;

                client.Position = new Vector3(-143.7677, 6438.123, 31.4298);
                client.Rotation = new Vector3(client.Rotation.X, client.Rotation.Y, -49.8411f);
                client.Dimension = 0;

                new ItemController().LoadPlayerItemsFromDB(client, data.Id);

                new InventorySystem.Inventory(client).Init();

                ////

                NAPI.Entity.SetEntityTransparency(client, 255);


                if (playerInfo.GetCustomize() != null)
                    NAPI.ClientEvent.TriggerClientEvent(client, "setPlayerCustomize", playerInfo.GetCustomize());

                if (playerInfo.GetClothes() != null)
                    NAPI.ClientEvent.TriggerClientEvent(client, "setPlayerClothes", playerInfo.GetClothes());

                NAPI.ClientEvent.TriggerClientEvent(client, "createHud", 50, 60, playerInfo.GetMoney(), playerInfo.GetBankMoney());

                NAPI.ClientEvent.TriggerClientEvent(client, "destroyAuthBrowser");

                Utils.UtilityFuncs.SendPlayerNotify(client, 2, "Вы успешно авторизировались!");
            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput(e.ToString());
            }
        }
    }
}
