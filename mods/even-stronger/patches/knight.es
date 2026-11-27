# Even Stronger - the rollercoaster: Berdly's car fight, the Roaring Knight, BlackKnife

@@ gml_GlobalScript_scr_encountersetup  # encounter 240: the Roaring Knight
@find
case 59:
@before
case 240:
    scr_es_knight_encounter();
    break;
@end

@@ gml_GlobalScript_scr_weaponinfo  # weapon 23: BlackKnife, the knight's sword
@find
case 22:
@before
case 23:
    scr_es_blackknife_info();
    break;
@end

@@ gml_Object_o_coaster_controller_Create_0  # coaster fight: Noelle rides in the second car, no third car
@find
jamatime = 90;
@before
if (scr_es_on())
{
    if (instance_exists(obj_heronoelle))
        obj_heronoelle.x = camerax() - 200;
    HeroCoaster[1].sprite_index = spr_noelleb_idle;
    with (HeroCoaster[2])
    {
        visible = 0;
        with (back)
            visible = 0;
    }
}
@end

@@ gml_Object_o_coaster_hero_Step_0  # coaster fight: the third car stays empty and hidden
@prepend
if (scr_es_on() && HeroID == 2)
{
    visible = 0;
    exit;
}
@end

@@ gml_Object_o_coaster_hero_Step_0  # coaster fight: the second car shows Noelle's battle sprite
@find
if (HeroID == 2 && forceact == 0 && instance_exists(obj_heroralsei))
@before
if (scr_es_on() && HeroID == 1 && instance_exists(obj_heronoelle))
{
    sprite_index = obj_heronoelle.sprite_index;
    image_index = obj_heronoelle.image_index - 1;
    obj_heronoelle.x = camerax() - 200;
}
@end

@@ gml_Object_o_coaster_hero_back_Draw_0  # coaster fight: no third car, and Noelle's sleigh has no back piece
@prepend
if (scr_es_on() && i_ex(parentid) && (parentid.HeroID == 2 || parentid.HeroID == 1))
    exit;
@end

@@ gml_Object_o_coaster_hero_Draw_0  # coaster fight: Noelle rides her own ice sleigh (vanilla's, mirrored to face right)
@find
if (hurt == 0)
{
draw_sprite_ext(spr_coaster_hero, HeroID, x, y + 8, image_xscale, image_yscale, 0, c_white, image_alpha);
}
if (hurt == 1)
{
draw_sprite_ext(spr_coaster_hero_hurt, HeroID, x, y + 8, image_xscale, image_yscale, 0, c_white, image_alpha);
}
@replace
if (scr_es_on() && HeroID == 1)
{
    draw_sprite_ext(spr_noelle_coaster_front, 0, x + 126, y - 58, -image_xscale, image_yscale, 0, hurt ? c_red : c_white, image_alpha);
}
else
{
    if (hurt == 0)
        draw_sprite_ext(spr_coaster_hero, HeroID, x, y + 8, image_xscale, image_yscale, 0, c_white, image_alpha);
    if (hurt == 1)
        draw_sprite_ext(spr_coaster_hero_hurt, HeroID, x, y + 8, image_xscale, image_yscale, 0, c_white, image_alpha);
}
@end

@@ gml_Object_obj_coaster_Draw_0  # the ride: Noelle's car is her own ice sleigh, mirrored to face right
@prepend
if (scr_es_on() && variable_instance_exists(id, "es_sleigh") && es_sleigh == 1 && actor != -1)
{
    if (animate)
        siner++;
    draw_sprite_ext(actor.sprite_index, actor.image_index, x + sinerx + character_offset_x, y + sinery + character_offset_y, 2, 2, 0, c_white, 1);
    draw_sprite_ext(spr_noelle_coaster_front, 0, x + sinerx + 118, y + sinery + 31, -2, 2, 0, c_white, 1);
    exit;
}
@end

@@ gml_Object_obj_berdlyb_enemy_Create_0  # Berdly's weird-route look
@find
idlesprite = spr_berdlyb_idle;
@replace
idlesprite = scr_es_on() ? spr_berdlyb_idle_serious : spr_berdlyb_idle;
@end

@@ gml_Object_o_coaster_berdly_Step_0  # Berdly's weird-route look (his coaster)
@find
sprite_index = spr_berdlyb_idle;
@replace
sprite_index = scr_es_on() ? spr_berdlyb_idle_serious : spr_berdlyb_idle;
@end

@@ gml_Object_obj_ch2_city_berdly_Create_0  # Berdly died on the rollercoaster: his city scene never happens
@prepend
if (scr_es_on() && global.chapter == 2 && global.plot < 79)
    global.plot = 79;
@end

@@ gml_Object_obj_heroparent_Alarm_1  # BlackKnife slashes like Chapter 3's BlackShard
@find
if (points == 150)
{
attack.image_xscale = 2.5;
attack.image_yscale = 2.5;
}
@before
if (scr_es_on() && global.charweapon[global.char[myself]] == 23)
{
    attack.sprite_index = scr_es_spr("es_attack_shard");
    attack.image_speed = 0.334;
}
@end
