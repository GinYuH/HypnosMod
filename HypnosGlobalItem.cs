using CalamityMod.Items.TreasureBags;
using HypnosMod.Items;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace HypnosMod
{
    // Inject additional loot into specific boss bags without modifying CalRemix.
    public class HypnosGlobalItem : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            // Only act after Hypnos has been defeated in this world.
            if (!HypnosWorld.downedHypnos)
                return;

            // Draedon treasure bag (Exo Mechs) from Calamity.
            if (item.type == ModContent.ItemType<DraedonBag>())
            {
                // 25% each
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<AergianTechnistaff>(), chanceDenominator: 4));
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Neuraze>(), chanceDenominator: 4));
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Planetarium>(), chanceDenominator: 4));
            }

            // Note: Do NOT inject into CalamitasCoffer. Only DraedonBag should include these weapons.
        }
    }
}
