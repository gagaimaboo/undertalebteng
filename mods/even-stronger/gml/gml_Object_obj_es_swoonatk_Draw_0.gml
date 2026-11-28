// the screen goes black while the cut lands
if (t >= 1 && t < 46)
{
    draw_set_color(c_black);
    draw_rectangle(camerax() - 10, cameray() - 10, camerax() + 650, cameray() + 490, false);
    draw_set_color(c_white);
}
