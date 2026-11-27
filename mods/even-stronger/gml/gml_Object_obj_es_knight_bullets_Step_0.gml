if (!instance_exists(obj_growtangle) || global.turntimer < 1 || global.mnfight != 2)
{
    with (obj_es_kbullet)
        instance_destroy();
    instance_destroy();
    exit;
}
t++;
var bx1 = obj_growtangle.bbox_left;
var bx2 = obj_growtangle.bbox_right;
var by1 = obj_growtangle.bbox_top;
var by2 = obj_growtangle.bbox_bottom;
var hx = instance_exists(obj_heart) ? (obj_heart.x + 10) : ((bx1 + bx2) / 2);
var hy = instance_exists(obj_heart) ? (obj_heart.y + 10) : ((by1 + by2) / 2);
// crescents from the sides, aimed at the SOUL
if ((type == 0 || type == 3) && t > 12 && (t mod ((type == 3) ? 26 : 15)) == 0)
{
    var side = (t div 15) mod 2;
    var x0 = side ? (bx2 + 70) : (bx1 - 70);
    var y0 = random_range(by1 - 30, by2 + 30);
    var b = instance_create(x0, y0, obj_es_kbullet);
    b.sprite_index = scr_es_spr("es_rk_crescent");
    b.image_xscale = 2;
    b.image_yscale = 2;
    b.kind = 0;
    b.direction = point_direction(x0, y0, hx, hy);
    b.speed = 5.5;
    b.damage = damage;
    b.target = target;
    if ((t mod 30) == 0)
        scr_es_snd("es_snd_knight_cut", 0.35, 1.2 + random(0.3));
}
// stars rain down and split
if (type == 1 && t > 8 && (t mod 13) == 0)
{
    var b = instance_create(random_range(bx1 + 10, bx2 - 10), by1 - 50, obj_es_kbullet);
    b.sprite_index = scr_es_spr("es_rk_star");
    b.image_xscale = 0.7;
    b.image_yscale = 0.7;
    b.image_speed = 0.25;
    b.kind = 1;
    b.vspeed = 1.5;
    b.grav = 0.12;
    b.spin = 4;
    b.damage = damage;
    b.target = target;
}
// sword lines: a red warning, then the cut
if ((type == 2 || type == 3) && t > 10 && (t mod ((type == 3) ? 40 : 28)) == 0)
{
    var horiz = irandom(1);
    array_push(warn, [horiz, horiz ? hy : hx, 22]);
}
for (var i = array_length(warn) - 1; i >= 0; i--)
{
    var w = warn[i];
    w[2] -= 1;
    warn[i] = w;
    if (w[2] <= 0)
    {
        var b;
        if (w[0])
        {
            b = instance_create(bx1 - 30, w[1] - 6, obj_es_kbullet);
            b.image_xscale = (bx2 - bx1) + 60;
            b.image_yscale = 12;
        }
        else
        {
            b = instance_create(w[1] - 6, by1 - 30, obj_es_kbullet);
            b.image_xscale = 12;
            b.image_yscale = (by2 - by1) + 60;
        }
        b.sprite_index = spr_pixel_white;
        b.kind = 2;
        b.destroyonhit = 0;
        b.damage = damage;
        b.target = target;
        scr_es_snd("es_snd_knight_cut2", 0.5, 1 + random(0.2));
        array_delete(warn, i, 1);
    }
}
