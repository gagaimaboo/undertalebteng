var a = (t < 30) ? 1 : max(0, 1 - ((t - 30) / 60));
draw_set_alpha(a);
draw_set_color(c_white);
draw_rectangle(camerax() - 10, cameray() - 10, camerax() + 650, cameray() + 490, false);
draw_set_alpha(1);
