if (dark > 0)
{
    draw_set_alpha(dark);
    draw_set_color(c_black);
    draw_rectangle(camerax() - 10, cameray() - 10, camerax() + 650, cameray() + 490, false);
    draw_set_alpha(1);
}
if (flash > 0)
{
    draw_set_alpha(flash);
    draw_set_color(c_red);
    draw_rectangle(camerax() - 10, cameray() - 10, camerax() + 650, cameray() + 490, false);
    draw_set_alpha(1);
}
