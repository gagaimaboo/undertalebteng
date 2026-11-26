if (!scr_es_on())
    exit;
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
    }
    if (global.mnfight != 2)
        scr_es_addkrisacts();
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
