siner += 1;
for (var i = 0; i < array_length(parts); i++)
{
    if (!i_ex(parts[i]))
        continue;
    var p = parts[i];
    if (p.object_index == obj_forcefield || !p.visible || p.sprite_index < 0 || (p.x == 0 && p.y == 0))
        continue;
    draw_set_alpha(0.28 + (sin((siner / 30) + i) * 0.05));
    draw_set_color(merge_color(c_aqua, c_white, 0.4));
    draw_rectangle(p.bbox_left, p.bbox_top, p.bbox_right, p.bbox_bottom, false);
    draw_set_alpha(1);
    draw_sprite_ext(spr_icespell_snowflake, 0, p.x + (p.sprite_width / 2), p.y + (p.sprite_height / 2), 0.5, 0.5, siner + (i * 40), c_white, 0.55);
}
