# Even Stronger - from the rollercoaster into the city, with Kris + Noelle

@@ gml_Object_obj_ch2_scene11a_Step_0  # rollercoaster: Queen cancels the capture, no Berdly coaster fight
@prepend
if (scr_es_on())
{
    scr_es_scene11a_step();
    exit;
}
@end

@@ gml_Object_obj_ch2_scene11b_Create_0  # trash landing: keep Noelle, don't bring Susie/Ralsei back
@find
scr_losechar();
if (instance_exists(obj_caterpillarchara))
{
instance_destroy(obj_caterpillarchara);
}
scr_getchar(2);
scr_getchar(3);
scr_makecaterpillar(obj_mainchara.x - 60, obj_mainchara.y, 2, 0);
scr_makecaterpillar(obj_mainchara.x - 140, obj_mainchara.y, 3, 1);
@replace
if (!scr_es_on())
{
    scr_losechar();
    if (instance_exists(obj_caterpillarchara))
        instance_destroy(obj_caterpillarchara);
    scr_getchar(2);
    scr_getchar(3);
    scr_makecaterpillar(obj_mainchara.x - 60, obj_mainchara.y, 2, 0);
    scr_makecaterpillar(obj_mainchara.x - 140, obj_mainchara.y, 3, 1);
}
@end

@@ gml_Object_obj_ch2_scene11b_Step_0  # trash landing, Noelle's version
@prepend
if (scr_es_on())
{
    scr_es_scene11b_step();
    exit;
}
@end

@@ gml_Object_obj_ch2_scene12_Step_0  # city split: nobody leaves
@prepend
if (scr_es_on())
{
    scr_es_scene12_step();
    exit;
}
@end

@@ gml_Object_obj_ch2_city01_Create_0  # Noelle never left, so don't drop her here
@find
else if (global.plot < 70)
{
scr_losechar();
if (instance_exists(obj_caterpillarchara))
{
instance_destroy(obj_caterpillarchara);
}
@replace
else if (global.plot < 70)
{
if (!scr_es_on())
{
    scr_losechar();
    if (instance_exists(obj_caterpillarchara))
        instance_destroy(obj_caterpillarchara);
}
@end

@@ gml_Object_obj_ch2_city01_Step_0  # Queen and Berdly back away from Kris + Noelle
@prepend
if (scr_es_on() && con != 0)
{
    scr_es_city01_step();
    exit;
}
@end

@@ gml_Object_obj_ch2_city_noelle_chase_Create_0  # Noelle is already with Kris: hide vanilla's Noelle running ahead (plot still advances)
@append
if (scr_es_on() && i_ex(noellemarker))
    noellemarker.visible = 0;
@end

@@ gml_Object_obj_queen_enemy_Create_0  # early Queen fight: Berdly already ran, he isn't plugged in as her battery (vanilla's weird-route version)
@find
if (scr_sideb_get_phase() < 2 && !instance_exists(obj_berdlyplug_enemy))
@replace
if (scr_sideb_get_phase() < 2 && !instance_exists(obj_berdlyplug_enemy) && !scr_es_on())
@end

@@ gml_Object_obj_ch2_city07_Create_0  # the carnival on reload: Susie and Ralsei never rejoined
@find
scr_losechar();
if (instance_exists(obj_caterpillarchara))
{
instance_destroy(obj_caterpillarchara);
}
scr_getchar(2);
scr_getchar(3);
scr_getchar(4);
scr_makecaterpillar(obj_mainchara.x + 60, obj_mainchara.y, 2, 0);
scr_makecaterpillar(obj_mainchara.x + 100, obj_mainchara.y, 4, 1);
scr_makecaterpillar(obj_mainchara.x + 140, obj_mainchara.y, 3, 2);
@replace
if (!scr_es_on())
{
    scr_losechar();
    if (instance_exists(obj_caterpillarchara))
        instance_destroy(obj_caterpillarchara);
    scr_getchar(2);
    scr_getchar(3);
    scr_getchar(4);
    scr_makecaterpillar(obj_mainchara.x + 60, obj_mainchara.y, 2, 0);
    scr_makecaterpillar(obj_mainchara.x + 100, obj_mainchara.y, 4, 1);
    scr_makecaterpillar(obj_mainchara.x + 140, obj_mainchara.y, 3, 2);
}
@end

@@ gml_Object_obj_ch2_city07_Step_0  # Susie and Ralsei back away, then the early Queen fight and her death
@prepend
if (scr_es_on() && global.plot < 95 && con != 0)
{
    scr_es_city07_step();
    exit;
}
@end
