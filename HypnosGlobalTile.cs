using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using HypnosMod.HypnosNPCs;
using CalamityMod.Items.Pets;
using CalamityMod.Tiles.DraedonSummoner;

namespace HypnosMod
{
	public class HypnosGlobalTile : GlobalTile
	{

		public override void RightClick(int i, int j, int type)
        {
			if (type == ModContent.TileType<CodebreakerTile>() && Main.LocalPlayer.HeldItem.type == ModContent.ItemType<BloodyVein>() && NPC.CountNPCS(ModContent.NPCType<Draedon>()) <= 0)
            {
				Terraria.Audio.SoundEngine.PlaySound(CalamityMod.UI.DraedonSummoning.CodebreakerUI.BloodSound, Main.LocalPlayer.Center);

				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					ModPacket packet = Mod.GetPacket();
					packet.Write((byte)HypnosMessageType.HypnosSummoned);
					packet.Write((byte)Main.myPlayer);
					packet.Send();
				}
				else
				{
					HypnosBoss.SummonDraedon(Main.LocalPlayer);
				}
				
            }
        }
	}
}