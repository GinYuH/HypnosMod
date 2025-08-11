using Terraria;
using Terraria.ModLoader;
using HypnosMod.Projectiles;
using HypnosMod;

namespace HypnosMod.Buffs
{
    public class AergiaNeuronBuff : ModBuff
    {
    public override string Texture => "HypnosMod/Sprites/AergiaNeuron";
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Terraria.Player player, ref int buffIndex)
        {
            // If the player owns any neuron core, set the flag on this tick
            if (player.ownedProjectileCounts[ModContent.ProjectileType<AergiaNeuronCore>()] > 0)
                player.GetModPlayer<HypnosPlayer>().neuron = true;

            // If flag not set, remove the buff; else keep it alive
            if (!player.GetModPlayer<HypnosPlayer>().neuron)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
            else
            {
                player.buffTime[buffIndex] = 18000;
            }
        }
    }
}
