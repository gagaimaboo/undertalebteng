var st = 0.2 + (0.8 * min(1, t / 8));
var kill = max(0, (t - 100) / 30);
draw_sprite_ext(scr_es_spr("es_battlemsg3"), 13, x + 30, y - (t * 0.15), 2 - st, st + kill, 0, c_red, 1 - kill);
