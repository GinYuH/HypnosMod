using System;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent.Bestiary;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod;
using static CalamityMod.World.CalamityWorld;
using CalamityMod.Skies;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.LoreItems;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Mounts;
using CalamityMod.Items.Placeables.Furniture.BossRelics;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Items.Potions;
using CalamityMod.Items.TreasureBags;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using HypnosMod.Items;
using HypnosMod.Projectiles;
using Terraria.GameContent.ItemDropRules;
using Terraria.Audio;
using Terraria.Localization;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using Terraria.Graphics.Effects;

namespace HypnosMod.HypnosNPCs
{
    [AutoloadBossHead]
    internal class HypnosBoss : ModNPC
    {
        public bool initializedLocal = false;
        public bool afterimages = false;
        public bool p2 = false;
        public bool beserk = false;
        public bool enraged = false;

        public Particle ring;
        public Particle ring2;
        public Particle aura;

        public int ragetimer = 0;
        public int hostdamage = 400;
        public int beserktimer = 0;

    // Remix-style intro assemblage support
    public List<HypnosAssemblagePiece> assemblagePieces = new List<HypnosAssemblagePiece>();
    public int brainFrame = 0;

        public ThanatosSmokeParticleSet SmokeDrawer = new ThanatosSmokeParticleSet(-1, 3, 0f, 16f, 1.5f);
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("XP-00 Hypnos");
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;
            NPC.LifeMaxNERB(1320000, 1980000);
            double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);
            // Use Remix death sound for better presentation
            NPC.DeathSound = CalamityMod.Sounds.CommonCalamitySounds.ExoDeathSound;
            NPC.knockBackResist = 0f;
            NPC.noTileCollide = true;
            NPC.width = 208;
            NPC.height = 138;
            NPC.boss = true;
            NPC.dontTakeDamage = true;
            NPC.damage = 1;
            NPC.defense = 90;
            // Fade-in like Remix during intro
            NPC.alpha = 255;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.HypnosMod.Hypnos.Bestiary"))
            });
        }

        public override void AI()
        {
            // Lol
            //Main.player[Main.myPlayer].ZoneTowerVortex = true;
            if (Main.getGoodWorld)
            {
                NPC.scale = 1.75f;
            }
            //Boss zen
            Main.player[Main.myPlayer].Calamity().isNearbyBoss = true;
            Main.player[Main.myPlayer].AddBuff(ModContent.BuffType<CalamityMod.Buffs.StatBuffs.BossEffects>(), 10, true);
            // Keep ExoMechs sky active for the whole fight on client so lightning continues
            if (Main.netMode != NetmodeID.Server)
            {
                var cp = Main.LocalPlayer?.Calamity();
                if (cp != null)
                {
                    cp.monolithExoShader = Math.Max(cp.monolithExoShader, 10);
                    try { SkyManager.Instance.Activate("CalamityMod:ExoMechs"); } catch { }
                }
            }
            //Handle transitioning to phase 2
            if (NPC.CountNPCS(ModContent.NPCType<HypnosPlug>()) <= 0 && NPC.ai[0] > 1)
            {
                NPC.dontTakeDamage = false;
                if (!p2 && Main.netMode != NetmodeID.Server)
                {
                    ring2 = new BloomRing(NPC.Center, Vector2.Zero, Color.Purple * 1.2f, NPC.scale * 1.5f, 40);
                    GeneralParticleHandler.SpawnParticle(ring2);
                }
                p2 = true;
                NPC.netUpdate = true;
            }
            else if (NPC.ai[0] != 11)
            {
                NPC.dontTakeDamage = true;
            }

            SmokeDrawer.ParticleSpawnRate = 9999999;
            if (ragetimer > 0)
            {
                SmokeDrawer.ParticleSpawnRate = 3;
                SmokeDrawer.BaseMoveRotation = NPC.rotation + MathHelper.PiOver2;
                SmokeDrawer.SpawnAreaCompactness = 200f;
            }
            SmokeDrawer.Update();
            //Pulse fx
            if (Main.netMode != NetmodeID.Server)
            {
				if (NPC.ai[0] == 1 && !initializedLocal)
                {
                    initializedLocal = true;
				}
				if (ring != null)
                {
                    ring.Scale *= 1.1f;
                    ring.Time += 1;
                }
				if (aura != null)
				{
					aura.Position = NPC.Center;
					aura.Velocity = NPC.velocity;
					aura.Time = 0;
				}
				if (ring2 != null)
				{
					ring2.Position = NPC.Center;
					ring2.Velocity = NPC.velocity;
					ring2.Scale *= 1.1f;
					ring2.Time += 1;
				}
			}
            
            hostdamage = Main.expertMode ? 400 : 600;
            HypnosGlobalNPC.hypnos = NPC.whoAmI;
            if ((NPC.life <= NPC.lifeMax * 0.25f && !beserk && revenge) || (NPC.life <= NPC.lifeMax * 0.75f && !beserk && death))
            {
                ChangePhase(11);
                ragetimer = 0;
                beserk = true;
            }

            if (NPC.life <= NPC.lifeMax * 0.25f && revenge)
            {
                if (!enraged && NPC.ai[1] == 0)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar with { Pitch = SoundID.Roar.Pitch + 0.2f}, NPC.Center);
                    enraged = true;
                }
            }

            //The sweet juicy AI
            switch (NPC.ai[0])
            {
                case 0: // Remix-style Spawn animation (assemblage from Brain of Cthulhu)
                    {
                        NPC.ai[1]++;
                        NPC.alpha = (int)MathHelper.Max(NPC.alpha - 20, 0); // fade-in
                        NPC.damage = 0;

                        // Timeline checkpoints
                        int start = 60;
                        int placelower = start + 30;
                        int placeupper = placelower + 30;
                        int placecrown = placeupper + 30;
                        int placebowl = placecrown + 30;
                        int startanim = placebowl + 60;
                        int totaltime = startanim + 70;

                        if (NPC.ai[1] < totaltime)
                        {
                            if (NPC.ai[1] == start + 1)
                            {
                                int xStart = 210;
                                int yStart = -50;
                                int xEnd = 69;
                                int yEnd = -33;
                                CreatePiece(NPC.Center + new Vector2(xStart, yStart), "Side", 0f, false, NPC.Center + new Vector2(xEnd, yEnd), 10, -0.2f);
                                CreatePiece(NPC.Center + new Vector2(-xStart, yStart), "Side", 0f, true, NPC.Center + new Vector2(-xEnd, yEnd), 10, -0.2f);
                            }
                            if (NPC.ai[1] == placelower)
                            {
                                int xStart = 170;
                                int yStart = -90;
                                int xEnd = 70;
                                int yEnd = -70;
                                CreatePiece(NPC.Center + new Vector2(xStart, yStart), "LowerTube", 0f, false, NPC.Center + new Vector2(xEnd, yEnd), 6, 0.1f);
                                CreatePiece(NPC.Center + new Vector2(-xStart, yStart), "LowerTube", 0f, true, NPC.Center + new Vector2(-xEnd, yEnd), 6, 0.1f);
                            }
                            if (NPC.ai[1] == placeupper)
                            {
                                int xStart = 170;
                                int yStart = -120;
                                int xEnd = 60;
                                int yEnd = -96;
                                CreatePiece(NPC.Center + new Vector2(xStart, yStart), "UpperTube", 0f, false, NPC.Center + new Vector2(xEnd, yEnd), 6, 0.3f);
                                CreatePiece(NPC.Center + new Vector2(-xStart, yStart), "UpperTube", 0f, true, NPC.Center + new Vector2(-xEnd, yEnd), 6, 0.3f);
                            }
                            if (NPC.ai[1] == placecrown)
                            {
                                CreatePiece(NPC.Center + new Vector2(0, -200), "Crown", 0f, false, NPC.Center + new Vector2(0, -41), 10, 0.5f);
                            }
                            if (NPC.ai[1] == placebowl)
                            {
                                CreatePiece(NPC.Center + new Vector2(0, 200), "Wires", 0f, false, NPC.Center + new Vector2(0, 0), 10, 0.8f);
                            }
                            if (NPC.ai[1] >= startanim)
                            {
                                if (NPC.localAI[1] == 0.3f)
                                {
                                    SoundEngine.PlaySound(AresTeslaCannon.TeslaOrbShootSound with { Volume = 0.75f }, NPC.Center);
                                }
                                if (NPC.localAI[1] == 1.7f)
                                {
                                    SoundEngine.PlaySound(Artemis.ChargeTelegraphSound with { Volume = 1f }, NPC.Center);
                                }
                                NPC.localAI[1] += 0.05f;
                            }
                        }
                        else
                        {
                            // Spawn the 4 plugs like original
                            for (int i = 0; i < 4; i++)
                            {
                                NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<HypnosPlug>(), 0, NPC.whoAmI, i);
                            }

                            // Sky lightning and flare sound on client only (safe + dedupe)
                            TriggerSpawnLightningClientSafe();
                            assemblagePieces.Clear();

                            ChangePhase(1);
                        }

                        // drive piece movement each tick
                        if (assemblagePieces != null && assemblagePieces.Count > 0)
                        {
                            foreach (HypnosAssemblagePiece piece in assemblagePieces)
                                piece.Move();
                        }
                    }
                    break;
                case 1: //Move downward
                    {
                        NPC.ai[1]++;
                        // Failsafe: if lightning wasn't shown due to net order, show it now on client once
                        if (NPC.ai[1] == 1 && NPC.localAI[2] == 0f)
                            TriggerSpawnLightningClientSafe();
                        afterimages = true;
                        Vector2 playerpos = new Vector2(Main.player[NPC.target].Center.X, Main.player[NPC.target].Center.Y + 200);
                        Vector2 distanceFromDestination = playerpos - NPC.Center;
                        CalamityUtils.SmoothMovement(NPC, 100, distanceFromDestination, 30, 1, true);
                        if (NPC.ai[1] == 1)
                        {
                            Terraria.Audio.SoundEngine.PlaySound(CalamityMod.Sounds.CommonCalamitySounds.ELRFireSound, NPC.Center);
                            int tesladamage = Main.expertMode ? 175 : 250;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<LargeTeslaSphere>(), tesladamage, 0, 255, NPC.target, NPC.whoAmI);
                        }
                        if (NPC.ai[1] >= 120)
                        {
                            ChangePhase(2);
                        }
                    }
                    break;
                case 2: //Fan attack
                    {
                        int stop = 60;
                        if (revenge)
                        {
                            stop -= 5;
                        }
                        if (death)
                        {
                            stop -= 5;
                        }
                        NPC.ai[1]++;
                        if (NPC.ai[1] >= stop && NPC.ai[1] < stop + 120)
                        {
                            NPC.velocity = Vector2.Zero;
                        }
                        else
                        {
                            Vector2 playerpos = new Vector2(Main.player[NPC.target].Center.X, Main.player[NPC.target].Center.Y + 300);
                            Vector2 distanceFromDestination = playerpos - NPC.Center;
                            CalamityUtils.SmoothMovement(NPC, 100, distanceFromDestination, Main.getGoodWorld ? 35 : 30, 1, true);
                        }
                        if (NPC.ai[1] >= stop + 140)
                        {
                            ChangePhase(3);
                        }
                    }
                    break;
                case 3: //Back & forth dashes
                    {
                        float movespeed = Main.getGoodWorld ? 0.06f : 0.04f;
                        NPC.ai[1]++;
                        int start = 90;
                        if (revenge)
                        {
                            start -= 5;
                        }
                        if (death)
                        {
                            start -= 5;
                        }
                        if (NPC.ai[1] >= start)
                        {
                            NPC.ai[2] += movespeed + (NPC.ai[1] * 0.000075f);
                        }
                        else
                        {
                            NPC.ai[2] = 5;
                        }
                        afterimages = true;
                        Vector2 hypos = new Vector2(Main.player[NPC.target].Center.X + ((float)Math.Sin(NPC.ai[2]) * 800), Main.player[NPC.target].Center.Y + 300);
                        float idealx = MathHelper.Lerp(NPC.position.X, hypos.X, 0.4f);
                        float idealy = MathHelper.Lerp(NPC.position.Y, hypos.Y, 0.4f);
                        NPC.position = new Vector2(idealx, idealy);
                        if (NPC.ai[1] >= 420)
                        {
                            ChangePhase(4);
                        }
                    }
                    break;
                case 4: //Spinspinspin
                    {
                        afterimages = true;
                        Player target = Main.player[NPC.target];
                        int chargetime = 60;
                        int chargestart = 100;
                        int chargespeed = 15;
                        Vector2 position = NPC.Center;
                        Vector2 targetPosition = target.Center;
                        int predictamt = revenge ? 2 : 3;
                        NPC.ai[1]++;
                        NPC.damage = hostdamage;
                        NPC.Calamity().canBreakPlayerDefense = true;
                        if (death)
                        {
                            chargestart -= 10;
                        }
                        if (revenge)
                        {
                            chargestart -= 10;
                        }
                        if (NPC.ai[1] % chargetime == 0 && NPC.ai[1] >= chargestart && NPC.ai[1] < 340)
                        {
                            Terraria.Audio.SoundEngine.PlaySound(CalamityMod.NPCs.ExoMechs.Artemis.Artemis.ChargeSound, NPC.Center);
                            if (NPC.ai[1] % (chargetime * predictamt) == 0)
                            {
                                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, position);
                                Vector2 pos = targetPosition + target.velocity * 20f - position;
                                NPC.velocity = Vector2.Normalize(pos) * chargespeed;
                                if (Main.netMode != NetmodeID.Server)
                                {
									ring2 = new BloomRing(NPC.Center, Vector2.Zero, Color.Purple * 1.2f, NPC.scale * 1.5f, 40);
									GeneralParticleHandler.SpawnParticle(ring2);
								}
                                
                            } 
                            else
                            {
                                Vector2 direction = targetPosition - position;
                                direction.Normalize();
                                NPC.velocity = direction * chargespeed;
                            }
                        }
                        if (NPC.ai[1] < chargestart)
                        {
                            Vector2 playerpos = new Vector2(target.Center.X, target.Center.Y + 300);
                            Vector2 distanceFromDestination = playerpos - position;
                            CalamityUtils.SmoothMovement(NPC, 100, distanceFromDestination, 30, 1, true);

                        }
                        else
                        {
                            NPC.velocity *= 1.01f;
                        }
                        if (NPC.ai[1] >= 361)
                        {
                            ChangePhase(5);
                        }
                    }
                    break;
                case 5: //Neuron charges
                    {
                        NPC.ai[1]++;
                        afterimages = false;
                        Vector2 playerpos = new Vector2(Main.player[NPC.target].Center.X, Main.player[NPC.target].Center.Y + 400);
                        Vector2 distanceFromDestination = playerpos - NPC.Center;
                        CalamityUtils.SmoothMovement(NPC, 100, distanceFromDestination, Main.getGoodWorld ? 35 : 30, 1, true);
                        if (NPC.ai[1] >= 480)
                        {
                            if (Main.expertMode)
                            {
                                ChangePhase(10);
                            }
                            else
                            {
                                ChangePhase(2);
                            }
                        }
                    }
                    break;
                //Phase 2
                case 6: //Neuron Lightning gates
                    {
                        Player target = Main.player[NPC.target];
                        if (target.chaosState)
                        {
                            ragetimer = 2;
                        }
                        float chargespeed = enraged ? 22 : 20;
                        float walkspeed = Main.getGoodWorld ? 4 : 2;
                        int chargetime = 60;
                        int chargegate = 60;
                        int setuptime = 180;
                        int attackamt = 2;
                        if (death)
                        {
                            chargegate = 120;
                            chargetime = 25;
                            attackamt = 4;
                        }
                        else if (revenge)
                        {
                            chargegate = 120;
                            chargetime = 40;
                            attackamt = 3;
                        }
                        else if (Main.expertMode)
                        {
                            chargetime = 20;
                        }
                        if (enraged)
                        {
                            attackamt--;
                        }
                        Vector2 position = NPC.Center;
                        Vector2 targetPosition = target.Center;
                        NPC.ai[1]++;
                        NPC.ai[2]++;
                        Vector2 direction = targetPosition - position;
                        direction.Normalize();
                        if (NPC.ai[2] <= setuptime)
                        {
                            NPC.velocity = direction * 2 + direction * walkspeed;
                        }
                        else if (NPC.ai[2] % chargetime == 0 && NPC.ai[2] < setuptime + chargegate)
                        {
                            if (Main.netMode != NetmodeID.Server)
                            {
                                ring?.Kill();
                            }
                            Terraria.Audio.SoundEngine.PlaySound(CalamityMod.NPCs.ExoMechs.Artemis.Artemis.ChargeSound, NPC.Center);
                            NPC.velocity = direction * chargespeed;
                            NPC.damage = hostdamage;
                            NPC.Calamity().canBreakPlayerDefense = true;
                            afterimages = true;
                        }
                        if (NPC.ai[2] > setuptime + chargegate + chargetime)
                        {
                            NPC.damage = 0;
                            NPC.Calamity().canBreakPlayerDefense = false;
                            afterimages = false;
                            NPC.ai[2] = 0;
                        }
                        if (NPC.ai[1] > attackamt * (setuptime + chargegate + chargetime) + 1)
                        {
                            ChangePhase(7);
                        }

                        if (Main.netMode != NetmodeID.Server)
                        {
                            if (!Main.player[Main.myPlayer].dead && Main.player[Main.myPlayer].active)
                            {
                                if (Main.player[Main.myPlayer].wingTime < Main.player[Main.myPlayer].wingTimeMax)
                                {
                                    Main.player[Main.myPlayer].wingTime = Main.player[Main.myPlayer].wingTimeMax;
                                }
                            }
                        }
                    }
                    break;
                case 7: //Rings
                    {
                        Player target = Main.player[NPC.target];
                        int phasetime = 0;
                        if (enraged)
                        {
                            phasetime = death ? 360 : 240;
                        }
                        else
                        {
                            phasetime = death ? 480 : 360;
                        }
                        NPC.ai[1]++;
                        NPC.ai[2]++;

                        if (NPC.ai[1] == 1)
                        {
                            NPC.ai[3] = Main.rand.Next(1, 361);
                            NPC.netUpdate = true;
                        }
                        float rotspeed = NPC.ai[2] * 0.01f;
                        double deg = NPC.ai[3] + NPC.ai[2] * MathHelper.Clamp(rotspeed, NPC.ai[2] * 0.01f, 4);
                        double rad = deg * (Math.PI / 180);
                        double dist = MathHelper.Clamp(200 + NPC.ai[2], 200, 1200);
                        float hyposx = target.Center.X - (int)(Math.Cos(rad) * dist) - NPC.width / 2;
                        float hyposy = target.Center.Y - (int)(Math.Sin(rad) * dist) - NPC.height / 2;                        
                        float idealx = MathHelper.Lerp(NPC.position.X, hyposx, 0.4f);
                        float idealy = MathHelper.Lerp(NPC.position.Y, hyposy, 0.4f);

                        if (NPC.ai[1] == 0)
                        {
                            Terraria.Audio.SoundEngine.PlaySound(CalamityMod.NPCs.ExoMechs.Artemis.Artemis.ChargeSound, NPC.Center);
                        }
                        if (NPC.ai[1] < phasetime - 90)
                        {
                            afterimages = true;
                            NPC.position = new Vector2(idealx, idealy);
                        }
                        else if (NPC.ai[1] < phasetime - 60)
                        {
                            NPC.velocity = Vector2.Zero;
                        }
                        else if (NPC.ai[1] == phasetime - 60)
                        {
                            Vector2 position = NPC.Center;
                            Vector2 targetPosition = target.Center;
                            Vector2 direction = targetPosition - position;
                            direction.Normalize();
                            Terraria.Audio.SoundEngine.PlaySound(CalamityMod.NPCs.ExoMechs.Artemis.Artemis.ChargeSound, NPC.Center);
                            NPC.velocity = direction * 20;
                            NPC.damage = hostdamage;
                            NPC.Calamity().canBreakPlayerDefense = true;
                        }
                        if (NPC.ai[1] > phasetime)
                        {
                            ragetimer--;
                            ChangePhase(8);
                        }
                    }
                    break;
                case 8: //Predictive charge
                    {
                        int attacktime = 180;
                        if (death)
                        {
                            attacktime += 60;
                        }
                        if (Main.expertMode)
                        {
                            attacktime += 60;
                        }
                        if (enraged)
                        {
                            attacktime /= 2;
                        }
                        afterimages = true;
                        Player target = Main.player[NPC.target];
                        int chargetime = 59;
                        int chargespeed = enraged ? 20 : 15;
                        Vector2 position = NPC.Center;
                        Vector2 targetPosition = target.Center;
                        NPC.ai[1]++;
                        NPC.damage = hostdamage;
                        NPC.Calamity().canBreakPlayerDefense = true;
                        if (NPC.ai[1] % chargetime == 0)
                        {
                            Terraria.Audio.SoundEngine.PlaySound(CalamityMod.NPCs.ExoMechs.Artemis.Artemis.ChargeSound, NPC.Center);
                            Vector2 pos = targetPosition + target.velocity * 20f - position;
                            NPC.velocity = Vector2.Normalize(pos) * chargespeed;
                            if (Main.netMode != NetmodeID.Server)
                            {
                                Color ringcolor = ragetimer > 0 ? Color.Red * 1.2f : Color.CornflowerBlue * 0.6f;
                                ring2 = new BloomRing(NPC.Center, Vector2.Zero, ringcolor, NPC.scale * 1.5f, 40);
                                GeneralParticleHandler.SpawnParticle(ring2);
                            }
                            afterimages = true;
                        }
                        if (NPC.ai[1] < (chargetime - 1))
                        {
                            Vector2 playerpos = new Vector2(target.Center.X, target.Center.Y + 300);
                            Vector2 distanceFromDestination = playerpos - position;
                            CalamityUtils.SmoothMovement(NPC, 100, distanceFromDestination, 30, 1, true);

                        }
                        else
                        {
                            NPC.velocity *= 1.01f;
                        }
                        if (NPC.ai[1] >= attacktime)
                        {
                            NPC.velocity = Vector2.Zero;
                            ragetimer--;
                            ChangePhase(9);
                        }
                    }
                    break;
                case 9: //SWR Yukari attack
                    {
                        int phasetime = enraged ? 600 : 840;
                        NPC.ai[1]++;
                        Player target = Main.player[NPC.target];
                        Vector2 distance = target.Center - NPC.Center;
                        distance *= 6;
                        NPC.velocity = (NPC.velocity * 24f + distance) / 25f;
                        NPC.velocity.Normalize();
                        NPC.velocity *= 6;
                        if (NPC.ai[1] > phasetime)
                        {
                            if (death && beserk)
                            {
                                ChangePhase(11);
                            }
                            else
                            {
                                ChangePhase(6);
                            }
                        }

                        if (Main.netMode != NetmodeID.Server)
                        {
                            if (!Main.player[Main.myPlayer].dead && Main.player[Main.myPlayer].active)
                            {
                                if (Main.player[Main.myPlayer].wingTime < Main.player[Main.myPlayer].wingTimeMax)
                                {
                                    Main.player[Main.myPlayer].wingTime = Main.player[Main.myPlayer].wingTimeMax;
                                }
                            }
                        }
                    }
                    break;
                case 10: //Lightning wall
                    {
                        NPC.ai[1]++;
                        afterimages = false;
                        Vector2 playerpos = new Vector2(Main.player[NPC.target].Center.X, Main.player[NPC.target].Center.Y + 300);
                        Vector2 distanceFromDestination = playerpos - NPC.Center;
                        CalamityUtils.SmoothMovement(NPC, 100, distanceFromDestination, Main.getGoodWorld ? 35 : 30, 1, true);
                        int neuroncharge = -1;
                        if (NPC.ai[1] % 40 == 0)
                        {
                            neuroncharge = Main.rand.Next(0, 12);
                        }
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC neuron = Main.npc[i];
                            if (neuron.type == ModContent.NPCType<AergiaNeuron>() && neuron.active && neuroncharge != 0 && neuron.ai[1] == neuroncharge && neuron.ai[2] <= 0)
                            {
                                neuron.ai[2] = 1;
                                neuroncharge = 0;
                            }
                        }
                        if (NPC.ai[1] >= 480)
                        {
                            ChangePhase(2);
                        }
                    }
                    break;
                case 11: //Vanish
                    {
                        int attackamt = enraged ? 2 : 4;
                        NPC.ai[1]++;
                        if (NPC.ai[1] > 30)
                        {
                            NPC.ai[2]++;
                        }
                        if (NPC.ai[1] == 31)
                        {
                            if (Main.netMode != NetmodeID.Server)
                            {
                                ring2 = new BloomRing(NPC.Center, Vector2.Zero, Color.Purple * 1.2f, NPC.scale * 1.5f, 40);
                                GeneralParticleHandler.SpawnParticle(ring2);
                            }
                        }
                        if (NPC.ai[3] == attackamt)
                        {
                            NPC.dontTakeDamage = false;
                            ChangePhase(6);
                        }
                        if (NPC.ai[1] >= 31)
                        {
                            NPC.dontTakeDamage = true;
                        }
                        Vector2 playerpos = new Vector2(Main.player[NPC.target].Center.X, Main.player[NPC.target].Center.Y + 400);
                        Vector2 distanceFromDestination = playerpos - NPC.Center;
                        CalamityUtils.SmoothMovement(NPC, 100, distanceFromDestination, 30, 1.1f, true);
                    }
                    break;
            }
            //Change to phase 2 
            if ((NPC.ai[0] < 6 || NPC.ai[0] == 10) && p2)
            {
                ChangePhase(6);
            }
        }

        public void ChangePhase(int phasenum, bool reset1 = true, bool reset2 = true, bool reset3 = true)
        {
            if (beserk && NPC.ai[0] != 10)
            {
                beserktimer++;
            }
            NPC.ai[0] = beserktimer == 5 ? 11 : phasenum;
            if (reset1)
            {
                NPC.ai[1] = 0;
            }
            if (reset2)
            {
                NPC.ai[2] = 0;
            }
            if (reset3)
            {
                NPC.Calamity().newAI[3] = 0;
            }
            NPC.ai[3] = 0;
            NPC.damage = 0;
            NPC.Calamity().canBreakPlayerDefense = false;
            afterimages = false;
            //Reset Aergia Neuron variables
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC neur = Main.npc[i];
                if (neur.type == NPCType<AergiaNeuron>())
                {
                    neur.velocity = Vector2.Zero;
                    neur.ai[2] = 0;
                    neur.Calamity().newAI[0] = 0;
                    neur.Calamity().newAI[1] = 0;
                    neur.Calamity().newAI[2] = 0;
                    neur.damage = 0;
                    neur.Calamity().canBreakPlayerDefense = false;
                    neur.ModNPC<AergiaNeuron>().afterimages = false;
                    if (p2)
                    {
                        neur.ai[3] = 0;
                    }
                    neur.ModNPC<AergiaNeuron>().rottimer = 0;
                }
            }
            NPC.TargetClosest();
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.15f;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
            // Drive brain animation speed during intro like Remix
            NPC.localAI[0]++;
            int gate = 6;
            if (NPC.ai[1] > 91)
                gate = 4;
            if (NPC.localAI[0] % gate == 0)
            {
                brainFrame++;
                if (brainFrame == 3)
                    brainFrame = 0;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D eyetexture = Request<Texture2D>("HypnosMod/HypnosNPCs/Hypnos_Eye").Value;
            Texture2D glowmask = Request<Texture2D>("HypnosMod/HypnosNPCs/Hypnos_Glow").Value;
            if (NPC.IsABestiaryIconDummy)
            {
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (NPC.spriteDirection == 1)
                    spriteEffects = SpriteEffects.FlipHorizontally;

                Texture2D texture = TextureAssets.Npc[NPC.type].Value;
                Vector2 origin = new Vector2((float)(texture.Width / 2), (float)(texture.Height / Main.npcFrameCount[NPC.type] / 2));
                Color white = Color.White;
                float colorLerpAmt = 0.5f;
                int afterimageAmt = 7;

                Color eyecolor = NPC.ModNPC<HypnosBoss>().ragetimer > 0 ? Color.Red : Lighting.GetColor((int)NPC.position.X / 16, (int)NPC.position.Y / 16);
                Color glowcolor = NPC.ModNPC<HypnosBoss>().ragetimer > 0 ? Color.Red : Color.White;

                if (CalamityConfig.Instance.Afterimages && NPC?.ModNPC<HypnosBoss>().afterimages == true)
                {
                    for (int i = 1; i < afterimageAmt; i += 2)
                    {
                        Color color1 = drawColor;
                        color1 = Color.Lerp(color1, white, colorLerpAmt);
                        color1 = NPC.GetAlpha(color1);
                        color1 *= (float)(afterimageAmt - i) / 15f;
                        Vector2 offset = NPC.oldPos[i] + new Vector2((float)NPC.width, (float)NPC.height) / 2f - screenPos;
                        offset -= new Vector2((float)texture.Width, (float)(texture.Height / Main.npcFrameCount[NPC.type])) * NPC.scale / 2f;
                        offset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                        spriteBatch.Draw(texture, offset, NPC.frame, color1, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
                    }
                }

                Vector2 npcOffset = NPC.Center - screenPos;
                npcOffset -= new Vector2((float)texture.Width, (float)(texture.Height / Main.npcFrameCount[NPC.type])) * NPC.scale / 2f;
                npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                spriteBatch.Draw(texture, npcOffset, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
                spriteBatch.Draw(glowmask, npcOffset, NPC.frame, glowcolor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
                spriteBatch.Draw(eyetexture, npcOffset, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
            }
            else if (NPC.ai[0] == 0)
            {
                // Intro disguise: draw Brain of Cthulhu body and assemblage parts
                Texture2D brian = ModContent.Request<Texture2D>("Terraria/Images/NPC_266").Value;
                Vector2 origin = new Vector2((float)(brian.Width / 2), (float)(brian.Height / 8));
                Vector2 npcOffset = NPC.Center - screenPos;
                npcOffset -= new Vector2((float)brian.Width, (float)(brian.Height / 4)) * NPC.scale / 2f;
                npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY + 100);

                int hideSpine = 0;
                if (NPC.ai[1] > 195)
                    hideSpine = (int)MathHelper.Max((int)MathHelper.Lerp(0, -80, (NPC.ai[1] - 195) / 2), -80);

                spriteBatch.Draw(brian, npcOffset, brian.Frame(1, 8, 0, brainFrame, sizeOffsetY: hideSpine), NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale * 0.95f, SpriteEffects.None, 0f);

                // Draw assemblage pieces (textures are expected in HypnosMod/HypnosNPCs/Assemblage*)
                if (assemblagePieces != null && assemblagePieces.Count > 0)
                {
                    foreach (HypnosAssemblagePiece piece in assemblagePieces)
                    {
                        Texture2D tex = ModContent.Request<Texture2D>("HypnosMod/HypnosNPCs/Assemblage" + piece.texture).Value;
                        Vector2 texHalf = new Vector2(tex.Width, tex.Height) * 0.5f;
                        spriteBatch.Draw(tex, piece.position - screenPos + Vector2.UnitY * 40, null, NPC.GetAlpha(drawColor) * (0.00392156863f * piece.opacity), 0, texHalf, NPC.scale, piece.leftSide ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);
                    }
                }

                // Eye/glow flicker near the end of the intro
                int flickrate = (int)MathHelper.Lerp(5, 2, NPC.localAI[1] / 2);
                bool flickOn = NPC.localAI[1] >= 2 ? true : Main.rand.NextBool(flickrate);
                if (NPC.ai[1] > 240)
                {
                    if (flickOn)
                    {
                        spriteBatch.Draw(eyetexture, npcOffset + new Vector2(-4, 11), eyetexture.Frame(1, 4, 0, 0), Color.White * MathHelper.Clamp(NPC.localAI[1], 0, 1), NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
                        spriteBatch.Draw(glowmask, npcOffset + new Vector2(-4, 11), eyetexture.Frame(1, 4, 0, 0), Color.White * MathHelper.Clamp(NPC.localAI[1], 0, 1), NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
                    }
                }
            }

            SmokeDrawer.DrawSet(NPC.Center);
            return false;
        }
        public override void ModifyTypeName(ref string typeName)
        {
            // Disguise as Brain of Cthulhu during intro timeline
            typeName = (NPC.ai[0] == 0 && NPC.ai[1] > 0) ? ContentSamples.NpcsByNetId[NPCID.BrainofCthulhu].TypeName : typeName;
        }
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance);
            NPC.damage = (int)(NPC.damage * NPC.GetExpertDamageMultiplier());
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule normalOnly = new LeadingConditionRule(new Conditions.NotExpert());
            npcLoot.Add(normalOnly);

            // Trophies
            npcLoot.Add(ModContent.ItemType<HypnosTrophyInv>());
            npcLoot.Add(ModContent.ItemType<HypnosMask>(), 3);

            // Relic
            npcLoot.Add(ItemDropRule.ByCondition(DropHelper.If(() => Main.masterMode || revenge), ModContent.ItemType<DraedonRelic>()));

            // Lore item
            npcLoot.Add(ItemDropRule.ByCondition(DropHelper.If(() => !HypnosWorld.downedHypnos), ModContent.ItemType<LoreExoMechs>()));

            // Treasure bag
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<DraedonBag>()));

            // All other drops are contained in the bag, so they only drop directly on Normal
            if (!Main.expertMode)
            {
                // Materials
                normalOnly.Add(ModContent.ItemType<ExoPrism>(), 1, 24, 32);

                // Equipment
                normalOnly.Add(ModContent.ItemType<ExoThrone>());
                normalOnly.Add(ModContent.ItemType<DraedonsHeart>());

                // Weapons from this addon (25% each in Normal mode)
                normalOnly.Add(ModContent.ItemType<AergianTechnistaff>(), 4);
                normalOnly.Add(ModContent.ItemType<Neuraze>(), 4);
                normalOnly.Add(ModContent.ItemType<Planetarium>(), 4);

                // Vanity
                normalOnly.Add(ModContent.ItemType<DraedonMask>(), 3);
            }
        }

        public override void OnKill()
        {
            HypnosWorld.downedHypnos = true;
            if (Main.netMode != NetmodeID.Server)
            {
                if (aura != null)
                    aura.Kill();
                if (ring != null)
                    ring.Kill();
                // Reset sky boost on client when fight ends
                try { var cp = Main.LocalPlayer?.Calamity(); if (cp != null) cp.monolithExoShader = 0; } catch { }
            }

            if (NPC.life <= 0)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Hypnos1").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Hypnos2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Hypnos3").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Hypnos4").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Hypnos5").Type, 1f);
                }
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.soundDelay == 0)
            {
                NPC.soundDelay = 5;
                SoundEngine.PlaySound(CalamityMod.Sounds.CommonCalamitySounds.ExoHitSound, NPC.Center);
            }
        }

        public override bool CheckActive()
        {
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(afterimages);
            //writer.Write(initializedLocal);
            writer.Write(p2);
            writer.Write(enraged);
            writer.Write(beserk);
            writer.Write(hostdamage);
            writer.Write(ragetimer);
            writer.Write(beserktimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            afterimages = reader.ReadBoolean();
			//initializedLocal = reader.ReadBoolean();
			p2 = reader.ReadBoolean();

			enraged = reader.ReadBoolean();
			beserk = reader.ReadBoolean();

			hostdamage = reader.ReadInt32();
			ragetimer = reader.ReadInt32();
            
            beserktimer = reader.ReadInt32();
        }

        public static void SummonDraedon(Player player)
        { // don't call it on multiplayer client
			NPC.NewNPC(new Terraria.DataStructures.EntitySource_BossSpawn(player), (int)player.Center.X, (int)(player.Center.Y - 1200), NPCType<Draedon>(), 0, 0, 0, 0, player.whoAmI, player.whoAmI);

		}

        // Helpers for Remix-style assemblage
        public HypnosAssemblagePiece CreatePiece(Vector2 position, string texture, float opacity, bool leftSide, Vector2 destination, float power, float pitch)
        {
            HypnosAssemblagePiece piece = new HypnosAssemblagePiece(position, texture, opacity, leftSide, destination, power, pitch);
            assemblagePieces.Add(piece);
            return piece;
        }

        // Client-safe trigger for ExoMechs sky lightning with a tiny particle fallback
        private void TriggerSpawnLightningClientSafe()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            if (NPC.localAI[2] >= 1f)
                return; // already played locally

            bool played = false;
            try
            {
                // Ensure the sky draws even when ExoMechs/Draedon are not present
                var cp = Main.LocalPlayer?.Calamity();
                if (cp != null)
                    cp.monolithExoShader = Math.Max(cp.monolithExoShader, 5);

                try { SkyManager.Instance.Activate("CalamityMod:ExoMechs"); } catch { }
                ExoMechsSky.CreateLightningBolt(22, true);
                played = true;
            }
            catch
            {
                // Fallback: subtle particles so players still get feedback if the sky effect isn't available
                try
                {
                    Color c = Color.Lerp(Color.CornflowerBlue, Color.White, 0.5f) * 0.9f;
                    for (int i = 0; i < 6; i++)
                    {
                        var ring = new BloomRing(NPC.Center, Main.rand.NextVector2Circular(2f, 2f), c, NPC.scale * Main.rand.NextFloat(1.2f, 1.8f), 30 + i * 3);
                        GeneralParticleHandler.SpawnParticle(ring);
                    }
                    Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, 6f);
                }
                catch { }
            }

            try
            {
                SoundEngine.PlaySound(CalamityMod.Sounds.CommonCalamitySounds.FlareSound, NPC.Center);
            }
            catch { }

            NPC.localAI[2] = played ? 1f : 0.5f; // mark as handled to avoid repeats
        }
	}

    // Local copy of Remix's piece class (trimmed to essentials) to avoid CalRemix dependency
    public class HypnosAssemblagePiece
    {
        public Vector2 position;
        public string texture;
        public float opacity;
        public bool leftSide;
        public Vector2 destination;
        public float time = -1f;
        public bool playedEffect = false;
        public float power;
        public float pitch;

        public HypnosAssemblagePiece(Vector2 position, string texture, float opacity, bool leftSide, Vector2 destination, float power, float pitch)
        {
            this.position = position;
            this.texture = texture;
            this.opacity = opacity;
            this.leftSide = leftSide;
            this.destination = destination;
            this.power = power;
            this.pitch = pitch;
        }

        public void Move()
        {
            time += 0.1f;
            if (time > 0)
                position = Vector2.Lerp(position, destination, time);

            if (opacity < 255)
                opacity += 30;
            else
                opacity = 255;

            if (time >= 1 && !playedEffect)
            {
                SoundEngine.PlaySound(CalamityMod.Sounds.CommonCalamitySounds.ExoHitSound with { Pitch = pitch }, position);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = power;
                playedEffect = true;
            }
        }
    }
}