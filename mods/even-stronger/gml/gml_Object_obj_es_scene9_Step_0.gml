if (con == 1 && global.interact == 0 && instance_exists(obj_mainchara) && obj_mainchara.x >= 540)
{
    con = 2;
    global.interact = 1;
    cutscene_master = scr_cutscene_make();
    scr_maincharacters_actors();
    c_sel(no);
    c_facing("r");
    c_speaker("noelle");
    c_msgsetloc(0, "\\E2* K-Kris..^1. something's moving up there.../%", "es_s9_1");
    c_talk_wait();
    c_actortokris();
    c_actortocaterpillar();
    c_terminatekillactors();
}
if (con == 2 && !i_ex(obj_cutscene_master))
{
    con = 3;
    global.batmusic[0] = snd_init("battle.ogg");
    global.flag[9] = 1;
    instance_create(0, 0, obj_battleback);
    instance_create(0, 0, obj_encounterbasic);
    global.flag[54] = 552;
    global.encounterno = 53;
    scr_encountersetup(global.encounterno);
}
if (con == 3 && instance_exists(obj_battlecontroller))
    con = 4;
if (con == 4 && !instance_exists(obj_battlecontroller))
{
    timer++;
    if (timer > 20)
    {
        con = 5;
        global.interact = 1;
        cutscene_master = scr_cutscene_make();
        scr_maincharacters_actors();
        c_sel(no);
        c_facing("r");
        c_wait(20);
        c_speaker("noelle");
        if (global.flag[2452] > 0)
        {
            c_msgsetloc(0, "\\E2* D..^1. did we just.../", "es_s9_2");
            c_msgnextloc("\\E7* They're not moving^1, Kris./", "es_s9_3");
            c_msgnextloc("\\E6* ..^1. it's fine^1. They're just..^1. programs^1. Right?/", "es_s9_4");
            c_msgnextloc("\\E1* Right./%", "es_s9_5");
        }
        else
        {
            c_msgsetloc(0, "\\E2* Th..^1. they ran away?/", "es_s9_6");
            c_msgnextloc("\\E1* ..^1. somehow I don't think they'll stay away./%", "es_s9_7");
        }
        c_talk_wait();
        c_actortokris();
        c_actortocaterpillar();
        c_terminatekillactors();
    }
}
if (con == 5 && !i_ex(obj_cutscene_master))
{
    con = 6;
    if (global.plot < 51)
        global.plot = 51;
    global.interact = 0;
    instance_destroy();
}
