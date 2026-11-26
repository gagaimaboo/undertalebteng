// The giant arcade machine, left standing in the empty room.
// Check it and it turns on, asks one question, and powers off like a real CRT.
state = 0;      // 0 off, 1 warming up, 2 talking, 3 shutting down, 4 dead
t = 0;
image_xscale = 2;
image_yscale = 2;
sprite_index = spr_cutscene_10_arcade_off;
depth = 95000;
platter = instance_create(290, 145, obj_marker);
platter.sprite_index = spr_cutscene_10_platter_bottom;
platter.image_xscale = 2;
platter.image_yscale = 2;
platter.depth = depth + 5000;
blocker = instance_create(x - 56, y - 50, obj_solidblock);
blocker.image_xscale = 112 / sprite_get_width(blocker.sprite_index);
blocker.image_yscale = 50 / sprite_get_height(blocker.sprite_index);
blocker.visible = 0;
// the screen, in room pixels (sprite pixels 37..72 x 12..37, origin 54,82, scale 2)
sx1 = x + ((37 - 54) * 2);
sy1 = y + ((12 - 82) * 2);
sx2 = x + ((72 - 54) * 2);
sy2 = y + ((37 - 82) * 2);
