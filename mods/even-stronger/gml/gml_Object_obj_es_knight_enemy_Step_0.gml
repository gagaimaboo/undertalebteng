if (global.monster[myself] != 1)
    exit;
global.mercymod[myself] = 0;
if (endcon > 0)
{
    scr_es_knight_endstep();
    exit;
}
// half its HP gone: the fight is over
if (global.monsterhp[myself] <= (global.monstermaxhp[myself] * 0.5))
{
    scr_es_knight_endstart();
    exit;
}
if (scr_isphase("enemytalk") && talked == 0)
{
    scr_randomtarget();
    if (!instance_exists(obj_darkener))
        instance_create(0, 0, obj_darkener);
    talked = 1;
    talktimer = 0;
}
if (talked == 1 && scr_isphase("enemytalk"))
{
    rtimer = 0;
    scr_blconskip(-2);
    if (scr_isphase("bullets"))
    {
        if (!instance_exists(obj_moveheart))
            scr_moveheart();
        if (!instance_exists(obj_growtangle))
            instance_create(camerax() + 320, cameray() + 170, obj_growtangle);
    }
}
if (scr_isphase("bullets") && attacked == 0)
{
    rtimer += 1;
    if (rtimer == 16)
    {
        var dc = instance_create(0, 0, obj_es_knight_bullets);
        dc.type = (kattack < 3) ? kattack : choose(0, 1, 2, 3);
        dc.target = mytarget;
        dc.damage = 26;
        kattack++;
        scr_turntimer(280);
        turns += 1;
        attacked = 1;
    }
}
if (global.myfight == 3)
{
    if (acting == 1 && actcon == 0)
    {
        actcon = 1;
        msgset(0, "* ROARING KNIGHT - AT ?? DF ??&* It doesn't speak^1. It doesn't need to./%");
        scr_battletext_default();
    }
    if (actcon == 1 && !instance_exists(obj_writer))
    {
        scr_act_charsprite_end();
        scr_nextact();
    }
}
