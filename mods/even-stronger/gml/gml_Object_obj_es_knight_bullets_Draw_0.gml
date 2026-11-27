if (!instance_exists(obj_growtangle))
    exit;
var bx1 = obj_growtangle.bbox_left;
var bx2 = obj_growtangle.bbox_right;
var by1 = obj_growtangle.bbox_top;
var by2 = obj_growtangle.bbox_bottom;
for (var i = 0; i < array_length(warn); i++)
{
    var w = warn[i];
    draw_set_alpha(((w[2] mod 4) < 2) ? 0.9 : 0.4);
    draw_set_color(c_red);
    if (w[0])
        draw_rectangle(bx1, w[1] - 1, bx2, w[1] + 1, false);
    else
        draw_rectangle(w[1] - 1, by1, w[1] + 1, by2, false);
}
draw_set_alpha(1);
draw_set_color(c_white);
