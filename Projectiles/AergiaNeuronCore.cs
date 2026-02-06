using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Sounds;
using HypnosMod.Buffs;
using HypnosMod.Items;
using HypnosMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HypnosMod.Projectiles
{
    public class AergiaNeuronCore : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CheckActive(player);

            bool channelingStaff = player.controlUseTile && player.HeldItem.type == ModContent.ItemType<AergianTechnistaff>() && !player.CCed;

            if (channelingStaff)
                Projectile.ai[2]++;

            if (Projectile.ai[2] > 120 && channelingStaff)
            {
                float projX = Projectile.position.X;
                float projY = Projectile.position.Y;
                float attackRange = 1300f;
                bool canAttack = false;
                int separationAnxietyDist = 1100;
                if (Projectile.ai[1] != 0f)
                {
                    separationAnxietyDist = 1800;
                }
                if (Math.Abs(Projectile.Center.X - Main.player[Projectile.owner].Center.X) + Math.Abs(Projectile.Center.Y - Main.player[Projectile.owner].Center.Y) > (float)separationAnxietyDist)
                {
                    Projectile.ai[0] = 1f;
                }
                if (Projectile.ai[0] == 0f)
                {
                    if (player.HasMinionAttackTargetNPC)
                    {
                        NPC npc = Main.npc[player.MinionAttackTargetNPC];
                        if (npc.CanBeChasedBy(Projectile, false))
                        {
                            float npcX = npc.position.X + (float)(npc.width / 2);
                            float npcY = npc.position.Y + (float)(npc.height / 2);
                            float npcDist = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - npcX) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - npcY);
                            if (npcDist < attackRange && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height))
                            {
                                projX = npcX;
                                projY = npcY;
                                canAttack = true;
                            }
                        }
                    }
                    if (!canAttack)
                    {
                        foreach (NPC n in Main.ActiveNPCs)
                        {
                            if (n.CanBeChasedBy(Projectile, false))
                            {
                                float otherNPCX = n.position.X + (float)(n.width / 2);
                                float otherNPCY = n.position.Y + (float)(n.height / 2);
                                float otherNPCDist = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - otherNPCX) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - otherNPCY);
                                if (otherNPCDist < attackRange && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, n.position, n.width, n.height))
                                {
                                    attackRange = otherNPCDist;
                                    projX = otherNPCX;
                                    projY = otherNPCY;
                                    canAttack = true;
                                }
                            }
                        }
                    }
                }
                if (!canAttack)
                {
                    float returnSpeed = 8f;
                    if (Projectile.ai[0] == 1f)
                    {
                        returnSpeed = 12f;
                    }
                    Vector2 playerDirection = Projectile.Center;
                    float playerXDist = player.Center.X - playerDirection.X;
                    float playerYDist = player.Center.Y - playerDirection.Y - 60f;
                    float playerDist = (float)Math.Sqrt((double)(playerXDist * playerXDist + playerYDist * playerYDist));
                    if (playerDist < 100f && Projectile.ai[0] == 1f && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
                    {
                        Projectile.ai[0] = 0f;
                    }
                    if (playerDist > 2000f)
                    {
                        Projectile.position.X = player.Center.X - (float)(Projectile.width / 2);
                        Projectile.position.Y = player.Center.Y - (float)(Projectile.width / 2);
                    }
                    if (playerDist > 70f)
                    {
                        playerDist = returnSpeed / playerDist;
                        playerXDist *= playerDist;
                        playerYDist *= playerDist;
                        Projectile.velocity.X = (Projectile.velocity.X * 20f + playerXDist) / 21f;
                        Projectile.velocity.Y = (Projectile.velocity.Y * 20f + playerYDist) / 21f;
                    }
                    else
                    {
                        if (Projectile.velocity.X == 0f && Projectile.velocity.Y == 0f)
                        {
                            Projectile.velocity.X = -0.15f;
                            Projectile.velocity.Y = -0.05f;
                        }
                        Projectile.velocity *= 1.01f;
                    }
                    Projectile.rotation = Projectile.velocity.X * 0.05f;
                    if ((double)Math.Abs(Projectile.velocity.X) > 0.2)
                    {
                        Projectile.spriteDirection = -Projectile.direction;
                        return;
                    }
                }
                else
                {
                    if (Projectile.ai[1] == -1f)
                    {
                        Projectile.ai[1] = 11f;
                    }
                    if (Projectile.ai[1] > 0f)
                    {
                        Projectile.ai[1] -= 1f;
                    }
                    if (Projectile.ai[1] == 0f)
                    {
                        float hoverSpeed = 8f; //12
                        Vector2 playerDirectionAgain = Projectile.Center;
                        float playerXDistAgain = projX - playerDirectionAgain.X;
                        float playerYDistAgain = projY - playerDirectionAgain.Y;
                        float playerDistAgain = (float)Math.Sqrt((double)(playerXDistAgain * playerXDistAgain + playerYDistAgain * playerYDistAgain));
                        if (playerDistAgain < 100f)
                        {
                            hoverSpeed = 10f; //14
                        }
                        playerDistAgain = hoverSpeed / playerDistAgain;
                        playerXDistAgain *= playerDistAgain;
                        playerYDistAgain *= playerDistAgain;
                        Projectile.velocity.X = (Projectile.velocity.X * 40f + playerXDistAgain) / 41f;
                        Projectile.velocity.Y = (Projectile.velocity.Y * 40f + playerYDistAgain) / 41f;
                    }
                    else
                    {
                        if (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y) < 10f)
                        {
                            Projectile.velocity *= 1.05f;
                        }
                    }
                    Projectile.rotation = Projectile.velocity.X * 0.05f;
                    if ((double)Math.Abs(Projectile.velocity.X) > 0.2)
                    {
                        Projectile.spriteDirection = -Projectile.direction;
                        return;
                    }
                }
            }
            else if (!channelingStaff)
            {
                Projectile.ai[2] = 0;
            }
            if (Projectile.ai[2] <= 120)
            {
                if (Projectile.Distance(player.Center) > 22)
                    Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center - Vector2.UnitY * player.gfxOffY, 0.5f);
                else
                    Projectile.Center = player.Center;
                Projectile.velocity = Vector2.Zero;
            }
            if (Projectile.ai[2] == 120)
                SoundEngine.PlaySound(CommonCalamitySounds.ELRFireSound, Projectile.Center);

            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner != Projectile.owner)
                    continue;
                if (p.type == ModContent.ProjectileType<AergiaNeuronSummon>())
                    NeuronAI(p);
            }
        }

        private void CheckActive(Player owner)
        {
            owner.AddBuff(ModContent.BuffType<AergiaNeuronBuff>(), 3600);
            if (Projectile.type != ModContent.ProjectileType<AergiaNeuronCore>())
                return;
            if (owner.dead)
                owner.GetModPlayer<HypnosPlayer>().neuron = false;
            if (owner.GetModPlayer<HypnosPlayer>().neuron)
                Projectile.timeLeft = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int neuronType = ModContent.ProjectileType<AergiaNeuronSummon>();
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type != neuronType || p.owner != Projectile.owner || p.ai[0] != Projectile.whoAmI)
                    continue;

                foreach (Projectile pe in Main.ActiveProjectiles)
                {
                    if (pe.type != neuronType || pe.owner != Projectile.owner || pe.ai[0] != Projectile.whoAmI)
                        continue;
                    if (pe.ai[1] != p.ai[1] + 1 && !(pe.ai[1] == (pe.ai[2] - 1) && p.ai[1] == 0))
                        continue;
                    List<Vector2> points = AresTeslaOrb.DetermineElectricArcPoints(p.Center, pe.Center, (int)(250290787 * pe.ai[1]));
                    PrimitiveRenderer.RenderTrail(points, new(BackgroundWidthFunction, BackgroundColorFunction, smoothen: false), 90);
                    PrimitiveRenderer.RenderTrail(points, new(WidthFunction, ColorFunction, smoothen: false), 90);
                }
            }
            return false;
        }

        internal float WidthFunction(float completionRatio, Vector2 v) => 0.9f;
        internal float BackgroundWidthFunction(float completionRatio, Vector2 v) => WidthFunction(completionRatio, v) * 4f;
        public Color BackgroundColorFunction(float completionRatio, Vector2 v) => Color.CornflowerBlue * 0.4f;
        internal Color ColorFunction(float completionRatio, Vector2 v)
        {
            Color baseColor1 = Color.Cyan;
            Color baseColor2 = Color.Cyan;
            float fadeToWhite = MathHelper.Lerp(0f, 0.65f, (float)Math.Sin(MathHelper.TwoPi * completionRatio + Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f);
            Color baseColor = Color.Lerp(baseColor1, Color.White, fadeToWhite);
            Color color = Color.Lerp(baseColor, baseColor2, ((float)Math.Sin(MathHelper.Pi * completionRatio + Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f) * 0.8f) * 0.65f;
            color.A = (byte)MathHelper.Lerp(0, 84, MathHelper.Min(Projectile.ai[2], 120) / 120);
            return color;
        }

        public static void NeuronAI(Projectile p)
        {
            Projectile owner = Main.projectile[(int)p.ai[0]];
            Vector2 destination = Vector2.Zero;
            p.localAI[0] += MathHelper.Lerp(1, 12, MathHelper.Min(owner.ai[2], 120) / 120);
            int distance = 200;
            double deg = p.ai[1] * 360 / p.ai[2] + p.localAI[0];
            double rad = deg * (Math.PI / 180);
            destination.X = owner.Center.X - (int)(Math.Cos(rad) * distance);
            destination.Y = owner.Center.Y - (int)(Math.Sin(rad) * distance);
            p.Center = destination;
            p.velocity = Vector2.Zero;
            p.extraUpdates = owner.extraUpdates;
            p.numUpdates = owner.numUpdates;
            p.netUpdate = owner.netUpdate;
        }
    }
}
