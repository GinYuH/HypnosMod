using System;
using Terraria;
using Terraria.ModLoader;

namespace HypnosMod
{
    public class HypnosBossMusic : ModSceneEffect
    {
    // To avoid tug-of-war with other scene effects/cutscenes that repeatedly fade music to 0,
    // we use hysteresis and stricter activation conditions to stabilize music switching.
        private static bool _latchedActive;
        private static int _stabilityCounter;

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/HypnosSong");

        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        private static bool ShouldPlay(Player player)
        {
            // Basic exclusions: don't play boss music on main menu or in graveyard biome
            if (Main.gameMenu)
                return false;
            if (player.ZoneGraveyard)
                return false;

            // Lock onto the current HypnosBoss via global index (kept updated by HypnosBoss.AI)
            int idx = global::HypnosMod.HypnosGlobalNPC.hypnos;
            if (idx < 0 || idx >= Main.maxNPCs)
                return false;

            NPC n = Main.npc[idx];
            if (!n.active || n.type != ModContent.NPCType<global::HypnosMod.HypnosNPCs.HypnosBoss>())
                return false;

            // During Draedon intro while Hypnos is assembling/intro (AI 0), avoid taking over the music
            // to prevent clashes with cutscene audio from other mods.
            bool draedonPresent = NPC.AnyNPCs(ModContent.NPCType<global::HypnosMod.HypnosNPCs.Draedon>());
            if (draedonPresent && n.ai[0] <= 0f)
                return false;

            // Start playing only when Hypnos has entered actual combat (AI >= 1)
            return n.ai[0] >= 1f;
        }

        public override bool IsSceneEffectActive(Player player)
        {
            bool desired = ShouldPlay(player);

            // Hysteresis: require the desired state to be stable for several frames before flipping
            // to avoid rapid toggling that repeatedly forces the music volume to 0.
            if (desired != _latchedActive)
            {
                _stabilityCounter++;
                if (_stabilityCounter >= 30) // ~0.5s stabilization window
                {
                    _latchedActive = desired;
                    _stabilityCounter = 0;
                }
            }
            else
            {
                _stabilityCounter = 0;
            }

            return _latchedActive;
        }
    }
}
