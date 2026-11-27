// The rollercoaster's second half: Berdly's car fight, the Roaring Knight, BlackKnife.

// encounter 240 (called from scr_encountersetup, where xx/yy are the view corner)
function scr_es_knight_encounter()
{
    global.monsterinstancetype[0] = obj_knight_enemy;
    global.monstertype[0] = 240;
    global.monstermakex[0] = xx + 425;
    global.monstermakey[0] = yy + 78;
    global.monstertype[1] = 0;
    global.monstertype[2] = 0;
    global.battlemsg[0] = "* The Roaring Knight.";
}

// weapon 23 (called from scr_weaponinfo)
function scr_es_blackknife_info()
{
    weaponnametemp = "BlackKnife";
    weapondesctemp = "The Roaring Knight's sword. Starts#battles at 40% TP. Darkners take 1.5x.";
    wmessage2temp = "...Kris. Where'd you get that.";
    wmessage3temp = "That's... not yours, Kris...";
    wmessage4temp = "(It hums when I hold it...)";
    weaponattemp = 20;
    weapondftemp = 3;
    weaponmagtemp = 0;
    weaponboltstemp = 1;
    weaponstyletemp = "?";
    weapongrazeamttemp = 0;
    weapongrazesizetemp = 0;
    weaponchar1temp = 1;
    weaponchar2temp = 0;
    weaponchar3temp = 0;
    weaponchar4temp = 0;
    weaponicontemp = 1;
    weaponabilityicontemp = 7;
    weaponabilitytemp = "SlayCyber";
    value = 0;
}

// Kris takes the sword and equips it (the old weapon goes to the bag)
function scr_es_get_blackknife()
{
    var old = global.charweapon[1];
    if (old == 23)
        exit;
    global.charweapon[1] = 23;
    if (old != 0)
        scr_weaponget(old);
    scr_weaponinfo_mine();
}

// the knight's stats (its own Chapter 3 setup knows a different party): 3000 HP, so its 50% mark
// is about as much damage as Chapter 3's 80% mark is there
function scr_es_knight_stats()
{
    global.monstername[myself] = "Knight";
    global.monstermaxhp[myself] = 3000;
    global.monsterhp[myself] = 3000;
    global.monsterat[myself] = 22;
    global.monsterdf[myself] = 0;
    global.monsterexp[myself] = 0;
    global.monstergold[myself] = 0;
    global.sparepoint[myself] = 0;
    global.mercymod[myself] = 0;
    global.mercymax[myself] = 100;
    global.canact[myself][0] = 1;
    global.actname[myself][0] = "Check";
    global.actdesc[myself][0] = "Useless#analysis";
}

// ---- tearing a battle down by hand ----
function scr_es_battle_teardown()
{
    instance_activate_object(obj_battlecontroller);
    with (obj_darkener)
        instance_destroy();
    // the knight's own Chapter 3 ending fades to white: hold it, then fade back into the scene
    if (instance_exists(obj_fadeout))
    {
        with (obj_fadeout)
            instance_destroy();
        instance_create(0, 0, obj_es_whitein);
    }
    with (obj_es_kdbc)
        instance_destroy();
    with (obj_es_boomfx)
        instance_destroy();
    scr_es_queen_teardown();
}

function scr_es_coaster_teardown()
{
    with (o_coaster_controller)
        instance_destroy();
    with (o_coaster_hero_back)
        instance_destroy();
    with (o_coaster_hero)
        instance_destroy();
    with (o_coaster_berdly)
        instance_destroy();
    with (o_berdly_hide_intro_markers)
        instance_destroy();
    with (obj_dbulletcontroller)
        instance_destroy();
    scr_es_battle_teardown();
}

// a screen shake that never errors (obj_shake can remove itself if another one is running)
function scr_es_shake(_sx, _sy, _sp)
{
    with (instance_create(0, 0, obj_shake))
    {
        shakex = _sx;
        shakey = _sy;
        shakespeed = _sp;
    }
}
