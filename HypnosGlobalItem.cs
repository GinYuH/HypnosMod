using CalamityMod.Items.TreasureBags;
using CalamityMod; // 引入 CalamityMod 命名空间以访问其 DropHelper
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
            // Draedon treasure bag (Exo Mechs) from Calamity.
            if (item.type == ModContent.ItemType<DraedonBag>())
            {
                // 25% each, only after Hypnos has been defeated (condition evaluated at drop time)
                itemLoot.Add(ItemDropRule.ByCondition(DropHelper.If(() => HypnosWorld.downedHypnos), ModContent.ItemType<AergianTechnistaff>(), 4));
                itemLoot.Add(ItemDropRule.ByCondition(DropHelper.If(() => HypnosWorld.downedHypnos), ModContent.ItemType<Neuraze>(), 4));
                itemLoot.Add(ItemDropRule.ByCondition(DropHelper.If(() => HypnosWorld.downedHypnos), ModContent.ItemType<Planetarium>(), 4));
                itemLoot.Add(ItemDropRule.ByCondition(DropHelper.If(() => HypnosWorld.downedHypnos), ModContent.ItemType<HypnosMask>(), 3));
            }
        }
    }
}
