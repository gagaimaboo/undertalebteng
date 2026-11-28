if (stage == 3 && !instance_exists(obj_cutscene_master))
{
    stage = 4;
    with (obj_actor)
        visible = 0;
    scr_losechar();
    scr_getchar(4);
    scr_es_setspells();
    if (global.plot < 20)
        global.plot = 20;
    global.flag[2453] = 1;
    trans = instance_create(0, 0, obj_dw_transition);
    with (trans)
    {
        finaly = 60;
        skiprunback = 1;
        kris_only = 1;
        nextroom = room_dw_cyber_intro_1;
    }
}
