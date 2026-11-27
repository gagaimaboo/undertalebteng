life++;
image_angle += spin;
if (kind == 0)
    image_angle = direction;
if (kind == 1)
{
    vspeed += grav;
    // a star that reaches the bottom of the box splits in two
    if (split == 0 && instance_exists(obj_growtangle) && y > (obj_growtangle.bbox_bottom - 24))
    {
        for (var i = -1; i <= 1; i += 2)
        {
            var b = instance_create(x, y, obj_es_kbullet);
            b.sprite_index = sprite_index;
            b.image_xscale = image_xscale * 0.6;
            b.image_yscale = image_yscale * 0.6;
            b.image_speed = 0.25;
            b.kind = 1;
            b.split = 1;
            b.hspeed = 2.6 * i;
            b.vspeed = -3.2;
            b.grav = 0.16;
            b.spin = 6 * i;
            b.damage = damage;
            b.target = target;
        }
        instance_destroy();
        exit;
    }
}
if (kind == 2)
{
    // a sword line: there for a moment, then gone
    active = (life < 9);
    image_alpha = (life < 9) ? 1 : max(0, 1 - ((life - 9) / 8));
    if (life > 17)
        instance_destroy();
}
if (life > lifemax || x < (camerax() - 200) || x > (camerax() + 840) || y > (cameray() + 680))
    instance_destroy();
