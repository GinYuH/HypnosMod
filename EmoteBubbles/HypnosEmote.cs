using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace HypnosMod.EmoteBubbles
{
	public class HypnosEmote : ModEmoteBubble
	{
		public override void SetStaticDefaults()
		{
			AddToCategory(EmoteID.Category.Dangers);
		}

		public override bool IsUnlocked()
		{
			return HypnosWorld.downedHypnos;
		}
	}
}