if (!scr_es_on())
    exit;
if (room != lastroom)
{
    lastroom = room;
    roomtimer = 0;
}
roomtimer++;
if (roomtimer == 2)
    scr_es_roomstart();
scr_es_cyber_music_tick();
if (global.flag[2455] == 1)
{
    with (obj_caterpillarchara)
    {
        if (name == "noelle" && dsprite != spr_noelle_walk_down_sad_dw)
            scr_es_noelle_sad_sprites();
    }
}
// enemies come in with their guard up: +20% HP (Queen gets +10% in her own fight)
if (instance_exists(obj_battlecontroller) && global.fighting == 1)
{
    if (buffed == 0)
    {
        buffed = 1;
        var mul = (global.flag[2451] == 1) ? 1.1 : 1.2;
        for (var i = 0; i < 3; i++)
        {
            if (global.monster[i] == 1 && global.monsterhp[i] > 0 && global.monsterhp[i] >= global.monstermaxhp[i])
            {
                global.monstermaxhp[i] = ceil(global.monstermaxhp[i] * mul);
                global.monsterhp[i] = global.monstermaxhp[i];
            }
        }
        scr_es_setspells();
        // BlackKnife: every battle starts at 40% TP
        if (scr_es_blackknife())
            global.tension = max(global.tension, round(global.maxtension * 0.4));
    }
    if (global.mnfight != 2)
        scr_es_addkrisacts();
    // Queen warned everyone about you: every enemy is on guard, like Ralsei's Warn in Chapter 1
    for (var w = 0; w < 3; w++)
    {
        if (global.monster[w] == 1 && global.monstercomment[w] != "(Warned)")
            global.monstercomment[w] = "(Warned)";
    }
    // count the trio's turns (Kris snaps after three)
    if (instance_exists(obj_sweet_enemy) && variable_global_exists("es_sweet_turns"))
    {
        if (!variable_instance_exists(id, "lastmn"))
            lastmn = global.mnfight;
        if (lastmn == 2 && global.mnfight != 2)
            global.es_sweet_turns++;
        lastmn = global.mnfight;
    }
    // ...and attack 5% faster (3% for Queen)
    var want = (global.mnfight == 2) ? ((global.flag[2451] == 1) ? 30.9 : 31.5) : 30;
    if (room_speed != want && (want != 30 || fast == 1))
    {
        room_speed = want;
        fast = (want != 30);
    }
}
else
{
    buffed = 0;
    if (variable_global_exists("es_flv"))
        global.es_flv = "";
    if (fast == 1)
    {
        room_speed = 30;
        fast = 0;
    }
}
