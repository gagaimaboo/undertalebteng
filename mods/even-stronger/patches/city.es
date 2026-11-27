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
