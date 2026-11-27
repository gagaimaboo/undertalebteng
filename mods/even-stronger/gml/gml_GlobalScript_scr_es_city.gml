// after the Cyber Field: the rollercoaster (scene11a), the trash landing (scene11b) and the
// city split (scene12), rewritten for Kris + Noelle. Queen has seen the field by now.

function scr_es_noelle_rsprite()
{
    return scr_es_noelle_changed() ? spr_noelle_walk_right_sad_dw : spr_noelle_walk_right_dw;
}

// ---------------------------------------------------------------- rollercoaster
// runs instead of obj_ch2_scene11a's Step (its Create still builds the coasters and the track)
function scr_es_scene11a_step()
{
    if (!variable_instance_exists(id, "es_init"))
    {
        es_init = 1;
        // nobody else rides today: Kris and Noelle take the first two cars
        coaster_ralsei.x = -2000;
        coaster_berdly.x = -2000;
        coaster_berdly.visible = 0;
        coaster_noelle.x = -2000;
        coaster_noelle.visible = 0;
        nise_berdly.visible = 0;
        with (obj_ch2_capsule)
            instance_destroy();
    }
    // the second car carries Noelle (after its own init picked Susie's sprite)
    if (i_ex(coaster_susie) && coaster_susie.init)
    {
        coaster_susie.character_sprite = scr_es_noelle_rsprite();
        coaster_susie.character_offset_y = 14;
        coaster_susie.character_offset_x = 4;
        if (variable_instance_exists(id, "es_shock") && es_shock)
            coaster_susie.character_sprite = spr_noelle_shocked_dw;
    }
    if (con == -1 && obj_mainchara.x > 400 && obj_mainchara.y < 300)
    {
        con = 1;
        es_shock = 0;
        global.interact = 1;
        global.facing = 1;
        cutscene_master = scr_cutscene_make();
        cutscene_master.save_object[0] = id;
        scr_maincharacters_actors();
        qu = actor_count + 1;
        qu_actor = instance_create(camerax() + 700, 120, obj_actor);
        scr_actor_setup(qu, qu_actor, "queen");
        qu_actor.sprite_index = spr_queen_chair_2;
        c_sel(qu);
        c_autowalk(0);
        c_imagespeed(0.25);
        c_visible(0);
        qu_flame = actor_count + 2;
        qu_flame_actor = scr_dark_marker(camerax() + 700, 120, spr_queen_chair_flame_side);
        scr_actor_setup_nofacing(qu_flame, qu_flame_actor, "qu_flame");
        c_sel(qu_flame);
        c_imagespeed(0.25);
        c_stickto(qu_actor, 100);
        c_visible(0);

        // board
        c_sel(kr);
        c_walkdirect(478, kr_actor.y - 40, 15);
        c_delaycmd(16, "visible", 0);
        if (variable_instance_exists(id, "no"))
        {
            c_sel(no);
            c_walkdirect(478, kr_actor.y, 15);
            c_delaywalk(16, "u", 4, 20);
            c_delaycmd(37, "visible", 0);
        }
        c_wait(40);
        c_var_instance(coaster_entry, "depth", 5010);
        c_var_instance(coaster_entry_hood, "depth", 5000);
        c_var_instance(coaster_kris, "visible", 1);
        c_var_instance(coaster_susie, "visible", 1);
        c_wait(10);
        c_pannable(1);
        c_pan(650, 0, 120);
        c_wait(60);
        c_speaker("noelle");
        c_msgsetloc(0, "\\E6* ...a rollercoaster./", "es_rc_1");
        c_msgnextloc("\\E1* It goes toward that big fountain^1, Kris. The one leaking darkness./", "es_rc_2");
        c_msgnextloc("\\E0* ...nobody's in line./%", "es_rc_3");
        c_talk_wait();
        c_wait(30);

        // ride
        c_panspeed(15, 0, 30);
        c_var_lerp_instance(coaster_kris, "x", coaster_kris.x, 1220, 20);
        c_var_lerp_instance(coaster_susie, "x", coaster_susie.x, 1220, 20);
        c_wait(21);
        c_var_instance(coaster_kris, "siner_add0", 0.1);
        c_var_instance(coaster_susie, "siner_add0", 0.08);
        c_wait(5);
        c_var_instance(id, "looptrack", true);
        c_var_instance(id, "loopcity", true);
        c_wait(60);

        // Queen catches up. No capsules.
        c_sel(qu);
        c_setxy(1790, 90);
        c_sel(qu_flame);
        c_visible(1);
        c_sel(qu);
        c_visible(1);
        c_autowalk(0);
        c_walkdirect_wait(1585, 90, 20);
        c_wait(20);
        c_msgside("bottom");
        c_speaker("queen");
        c_msgsetloc(0, "\\E6* Stop^1. No^1. Keep Going^1. But Listen/", "es_rc_4");
        c_msgnextloc("\\E1* I Had A Whole Plan For This Ride/", "es_rc_5");
        c_msgnextloc("\\E7* Capturing Capsules^1. Dramatic Lighting^1. A Plot Twist/", "es_rc_6");
        c_msgnextloc("\\ED* I Cancelled All Of It/%", "es_rc_7");
        c_talk_wait();
        c_speaker("noelle");
        c_msgsetloc(0, "\\E6* ...why?/%", "es_rc_8");
        c_talk_wait();
        c_speaker("queen");
        c_msgsetloc(0, "\\E1* My Field Is Empty/", "es_rc_9");
        c_msgnextloc("\\E9* Sweet Is Not Answering His Phone/", "es_rc_10");
        c_msgnextloc("\\E6* My Capsules Would Only Become Two More Things You Broke/", "es_rc_11");
        c_msgnextloc("\\E1* So Here Is The New Plan/", "es_rc_12");
        c_msgnextloc("\\ED* You Ride^1. Very Fast^1. Far Away From My Citizens/%", "es_rc_13");
        c_talk_wait();
        c_speaker("noelle");
        if (scr_es_noelle_changed())
        {
            c_msgsetloc(0, "\\E1* ...she's scared of us^1, Kris./", "es_rc_14");
            c_msgnextloc("\\E0* ...good./%", "es_rc_15");
        }
        else
        {
            c_msgsetloc(0, "\\E1* ...she's scared of us^1, Kris./", "es_rc_16");
            c_msgnextloc("\\E6* I don't..^1. like that./%", "es_rc_17");
        }
        c_talk_wait();
        c_speaker("queen");
        c_msgsetloc(0, "\\E1* Oh And One More Thing/", "es_rc_18");
        c_msgnextloc("\\EA* This \"Roller\"^1, \"Coaster\"/", "es_rc_19");
        c_msgnextloc("\\E9* Is Not \"Finished\"/%", "es_rc_20");
        c_talk_wait();
        c_sel(qu);
        c_walk("u", 10, 30);
        c_mus2("volume", 0, 30);
        c_wait(45);
        c_mus("free_all");
        c_soundplay(snd_cardrive);
        c_var_instance(id, "es_shock", 1);
        c_var_instance(coaster_entry, "visible", 0);
        c_var_instance(coaster_entry_hood, "visible", 0);
        c_var_instance(id, "stoplooptrack", true);
        c_var_lerp_instance(coaster_kris, "x", 1220, 1650, 40);
        c_var_instance(coaster_kris, "siner_add0", 0);
        c_var_lerp_instance(coaster_susie, "x", 1220, 1650, 40);
        c_var_instance(coaster_susie, "siner_add0", 0);
        c_panspeed(15, 0, 40);
        c_wait(20);
        c_var_instance(whiteall, "x", 0);
        c_var_lerp_instance(whiteall, "image_alpha", 0, 1, 20);
        c_wait(20);
        c_soundplay(snd_badexplosion);
        c_wait(60);
        c_var_instance(blackall, "x", 0);
        c_var_lerp_instance(blackall, "image_alpha", 0, 1, 80);
        c_var_instance(id, "hide_border", true);
        c_wait(60);
        c_soundplay(snd_impact);
        c_wait(90);
        c_actortokris();
        c_terminatekillactors();
        con = 49;
    }
    if (con == 49 && !i_ex(obj_cutscene_master))
    {
        con = 60;
        snd_free_all();
        global.plot = 64;
        room_goto(room_dw_city_intro);
    }
    // the track/city scrolling, same as vanilla
    if (loopcity)
    {
        loopcity = false;
        with (obj_ch2_scene11a_bg)
        {
            parallax = false;
            looping = true;
        }
    }
    if (looptrack)
    {
        looptrack = false;
        with (obj_ch2_scene11a_bg)
        {
            coaster_bg_1.x_speed = -15;
            cityscape_bg_1.x_speed = -0.5;
            cityscape_bg_2.x_speed = -1;
            for (var i = 0; i < 3; i++)
                coaster_track[i].x_speed = -15;
        }
    }
    if (stoplooptrack)
    {
        stoplooptrack = false;
        with (obj_ch2_scene11a_bg)
        {
            coaster_bg_1.x_speed = 0;
            for (var i = 0; i < 3; i++)
                coaster_track[i].x_speed = 0;
        }
    }
}

// ---------------------------------------------------------------- trash landing
function scr_es_scene11b_step()
{
    if (con == 0)
    {
        global.interact = 1;
        snd_free_all();
        con = 1;
    }
    if (con == 1)
    {
        con = 3;
        global.facing = 0;
        cutscene_master = scr_cutscene_make();
        scr_maincharacters_actors();
        kr_actor.sprite_index = spr_krisd_dark;
        var hasno = variable_instance_exists(id, "no");
        if (hasno)
            no_actor.sprite_index = scr_es_noelle_changed() ? spr_noelle_walk_down_sad_dw : spr_noelle_walk_down_dw;
        c_pannable(1);
        c_sel(kr);
        c_flip("y");
        c_setxy(368, 186);
        if (hasno)
        {
            c_sel(no);
            c_flip("y");
            c_setxy(262, 162);
        }
        c_var_lerp_instance(blackall, "image_alpha", 1, 0, 30);
        c_wait(70);
        if (hasno)
        {
            c_sel(no);
            c_autowalk(0);
            c_walk_wait("d", 5, 5);
            c_wait(10);
            c_flip("y");
            c_walk_wait("u", 5, 12);
            c_autowalk(1);
            c_walkdirect_wait(282, 214, 18);
            c_facing("r");
            c_wait(15);
        }
        c_sel(kr);
        c_autowalk(0);
        c_walk_wait("d", 5, 5);
        c_wait(10);
        c_flip("y");
        c_autodepth(0);
        c_walk_wait("u", 5, 10);
        c_autodepth(1);
        c_autowalk(1);
        c_walkdirect_wait(352, 222, 18);
        c_facing("l");
        c_wait(20);
        if (hasno)
        {
            c_sel(no);
            c_facing("r");
            c_speaker("noelle");
            c_msgsetloc(0, "\\E6* ...we landed in garbage./", "es_tr_1");
            c_msgnextloc("\\E1* She blew up the ride^1, Kris. With us on it./", "es_tr_2");
            c_msgnextloc("\\E9* She wanted us gone that badly./%", "es_tr_3");
            c_talk_wait();
            c_wait(30);
            c_speaker("noelle");
            if (scr_es_noelle_changed())
            {
                c_msgsetloc(0, "\\E0* ...my hands aren't even shaking anymore./", "es_tr_4");
                c_msgnextloc("\\E1* That's weird^1, right? It should be shaking./%", "es_tr_5");
            }
            else
            {
                c_msgsetloc(0, "\\E6* My hands are shaking./", "es_tr_6");
                c_msgnextloc("\\E1* ...it's just the cold. It's always the cold./%", "es_tr_7");
            }
            c_talk_wait();
            c_wait(20);
            c_msgsetloc(0, "\\E1* C'mon. Let's keep going./%", "es_tr_8");
            c_talk_wait();
            c_sel(no);
            c_facing("r");
            c_wait(10);
        }
        c_panobj(kr_actor, 10);
        c_wait(10);
        c_sel(kr);
        c_facing("d");
        c_pannable(0);
        c_actortokris();
        c_actortocaterpillar();
        c_mus2("initloop", "alley_ambience.ogg", 0);
        c_terminatekillactors();
        con = 4;
    }
    if (con == 4 && !i_ex(obj_cutscene_master))
    {
        global.interact = 0;
        global.facing = 0;
        global.plot = 65.5;
        con = -1;
    }
}

// ---------------------------------------------------------------- city split
// vanilla splits the party here (Susie takes Ralsei). Nobody's here to split from.
function scr_es_scene12_step()
{
    if (con == -1)
    {
        con = 1;
        global.interact = 1;
        global.facing = 0;
    }
    if (con == 1)
    {
        con = 3;
        with (obj_mainchara)
            visible = 0;
        with (obj_caterpillarchara)
            visible = 0;
        cutscene_master = scr_cutscene_make();
        kr = 0;
        kr_actor = instance_create(-20, 628, obj_actor);
        scr_actor_setup(kr, kr_actor, "kris");
        kr_actor.sprite_index = spr_krisr_dark;
        no = 1;
        no_actor = instance_create(-100, 612, obj_actor);
        scr_actor_setup(no, no_actor, "noelle");
        no_actor.sprite_index = scr_es_noelle_rsprite();
        c_sel(kr);
        c_walkdirect(242, 628, 45);
        c_delayfacing(60, "u");
        c_sel(no);
        c_walkdirect_wait(330, 612, 60);
        c_facing("u");
        c_wait(40);
        c_speaker("noelle");
        c_msgsetloc(0, "\\E6* ...the road splits./", "es_sp_1");
        c_msgnextloc("\\E1* I guess this is where people would split up^1, huh?/", "es_sp_2");
        c_facing("l");
        c_msgnextloc("\\E0* ...I'm not going anywhere without you^1, Kris./", "es_sp_3");
        c_msgnextloc("\\E1* ...not after all that./%", "es_sp_4");
        c_talk_wait();
        c_wait(15);
        c_sel(no);
        c_walkdirect_wait(282, 628, 10);
        c_facing("r");
        c_sel(kr);
        c_facing("r");
        c_actortokris();
        c_actortocaterpillar();
        c_terminatekillactors();
        con = 50;
    }
    if (con == 50 && !i_ex(obj_cutscene_master))
    {
        with (obj_mainchara)
            visible = 1;
        with (obj_caterpillarchara)
            visible = 1;
        global.plot = 66;
        global.interact = 0;
        global.facing = 0;
        con = 0;
        scr_tempsave();
    }
}

// ---------------------------------------------------------------- city01
// vanilla: Noelle hides from Queen and joins Kris here. Now Queen and Berdly come looking,
// see who's standing in the alley, and back away.
function scr_es_city01_step()
{
    if (i_ex(noellemarker))
        noellemarker.visible = 0;
    if (con == -1 && obj_mainchara.x > x)
    {
        con = 2;
        global.interact = 1;
        global.facing = 1;
        cutscene_master = scr_cutscene_make();
        scr_maincharacters_actors();
        qu = actor_count + 1;
        qu_actor = instance_create(camerax() + 645, cameray() + view_hport[0] + 200, obj_actor);
        scr_actor_setup(qu, qu_actor, "queen");
        qu_actor.sprite_index = spr_queen_walk_up;
        be = actor_count + 2;
        be_actor = instance_create(camerax() + 690, cameray() + view_hport[0] + 230, obj_actor);
        scr_actor_setup(be, be_actor, "berdly");
        be_actor.sprite_index = spr_berdly_walk_up_dw;
        var hasno = variable_instance_exists(id, "no");
        c_mus2("volume", 0, 30);
        c_sel(kr);
        c_walkdirect(210, 222, 15);
        c_delayfacing(16, "r");
        if (hasno)
        {
            c_sel(no);
            c_walkdirect(170, 200, 15);
            c_delayfacing(16, "r");
        }
        c_pannable(1);
        c_panspeed_wait(5, 0, 18);
        c_mus("free_all");
        c_wait(20);
        c_speaker("berdly");
        c_msgsetloc(0, "\\E6* Queen!^1! My splendid Queen!^1! Wait for me!!/%", "es_c1_1");
        c_talk_wait();
        c_sel(qu);
        c_walkdirect_wait(645, 173, 40);
        c_sel(be);
        c_walkdirect_wait(700, 200, 30);
        c_wait(15);
        c_sel(qu);
        c_facing("l");
        c_emote("!", 30, 15);
        c_wait(30);
        c_speaker("queen");
        c_msgsetloc(0, "\\ED* ..^1. Oh/", "es_c1_2");
        c_msgnextloc("\\E1* Berdly^1. Stop Walking/%", "es_c1_3");
        c_talk_wait();
        c_sel(be);
        c_facing("l");
        c_speaker("berdly");
        c_msgsetloc(0, "\\E0* Ah^1, Kris! And..^1. N-Noelle!?/", "es_c1_4");
        c_msgnextloc("\\E2* My beloved Noelle! Have you finally come to recognize--/%", "es_c1_5");
        c_talk_wait();
        if (hasno)
        {
            c_speaker("noelle");
            c_msgsetloc(0, "\\E1* Hi^1, Berdly./%", "es_c1_6");
            c_talk_wait();
        }
        c_wait(20);
        c_speaker("berdly");
        c_msgsetloc(0, "\\E3* ..^1. why did it get so cold all of a sudden?/", "es_c1_7");
        c_msgnextloc("\\E3* And Kris^1, why is your sword..^1. like THAT?/%", "es_c1_8");
        c_talk_wait();
        c_speaker("queen");
        c_msgsetloc(0, "\\E1* Berdly/", "es_c1_9");
        c_msgnextloc("\\E9* Those Two Are What Happened To The Field/", "es_c1_10");
        c_msgnextloc("\\E5* And To Sweet^1. And K_K^1. And Cap'n/", "es_c1_11");
        c_msgnextloc("\\ED* Back Away Slowly/%", "es_c1_12");
        c_talk_wait();
        c_speaker("berdly");
        c_msgsetloc(0, "\\EB* B-back away!? I^1, LORD Berdly^1, do NOT back away from--/%", "es_c1_13");
        c_talk_wait();
        if (hasno)
        {
            c_sel(no);
            c_walk("r", 2, 10);
            c_wait(12);
        }
        c_sel(be);
        c_emote("!", 30, 15);
        c_shakeobj();
        c_wait(30);
        c_speaker("berdly");
        c_msgsetloc(0, "\\E8* ...backing away./%", "es_c1_14");
        c_talk_wait();
        c_sel(qu);
        c_autowalk(0);
        c_walk("d", 2, 30);
        c_sel(be);
        c_autowalk(0);
        c_walk_wait("d", 2, 30);
        c_speaker("queen");
        c_msgsetloc(0, "\\E1* We Are Forming An Uneasy Truce Berdly/", "es_c1_15");
        c_msgnextloc("\\E4* The Kind Where We Run/%", "es_c1_16");
        c_talk_wait();
        c_sel(qu);
        c_autowalk(1);
        c_walk("d", 8, 40);
        c_sel(be);
        c_autowalk(1);
        c_walk_wait("d", 8, 40);
        c_wait(40);
        if (hasno)
        {
            c_sel(no);
            c_facing("l");
            c_speaker("noelle");
            c_msgsetloc(0, "\\E6* ...he used to talk to me every single day./", "es_c1_17");
            c_msgnextloc("\\E1* He couldn't even look at me./%", "es_c1_18");
            c_talk_wait();
        }
        c_panobj(kr_actor, 15);
        c_wait(15);
        c_pannable(0);
        c_actortokris();
        c_actortocaterpillar();
        c_terminatekillactors();
        con = 50;
    }
    if (con == 50 && !i_ex(obj_cutscene_master))
    {
        con = 0;
        global.plot = 70;
        global.interact = 0;
        global.facing = 0;
        snd_free_all();
        var savepoint = instance_create(360, 185, obj_savepoint);
        savepoint.depth = alleyoverlay.depth - 100;
        scr_tempsave();
    }
}
