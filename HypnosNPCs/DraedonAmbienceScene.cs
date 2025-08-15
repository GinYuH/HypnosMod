using Terraria;
using Terraria.ModLoader;
using HypnosMod.HypnosNPCs;
using CalamityMod.Systems;

namespace HypnosMod.Systems
{
    // Plays Calamity's Draedon ambience during our Draedon conversation (before Hypnos music takes over)
    public class DraedonAmbienceScene : BaseMusicSceneEffect
    {
        // Keep priority lower than typical boss themes so Hypnos music overrides seamlessly
        public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;

        // Tie the scene to our Draedon NPC
        public override int NPCType => ModContent.NPCType<Draedon>();

        // Use Calamity's ambience track
        public override int? MusicModMusic
        {
            get
            {
                if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                    return MusicLoader.GetMusicSlot(calamity, "Sounds/Music/DraedonExoSelect");
                return null;
            }
        }

        public override int VanillaMusic => -1;
        public override int OtherworldMusic => -1;

        // Only play during our Draedon dialogue phase (ai[0] == 0)
        public override bool AdditionalCheck()
        {
            if (!ModLoader.TryGetMod("CalamityMod", out _))
                return false;

            int draedonType = ModContent.NPCType<Draedon>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var n = Main.npc[i];
                if (n.active && n.type == draedonType && n.ai[0] == 0)
                    return true;
            }
            return false;
        }
    }
}
