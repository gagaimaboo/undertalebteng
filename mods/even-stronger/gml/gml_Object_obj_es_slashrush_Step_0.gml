t++;
if (t == 1)
{
    var krs = scr_act_charsprite("kris", spr_krisb_attackready, 0, 0);
    if (i_ex(krs) && i_ex(obj_herokris))
        krs.depth = obj_herokris.depth + 1;
    snd_play(snd_criticalswing);
}
if (t > 8 && t <= (8 + window) && hits < maxhits)
{
    if (button1_p())
    {
        if (global.monster[star] == 0)
        {
            for (var s = 0; s < 3; s++)
            {
                if (global.monster[s] == 1)
                {
                    star = s;
                    break;
                }
            }
        }
        if (global.monster[star] == 1)
        {
            hits++;
            flip = -flip;
            scr_act_charsprite_end();
            var krs = scr_act_charsprite("kris", spr_krisb_attack, 0.5, 1);
            if (i_ex(krs) && i_ex(obj_herokris))
                krs.depth = obj_herokris.depth + 1;
            snd_pitch(snd_play(snd_scytheburst), 0.9 + random(0.5));
            var att = instance_create((global.monsterx[star] - 20) + random(40), (global.monstery[star] - 20) + random(40), obj_basicattack);
            att.image_xscale = 1.6 * flip;
            att.image_yscale = 1.6;
            att.image_angle = random_range(-30, 30);
            var dam = max(1, round(scr_es_xslashdmg(caster, star) * 0.3));
            scr_es_markslashkill(star, dam);
            global.hittarget[star] = hits mod 4;
            scr_damage_enemy(star, dam);
        }
    }
}
if (t > (8 + window) || hits >= maxhits || scr_monsterpop() == 0)
{
    if (!variable_instance_exists(id, "endt"))
        endt = t;
    if (t >= (endt + 20))
    {
        scr_act_charsprite_end();
        global.spelldelay = 15;
        instance_destroy();
    }
}
