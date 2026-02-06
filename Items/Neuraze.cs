using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Collections.Generic;
using CalamityMod.Rarities;

namespace HypnosMod.Items
{
    public class Neuraze : ModItem
    {
    public override string Texture => "HypnosMod/Sprites/Neuraze";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.useTime = 3;
            Item.useAnimation = 3;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item41;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 125;
            Item.knockBack = 8f;
            Item.noMelee = true;
            Item.crit = 4;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 26f;
            Item.useAmmo = AmmoID.Bullet;
        }
        public override bool CanConsumeAmmo(Item ammo, Player player) => Main.rand.NextFloat() >= 0.80f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            List<Item> ammoTypes = ChooseMultipleAmmo(player, Item);
            if (ammoTypes.Count == 0)
                return false;
            int missingTypes = 5 - ammoTypes.Count;
            int originalAmmoCount = ammoTypes.Count;
            if (missingTypes > 0)
            {
                for (int i = 0; i < missingTypes; i++)
                {
                    if (originalAmmoCount > 1)
                        ammoTypes.Add(ammoTypes[Main.rand.Next(0, originalAmmoCount)]);
                    else
                        ammoTypes.Add(ammoTypes[0]);
                }
            }
            int n = ammoTypes.Count;
            while (n > 1)
            {
                n--;
                int k = Main.rand.Next(n + 1);
                Item value = ammoTypes[k];
                ammoTypes[k] = ammoTypes[n];
                ammoTypes[n] = value;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 newVel = velocity.RotatedBy(MathHelper.Lerp(-MathHelper.PiOver4 / 8, MathHelper.PiOver4 / 8, (i) / 4f));
                Projectile.NewProjectile(source, position + velocity * 4, newVel, ammoTypes[i].shoot, damage, knockback, player.whoAmI);
            }
            return false;
        }
        
        public static List<Item> ChooseMultipleAmmo(Player p, Item weapon)
        {
            List<Item> itemTypes = new List<Item>() { };
            bool flag = false;
            for (int j = 54; j < 58; j++)
            {
                if (p.inventory[j].stack > 0 && ItemLoader.CanChooseAmmo(weapon, p.inventory[j], p))
                {
                    itemTypes.Add(p.inventory[j]);
                    if (itemTypes.Count >= 5)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                for (int k = 0; k < 54; k++)
                {
                    if (p.inventory[k].stack > 0 && ItemLoader.CanChooseAmmo(weapon, p.inventory[k], p))
                    {
                        itemTypes.Add(p.inventory[k]);
                        if (itemTypes.Count >= 5)
                            break;
                    }
                }
            }
            return itemTypes;
        }
    }
}
