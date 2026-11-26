t++;
if (t == 1)
{
    var krs = scr_act_charsprite("kris", spr_krisb_attackready, 0, 0);
    if (i_ex(krs) && i_ex(obj_herokris))
        krs.depth = obj_herokris.depth + 1;
    snd_pitch(snd_play(snd_scytheburst), 0.4);
}
if (t < 40)
    dark = min(0.65, dark + 0.03);
if (t == 40)
{
    flash = 1;
    snd_play(snd_criticalswing);
    instance_create(0, 0, obj_shake);
}
// the storm
if (t > 40 && t < 160)
{
    if ((t mod 2) == 0)
    {
        var s = irandom(2);
        var tries = 0;
        while (global.monster[s] == 0 && tries < 3)
        {
            s = (s + 1) mod 3;
            tries++;
        }
        if (global.monster[s] == 1)
        {
            var att = instance_create((global.monsterx[s] - 50) + random(100), (global.monstery[s] - 50) + random(100), obj_basicattack);
            att.image_xscale = choose(-1, 1) * (1.2 + random(1.6));
            att.image_yscale = 1.2 + random(1.6);
            att.image_angle = random(360);
            global.hittarget[s] = irandom(5);
            scr_damage_enemy(s, irandom_range(40, 99));
        }
        if ((t mod 4) == 0)
        {
            snd_pitch(snd_play(snd_scytheburst), 0.8 + random(0.8));
            scr_act_charsprite_end();
            var krs = scr_act_charsprite("kris", spr_krisb_attack, 1, 1);
            if (i_ex(krs) && i_ex(obj_herokris))
                krs.depth = obj_herokris.depth + 1;
        }
        if ((t mod 10) == 0)
            instance_create(0, 0, obj_shake);
    }
}
// one giant X through all of them
if (t == 170)
{
    flash = 1;
    snd_play(snd_break2);
    snd_pitch(snd_play(snd_scytheburst), 0.5);
    instance_create(0, 0, obj_shake);
    var cx = 0, cy = 0, n = 0;
    for (var s = 0; s < 3; s++)
    {
        if (global.monster[s] == 1)
        {
            cx += global.monsterx[s];
            cy += global.monstery[s];
            n++;
        }
    }
    if (n > 0)
    {
        cx /= n;
        cy /= n;
        var a = instance_create(cx, cy, obj_basicattack);
        a.image_xscale = 7;
        a.image_yscale = 7;
        var b = instance_create(cx, cy, obj_basicattack);
        b.image_xscale = -7;
        b.image_yscale = 7;
    }
    for (var s = 0; s < 3; s++)
    {
        if (global.monster[s] == 1 && i_ex(global.monsterinstance[s]))
        {
            global.monsterhp[s] = 0;
            global.flag[51 + s] = 1;
            global.flag[63] = 1;
            var m = global.monsterinstance[s];
            m.fatal = 1;
            with (m)
                scr_monsterdefeat();
        }
    }
    killed = 1;
}
// anything that didn't go down through its own code goes down now
if (t == 190)
{
    for (var s = 0; s < 3; s++)
    {
        if (i_ex(global.monsterinstance[s]) && global.monsterinstance[s].object_index != obj_deathanim)
        {
            var m = global.monsterinstance[s];
            if (instance_exists(m) && object_is_ancestor(m.object_index, obj_monsterparent))
            {
                var d = instance_create(m.x, m.y, obj_deathanim);
                d.sprite_index = m.sprite_index;
                d.image_xscale = m.image_xscale;
                d.image_yscale = m.image_yscale;
                with (m)
                    instance_destroy();
            }
        }
        global.monster[s] = 0;
    }
}
if (flash > 0)
    flash -= 0.06;
if (t > 190)
    dark = max(0, dark - 0.04);
if (t == 230)
{
    scr_act_charsprite_end();
    global.spelldelay = 10;
    instance_destroy();
}
