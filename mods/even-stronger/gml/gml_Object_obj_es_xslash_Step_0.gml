t++;
if (t == 1 || t == 15)
{
    if (t == 15)
        scr_act_charsprite_end();
    var krs = scr_act_charsprite("kris", spr_krisb_attack, 0.25, 1);
    if (i_ex(krs) && i_ex(obj_herokris))
        krs.depth = obj_herokris.depth + 1;
    snd_pitch(snd_play(snd_scytheburst), (t == 1) ? 1.2 : 0.8);
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
        var att = instance_create(global.monsterx[star], global.monstery[star], obj_basicattack);
        att.image_xscale = (t == 1) ? 2 : -2;
        att.image_yscale = 2;
        var me = caster;
        var dam = scr_es_xslashdmg(me, star);
        scr_es_markslashkill(star, dam);
        global.hittarget[star] = (t == 1) ? 0 : 1;
        scr_damage_enemy(star, dam);
    }
}
if (t == 30)
{
    scr_act_charsprite_end();
    instance_destroy();
}
