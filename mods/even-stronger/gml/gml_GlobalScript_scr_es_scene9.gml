// Even Stronger: Queen's entrance and the first Werewire fight, rewritten for Kris + Noelle.
// Runs as obj_ch2_scene9's Step (its Create, alarms and Queen screens are the game's own).
// Nobody is captured: Queen comes to look at the two Lightners, wants Noelle, gets told no.

function scr_es_scene9_step()
{
    if (!variable_instance_exists(id, "es_qset"))
    {
        es_qset = -1;
        es_qdone = -1;
    }
    if (es_qset != es_qdone)
    {
        es_qdone = es_qset;
        scr_es_qscreens(es_qset);
    }
    if (con == 1 && obj_mainchara.x >= 540)
    {
        con = 99;
        global.interact = 1;
    }
    if (con == 99 && obj_mainchara.x >= 540)
    {
        global.interact = 1;
        global.facing = 2;
        cutscene_master = scr_cutscene_make();
        scr_maincharacters_actors();
        kr_actor.sprite_index = spr_krisr_dark;
        qu = actor_count + 2;
        qu_actor = instance_create(725, 273, obj_actor_queenchair);
        scr_actor_setup_nofacing(qu, qu_actor, "queen");
        qu_actor.visible = 0;
        qu_actor.shadow = 0;
        qu_actor.auto_depth = 0;
        qu_actor.depth = 1000300;
        with (qu_actor)
            fakeshadow.visible = 0;
        qu_flame = actor_count + 8;
        qu_flame_actor = scr_dark_marker(qu_actor.x, cameray() + view_hport[0] + 200, spr_queen_chair_flame);
        qu_flame_actor.visible = 0;
        qu_flame_actor.auto_depth = 0;
        qu_flame_actor.depth = qu_actor.depth + 100;
        npc1 = actor_count + 4;
        npc1_actor = scr_dark_marker(npc1_o.x, npc1_o.y, spr_plugboy_cower);
        scr_actor_setup_nofacing(npc1, npc1_actor, "npc1");
        with (npc1_actor)
            scr_depth();
        npc2 = actor_count + 5;
        npc2_actor = scr_dark_marker(npc2_o.x, npc2_o.y, spr_plugboy_cower);
        scr_actor_setup_nofacing(npc2, npc2_actor, "npc2");
        with (npc2_actor)
            scr_depth();
        wire1 = actor_count + 6;
        wire1_actor = scr_dark_marker(npc2_o.x, npc1_o.y - 400, spr_plugboy_plug);
        scr_actor_setup_nofacing(wire1, wire1_actor, "wire1");
        wire2 = actor_count + 7;
        wire2_actor = scr_dark_marker(npc2_o.x, npc2_o.y - 400, spr_plugboy_plug);
        scr_actor_setup_nofacing(wire2, wire2_actor, "wire1");
        scr_actor_setup_nofacing(qu_flame, qu_flame_actor, "qu_flame");
        with (obj_npc_room)
            instance_destroy();

        // two locals, hiding
        c_mus2("volume", 0, 30);
        c_sel(no);
        c_facing("l");
        c_wait(20);
        c_speaker("noelle");
        c_msgsetloc(0, "\\E2* Kris^1, look.../", "es_q_1");
        c_msgnextloc("\\E7* Those two are..^1. shaking./", "es_q_2");
        c_msgnextloc("\\E1* Like they're hiding from something./%", "es_q_3");
        c_talk_wait();
        c_wait(20);
        c_pannable(1);
        c_pan(470, 0, 40);
        c_sel(qu_flame);
        c_setxy(qu_actor.x, qu_actor.y - 80);
        c_imagespeed(0.25);
        c_stickto(qu_actor, 100);
        c_sel(kr);
        c_delayfacing(21, "r");
        c_walkdirect(600, 200, 20);
        c_sel(no);
        c_delayfacing(21, "r");
        c_walkdirect(552, 222, 20);
        c_wait(50);
        c_mus("free_all");
        c_wait(21);

        // Queen rises
        c_mus2("initloop", "queen_intro.ogg", 0);
        c_sel(kr);
        c_facing("u");
        c_sel(no);
        c_facing("u");
        c_sel(qu);
        c_autowalk(0);
        c_sprite(spr_queen_chair_ohoho_1);
        c_imagespeed(0.1);
        c_visible(1);
        c_sel(qu_flame);
        c_visible(1);
        c_sel(qu);
        c_walkdirect_wait(qu_actor.x, 90, 180);
        c_autodepth(1);
        c_wait(52);
        c_mus("free");
        c_wait(12);
        c_sprite(spr_queen_chair_swish_wine);
        c_imagespeed(0.1);
        c_mus2("initloop", "queen.ogg", 0);
        c_msgside("bottom");
        c_speaker("queen");
        c_msgsetloc(0, "\\E0* Well Well Well/", "es_q_4");
        c_msgnextloc("\\E2* Two Unregistered Lightners In My Beautiful Cyber Field/", "es_q_5");
        c_msgnextloc("\\E1* Uninvited. Unannounced. Unbelievably Underdressed/%", "es_q_6");
        c_talk_wait();
        c_sel(no);
        c_sprite(spr_noelle_shocked_dw);
        c_shakeobj();
        c_speaker("noelle");
        c_msgsetloc(0, "\\EK* W-who are you!?/%", "es_q_7");
        c_talk_wait();
        c_speaker("queen");
        c_msgsetloc(0, "\\E2* I Am Known As Serial Number Q5U4EX7YY2E9N/", "es_q_8");
        c_msgnextloc("\\E0* But You Foolish Children May Call Me/%", "es_q_9");
        c_talk_wait();
        c_sel(qu);
        c_sprite(spr_queen_chair_drink_wine);
        c_imageindex(0);
        c_imagespeed(0.05);
        c_wait(50);
        c_imagespeed(0);
        c_wait(15);
        c_sprite(spr_queen_chair_pose);
        c_imagespeed(0.1);
        c_var_instance(id, "es_qset", 1);
        c_speaker("queen");
        c_msgsetloc(0, "\\E1* \"Queen\"/%", "es_q_10");
        c_talk_wait();
        c_sprite(spr_queen_chair_swish_wine);
        c_sel(no);
        c_facing("u");
        c_speaker("noelle");
        c_msgsetloc(0, "\\E1* ..^1. Queen?/", "es_q_11");
        c_msgnextloc("\\E2* Um^1. Hi^1. We're just..^1. looking for the way out?/", "es_q_12");
        c_msgnextloc("\\E0* We have a group project^1, eheheh.../%", "es_q_13");
        c_talk_wait();
        c_var_instance(id, "es_qset", 2);
        c_speaker("queen");
        c_msgsetloc(0, "\\E0* A Group Project/", "es_q_14");
        c_msgnextloc("\\EQ* Children Doing Work Together/", "es_q_15");
        c_msgnextloc("\\E1* Adorable. Inefficient. Slightly Disgusting/%", "es_q_16");
        c_talk_wait();
        c_wait(20);

        // she looks closer at Noelle
        c_sel(qu);
        c_sprite(spr_queen_chair_pose);
        c_speaker("queen");
        c_msgsetloc(0, "\\E2* Wait/", "es_q_17");
        c_msgnextloc("\\E2* Zoom In/%", "es_q_18");
        c_var_instance(id, "es_qset", 3);
        c_talk_wait();
        c_wait(20);
        c_msgsetloc(0, "\\E0* You. Girl With The Antlers/", "es_q_19");
        c_msgnextloc("\\EE* Your Data Is..^1. Interesting/", "es_q_20");
        c_msgnextloc("\\E2* There Is Something Very Cold In Your Code/%", "es_q_21");
        c_talk_wait();
        c_sel(no);
        c_sprite(spr_noelle_shocked_dw);
        c_speaker("noelle");
        c_msgsetloc(0, "\\E2* C-cold...?/%", "es_q_22");
        c_talk_wait();
        c_sel(qu);
        c_sprite(spr_queen_chair_swish_wine);
        c_var_instance(id, "es_qset", 4);
        c_speaker("queen");
        c_msgsetloc(0, "\\E0* I Have A Proposition/", "es_q_23");
        c_msgnextloc("\\E6* Become My Willing Peon/", "es_q_24");
        c_msgnextloc("\\E9* I Will Even Upgrade Your Face Into A Robot One/%", "es_q_25");
        c_talk_wait();
        c_speaker("noelle");
        c_msgsetloc(0, "\\EK* N-no thank you!!/%", "es_q_26");
        c_talk_wait();

        // Kris steps between them
        c_sel(kr);
        c_walkdirect(620, 214, 14);
        c_wait(16);
        c_facing("u");
        c_sel(no);
        c_facing("r");
        c_speaker("noelle");
        c_msgsetloc(0, "\\E2* K-Kris...?/%", "es_q_27");
        c_talk_wait();
        c_sel(qu);
        c_sprite(spr_queen_chair_ohoho_1);
        c_imagespeed(0.1);
        c_var_instance(id, "es_qset", 5);
        c_speaker("queen");
        c_msgsetloc(0, "\\EO* Rejected. By A Teenager. Again/", "es_q_28");
        c_msgnextloc("\\E1* Very Well/", "es_q_29");
        c_msgnextloc("\\E0* Perhaps My Citizens Can Change Your Mind/%", "es_q_30");
        c_var_instance(id, "es_qset", 6);
        c_talk_wait();
        c_var_instance(id, "screencon", 1);
        c_sel(qu);
        c_walkdirect(qu_actor.x + 400, 90, 40);
        c_sel(kr);
        c_delaywalkdirect(30, 707, 194, 20);
        c_sel(no);
        c_delaywalkdirect(30, 650, 214, 20);
        c_wait(55);

        // the wires take the two locals (the game's own animation)
        c_sel(npc1);
        c_setxy(npc2_actor.x, npc1_actor.y);
        c_sel(npc2);
        c_setxy(npc2_actor.x + 50, npc2_actor.y);
        c_pan_wait(280, 0, 20);
        c_mus2("volume", 0, 60);
        c_sel(no);
        c_facing("l");
        c_sel(kr);
        c_facing("l");
        c_var_instance(id, "wirecon", 1);
        c_sel(no);
        c_sprite(spr_noelle_shocked_dw);
        c_wait(60);
        c_pan(620, 0, 30);
        c_wait(5);
        c_var_instance(id, "funcon", 1);
        c_wait(5);
        c_sel(kr);
        c_facing("r");
        c_autofacing(0);
        c_sel(no);
        c_facing("r");
        c_autofacing(0);
        xx = 620;
        c_sel(npc1);
        c_setxy((xx + 480) - 200, -320);
        c_walkdirect((xx + 480) - 200, 120, 30);
        c_sel(npc2);
        c_setxy(xx + 440, -210);
        c_walkdirect(xx + 440, 170, 30);
        c_sel(wire1);
        c_setxy((xx + 480 + 2) - 200, -320);
        c_walkdirect((xx + 480) - 200, 124, 30);
        c_sel(wire2);
        c_setxy(xx + 440 + 2, -210);
        c_walkdirect(xx + 440, 174, 30);
        c_wait(10);
        c_sel(npc1);
        c_animate(18, 23, 0.5);
        c_sel(npc2);
        c_animate(18, 23, 0.5);
        c_wait(60);
        c_speaker("queen");
        c_msgsetloc(0, "* Enjoy: Your Assimilation/%", "es_q_31");
        c_mus("free");
        c_talk_wait();
        c_speaker("noelle");
        c_msgsetloc(0, "\\E3* Kris^1, th-they're coming!!/%", "es_q_32");
        c_talk_wait();
        c_actortokris();
        c_actortocaterpillar();
        con = 2;
    }
    if (screencon == 1)
    {
        with (obj_queenscreen)
            dismiss = 1;
        screencon = 2;
    }
    if (funcon == 1)
    {
        global.encounterno = 53;
        scr_encountersetup(global.encounterno);
        funcon = 2;
    }
    // wire animation, as in the original scene
    // the fight deletes the wire/local markers when it starts; if the wire animation
    // hasn't finished by then (text skipped fast), stop it instead of reading deleted markers
    if (wirecon >= 1 && wirecon <= 5)
    {
        if (!variable_instance_exists(id, "wire1_actor") || !i_ex(wire1_actor) || !i_ex(wire2_actor) || !i_ex(npc1_actor) || !i_ex(npc2_actor))
            wirecon = 6;
    }
    if (wirecon == 1)
    {
        with (obj_marker)
        {
            if (sprite_index == spr_plugboy_cower)
                sprite_index = spr_plugboy_shock;
            if (sprite_index == spr_plugboy_plug)
                sprite_index = spr_werewire_plug_vertical;
            image_index = 1;
        }
        wire1_actor.x = npc1_actor.x + 20;
        wire1_actor.y = npc1_actor.y - 300;
        wire2_actor.x = npc2_actor.x + 20;
        wire2_actor.y = npc2_actor.y - 300;
        with (wire1_actor)
        {
            gravity = 1;
            depth = -400;
        }
        with (wire2_actor)
        {
            gravity = 1;
            depth = -400;
        }
        wirecon = 2;
    }
    if (wirecon == 2)
    {
        if (wire1_actor.y >= (npc1_actor.y + 10))
        {
            wire1_actor.gravity = 0;
            wire1_actor.vspeed = 0;
            wire2_actor.gravity = 0;
            wire2_actor.vspeed = 0;
            wire1_actor.y = npc1_actor.y + 20;
            wire2_actor.y = npc2_actor.y + 20;
            snd_play(snd_grab);
            with (npc1_actor)
                scr_shakeobj();
            with (npc2_actor)
                scr_shakeobj();
            wirecon = 3;
            wiretimer = 0;
        }
    }
    if (wirecon == 3)
    {
        wiretimer++;
        if (wiretimer >= 8)
        {
            with (obj_marker)
                gravity = -2;
            wirecon = 4;
            wiretimer = 0;
        }
    }
    if (wirecon == 4)
    {
        if (wire1_actor.y <= -140)
        {
            with (obj_marker)
            {
                vspeed = 0;
                gravity = 0;
                if (sprite_index == spr_werewire_plug_vertical)
                    sprite_index = spr_plugboy_plug;
                if (sprite_index == spr_plugboy_shock)
                    sprite_index = spr_plugboy_transform;
            }
            wirecon = 5;
            wiretimer = 0;
        }
    }
    if (wirecon == 5)
    {
        wiretimer++;
        if (wiretimer == 70)
        {
            snd_play(snd_electric_talk);
            wiretimer = 0;
            wirecon++;
        }
    }
    // the fight
    if (con == 2 && !instance_exists(obj_cutscene_master))
    {
        es_kills0 = global.flag[2452];
        es_frozen0 = global.flag[2454];
        global.specialbattle = 3;
        global.flag[9] = 1;
        global.batmusic[0] = snd_init("battle.ogg");
        instance_create(0, 0, obj_battleback);
        instance_create(0, 0, obj_encounterbasic);
        encounterflag = 552;
        global.flag[54] = encounterflag;
        global.encounterno = 53;
        scr_encountersetup(global.encounterno);
        with (obj_actor)
        {
            if (name == "queen")
            {
                sprite_index = spr_queen_chair_ohoho_1;
                hspeed = 6;
            }
        }
        with (npc1_actor)
        {
            depth = -100;
            scr_move_to_point_over_time(global.monstermakex[0] - 2, global.monstermakey[0] - 34, 15);
            depth += 20;
        }
        with (wire1_actor)
        {
            depth = -200;
            scr_move_to_point_over_time(global.monstermakex[0] - 2, global.monstermakey[0] - 34, 15);
            depth += 20;
        }
        with (npc2_actor)
        {
            depth = -100;
            scr_move_to_point_over_time(global.monstermakex[1] - 2, global.monstermakey[1] - 34, 15);
        }
        with (wire2_actor)
        {
            depth = -200;
            scr_move_to_point_over_time(global.monstermakex[1] - 2, global.monstermakey[1] - 34, 15);
        }
        con = 3;
        alarm[4] = 25;
    }
    if (con == 4)
    {
        with (obj_actor)
            instance_destroy();
        with (obj_marker)
            instance_destroy();
        con = 5;
        alarm[4] = 30;
    }
    if (con == 6 && !instance_exists(obj_battlecontroller))
    {
        global.interact = 1;
        con = 8.5;
        alarm[4] = 30;
        obj_mainchara.cutscene = 1;
        obj_mainchara.x = 707;
        obj_mainchara.y = 194;
        if (instance_exists(obj_caterpillarchara))
        {
            obj_caterpillarchara.x = 650;
            obj_caterpillarchara.y = 214;
        }
        camerax_set(620);
    }
    // the aftermath
    if (con == 9.5)
    {
        con = 9.6;
        var killed = global.flag[2452] - es_kills0;
        var froze = global.flag[2454] - es_frozen0;
        cutscene_master = scr_cutscene_make();
        scr_maincharacters_actors();
        kr_actor.sprite_index = spr_krisr_dark;
        // Queen comes back in person, rising behind the platform like before
        qu = actor_count + 2;
        qu_actor = instance_create(camerax() + 255, 273, obj_actor_queenchair);
        scr_actor_setup_nofacing(qu, qu_actor, "queen");
        qu_actor.visible = 0;
        qu_actor.shadow = 0;
        qu_actor.auto_depth = 0;
        qu_actor.depth = 1000300;
        with (qu_actor)
            fakeshadow.visible = 0;
        qu_flame = actor_count + 8;
        qu_flame_actor = scr_dark_marker(qu_actor.x, cameray() + view_hport[0] + 200, spr_queen_chair_flame);
        qu_flame_actor.visible = 0;
        qu_flame_actor.auto_depth = 0;
        qu_flame_actor.depth = qu_actor.depth + 100;
        scr_actor_setup_nofacing(qu_flame, qu_flame_actor, "qu_flame");
        c_sel(qu_flame);
        c_setxy(qu_actor.x, qu_actor.y - 80);
        c_imagespeed(0.25);
        c_stickto(qu_actor, 100);
        c_wait(40);
        c_sel(no);
        c_facing("r");
        c_speaker("noelle");
        if (killed > 0)
        {
            c_msgsetloc(0, "\\E2* Th..^1. they're gone./%", "es_q_40");
            c_talk_wait();
            c_wait(40);
            c_var_instance(id, "es_qset", 10);
            c_wait(20);
            c_sel(kr);
            c_facing("u");
            c_sel(no);
            c_facing("u");
            c_sel(qu);
            c_autowalk(0);
            c_sprite(spr_queen_chair_ohoho_1);
            c_imagespeed(0.1);
            c_visible(1);
            c_sel(qu_flame);
            c_visible(1);
            c_sel(qu);
            c_walkdirect_wait(qu_actor.x, 90, 70);
            c_autodepth(1);
            c_sprite(spr_queen_chair_swish_wine);
            c_imagespeed(0.1);
            c_mus2("initloop", "queen.ogg", 0);
            c_speaker("queen");
            c_msgside("bottom");
            c_msgsetloc(0, "\\E0* Hello Again I Forgot To Mention That--/%", "es_q_41");
            c_talk_wait();
            c_wait(30);
            c_var_instance(id, "es_qset", 11);
            c_sel(qu);
            c_sprite(spr_queen_chair_pose);
            c_msgsetloc(0, "\\E2* .../", "es_q_42");
            c_msgnextloc("\\E2* Where Are My Werewires/%", "es_q_43");
            c_talk_wait();
            c_speaker("noelle");
            c_msgsetloc(0, "\\E7* I..^1. I don't.../%", "es_q_44");
            c_talk_wait();
            c_speaker("queen");
            c_var_instance(id, "es_qset", 12);
            c_msgsetloc(0, "\\EE* My Readings Say They Were Not Defeated/", "es_q_45");
            c_msgnextloc("\\E2* They Are Not Asleep. They Are Not Hiding/", "es_q_46");
            c_msgnextloc("\\E1* They Are Simply: Not There Anymore/%", "es_q_47");
            c_talk_wait();
            c_wait(20);
            c_var_instance(id, "es_qset", 13);
            c_sel(qu);
            c_sprite(spr_queen_chair_ohoho_1);
            c_msgsetloc(0, "\\E0* Hm/", "es_q_48");
            c_msgnextloc("\\EQ* Hm Hm Hm/", "es_q_49");
            c_msgnextloc("\\E1* I Was Going To Make You My Peons/", "es_q_50");
            c_msgnextloc("\\E2* But I Do Not Think I Want You In My City At All/%", "es_q_51");
            c_talk_wait();
            c_var_instance(id, "es_qset", 14);
            c_sel(qu);
            c_sprite(spr_queen_chair_pose);
            c_msgsetloc(0, "\\E0* Attention All Citizens Of Cyber City/", "es_q_52");
            c_msgnextloc("\\E2* A Human And A Girl Have Entered The Cyber Field/", "es_q_53");
            c_msgnextloc("\\E1* Do Not Engage. Do Not Make Eye Contact. Do Not Be Interesting/", "es_q_54");
            c_msgnextloc("\\E0* And Raise Your Guard/%", "es_q_55");
            c_talk_wait();
            c_var_instance(id, "screencon", 1);
            c_sel(qu);
            c_sprite(spr_queen_chair_ohoho_1);
            c_walkdirect(qu_actor.x + 400, 90, 40);
            c_wait(30);
            c_mus("free");
            c_wait(30);
            c_sel(kr);
            c_facing("r");
            c_sel(no);
            c_facing("d");
            c_wait(30);
            c_speaker("noelle");
            c_msgsetloc(0, "\\E7* Kris.../", "es_q_56");
            c_msgnextloc("\\E6* They were people^1, weren't they?/", "es_q_57");
            c_msgnextloc("\\E6* Little..^1. computer people./%", "es_q_58");
            c_talk_wait();
            c_wait(45);
            if (froze > 0)
            {
                c_msgsetloc(0, "\\E1* When the ice came out.../", "es_q_59");
                c_msgnextloc("\\E7* It didn't feel like it came from my hands./", "es_q_60");
                c_msgnextloc("\\E7* It felt like it came from somewhere..^1. inside./%", "es_q_61");
            }
            else
            {
                c_msgsetloc(0, "\\E1* You didn't even..^1. hesitate./", "es_q_62");
                c_msgnextloc("\\E7* Your sword just..^1. went through them./", "es_q_63");
                c_msgnextloc("\\E7* Like it knew where to go./%", "es_q_64");
            }
            c_talk_wait();
            c_wait(50);
            c_sel(no);
            c_facing("r");
            c_msgsetloc(0, "\\E1* ..^1. let's just find the way out./", "es_q_65");
            c_msgnextloc("\\E6* Please./%", "es_q_66");
            c_talk_wait();
        }
        else
        {
            c_msgsetloc(0, "\\E2* Th..^1. they ran off./%", "es_q_70");
            c_talk_wait();
            c_wait(30);
            c_var_instance(id, "es_qset", 20);
            c_wait(20);
            c_sel(kr);
            c_facing("u");
            c_sel(no);
            c_facing("u");
            c_sel(qu);
            c_autowalk(0);
            c_sprite(spr_queen_chair_ohoho_1);
            c_imagespeed(0.1);
            c_visible(1);
            c_sel(qu_flame);
            c_visible(1);
            c_sel(qu);
            c_walkdirect_wait(qu_actor.x, 90, 70);
            c_autodepth(1);
            c_sprite(spr_queen_chair_swish_wine);
            c_imagespeed(0.1);
            c_mus2("initloop", "queen.ogg", 0);
            c_speaker("queen");
            c_msgside("bottom");
            c_msgsetloc(0, "\\E1* Oh. You Did Not Even Hurt Them/", "es_q_71");
            c_msgnextloc("\\E0* How Boring/", "es_q_72");
            c_msgnextloc("\\EQ* My Offer Still Stands Antler Girl. Think About It/%", "es_q_73");
            c_var_instance(id, "es_qset", 21);
            c_talk_wait();
            c_var_instance(id, "screencon", 1);
            c_sel(qu);
            c_sprite(spr_queen_chair_ohoho_1);
            c_walkdirect(qu_actor.x + 400, 90, 40);
            c_wait(30);
            c_mus("free");
            c_wait(20);
            c_sel(no);
            c_facing("r");
            c_speaker("noelle");
            c_msgsetloc(0, "\\E6* ...I don't think I want to think about it./", "es_q_74");
            c_msgnextloc("\\E1* Let's find the way out^1, Kris./%", "es_q_75");
            c_talk_wait();
        }
        c_actortokris();
        c_actortocaterpillar();
        con = 10;
    }
    if (con == 10 && !instance_exists(obj_cutscene_master))
    {
        with (obj_actor_queenchair)
            instance_destroy();
        if (variable_instance_exists(id, "qu_flame_actor") && i_ex(qu_flame_actor))
            with (qu_flame_actor)
                instance_destroy();
        global.interact = 0;
        con = 11;
        global.plot = 51;
        scr_tempsave();
    }
}

// Queen's screens for each beat: [x, y, frame of spr_queenscreen], in the original scene's
// half-scale screen coordinates.
function scr_es_qscreens(argument0)
{
    var g = [];
    switch (argument0)
    {
        case 1: // her name
        case 20:
            g = [[117, 40, 0], [80, 40, 2], [210, 40, 1], [245, 40, 2]];
            break;
        case 2: // "Children Doing Work Together"
            g = [[30, 35, 22], [63, 39, 23], [253, 39, 27], [284, 35, 31]];
            break;
        case 3: // zooming in on Noelle's data
            g = [[28, 67, 7], [95, 74, 20], [224, 76, 9], [282, 78, 19]];
            break;
        case 4: // willing peon, robot face
            g = [[94, 35, 7], [220, 35, 33], [253, 39, 8]];
            break;
        case 5: // rejected
            g = [[73, 34, 24], [224, 76, 24]];
            break;
        case 6: // her citizens
            g = [[30, 35, 15], [284, 35, 12], [63, 39, 13]];
            break;
        case 10: // she's back
            g = [[117, 40, 0], [80, 40, 14], [210, 40, 1], [245, 40, 14]];
            break;
        case 11: // where are my Werewires
            g = [[30, 35, 20], [94, 35, 20], [220, 35, 20], [284, 35, 20]];
            break;
        case 12: // not there anymore
            g = [[28, 67, 11], [95, 74, 25], [224, 76, 25], [282, 78, 26]];
            break;
        case 13: // she changes her mind about you
            g = [[63, 39, 21], [94, 74, 13], [253, 39, 6]];
            break;
        case 14: // warning to the whole city
            g = [[30, 35, 24], [94, 35, 6], [220, 35, 6], [284, 35, 24], [28, 67, 38], [282, 78, 7]];
            break;
        case 21: // the offer still stands
            g = [[28, 67, 7], [282, 78, 10]];
            break;
    }
    // the entrance builds up a collage like the original; after the fight each beat replaces the last
    if (argument0 >= 10)
    {
        with (obj_queenscreen)
            dismiss = 1;
        screencount = 0;
        screenmax = 0;
    }
    for (var i = 0; i < array_length(g); i++)
    {
        var e = g[i];
        screenx[screenmax] = e[0];
        screeny[screenmax] = e[1];
        screenimage[screenmax] = e[2];
        screenmax++;
    }
}
