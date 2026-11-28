t++;
if (global.interact == 0 && instance_exists(obj_mainchara) && button1_p())
{
    var kx = obj_mainchara.x + 19;
    var ky = obj_mainchara.y + 38;
    var facing_it = (global.facing == 2) || (global.facing == 3 && kx > x) || (global.facing == 1 && kx < x);
    if (abs(kx - x) < 95 && ky > (y - 60) && ky < (y + 60) && facing_it)
    {
        global.interact = 1;
        if (state == 0)
        {
            state = 1;
            t = 0;
            snd_play(snd_power);
        }
        else
        {
            // dead for good
            cutscene_master = scr_cutscene_make();
            c_speaker("no_name");
            c_msgsetloc(0, "* (It won't turn on anymore.)/%", "es_arc_2");
            c_talk_wait();
            state = 5;
        }
    }
}
if (state == 1 && t == 25)
{
    state = 2;
    snd_play(snd_noise);
    cutscene_master = scr_cutscene_make();
    c_speaker("no_name");
    c_msgsetloc(0, "* WHY AREN'T YOU MOVING ON?/%", "es_arc_1");
    c_talk_wait();
}
if (state == 2 && !instance_exists(obj_cutscene_master))
{
    state = 3;
    t = 0;
    snd_pitch(snd_play(snd_power), 0.6);
}
if (state == 3 && t > 40)
{
    state = 4;
    global.interact = 0;
}
if (state == 5 && !instance_exists(obj_cutscene_master))
{
    state = 4;
    global.interact = 0;
}
