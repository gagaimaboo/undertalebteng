# Even Stronger - the teacup rides: every per-character sprite list gets a Noelle entry (index 4)

@@ gml_Object_obj_teacup_Other_10  # jump-in roll
@find
var _rollSprites = [spr_kris_fall_ball, spr_kris_fall_ball, spr_susie_dw_fall_ball, spr_ralsei_teacup_enter];
@replace
var _rollSprites = [spr_kris_fall_ball, spr_kris_fall_ball, spr_susie_dw_fall_ball, spr_ralsei_teacup_enter, scr_es_tc(3)];
@end

@@ gml_Object_obj_teacup_Step_0  # riding
@find
var _sprites = [spr_teacup_kris, spr_teacup_kris, spr_teacup_susie, spr_teacup_ralsei];
@replace
var _sprites = [spr_teacup_kris, spr_teacup_kris, spr_teacup_susie, spr_teacup_ralsei, scr_es_tc(0)];
@end

@@ gml_Object_obj_teacup_Step_0  # jumping out
@find
var _sprites = [spr_kris_fall_ball, spr_kris_fall_ball, spr_susie_dw_fall_ball, spr_ralsei_teacup_enter];
@replace
var _sprites = [spr_kris_fall_ball, spr_kris_fall_ball, spr_susie_dw_fall_ball, spr_ralsei_teacup_enter, scr_es_tc(3)];
@end

@@ gml_Object_obj_teacup_Step_0  # landing
@find
var _sprites = [spr_kris_dw_land_example_dark, spr_kris_dw_land_example_dark, spr_susie_dw_landed, spr_teacup_ralsei_land];
@replace
var _sprites = [spr_kris_dw_land_example_dark, spr_kris_dw_land_example_dark, spr_susie_dw_landed, spr_teacup_ralsei_land, scr_es_tc(4)];
@end

@@ gml_Object_obj_teacup_Step_0  # filling with tea
@find
var _spritesTea = [spr_teacup_tea, spr_teacup_kris_tea, spr_teacup_kris_tea, spr_teacup_ralsei_tea];
@replace
var _spritesTea = [spr_teacup_tea, spr_teacup_kris_tea, spr_teacup_kris_tea, spr_teacup_ralsei_tea, scr_es_tc(1)];
@end

@@ gml_Object_obj_teacup_Step_0  # more tea
@find
_spritesTea = [spr_teacup_tea2, spr_teacup_kris_tea2, spr_teacup_kris_tea, spr_teacup_ralsei_tea2];
@replace
_spritesTea = [spr_teacup_tea2, spr_teacup_kris_tea2, spr_teacup_kris_tea, spr_teacup_ralsei_tea2, scr_es_tc(2)];
@end
