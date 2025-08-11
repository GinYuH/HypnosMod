using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Buffs.DamageOverTime;

namespace HypnosMod.Projectiles
{
    // Visual explosion that damages in a small radius
    public class PlanetariumExplosion : ModProjectile
    {
        public override string Texture => "HypnosMod/Sprites/PrismExplosionSmall";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7; // 7 vertical frames
        }

        public override void SetDefaults()
        {
            // 3 tiles radius visual; we'll scale/draw via frames
            Projectile.width = Projectile.height = 130;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 150;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 11;
            Projectile.scale = 0.5f;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 1.15f);
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 9 == 8)
                Projectile.frame++;

            if (Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.Kill();
            Projectile.scale *= 1.0115f;
            Projectile.Opacity = Utils.GetLerpValue(5f, 36f, Projectile.timeLeft, true);

            // Damage pulse on first tick with debuffs
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                float radius = 48f; // ~3 tiles
        foreach (NPC n in Main.npc)
                {
                    if (n.active && !n.friendly && Vector2.Distance(n.Center, Projectile.Center) <= radius)
                    {
                        int dmg = Projectile.damage > 0 ? Projectile.damage : 0;
                        n.SimpleStrikeNPC(dmg, 0);
            n.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
            n.AddBuff(ModContent.BuffType<HolyFlames>(), 180);
            n.AddBuff(ModContent.BuffType<Vaporfied>(), 180);
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = frame.Size() * 0.5f;
            Main.EntitySpriteDraw(texture, drawPosition, frame, Color.White, 0f, origin, 1f, SpriteEffects.None, 0);
            return false;
        }

        
    }
}
