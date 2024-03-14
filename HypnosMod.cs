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

		public override void Load()
        {
            instance = this;
}
		public override void PostSetupContent()
		{

			Mod cal = ModLoader.GetMod("CalamityMod");
			//cal.Call("DeclareOneToManyRelationshipForHealthBar", ModContent.NPCType<HypnosBoss>(), ModContent.NPCType<HypnosPlug>());
			List<(int, int, Action<int>, int, bool, float, int[], int[])> brEntries = (List<(int, int, Action<int>, int, bool, float, int[], int[])>)cal.Call("GetBossRushEntries");
			int[] excIDs = { ModContent.NPCType<AergiaNeuron>(), ModContent.NPCType<HypnosPlug>() };
			int[] headID = { ModContent.NPCType<HypnosBoss>() };
			Action<int> pr = delegate (int npc) 
			{
				NPC.SpawnOnPlayer(CalamityMod.Events.BossRushEvent.ClosestPlayerToWorldCenter, ModContent.NPCType<HypnosBoss>()); 
			};
			brEntries.Insert(brEntries.Count() - 2, (ModContent.NPCType<HypnosBoss>(), -1, pr, 180, false, 0f, excIDs, headID));
			cal.Call("SetBossRushEntries", brEntries);

			{
				Mod bossChecklist;
				ModLoader.TryGetMod("BossChecklist", out bossChecklist);
				if (bossChecklist != null)
				{
					bossChecklist.Call(new object[12]
				{
				"AddBoss",
				22.5f,
				ModContent.NPCType<HypnosNPCs.HypnosBoss>(),
				this,
				"XP-00 Hypnos",
				(Func<bool>)(() => HypnosWorld.downedHypnos),
				ModContent.ItemType<CalamityMod.Items.Pets.BloodyVein>(),
				null,
				new List<int>
				{
					ModLoader.GetMod("CalamityMod").Find<ModItem>("ExoPrism").Type
				},
				$"Jam a [i:{ModContent.ItemType<CalamityMod.Items.Pets.BloodyVein>()}] into the codebreaker",
				"An imperfection after allÅc what a shame.",
				null
				});
				}
			}
		}
        public override void Unload()
        {
            instance = null;
}
	}
}