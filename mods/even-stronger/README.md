# DT Chapter 2: Even Stronger

A DELTARUNE Chapter 2 mod: the weird route, but the whole Cyber World, and Noelle is with you from the start.

It ships as a **patcher**, not a modded `data.win`. It edits your own copy of Chapter 2 by matching code
*content* (not byte offsets), so it works across game versions. Tested on DELTARUNE 1.49.

## Install

Two ways:

**A. Standalone zip** (`DT_Even_Stronger_vX.zip`, built with `./make_zip.sh <DELTARUNE folder>`): unzip anywhere and run
`DELTARUNE.exe`. It boots straight into the modded Chapter 2 (no launcher). It saves to
`%LOCALAPPDATA%\DELTARUNE_EVEN_STRONGER`, so your real install and saves are untouched. The zip holds game files,
so it's for your own use; it's never committed here.

**B. Patcher**, which modifies your installed game:

1. Put `EvenStrongerPatcher.exe` in your DELTARUNE folder (next to `DELTARUNE.exe`) and run it.
   (Or drag the DELTARUNE folder onto the exe.)
2. Start Chapter 2 on a **new file**.

The patcher backs up the original as `chapter2_windows/data.win.even_stronger_backup`. Running it again re-patches from
that clean backup. To uninstall run `EvenStrongerPatcher.exe --restore`.

If a game update moves something the mod hooks into, the patcher still finishes. It prints a warning for each hook
it couldn't place, so send that list over.

## What's in v0.1

- **Opening:** no Susie. A cold open on black, then the library:
  - Noelle doesn't notice you at first.
  - Susie texted her "lol". Berdly has a "prior engagement".
  - The project needs a local legend. She almost says the forest, then takes it back.
  - Her dad is still in the hospital. She asks if that's weird, and you choose your answer.
  - Then the computer lab door opens, and you both fall into the Cyber World.
- **Queen's entrance** (the vanilla scene-9 room, rewritten for Noelle):
  - Queen comes to look at the two unregistered Lightners. She finds "something very cold" in Noelle's code and wants her as a peon.
  - Kris steps in, and she wires the two scared locals into Werewires.
  - **After the fight:** if you killed them, Queen comes back to a field that's "Simply: Not There Anymore" and warns the whole city ("Do Not Engage... And Raise Your Guard"). Noelle talks about the ice, or about your sword.
  - If nobody died, she leaves bored.
- **Warned:** every enemy shows "(Warned)" next to its name, the label from Ralsei's Warn act in Chapter 1.
- **FIGHT:** Kris always MISSes. Noelle's FIGHT barely scratches and can never land the kill.
- **Abilities:**
  - **X-Slash** (Kris, 25% TP): Spamton NEO's X-Slash, on every enemy.
  - **Slashrush** (Kris, 80% TP): mash Z, every press is a slash.
  - **IceShock** (Noelle): unchanged.
  - **FrostNova** (Noelle, 60% TP): a giant snowflake forms, splits, gets thrown down and rains on every enemy.
- **Kills are real:**
  - Ice kills freeze, using the game's own frozen enemies.
  - Slash kills use `obj_deathanim`, the same death animation Chapter 4 uses for Titan Spawns.
  - Every kill feeds the real "Noelle became stronger" level-up.
- **Enemies:** +20% HP, and their attacks run 5% faster.
- **Pain:** the first ability you ever use asks first.
- **Battle text:** the box text in every battle is only "Hold.", "Its burning.." or "You are filled with power." (a new one each turn). Action results and victory text are unchanged.
- **Battle theme:** regular encounters play **"SWORD (FM Cover)" by plrusek** instead of the normal battle music. Bosses keep theirs.

## Credits

- Encounter battle theme: "SWORD (FM Cover)" by **plrusek**. It's a cover of SWORD from DELTARUNE Chapter 3.

## Building

Needs the .NET 10 SDK and git. `./build.sh` clones UndertaleModTool 0.9.2.0 into `.utmt/`.

```
./build.sh            # dev build -> patcher/bin/Release/net10.0/
./build.sh publish    # dist/EvenStrongerPatcher.exe (self-contained, Windows x64)
```

Layout:

| Path | What it is |
|---|---|
| `gml/` | New scripts and objects, named like UTMT code entries (`gml_Object_obj_x_Step_0.gml`) |
| `patches/*.es` | Anchored edits to the game's own code (`@@ entry` / `@find` / `@after\|@before\|@replace\|@prepend\|@append` / `@end`). Indentation and blank lines are ignored when matching. |
| `music/` | The mod's own music. It ships inside the exe and is written to `chapter2_windows/even_stronger/`. `snd_init` swaps it in whenever an encounter asks for `battle.ogg`. |
| `objects.txt` | Object flags (persistent, depth) |
| `assets.txt` | Sprites, sounds and music borrowed from Chapters 3 and 4, exported from the player's own install at patch time. Nothing is borrowed yet. |
| `patches/test_harness.es` + `gml/*_es_test.gml` | Dev-only test harness, compiled only with `--test` |

Dev loop: `EvenStrongerPatcher <clean data.win> --mod . --out <test data.win> --test`. The test build reads
`es_test.txt` from the save folder and plays it back: key presses, waits, warps, battles and in-game screenshots.
