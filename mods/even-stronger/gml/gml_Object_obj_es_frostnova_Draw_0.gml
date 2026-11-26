if (t < 40)
    draw_sprite_ext(spr_icespell_snowflake, 0, cx, cy, bigscale, bigscale, spin, c_white, min(1, t / 12));
for (var i = 0; i < array_length(pieces); i++)
{
    var p = pieces[i];
    draw_sprite_ext(spr_icespell_snowflake, 0, p[0], p[1], 1.6, 1.6, p[4], c_white, 1);
}
for (var i = 0; i < array_length(rain); i++)
{
    var r = rain[i];
    draw_sprite_ext(spr_icespell_snowflake, 0, r[0], r[1], r[4], r[4], r[3], merge_color(c_white, c_aqua, 0.3), 0.9);
}
if (flash > 0)
{
    draw_set_alpha(flash * 0.6);
    draw_set_color(c_white);
    draw_rectangle(camerax(), cameray(), camerax() + 640, cameray() + 480, false);
    draw_set_alpha(1);
}
