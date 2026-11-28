t++;

if (t == 1)
{
    snd_pitch(snd_play(snd_scytheburst), 0.35);
    kpos = -1;
}
if (t <= 24)
    red = t / 24;
// the storm: Kris blinks to an enemy every few frames and cuts through it
if (t > 30 && t < 150 && (t mod 6) == 0)
{
    var s = irandom(2);
    var tries = 0;
    while (global.monster[s] == 0 && tries < 3)
    {
        s = (s + 1) mod 3;
        tries++;
    }
    kpos = s;
    var cx = global.monsterx[s];
    var cy = global.monstery[s];
    var a = random(180);
    var r = 70 + random(40);
    array_push(slashes, [cx + lengthdir_x(r, a), cy + lengthdir_y(r, a), cx - lengthdir_x(r, a), cy - lengthdir_y(r, a), 10]);
    if ((t mod 12) == 0)
        snd_pitch(snd_play(snd_scytheburst), 0.9 + random(0.6));
    if ((t mod 18) == 0)
        instance_create(0, 0, obj_shake);
}
// stillness
if (t == 150)
    kpos = -1;
// the X: they die, and the fight ends right there
if (t == 175)
{
    xslash = 1;
    snd_play(snd_break2);
    snd_pitch(snd_play(snd_scytheburst), 0.5);
    instance_create(0, 0, obj_shake);
    for (var s = 0; s < 3; s++)
    {
        if (global.monster[s] == 1)
        {
            global.monsterhp[s] = 0;
            global.flag[51 + s] = 1;
            global.flag[2452] += 1;
            global.monster[s] = 0;
        }
    }
    global.flag[63] = 1;
    killed = 1;
    scr_es_sweet_teardown();
}
for (var i = array_length(slashes) - 1; i >= 0; i--)
{
    var sl = slashes[i];
    sl[4] -= 1;
    slashes[i] = sl;
    if (sl[4] <= 0)
        array_delete(slashes, i, 1);
}
if (xslash > 0 && t > 185)
    xslash = max(0, xslash - 0.03);
// the red fades over the empty room
if (t > 180)
    red = max(0, red - 0.03);
if (t >= 215)
    instance_destroy();
