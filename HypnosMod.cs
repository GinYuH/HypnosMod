using Terraria.ModLoader;
using Terraria;
using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using HypnosMod.HypnosNPCs;
using Terraria.ID;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using CalamityMod;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Localization;
using HypnosMod.Items;

namespace HypnosMod
{
	enum HypnosMessageType
	{
		HypnosSummoned
	}

	public class HypnosMod : Mod
	{
        public static HypnosMod instance;

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			HypnosMessageType msgType = (HypnosMessageType)reader.ReadByte();
			switch (msgType)
			{
				case HypnosMessageType.HypnosSummoned:
					int player = reader.ReadByte();
					
					HypnosBoss.SummonDraedon(Main.player[player]);
					break;
			}
        }
        private static void RegisterSceneFilter(ScreenShaderData passReg, string registrationName, EffectPriority priority = EffectPriority.High)
        {
            string prefixedRegistrationName = "HypnosMod:" + registrationName;
            Filters.Scene[prefixedRegistrationName] = new Filter(passReg, priority);
            Filters.Scene[prefixedRegistrationName].Load();
        }

        private static void RegisterScreenShader(Effect shader, string passName, string registrationName, EffectPriority priority = EffectPriority.High)
        {
            Ref<Effect> shaderPointer = new(shader);
            ScreenShaderData passParamRegistration = new(shaderPointer, passName);
            RegisterSceneFilter(passParamRegistration, registrationName, priority);
        }

        internal static Effect ShieldShader;
        public override void Load()
        {
            instance = this;
            AssetRepository calAss = instance.Assets;
            Effect LoadShader(string path) => calAss.Request<Effect>("Effects/" + path, AssetRequestMode.ImmediateLoad).Value;
            ShieldShader = LoadShader("HoloShield");
            RegisterScreenShader(ShieldShader, "ShieldPass", "HoloShieldShader");
        }
		public override void PostSetupContent()
		{
			try
			{
				Mod cal = ModLoader.GetMod("CalamityMod");
				cal?.Call(
					"DeclareOneToManyRelationshipForHealthBar",
					ModContent.NPCType<HypnosNPCs.HypnosBoss>(),
					ModContent.NPCType<HypnosNPCs.AergiaNeuron>(),
					ModContent.NPCType<HypnosNPCs.HypnosPlug>()
				);
				var brEntries = (List<(int, int, Action<int>, int, bool, float, int[], int[])>)cal.Call("GetBossRushEntries");
				int[] excIDs = { ModContent.NPCType<AergiaNeuron>(), ModContent.NPCType<HypnosPlug>() };
				int[] headID = { ModContent.NPCType<HypnosBoss>() };
				Action<int> pr = delegate (int npc)
				{
					NPC.SpawnOnPlayer(CalamityMod.Events.BossRushEvent.ClosestPlayerToWorldCenter, ModContent.NPCType<HypnosBoss>());
				};
				brEntries.Insert(brEntries.Count() - 2, (ModContent.NPCType<HypnosBoss>(), -1, pr, 180, false, 0f, excIDs, headID));
				cal.Call("SetBossRushEntries", brEntries);
			}
			catch { /* ignore Calamity Boss Rush integration failures to not block other integrations */ }

			{
				Mod bossChecklist;
				ModLoader.TryGetMod("BossChecklist", out bossChecklist);
				if (bossChecklist != null)
				{
					// Follow Calamity's BossChecklist integration style: LogBoss with entryName and data dict
					string entryName = "Hypnos"; // unique key used by BossChecklist
					float order = 22.5f; // After Yharon (22.0), before Exo Mechs (22.99)
					var displayName = Language.GetText("Mods.HypnosMod.BossChecklist.Hypnos.EntryName");
					var spawnInfoLoc = Language.GetText("Mods.HypnosMod.BossChecklist.Hypnos.SpawnInfo");
					var despawnMessage = Language.GetText("Mods.HypnosMod.BossChecklist.Hypnos.Flavor");
					var collectibles = new List<int>
					{
						ModContent.ItemType<CalamityMod.Items.Materials.ExoPrism>(),
						ModContent.ItemType<HypnosTrophyInv>(),
						ModContent.ItemType<HypnosMask>()
					};
					// Resolve BloodyVein item id for icon in the spawn info panel.
					int spawnItemId = 0;
					if (ModLoader.TryGetMod("CalamityMod", out Mod calamityMod))
					{
						if (calamityMod.TryFind<ModItem>("BloodyVein", out var item))
							spawnItemId = item.Type;
					}

					var bcData = new Dictionary<string, object>
					{
						["displayName"] = displayName,
						["spawnInfo"] = spawnInfoLoc,
						["despawnMessage"] = despawnMessage,
						["collectibles"] = collectibles
					};
					if (spawnItemId > 0)
						bcData["spawnItems"] = spawnItemId;
					// Prefer modern API: LogBoss
					try
					{
						bossChecklist.Call("LogBoss", this, entryName, order, (Func<bool>)(() => HypnosWorld.downedHypnos), ModContent.NPCType<HypnosNPCs.HypnosBoss>(), bcData);
						Logger.Info("[HypnosMod] BossChecklist: registered via LogBoss");
					}
					catch
					{
						// Fallback to AddBoss if LogBoss is unavailable
						bossChecklist.Call("AddBoss", this, displayName.Value, order, (Func<bool>)(() => HypnosWorld.downedHypnos), new int[] { ModContent.NPCType<HypnosNPCs.HypnosBoss>() }, bcData);
						Logger.Info("[HypnosMod] BossChecklist: registered via AddBoss fallback");
					}
				}
			}
		}
        public override void Unload()
        {
            instance = null;
        }
    }
}