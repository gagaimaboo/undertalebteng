var a = (t < 6) ? 1 : max(0, 1 - ((t - 6) / (life - 6)));
var gx = grow ? min(1, t / 4) : 1;
for (var i = 0; i < array_length(ang); i++)
    draw_sprite_ext(scr_es_spr("es_rk_whiteslash"), 0, x, y, sc * gx, sc, ang[i], c_white, a);
