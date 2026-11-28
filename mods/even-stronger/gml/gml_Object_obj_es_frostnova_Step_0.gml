t++;
spin += 4 + (t / 10);
if (t == 1)
    snd_pitch(snd_play(snd_icespell), 0.6);
if (t < 40)
    bigscale = min(4, bigscale + 0.12);
// split
if (t == 40)
{
    snd_play(snd_break2);
    snd_pitch(snd_play(snd_icespell), 1.3);
    for (var i = 0; i < 6; i++)
    {
        var d = i * 60;
        array_push(pieces, [cx, cy, lengthdir_x(5, d), lengthdir_y(5, d), d]);
    }
    flash = 1;
}
// pieces drift out, then get thrown down at the enemies
for (var i = 0; i < array_length(pieces); i++)
{
    var p = pieces[i];
    if (t < 58)
    {
        p[0] += p[2];
        p[1] += p[3];
        p[2] *= 0.9;
        p[3] *= 0.9;
    }
    else
    {
        p[3] += 2.2;
        p[1] += p[3];
        p[0] += p[2];
    }
    p[4] += 12;
    pieces[i] = p;
}
if (t == 58)
{
    snd_pitch(snd_play(snd_criticalswing), 0.7);
    for (var i = 0; i < array_length(pieces); i++)
    {
        var p = pieces[i];
        var tgt = i mod 3;
        var tries = 0;
        while (global.monster[tgt] == 0 && tries < 3)
        {
            tgt = (tgt + 1) mod 3;
            tries++;
        }
        p[2] = (global.monsterx[tgt] - p[0]) / 12;
        p[3] = -4;
        pieces[i] = p;
    }
}
if (t == 72)
{
    pieces = [];
    snd_play(snd_snowgrave);
    flash = 1;
}
// the rain
if (t >= 70 && t < 120)
{
    for (var s = 0; s < 3; s++)
    {
        if (global.monster[s] == 1 || t < 76)
        {
            repeat (2)
                array_push(rain, [global.monsterx[s] - 50 + random(100), cameray() - 20 - random(40), 7 + random(5), random(360), 0.5 + random(0.6)]);
        }
    }
}
for (var i = array_length(rain) - 1; i >= 0; i--)
{
    var r = rain[i];
    r[1] += r[2];
    r[3] += 9;
    rain[i] = r;
    if (r[1] > (cameray() + 330))
        array_delete(rain, i, 1);
}
if (t == 90 && hitdone == 0)
{
    hitdone = 1;
    for (var s = 0; s < 3; s++)
    {
        if (global.monster[s] == 1)
        {
            global.hittarget[s] = 0;
            if (damage >= global.monsterhp[s] && i_ex(global.monsterinstance[s]))
            {
                if (global.monsterinstance[s].freezable == 1)
                    global.flag[51 + s] = 6;
                else
                    scr_es_markslashkill(s, damage);
            }
            scr_damage_enemy(s, damage);
            if (global.monster[s] == 1)
            {
                with (global.monsterinstance[s])
                    scr_oflash();
            }
        }
    }
}
if (flash > 0)
    flash -= 0.08;
if (t > 135 && array_length(rain) == 0)
    instance_destroy();
