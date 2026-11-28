if (con == 1 && global.interact == 0 && instance_exists(obj_mainchara) && obj_mainchara.x > 200)
{
    con = 2;
    global.interact = 1;
    cutscene_master = scr_cutscene_make();
    scr_maincharacters_actors();
    c_wait(30);
    c_sel(no);
    c_facing("r");
    c_speaker("noelle");
    c_msgsetloc(0, "\\E1* ..^1. it's empty./%", "es_s10_1");
    c_talk_wait();
    c_wait(40);
    c_sel(no);
    c_facing("u");
    c_msgsetloc(0, "\\E2* Kris^1, I can hear them./", "es_s10_2");
    c_msgnextloc("\\E7* Behind the walls. Inside the screens. Holding their breath./", "es_s10_3");
    c_msgnextloc("\\E6* They're hiding from us./%", "es_s10_4");
    c_talk_wait();
    c_wait(30);
    c_sel(no);
    c_facing("r");
    c_msgsetloc(0, "\\E1* ...Let's keep going./%", "es_s10_5");
    c_talk_wait();
    c_actortokris();
    c_actortocaterpillar();
    c_terminatekillactors();
}
if (con == 2 && !instance_exists(obj_cutscene_master))
{
    con = 3;
    if (global.plot < 55)
        global.plot = 55;
    global.interact = 0;
    // the way on opens like it does after the arcade game
    with (obj_doorA_musfade)
    {
        door = instance_create(x, y, obj_doorA);
        door.image_yscale = image_yscale;
        instance_destroy();
    }
    instance_destroy();
}
