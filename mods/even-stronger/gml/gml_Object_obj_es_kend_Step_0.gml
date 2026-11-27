t++;
if (t == 1)
{
    // the fight stops here
    instance_deactivate_object(obj_battlecontroller);
    with (obj_es_kdbc)
        instance_destroy();
    with (obj_bulletparent)
        instance_destroy();
}
if (t == 60)
{
    with (obj_herokris)
    {
        sprite_index = spr_krisb_attack;
        image_index = 0;
        image_speed = 0.5;
    }
    snd_play(snd_criticalswing);
    var fx = instance_create(global.monsterx[0], global.monstery[0], obj_es_boomfx);
    fx.ang = [45, -45];
    fx.sc = 3;
}
if (t == 66)
{
    var dm = instance_create(global.monsterx[0], global.monstery[0] - 20, obj_dmgwriter);
    dm.damage = global.monsterhp[0];
    global.monsterhp[0] = 0;
    scr_es_snd("es_snd_knight_cut2", 1, 0.8);
    scr_es_snd("es_snd_knight_cut2", 1, 1);
    scr_es_snd("es_snd_knight_cut2", 1, 1.2);
    scr_es_shake(20, 20, 2);
}
if (t == 80 && i_ex(k))
{
    var spr = k.idlesprite;
    var d = instance_create(k.x - (sprite_get_xoffset(spr) * 2), k.y - (sprite_get_yoffset(spr) * 2), obj_deathanim);
    d.sprite_index = spr;
    d.image_index = 0;
    d.image_xscale = 2;
    d.image_yscale = 2;
    d.depth = k.depth;
    k.visible = 0;
    global.flag[2452] += 1;
}
if (t == 200)
{
    global.es_kfight = 0;
    global.es_kdone = 1;
    scr_es_battle_teardown();
    instance_destroy();
}
