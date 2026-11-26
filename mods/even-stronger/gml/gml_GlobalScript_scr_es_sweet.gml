// Even Stronger: Sweet Cap'n Cakes. Runs as obj_ch2_cyber01's Step (its Create and alarm are the game's own).
// They heard Queen's warning, they're the last ones left in the field, and they don't run.

function scr_es_sweet_step()
{
    // Sweet slides in (as in the original)
    if (obj_mainchara.x > (x - 80) && con == -1)
    {
        with (nisesweet)
            x -= 5;
        with (djtable)
            x -= 5;
        if (nisesweet.x <= (camerax() + 520))
            con = 10;
    }
    if (con == 10)
    {
        with (nisesweet)
            x = camerax() + 520;
        floattimer += 0.35;
        desy = targy + sin(floattimer);
        with (nisesweet)
        {
            if (y != other.desy)
                y = lerp(y, other.desy, 0.25);
            sprite_index = spr_sweet_idle;
        }
        var xpos = nisesweet.x - 40;
        var ypos = nisesweet.y + 64;
        with (djtable)
        {
            x = xpos;
            y = ypos;
        }
        if (djtable.x >= 2250)
            con = 20;
    }
    if (con == 20)
    {
        con = 21;
        global.interact = 1;
        global.facing = 1;
        cutscene_master = scr_cutscene_make();
        cutscene_master.save_object[0] = id;
        scr_maincharacters_actors();
        sw = 5;
        sw_actor = instance_create(nisesweet.x, nisesweet.y, obj_actor);
        scr_actor_setup(sw, sw_actor, "sweet");
        sw_actor.sprite_index = spr_sweet_idle;
        c_sel(sw);
        c_autowalk(0);
        nisesweet.visible = 0;
        hg = 6;
        hg_actor = instance_create(2200, cameray() - 80, obj_actor);
        hg_actor.y = cameray() - 800;
        scr_actor_setup(hg, hg_actor, "hatguy");
        hg_actor.sprite_index = spr_hatguy_idle;
        c_sel(hg);
        c_autowalk(0);
        nisekk.x = 2200;
        nisekk.y = cameray() - 400;
        with (nisekk)
            depth = 95000;
    }
    if (con == 21)
    {
        con = 22;
        c_mus2("volume", 0, 30);
        c_soundplay(snd_impact);
        c_shake();
        c_sel(sw);
        c_jump(2156, 175, 40, 10);
        c_wait(8);
        c_soundplay(snd_noise);
        c_sprite(spr_cutscene_cyber01_sweet_splat);
        c_shakeobj();
        c_wait(10);
        c_var_instance(djtable, "sprite_index", spr_cutscene_cyber01_dj_table_hover);
        c_var_lerp_instance(djtable, "y", djtable.y, cameray() - 100, 30);
        c_wait(40);
        c_sprite(spr_sweet_idle);
        c_shakeobj();
        c_imagespeed(0.25);
        c_msgside("bottom");
        scr_miniface_init_sweet();
        c_speaker("no_name");
        c_msgsetloc(0, "\\m1\t\t*D-dead end...!/%", "es_sw_1");
        c_talk_wait();
        c_sel(sw);
        c_facing("l");
        c_msgsetloc(0, "\\m1\t\t*Y-you're them!^1! The Human&\tand the Girl!/", "es_sw_2");
        c_msgnextloc("\\m1\t\t*Queen put you on every&\tscreen in the city!/%", "es_sw_3");
        c_talk_wait();
        c_var_instance(hg_actor, "y", cameray() - 80);
        c_var_instance(nisekk, "y", cameray() - 180);
        c_wait(20);
        c_msgsetloc(0, "\\m3\t\t*\"Do not engage.\"&\tHeh. Funny word^1, engage./%", "es_sw_4");
        c_talk_wait();
        c_msgsetloc(0, "\\m2\t\t*.../", "es_sw_5");
        c_msgnextloc("\\m2\t\t*Where's everyone else?/%", "es_sw_6");
        c_talk_wait();
        c_wait(30);
        c_speaker("noelle");
        c_msgside("top");
        c_msgsetloc(0, "\\E7* ..^1. gone./%", "es_sw_7");
        c_talk_wait();
        c_wait(30);
        c_msgside("bottom");
        scr_miniface_init_sweet();
        c_speaker("no_name");
        c_msgsetloc(0, "\\m1\t\t*...gone?/%", "es_sw_8");
        c_talk_wait();
        c_msgsetloc(0, "\\m1\t\t*Alright^1. Fine^1. FINE!/", "es_sw_9");
        c_msgnextloc("\\m1\t\t*We're the last resistance&\tin this field^1, and we're&\tNOT running!/", "es_sw_10");
        c_msgnextloc("\\m1\t\t*K_K^1! Battle music!!/%", "es_sw_11");
        c_talk_wait();
        c_msgsetloc(0, "\\m2\t\t*...OK./%", "es_sw_12");
        c_talk_wait();
        c_var_instance(id, "con", 23);
    }
    // the fight
    if (con == 23 && !instance_exists(obj_cutscene_master))
    {
        with (obj_actor)
            visible = 0;
        con = 29;
        alarm[0] = 30;
        global.es_sweet_turns = 0;
        global.es_snapped = 0;
        var sweet_marker = scr_dark_marker(sw_actor.x, sw_actor.y, sw_actor.sprite_index);
        var hg_marker = scr_dark_marker(hg_actor.x, hg_actor.y, hg_actor.sprite_index);
        var kk_marker = instance_create(x, y, obj_marker);
        kk_marker_fake = scr_dark_marker((room_width - 442) + 92, room_height - 176 - 109, nisekk.sprite_index);
        nisekk.visible = false;
        encounterflag = 552;
        global.flag[54] = encounterflag;
        obj_mainchara.cutscene = 1;
        obj_mainchara.x = kr_actor.x;
        obj_mainchara.y = kr_actor.y;
        if (i_ex(global.cinstance[0]))
        {
            global.cinstance[0].x = no_actor.x;
            global.cinstance[0].y = no_actor.y;
        }
        global.flag[9] = 2;
        global.batmusic[0] = -1;
        snd_init("spamton_neo_mix_ex_wip.ogg");
        scr_battle(62, 1, sweet_marker, kk_marker, hg_marker);
        with (kk_marker_fake)
        {
            scr_move_to_point_over_time(camerax() + 520, cameray() + 110, 20);
            __doom = instance_create(x, y, obj_doom);
            __doom.target = id;
            __doom.alarm[0] = 25;
        }
    }
    // after the snap: everything's gone
    if (con == 30 && !i_ex(obj_battlecontroller) && global.interact == 0)
    {
        con = 35;
        global.interact = 1;
        nisekk.visible = false;
        with (obj_marker)
            instance_destroy();
        cutscene_master = scr_cutscene_make();
        scr_maincharacters_actors();
        c_mus("free_all");
        c_wait(70);
        c_sel(no);
        c_facing("r");
        c_speaker("noelle");
        c_msgsetloc(0, "\\E2* K..^1. Kris...?/%", "es_sw_20");
        c_talk_wait();
        c_wait(45);
        c_sel(no);
        c_facing("d");
        c_msgsetloc(0, "\\E7* That wasn't..^1. that wasn't you./", "es_sw_21");
        c_msgnextloc("\\E7* Was it?/%", "es_sw_22");
        c_talk_wait();
        c_wait(60);
        c_msgsetloc(0, "\\E6* ...The music stopped./", "es_sw_23");
        c_msgnextloc("\\E1* Everything here stopped./%", "es_sw_24");
        c_talk_wait();
        c_wait(30);
        c_mus2("initloop", "spamton_neo_meeting.ogg", 0);
        c_wait(60);
        c_sel(no);
        c_facing("r");
        c_msgsetloc(0, "\\E1* ...Let's go./%", "es_sw_25");
        c_talk_wait();
        c_actortokris();
        c_actortocaterpillar();
        c_terminatekillactors();
        con = 50;
    }
    if (con == 50 && !i_ex(obj_cutscene_master))
    {
        con = 0;
        global.interact = 0;
        global.encounterno = 52;
        global.facing = 0;
        global.plot = 60;
        global.flag[34] = 0;
    }
}

// Kris snaps (called when he acts in the Sweet Cap'n Cakes fight after three of their turns)
function scr_es_try_snap()
{
    if (!instance_exists(obj_sweet_enemy) || !variable_global_exists("es_sweet_turns"))
        return 0;
    if (global.es_sweet_turns < 3 || global.es_snapped == 1)
        return 0;
    global.es_snapped = 1;
    instance_create(0, 0, obj_es_snap);
    global.spelldelay = 99999;
    return 1;
}
