// Even Stronger: Noelle's teacup sprites. The teacup ride indexes its sprite lists by character
// (Kris 1, Susie 2, Ralsei 3) and crashed on Noelle (4). Her sprites are composed once at run time
// from the game's own art: the tea cup, her walking frames, and the cup's front lip redrawn over her.
//   scr_es_tc(0) cup   1 cup with tea   2 cup with more tea   3 jump-in roll   4 landing

function scr_es_tc(argument0)
{
    if (!variable_global_exists("es_tc_spr"))
        scr_es_tc_build();
    return global.es_tc_spr[argument0];
}

function scr_es_tc_build()
{
    global.es_tc_spr = [spr_teacup_kris, spr_teacup_kris_tea, spr_teacup_kris_tea2, spr_kris_fall_ball, spr_noelle_walk_down_dw];
    var W = 44;
    var H = 55;
    var OY = 12;   // extra room above the cup for her antlers
    var faces = [spr_noelle_walk_down_dw, spr_noelle_walk_right_dw, spr_noelle_walk_up_dw, spr_noelle_walk_left_dw];
    var bases = [spr_teacup_tea, spr_teacup_tea, spr_teacup_tea2];
    var surf = surface_create(W, H);
    for (var v = 0; v < 3; v++)
    {
        var spr = -1;
        for (var f = 0; f < 4; f++)
        {
            surface_set_target(surf);
            draw_clear_alpha(c_black, 0);
            // the cup (its origin is 13,0)
            draw_sprite(bases[v], f, 13, OY);
            // empty cup: paint over the tea with the cup's inside color
            if (v == 0)
            {
                draw_set_color(make_color_rgb(197, 180, 194));
                draw_ellipse(10, OY + 23, 34, OY + 27, false);
            }
            // Noelle sitting in it
            draw_sprite(faces[f], 0, 11, 0);
            // the front of the cup over her
            draw_sprite_part(bases[v], f, 0, 28, 44, 15, 0, OY + 28);
            surface_reset_target();
            if (f == 0)
                spr = sprite_create_from_surface(surf, 0, 0, W, H, false, false, 13, OY);
            else
                sprite_add_from_surface(spr, surf, 0, 0, W, H, false, false);
        }
        if (spr >= 0)
            global.es_tc_spr[v] = spr;
    }
    surface_free(surf);
    // the jump-in roll: Noelle curled into a spinning ball, sized like Kris's
    var rs = surface_create(29, 50);
    var roll = -1;
    for (var f = 0; f < 4; f++)
    {
        surface_set_target(rs);
        draw_clear_alpha(c_black, 0);
        var cx = 14;
        var cy = 34;
        draw_set_color(c_black);
        draw_circle(cx, cy, 12, false);
        draw_set_color(make_color_rgb(255, 236, 160));
        draw_circle(cx, cy, 11, false);
        var a = f * 90;
        var s = 0.5;
        var hw = (23 * s) / 2;
        var hh = (48 * s) / 2;
        var rx = (-hw * dcos(a)) + (-hh * dsin(a));
        var ry = (hw * dsin(a)) + (-hh * dcos(a));
        draw_sprite_ext(spr_noelle_walk_down_dw, 0, cx + rx, cy + ry, s, s, a, c_white, 1);
        draw_set_color(c_white);
        draw_set_alpha(0.5);
        draw_circle(cx - 4, cy - 5, 3, false);
        draw_set_alpha(1);
        surface_reset_target();
        if (f == 0)
            roll = sprite_create_from_surface(rs, 0, 0, 29, 50, false, false, 0, 0);
        else
            sprite_add_from_surface(roll, rs, 0, 0, 29, 50, false, false);
    }
    surface_free(rs);
    if (roll >= 0)
        global.es_tc_spr[3] = roll;
}
