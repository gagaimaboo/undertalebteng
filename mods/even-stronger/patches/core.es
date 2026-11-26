# Even Stronger - hooks into the game's own code. Each edit is found by content, not position.

@@ gml_Object_obj_time_Step_1  # controller
@prepend
if (!instance_exists(obj_es_ctrl))
    instance_create(0, 0, obj_es_ctrl);
@end

@@ gml_Object_obj_heroparent_Alarm_1  # fight damage
@find
damage = round(((global.battleat[myself] * points) / 20) - (global.monsterdf[global.chartarget[myself]] * 3));
@after
if (scr_es_on())
    damage = scr_es_fightdmg(damage);
@end

@@ gml_GlobalScript_scr_spellinfo  # spell info
@find
switch (arg0)
@before
if (scr_es_on() && scr_es_spellinfo(arg0))
    exit;
@end

@@ gml_GlobalScript_scr_spelltext  # spell text
@find
switch (spell)
@before
if (scr_es_on() && scr_es_spelltext(spell, caster))
    exit;
@end

@@ gml_GlobalScript_scr_spell  # spell effects
@find
switch (spell)
@before
if (scr_es_on() && scr_es_spell(spell, caster))
    exit;
@end

@@ gml_GlobalScript_scr_spell  # iceshock extra damage at Queen
@find
damage = ceil((minbattlemag * 30) + 90 + random(10));
@after
if (scr_es_on())
    damage = ceil(damage * scr_es_abilitymul());
@end

@@ gml_GlobalScript_scr_monsterdefeat  # count kills
@find
global.monster[myself] = 0;
@before
if (scr_es_on())
    scr_es_countkill(myself);
@end

@@ gml_Object_DEVICE_MENU_Step_0  # new file starts at the library
@find
if (global.chapter >= 2)
{
room_goto(room_krisroom);
}
@replace
if (global.chapter >= 2)
{
    if (scr_es_on())
    {
        scr_es_newgame();
        global.plot = 17;
        room_goto(room_library);
    }
    else
    {
        room_goto(room_krisroom);
    }
}
@end

@@ gml_Object_obj_ch2_scene8_Create_0  # library scene
@prepend
if (scr_es_on() && global.chapter == 2 && global.plot < 20)
{
    instance_create(0, 0, obj_es_library);
    instance_destroy();
    exit;
}
@end

@@ gml_Object_obj_ch2_room_cyber_intro_1_Create_0  # landing
@prepend
if (scr_es_on() && global.plot < 50)
{
    instance_create(0, 0, obj_es_land);
    instance_destroy();
    exit;
}
@end

@@ gml_Object_DEVICE_NAMER_Draw_0  # new file (after naming) starts at the library
@find
scr_initialize_charnames();
room_goto(room_krisroom);
@replace
scr_initialize_charnames();
if (scr_es_on())
{
    scr_es_newgame();
    global.plot = 17;
    room_goto(room_library);
}
else
{
    room_goto(room_krisroom);
}
@end

@@ gml_GlobalScript_scr_actselect  # kris skills in the act menu
@find
function scr_actselect(arg0, arg1)
{
@after
if (scr_es_on() && scr_es_actselect(arg0, arg1))
    exit;
@end

@@ gml_Object_obj_ch2_scene9_Step_0  # Queen's entrance + first fight, with Noelle
@prepend
if (scr_es_on())
{
    scr_es_scene9_step();
    exit;
}
@end

@@ gml_GlobalScript_snd_init  # encounter battle theme: SWORD (FM Cover) by plrusek
@find
initsongvar = dir + arg0;
@before
if (scr_es_on() && arg0 == "battle.ogg" && file_exists(working_directory + "even_stronger/es_battle.ogg"))
{
    dir = working_directory + "even_stronger/";
    arg0 = "es_battle.ogg";
}
@end

@@ gml_Object_obj_battlecontroller_Step_0  # battle box text: Hold. / Its burning.. / You are filled with power.
@find
if (!i_ex(battlewriter) || (global.chapter == 2 && !i_ex(battlewriter) && gigaqueencon == 0))
{
global.msg[0] = global.battlemsg[0];
@replace
if (!i_ex(battlewriter) || (global.chapter == 2 && !i_ex(battlewriter) && gigaqueencon == 0))
{
global.msg[0] = global.battlemsg[0];
if (scr_es_on())
    global.msg[0] = scr_es_flavor();
@end

@@ gml_GlobalScript_scr_text  # the mod's own dialogue entries (choice follow-ups)
@find
function scr_text(arg0)
{
@after
if (scr_es_on() && scr_es_text(arg0))
    exit;
@end
