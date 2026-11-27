// The rollercoaster's second half: Berdly's car fight, the Roaring Knight, BlackKnife.

// encounter 240 (called from scr_encountersetup, where xx/yy are the view corner)
function scr_es_knight_encounter()
{
    global.monsterinstancetype[0] = obj_es_knight_enemy;
    global.monstertype[0] = 240;
    global.monstermakex[0] = xx + 390;
    global.monstermakey[0] = yy + 60;
    global.monstertype[1] = 0;
    global.monstertype[2] = 0;
    global.battlemsg[0] = "* The Roaring Knight.";
}

// weapon 23 (called from scr_weaponinfo)
function scr_es_blackknife_info()
{
    weaponnametemp = "BlackKnife";
    weapondesctemp = "The Roaring Knight's sword.#It's still cold.";
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
    weaponabilityicontemp = 0;
    weaponabilitytemp = " ";
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

// ---- the knight's ending (runs in obj_es_knight_enemy) ----
function scr_es_knight_endstart()
{
    endcon = 1;
    endtimer = 0;
    global.monsterhp[myself] = ceil(global.monstermaxhp[myself] * 0.5);
    instance_deactivate_object(obj_battlecontroller);
    with (obj_es_knight_bullets)
        instance_destroy();
    with (obj_es_kbullet)
        instance_destroy();
    if (variable_global_exists("batmusic") && array_length(global.batmusic) > 1)
        snd_volume(global.batmusic[1], 0, 30);
    scr_es_shake(30, 8, 2);
    scr_es_snd("es_snd_knight_hurt", 0.8, 1);
    scr_es_snd("es_snd_knight_hurt", 0.8, 0.7);
    scr_es_snd("es_snd_knight_hurt", 0.8, 1.3);
}

function scr_es_knight_endstep()
{
    endtimer++;
    // Kris doesn't let it leave
    if (endtimer == 100)
    {
        with (obj_herokris)
        {
            sprite_index = spr_krisb_attack;
            image_index = 0;
            image_speed = 0.5;
        }
        snd_play(snd_criticalswing);
        var fx = instance_create(x + 110, y + 90, obj_es_boomfx);
        fx.ang = [45, -45];
        fx.sc = 3;
    }
    if (endtimer == 106)
    {
        var dm = instance_create(global.monsterx[myself], global.monstery[myself] - 20, obj_dmgwriter);
        dm.damage = global.monsterhp[myself];
        global.monsterhp[myself] = 0;
        scr_es_snd("es_snd_knight_cut2", 1, 0.8);
        scr_es_snd("es_snd_knight_cut2", 1, 1);
        scr_es_snd("es_snd_knight_cut2", 1, 1.2);
        scr_es_shake(20, 20, 2);
    }
    if (endtimer == 120)
    {
        endcon = 2;
        var d = instance_create(x - 14, y - 46, obj_deathanim);
        d.sprite_index = idlesprite;
        d.image_index = 0;
        d.image_xscale = 2;
        d.image_yscale = 2;
        d.depth = depth;
        global.flag[2452] += 1;
    }
    if (endtimer == 240)
    {
        global.es_kfight = 0;
        global.es_kdone = 1;
        scr_es_battle_teardown();
    }
}

function scr_es_knight_enddraw()
{
    if (endcon != 1)
        exit;
    var sx = (endtimer < 100) ? (random_range(-1, 1) * 10 * (1 - (endtimer / 110))) : 0;
    // the Chapter 3 ending: idle and the curled-up ball, every few frames
    if ((endtimer mod 3) == 0)
        draw_sprite_ext(idlesprite, 0, x + sx, y, 2, 2, 0, image_blend, 1);
    else
        draw_sprite_ext(scr_es_spr("es_rk_balltrans"), 7, x + sx, y, 2, 2, 0, image_blend, 1);
}

// ---- tearing a battle down by hand ----
function scr_es_battle_teardown()
{
    instance_activate_object(obj_battlecontroller);
    with (obj_darkener)
        instance_destroy();
    with (obj_es_knight_bullets)
        instance_destroy();
    with (obj_es_kbullet)
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
