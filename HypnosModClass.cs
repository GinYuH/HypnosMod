using Terraria.ModLoader;
using Terraria;
using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Hypnos.HypnosNPCs;
using Terraria.ID;

namespace Hypnos
{
	public class HypnosModClass : Mod
	{
		public override void PostSetupContent()
		{

            Mod cal = ModLoader.GetMod("CalamityMod");
            cal.Call("DeclareOneToManyRelationshipForHealthBar", ModContent.NPCType<HypnosBoss>(), ModContent.NPCType<HypnosPlug>());
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
	}
}