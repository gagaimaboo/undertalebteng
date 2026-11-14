// Desktop wrapper: serves the offline build (every sprite, sound and font bundled) from a local
// server on 127.0.0.1 and shows it in its own window. No browser or internet needed.
const {app,BrowserWindow,Menu}=require('electron');
const http=require('http'),fs=require('fs'),path=require('path');
const ROOT=path.join(__dirname,'app');
const TYPES={'.html':'text/html; charset=utf-8','.json':'application/json','.js':'text/javascript','.png':'image/png',
 '.ogg':'audio/ogg','.wav':'audio/wav','.mp3':'audio/mpeg','.mp4':'video/mp4','.ttf':'font/ttf','.woff':'font/woff','.woff2':'font/woff2'};
function serve(){
 return new Promise(res=>{
  const srv=http.createServer((q,s)=>{
   let u=decodeURIComponent(q.url.split('?')[0]);if(u==='/')u='/index.html';
   const f=path.join(ROOT,path.normalize(u));
   if(!f.startsWith(ROOT)){s.writeHead(403);return s.end()}
   fs.stat(f,(e,st)=>{
    if(e||!st.isFile()){s.writeHead(404);return s.end()}
    s.writeHead(200,{'content-type':TYPES[path.extname(f).toLowerCase()]||'application/octet-stream','content-length':st.size});
    fs.createReadStream(f).pipe(s);
   });
  }).listen(0,'127.0.0.1',()=>res(srv.address().port));
 });
}
app.whenReady().then(async()=>{
 Menu.setApplicationMenu(null);
 const port=await serve();
 const win=new BrowserWindow({width:1280,height:720,minWidth:640,minHeight:360,backgroundColor:'#000000',
  title:'Undertale and Deltarune Battle Engine',autoHideMenuBar:true,
  webPreferences:{contextIsolation:true,nodeIntegration:false,autoplayPolicy:'no-user-gesture-required'}});
 // F11 / Alt+Enter: fullscreen
 win.webContents.on('before-input-event',(e,i)=>{
  if(i.type==='keyDown'&&(i.key==='F11'||(i.key==='Enter'&&i.alt))){win.setFullScreen(!win.isFullScreen());e.preventDefault()}
 });
 win.loadURL(`http://127.0.0.1:${port}/`);
});
app.on('window-all-closed',()=>app.quit());
