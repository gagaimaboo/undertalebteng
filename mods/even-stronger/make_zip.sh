#!/bin/bash
# Builds the standalone "just unzip and play" package from a DELTARUNE install:
# DELTARUNE.exe runs the modded Chapter 2 directly (no launcher). Saves go to
# %LOCALAPPDATA%\DELTARUNE_EVEN_STRONGER, so the real install and its saves are untouched.
#   ./make_zip.sh <DELTARUNE folder> [out.zip]
# Needs a dev build of the patcher (./build.sh) and a CLEAN chapter2_windows/data.win.
set -e
cd "$(dirname "$0")"
DR="$(cd "$1" && pwd)"; OUT="${2:-dist/DT_Even_Stronger.zip}"
STAGE=$(mktemp -d); D="$STAGE/DT_Even_Stronger"; mkdir -p "$D/mus"
cp "$DR/DELTARUNE.exe" "$D/"
cp "$DR/chapter2_windows/audiogroup1.dat" "$DR/chapter2_windows/options.ini" "$D/"
cp -r "$DR/chapter2_windows/lang" "$D/"
cp "$DR/chapter2_windows/"*.ogg "$D/"
[ -f "$DR/chapter2_windows/snd_power" ] && cp "$DR/chapter2_windows/snd_power" "$D/"
SRC="$DR/chapter2_windows/data.win"
[ -f "$DR/chapter2_windows/data.win.even_stronger_backup" ] && SRC="$DR/chapter2_windows/data.win.even_stronger_backup"
patcher/bin/Release/net10.0/EvenStrongerPatcher "$SRC" --mod . --out "$D/data.win" --own-saves --no-pause --assets-from "$DR"
# only the music Chapter 2 actually streams (every name is a string literal in its code)
for f in $(strings -n 5 "$D/data.win" | grep -oE '^[A-Za-z0-9_-]+\.ogg$' | sort -u); do
  [ -f "$DR/mus/$f" ] && cp "$DR/mus/$f" "$D/mus/"
done
cp zip_README.txt "$D/README.txt"
mkdir -p "$(dirname "$OUT")"; rm -f "$OUT"
(cd "$STAGE" && zip -qr -9 - DT_Even_Stronger) > "$OUT"
rm -rf "$STAGE"; ls -la "$OUT"
