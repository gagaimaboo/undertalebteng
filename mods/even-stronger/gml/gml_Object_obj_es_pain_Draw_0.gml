var vx = camerax();
var vy = cameray();
if (phase <= 1)
{
    draw_set_alpha(0.75);
    draw_set_color(c_black);
    draw_rectangle(vx, vy, vx + 640, vy + 480, false);
    draw_set_alpha(1);
    draw_set_color(c_white);
    draw_rectangle(vx + 80, vy + 120, vx + 560, vy + 300, false);
    draw_set_color(c_black);
    draw_rectangle(vx + 86, vy + 126, vx + 554, vy + 294, false);
    draw_set_font(fnt_mainbig);
    draw_set_color(c_white);
    draw_set_halign(fa_center);
    if (phase == 0)
    {
        draw_text(vx + 320, vy + 150, "After you use this spell");
        draw_text(vx + 320, vy + 180, "you will be granted: Pain.");
        draw_text(vx + 320, vy + 210, "do you choose to proceed?");
        draw_set_halign(fa_left);
        draw_set_color((sel == 0) ? c_yellow : c_white);
        draw_text(vx + 220, vy + 256, "Yes");
        draw_set_color((sel == 1) ? c_yellow : c_white);
        draw_text(vx + 380, vy + 256, "Yes");
        draw_sprite_ext(spr_heart, 0, vx + ((sel == 0) ? 196 : 356), vy + 264, 1, 1, 0, c_red, 1);
    }
    else
    {
        var s = string_copy("...Alright, i guess.", 1, floor(t / 2));
        draw_text(vx + 320, vy + 195, s);
    }
    draw_set_halign(fa_left);
}
if (phase >= 2)
{
    var fr = (dogx > camerax() + 311 || phase == 3) ? ((t div 4) mod 2) : 0;
    var sy = (barked > 0 && phase == 2 && (t mod 18) < 6) ? -6 : 0;
    draw_sprite_ext(spr_dog_walk, fr, dogx, vy + 300 + sy, (phase == 3) ? 2 : -2, 2, 0, c_white, 1);
}
