// Exports what the C# port needs from the built offline game: sprites (as PNG files named by
// their key in the JS `images` table), the glyph tables of the bitmap fonts, and the sound
// files (named by their key in SFX). Run: NODE_PATH=$(npm root -g) node export_assets.js
const {chromium}=require('playwright');const http=require('http'),fs=require('fs'),path=require('path');
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
const ROOT=path.resolve(__dirname,'../../.artifact-build'),OUT=path.resolve(__dirname,'../UTBattle/assets');
const IMAGES=['nbBattleBg','target','targetChoice','btn_fight_sel','btn_fight_unsel','btn_act_sel','btn_act_unsel','btn_item_sel','btn_item_unsel','btn_mercy_sel','btn_mercy_unsel',
 'nbHeart','heart','damageDigits','dmgMiss','hpHudSprite','marsHudAtlas','fntMainAtlas','nbPlainAtlas','torielBubbleSmall','torielBubbleWide','nbBulletGen','dustCloud','strike',
 'froggit','froggitHead','froggitLegs','froggitLeap','froggitFly','dummy','dummyDeath','heartbreak','nbHeartBreak','nbShards','gameOverBg','nbHeartGtfo',
 'nbHeartGreen','nbBlconWdShrt','monsterSoul','monsterSoulBreak','monsterSoulShards',
 // Toriel
 'toriel','torielHurt','torielReallyHurt','nbTorSide','nbTorSideSad','nbTorSad','nbTorSadHappy','nbTorSideSad2','nbTorSideSadHappy','nbTorNeutral','nbTorMurdered',
 'torielKneel','nbTorKneelAnguish','nbTorKneelAnguish2','torielKneelSmile','nbTorKneelSmile2','torielFire','torielHand',
 // Undyne
 'nbUndLegs','nbUndLarm','nbUndPants','nbUndRarm','nbUndArmor','nbUndHair','nbUndFace','nbUndSmear','nbUndMelt','nbUndB','nbUndLastMelt',
 'nbUndArrowX','nbUndArrowL','nbUndArrowR','nbUndArrowU','nbUndArrowD','nbUndWhiteSpear','nbUndRiseSpear','nbUndSpearR',
 // Undyne the Undying
 'nbUxLarm','nbUxHair','nbUxLegs','nbUxRarm','nbUxTorso','nbUxPants','nbUxFace1','nbUxFaceDmg','nbUxFaceLaugh','nbUxFaceE','nbUxEyebeam','nbUxSpear2','nbUxMelt'];
// battle music (MUSIC keys), copied out of the bundle as .ogg
const MUSIC_KEYS=['battle','toriel','undyne','undyneex'];
const SOUNDS=['text','txt2','move','select','hurt','damage','laz','vaporized','heal','escaped','break1','break2','asgoreText','power','item',
 'torielText','encounterNoise','txtUnd','txtUnd2','txtUnd3','txtUnd4','undyneSpearAppear','undyneArrow','undyneSpearRise','mdBell'];
(async()=>{
 const page=fs.readFileSync(path.join(ROOT,'ub.html'),'utf8');
 const doc=`<!doctype html><html><head><meta charset="utf-8"></head><body>${page}</body></html>`;
 const srv=http.createServer((q,s)=>{const u=decodeURIComponent(q.url.split('?')[0]);if(u==='/'){s.writeHead(200,{'content-type':'text/html'});return s.end(doc)}
  const f=path.join(ROOT,path.normalize(u));if(!f.startsWith(ROOT)||!fs.existsSync(f)){s.writeHead(404);return s.end()}s.writeHead(200);fs.createReadStream(f).pipe(s)}).listen(0);
 const b=await chromium.launch();const p=await (await b.newContext()).newPage();
 await p.goto(`http://localhost:${srv.address().port}/`);
 for(let i=0;i<240;i++){if(await p.evaluate(()=>typeof screen==='string'&&screen!=='loading'))break;await sleep(250)}
 await p.evaluate(()=>new Promise(r=>withFightAssets(r)));await sleep(1500);
 const out=await p.evaluate(({IMAGES,SOUNDS,MUSIC_KEYS})=>{
  const toPng=im=>{if(!im)return null;const w=im.naturalWidth||im.width,h=im.naturalHeight||im.height;if(!w||!h)return null;const c=document.createElement('canvas');c.width=w;c.height=h;c.getContext('2d').drawImage(im,0,0);return c.toDataURL('image/png')};
  const img={};for(const k of IMAGES){const v=images[k];if(Array.isArray(v))img[k]=v.map(toPng);else if(v)img[k]=toPng(v)}
  const snd={};for(const k of SOUNDS)snd[k]=SFX[k]||null;
  // spr_undynex_melt sits on an opaque black backdrop: key it out for the dust effect
  const keyBlack=im=>{if(!im)return null;const c=document.createElement('canvas');c.width=im.naturalWidth||im.width;c.height=im.naturalHeight||im.height;
   const g=c.getContext('2d');g.drawImage(im,0,0);const d=g.getImageData(0,0,c.width,c.height),a=d.data;for(let i=0;i<a.length;i+=4)if(a[i]<48&&a[i+1]<48&&a[i+2]<48)a[i+3]=0;g.putImageData(d,0,0);return c.toDataURL('image/png')};
  const um=images.nbUxMelt;if(Array.isArray(um))img.nbUxMeltKeyed=um.map(keyBlack);
  const mus={};for(const k of MUSIC_KEYS)mus[k]=MUSIC[k]||null;
  return {img,snd,mus,data:{undText:NB.undText},fonts:{main:UT_FNT_MAIN_GLYPHS,mars:UT_HUD_MARS_GLYPHS,plain:NB_PLAIN_GLYPHS}};
 },{IMAGES,SOUNDS,MUSIC_KEYS});
 await b.close();srv.close();
 fs.mkdirSync(path.join(OUT,'img'),{recursive:true});fs.mkdirSync(path.join(OUT,'snd'),{recursive:true});fs.mkdirSync(path.join(OUT,'fonts'),{recursive:true});
 fs.mkdirSync(path.join(OUT,'mus'),{recursive:true});fs.mkdirSync(path.join(OUT,'data'),{recursive:true});
 const save=(name,d)=>{if(!d)return 0;fs.writeFileSync(path.join(OUT,'img',name+'.png'),Buffer.from(d.split(',')[1],'base64'));return 1};
 let n=0;const missing=[];
 for(const [k,v] of Object.entries(out.img)){if(Array.isArray(v))v.forEach((d,i)=>n+=save(`${k}_${i}`,d));else n+=save(k,v)}
 for(const k of IMAGES)if(!(k in out.img))missing.push(k);
 const pack=JSON.parse(fs.readFileSync(path.join(ROOT,'pack/assets.json'),'utf8'));let ns=0;
 for(const [k,url] of Object.entries(out.snd)){const f=url&&pack.aud[url];if(!f){missing.push('snd:'+k);continue}fs.copyFileSync(path.join(ROOT,f),path.join(OUT,'snd',k+path.extname(f)));ns++}
 let nm=0;for(const [k,url] of Object.entries(out.mus)){const f=url&&pack.aud[url];if(!f){missing.push('mus:'+k);continue}fs.copyFileSync(path.join(ROOT,f),path.join(OUT,'mus',k+path.extname(f)));nm++}
 for(const [k,v] of Object.entries(out.data))fs.writeFileSync(path.join(OUT,'data',k+'.json'),JSON.stringify(v));
 for(const [k,v] of Object.entries(out.fonts))fs.writeFileSync(path.join(OUT,'fonts',k+'.json'),JSON.stringify(v));
 console.log('images',n,'sounds',ns,'music',nm,'missing',missing.join(',')||'none');
})().catch(e=>{console.error(e);process.exit(1)});
