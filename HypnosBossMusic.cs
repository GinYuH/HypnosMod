using System;
using Terraria;
using Terraria.ModLoader;

namespace HypnosMod
{
    public class HypnosBossMusic : ModSceneEffect
    {
        private static ulong _lastToggleTick;
        private static bool _latchedActive;
    private const ulong CooldownTicks = 120;

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/HypnosSong");

        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override bool IsSceneEffectActive(Player player)
        {
            if (Main.gameMenu)
                return false;
            if (player.ZoneGraveyard)
                return false;
            bool desired = NPC.AnyNPCs(ModContent.NPCType<global::HypnosMod.HypnosNPCs.HypnosBoss>());

            ulong now = Main.GameUpdateCount;
            if (_lastToggleTick == 0UL)
            {
                _latchedActive = desired;
                _lastToggleTick = now;
                return _latchedActive;
            }

            if (desired != _latchedActive)
            {
                if (now - _lastToggleTick >= CooldownTicks)
                {
                    _latchedActive = desired;
                    _lastToggleTick = now;
                }
            }

            return _latchedActive;
        }
    }
}
