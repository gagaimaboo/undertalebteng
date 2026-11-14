# C# port (work in progress)

A native rewrite of the battle engine in C# (.NET 8 + [Raylib-cs](https://github.com/ChrisDill/Raylib-cs)).
It is ported from the web build's `NB` engine (UNDERTALE's battle controller, rebuilt from the
game's own objects), function by function, so it plays the same.

## What works

- The UNDERTALE battle: the box and its border walking, the SOUL, FIGHT / ACT / ITEM / MERCY,
  the text writer with UNDERTALE's codes (`&` `^n` `/` `%` `\Y` ...), the knife FIGHT bar and
  its damage maths, damage numbers and HP bar, monster speech bubbles, the enemy's turn,
  bullets and hits, spare (dust), death (vaporize), flee, items, game over, YOU WON
- Monsters: **Froggit**, **Dummy**
- A fight-select menu with LV 1-20
- 16:9 window, the 640x480 picture scaled into the middle; F11 / Alt+Enter fullscreen

## Layout

```
port/
  tools/export_assets.js   pulls sprites, sounds and font tables out of the web build
  UTBattle/
    Program.cs             window, 30 fps fixed step, menu, --autotest mode
    Engine/                Assets (embedded in the exe), Input, Gfx (fonts, sprites)
    Battle/Fight.cs        the battle controller (NB core)
    Battle/Writer.cs       OBJ_WRITER
    Battle/Monster.cs      Monster, Bullet, MonsterDef (what a monster script overrides)
    Battle/Monsters/       one class per monster (Ruins.cs: Froggit, Dummy)
```

Adding a monster: write a `MonsterDef` subclass (the JS `NB.defs.set('id', {...})` entry maps
onto its overrides one to one), register it in `Program.Main`, add its sprite keys to
`tools/export_assets.js` and re-export.

## Build

```
# 1. in the repo root: build the web game's offline bundle (.artifact-build)
python3 tools/artifact/build.py
# 2. export what the port needs into port/UTBattle/assets
cd port/tools && NODE_PATH=$(npm root -g) node export_assets.js
# 3. run, or make a single Windows .exe (assets are built into it)
cd ../UTBattle
dotnet run -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish/win-x64
```

Installer (per user, no admin prompt, Start menu + desktop shortcuts, shows up in Add/Remove Programs):

```
# after the publish step above; needs NSIS (apt install nsis)
cd port/installer && makensis UTBattle.nsi    # -> UTBattle-CSharp-Setup.exe
```

`UTBattle --autotest DIR` plays a scripted Froggit fight and saves screenshots to DIR.

## Next

The rest of the web build, roughly in this order: the other weapons' FIGHT bars, the remaining
UNDERTALE monsters and bosses (one file each), the menus, then the DELTARUNE engine.
