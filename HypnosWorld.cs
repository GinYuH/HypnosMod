using Microsoft.Xna.Framework;
//using StructureHelper;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace Hypnos
{
    public class HypnosWorld : ModSystem
    {
		public static bool downedHypnos = false;

		
		public static void UpdateWorldBool()
		{
			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.WorldData);
			}
		}
		public override void OnWorldLoad()
		{
			downedHypnos = false;
		}
		public override void OnWorldUnload()
		{
			downedHypnos = false;
		}
		public override void SaveWorldData(TagCompound tag)
		{
            tag["downedHypnos"] = downedHypnos;
		}

		public override void LoadWorldData(TagCompound tag)
		{
            downedHypnos = tag.Get<bool>("downedHypnos");
		}
		public override void NetSend(BinaryWriter writer)
		{
            writer.Write(downedHypnos);
		}
		public override void NetReceive(BinaryReader reader)
		{
            downedHypnos = reader.ReadBoolean();
		}
	}
}