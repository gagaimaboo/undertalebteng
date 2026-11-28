# Even Stronger - Noelle after her first kill: the weird route's sprites

@@ gml_Object_obj_heroparent_Create_0  # battle: idle/defend/hurt sideb sprites
@find
if (scr_sideb_get_phase() >= 2)
{
_sideb = 1;
}
@replace
if (scr_sideb_get_phase() >= 2 || scr_es_noelle_changed())
{
    _sideb = 1;
}
@end

@@ gml_GlobalScript_scr_makecaterpillar  # overworld: following Kris
@find
dsprite = spr_noelle_walk_down_blush_dw;
rsprite = spr_noelle_walk_right_blush_dw;
lsprite = spr_noelle_walk_left_blush_dw;
}
@after
if (scr_es_noelle_changed())
    scr_es_noelle_sad_sprites();
@end

@@ gml_GlobalScript_scr_set_facing_sprites  # cutscene actors
@find
if (_actorname == "noelledark")
{
usprite = spr_noelle_walk_up_dw;
lsprite = spr_noelle_walk_left_dw;
rsprite = spr_noelle_walk_right_dw;
dsprite = spr_noelle_walk_down_dw;
@after
if (scr_es_noelle_changed())
    scr_es_noelle_sad_sprites();
@end
