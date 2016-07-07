﻿using System;
using System.Threading.Tasks;
using Windows.UI.StartScreen;
using MoneyFox.Shared.Constants;
using MoneyFox.Shared.Interfaces.Shotcuts;
using MoneyFox.Shared.Resources;

namespace MoneyFox.Windows.Shortcuts
{
    public class IncomeTile : Tile, IIncomeShortcut
    {
        public bool IsShortcutExisting => TileExists(Constants.ADD_INCOME_TILE_ID);

        public async Task CreateShortCut()
        {
            await Create(new SecondaryTile(
                Constants.ADD_INCOME_TILE_ID,
                Strings.AddIncomeLabel,
                Constants.ADD_INCOME_TILE_ID,
                new Uri("ms-appx:///Assets/IncomeTileIcon.png", UriKind.Absolute),
                TileSize.Default));
        }

        public async Task RemoveShortcut()
        {
            if (IsShortcutExisting)
            {
                await Remove(new SecondaryTile(Constants.ADD_INCOME_TILE_ID));
            }
        }
    }
}