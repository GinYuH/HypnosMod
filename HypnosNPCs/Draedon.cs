using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Shaders;
using Terraria.Audio;
using CalamityMod;
using Terraria.Chat;
using Terraria.Localization;

namespace HypnosMod.HypnosNPCs
{
    internal class Draedon : ModNPC
    {
        public bool initialized = false;
        public static readonly Color TextColor = new(155, 255, 255);
        public static readonly Color TextColorEdgy = new(213, 4, 11);

        bool p2dial = false;
        bool revdial = false;
        public override string Texture => "CalamityMod/NPCs/ExoMechs/Draedon";

        NPC hypnos;
        int hypnosWhoAmI;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Draedon");
            Main.npcFrameCount[NPC.type] = 12;
            this.HideFromBestiary();
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;
            NPC.lifeMax = 2;
            NPC.damage = 0;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.Item14;
            NPC.knockBackResist = 0f;
            NPC.noTileCollide = true;
            NPC.width = NPC.height = 86;
        }

        private void NewText(string text, Color textColor)
        {
            CalamityUtils.BroadcastLocalizedText(text, textColor);
        }

        public int PlayerWhoAmI
        {
            get
            {
                return (int)NPC.ai[3];
            }
        }

        public override void AI()
        {
			NPC.TargetClosest();

			hypnos = Main.npc[hypnosWhoAmI];

			Player player = Main.player[PlayerWhoAmI];



			
            NPC.dontTakeDamage = true;
            int basetime = 120;
            int timemult = 130;
            switch (NPC.ai[0])
            {
                case 0:
                    {
                        Vector2 playerpos = new Vector2(player.Center.X - 100, player.Center.Y - 200);
                        Vector2 distanceFromDestination = playerpos - NPC.Center;
                        CalamityUtils.SmoothMovement(NPC, 100, distanceFromDestination, 30, 1, true);

                        if (NPC.ai[1] == 1)
                        {
                            SoundEngine.PlaySound(CalamityMod.NPCs.ExoMechs.Draedon.TeleportSound);
                        }
                        else if (NPC.ai[1] == basetime)
                        {
                            NewText("Mods.HypnosMod.Draedon.Line1", TextColor);
                        }
                        else if (NPC.ai[1] == basetime + timemult)
                        {
                            NewText("Mods.HypnosMod.Draedon.Line2", TextColor);
                        }
                        else if (NPC.ai[1] == basetime + timemult * 2)
                        {
                            NewText("Mods.HypnosMod.Draedon.Line3", TextColor);
                        }
                        else if (NPC.ai[1] == basetime + timemult * 3)
                        {
                            NewText("Mods.HypnosMod.Draedon.Line4", TextColorEdgy);
                            NPC.ai[2] = 1;
                        }
                        else if (NPC.ai[1] == basetime + timemult * 4)
                        {
                            
                            if (Main.netMode != NetmodeID.Server)
                            {
                                SoundEngine.PlaySound(CalamityMod.Sounds.CommonCalamitySounds.FlareSound with { Volume = CalamityMod.Sounds.CommonCalamitySounds.FlareSound.Volume * 1.55f }, NPC.Center);
                            }
                            int hypy = NPC.NewNPC(NPC.GetSource_FromAI(), (int)player.Center.X, (int)(player.Center.Y - 400), ModContent.NPCType<HypnosBoss>());
                                hypnosWhoAmI = hypy;
								hypnos = Main.npc[hypnosWhoAmI];
                                hypnos.netUpdate = true;
							NPC.ai[1] = 0;
                            NPC.ai[0] = 1;
                        }
                        NPC.ai[1]++;
                    }
                    break;
                case 1:
                    {
                        int offset = hypnos.ai[0] == 9 ? 1100 : 900;
                        Vector2 playerpos = new Vector2(Main.player[NPC.target].Center.X - offset, Main.player[NPC.target].Center.Y);
                        Vector2 distanceFromDestination = playerpos - NPC.Center;
                        CalamityUtils.SmoothMovement(NPC, 100, distanceFromDestination, 30, 1, true);

                        if (hypnos.ai[0] == 6 && !p2dial)
                        {
                            if (NPC.ai[1] == 20)
                            {
                                NewText("Mods.HypnosMod.Draedon.Line5", TextColor);
                            }
                            else if (NPC.ai[1] == timemult)
                            {
                                NewText("Mods.HypnosMod.Draedon.Line6", TextColor);
                                NPC.ai[1] = 0;
                                p2dial = true;
                            }
                            NPC.ai[1]++;
                        }
                        if (hypnos.life <= hypnos.lifeMax * 0.25f && !revdial && CalamityMod.World.CalamityWorld.revenge)
                        {
                            if (NPC.ai[1] == 20)
                            {
                                NewText("Mods.HypnosMod.Draedon.Line7", TextColor);
                            }
                            else if (NPC.ai[1] == timemult)
                            {
                                SoundEngine.PlaySound(CalamityMod.NPCs.ExoMechs.Draedon.LaughSound, NPC.Center);
                                NewText("Mods.HypnosMod.Draedon.Line8", TextColor);
                                NPC.ai[1] = 0;
                                revdial = true;
                            }

                            NPC.ai[1]++;
                        }
                        if (!hypnos.active)
                        {
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 2;
                        }

                    }
                    break;
                case 2:
                    {
                        Vector2 playerpos = new Vector2(Main.player[NPC.target].Center.X - 300, Main.player[NPC.target].Center.Y);
                        Vector2 distanceFromDestination = playerpos - NPC.Center;
                        CalamityUtils.SmoothMovement(NPC, 100, distanceFromDestination, 30, 1, true);
                        NPC.ai[1]++;
                        if (NPC.ai[1] == basetime)
                        {
                            NewText("Mods.HypnosMod.Draedon.Line9", TextColor);
                        }
                        else if (NPC.ai[1] == basetime + timemult)
                        {
                            NewText("Mods.HypnosMod.Draedon.Line10", TextColor);
                        }
                        else if (NPC.ai[1] == basetime + timemult * 2)
                        {
                            NewText("Mods.HypnosMod.Draedon.Line11", TextColor);
                        }
                        else if (NPC.ai[1] == basetime + timemult * 3)
                        {
                            NewText("Mods.HypnosMod.Draedon.Line12", TextColor);
                        }
                        else if (NPC.ai[1] == basetime + timemult * 4)
                        {
                            NewText("Mods.HypnosMod.Draedon.Line13", TextColor);
                        }
                        else if (NPC.ai[1] >= basetime + timemult * 5)
                        {
                            NPC.alpha += 10;
                            if (NPC.alpha >= 255)
                            {
                                NPC.active = false;
                            }
                        }
                    }
                    break;
            }
        }


        public override bool CheckActive()
        {
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((byte)hypnosWhoAmI);

            writer.Write(p2dial);
            writer.Write(revdial);
            writer.Write(initialized);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            hypnosWhoAmI = reader.ReadByte();

            p2dial = reader.ReadBoolean();
            revdial = reader.ReadBoolean();
            initialized = reader.ReadBoolean();
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = 100;

            int xFrame = NPC.frame.X / NPC.frame.Width;
            int yFrame = NPC.frame.Y / frameHeight;
            int frame = xFrame * Main.npcFrameCount[NPC.type] + yFrame;


            int frameChangeDelay = 7;

            NPC.frameCounter++;
            if (NPC.frameCounter >= frameChangeDelay)
            {
                frame++;

                if (frame >= 16)
                    frame = 11;

                NPC.frameCounter = 0;
            }

            NPC.frame.X = frame / Main.npcFrameCount[NPC.type] * NPC.frame.Width;
            NPC.frame.Y = frame % Main.npcFrameCount[NPC.type] * frameHeight;
        }


        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
            Texture2D glowmask = ModContent.Request<Texture2D>("CalamityMod/NPCs/ExoMechs/DraedonGlowmask").Value;
            Rectangle frame = NPC.frame;

            Vector2 drawPosition = NPC.Center - screenPos - Vector2.UnitY * 38f;
            Vector2 origin = frame.Size() * 0.5f;
            Color color = NPC.GetAlpha(drawColor);
            SpriteEffects direction = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(texture, drawPosition, frame, drawColor * NPC.Opacity, NPC.rotation, origin, NPC.scale, direction, 0f);
            spriteBatch.Draw(glowmask, drawPosition, frame, Color.White, NPC.rotation, origin, NPC.scale, direction, 0f);

            return false;
        }
    }
}