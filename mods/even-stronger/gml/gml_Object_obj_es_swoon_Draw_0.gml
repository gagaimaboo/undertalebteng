var st = min(1, t / 6);
var kill = max(0, (t - 100) / 30);
var xs = 2 + ((1 - st) * 1.5);
var ys = (2 * st) + (kill * 2);
draw_sprite_ext(scr_es_spr("es_battlemsg3"), 13, x + 30, y - (t * 0.15), xs, ys, 0, c_red, 1 - kill);
