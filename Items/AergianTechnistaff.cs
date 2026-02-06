using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Rarities;
using HypnosMod.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HypnosMod.Items;

public class AergianTechnistaff : ModItem
{
    public override string Texture => "HypnosMod/Sprites/AergianTechnistaff";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        Item.staff[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.damage = 127;
        Item.DamageType = DamageClass.Summon;
        Item.width = 10;
        Item.height = 10;
    Item.useTime = 10;
    Item.useAnimation = 10;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 0;
        Item.rare = ModContent.RarityType<BurnishedAuric>();
        Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
    Item.UseSound = SoundID.Item113;
    Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<AergiaNeuronSummon>();
        Item.shootSpeed = 0f;
        Item.mana = 28;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player)
    {
        Item.noMelee = player.altFunctionUse == 2;
        return null;
    }

    public override void HoldItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
            player.Calamity().rightClickListener = true;

        if (player.Calamity().mouseRight && CanUseItem(player) && player.whoAmI == Main.myPlayer && !Main.mapFullscreen && !Main.blockMouse)
            Item.autoReuse = true;
        else
            Item.autoReuse = true;
    }

    public override void UseAnimation(Player player)
    {
        if (player.altFunctionUse == 2f)
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ProjectileID.None;
            Item.mana = 0;
        }
        else
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shoot = ModContent.ProjectileType<AergiaNeuronSummon>();
            Item.mana = 28;
        }
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        float slots = 0f;
        foreach (Projectile proj in Main.ActiveProjectiles)
        {
            if (proj.owner == player.whoAmI && proj.minionSlots > 0)
                slots += proj.minionSlots;
        }
        if (slots + 1f > player.maxMinions)
            return false;

        if (player.ownedProjectileCounts[ModContent.ProjectileType<AergiaNeuronCore>()] <= 0)
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<AergiaNeuronCore>(), damage, knockback, player.whoAmI);

        int coreIndex = -1;
        int totalNeurons = player.ownedProjectileCounts[type];
        float currentRot = 0f;
        foreach (Projectile proj in Main.ActiveProjectiles)
        {
            if (proj.type == ModContent.ProjectileType<AergiaNeuronCore>() && proj.owner == player.whoAmI)
                coreIndex = proj.whoAmI;
            if (proj.type == ModContent.ProjectileType<AergiaNeuronSummon>() && proj.owner == player.whoAmI)
            {
                proj.ai[2] = totalNeurons + 1;
                currentRot = proj.localAI[0];
            }
        }

        int neuron = Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, coreIndex, totalNeurons, totalNeurons + 1);
        Main.projectile[neuron].localAI[0] = currentRot;
        Main.projectile[neuron].localAI[1] = 1;
        return false;
    }
}
