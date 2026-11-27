if (a <= 0)
    exit;
var px = camerax() + 120;
var py = cameray() + 24;
draw_set_alpha(a * 0.85);
draw_set_color(c_black);
draw_rectangle(camerax() - 10, cameray() - 10, camerax() + 650, cameray() + 300, false);
draw_set_alpha(a);
// frame
draw_set_color(c_white);
draw_rectangle(px - 4, py - 4, px + 403, py + 263, false);
if (phase < 3)
{
    // night sky
    draw_rectangle_color(px, py, px + 399, py + 259, make_color_rgb(8, 10, 40), make_color_rgb(8, 10, 40), make_color_rgb(30, 20, 70), make_color_rgb(30, 20, 70), false);
    // skyline
    var bx = px;
    for (var i = 0; i < bn; i++)
    {
        draw_set_color(make_color_rgb(14, 30, 70));
        draw_rectangle(bx, py + 259 - bh[i], bx + bw[i], py + 259, false);
        bx += bw[i] + 4;
        if (bx > px + 399)
            break;
    }
    // street
    draw_set_color(make_color_rgb(20, 20, 30));
    draw_rectangle(px, py + 230, px + 399, py + 259, false);
    // the lights
    for (var i = 0; i < ln; i++)
    {
        if (lon[i])
        {
            var fl = 0.7 + (0.3 * sin((t + i * 7) / 9));
            draw_set_alpha(a * fl);
            draw_set_color(lc[i]);
            draw_rectangle(px + lx[i], py + ly[i], px + lx[i] + 2, py + ly[i] + 2, false);
        }
        else
        {
            draw_set_alpha(a * 0.6);
            draw_set_color(make_color_rgb(150, 210, 255));
            draw_rectangle(px + lx[i], py + ly[i], px + lx[i] + 2, py + ly[i] + 2, false);
        }
    }
    draw_set_alpha(a);
}
else
{
    // all of it under snow, one small figure in the middle
    draw_set_color(make_color_rgb(235, 240, 250));
    draw_rectangle(px, py, px + 399, py + 259, false);
    draw_set_color(make_color_rgb(200, 210, 230));
    var bx = px;
    for (var i = 0; i < bn; i++)
    {
        draw_rectangle(bx, py + 259 - (bh[i] * 0.6), bx + bw[i], py + 259, false);
        bx += bw[i] + 4;
        if (bx > px + 399)
            break;
    }
    draw_set_color(c_black);
    draw_rectangle(px + 198, py + 200, px + 201, py + 212, false);
    draw_rectangle(px + 197, py + 196, px + 202, py + 200, false);
}
// snow falls on everything once the lights start going out
if (phase >= 2)
{
    draw_set_color(c_white);
    for (var i = 0; i < 60; i++)
    {
        var sxx = px + ((sx[i] + 400) mod 400);
        draw_rectangle(sxx, py + sy[i], sxx + 1, py + sy[i] + 1, false);
    }
}
draw_set_alpha(1);
draw_set_color(c_white);
