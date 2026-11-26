# dev only: drive the game from es_test.txt (built with --test)
@@ gml_Object_obj_time_Step_1  # harness
@prepend
scr_es_test_tick();
@end
