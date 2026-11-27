# Even Stronger - Cyber Field, played with Kris + Noelle

@@ gml_Object_obj_musicalslide_Step_2  # the slide: Noelle in Susie's place, no Ralsei (vanilla crashed reading susY)
@find
slideSus = instance_create(susX, susY, obj_marker);
slideRal = instance_create(ralX, ralY, obj_marker);
@before
if (scr_es_on())
{
    susX = obj_mainchara.x - 40;
    susY = obj_mainchara.y;
    with (obj_caterpillarchara)
    {
        if (name == "noelle")
        {
            other.susX = x;
            other.susY = y;
            visible = false;
        }
    }
    ralX = susX;
    ralY = susY;
}
@end

@@ gml_Object_obj_musicalslide_Step_2  # the slide: Noelle's sprite, Ralsei hidden
@find
snd_play(snd_noise);
slideSound = snd_loop(snd_paper_surf);
@before
if (scr_es_on())
{
    with (slideSus)
    {
        sprite_index = spr_noelle_walk_down_dw;
        image_speed = 0;
    }
    with (slideRal)
        visible = false;
}
@end

@@ gml_Object_obj_musicer_cyber_Create_0  # the field's music: slowed, and gone once it's empty
@find
if (!snd_is_playing(global.currentsong[1]))
{
global.currentsong[0] = snd_init("cyber.ogg");
global.currentsong[1] = mus_loop_ext(global.currentsong[0], 1, 1);
}
@replace
if (scr_es_on())
{
    if (global.plot >= 60)
    {
        if (!snd_is_playing(global.currentsong[1]))
        {
            global.currentsong[0] = snd_init("spamton_neo_meeting.ogg");
            global.currentsong[1] = mus_loop_ext(global.currentsong[0], 1, 1);
        }
    }
    else if (!scr_es_cyber_music_gone())
    {
        if (!variable_global_exists("es_cybersong") || !audio_is_playing(global.es_cybersong))
        {
            if (!snd_is_playing(global.currentsong[1]))
            {
                global.currentsong[0] = snd_init("cyber.ogg");
                global.currentsong[1] = mus_loop_ext(global.currentsong[0], 1, 0.82);
            }
            global.es_cybersong = global.currentsong[1];
        }
    }
}
else if (!snd_is_playing(global.currentsong[1]))
{
    global.currentsong[0] = snd_init("cyber.ogg");
    global.currentsong[1] = mus_loop_ext(global.currentsong[0], 1, 1);
}
@end

@@ gml_Object_obj_ch2_scene10_Create_0  # the arcade room: no Queen boxing (it needs Susie + Ralsei)
@prepend
if (scr_es_on())
{
    instance_create(325, 200, obj_es_arcade);
    if (global.plot < 55)
        instance_create(0, 0, obj_es_scene10);
    instance_destroy();
    exit;
}
@end

@@ gml_Object_obj_ch2_cyber01_Step_0  # Sweet Cap'n Cakes: Kris + Noelle, and the snap
@prepend
if (scr_es_on())
{
    scr_es_sweet_step();
    exit;
}
@end

@@ gml_Object_obj_controller_dw_cyber_music_final_Step_0  # the platform ride: park Noelle where Susie/Ralsei went
@find
c_sel(su);
c_visible(false);
c_setxy(1710, 174);
c_sel(ra);
c_visible(false);
c_setxy(1812, 174);
@replace
if (scr_es_on())
{
    if (variable_instance_exists(id, "no"))
    {
        c_sel(no);
        c_visible(false);
        c_setxy(1710, 174);
    }
}
else
{
    c_sel(su);
    c_visible(false);
    c_setxy(1710, 174);
    c_sel(ra);
    c_visible(false);
    c_setxy(1812, 174);
}
@end

@@ gml_Object_obj_controller_dw_cyber_post_music_boss_slide_Step_0  # after Sweet: no trio flyby (they're dead), no Susie/Ralsei catch-up (vanilla crashed reading realsus)
@prepend
if (scr_es_on())
{
    if (con == 0 && obj_mainchara.y > 700 && obj_mainchara.x > 1900 && global.flag[408] == 0)
        global.flag[408] = 1;
    exit;
}
@end
