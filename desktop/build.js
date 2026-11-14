// node build.js [win32|linux|darwin]: copies the offline build into app/ and packages it with Electron.
// Run `python3 tools/artifact/build.py` in the repo first (it makes ../.artifact-build).
const fs=require('fs'),path=require('path');
const SRC=path.join(__dirname,'..','.artifact-build'),APP=path.join(__dirname,'app');
const platform=process.argv[2]||'win32';
if(!fs.existsSync(path.join(SRC,'ub.html'))){console.error('no ../.artifact-build/ub.html: run python3 tools/artifact/build.py first');process.exit(1)}
fs.rmSync(APP,{recursive:true,force:true});fs.mkdirSync(APP,{recursive:true});
const page=fs.readFileSync(path.join(SRC,'ub.html'),'utf8');
fs.writeFileSync(path.join(APP,'index.html'),`<!doctype html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head><body>${page}</body></html>`);
for(const d of ['pack','a'])fs.cpSync(path.join(SRC,d),path.join(APP,d),{recursive:true});
(async()=>{
 const {packager}=require('@electron/packager');
 const out=await packager({dir:__dirname,out:path.join(__dirname,'dist'),overwrite:true,platform,arch:'x64',asar:true,prune:true,
  name:'UndertaleDeltaruneBattleEngine',ignore:[/^\/dist/,/^\/build\.js$/]});
 console.log('built',out.join(', '));
})().catch(e=>{console.error(e);process.exit(1)});
