﻿using DumpsterDiving.Properties;
using GTA;
using GTA.Math;
using GTA.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DumpsterDiving
{
    public class DumpsterDiving : Script
    {
        /// <summary>
        /// The items that the player can get in the dumpsters.
        /// </summary>
        private enum Items
        {
            Hotdog = 0,
            Hamburger = 1,
            MoldyHotDog = 2,
            MoldyHamburger = 3,
            Money = 4,
            Dildo = 5,
            Boot = 6,
            Fish = 7,
            Condom = 8,
            Pistol = 9,
            MicroSMG = 10,
            AssaultRifle = 11,
            Shotgun = 12,
            SawnOffShotgun = 13,
            Grenades = 14,
            BZ = 15
        }
        /// <summary>
        /// A list that contains models of dumpsters.
        /// </summary>
        public static List<Model> Models = new List<Model>
        {
            new Model("prop_dumpster_01a"),
            new Model("prop_dumpster_02a"),
            new Model("prop_dumpster_02b"),
            new Model("prop_dumpster_04a"),
            new Model("prop_dumpster_4b"),
            new Model("prop_dumpster_3a")
        };
        /// <summary>
        /// The configuration for our current script.
        /// </summary>
        public static ScriptSettings ScriptConfig = ScriptSettings.Load("scripts\\DumpsterDiving.ini");
        public static Configuration Config = JsonConvert.DeserializeObject<Configuration>("scripts\\DumpsterDiving.json");
        /// <summary>
        /// Proximity between the player and the dumpster to show a Blip.
        /// </summary>
        public static float Proximity = ScriptConfig.GetValue("CWDD", "Proximity", 25f);
        /// <summary>
        /// The dumpsters that exist arround the map.
        /// </summary>
        private static List<Prop> Dumpsters = new List<Prop>();
        /// <summary>
        /// Next game time that we should update the lists of peds.
        /// </summary>
        private static int NextFetch = 0;

        public DumpsterDiving()
        {
            // Add our events
            Tick += OnTick;
            KeyDown += OnKeyDown;

            // Just an example message
            // TODO: Print the version and type of build
            UI.Notify(Strings.Loaded);
        }

        private void OnTick(object sender, EventArgs e)
        {
            // Return if the player is in a vehicle
            if (Game.Player.Character.CurrentVehicle != null)
            {
                return;
            }

            // If the current time is higher or equal than the next fetch time
            if (Game.GameTime >= NextFetch)
            {
                // Reset the list of dumpsters
                Dumpsters = new List<Prop>();
                // Iterate over the dumpster models
                foreach (Model DumpsterModel in Models)
                {
                    // Fill the list with all of those props
                    Dumpsters.AddRange(World.GetAllProps(DumpsterModel));
                }
                // Finally, set the next fetch time to one second in the future
                NextFetch = Game.GameTime + 1000;
            }

            // Iterate over the stored dumpsters
            foreach (Prop DumpsterProp in Dumpsters)
            {
                // If the user wants blips and the dumpster doesn't have one
                if (Config.Blips && DumpsterProp.CurrentBlip == null)
                {
                    // Create the blip
                    DumpsterProp.AddBlip();
                    DumpsterProp.CurrentBlip.Name = "Dumpster";
                    DumpsterProp.CurrentBlip.Color = BlipColor.Purple;
                }

                // Get the position of the front
                Vector3 Front = DumpsterProp.GetOffsetInWorldCoords(new Vector3(0, -1f, 0));

                // If the distance is lower or equal to 25 units
                if (World.GetDistance(Game.Player.Character.Position, DumpsterProp.Position) <= 25)
                {
                    // Draw a marker that will trigger the dumpster diving
                    World.DrawMarker(MarkerType.VerticalCylinder, Front, Vector3.Zero, Vector3.Zero, new Vector3(), Color.Purple);
                }

                // If the distance between the front and the player is lower or equal to 1.5
                if (World.GetDistance(Game.Player.Character.Position, Front) <= 1.5f)
                {
                    // Notify the user
                    UI.ShowSubtitle("Press [PLACEHOLDER] to loot the dumpster.");
                }
            }
        }

        private void OnKeyDown(object Sender, KeyEventArgs Args)
        {
            // If the player preses E and is a dumpster available, "loot it"
            if (Args.KeyCode == ScriptConfig.GetValue("CWDD", "KeyInteract", Keys.None) && true)
            {
                Game.FadeScreenOut(1000);
                Game.Player.Character.FreezePosition = true;
                Wait(1000);
                SearchDumpster();
                Wait(1000);
                Game.Player.Character.FreezePosition = false;
                Game.FadeScreenIn(1000);
            }
            // In the case of pressing Page Down
            if (Args.KeyCode == ScriptConfig.GetValue("CWDD", "KeyBlipRemoval", Keys.None))
            {
                // Iterate over the map blips
                foreach (Blip CurrentBlip in World.GetActiveBlips())
                {
                    // If is a D blip
                    if (CurrentBlip.Sprite == BlipSprite.Devin)
                    {
                        // Remove it
                        CurrentBlip.Remove();
                    }
                }
            }
        }

        private void SearchDumpster()
        {
            // Temporary variables to know if the player has found a weapon
            string Text = string.Empty;

            // Get a random item from the enum at the top
            Random Generator = new Random();
            int Number = Generator.Next(0, Enum.GetValues(typeof(Items)).Length);
            Items Item = (Items)Number;

            // See what the user got
            switch (Item)
            {
                case Items.Hotdog:
                    Text = Strings.Hotdog;
                    Heal();
                    break;
                case Items.Hamburger:
                    Text = Strings.Hamburger;
                    Heal();
                    break;
                case Items.MoldyHotDog:
                    Text = Strings.MoldyHotdog;
                    break;
                case Items.MoldyHamburger:
                    Text = Strings.MoldyHamburger;
                    break;
                case Items.Money:
                    int Money = Generator.Next(10, 100);
                    Text = string.Format(Strings.Hamburger, Money);
                    Game.Player.Money += Money;
                    break;
                case Items.Dildo:
                    Text = Strings.Dildo;
                    break;
                case Items.Boot:
                    Text = Strings.Boot;
                    break;
                case Items.Fish:
                    Text = Strings.Fish;
                    break;
                case Items.Condom:
                    Text = Strings.Condom;
                    break;
                case Items.Pistol:
                    Text = Strings.GunPistol;
                    Weapon(WeaponHash.Pistol);
                    break;
                case Items.MicroSMG:
                    Text = Strings.GunMicro;
                    Weapon(WeaponHash.MicroSMG);
                    break;
                case Items.AssaultRifle:
                    Text = Strings.GunAssaultRifle;
                    Weapon(WeaponHash.AssaultRifle);
                    break;
                case Items.Shotgun:
                    Text = Strings.GunShotgun;
                    Weapon(WeaponHash.PumpShotgun);
                    break;
                case Items.SawnOffShotgun:
                    Text = Strings.GunSawnOff;
                    Weapon(WeaponHash.SawnOffShotgun);
                    break;
                case Items.Grenades:
                    Text = Strings.GunGrenades;
                    Weapon(WeaponHash.Grenade);
                    break;
                case Items.BZ:
                    Text = Strings.GunBZ;
                    Weapon(WeaponHash.BZGas);
                    break;
            }

            // Notify the user about what has been found on the dumpster
            UI.Notify(string.Format(Strings.Found, Text));
        }

        private static void Heal()
        {
            int MaxHealth = Function.Call<int>(Hash.GET_ENTITY_MAX_HEALTH, Game.Player.Character);
            Function.Call(Hash.SET_ENTITY_HEALTH, Game.Player.Character, MaxHealth);
        }

        private static void Weapon(WeaponHash Weapon)
        {
            // If the player does not have the weapon, give one to him
            if (!Game.Player.Character.Weapons.HasWeapon(Weapon))
            {
                Game.Player.Character.Weapons.Give(Weapon, 0, true, true);
            }

            // Then, select the weapon and give 2 magazines
            Game.Player.Character.Weapons.Select(Weapon);
            Game.Player.Character.Weapons.Current.Ammo += (Game.Player.Character.Weapons.Current.MaxAmmoInClip * 2);
        }
    }
}
