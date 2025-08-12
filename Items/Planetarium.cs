using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using HypnosMod.Projectiles;
using Terraria.Audio;

namespace HypnosMod.Items
{
    // The Planetarium: a wand dropped by XP-00 Hypnos
    public class Planetarium : ModItem
    {
        public override string Texture => "HypnosMod/Sprites/Planetarium";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            DisplayName.SetDefault("The Planetarium");
            Tooltip.SetDefault("Fires astral lasers upward that return from above to smite foes");
            Item.staff[Item.type] = true;
            // Allow holding right-click to repeatedly use the alternate function
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

    // Enable right-click alternate use
    public override bool AltFunctionUse(Player player) => true;

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 28;
            Item.useTime = 28;
            Item.reuseDelay = 0;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 5;
            Item.damage = 300;
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red;
            // Restore default sound so cadence matches vanilla play timing
            Item.UseSound = SoundID.Item72;
            Item.shoot = ModContent.ProjectileType<PlanetariumLaser>();
            Item.shootSpeed = 16f; // upward speed base
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Always fire straight up from player center; ignore cursor direction
            const float spreadDeg = 2f;
            float rot = MathHelper.ToRadians(Main.rand.NextFloat(-spreadDeg, spreadDeg));
            Vector2 up = Vector2.UnitY * -1f;
            up = up.RotatedBy(rot) * Item.shootSpeed;
            // Pass ai0 = 1 when right-click is used to force mouse-tracking behavior in the projectile
            float ai0 = player.altFunctionUse == 2 ? 1f : 0f;
            Projectile.NewProjectile(source, player.Center, up, type, damage, knockback, player.whoAmI, ai0);
            return false; // we spawn manually
        }
    }
}
