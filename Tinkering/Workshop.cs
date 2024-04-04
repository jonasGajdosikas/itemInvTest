using itemInvTest.Weapons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace itemInvTest.Tinkering
{
    public class WorkshopTE : ModTileEntity
    {
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<Workshop>();
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Sync the entire multitile's area.  Modify "width" and "height" to the size of your multitile in tiles
                int width = 3;
                int height = 3;
                NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);

                // Sync the placement of the tile entity with other clients
                // The "type" parameter refers to the tile type which placed the tile entity, so "Type" (the type of the tile entity) needs to be used here instead
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
                return -1;
            }

            // ModTileEntity.Place() handles checking if the entity can be placed, then places it for you
            int placedEntity = Place(i, j);
            return placedEntity;
        }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
            }
        }
    }
    public class Workshop : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Origin = new Point16(1, 1);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<WorkshopTE>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);

            AdjTiles = new int[] { TileID.TinkerersWorkbench };
            AddMapEntry(new Color(16, 160, 16), ModContent.GetInstance<WorkshopItem>().DisplayName);
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<WorkshopTE>().Kill(i, j);
            base.KillMultiTile(i, j, frameX, frameY);
        }
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;

            // Should your tile entity bring up a UI, this line is useful to prevent item slots from misbehaving
            Main.mouseRightRelease = false;

            // The following four (4) if-blocks are recommended to be used if your multitile opens a UI when right clicked:
            if (player.sign > -1)
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
                player.sign = -1;
                Main.editSign = false;
                Main.npcChatText = string.Empty;
            }
            if (Main.editChest)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                Main.editChest = false;
                Main.npcChatText = string.Empty;
            }
            if (player.editedChestName)
            {
                NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
                player.editedChestName = false;
            }
            if (player.talkNPC > -1)
            {
                player.SetTalkNPC(-1);
                Main.npcChatCornerItem = 0;
                Main.npcChatText = string.Empty;
            }

            if (TileUtils.TryGetTileEntityAs(i, j, out WorkshopTE entity))
            {
                




                return true;
            }
            return false;
        }
    }
    public class WorkshopItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.TinkerersWorkshop);
            Item.createTile = ModContent.TileType<Workshop>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.TinkerersWorkshop)
                .Register();
        }
    }
}
