using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;

namespace HypnosMod.Projectiles
{
    public class PlanetariumLaser : ModProjectile
    {
        public override string Texture => "HypnosMod/Sprites/BlueExoPulseLaser";

    private int phase = 0; // 0 = going up; 1 = choose target; 2 = homing/flying; 3 = no-target drift
        private int targetIndex = -1;
    private float traveled; // distance traveled in mouse mode
    private bool forceMouseMode; // set by alt fire
    private bool initDone; // ensure we read flags once

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1; // custom pierce control (infinite while going up)
            Projectile.timeLeft = 180; // enough to reach top
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.light = 0f; // avoid white wash, we'll add colored light manually
            Projectile.Opacity = 1f;
            Projectile.usesLocalNPCImmunity = true; // allow multi-hits while going up
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Read initial flag only once at startup
            if (!initDone)
            {
                forceMouseMode = Projectile.ai[0] == 1f;
                // clear ai[0] so it can be reused later as needed
                if (forceMouseMode)
                    Projectile.ai[0] = 0f;
                initDone = true;
            }

            // advance animation frames
            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            // colored light matching tint
            Lighting.AddLight(Projectile.Center, 1.0f, 0.4f, 0.12f);

            // Boss detection (on screen)
            int bossIdx = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.active && n.boss && n.CanBeChasedBy(this) && n.WithinRange(Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2), Math.Max(Main.screenWidth, Main.screenHeight)))
                {
                    bossIdx = i;
                    break;
                }
            }

            if (phase == 0)
            {
                if (Projectile.Center.Y < Main.screenPosition.Y - 32f)
                {
                    phase = 1;
                    Projectile.hide = true;
                }
                Projectile.rotation = Projectile.velocity.ToRotation();
                return;
            }

            if (phase == 1)
            {
                // acquire target
                targetIndex = -1;
                if (bossIdx != -1)
                {
                    // highest priority: on-screen boss
                    targetIndex = bossIdx;
                }
                else
                {
                    // next: any enemy anywhere (random pick)
                    for (int attempts = 0; attempts < 200 && targetIndex == -1; attempts++)
                    {
                        int i = Main.rand.Next(Main.maxNPCs);
                        NPC n = Main.npc[i];
                        if (n.active && !n.friendly && n.CanBeChasedBy(this))
                        {
                            targetIndex = i;
                            break;
                        }
                    }
                    // fallback deterministic scan
                    if (targetIndex == -1)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC n = Main.npc[i];
                            if (n.active && !n.friendly && n.CanBeChasedBy(this))
                            {
                                targetIndex = i;
                                break;
                            }
                        }
                    }
                }

                // If left click and no enemy found, do not fallback to mouse: just drift and expire
                if (!forceMouseMode && targetIndex == -1)
                {
                    Projectile.timeLeft = Math.Min(Projectile.timeLeft, 30);
                    phase = 3;
                    return;
                }

                // Right-click should track mouse; left-click keeps NPC homing when found
                bool mouseMode = forceMouseMode || targetIndex == -1;
                Vector2 basePoint = mouseMode ? Main.MouseWorld : Main.npc[targetIndex].Center;
                float offsetX = Main.rand.NextFloat(-64f, 64f);
                Vector2 dropPos = new Vector2(basePoint.X + offsetX, Main.screenPosition.Y - 80f);
                Projectile.Center = dropPos;

                if (mouseMode)
                {
                    // store mouse Y threshold in ai[0] to enable tile collision after passing it
                    Projectile.ai[0] = Main.MouseWorld.Y;
                    Projectile.ai[1] = 0f;
                    Projectile.localAI[0] = 1f; // flag mouse mode
                    Vector2 dir = (Main.MouseWorld - dropPos).SafeNormalize(Vector2.UnitY);
                    float speed = 25f;
                    Projectile.velocity = dir * speed;
                    Projectile.tileCollide = false; // pierce until crossing mouse Y
                    traveled = 0f;
                    Projectile.timeLeft = 300;
                }
                else
                {
                    Vector2 tp = Main.npc[targetIndex].Center;
                    Projectile.ai[0] = tp.X;
                    Projectile.ai[1] = tp.Y;
                    Projectile.localAI[0] = 0f; // npc mode
                    Vector2 dir = (tp - dropPos).SafeNormalize(Vector2.UnitY);
                    float speed = 25f;
                    Projectile.velocity = dir * speed;
                    Projectile.timeLeft = 180;
                }

                Projectile.hide = false;
                phase = 2;
                return;
            }

            if (phase == 2)
            {
                if (Projectile.localAI[0] == 1f)
                {
                    // mouse-directed flight: accelerate forward; after crossing mouse Y, wait until we're in non-solid space then enable tile collision
                    float thresholdY = Projectile.ai[0];
                    if (!Projectile.tileCollide)
                    {
                        if (Projectile.Center.Y >= thresholdY)
                        {
                            // mark as crossed threshold
                            Projectile.localAI[1] = 1f;
                            if (!IsInsideSolid(Projectile.Center))
                                Projectile.tileCollide = true;
                        }
                        else if (Projectile.localAI[1] == 1f && !IsInsideSolid(Projectile.Center))
                        {
                            // already crossed before and now entered air; enable collision now
                            Projectile.tileCollide = true;
                        }
                    }

                    Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = dir * 25f;
                    Projectile.rotation = Projectile.velocity.ToRotation();

                    traveled += 25f;
                    if (traveled >= 3200f)
                        Projectile.Kill();
                }
                else
                {
                    // NPC homing
                    Vector2 tp = new Vector2(Projectile.ai[0], Projectile.ai[1]);
                    Vector2 desired = (tp - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Vector2 desiredVel = desired * 25f;
                    Projectile.velocity = desiredVel;
                    Projectile.rotation = Projectile.velocity.ToRotation();
                    if (Vector2.Distance(Projectile.Center, tp) <= 16f)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tp, Vector2.Zero, ModContent.ProjectileType<PlanetariumExplosion>(), Projectile.damage, 0, Projectile.owner);
                        Projectile.Kill();
                        return;
                    }
                    // expire if leaves bottom screen
                    if (Projectile.Center.Y > Main.screenPosition.Y + Main.screenHeight + 32f)
                        Projectile.Kill();
                }
            }

            // phase 3: no target found on left click, keep flying and disappear shortly
            if (phase == 3)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                return;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.localAI[0] == 1f)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PlanetariumExplosion>(), Projectile.damage, 0, Projectile.owner);
                return true; // kill projectile
            }
            return false;
        }

    private bool IsInsideSolid(Vector2 worldPos)
        {
            // use projectile dimensions to check overlap with solid tiles
            Point pos = worldPos.ToTileCoordinates();
            int w = Math.Max(1, Projectile.width / 2);
            int h = Math.Max(1, Projectile.height / 2);
            return Collision.SolidCollision(worldPos - new Vector2(w, h), w * 2, h * 2);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Use original texture with orange-red tint; follow RedExoPulseLaser style
            lightColor = new Color(255f * Projectile.Opacity, 100f * Projectile.Opacity, 20f * Projectile.Opacity);
            Vector2 drawOffset = Projectile.velocity.SafeNormalize(Vector2.Zero) * -30f;
            Projectile.Center += drawOffset;
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            Projectile.Center -= drawOffset;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (phase == 2)
            {
                target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
                target.AddBuff(ModContent.BuffType<HolyFlames>(), 180);
                target.AddBuff(ModContent.BuffType<Vaporfied>(), 180);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<PlanetariumExplosion>(), damageDone, 0, Projectile.owner);
                Projectile.Kill();
            }
        }
    }
}
