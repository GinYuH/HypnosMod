using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Hypnos.HypnosNPCs;
using CalamityMod.Items.Pets;
using CalamityMod.Tiles.DraedonSummoner;

namespace Hypnos
{
	public class HypnosGlobalTile : GlobalTile
	{
		public override void RightClick(int i, int j, int type)
        {
			if (type == ModContent.TileType<CodebreakerTile>() && Main.LocalPlayer.HeldItem.type == ModContent.ItemType<BloodyVein>() && NPC.CountNPCS(ModContent.NPCType<Draedon>()) <= 0)
            {
				NPC.NewNPC(new Terraria.DataStructures.EntitySource_TileBreak(i, j), (int)Main.LocalPlayer.Center.X, (int)(Main.LocalPlayer.Center.Y - 1200), ModContent.NPCType<Draedon>());
                            Terraria.Audio.SoundEngine.PlaySound(CalamityMod.UI.DraedonSummoning.CodebreakerUI.BloodSound, Main.LocalPlayer.Center);
            }
        }
	}
}