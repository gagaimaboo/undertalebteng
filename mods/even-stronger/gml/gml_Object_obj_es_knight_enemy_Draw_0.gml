siner2++;
if (endcon > 0)
{
    scr_es_knight_enddraw();
    exit;
}
if (state == 3)
{
    hurttimer -= 1;
    if (hurttimer < 0)
    {
        state = 0;
    }
    else
    {
        hurtshake += 1;
        if (hurtshake > 1)
        {
            if (shakex > 0)
                shakex -= 1;
            if (shakex < 0)
                shakex += 1;
            shakex = -shakex;
            hurtshake = 0;
        }
        // hurt: the knight flickers into its ball for a frame, like Chapter 3
        if ((hurttimer mod 2) == 0)
            draw_sprite_ext(idlesprite, 0, x + shakex, y, 2, 2, 0, image_blend, 1);
        else
            draw_sprite_ext(scr_es_spr("es_rk_balltrans"), 7, x + shakex, y, 2, 2, 0, image_blend, 1);
    }
}
if (state == 0)
{
    y = ystart + (cos(siner2 / 8) * 8);
    aetimer++;
    if ((aetimer mod 4) == 0)
    {
        var ae = instance_create(x, y, obj_afterimage);
        ae.sprite_index = idlesprite;
        ae.image_xscale = 2;
        ae.image_yscale = 2;
        ae.image_alpha = 0.6;
        ae.fadeSpeed = 0.02;
        ae.hspeed = 2;
        ae.image_speed = 0;
        ae.depth = depth + 1;
    }
    draw_sprite_ext(idlesprite, 0, x, y, 2, 2, 0, image_blend, 1);
}
if (becomeflash == 0)
    flash = 0;
becomeflash = 0;
