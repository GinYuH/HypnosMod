using Terraria.ModLoader;
using Terraria.ID;

namespace Hypnos.Items
{
	public class HypnosMusicBox : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
			MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(Mod, "Sounds/Music/HypnosSong"), ModContent.ItemType<HypnosMusicBox>(), ModContent.TileType<HypnosMusicBoxPlaced>());
		}

		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<HypnosMusicBoxPlaced>();
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Blue;
			Item.value = 100000;
			Item.accessory = true;
		}
	}
}