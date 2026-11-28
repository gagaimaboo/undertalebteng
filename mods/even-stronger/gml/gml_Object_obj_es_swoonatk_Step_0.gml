t++;
if (t == 1)
{
    if (global.monster[star] == 0)
        for (var s = 0; s < 3; s++)
            if (global.monster[s] == 1)
            {
                star = s;
                break;
            }
    var krs = scr_act_charsprite("kris", spr_krisb_attack, 0.25, 1);
    if (i_ex(krs) && i_ex(obj_herokris))
        krs.depth = obj_herokris.depth + 1;
    var fx = instance_create(global.monsterx[star], global.monstery[star], obj_es_boomfx);
    fx.ang = [0];
    fx.grow = 0;
    fx.sc = 2;
    fx.life = 45;
    fx.depth = depth - 1;
    scr_es_snd("es_snd_knight_cut2", 1, 1);
}
// the five cuts (Chapter 3 plays them 0.06 to 0.24s apart)
if (t == 3 || t == 5 || t == 7 || t == 9 || t == 13)
    scr_es_snd("es_snd_knight_cut2", 0.9, 1);
if (t == 46)
{
    // the big hit: Chapter 3's swoon shake and sound stack, SWOON instead of a number
    scr_es_shake(14, 14, 1);
    scr_es_sndx(snd_impact, 1, 1);
    scr_es_sndx(snd_closet_impact, 1, 1);
    scr_es_sndx(snd_closet_impact, 1, 0.5);
    scr_es_sndx(snd_bageldefeat, 0.8, 0.8);
    scr_es_sndx(snd_damage, 1, 1);
    scr_es_sndx(snd_glassbreak, 0.8, 0.4);
    if (global.monster[star] == 1)
    {
        var dam = scr_es_xslashdmg(caster, star) * 2;
        scr_es_markslashkill(star, dam);
        global.hittarget[star] = 0;
        var maxid = -1;
        with (obj_dmgwriter)
            maxid = max(maxid, id);
        scr_damage_enemy(star, dam);
        with (obj_dmgwriter)
            if (id > maxid)
                instance_destroy();
        instance_create(global.monsterx[star] - 40, global.monstery[star] - 20, obj_es_swoon);
    }
}
if (t == 80)
{
    scr_act_charsprite_end();
    instance_destroy();
}
