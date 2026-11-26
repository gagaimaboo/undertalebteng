#!/bin/bash
# builds the patcher. Needs .NET 10 SDK and git.
#   ./build.sh            -> patcher/bin/Release/net10.0/ (for the dev loop)
#   ./build.sh publish    -> dist/EvenStrongerPatcher.exe (self-contained, Windows x64)
set -e
cd "$(dirname "$0")"
[ -d .utmt ] || git clone --depth 1 --branch 0.9.2.0 https://github.com/UnderminersTeam/UndertaleModTool .utmt
if [ "$1" = publish ]; then
  dotnet publish patcher/EvenStronger.csproj -c Release -r win-x64 --self-contained true \
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
else
  dotnet build patcher/EvenStronger.csproj -c Release
fi
