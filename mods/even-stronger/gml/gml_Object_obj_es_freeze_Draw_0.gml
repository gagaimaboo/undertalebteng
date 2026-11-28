// Noelle's ice over the puzzle
if (con == 3 && timer < 45)
{
    var k = timer / 45;
    for (var i = 0; i < array_length(bursts); i++)
    {
        var b = bursts[i];
        var sc = 0.4 + (k * 2.2);
        draw_sprite_ext(spr_icespell_snowflake, 0, b[0], b[1], sc, sc, timer * 9, c_white, 1 - (k * 0.7));
        for (var j = 0; j < 6; j++)
        {
            var d = (j * 60) + (timer * 4);
            var r = 10 + (k * 60);
            draw_sprite_ext(spr_icespell_snowflake, 0, b[0] + lengthdir_x(r, d), b[1] + lengthdir_y(r, d), 0.35, 0.35, -timer * 12, merge_color(c_aqua, c_white, 0.5), 1 - k);
        }
    }
    if (timer > 20 && timer < 34)
    {
        draw_set_alpha((34 - timer) / 20);
        draw_set_color(merge_color(c_aqua, c_white, 0.6));
        draw_rectangle(camerax(), cameray(), camerax() + 640, cameray() + 480, false);
        draw_set_alpha(1);
    }
}
