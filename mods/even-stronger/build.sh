#!/bin/bash
# builds the patcher. Needs .NET 10 SDK and git.
#   ./build.sh            -> patcher/bin/Release/net10.0/ (for the dev loop)
#   ./build.sh publish    -> dist/EvenStrongerPatcher.exe (self-contained, Windows x64)
set -e
cd "$(dirname "$0")"
[ -d .utmt ] || git clone --depth 1 --branch 0.9.2.0 https://github.com/UnderminersTeam/UndertaleModTool .utmt
if [ "$1" = publish ]; then
  dotnet publish patcher/EvenStronger.csproj -c Release -r win-x64 --self-contained true \
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true -p:DebugType=none -o dist
  # loose files the single-file publish leaves behind (GameSpecificData is embedded in the exe)
  rm -f dist/*.pdb dist/*.xml dist/deltarune.json dist/gamemaker.json dist/template.json dist/undertale.json
else
  dotnet build patcher/EvenStronger.csproj -c Release
fi
