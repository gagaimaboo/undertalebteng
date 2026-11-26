if (global.interact == 0 && con == 1)
{
    con = 2;
    global.interact = 1;
    global.facing = 0;
    cutscene_master = scr_cutscene_make();
    scr_maincharacters_actors();
    kr_actor.sprite_index = spr_krisd_dark;
    c_wait(30);
    c_sel(no);
    c_facing("u");
    c_speaker("noelle");
    c_msgsetloc(0, "\\E2* K..^1. Kris.../", "es_land_1");
    c_msgnextloc("\\E1* Where..^1. are we?/", "es_land_2");
    c_msgnextloc("\\E2* And why are you dressed like a..^1. like a knight in a video game?/", "es_land_3");
    c_msgnextloc("\\E5* ..^1. why am I wearing this!?/%", "es_land_4");
    c_talk_wait();
    c_facing("r");
    c_wait(20);
    c_speaker("noelle");
    c_msgsetloc(0, "\\E1* ..^1. this is new./%", "es_land_5");
    c_talk_wait();
    c_actortokris();
    c_actortocaterpillar();
    c_terminatekillactors();
    con = 3;
}
if (con == 3 && !i_ex(obj_cutscene_master))
{
    con = 0;
    global.plot = 50;
    global.facing = 0;
    global.interact = 0;
    scr_tempsave();
    instance_destroy();
}
