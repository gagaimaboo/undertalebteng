# Even Stronger - the Roaring Knight's real fight (copied from Chapter 3 by transplant.txt), adjusted

@@ gml_Object_obj_knight_enemy_Step_0  # it only fights Kris + Susie + Ralsei in Chapter 3: let it fight Kris + Noelle
@find
if (!i_ex(obj_herosusie) || !i_ex(obj_heroralsei))
@replace
if (!scr_es_on() && (!i_ex(obj_herosusie) || !i_ex(obj_heroralsei)))
@end

@@ gml_Object_obj_knight_enemy_Step_0  # no Susie here to talk back to it
@find
if (createballoon)
@replace
if (scr_es_on())
    createballoon = false;
if (createballoon)
@end

@@ gml_Object_obj_knight_enemy_Step_0 all  # its last phase starts at 50% instead of 80%
@find
if (global.monsterhp[myself] <= (global.monstermaxhp[myself] * 0.8))
@replace
if (global.monsterhp[myself] <= (global.monstermaxhp[myself] * (scr_es_on() ? 0.5 : 0.8)))
@end

@@ gml_Object_obj_knight_enemy_Step_0  # its last phase starts at 50% instead of 80%
@find
if (global.monsterhp[myself] <= (global.monstermaxhp[myself] * 0.8) && haveusedroaring == false && phase != 4)
@replace
if (global.monsterhp[myself] <= (global.monstermaxhp[myself] * (scr_es_on() ? 0.5 : 0.8)) && haveusedroaring == false && phase != 4)
@end

@@ gml_Object_obj_knight_enemy_Draw_0  # the fight ends at 50% instead of 80%
@find
if (haveusedroaring == true && end_cutscene_version == 0 && global.monsterhp[myself] <= (global.monstermaxhp[myself] * 0.8) && endcon != 1)
@replace
if (haveusedroaring == true && end_cutscene_version == 0 && global.monsterhp[myself] <= (global.monstermaxhp[myself] * (scr_es_on() ? 0.5 : 0.8)) && endcon != 1)
@end

@@ gml_Object_obj_knight_enemy_Step_0  # 50% catches it off guard: the fight ends (Kris cuts it down afterwards, on the track)
@find
if (endcon == 1 && endtimer > 45)
@replace
if (scr_es_on() && endcon == 1 && endtimer > 45)
{
    endcon = 2;
    global.es_kfight = 0;
    global.es_kdone = 1;
    scr_es_battle_teardown();
    exit;
}
if (!scr_es_on() && endcon == 1 && endtimer > 45)
@end

@@ gml_Object_obj_knight_enemy_Other_22  # its stats for a Chapter 2 party
@append
if (scr_es_on())
    scr_es_knight_stats();
@end

@@ gml_GlobalScript_scr_damage  # the knight's attacks are tuned for a Chapter 3 party (206 a hit): 30% against this one
@find
tdamage = ceil(tdamage * scr_element_damage_reduction(__element, global.char[target]));
@after
if (scr_es_knightfight())
    tdamage = max(1, ceil(tdamage * 0.3));
@end
