using Terraria.ModLoader;

namespace HypnosMod
{
    public class HypnosPlayer : ModPlayer
    {
        public bool neuron;
        public override void ResetEffects()
        {
            // Match CalRemix pattern: reset each tick, Buff will set true when core exists
            neuron = false;
        }
    }
}
