# Prego Wars Asset Editor
This is the official implementation of the Prego Wars asset spec with a nice Winforms editor wrapped around it.

## Features
- Opening Prego Wars' `pw-assets` folder.
- Parsing maps and their containing blocks.
- Parsing props.
- Parsing materials.
- User interface for viewing and editing assets via. GUI controls.
- Create empty maps, new materials, and new props.
- Automatic managing of resource files (models/textures).
- Smart cleanup of unused resource files.
- Smart cleanup of resource files used by a file that's being deleted.
- Author-specific editing:
    - You'll only see your assets.
    - All assets you create will be under your name.
    - Changing your name will reflect in all of your assets and their usages.
- Format validation; the editor will not write an invalid asset.
- Smart refactoring; rename assets and anywhere they're used will be updated.

# Getting Started
1. Compile yourself or grab a pre-compiled binary from the releases page.
2. Unzip everything into a folder.
3. Open `PWAssetEditor.exe`
4. Locate your `pw-assets` folder (generally inside the game's install directory)
5. Choose it in the GUI. The directory will be automatically saved for future use.
6. Choose the name you want to use for your assets. You can always change it later, but make sure it's unique and won't collide with someone else's work!
7. Create some stuff.
    - `CTRL + P` Create a Prop.
    - `CTRL + M` Create a Material.
    - `CTRL + SHIFT + M` Create a Map.
8. Open the map editor in-game and you'll see any new maps you've created and maps you've uploaded to the Workshop.
9. Profit
