draw_sprite_ext(spr_cutscene_10_arcade_off, 0, x, y, 2, 2, 0, c_white, 1);
var cx = (sx1 + sx2) / 2;
var cy = (sy1 + sy2) / 2;
var w = sx2 - sx1;
var h = sy2 - sy1;
// warming up: the picture blooms in from the middle
if (state == 1 || state == 2)
{
    var k = (state == 1) ? min(1, t / 25) : 1;
    var hh = h * k;
    draw_set_color(merge_color(c_black, make_color_rgb(20, 60, 30), 0.6 + (random(0.4))));
    draw_rectangle(cx - (w / 2), cy - (hh / 2), cx + (w / 2), cy + (hh / 2), false);
    // static and scanlines
    for (var i = 0; i < 30; i++)
    {
        draw_set_color(choose(c_lime, c_white, make_color_rgb(40, 140, 60)));
        draw_set_alpha(random(0.6));
        var px = (cx - (w / 2)) + random(w);
        var py = (cy - (hh / 2)) + random(hh);
        draw_rectangle(px, py, px + 1 + random(3), py, false);
    }
    draw_set_alpha(0.25);
    draw_set_color(c_black);
    for (var yy = cy - (hh / 2); yy < (cy + (hh / 2)); yy += 2)
        draw_line(cx - (w / 2), yy, cx + (w / 2), yy);
    draw_set_alpha(1);
    if (state == 2)
    {
        // Queen's face flickers through the static
        if ((t mod 7) < 5)
            draw_sprite_part_ext(spr_cutscene_10_arcade_on, 0, 37, 12, 35, 25, sx1, sy1, 2, 2, c_white, 0.35 + random(0.3));
    }
}
// power off: squash to a bright line, shrink to a dot, fade
if (state == 3 || (state == 4 && t < 60))
{
    var lh, lw, a;
    if (t < 8)
    {
        lh = max(2, h * (1 - (t / 8)));
        lw = w;
        a = 1;
    }
    else if (t < 16)
    {
        lh = 2;
        lw = max(2, w * (1 - ((t - 8) / 8)));
        a = 1;
    }
    else
    {
        lh = 2;
        lw = 2;
        a = max(0, 1 - ((t - 16) / 24));
    }
    draw_set_alpha(a);
    draw_set_color(c_white);
    draw_rectangle(cx - (lw / 2), cy - (lh / 2), cx + (lw / 2), cy + (lh / 2), false);
    draw_set_alpha(a * 0.4);
    draw_set_color(make_color_rgb(180, 255, 200));
    draw_rectangle(cx - (lw / 2) - 2, cy - (lh / 2) - 1, cx + (lw / 2) + 2, cy + (lh / 2) + 1, false);
    draw_set_alpha(1);
}
