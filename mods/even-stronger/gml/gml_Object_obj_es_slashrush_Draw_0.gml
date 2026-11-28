if (t > 8 && t <= (8 + window) && hits < maxhits)
{
    draw_set_font(fnt_mainbig);
    draw_set_halign(fa_center);
    var c = ((t div 4) mod 2) ? c_yellow : c_white;
    draw_text_color(320 + camerax(), 150 + cameray(), "MASH Z!", c, c, c, c, 1);
    draw_set_color(c_black);
    draw_rectangle(220 + camerax(), 185 + cameray(), 420 + camerax(), 195 + cameray(), false);
    draw_set_color(c_red);
    draw_rectangle(220 + camerax(), 185 + cameray(), 220 + camerax() + (200 * (1 - ((t - 8) / window))), 195 + cameray(), false);
    draw_set_color(c_white);
    draw_text(320 + camerax(), 200 + cameray(), string(hits) + " HITS");
    draw_set_halign(fa_left);
}
