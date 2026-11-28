// Queen's memories during her death scene: a small framed "desktop wallpaper" of Cyber City.
// phase 0 hidden, 1 the lit city, 2 the lights going out (edges first), 3 snow over everything
phase = 0;
a = 0;
t = 0;
t2 = 0;
random_set_seed(2402);
// buildings
bn = 14;
for (var i = 0; i < bn; i++)
{
    bw[i] = 18 + irandom(14);
    bh[i] = 50 + irandom(110);
}
// the citizens: lights on the streets and in windows
ln = 70;
for (var i = 0; i < ln; i++)
{
    lx[i] = 8 + random(384);
    ly[i] = 120 + random(130);
    lc[i] = choose(c_yellow, c_aqua, c_fuchsia, c_lime, c_white);
    lon[i] = 1;
    ld[i] = abs(lx[i] - 200) + abs(ly[i] - 180) * 1.5;
}
// turn-off order: furthest from the middle first
lorder = array_create(ln, 0);
for (var i = 0; i < ln; i++)
    lorder[i] = i;
for (var i = 0; i < ln - 1; i++)
    for (var j = 0; j < ln - 1 - i; j++)
        if (ld[lorder[j]] < ld[lorder[j + 1]])
        {
            var tmp = lorder[j];
            lorder[j] = lorder[j + 1];
            lorder[j + 1] = tmp;
        }
loff = 0;
for (var i = 0; i < 60; i++)
{
    sx[i] = random(400);
    sy[i] = random(260);
    ss[i] = 0.4 + random(1.2);
}
randomize();
