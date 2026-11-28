if (con == 0 && (!is_array(pf) || global.flag[pf[0]] == 1))
{
    instance_destroy();
    exit;
}
if (con == 0 && global.interact == 0 && instance_exists(obj_mainchara))
{
    parts = scr_es_puzzle_parts();
    var near = 99999;
    for (var i = 0; i < array_length(parts); i++)
    {
        if (i_ex(parts[i]))
        {
            var p = parts[i];
            near = min(near, point_distance(obj_mainchara.x + 20, obj_mainchara.y + 40, p.x + (p.sprite_width / 2), p.y + (p.sprite_height / 2)));
        }
    }
    if (near < 170 && array_length(parts) > 0)
    {
        con = 1;
        global.interact = 1;
        var first = (global.flag[2460] + global.flag[2461] + global.flag[2462] + global.flag[2463] + global.flag[2464]) == 0;
        cutscene_master = scr_cutscene_make();
        scr_maincharacters_actors();
        c_sel(no);
        c_facing("u");
        c_speaker("noelle");
        if (first)
        {
            c_msgsetloc(0, "\\E2* Kris^1, wait./", "es_fz_1");
            c_msgnextloc("\\E1* This is a puzzle^1, right? We have to..^1. solve it?/", "es_fz_2");
            c_msgnextloc("\\E7* ..^1. no./", "es_fz_3");
            c_msgnextloc("\\E1* I think I can just.../%", "es_fz_4");
        }
        else
        {
            switch (room)
            {
                case room_dw_cyber_musical_door:
                    c_msgsetloc(0, "\\E1* It wants a song./", "es_fz_5");
                    c_msgnextloc("\\E7* ...I don't feel like singing./%", "es_fz_6");
                    break;
                case room_dw_cyber_viro_ring:
                    c_msgsetloc(0, "\\E7* Hold still^1, Kris./%", "es_fz_7");
                    break;
                default:
                    c_msgsetloc(0, choose("\\E7* Let me./%", "\\E7* Stand back./%", "\\E1* I've got it./%"), "es_fz_8");
            }
        }
        c_talk_wait();
        c_sel(no);
        c_sprite(spr_noelle_point_dw);
        c_wait(10);
        c_var_instance(id, "con", 2);
        c_wait(70);
        c_sel(no);
        c_facing("u");
        if (first)
        {
            c_wait(20);
            c_speaker("noelle");
            c_msgsetloc(0, "\\E2* ..^1. it worked./", "es_fz_9");
            c_msgnextloc("\\E7* I didn't even have to think about it./", "es_fz_10");
            c_msgnextloc("\\E7* The cold just..^1. came./%", "es_fz_11");
            c_talk_wait();
        }
        c_actortokris();
        c_actortocaterpillar();
        c_terminatekillactors();
    }
}
// the ice
if (con == 2)
{
    con = 3;
    timer = 0;
    snd_play(snd_icespell);
    snd_pitch(snd_play(snd_snowgrave), 1.2);
    bursts = [];
    for (var i = 0; i < array_length(parts); i++)
    {
        if (i_ex(parts[i]))
        {
            var p = parts[i];
            if (!p.visible || p.sprite_index < 0 || (p.x == 0 && p.y == 0))
                continue;
            array_push(bursts, [p.x + (p.sprite_width / 2), p.y + (p.sprite_height / 2)]);
        }
    }
    if (array_length(bursts) == 0)
        array_push(bursts, [camerax() + 320, cameray() + 200]);
}
if (con == 3)
{
    timer++;
    if (timer == 25)
    {
        global.flag[pf[0]] = 1;
        global.flag[pf[1]] = 1;
        snd_play(snd_break2);
        with (obj_forcefield)
            instance_destroy();
        with (obj_beatbullet_wall)
            instance_destroy();
        with (obj_ow_viroring)
            instance_destroy();
        with (obj_ow_viroringTrig)
            instance_destroy();
        with (obj_ch2_keyboardpuzzle_controller)
        {
            won = true;
            currentString = idealString;
        }
        if (!instance_exists(obj_es_frost))
            instance_create(0, 0, obj_es_frost);
    }
    if (timer > 30 && !instance_exists(obj_cutscene_master))
    {
        global.interact = 0;
        instance_destroy();
    }
}
