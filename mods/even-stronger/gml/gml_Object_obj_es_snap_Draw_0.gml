var vx = camerax();
var vy = cameray();
if (red > 0)
{
    draw_set_alpha(red);
    draw_set_color(make_color_rgb(200, 0, 0));
    draw_rectangle(vx - 20, vy - 20, vx + 660, vy + 500, false);
    // everyone, as black silhouettes
    var sil = c_black;
    if (t < 230)
    with (obj_heroparent)
    {
        if (other.killed == 0 && sprite_index >= 0 && !(other.kpos >= 0 && object_index == obj_herokris))
            draw_sprite_ext(sprite_index, image_index, x, y, image_xscale, image_yscale, image_angle, sil, other.red);
    }
    for (var s = 0; s < 3; s++)
    {
        var m = global.monsterinstance[s];
        if (i_ex(m) && m.sprite_index >= 0 && global.monster[s] == 1)
        {
            var sh = (t > 30 && t < 150) ? random_range(-3, 3) : 0;
            draw_sprite_ext(m.sprite_index, m.image_index, m.x + sh, m.y, m.image_xscale, m.image_yscale, m.image_angle, sil, red);
        }
    }
    // the trio coming apart, still in silhouette
    with (obj_deathanim)
    {
        if (sprite_index >= 0)
            draw_sprite_ext(sprite_index, image_index, x, y, image_xscale, image_yscale, 0, sil, other.red * image_alpha);
    }
    // Kris, blinking from enemy to enemy
    if (kpos >= 0 && i_ex(obj_herokris))
    {
        var kx = global.monsterx[kpos] - 130;
        var ky = global.monstery[kpos] - 40;
        draw_sprite_ext(spr_krisb_attack, (t div 2) mod 4, kx, ky, 2, 2, 0, sil, red);
    }
    draw_set_alpha(1);
}
// the cuts
draw_set_color(c_white);
for (var i = 0; i < array_length(slashes); i++)
{
    var sl = slashes[i];
    draw_set_alpha(sl[4] / 10);
    draw_line_width(sl[0], sl[1], sl[2], sl[3], 2 + (sl[4] / 3));
}
draw_set_alpha(1);
if (xslash > 0)
{
    draw_set_alpha(xslash);
    draw_line_width(vx + 380, vy + 20, vx + 620, vy + 300, 10 * xslash + 2);
    draw_line_width(vx + 620, vy + 20, vx + 380, vy + 300, 10 * xslash + 2);
    draw_set_alpha(1);
}
